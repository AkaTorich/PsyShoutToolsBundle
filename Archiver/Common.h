#pragma once
#include <windows.h>
#include <commctrl.h>
#include <commdlg.h>
#include <shlobj.h>
#include <string>
#include <vector>
#include <fstream>
#include <cstdint>
#include <locale>
#include <wincrypt.h>
#include <filesystem>
#include <exception>
#include <thread>
#include <sstream>
#include <cstdlib>
#include <cstring>

#pragma execution_character_set("utf-8")
#include "resource.h"
#include "easylzma/easylzma.h"

#pragma comment(lib, "advapi32.lib")
#pragma comment(lib, "shell32.lib")
#pragma comment(lib, "comdlg32.lib")
#pragma comment(lib, "comctl32.lib")

namespace fs = std::filesystem;

class RuntimeErrorW : public std::exception {
public:
    explicit RuntimeErrorW(const std::wstring& message) : message_(message) {}

    const wchar_t* wwhat() const noexcept {
        return message_.c_str();
    }

    const char* what() const noexcept override {
        return "RuntimeErrorW occurred";
    }

private:
    std::wstring message_;
};

extern std::vector<std::wstring> inputItems;
extern std::vector<std::wstring> keyFiles;
extern std::wstring archiveFile;
extern std::wstring outputFolder;

#define ID_BTN_ENCRYPT 101
#define ID_BTN_DECRYPT 102
#define ID_BTN_SELECT_ITEMS 103
#define ID_BTN_SELECT_KEYS 104
#define ID_BTN_SELECT_ARCHIVE 105
#define ID_LBL_STATUS 106
#define ID_LISTVIEW 107
#define ID_BTN_SELECT_FOLDERS 108
#define ID_PROGRESS_BAR 109

#define WM_UPDATE_STATUS (WM_USER + 1)
#define WM_PROGRESS_UPDATE (WM_USER + 2)
#define WM_PROGRESS_RESET (WM_USER + 3)