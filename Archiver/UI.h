#pragma once
#include "Common.h"

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
void UpdateListView(HWND hListView);
void StartEncryption(HWND hwnd);
void StartDecryption(HWND hwnd);