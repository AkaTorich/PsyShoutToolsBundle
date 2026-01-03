using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrycryperClient
{
    public partial class Form1 : Form
    {
        private UdpClient _udpClient;
        private RSACryptoServiceProvider _rsa;
        private Aes _aes;
        private IPEndPoint _serverEndPoint;
        private string _nickName;
        private bool _isConnected = false;
        private System.Threading.CancellationTokenSource _pingTokenSource;

        // Переменные для эмодзи
        private bool _emojiPanelShown = false;
        private Dictionary<string, string> _emojiDict;

        // Переменные для передачи файлов
        private string _selectedFilePath = string.Empty;
        private const int CHUNK_SIZE = 32768; // 32KB chunks
        private Dictionary<string, FileTransferInfo> _activeFileTransfers;
        private Dictionary<string, ReceivedFileInfo> _receivedFiles;
        private System.Collections.Concurrent.ConcurrentQueue<string> _fileChunkQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private System.Collections.Concurrent.ConcurrentQueue<string> _messageQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private CancellationTokenSource _fileProcessingCancellation = new CancellationTokenSource();
        private CancellationTokenSource _messageProcessingCancellation = new CancellationTokenSource();

        /// <summary>
        /// Информация о передаваемом файле
        /// </summary>
        private class FileTransferInfo
        {
            public string FileId { get; set; }
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public int TotalChunks { get; set; }
            public byte[] FileData { get; set; }
            public DateTime StartTime { get; set; }
            public int SentChunks { get; set; } = 0;
        }

        /// <summary>
        /// Информация о получаемом файле
        /// </summary>
        private class ReceivedFileInfo
        {
            public string FileId { get; set; }
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public int TotalChunks { get; set; }
            public byte[] FileData { get; set; }
            public DateTime StartTime { get; set; }
            public int ReceivedChunks { get; set; } = 0;
            public bool IsComplete { get; set; } = false;
            public HashSet<int> ReceivedChunkNumbers { get; set; } = new HashSet<int>();
            public DateTime LastMissingChunksRequestTime { get; set; } = DateTime.MinValue;
            public int MissingChunksRequestCount { get; set; } = 0;
        }

        public Form1()
        {
            InitializeComponent();

            // Блокируем изменение размера окна
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            _rsa = new RSACryptoServiceProvider(2048);
            _aes = Aes.Create();
            _aes.KeySize = 256;
            _aes.GenerateKey();
            _aes.GenerateIV();

            // Добавляем обработчик события для отправки сообщения по нажатию Enter
            messageRichTextBox.KeyDown += MessageRichTextBox_KeyDown;

            // Инициализируем панель эмодзи
            InitializeEmojis();

            // Добавляем обработчик для очистки подсказки при фокусе
            messageRichTextBox.Enter += MessageRichTextBox_Enter;

            // Устанавливаем шрифт для поля ввода сообщений, поддерживающий эмодзи
            messageRichTextBox.Font = new Font("Segoe UI Emoji", 12, FontStyle.Regular);

            // Добавляем обработчик закрытия формы
            this.FormClosing += Form1_FormClosing;

            // Инициализируем словарь для активных передач файлов
            _activeFileTransfers = new Dictionary<string, FileTransferInfo>();
            _receivedFiles = new Dictionary<string, ReceivedFileInfo>();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Отключаем клиент перед закрытием
            DisconnectFromServer();
        }

        private async void DisconnectFromServer()
        {
            if (_udpClient != null && _isConnected && _serverEndPoint != null)
            {
                try
                {
                    // Останавливаем проверку соединения
                    _pingTokenSource?.Cancel();

                    // Отправляем сообщение о отключении
                    var disconnectMessage = "DISCONNECT:" + _nickName;
                    var disconnectBytes = Encoding.UTF8.GetBytes(disconnectMessage);
                    await _udpClient.SendAsync(disconnectBytes, disconnectBytes.Length, _serverEndPoint);
                    UpdateStatus("Disconnected from server.", Color.DarkBlue);
                }
                catch (Exception ex)
                {
                    UpdateStatus("Error while disconnecting: " + ex.Message, Color.Red);
                }
                finally
                {
                    _isConnected = false;
                    sendFileButton.Enabled = false; // Деактивируем кнопку отправки файла
                    
                    // Останавливаем все процессоры
                    _messageProcessingCancellation?.Cancel();
                    _messageProcessingCancellation?.Dispose();
                    _fileProcessingCancellation?.Cancel();
                    _fileProcessingCancellation?.Dispose();
                    
                    _udpClient?.Close();
                }
            }
        }

        private void MessageRichTextBox_Enter(object sender, EventArgs e)
        {
            if (messageRichTextBox.Text == "Type message...")
            {
                messageRichTextBox.Text = string.Empty;
            }
        }

        private void InitializeEmojis()
        {
            // Словарь соответствия текстовых кодов и UTF-8 эмодзи
            _emojiDict = new Dictionary<string, string>
            {
                // Смайлы
                { ":)", "😊" }, { ":-)", "😊" }, { ":D", "😃" }, { ":-D", "😃" },
                { ":(", "😞" }, { ":-(", "😞" }, { ";)", "😉" }, { ";-)", "😉" },
                { ":P", "😛" }, { ":-P", "😛" }, { ":O", "😮" }, { ":-O", "😮" },
                { ":|", "😐" }, { ":-|", "😐" }, { ":*", "😘" }, { ":-*", "😘" },
                
                // Эмоции
                { "<3", "❤️" }, { ":heart:", "❤️" }, { ":love:", "😍" }, { ":angry:", "😠" },
                { ":cry:", "😢" }, { ":'(", "😢" }, { ":lol:", "🤣" }, { ":XD:", "🤣" },
                { ":cool:", "😎" }, { "B)", "😎" }, { "8)", "😎" }, { ":fear:", "😱" },
                { ":blush:", "😊" }, { ":sad:", "😔" }, { ":sleepy:", "😴" }, { ":confused:", "😕" },
                
                // Жесты
                { ":thumbsup:", "👍" }, { ":+1:", "👍" }, { ":thumbsdown:", "👎" }, { ":-1:", "👎" },
                { ":ok:", "👌" }, { ":victory:", "✌️" }, { ":clap:", "👏" }, { ":wave:", "👋" },
                { ":hand:", "✋" }, { ":fist:", "👊" }, { ":muscle:", "💪" }, { ":pray:", "🙏" },
                
                // Объекты и животные
                { ":star:", "⭐" }, { ":fire:", "🔥" }, { ":sun:", "☀️" }, { ":moon:", "🌙" },
                { ":cat:", "🐱" }, { ":dog:", "🐶" }, { ":mouse:", "🐭" }, { ":bunny:", "🐰" },
                { ":flower:", "🌸" }, { ":rose:", "🌹" }, { ":gift:", "🎁" }, { ":beer:", "🍺" },
                { ":coffee:", "☕" }, { ":pizza:", "🍕" }, { ":cake:", "🍰" }, { ":banana:", "🍌" }
            };

            // Настройка панели эмодзи - делаем её шире
            if (emojiPanel == null)
            {
                emojiPanel = new Panel();
                emojiPanel.BorderStyle = BorderStyle.FixedSingle;
                emojiPanel.BackColor = Color.White;
                emojiPanel.Size = new Size(500, 300); // Увеличиваем размер панели
                emojiPanel.Location = new Point(9, 600); // Располагаем над полем ввода
                emojiPanel.Visible = false;
                emojiPanel.AutoScroll = true;
                this.Controls.Add(emojiPanel);
            }

            // Создаем вкладки для категорий эмодзи
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            emojiPanel.Controls.Add(tabControl);

            // Создаем категории эмодзи
            Dictionary<string, List<string>> emojiCategories = new Dictionary<string, List<string>>();

            // Категория "Смайлы"
            emojiCategories["Смайлы"] = new List<string> {
                "😊", "😃", "😄", "😁", "😆", "😅", "😂", "🤣", "😉", "😌",
                "😍", "😘", "😗", "😙", "😚", "😋", "😜", "😝", "😛", "😎"
            };

            // Категория "Эмоции"
            emojiCategories["Эмоции"] = new List<string> {
                "😠", "😡", "😨", "😱", "😢", "😭", "😰", "😓", "😞", "😔",
                "😕", "😖", "😣", "😩", "😫", "😤", "😮", "😯", "😲", "😴"
            };

            // Категория "Жесты"
            emojiCategories["Жесты"] = new List<string> {
                "👍", "👎", "👌", "✌️", "🤞", "🤟", "🤘", "🤙", "👈", "👉",
                "👆", "👇", "👋", "🖐️", "🤚", "✋", "🖖", "👏", "🙌", "👐"
            };

            // Категория "Сердца"
            emojiCategories["Сердца"] = new List<string> {
                "❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "🤍", "🤎", "💔",
                "💕", "💖", "💗", "💓", "💞", "💘", "💝", "💟", "☮️", "✝️"
            };

            // Категория "Животные"
            emojiCategories["Животные"] = new List<string> {
                "🐶", "🐱", "🐭", "🐹", "🐰", "🦊", "🐻", "🐼", "🐨", "🐯",
                "🦁", "🐮", "🐷", "🐸", "🐵", "🙈", "🙉", "🙊", "🐒", "🐔"
            };

            // Категория "Еда"
            emojiCategories["Еда"] = new List<string> {
                "🍏", "🍎", "🍐", "🍊", "🍋", "🍌", "🍉", "🍇", "🍓", "🍈",
                "🍒", "🍑", "🥭", "🍍", "🥥", "🥝", "🍅", "🍆", "🥑", "🥦"
            };

            // Создаем вкладки и добавляем эмодзи
            foreach (var category in emojiCategories)
            {
                TabPage tabPage = new TabPage(category.Key);
                tabControl.TabPages.Add(tabPage);

                FlowLayoutPanel flowPanel = new FlowLayoutPanel();
                flowPanel.Dock = DockStyle.Fill;
                flowPanel.AutoScroll = true;
                tabPage.Controls.Add(flowPanel);

                // Добавляем кнопки с эмодзи в каждую категорию
                foreach (var emoji in category.Value)
                {
                    Button emojiButton = new Button();
                    emojiButton.Text = emoji;
                    emojiButton.Font = new Font("Segoe UI Emoji", 24);  // Увеличиваем размер шрифта для эмодзи
                    emojiButton.Size = new Size(60, 60);  // Увеличиваем размер кнопок
                    emojiButton.FlatStyle = FlatStyle.Flat;
                    emojiButton.FlatAppearance.BorderSize = 0;
                    emojiButton.Margin = new Padding(2);
                    emojiButton.Click += (s, e) => {
                        // Вставляем эмодзи напрямую
                        messageRichTextBox.SelectedText = emoji + " ";
                        messageRichTextBox.Focus();
                    };

                    flowPanel.Controls.Add(emojiButton);
                }
            }

            // Кнопка для открытия/закрытия панели эмодзи
            if (emojiToggleButton == null)
            {
                emojiToggleButton = new Button();
                emojiToggleButton.Text = "😊";
                emojiToggleButton.Font = new Font("Segoe UI Emoji", 24);  // Увеличиваем размер шрифта
                emojiToggleButton.Size = new Size(60, 118);
                emojiToggleButton.Location = new Point(1314, 905);
                emojiToggleButton.Click += EmojiToggleButton_Click;
                this.Controls.Add(emojiToggleButton);
            }
        }

        private void EmojiToggleButton_Click(object sender, EventArgs e)
        {
            _emojiPanelShown = !_emojiPanelShown;
            emojiPanel.Visible = _emojiPanelShown;

            if (_emojiPanelShown)
            {
                emojiPanel.BringToFront();
            }
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                _udpClient = new UdpClient();
                _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp.Text), int.Parse(serverPort.Text));
                _nickName = nickNameTextBox.Text;

                // Отправка никнейма
                var nickNameMessage = "NICKNAME:" + _nickName;
                var nickNameBytes = Encoding.UTF8.GetBytes(nickNameMessage);
                await _udpClient.SendAsync(nickNameBytes, nickNameBytes.Length, _serverEndPoint);
                UpdateStatus("Nickname sent to server.", Color.DarkBlue);

                // Ожидание публичного ключа от сервера
                var receivedData = await _udpClient.ReceiveAsync();
                var publicKeyXml = Encoding.UTF8.GetString(receivedData.Buffer);
                UpdateStatus($"Received public key from server: {publicKeyXml}", Color.DarkGreen);

                // Импорт публичного ключа сервера
                var serverRsa = new RSACryptoServiceProvider();
                serverRsa.FromXmlString(publicKeyXml);

                // Шифрование и отправка AES ключа и IV
                var encryptedAesKey = serverRsa.Encrypt(_aes.Key, false);
                var aesKeyMessage = "AES_KEY:" + Convert.ToBase64String(encryptedAesKey);
                var aesKeyBytes = Encoding.UTF8.GetBytes(aesKeyMessage);
                await _udpClient.SendAsync(aesKeyBytes, aesKeyBytes.Length, _serverEndPoint);
                UpdateStatus($"AES key sent to server: {Convert.ToBase64String(_aes.Key)}", Color.DarkBlue);

                var ivMessage = "AES_IV:" + Convert.ToBase64String(_aes.IV);
                var ivBytes = Encoding.UTF8.GetBytes(ivMessage);
                await _udpClient.SendAsync(ivBytes, ivBytes.Length, _serverEndPoint);
                UpdateStatus($"AES IV sent to server: {Convert.ToBase64String(_aes.IV)}", Color.DarkBlue);

                // Успешное подключение
                _isConnected = true;
                sendFileButton.Enabled = !string.IsNullOrEmpty(_selectedFilePath); // Активируем кнопку отправки файла

                // Пересоздаем токены отмены для обработки сообщений и файлов
                _messageProcessingCancellation?.Dispose();
                _messageProcessingCancellation = new CancellationTokenSource();
                _fileProcessingCancellation?.Dispose();
                _fileProcessingCancellation = new CancellationTokenSource();

                // Начало прослушивания сообщений от сервера
                _ = Task.Run(() => ListenForMessages());

                // Запуск процессоров очередей в отдельных потоках
                _ = Task.Run(() => ProcessMessageQueue());
                _ = Task.Run(() => ProcessFileChunkQueue());

                // Запуск периодической проверки соединения
                StartPingServer();
            }
            catch (Exception ex)
            {
                UpdateStatus("Error in connectButton_Click: " + ex.Message, Color.Red);
            }
        }

        private void StartPingServer()
        {
            // Отменяем предыдущую задачу, если она существует
            _pingTokenSource?.Cancel();
            _pingTokenSource = new System.Threading.CancellationTokenSource();

            // Запускаем новую задачу пинга
            Task.Run(async () =>
            {
                try
                {
                    while (!_pingTokenSource.Token.IsCancellationRequested && _isConnected)
                    {
                        try
                        {
                            // Отправляем PING каждые 10 секунд
                            var pingBytes = Encoding.UTF8.GetBytes("PING");
                            await _udpClient.SendAsync(pingBytes, pingBytes.Length, _serverEndPoint);

                            // Ждем 10 секунд до следующего пинга
                            await Task.Delay(10000, _pingTokenSource.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            // Задача была отменена, выходим из цикла
                            break;
                        }
                        catch (Exception ex)
                        {
                            // Ошибка при отправке пинга, возможно соединение потеряно
                            if (_isConnected)
                            {
                                UpdateStatus("Connection to server may be lost: " + ex.Message, Color.Orange);
                                // Пробуем еще раз через некоторое время
                                await Task.Delay(2000, _pingTokenSource.Token);
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Задача отменена, ничего делать не нужно
                }
                catch (Exception ex)
                {
                    UpdateStatus("Error in ping task: " + ex.Message, Color.Red);
                }
            }, _pingTokenSource.Token);
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            await SendMessage();
        }

        private async Task SendMessage()
        {
            try
            {
                var message = messageRichTextBox.Text;
                if (string.IsNullOrWhiteSpace(message) || message == "Type message..." || !_isConnected)
                {
                    return; // Не отправляем пустое сообщение или если не подключены
                }

                var fullMessage = $"{_nickName}: {message}"; // Добавляем ник к сообщению

                // Реализация отправки зашифрованного сообщения
                var encryptedMessage = EncryptWithAES(fullMessage, _aes.Key, _aes.IV);
                var encryptedMessageBase64 = Convert.ToBase64String(encryptedMessage);
                var encryptedMessageBytes = Encoding.UTF8.GetBytes(encryptedMessageBase64);
                await _udpClient.SendAsync(encryptedMessageBytes, encryptedMessageBytes.Length, _serverEndPoint);
                UpdateStatus($"{_nickName}: {message}", Color.DarkBlue); // Отображаем расшифрованное сообщение
                messageRichTextBox.Clear(); // Очистка поля ввода после отправки
            }
            catch (Exception ex)
            {
                UpdateStatus("Error in sendButton_Click: " + ex.Message, Color.Red);

                // Если не удалось отправить сообщение, возможно соединение потеряно
                if (ex is SocketException || ex.InnerException is SocketException)
                {
                    UpdateStatus("Connection to server lost. Please reconnect.", Color.Red);
                    _isConnected = false;
                    if (InvokeRequired)
                    {
                        this.Invoke(new Action(() => sendFileButton.Enabled = false));
                    }
                    else
                    {
                        sendFileButton.Enabled = false;
                    }
                }
            }
        }

        private void MessageRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Предотвращаем добавление новой строки
                sendButton.PerformClick();  // Имитация нажатия на кнопку отправки
            }
        }

        private async Task ListenForMessages()
        {
            try
            {
                while (_isConnected)
                {
                    try
                    {
                        // Быстрое чтение UDP пакетов без таймаутов
                        var result = await _udpClient.ReceiveAsync();
                        var receivedMessageBase64 = Encoding.UTF8.GetString(result.Buffer);

                        // Быстрая проверка PONG без обработки
                        if (receivedMessageBase64 == "PONG")
                        {
                            continue; // Игнорируем PONG сообщения
                        }

                        // ВСЕ сообщения в единую очередь (нельзя определить тип до расшифровки!)
                        _messageQueue.Enqueue(receivedMessageBase64);
                    }
                    catch (Exception ex)
                    {
                        // Если произошла ошибка при получении сообщения, проверяем статус подключения
                        if (_isConnected)
                        {
                            _ = Task.Run(() => UpdateStatus("Error receiving message: " + ex.Message, Color.Red));
                            await Task.Delay(100); // Короткая пауза перед следующей попыткой
                        }
                        else
                        {
                            break; // Выходим из цикла, если клиент отключен
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = Task.Run(() => UpdateStatus("Error in ListenForMessages: " + ex.Message, Color.Red));
            }
            finally
            {
                _isConnected = false;
                if (InvokeRequired)
                {
                    this.Invoke(new Action(() => sendFileButton.Enabled = false));
                }
                else
                {
                    sendFileButton.Enabled = false;
                }
            }
        }

        private byte[] EncryptWithAES(string message, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var sw = new System.IO.StreamWriter(cs))
                            {
                                sw.Write(message);
                            }
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        private string DecryptWithAES(byte[] encryptedMessage, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (var ms = new System.IO.MemoryStream(encryptedMessage))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new System.IO.StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        public void UpdateStatus(string message, Color color)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string, Color>(UpdateStatus), new object[] { message, color });
                return;
            }

            // Добавляем временную метку
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var messageWithTime = $"[{timestamp}] {message}";

            // Устанавливаем позицию и цвет текста
            clientStatus.SelectionStart = clientStatus.TextLength;
            clientStatus.SelectionLength = 0;
            clientStatus.SelectionColor = color;

            // Добавляем сообщение с временной меткой
            clientStatus.AppendText(messageWithTime + Environment.NewLine);

            // Прокрутка к последней строке
            clientStatus.SelectionStart = clientStatus.TextLength;
            clientStatus.ScrollToCaret();
        }

        private void serverIp_TextChanged(object sender, EventArgs e) { }

        private void serverIpLabel_Click(object sender, EventArgs e) { }

        private void serverPortLabel_Click(object sender, EventArgs e) { }

        private void serverPort_TextChanged(object sender, EventArgs e) { }

        private void nickName_Click(object sender, EventArgs e) { }

        private void nickNameTextBox_TextChanged(object sender, EventArgs e) { }

        private void messageRichTextBox_TextChanged(object sender, EventArgs e) { }

        #region File Transfer Methods

        private void selectFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Select file to send";
                    openFileDialog.Filter = "All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _selectedFilePath = openFileDialog.FileName;
                        var fileInfo = new FileInfo(_selectedFilePath);
                        selectedFileLabel.Text = $"Selected: {fileInfo.Name} ({FormatFileSize(fileInfo.Length)})";
                        sendFileButton.Enabled = _isConnected && !string.IsNullOrEmpty(_selectedFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error selecting file: " + ex.Message, Color.Red);
            }
        }

        private async void sendFileButton_Click(object sender, EventArgs e)
        {
            if (!_isConnected || string.IsNullOrEmpty(_selectedFilePath))
            {
                UpdateStatus("Not connected or no file selected.", Color.Red);
                return;
            }

            try
            {
                sendFileButton.Enabled = false;
                await SendFileAsync(_selectedFilePath);
            }
            catch (Exception ex)
            {
                UpdateStatus("Error sending file: " + ex.Message, Color.Red);
            }
            finally
            {
                sendFileButton.Enabled = _isConnected && !string.IsNullOrEmpty(_selectedFilePath);
            }
        }

        private async Task SendFileAsync(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var fileId = Guid.NewGuid().ToString();
                var fileData = File.ReadAllBytes(filePath);
                var totalChunks = (int)Math.Ceiling((double)fileData.Length / CHUNK_SIZE);

                // Создаем информацию о передаче файла
                var transferInfo = new FileTransferInfo
                {
                    FileId = fileId,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    TotalChunks = totalChunks,
                    FileData = fileData,
                    StartTime = DateTime.Now
                };

                _activeFileTransfers[fileId] = transferInfo;

                UpdateStatus($"📤 Starting file transfer: {fileInfo.Name} ({FormatFileSize(fileInfo.Length)})", Color.Blue);
                ShowFileProgress("Preparing to send...");
                
                // Освобождаем UI поток для обеспечения отзывчивости чата
                await Task.Yield();

                // Отправляем сообщение о начале передачи файла
                var fileTransferStart = new FileTransferModels.FileTransferStart
                {
                    FileId = fileId,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    TotalChunks = totalChunks
                };

                var startMessage = "FILE_TRANSFER_START:" + JsonConvert.SerializeObject(fileTransferStart);
                var encryptedStartMessage = EncryptWithAES(startMessage, _aes.Key, _aes.IV);
                var encryptedStartMessageBase64 = Convert.ToBase64String(encryptedStartMessage);
                var startMessageBytes = Encoding.UTF8.GetBytes(encryptedStartMessageBase64);
                await _udpClient.SendAsync(startMessageBytes, startMessageBytes.Length, _serverEndPoint);

                // Отправляем файл по чанкам
                for (int chunkNumber = 0; chunkNumber < totalChunks; chunkNumber++)
                {
                    var startIndex = chunkNumber * CHUNK_SIZE;
                    var chunkSize = Math.Min(CHUNK_SIZE, fileData.Length - startIndex);
                    var chunkData = new byte[chunkSize];
                    Array.Copy(fileData, startIndex, chunkData, 0, chunkSize);

                    // Вычисляем контрольную сумму чанка
                    var checkSum = ComputeMD5Hash(chunkData);

                    var fileChunk = new FileTransferModels.FileChunk
                    {
                        FileId = fileId,
                        ChunkNumber = chunkNumber,
                        TotalChunks = totalChunks,
                        Data = chunkData,
                        CheckSum = checkSum
                    };

                    var chunkMessage = "FILE_CHUNK:" + JsonConvert.SerializeObject(fileChunk);
                    var encryptedChunkMessage = EncryptWithAES(chunkMessage, _aes.Key, _aes.IV);
                    var encryptedChunkMessageBase64 = Convert.ToBase64String(encryptedChunkMessage);
                    var chunkMessageBytes = Encoding.UTF8.GetBytes(encryptedChunkMessageBase64);
                    await _udpClient.SendAsync(chunkMessageBytes, chunkMessageBytes.Length, _serverEndPoint);

                    transferInfo.SentChunks++;
                    var progress = (double)transferInfo.SentChunks / totalChunks * 100;
                    UpdateFileProgress((int)progress, $"Sending: {progress:F1}%");

                    // Небольшая задержка между чанками, чтобы не перегружать сеть
                    await Task.Delay(10);
                    
                    // Освобождаем UI поток каждые 10 чанков для обеспечения отзывчивости чата
                    if (chunkNumber % 10 == 0)
                    {
                        await Task.Yield();
                    }
                }

                // Отправляем сообщение о завершении передачи файла
                var fileTransferComplete = new FileTransferModels.FileTransferComplete
                {
                    FileId = fileId,
                    Success = true
                };

                var completeMessage = "FILE_TRANSFER_COMPLETE:" + JsonConvert.SerializeObject(fileTransferComplete);
                var encryptedCompleteMessage = EncryptWithAES(completeMessage, _aes.Key, _aes.IV);
                var encryptedCompleteMessageBase64 = Convert.ToBase64String(encryptedCompleteMessage);
                var completeMessageBytes = Encoding.UTF8.GetBytes(encryptedCompleteMessageBase64);
                await _udpClient.SendAsync(completeMessageBytes, completeMessageBytes.Length, _serverEndPoint);

                UpdateStatus($"✅ File transfer completed: {fileInfo.Name}", Color.Green);
                HideFileProgress();

                // НЕ удаляем информацию о передаче сразу - сервер может запросить недостающие чанки
                // Удалим её через 5 МИНУТ (достаточно времени для восстановления)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(300000); // 5 минут = 300 секунд
                    if (_activeFileTransfers.ContainsKey(fileId))
                    {
                _activeFileTransfers.Remove(fileId);
                    }
                });
            }
            catch (Exception ex)
            {
                UpdateStatus("❌ File transfer failed: " + ex.Message, Color.Red);
                HideFileProgress();
            }
        }

        private string ComputeMD5Hash(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                return Convert.ToBase64String(hash);
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";
            else if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024.0):F1} MB";
            else
                return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }

        #region Incoming File Handling

        private void HandleIncomingFileTransferStart(string json)
        {
            try
            {
                var fileTransferStart = JsonConvert.DeserializeObject<FileTransferModels.FileTransferStart>(json);
                if (fileTransferStart == null) return;

                var receivedFileInfo = new ReceivedFileInfo
                {
                    FileId = fileTransferStart.FileId,
                    FileName = fileTransferStart.FileName,
                    FileSize = fileTransferStart.FileSize,
                    TotalChunks = fileTransferStart.TotalChunks,
                    FileData = new byte[fileTransferStart.FileSize],
                    StartTime = DateTime.Now,
                    ReceivedChunks = 0,
                    IsComplete = false
                };

                _receivedFiles[fileTransferStart.FileId] = receivedFileInfo;
                UpdateStatus($"📥 Receiving file: {fileTransferStart.FileName} ({FormatFileSize(fileTransferStart.FileSize)})", Color.Blue);
                ShowFileProgress("Receiving...");
            }
            catch (Exception)
            {
                // Игнорируем ошибки тихо
            }
        }

        private async Task ProcessFileChunkQueue()
        {
            try
            {
                while (!_fileProcessingCancellation.Token.IsCancellationRequested)
                {
                    var processedCount = 0;
                    
                    // Обрабатываем меньше чанков за раз чтобы не блокировать чат
                    while (_fileChunkQueue.TryDequeue(out string chunkData) && processedCount < 5)
                    {
                        try
                        {
                            // Проверяем, что это за тип чанка
                            if (chunkData.StartsWith("MISSING_CHUNK:"))
                            {
                                // Обрабатываем недостающий чанк от сервера
                                _ = HandleIncomingFileChunk(chunkData.Substring(14));
                            }
                            else
                            {
                                // Обычный чанк файла
                                _ = HandleIncomingFileChunk(chunkData);
                            }
                            processedCount++;
                        }
                        catch (Exception ex)
                        {
                            _ = Task.Run(() => UpdateStatus($"❌ Error processing file chunk: {ex.Message}", Color.Red));
                        }
                    }
                    
                    if (processedCount == 0)
                    {
                        // Если очередь пуста, ждем короткое время
                        await Task.Delay(2, _fileProcessingCancellation.Token);
                    }
                    else
                    {
                        // После обработки файловых чанков ВСЕГДА даем время потоку сообщений
                        await Task.Delay(1, _fileProcessingCancellation.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены - это нормально
            }
            catch (Exception ex)
            {
                _ = Task.Run(() => UpdateStatus($"❌ Error in file chunk queue processor: {ex.Message}", Color.Red));
            }
        }

        // Приоритетная очередь для мгновенной обработки обычных сообщений чата


        // Единая очередь для всех сообщений с правильной сортировкой ПОСЛЕ расшифровки
        private async Task ProcessMessageQueue()
        {
            try
            {
                while (!_messageProcessingCancellation.Token.IsCancellationRequested)
                {
                    var processedCount = 0;
                    
                    // Обрабатываем до 20 сообщений за раз
                    while (_messageQueue.TryDequeue(out string receivedMessageBase64) && processedCount < 20)
                    {
                        try
                        {
                            var encryptedMessage = Convert.FromBase64String(receivedMessageBase64);
                            var decryptedMessage = DecryptWithAES(encryptedMessage, _aes.Key, _aes.IV);
                            
                            // ПРАВИЛЬНОЕ разделение ПОСЛЕ расшифровки
                            if (decryptedMessage.StartsWith("FILE_TRANSFER_START:"))
                            {
                                _ = Task.Run(() => HandleIncomingFileTransferStart(decryptedMessage.Substring(20)));
                            }
                            else if (decryptedMessage.StartsWith("FILE_CHUNK:"))
                            {
                                // Чанки в отдельную очередь
                                _fileChunkQueue.Enqueue(decryptedMessage.Substring(11));
                            }
                            else if (decryptedMessage.StartsWith("FILE_TRANSFER_COMPLETE:"))
                            {
                                _ = Task.Run(async () => await HandleIncomingFileTransferComplete(decryptedMessage.Substring(23)));
                            }
                            else if (decryptedMessage.StartsWith("MISSING_CHUNKS_REQUEST:"))
                            {
                                // ТИХО обрабатываем запросы недостающих чанков
                                var json = decryptedMessage.Substring(23);
                                _ = Task.Run(async () => await HandleMissingChunksRequestFromServerSilently(json));
                            }
                            else if (decryptedMessage.StartsWith("MISSING_CHUNK:"))
                            {
                                // Недостающие чанки в файловую очередь
                                _fileChunkQueue.Enqueue(decryptedMessage.Substring(14));
                            }
                            else if (decryptedMessage.StartsWith("FILE_RECEIVED:"))
                            {
                                // Уведомления о файлах В ЧАТ
                                var fileNotification = decryptedMessage.Substring(14);
                                _ = Task.Run(() => UpdateStatus($"📁 {fileNotification}", Color.Purple));
                            }
                            else if (decryptedMessage.StartsWith("FILE_RECEIPT_CONFIRMATION:"))
                            {
                                // Игнорируем служебные подтверждения
                            }
                            else
                            {
                                // ТОЛЬКО обычные сообщения чата В ЧАТ
                                _ = Task.Run(() => UpdateStatus(decryptedMessage, Color.DarkGreen));
                                
                                // Звук при подключении/отключении
                                if (decryptedMessage.Contains("has joined the chat.") ||
                                    decryptedMessage.Contains("has left the chat."))
                                {
                                    SystemSounds.Asterisk.Play();
                                }
                            }
                            
                            processedCount++;
                        }
                        catch (Exception)
                        {
                            // Игнорируем ошибки расшифровки - возможно поврежденные данные
                        }
                    }
                    
                    if (processedCount == 0)
                    {
                        await Task.Delay(1, _messageProcessingCancellation.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены - это нормально
            }
            catch (Exception)
            {
                // Игнорируем ошибки обработки очереди
            }
        }

        private Task HandleIncomingFileChunk(string json)
        {
            try
            {
                var fileChunk = JsonConvert.DeserializeObject<FileTransferModels.FileChunk>(json);
                if (fileChunk == null) return Task.CompletedTask;

                if (!_receivedFiles.ContainsKey(fileChunk.FileId))
                {
                    // Асинхронно обновляем UI без блокировки
                    _ = Task.Run(() => UpdateStatus($"❌ Received data for unknown file transfer", Color.Red));
                    return Task.CompletedTask;
                }

                var receivedFileInfo = _receivedFiles[fileChunk.FileId];

                // Быстрая проверка дублирования
                if (receivedFileInfo.ReceivedChunkNumbers.Contains(fileChunk.ChunkNumber))
                {
                    return Task.CompletedTask; // Чанк уже получен, пропускаем
                }

                // Быстрая проверка контрольной суммы
                var computedCheckSum = ComputeMD5Hash(fileChunk.Data);
                if (computedCheckSum != fileChunk.CheckSum)
                {
                    _ = Task.Run(() => {
                        UpdateStatus($"❌ File transfer error: integrity check failed for chunk {fileChunk.ChunkNumber}", Color.Red);
                        HideFileProgress();
                    });
                    return Task.CompletedTask;
                }

                // Быстрые операции с данными
                var startIndex = fileChunk.ChunkNumber * CHUNK_SIZE;
                Array.Copy(fileChunk.Data, 0, receivedFileInfo.FileData, startIndex, fileChunk.Data.Length);
                receivedFileInfo.ReceivedChunkNumbers.Add(fileChunk.ChunkNumber);
                receivedFileInfo.ReceivedChunks++;

                // Показываем прогресс только при первом чанке
                if (receivedFileInfo.ReceivedChunks == 1)
                {
                    _ = Task.Run(() => ShowFileProgress("Receiving: 0%"));
                }

                // Обновляем прогресс только каждые 500 чанков для уменьшения нагрузки на UI
                if (receivedFileInfo.ReceivedChunks % 500 == 0 || receivedFileInfo.ReceivedChunks == receivedFileInfo.TotalChunks)
                {
                    var progress = (double)receivedFileInfo.ReceivedChunks / receivedFileInfo.TotalChunks * 100;
                    _ = Task.Run(() => UpdateFileProgress((int)progress, $"Receiving: {progress:F1}%"));
                }

                // Проверяем завершение передачи
                if (receivedFileInfo.ReceivedChunks == receivedFileInfo.TotalChunks)
                {
                    receivedFileInfo.IsComplete = true;
                    // Сохранение файла делаем асинхронно
                    _ = Task.Run(async () => await SaveReceivedFile(receivedFileInfo));
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _ = Task.Run(() => {
                    UpdateStatus($"❌ Error processing file data: {ex.Message}", Color.Red);
                    HideFileProgress();
                });
                return Task.CompletedTask;
            }
        }

        private async Task HandleIncomingFileTransferComplete(string json)
        {
            try
            {
                var fileTransferComplete = JsonConvert.DeserializeObject<FileTransferModels.FileTransferComplete>(json);
                if (fileTransferComplete == null) 
                {
                    UpdateStatus($"❌ Failed to parse file transfer complete", Color.Red);
                    return;
                }

                if (_receivedFiles.ContainsKey(fileTransferComplete.FileId))
                {
                    var receivedFileInfo = _receivedFiles[fileTransferComplete.FileId];

                    if (!receivedFileInfo.IsComplete)
                    {
                        // Проверяем, какие чанки отсутствуют
                        var missingChunks = new List<int>();
                        for (int i = 0; i < receivedFileInfo.TotalChunks; i++)
                        {
                            if (!receivedFileInfo.ReceivedChunkNumbers.Contains(i))
                            {
                                missingChunks.Add(i);
                            }
                        }

                        if (missingChunks.Count > 0)
                        {
                            UpdateStatus($"⚠️ File transfer incomplete: {receivedFileInfo.FileName} (missing {missingChunks.Count} chunks)", Color.Orange);
                            UpdateStatus($"🔄 Requesting missing chunks: {string.Join(", ", missingChunks.Take(10))}{(missingChunks.Count > 10 ? "..." : "")}", Color.Blue);
                            
                            // Запрашиваем недостающие чанки
                            _ = RequestMissingChunks(fileTransferComplete.FileId, missingChunks);
                        }
                        else
                        {
                            // Все чанки получены, завершаем передачу
                            receivedFileInfo.IsComplete = true;
                            await SaveReceivedFile(receivedFileInfo);
                        }
                    }
                    else
                    {
                        UpdateStatus($"✅ File transfer marked as complete: {receivedFileInfo.FileName}", Color.Blue);
                    }
                }
                else
                {
                    UpdateStatus($"⚠️ Received completion for unknown file transfer", Color.Orange);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error completing file transfer: {ex.Message}", Color.Red);
                HideFileProgress();
            }
        }

        private async Task SaveReceivedFile(ReceivedFileInfo receivedFileInfo)
        {
            try
            {
                UpdateStatus($"💾 Saving file {receivedFileInfo.FileName} ({FormatFileSize(receivedFileInfo.FileSize)})...", Color.Blue);
                
                // Создаем папку Downloads, если она не существует
                var downloadsFolder = "Downloads";
                if (!Directory.Exists(downloadsFolder))
                {
                    Directory.CreateDirectory(downloadsFolder);
                }

                // Создаем уникальное имя файла
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{Path.GetFileNameWithoutExtension(receivedFileInfo.FileName)}_{timestamp}{Path.GetExtension(receivedFileInfo.FileName)}";
                var filePath = Path.Combine(downloadsFolder, fileName);

                // Сохраняем файл
                File.WriteAllBytes(filePath, receivedFileInfo.FileData);

                UpdateStatus($"✅ File saved successfully: {filePath}", Color.Green);
                HideFileProgress();

                // Отправляем подтверждение получения файла серверу
                UpdateStatus($"📤 Sending receipt confirmation to server...", Color.Blue);
                await SendFileReceiptConfirmation(receivedFileInfo.FileId, true);

                // Удаляем информацию о файле из активных
                _receivedFiles.Remove(receivedFileInfo.FileId);
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error saving received file: {ex.Message}", Color.Red);
                HideFileProgress();
                await SendFileReceiptConfirmation(receivedFileInfo.FileId, false);
            }
        }

        private async Task SendFileReceiptConfirmation(string fileId, bool success)
        {
            try
            {
                var confirmation = new FileTransferModels.FileReceiptConfirmation
                {
                    FileId = fileId,
                    Success = success
                };

                var confirmationJson = JsonConvert.SerializeObject(confirmation);
                var confirmationMessage = $"FILE_RECEIPT_CONFIRMATION:{confirmationJson}";

                var encryptedConfirmation = EncryptWithAES(confirmationMessage, _aes.Key, _aes.IV);
                var encryptedConfirmationBase64 = Convert.ToBase64String(encryptedConfirmation);
                var encryptedConfirmationBytes = Encoding.UTF8.GetBytes(encryptedConfirmationBase64);
                await _udpClient.SendAsync(encryptedConfirmationBytes, encryptedConfirmationBytes.Length, _serverEndPoint);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error sending file receipt confirmation: {ex.Message}", Color.Red);
            }
        }

        private async Task RequestMissingChunks(string fileId, List<int> missingChunks)
        {
            try
            {
                if (!_receivedFiles.ContainsKey(fileId)) return;
                
                var receivedFileInfo = _receivedFiles[fileId];
                
                // Проверяем, не слишком ли часто мы запрашиваем недостающие чанки
                var timeSinceLastRequest = DateTime.Now - receivedFileInfo.LastMissingChunksRequestTime;
                if (timeSinceLastRequest.TotalSeconds < 10 && receivedFileInfo.MissingChunksRequestCount > 0)
                {
                    UpdateStatus($"⏳ Waiting before retrying missing chunks request...", Color.Blue);
                    return;
                }

                // Ограничиваем количество попыток
                if (receivedFileInfo.MissingChunksRequestCount >= 3)
                {
                    UpdateStatus($"❌ Too many retry attempts for {receivedFileInfo.FileName}. Transfer failed.", Color.Red);
                    HideFileProgress();
                    await SendFileReceiptConfirmation(fileId, false);
                    _receivedFiles.Remove(fileId);
                    return;
                }

                receivedFileInfo.LastMissingChunksRequestTime = DateTime.Now;
                receivedFileInfo.MissingChunksRequestCount++;

                var request = new FileTransferModels.MissingChunksRequest
                {
                    FileId = fileId,
                    MissingChunkNumbers = missingChunks
                };

                var requestJson = JsonConvert.SerializeObject(request);
                var requestMessage = $"MISSING_CHUNKS_REQUEST:{requestJson}";

                var encryptedRequest = EncryptWithAES(requestMessage, _aes.Key, _aes.IV);
                var encryptedRequestBase64 = Convert.ToBase64String(encryptedRequest);
                var encryptedRequestBytes = Encoding.UTF8.GetBytes(encryptedRequestBase64);
                await _udpClient.SendAsync(encryptedRequestBytes, encryptedRequestBytes.Length, _serverEndPoint);

                // Запускаем таймер для повторной проверки через 15 секунд
                _ = Task.Run(async () =>
                {
                    await Task.Delay(15000); // 15 секунд
                    if (_receivedFiles.ContainsKey(fileId) && !_receivedFiles[fileId].IsComplete)
                    {
                        var info = _receivedFiles[fileId];
                        
                        // Проверяем, какие чанки все еще отсутствуют
                        var stillMissingChunks = new List<int>();
                        for (int i = 0; i < info.TotalChunks; i++)
                        {
                            if (!info.ReceivedChunkNumbers.Contains(i))
                            {
                                stillMissingChunks.Add(i);
                            }
                        }

                        if (stillMissingChunks.Count > 0)
                        {
                            UpdateStatus($"⚠️ Still missing {stillMissingChunks.Count} chunks, retrying...", Color.Orange);
                            await RequestMissingChunks(fileId, stillMissingChunks);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error requesting missing chunks: {ex.Message}", Color.Red);
            }
        }

        private async Task HandleMissingChunksRequestFromServer(string json)
        {
            try
            {
                UpdateStatus($"🔍 Processing missing chunks request from server...", Color.Cyan);
                UpdateStatus($"🔍 Raw JSON: {json.Substring(0, Math.Min(100, json.Length))}...", Color.Gray);
                
                var missingChunksRequest = JsonConvert.DeserializeObject<dynamic>(json);
                if (missingChunksRequest == null) 
                {
                    UpdateStatus($"❌ Failed to parse missing chunks request", Color.Red);
                    return;
                }

                string fileId = missingChunksRequest.FileId;
                var missingChunkNumbers = ((JArray)missingChunksRequest.MissingChunks).ToObject<List<int>>();

                UpdateStatus($"🔍 Server requesting chunks for file ID: {fileId}", Color.Cyan);
                UpdateStatus($"🔍 Missing chunks: [{string.Join(", ", missingChunkNumbers)}]", Color.Cyan);
                UpdateStatus($"🔍 Available file transfers: {_activeFileTransfers.Count}", Color.Cyan);

                if (!_activeFileTransfers.ContainsKey(fileId))
                {
                    UpdateStatus($"❌ Server requested missing chunks for unknown file transfer: {fileId}", Color.Red);
                    UpdateStatus($"❌ Available file IDs: {string.Join(", ", _activeFileTransfers.Keys.Take(3))}", Color.Red);
                    
                    // Покажем все доступные file ID для отладки
                    foreach (var id in _activeFileTransfers.Keys)
                    {
                        UpdateStatus($"❌ Available ID: {id}", Color.Red);
                    }
                    return;
                }

                var fileTransferInfo = _activeFileTransfers[fileId];
                UpdateStatus($"🔄 Server requested {missingChunkNumbers.Count} missing chunks for {fileTransferInfo.FileName}", Color.Blue);
                UpdateStatus($"📤 Resending chunks: {string.Join(", ", missingChunkNumbers.Take(10))}{(missingChunkNumbers.Count > 10 ? "..." : "")}", Color.Cyan);
                UpdateStatus($"📋 File size: {fileTransferInfo.FileSize} bytes, Total chunks: {fileTransferInfo.TotalChunks}", Color.Gray);

                // Отправляем недостающие чанки
                var successCount = 0;
                foreach (var chunkNumber in missingChunkNumbers)
                {
                    try
                    {
                        // Читаем данные чанка из файла
                        var chunkSize = Math.Min(CHUNK_SIZE, (int)(fileTransferInfo.FileSize - (chunkNumber * CHUNK_SIZE)));
                        var chunkData = new byte[chunkSize];
                        Array.Copy(fileTransferInfo.FileData, chunkNumber * CHUNK_SIZE, chunkData, 0, chunkSize);

                        // Создаем чанк
                        var fileChunk = new FileTransferModels.FileChunk
                        {
                            FileId = fileId,
                            ChunkNumber = chunkNumber,
                            TotalChunks = fileTransferInfo.TotalChunks,
                            Data = chunkData,
                            CheckSum = ComputeMD5Hash(chunkData)
                        };

                        // Шифруем чанк
                        var encryptedChunkData = EncryptWithAES(JsonConvert.SerializeObject(fileChunk), _aes.Key, _aes.IV);
                        var chunkJson = Convert.ToBase64String(encryptedChunkData);

                        // Отправляем как недостающий чанк
                        var missingChunkMessage = $"MISSING_CHUNK:{chunkJson}";
                        var messageBytes = Encoding.UTF8.GetBytes(missingChunkMessage);
                        await _udpClient.SendAsync(messageBytes, messageBytes.Length, _serverEndPoint);
                        
                        UpdateStatus($"✅ Sent missing chunk {chunkNumber} to server ({chunkData.Length} bytes)", Color.Cyan);
                        successCount++;

                        // Небольшая задержка между чанками
                        await Task.Delay(2);
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"❌ Error sending missing chunk {chunkNumber}: {ex.Message}", Color.Red);
                    }
                }

                UpdateStatus($"✅ Successfully sent {successCount}/{missingChunkNumbers.Count} missing chunks to server", Color.Green);
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error handling missing chunks request: {ex.Message}", Color.Red);
            }
        }

        // ТИХАЯ версия - без логирования в чат
        private async Task HandleMissingChunksRequestFromServerSilently(string json)
        {
            try
            {
                var missingChunksRequest = JsonConvert.DeserializeObject<dynamic>(json);
                if (missingChunksRequest == null) return;

                string fileId = missingChunksRequest.FileId;
                var missingChunkNumbers = ((JArray)missingChunksRequest.MissingChunks).ToObject<List<int>>();

                if (!_activeFileTransfers.ContainsKey(fileId)) return;

                var fileTransferInfo = _activeFileTransfers[fileId];

                // Отправляем недостающие чанки БЕЗ логирования
                foreach (var chunkNumber in missingChunkNumbers)
                {
                    try
                    {
                        var chunkSize = Math.Min(CHUNK_SIZE, (int)(fileTransferInfo.FileSize - (chunkNumber * CHUNK_SIZE)));
                        var chunkData = new byte[chunkSize];
                        Array.Copy(fileTransferInfo.FileData, chunkNumber * CHUNK_SIZE, chunkData, 0, chunkSize);

                        var fileChunk = new FileTransferModels.FileChunk
                        {
                            FileId = fileId,
                            ChunkNumber = chunkNumber,
                            TotalChunks = fileTransferInfo.TotalChunks,
                            Data = chunkData,
                            CheckSum = ComputeMD5Hash(chunkData)
                        };

                        var encryptedChunkData = EncryptWithAES(JsonConvert.SerializeObject(fileChunk), _aes.Key, _aes.IV);
                        var chunkJson = Convert.ToBase64String(encryptedChunkData);
                        var missingChunkMessage = $"MISSING_CHUNK:{chunkJson}";
                        var messageBytes = Encoding.UTF8.GetBytes(missingChunkMessage);
                        await _udpClient.SendAsync(messageBytes, messageBytes.Length, _serverEndPoint);
                        
                        await Task.Delay(2);
                    }
                    catch (Exception)
                    {
                        // Игнорируем ошибки тихо
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки тихо
            }
        }

        #endregion

        #region File Progress UI

        private void ShowFileProgress(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(ShowFileProgress), text);
                return;
            }

            fileProgressBar.Value = 0;
            fileProgressBar.Visible = true;
            fileProgressLabel.Text = text;
            fileProgressLabel.Visible = true;
        }

        private void UpdateFileProgress(int percentage, string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, string>(UpdateFileProgress), percentage, text);
                return;
            }

            if (percentage < 0) percentage = 0;
            if (percentage > 100) percentage = 100;

            fileProgressBar.Value = percentage;
            fileProgressLabel.Text = text;
        }

        private void HideFileProgress()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(HideFileProgress));
                return;
            }

            fileProgressBar.Visible = false;
            fileProgressLabel.Visible = false;
        }

        #endregion

        #endregion
    }
}