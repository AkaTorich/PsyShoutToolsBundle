/*******************************************************************
  Полный рабочий пример Win32-приложения в ОДНОМ .CPP файле
  с ИКОНКОЙ В ТРЕЕ:
  - Главное окно (Calendar + ListBox + кнопки)
  - Окно "Add/Edit Task" (без .rc-файлов)
  - Сохранение/загрузка задач (binary) из tasks.dat
  - Иконка в системном трее (Shell_NotifyIcon)
********************************************************************/
#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <commctrl.h>     // Для календаря (MonthCal) + Common Controls
#include <vector>
#include <string>
#include <ctime>
#include <fstream>
#include "resource.h"
// Подключаем библиотеку comctl32.lib (иначе будет ошибка линковки InitCommonControlsEx)
#pragma comment(lib, "comctl32.lib")

// -----------------------------------------------------------------
// Структура задачи
struct Task {
    std::wstring name;
    time_t       timestamp;   // текущее время напоминания
    int          repeatDays;  // повтор через N дней (0 если не повторяется)
};

// Глобальные переменные
HINSTANCE   g_hInst = nullptr; // Дескриптор приложения
HWND        g_hMainWnd = nullptr; // Главное окно
HWND        g_hCalendar = nullptr; // Календарь на главном окне
HWND        g_hTaskList = nullptr; // ListBox со списком задач

// Массив всех задач
static std::vector<Task> g_tasks;

// Индекс задачи, которую мы редактируем. -1 => создаём новую
static int g_editIndex = -1;

// Окно "Add/Edit Task" (диалог без .rc)
HWND g_hAddEditWnd = nullptr;
HWND g_hNameEdit = nullptr;
HWND g_hAddCal = nullptr;
HWND g_hHourEdit = nullptr;
HWND g_hMinEdit = nullptr;
HWND g_hOkButton = nullptr;
HWND g_hCancelButton = nullptr;

// Иконка в трее
NOTIFYICONDATAW g_nid = {};

// Идентификаторы
enum {
    // Элементы главного окна
    IDC_MAIN_CAL = 1001,
    IDC_MAIN_TASKLIST = 1002,
    IDC_BTN_ADD = 1003,
    IDC_BTN_EDIT = 1004,
    IDC_BTN_DELETE = 1005,
    IDC_BTN_EXIT = 1006,

    IDC_TIMER_CHECK = 1101,

    // Окно добавления/редактирования
    IDC_ADD_NAMEEDIT = 2001,
    IDC_ADD_CAL = 2002,
    IDC_ADD_HOUREDIT = 2003,
    IDC_ADD_MINEDIT = 2004,
    IDC_ADD_OK = 2005,
    IDC_ADD_CANCEL = 2006,
};

// Константы для иконки в трее и меню
#define WM_TRAYICON           (WM_APP + 1)
#define ID_TRAYICON           3001
#define ID_TRAY_MENU_OPEN     3002
#define ID_TRAY_MENU_EXIT     3003

// -----------------------------------------------------------------
// Прототипы функций
LRESULT CALLBACK MainWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK AddEditWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

void RegisterMainWindowClass();
void RegisterAddEditClass();
void LoadTasks();
void SaveTasks();
void RefreshTaskList(); // перезаполнить ListBox из g_tasks

// Для трея:
bool AddTrayIcon(HWND hwnd);
void RemoveTrayIcon(HWND hwnd);
// Базовый заголовок окна
const wchar_t* BASE_WINDOW_TITLE = L"Calendar with Tasks";
// -----------------------------------------------------------------
// WinMain - точка входа
// WinMain - точка входа
int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE, PWSTR, int nCmdShow)
{
	g_hInst = hInstance;

	// Инициализация Common Controls (для календарей, и т.д.)
	INITCOMMONCONTROLSEX icex = {};
	icex.dwSize = sizeof(icex);
	icex.dwICC = ICC_DATE_CLASSES;
	InitCommonControlsEx(&icex);

	// Регистрируем класс главного окна
	RegisterMainWindowClass();

	// Регистрируем класс окна добавления/редактирования
	RegisterAddEditClass();

	// Загружаем задачи из файла (до создания окна)
	LoadTasks();

	// Создаём главное окно
	g_hMainWnd = CreateWindowW(
		L"MainWindowClass",          // Имя класса
		BASE_WINDOW_TITLE,           // Базовый заголовок
		WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, CW_USEDEFAULT,
		600, 310,
		nullptr, nullptr,
		g_hInst, nullptr
	);

	if (!g_hMainWnd) {
		MessageBoxW(nullptr, L"Failed to create main window!", L"Error", MB_ICONERROR);
		return 0;
	}

	ShowWindow(g_hMainWnd, nCmdShow);
	UpdateWindow(g_hMainWnd);

	// Добавляем иконку в трей
	if (!AddTrayIcon(g_hMainWnd)) {
		MessageBoxW(g_hMainWnd, L"Failed to add tray icon!", L"Error", MB_OK | MB_ICONERROR);
	}

	// Запуск цикла сообщений
	MSG msg;
	while (GetMessageW(&msg, nullptr, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessageW(&msg);
	}

	// Удаляем иконку из трея перед выходом
	RemoveTrayIcon(g_hMainWnd);
	return (int)msg.wParam;
}


// -----------------------------------------------------------------
// Регистрируем класс главного окна
void RegisterMainWindowClass()
{
    WNDCLASSEXW wc = {};
    wc.cbSize = sizeof(wc);
    wc.lpszClassName = L"MainWindowClass";
    wc.hInstance = g_hInst;
    wc.lpfnWndProc = MainWndProc;
    wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);

    RegisterClassExW(&wc);
}

// -----------------------------------------------------------------
// Регистрируем класс окна добавления/редактирования
void RegisterAddEditClass()
{
	WNDCLASSEXW wc = {};
	wc.cbSize = sizeof(wc);
	wc.lpszClassName = L"AddEditTaskClass"; // Исправлено на уникальное имя класса
	wc.hInstance = g_hInst;
	wc.lpfnWndProc = AddEditWndProc;
	wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wc.hIcon = LoadIcon(g_hInst, MAKEINTRESOURCE(IDI_APP_ICON));
	wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);

	RegisterClassExW(&wc);
}


// -----------------------------------------------------------------
// Функция добавления иконки в трей
bool AddTrayIcon(HWND hwnd)
{
	ZeroMemory(&g_nid, sizeof(g_nid));
	g_nid.cbSize = sizeof(NOTIFYICONDATAW);
	g_nid.hWnd = hwnd;
	g_nid.uID = ID_TRAYICON;
	g_nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
	g_nid.uCallbackMessage = WM_TRAYICON;

	// Загрузка иконки приложения для трея
	g_nid.hIcon = LoadIcon(g_hInst, MAKEINTRESOURCE(IDI_APP_ICON));
	if (!g_nid.hIcon) {
		// fallback
		g_nid.hIcon = LoadIcon(nullptr, IDI_APPLICATION);
	}

	// Подсказка
	wcscpy_s(g_nid.szTip, L"Calendar with Tasks (Tray)");

	return (Shell_NotifyIconW(NIM_ADD, &g_nid) == TRUE);
}

// -----------------------------------------------------------------
// Удаление иконки из трея
void RemoveTrayIcon(HWND hwnd)
{
    NOTIFYICONDATAW nid = {};
    nid.cbSize = sizeof(nid);
    nid.hWnd = hwnd;
    nid.uID = ID_TRAYICON;

    Shell_NotifyIconW(NIM_DELETE, &nid);
}

// -----------------------------------------------------------------
// Оконная процедура главного окна
// Оконная процедура главного окна
LRESULT CALLBACK MainWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_CREATE:
	{
		// Создаём календарь
		g_hCalendar = CreateWindowExW(
			0,
			MONTHCAL_CLASSW,
			nullptr,
			WS_BORDER | WS_CHILD | WS_VISIBLE,
			10, 10,
			250, 200,
			hwnd,
			(HMENU)IDC_MAIN_CAL,
			g_hInst,
			nullptr
		);

		// Список задач
		g_hTaskList = CreateWindowExW(
			WS_EX_CLIENTEDGE,
			L"LISTBOX",
			nullptr,
			WS_CHILD | WS_VISIBLE | WS_VSCROLL | LBS_NOTIFY,
			270, 10,
			300, 200,
			hwnd,
			(HMENU)IDC_MAIN_TASKLIST,
			g_hInst,
			nullptr
		);

		// 4 кнопки в одну линию
		int xStart = 10;
		int yPos = 230;
		int btnWidth = 100;
		int btnHeight = 30;
		int spacing = 10;

		// Add Task
		CreateWindowW(L"BUTTON", L"Add Task",
			WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
			xStart, yPos, btnWidth, btnHeight,
			hwnd, (HMENU)IDC_BTN_ADD, g_hInst, nullptr);
		xStart += btnWidth + spacing;

		// Edit Task
		CreateWindowW(L"BUTTON", L"Edit Task",
			WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
			xStart, yPos, btnWidth, btnHeight,
			hwnd, (HMENU)IDC_BTN_EDIT, g_hInst, nullptr);
		xStart += btnWidth + spacing;

		// Delete Task
		CreateWindowW(L"BUTTON", L"Delete Task",
			WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
			xStart, yPos, btnWidth, btnHeight,
			hwnd, (HMENU)IDC_BTN_DELETE, g_hInst, nullptr);
		xStart += btnWidth + spacing + 130;

		// Exit
		CreateWindowW(L"BUTTON", L"Exit",
			WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
			xStart, yPos, btnWidth, btnHeight,
			hwnd, (HMENU)IDC_BTN_EXIT, g_hInst, nullptr);

		// Таймер для проверки задач и обновления времени
		SetTimer(hwnd, IDC_TIMER_CHECK, 1000, nullptr);

		// Заполняем ListBox уже загруженными задачами
		RefreshTaskList();
	}
	break;

	// Сообщение от иконки в трее
	case WM_TRAYICON:
		switch (lParam)
		{
		case WM_LBUTTONDOWN:
			// ЛКМ: Показать/спрятать окно
			if (IsWindowVisible(hwnd)) {
				ShowWindow(hwnd, SW_HIDE);
			}
			else {
				ShowWindow(hwnd, SW_SHOW);
				SetForegroundWindow(hwnd);
			}
			break;

		case WM_RBUTTONDOWN:
		{
			// ПКМ: контекстное меню
			HMENU hMenu = CreatePopupMenu();
			AppendMenuW(hMenu, MF_STRING, ID_TRAY_MENU_OPEN, L"Open");
			AppendMenuW(hMenu, MF_STRING, ID_TRAY_MENU_EXIT, L"Exit");

			POINT pt;
			GetCursorPos(&pt);
			SetForegroundWindow(hwnd);

			TrackPopupMenu(hMenu, TPM_RIGHTBUTTON, pt.x, pt.y, 0, hwnd, NULL);
			DestroyMenu(hMenu);
		}
		break;
		}
		return 0;

	case WM_COMMAND:
	{
		switch (LOWORD(wParam))
		{
		case IDC_BTN_ADD:
		{
			// g_editIndex = -1 => создаём новую задачу
			g_editIndex = -1;

			// Создаём окно Add/Edit
			if (!g_hAddEditWnd) {
				g_hAddEditWnd = CreateWindowExW(
					WS_EX_DLGMODALFRAME,
					L"AddEditTaskClass",
					L"Add Task",
					WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU,
					CW_USEDEFAULT, CW_USEDEFAULT,
					300, 330,
					hwnd,
					nullptr,
					g_hInst,
					nullptr
				);
			}
			ShowWindow(g_hAddEditWnd, SW_SHOW);
			UpdateWindow(g_hAddEditWnd);
		}
		break;

		case IDC_BTN_EDIT:
		{
			// Узнаём, что выделено в списке
			int sel = (int)SendMessageW(g_hTaskList, LB_GETCURSEL, 0, 0);
			if (sel != LB_ERR && sel < (int)g_tasks.size()) {
				g_editIndex = sel;

				// Открываем окно Add/Edit
				if (!g_hAddEditWnd) {
					g_hAddEditWnd = CreateWindowExW(
						WS_EX_DLGMODALFRAME,
						L"AddEditTaskClass",
						L"Edit Task",
						WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU,
						CW_USEDEFAULT, CW_USEDEFAULT,
						300, 330,
						hwnd,
						nullptr,
						g_hInst,
						nullptr
					);
				}
				// Меняем заголовок
				SetWindowTextW(g_hAddEditWnd, L"Edit Task");
				ShowWindow(g_hAddEditWnd, SW_SHOW);
				UpdateWindow(g_hAddEditWnd);
			}
			else {
				MessageBoxW(hwnd, L"Select a task to edit!", L"Error", MB_OK | MB_ICONERROR);
			}
		}
		break;

		case IDC_BTN_DELETE:
		{
			int sel = (int)SendMessageW(g_hTaskList, LB_GETCURSEL, 0, 0);
			if (sel != LB_ERR && sel < (int)g_tasks.size()) {
				g_tasks.erase(g_tasks.begin() + sel);
				RefreshTaskList();
			}
			else {
				MessageBoxW(hwnd, L"Select a task to delete!", L"Error", MB_OK | MB_ICONERROR);
			}
		}
		break;

		case IDC_BTN_EXIT:
			SendMessageW(hwnd, WM_CLOSE, 0, 0);
			break;

			// Пункты контекстного меню из трея
		case ID_TRAY_MENU_OPEN:
			ShowWindow(hwnd, SW_SHOW);
			SetForegroundWindow(hwnd);
			break;
		case ID_TRAY_MENU_EXIT:
			PostMessageW(hwnd, WM_CLOSE, 0, 0);
			break;
		}
	}
	break;

	case WM_TIMER:
		if (wParam == IDC_TIMER_CHECK) {
			// Обновление заголовка окна с текущим временем
			SYSTEMTIME st;
			GetLocalTime(&st);
			wchar_t timeStr[100];
			swprintf_s(timeStr, L"%s - %02d:%02d:%02d",
				BASE_WINDOW_TITLE,
				st.wHour, st.wMinute, st.wSecond);
			SetWindowTextW(hwnd, timeStr);

			// Существующая логика проверки задач
			static bool isProcessing = false; // Флаг для предотвращения повторной обработки
			if (isProcessing) return 0; // Если обработка уже идёт, выходим

			isProcessing = true; // Устанавливаем флаг, чтобы не обрабатывать задачи одновременно
			time_t now = std::time(nullptr);

			for (auto& task : g_tasks)
			{
				if (task.timestamp != 0 && task.timestamp <= now)
				{
					// Показываем напоминание
					std::wstring msg = L"Пора выполнить задачу: " + task.name;
					MessageBoxW(hwnd, msg.c_str(), L"Напоминание", MB_OK | MB_ICONINFORMATION);

					if (task.repeatDays > 0) {
						// Если задача повторяется, обновляем время напоминания
						task.timestamp += (86400LL * task.repeatDays); // 86400 секунд в сутках
					}
					else {
						// Если не повторяется, сбрасываем время
						task.timestamp = 0;
					}

					SaveTasks(); // Сохраняем задачи после изменения
					break; // Выходим из цикла, чтобы обработать только одну задачу за раз
				}
			}

			isProcessing = false; // Сбрасываем флаг после завершения обработки
		}
		break;

	case WM_CLOSE:
	{
		// Сохраняем задачи перед выходом
		SaveTasks();

		if (MessageBoxW(hwnd, L"Are you sure to exit?", L"Exit", MB_OKCANCEL | MB_ICONQUESTION) == IDOK) {
			KillTimer(hwnd, IDC_TIMER_CHECK);
			DestroyWindow(hwnd);
		}
	}
	return 0;

	case WM_DESTROY:
		PostQuitMessage(0);
		break;

	default:
		return DefWindowProcW(hwnd, msg, wParam, lParam);
	}
	return 0;
}
// -----------------------------------------------------------------
// Оконная процедура "Add/Edit Task" (диалог без .rc)
LRESULT CALLBACK AddEditWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_CREATE:
    {
        // Текст "Task Name:"
        CreateWindowW(L"STATIC", L"Task Name:",
            WS_CHILD | WS_VISIBLE,
            10, 10, 80, 20,
            hwnd, nullptr, g_hInst, nullptr);

        // Поле ввода имени
        g_hNameEdit = CreateWindowW(L"EDIT", L"",
            WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL,
            100, 10, 160, 20,
            hwnd, (HMENU)IDC_ADD_NAMEEDIT, g_hInst, nullptr);

        // Текст "Date:"
        CreateWindowW(L"STATIC", L"Date:",
            WS_CHILD | WS_VISIBLE,
            10, 40, 80, 20,
            hwnd, nullptr, g_hInst, nullptr);

        // Месячный календарь – увеличиваем высоту (160 вместо 130)
        g_hAddCal = CreateWindowExW(
            0,
            MONTHCAL_CLASSW,
            nullptr,
            WS_BORDER | WS_CHILD | WS_VISIBLE,
            100, 40,
            160, 160,
            hwnd,
            (HMENU)IDC_ADD_CAL,
            g_hInst,
            nullptr);

        // Часы
        CreateWindowW(L"STATIC", L"Hour:",
            WS_CHILD | WS_VISIBLE,
            10, 210, 40, 20,
            hwnd, nullptr, g_hInst, nullptr);

        g_hHourEdit = CreateWindowW(L"EDIT", L"",
            WS_CHILD | WS_VISIBLE | WS_BORDER | ES_NUMBER,
            60, 210, 40, 20,
            hwnd, (HMENU)IDC_ADD_HOUREDIT, g_hInst, nullptr);

        // Минуты
        CreateWindowW(L"STATIC", L"Min:",
            WS_CHILD | WS_VISIBLE,
            110, 210, 30, 20,
            hwnd, nullptr, g_hInst, nullptr);

        g_hMinEdit = CreateWindowW(L"EDIT", L"",
            WS_CHILD | WS_VISIBLE | WS_BORDER | ES_NUMBER,
            150, 210, 40, 20,
            hwnd, (HMENU)IDC_ADD_MINEDIT, g_hInst, nullptr);

        // Кнопки OK/Cancel
        g_hOkButton = CreateWindowW(L"BUTTON", L"OK",
            WS_CHILD | WS_VISIBLE | BS_DEFPUSHBUTTON,
            60, 250, 60, 30,
            hwnd, (HMENU)IDC_ADD_OK, g_hInst, nullptr);

        g_hCancelButton = CreateWindowW(L"BUTTON", L"Cancel",
            WS_CHILD | WS_VISIBLE,
            140, 250, 60, 30,
            hwnd, (HMENU)IDC_ADD_CANCEL, g_hInst, nullptr);
    }
    break;

    case WM_SHOWWINDOW:
        if (wParam == TRUE) {
            // Если окно показывается (Add или Edit),
            // заполняем поля, если g_editIndex != -1 (EDIT MODE)
            if (g_editIndex >= 0 && g_editIndex < (int)g_tasks.size()) {
                const Task& t = g_tasks[g_editIndex];

                // Имя задачи
                SetWindowTextW(g_hNameEdit, t.name.c_str());

                // Разбираем t.timestamp => SYSTEMTIME
                tm* localTime = localtime(&t.timestamp);
                SYSTEMTIME st = {};
                st.wYear = localTime->tm_year + 1900;
                st.wMonth = localTime->tm_mon + 1;
                st.wDay = localTime->tm_mday;
                st.wHour = localTime->tm_hour;
                st.wMinute = localTime->tm_min;
                SendMessageW(g_hAddCal, MCM_SETCURSEL, 0, (LPARAM)&st);

                // Hour/Min
                wchar_t buf[8];
                swprintf_s(buf, L"%d", localTime->tm_hour);
                SetWindowTextW(g_hHourEdit, buf);
                swprintf_s(buf, L"%d", localTime->tm_min);
                SetWindowTextW(g_hMinEdit, buf);
            }
            else {
                // ADD MODE: Очищаем поля
                SetWindowTextW(g_hNameEdit, L"");

                // Устанавливаем календарь на сегодня
                SYSTEMTIME st = {};
                GetLocalTime(&st);
                SendMessageW(g_hAddCal, MCM_SETCURSEL, 0, (LPARAM)&st);

                // Час/минуту в 0
                SetWindowTextW(g_hHourEdit, L"0");
                SetWindowTextW(g_hMinEdit, L"0");
            }
        }
        break;

    case WM_COMMAND:
        switch (LOWORD(wParam))
        {
        case IDC_ADD_OK:
        {
            // Читаем поля
            wchar_t nameBuf[256];
            GetWindowTextW(g_hNameEdit, nameBuf, 256);

            SYSTEMTIME st;
            SendMessageW(g_hAddCal, MCM_GETCURSEL, 0, (LPARAM)&st);

            wchar_t hourBuf[8], minBuf[8];
            GetWindowTextW(g_hHourEdit, hourBuf, 8);
            GetWindowTextW(g_hMinEdit, minBuf, 8);

            int hour = _wtoi(hourBuf);
            int mins = _wtoi(minBuf);
            if (hour < 0 || hour > 23 || mins < 0 || mins > 59) {
                MessageBoxW(hwnd, L"Invalid time!", L"Error", MB_OK | MB_ICONERROR);
                return 0;
            }

            // Формируем time_t
            tm taskTime = {};
            taskTime.tm_year = st.wYear - 1900;
            taskTime.tm_mon = st.wMonth - 1;
            taskTime.tm_mday = st.wDay;
            taskTime.tm_hour = hour;
            taskTime.tm_min = mins;
            taskTime.tm_sec = 0;

            time_t stamp = mktime(&taskTime);

            if (g_editIndex >= 0 && g_editIndex < (int)g_tasks.size()) {
                // EDIT
                g_tasks[g_editIndex].name = nameBuf;
                g_tasks[g_editIndex].timestamp = stamp;
                // Не трогаем repeatDays, если хотите всегда 0 – можно обнулить
                g_tasks[g_editIndex].repeatDays = 0;
            }
            else {
                // ADD
                Task t;
                t.name = nameBuf;
                t.timestamp = stamp;
                t.repeatDays = 0; // ВАЖНО: обнуляем, чтобы не было мусорного значения
                g_tasks.push_back(t);
            }

            // Обновляем ListBox
            RefreshTaskList();

            // Скрываем окно
            ShowWindow(hwnd, SW_HIDE);
        }
        break;

        case IDC_ADD_CANCEL:
        {
            // Закрываем окно без изменений
            ShowWindow(hwnd, SW_HIDE);
        }
        break;
        }
        break;

    case WM_CLOSE:
        // При закрытии крестиком — просто скрываем
        ShowWindow(hwnd, SW_HIDE);
        return 0;

    case WM_DESTROY:
        return 0;

    default:
        return DefWindowProcW(hwnd, msg, wParam, lParam);
    }
    return 0;
}

// -----------------------------------------------------------------
// Функция перезаполнения ListBox
void RefreshTaskList()
{
    if (!g_hTaskList) return;

    SendMessageW(g_hTaskList, LB_RESETCONTENT, 0, 0);
    for (auto& t : g_tasks) {
        SendMessageW(g_hTaskList, LB_ADDSTRING, 0, (LPARAM)t.name.c_str());
    }
}

// -----------------------------------------------------------------
// Загрузка задач из файла tasks.dat
void LoadTasks()
{
    std::ifstream file("tasks.dat", std::ios::binary);
    if (!file.is_open()) {
        // Если нет файла - не ошибка
        return;
    }

    size_t count = 0;
    file.read(reinterpret_cast<char*>(&count), sizeof(count));
    if (!file || count > 100000) {
        // Файл битый или слишком большой
        return;
    }

    g_tasks.clear();
    g_tasks.reserve(count);

    for (size_t i = 0; i < count; i++) {
        size_t nameSize = 0;
        file.read(reinterpret_cast<char*>(&nameSize), sizeof(nameSize));
        if (!file || nameSize > 1024) {
            break;
        }

        std::vector<wchar_t> buf(nameSize);
        file.read(reinterpret_cast<char*>(buf.data()), nameSize * sizeof(wchar_t));
        if (!file) {
            break;
        }
        std::wstring tname(buf.begin(), buf.end());

        time_t stamp;
        file.read(reinterpret_cast<char*>(&stamp), sizeof(stamp));
        if (!file) {
            break;
        }

        // Добавляем задачу
        Task t;
        t.name = tname;
        t.timestamp = stamp;
        // ВАЖНО: повтор тут не загружается, значит обнуляем
        t.repeatDays = 0;

        g_tasks.push_back(t);
    }
    file.close();
}

// -----------------------------------------------------------------
// Сохранение задач в файл tasks.dat
void SaveTasks()
{
    std::ofstream file("tasks.dat", std::ios::binary);
    if (!file.is_open()) {
        return;
    }

    size_t count = g_tasks.size();
    file.write(reinterpret_cast<const char*>(&count), sizeof(count));

    for (auto& t : g_tasks) {
        // Сохраняем имя
        size_t nameSize = t.name.size();
        file.write(reinterpret_cast<const char*>(&nameSize), sizeof(nameSize));
        file.write(reinterpret_cast<const char*>(t.name.data()), nameSize * sizeof(wchar_t));

        // Сохраняем время
        file.write(reinterpret_cast<const char*>(&t.timestamp), sizeof(t.timestamp));

        // Если бы хотели сохранять repeatDays, нужно и его писать/читать,
        // но пока не делаем, раз не используется:
        // file.write(reinterpret_cast<const char*>(&t.repeatDays), sizeof(t.repeatDays));
    }
    file.close();
}
