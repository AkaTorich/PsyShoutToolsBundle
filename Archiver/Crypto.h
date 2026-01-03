#pragma once
#include "Common.h"

std::vector<uint8_t> combineKeys(const std::vector<std::wstring>& keyfiles);
std::vector<uint8_t> aesEncrypt(const std::vector<uint8_t>& data, const std::vector<uint8_t>& key, std::vector<uint8_t>& iv);
std::vector<uint8_t> aesDecrypt(const std::vector<uint8_t>& data, const std::vector<uint8_t>& key, const std::vector<uint8_t>& iv);