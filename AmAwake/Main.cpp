#include <windows.h>
#include <shellapi.h>
#include <commctrl.h> // Заголовок для работы с Trackbar
#include <string>
#pragma comment(lib, "comctl32.lib") // Линковка библиотеки comctl32.lib
#include "resource.h"
// Идентификаторы элементов управления и меню
#define ID_EDIT            101
#define ID_SLIDER          102
#define ID_START           103
#define ID_TIMER           105
#define ID_TRAYICON        106
#define ID_TRAYMENU_EXIT   203

// Глобальные переменные
HINSTANCE hInst;
HWND hEdit, hSlider, hStart;
UINT_PTR timerId = 0;
NOTIFYICONDATA nid = {}; // Глобальная переменная для уведомлений

// Уведомление с приоритетом реального времени
void ShowTrayNotification(const std::wstring& title, const std::wstring& message) {
	nid.uFlags = NIF_INFO; // Флаг, что будет использоваться информационное уведомление
	wcscpy_s(nid.szInfoTitle, title.c_str()); // Заголовок уведомления
	wcscpy_s(nid.szInfo, message.c_str());   // Текст уведомления
	nid.dwInfoFlags = NIIF_ERROR;            // Тип значка в уведомлении (ошибка)

	Shell_NotifyIcon(NIM_MODIFY, &nid);      // Отображаем уведомление
}

// Обработчик сообщений
LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
	switch (msg) {
	case WM_CREATE: {
		// Поле для ввода текста
		hEdit = CreateWindowEx(0, L"EDIT", L"", WS_CHILD | WS_VISIBLE | WS_BORDER | ES_LEFT,
			10, 10, 300, 25, hwnd, (HMENU)ID_EDIT, hInst, NULL);

		// Ползунок (Trackbar)
		hSlider = CreateWindowEx(0, TRACKBAR_CLASS, L"Trackbar",
			WS_CHILD | WS_VISIBLE | TBS_AUTOTICKS,
			10, 50, 300, 30, hwnd, (HMENU)ID_SLIDER, hInst, NULL);
		SendMessage(hSlider, TBM_SETRANGE, TRUE, MAKELONG(2, 120)); // От 1 до 60 минут (шаг 0.5)
		SendMessage(hSlider, TBM_SETTICFREQ, 2, 0);
		SendMessage(hSlider, TBM_SETPOS, TRUE, 2); // Начальная позиция 1 минута

		// Кнопка "Старт/Стоп"
		hStart = CreateWindowEx(0, L"BUTTON", L"Старт", WS_CHILD | WS_VISIBLE,
			10, 100, 100, 30, hwnd, (HMENU)ID_START, hInst, NULL);

		// Инициализация значка в трее
		ZeroMemory(&nid, sizeof(NOTIFYICONDATA));
		nid.cbSize = sizeof(NOTIFYICONDATA);
		nid.hWnd = hwnd;
		nid.uID = ID_TRAYICON;
		nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
		nid.uCallbackMessage = WM_USER + 1;
		nid.hIcon = LoadIcon(hInst, MAKEINTRESOURCE(IDI_MYICON));
		wcscpy_s(nid.szTip, L"AmAwake");
		Shell_NotifyIcon(NIM_ADD, &nid);
		break;
	}

	case WM_COMMAND:
		switch (LOWORD(wParam)) {
		case ID_START: {
			if (timerId == 0) {
				// Запуск таймера
				wchar_t buffer[256];
				GetWindowText(hEdit, buffer, 256); // Получение текста из поля
				std::wstring messageText = buffer;

				int pos = SendMessage(hSlider, TBM_GETPOS, 0, 0);
				UINT interval = pos * 30 * 1000;

				timerId = SetTimer(hwnd, ID_TIMER, interval, NULL);
				SetWindowText(hStart, L"Стоп"); // Меняем текст кнопки на "Стоп"
				MessageBox(hwnd, L"Таймер запущен.", L"Информация", MB_OK | MB_ICONINFORMATION);
			}
			else {
				// Остановка таймера
				KillTimer(hwnd, ID_TIMER);
				timerId = 0;
				SetWindowText(hStart, L"Старт"); // Меняем текст кнопки на "Старт"
				MessageBox(hwnd, L"Таймер остановлен.", L"Информация", MB_OK | MB_ICONINFORMATION);
			}
			break;
		}

		case ID_TRAYMENU_EXIT:
			PostMessage(hwnd, WM_DESTROY, 0, 0);
			break;
		}
		break;

	case WM_TIMER:
		if (wParam == ID_TIMER) {
			wchar_t buffer[256];
			GetWindowText(hEdit, buffer, 256); // Получение текста из поля
			std::wstring messageText = buffer;

			// Показ всплывающего уведомления с текстом из поля
			ShowTrayNotification(L"Напоминание", messageText);
			MessageBoxW(hwnd, messageText.c_str(), L"Напоминание", MB_OK | MB_ICONINFORMATION);
		}
		break;

	case WM_CLOSE:
		// Скрываем окно вместо закрытия
		ShowWindow(hwnd, SW_HIDE);
		break;

	case WM_ERASEBKGND: {
		// Устраняем черноту окна при перерисовке
		HDC hdc = (HDC)wParam;
		RECT rect;
		GetClientRect(hwnd, &rect);
		FillRect(hdc, &rect, (HBRUSH)(COLOR_WINDOW + 1));
		return 1; // Указываем, что фон уже был перерисован
	}

	case WM_SIZE:
		if (wParam == SIZE_RESTORED) {
			// Перерисовка дочерних окон при восстановлении
			RedrawWindow(hwnd, NULL, NULL, RDW_INVALIDATE | RDW_ALLCHILDREN);
		}
		break;

	case WM_DESTROY:
		Shell_NotifyIcon(NIM_DELETE, &nid);
		PostQuitMessage(0);
		break;

	case WM_USER + 1:
		if (lParam == WM_RBUTTONUP) {
			// Контекстное меню при правом клике
			POINT pt;
			GetCursorPos(&pt);

			HMENU hMenu = CreatePopupMenu();
			if (hMenu) {
				InsertMenu(hMenu, -1, MF_BYPOSITION | MF_STRING, ID_TRAYMENU_EXIT, L"Выход");

				SetForegroundWindow(hwnd);
				TrackPopupMenu(hMenu, TPM_RIGHTBUTTON, pt.x, pt.y, 0, hwnd, NULL);
				DestroyMenu(hMenu);
			}
		}
		else if (lParam == WM_LBUTTONUP) {
			// Восстанавливаем окно при левом клике
			ShowWindow(hwnd, SW_SHOW);
			SetForegroundWindow(hwnd);
			RedrawWindow(hwnd, NULL, NULL, RDW_INVALIDATE | RDW_ALLCHILDREN);
		}
		break;

	default:
		return DefWindowProc(hwnd, msg, wParam, lParam);
	}
	return 0;
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE, PWSTR, int nCmdShow) {
	hInst = hInstance;

	const wchar_t CLASS_NAME[] = L"AmAwakeClass";

	WNDCLASS wc = { };
	wc.lpfnWndProc = WndProc;
	wc.hInstance = hInstance;
	wc.lpszClassName = CLASS_NAME;
	wc.hIcon = LoadIcon(NULL, IDC_ICON);
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);

	RegisterClass(&wc);

	HWND hwnd = CreateWindowEx(
		0,
		CLASS_NAME,
		L"Напоминалка",
		WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX, // Без максимизации
		CW_USEDEFAULT, CW_USEDEFAULT, 350, 200,
		NULL,
		NULL,
		hInstance,
		NULL
	);

	if (hwnd == NULL) {
		return 0;
	}

	ShowWindow(hwnd, nCmdShow);

	MSG msg = { };
	while (GetMessage(&msg, NULL, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return 0;
}
