using System;
using System.Collections.Generic;
using System.Net;

namespace TrycryperServer
{
    /// <summary>
    /// Модель для начала передачи файла
    /// </summary>
    public class FileTransferStart
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileId { get; set; } = string.Empty;
        public int TotalChunks { get; set; }
    }

    /// <summary>
    /// Модель для чанка файла
    /// </summary>
    public class FileChunk
    {
        public string FileId { get; set; } = string.Empty;
        public int ChunkNumber { get; set; }
        public int TotalChunks { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string CheckSum { get; set; } = string.Empty;
    }

    /// <summary>
    /// Модель для подтверждения получения чанка
    /// </summary>
    public class ChunkAcknowledgment
    {
        public string FileId { get; set; } = string.Empty;
        public int ChunkNumber { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    /// Модель для завершения передачи файла
    /// </summary>
    public class FileTransferComplete
    {
        public string FileId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Модель для информации о передаваемом файле
    /// </summary>
    public class FileTransferInfo
    {
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int TotalChunks { get; set; }
        public string SenderNickname { get; set; } = string.Empty;
        public IPEndPoint SenderEndPoint { get; set; } = null!;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public DateTime StartTime { get; set; }
        public int ReceivedChunks { get; set; }
        public bool IsComplete { get; set; }
        public List<IPEndPoint> TargetClients { get; set; } = new List<IPEndPoint>();
        public List<IPEndPoint> ClientsConfirmed { get; set; } = new List<IPEndPoint>();
        public bool IsDistributed { get; set; } = false;
        public string FilePath { get; set; } = string.Empty; // Путь к сохраненному файлу
    }

    /// <summary>
    /// Модель для подтверждения получения целого файла клиентом
    /// </summary>
    public class FileReceiptConfirmation
    {
        public string FileId { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}