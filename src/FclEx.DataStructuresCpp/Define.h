#pragma once

#include <cstdint>

#ifdef _DEBUG
#include <cstdio>
#include <cassert>
#endif

using sbyte = int8_t;
using byte = uint8_t;
using ushort = uint16_t;
using uint = uint32_t;
using ulong = uint64_t;

using Int8 = int8_t;
using Int16 = int16_t;
using Int32 = int32_t;
using Int64 = int64_t;

using UInt8 = uint8_t;
using UInt16 = uint16_t;
using UInt32 = uint32_t;
using UInt64 = uint64_t;

using SizeType = size_t;

#define var auto
#define null nullptr

#define default(type) type{}

#define nameof(v) #v