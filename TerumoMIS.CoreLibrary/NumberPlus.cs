﻿//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: NumberPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  NumberPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 16:46:00
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System.Globalization;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 数值相关扩展操作
    /// </summary>
    public unsafe static class NumberPlus
    {
        #region 转字符串(用于代码生成)
        /// <summary>
        /// 16位除以10转乘法的乘数
        /// </summary>
        public const uint Div1016Mul = ((1 << 19) + 9) / 10;
        /// <summary>
        /// 16位除以10转乘法的位移
        /// </summary>
        public const int Div1016Shift = 19;
        //public const int Div100_16Mul = ((1 << 22) + 99) / 100;
        //public const int Div100_16Shift = 22;
        /// <summary>
        /// 32位除以10000转乘法的乘数
        /// </summary>
        public const ulong Div10000Mul = ((1L << 45) + 9999) / 10000;
        /// <summary>
        /// 32位除以10000转乘法的位移
        /// </summary>
        public const int Div10000Shift = 45;
        /// <summary>
        /// 32位除以100000000转乘法的乘数
        /// </summary>
        public const ulong Div100000000Mul = ((1L << 58) + 99999999) / 100000000;
        /// <summary>
        /// 32位除以100000000转乘法的位移
        /// </summary>
        public const int Div100000000Shift = 58;
        /// <summary>
        /// 获取除法转乘法的乘数与位移
        /// </summary>
        /// <param name="value">除数</param>
        /// <returns>乘数与位移</returns>
        public static KeyValueStruct<uint, int> DivShift(ushort value)
        {
            var divMul = uint.MaxValue / value;
            var shift = divMul.Bits() - 16;
            divMul >>= shift;
            shift ^= 31;
            return new KeyValueStruct<uint, int>(++divMul, ++shift);
        }
        /// <summary>
        /// 获取除法转乘法的乘数与位移
        /// </summary>
        /// <param name="value">除数</param>
        /// <returns>乘数与位移</returns>
        public static KeyValueStruct<ulong, int> DivShift(uint value)
        {
            var divMul = ulong.MaxValue / value;
            var shift = ((uint)(divMul >> 32)).Bits();
            divMul >>= shift;
            shift ^= 63;
            return new KeyValueStruct<ulong, int>(++divMul, ++shift);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>字符串</returns>
        public static string ToString(this byte value)
        {
            char* chars = stackalloc char[3];
            return new string(chars, 0, ToString(value, chars));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="charStream">字符流</param>
        internal static void ToString(byte value, CharStreamPlus charStream)
        {
            charStream.PrepLength(3);
            charStream.Unsafer.AddLength(ToString(value, charStream.CurrentChar));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>字符串长度</returns>
        private static int ToString(byte value, char* chars)
        {
            if (value >= 100)
            {
                var value10 = (value * (int)Div1016Mul) >> Div1016Shift;
                chars[2] = (char)((value - value10 * 10) + '0');
                var value100 = (value10 * (int)Div1016Mul) >> Div1016Shift;
                *(int*)chars = ((value10 - value100 * 10) << 16) | value100 | 0x300030;
                return 3;
            }
            if (value >= 10)
            {
                var value10 = (value * (int)Div1016Mul) >> Div1016Shift;
                *(int*)chars = ((value - value10 * 10) << 16) | value10 | 0x300030;
                return 2;
            }
            *chars = (char)(value + '0');
            return 1;
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>字符串</returns>
        public static string ToString(this sbyte value)
        {
            char* chars = stackalloc char[4];
            return new string(chars, 0, ToString(value, chars));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="charStream">字符流</param>
        internal static void ToString(sbyte value, CharStreamPlus charStream)
        {
            charStream.PrepLength(4);
            charStream.Unsafer.AddLength(ToString(value, charStream.Char + charStream.Length));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>字符串长度</returns>
        private static int ToString(sbyte value, char* chars)
        {
            if (value >= 0)
            {
                if (value >= 100)
                {
                    value -= 100;
                    *chars = '1';
                    var value10 = (value * (int)Div1016Mul) >> Div1016Shift;
                    *(int*)(chars + 1) = ((value - value10 * 10) << 16) | value10 | 0x300030;
                    return 3;
                }
                if (value >= 10)
                {
                    var value10 = (value * (int)Div1016Mul) >> Div1016Shift;
                    *(int*)chars = ((value - value10 * 10) << 16) | value10 | 0x300030;
                    return 2;
                }
                *chars = (char)(value + '0');
                return 1;
            }
            var value32 = -value;
            if (value32 >= 100)
            {
                value32 -= 100;
                *(int*)chars = '-' + ('1' << 16);
                var value10 = (value32 * (int)Div1016Mul) >> Div1016Shift;
                *(int*)(chars + 2) = ((value32 - value10 * 10) << 16) | value10 | 0x300030;
                return 4;
            }
            if (value32 >= 10)
            {
                *chars = '-';
                var value10 = (value32 * (int)Div1016Mul) >> Div1016Shift;
                *(int*)(chars + 1) = ((value32 - value10 * 10) << 16) | value10 | 0x300030;
                return 3;
            }
            *(int*)chars = '-' + ((value32 + '0') << 16);
            return 2;
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>字符串</returns>
        public static string ToString(this ushort value)
        {
            char* chars = stackalloc char[5];
            return new string(chars, 0, ToString(value, chars));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="charStream">字符流</param>
        internal static void ToString(ushort value, CharStreamPlus charStream)
        {
            charStream.PrepLength(5);
            charStream.Unsafer.AddLength(ToString(value, charStream.CurrentChar));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>字符串长度</returns>
        private static int ToString(ushort value, char* chars)
        {
            if (value >= 10000)
            {
                var value10 = (int)(value * Div1016Mul >> Div1016Shift);
                var value100 = (value10 * (int)Div1016Mul) >> Div1016Shift;
                *(int*)(chars + 3) = ((value - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030;
                value10 = (value100 * (int)Div1016Mul) >> Div1016Shift;
                value = (ushort)((value10 * Div1016Mul) >> Div1016Shift);
                *(int*)(chars + 1) = ((value100 - value10 * 10) << 16) | (value10 - value * 10) | 0x300030;
                *chars = (char)(value + '0');
                return 5;
            }
            if (value >= 100)
            {
                var value10 = (value * (int)Div1016Mul) >> Div1016Shift;
                if (value >= 1000)
                {
                    var value100 = (value10 * (int)Div1016Mul) >> Div1016Shift;
                    *(int*)(chars + 2) = ((value - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030;
                    value10 = (value100 * (int)Div1016Mul) >> Div1016Shift;
                    *(int*)chars = ((value100 - value10 * 10) << 16) | value10 | 0x300030;
                    return 4;
                }
                else
                {
                    chars[2] = (char)((value - value10 * 10) + '0');
                    int value100 = (value10 * (int)Div1016Mul) >> Div1016Shift;
                    *(int*)chars = ((value10 - value100 * 10) << 16) | value100 | 0x300030;
                    return 3;
                }
            }
            if (value >= 10)
            {
                var value10 = (value * (int)Div1016Mul) >> Div1016Shift;
                *(int*)chars = ((value - value10 * 10) << 16) | value10 | 0x300030;
                return 2;
            }
            *chars = (char)(value + '0');
            return 1;
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>字符串</returns>
        public static string ToString(this short value)
        {
            char* chars = stackalloc char[6];
            return new string(chars, 0, ToString(value, chars));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="charStream">字符流</param>
        internal static void ToString(short value, CharStreamPlus charStream)
        {
            charStream.PrepLength(6);
            charStream.Unsafer.AddLength(ToString(value, charStream.CurrentChar));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>字符串长度</returns>
        private static int ToString(short value, char* chars)
        {
            if (value >= 0) return ToString((ushort)value, chars);
            var value32 = -value;
            if (value32 >= 10000)
            {
                var value10 = (int)((uint)(value32 * Div1016Mul) >> Div1016Shift);
                var value100 = (value10 * (int)Div1016Mul) >> Div1016Shift;
                *(int*)(chars + 4) = ((value32 - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030;
                value10 = (value100 * (int)Div1016Mul) >> Div1016Shift;
                value32 = (value10 * (int)Div1016Mul) >> Div1016Shift;
                *(int*)(chars + 2) = ((value100 - value10 * 10) << 16) | (value10 - value32 * 10) | 0x300030;
                *(int*)chars = '-' + ((value32 + '0') << 16);
                return 6;
            }
            if (value32 >= 100)
            {
                if (value32 >= 1000)
                {
                    *chars = '-';
                    var value10 = (value32 * (int)Div1016Mul) >> Div1016Shift;
                    var value100 = (value10 * (int)Div1016Mul) >> Div1016Shift;
                    *(int*)(chars + 3) = ((value32 - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030;
                    value10 = (value100 * (int)Div1016Mul) >> Div1016Shift;
                    *(int*)(chars + 1) = ((value100 - value10 * 10) << 16) | value10 | 0x300030;
                    return 5;
                }
                else
                {
                    var value10 = (value32 * (int)Div1016Mul) >> Div1016Shift;
                    var value100 = (value10 * (int)Div1016Mul) >> Div1016Shift;
                    *(int*)(chars + 2) = ((value32 - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030;
                    *(int*)chars = '-' + ((value100 + '0') << 16);
                    return 4;
                }
            }
            if (value32 >= 10)
            {
                *chars = '-';
                var value10 = (value32 * (int)Div1016Mul) >> Div1016Shift;
                *(int*)(chars + 1) = ((value32 - value10 * 10) << 16) | value10 | 0x300030;
                return 3;
            }
            *(int*)chars = '-' + ((value32 + '0') << 16);
            return 2;
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>字符串</returns>
        public static string ToString(this uint value)
        {
            char* chars = stackalloc char[10];
            KeyValueStruct<int, int> index = ToString(value, chars);
            return new string(chars + index.Key, 0, index.Value);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="charStream">字符流</param>
        internal static void ToString(uint value, CharStreamPlus charStream)
        {
            char* chars = stackalloc char[10];
            var index = ToString(value, chars);
            charStream.WriteBase(chars + index.Key, index.Value);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>起始位置+字符串长度</returns>
        private static KeyValueStruct<int, int> ToString(this uint value, char* chars)
        {
            if (value >= 100000000)
            {
                var value100000000 = (uint)((value * Div100000000Mul) >> Div100000000Shift);
                value -= value100000000 * 100000000;
                var value10000 = (uint)((value * Div10000Mul) >> Div10000Shift);
                UIntToString(value10000, chars + 2);
                UIntToString(value - value10000 * 10000, chars + 6);
                if (value100000000 >= 10)
                {
                    value10000 = (value100000000 * Div1016Mul) >> Div1016Shift;
                    *(uint*)chars = ((value100000000 - value10000 * 10) << 16) | value10000 | 0x300030U;
                    return new KeyValueStruct<int, int>(0, 10);
                }
                *(chars + 1) = (char)(value100000000 + '0');
                return new KeyValueStruct<int, int>(1, 9);
            }
            return new KeyValueStruct<int, int>(0, ToString99999999(value, chars));
        }
        /// <summary>
        /// 小于100000000的正整数转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>字符串长度</returns>
        private static int ToString99999999(uint value, char* chars)
        {
            if (value >= 10000)
            {
                var value10000 = (uint)((value * Div10000Mul) >> Div10000Shift);
                if (value10000 >= 100)
                {
                    var value10 = (value10000 * Div1016Mul) >> Div1016Shift;
                    if (value10000 >= 1000)
                    {
                        UIntToString(value - value10000 * 10000, chars + 4);
                        value = (value10 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 2) = ((value10000 - value10 * 10) << 16) | (value10 - value * 10) | 0x300030U;
                        value10 = (value * Div1016Mul) >> Div1016Shift;
                        *(uint*)chars = ((value - value10 * 10) << 16) | value10 | 0x300030U;
                        return 8;
                    }
                    UIntToString(value - value10000 * 10000, chars + 3);
                    chars[2] = (char)((value10000 - value10 * 10) + '0');
                    value = (value10 * Div1016Mul) >> Div1016Shift;
                    *(uint*)chars = ((value10 - value * 10) << 16) | value | 0x300030U;
                    return 7;
                }
                if (value10000 >= 10)
                {
                    UIntToString(value - value10000 * 10000, chars + 2);
                    value = (value10000 * Div1016Mul) >> Div1016Shift;
                    *(uint*)chars = ((value10000 - value * 10) << 16) | value | 0x300030U;
                    return 6;
                }
                UIntToString(value - value10000 * 10000, chars + 1);
                chars[0] = (char)(value10000 + '0');
                return 5;
            }
            if (value >= 100)
            {
                var value10 = (value * Div1016Mul) >> Div1016Shift;
                if (value >= 1000)
                {
                    var value100 = (value10 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 2) = ((value - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030U;
                    value10 = (value100 * Div1016Mul) >> Div1016Shift;
                    *(uint*)chars = ((value100 - value10 * 10) << 16) | value10 | 0x300030U;
                    return 4;
                }
                else
                {
                    chars[2] = (char)((value - value10 * 10) + '0');
                    var value100 = (value10 * Div1016Mul) >> Div1016Shift;
                    *(uint*)chars = ((value10 - value100 * 10) << 16) | value100 | 0x300030U;
                    return 3;
                }
            }
            if (value >= 10)
            {
                var value10 = (value * Div1016Mul) >> Div1016Shift;
                *(uint*)chars = ((value - value10 * 10) << 16) | value10 | 0x300030U;
                return 2;
            }
            *chars = (char)(value + '0');
            return 1;
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>字符串</returns>
        public static string ToString(this int value)
        {
            char* chars = stackalloc char[12];
            KeyValueStruct<int, int> index = ToString(value, chars);
            return new string(chars + index.Key, 0, index.Value);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="charStream">字符流</param>
        internal static void ToString(int value, CharStreamPlus charStream)
        {
            char* chars = stackalloc char[12];
            KeyValueStruct<int, int> index = ToString(value, chars);
            charStream.WriteBase(chars + index.Key, index.Value);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>起始位置+字符串长度</returns>
        private static KeyValueStruct<int, int> ToString(int value, char* chars)
        {
            if (value >= 0) return ToString((uint)value, chars);
            var value32 = (uint)-value;
            if (value32 >= 100000000)
            {
                uint value100000000 = (uint)((value32 * Div100000000Mul) >> Div100000000Shift);
                value32 -= value100000000 * 100000000;
                uint value10000 = (uint)((value32 * Div10000Mul) >> Div10000Shift);
                UIntToString(value10000, chars + 4);
                UIntToString(value32 - value10000 * 10000, chars + 8);
                if (value100000000 >= 10)
                {
                    value10000 = (value100000000 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 2) = ((value100000000 - value10000 * 10) << 16) | value10000 | 0x300030U;
                    *(chars + 1) = '-';
                    return new KeyValueStruct<int, int>(1, 11);
                }
                *(uint*)(chars + 2) = '-' + ((value100000000 + '0') << 16);
                return new KeyValueStruct<int, int>(2, 10);
            }
            return ToString_99999999(value32, chars);
        }
        /// <summary>
        /// 绝对值小于100000000的负整数转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>起始位置+字符串长度</returns>
        private static KeyValueStruct<int, int> ToString_99999999(uint value, char* chars)
        {
            if (value >= 10000)
            {
                uint value10000 = (uint)((value * Div10000Mul) >> Div10000Shift);
                if (value10000 >= 100)
                {
                    uint value10 = (value10000 * Div1016Mul) >> Div1016Shift;
                    if (value10000 >= 1000)
                    {
                        UIntToString(value - value10000 * 10000, chars + 6);
                        value = (value10 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 4) = ((value10000 - value10 * 10) << 16) | (value10 - value * 10) | 0x300030U;
                        value10 = (value * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 2) = ((value - value10 * 10) << 16) | value10 | 0x300030U;
                        *(chars + 1) = '-';
                        return new KeyValueStruct<int, int>(1, 9);
                    }
                    UIntToString(value - value10000 * 10000, chars + 4);
                    value = (value10 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 2) = ((value10000 - value10 * 10) << 16) | (value10 - value * 10) | 0x300030U;
                    *(uint*)chars = '-' + ((value + '0') << 16);
                    return new KeyValueStruct<int, int>(0, 8);
                }
                if (value10000 >= 10)
                {
                    UIntToString(value - value10000 * 10000, chars + 4);
                    value = (value10000 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 2) = ((value10000 - value * 10) << 16) | value | 0x300030U;
                    *(chars + 1) = '-';
                    return new KeyValueStruct<int, int>(1, 7);
                }
                UIntToString(value - value10000 * 10000, chars + 2);
                *(uint*)chars = '-' + ((value10000 + '0') << 16);
                return new KeyValueStruct<int, int>(0, 6);
            }
            if (value >= 100)
            {
                if (value >= 1000)
                {
                    var value10 = (value * Div1016Mul) >> Div1016Shift;
                    var value100 = (value10 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 4) = ((value - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030U;
                    value10 = (value100 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 2) = ((value100 - value10 * 10) << 16) | value10 | 0x300030U;
                    *(chars + 1) = '-';
                    return new KeyValueStruct<int, int>(1, 5);
                }
                else
                {
                    var value10 = (value * Div1016Mul) >> Div1016Shift;
                    var value100 = (value10 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 2) = ((value - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030U;
                    *(uint*)chars = '-' + ((value100 + '0') << 16);
                    return new KeyValueStruct<int, int>(0, 4);
                }
            }
            if (value >= 10)
            {
                *chars = '-';
                var value10 = (value * Div1016Mul) >> Div1016Shift;
                *(uint*)(chars + 1) = ((value - value10 * 10) << 16) | value10 | 0x300030U;
                return new KeyValueStruct<int, int>(0, 3);
            }
            *(uint*)chars = '-' + ((value + '0') << 16);
            return new KeyValueStruct<int, int>(0, 2);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        private static void UIntToString(uint value, char* chars)
        {
            var value10 = (value * Div1016Mul) >> Div1016Shift;
            var value100 = (value10 * Div1016Mul) >> Div1016Shift;
            *(uint*)(chars + 2) = ((value - value10 * 10) << 16) | (value10 - value100 * 10) | 0x300030U;
            value10 = (value100 * Div1016Mul) >> Div1016Shift;
            *(uint*)chars = ((value100 - value10 * 10) << 16) | value10 | 0x300030U;
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>字符串</returns>
        public static string ToString(this ulong value)
        {
            char* chars = stackalloc char[20];
            KeyValueStruct<int, int> index = ToString(value, chars);
            return new string(chars + index.Key, 0, index.Value);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="charStream">字符流</param>
        internal static void ToString(ulong value, CharStreamPlus charStream)
        {
            char* chars = stackalloc char[20];
            KeyValueStruct<int, int> index = ToString(value, chars);
            charStream.WriteBase(chars + index.Key, index.Value);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>起始位置+字符串长度</returns>
        private static KeyValueStruct<int, int> ToString(ulong value, char* chars)
        {
            if (value >= 10000000000000000L)
            {
                var value100000000 = value / 100000000;
                value -= value100000000 * 100000000;
                var value10000 = (uint)((value * Div10000Mul) >> Div10000Shift);
                UIntToString(value10000, chars + 12);
                UIntToString((uint)value - value10000 * 10000U, chars + 16);
                value = value100000000 / 100000000;
                value100000000 -= value * 100000000;
                value10000 = (uint)((value100000000 * Div10000Mul) >> Div10000Shift);
                UIntToString(value10000, chars + 4);
                UIntToString((uint)value100000000 - value10000 * 10000U, chars + 8);
                var value32 = (uint)value;
                if (value32 >= 100)
                {
                    value10000 = (value32 * Div1016Mul) >> Div1016Shift;
                    var value100 = (value10000 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 2) = ((value32 - value10000 * 10) << 16) | (value10000 - value100 * 10) | 0x300030U;
                    if (value32 >= 1000)
                    {
                        value10000 = (value100 * Div1016Mul) >> Div1016Shift;
                        *(uint*)chars = ((value100 - value10000 * 10) << 16) | value10000 | 0x300030U;
                        return new KeyValueStruct<int, int>(0, 20);
                    }
                    *(chars + 1) = (char)(value100 + '0');
                    return new KeyValueStruct<int, int>(1, 19);
                }
                if (value32 >= 10)
                {
                    value10000 = (value32 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 2) = ((value32 - value10000 * 10) << 16) | value10000 | 0x300030U;
                    return new KeyValueStruct<int, int>(2, 18);
                }
                *(chars + 3) = (char)(value32 + '0');
                return new KeyValueStruct<int, int>(3, 17);
            }
            if (value >= 100000000)
            {
                var value100000000 = value / 100000000;
                value -= value100000000 * 100000000;
                var value10000 = (uint)((value * Div10000Mul) >> Div10000Shift);
                UIntToString(value10000, chars + 8);
                UIntToString((uint)value - value10000 * 10000U, chars + 12);
                var value32 = (uint)value100000000;
                if (value32 >= 10000)
                {
                    value10000 = (uint)((value100000000 * Div10000Mul) >> Div10000Shift);
                    UIntToString(value32 - value10000 * 10000, chars + 4);
                    if (value10000 >= 100)
                    {
                        value32 = (value10000 * Div1016Mul) >> Div1016Shift;
                        if (value10000 >= 1000)
                        {
                            uint value100 = (value32 * Div1016Mul) >> Div1016Shift;
                            *(uint*)(chars + 2) = ((value10000 - value32 * 10) << 16) | (value32 - value100 * 10) | 0x300030U;
                            value32 = (value100 * Div1016Mul) >> Div1016Shift;
                            *(uint*)chars = ((value100 - value32 * 10) << 16) | value32 | 0x300030U;
                            return new KeyValueStruct<int, int>(0, 16);
                        }
                        else
                        {
                            chars[3] = (char)((value10000 - value32 * 10) + '0');
                            var value100 = (value32 * Div1016Mul) >> Div1016Shift;
                            *(uint*)(chars + 1) = ((value32 - value100 * 10) << 16) | value100 | 0x300030U;
                            return new KeyValueStruct<int, int>(1, 15);
                        }
                    }
                    if (value10000 >= 10)
                    {
                        value32 = (value10000 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 2) = ((value10000 - value32 * 10) << 16) | value32 | 0x300030U;
                        return new KeyValueStruct<int, int>(2, 14);
                    }
                    *(chars + 3) = (char)(value10000 + '0');
                    return new KeyValueStruct<int, int>(3, 13);
                }
                if (value32 >= 100)
                {
                    value10000 = (value32 * Div1016Mul) >> Div1016Shift;
                    if (value32 >= 1000)
                    {
                        var value100 = (value10000 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 6) = ((value32 - value10000 * 10) << 16) | (value10000 - value100 * 10) | 0x300030U;
                        value10000 = (value100 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 4) = ((value100 - value10000 * 10) << 16) | value10000 | 0x300030U;
                        return new KeyValueStruct<int, int>(4, 12);
                    }
                    else
                    {
                        chars[7] = (char)((value32 - value10000 * 10) + '0');
                        var value100 = (value10000 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 5) = ((value10000 - value100 * 10) << 16) | value100 | 0x300030U;
                        return new KeyValueStruct<int, int>(5, 11);
                    }
                }
                if (value32 >= 10)
                {
                    value10000 = (value32 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 6) = ((value32 - value10000 * 10) << 16) | value10000 | 0x300030U;
                    return new KeyValueStruct<int, int>(6, 10);
                }
                *(chars + 7) = (char)(value32 + '0');
                return new KeyValueStruct<int, int>(7, 9);
            }
            return new KeyValueStruct<int, int>(0, ToString99999999((uint)value, chars));
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>字符串</returns>
        public static string ToString(this long value)
        {
            char* chars = stackalloc char[22];
            var index = ToString(value, chars);
            return new string(chars + index.Key, 0, index.Value);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="charStream">字符流</param>
        internal static void ToString(long value, CharStreamPlus charStream)
        {
            char* chars = stackalloc char[22];
            var index = ToString(value, chars);
            charStream.WriteBase(chars + index.Key, index.Value);
        }
        /// <summary>
        /// 数值转字符串
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="chars">字符串</param>
        /// <returns>起始位置+字符串长度</returns>
        private static KeyValueStruct<int, int> ToString(long value, char* chars)
        {
            if (value >= 0) return ToString((ulong)value, chars);
            var value64 = (ulong)-value;
            if (value64 >= 10000000000000000L)
            {
                var value100000000 = value64 / 100000000;
                value64 -= value100000000 * 100000000;
                var value10000 = (uint)((value64 * Div10000Mul) >> Div10000Shift);
                UIntToString(value10000, chars + 14);
                UIntToString((uint)value64 - value10000 * 10000U, chars + 18);
                value64 = value100000000 / 100000000;
                value100000000 -= value64 * 100000000;
                value10000 = (uint)((value100000000 * Div10000Mul) >> Div10000Shift);
                UIntToString(value10000, chars + 6);
                UIntToString((uint)value100000000 - value10000 * 10000U, chars + 10);
                var value32 = (uint)value64;
                if (value32 >= 100)
                {
                    value10000 = (value32 * Div1016Mul) >> Div1016Shift;
                    var value100 = (value10000 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 4) = ((value32 - value10000 * 10) << 16) | (value10000 - value100 * 10) | 0x300030U;
                    if (value32 >= 1000)
                    {
                        value10000 = (value100 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 2) = ((value100 - value10000 * 10) << 16) | value10000 | 0x300030U;
                        *(chars + 1) = '-';
                        return new KeyValueStruct<int, int>(1, 21);
                    }
                    *(uint*)(chars + 2) = '-' + ((value100 + '0') << 16);
                    return new KeyValueStruct<int, int>(2, 20);
                }
                if (value32 >= 10)
                {
                    value10000 = (value32 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 4) = ((value32 - value10000 * 10) << 16) | value10000 | 0x300030U;
                    *(chars + 3) = '-';
                    return new KeyValueStruct<int, int>(3, 19);
                }
                *(uint*)(chars + 4) = '-' + ((value32 + '0') << 16);
                return new KeyValueStruct<int, int>(4, 18);
            }
            if (value64 >= 100000000)
            {
                var value100000000 = value64 / 100000000;
                value64 -= value100000000 * 100000000;
                var value10000 = (uint)((value64 * Div10000Mul) >> Div10000Shift);
                UIntToString(value10000, chars + 10);
                UIntToString((uint)value64 - value10000 * 10000U, chars + 14);
                uint value32 = (uint)value100000000;
                if (value32 >= 10000)
                {
                    value10000 = (uint)((value100000000 * Div10000Mul) >> Div10000Shift);
                    UIntToString(value32 - value10000 * 10000, chars + 6);
                    if (value10000 >= 100)
                    {
                        value32 = (value10000 * Div1016Mul) >> Div1016Shift;
                        uint value100 = (value32 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 4) = ((value10000 - value32 * 10) << 16) | (value32 - value100 * 10) | 0x300030U;
                        if (value10000 >= 1000)
                        {
                            value32 = (value100 * Div1016Mul) >> Div1016Shift;
                            *(uint*)(chars + 2) = ((value100 - value32 * 10) << 16) | value32 | 0x300030U;
                            *(chars + 1) = '-';
                            return new KeyValueStruct<int, int>(1, 17);
                        }
                        *(uint*)(chars + 2) = '-' + ((value100 + '0') << 16);
                        return new KeyValueStruct<int, int>(2, 16);
                    }
                    if (value10000 >= 10)
                    {
                        value32 = (value10000 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 4) = ((value10000 - value32 * 10) << 16) | value32 | 0x300030U;
                        *(chars + 3) = '-';
                        return new KeyValueStruct<int, int>(3, 15);
                    }
                    *(uint*)(chars + 4) = '-' + ((value10000 + '0') << 16);
                    return new KeyValueStruct<int, int>(4, 14);
                }
                if (value32 >= 100)
                {
                    value10000 = (value32 * Div1016Mul) >> Div1016Shift;
                    uint value100 = (value10000 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 8) = ((value32 - value10000 * 10) << 16) | (value10000 - value100 * 10) | 0x300030U;
                    if (value32 >= 1000)
                    {
                        value10000 = (value100 * Div1016Mul) >> Div1016Shift;
                        *(uint*)(chars + 6) = ((value100 - value10000 * 10) << 16) | value10000 | 0x300030U;
                        *(chars + 5) = '-';
                        return new KeyValueStruct<int, int>(5, 13);
                    }
                    *(uint*)(chars + 6) = '-' + ((value100 + '0') << 16);
                    return new KeyValueStruct<int, int>(6, 12);
                }
                if (value32 >= 10)
                {
                    value10000 = (value32 * Div1016Mul) >> Div1016Shift;
                    *(uint*)(chars + 8) = ((value32 - value10000 * 10) << 16) | value10000 | 0x300030U;
                    *(chars + 7) = '-';
                    return new KeyValueStruct<int, int>(7, 11);
                }
                *(uint*)(chars + 8) = '-' + ((value32 + '0') << 16);
                return new KeyValueStruct<int, int>(8, 10);
            }
            return ToString_99999999((uint)value64, chars);
        }
        /// <summary>
        /// 双精度浮点数转字符串(用于代码生成)
        /// </summary>
        /// <param name="value">浮点数</param>
        /// <returns>字符串</returns>
        public static string ToString(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// 双精度浮点数转字符串(用于代码生成)
        /// </summary>
        /// <param name="value">浮点数</param>
        /// <returns>字符串</returns>
        public static string ToString(this float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        #endregion

        /// <summary>
        /// 2^n相关32位deBruijn序列集合
        /// </summary>
        private static PointerStruct _deBruijn32;
        /// <summary>
        /// 2^n相关32位deBruijn序列
        /// </summary>
        public const uint DeBruijn32Number = 0x04653adfU;
        /// <summary>
        /// 获取2^n相关32位deBruijn序列集合
        /// </summary>
        /// <returns>2^n相关32位deBruijn序列集合</returns>
        public static PointerStruct GetDeBruijn32()
        {
            PointerStruct deBruijn32 = UnmanagedPlus.Get(32);
            var deBruijn32Data = deBruijn32.Byte;
            for (byte bit = 0; bit != 32; ++bit) deBruijn32Data[((1U << bit) * DeBruijn32Number) >> 27] = bit;
            //deBruijn64 = new byte[64];
            //for (byte bit = 0; bit != 64; ++bit) deBruijn64[((1UL << bit) * deBruijn64Number) >> 58] = bit;
            return deBruijn32;
        }
        /// <summary>
        /// 获取有效位长度
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>有效位长度</returns>
        public static int Bits(this uint value)
        {
            if ((value & 0x80000000U) == 0)
            {
                var code = value;
                code |= code >> 16;
                code |= code >> 8;
                code |= code >> 4;
                code |= code >> 2;
                code |= code >> 1;
                return _deBruijn32.Byte[((++code) * DeBruijn32Number) >> 27];
            }
            return 32;
        }
        /// <summary>
        /// 获取有效位长度
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>有效位长度</returns>
        public static int Bits(this ulong value)
        {
            return (value & 0xffffffff00000000UL) == 0 ? Bits((uint)value) : (Bits((uint)(value >> 32)) + 32);
            //if ((value & 0x8000000000000000UL) == 0)
            //{
            //    ulong code = value;
            //    code |= code >> 32;
            //    code |= code >> 16;
            //    code |= code >> 8;
            //    code |= code >> 4;
            //    code |= code >> 2;
            //    code |= code >> 1;
            //    return DeBruijn64[((++code) * DeBruijn64Number) >> 58];
            //}
            //else return 32;
        }
        /// <summary>
        /// 获取二进制1位的个数
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>二进制1位的个数</returns>
        public static int BitCount(this uint value)
        {
            //return bitCounts[(byte)value] + bitCounts[(byte)(value >> 8)] + bitCounts[(byte)(value >> 16)] + bitCounts[value >> 24];

            //value = (value & 0x49249249) + ((value >> 1) & 0x49249249) + ((value >> 2) & 0x49249249);
            //value = (value & 0xc71c71c7) + ((value >> 3) & 0xc71c71c7);
            //uint div = (uint)(((ulong)value * (((1UL << 37) + 62) / 63)) >> 37);
            //return (int)(value - (div << 6) + div);

            //value = (value & 0x49249249) + ((value >> 1) & 0x49249249) + ((value >> 2) & 0x49249249);
            //value = (value & 0x71c71c7) + ((value >> 3) & 0x71c71c7) + (value >> 30);
            //uint nextValue = (uint)((value * 0x41041042UL) >> 36);
            //return (int)(value - (nextValue << 6) + nextValue);

            value -= ((value >> 1) & 0x55555555U);//2:2
            value = (value & 0x33333333U) + ((value >> 2) & 0x33333333U);//4:4
            value += value >> 4;
            value &= 0x0f0f0f0f;//8:8

            //uint div = (uint)(((ulong)value * (((1UL << 39) + 254) / 255)) >> 39);
            //return (int)(value - (div << 8) + div);
            value += (value >> 8);
            return (byte)(value + (value >> 16));
        }
        /// <summary>
        /// 转十六进制字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <returns>十六进制字符串</returns>
        public static string ToHex(this ushort value)
        {
            var hexs = StringPlus.FastAllocateString(4);
            fixed (char* chars = hexs)
            {
                var nextChar = chars + 3;
                var next = value & 15;
                *nextChar = (char)(next < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
                next = (value >>= 4) & 15;
                *--nextChar = (char)(next < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
                next = (value >>= 4) & 15;
                *--nextChar = (char)(next < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
                *--nextChar = (char)((value >>= 4) < 10 ? value + '0' : (value + ('0' + 'A' - '9' - 1)));
            }
            return hexs;
        }
        /// <summary>
        /// 数字值转换为十六进制字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="hexs">十六进制字符串</param>
        public static void ToHex(this uint value, char* hexs)
        {
            if (hexs != null) UIntToHex(value, hexs);
        }
        /// <summary>
        /// 数字值转换为十六进制字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="hexs">十六进制字符串</param>
        private static void UIntToHex(uint value, char* hexs)
        {
            var nextChar = hexs + 8;
            var next = value & 15;
            *--nextChar = (char)(next < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (char)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (char)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (char)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (char)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (char)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (char)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (char)((value >>= 4) < 10 ? value + '0' : (value + ('0' + 'A' - '9' - 1)));
        }
        /// <summary>
        /// 转换8位十六进制字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <returns>8位十六进制字符串</returns>
        public static string ToHex8(this uint value)
        {
            string hexs = StringPlus.FastAllocateString(8);
            fixed (char* hexFixed = hexs) UIntToHex(value, hexFixed);
            return hexs;
        }
        /// <summary>
        /// 转换16位十六进制字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <returns>16位十六进制字符串</returns>
        public static string ToHex16(this ulong value)
        {
            var hexs = StringPlus.FastAllocateString(16);
            fixed (char* hexFixed = hexs)
            {
                UIntToHex((uint)value, hexFixed + 8);
                UIntToHex((uint)(value >> 32), hexFixed);
            }
            return hexs;
        }
        /// <summary>
        /// 16进制字符串转换成整数
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>整数</returns>
        internal static uint ParseHex(char* start, char* end)
        {
            var value = (uint)(*start - '0');
            if (value >= 10) value = (uint)((*start & 0xdf) - ('0' + 'A' - '9' - 1));
            while (++start != end)
            {
                value <<= 4;
                var hex = (uint)(*start - '0');
                value += hex < 10 ? hex : (uint)((*start & 0xdf) - ('0' + 'A' - '9' - 1));
            }
            return value;
        }
        /// <summary>
        /// 16位十六进制转64二进制位整数(无格式检测)
        /// </summary>
        /// <param name="value">十六进制字符串</param>
        /// <returns>64二进制位整数</returns>
        public static ulong ParseHex16NoCheck(this string value)
        {
            if (value != null && value.Length == 16)
            {
                fixed (char* hexs = value) return ParseHex64(hexs, hexs + value.Length);
            }
            return 0;
        }
        /// <summary>
        /// 16进制字符串转换成64位整数
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>64位整数</returns>
        internal static ulong ParseHex64(char* start, char* end)
        {
            var length = (int)(end - start);
            if (length <= 8) return ParseHex(start, end);
            var lowStart = end - 8;
            return ((ulong)ParseHex(start, lowStart) << 32) + ParseHex(lowStart, end);
        }
        /// <summary>
        /// 10进制字符串转换成整数
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>整数</returns>
        internal static uint Parse(char* start, char* end)
        {
            var value = (uint)(*start - '0');
            while (++start != end)
            {
                value *= 10;
                value += (uint)(*start - '0');
            }
            return value;
        }
        /// <summary>
        /// 10进制字符串转换成64位整数
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>64位整数</returns>
        internal static ulong Parse64(char* start, char* end)
        {
            var length = (int)(end - start);
            if (length <= 9) return Parse(start, end);
            var lowStart = end - 9;
            if (length <= 18) return (Parse(start, lowStart) * 1000000000UL) + Parse(lowStart, end);
            var highStart = lowStart - 9;
            return (Parse(start, highStart) * 1000000000000000000UL) + (Parse(highStart, lowStart) * 1000000000UL) + Parse(lowStart, end);
        }
        /// <summary>
        /// 16进制字符串转换成整数
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>整数</returns>
        internal static uint ParseHex(byte* start, byte* end)
        {
            var value = (uint)(*start - '0');
            if (value >= 10) value = (uint)((*start & 0xdf) - ('0' + 'A' - '9' - 1));
            while (++start != end)
            {
                value <<= 4;
                var hex = (uint)(*start - '0');
                value += hex < 10 ? hex : (uint)((*start & 0xdf) - ('0' + 'A' - '9' - 1));
            }
            return value;
        }
        /// <summary>
        /// 数字值转换为十六进制字符串
        /// </summary>
        /// <param name="value">数字值</param>
        /// <param name="hexs">十六进制字符串</param>
        internal static void ToHex(uint value, byte* hexs)
        {
            var nextChar = hexs + 8;
            var next = value & 15;
            *--nextChar = (byte)(next < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (byte)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (byte)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (byte)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (byte)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (byte)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (byte)((next = (value >>= 4) & 15) < 10 ? next + '0' : (next + ('0' + 'A' - '9' - 1)));
            *--nextChar = (byte)((value >>= 4) < 10 ? value + '0' : (value + ('0' + 'A' - '9' - 1)));
        }
        /// <summary>
        /// 求平方根
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="mod">余数</param>
        /// <returns>平方根</returns>
        public static uint Sqrt(this uint value, out uint mod)
        {
            uint sqrtValue = 0;
            if ((mod = value) >= 0x40000000)
            {
                sqrtValue = 0x8000;
                mod -= 0x40000000;
            }
            value = (sqrtValue << 15) + 0x10000000;
            if (mod >= value)
            {
                sqrtValue |= 0x4000;
                mod -= value;
            }
            value = (sqrtValue << 14) + 0x4000000;
            if (mod >= value)
            {
                sqrtValue |= 0x2000;
                mod -= value;
            }
            value = (sqrtValue << 13) + 0x1000000;
            if (mod >= value)
            {
                sqrtValue |= 0x1000;
                mod -= value;
            }
            value = (sqrtValue << 12) + 0x400000;
            if (mod >= value)
            {
                sqrtValue |= 0x800;
                mod -= value;
            }
            value = (sqrtValue << 11) + 0x100000;
            if (mod >= value)
            {
                sqrtValue |= 0x400;
                mod -= value;
            }
            value = (sqrtValue << 10) + 0x40000;
            if (mod >= value)
            {
                sqrtValue |= 0x200;
                mod -= value;
            }
            value = (sqrtValue << 9) + 0x10000;
            if (mod >= value)
            {
                sqrtValue |= 0x100;
                mod -= value;
            }
            value = (sqrtValue << 8) + 0x4000;
            if (mod >= value)
            {
                sqrtValue |= 0x80;
                mod -= value;
            }
            value = (sqrtValue << 7) + 0x1000;
            if (mod >= value)
            {
                sqrtValue |= 0x40;
                mod -= value;
            }
            value = (sqrtValue << 6) + 0x400;
            if (mod >= value)
            {
                sqrtValue |= 0x20;
                mod -= value;
            }
            value = (sqrtValue << 5) + 0x100;
            if (mod >= value)
            {
                sqrtValue |= 0x10;
                mod -= value;
            }
            value = (sqrtValue << 4) + 0x40;
            if (mod >= value)
            {
                sqrtValue |= 0x8;
                mod -= value;
            }
            value = (sqrtValue << 3) + 0x10;
            if (mod >= value)
            {
                sqrtValue |= 0x4;
                mod -= value;
            }
            value = (sqrtValue << 2) + 0x4;
            if (mod >= value)
            {
                sqrtValue |= 0x2;
                mod -= value;
            }
            value = (sqrtValue << 1) + 0x1;
            if (mod >= value)
            {
                sqrtValue++;
                mod -= value;
            }
            return sqrtValue;
        }
        static NumberPlus()
        {
            _deBruijn32 = GetDeBruijn32();
        }
    }
}
