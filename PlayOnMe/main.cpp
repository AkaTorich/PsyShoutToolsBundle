#include <windows.h>
#include <tchar.h>

#include <string>
#include <vector>
#include <commdlg.h>
#include <commctrl.h>
#include <shlobj.h>
#include <propkey.h>
#include <propvarutil.h>
#include <shellapi.h>
#include <comdef.h>
#include <shlwapi.h>
#include <uxtheme.h>
#include "Resource.h"
#pragma comment(lib, "uxtheme.lib")
// Импорт Windows Media Player DLL с переименованием пространства имен
#import <wmp.dll> rename_namespace("WMP") named_guids

#pragma comment(lib, "comctl32.lib")
#pragma comment(lib, "shell32.lib")
#pragma comment(lib, "Shlwapi.lib")

using namespace WMP;

// Структура для хранения элементов плейлиста
struct PlaylistItem {
    std::wstring filePath;
    std::wstring duration;
};

std::vector<PlaylistItem> playlist;

// Глобальные переменные
HWND hStaticFile;
HWND hListView;
HWND hTrackBar;
HWND hStaticTimer;
HWND hStaticTrackName;          // Контрол для названия трека
HWND hTrackBarGrooviness;       // Trackbar громкости
HWND hStaticGroovinessValue;    // Контрол для отображения значения громкости
int currentGrooviness = 50;     // Изначальное значение громкости
IWMPPlayerPtr spWMPPlayer;      // Указатель на Windows Media Player
IWMPPlayer4Ptr spWMPPlayer4;    // Указатель для IWMPPlayer4

// Путь к файлу настроек и папке с музыкой
std::wstring g_settingsPath;
std::wstring g_musicFolder = L"C:\\Users\\Admin\\Music"; // Начальная папка по умолчанию

// ID таймера
#define IDT_TIMER 2001

// Текущий играющий индекс
int currentPlayingIndex = -1;

// Переменные для запоминания позиции и режима возобновления
double g_savedPosition = 0.0;
bool g_resumeMode = false;

// Предварительные объявления функций
bool InitializeWMP();
void PlayFileByIndex(int index);
void AddFolderToPlaylist(const std::wstring& folderPath);
void PopulateListView(HWND hListView, const std::vector<PlaylistItem>& playlist);
void OnListViewDoubleClick();
std::wstring ConvertSecondsToTime(double seconds);
std::wstring GetFileDuration(const std::wstring& filePath);
void UpdateTrackBarAndTimer();
void ApplyGrooviness(int grooviness);
void InitializeSettingsPath();
void AdjustPlaylistColumnWidth();
COLORREF g_clrBackground = RGB(30, 30, 30);        // Темно-серый фон окна
COLORREF g_clrText = RGB(220, 220, 220);           // Основной светлый текст
COLORREF g_clrControlBg = RGB(50, 50, 50);         // Серый фон для списка и кнопок
COLORREF g_clrControlText = RGB(220, 220, 220);    // Белый текст для списка и кнопок
COLORREF g_clrHighlight = RGB(50, 100, 50);        // Темно-зеленый для текущего трека
COLORREF g_clrHighlightText = RGB(255, 255, 255);  // Белый текст для выделенного трека
HBRUSH g_hbrBackground = CreateSolidBrush(g_clrBackground);  // Кисть для фона окна
HBRUSH g_hbrControlBg = CreateSolidBrush(g_clrControlBg);    // Кисть для фона списка и кнопок

// Функции сохранения/загрузки настроек
void SaveSettings()
{
    std::wstring indexStr = std::to_wstring(currentPlayingIndex);
    WritePrivateProfileString(L"Settings", L"LastTrackIndex", indexStr.c_str(), g_settingsPath.c_str());

    std::wstring volumeStr = std::to_wstring(currentGrooviness);
    WritePrivateProfileString(L"Settings", L"Volume", volumeStr.c_str(), g_settingsPath.c_str());

    WritePrivateProfileString(L"Settings", L"MusicFolder", g_musicFolder.c_str(), g_settingsPath.c_str());
}

void LoadSettings()
{
    wchar_t buffer[32];
    GetPrivateProfileString(L"Settings", L"LastTrackIndex", L"-1", buffer, 32, g_settingsPath.c_str());
    currentPlayingIndex = _wtoi(buffer);

    GetPrivateProfileString(L"Settings", L"Volume", L"50", buffer, 32, g_settingsPath.c_str());
    currentGrooviness = _wtoi(buffer);

    wchar_t folderBuffer[MAX_PATH];
    GetPrivateProfileString(L"Settings", L"MusicFolder", g_musicFolder.c_str(), folderBuffer, MAX_PATH, g_settingsPath.c_str());
    g_musicFolder = folderBuffer;
}

// Вычисление пути к файлу настроек относительно EXE
void InitializeSettingsPath()
{
    wchar_t exePath[MAX_PATH];
    GetModuleFileName(NULL, exePath, MAX_PATH);
    std::wstring exeDir = exePath;
    size_t pos = exeDir.find_last_of(L'\\');
    if (pos != std::wstring::npos)
        exeDir = exeDir.substr(0, pos + 1);
    g_settingsPath = exeDir + L"PlayOnMe_settings.ini";

    // Проверяем существование файла настроек и создаем его если нужно
    if (!PathFileExists(g_settingsPath.c_str()))
    {
        HANDLE hFile = CreateFile(g_settingsPath.c_str(), GENERIC_WRITE, 0, NULL, CREATE_NEW, FILE_ATTRIBUTE_NORMAL, NULL);
        if (hFile != INVALID_HANDLE_VALUE)
            CloseHandle(hFile);
    }
}

std::wstring ConvertSecondsToTime(double seconds) {
    int mins = static_cast<int>(seconds) / 60;
    int secs = static_cast<int>(seconds) % 60;
    wchar_t buffer[16];
    swprintf_s(buffer, 16, L"%d:%02d", mins, secs);
    return std::wstring(buffer);
}

std::wstring GetFileDuration(const std::wstring& filePath)
{
    std::wstring durationStr = L"0:00";
    if (spWMPPlayer4)
    {
        _bstr_t bstrURL(filePath.c_str());
        IWMPMediaPtr spMedia = spWMPPlayer4->newMedia(bstrURL);
        if (spMedia)
        {
            double duration = 0.0;
            HRESULT hr = spMedia->get_duration(&duration);
            if (SUCCEEDED(hr))
            {
                durationStr = ConvertSecondsToTime(duration);
            }
        }
    }
    return durationStr;
}

bool InitializeWMP()
{
    HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    if (FAILED(hr))
    {
        MessageBox(NULL, _T("Не удалось инициализировать COM."), _T("Ошибка"), MB_OK | MB_ICONERROR);
        return false;
    }
    hr = spWMPPlayer.CreateInstance(__uuidof(WMP::WindowsMediaPlayer));
    if (FAILED(hr) || !spWMPPlayer)
    {
        MessageBox(NULL, _T("Не удалось создать экземпляр Windows Media Player."), _T("Ошибка"), MB_OK | MB_ICONERROR);
        CoUninitialize();
        return false;
    }
    hr = spWMPPlayer->QueryInterface(__uuidof(WMP::IWMPPlayer4), (void**)&spWMPPlayer4);
    if (FAILED(hr) || !spWMPPlayer4)
    {
        MessageBox(NULL, _T("Не удалось получить интерфейс IWMPPlayer4."), _T("Ошибка"), MB_OK | MB_ICONERROR);
        spWMPPlayer = nullptr;
        CoUninitialize();
        return false;
    }
    spWMPPlayer->settings->put_volume(currentGrooviness);
    return true;
}

void PlayFileByIndex(int index)
{
    if (index < 0 || index >= static_cast<int>(playlist.size()))
        return;

    std::wstring filePath = playlist[index].filePath;
    if (!PathFileExistsW(filePath.c_str()))
    {
        MessageBox(NULL, _T("Файл не существует."), _T("Ошибка"), MB_OK | MB_ICONERROR);
        return;
    }

    // Если выбран новый трек или не в режиме возобновления, устанавливаем новый URL
    if (!g_resumeMode || (currentPlayingIndex != index))
    {
        _bstr_t bstrURL(filePath.c_str());
        HRESULT hr = spWMPPlayer->put_URL(bstrURL);
        if (FAILED(hr))
        {
            _com_error err(hr);
            std::wstring errorMsg = L"Не удалось установить URL.\nКод ошибки: " + std::to_wstring(hr) +
                L"\nОписание: " + err.ErrorMessage();
            MessageBox(NULL, errorMsg.c_str(), _T("Ошибка"), MB_OK | MB_ICONERROR);
            return;
        }
    }

    HRESULT hr = spWMPPlayer->controls->play();
    if (FAILED(hr))
    {
        MessageBox(NULL, _T("Не удалось начать воспроизведение."), _T("Ошибка"), MB_OK | MB_ICONERROR);
    }

    g_resumeMode = false;
    currentPlayingIndex = index;

    // Извлекаем имя файла
    size_t pos = filePath.find_last_of(L'\\');
    std::wstring fileName = (pos != std::wstring::npos) ? filePath.substr(pos + 1) : filePath;

    // Устанавливаем текст в контроле для отображения названия трека
    SetWindowText(hStaticTrackName, fileName.c_str());

    // Измеряем ширину текста и при необходимости изменяем размер контрола
    HDC hdc = GetDC(hStaticTrackName);
    SIZE textSize;
    GetTextExtentPoint32(hdc, fileName.c_str(), static_cast<int>(fileName.length()), &textSize);
    ReleaseDC(hStaticTrackName, hdc);
    RECT rc;
    GetWindowRect(hStaticTrackName, &rc);
    MapWindowPoints(NULL, GetParent(hStaticTrackName), reinterpret_cast<LPPOINT>(&rc), 2);
    MoveWindow(hStaticTrackName, rc.left, rc.top, 1000, rc.bottom - rc.top, TRUE);

    // Очищаем предыдущее выделение и устанавливаем новое
    ListView_SetItemState(hListView, -1, 0, LVIS_SELECTED | LVIS_FOCUSED); // Сбрасываем все выделения
    ListView_SetItemState(hListView, index, LVIS_SELECTED | LVIS_FOCUSED, LVIS_SELECTED | LVIS_FOCUSED); // Выделяем текущий трек

    // Прокручиваем список к текущему элементу
    ListView_EnsureVisible(hListView, index, FALSE);

    // Обновляем отображение ListView
    InvalidateRect(hListView, NULL, TRUE);
    UpdateWindow(hListView);

    // Устанавливаем фокус на ListView
    SetFocus(hListView);
}

void AddFolderToPlaylist(const std::wstring& folderPath)
{
    std::wstring searchPath = folderPath + L"\\*.*";
    WIN32_FIND_DATA findData;
    HANDLE hFind = FindFirstFile(searchPath.c_str(), &findData);
    if (hFind == INVALID_HANDLE_VALUE)
        return;
    do
    {
        std::wstring fileName = findData.cFileName;
        if (fileName == L"." || fileName == L"..")
            continue;
        std::wstring fullPath = folderPath + L"\\" + fileName;
        if (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
        {
            AddFolderToPlaylist(fullPath);
        }
        else
        {
            size_t pos = fileName.find_last_of(L'.');
            if (pos != std::wstring::npos)
            {
                std::wstring ext = fileName.substr(pos + 1);
                for (auto& c : ext)
                    c = towlower(c);
                if (ext == L"mp3" || ext == L"flac" || ext == L"wav" ||
                    ext == L"aiff" || ext == L"ogg")
                {
                    PlaylistItem item;
                    item.filePath = fullPath;
                    item.duration = GetFileDuration(fullPath);
                    playlist.push_back(item);
                }
            }
        }
    } while (FindNextFile(hFind, &findData) != 0);
    FindClose(hFind);
}

void PopulateListView(HWND hListView, const std::vector<PlaylistItem>& playlist)
{
    ListView_DeleteAllItems(hListView);
    static bool columnsAdded = false;
    if (!columnsAdded)
    {
        LVCOLUMN lvCol;
        ZeroMemory(&lvCol, sizeof(lvCol));
        lvCol.mask = LVCF_TEXT | LVCF_WIDTH | LVCF_SUBITEM;
        lvCol.pszText = const_cast<LPWSTR>(L"Duration");
        lvCol.cx = 70; // фиксированная ширина
        lvCol.iSubItem = 0;
        ListView_InsertColumn(hListView, 0, &lvCol);
        lvCol.pszText = const_cast<LPWSTR>(L"Playlist");
        lvCol.cx = 280; // изначальная ширина – будет изменяться
        lvCol.iSubItem = 1;
        ListView_InsertColumn(hListView, 1, &lvCol);
        columnsAdded = true;
    }
    LVITEM lvItem;
    ZeroMemory(&lvItem, sizeof(lvItem));
    lvItem.mask = LVIF_TEXT;
    for (size_t i = 0; i < playlist.size(); ++i)
    {
        std::wstring fileName;
        size_t pos = playlist[i].filePath.find_last_of(L'\\');
        fileName = (pos != std::wstring::npos) ? playlist[i].filePath.substr(pos + 1) : playlist[i].filePath;
        lvItem.iItem = static_cast<int>(i);
        lvItem.iSubItem = 0;
        lvItem.pszText = const_cast<LPWSTR>(playlist[i].duration.c_str());
        int itemIndex = ListView_InsertItem(hListView, &lvItem);
        if (itemIndex == -1)
            continue;
        ListView_SetItemText(hListView, itemIndex, 1, const_cast<LPWSTR>(fileName.c_str()));
    }
}

// Функция, которая устанавливает ширину второго столбца так,
// чтобы он занимал все оставшееся пространство после первого (ширина 70)
void AdjustPlaylistColumnWidth()
{
    RECT rc;
    if (GetClientRect(hListView, &rc))
    {
        int totalWidth = rc.right - rc.left;
        int freeWidth = totalWidth - 70;
        if (freeWidth < 0)
            freeWidth = 0;
        ListView_SetColumnWidth(hListView, 1, freeWidth);
    }
}

void OnListViewDoubleClick()
{
    int iItem = ListView_GetNextItem(hListView, -1, LVNI_SELECTED);
    if (iItem != -1)
        PlayFileByIndex(iItem);
}

void UpdateTrackBarAndTimer()
{
    if (!spWMPPlayer)
        return;
    long state = spWMPPlayer->playState;
    if (state == wmppsMediaEnded)
    {
        if (currentPlayingIndex < static_cast<int>(playlist.size()) - 1)
            PlayFileByIndex(currentPlayingIndex + 1);
        else
        {
            spWMPPlayer->controls->stop();
            currentPlayingIndex = -1;
            InvalidateRect(hListView, NULL, TRUE);
            UpdateWindow(hListView);
            SetWindowText(hStaticTrackName, L"");
            MessageBox(NULL, _T("Плейлист завершен."), _T("Информация"), MB_OK | MB_ICONINFORMATION);
        }
        return;
    }
    IWMPMediaPtr spMedia = spWMPPlayer->currentMedia;
    if (!spMedia)
        return;
    double currentPosition = 0.0, duration = 0.0;
    if (FAILED(spWMPPlayer->controls->get_currentPosition(&currentPosition)))
        return;
    if (FAILED(spMedia->get_duration(&duration)))
        return;
    if (duration > 0)
    {
        double progressPercent = (currentPosition / duration) * 100.0;
        if (progressPercent > 100.0)
            progressPercent = 100.0;
        else if (progressPercent < 0.0)
            progressPercent = 0.0;
        SendMessage(hTrackBar, TBM_SETPOS, TRUE, static_cast<int>(progressPercent));
        double remaining = duration - currentPosition;
        std::wstring timerText = ConvertSecondsToTime(remaining);
        SetWindowText(hStaticTimer, timerText.c_str());
        if (progressPercent >= 99.0)
        {
            if (currentPlayingIndex < static_cast<int>(playlist.size()) - 1)
                PlayFileByIndex(currentPlayingIndex + 1);
            else
            {
                spWMPPlayer->controls->stop();
                currentPlayingIndex = -1;
                InvalidateRect(hListView, NULL, TRUE);
                UpdateWindow(hListView);
                SetWindowText(hStaticTrackName, L"");
                MessageBox(NULL, _T("Плейлист завершен."), _T("Информация"), MB_OK | MB_ICONINFORMATION);
            }
        }
    }
}

void ApplyGrooviness(int grooviness)
{
    if (spWMPPlayer)
        spWMPPlayer->settings->put_volume(grooviness);
}

// Обработчик диалога настроек
INT_PTR CALLBACK SettingsDialogProc(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    switch (uMsg)
    {
    case WM_INITDIALOG:
    {
        HWND hEditFolder = GetDlgItem(hwndDlg, IDC_EDIT_FOLDER);
        SetWindowText(hEditFolder, g_musicFolder.c_str());
        return TRUE;
    }
    case WM_COMMAND:
        switch (LOWORD(wParam))
        {
        case IDC_BUTTON_BROWSE:
        {
            BROWSEINFO bi;
            ZeroMemory(&bi, sizeof(bi));
            bi.lpszTitle = _T("Выберите папку с аудиофайлами");
            bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;
            LPITEMIDLIST pidl = SHBrowseForFolder(&bi);
            if (pidl != NULL)
            {
                wchar_t path[MAX_PATH];
                if (SHGetPathFromIDList(pidl, path))
                {
                    SetWindowText(GetDlgItem(hwndDlg, IDC_EDIT_FOLDER), path);
                }
                CoTaskMemFree(pidl);
            }
            return TRUE;
        }
        case IDOK:
        {
            wchar_t buffer[MAX_PATH];
            GetWindowText(GetDlgItem(hwndDlg, IDC_EDIT_FOLDER), buffer, MAX_PATH);
            if (PathFileExists(buffer))
            {
                g_musicFolder = buffer;
                SaveSettings();

                // Перезагружаем плейлист
                playlist.clear();
                AddFolderToPlaylist(g_musicFolder);
                PopulateListView(hListView, playlist);
                SetWindowText(hStaticFile, g_musicFolder.c_str());
                SetWindowText(hStaticTrackName, L"");
                AdjustPlaylistColumnWidth();

                EndDialog(hwndDlg, IDOK);
            }
            else
            {
                MessageBox(hwndDlg, L"Указанная папка не существует.", L"Ошибка", MB_OK | MB_ICONERROR);
            }
            return TRUE;
        }
        case IDCANCEL:
            EndDialog(hwndDlg, IDCANCEL);
            return TRUE;
        }
    }
    return FALSE;
}

INT_PTR CALLBACK DialogProc(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    switch (uMsg)
    {
    case WM_INITDIALOG:
    {
        // Добавляем кнопку минимизации и системное меню
        LONG style = GetWindowLong(hwndDlg, GWL_STYLE);
        style |= WS_MINIMIZEBOX | WS_SYSMENU;
        SetWindowLong(hwndDlg, GWL_STYLE, style);
        SetWindowPos(hwndDlg, NULL, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

        InitializeSettingsPath();
        LoadSettings();
        INITCOMMONCONTROLSEX icex;
        icex.dwSize = sizeof(INITCOMMONCONTROLSEX);
        icex.dwICC = ICC_LISTVIEW_CLASSES | ICC_BAR_CLASSES;
        InitCommonControlsEx(&icex);
        if (!InitializeWMP())
        {
            EndDialog(hwndDlg, 0);
            return TRUE;
        }
        hStaticFile = GetDlgItem(hwndDlg, IDC_STATIC_FILE);
        hListView = GetDlgItem(hwndDlg, IDC_LISTVIEW_PLAYLIST);
        hTrackBar = GetDlgItem(hwndDlg, IDC_TRACKBAR_PROGRESS);
        hStaticTimer = GetDlgItem(hwndDlg, IDC_STATIC_TIMER);
        hStaticTrackName = GetDlgItem(hwndDlg, IDC_STATIC_TRACK_NAME);
        hTrackBarGrooviness = GetDlgItem(hwndDlg, IDC_TRACKBAR_GROOVINESS);
        hStaticGroovinessValue = GetDlgItem(hwndDlg, IDC_STATIC_GROOVINESS_VALUE);

        // Установка темного фона для диалога
        SetClassLongPtr(hwndDlg, GCLP_HBRBACKGROUND, (LONG_PTR)g_hbrBackground);

        // Отключаем визуальные стили для ListView
        SetWindowTheme(hListView, L"", L"");

        // Устанавливаем серый фон для ListView напрямую
        SendMessage(hListView, LVM_SETBKCOLOR, 0, (LPARAM)g_clrControlBg);
        SendMessage(hListView, LVM_SETTEXTBKCOLOR, 0, (LPARAM)g_clrControlBg);
        SendMessage(hListView, LVM_SETTEXTCOLOR, 0, (LPARAM)g_clrControlText);

        // Первоначальное изменение размера контрола для названия трека
        {
            RECT rc;
            GetWindowRect(hStaticTrackName, &rc);
            MapWindowPoints(NULL, hwndDlg, reinterpret_cast<LPPOINT>(&rc), 2);
            MoveWindow(hStaticTrackName, rc.left, rc.top, 1000, rc.bottom - rc.top, TRUE);
        }

        // Устанавливаем стиль без сетки
        ListView_SetExtendedListViewStyle(hListView, LVS_EX_FULLROWSELECT);
        ListView_SetExtendedListViewStyleEx(hListView, LVS_EX_CHECKBOXES, 0);

        // Настройка TrackBar для громкости
        SendMessage(hTrackBarGrooviness, TBM_SETRANGE, TRUE, MAKELPARAM(0, 100));
        SendMessage(hTrackBarGrooviness, TBM_SETTICFREQ, 10, 0);
        SendMessage(hTrackBarGrooviness, TBM_SETPAGESIZE, 10, 0);
        SendMessage(hTrackBarGrooviness, TBM_SETPOS, TRUE, currentGrooviness);
        wchar_t groovinessStr[4];
        swprintf_s(groovinessStr, 4, L"%d", currentGrooviness);
        SetWindowText(hStaticGroovinessValue, groovinessStr);

        // Настройка TrackBar для прогресса воспроизведения
        SendMessage(hTrackBar, TBM_SETRANGE, TRUE, MAKELPARAM(0, 100));
        SendMessage(hTrackBar, TBM_SETTICFREQ, 10, 0);
        SendMessage(hTrackBar, TBM_SETPAGESIZE, 10, 0);
        SendMessage(hTrackBar, TBM_SETPOS, TRUE, 0);

        SetWindowText(hStaticTimer, L"0:00");
        SetWindowText(hStaticTrackName, L"");

        std::wstring initialFolder = g_musicFolder;
        AddFolderToPlaylist(initialFolder);
        PopulateListView(hListView, playlist);
        AdjustPlaylistColumnWidth();

        if (currentPlayingIndex >= 0 && currentPlayingIndex < static_cast<int>(playlist.size()))
        {
            LVITEM lvItem;
            ZeroMemory(&lvItem, sizeof(lvItem));
            lvItem.mask = LVIF_STATE;
            lvItem.iItem = currentPlayingIndex;
            lvItem.iSubItem = 0;
            lvItem.state = LVIS_SELECTED | LVIS_FOCUSED;
            lvItem.stateMask = LVIS_SELECTED | LVIS_FOCUSED;
            ListView_SetItem(hListView, &lvItem);
            size_t pos = playlist[currentPlayingIndex].filePath.find_last_of(L'\\');
            std::wstring fileName = (pos != std::wstring::npos) ? playlist[currentPlayingIndex].filePath.substr(pos + 1) : playlist[currentPlayingIndex].filePath;
            SetWindowText(hStaticTrackName, fileName.c_str());

            HDC hdc = GetDC(hStaticTrackName);
            SIZE textSize;
            GetTextExtentPoint32(hdc, fileName.c_str(), static_cast<int>(fileName.length()), &textSize);
            ReleaseDC(hStaticTrackName, hdc);
            RECT rc;
            GetWindowRect(hStaticTrackName, &rc);
            MapWindowPoints(NULL, hwndDlg, reinterpret_cast<LPPOINT>(&rc), 2);
            MoveWindow(hStaticTrackName, rc.left, rc.top, 1000, rc.bottom - rc.top, TRUE);
        }
        else
        {
            currentPlayingIndex = -1;
        }
        spWMPPlayer->settings->put_volume(currentGrooviness);
        SetTimer(hwndDlg, IDT_TIMER, 1000, NULL);

        // Принудительное обновление ListView
        InvalidateRect(hListView, NULL, TRUE);
        UpdateWindow(hListView);
    }
    return TRUE;

    case WM_SYSCOMMAND:
        if ((wParam & 0xFFF0) == SC_MINIMIZE)
        {
            ShowWindow(hwndDlg, SW_MINIMIZE);
            return TRUE;
        }
        break;

    case WM_SIZE:
    {
        AdjustPlaylistColumnWidth();
    }
    break;

    case WM_TIMER:
        if (wParam == IDT_TIMER)
            UpdateTrackBarAndTimer();
        break;

    case WM_COMMAND:
        switch (LOWORD(wParam))
        {
        case IDC_BUTTON_OPEN:
        {
            BROWSEINFO bi;
            ZeroMemory(&bi, sizeof(bi));
            bi.lpszTitle = _T("Выберите папку с аудиофайлами");
            bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;
            LPITEMIDLIST pidl = SHBrowseForFolder(&bi);
            if (pidl != NULL)
            {
                wchar_t path[MAX_PATH];
                if (SHGetPathFromIDList(pidl, path))
                {
                    std::wstring selectedFolder = path;
                    g_musicFolder = selectedFolder; // Обновляем глобальную переменную
                    SaveSettings(); // Сохраняем настройки
                    playlist.clear();
                    AddFolderToPlaylist(selectedFolder);
                    PopulateListView(hListView, playlist);
                    SetWindowText(hStaticFile, selectedFolder.c_str());
                    SetWindowText(hStaticTrackName, L"");
                    AdjustPlaylistColumnWidth();
                }
                CoTaskMemFree(pidl);
            }
        }
        break;

        case IDC_BUTTON_PLAY:
            if (!playlist.empty())
            {
                if (g_resumeMode)
                {
                    spWMPPlayer->controls->play();
                    g_resumeMode = false;
                }
                else if (currentPlayingIndex == -1)
                    PlayFileByIndex(0);
                else
                    PlayFileByIndex(currentPlayingIndex);
            }
            else
            {
                MessageBox(NULL, _T("Плейлист пуст."), _T("Информация"), MB_OK | MB_ICONINFORMATION);
            }
            break;

        case IDC_BUTTON_PAUSE:
            if (spWMPPlayer)
            {
                spWMPPlayer->controls->get_currentPosition(&g_savedPosition);
                spWMPPlayer->controls->pause();
                g_resumeMode = true;
            }
            break;

        case IDC_BUTTON_STOP:
            if (spWMPPlayer)
            {
                spWMPPlayer->controls->get_currentPosition(&g_savedPosition);
                spWMPPlayer->controls->pause();
                g_resumeMode = true;
            }
            break;

        case IDC_BUTTON_PREV:
            if (currentPlayingIndex > 0)
                PlayFileByIndex(currentPlayingIndex - 1);
            else
                MessageBox(NULL, _T("Это первый трек в плейлисте."), _T("Информация"), MB_OK | MB_ICONINFORMATION);
            break;

        case IDC_BUTTON_NEXT:
            if (currentPlayingIndex < static_cast<int>(playlist.size()) - 1)
                PlayFileByIndex(currentPlayingIndex + 1);
            else
                MessageBox(NULL, _T("Это последний трек в плейлисте."), _T("Информация"), MB_OK | MB_ICONINFORMATION);
            break;

        case IDC_BUTTON_SETTINGS:
        {
            DialogBox(GetModuleHandle(NULL), MAKEINTRESOURCE(IDD_DIALOG_SETTINGS), hwndDlg, SettingsDialogProc);
            break;
        }

        case IDCANCEL:
            EndDialog(hwndDlg, 0);
            return TRUE;
        }
        return TRUE;

    case WM_CTLCOLORSTATIC:
    {
        HDC hdcStatic = (HDC)wParam;
        SetTextColor(hdcStatic, g_clrText);
        SetBkColor(hdcStatic, g_clrBackground);
        return (INT_PTR)g_hbrBackground;
    }

    case WM_CTLCOLORDLG:
    {
        HDC hdcDlg = (HDC)wParam;
        SetBkColor(hdcDlg, g_clrBackground);
        return (INT_PTR)g_hbrBackground;
    }

    case WM_CTLCOLORBTN:
    {
        HDC hdcBtn = (HDC)wParam;
        SetTextColor(hdcBtn, g_clrControlText);      // Белый текст
        SetBkColor(hdcBtn, g_clrControlBg);          // Серый фон
        return (INT_PTR)g_hbrControlBg;              // Используем кисть с серым фоном
    }

    case WM_DRAWITEM:
    {
        LPDRAWITEMSTRUCT pdis = (LPDRAWITEMSTRUCT)lParam;
        if (pdis->CtlType == ODT_BUTTON)
        {
            HBRUSH hBrush = CreateSolidBrush(g_clrControlBg); // Серый фон кнопки
            FillRect(pdis->hDC, &pdis->rcItem, hBrush);       // Заполняем фон
            SetTextColor(pdis->hDC, g_clrControlText);        // Белый текст
            SetBkMode(pdis->hDC, TRANSPARENT);                // Прозрачный фон текста

            // Получаем текст кнопки
            wchar_t buttonText[256];
            GetWindowText(pdis->hwndItem, buttonText, 256);

            // Рисуем текст по центру
            RECT rc = pdis->rcItem;
            DrawText(pdis->hDC, buttonText, -1, &rc, DT_CENTER | DT_VCENTER | DT_SINGLELINE);

            // Рисуем рамку, если кнопка нажата
            if (pdis->itemState & ODS_SELECTED)
            {
                FrameRect(pdis->hDC, &pdis->rcItem, (HBRUSH)GetStockObject(BLACK_BRUSH));
            }

            // Очищаем временную кисть
            DeleteObject(hBrush);
            return TRUE;
        }
        break;
    }

    case WM_NOTIFY:
    {
        LPNMHDR pnmh = (LPNMHDR)lParam;
        if (pnmh->idFrom == IDC_LISTVIEW_PLAYLIST)
        {
            if (pnmh->code == NM_CLICK)
            {
                LPNMITEMACTIVATE pnmItem = reinterpret_cast<LPNMITEMACTIVATE>(lParam);
                if (pnmItem->iItem != -1)
                {
                    ListView_SetItemState(hListView, -1, 0, LVIS_SELECTED);
                    ListView_SetItemState(hListView, pnmItem->iItem,
                        LVIS_SELECTED | LVIS_FOCUSED,
                        LVIS_SELECTED | LVIS_FOCUSED);
                }
                return 0;
            }
            else if (pnmh->code == NM_DBLCLK)
            {
                OnListViewDoubleClick();
            }
            else if (pnmh->code == NM_CUSTOMDRAW)
            {
                LPNMLVCUSTOMDRAW lplvcd = (LPNMLVCUSTOMDRAW)lParam;
                switch (lplvcd->nmcd.dwDrawStage)
                {
                case CDDS_PREPAINT:
                    return CDRF_NOTIFYITEMDRAW | CDRF_NOTIFYSUBITEMDRAW;

                case CDDS_ITEMPREPAINT:
                    lplvcd->clrText = g_clrControlText;      // Белый текст
                    lplvcd->clrTextBk = g_clrControlBg;      // Серый фон
                    if (static_cast<int>(lplvcd->nmcd.dwItemSpec) == currentPlayingIndex)
                    {
                        lplvcd->clrTextBk = g_clrHighlight;      // Темно-зеленый фон для текущего трека
                        lplvcd->clrText = g_clrHighlightText;    // Белый текст для текущего трека
                    }
                    return CDRF_NEWFONT | CDRF_NOTIFYSUBITEMDRAW;

                case CDDS_ITEMPREPAINT | CDDS_SUBITEM:
                    lplvcd->clrText = g_clrControlText;      // Белый текст
                    lplvcd->clrTextBk = g_clrControlBg;      // Серый фон
                    if (static_cast<int>(lplvcd->nmcd.dwItemSpec) == currentPlayingIndex)
                    {
                        lplvcd->clrTextBk = g_clrHighlight;      // Темно-зеленый фон для текущего трека
                        lplvcd->clrText = g_clrHighlightText;    // Белый текст для текущего трека
                    }
                    return CDRF_NEWFONT;
                }
                return CDRF_DODEFAULT;
            }
        }
    }
    return TRUE;

    case WM_HSCROLL:
        if ((HWND)lParam == hTrackBar)
        {
            int pos = SendMessage(hTrackBar, TBM_GETPOS, 0, 0);
            if (spWMPPlayer4 && currentPlayingIndex != -1)
            {
                std::wstring currentFile = playlist[currentPlayingIndex].filePath;
                _bstr_t bstrURL(currentFile.c_str());
                IWMPMediaPtr spMedia = spWMPPlayer4->newMedia(bstrURL);
                if (spMedia)
                {
                    double duration = 0.0;
                    HRESULT hr = spMedia->get_duration(&duration);
                    if (SUCCEEDED(hr) && duration > 0)
                    {
                        double newPosition = (static_cast<double>(pos) / 100.0) * duration;
                        spWMPPlayer->controls->put_currentPosition(newPosition);
                    }
                }
            }
        }
        else if ((HWND)lParam == hTrackBarGrooviness)
        {
            int groovinessPos = SendMessage(hTrackBarGrooviness, TBM_GETPOS, 0, 0);
            currentGrooviness = groovinessPos;
            ApplyGrooviness(currentGrooviness);
            wchar_t groovinessStr[4];
            swprintf_s(groovinessStr, 4, L"%d", currentGrooviness);
            SetWindowText(hStaticGroovinessValue, groovinessStr);
        }
        break;

    case WM_CLOSE:
        EndDialog(hwndDlg, 0);
        return TRUE;

    case WM_DESTROY:
        KillTimer(hwndDlg, IDT_TIMER);
        SaveSettings();
        if (spWMPPlayer)
        {
            spWMPPlayer->controls->stop();
            spWMPPlayer = nullptr;
        }
        if (spWMPPlayer4)
        {
            spWMPPlayer4 = nullptr;
        }
        DeleteObject(g_hbrBackground);
        DeleteObject(g_hbrControlBg);
        CoUninitialize();
        return TRUE;
    }
    return FALSE;
}

int APIENTRY _tWinMain(_In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPTSTR    lpCmdLine,
    _In_ int       nCmdShow)
{
    HICON hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_MAIN_ICON));
    if (hIcon)
    {
        HWND hWnd = GetConsoleWindow();
        if (hWnd)
        {
            SendMessage(hWnd, WM_SETICON, ICON_BIG, (LPARAM)hIcon);
            SendMessage(hWnd, WM_SETICON, ICON_SMALL, (LPARAM)hIcon);
        }
    }
    DialogBox(hInstance, MAKEINTRESOURCE(IDD_DIALOG_MAIN), NULL, DialogProc);
    return 0;
}