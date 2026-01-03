#include <windows.h>
#include <commctrl.h>
#include <portaudio.h>
#include <cmath>
#include <string>
#include <vector>
#include <sstream>
#include <iomanip> // Для std::setprecision
#include <cctype>  // Для проверки символов
#include <fstream> // Для работы с файлами
#include <random>  // Для генерации шума

// Линки к библиотекам
#ifndef _WIN64
#pragma comment(lib, "portaudio_x86.lib") // Для 32-битной сборки
#else
#pragma comment(lib, "portaudio_x64.lib") // Для 64-битной сборки
#endif
#pragma comment(lib, "Comctl32.lib")
// Определение M_PI, если оно не определено
#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

// Глобальная переменная для hInstance
HINSTANCE g_hInstance;

// Глобальная переменная для главного окна
HWND g_hwndMain = NULL;

// Идентификаторы контролов
#define IDC_BUTTON_STARTSTOP     101
#define IDC_TRACKBAR_FREQ_LEFT   102
#define IDC_LABEL_FREQ_LEFT      103
#define IDC_EDIT_FREQ_LEFT       104
#define IDC_TRACKBAR_FREQ_RIGHT  105
#define IDC_LABEL_FREQ_RIGHT     106
#define IDC_EDIT_FREQ_RIGHT      107
#define IDC_TRACKBAR_BLINK_FREQ  108
#define IDC_LABEL_BLINK_FREQ     109
#define IDC_COMBO_DEVICES        110
#define IDC_CHECK_BLINK          111 // Идентификатор для чекбокса блінкера
#define IDC_COMBO_WAVE_LEFT      112 // Новый идентификатор для выбора волны левого канала
#define IDC_COMBO_WAVE_RIGHT     113 // Новый идентификатор для выбора волны правого канала

// Идентификаторы меню
#define ID_FILE_SAVE_PRESET      40001
#define ID_FILE_LOAD_PRESET      40002
#define ID_HELP_ABOUT            40003

#define TIMER_BLINK_EXTRA_ID     201 // Идентификатор таймера для дополнительного окна

// Пользовательские сообщения
#define WM_STOP_PLAYBACK         (WM_USER + 1)

// Перечисление типов волн
enum WaveformType {
	WAVE_SINE = 0,
	WAVE_SQUARE,
	WAVE_TRIANGLE,
	WAVE_SAWTOOTH,
	WAVE_NOISE,
	WAVE_COUNT
};

// Массив опций для типов волн (перемещён в глобальную область видимости)
const char* waveOptions[WAVE_COUNT] = { "Sine", "Square", "Triangle", "Sawtooth", "Noise" };

// Глобальные переменные
PaStream* stream = nullptr;
bool isPlaying = false;
double frequencyLeft = 440.0;
double frequencyRight = 446.0;
double blinkFrequency = 2.0; // Начальная частота мерцания (Гц)
bool isBlinkEnabled = true;   // Флаг включения блінкера

// Структура для хранения состояния волн
struct WaveData {
	double leftFrequency;
	double leftPhase;
	WaveformType leftWaveform;

	double rightFrequency;
	double rightPhase;
	WaveformType rightWaveform;

	// Генератор случайных чисел для шума
	std::default_random_engine generator;
	std::uniform_real_distribution<float> distribution;

	WaveData() : leftFrequency(440.0), leftPhase(0.0),
		rightFrequency(446.0), rightPhase(0.0),
		leftWaveform(WAVE_SINE), rightWaveform(WAVE_SINE),
		distribution(-1.0f, 1.0f) {
	}
} waveData;

// Переменные для дополнительного окна
HWND hwndBlink = NULL;
bool isBlinkWhite = false; // Флаг текущего цвета фона в дополнительном окне
static HWND hwndButton, hwndTrackbarFreqLeft, hwndLabelFreqLeft, hwndEditFreqLeft;
static HWND hwndTrackbarFreqRight, hwndLabelFreqRight, hwndEditFreqRight;
static HWND hwndTrackbarBlinkFreq, hwndLabelBlinkFreq;
static HWND hwndComboBox;
static HWND hwndCheckBlink; // Хендл чекбокса блінкера
static HWND hwndComboWaveLeft, hwndComboWaveRight; // Хендлы для выбора волны
static HWND hwndLabelWaveLeft, hwndLabelWaveRight; // Метки для выбора волны
static std::vector<std::pair<int, std::string>> deviceList;
static bool isBlinkClassRegistered = false; // Флаг регистрации класса окна мерцания
// Функция обратного вызова PortAudio
static int paCallback(const void* inputBuffer, void* outputBuffer,
	unsigned long framesPerBuffer,
	const PaStreamCallbackTimeInfo* timeInfo,
	PaStreamCallbackFlags statusFlags,
	void* userData)
{
	float* out = (float*)outputBuffer;
	WaveData* wave = (WaveData*)userData;
	double leftPhaseIncrement = 2.0 * M_PI * wave->leftFrequency / 44100.0;
	double rightPhaseIncrement = 2.0 * M_PI * wave->rightFrequency / 44100.0;

	for (unsigned long i = 0; i < framesPerBuffer; ++i) {
		// Генерация левого канала
		float leftSample = 0.0f;
		switch (wave->leftWaveform) {
		case WAVE_SINE:
			leftSample = 0.5f * sin(wave->leftPhase);
			break;
		case WAVE_SQUARE:
			leftSample = (sin(wave->leftPhase) >= 0.0) ? 0.5f : -0.5f;
			break;
		case WAVE_TRIANGLE:
			leftSample = (float)(2.0 / M_PI * asin(sin(wave->leftPhase)));
			break;
		case WAVE_SAWTOOTH:
			leftSample = (float)((wave->leftPhase / M_PI) - 1.0);
			break;
		case WAVE_NOISE:
			leftSample = wave->distribution(wave->generator) * 0.5f;
			break;
		default:
			leftSample = 0.0f;
			break;
		}

		// Генерация правого канала
		float rightSample = 0.0f;
		switch (wave->rightWaveform) {
		case WAVE_SINE:
			rightSample = 0.5f * sin(wave->rightPhase);
			break;
		case WAVE_SQUARE:
			rightSample = (sin(wave->rightPhase) >= 0.0) ? 0.5f : -0.5f;
			break;
		case WAVE_TRIANGLE:
			rightSample = (float)(2.0 / M_PI * asin(sin(wave->rightPhase)));
			break;
		case WAVE_SAWTOOTH:
			rightSample = (float)((wave->rightPhase / M_PI) - 1.0);
			break;
		case WAVE_NOISE:
			rightSample = wave->distribution(wave->generator) * 0.5f;
			break;
		default:
			rightSample = 0.0f;
			break;
		}

		out[2 * i] = leftSample;       // Левый канал
		out[2 * i + 1] = rightSample; // Правый канал

		// Инкремент фаз
		if (wave->leftWaveform != WAVE_NOISE) {
			wave->leftPhase += leftPhaseIncrement;
			if (wave->leftPhase >= 2.0 * M_PI)
				wave->leftPhase -= 2.0 * M_PI;
		}

		if (wave->rightWaveform != WAVE_NOISE) {
			wave->rightPhase += rightPhaseIncrement;
			if (wave->rightPhase >= 2.0 * M_PI)
				wave->rightPhase -= 2.0 * M_PI;
		}
	}

	return paContinue;
}

// Функция для перечисления аудио устройств с отображением Host API
std::vector<std::pair<int, std::string>> GetOutputDevicesWithHostAPI() {
	std::vector<std::pair<int, std::string>> devices;
	int numDevices = Pa_GetDeviceCount();
	if (numDevices < 0) {
		MessageBoxA(NULL, "Не удалось получить количество аудио устройств от PortAudio.", "PortAudio Ошибка", MB_OK | MB_ICONERROR);
		return devices;
	}

	for (int i = 0; i < numDevices; ++i) {
		const PaDeviceInfo* deviceInfo = Pa_GetDeviceInfo(i);
		if (deviceInfo->maxOutputChannels >= 2) { // Требуется минимум 2 канала для стерео
			int hostApiIndex = deviceInfo->hostApi;
			const PaHostApiInfo* hostApiInfo = Pa_GetHostApiInfo(hostApiIndex);
			if (hostApiInfo) {
				std::string hostApiName(hostApiInfo->name);
				std::string deviceName(deviceInfo->name);
				devices.emplace_back(i, hostApiName + ": " + deviceName);
			}
			else {
				// Если Host API не найден, просто добавляем имя устройства
				std::string deviceName(deviceInfo->name);
				devices.emplace_back(i, deviceName);
			}
		}
	}

	return devices;
}

// Функция для логарифмического масштабирования
double LogScale(int pos, int minPos, int maxPos, double minFreq, double maxFreq) {
	if (pos < minPos) pos = minPos;
	if (pos > maxPos) pos = maxPos;
	double logMin = log10(minFreq);
	double logMax = log10(maxFreq);
	double scale = (double)(pos - minPos) / (maxPos - minPos);
	double logFreq = logMin + scale * (logMax - logMin);
	return pow(10.0, logFreq);
}

// Функция для получения позиции на логарифмическом слайдере
int LogScalePosition(double freq, int minPos, int maxPos, double minFreq, double maxFreq) {
	if (freq < minFreq) freq = minFreq;
	if (freq > maxFreq) freq = maxFreq;
	double logMin = log10(minFreq);
	double logMax = log10(maxFreq);
	double logFreq = log10(freq);
	double scale = (logFreq - logMin) / (logMax - logMin);
	return (int)(minPos + scale * (maxPos - minPos));
}

// Функция для проверки, является ли строка числом
bool IsNumeric(const std::string& s) {
	std::istringstream iss(s);
	double d;
	return (iss >> d) && (iss.eof());
}

// Функция для сохранения пресета в файл
bool SavePreset(const std::string& filename, double freqLeft, double freqRight, double blinkFreq, bool blinkEnabled, WaveformType waveLeft, WaveformType waveRight) {
	std::ofstream ofs(filename);
	if (!ofs.is_open()) return false;
	ofs << "frequencyLeft=" << std::fixed << std::setprecision(2) << freqLeft << "\n";
	ofs << "frequencyRight=" << std::fixed << std::setprecision(2) << freqRight << "\n";
	ofs << "blinkFrequency=" << std::fixed << std::setprecision(2) << blinkFreq << "\n";
	ofs << "blinkEnabled=" << (blinkEnabled ? "1" : "0") << "\n";
	ofs << "waveformLeft=" << static_cast<int>(waveLeft) << "\n";
	ofs << "waveformRight=" << static_cast<int>(waveRight) << "\n";
	ofs.close();
	return true;
}

// Функция для загрузки пресета из файла
bool LoadPreset(const std::string& filename, double& freqLeft, double& freqRight, double& blinkFreq, bool& blinkEnabled, WaveformType& waveLeft, WaveformType& waveRight) {
	std::ifstream ifs(filename);
	if (!ifs.is_open()) return false;
	std::string line;
	while (std::getline(ifs, line)) {
		size_t eqPos = line.find('=');
		if (eqPos == std::string::npos) continue;
		std::string key = line.substr(0, eqPos);
		std::string value = line.substr(eqPos + 1);
		if (key == "frequencyLeft") {
			freqLeft = atof(value.c_str());
			if (freqLeft < 1.0) freqLeft = 1.0;
			if (freqLeft > 40000.0) freqLeft = 40000.0;
		}
		else if (key == "frequencyRight") {
			freqRight = atof(value.c_str());
			if (freqRight < 1.0) freqRight = 1.0;
			if (freqRight > 40000.0) freqRight = 40000.0;
		}
		else if (key == "blinkFrequency") {
			blinkFreq = atof(value.c_str());
			if (blinkFreq < 1.0) blinkFreq = 1.0;
			if (blinkFreq > 10.0) blinkFreq = 10.0;
		}
		else if (key == "blinkEnabled") {
			blinkEnabled = (value == "1") ? true : false;
		}
		else if (key == "waveformLeft") {
			int wave = atoi(value.c_str());
			if (wave >= 0 && wave < WAVE_COUNT)
				waveLeft = static_cast<WaveformType>(wave);
		}
		else if (key == "waveformRight") {
			int wave = atoi(value.c_str());
			if (wave >= 0 && wave < WAVE_COUNT)
				waveRight = static_cast<WaveformType>(wave);
		}
	}
	ifs.close();
	return true;
}

// Функция для логирования сообщений (без блокирования GUI)
void LogMessage(const std::string& message) {
	OutputDebugStringA(message.c_str());
}

// Процедура дополнительного окна для мерцания
LRESULT CALLBACK BlinkWindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
	case WM_CREATE:
	{
		// Установка фокуса на окно мерцания
		SetFocus(hwnd);
		OutputDebugStringA("BlinkWindowProc: WM_CREATE, focus set.\n");
	}
	break;

	case WM_ACTIVATE:
	{
		if (WA_ACTIVE == LOWORD(wParam) || WA_CLICKACTIVE == LOWORD(wParam)) {
			SetFocus(hwnd);
			OutputDebugStringA("BlinkWindowProc: WM_ACTIVATE, focus set.\n");
		}
	}
	break;

	case WM_TIMER:
	{
		if (wParam == TIMER_BLINK_EXTRA_ID) {
			// Переключаем цвет фона между чёрным и белым
			isBlinkWhite = !isBlinkWhite;
			InvalidateRect(hwnd, NULL, TRUE); // Запрос на перерисовку окна
		}
	}
	break;

	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hwnd, &ps);
		RECT rect;
		GetClientRect(hwnd, &rect);
		HBRUSH hBrush;
		if (isBlinkWhite) {
			hBrush = CreateSolidBrush(RGB(255, 255, 255)); // Белый цвет
		}
		else {
			hBrush = CreateSolidBrush(RGB(0, 0, 0)); // Чёрный цвет
		}
		FillRect(hdc, &rect, hBrush);
		DeleteObject(hBrush);
		EndPaint(hwnd, &ps);
	}
	break;

	case WM_KEYDOWN:
	{

		LogMessage("Stop button clicked.\n");

		// Остановка воспроизведения
		Pa_StopStream(stream);
		Pa_CloseStream(stream);
		stream = nullptr;
		isPlaying = false;
		SetWindowTextA(hwndButton, "Start");
		LogMessage("Playback stopped.\n");

		// Закрытие дополнительного окна, если оно существует
		if (hwndBlink != NULL) {
			DestroyWindow(hwndBlink);
			hwndBlink = NULL;
			LogMessage("Blink window destroyed.\n");
		}
	}
	break;

	case WM_CLOSE:
	{
		OutputDebugStringA("BlinkWindowProc: WM_CLOSE received.\n");
		// Останавливаем таймер
		KillTimer(hwnd, TIMER_BLINK_EXTRA_ID);
		DestroyWindow(hwnd);
	}
	break;

	case WM_DESTROY:
	{
		// Не вызываем PostQuitMessage, чтобы основное окно продолжало работать
	}
	break;

	default:
		return DefWindowProcA(hwnd, uMsg, wParam, lParam);
	}
	return 0;
}

// Процедура основного окна
LRESULT CALLBACK MainWindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{


	switch (uMsg)
	{
	case WM_CREATE:
	{
		// Инициализация общих контролов
		INITCOMMONCONTROLSEX icex;
		icex.dwSize = sizeof(icex);
		icex.dwICC = ICC_BAR_CLASSES | ICC_WIN95_CLASSES;
		InitCommonControlsEx(&icex);

		// Создание меню
		HMENU hMenubar = CreateMenu();
		HMENU hFileMenu = CreateMenu();
		HMENU hHelpMenu = CreateMenu();

		AppendMenuA(hFileMenu, MF_STRING, ID_FILE_SAVE_PRESET, "Сохранить Пресет");
		AppendMenuA(hFileMenu, MF_STRING, ID_FILE_LOAD_PRESET, "Загрузить Пресет");
		AppendMenuA(hMenubar, MF_POPUP, (UINT_PTR)hFileMenu, "Файл");

		AppendMenuA(hHelpMenu, MF_STRING, ID_HELP_ABOUT, "О программе");
		AppendMenuA(hMenubar, MF_POPUP, (UINT_PTR)hHelpMenu, "Справка");

		SetMenu(hwnd, hMenubar);

		// Кнопка Start/Stop
		hwndButton = CreateWindowExA(0, "BUTTON", "Start",
			WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
			20, 20, 100, 30,
			hwnd, (HMENU)IDC_BUTTON_STARTSTOP, g_hInstance, NULL);

		// Чекбокс для включения/отключения блінкера
		hwndCheckBlink = CreateWindowExA(0, "BUTTON", "Enable Blinker",
			WS_CHILD | WS_VISIBLE | BS_AUTOCHECKBOX,
			140, 25, 150, 20,
			hwnd, (HMENU)IDC_CHECK_BLINK, g_hInstance, NULL);
		SendMessageA(hwndCheckBlink, BM_SETCHECK, isBlinkEnabled ? BST_CHECKED : BST_UNCHECKED, 0);

		// Слайдер (Trackbar) для частоты левого канала
		hwndTrackbarFreqLeft = CreateWindowExA(0, TRACKBAR_CLASSA, NULL,
			WS_CHILD | WS_VISIBLE | TBS_AUTOTICKS,
			20, 70, 300, 30,
			hwnd, (HMENU)IDC_TRACKBAR_FREQ_LEFT, g_hInstance, NULL);
		SendMessageA(hwndTrackbarFreqLeft, TBM_SETRANGE, TRUE, MAKELPARAM(0, 32767)); // Стандартный диапазон
		SendMessageA(hwndTrackbarFreqLeft, TBM_SETPOS, TRUE, LogScalePosition(frequencyLeft, 0, 32767, 1.0, 40000.0));

		// Метка для частоты левого канала
		hwndLabelFreqLeft = CreateWindowExA(0, "STATIC", "Left Frequency: 440.00 Hz",
			WS_CHILD | WS_VISIBLE,
			350, 70, 200, 30,
			hwnd, (HMENU)IDC_LABEL_FREQ_LEFT, g_hInstance, NULL);

		// Текстовое поле для ввода частоты левого канала
		hwndEditFreqLeft = CreateWindowExA(WS_EX_CLIENTEDGE, "EDIT", "440.00",
			WS_CHILD | WS_VISIBLE | ES_NUMBER | ES_AUTOHSCROLL,
			560, 70, 100, 30,
			hwnd, (HMENU)IDC_EDIT_FREQ_LEFT, g_hInstance, NULL);

		// Слайдер (Trackbar) для частоты правого канала
		hwndTrackbarFreqRight = CreateWindowExA(0, TRACKBAR_CLASSA, NULL,
			WS_CHILD | WS_VISIBLE | TBS_AUTOTICKS,
			20, 110, 300, 30,
			hwnd, (HMENU)IDC_TRACKBAR_FREQ_RIGHT, g_hInstance, NULL);
		SendMessageA(hwndTrackbarFreqRight, TBM_SETRANGE, TRUE, MAKELPARAM(0, 32767)); // Стандартный диапазон
		SendMessageA(hwndTrackbarFreqRight, TBM_SETPOS, TRUE, LogScalePosition(frequencyRight, 0, 32767, 1.0, 40000.0));

		// Метка для частоты правого канала
		hwndLabelFreqRight = CreateWindowExA(0, "STATIC", "Right Frequency: 446.00 Hz",
			WS_CHILD | WS_VISIBLE,
			350, 110, 200, 30,
			hwnd, (HMENU)IDC_LABEL_FREQ_RIGHT, g_hInstance, NULL);

		// Текстовое поле для ввода частоты правого канала
		hwndEditFreqRight = CreateWindowExA(WS_EX_CLIENTEDGE, "EDIT", "446.00",
			WS_CHILD | WS_VISIBLE | ES_NUMBER | ES_AUTOHSCROLL,
			560, 110, 100, 30,
			hwnd, (HMENU)IDC_EDIT_FREQ_RIGHT, g_hInstance, NULL);

		// Слайдер (Trackbar) для частоты мерцания
		hwndTrackbarBlinkFreq = CreateWindowExA(0, TRACKBAR_CLASSA, NULL,
			WS_CHILD | WS_VISIBLE | TBS_AUTOTICKS,
			20, 150, 300, 30,
			hwnd, (HMENU)IDC_TRACKBAR_BLINK_FREQ, g_hInstance, NULL);
		SendMessageA(hwndTrackbarBlinkFreq, TBM_SETRANGE, TRUE, MAKELPARAM(1, 10)); // Частота мерцания от 1 до 10 Гц
		SendMessageA(hwndTrackbarBlinkFreq, TBM_SETPOS, TRUE, (LPARAM)(int)blinkFrequency);

		// Метка для частоты мерцания
		hwndLabelBlinkFreq = CreateWindowExA(0, "STATIC", "Blink Frequency: 2 Hz",
			WS_CHILD | WS_VISIBLE,
			350, 150, 200, 30,
			hwnd, (HMENU)IDC_LABEL_BLINK_FREQ, g_hInstance, NULL);

		// Выпадающий список для аудио устройств
		hwndComboBox = CreateWindowExA(0, "COMBOBOX", NULL,
			CBS_DROPDOWNLIST | WS_CHILD | WS_VISIBLE,
			20, 190, 300, 300, // Высота 300 для выпадающего списка
			hwnd, (HMENU)IDC_COMBO_DEVICES, g_hInstance, NULL);

		// Заполнение выпадающего списка аудио устройств с Host API
		deviceList = GetOutputDevicesWithHostAPI();
		for (size_t i = 0; i < deviceList.size(); ++i) {
			const std::string& deviceDesc = deviceList[i].second;
			SendMessageA(hwndComboBox, CB_ADDSTRING, 0, (LPARAM)deviceDesc.c_str());
		}

		if (!deviceList.empty()) {
			SendMessageA(hwndComboBox, CB_SETCURSEL, 0, 0);
		}
		else {
			SendMessageA(hwndButton, WM_SETTEXT, 0, (LPARAM)"Start (No Devices)");
			EnableWindow(hwndButton, FALSE);
		}

		// Создание ComboBox для выбора типа волны левого канала
		hwndComboWaveLeft = CreateWindowExA(0, "COMBOBOX", NULL,
			CBS_DROPDOWNLIST | WS_CHILD | WS_VISIBLE,
			20, 230, 150, 200, // Позиция и размер
			hwnd, (HMENU)IDC_COMBO_WAVE_LEFT, g_hInstance, NULL);

		// Заполнение ComboBox для левого канала
		for (int i = 0; i < WAVE_COUNT; ++i) {
			SendMessageA(hwndComboWaveLeft, CB_ADDSTRING, 0, (LPARAM)waveOptions[i]);
		}
		SendMessageA(hwndComboWaveLeft, CB_SETCURSEL, WAVE_SINE, 0); // Выбор по умолчанию

		// Метка для выбора волны левого канала
		hwndLabelWaveLeft = CreateWindowExA(0, "STATIC", "Left Waveform:",
			WS_CHILD | WS_VISIBLE,
			180, 230, 140, 30,
			hwnd, NULL, g_hInstance, NULL);

		// Создание ComboBox для выбора типа волны правого канала
		hwndComboWaveRight = CreateWindowExA(0, "COMBOBOX", NULL,
			CBS_DROPDOWNLIST | WS_CHILD | WS_VISIBLE,
			20, 270, 150, 200, // Позиция и размер
			hwnd, (HMENU)IDC_COMBO_WAVE_RIGHT, g_hInstance, NULL);

		// Заполнение ComboBox для правого канала
		for (int i = 0; i < WAVE_COUNT; ++i) {
			SendMessageA(hwndComboWaveRight, CB_ADDSTRING, 0, (LPARAM)waveOptions[i]);
		}
		SendMessageA(hwndComboWaveRight, CB_SETCURSEL, WAVE_SINE, 0); // Выбор по умолчанию

		// Метка для выбора волны правого канала
		hwndLabelWaveRight = CreateWindowExA(0, "STATIC", "Right Waveform:",
			WS_CHILD | WS_VISIBLE,
			180, 270, 140, 30,
			hwnd, NULL, g_hInstance, NULL);

		// Установка фокуса для обработки клавиш
		SetFocus(hwnd);
		OutputDebugStringA("Main Window: WM_CREATE completed.\n");

		// Инициализация глобальной переменной для главного окна
		g_hwndMain = hwnd;
	}
	break;

	case WM_COMMAND:
	{
		int wmId = LOWORD(wParam);
		int wmEvent = HIWORD(wParam);
		HWND hwndCtl = (HWND)lParam;

		// Обработка событий из текстовых полей
		if (wmEvent == EN_CHANGE) {
			if (wmId == IDC_EDIT_FREQ_LEFT || wmId == IDC_EDIT_FREQ_RIGHT) {
				int length = GetWindowTextLengthA(hwndCtl);
				if (length == 0) return 0; // Предотвращение выделения памяти при пустом вводе

				char* buffer = new char[length + 1];
				GetWindowTextA(hwndCtl, buffer, length + 1);
				std::string text(buffer);
				delete[] buffer;

				// Проверка, что введено число
				if (!IsNumeric(text)) {
					// Можно показать сообщение об ошибке или просто игнорировать
					MessageBoxA(hwnd, "Пожалуйста, введите корректное число.", "Некорректный ввод", MB_OK | MB_ICONWARNING);
					return 0;
				}

				double newFreq = atof(text.c_str());
				// Валидация диапазона частот
				if (newFreq < 1.0) newFreq = 1.0;
				if (newFreq > 40000.0) newFreq = 40000.0;

				if (wmId == IDC_EDIT_FREQ_LEFT) {
					frequencyLeft = newFreq;
					// Обновление слайдера
					int pos = LogScalePosition(frequencyLeft, 0, 32767, 1.0, 40000.0);
					SendMessageA(hwndTrackbarFreqLeft, TBM_SETPOS, TRUE, pos);
					// Обновление метки
					char label[50];
					sprintf_s(label, "Left Frequency: %.2f Hz", frequencyLeft);
					SetWindowTextA(hwndLabelFreqLeft, label);
					// Обновление структуры для PortAudio
					if (isPlaying && stream != nullptr) {
						waveData.leftFrequency = frequencyLeft;
					}
				}
				else if (wmId == IDC_EDIT_FREQ_RIGHT) {
					frequencyRight = newFreq;
					// Обновление слайдера
					int pos = LogScalePosition(frequencyRight, 0, 32767, 1.0, 40000.0);
					SendMessageA(hwndTrackbarFreqRight, TBM_SETPOS, TRUE, pos);
					// Обновление метки
					char label[50];
					sprintf_s(label, "Right Frequency: %.2f Hz", frequencyRight);
					SetWindowTextA(hwndLabelFreqRight, label);
					// Обновление структуры для PortAudio
					if (isPlaying && stream != nullptr) {
						waveData.rightFrequency = frequencyRight;
					}
				}
			}
		}

		// Обработка событий ComboBox для выбора типа волны
		if (wmEvent == CBN_SELCHANGE) {
			if (wmId == IDC_COMBO_WAVE_LEFT) {
				int sel = SendMessageA(hwndCtl, CB_GETCURSEL, 0, 0);
				if (sel != CB_ERR && sel < WAVE_COUNT) {
					waveData.leftWaveform = static_cast<WaveformType>(sel);
					// Логирование изменения
					std::string log = "Left waveform changed to " + std::string(waveOptions[sel]) + ".\n";
					LogMessage(log);
				}
			}
			else if (wmId == IDC_COMBO_WAVE_RIGHT) {
				int sel = SendMessageA(hwndCtl, CB_GETCURSEL, 0, 0);
				if (sel != CB_ERR && sel < WAVE_COUNT) {
					waveData.rightWaveform = static_cast<WaveformType>(sel);
					// Логирование изменения
					std::string log = "Right waveform changed to " + std::string(waveOptions[sel]) + ".\n";
					LogMessage(log);
				}
			}
		}

		// Обработка событий чекбокса блінкера
		if (wmId == IDC_CHECK_BLINK && wmEvent == BN_CLICKED) {
			LRESULT checkState = SendMessageA(hwndCheckBlink, BM_GETCHECK, 0, 0);
			isBlinkEnabled = (checkState == BST_CHECKED) ? true : false;
			LogMessage(std::string("Blinker enabled: ") + (isBlinkEnabled ? "true\n" : "false\n"));
		}

		// Обработка команд меню и кнопок
		switch (wmId) {
		case ID_FILE_SAVE_PRESET:
		{
			// Диалоговое окно для сохранения файла
			OPENFILENAMEA ofn;
			char szFileName[MAX_PATH] = "preset.txt";
			ZeroMemory(&ofn, sizeof(ofn));
			ofn.lStructSize = sizeof(ofn);
			ofn.hwndOwner = hwnd;
			ofn.lpstrFilter = "Preset Files (*.txt)\0*.txt\0All Files (*.*)\0*.*\0";
			ofn.lpstrFile = szFileName;
			ofn.nMaxFile = MAX_PATH;
			ofn.Flags = OFN_OVERWRITEPROMPT;
			ofn.lpstrDefExt = "txt";

			if (GetSaveFileNameA(&ofn)) {
				if (SavePreset(szFileName, frequencyLeft, frequencyRight, blinkFrequency, isBlinkEnabled, waveData.leftWaveform, waveData.rightWaveform)) {
					MessageBoxA(hwnd, "Пресет успешно сохранён.", "Сохранение Пресета", MB_OK | MB_ICONINFORMATION);
					LogMessage("Preset saved successfully.\n");
				}
				else {
					MessageBoxA(hwnd, "Не удалось сохранить пресет.", "Ошибка", MB_OK | MB_ICONERROR);
					LogMessage("Failed to save preset.\n");
				}
			}
		}
		break;

		case ID_FILE_LOAD_PRESET:
		{
			// Диалоговое окно для открытия файла
			OPENFILENAMEA ofn;
			char szFileName[MAX_PATH] = "";
			ZeroMemory(&ofn, sizeof(ofn));
			ofn.lStructSize = sizeof(ofn);
			ofn.hwndOwner = hwnd;
			ofn.lpstrFilter = "Preset Files (*.txt)\0*.txt\0All Files (*.*)\0*.*\0";
			ofn.lpstrFile = szFileName;
			ofn.nMaxFile = MAX_PATH;
			ofn.Flags = OFN_FILEMUSTEXIST;
			ofn.lpstrDefExt = "txt";

			if (GetOpenFileNameA(&ofn)) {
				double loadedFreqLeft, loadedFreqRight, loadedBlinkFreq;
				bool loadedBlinkEnabled;
				WaveformType loadedWaveLeft, loadedWaveRight;
				if (LoadPreset(szFileName, loadedFreqLeft, loadedFreqRight, loadedBlinkFreq, loadedBlinkEnabled, loadedWaveLeft, loadedWaveRight)) {
					frequencyLeft = loadedFreqLeft;
					frequencyRight = loadedFreqRight;
					blinkFrequency = loadedBlinkFreq;
					isBlinkEnabled = loadedBlinkEnabled;
					waveData.leftWaveform = loadedWaveLeft;
					waveData.rightWaveform = loadedWaveRight;

					// Обновление чекбокса блінкера
					SendMessageA(hwndCheckBlink, BM_SETCHECK, isBlinkEnabled ? BST_CHECKED : BST_UNCHECKED, 0);

					// Обновление слайдеров
					int posLeft = LogScalePosition(frequencyLeft, 0, 32767, 1.0, 40000.0);
					SendMessageA(hwndTrackbarFreqLeft, TBM_SETPOS, TRUE, posLeft);
					int posRight = LogScalePosition(frequencyRight, 0, 32767, 1.0, 40000.0);
					SendMessageA(hwndTrackbarFreqRight, TBM_SETPOS, TRUE, posRight);
					int posBlink = (int)blinkFrequency;
					SendMessageA(hwndTrackbarBlinkFreq, TBM_SETPOS, TRUE, posBlink);

					// Обновление меток
					char labelLeft[50];
					sprintf_s(labelLeft, "Left Frequency: %.2f Hz", frequencyLeft);
					SetWindowTextA(hwndLabelFreqLeft, labelLeft);
					char labelRight[50];
					sprintf_s(labelRight, "Right Frequency: %.2f Hz", frequencyRight);
					SetWindowTextA(hwndLabelFreqRight, labelRight);
					char labelBlink[50];
					sprintf_s(labelBlink, "Blink Frequency: %.0f Hz", blinkFrequency);
					SetWindowTextA(hwndLabelBlinkFreq, labelBlink);

					// Обновление текстовых полей
					char textLeft[20];
					sprintf_s(textLeft, "%.2f", frequencyLeft);
					SetWindowTextA(hwndEditFreqLeft, textLeft);
					char textRight[20];
					sprintf_s(textRight, "%.2f", frequencyRight);
					SetWindowTextA(hwndEditFreqRight, textRight);

					// Обновление выборов типов волн
					SendMessageA(hwndComboWaveLeft, CB_SETCURSEL, waveData.leftWaveform, 0);
					SendMessageA(hwndComboWaveRight, CB_SETCURSEL, waveData.rightWaveform, 0);

					MessageBoxA(hwnd, "Пресет успешно загружен.", "Загрузка Пресета", MB_OK | MB_ICONINFORMATION);
					LogMessage("Preset loaded successfully.\n");
				}
				else {
					MessageBoxA(hwnd, "Не удалось загрузить пресет.", "Ошибка", MB_OK | MB_ICONERROR);
					LogMessage("Failed to load preset.\n");
				}
			}
		}
		break;

		case ID_HELP_ABOUT:
		{
			// Отображение окна "О программе"
			std::string aboutText = "Brain Wave Generator\n\nРазработчик: PsyShout\nВерсия: 1.2\n\nСоздано для входа в различные\nсостояния сознания с помощью\nбинауральных биений.";
			MessageBoxA(hwnd, aboutText.c_str(), "О программе", MB_OK | MB_ICONINFORMATION);
			LogMessage("Displayed About dialog.\n");
		}
		break;

		case IDC_BUTTON_STARTSTOP:
		{
			// Обработка кнопки Start/Stop
			if (!isPlaying)
			{
				LogMessage("Start button clicked.\n");

				// Получение выбранного устройства
				int sel = SendMessageA(hwndComboBox, CB_GETCURSEL, 0, 0);
				if (sel == CB_ERR || deviceList.empty()) {
					MessageBoxA(hwnd, "Нет доступных аудио устройств.", "Ошибка", MB_OK | MB_ICONERROR);
					return 0;
				}

				int deviceIndex = deviceList[sel].first;
				const PaDeviceInfo* deviceInfo = Pa_GetDeviceInfo(deviceIndex);
				if (!deviceInfo) {
					MessageBoxA(hwnd, "Не удалось получить информацию об аудио устройстве.", "Ошибка", MB_OK | MB_ICONERROR);
					return 0;
				}

				PaStreamParameters outputParameters;
				outputParameters.device = deviceIndex;
				outputParameters.channelCount = 2; // Стерео
				outputParameters.sampleFormat = paFloat32;
				outputParameters.suggestedLatency = deviceInfo->defaultLowOutputLatency;
				outputParameters.hostApiSpecificStreamInfo = NULL;

				// Инициализация структуры WaveData
				waveData.leftFrequency = frequencyLeft;
				waveData.leftPhase = 0.0;
				waveData.rightFrequency = frequencyRight;
				waveData.rightPhase = 0.0;

				// Открытие потока
				PaError err = Pa_OpenStream(
					&stream,
					NULL, // Нет входных каналов
					&outputParameters,
					44100,
					256,
					paNoFlag,
					paCallback,
					&waveData
				);

				if (err != paNoError) {
					std::string errorMsg = "Не удалось открыть поток PortAudio: ";
					errorMsg += Pa_GetErrorText(err);
					MessageBoxA(hwnd, errorMsg.c_str(), "PortAudio Ошибка", MB_OK | MB_ICONERROR);
					LogMessage("Failed to open PortAudio stream.\n");
					return 0;
				}
				else {
					LogMessage("PortAudio stream opened successfully.\n");
				}

				// Запуск потока
				err = Pa_StartStream(stream);
				if (err != paNoError) {
					std::string errorMsg = "Не удалось запустить поток PortAudio: ";
					errorMsg += Pa_GetErrorText(err);
					MessageBoxA(hwnd, errorMsg.c_str(), "PortAudio Ошибка", MB_OK | MB_ICONERROR);
					Pa_CloseStream(stream);
					stream = nullptr;
					LogMessage("Failed to start PortAudio stream.\n");
					return 0;
				}
				else {
					LogMessage("PortAudio stream started successfully.\n");
				}

				isPlaying = true;
				SetWindowTextA(hwndButton, "Stop");
				LogMessage("Playback started.\n");

				// Проверка, включен ли блінкер
				if (isBlinkEnabled) {
					// Создание дополнительного окна для мерцания

					// Регистрация класса окна, если еще не зарегистрирован
					if (!isBlinkClassRegistered) {
						const char BCLASS_NAME[] = "BlinkWindowClass";
						WNDCLASSEXA wcBlink = { 0 };
						wcBlink.cbSize = sizeof(WNDCLASSEXA);
						wcBlink.style = CS_HREDRAW | CS_VREDRAW;
						wcBlink.lpfnWndProc = BlinkWindowProc;
						wcBlink.hInstance = g_hInstance;
						wcBlink.hCursor = LoadCursorA(NULL, IDC_ARROW);
						wcBlink.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1); // Начальный цвет будет перерисовываться в WM_PAINT
						wcBlink.lpszClassName = BCLASS_NAME;

						// Проверка, зарегистрирован ли класс окна уже
						WNDCLASSEXA existingWc = { 0 };
						if (!GetClassInfoExA(g_hInstance, BCLASS_NAME, &existingWc)) {
							if (!RegisterClassExA(&wcBlink)) {
								MessageBoxA(hwnd, "Не удалось зарегистрировать класс дополнительного окна.", "Ошибка", MB_OK | MB_ICONERROR);
								// Остановка воспроизведения при ошибке регистрации класса
								Pa_StopStream(stream);
								Pa_CloseStream(stream);
								stream = nullptr;
								isPlaying = false;
								SetWindowTextA(hwndButton, "Start");
								LogMessage("Failed to register BlinkWindowClass.\n");
								return 0;
							}
							else {
								LogMessage("BlinkWindowClass registered successfully.\n");
								isBlinkClassRegistered = true;
							}
						}
					}

					// Получение размеров виртуального экрана (все мониторы)
					int screenWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
					int screenHeight = GetSystemMetrics(SM_CYVIRTUALSCREEN);
					int screenLeft = GetSystemMetrics(SM_XVIRTUALSCREEN);
					int screenTop = GetSystemMetrics(SM_YVIRTUALSCREEN);

					// Задание размеров полного экрана
					int blinkWidth = screenWidth;
					int blinkHeight = screenHeight;
					int blinkX = screenLeft;
					int blinkY = screenTop;

					// Создание дополнительного окна с расширенными стилями WS_EX_TOPMOST | WS_EX_APPWINDOW
					hwndBlink = CreateWindowExA(
						WS_EX_TOPMOST | WS_EX_APPWINDOW, // Добавлен WS_EX_APPWINDOW
						"BlinkWindowClass",                // Имя класса окна
						NULL,                              // Заголовок окна (можно оставить пустым)
						WS_POPUP,                          // Стиль окна (без рамок и заголовка)
						blinkX, blinkY,                    // Позиция
						blinkWidth, blinkHeight,           // Размер окна (полноэкранное)
						NULL,                              // Родительское окно изменено на NULL
						NULL,                              // Меню
						g_hInstance,                       // Дескриптор приложения
						NULL                               // Дополнительные данные
					);

					if (hwndBlink == NULL) {
						MessageBoxA(hwnd, "Не удалось создать дополнительное окно.", "Ошибка", MB_OK | MB_ICONERROR);
						Pa_StopStream(stream);
						Pa_CloseStream(stream);
						stream = nullptr;
						isPlaying = false;
						SetWindowTextA(hwndButton, "Start");
						LogMessage("Failed to create blink window.\n");
						return 0;
					}

					ShowWindow(hwndBlink, SW_SHOW);
					UpdateWindow(hwndBlink); // Обновление окна для немедленного отображения
					SetForegroundWindow(hwndBlink); // Установка окна как активного
					SetFocus(hwndBlink); // Установка фокуса клавиатуры на окно мерцания
					LogMessage("Blink window created and shown with focus.\n");

					// Установка таймера для мерцания в дополнительном окне
					int interval = (int)(1000.0 / (blinkFrequency * 2)); // Полупериод в миллисекундах
					SetTimer(hwndBlink, TIMER_BLINK_EXTRA_ID, interval, NULL);
					LogMessage("Blink timer set.\n");
				}
			}
			else
			{
				LogMessage("Stop button clicked.\n");

				// Остановка воспроизведения
				Pa_StopStream(stream);
				Pa_CloseStream(stream);
				stream = nullptr;
				isPlaying = false;
				SetWindowTextA(hwndButton, "Start");
				LogMessage("Playback stopped.\n");

				// Закрытие дополнительного окна, если оно существует
				if (hwndBlink != NULL) {
					DestroyWindow(hwndBlink);
					hwndBlink = NULL;
					LogMessage("Blink window destroyed.\n");
				}
			}
		}
		break;

		case WM_STOP_PLAYBACK:
		{
			OutputDebugStringA("Main Window: WM_STOP_PLAYBACK received.\n");
			// Обработка остановки воспроизведения и закрытия мерцающего окна
			if (isPlaying) {
				// Остановка воспроизведения
				Pa_StopStream(stream);
				Pa_CloseStream(stream);
				stream = nullptr;
				isPlaying = false;
				SetWindowTextA(hwndButton, "Start");
				LogMessage("Playback stopped via WM_STOP_PLAYBACK.\n");

				// Закрытие дополнительного окна, если оно существует
				if (hwndBlink != NULL) {
					DestroyWindow(hwndBlink);
					hwndBlink = NULL;
					LogMessage("Blink window destroyed via WM_STOP_PLAYBACK.\n");
				}
			}
		}
		break;

		default:
			break;
		}
	}
	break;

	case WM_HSCROLL:
	{
		// Обработка изменения слайдеров
		if ((HWND)lParam == hwndTrackbarFreqLeft) {
			// Получение текущей позиции слайдера
			int pos = SendMessageA(hwndTrackbarFreqLeft, TBM_GETPOS, 0, 0);
			double newFrequencyLeft = LogScale(pos, 0, 32767, 1.0, 40000.0);

			// Проверка, зажата ли клавиша Shift
			bool isShiftPressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;

			if (isShiftPressed) {
				// Более точное изменение (например, 10% от изменения)
				double delta = newFrequencyLeft - frequencyLeft;
				frequencyLeft += delta * 0.1; // Уменьшаем изменение на 90%
				if (frequencyLeft < 1.0) frequencyLeft = 1.0;
				if (frequencyLeft > 40000.0) frequencyLeft = 40000.0;
			}
			else {
				frequencyLeft = newFrequencyLeft;
			}

			// Обновление метки с частотой
			char label[50];
			sprintf_s(label, "Left Frequency: %.2f Hz", frequencyLeft);
			SetWindowTextA(hwndLabelFreqLeft, label);

			// Обновление текста в текстовом поле
			char text[20];
			sprintf_s(text, "%.2f", frequencyLeft);
			SetWindowTextA(hwndEditFreqLeft, text);

			if (isPlaying && stream != nullptr) {
				// Обновление частоты левого канала в реальном времени
				waveData.leftFrequency = frequencyLeft;
				LogMessage("Left frequency updated via slider.\n");
			}
		}
		else if ((HWND)lParam == hwndTrackbarFreqRight) {
			// Получение текущей позиции слайдера
			int pos = SendMessageA(hwndTrackbarFreqRight, TBM_GETPOS, 0, 0);
			double newFrequencyRight = LogScale(pos, 0, 32767, 1.0, 40000.0);

			// Проверка, зажата ли клавиша Shift
			bool isShiftPressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;

			if (isShiftPressed) {
				// Более точное изменение (например, 10% от изменения)
				double delta = newFrequencyRight - frequencyRight;
				frequencyRight += delta * 0.1; // Уменьшаем изменение на 90%
				if (frequencyRight < 1.0) frequencyRight = 1.0;
				if (frequencyRight > 40000.0) frequencyRight = 40000.0;
			}
			else {
				frequencyRight = newFrequencyRight;
			}

			// Обновление метки с частотой
			char label[50];
			sprintf_s(label, "Right Frequency: %.2f Hz", frequencyRight);
			SetWindowTextA(hwndLabelFreqRight, label);

			// Обновление текста в текстовом поле
			char text[20];
			sprintf_s(text, "%.2f", frequencyRight);
			SetWindowTextA(hwndEditFreqRight, text);

			if (isPlaying && stream != nullptr) {
				// Обновление частоты правого канала в реальном времени
				waveData.rightFrequency = frequencyRight;
				LogMessage("Right frequency updated via slider.\n");
			}
		}
		else if ((HWND)lParam == hwndTrackbarBlinkFreq) {
			// Получение текущей позиции слайдера
			int pos = SendMessageA(hwndTrackbarBlinkFreq, TBM_GETPOS, 0, 0);
			blinkFrequency = (double)pos;
			char label[50];
			sprintf_s(label, "Blink Frequency: %.0f Hz", blinkFrequency);
			SetWindowTextA(hwndLabelBlinkFreq, label);
			LogMessage("Blink frequency updated via slider.\n");

			if (isPlaying && hwndBlink != NULL) {
				// Обновление интервала таймера для мерцания
				KillTimer(hwndBlink, TIMER_BLINK_EXTRA_ID);
				int interval = (int)(1000.0 / (blinkFrequency * 2)); // Полупериод в миллисекундах
				SetTimer(hwndBlink, TIMER_BLINK_EXTRA_ID, interval, NULL);
				LogMessage("Blink timer interval updated.\n");
			}
		}
	}
	break;

	case WM_DESTROY:
	{
		if (isPlaying && stream != nullptr) {
			Pa_StopStream(stream);
			Pa_CloseStream(stream);
			stream = nullptr;
			LogMessage("Playback stopped during WM_DESTROY.\n");
		}
		Pa_Terminate();
		PostQuitMessage(0);
	}
	break;
	default:
		return DefWindowProcA(hwnd, uMsg, wParam, lParam);
	}
	return 0;
}

// Функция регистрации класса окна
ATOM RegisterWindowClassA(HINSTANCE hInstance, const char* className)
{
	WNDCLASSEXA wc = { 0 };
	wc.cbSize = sizeof(WNDCLASSEXA);
	wc.style = CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc = MainWindowProc; // Используйте MainWindowProc вместо WindowProc
	wc.hInstance = hInstance;
	wc.hCursor = LoadCursorA(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1); // Белый фон
	wc.lpszClassName = className;

	return RegisterClassExA(&wc);
}

// Точка входа
int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE, LPSTR, int nCmdShow)
{
	g_hInstance = hInstance; // Инициализация глобальной переменной

	// Инициализация PortAudio
	PaError err = Pa_Initialize();
	if (err != paNoError) {
		MessageBoxA(NULL, "Не удалось инициализировать PortAudio.", "PortAudio Ошибка", MB_OK | MB_ICONERROR);
		return -1;
	}

	// Регистрация класса основного окна
	const char CLASS_NAME[] = "BrainWaveGeneratorWindowClass";
	if (!RegisterWindowClassA(g_hInstance, CLASS_NAME)) {
		Pa_Terminate();
		return -1;
	}

	// Создание окна
	HWND hwnd = CreateWindowExA(
		0,                              // Расширенные стили окна
		CLASS_NAME,                     // Имя класса окна
		"Brain Wave Generator",        // Заголовок окна
		WS_OVERLAPPEDWINDOW,            // Стиль окна

		// Позиция и размер окна
		CW_USEDEFAULT, CW_USEDEFAULT, 700, 370,

		NULL,       // Родительское окно
		NULL,       // Меню (уже создано вручную)
		g_hInstance,  // Дескриптор приложения
		NULL        // Дополнительные данные
	);

	if (hwnd == NULL)
	{
		Pa_Terminate();
		return 0;
	}

	ShowWindow(hwnd, nCmdShow);
	LogMessage("Main window created and shown.\n");

	// Цикл сообщений
	MSG msg = { };
	while (GetMessageA(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessageA(&msg);
	}

	return 0;
}
