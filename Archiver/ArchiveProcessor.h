#pragma once
#include "Common.h"

void processSingleItem(const fs::path& itemPath, const fs::path& basePath, const std::vector<uint8_t>& key, std::ofstream& outfile, HWND hwnd);
void processItems(const std::vector<std::wstring>& items, const std::vector<uint8_t>& key, std::ofstream& outfile, HWND hwnd);
void compressAndEncrypt(const std::vector<std::wstring>& keyfiles, const std::wstring& outputArchive, const std::vector<std::wstring>& inputItems, HWND hwnd);
void decryptAndExtract(const std::vector<std::wstring>& keyfiles, const std::wstring& inputArchive, const std::wstring& outputFolder, HWND hwnd);
uint64_t calculateTotalChunks(const std::vector<std::wstring>& items);