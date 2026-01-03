using System;
using System.Collections.Generic;
using System.Net;

namespace TrycryperClient
{
    namespace FileTransferModels
    {
        /// <summary>
        /// Модель для начала передачи файла
        /// </summary>
        public class FileTransferStart
        {
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public string FileId { get; set; }
            public int TotalChunks { get; set; }
        }

        /// <summary>
        /// Модель для чанка файла
        /// </summary>
        public class FileChunk
        {
            public string FileId { get; set; }
            public int ChunkNumber { get; set; }
            public int TotalChunks { get; set; }
            public byte[] Data { get; set; }
            public string CheckSum { get; set; }
        }

        /// <summary>
        /// Модель для подтверждения получения чанка
        /// </summary>
        public class ChunkAcknowledgment
        {
            public string FileId { get; set; }
            public int ChunkNumber { get; set; }
            public bool Success { get; set; }
        }

        /// <summary>
        /// Модель для завершения передачи файла
        /// </summary>
        public class FileTransferComplete
        {
            public string FileId { get; set; }
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
        }

        /// <summary>
        /// Модель для подтверждения получения целого файла клиентом
        /// </summary>
        public class FileReceiptConfirmation
        {
            public string FileId { get; set; }
            public bool Success { get; set; }
        }

        /// <summary>
        /// Модель для запроса недостающих чанков
        /// </summary>
        public class MissingChunksRequest
        {
            public string FileId { get; set; }
            public List<int> MissingChunkNumbers { get; set; }
        }
    }
}