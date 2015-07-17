//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SubStringStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  SubStringStruct
//	User name:  C1400008
//	Location Time: 2015/7/10 15:26:06
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using TerumoMIS.CoreLibrary.Algorithm;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     字符子串
    /// </summary>
    public unsafe struct SubStringStruct : IEquatable<SubStringStruct>, IEquatable<string>
    {
        /// <summary>
        ///     Trim删除字符位图
        /// </summary>
        private static readonly PointerStruct TrimMap =
            new StringPlus.AsciiMapStruct(UnmanagedPlus.Get(StringPlus.AsciiMapStruct.MapBytes), " \t\r").Pointer;

        /// <summary>
        ///     长度
        /// </summary>
        private int _length;

        /// <summary>
        ///     原字符串中的起始位置
        /// </summary>
        private int _startIndex;

        /// <summary>
        ///     原字符串
        /// </summary>
        private string _value;

        /// <summary>
        ///     字符子串
        /// </summary>
        /// <param name="value">字符串</param>
        public SubStringStruct(string value)
        {
            _value = value;
            _startIndex = 0;
            _length = value.Length;
        }

        /// <summary>
        ///     字符子串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="startIndex">起始位置</param>
        public SubStringStruct(string value, int startIndex)
        {
            if (value == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            _length = value.Length - (_startIndex = startIndex);
            if (_length < 0 || startIndex < 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            _value = _length != 0 ? value : string.Empty;
        }

        /// <summary>
        ///     字符子串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public SubStringStruct(string value, int startIndex, int length)
        {
            if (value == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            var range = new ArrayPlus.RangeStruct(value.Length, startIndex, length);
            if (range.GetCount != length) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            if (range.GetCount != 0)
            {
                _value = value;
                _startIndex = range.SkipCount;
                _length = range.GetCount;
            }
            else
            {
                _value = string.Empty;
                _startIndex = _length = 0;
            }
        }

        /// <summary>
        ///     原字符串
        /// </summary>
        public string Value
        {
            get { return _value; }
        }

        /// <summary>
        ///     原字符串中的起始位置
        /// </summary>
        public int StartIndex
        {
            get { return _startIndex; }
        }

        /// <summary>
        ///     长度
        /// </summary>
        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        ///     获取字符
        /// </summary>
        /// <param name="index">字符位置</param>
        /// <returns>字符</returns>
        public char this[int index]
        {
            get { return _value[index + _startIndex]; }
        }

        /// <summary>
        ///     判断子串是否相等
        /// </summary>
        /// <param name="other">待比较子串</param>
        /// <returns>子串是否相等</returns>
        public bool Equals(string other)
        {
            if (_value == null) return other == null;
            if (other != null && _length == other.Length)
            {
                if (_value == other && _startIndex == 0) return true;
                fixed (char* valueFixed = _value, otherFixed = other)
                {
                    return MemoryUnsafe.Equal(valueFixed + _startIndex, otherFixed, _length << 1);
                }
            }
            return false;
        }

        /// <summary>
        ///     判断子串是否相等
        /// </summary>
        /// <param name="other">待比较子串</param>
        /// <returns>子串是否相等</returns>
        public bool Equals(SubStringStruct other)
        {
            if (_value == null) return false;
            if (_length == other.Length)
            {
                if (_value == other.Value && _startIndex == other.StartIndex) return true;
                fixed (char* valueFixed = _value, otherFixed = other.Value)
                {
                    return MemoryUnsafe.Equal(valueFixed + _startIndex, otherFixed + other.StartIndex, _length << 1);
                }
            }
            return false;
        }

        /// <summary>
        ///     字符串隐式转换为子串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字符子串</returns>
        public static implicit operator SubStringStruct(string value)
        {
            return value != null ? Unsafe(value, 0, value.Length) : default(SubStringStruct);
        }

        /// <summary>
        ///     字符子串隐式转换为字符串
        /// </summary>
        /// <param name="value">字符子串</param>
        /// <returns>字符串</returns>
        public static implicit operator string(SubStringStruct value)
        {
            return value.ToString();
        }

        /// <summary>
        ///     清空数据
        /// </summary>
        public void Null()
        {
            _value = null;
            _startIndex = _length = 0;
        }

        /// <summary>
        ///     字符查找
        /// </summary>
        /// <param name="value">查找值</param>
        /// <returns>字符位置,失败返回-1</returns>
        public int IndexOf(char value)
        {
            if (_length != 0)
            {
                fixed (char* valueFixed = _value)
                {
                    char* start = valueFixed + _startIndex, find = StringUnsafe.Find(start, start + _length, value);
                    if (find != null) return (int) (find - start);
                }
            }
            return -1;
        }

        /// <summary>
        ///     获取子串
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <returns>子串</returns>
        public SubStringStruct SubString(int startIndex)
        {
            return new SubStringStruct(_value, _startIndex + startIndex, _length - startIndex);
        }

        /// <summary>
        ///     获取子串
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>子串</returns>
        public SubStringStruct SubString(int startIndex, int length)
        {
            return new SubStringStruct(_value, _startIndex + startIndex, length);
        }

        /// <summary>
        ///     删除前后空格
        /// </summary>
        /// <returns>删除前后空格</returns>
        public SubStringStruct Trim()
        {
            if (_length != 0)
            {
                fixed (char* valueFixed = _value)
                {
                    char* start = valueFixed + _startIndex, end = start + _length;
                    start = StringUnsafe.FindNotAscii(start, end, TrimMap.Byte);
                    if (start == null) return new SubStringStruct(string.Empty);
                    end = StringUnsafe.FindLastNotAscii(start, end, TrimMap.Byte);
                    if (end == null) return new SubStringStruct(string.Empty);
                    return Unsafe(_value, (int) (start - valueFixed), (int) (end - start));
                }
            }
            return this;
        }

        /// <summary>
        ///     分割字符串
        /// </summary>
        /// <param name="split">分割符</param>
        /// <returns>字符子串集合</returns>
        public SubArrayStruct<SubStringStruct> Split(char split)
        {
            return _value.Split(_startIndex, _length, split);
        }

        /// <summary>
        ///     是否以字符串开始
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>是否以字符串开始</returns>
        public bool StartsWith(string value)
        {
            if (value == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (value != null && _length >= value.Length)
            {
                fixed (char* valueFixed = _value, cmpFixed = value)
                {
                    return MemoryUnsafe.Equal(valueFixed + _startIndex, cmpFixed, value.Length << 1);
                }
            }
            return false;
        }

        /// <summary>
        ///     HASH值
        /// </summary>
        /// <returns>HASH值</returns>
        public override int GetHashCode()
        {
            if (_length == 0) return 0;
            fixed (char* valueFixed = _value) return HashCodePlus.GetHashCode(valueFixed + _startIndex, _length << 1);
        }

        /// <summary>
        ///     判断子串是否相等
        /// </summary>
        /// <param name="obj">待比较子串</param>
        /// <returns>子串是否相等</returns>
        public override bool Equals(object obj)
        {
            return Equals((SubStringStruct) obj); //obj != null & obj.GetType() == typeof(subString) && 
        }

        /// <summary>
        ///     转换成字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            if (_value != null)
            {
                if (_startIndex == 0 && _length == _value.Length) return _value;
                fixed (char* valueFixed = _value) return new string(valueFixed, _startIndex, _length);
            }
            return null;
        }

        /// <summary>
        ///     设置数据长度
        /// </summary>
        /// <param name="value">字符串,不能为null</param>
        /// <param name="startIndex">起始位置,必须合法</param>
        /// <param name="length">长度,必须合法</param>
        internal void UnsafeSet(string value, int startIndex, int length)
        {
            _value = value;
            _startIndex = startIndex;
            _length = length;
        }

        /// <summary>
        ///     设置数据长度
        /// </summary>
        /// <param name="length">长度,必须合法</param>
        internal void UnsafeSetLength(int length)
        {
            _length = length;
        }

        /// <summary>
        ///     设置数据长度
        /// </summary>
        /// <param name="startIndex">起始位置,必须合法</param>
        /// <param name="length">长度,必须合法</param>
        internal void UnsafeSet(int startIndex, int length)
        {
            _startIndex = startIndex;
            _length = length;
        }

        /// <summary>
        ///     字符子串(非安全,请自行确保数据可靠性)
        /// </summary>
        /// <param name="value">字符串,不能为null</param>
        /// <param name="startIndex">起始位置,必须合法</param>
        public static SubStringStruct Unsafe(string value, int startIndex)
        {
            return new SubStringStruct {_value = value, _startIndex = startIndex, _length = value.Length - startIndex};
        }

        /// <summary>
        ///     字符子串(非安全,请自行确保数据可靠性)
        /// </summary>
        /// <param name="value">字符串,不能为null</param>
        /// <param name="startIndex">起始位置,必须合法</param>
        /// <param name="length">长度,必须合法</param>
        public static SubStringStruct Unsafe(string value, int startIndex, int length)
        {
            return new SubStringStruct {_value = value, _startIndex = startIndex, _length = length};
        }
    }
}