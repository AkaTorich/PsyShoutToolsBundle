#include "ArchiveProcessor.h"
#include "StringUtils.h"
#include "Crypto.h"
#include "Compression.h"

void processSingleItem(const fs::path& itemPath, const fs::path& basePath, const std::vector<uint8_t>& key, std::ofstream& outfile, HWND hwnd) {
    fs::path relativePath = fs::relative(itemPath, basePath);
    std::string relativePathUtf8 = WideStringToUTF8(relativePath.wstring());

    std::vector<uint8_t> filenameData(relativePathUtf8.begin(), relativePathUtf8.end());
    std::vector<uint8_t> ivName;
    filenameData = aesEncrypt(filenameData, key, ivName);

    uint32_t ivNameSize = static_cast<uint32_t>(ivName.size());
    outfile.write(reinterpret_cast<const char*>(&ivNameSize), sizeof(ivNameSize));
    outfile.write(reinterpret_cast<const char*>(ivName.data()), ivNameSize);

    uint32_t nameLen = static_cast<uint32_t>(filenameData.size());
    outfile.write(reinterpret_cast<const char*>(&nameLen), sizeof(nameLen));
    outfile.write(reinterpret_cast<const char*>(filenameData.data()), nameLen);

    uint8_t isDirectory = fs::is_directory(itemPath) ? 1 : 0;
    outfile.write(reinterpret_cast<const char*>(&isDirectory), sizeof(isDirectory));

    if (isDirectory) {
        return;
    }
    else {
        std::ifstream infile(itemPath, std::ios::binary);
        if (!infile) {
            throw RuntimeErrorW(L"Failed to open file for reading: " + itemPath.wstring());
        }

        const size_t CHUNK_SIZE = 1024 * 1024;
        std::vector<uint8_t> buffer(CHUNK_SIZE);
        uint64_t chunkCount = 0;

        infile.seekg(0, std::ios::end);
        std::streamsize fileSize = infile.tellg();
        infile.seekg(0, std::ios::beg);
        chunkCount = (fileSize + CHUNK_SIZE - 1) / CHUNK_SIZE;

        uint64_t numChunks = chunkCount;
        outfile.write(reinterpret_cast<const char*>(&numChunks), sizeof(numChunks));

        if (numChunks == 0) {
            return;
        }

        while (infile) {
            infile.read(reinterpret_cast<char*>(buffer.data()), CHUNK_SIZE);
            std::streamsize bytesRead = infile.gcount();
            if (bytesRead <= 0) break;

            std::vector<uint8_t> chunkData(buffer.begin(), buffer.begin() + bytesRead);
            std::vector<uint8_t> iv;

            std::vector<uint8_t> compressedChunk = lzmaCompressBuffer(chunkData);
            std::vector<uint8_t> encryptedChunk = aesEncrypt(compressedChunk, key, iv);

            uint32_t ivSize = static_cast<uint32_t>(iv.size());
            outfile.write(reinterpret_cast<const char*>(&ivSize), sizeof(ivSize));
            outfile.write(reinterpret_cast<const char*>(iv.data()), ivSize);

            uint64_t dataSize = static_cast<uint64_t>(encryptedChunk.size());
            outfile.write(reinterpret_cast<const char*>(&dataSize), sizeof(dataSize));
            outfile.write(reinterpret_cast<const char*>(encryptedChunk.data()), encryptedChunk.size());

            PostMessageW(hwnd, WM_PROGRESS_UPDATE, 0, 0);
        }
    }
}

void processItems(const std::vector<std::wstring>& items, const std::vector<uint8_t>& key, std::ofstream& outfile, HWND hwnd) {
    for (const auto& item : items) {
        fs::path itemPath = fs::absolute(item);
        fs::path basePath = itemPath.parent_path();
        processSingleItem(itemPath, basePath, key, outfile, hwnd);

        if (fs::is_directory(itemPath)) {
            for (const auto& entry : fs::recursive_directory_iterator(itemPath)) {
                processSingleItem(entry.path(), basePath, key, outfile, hwnd);
            }
        }
    }
}

void compressAndEncrypt(const std::vector<std::wstring>& keyfiles, const std::wstring& outputArchive, const std::vector<std::wstring>& inputItems, HWND hwnd) {
    std::vector<uint8_t> key = combineKeys(keyfiles);

    uint64_t totalChunks = calculateTotalChunks(inputItems);
    if (totalChunks == 0) totalChunks = 1;

    SendMessageW(GetDlgItem(hwnd, ID_PROGRESS_BAR), PBM_SETRANGE32, 0, static_cast<LPARAM>(totalChunks));
    SendMessageW(GetDlgItem(hwnd, ID_PROGRESS_BAR), PBM_SETSTEP, 1, 0);
    SendMessageW(hwnd, WM_PROGRESS_RESET, 0, 0);

    std::ofstream outfile(outputArchive, std::ios::binary);
    if (!outfile) {
        throw RuntimeErrorW(L"Failed to create output archive file");
    }

    outfile.write(reinterpret_cast<const char*>(&totalChunks), sizeof(totalChunks));

    processItems(inputItems, key, outfile, hwnd);
}

void decryptAndExtract(const std::vector<std::wstring>& keyfiles, const std::wstring& inputArchive, const std::wstring& outputFolder, HWND hwnd) {
    std::vector<uint8_t> key = combineKeys(keyfiles);

    std::ifstream infile(inputArchive, std::ios::binary);
    if (!infile) {
        throw RuntimeErrorW(L"Failed to open input archive file");
    }

    uint64_t totalChunks = 0;
    if (!infile.read(reinterpret_cast<char*>(&totalChunks), sizeof(totalChunks))) {
        throw RuntimeErrorW(L"Failed to read total chunks from archive");
    }

    SendMessageW(GetDlgItem(hwnd, ID_PROGRESS_BAR), PBM_SETRANGE32, 0, static_cast<LPARAM>(totalChunks));
    SendMessageW(GetDlgItem(hwnd, ID_PROGRESS_BAR), PBM_SETSTEP, 1, 0);
    SendMessageW(hwnd, WM_PROGRESS_RESET, 0, 0);

    while (infile && infile.peek() != EOF) {
        uint32_t ivNameSize = 0;
        if (!infile.read(reinterpret_cast<char*>(&ivNameSize), sizeof(ivNameSize))) {
            break;
        }

        if (ivNameSize == 0 || ivNameSize > 1024) {
            throw RuntimeErrorW(L"Invalid IV size for file/folder name");
        }

        std::vector<uint8_t> ivName(ivNameSize);
        if (!infile.read(reinterpret_cast<char*>(ivName.data()), ivNameSize)) {
            throw RuntimeErrorW(L"Error reading IV for file/folder name");
        }

        uint32_t nameLen = 0;
        if (!infile.read(reinterpret_cast<char*>(&nameLen), sizeof(nameLen))) {
            throw RuntimeErrorW(L"Error reading file/folder name size");
        }

        if (nameLen == 0 || nameLen > 65536) {
            throw RuntimeErrorW(L"Invalid file/folder name size");
        }

        std::vector<uint8_t> filenameData(nameLen);
        if (!infile.read(reinterpret_cast<char*>(filenameData.data()), nameLen)) {
            throw RuntimeErrorW(L"Error reading file/folder name");
        }

        std::vector<uint8_t> decryptedFilenameData = aesDecrypt(filenameData, key, ivName);

        std::string filenameUtf8(decryptedFilenameData.begin(), decryptedFilenameData.end());
        std::wstring relativePath = UTF8ToWideString(filenameUtf8);

        std::wstring fullPath = fs::path(outputFolder) / relativePath;

        uint8_t isDirectory = 0;
        if (!infile.read(reinterpret_cast<char*>(&isDirectory), sizeof(isDirectory))) {
            throw RuntimeErrorW(L"Error reading directory flag");
        }

        if (isDirectory) {
            fs::create_directories(fullPath);
        }
        else {
            uint64_t numChunks = 0;
            if (!infile.read(reinterpret_cast<char*>(&numChunks), sizeof(numChunks))) {
                throw RuntimeErrorW(L"Error reading file chunk count");
            }

            if (numChunks == 0) {
                std::ofstream outfile(fullPath, std::ios::binary);
                if (!outfile) {
                    throw RuntimeErrorW(L"Failed to create empty file: " + fullPath);
                }
                continue;
            }

            for (uint64_t i = 0; i < numChunks; ++i) {
                uint32_t ivSize = 0;
                if (!infile.read(reinterpret_cast<char*>(&ivSize), sizeof(ivSize))) {
                    throw RuntimeErrorW(L"Error reading chunk IV size");
                }

                if (ivSize == 0 || ivSize > 1024) {
                    throw RuntimeErrorW(L"Invalid chunk IV size");
                }

                std::vector<uint8_t> iv(ivSize);
                if (!infile.read(reinterpret_cast<char*>(iv.data()), ivSize)) {
                    throw RuntimeErrorW(L"Error reading chunk IV");
                }

                uint64_t dataSize = 0;
                if (!infile.read(reinterpret_cast<char*>(&dataSize), sizeof(dataSize))) {
                    throw RuntimeErrorW(L"Error reading chunk data size");
                }

                if (dataSize == 0) {
                    throw RuntimeErrorW(L"Invalid chunk data size");
                }

                std::vector<uint8_t> encryptedData(static_cast<size_t>(dataSize));
                if (!infile.read(reinterpret_cast<char*>(encryptedData.data()), dataSize)) {
                    throw RuntimeErrorW(L"Error reading chunk data");
                }

                std::vector<uint8_t> decryptedData = aesDecrypt(encryptedData, key, iv);
                std::vector<uint8_t> decompressedData = lzmaDecompressBuffer(decryptedData);

                std::ofstream outfile(fullPath, std::ios::binary | std::ios::app);
                if (!outfile) {
                    throw RuntimeErrorW(L"Failed to open file for writing: " + fullPath);
                }
                outfile.write(reinterpret_cast<const char*>(decompressedData.data()), decompressedData.size());

                PostMessageW(hwnd, WM_PROGRESS_UPDATE, 0, 0);
            }
        }
    }
}

uint64_t calculateTotalChunks(const std::vector<std::wstring>& items) {
    uint64_t totalChunks = 0;
    const size_t CHUNK_SIZE = 1024 * 1024;
    for (const auto& item : items) {
        fs::path itemPath = fs::absolute(item);

        if (fs::is_directory(itemPath)) {
            for (const auto& entry : fs::recursive_directory_iterator(itemPath)) {
                if (fs::is_regular_file(entry.path())) {
                    uintmax_t fileSize = fs::file_size(entry.path());
                    totalChunks += (fileSize + CHUNK_SIZE - 1) / CHUNK_SIZE;
                }
            }
        }
        else if (fs::is_regular_file(itemPath)) {
            uintmax_t fileSize = fs::file_size(itemPath);
            totalChunks += (fileSize + CHUNK_SIZE - 1) / CHUNK_SIZE;
        }
    }

    return totalChunks;
}