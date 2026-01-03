#include "UI.h"
#include "StringUtils.h"
#include "FileOperations.h"
#include "ArchiveProcessor.h"

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    static HWND btnEncrypt, btnDecrypt, btnSelectItems, btnSelectKeys, btnSelectArchive, lblStatus, hListView, btnSelectFolders, hProgressBar;

    switch (uMsg) {
    case WM_CREATE: {
        btnSelectItems = CreateWindowW(L"BUTTON", L"Select Files", WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
            20, 20, 180, 30, hwnd, (HMENU)ID_BTN_SELECT_ITEMS, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        btnSelectFolders = CreateWindowW(L"BUTTON", L"Select Folders", WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
            220, 20, 180, 30, hwnd, (HMENU)ID_BTN_SELECT_FOLDERS, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        btnSelectKeys = CreateWindowW(L"BUTTON", L"Select Keys", WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
            420, 20, 180, 30, hwnd, (HMENU)ID_BTN_SELECT_KEYS, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        btnEncrypt = CreateWindowW(L"BUTTON", L"Encrypt/Pack", WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
            20, 70, 180, 30, hwnd, (HMENU)ID_BTN_ENCRYPT, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        btnDecrypt = CreateWindowW(L"BUTTON", L"Decrypt/Unpack", WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
            220, 70, 180, 30, hwnd, (HMENU)ID_BTN_DECRYPT, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        btnSelectArchive = CreateWindowW(L"BUTTON", L"Select Archive", WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
            420, 70, 180, 30, hwnd, (HMENU)ID_BTN_SELECT_ARCHIVE, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        lblStatus = CreateWindowW(L"STATIC", L"", WS_VISIBLE | WS_CHILD | SS_LEFT,
            20, 110, 580, 20, hwnd, (HMENU)ID_LBL_STATUS, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        hListView = CreateWindowW(WC_LISTVIEWW, NULL,
            WS_CHILD | WS_VISIBLE | LVS_REPORT | LVS_SHOWSELALWAYS,
            20, 140, 580, 300, hwnd, (HMENU)ID_LISTVIEW, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        LVCOLUMNW lvc = {};
        lvc.mask = LVCF_WIDTH | LVCF_TEXT | LVCF_SUBITEM;

        lvc.iSubItem = 0;
        lvc.pszText = (LPWSTR)L"File/Folder Name";
        lvc.cx = 400;
        ListView_InsertColumn(hListView, 0, &lvc);

        lvc.iSubItem = 1;
        lvc.pszText = (LPWSTR)L"Size";
        lvc.cx = 160;
        ListView_InsertColumn(hListView, 1, &lvc);

        ListView_SetExtendedListViewStyle(hListView, LVS_EX_FULLROWSELECT | LVS_EX_GRIDLINES);

        hProgressBar = CreateWindowExW(0, PROGRESS_CLASSW, NULL, WS_CHILD | WS_VISIBLE,
            20, 460, 580, 20, hwnd, (HMENU)ID_PROGRESS_BAR, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);

        SendMessageW(hProgressBar, PBM_SETRANGE32, 0, 100);
        SendMessageW(hProgressBar, PBM_SETPOS, 0, 0);
    }
    return 0;

    case WM_COMMAND: {
        switch (LOWORD(wParam)) {
        case ID_BTN_SELECT_ITEMS:
            SelectItems(hwnd);
            SetWindowTextW(lblStatus, L"List updated.");
            UpdateListView(hListView);
            break;
        case ID_BTN_SELECT_FOLDERS:
            SelectFolders(hwnd);
            SetWindowTextW(lblStatus, L"List updated.");
            UpdateListView(hListView);
            break;
        case ID_BTN_SELECT_KEYS:
            SelectKeyFiles(hwnd);
            break;
        case ID_BTN_SELECT_ARCHIVE:
            SelectArchiveFile(hwnd, false);
            if (!archiveFile.empty()) {
                SetWindowTextW(lblStatus, L"Archive selected.");
            }
            break;
        case ID_BTN_ENCRYPT: {
            if (inputItems.empty()) {
                MessageBoxW(hwnd, L"Please select files or folders to process.", L"Error", MB_OK | MB_ICONERROR);
                break;
            }
            if (keyFiles.empty()) {
                MessageBoxW(hwnd, L"Please select key files.", L"Error", MB_OK | MB_ICONERROR);
                break;
            }
            SelectArchiveFile(hwnd, true);
            if (archiveFile.empty()) break;

            std::thread encryptionThread(StartEncryption, hwnd);
            encryptionThread.detach();

            SetWindowTextW(lblStatus, L"Encrypting and packing...");
        }
        break;
        case ID_BTN_DECRYPT: {
            if (archiveFile.empty()) {
                MessageBoxW(hwnd, L"Please select archive file for unpacking.", L"Error", MB_OK | MB_ICONERROR);
                break;
            }
            if (keyFiles.empty()) {
                MessageBoxW(hwnd, L"Please select key files.", L"Error", MB_OK | MB_ICONERROR);
                break;
            }
            SelectOutputFolder(hwnd);
            if (outputFolder.empty()) break;

            std::thread decryptionThread(StartDecryption, hwnd);
            decryptionThread.detach();

            SetWindowTextW(lblStatus, L"Decrypting and unpacking...");
        }
        break;
        }
    }
    return 0;

    case WM_UPDATE_STATUS:
        SetWindowTextW(lblStatus, reinterpret_cast<LPCWSTR>(lParam));
        return 0;

    case WM_PROGRESS_RESET:
        SendMessageW(hProgressBar, PBM_SETPOS, 0, 0);
        return 0;

    case WM_PROGRESS_UPDATE: {
        SendMessageW(hProgressBar, PBM_STEPIT, 0, 0);
    }
    return 0;
    case WM_GETMINMAXINFO: {
        MINMAXINFO* pMinMax = reinterpret_cast<MINMAXINFO*>(lParam);

        pMinMax->ptMaxSize.x = pMinMax->ptMaxTrackSize.x = 635;
        pMinMax->ptMaxSize.y = pMinMax->ptMaxTrackSize.y = 535;
        pMinMax->ptMinTrackSize.x = 635;
        pMinMax->ptMinTrackSize.y = 535;
    }
    return 0;
    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;
    }
    return DefWindowProcW(hwnd, uMsg, wParam, lParam);
}

void UpdateListView(HWND hListView) {
    ListView_DeleteAllItems(hListView);

    LVITEMW lvItem = {};
    lvItem.mask = LVIF_TEXT;

    int index = 0;
    for (const auto& item : inputItems) {
        fs::path itemPath = item;
        std::wstring name = itemPath.wstring();
        std::wstring sizeStr;

        try {
            if (fs::is_directory(itemPath)) {
                uintmax_t folderSize = 0;
                for (const auto& entry : fs::recursive_directory_iterator(itemPath)) {
                    if (fs::is_regular_file(entry.path())) {
                        folderSize += fs::file_size(entry.path());
                    }
                }
                std::wstringstream ss;
                ss << folderSize << L" bytes";
                sizeStr = ss.str();
            }
            else if (fs::is_regular_file(itemPath)) {
                uintmax_t fileSize = fs::file_size(itemPath);
                std::wstringstream ss;
                ss << fileSize << L" bytes";
                sizeStr = ss.str();
            }
            else {
                sizeStr = L"Unknown";
            }
        }
        catch (const std::exception&) {
            sizeStr = L"Error calculating size";
        }

        lvItem.iItem = index;
        lvItem.iSubItem = 0;
        lvItem.pszText = const_cast<LPWSTR>(name.c_str());
        ListView_InsertItem(hListView, &lvItem);

        lvItem.iSubItem = 1;
        lvItem.pszText = const_cast<LPWSTR>(sizeStr.c_str());
        ListView_SetItem(hListView, &lvItem);

        index++;
    }
}

void StartEncryption(HWND hwnd) {
    try {
        compressAndEncrypt(keyFiles, archiveFile, inputItems, hwnd);

        std::wstring statusMessage = L"Files and folders encrypted and packed.";
        PostMessageW(hwnd, WM_UPDATE_STATUS, 0, reinterpret_cast<LPARAM>(statusMessage.c_str()));
    }
    catch (const RuntimeErrorW& ex) {
        MessageBoxW(hwnd, ex.wwhat(), L"Error", MB_OK | MB_ICONERROR);
        std::wstring statusMessage = L"Error during operations.";
        PostMessageW(hwnd, WM_UPDATE_STATUS, 0, reinterpret_cast<LPARAM>(statusMessage.c_str()));
    }
    catch (const std::exception& ex) {
        std::wstring errorMessage = UTF8ToWideString(ex.what());
        MessageBoxW(hwnd, errorMessage.c_str(), L"Error", MB_OK | MB_ICONERROR);
        std::wstring statusMessage = L"Error during operations.";
        PostMessageW(hwnd, WM_UPDATE_STATUS, 0, reinterpret_cast<LPARAM>(statusMessage.c_str()));
    }
}

void StartDecryption(HWND hwnd) {
    try {
        decryptAndExtract(keyFiles, archiveFile, outputFolder, hwnd);

        std::wstring statusMessage = L"Files unpacked.";
        PostMessageW(hwnd, WM_UPDATE_STATUS, 0, reinterpret_cast<LPARAM>(statusMessage.c_str()));
    }
    catch (const RuntimeErrorW& ex) {
        MessageBoxW(hwnd, ex.wwhat(), L"Error", MB_OK | MB_ICONERROR);
        std::wstring statusMessage = L"Error during unpacking.";
        PostMessageW(hwnd, WM_UPDATE_STATUS, 0, reinterpret_cast<LPARAM>(statusMessage.c_str()));
    }
    catch (const std::exception& ex) {
        std::wstring errorMessage = UTF8ToWideString(ex.what());
        MessageBoxW(hwnd, errorMessage.c_str(), L"Error", MB_OK | MB_ICONERROR);
        std::wstring statusMessage = L"Error during unpacking.";
        PostMessageW(hwnd, WM_UPDATE_STATUS, 0, reinterpret_cast<LPARAM>(statusMessage.c_str()));
    }
}