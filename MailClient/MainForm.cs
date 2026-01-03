using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Net.Pop3;
using MailKit;
using MimeKit;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using MailKit.Security;
using MailKit.Search;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace MailClient
{
    public partial class MainForm : Form
    {
        private const string KeyFilePath = "aes_key.bin";
        private const string AccountsFilePath = "accounts.enc";
        private byte[] aesKey;
        private byte[] aesIV;
        private List<Account> accounts = new List<Account>();
        private List<MimeMessage> loadedMessages = new List<MimeMessage>();
        private List<string> messageIds = new List<string>();

        private class MailServer
        {
            public string Name { get; set; }
            public string ImapServer { get; set; }
            public int ImapPort { get; set; }
            public string Pop3Server { get; set; }
            public int Pop3Port { get; set; }
            public string SmtpServer { get; set; }
            public int SmtpPort { get; set; }
            public bool SupportsOAuth2 { get; set; }
            public string SmtpEncryption { get; set; }

            public override string ToString() => Name;
        }

        private readonly List<MailServer> mailServers = new List<MailServer>
        {
            new MailServer { Name = "Gmail", ImapServer = "imap.gmail.com", ImapPort = 993, Pop3Server = "pop.gmail.com", Pop3Port = 995, SmtpServer = "smtp.gmail.com", SmtpPort = 587, SupportsOAuth2 = true, SmtpEncryption = "STARTTLS" },
            new MailServer { Name = "Яндекс.Почта", ImapServer = "imap.yandex.com", ImapPort = 993, Pop3Server = "pop.yandex.com", Pop3Port = 995, SmtpServer = "smtp.yandex.com", SmtpPort = 465, SupportsOAuth2 = false, SmtpEncryption = "SSL/TLS" },
            new MailServer { Name = "Mail.ru", ImapServer = "imap.mail.ru", ImapPort = 993, Pop3Server = "pop.mail.ru", Pop3Port = 995, SmtpServer = "smtp.mail.ru", SmtpPort = 465, SupportsOAuth2 = false, SmtpEncryption = "SSL/TLS" },
            new MailServer { Name = "Vivaldi Webmail", ImapServer = "imap.vivaldi.net", ImapPort = 993, Pop3Server = "pop.vivaldi.net", Pop3Port = 995, SmtpServer = "smtp.vivaldi.net", SmtpPort = 465, SupportsOAuth2 = true, SmtpEncryption = "SSL/TLS" },
            new MailServer { Name = "ProtonMail (via Bridge)", ImapServer = "127.0.0.1", ImapPort = 1143, Pop3Server = "", Pop3Port = 0, SmtpServer = "127.0.0.1", SmtpPort = 1025, SupportsOAuth2 = true, SmtpEncryption = "None" },
            new MailServer { Name = "Outlook.com", ImapServer = "outlook.office365.com", ImapPort = 993, Pop3Server = "outlook.office365.com", Pop3Port = 995, SmtpServer = "smtp-mail.outlook.com", SmtpPort = 587, SupportsOAuth2 = true, SmtpEncryption = "STARTTLS" },
            new MailServer { Name = "Пользовательский", ImapServer = "", ImapPort = 0, Pop3Server = "", Pop3Port = 0, SmtpServer = "", SmtpPort = 0, SupportsOAuth2 = true, SmtpEncryption = "SSL/TLS" }
        };

        public MainForm()
        {
            InitializeComponent();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;
            LoadOrGenerateKey();
            LoadAccounts();
        }

        private void LoadOrGenerateKey()
        {
            try
            {
                if (File.Exists(KeyFilePath))
                {
                    byte[] keyData = File.ReadAllBytes(KeyFilePath);
                    aesKey = new byte[32];
                    aesIV = new byte[16];
                    Array.Copy(keyData, 0, aesKey, 0, Math.Min(keyData.Length, 32));
                    Array.Copy(keyData, 32, aesIV, 0, Math.Min(keyData.Length - 32, 16));
                }
                else
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.GenerateKey();
                        aes.GenerateIV();
                        aesKey = aes.Key;
                        aesIV = aes.IV;
                        byte[] keyData = new byte[aesKey.Length + aesIV.Length];
                        Array.Copy(aesKey, 0, keyData, 0, aesKey.Length);
                        Array.Copy(aesIV, 0, keyData, aesKey.Length, aesIV.Length);
                        File.WriteAllBytes(KeyFilePath, keyData);
                    }
                    MessageBox.Show("Ключ сгенерирован и сохранен в aes_key.bin");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при работе с ключом: {ex.Message}");
            }
        }

        private void LoadAccounts()
        {
            try
            {
                if (File.Exists(AccountsFilePath))
                {
                    byte[] encryptedData = File.ReadAllBytes(AccountsFilePath);
                    byte[] decryptedData = DecryptBytes(encryptedData);
                    using (var ms = new MemoryStream(decryptedData))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        accounts = (List<Account>)formatter.Deserialize(ms);
                    }
                    foreach (var account in accounts)
                    {
                        accountSelector.Items.Add(account.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки аккаунтов: {ex.Message}");
            }
        }

        private void SaveAccounts()
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, accounts);
                    byte[] plainData = ms.ToArray();
                    byte[] encryptedData = EncryptBytes(plainData);
                    File.WriteAllBytes(AccountsFilePath, encryptedData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения аккаунтов: {ex.Message}");
            }
        }

        private void SaveEmails(string email)
        {
            try
            {
                string filePath = $"email_{email}.enc";
                messageIds.Clear();
                foreach (var message in loadedMessages)
                {
                    if (!string.IsNullOrEmpty(message.MessageId))
                    {
                        messageIds.Add(message.MessageId);
                    }
                }
                using (var ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, messageIds);
                    byte[] plainData = ms.ToArray();
                    byte[] encryptedData = EncryptBytes(plainData);
                    File.WriteAllBytes(filePath, encryptedData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения писем: {ex.Message}");
            }
        }

        private void LoadEmailsFromFile(string email)
        {
            try
            {
                string filePath = $"email_{email}.enc";
                if (File.Exists(filePath))
                {
                    byte[] encryptedData = File.ReadAllBytes(filePath);
                    byte[] decryptedData = DecryptBytes(encryptedData);
                    using (var ms = new MemoryStream(decryptedData))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        messageIds = (List<string>)formatter.Deserialize(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки писем из файла: {ex.Message}");
            }
        }

        private void ShowKey(object sender, EventArgs e)
        {
            string keyBase64 = Convert.ToBase64String(aesKey);
            string ivBase64 = Convert.ToBase64String(aesIV);
            MessageBox.Show($"Ключ: {keyBase64}\nIV: {ivBase64}", "Текущий ключ");
        }

        private void GenerateNewKey(object sender, EventArgs e)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.GenerateKey();
                    aes.GenerateIV();
                    aesKey = aes.Key;
                    aesIV = aes.IV;
                    byte[] keyData = new byte[aesKey.Length + aesIV.Length];
                    Array.Copy(aesKey, 0, keyData, 0, aesKey.Length);
                    Array.Copy(aesIV, 0, keyData, aesKey.Length, aesIV.Length);
                    File.WriteAllBytes(KeyFilePath, keyData);
                }
                MessageBox.Show("Новый ключ сгенерирован и сохранен! Старые данные могут быть недоступны.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации ключа: {ex.Message}");
            }
        }

        private string TruncateString(string input, int maxLength = 10)
        {
            if (string.IsNullOrEmpty(input)) return "null";
            return input.Length >= maxLength ? input.Substring(0, maxLength) + "..." : input;
        }

        private async Task<string> RefreshAccessToken(Account account)
        {
            if (string.IsNullOrEmpty(account.RefreshToken))
            {
                MessageBox.Show($"Refresh-токен отсутствует для {account.Email}. Требуется повторная авторизация.");
                throw new Exception("Refresh-токен отсутствует.");
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    string clientId = "0971cc7d-0742-42ef-b3cf-35e483ae40d2";
                    var requestContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", clientId),
                        new KeyValuePair<string, string>("refresh_token", account.RefreshToken),
                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                        new KeyValuePair<string, string>("scope", "https://outlook.office.com/IMAP.AccessAsUser.All https://outlook.office.com/SMTP.Send offline_access")
                    });

                    var response = await httpClient.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", requestContent);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Ошибка обновления токена для {account.Email}: {response.StatusCode}. Ответ сервера: {responseString}");
                        throw new Exception($"Ошибка обновления токена: {response.StatusCode}. Ответ: {responseString}");
                    }

                    var tokens = JObject.Parse(responseString);
                    string newAccessToken = tokens["access_token"]?.ToString();
                    string newRefreshToken = tokens["refresh_token"]?.ToString();

                    if (string.IsNullOrEmpty(newAccessToken))
                    {
                        MessageBox.Show($"Новый access-токен не получен для {account.Email}. Ответ сервера: {responseString}");
                        throw new Exception("Новый access-токен не получен.");
                    }

                    account.OAuth2Token = newAccessToken;
                    if (!string.IsNullOrEmpty(newRefreshToken))
                    {
                        account.RefreshToken = newRefreshToken;
                        MessageBox.Show($"Refresh-токен обновлен для {account.Email}: {TruncateString(newRefreshToken)}");
                    }
                    else
                    {
                        MessageBox.Show($"Новый refresh-токен не получен для {account.Email}, используется старый: {TruncateString(account.RefreshToken)}");
                    }
                    SaveAccounts();
                    MessageBox.Show($"Access-токен успешно обновлен для {account.Email}: {TruncateString(newAccessToken)}");
                    return newAccessToken;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в RefreshAccessToken для {account.Email}: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> TestConnectionAsync(Account account, CancellationToken cancellationToken = default)
        {
            try
            {
                if (account.UsePop3)
                {
                    using (var pop3Client = new Pop3Client())
                    {
                        await pop3Client.ConnectAsync(account.Pop3Server, account.Pop3Port, SecureSocketOptions.SslOnConnect, cancellationToken);
                        if (account.UseOAuth2)
                        {
                            string accessToken = account.OAuth2Token;
                            MessageBox.Show($"Попытка аутентификации POP3 для {account.Email} с токеном: {TruncateString(accessToken)}");
                            try
                            {
                                var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                                await pop3Client.AuthenticateAsync(oauth2, cancellationToken);
                            }
                            catch (AuthenticationException ex)
                            {
                                MessageBox.Show($"Ошибка аутентификации POP3 для {account.Email}: {ex.Message}. Пробуем обновить токен...");
                                accessToken = await RefreshAccessToken(account);
                                var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                                await pop3Client.AuthenticateAsync(oauth2, cancellationToken);
                            }
                        }
                        else
                        {
                            await pop3Client.AuthenticateAsync(account.Email, account.Password, cancellationToken);
                        }
                        await pop3Client.DisconnectAsync(true, cancellationToken);
                    }
                }
                else
                {
                    using (var imapClient = new ImapClient())
                    {
                        await imapClient.ConnectAsync(account.ImapServer, account.ImapPort, SecureSocketOptions.SslOnConnect, cancellationToken);
                        if (account.UseOAuth2)
                        {
                            string accessToken = account.OAuth2Token;
                            MessageBox.Show($"Попытка аутентификации IMAP для {account.Email} с токеном: {TruncateString(accessToken)}");
                            try
                            {
                                var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                                await imapClient.AuthenticateAsync(oauth2, cancellationToken);
                            }
                            catch (AuthenticationException ex)
                            {
                                MessageBox.Show($"Ошибка аутентификации IMAP для {account.Email}: {ex.Message}. Пробуем обновить токен...");
                                accessToken = await RefreshAccessToken(account);
                                var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                                await imapClient.AuthenticateAsync(oauth2, cancellationToken);
                            }
                        }
                        else
                        {
                            await imapClient.AuthenticateAsync(account.Email, account.Password, cancellationToken);
                        }
                        await imapClient.DisconnectAsync(true, cancellationToken);
                    }
                }

                using (var smtpClient = new SmtpClient())
                {
                    SecureSocketOptions options = account.SmtpEncryption == "STARTTLS" ? SecureSocketOptions.StartTls : (account.SmtpEncryption == "None" ? SecureSocketOptions.None : SecureSocketOptions.SslOnConnect);
                    await smtpClient.ConnectAsync(account.SmtpServer, account.SmtpPort, options, cancellationToken);
                    if (account.UseOAuth2)
                    {
                        string accessToken = account.OAuth2Token;
                        MessageBox.Show($"Попытка аутентификации SMTP для {account.Email} с токеном: {TruncateString(accessToken)}");
                        try
                        {
                            var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                            await smtpClient.AuthenticateAsync(oauth2, cancellationToken);
                        }
                        catch (AuthenticationException ex)
                        {
                            MessageBox.Show($"Ошибка аутентификации SMTP для {account.Email}: {ex.Message}. Пробуем обновить токен...");
                            accessToken = await RefreshAccessToken(account);
                            var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                            await smtpClient.AuthenticateAsync(oauth2, cancellationToken);
                        }
                    }
                    else
                    {
                        await smtpClient.AuthenticateAsync(account.Email, account.Password, cancellationToken);
                    }
                    await smtpClient.DisconnectAsync(true, cancellationToken);
                }
                MessageBox.Show($"Подключение к {account.Email} успешно!");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в TestConnectionAsync для {account.Email}: {ex.Message}");
                return false;
            }
        }

        private async Task SendEmailAsync(Account account, string to, string subject, string body, List<string> attachments, bool encrypt, string encryptionPassword)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(account.Email, account.Email));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var builder = new BodyBuilder();
                if (encrypt)
                {
                    builder.TextBody = EncryptString(body, encryptionPassword);
                    foreach (var file in attachments)
                    {
                        byte[] fileBytes = File.ReadAllBytes(file);
                        byte[] encryptedBytes = EncryptBytes(fileBytes, encryptionPassword);
                        builder.Attachments.Add(Path.GetFileName(file) + ".enc", encryptedBytes);
                    }
                }
                else
                {
                    builder.TextBody = body;
                    foreach (var file in attachments)
                    {
                        builder.Attachments.Add(file);
                    }
                }
                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                    {
                        SecureSocketOptions options = account.SmtpEncryption == "STARTTLS" ? SecureSocketOptions.StartTls : (account.SmtpEncryption == "None" ? SecureSocketOptions.None : SecureSocketOptions.SslOnConnect);
                        await client.ConnectAsync(account.SmtpServer, account.SmtpPort, options, cts.Token);
                    }
                    if (account.UseOAuth2)
                    {
                        string accessToken = account.OAuth2Token;
                        try
                        {
                            var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                            await client.AuthenticateAsync(oauth2);
                        }
                        catch (AuthenticationException)
                        {
                            MessageBox.Show($"Ошибка аутентификации SMTP при отправке для {account.Email}. Пробуем обновить токен...");
                            accessToken = await RefreshAccessToken(account);
                            var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                            await client.AuthenticateAsync(oauth2);
                        }
                    }
                    else
                    {
                        await client.AuthenticateAsync(account.Email, account.Password);
                    }
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                MessageBox.Show("Письмо отправлено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки письма: {ex.Message}");
            }
        }

        private async Task LoadFoldersAndEmailsAsync(object sender, EventArgs e)
        {
            if (accountSelector.SelectedIndex < 0) return;
            var account = accounts[accountSelector.SelectedIndex];

            folderTreeView.Nodes.Clear();
            emailListView.Items.Clear();
            loadedMessages.Clear();
            this.Cursor = Cursors.WaitCursor;

            try
            {
                using (var client = new ImapClient())
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                    {
                        await client.ConnectAsync(account.ImapServer, account.ImapPort, SecureSocketOptions.SslOnConnect, cts.Token);
                    }

                    if (account.UseOAuth2)
                    {
                        string accessToken = account.OAuth2Token;
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            throw new Exception("Токен OAuth2 не указан.");
                        }
                        try
                        {
                            var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                            await client.AuthenticateAsync(oauth2);
                        }
                        catch (AuthenticationException)
                        {
                            MessageBox.Show($"Ошибка аутентификации IMAP при загрузке папок для {account.Email}. Пробуем обновить токен...");
                            accessToken = await RefreshAccessToken(account);
                            var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                            await client.AuthenticateAsync(oauth2);
                        }
                    }
                    else
                    {
                        await client.AuthenticateAsync(account.Email, account.Password);
                    }

                    if (!account.UsePop3)
                    {
                        var folders = client.GetFolders(client.PersonalNamespaces[0]);
                        foreach (var folder in folders)
                        {
                            string displayName = GetFolderDisplayName(folder, client);
                            var node = folderTreeView.Nodes.Add(displayName);
                            node.Tag = folder.FullName;

                            if (folder.GetSubfolders().Any())
                            {
                                foreach (var subfolder in folder.GetSubfolders())
                                {
                                    AddFolderToTreeView(subfolder, node.Nodes, client);
                                }
                            }
                        }

                        var inboxNode = folderTreeView.Nodes.Cast<TreeNode>().FirstOrDefault(n => (string)n.Tag == client.Inbox.FullName);
                        if (inboxNode != null)
                        {
                            folderTreeView.SelectedNode = inboxNode;
                        }
                    }
                    else
                    {
                        MessageBox.Show("POP3 не поддерживает работу с папками. Переключитесь на IMAP.");
                    }

                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки папок для {account.Email}: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void AddFolderToTreeView(IMailFolder folder, TreeNodeCollection nodes, ImapClient client)
        {
            string displayName = GetFolderDisplayName(folder, client);
            var node = nodes.Add(displayName);
            node.Tag = folder.FullName;

            if (folder.GetSubfolders().Any())
            {
                foreach (var subfolder in folder.GetSubfolders())
                {
                    AddFolderToTreeView(subfolder, node.Nodes, client);
                }
            }
        }

        private string GetFolderDisplayName(IMailFolder folder, ImapClient client)
        {
            if (folder.FullName.Equals("INBOX", StringComparison.OrdinalIgnoreCase))
                return "Входящие";

            bool supportsSpecialUse = client.Capabilities.HasFlag(ImapCapabilities.SpecialUse);
            bool supportsXList = client.Capabilities.HasFlag(ImapCapabilities.XList);

            if (supportsSpecialUse || supportsXList)
            {
                try
                {
                    if (folder == client.GetFolder(SpecialFolder.Sent)) return "Отправленные";
                    if (folder == client.GetFolder(SpecialFolder.Drafts)) return "Черновики";
                    if (folder == client.GetFolder(SpecialFolder.Trash)) return "Корзина";
                    if (folder == client.GetFolder(SpecialFolder.Junk)) return "Спам";
                    if (folder == client.GetFolder(SpecialFolder.Archive)) return "Архив";
                }
                catch (NotSupportedException)
                {
                }
            }

            string folderName = folder.Name.ToLower();
            if (folderName.Contains("sent")) return "Отправленные";
            if (folderName.Contains("drafts")) return "Черновики";
            if (folderName.Contains("trash") || folderName.Contains("deleted")) return "Корзина";
            if (folderName.Contains("spam") || folderName.Contains("junk")) return "Спам";
            if (folderName.Contains("archive")) return "Архив";

            return folder.Name;
        }

        private async void FolderTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (accountSelector.SelectedIndex < 0) return;
            var account = accounts[accountSelector.SelectedIndex];

            var folderFullName = e.Node.Tag as string;
            if (string.IsNullOrEmpty(folderFullName)) return;

            this.Cursor = Cursors.WaitCursor;
            emailListView.Items.Clear();
            loadedMessages.Clear();

            try
            {
                using (var client = new ImapClient())
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                    {
                        await client.ConnectAsync(account.ImapServer, account.ImapPort, SecureSocketOptions.SslOnConnect, cts.Token);
                    }

                    if (account.UseOAuth2)
                    {
                        string accessToken = account.OAuth2Token;
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            throw new Exception("Токен OAuth2 не указан.");
                        }
                        try
                        {
                            var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                            await client.AuthenticateAsync(oauth2);
                        }
                        catch (AuthenticationException)
                        {
                            MessageBox.Show($"Ошибка аутентификации IMAP для папки {folderFullName} ({account.Email}). Пробуем обновить токен...");
                            accessToken = await RefreshAccessToken(account);
                            var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                            await client.AuthenticateAsync(oauth2);
                        }
                    }
                    else
                    {
                        await client.AuthenticateAsync(account.Email, account.Password);
                    }

                    var folders = client.GetFolders(client.PersonalNamespaces[0]);
                    var selectedFolder = folders.FirstOrDefault(f => f.FullName == folderFullName) ?? client.GetFolder(folderFullName);
                    if (selectedFolder == null)
                    {
                        throw new Exception($"Папка {folderFullName} не найдена.");
                    }

                    await selectedFolder.OpenAsync(FolderAccess.ReadOnly);
                    var allMessages = selectedFolder.Search(SearchQuery.All);

                    foreach (var uniqueId in allMessages)
                    {
                        var message = await selectedFolder.GetMessageAsync(uniqueId);
                        loadedMessages.Add(message);
                    }

                    emailListView.BeginUpdate();
                    foreach (var message in loadedMessages)
                    {
                        string from = message.From?.ToString() ?? "Неизвестный отправитель";
                        string subject = message.Subject ?? "(Без темы)";
                        string date = message.Date.ToString("g");
                        string attachmentsIndicator = message.Attachments.Any() ? "Да" : "Нет";

                        var item = new ListViewItem(new[] { from, subject, date, attachmentsIndicator });
                        item.Tag = message;
                        emailListView.Items.Add(item);
                    }
                    emailListView.EndUpdate();
                    await Task.Delay(100);
                    emailListView.Refresh();

                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки писем из папки {folderFullName} для {account.Email}: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void DeleteEmailAsync(object sender, EventArgs e)
        {
            if (accountSelector.SelectedIndex < 0 || emailListView.SelectedItems.Count == 0 || folderTreeView.SelectedNode == null) return;
            var account = accounts[accountSelector.SelectedIndex];
            var selectedItem = emailListView.SelectedItems[0];
            var message = (MimeMessage)selectedItem.Tag;
            var folderName = folderTreeView.SelectedNode.Tag as string;

            if (string.IsNullOrEmpty(folderName)) return;

            try
            {
                if (account.UsePop3)
                {
                    MessageBox.Show("Удаление писем через POP3 не поддерживается. Письмо будет удалено только локально.");
                    loadedMessages.Remove(message);
                    emailListView.Items.Remove(selectedItem);
                    SaveEmails(account.Email);
                    MessageBox.Show("Письмо удалено локально!");
                }
                else
                {
                    using (var client = new ImapClient())
                    {
                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            await client.ConnectAsync(account.ImapServer, account.ImapPort, SecureSocketOptions.SslOnConnect, cts.Token);
                        }

                        if (account.UseOAuth2)
                        {
                            string accessToken = account.OAuth2Token;
                            if (string.IsNullOrEmpty(accessToken))
                            {
                                throw new Exception("Токен OAuth2 не указан.");
                            }
                            try
                            {
                                var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                                await client.AuthenticateAsync(oauth2);
                            }
                            catch (AuthenticationException)
                            {
                                MessageBox.Show($"Ошибка аутентификации IMAP при удалении письма для {account.Email}. Пробуем обновить токен...");
                                accessToken = await RefreshAccessToken(account);
                                var oauth2 = new SaslMechanismOAuth2(account.Email, accessToken);
                                await client.AuthenticateAsync(oauth2);
                            }
                        }
                        else
                        {
                            await client.AuthenticateAsync(account.Email, account.Password);
                        }

                        var folders = client.GetFolders(client.PersonalNamespaces[0]);
                        var selectedFolder = folders.FirstOrDefault(f => f.FullName == folderName) ?? client.GetFolder(folderName);
                        if (selectedFolder == null)
                        {
                            throw new Exception($"Папка {folderName} не найдена.");
                        }

                        await selectedFolder.OpenAsync(FolderAccess.ReadWrite);
                        var uniqueId = selectedFolder.Search(SearchQuery.All).FirstOrDefault(id => selectedFolder.GetMessage(id).MessageId == message.MessageId);
                        if (uniqueId != null)
                        {
                            await selectedFolder.AddFlagsAsync(uniqueId, MessageFlags.Deleted, true);
                            await selectedFolder.ExpungeAsync();
                        }

                        await client.DisconnectAsync(true);
                    }

                    loadedMessages.Remove(message);
                    emailListView.Items.Remove(selectedItem);
                    SaveEmails(account.Email);
                    MessageBox.Show("Письмо удалено!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления письма для {account.Email}: {ex.Message}");
            }
        }

        private async void DeleteAccount(object sender, EventArgs e)
        {
            if (accountSelector.SelectedIndex < 0) return;
            var account = accounts[accountSelector.SelectedIndex];
            if (MessageBox.Show($"Удалить аккаунт {account.Email}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                accounts.RemoveAt(accountSelector.SelectedIndex);
                accountSelector.Items.RemoveAt(accountSelector.SelectedIndex);
                SaveAccounts();
                emailListView.Items.Clear();
                loadedMessages.Clear();
                folderTreeView.Nodes.Clear();
                File.Delete($"email_{account.Email}.enc");
                MessageBox.Show("Аккаунт удален!");
                await LoadFoldersAndEmailsAsync(sender, e);
            }
        }

        private async void SearchEmails(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                await LoadFoldersAndEmailsAsync(sender, e);
                return;
            }
            var query = searchBox.Text.ToLower();
            emailListView.Items.Clear();

            var filteredMessages = loadedMessages.Where(m =>
                (m.Subject?.ToLower().Contains(query) ?? false) ||
                (m.TextBody?.ToLower().Contains(query) ?? false) ||
                (m.HtmlBody?.ToLower().Contains(query) ?? false));

            foreach (var message in filteredMessages)
            {
                var item = new ListViewItem(new[] { message.From.ToString(), message.Subject, message.Date.ToString("g"), message.Attachments.Any() ? "Да" : "Нет" });
                item.Tag = message;
                emailListView.Items.Add(item);
            }
        }

        private void DisplayEmailContent(object sender, EventArgs e)
        {
            if (emailListView.SelectedItems.Count == 0) return;
            var message = (MimeMessage)emailListView.SelectedItems[0].Tag;

            string content = message.TextBody ?? message.HtmlBody ?? "Нет содержимого";
            bool isBase64 = IsBase64String(content);

            if (isBase64)
            {
                using (var form = new Form { Text = "Введите пароль для дешифровки", Size = new System.Drawing.Size(300, 150) })
                {
                    Label label = new Label { Text = "Пароль:", Location = new System.Drawing.Point(10, 10), Width = 100 };
                    TextBox passwordBox = new TextBox { Location = new System.Drawing.Point(10, 40), Width = 260, UseSystemPasswordChar = true };
                    Button okButton = new Button { Text = "ОК", Location = new System.Drawing.Point(10, 70), Width = 100 };
                    Button cancelButton = new Button { Text = "Отмена", Location = new System.Drawing.Point(120, 70), Width = 100 };

                    okButton.Click += (s, ev) =>
                    {
                        try
                        {
                            string decryptedBody = DecryptString(content, passwordBox.Text);
                            if (message.TextBody != null || !decryptedBody.Contains("<html"))
                            {
                                emailWebBrowser.DocumentText = "<pre>" + decryptedBody + "</pre>";
                            }
                            else
                            {
                                emailWebBrowser.DocumentText = decryptedBody;
                            }
                            form.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка дешифровки: {ex.Message}");
                            form.Close();
                        }
                    };

                    cancelButton.Click += (s, ev) => form.Close();

                    form.Controls.Add(label);
                    form.Controls.Add(passwordBox);
                    form.Controls.Add(okButton);
                    form.Controls.Add(cancelButton);
                    form.ShowDialog();
                }
            }
            else
            {
                if (message.TextBody != null || !content.Contains("<html"))
                {
                    emailWebBrowser.DocumentText = "<pre>" + content + "</pre>";
                }
                else
                {
                    emailWebBrowser.DocumentText = content;
                }
            }
        }

        private void DownloadAttachments(object sender, EventArgs e)
        {
            if (emailListView.SelectedItems.Count == 0) return;
            var message = (MimeMessage)emailListView.SelectedItems[0].Tag;

            if (!message.Attachments.Any())
            {
                MessageBox.Show("У этого письма нет вложений.");
                return;
            }

            using (var folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.Description = "Выберите папку для сохранения вложений";
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        if (attachment is MimePart part)
                        {
                            string fileName = part.FileName ?? "attachment_" + DateTime.Now.Ticks;
                            string filePath = Path.Combine(folderBrowser.SelectedPath, fileName);

                            if (fileName.EndsWith(".enc"))
                            {
                                using (var form = new Form { Text = "Введите пароль для дешифровки вложения", Size = new System.Drawing.Size(300, 150) })
                                {
                                    Label label = new Label { Text = "Пароль:", Location = new System.Drawing.Point(10, 10), Width = 100 };
                                    TextBox passwordBox = new TextBox { Location = new System.Drawing.Point(10, 40), Width = 260, UseSystemPasswordChar = true };
                                    Button okButton = new Button { Text = "ОК", Location = new System.Drawing.Point(10, 70), Width = 100 };
                                    Button cancelButton = new Button { Text = "Отмена", Location = new System.Drawing.Point(120, 70), Width = 100 };

                                    okButton.Click += (s, ev) =>
                                    {
                                        try
                                        {
                                            using (var stream = new MemoryStream())
                                            {
                                                part.Content.DecodeTo(stream);
                                                byte[] encryptedBytes = stream.ToArray();
                                                byte[] decryptedBytes = DecryptBytes(encryptedBytes, passwordBox.Text);
                                                string decryptedFileName = fileName.Substring(0, fileName.Length - 4);
                                                File.WriteAllBytes(Path.Combine(folderBrowser.SelectedPath, decryptedFileName), decryptedBytes);
                                                MessageBox.Show($"Вложение {decryptedFileName} успешно сохранено!");
                                            }
                                            form.Close();
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Ошибка дешифровки вложения {fileName}: {ex.Message}");
                                            form.Close();
                                        }
                                    };

                                    cancelButton.Click += (s, ev) => form.Close();

                                    form.Controls.Add(label);
                                    form.Controls.Add(passwordBox);
                                    form.Controls.Add(okButton);
                                    form.Controls.Add(cancelButton);
                                    form.ShowDialog();
                                }
                            }
                            else
                            {
                                try
                                {
                                    using (var stream = File.Create(filePath))
                                    {
                                        part.Content.DecodeTo(stream);
                                    }
                                    MessageBox.Show($"Вложение {fileName} успешно сохранено!");
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Ошибка сохранения вложения {fileName}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsBase64String(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim();
            if (input.Length % 4 != 0) return false;
            try
            {
                Convert.FromBase64String(input);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ShowSendEmailForm(object sender, EventArgs e)
        {
            using (Form sendForm = new Form { Text = "Новое письмо", Size = new System.Drawing.Size(400, 500) })
            {
                TextBox toBox = new TextBox { Location = new System.Drawing.Point(10, 10), Width = 360, Text = "Кому" };
                TextBox subjectBox = new TextBox { Location = new System.Drawing.Point(10, 40), Width = 360, Text = "Тема" };
                TextBox bodyBox = new TextBox { Location = new System.Drawing.Point(10, 70), Size = new System.Drawing.Size(360, 200), Multiline = true };
                CheckBox encryptCheckBox = new CheckBox { Text = "Шифровать", Location = new System.Drawing.Point(10, 280), Checked = false };
                TextBox encryptionPasswordBox = new TextBox { Location = new System.Drawing.Point(10, 310), Width = 360, UseSystemPasswordChar = true, Enabled = false, Text = "Введите пароль для шифрования" };
                Button attachButton = new Button { Text = "Добавить вложение", Location = new System.Drawing.Point(10, 340) };
                Button sendButton = new Button { Text = "Отправить", Location = new System.Drawing.Point(10, 370) };
                List<string> attachments = new List<string>();

                encryptCheckBox.CheckedChanged += (s, ev) => encryptionPasswordBox.Enabled = encryptCheckBox.Checked;

                attachButton.Click += (s, ev) =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog { Multiselect = true })
                    {
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            attachments.AddRange(ofd.FileNames);
                            MessageBox.Show($"Добавлено вложений: {ofd.FileNames.Length}");
                        }
                    }
                };

                sendButton.Click += async (s, ev) =>
                {
                    if (accountSelector.SelectedIndex < 0) { MessageBox.Show("Выберите аккаунт!"); return; }
                    var account = accounts[accountSelector.SelectedIndex];
                    if (encryptCheckBox.Checked && string.IsNullOrWhiteSpace(encryptionPasswordBox.Text))
                    {
                        MessageBox.Show("Введите пароль для шифрования!");
                        return;
                    }
                    await SendEmailAsync(account, toBox.Text, subjectBox.Text, bodyBox.Text, attachments, encryptCheckBox.Checked, encryptionPasswordBox.Text);
                    sendForm.Close();
                };

                sendForm.Controls.Add(toBox);
                sendForm.Controls.Add(subjectBox);
                sendForm.Controls.Add(bodyBox);
                sendForm.Controls.Add(encryptCheckBox);
                sendForm.Controls.Add(encryptionPasswordBox);
                sendForm.Controls.Add(attachButton);
                sendForm.Controls.Add(sendButton);
                sendForm.ShowDialog();
            }
        }

        private string EncryptString(string plainText, string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            try
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] key = new byte[32];
                    byte[] iv = new byte[16];
                    Array.Copy(passwordBytes, 0, key, 0, Math.Min(passwordBytes.Length, key.Length));
                    Array.Copy(passwordBytes, 0, iv, 0, Math.Min(passwordBytes.Length, iv.Length));
                    aes.Key = key;
                    aes.IV = iv;
                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка шифрования текста: {ex.Message}");
            }
        }

        private string DecryptString(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] key = new byte[32];
                    byte[] iv = new byte[16];
                    Array.Copy(passwordBytes, 0, key, 0, Math.Min(passwordBytes.Length, key.Length));
                    Array.Copy(passwordBytes, 0, iv, 0, Math.Min(passwordBytes.Length, iv.Length));
                    aes.Key = key;
                    aes.IV = iv;
                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(cipherBytes))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка расшифровки текста: {ex.Message}");
            }
        }

        private byte[] EncryptBytes(byte[] plainBytes)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIV;
                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                        }
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка шифрования данных: {ex.Message}");
            }
        }

        private byte[] DecryptBytes(byte[] cipherBytes)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIV;
                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка расшифровки данных: {ex.Message}");
            }
        }

        private byte[] EncryptBytes(byte[] plainBytes, string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            try
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] key = new byte[32];
                    byte[] iv = new byte[16];
                    Array.Copy(passwordBytes, 0, key, 0, Math.Min(passwordBytes.Length, key.Length));
                    Array.Copy(passwordBytes, 0, iv, 0, Math.Min(passwordBytes.Length, iv.Length));
                    aes.Key = key;
                    aes.IV = iv;
                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                        }
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка шифрования данных: {ex.Message}");
            }
        }

        private byte[] DecryptBytes(byte[] cipherBytes, string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            try
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] key = new byte[32];
                    byte[] iv = new byte[16];
                    Array.Copy(passwordBytes, 0, key, 0, Math.Min(passwordBytes.Length, key.Length));
                    Array.Copy(passwordBytes, 0, iv, 0, Math.Min(passwordBytes.Length, iv.Length));
                    aes.Key = key;
                    aes.IV = iv;
                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка расшифровки данных: {ex.Message}");
            }
        }

        private async Task<string> StartLocalServerForCode(string redirectUri)
        {
            using (var listener = new System.Net.HttpListener())
            {
                listener.Prefixes.Add(redirectUri + "/");
                listener.Start();

                var tcs = new TaskCompletionSource<string>();
                Task.Run(() =>
                {
                    try
                    {
                        var context = listener.GetContext();
                        var request = context.Request;
                        var code = request.QueryString["code"];
                        var response = context.Response;
                        string responseString = "<html><body>Токен получен, можете закрыть окно.</body></html>";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                        listener.Stop();
                        if (string.IsNullOrEmpty(code))
                        {
                            tcs.SetException(new Exception("Код авторизации не получен."));
                        }
                        else
                        {
                            tcs.SetResult(code);
                        }
                    }
                    catch (Exception ex)
                    {
                        listener.Stop();
                        tcs.SetException(ex);
                    }
                });

                return await tcs.Task;
            }
        }

        private async Task<JObject> GetTokensFromCode(string clientId, string code, string redirectUri)
        {
            using (var httpClient = new HttpClient())
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("scope", "https://outlook.office.com/IMAP.AccessAsUser.All https://outlook.office.com/SMTP.Send offline_access")
                });

                var response = await httpClient.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", requestContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Ошибка при получении токена: {response.StatusCode}. Ответ сервера: {responseString}");
                    throw new Exception($"Ошибка при получении токена: {response.StatusCode}. Ответ: {responseString}");
                }

                try
                {
                    return JObject.Parse(responseString);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка парсинга JSON ответа сервера: {ex.Message}. Ответ: {responseString}");
                    throw;
                }
            }
        }

        private void ShowAddAccountForm(object sender, EventArgs e)
        {
            using (Form addForm = new Form { Text = "Добавить аккаунт", Size = new System.Drawing.Size(300, 510) })
            {
                int yPosition = 10;
                const int fieldHeight = 30;
                const int labelWidth = 100;
                const int inputWidth = 160;

                Label serverLabel = new Label { Text = "Почтовый сервис:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth };
                ComboBox serverBox = new ComboBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth };
                serverBox.Items.AddRange(mailServers.ToArray());
                serverBox.SelectedIndex = 0;
                yPosition += fieldHeight;

                Label protocolLabel = new Label { Text = "Протокол:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth };
                ComboBox protocolBox = new ComboBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth };
                protocolBox.Items.AddRange(new[] { "IMAP", "POP3" });
                protocolBox.SelectedIndex = 0;
                yPosition += fieldHeight;

                Label emailLabel = new Label { Text = "Логин (Email):", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth };
                TextBox emailBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth };
                yPosition += fieldHeight;

                CheckBox useOAuth2CheckBox = new CheckBox { Text = "Использовать OAuth2", Location = new System.Drawing.Point(10, yPosition), Width = 150 };
                yPosition += fieldHeight;

                Label passwordLabel = new Label { Text = "Пароль:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth };
                TextBox passwordBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, UseSystemPasswordChar = true };
                
                // Добавляем обработчик клика на поле пароля для Gmail
                passwordBox.Click += (s, ev) =>
                {
                    var selectedServer = (MailServer)serverBox.SelectedItem;
                    if (selectedServer != null && selectedServer.Name == "Gmail")
                    {
                        string gmailMessage = "Для получения пароля для сторонних приложений почта Gmail требует перехода по той ссылке:\n\nhttps://myaccount.google.com/apppasswords\n\nПосле создания и получения пароля введите его в соответствующее поле.\n\nСсылка скопирована в ваш буфер обмена!";
                        
                        try
                        {
                            Clipboard.SetText("https://myaccount.google.com/apppasswords");
                            MessageBox.Show(gmailMessage, "Информация о Gmail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch
                        {
                            // Если не удалось скопировать в буфер, показываем сообщение без уведомления о копировании
                            MessageBox.Show(gmailMessage.Replace("\n\nСсылка скопирована в ваш буфер обмена!", ""), "Информация о Gmail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };
                
                yPosition += fieldHeight;

                Label oauthTokenLabel = new Label { Text = "OAuth2 Токен:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth };
                TextBox oauthTokenBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, Enabled = false };
                yPosition += fieldHeight;

                Button getTokenButton = new Button { Text = "Получить токен", Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, Enabled = false };
                yPosition += fieldHeight;

                Label smtpEncryptionLabel = new Label { Text = "SMTP Шифрование:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth };
                ComboBox smtpEncryptionBox = new ComboBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth };
                smtpEncryptionBox.Items.AddRange(new[] { "SSL/TLS", "STARTTLS", "None" });
                smtpEncryptionBox.SelectedIndex = 0;
                yPosition += fieldHeight;

                Label imapServerLabel = new Label { Text = "IMAP Сервер:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth, Visible = false };
                TextBox imapServerBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, Visible = false };
                yPosition += fieldHeight;

                Label imapPortLabel = new Label { Text = "IMAP Порт:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth, Visible = false };
                TextBox imapPortBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, Visible = false };
                yPosition += fieldHeight;

                Label pop3ServerLabel = new Label { Text = "POP3 Сервер:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth, Visible = false };
                TextBox pop3ServerBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, Visible = false };
                yPosition += fieldHeight;

                Label pop3PortLabel = new Label { Text = "POP3 Порт:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth, Visible = false };
                TextBox pop3PortBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, Visible = false };
                yPosition += fieldHeight;

                Label smtpServerLabel = new Label { Text = "SMTP Сервер:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth, Visible = false };
                TextBox smtpServerBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, Visible = false };
                yPosition += fieldHeight;

                Label smtpPortLabel = new Label { Text = "SMTP Порт:", Location = new System.Drawing.Point(10, yPosition), Width = labelWidth, Visible = false };
                TextBox smtpPortBox = new TextBox { Location = new System.Drawing.Point(110, yPosition), Width = inputWidth, Visible = false };
                yPosition += fieldHeight;

                Button saveButton = new Button { Text = "Сохранить", Location = new System.Drawing.Point(10, yPosition), Width = 100 };

                // Инициализируем состояние чекбокса OAuth2 для Gmail (выбран по умолчанию)
                var defaultServer = (MailServer)serverBox.SelectedItem;
                if (defaultServer.Name == "Gmail")
                {
                    useOAuth2CheckBox.Enabled = false;
                    useOAuth2CheckBox.Checked = false;
                }

                useOAuth2CheckBox.CheckedChanged += (s, ev) =>
                {
                    passwordBox.Enabled = !useOAuth2CheckBox.Checked;
                    oauthTokenBox.Enabled = useOAuth2CheckBox.Checked;
                    getTokenButton.Enabled = useOAuth2CheckBox.Checked;
                };

                serverBox.SelectedIndexChanged += (s, ev) =>
                {
                    var selectedServer = (MailServer)serverBox.SelectedItem;
                    
                    bool isCustom = selectedServer.Name == "Пользовательский";
                    imapServerLabel.Visible = isCustom;
                    imapServerBox.Visible = isCustom;
                    imapPortLabel.Visible = isCustom;
                    imapPortBox.Visible = isCustom;
                    pop3ServerLabel.Visible = isCustom;
                    pop3ServerBox.Visible = isCustom;
                    pop3PortLabel.Visible = isCustom;
                    pop3PortBox.Visible = isCustom;
                    smtpServerLabel.Visible = isCustom;
                    smtpServerBox.Visible = isCustom;
                    smtpPortLabel.Visible = isCustom;
                    smtpPortBox.Visible = isCustom;

                    // Для Gmail отключаем OAuth2 и сбрасываем чекбокс
                    if (selectedServer.Name == "Gmail")
                    {
                        useOAuth2CheckBox.Enabled = false;
                        useOAuth2CheckBox.Checked = false;
                    }
                    else
                    {
                        useOAuth2CheckBox.Enabled = selectedServer.SupportsOAuth2;
                        if (!selectedServer.SupportsOAuth2)
                        {
                            useOAuth2CheckBox.Checked = false;
                        }
                    }

                    if (selectedServer.SmtpEncryption == "STARTTLS")
                    {
                        smtpEncryptionBox.SelectedIndex = 1;
                        if (!isCustom) smtpPortBox.Text = "587";
                    }
                    else if (selectedServer.SmtpEncryption == "None")
                    {
                        smtpEncryptionBox.SelectedIndex = 2;
                        if (!isCustom) smtpPortBox.Text = selectedServer.Name == "ProtonMail (via Bridge)" ? "1025" : "25";
                    }
                    else
                    {
                        smtpEncryptionBox.SelectedIndex = 0;
                        if (!isCustom) smtpPortBox.Text = "465";
                    }

                    bool usePop3 = protocolBox.SelectedItem?.ToString() == "POP3";
                    imapServerLabel.Visible = isCustom && !usePop3;
                    imapServerBox.Visible = isCustom && !usePop3;
                    imapPortLabel.Visible = isCustom && !usePop3;
                    imapPortBox.Visible = isCustom && !usePop3;
                    pop3ServerLabel.Visible = isCustom && usePop3;
                    pop3ServerBox.Visible = isCustom && usePop3;
                    pop3PortLabel.Visible = isCustom && usePop3;
                    pop3PortBox.Visible = isCustom && usePop3;
                };

                protocolBox.SelectedIndexChanged += (s, ev) =>
                {
                    var selectedServer = (MailServer)serverBox.SelectedItem;
                    bool isCustom = selectedServer.Name == "Пользовательский";
                    bool usePop3 = protocolBox.SelectedItem?.ToString() == "POP3";
                    imapServerLabel.Visible = isCustom && !usePop3;
                    imapServerBox.Visible = isCustom && !usePop3;
                    imapPortLabel.Visible = isCustom && !usePop3;
                    imapPortBox.Visible = isCustom && !usePop3;
                    pop3ServerLabel.Visible = isCustom && usePop3;
                    pop3ServerBox.Visible = isCustom && usePop3;
                    pop3PortLabel.Visible = isCustom && usePop3;
                    pop3PortBox.Visible = isCustom && usePop3;
                };

                smtpEncryptionBox.SelectedIndexChanged += (s, ev) =>
                {
                    var selectedServer = (MailServer)serverBox.SelectedItem;
                    bool isCustom = selectedServer.Name == "Пользовательский";
                    if (isCustom)
                    {
                        if (smtpEncryptionBox.SelectedItem?.ToString() == "STARTTLS")
                            smtpPortBox.Text = "587";
                        else if (smtpEncryptionBox.SelectedItem?.ToString() == "SSL/TLS")
                            smtpPortBox.Text = "465";
                        else if (smtpEncryptionBox.SelectedItem?.ToString() == "None")
                            smtpPortBox.Text = "25";
                    }
                };

                getTokenButton.Click += async (s, ev) =>
                {
                    var selectedServer = (MailServer)serverBox.SelectedItem;
                    if (selectedServer.Name != "Outlook.com")
                    {
                        MessageBox.Show("Токен можно получить только для Outlook.com.");
                        return;
                    }

                    try
                    {
                        string clientId = "0971cc7d-0742-42ef-b3cf-35e483ae40d2";
                        string redirectUri = "http://localhost:8000";
                        string scope = "https://outlook.office.com/IMAP.AccessAsUser.All https://outlook.office.com/SMTP.Send offline_access";
                        string authUrl = $"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scope)}";

                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = authUrl,
                            UseShellExecute = true
                        });

                        string authCode = await StartLocalServerForCode(redirectUri);
                        var tokens = await GetTokensFromCode(clientId, authCode, redirectUri);
                        oauthTokenBox.Text = tokens["access_token"]?.ToString();
                        string refreshToken = tokens["refresh_token"]?.ToString();
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            MessageBox.Show($"Refresh-токен получен: {TruncateString(refreshToken)}");
                            addForm.Tag = refreshToken;
                        }
                        else
                        {
                            MessageBox.Show("Refresh-токен не был получен от сервера.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка получения токена: {ex.Message}");
                    }
                };

                saveButton.Click += async (s, ev) =>
                {
                    try
                    {
                        var selectedServer = (MailServer)serverBox.SelectedItem;
                        bool usePop3 = protocolBox.SelectedItem?.ToString() == "POP3";
                        bool isCustom = selectedServer.Name == "Пользовательский";

                        if (string.IsNullOrWhiteSpace(emailBox.Text) || !emailBox.Text.Contains("@"))
                        {
                            MessageBox.Show("Пожалуйста, введите корректный email.");
                            return;
                        }

                        if (!useOAuth2CheckBox.Checked && string.IsNullOrWhiteSpace(passwordBox.Text))
                        {
                            MessageBox.Show("Пожалуйста, введите пароль.");
                            return;
                        }

                        if (useOAuth2CheckBox.Checked && string.IsNullOrWhiteSpace(oauthTokenBox.Text))
                        {
                            MessageBox.Show("Пожалуйста, введите OAuth2 токен или используйте кнопку 'Получить токен'.");
                            return;
                        }

                        int imapPort = 0, pop3Port = 0, smtpPort = 0;
                        if (isCustom)
                        {
                            if (!usePop3)
                            {
                                if (string.IsNullOrWhiteSpace(imapServerBox.Text) || !int.TryParse(imapPortBox.Text, out imapPort) || imapPort <= 0)
                                {
                                    MessageBox.Show("Пожалуйста, введите корректные IMAP данные.");
                                    return;
                                }
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(pop3ServerBox.Text) || !int.TryParse(pop3PortBox.Text, out pop3Port) || pop3Port <= 0)
                                {
                                    MessageBox.Show("Пожалуйста, введите корректные POP3 данные.");
                                    return;
                                }
                            }

                            if (string.IsNullOrWhiteSpace(smtpServerBox.Text) || !int.TryParse(smtpPortBox.Text, out smtpPort) || smtpPort <= 0)
                            {
                                MessageBox.Show("Пожалуйста, введите корректные SMTP данные.");
                                return;
                            }
                        }
                        else
                        {
                            smtpPort = selectedServer.SmtpPort;
                        }

                        string finalEncryption = smtpEncryptionBox.SelectedItem?.ToString();

                        Account account = new Account
                        {
                            Email = emailBox.Text,
                            Password = useOAuth2CheckBox.Checked ? "" : passwordBox.Text,
                            OAuth2Token = useOAuth2CheckBox.Checked ? oauthTokenBox.Text : "",
                            RefreshToken = useOAuth2CheckBox.Checked ? (addForm.Tag as string) : "",
                            UseOAuth2 = useOAuth2CheckBox.Checked,
                            UsePop3 = usePop3,
                            ImapServer = isCustom && !usePop3 ? imapServerBox.Text : selectedServer.ImapServer,
                            ImapPort = isCustom && !usePop3 ? imapPort : selectedServer.ImapPort,
                            Pop3Server = isCustom && usePop3 ? pop3ServerBox.Text : selectedServer.Pop3Server,
                            Pop3Port = isCustom && usePop3 ? pop3Port : selectedServer.Pop3Port,
                            SmtpServer = isCustom ? smtpServerBox.Text : selectedServer.SmtpServer,
                            SmtpPort = smtpPort,
                            SmtpEncryption = finalEncryption
                        };

                        MessageBox.Show($"Сохранение аккаунта {account.Email}. Access-токен: {TruncateString(account.OAuth2Token)}, Refresh-токен: {TruncateString(account.RefreshToken)}");

                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        if (await TestConnectionAsync(account, cts.Token))
                        {
                            accounts.Add(account);
                            accountSelector.Items.Add(account.Email);
                            SaveAccounts();
                            MessageBox.Show("Аккаунт успешно добавлен!");
                            addForm.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка подключения. Проверьте данные.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении аккаунта: {ex.Message}");
                    }
                };

                addForm.Controls.Add(serverLabel);
                addForm.Controls.Add(serverBox);
                addForm.Controls.Add(protocolLabel);
                addForm.Controls.Add(protocolBox);
                addForm.Controls.Add(emailLabel);
                addForm.Controls.Add(emailBox);
                addForm.Controls.Add(useOAuth2CheckBox);
                addForm.Controls.Add(passwordLabel);
                addForm.Controls.Add(passwordBox);
                addForm.Controls.Add(oauthTokenLabel);
                addForm.Controls.Add(oauthTokenBox);
                addForm.Controls.Add(getTokenButton);
                addForm.Controls.Add(imapServerLabel);
                addForm.Controls.Add(imapServerBox);
                addForm.Controls.Add(imapPortLabel);
                addForm.Controls.Add(imapPortBox);
                addForm.Controls.Add(pop3ServerLabel);
                addForm.Controls.Add(pop3ServerBox);
                addForm.Controls.Add(pop3PortLabel);
                addForm.Controls.Add(pop3PortBox);
                addForm.Controls.Add(smtpServerLabel);
                addForm.Controls.Add(smtpServerBox);
                addForm.Controls.Add(smtpPortLabel);
                addForm.Controls.Add(smtpPortBox);
                addForm.Controls.Add(smtpEncryptionLabel);
                addForm.Controls.Add(smtpEncryptionBox);
                addForm.Controls.Add(saveButton);
                addForm.ShowDialog();
            }
        }

        private async void AccountSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadFoldersAndEmailsAsync(sender, e);
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            await LoadFoldersAndEmailsAsync(sender, e);
        }
    }

    [Serializable]
    public class Account
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string OAuth2Token { get; set; }
        public string RefreshToken { get; set; }
        public bool UseOAuth2 { get; set; }
        public bool UsePop3 { get; set; }
        public string ImapServer { get; set; }
        public int ImapPort { get; set; }
        public string Pop3Server { get; set; }
        public int Pop3Port { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpEncryption { get; set; }
    }
}