using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TrycryperServer
{
    class Program
    {
        private static UdpClient _udpClient = null!;
        private static RSACryptoServiceProvider _rsa = null!;
        private static List<IPEndPoint> _clientEndPoints = null!;
        private static Dictionary<IPEndPoint, (byte[] AesKey, byte[] AesIV)> _clientAesKeys = null!;
        private static Dictionary<IPEndPoint, string> _clientNicknames = null!; // Словарь для хранения никнеймов клиентов
        private static Dictionary<string, FileTransferInfo> _activeFileTransfers = null!; // Активные передачи файлов
        private const string DOWNLOADS_FOLDER = "Downloads";

        private static void LogWithTimestamp(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"[{timestamp}] {message}");
        }

        static async Task Main(string[] args)
        {
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

            // Чтение порта из файла конфигурации
            var port = ReadPortFromConfig("TryCryp.cfg");

            if (port == -1)
            {
                Console.WriteLine("Ошибка конфигурации. Проверь конфиг.");
                return;
            }
            
            try
            {
                _udpClient = new UdpClient(port);
                LogWithTimestamp("Сервер слушает на порту: " + port);
                //Console.WriteLine($"Открытый ключ сервера: {_rsa.ToXmlString(false)}");

                // Добавляем обработчик закрытия для корректного завершения работы
                Console.CancelKeyPress += async (sender, e) => 
                {
                    e.Cancel = true; // Предотвращаем немедленное завершение
                    LogWithTimestamp("Завершение работы сервера...");
                    await BroadcastMessage("Сервер завершает работу.", null!);
                    _udpClient.Close();
                    Environment.Exit(0);
                };

                // Запуск прослушивания
                await StartListening();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при запуске сервера: " + ex.Message);
            }
        }

        private static int ReadPortFromConfig(string configPath)
        {
            try
            {
                var lines = File.ReadAllLines(configPath);

                foreach (var line in lines)
                {
                    if (line.StartsWith("port="))
                    {
                        // Извлекаем порт и пытаемся его преобразовать в целое число
                        if (int.TryParse(line.Substring(5), out int port))
                        {
                            return port;
                        }
                    }
                }

                Console.WriteLine("Порт не найден в конфигурационном файле.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении конфигурации: " + ex.Message);
            }

            return -1;  // Возвращаем -1 в случае ошибки
        }

        private static async Task StartListening()
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
                        LogWithTimestamp("Получен никнейм: " + nickname);

                        if (!_clientEndPoints.Contains(clientEndPoint))
                        {
                            _clientEndPoints.Add(clientEndPoint);
                            _clientNicknames[clientEndPoint] = nickname; // Сохраняем никнейм клиента
                        }

                        // Отправка публичного ключа
                        var publicKeyXml = _rsa.ToXmlString(false);
                        var publicKeyBytes = Encoding.UTF8.GetBytes(publicKeyXml);
                        await _udpClient.SendAsync(publicKeyBytes, publicKeyBytes.Length, clientEndPoint);
                        LogWithTimestamp("Публичный ключ отправлен клиенту.");

                        // Уведомление всех клиентов о новом подключении
                        var broadcastMessage = $"{nickname} присоединился к чату.";
                        await BroadcastMessage(broadcastMessage, clientEndPoint);
                    }
                    else if (receivedMessage.StartsWith("DISCONNECT:"))
                    {
                        // Обработка явного отключения клиента
                        var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Неизвестный пользователь";
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
                            _clientAesKeys.Add(clientEndPoint, (aesKey, Array.Empty<byte>()));
                        }

                        //Console.WriteLine($"AES ключ получен от клиента: {Convert.ToBase64String(aesKey)}");
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
                            _clientAesKeys.Add(clientEndPoint, (Array.Empty<byte>(), aesIV));
                        }

                        //Console.WriteLine($"AES IV получен от клиента: {Convert.ToBase64String(aesIV)}");
                    }
                    else if (receivedMessage.StartsWith("PING"))
                    {
                        // Обработка проверки соединения
                        var responseBytes = Encoding.UTF8.GetBytes("PONG");
                        await _udpClient.SendAsync(responseBytes, responseBytes.Length, clientEndPoint);
                        // Console.WriteLine($"Получен PING от {clientEndPoint}, ответили PONG");
                    }
                    else
                    {
                        try
                        {
                            var encryptedMessage = Convert.FromBase64String(receivedMessage);
                            //Console.WriteLine($"Получено зашифрованное сообщение: {receivedMessage}");

                            var aesKeyIVPair = _clientAesKeys[clientEndPoint];
                            var decryptedMessage = DecryptWithAES(encryptedMessage, aesKeyIVPair.AesKey, aesKeyIVPair.AesIV);
                            
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
                                LogWithTimestamp($"Сообщение: {decryptedMessage}");
                                await BroadcastMessage(decryptedMessage, clientEndPoint);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Если возникает ошибка при обработке сообщения, это может означать, что клиент отключился
                            Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
                            
                            if (_clientEndPoints.Contains(clientEndPoint))
                            {
                                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Неизвестный пользователь";
                                await HandleClientDisconnect(clientEndPoint, nickname);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка в StartListening: " + ex.Message);
            }
        }

        private static async Task HandleClientDisconnect(IPEndPoint clientEndPoint, string nickname)
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
                    var disconnectMessage = $"{nickname} покинул чат.";
                    await BroadcastMessage(disconnectMessage, null!);
                    Console.WriteLine(disconnectMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке отключения клиента: {ex.Message}");
            }
        }

        private static async Task BroadcastMessage(string message, IPEndPoint senderEndPoint)
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
                        //Console.WriteLine($"Отправка зашифрованного сообщения клиенту {clientEndPoint}: {encryptedMessageBase64}");

                        var encryptedMessageBytes = Encoding.UTF8.GetBytes(encryptedMessageBase64);
                        await _udpClient.SendAsync(encryptedMessageBytes, encryptedMessageBytes.Length, clientEndPoint);
                    }
                    catch (Exception ex)
                    {
                        // Если не удалось отправить сообщение клиенту, возможно, он отключился
                        Console.WriteLine($"Ошибка при отправке сообщения клиенту {clientEndPoint}: {ex.Message}");
                        
                        var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Неизвестный пользователь";
                        await HandleClientDisconnect(clientEndPoint, nickname);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке сообщения: " + ex.Message);
            }
        }

        private static byte[] EncryptWithAES(string message, byte[] key, byte[] iv)
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

        private static string DecryptWithAES(byte[] encryptedMessage, byte[] key, byte[] iv)
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

        #region File Transfer Methods

        private static Task HandleFileTransferStart(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var fileTransferStart = JsonConvert.DeserializeObject<FileTransferStart>(json);
                if (fileTransferStart == null) return Task.CompletedTask;
                
                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Неизвестный пользователь";

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
                LogWithTimestamp($"Начата передача файла: {fileTransferStart.FileName} от {nickname} ({FormatFileSize(fileTransferStart.FileSize)})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке начала передачи файла: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private static async Task HandleFileChunk(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var fileChunk = JsonConvert.DeserializeObject<FileChunk>(json);
                if (fileChunk == null) return;
                
                if (!_activeFileTransfers.ContainsKey(fileChunk.FileId))
                {
                    Console.WriteLine($"Получен чанк для неизвестного файла: {fileChunk.FileId}");
                    return;
                }

                var fileTransferInfo = _activeFileTransfers[fileChunk.FileId];
                
                // Проверяем контрольную сумму
                var computedCheckSum = ComputeMD5Hash(fileChunk.Data);
                if (computedCheckSum != fileChunk.CheckSum)
                {
                    Console.WriteLine($"Несоответствие контрольной суммы для чанка {fileChunk.ChunkNumber} файла {fileTransferInfo.FileName}");
                    return;
                }

                // Копируем данные чанка в файл
                var startIndex = fileChunk.ChunkNumber * 32768; // CHUNK_SIZE
                Array.Copy(fileChunk.Data, 0, fileTransferInfo.FileData, startIndex, fileChunk.Data.Length);
                
                fileTransferInfo.ReceivedChunks++;
                
                var progress = (double)fileTransferInfo.ReceivedChunks / fileTransferInfo.TotalChunks * 100;
                                    LogWithTimestamp($"Получение {fileTransferInfo.FileName}: {progress:F1}% ({fileTransferInfo.ReceivedChunks}/{fileTransferInfo.TotalChunks} чанков)");
                
                // Проверяем, завершена ли передача
                if (fileTransferInfo.ReceivedChunks == fileTransferInfo.TotalChunks)
                {
                    LogWithTimestamp($"Все чанки файла получены: {fileTransferInfo.FileName} ({fileTransferInfo.ReceivedChunks}/{fileTransferInfo.TotalChunks})");
                    // Файл полный, но подождем сигнала FILE_TRANSFER_COMPLETE от клиента для гарантии
                    // или запустим распространение если уже получили сигнал завершения
                    if (!fileTransferInfo.IsComplete)
                    {
                        fileTransferInfo.IsComplete = true;
                        await SaveFileAndDistributeToClients(fileTransferInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке чанка файла: {ex.Message}");
            }
        }

        private static async Task HandleFileTransferComplete(string json, IPEndPoint clientEndPoint)
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
                        LogWithTimestamp($"📤 Файл получен: {fileTransferInfo.FileName} от {fileTransferInfo.SenderNickname}");
                        fileTransferInfo.IsComplete = true;
                        await SaveFileAndDistributeToClients(fileTransferInfo);
                    }
                    else
                    {
                        // Ждем дополнительные 5 секунд для получения оставшихся чанков
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(5000);
                            if (_activeFileTransfers.ContainsKey(fileTransferComplete.FileId))
                            {
                                var info = _activeFileTransfers[fileTransferComplete.FileId];
                                if (info.ReceivedChunks == info.TotalChunks && !info.IsComplete)
                                {
                                    info.IsComplete = true;
                                    await SaveFileAndDistributeToClients(info);
                                }
                                else if (info.ReceivedChunks < info.TotalChunks)
                                {
                                    _activeFileTransfers.Remove(fileTransferComplete.FileId);
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке завершения передачи файла: {ex.Message}");
            }
        }

        private static async Task SaveFileAndDistributeToClients(FileTransferInfo fileTransferInfo)
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

                LogWithTimestamp($"Файл сохранен: {filePath}");

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
                    Console.WriteLine($"Файл обработан и удален (нет целевых клиентов): {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении файла: {ex.Message}");
            }
        }

        private static string ComputeMD5Hash(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                return Convert.ToBase64String(hash);
            }
        }

        private static string FormatFileSize(long bytes)
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

        private static async Task DistributeFileToClients(FileTransferInfo fileTransferInfo, string filePath)
        {
            try
            {
                // Определяем целевых клиентов в момент распространения (все кроме отправителя)
                var targetClients = _clientEndPoints.Where(ep => !ep.Equals(fileTransferInfo.SenderEndPoint)).ToList();
                fileTransferInfo.TargetClients = targetClients;
                fileTransferInfo.IsDistributed = true;
                
                LogWithTimestamp($"Распределение файла '{fileTransferInfo.FileName}' на {targetClients.Count} клиентов ПАРАЛЛЕЛЬНО");
                
                // Отправляем файл ПАРАЛЛЕЛЬНО всем целевым клиентам одновременно
                var sendTasks = new List<Task>();
                foreach (var targetClient in targetClients)
                {
                    var nickname = _clientNicknames.ContainsKey(targetClient) ? _clientNicknames[targetClient] : "Неизвестный";
                    Console.WriteLine($"Начало параллельной отправки файла клиенту: {nickname} ({targetClient})");
                    sendTasks.Add(SendFileToClient(targetClient, fileTransferInfo, filePath));
                }
                
                // Ждем завершения отправки всем клиентам
                await Task.WhenAll(sendTasks);
                Console.WriteLine($"Параллельное распределение файла завершено для всех {targetClients.Count} клиентов");
                
                // Запускаем таймер для проверки подтверждений (30 секунд)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(30000); // 30 секунд
                    if (_activeFileTransfers.ContainsKey(fileTransferInfo.FileId))
                    {
                        var info = _activeFileTransfers[fileTransferInfo.FileId];
                        if (info.ClientsConfirmed.Count < info.TargetClients.Count)
                        {
                            Console.WriteLine($"ПРЕДУПРЕЖДЕНИЕ: Таймаут передачи файла - только {info.ClientsConfirmed.Count}/{info.TargetClients.Count} клиентов подтвердили получение '{info.FileName}'");
                            
                            // Удаляем файл и передачу из-за таймаута
                            if (!string.IsNullOrEmpty(info.FilePath) && File.Exists(info.FilePath))
                            {
                                File.Delete(info.FilePath);
                                Console.WriteLine($"Файл удален из-за таймаута: {info.FilePath}");
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
                Console.WriteLine($"Ошибка при распределении файла клиентам: {ex.Message}");
            }
        }

        private static async Task SendFileToClient(IPEndPoint clientEndPoint, FileTransferInfo fileTransferInfo, string filePath)
        {
            try
            {
                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Неизвестный";
                Console.WriteLine($"Начало передачи файла клиенту {nickname}...");
                
                // Проверяем, есть ли у клиента ключи шифрования
                if (!_clientAesKeys.ContainsKey(clientEndPoint) ||
                    _clientAesKeys[clientEndPoint].AesKey == null ||
                    _clientAesKeys[clientEndPoint].AesIV == null)
                {
                    Console.WriteLine($"ОШИБКА: Нет ключей шифрования для клиента {nickname}");
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
                    
                    // Освобождаем поток каждые 20 чанков для обеспечения отзывчивости чата
                    if (i % 20 == 0)
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
                
                Console.WriteLine($"Передача файла завершена для {nickname}");
            }
            catch (Exception ex)
            {
                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Неизвестный";
                Console.WriteLine($"ОШИБКА отправки файла клиенту {nickname}: {ex.Message}");
            }
        }

        private static async Task HandleFileReceiptConfirmation(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                var confirmation = JsonConvert.DeserializeObject<FileReceiptConfirmation>(json);
                if (confirmation == null) return;

                var nickname = _clientNicknames.ContainsKey(clientEndPoint) ? _clientNicknames[clientEndPoint] : "Неизвестный";

                if (_activeFileTransfers.ContainsKey(confirmation.FileId))
                {
                    var fileTransferInfo = _activeFileTransfers[confirmation.FileId];

                    if (confirmation.Success && !fileTransferInfo.ClientsConfirmed.Contains(clientEndPoint))
                    {
                        fileTransferInfo.ClientsConfirmed.Add(clientEndPoint);
                        LogWithTimestamp($"Подтверждение получения файла от {nickname} ({fileTransferInfo.ClientsConfirmed.Count}/{fileTransferInfo.TargetClients.Count})");

                        // Проверяем, все ли целевые клиенты подтвердили получение
                        if (fileTransferInfo.ClientsConfirmed.Count >= fileTransferInfo.TargetClients.Count)
                        {
                            LogWithTimestamp($"Все клиенты подтвердили получение. Отправка уведомления и удаление файла.");
                            
                            // Уведомляем всех клиентов о получении файла ТОЛЬКО после подтверждения всеми
                            var notificationMessage = $"FILE_RECEIVED:Файл '{fileTransferInfo.FileName}' получен от {fileTransferInfo.SenderNickname} ({FormatFileSize(fileTransferInfo.FileSize)})";
                            await BroadcastMessage(notificationMessage, null!);

                            // Удаляем файл с сервера, используя сохраненный путь
                            if (!string.IsNullOrEmpty(fileTransferInfo.FilePath) && File.Exists(fileTransferInfo.FilePath))
                            {
                                File.Delete(fileTransferInfo.FilePath);
                                Console.WriteLine($"Файл удален после распределения: {fileTransferInfo.FilePath}");
                            }

                            // Удаляем информацию о передаче из активных
                            _activeFileTransfers.Remove(confirmation.FileId);
                        }
                    }
                    else if (!confirmation.Success)
                    {
                        Console.WriteLine($"ОШИБКА получения файла у {nickname}");
                    }
                }
                else
                {
                    Console.WriteLine($"Получено подтверждение для неизвестного файла от {nickname}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке подтверждения получения файла: {ex.Message}");
            }
        }

        // ТИХИЕ версии - без логирования в чат
        private static async Task HandleFileReceiptConfirmationSilently(string json, IPEndPoint clientEndPoint)
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
                            var notificationMessage = $"FILE_RECEIVED:Файл '{fileTransferInfo.FileName}' получен от {fileTransferInfo.SenderNickname} ({FormatFileSize(fileTransferInfo.FileSize)})";
                            await BroadcastMessage(notificationMessage, null!);

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

        private static async Task HandleMissingChunksRequestSilently(string json, IPEndPoint clientEndPoint)
        {
            try
            {
                // Создаем анонимный объект для десериализации
                var request = JsonConvert.DeserializeObject<dynamic>(json);
                if (request == null) return;
                
                string fileId = request.FileId;
                var missingChunkNumbers = ((Newtonsoft.Json.Linq.JArray)request.MissingChunks).ToObject<List<int>>();
                if (missingChunkNumbers == null) return;

                if (_activeFileTransfers.ContainsKey(fileId))
                {
                    var fileTransferInfo = _activeFileTransfers[fileId];
                    
                    if (!_clientAesKeys.ContainsKey(clientEndPoint) ||
                        _clientAesKeys[clientEndPoint].AesKey == null ||
                        _clientAesKeys[clientEndPoint].AesIV == null)
                    {
                        return;
                    }

                    var aesKeyIVPair = _clientAesKeys[clientEndPoint];

                    // Отправляем только запрошенные чанки БЕЗ логирования
                    foreach (var chunkNumber in missingChunkNumbers)
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