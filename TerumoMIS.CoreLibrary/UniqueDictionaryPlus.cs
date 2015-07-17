//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: UniqueDictionaryPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  UniqueDictionaryPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 17:02:54
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     唯一静态哈希字典
    /// </summary>
    /// <typeparam name="TKeyType">键值类型</typeparam>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class UniqueDictionaryPlus<TKeyType, TValueType>
        where TKeyType : struct, IEquatable<TKeyType>
    {
        /// <summary>
        ///     哈希数据数组
        /// </summary>
        private readonly KeyValueStruct<TKeyType, TValueType>[] _array;

        /// <summary>
        ///     唯一静态哈希字典
        /// </summary>
        /// <param name="keys">键值数据集合</param>
        /// <param name="getValue">数据获取器</param>
        /// <param name="size">哈希容器尺寸</param>
        public unsafe UniqueDictionaryPlus(IReadOnlyCollection<TKeyType> keys, Func<TKeyType, TValueType> getValue,
            int size)
        {
            var count = keys.Count;
            if (count > size || size <= 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            _array = new KeyValueStruct<TKeyType, TValueType>[size];
            if (count != 0)
            {
                var length = ((size + 31) >> 5) << 2;
                byte* isValue = stackalloc byte[length];
                var map = new FixedMapStruct(isValue, length);
                foreach (var key in keys)
                {
                    var index = key.GetHashCode();
                    if ((uint) index >= size) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
                    if (map.Get(index)) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
                    map.Set(index);
                    if (getValue != null) _array[index] = new KeyValueStruct<TKeyType, TValueType>(key, getValue(key));
                }
            }
        }

        /// <summary>
        ///     唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public UniqueDictionaryPlus(ListPlus<KeyValueStruct<TKeyType, TValueType>> values, int size)
        {
            var count = values.count();
            if (count > size || size <= 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            _array = new KeyValueStruct<TKeyType, TValueType>[size];
            if (count != 0) FromArray(values.Array, count, size);
        }

        /// <summary>
        ///     唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public UniqueDictionaryPlus(KeyValueStruct<TKeyType, TValueType>[] values, int size)
        {
            var count = values.Length;
            if (count > size || size <= 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            _array = new KeyValueStruct<TKeyType, TValueType>[size];
            if (count != 0) FromArray(values, count, size);
        }

        /// <summary>
        ///     唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="count">数据数量</param>
        /// <param name="size">哈希容器尺寸</param>
        private unsafe void FromArray(IReadOnlyList<KeyValueStruct<TKeyType, TValueType>> values, int count, int size)
        {
            var length = ((size + 31) >> 5) << 2;
            byte* isValue = stackalloc byte[length];
            var map = new FixedMapStruct(isValue, length);
            do
            {
                var value = values[--count];
                var index = value.Key.GetHashCode();
                if ((uint) index >= size) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
                if (map.Get(index)) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
                map.Set(index);
                _array[index] = value;
            } while (count != 0);
        }

        /// <summary>
        ///     判断是否存在某键值
        /// </summary>
        /// <param name="key">待匹配键值</param>
        /// <returns>是否存在某键值</returns>
        public bool ContainsKey(TKeyType key)
        {
            var index = key.GetHashCode();
            return (uint) index < _array.Length && key.Equals(_array[index].Key);
        }

        /// <summary>
        ///     获取匹配数据
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>匹配数据,失败返回默认空值</returns>
        public TValueType Get(TKeyType key, TValueType nullValue)
        {
            var index = key.GetHashCode();
            return (uint) index < _array.Length && key.Equals(_array[index].Key) ? _array[index].Value : nullValue;
        }
    }
}