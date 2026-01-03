#include "Compression.h"

static int LzmaInputCallback(void* ctx, void* buf, size_t* size) {
    LzmaInCtx* in = reinterpret_cast<LzmaInCtx*>(ctx);
    size_t toRead = (*size < in->remaining) ? *size : in->remaining;
    if (toRead > 0) {
        std::memcpy(buf, in->data, toRead);
        in->data += toRead;
        in->remaining -= toRead;
    }
    *size = toRead;
    return 0;
}

static size_t LzmaOutputCallback(void* ctx, const void* buf, size_t size) {
    std::vector<uint8_t>* out = reinterpret_cast<std::vector<uint8_t>*>(ctx);
    if (size > 0) {
        size_t old = out->size();
        out->resize(old + size);
        std::memcpy(out->data() + old, buf, size);
    }
    return size;
}

std::vector<uint8_t> lzmaCompressBuffer(const std::vector<uint8_t>& data) {
    if (data.empty()) return std::vector<uint8_t>();

    elzma_compress_handle hand = elzma_compress_alloc();
    if (!hand) {
        throw RuntimeErrorW(L"LZMA error: failed to create compression handler");
    }

    unsigned int dict = elzma_get_dict_size(static_cast<unsigned long long>(data.size()));
    int rc = elzma_compress_config(hand,
        ELZMA_LC_DEFAULT,
        ELZMA_LP_DEFAULT,
        ELZMA_PB_DEFAULT,
        5,
        dict,
        ELZMA_lzma,
        static_cast<unsigned long long>(data.size()));
    if (rc != ELZMA_E_OK) {
        elzma_compress_free(&hand);
        throw RuntimeErrorW(L"LZMA error: failed to configure compression, rc=" + std::to_wstring(rc));
    }

    struct InCtx inCtx{ reinterpret_cast<const unsigned char*>(data.data()), data.size() };
    std::vector<uint8_t> out;

    rc = elzma_compress_run(hand,
        LzmaInputCallback, &inCtx,
        LzmaOutputCallback, &out,
        NULL, NULL);

    elzma_compress_free(&hand);

    if (rc != ELZMA_E_OK) {
        throw RuntimeErrorW(L"LZMA compression error, rc=" + std::to_wstring(rc));
    }

    return out;
}

std::vector<uint8_t> lzmaDecompressBuffer(const std::vector<uint8_t>& data) {
    if (data.empty()) return std::vector<uint8_t>();

    elzma_decompress_handle hand = elzma_decompress_alloc();
    if (!hand) {
        throw RuntimeErrorW(L"LZMA error: failed to create decompression handler");
    }

    struct InCtx inCtx{ reinterpret_cast<const unsigned char*>(data.data()), data.size() };
    std::vector<uint8_t> out;

    int rc = elzma_decompress_run(hand,
        LzmaInputCallback, &inCtx,
        LzmaOutputCallback, &out,
        ELZMA_lzma);

    elzma_decompress_free(&hand);

    if (rc != ELZMA_E_OK) {
        throw RuntimeErrorW(L"LZMA decompression error, rc=" + std::to_wstring(rc));
    }

    return out;
}