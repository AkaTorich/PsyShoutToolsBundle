#include "Crypto.h"
#include "FileOperations.h"

std::vector<uint8_t> combineKeys(const std::vector<std::wstring>& keyfiles) {
    std::vector<uint8_t> combinedKey;
    for (const auto& keyfile : keyfiles) {
        std::vector<uint8_t> keyPart = readFile(keyfile);
        combinedKey.insert(combinedKey.end(), keyPart.begin(), keyPart.end());
    }

    HCRYPTPROV hProv = NULL;
    HCRYPTHASH hHash = NULL;
    DWORD cbHash = 0;
    BYTE* pbHash = NULL;

    if (!CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT)) {
        throw RuntimeErrorW(L"Failed to initialize crypto provider");
    }

    if (!CryptCreateHash(hProv, CALG_SHA_256, 0, 0, &hHash)) {
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to create hash object");
    }

    if (!CryptHashData(hHash, combinedKey.data(), static_cast<DWORD>(combinedKey.size()), 0)) {
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to compute hash");
    }

    if (!CryptGetHashParam(hHash, HP_HASHVAL, NULL, &cbHash, 0)) {
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to get hash size");
    }

    pbHash = new BYTE[cbHash];
    if (!CryptGetHashParam(hHash, HP_HASHVAL, pbHash, &cbHash, 0)) {
        delete[] pbHash;
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to get hash value");
    }

    std::vector<uint8_t> hash(pbHash, pbHash + cbHash);

    delete[] pbHash;
    CryptDestroyHash(hHash);
    CryptReleaseContext(hProv, 0);

    return hash;
}

std::vector<uint8_t> aesEncrypt(const std::vector<uint8_t>& data, const std::vector<uint8_t>& key, std::vector<uint8_t>& iv) {
    HCRYPTPROV hProv = NULL;
    HCRYPTKEY hKey = NULL;
    HCRYPTHASH hHash = NULL;
    std::vector<uint8_t> encryptedData = data;
    DWORD dataSize = static_cast<DWORD>(encryptedData.size());

    if (!CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT)) {
        throw RuntimeErrorW(L"Failed to initialize crypto provider");
    }

    if (!CryptCreateHash(hProv, CALG_SHA_256, 0, 0, &hHash)) {
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to create hash object");
    }

    if (!CryptHashData(hHash, key.data(), static_cast<DWORD>(key.size()), 0)) {
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to hash key");
    }

    if (!CryptDeriveKey(hProv, CALG_AES_256, hHash, 0, &hKey)) {
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to derive key");
    }

    CryptDestroyHash(hHash);

    DWORD blockLen = 16;
    iv.resize(blockLen);
    if (!CryptGenRandom(hProv, blockLen, iv.data())) {
        CryptDestroyKey(hKey);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to generate IV");
    }

    if (!CryptSetKeyParam(hKey, KP_IV, iv.data(), 0)) {
        CryptDestroyKey(hKey);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to set IV");
    }

    DWORD bufferSize = dataSize;
    if (!CryptEncrypt(hKey, 0, TRUE, 0, NULL, &bufferSize, 0)) {
        CryptDestroyKey(hKey);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Error calculating encryption size");
    }

    encryptedData.resize(bufferSize);
    dataSize = static_cast<DWORD>(data.size());
    if (!CryptEncrypt(hKey, 0, TRUE, 0, encryptedData.data(), &dataSize, bufferSize)) {
        CryptDestroyKey(hKey);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Error encrypting data");
    }

    encryptedData.resize(dataSize);

    CryptDestroyKey(hKey);
    CryptReleaseContext(hProv, 0);

    return encryptedData;
}

std::vector<uint8_t> aesDecrypt(const std::vector<uint8_t>& data, const std::vector<uint8_t>& key, const std::vector<uint8_t>& iv) {
    HCRYPTPROV hProv = NULL;
    HCRYPTKEY hKey = NULL;
    HCRYPTHASH hHash = NULL;
    std::vector<uint8_t> decryptedData = data;
    DWORD dataSize = static_cast<DWORD>(decryptedData.size());

    if (!CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT)) {
        throw RuntimeErrorW(L"Failed to initialize crypto provider");
    }

    if (!CryptCreateHash(hProv, CALG_SHA_256, 0, 0, &hHash)) {
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to create hash object");
    }

    if (!CryptHashData(hHash, key.data(), static_cast<DWORD>(key.size()), 0)) {
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to hash key");
    }

    if (!CryptDeriveKey(hProv, CALG_AES_256, hHash, 0, &hKey)) {
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to derive key");
    }

    CryptDestroyHash(hHash);

    if (!CryptSetKeyParam(hKey, KP_IV, (BYTE*)iv.data(), 0)) {
        CryptDestroyKey(hKey);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Failed to set IV");
    }

    if (!CryptDecrypt(hKey, 0, TRUE, 0, decryptedData.data(), &dataSize)) {
        CryptDestroyKey(hKey);
        CryptReleaseContext(hProv, 0);
        throw RuntimeErrorW(L"Error decrypting data");
    }

    decryptedData.resize(dataSize);

    CryptDestroyKey(hKey);
    CryptReleaseContext(hProv, 0);

    return decryptedData;
}