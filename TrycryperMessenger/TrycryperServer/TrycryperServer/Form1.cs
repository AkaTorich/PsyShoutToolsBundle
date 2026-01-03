using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrycryperServer
{
    public partial class Form1 : Form
    {
        private UdpClient _udpClient;
        private RSACryptoServiceProvider _rsa;
        private List<IPEndPoint> _clientEndPoints;
        private Dictionary<IPEndPoint, (byte[] AesKey, byte[] AesIV)> _clientAesKeys;
        private Dictionary<IPEndPoint, string> _clientNicknames; // Хранение ников клиентов
        private Dictionary<string, FileTransferInfo> _activeFileTransfers; // Активные передачи файлов
        private const string DOWNLOADS_FOLDER = "Downloads";

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            // Блокируем изменение размера окна
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            _rsa = new RSACryptoServiceProvider(2048);
            _clientEndPoints = new List<IPEndPoint>();
            _clientAesKeys = new Dictionary<IPEndPoint, (byte[] AesKey, byte[] AesIV)>();
            _clientNicknames = new Dictionary<IPEndPoint, string>(); // Инициализация словаря
            _activeFileTransfers = new Dictionary<string, FileTransferInfo>(); // Инициализация словаря передач файлов

            // Создаем папку для загрузок, если она не существует
            if (!Directory.Exists(DOWNLOADS_FOLDER))
            {
                Directory.CreateDirectory(DOWNLOADS_FOLDER);
            }
        }

        private void startServer_Click(object sender, EventArgs e)
        {
            try
            {
                _udpClient = new UdpClient(int.Parse(serverPort.Text));
                //UpdateStatus("Server is listening...");
                //UpdateStatus($"Server Public Key: {_rsa.ToXmlString(false)}");

                Task.Run(() => StartListening());
            }
            catch (Exception)
            {
                //UpdateStatus("Error in startServer_Click: " + ex.Message);
            }
        }

        private async Task StartListening()
        {
            try
            {
                while (true)
                {
                    var result = await _udpClient.ReceiveAsync();
                    var receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                    var clientEndPoint = result.RemoteEndPoint;

                    if (receivedMessage.StartsWith("NICKNAME:"))
                    {
                        var nickname = receivedMessage.Substring(9);
                        //UpdateStatus("Received nickname: " + nickname);

                        if (!_clientEndPoints.Contains(clientEndPoint))
                        {
                            _clientEndPoints.Add(clientEndPoint);
                            _clientNicknames[clientEndPoint] = nickname; // Сохраняем ник клиента
                        }

                        // Отправка публичного ключа
                        var publicKeyXml = _rsa.ToXmlString(false);
                        var publicKeyBytes = Encoding.UTF8.GetBytes(publicKeyXml);
                        await _udpClient.SendAsync(publicKeyBytes, publicKeyBytes.Length, clientEndPoint);
                        //UpdateStatus("Public key sent to client.");

                        // Уведомление всех клиентов о новом подключении
                        var broadcastMessage = $"{nickname} has joined the chat.";
                        await BroadcastMessage(broadcastMessage, clientEndPoint);
                    }
                    else if (receivedMessage.StartsWith("DISCONNECT:"))
                    {
                        // Обработка явного отключения клиента
                        var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Unknown user";
                        await HandleClientDisconnect(clientEndPoint, nickname);
                    }
                    else if (receivedMessage.StartsWith("AES_KEY:"))
                    {
                        var encryptedAesKey = Convert.FromBase64String(receivedMessage.Substring(8));
                        var aesKey = _rsa.Decrypt(encryptedAesKey, false);

                        if (_clientAesKeys.ContainsKey(clientEndPoint))
                        {
                            _clientAesKeys[clientEndPoint] = (aesKey, _clientAesKeys[clientEndPoint].AesIV);
                        }
                        else
                        {
                            _clientAesKeys.Add(clientEndPoint, (aesKey, null));
                        }

                        //UpdateStatus($"AES key received from client: {Convert.ToBase64String(aesKey)}");
                    }
                    else if (receivedMessage.StartsWith("AES_IV:"))
                    {
                        var aesIV = Convert.FromBase64String(receivedMessage.Substring(7));

                        if (_clientAesKeys.ContainsKey(clientEndPoint))
                        {
                            _clientAesKeys[clientEndPoint] = (_clientAesKeys[clientEndPoint].AesKey, aesIV);
                        }
                        else
                        {
                            _clientAesKeys.Add(clientEndPoint, (null, aesIV));
                        }

                        //UpdateStatus($"AES IV received from client: {Convert.ToBase64String(aesIV)}");
                    }
                    else if (receivedMessage.StartsWith("PING"))
                    {
                        // Обработка проверки соединения
                        var responseBytes = Encoding.UTF8.GetBytes("PONG");
                        await _udpClient.SendAsync(responseBytes, responseBytes.Length, clientEndPoint);
                    }
                    else
                    {
                        try
                        {
                            var encryptedMessage = Convert.FromBase64String(receivedMessage);
                            //UpdateStatus($"Received encrypted message: {receivedMessage}");

                            var aesKeyIVPair = _clientAesKeys[clientEndPoint];
                            var decryptedMessage = DecryptWithAES(encryptedMessage, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                            //UpdateStatus($"Received decrypted message: {decryptedMessage}");

                            // Проверяем, является ли сообщение связанным с передачей файлов
                            if (decryptedMessage.StartsWith("FILE_TRANSFER_START:"))
                            {
                                await HandleFileTransferStart(decryptedMessage.Substring(20), clientEndPoint);
                            }
                            else if (decryptedMessage.StartsWith("FILE_CHUNK:"))
                            {
                                await HandleFileChunk(decryptedMessage.Substring(11), clientEndPoint);
                            }
                            else if (decryptedMessage.StartsWith("FILE_TRANSFER_COMPLETE:"))
                            {
                                await HandleFileTransferComplete(decryptedMessage.Substring(23), clientEndPoint);
                            }
                            else if (decryptedMessage.StartsWith("FILE_RECEIPT_CONFIRMATION:"))
                            {
                                await HandleFileReceiptConfirmationSilently(decryptedMessage.Substring(26), clientEndPoint);
                            }
                            else if (decryptedMessage.StartsWith("MISSING_CHUNKS_REQUEST:"))
                            {
                                await HandleMissingChunksRequestSilently(decryptedMessage.Substring(23), clientEndPoint);
                            }
                            else if (decryptedMessage.StartsWith("MISSING_CHUNK:"))
                            {
                                await HandleFileChunk(decryptedMessage.Substring(14), clientEndPoint);
                            }
                            else
                            {
                                // ТОЛЬКО обычные сообщения чата В ЧАТ
                                UpdateStatus(decryptedMessage, Color.DarkGreen);
                                await BroadcastMessage(decryptedMessage, clientEndPoint);
                            }
                        }
                        catch (Exception)
                        {
                            // Если возникает ошибка при обработке сообщения, это может означать, что клиент отключился
                            if (_clientEndPoints.Contains(clientEndPoint))
                            {
                                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Unknown user";
                                await HandleClientDisconnect(clientEndPoint, nickname);
                            }
                            //UpdateStatus($"Error processing message: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception)
            {
                //UpdateStatus("Error in StartListening: " + ex.Message);
            }
        }

        private async Task HandleClientDisconnect(IPEndPoint clientEndPoint, string nickname)
        {
            try
            {
                if (_clientEndPoints.Contains(clientEndPoint))
                {
                    // Удаляем клиента из списков
                    _clientEndPoints.Remove(clientEndPoint);
                    _clientAesKeys.Remove(clientEndPoint);
                    _clientNicknames.Remove(clientEndPoint);

                    // Отправляем сообщение об отключении всем оставшимся клиентам
                    var disconnectMessage = $"{nickname} has left the chat.";
                    await BroadcastMessage(disconnectMessage, null);
                    //UpdateStatus(disconnectMessage);
                }
            }
            catch (Exception)
            {
                //UpdateStatus($"Error handling client disconnect: {ex.Message}");
            }
        }

        private async Task BroadcastMessage(string message, IPEndPoint senderEndPoint)
        {
            try
            {
                // Создаем копию списка клиентов, чтобы избежать проблем с модификацией коллекции во время итерации
                var clientEndPointsCopy = new List<IPEndPoint>(_clientEndPoints);

                foreach (var clientEndPoint in clientEndPointsCopy)
                {
                    try
                    {
                        // Пропускаем отправителя, если он указан (кроме системных сообщений)
                        if (senderEndPoint != null && clientEndPoint.Equals(senderEndPoint))
                            continue;

                        // Проверяем, есть ли у клиента ключи шифрования
                        if (!_clientAesKeys.ContainsKey(clientEndPoint) ||
                            _clientAesKeys[clientEndPoint].AesKey == null ||
                            _clientAesKeys[clientEndPoint].AesIV == null)
                            continue;

                        var aesKeyIVPair = _clientAesKeys[clientEndPoint];
                        var encryptedMessage = EncryptWithAES(message, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                        var encryptedMessageBase64 = Convert.ToBase64String(encryptedMessage);
                        //UpdateStatus($"Broadcasting encrypted message to {clientEndPoint}: {encryptedMessageBase64}");

                        var encryptedMessageBytes = Encoding.UTF8.GetBytes(encryptedMessageBase64);
                        await _udpClient.SendAsync(encryptedMessageBytes, encryptedMessageBytes.Length, clientEndPoint);
                    }
                    catch (Exception)
                    {
                        // Если не удалось отправить сообщение клиенту, возможно, он отключился
                        //UpdateStatus($"Error sending message to client {clientEndPoint}: {ex.Message}");
                        var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Unknown user";
                        await HandleClientDisconnect(clientEndPoint, nickname);
                    }
                }
            }
            catch (Exception)
            {
                //UpdateStatus("Error in BroadcastMessage: " + ex.Message);
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Отправляем всем клиентам сообщение о закрытии сервера
            Task.Run(async () =>
            {
                await BroadcastMessage("Server is shutting down.", null);
            }).Wait();

            _udpClient?.Close();
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
            serverStatus.SelectionStart = serverStatus.TextLength;
            serverStatus.SelectionLength = 0;
            serverStatus.SelectionColor = color;

            // Добавляем сообщение с временной меткой
            serverStatus.AppendText(messageWithTime + Environment.NewLine);

            // Прокрутка к последней строке
            serverStatus.SelectionStart = serverStatus.TextLength;
            serverStatus.ScrollToCaret();
        }

        private void serverIp_TextChanged(object sender, EventArgs e) { }

        private void serverPort_TextChanged(object sender, EventArgs e) { }

        private void serverPortLabel_Click(object sender, EventArgs e) { }

        private void serverStatus_TextChanged(object sender, EventArgs e) { }

        #region File Transfer Methods

        private Task HandleFileTransferStart(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var fileTransferStart = JsonConvert.DeserializeObject<FileTransferStart>(json);
                if (fileTransferStart == null) return Task.CompletedTask;
                
                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Unknown user";

                // Создаем информацию о передаче файла
                // targetClients будет определен позже при распространении файла
                var fileTransferInfo = new FileTransferInfo
                {
                    FileId = fileTransferStart.FileId,
                    FileName = fileTransferStart.FileName,
                    FileSize = fileTransferStart.FileSize,
                    TotalChunks = fileTransferStart.TotalChunks,
                    SenderNickname = nickname,
                    SenderEndPoint = clientEndPoint,
                    FileData = new byte[fileTransferStart.FileSize],
                    StartTime = DateTime.Now,
                    ReceivedChunks = 0,
                    IsComplete = false,
                    TargetClients = new List<IPEndPoint>(), // Будет заполнен при распространении
                    ClientsConfirmed = new List<IPEndPoint>(),
                    IsDistributed = false
                };

                _activeFileTransfers[fileTransferStart.FileId] = fileTransferInfo;
                //UpdateStatus($"File transfer started: {fileTransferStart.FileName} from {nickname}");
            }
            catch (Exception)
            {
                //UpdateStatus($"Error handling file transfer start: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private Task HandleFileChunk(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var fileChunk = JsonConvert.DeserializeObject<FileChunk>(json);
                if (fileChunk == null) return Task.CompletedTask;
                
                if (!_activeFileTransfers.ContainsKey(fileChunk.FileId))
                {
                    //UpdateStatus($"Received chunk for unknown file: {fileChunk.FileId}");
                    return Task.CompletedTask;
                }

                var fileTransferInfo = _activeFileTransfers[fileChunk.FileId];
                
                // Проверяем контрольную сумму
                var computedCheckSum = ComputeMD5Hash(fileChunk.Data);
                if (computedCheckSum != fileChunk.CheckSum)
                {
                    //UpdateStatus($"Checksum mismatch for chunk {fileChunk.ChunkNumber} of file {fileTransferInfo.FileName}");
                    return Task.CompletedTask;
                }

                // Проверяем, не получали ли мы уже этот чанк
                if (fileTransferInfo.ReceivedChunkNumbers.Contains(fileChunk.ChunkNumber))
                {
                    // Чанк уже получен, пропускаем
                    return Task.CompletedTask;
                }

                // Копируем данные чанка в файл
                var startIndex = fileChunk.ChunkNumber * 32768; // CHUNK_SIZE
                Array.Copy(fileChunk.Data, 0, fileTransferInfo.FileData, startIndex, fileChunk.Data.Length);
                
                // Отмечаем чанк как полученный
                fileTransferInfo.ReceivedChunkNumbers.Add(fileChunk.ChunkNumber);
                fileTransferInfo.ReceivedChunks++;
                
                // Отправляем подтверждение получения чанка
                var ack = new ChunkAcknowledgment
                {
                    FileId = fileChunk.FileId,
                    ChunkNumber = fileChunk.ChunkNumber,
                    Success = true
                };

                // Проверяем, завершена ли передача
                if (fileTransferInfo.ReceivedChunks == fileTransferInfo.TotalChunks)
                {
                    // Файл полный, НО НЕ распространяем сразу - ждём сигнала FILE_TRANSFER_COMPLETE от клиента
                }
            }
            catch (Exception)
            {
                //UpdateStatus($"Error handling file chunk: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private async Task HandleFileTransferComplete(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var fileTransferComplete = JsonConvert.DeserializeObject<FileTransferComplete>(json);
                if (fileTransferComplete == null) return;
                
                if (_activeFileTransfers.ContainsKey(fileTransferComplete.FileId))
                {
                    var fileTransferInfo = _activeFileTransfers[fileTransferComplete.FileId];
                    
                    // Проверяем, получены ли все чанки
                    if (fileTransferInfo.ReceivedChunks == fileTransferInfo.TotalChunks)
                    {
                        // Проверяем, не был ли файл уже обработан
                        if (!fileTransferInfo.IsComplete)
                        {
                            UpdateStatus($"📤 File received: {fileTransferInfo.FileName} from {fileTransferInfo.SenderNickname}", Color.Green);
                            fileTransferInfo.IsComplete = true;
                            await SaveFileAndDistributeToClients(fileTransferInfo);
                        }
                    }
                    else
                    {
                        var completionRate = (double)fileTransferInfo.ReceivedChunks / fileTransferInfo.TotalChunks * 100;
                        
                        // Если получено 98%+ файла, попытаемся восстановить недостающие чанки
                        if (completionRate >= 98.0)
                        {
                            await RequestMissingChunksFromClient(fileTransferInfo, clientEndPoint);
                        }
                        else
                        {
                            _activeFileTransfers.Remove(fileTransferComplete.FileId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error handling file transfer complete: {ex.Message}", Color.Red);
            }
        }

        private async Task SaveFileAndDistributeToClients(FileTransferInfo fileTransferInfo)
        {
            try
            {
                // Создаем папку Downloads, если она не существует
                if (!Directory.Exists(DOWNLOADS_FOLDER))
                {
                    Directory.CreateDirectory(DOWNLOADS_FOLDER);
                }

                // Создаем уникальное имя файла
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{Path.GetFileNameWithoutExtension(fileTransferInfo.FileName)}_{timestamp}{Path.GetExtension(fileTransferInfo.FileName)}";
                var filePath = Path.Combine(DOWNLOADS_FOLDER, fileName);

                // Сохраняем файл
                File.WriteAllBytes(filePath, fileTransferInfo.FileData);
                
                // Сохраняем путь к файлу в объекте передачи
                fileTransferInfo.FilePath = filePath;

                // Проверяем, есть ли клиенты кроме отправителя
                var currentClients = _clientEndPoints.Where(ep => !ep.Equals(fileTransferInfo.SenderEndPoint)).ToList();
                if (currentClients.Count > 0)
                {
                    await DistributeFileToClients(fileTransferInfo, filePath);
                }
                else
                {
                    // Если нет целевых клиентов, удаляем файл сразу
                    File.Delete(filePath);
                    _activeFileTransfers.Remove(fileTransferInfo.FileId);
                    //UpdateStatus($"File processed and removed (no target clients): {filePath}");
                }

                //UpdateStatus($"File saved: {filePath}");
            }
            catch (Exception)
            {
                //UpdateStatus($"Error saving file: {ex.Message}");
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

        private async Task DistributeFileToClients(FileTransferInfo fileTransferInfo, string filePath)
        {
            try
            {
                // Определяем целевых клиентов в момент распространения (все кроме отправителя)
                var targetClients = _clientEndPoints.Where(ep => !ep.Equals(fileTransferInfo.SenderEndPoint)).ToList();
                fileTransferInfo.TargetClients = targetClients;
                fileTransferInfo.IsDistributed = true;
                
                UpdateStatus($"Distributing file '{fileTransferInfo.FileName}' to {targetClients.Count} clients SIMULTANEOUSLY", Color.Blue);
                
                // Отправляем файл ПАРАЛЛЕЛЬНО всем целевым клиентам одновременно
                var sendTasks = new List<Task>();
                foreach (var targetClient in targetClients)
                {
                    var nickname = _clientNicknames.ContainsKey(targetClient) ? _clientNicknames[targetClient] : "Unknown";
                    UpdateStatus($"Starting parallel file transfer to: {nickname} ({targetClient})", Color.Blue);
                    sendTasks.Add(SendFileToClient(targetClient, fileTransferInfo, filePath));
                }
                
                // Ждем завершения отправки всем клиентам
                await Task.WhenAll(sendTasks);
                UpdateStatus($"Parallel file distribution completed to all {targetClients.Count} clients", Color.Green);
                
                // Запускаем таймер для проверки подтверждений (адаптивный таймаут)
                _ = Task.Run(async () =>
                {
                    // Увеличиваем таймаут для больших файлов
                    var timeoutMs = fileTransferInfo.TotalChunks > 5000 ? 120000 : // 2 минуты для очень больших файлов
                                   fileTransferInfo.TotalChunks > 1000 ? 60000 :   // 1 минута для больших файлов
                                   30000; // 30 секунд для обычных файлов
                    
                    UpdateStatus($"Waiting for client confirmations (timeout: {timeoutMs/1000}s, file: {fileTransferInfo.TotalChunks} chunks)", Color.Blue);
                    await Task.Delay(timeoutMs);
                    if (_activeFileTransfers.ContainsKey(fileTransferInfo.FileId))
                    {
                        var info = _activeFileTransfers[fileTransferInfo.FileId];
                        if (info.ClientsConfirmed.Count < info.TargetClients.Count)
                        {
                            UpdateStatus($"WARNING: File transfer timeout - only {info.ClientsConfirmed.Count}/{info.TargetClients.Count} clients confirmed receipt of '{info.FileName}'", Color.Orange);
                            
                            // Удаляем файл и передачу из-за таймаута
                            if (!string.IsNullOrEmpty(info.FilePath) && File.Exists(info.FilePath))
                            {
                                File.Delete(info.FilePath);
                                UpdateStatus($"File deleted due to timeout: {info.FilePath}", Color.Orange);
                            }
                            _activeFileTransfers.Remove(fileTransferInfo.FileId);
                        }
                    }
                });

                // Уведомление будет отправлено только после подтверждения получения всеми клиентами
                // в методе HandleFileReceiptConfirmation
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error distributing file to clients: {ex.Message}", Color.Red);
            }
        }

        private async Task SendFileToClient(IPEndPoint clientEndPoint, FileTransferInfo fileTransferInfo, string filePath)
        {
            try
            {
                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Unknown";
                UpdateStatus($"Starting file transfer to {nickname}...", Color.Blue);
                
                // Проверяем, есть ли у клиента ключи шифрования
                if (!_clientAesKeys.ContainsKey(clientEndPoint) ||
                    _clientAesKeys[clientEndPoint].AesKey == null ||
                    _clientAesKeys[clientEndPoint].AesIV == null)
                {
                    UpdateStatus($"ERROR: No encryption keys for client {nickname}", Color.Red);
                    return;
                }

                var aesKeyIVPair = _clientAesKeys[clientEndPoint];

                // Отправляем начало передачи файла
                var fileStart = new FileTransferStart
                {
                    FileId = fileTransferInfo.FileId,
                    FileName = fileTransferInfo.FileName,
                    FileSize = fileTransferInfo.FileSize,
                    TotalChunks = fileTransferInfo.TotalChunks
                };

                var startJson = JsonConvert.SerializeObject(fileStart);
                var startMessage = $"FILE_TRANSFER_START:{startJson}";
                var encryptedStart = EncryptWithAES(startMessage, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                var encryptedStartBase64 = Convert.ToBase64String(encryptedStart);
                var encryptedStartBytes = Encoding.UTF8.GetBytes(encryptedStartBase64);
                await _udpClient.SendAsync(encryptedStartBytes, encryptedStartBytes.Length, clientEndPoint);

                // Отправляем чанки файла
                const int chunkSize = 32768;
                for (int i = 0; i < fileTransferInfo.TotalChunks; i++)
                {
                    var startIndex = i * chunkSize;
                    var chunkLength = Math.Min(chunkSize, (int)(fileTransferInfo.FileSize - startIndex));
                    var chunkData = new byte[chunkLength];
                    Array.Copy(fileTransferInfo.FileData, startIndex, chunkData, 0, chunkLength);

                    var fileChunk = new FileChunk
                    {
                        FileId = fileTransferInfo.FileId,
                        ChunkNumber = i,
                        TotalChunks = fileTransferInfo.TotalChunks,
                        Data = chunkData,
                        CheckSum = ComputeMD5Hash(chunkData)
                    };

                    var chunkJson = JsonConvert.SerializeObject(fileChunk);
                    var chunkMessage = $"FILE_CHUNK:{chunkJson}";
                    var encryptedChunk = EncryptWithAES(chunkMessage, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                    var encryptedChunkBase64 = Convert.ToBase64String(encryptedChunk);
                    var encryptedChunkBytes = Encoding.UTF8.GetBytes(encryptedChunkBase64);
                    await _udpClient.SendAsync(encryptedChunkBytes, encryptedChunkBytes.Length, clientEndPoint);

                    // Адаптивная задержка: больше для больших файлов для стабильности UDP
                    var delay = fileTransferInfo.TotalChunks > 1000 ? 20 : 10;
                    await Task.Delay(delay);
                    
                    // Освобождаем поток реже для больших файлов для повышения производительности
                    var yieldInterval = fileTransferInfo.TotalChunks > 1000 ? 100 : 50;
                    if (i % yieldInterval == 0)
                    {
                        await Task.Yield();
                    }
                }

                // Отправляем завершение передачи файла
                var fileComplete = new FileTransferComplete
                {
                    FileId = fileTransferInfo.FileId,
                    Success = true,
                    ErrorMessage = null
                };

                var completeJson = JsonConvert.SerializeObject(fileComplete);
                var completeMessage = $"FILE_TRANSFER_COMPLETE:{completeJson}";
                var encryptedComplete = EncryptWithAES(completeMessage, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                var encryptedCompleteBase64 = Convert.ToBase64String(encryptedComplete);
                var encryptedCompleteBytes = Encoding.UTF8.GetBytes(encryptedCompleteBase64);
                await _udpClient.SendAsync(encryptedCompleteBytes, encryptedCompleteBytes.Length, clientEndPoint);
                
                UpdateStatus($"File transfer completed to {nickname}", Color.Green);
            }
            catch (Exception ex)
            {
                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Unknown";
                UpdateStatus($"ERROR sending file to client {nickname}: {ex.Message}", Color.Red);
            }
        }

        private async Task HandleFileReceiptConfirmation(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var confirmation = JsonConvert.DeserializeObject<FileReceiptConfirmation>(json);
                if (confirmation == null) return;

                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Unknown";

                if (_activeFileTransfers.ContainsKey(confirmation.FileId))
                {
                    var fileTransferInfo = _activeFileTransfers[confirmation.FileId];

                    if (confirmation.Success && !fileTransferInfo.ClientsConfirmed.Contains(clientEndPoint))
                    {
                        fileTransferInfo.ClientsConfirmed.Add(clientEndPoint);
                        UpdateStatus($"File receipt confirmed by {nickname} ({fileTransferInfo.ClientsConfirmed.Count}/{fileTransferInfo.TargetClients.Count})", Color.Blue);

                        // Проверяем, все ли целевые клиенты подтвердили получение
                        if (fileTransferInfo.ClientsConfirmed.Count >= fileTransferInfo.TargetClients.Count)
                        {
                            UpdateStatus($"All clients confirmed receipt. Broadcasting notification and deleting file.", Color.Green);
                            
                            // Уведомляем всех клиентов о получении файла ТОЛЬКО после подтверждения всеми
                            var notificationMessage = $"FILE_RECEIVED:File '{fileTransferInfo.FileName}' received from {fileTransferInfo.SenderNickname} ({FormatFileSize(fileTransferInfo.FileSize)})";
                            await BroadcastMessage(notificationMessage, null);

                            // Удаляем файл с сервера, используя сохраненный путь
                            if (!string.IsNullOrEmpty(fileTransferInfo.FilePath) && File.Exists(fileTransferInfo.FilePath))
                            {
                                File.Delete(fileTransferInfo.FilePath);
                                UpdateStatus($"File deleted after distribution: {fileTransferInfo.FilePath}", Color.Blue);
                            }

                            // Удаляем информацию о передаче из активных
                            _activeFileTransfers.Remove(confirmation.FileId);
                        }
                    }
                    else if (!confirmation.Success)
                    {
                        UpdateStatus($"File receipt FAILED for {nickname}", Color.Red);
                    }
                }
                else
                {
                    UpdateStatus($"Received confirmation for unknown file from {nickname}", Color.Orange);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error handling file receipt confirmation: {ex.Message}", Color.Red);
            }
        }

        private async Task RequestMissingChunksFromClient(FileTransferInfo fileTransferInfo, IPEndPoint clientEndPoint)
        {
            try
            {
                // Определяем какие чанки отсутствуют используя точное отслеживание
                var missingChunkNumbers = new List<int>();
                
                for (int i = 0; i < fileTransferInfo.TotalChunks; i++)
                {
                    if (!fileTransferInfo.ReceivedChunkNumbers.Contains(i))
                    {
                        missingChunkNumbers.Add(i);
                    }
                }
                
                if (missingChunkNumbers.Count == 0)
                {
                    // Нет отсутствующих чанков - файл должен быть обработан в HandleFileTransferComplete
                    UpdateStatus($"No missing chunks detected, file should be processed via completion signal", Color.Blue);
                    return;
                }
                
                UpdateStatus($"Requesting {missingChunkNumbers.Count} missing chunks: {string.Join(", ", missingChunkNumbers.Take(10))}{(missingChunkNumbers.Count > 10 ? "..." : "")}", Color.Blue);
                
                // Отправляем запрос недостающих чанков клиенту
                var missingChunksRequest = new
                {
                    FileId = fileTransferInfo.FileId,
                    MissingChunks = missingChunkNumbers
                };
                
                var requestJson = JsonConvert.SerializeObject(missingChunksRequest);
                var requestMessage = $"MISSING_CHUNKS_REQUEST:{requestJson}";
                
                if (_clientAesKeys.ContainsKey(clientEndPoint))
                {
                    var aesKeyIVPair = _clientAesKeys[clientEndPoint];
                    var encryptedRequest = EncryptWithAES(requestMessage, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                    var encryptedRequestBase64 = Convert.ToBase64String(encryptedRequest);
                    var requestBytes = Encoding.UTF8.GetBytes(encryptedRequestBase64);
                    await _udpClient.SendAsync(requestBytes, requestBytes.Length, clientEndPoint);
                }
                
                // Устанавливаем таймаут на получение недостающих чанков
                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000); // 10 секунд на получение недостающих чанков
                    if (_activeFileTransfers.ContainsKey(fileTransferInfo.FileId))
                    {
                        var info = _activeFileTransfers[fileTransferInfo.FileId];
                        if (!info.IsComplete)
                        {
                            var newCompletionRate = (double)info.ReceivedChunks / info.TotalChunks * 100;
                            if (info.ReceivedChunks == info.TotalChunks && !info.IsComplete) // Если получили ВСЕ чанки
                            {
                                UpdateStatus($"Missing chunks recovery successful! File complete ({newCompletionRate:F1}%), starting distribution...", Color.Green);
                                info.IsComplete = true;
                                await SaveFileAndDistributeToClients(info);
                            }
                            else if (newCompletionRate < 95.0)
                            {
                                UpdateStatus($"Missing chunks recovery failed, file transfer incomplete ({newCompletionRate:F1}%)", Color.Red);
                                _activeFileTransfers.Remove(fileTransferInfo.FileId);
                            }
                            // Если получили некоторые чанки но не все - продолжаем ждать
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error requesting missing chunks: {ex.Message}", Color.Red);
            }
        }

        private async Task HandleMissingChunksRequest(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<MissingChunksRequest>(json);
                if (request == null) return;

                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Unknown";
                UpdateStatus($"Client {nickname} requested {request.MissingChunkNumbers.Count} missing chunks for file {request.FileId}", Color.Blue);

                if (_activeFileTransfers.ContainsKey(request.FileId))
                {
                    var fileTransferInfo = _activeFileTransfers[request.FileId];
                    
                    // Проверяем, есть ли у клиента ключи шифрования
                    if (!_clientAesKeys.ContainsKey(clientEndPoint) ||
                        _clientAesKeys[clientEndPoint].AesKey == null ||
                        _clientAesKeys[clientEndPoint].AesIV == null)
                    {
                        UpdateStatus($"ERROR: No encryption keys for client {nickname}", Color.Red);
                        return;
                    }

                    var aesKeyIVPair = _clientAesKeys[clientEndPoint];

                    // Отправляем только запрошенные чанки
                    foreach (var chunkNumber in request.MissingChunkNumbers)
                    {
                        if (chunkNumber >= 0 && chunkNumber < fileTransferInfo.TotalChunks)
                        {
                            const int chunkSize = 32768;
                            var startIndex = chunkNumber * chunkSize;
                            var chunkLength = Math.Min(chunkSize, (int)(fileTransferInfo.FileSize - startIndex));
                            var chunkData = new byte[chunkLength];
                            Array.Copy(fileTransferInfo.FileData, startIndex, chunkData, 0, chunkLength);

                            var fileChunk = new FileChunk
                            {
                                FileId = fileTransferInfo.FileId,
                                ChunkNumber = chunkNumber,
                                TotalChunks = fileTransferInfo.TotalChunks,
                                Data = chunkData,
                                CheckSum = ComputeMD5Hash(chunkData)
                            };

                            var chunkJson = JsonConvert.SerializeObject(fileChunk);
                            var chunkMessage = $"FILE_CHUNK:{chunkJson}";
                            var encryptedChunk = EncryptWithAES(chunkMessage, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                            var encryptedChunkBase64 = Convert.ToBase64String(encryptedChunk);
                            var encryptedChunkBytes = Encoding.UTF8.GetBytes(encryptedChunkBase64);
                            await _udpClient.SendAsync(encryptedChunkBytes, encryptedChunkBytes.Length, clientEndPoint);

                            // Небольшая задержка между повторными отправками
                            await Task.Delay(5);
                        }
                    }

                    UpdateStatus($"Resent {request.MissingChunkNumbers.Count} missing chunks to {nickname}", Color.Green);
                }
                else
                {
                    UpdateStatus($"Client {nickname} requested missing chunks for unknown file {request.FileId}", Color.Orange);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error handling missing chunks request: {ex.Message}", Color.Red);
            }
        }

        // ТИХИЕ версии - без логирования в чат
        private async Task HandleFileReceiptConfirmationSilently(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var confirmation = JsonConvert.DeserializeObject<FileReceiptConfirmation>(json);
                if (confirmation == null) return;

                if (_activeFileTransfers.ContainsKey(confirmation.FileId))
                {
                    var fileTransferInfo = _activeFileTransfers[confirmation.FileId];

                    if (confirmation.Success && !fileTransferInfo.ClientsConfirmed.Contains(clientEndPoint))
                    {
                        fileTransferInfo.ClientsConfirmed.Add(clientEndPoint);

                        // Проверяем, все ли целевые клиенты подтвердили получение
                        if (fileTransferInfo.ClientsConfirmed.Count >= fileTransferInfo.TargetClients.Count)
                        {
                            // Уведомляем всех клиентов о получении файла ТОЛЬКО после подтверждения всеми
                            var notificationMessage = $"FILE_RECEIVED:File '{fileTransferInfo.FileName}' received from {fileTransferInfo.SenderNickname} ({FormatFileSize(fileTransferInfo.FileSize)})";
                            await BroadcastMessage(notificationMessage, null);

                            // Удаляем файл с сервера
                            if (!string.IsNullOrEmpty(fileTransferInfo.FilePath) && File.Exists(fileTransferInfo.FilePath))
                            {
                                File.Delete(fileTransferInfo.FilePath);
                            }

                            _activeFileTransfers.Remove(confirmation.FileId);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки тихо
            }
        }

        private async Task HandleMissingChunksRequestSilently(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<MissingChunksRequest>(json);
                if (request == null) return;

                if (_activeFileTransfers.ContainsKey(request.FileId))
                {
                    var fileTransferInfo = _activeFileTransfers[request.FileId];
                    
                    if (!_clientAesKeys.ContainsKey(clientEndPoint) ||
                        _clientAesKeys[clientEndPoint].AesKey == null ||
                        _clientAesKeys[clientEndPoint].AesIV == null)
                    {
                        return;
                    }

                    var aesKeyIVPair = _clientAesKeys[clientEndPoint];

                    // Отправляем только запрошенные чанки БЕЗ логирования
                    foreach (var chunkNumber in request.MissingChunkNumbers)
                    {
                        if (chunkNumber >= 0 && chunkNumber < fileTransferInfo.TotalChunks)
                        {
                            const int chunkSize = 32768;
                            var startIndex = chunkNumber * chunkSize;
                            var chunkLength = Math.Min(chunkSize, (int)(fileTransferInfo.FileSize - startIndex));
                            var chunkData = new byte[chunkLength];
                            Array.Copy(fileTransferInfo.FileData, startIndex, chunkData, 0, chunkLength);

                            var fileChunk = new FileChunk
                            {
                                FileId = fileTransferInfo.FileId,
                                ChunkNumber = chunkNumber,
                                TotalChunks = fileTransferInfo.TotalChunks,
                                Data = chunkData,
                                CheckSum = ComputeMD5Hash(chunkData)
                            };

                            var chunkJson = JsonConvert.SerializeObject(fileChunk);
                            var chunkMessage = $"FILE_CHUNK:{chunkJson}";
                            var encryptedChunk = EncryptWithAES(chunkMessage, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                            var encryptedChunkBase64 = Convert.ToBase64String(encryptedChunk);
                            var encryptedChunkBytes = Encoding.UTF8.GetBytes(encryptedChunkBase64);
                            await _udpClient.SendAsync(encryptedChunkBytes, encryptedChunkBytes.Length, clientEndPoint);

                            await Task.Delay(5);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки тихо
            }
        }

        #endregion
    }
}