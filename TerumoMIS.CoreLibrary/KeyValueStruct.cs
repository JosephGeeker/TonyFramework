//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: KeyValuePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  KeyValuePlus
//	User name:  C1400008
//	Location Time: 2015/7/8 16:55:01
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System.Collections.Generic;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 键值对
    /// </summary>
    /// <typeparam name="TKeyType">键类型</typeparam>
    /// <typeparam name="TValueType">值类型</typeparam>
    public struct KeyValueStruct<TKeyType,TValueType>
    {
        /// <summary>
        /// 键
        /// </summary>
        public TKeyType Key;
        /// <summary>
        /// 值
        /// </summary>
        public TValueType Value;
        /// <summary>
        /// 键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public KeyValueStruct(TKeyType key, TValueType value)
        {
            Key = key;
            Value = value;
        }
        /// <summary>
        /// 重置键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Set(TKeyType key, TValueType value)
        {
            Key = key;
            Value = value;
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        public void Null()
        {
            Key = default(TKeyType);
            Value = default(TValueType);
        }

        /// <summary>
        /// 键值对隐式转换
        /// </summary>
        /// <param name="value">键值对</param>
        /// <returns>键值对</returns>
        public static implicit operator KeyValueStruct<TKeyType, TValueType>(KeyValuePair<TKeyType, TValueType> value)
        {
            return new KeyValueStruct<TKeyType, TValueType>(value.Key, value.Value);
        }
        /// <summary>
        /// 键值对隐式转换
        /// </summary>
        /// <param name="value">键值对</param>
        /// <returns>键值对</returns>
        public static implicit operator KeyValuePair<TKeyType, TValueType>(KeyValueStruct<TKeyType, TValueType> value)
        {
            return new KeyValuePair<TKeyType, TValueType>(value.Key, value.Value);
        }
    }
}
