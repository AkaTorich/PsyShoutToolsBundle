#include <windows.h> 
#include <shellapi.h> // Для Shell_NotifyIconW
#include <string>
#include <unordered_map>
#include "resource.h" // Убедитесь, что этот заголовочный файл содержит #define IDR_TRAYICON 1001

#define HOTKEY_ID 1
#define WM_TRAYICON (WM_USER + 1)
#define IDM_START 2001
#define IDM_STOP 2002
#define IDM_EXIT 2003

std::unordered_map<wchar_t, wchar_t> engToRus = {
    {L'q', L'й'}, {L'w', L'ц'}, {L'e', L'у'}, {L'r', L'к'}, {L't', L'е'}, {L'y', L'н'}, {L'u', L'г'}, {L'i', L'ш'}, {L'o', L'щ'}, {L'p', L'з'},
    {L'[', L'х'}, {L']', L'ъ'}, {L'a', L'ф'}, {L's', L'ы'}, {L'd', L'в'}, {L'f', L'а'}, {L'g', L'п'}, {L'h', L'р'}, {L'j', L'о'}, {L'k', L'л'},
    {L'l', L'д'}, {L';', L'ж'}, {L'\'', L'э'}, {L'z', L'я'}, {L'x', L'ч'}, {L'c', L'с'}, {L'v', L'м'}, {L'b', L'и'}, {L'n', L'т'}, {L'm', L'ь'},
    {L',', L'б'}, {L'.', L'ю'}, {L'`', L'ё'},
    {L'Q', L'Й'}, {L'W', L'Ц'}, {L'E', L'У'}, {L'R', L'К'}, {L'T', L'Е'}, {L'Y', L'Н'}, {L'U', L'Г'}, {L'I', L'Ш'}, {L'O', L'Щ'}, {L'P', L'З'},
    {L'{', L'Х'}, {L'}', L'Ъ'}, {L'A', L'Ф'}, {L'S', L'Ы'}, {L'D', L'В'}, {L'F', L'А'}, {L'G', L'П'}, {L'H', L'Р'}, {L'J', L'О'}, {L'K', L'Л'},
    {L'L', L'Д'}, {L':', L'Ж'}, {L'"', L'Э'}, {L'Z', L'Я'}, {L'X', L'Ч'}, {L'C', L'С'}, {L'V', L'М'}, {L'B', L'И'}, {L'N', L'Т'}, {L'M', L'Ь'},
    {L'<', L'Б'}, {L'>', L'Ю'}, {L'~', L'Ё'}
};

std::unordered_map<wchar_t, wchar_t> rusToEng = {
    {L'й', L'q'}, {L'ц', L'w'}, {L'у', L'e'}, {L'к', L'r'}, {L'е', L't'}, {L'н', L'y'}, {L'г', L'u'}, {L'ш', L'i'}, {L'щ', L'o'}, {L'з', L'p'},
    {L'х', L'['}, {L'ъ', L']'}, {L'ф', L'a'}, {L'ы', L's'}, {L'в', L'd'}, {L'а', L'f'}, {L'п', L'g'}, {L'р', L'h'}, {L'о', L'j'}, {L'л', L'k'},
    {L'д', L'l'}, {L'ж', L';'}, {L'э', L'\''}, {L'я', L'z'}, {L'ч', L'x'}, {L'с', L'c'}, {L'м', L'v'}, {L'и', L'b'}, {L'т', L'n'}, {L'ь', L'm'},
    {L'б', L','}, {L'ю', L'.'}, {L'ё', L'`'},
    {L'Й', L'Q'}, {L'Ц', L'W'}, {L'У', L'E'}, {L'К', L'R'}, {L'Е', L'T'}, {L'Н', L'Y'}, {L'Г', L'U'}, {L'Ш', L'I'}, {L'Щ', L'O'}, {L'З', L'P'},
    {L'Х', L'{'}, {L'Ъ', L'}'}, {L'Ф', L'A'}, {L'Ы', L'S'}, {L'В', L'D'}, {L'А', L'F'}, {L'П', L'G'}, {L'Р', L'H'}, {L'О', L'J'}, {L'Л', L'K'},
    {L'Д', L'L'}, {L'Ж', L':'}, {L'Э', L'"'}, {L'Я', L'Z'}, {L'Ч', L'X'}, {L'С', L'C'}, {L'М', L'V'}, {L'И', L'B'}, {L'Т', L'N'}, {L'Ь', L'M'},
    {L'Б', L'<'}, {L'Ю', L'>'}, {L'Ё', L'~'}
};

bool isRunning = true;

// Функция конвертации: для каждого символа проверяем оба словаря
std::wstring ConvertLayout(const std::wstring& text) {
    std::wstring converted;
    converted.reserve(text.size());
    for (wchar_t ch : text) {
        auto itEng = engToRus.find(ch);
        if (itEng != engToRus.end()) {
            converted.push_back(itEng->second);
        }
        else {
            auto itRus = rusToEng.find(ch);
            if (itRus != rusToEng.end()) {
                converted.push_back(itRus->second);
            }
            else {
                converted.push_back(ch);
            }
        }
    }
    return converted;
}

bool RegisterMyHotKey(HWND hwnd) {
    return RegisterHotKey(hwnd, HOTKEY_ID, 0, VK_F2); // Используем F2 вместо тильды
}

void UnregisterMyHotKey(HWND hwnd) {
    UnregisterHotKey(hwnd, HOTKEY_ID);
}

bool AddTrayIcon(HWND hwnd, const std::wstring& tooltip) {
    NOTIFYICONDATAW nid = {};
    nid.cbSize = sizeof(NOTIFYICONDATAW);
    nid.hWnd = hwnd;
    nid.uID = IDR_TRAYICON;
    nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
    nid.uCallbackMessage = WM_TRAYICON;

    HICON hIcon = (HICON)LoadImageW(
        GetModuleHandleW(NULL),
        MAKEINTRESOURCEW(IDR_TRAYICON),
        IMAGE_ICON,
        16, 16,
        LR_DEFAULTCOLOR
    );
    if (hIcon == NULL) {
        hIcon = LoadIconW(NULL, IDI_APPLICATION);
        MessageBoxW(NULL, L"Не удалось загрузить иконку для трея. Используется стандартная иконка.", L"Информация", MB_ICONINFORMATION);
    }
    nid.hIcon = hIcon;
    wcsncpy_s(nid.szTip, tooltip.c_str(), _TRUNCATE);

    return Shell_NotifyIconW(NIM_ADD, &nid);
}

void RemoveTrayIcon(HWND hwnd) {
    NOTIFYICONDATAW nid = {};
    nid.cbSize = sizeof(NOTIFYICONDATAW);
    nid.hWnd = hwnd;
    nid.uID = IDR_TRAYICON;

    Shell_NotifyIconW(NIM_DELETE, &nid);
}

void ShowTrayMenu(HWND hwnd) {
    POINT pt;
    GetCursorPos(&pt);

    HMENU hMenu = CreatePopupMenu();
    if (hMenu) {
        if (isRunning) {
            InsertMenuW(hMenu, -1, MF_BYPOSITION, IDM_STOP, L"Стоп");
        }
        else {
            InsertMenuW(hMenu, -1, MF_BYPOSITION, IDM_START, L"Старт");
        }
        InsertMenuW(hMenu, -1, MF_BYPOSITION, IDM_EXIT, L"Выход");

        SetForegroundWindow(hwnd);

        TrackPopupMenu(
            hMenu,
            TPM_BOTTOMALIGN | TPM_LEFTALIGN,
            pt.x, pt.y,
            0,
            hwnd,
            NULL
        );

        DestroyMenu(hMenu);
    }
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    switch (uMsg) {
    case WM_HOTKEY:
        if (wParam == HOTKEY_ID && isRunning) {
            // Отправляем Ctrl+C для копирования текста
            INPUT inputs[4] = { 0 };
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].ki.wVk = VK_CONTROL;
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].ki.wVk = 'C';
            inputs[2].type = INPUT_KEYBOARD;
            inputs[2].ki.wVk = 'C';
            inputs[2].ki.dwFlags = KEYEVENTF_KEYUP;
            inputs[3].type = INPUT_KEYBOARD;
            inputs[3].ki.wVk = VK_CONTROL;
            inputs[3].ki.dwFlags = KEYEVENTF_KEYUP;
            SendInput(4, inputs, sizeof(INPUT));
            Sleep(100);

            if (!OpenClipboard(NULL)) {
                MessageBoxW(NULL, L"Буфер обмена занят.", L"Ошибка", MB_ICONERROR);
                break;
            }

            HANDLE hData = GetClipboardData(CF_UNICODETEXT);
            if (hData) {
                wchar_t* pszText = static_cast<wchar_t*>(GlobalLock(hData));
                if (pszText) {
                    std::wstring originalText(pszText);
                    GlobalUnlock(hData);

                    std::wstring convertedText = ConvertLayout(originalText);

                    EmptyClipboard();
                    size_t size = (convertedText.length() + 1) * sizeof(wchar_t);
                    HGLOBAL hGlobal = GlobalAlloc(GMEM_MOVEABLE, size);
                    if (hGlobal) {
                        wchar_t* pGlobal = static_cast<wchar_t*>(GlobalLock(hGlobal));
                        if (pGlobal) {
                            memcpy(pGlobal, convertedText.c_str(), size);
                            GlobalUnlock(hGlobal);
                            SetClipboardData(CF_UNICODETEXT, hGlobal);
                        }
                    }
                }
            }
            CloseClipboard();

            // Вставляем текст обратно
            INPUT inputs2[4] = { 0 };
            inputs2[0].type = INPUT_KEYBOARD;
            inputs2[0].ki.wVk = VK_CONTROL;
            inputs2[1].type = INPUT_KEYBOARD;
            inputs2[1].ki.wVk = 'V';
            inputs2[2].type = INPUT_KEYBOARD;
            inputs2[2].ki.wVk = 'V';
            inputs2[2].ki.dwFlags = KEYEVENTF_KEYUP;
            inputs2[3].type = INPUT_KEYBOARD;
            inputs2[3].ki.wVk = VK_CONTROL;
            inputs2[3].ki.dwFlags = KEYEVENTF_KEYUP;
            SendInput(4, inputs2, sizeof(INPUT));
        }
        break;

    case WM_TRAYICON:
        if (lParam == WM_RBUTTONUP || lParam == WM_LBUTTONUP) {
            ShowTrayMenu(hwnd);
        }
        break;

    case WM_COMMAND:
        switch (LOWORD(wParam)) {
        case IDM_START:
            if (!isRunning) {
                if (RegisterMyHotKey(hwnd)) {
                    isRunning = true;
                }
            }
            break;
        case IDM_STOP:
            if (isRunning) {
                UnregisterMyHotKey(hwnd);
                isRunning = false;
            }
            break;
        case IDM_EXIT:
            PostMessage(hwnd, WM_CLOSE, 0, 0);
            break;
        }
        break;

    case WM_DESTROY:
        RemoveTrayIcon(hwnd);
        UnregisterMyHotKey(hwnd);
        PostQuitMessage(0);
        break;

    default:
        return DefWindowProcW(hwnd, uMsg, wParam, lParam);
    }
    return 0;
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE, PWSTR, int) {
    const wchar_t CLASS_NAME[] = L"HiddenWindowClass";

    WNDCLASSW wc = {};
    wc.lpfnWndProc = WindowProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = CLASS_NAME;

    if (!RegisterClassW(&wc)) {
        return 0;
    }

    HWND hwnd = CreateWindowExW(0, CLASS_NAME, L"Layout Converter", 0, CW_USEDEFAULT, CW_USEDEFAULT,
        CW_USEDEFAULT, CW_USEDEFAULT, NULL, NULL, hInstance, NULL);

    if (!hwnd) {
        return 0;
    }

    if (!RegisterMyHotKey(hwnd)) {
        return 0;
    }

    AddTrayIcon(hwnd, L"Layout Converter (Запущено)");

    MSG msg = {};
    while (GetMessageW(&msg, NULL, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }

    return 0;
}
