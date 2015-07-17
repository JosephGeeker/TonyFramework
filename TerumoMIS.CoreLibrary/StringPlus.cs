﻿//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: StringPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  StringPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 15:10:56
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     字符串相关操作
    /// </summary>
    public static unsafe class StringPlus
    {
        /// <summary>
        ///     申请字符串空间
        /// </summary>
        public static readonly Func<int, string> FastAllocateString =
            (Func<int, string>)
                Delegate.CreateDelegate(typeof (Func<int, string>),
                    typeof (string).GetMethod("FastAllocateString", BindingFlags.Static | BindingFlags.NonPublic, null,
                        new[] {typeof (int)}, null));

        /// <summary>
        ///     base64编码查询表
        /// </summary>
        internal static PointerStruct Base64;

        static StringPlus()
        {
            Base64 = UnmanagedPlus.Get(64);
            var base64 = Base64.Byte;
            foreach (var code in "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/")
                *base64++ = (byte) code;
        }

        /// <summary>
        ///     获取字符串长度
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>null为0</returns>
        public static int Length(this string value)
        {
            return value != null ? value.Length : 0;
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="values">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        public static string JoinString(this SubArrayStruct<string> values, char join)
        {
            if (values.Array != null)
            {
                if (values.Count > 1)
                {
                    var array = values.Array;
                    var length = 0;
                    for (int index = values.StartIndex, endIndex = index + values.Count; index != endIndex; ++index)
                    {
                        var value = array[index];
                        if (value != null) length += value.Length;
                    }
                    var newValue = FastAllocateString(length + values.Count - 1);
                    fixed (char* valueFixed = newValue)
                    {
                        var write = valueFixed;
                        for (int index = values.StartIndex, endIndex = index + values.Count; index != endIndex; ++index)
                        {
                            var value = array[index];
                            if (value != null)
                            {
                                StringUnsafe.Copy(value, write);
                                write += value.Length;
                            }
                            *write++ = join;
                        }
                    }
                    return newValue;
                }
                if (values.Count == 1) return values.Array[values.StartIndex] ?? string.Empty;
                return string.Empty;
            }
            return null;
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="values">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        public static string JoinString(this IEnumerable<string> values, string join)
        {
            return values != null ? string.Join(join, values.getSubArray().ToArray()) : null;
        }

        /// <summary>
        ///     字符替换
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="oldChar">原字符</param>
        /// <param name="newChar">目标字符</param>
        /// <returns>字符串</returns>
        public static string Replace(this string value, char oldChar, char newChar)
        {
            return !string.IsNullOrEmpty(value) ? StringUnsafe.Replace(value, oldChar, newChar) : value;
        }

        /// <summary>
        ///     字符替换
        /// </summary>
        /// <param name="value">字符串,不能为null</param>
        /// <param name="oldChar">原字符</param>
        /// <param name="newChar">目标字符</param>
        /// <returns>字符串</returns>
        public static string ReplaceNotNull(this string value, char oldChar, char newChar)
        {
            return value.Length != 0 ? StringUnsafe.Replace(value, oldChar, newChar) : value;
        }

        /// <summary>
        ///     分割字符串
        /// </summary>
        /// <param name="value">原字符串</param>
        /// <param name="split">分割符</param>
        /// <returns>字符子串集合</returns>
        public static SubArrayStruct<SubStringStruct> Split(this string value, char split)
        {
            return value.Split(0, value.Length(), split);
        }

        /// <summary>
        ///     分割字符串
        /// </summary>
        /// <param name="value">原字符串</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">分割字符串长度</param>
        /// <param name="split">分割符</param>
        /// <returns>字符子串集合</returns>
        public static SubArrayStruct<SubStringStruct> Split(this string value, int startIndex, int length, char split)
        {
            var range = new ArrayPlus.RangeStruct(value.Length(), startIndex, length);
            if (range.GetCount != length) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            var values = default(SubArrayStruct<SubStringStruct>);
            if (value != null)
            {
                fixed (char* valueFixed = value)
                {
                    char* last = valueFixed + range.SkipCount, end = last + range.GetCount;
                    for (var start = last; start != end;)
                    {
                        if (*start == split)
                        {
                            values.Add(SubStringStruct.Unsafe(value, (int) (last - valueFixed), (int) (start - last)));
                            last = ++start;
                        }
                        else ++start;
                    }
                    values.Add(SubStringStruct.Unsafe(value, (int) (last - valueFixed), (int) (end - last)));
                }
            }
            return values;
        }

        /// <summary>
        ///     获取字符串原始字节流
        /// </summary>
        /// <param name="value">字符串,不能为null</param>
        /// <returns>原始字节流</returns>
        public static byte[] SerializeNotNull(this string value)
        {
            return value.Length != 0 ? StringUnsafe.Serialize(value) : NullValuePlus<byte>.Array;
        }

        /// <summary>
        ///     获取Ascii字符串原始字节流
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字节流</returns>
        public static byte[] GetBytes(this string value)
        {
            if (value != null)
            {
                fixed (char* valueFixed = value) return StringUnsafe.GetBytes(valueFixed, value.Length);
            }
            return null;
        }

        /// <summary>
        ///     根据原始字节流生成字符串
        /// </summary>
        /// <param name="data">原始字节流</param>
        /// <returns>字符串</returns>
        public static string DeSerialize(this byte[] data)
        {
            if (data != null)
            {
                if (data.Length != 0)
                {
                    fixed (byte* dataFixed = data) return DeSerialize(dataFixed, -data.Length);
                }
                return string.Empty;
            }
            return null;
        }

        /// <summary>
        ///     根据原始字节流生成字符串
        /// </summary>
        /// <param name="data">原始字节流</param>
        /// <param name="length">字符串长度</param>
        /// <returns>字符串</returns>
        public static string DeSerialize(byte* data, int length)
        {
            if (length >= 0)
            {
                return length != 0 ? new string((char*) data, 0, length >> 1) : string.Empty;
            }
            var value = FastAllocateString(length = -length);
            fixed (char* valueFixed = value)
            {
                var start = valueFixed;
                for (var end = data + length; data != end; *start++ = (char) *data++)
                {
                }
            }
            return value;
        }

        /// <summary>
        ///     大写转小写
        /// </summary>
        /// <param name="value">大写字符串</param>
        /// <returns>小写字符串(原引用)</returns>
        public static string ToLower(this string value)
        {
            if (value != null)
            {
                fixed (char* valueFixed = value) StringUnsafe.ToLower(valueFixed, valueFixed + value.Length);
            }
            return value;
        }

        /// <summary>
        ///     比较字符串(忽略大小写)
        /// </summary>
        /// <param name="left">不能为null</param>
        /// <param name="right">不能为null</param>
        /// <returns>是否相等</returns>
        public static bool EqualCase(this string left, string right)
        {
            if (left != null)
            {
                if (right != null)
                {
                    if (left.Length == right.Length)
                    {
                        fixed (char* leftFixed = left, rightFixed = right)
                            return StringUnsafe.EqualCase(leftFixed, rightFixed, left.Length);
                    }
                }
                return false;
            }
            return right == null;
        }

        /// <summary>
        ///     比较字符串(忽略大小写)
        /// </summary>
        /// <param name="left">不能为null</param>
        /// <param name="right">不能为null</param>
        /// <param name="count">字符数量,大于等于0</param>
        /// <returns>是否相等</returns>
        public static bool EqualCase(this string left, string right, int count)
        {
            if (left != null)
            {
                if (right != null)
                {
                    int leftLength = left.Length, rightLength = right.Length;
                    if (leftLength > count) leftLength = count;
                    if (rightLength > count) rightLength = count;
                    if (leftLength == rightLength && count >= 0)
                    {
                        fixed (char* leftFixed = left, rightFixed = right)
                            return StringUnsafe.EqualCase(leftFixed, rightFixed, count);
                    }
                }
                return false;
            }
            return right == null;
        }

        #region ASCII位图

        /// <summary>
        ///     ASCII位图
        /// </summary>
        public struct AsciiMapStruct
        {
            /// <summary>
            ///     位图字节长度
            /// </summary>
            public const int MapBytes = 128 >> 3;

            /// <summary>
            ///     位图
            /// </summary>
            private readonly byte* _map;

            /// <summary>
            ///     初始化ASCII位图
            /// </summary>
            /// <param name="map">位图指针</param>
            /// <param name="value">初始值集合</param>
            /// <param name="isUnsafe">初始值是否安全</param>
            public AsciiMapStruct(PointerStruct map, string value, bool isUnsafe = true)
                : this(map.Byte, value, isUnsafe)
            {
            }

            /// <summary>
            ///     初始化ASCII位图
            /// </summary>
            /// <param name="map">位图指针</param>
            /// <param name="value">初始值集合</param>
            /// <param name="isUnsafe">初始值是否安全</param>
            public AsciiMapStruct(byte* map, string value, bool isUnsafe = true)
            {
                _map = map;
                if (isUnsafe && value.Length != 0)
                {
                    fixed (char* valueFixed = value)
                    {
                        for (char* start = valueFixed, end = valueFixed + value.Length; start != end; ++start)
                        {
                            map[*start >> 3] |= (byte) (1 << (*start & 7));
                        }
                    }
                }
                else Set(value);
            }

            /// <summary>
            ///     非安全访问ASCII位图
            /// </summary>
            public fixedMap.unsafer Unsafer
            {
                get { return new fixedMap.unsafer(_map); }
            }

            /// <summary>
            ///     位图
            /// </summary>
            public byte* Map
            {
                get { return _map; }
            }

            /// <summary>
            ///     指针
            /// </summary>
            public PointerStruct Pointer
            {
                get { return new PointerStruct {Data = _map}; }
            }

            /// <summary>
            ///     设置占位
            /// </summary>
            /// <param name="value">位值</param>
            public void Set(char value)
            {
                if ((value & 0xff80) == 0) _map[value >> 3] |= (byte) (1 << (value & 7));
            }

            /// <summary>
            ///     设置占位
            /// </summary>
            /// <param name="value">位值</param>
            public void Set(byte value)
            {
                if ((value & 0x80) == 0) _map[value >> 3] |= (byte) (1 << (value & 7));
            }

            /// <summary>
            ///     获取占位状态
            /// </summary>
            /// <param name="value">位值</param>
            /// <returns>是否占位</returns>
            public bool Get(char value)
            {
                return (value & 0xff80) == 0 && (_map[value >> 3] & (byte) (1 << (value & 7))) != 0;
            }

            /// <summary>
            ///     获取占位状态
            /// </summary>
            /// <param name="value">位值</param>
            /// <returns>是否占位</returns>
            public bool Get(int value)
            {
                return ((uint) value & 0xffffff80) == 0 && (_map[value >> 3] & (byte) (1 << (value & 7))) != 0;
            }

            /// <summary>
            ///     设置占位
            /// </summary>
            /// <param name="value">值集合</param>
            public void Set(string value)
            {
                if (value != null)
                {
                    fixed (char* valueFixed = value)
                    {
                        for (char* start = valueFixed, end = valueFixed + value.Length; start != end; ++start)
                        {
                            if ((*start & 0xff80) == 0) _map[*start >> 3] |= (byte) (1 << (*start & 7));
                        }
                    }
                }
            }
        }

        #endregion
    }
}