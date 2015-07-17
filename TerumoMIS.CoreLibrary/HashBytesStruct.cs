//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HashBytesPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  HashBytesPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 8:57:01
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
    public struct HashBytesStruct:IEquatable<HashBytesStruct>,IEquatable<SubArrayStruct<byte>>,IEquatable<byte[]>
    {
        /// <summary>
        /// 字节数组
        /// </summary>
        private SubArrayStruct<byte> _data;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int Length
        {
            get { return _data.Count; }
        }
        /// <summary>
        /// HASH值
        /// </summary>
        private int _hashCode;
        /// <summary>
        /// HASH字节数组隐式转换
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>HASH字节数组</returns>
        public static implicit operator HashBytesStruct(SubArrayStruct<byte> data) { return new HashBytesStruct(data); }
        /// <summary>
        /// HASH字节数组隐式转换
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>HASH字节数组</returns>
        public static implicit operator HashBytesStruct(byte[] data)
        {
            return new HashBytesStruct(data != null ? SubArrayStruct<byte>.Unsafe(data, 0, data.Length) : default(SubArrayStruct<byte>));
        }
        /// <summary>
        /// HASH字节数组隐式转换
        /// </summary>
        /// <param name="value">HASH字节数组</param>
        /// <returns>字节数组</returns>
        public static implicit operator SubArrayStruct<byte>(HashBytesStruct value) { return value._data; }
        /// <summary>
        /// 字节数组HASH
        /// </summary>
        /// <param name="data">字节数组</param>
        public unsafe HashBytesStruct(SubArrayStruct<byte> data)
        {
            _data = data;
            if (data.Count == 0) _hashCode = 0;
            else
            {
                fixed (byte* dataFixed = data.Array)
                {
                    _hashCode = HashCodePlus.GetHashCode(dataFixed + data.StartIndex, data.Count) ^ RandomPlus.Hash;
                }
            }
        }
        /// <summary>
        /// 获取HASH值
        /// </summary>
        /// <returns>HASH值</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }
        /// <summary>
        /// 比较字节数组是否相等
        /// </summary>
        /// <param name="other">字节数组HASH</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object other)
        {
            return Equals((HashBytesStruct)other);
            //return other != null && other.GetType() == typeof(hashBytes) && Equals((hashBytes)other);
        }
        /// <summary>
        /// 比较字节数组是否相等
        /// </summary>
        /// <param name="other">用于HASH的字节数组</param>
        /// <returns>是否相等</returns>
        public unsafe bool Equals(HashBytesStruct other)
        {
            if (_data.Array == null) return other._data.Array == null;
            if (other._data.Array != null && ((_hashCode ^ other._hashCode) | (_data.Count ^ other._data.Count)) == 0)
            {
                if (_data.Array == other._data.Array && _data.StartIndex == other._data.StartIndex) return true;
                fixed (byte* dataFixed = _data.Array, otherDataFixed = other._data.Array)
                {
                    return MemoryUnsafe.Equal(dataFixed + _data.StartIndex, otherDataFixed + other._data.StartIndex, _data.Count);
                }
            }
            return false;
        }
        /// <summary>
        /// 比较字节数组是否相等
        /// </summary>
        /// <param name="other">用于HASH的字节数组</param>
        /// <returns>是否相等</returns>
        public unsafe bool Equals(SubArrayStruct<byte> other)
        {
            if (_data.Array == null) return other.Array == null;
            if (other.Array != null && _data.Count == other.Count)
            {
                if (_data.Array == other.Array && _data.StartIndex == other.StartIndex) return true;
                fixed (byte* dataFixed = _data.Array, otherDataFixed = other.Array)
                {
                    return MemoryUnsafe.Equal(dataFixed + _data.StartIndex, otherDataFixed + other.StartIndex, _data.Count);
                }
            }
            return false;
        }
        /// <summary>
        /// 比较字节数组是否相等
        /// </summary>
        /// <param name="other">用于HASH的字节数组</param>
        /// <returns>是否相等</returns>
        public unsafe bool Equals(byte[] other)
        {
            if (_data.Array == null) return other == null;
            if (other != null && _data.Count == other.Length)
            {
                if (_data.Array == other && _data.StartIndex == 0) return true;
                fixed (byte* dataFixed = _data.Array) return MemoryUnsafe.Equal(other, dataFixed + _data.StartIndex, _data.Count);
            }
            return false;
        }
        /// <summary>
        /// 复制HASH字节数组
        /// </summary>
        /// <returns>HASH字节数组</returns>
        public HashBytesStruct Copy()
        {
            if (_data.Count == 0)
            {
                return new HashBytesStruct { _data = SubArrayStruct<byte>.Unsafe(NullValuePlus<byte>.Array, 0, 0), _hashCode = _hashCode };
            }
            var data = new byte[_data.Count];
            Buffer.BlockCopy(_data.Array, _data.StartIndex, data, 0, data.Length);
            return new HashBytesStruct { _data = SubArrayStruct<byte>.Unsafe(data, 0, data.Length), _hashCode = _hashCode };
        }
    }
}
