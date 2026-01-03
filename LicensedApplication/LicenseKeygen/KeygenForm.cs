using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;

namespace LicenseKeygen
{
    public partial class KeygenForm : Form
    {
        // The same encryption keys used in the application
        private static readonly byte[] AesKey = new byte[32]
        {
            0x42, 0x1A, 0x86, 0xE5, 0x3D, 0xC1, 0x5B, 0x9F,
            0x78, 0x2E, 0x4D, 0x8C, 0x36, 0xA7, 0xF9, 0x02,
            0x63, 0xB4, 0x19, 0xD7, 0x51, 0x8F, 0x0A, 0xE3,
            0x74, 0xC5, 0x2B, 0x96, 0x4E, 0x3F, 0xD8, 0x1C
        };

        // The same IV used in the application
        private static readonly byte[] AesIV = new byte[16]
        {
            0x3A, 0xF1, 0x84, 0x2D, 0xB9, 0x6C, 0x05, 0xE7,
            0x47, 0x98, 0x1F, 0xA2, 0xD5, 0x83, 0x6B, 0xC0
        };

        // Constant for product name
        private new const string ProductName = "YourProductName";

        public KeygenForm()
        {
            InitializeComponent();
            numYears.Value = 10;
            chkAllowHardwareChange.Checked = true;

            // Generate a key on startup
            txtLicenseKey.Text = GenerateLicenseKey(20);
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Generate a new key when button is clicked
            txtLicenseKey.Text = GenerateLicenseKey(20);
        }

        private void btnCreateLicense_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input fields
                if (string.IsNullOrEmpty(txtLicenseKey.Text) ||
                    string.IsNullOrEmpty(txtUserName.Text) ||
                    string.IsNullOrEmpty(txtEmail.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните обязательные поля: ключ лицензии, имя пользователя и email",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string hardwareId = txtHardwareId.Text.Trim();
                if (string.IsNullOrEmpty(hardwareId))
                {
                    hardwareId = "UNIVERSAL-LICENSE-NOT-HARDWARE-BOUND";
                    logBox.AppendText("Создание универсальной лицензии, не привязанной к оборудованию...\r\n");
                }

                // Create the license object
                var license = new LicenseInfo
                {
                    LicenseKey = txtLicenseKey.Text.Trim(),
                    UserName = txtUserName.Text.Trim(),
                    UserEmail = txtEmail.Text.Trim(),
                    CompanyName = txtCompany.Text.Trim(),
                    Type = LicenseType.Full,
                    IssueDate = DateTime.Now,
                    ExpirationDate = DateTime.Now.AddYears((int)numYears.Value),
                    HardwareId = hardwareId,
                    AllowHardwareChange = chkAllowHardwareChange.Checked,
                    MaximumInstances = 1,
                    AllowUpdates = true,
                    Features = new string[] { "all" },
                    ProductVersion = "1.0.0",
                    Notes = txtNotes.Text.Trim(),
                    ValidationToken = GenerateValidationToken(txtLicenseKey.Text.Trim(), hardwareId)
                };

                // Choose where to save the license file
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "License files (*.dat)|*.dat",
                    FileName = "license.dat",
                    Title = "Сохранить файл лицензии"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (SaveLicenseToFile(license, saveDialog.FileName))
                    {
                        logBox.AppendText($"\r\nЛицензия успешно сохранена в файл {saveDialog.FileName}\r\n");
                        logBox.AppendText("\r\nИнформация о лицензии:\r\n");
                        logBox.AppendText($"Ключ лицензии: {license.LicenseKey}\r\n");
                        logBox.AppendText($"Пользователь: {license.UserName}\r\n");
                        logBox.AppendText($"Email: {license.UserEmail}\r\n");
                        logBox.AppendText($"Аппаратный ID: {license.HardwareId}\r\n");
                        logBox.AppendText($"Срок действия до: {license.ExpirationDate.ToShortDateString()}\r\n");
                        logBox.AppendText($"Разрешено изменение оборудования: {(license.AllowHardwareChange ? "Да" : "Нет")}\r\n");

                        logBox.AppendText("\r\nИнструкции:\r\n");
                        logBox.AppendText("1. Скопируйте файл license.dat в директорию приложения\r\n");
                        logBox.AppendText("2. При запросе в приложении используйте следующие данные для активации:\r\n");
                        logBox.AppendText($"   - Ключ лицензии: {license.LicenseKey}\r\n");
                        logBox.AppendText($"   - Имя пользователя: {license.UserName}\r\n");
                        logBox.AppendText($"   - Email: {license.UserEmail}\r\n");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить файл лицензии.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании лицензии: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(logBox.Text))
            {
                Clipboard.SetText(logBox.Text);
                MessageBox.Show("Информация о лицензии скопирована в буфер обмена",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear form fields
            txtUserName.Clear();
            txtEmail.Clear();
            txtCompany.Clear();
            txtHardwareId.Clear();
            txtNotes.Clear();
            numYears.Value = 10;
            chkAllowHardwareChange.Checked = true;
            logBox.Clear();
            txtLicenseKey.Text = GenerateLicenseKey(20);
        }

        // License types enum (must match the application)
        public enum LicenseType
        {
            Trial = 0,
            Full = 1
        }

        // License information class (must match the application)
        public class LicenseInfo
        {
            public string LicenseKey { get; set; }
            public string UserName { get; set; }
            public string UserEmail { get; set; }
            public string CompanyName { get; set; }
            public LicenseType Type { get; set; }
            public DateTime IssueDate { get; set; }
            public DateTime ExpirationDate { get; set; }
            public string HardwareId { get; set; }
            public bool AllowHardwareChange { get; set; }
            public int MaximumInstances { get; set; }
            public bool AllowUpdates { get; set; }
            public string[] Features { get; set; }
            public string ProductVersion { get; set; }
            public string Notes { get; set; }
            public string ValidationToken { get; set; }
        }

        // Generates a random license key
        private static string GenerateLicenseKey(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(validChars[random.Next(validChars.Length)]);

                // Add dashes for readability
                if ((i + 1) % 5 == 0 && i < length - 1)
                    result.Append('-');
            }

            return result.ToString();
        }

        // Generates the validation token exactly like the application does
        private static string GenerateValidationToken(string licenseKey, string hardwareId)
        {
            using (HMACSHA256 hmac = new HMACSHA256(AesKey))
            {
                string data = licenseKey + "|" + hardwareId + "|" + ProductName;
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        // Saves license to file using the same encryption as the application
        private static bool SaveLicenseToFile(LicenseInfo license, string path)
        {
            try
            {
                // Create XML document with license info
                XmlDocument doc = new XmlDocument();
                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(xmlDeclaration);

                XmlElement root = doc.CreateElement("License");
                doc.AppendChild(root);

                AddXmlElement(doc, root, "Key", license.LicenseKey);
                AddXmlElement(doc, root, "UserName", license.UserName);
                AddXmlElement(doc, root, "UserEmail", license.UserEmail);
                AddXmlElement(doc, root, "CompanyName", license.CompanyName);
                AddXmlElement(doc, root, "Type", license.Type.ToString());
                AddXmlElement(doc, root, "IssueDate", license.IssueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                AddXmlElement(doc, root, "ExpirationDate", license.ExpirationDate.ToString("yyyy-MM-dd HH:mm:ss"));
                AddXmlElement(doc, root, "HardwareId", license.HardwareId);
                AddXmlElement(doc, root, "AllowHardwareChange", license.AllowHardwareChange.ToString());
                AddXmlElement(doc, root, "MaximumInstances", license.MaximumInstances.ToString());
                AddXmlElement(doc, root, "AllowUpdates", license.AllowUpdates.ToString());

                XmlElement featuresElement = doc.CreateElement("Features");
                root.AppendChild(featuresElement);

                if (license.Features != null)
                {
                    foreach (string feature in license.Features)
                    {
                        AddXmlElement(doc, featuresElement, "Feature", feature);
                    }
                }

                AddXmlElement(doc, root, "ProductVersion", license.ProductVersion);
                AddXmlElement(doc, root, "Notes", license.Notes);
                AddXmlElement(doc, root, "ValidationToken", license.ValidationToken);

                // Convert to XML string
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlTextWriter xw = new XmlTextWriter(sw))
                    {
                        doc.WriteTo(xw);
                        string xmlString = sw.ToString();

                        // Encrypt and save
                        byte[] encryptedData = EncryptData(xmlString);
                        File.WriteAllBytes(path, encryptedData);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения лицензии: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Helper to add XML elements
        private static void AddXmlElement(XmlDocument doc, XmlElement parent, string name, string value)
        {
            XmlElement element = doc.CreateElement(name);
            element.InnerText = value ?? string.Empty;
            parent.AppendChild(element);
        }

        // Encrypts data using AES - same method as in the application
        private static byte[] EncryptData(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = AesKey;
                aes.IV = AesIV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }

                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}