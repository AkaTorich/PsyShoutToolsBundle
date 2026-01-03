#pragma once
#include "Common.h"

std::vector<uint8_t> readFile(const std::wstring& filename);
void writeFile(const std::wstring& filename, const std::vector<uint8_t>& data);
void SelectItems(HWND hwnd);
void SelectFolders(HWND hwnd);
void SelectKeyFiles(HWND hwnd);
void SelectArchiveFile(HWND hwnd, bool save);
void SelectOutputFolder(HWND hwnd);