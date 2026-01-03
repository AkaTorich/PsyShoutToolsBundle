#include <windows.h>
#include <chrono>
#include <thread>
#include <string>
#include <sstream>

#include "Libraries/bass24/c/bass.h"
#include "Libraries/resource.h"

constexpr int WORK_INTERVAL = 25 * 60;
constexpr int BREAK_INTERVAL = 5 * 60;
constexpr UINT_PTR TIMER_ID = 1001;

#define TRAY_ICON_ID 5000
#define WM_TRAYICON (WM_USER + 1)
#define ID_TRAY_OPEN 100
#define ID_TRAY_TOGGLE 101 // Изменен ID_TRAY_EXIT на ID_TRAY_TOGGLE для переключения
#define ID_TRAY_EXIT 102   // Новый ID для выхода

bool isRunning = false;
int timeRemaining = WORK_INTERVAL;
bool isWorkInterval = true;

HWND hwndTimer;
HWND hToggleButton;
NOTIFYICONDATAW nid = { 0 };
HINSTANCE g_hInstance = NULL;
HHOOK hHook = NULL;

COLORREF BACKGROUND_COLOR = RGB(30, 30, 30);
COLORREF BUTTON_COLOR = RGB(70, 70, 70);
COLORREF TEXT_COLOR = RGB(255, 255, 255);

HMODULE hBass = NULL;

typedef BOOL(WINAPI* BASS_Init_t)(int device, DWORD freq, DWORD flags, HWND win, GUID* clsid);
typedef HSTREAM(WINAPI* BASS_StreamCreateFile_t)(BOOL mem, const char* file, QWORD offset, QWORD length, DWORD flags);
typedef BOOL(WINAPI* BASS_ChannelPlay_t)(HSTREAM handle, BOOL restart);
typedef DWORD(WINAPI* BASS_ChannelIsActive_t)(HSTREAM handle);
typedef BOOL(WINAPI* BASS_StreamFree_t)(HSTREAM handle);
typedef BOOL(WINAPI* BASS_Free_t)(void);

BASS_Init_t BASS_Init_ptr = nullptr;
BASS_StreamCreateFile_t BASS_StreamCreateFile_ptr = nullptr;
BASS_ChannelPlay_t BASS_ChannelPlay_ptr = nullptr;
BASS_ChannelIsActive_t BASS_ChannelIsActive_ptr = nullptr;
BASS_StreamFree_t BASS_StreamFree_ptr = nullptr;
BASS_Free_t BASS_Free_ptr = nullptr;

bool LoadBASSFunctions() {
	wchar_t exePath[MAX_PATH];
	GetModuleFileNameW(NULL, exePath, MAX_PATH);
	std::wstring pathStr(exePath);
	size_t pos = pathStr.find_last_of(L"\\/");
	std::wstring exeDir = (pos != std::wstring::npos) ? pathStr.substr(0, pos) : L".";
	std::wstring bassDllPath = exeDir + L"\\bass.dll";
	hBass = LoadLibraryW(bassDllPath.c_str());
	BASS_Init_ptr = (BASS_Init_t)GetProcAddress(hBass, "BASS_Init");
	BASS_StreamCreateFile_ptr = (BASS_StreamCreateFile_t)GetProcAddress(hBass, "BASS_StreamCreateFile");
	BASS_ChannelPlay_ptr = (BASS_ChannelPlay_t)GetProcAddress(hBass, "BASS_ChannelPlay");
	BASS_ChannelIsActive_ptr = (BASS_ChannelIsActive_t)GetProcAddress(hBass, "BASS_ChannelIsActive");
	BASS_StreamFree_ptr = (BASS_StreamFree_t)GetProcAddress(hBass, "BASS_StreamFree");
	BASS_Free_ptr = (BASS_Free_t)GetProcAddress(hBass, "BASS_Free");
	return (BASS_Init_ptr && BASS_StreamCreateFile_ptr && BASS_ChannelPlay_ptr &&
		BASS_ChannelIsActive_ptr && BASS_StreamFree_ptr && BASS_Free_ptr);
}

void ShowTrayMenu(HWND hwnd) {
	HMENU hMenu = CreatePopupMenu();
	if (hMenu) {
		AppendMenuW(hMenu, MF_STRING, ID_TRAY_OPEN, L"Открыть");
		AppendMenuW(hMenu, MF_STRING, ID_TRAY_TOGGLE, isRunning ? L"Stop" : L"Start"); // Динамический текст
		AppendMenuW(hMenu, MF_SEPARATOR, 0, NULL);
		AppendMenuW(hMenu, MF_STRING, ID_TRAY_EXIT, L"Выход");

		POINT pt;
		GetCursorPos(&pt);
		SetForegroundWindow(hwnd);
		TrackPopupMenu(hMenu, TPM_BOTTOMALIGN | TPM_LEFTALIGN, pt.x, pt.y, 0, hwnd, NULL);
		DestroyMenu(hMenu);
	}
}

void PlaySoundFile(const char* file) {
	if (!BASS_Init_ptr(-1, 44100, 0, 0, NULL)) return;
	HSTREAM stream = BASS_StreamCreateFile_ptr(FALSE, file, 0, 0, 0);
	if (!stream) {
		BASS_Free_ptr();
		return;
	}
	BASS_ChannelPlay_ptr(stream, FALSE);
	while (BASS_ChannelIsActive_ptr(stream) == BASS_ACTIVE_PLAYING) {
		std::this_thread::sleep_for(std::chrono::milliseconds(100));
	}
	BASS_StreamFree_ptr(stream);
	BASS_Free_ptr();
}

void UpdateTimerDisplay(HWND hwnd) {
	std::wostringstream oss;
	int minutes = timeRemaining / 60;
	int seconds = timeRemaining % 60;
	oss << (isWorkInterval ? L"Работа: " : L"Перерыв: ")
		<< minutes << L":" << (seconds < 10 ? L"0" : L"") << seconds;
	SetWindowTextW(hwnd, oss.str().c_str());
}

LRESULT CALLBACK KeyboardProc(int nCode, WPARAM wParam, LPARAM lParam) {
	if (nCode == HC_ACTION) return 1;
	return CallNextHookEx(hHook, nCode, wParam, lParam);
}

void InstallKeyboardHook() {
	if (!hHook) hHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardProc, g_hInstance, 0);
}

void UninstallKeyboardHook() {
	if (hHook) {
		UnhookWindowsHookEx(hHook);
		hHook = NULL;
	}
}

void CALLBACK TimerProc(HWND hwnd, UINT uMsg, UINT_PTR idEvent, DWORD dwTime) {
	if (idEvent != TIMER_ID) return;

	if (timeRemaining > 0) {
		timeRemaining--;
		UpdateTimerDisplay(hwndTimer);
	}
	else {
		KillTimer(hwnd, TIMER_ID);
		if (isWorkInterval) {
			std::thread soundThread(PlaySoundFile, "break.wav");
			soundThread.detach();
			MessageBoxW(hwnd, L"Рабочий интервал завершен! Время сделать перерыв.", L"Pomodoro", MB_OK | MB_ICONINFORMATION | MB_SYSTEMMODAL);
			timeRemaining = BREAK_INTERVAL;
		}
		else {
			std::thread soundThread(PlaySoundFile, "work.wav");
			soundThread.detach();
			MessageBoxW(hwnd, L"Перерыв завершен! Время вернуться к работе.", L"Pomodoro", MB_OK | MB_ICONINFORMATION | MB_SYSTEMMODAL);
			timeRemaining = WORK_INTERVAL;
		}
		isWorkInterval = !isWorkInterval;
		UpdateTimerDisplay(hwndTimer);
		SetTimer(hwnd, TIMER_ID, 1000, (TIMERPROC)TimerProc);

		if (!isWorkInterval) InstallKeyboardHook();
		else UninstallKeyboardHook();
	}
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
	static HBRUSH hBackgroundBrush = CreateSolidBrush(BACKGROUND_COLOR);
	static HBRUSH hButtonBrush = CreateSolidBrush(BUTTON_COLOR);
	static HFONT hFont = CreateFontW(16, 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE, DEFAULT_CHARSET,
		OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY,
		DEFAULT_PITCH | FF_SWISS, L"Segoe UI");

	switch (uMsg) {
	case WM_CREATE: {
		hwndTimer = CreateWindowW(
			L"STATIC",
			L"Работа: 25:00",
			WS_VISIBLE | WS_CHILD | SS_CENTER,
			10, 50, 180, 25,
			hwnd,
			NULL,
			g_hInstance,
			NULL
		);
		SendMessage(hwndTimer, WM_SETFONT, (WPARAM)hFont, TRUE);

		hToggleButton = CreateWindowW(
			L"BUTTON",
			L"Start",
			WS_VISIBLE | WS_CHILD | BS_OWNERDRAW,
			60, 10, 80, 25,
			hwnd,
			(HMENU)1,
			g_hInstance,
			NULL
		);
		SendMessage(hToggleButton, WM_SETFONT, (WPARAM)hFont, TRUE);

		HICON hIcon = LoadIcon(g_hInstance, MAKEINTRESOURCE(IDI_WINDOW_ICON));
		if (hIcon) {
			SendMessage(hwnd, WM_SETICON, ICON_BIG, (LPARAM)hIcon);
			SendMessage(hwnd, WM_SETICON, ICON_SMALL, (LPARAM)hIcon);
		}

		nid.cbSize = sizeof(NOTIFYICONDATAW);
		nid.hWnd = hwnd;
		nid.uID = TRAY_ICON_ID;
		nid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
		nid.uCallbackMessage = WM_TRAYICON;
		nid.hIcon = LoadIcon(g_hInstance, MAKEINTRESOURCE(IDI_TRAY_ICON));
		wcscpy_s(nid.szTip, L"Pomodoro Timer");
		Shell_NotifyIconW(NIM_ADD, &nid);

		HMENU hSysMenu = GetSystemMenu(hwnd, FALSE);
		if (hSysMenu) DeleteMenu(hSysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
		return 0;
	}

	case WM_CTLCOLORSTATIC: {
		HDC hdc = (HDC)wParam;
		SetTextColor(hdc, TEXT_COLOR);
		SetBkMode(hdc, TRANSPARENT);
		return (LRESULT)hBackgroundBrush;
	}

	case WM_DRAWITEM: {
		LPDRAWITEMSTRUCT pDIS = (LPDRAWITEMSTRUCT)lParam;
		if (pDIS->CtlType == ODT_BUTTON) {
			HDC hdc = pDIS->hDC;
			FillRect(hdc, &pDIS->rcItem, hButtonBrush);
			SetTextColor(hdc, TEXT_COLOR);
			SetBkMode(hdc, TRANSPARENT);
			wchar_t buttonText[10];
			GetWindowTextW(pDIS->hwndItem, buttonText, 10);
			DrawTextW(hdc, buttonText, -1, &pDIS->rcItem, DT_CENTER | DT_VCENTER | DT_SINGLELINE);
			if (pDIS->itemState & ODS_SELECTED) {
				DrawEdge(hdc, &pDIS->rcItem, EDGE_SUNKEN, BF_RECT);
			}
			return TRUE;
		}
		return FALSE;
	}

	case WM_COMMAND: {
		switch (LOWORD(wParam)) {
		case 1:        // Кнопка
		case ID_TRAY_TOGGLE: { // Пункт меню в трее
			if (!isRunning) {
				// Start
				isRunning = true;
				timeRemaining = WORK_INTERVAL;
				isWorkInterval = true;
				UpdateTimerDisplay(hwndTimer);
				SetTimer(hwnd, TIMER_ID, 1000, (TIMERPROC)TimerProc);
				SetWindowTextW(hToggleButton, L"Stop");
			}
			else {
				// Stop
				isRunning = false;
				KillTimer(hwnd, TIMER_ID);
				UpdateTimerDisplay(hwndTimer);
				UninstallKeyboardHook();
				SetWindowTextW(hToggleButton, L"Start");
			}
			break;
		}
		case ID_TRAY_OPEN: {
			ShowWindow(hwnd, SW_SHOW);
			SetForegroundWindow(hwnd);
			break;
		}
		case ID_TRAY_EXIT: {
			Shell_NotifyIconW(NIM_DELETE, &nid);
			PostMessage(hwnd, WM_CLOSE, 0, 0);
			break;
		}
		}
		return 0;
	}

	case WM_TRAYICON: {
		if (lParam == WM_RBUTTONUP || lParam == WM_LBUTTONDBLCLK) {
			ShowTrayMenu(hwnd);
		}
		return 0;
	}

	case WM_SYSCOMMAND: {
		switch (wParam & 0xFFF0) {
		case SC_MAXIMIZE:
		case SC_SIZE:
			return 0;
		case SC_MINIMIZE:
			ShowWindow(hwnd, SW_HIDE);
			return 0;
		}
		break;
	}

	case WM_GETMINMAXINFO: {
		MINMAXINFO* mmi = (MINMAXINFO*)lParam;
		mmi->ptMinTrackSize.x = 220;
		mmi->ptMinTrackSize.y = 150;
		mmi->ptMaxTrackSize.x = 220;
		mmi->ptMaxTrackSize.y = 150;
		return 0;
	}

	case WM_PAINT: {
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hwnd, &ps);
		FillRect(hdc, &ps.rcPaint, hBackgroundBrush);
		EndPaint(hwnd, &ps);
		break;
	}

	case WM_DESTROY: {
		DeleteObject(hBackgroundBrush);
		DeleteObject(hButtonBrush);
		DeleteObject(hFont);
		Shell_NotifyIconW(NIM_DELETE, &nid);
		UninstallKeyboardHook();
		PostQuitMessage(0);
		break;
	}
	}
	return DefWindowProcW(hwnd, uMsg, wParam, lParam);
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR pCmdLine, int nCmdShow) {



	if (!LoadBASSFunctions()) {
		MessageBoxW(NULL, L"Не удалось загрузить bass.dll", L"Ошибка", MB_OK | MB_ICONERROR);
		if (hBass) FreeLibrary(hBass);
		return 0;
	}

	g_hInstance = hInstance;

	WNDCLASSW wc = { 0 };
	wc.lpfnWndProc = WindowProc;
	wc.hInstance = hInstance;
	wc.lpszClassName = L"PomodoroClass";
	wc.hCursor = LoadCursorW(NULL, IDC_ARROW);
	wc.hbrBackground = CreateSolidBrush(BACKGROUND_COLOR);
	wc.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WINDOW_ICON));

	if (!RegisterClassW(&wc)) {
		FreeLibrary(hBass);
		return 0;
	}

	int windowWidth = 220;
	int windowHeight = 150;
	int screenWidth = GetSystemMetrics(SM_CXSCREEN);
	int screenHeight = GetSystemMetrics(SM_CYSCREEN);
	int posX = (screenWidth - windowWidth) / 2;
	int posY = (screenHeight - windowHeight) / 2;

	HWND hwnd = CreateWindowExW(
		0,
		wc.lpszClassName,
		L"Pomodoro Timer",
		WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX | WS_VISIBLE,
		posX, posY, windowWidth, windowHeight,
		NULL, NULL, hInstance, NULL
	);

	if (!hwnd) {
		FreeLibrary(hBass);
		return 0;
	}

	MSG msg = { 0 };
	while (GetMessageW(&msg, NULL, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessageW(&msg);
	}

	FreeLibrary(hBass);
	return (int)msg.wParam;
}