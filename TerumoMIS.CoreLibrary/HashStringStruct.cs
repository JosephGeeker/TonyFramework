//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HashStringPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  HashStringPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 9:07:51
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 字符串Hash
    /// </summary>
    public struct HashStringStruct:IEquatable<HashStringStruct>,IEquatable<SubStringStruct>,IEquatable<string>
    {
        /// <summary>
        /// 字符串
        /// </summary>
        private SubStringStruct _value;
        /// <summary>
        /// 字符串长度
        /// </summary>
        internal int Length
        {
            get { return _value.Length; }
        }
        /// <summary>
        /// 哈希值
        /// </summary>
        private int _hashCode;
        /// <summary>
        /// 清空数据
        /// </summary>
        internal void Null()
        {
            _value.Null();
            _hashCode = 0;
        }
        /// <summary>
        /// 字符串Hash
        /// </summary>
        /// <param name="value"></param>
        public HashStringStruct(SubStringStruct value)
        {
            _value = value;
            _hashCode = value == null ? 0 : (value.GetHashCode() ^ RandomPlus.Hash);
        }
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字符串</returns>
        public static implicit operator HashStringStruct(string value) { return new HashStringStruct(value); }
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字符串</returns>
        public static implicit operator HashStringStruct(SubStringStruct value) { return new HashStringStruct(value); }
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字符串</returns>
        public static implicit operator SubStringStruct(HashStringStruct value) { return value._value; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(HashStringStruct other)
        {
            return _hashCode == other._hashCode && _value.Equals(other._value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SubStringStruct other)
        {
            return _value.Equals(other);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(string other)
        {
            return _value.Equals(other);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals((HashStringStruct)obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _value == null ? null : _value.ToString();
        }
    }
}
