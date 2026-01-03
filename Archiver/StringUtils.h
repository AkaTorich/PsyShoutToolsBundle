#pragma once
#include "Common.h"

std::string WideStringToUTF8(const std::wstring& wstr);
std::wstring UTF8ToWideString(const std::string& str);