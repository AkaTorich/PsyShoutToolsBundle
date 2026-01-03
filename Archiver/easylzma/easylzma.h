#pragma once


#ifndef _EASYLZMA_H
#define _EASYLZMA_H

#include "compress.h"
#include "decompress.h"

#ifdef __cplusplus
extern "C" {
#endif

/* compress a chunk of memory and return a dynamically allocated buffer
 * if successful.  return value is an easylzma error code */
int simpleCompress(elzma_file_format format,
    const unsigned char* inData,
    unsigned int inLen,
    unsigned char** outData,
    unsigned int* outLen);

/* decompress a chunk of memory and return a dynamically allocated buffer
 * if successful.  return value is an easylzma error code */
int simpleDecompress(elzma_file_format format,
    const unsigned char* inData,
    unsigned int inLen,
    unsigned char** outData,
    unsigned int* outLen);

/* compress with explicit params (level, dict bytes) */
int simpleCompressWithParams(elzma_file_format format,
    const unsigned char* inData,
    unsigned int inLen,
    unsigned char** outData,
    unsigned int* outLen,
    unsigned int level,
    unsigned int dictBytes);

#ifdef __cplusplus
}
#endif


#endif // !_EASYLZMA_H
