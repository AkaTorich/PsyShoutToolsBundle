#pragma once
#include "Common.h"

struct LzmaInCtx {
    const unsigned char* data;
    size_t remaining;
};

struct InCtx {
    const unsigned char* data;
    size_t remaining;
};

std::vector<uint8_t> lzmaCompressBuffer(const std::vector<uint8_t>& data);
std::vector<uint8_t> lzmaDecompressBuffer(const std::vector<uint8_t>& data);