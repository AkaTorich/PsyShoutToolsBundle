#include "FileOperations.h"
#include "StringUtils.h"

std::vector<uint8_t> readFile(const std::wstring& filename) {
    std::ifstream infile(filename, std::ios::binary);
    if (!infile) {
        throw RuntimeErrorW(L"Failed to open file: " + filename);
    }
    infile.seekg(0, std::ios::end);
    std::streamsize size = infile.tellg();
    infile.seekg(0, std::ios::beg);
    std::vector<uint8_t> buffer(static_cast<size_t>(size));
    if (!infile.read(reinterpret_cast<char*>(buffer.data()), size)) {
        throw RuntimeErrorW(L"Failed to read file: " + filename);
    }
    return buffer;
}

void writeFile(const std::wstring& filename, const std::vector<uint8_t>& data) {
    fs::create_directories(fs::path(filename).parent_path());

    std::ofstream outfile(filename, std::ios::binary);
    if (!outfile) {
        throw RuntimeErrorW(L"Failed to create file for writing: " + filename);
    }
    outfile.write(reinterpret_cast<const char*>(data.data()), data.size());
}

void SelectItems(HWND hwnd) {
    OPENFILENAMEW ofn;
    wchar_t szFile[8192] = { 0 };
    ZeroMemory(&ofn, sizeof(ofn));
    ofn.lStructSize = sizeof(ofn);
    ofn.hwndOwner = hwnd;
    ofn.lpstrFile = szFile;
    ofn.nMaxFile = sizeof(szFile) / sizeof(wchar_t);
    ofn.lpstrFilter = L"All files\0*.*\0";
    ofn.Flags = OFN_ALLOWMULTISELECT | OFN_EXPLORER | OFN_FILEMUSTEXIST;

    if (GetOpenFileNameW(&ofn)) {
        wchar_t* p = ofn.lpstrFile;
        std::wstring dir = p;
        p += dir.size() + 1;
        if (*p == 0) {
            inputItems.push_back(dir);
        }
        else {
            while (*p) {
                inputItems.push_back(dir + L"\\" + p);
                p += wcslen(p) + 1;
            }
        }
    }
}

void SelectFolders(HWND hwnd) {
    HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    if (FAILED(hr)) {
        throw RuntimeErrorW(L"COM initialization error");
    }

    IFileOpenDialog* pFileOpen = nullptr;
    hr = CoCreateInstance(CLSID_FileOpenDialog, NULL, CLSCTX_ALL,
        IID_PPV_ARGS(&pFileOpen));
    if (SUCCEEDED(hr)) {
        DWORD dwOptions;
        hr = pFileOpen->GetOptions(&dwOptions);
        if (SUCCEEDED(hr)) {
            dwOptions |= FOS_PICKFOLDERS;
            dwOptions |= FOS_ALLOWMULTISELECT;
            hr = pFileOpen->SetOptions(dwOptions);
            if (SUCCEEDED(hr)) {
                hr = pFileOpen->Show(hwnd);
                if (SUCCEEDED(hr)) {
                    IShellItemArray* pItemArray = nullptr;
                    hr = pFileOpen->GetResults(&pItemArray);
                    if (SUCCEEDED(hr)) {
                        DWORD numItems = 0;
                        pItemArray->GetCount(&numItems);
                        for (DWORD i = 0; i < numItems; ++i) {
                            IShellItem* pItem = nullptr;
                            hr = pItemArray->GetItemAt(i, &pItem);
                            if (SUCCEEDED(hr)) {
                                PWSTR pszFilePath = NULL;
                                hr = pItem->GetDisplayName(SIGDN_FILESYSPATH, &pszFilePath);
                                if (SUCCEEDED(hr)) {
                                    inputItems.push_back(pszFilePath);
                                    CoTaskMemFree(pszFilePath);
                                }
                                pItem->Release();
                            }
                        }
                        pItemArray->Release();
                    }
                }
            }
        }
        pFileOpen->Release();
    }
    CoUninitialize();
}

void SelectOutputFolder(HWND hwnd) {
    BROWSEINFOW bi = { 0 };
    bi.lpszTitle = L"Select folder for extraction";
    bi.hwndOwner = hwnd;
    LPITEMIDLIST pidl = SHBrowseForFolderW(&bi);
    if (pidl != 0) {
        wchar_t path[MAX_PATH];
        if (SHGetPathFromIDListW(pidl, path)) {
            outputFolder = path;
        }
        CoTaskMemFree(pidl);
    }
}

void SelectKeyFiles(HWND hwnd) {
    OPENFILENAMEW ofn;
    wchar_t szFile[8192] = { 0 };
    ZeroMemory(&ofn, sizeof(ofn));
    ofn.lStructSize = sizeof(ofn);
    ofn.hwndOwner = hwnd;
    ofn.lpstrFile = szFile;
    ofn.nMaxFile = sizeof(szFile) / sizeof(wchar_t);
    ofn.lpstrFilter = L"All files\0*.*\0";
    ofn.Flags = OFN_ALLOWMULTISELECT | OFN_EXPLORER | OFN_FILEMUSTEXIST;

    if (GetOpenFileNameW(&ofn)) {
        keyFiles.clear();
        wchar_t* p = ofn.lpstrFile;
        std::wstring dir = p;
        p += dir.size() + 1;
        if (*p == 0) {
            keyFiles.push_back(dir);
        }
        else {
            while (*p) {
                keyFiles.push_back(dir + L"\\" + p);
                p += wcslen(p) + 1;
            }
        }

        if (!keyFiles.empty()) {
            std::wstring statusMessage = L"Selected key files:\n";
            for (const auto& keyFile : keyFiles) {
                statusMessage += keyFile + L"\n";
            }
            MessageBoxW(hwnd, statusMessage.c_str(), L"Key Files", MB_OK);
        }
    }
}

void SelectArchiveFile(HWND hwnd, bool save) {
    OPENFILENAMEW ofn;
    wchar_t szFile[260] = { 0 };
    ZeroMemory(&ofn, sizeof(ofn));
    ofn.lStructSize = sizeof(ofn);
    ofn.hwndOwner = hwnd;
    ofn.lpstrFile = szFile;
    ofn.nMaxFile = sizeof(szFile) / sizeof(wchar_t);
    ofn.lpstrFilter = L"Archive (*.bin)\0*.bin\0All files\0*.*\0";
    ofn.Flags = save ? OFN_OVERWRITEPROMPT : OFN_FILEMUSTEXIST;
    ofn.lpstrDefExt = L"bin";

    if (save) {
        if (GetSaveFileNameW(&ofn)) {
            archiveFile = ofn.lpstrFile;
        }
    }
    else {
        if (GetOpenFileNameW(&ofn)) {
            archiveFile = ofn.lpstrFile;
        }
    }
}