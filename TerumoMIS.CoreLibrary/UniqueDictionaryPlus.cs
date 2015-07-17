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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 唯一静态哈希字典
    /// </summary>
    /// <typeparam name="TKeyType">键值类型</typeparam>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class UniqueDictionaryPlus<TKeyType,TValueType>
        where TKeyType:struct ,IEquatable<TKeyType>
    {
        /// <summary>
        /// 哈希数据数组
        /// </summary>
        private keyValue<TKeyType, TValueType>[] array;
        /// <summary>
        /// 唯一静态哈希字典
        /// </summary>
        /// <param name="keys">键值数据集合</param>
        /// <param name="getValue">数据获取器</param>
        /// <param name="size">哈希容器尺寸</param>
        public unsafe uniqueDictionary(TKeyType[] keys, Func<TKeyType, TValueType> getValue, int size)
        {
            int count = keys.length();
            if (count > size || size <= 0) log.Error.Throw(log.exceptionType.IndexOutOfRange);
            if (getValue == null) log.Error.Throw(log.exceptionType.Null);
            array = new keyValue<TKeyType, TValueType>[size];
            if (count != 0)
            {
                int length = ((size + 31) >> 5) << 2;
                byte* isValue = stackalloc byte[length];
                fixedMap map = new fixedMap(isValue, length, 0);
                foreach (TKeyType key in keys)
                {
                    int index = key.GetHashCode();
                    if ((uint)index >= size) log.Error.Throw(log.exceptionType.IndexOutOfRange);
                    if (map.Get(index)) log.Error.Throw(log.exceptionType.ErrorOperation);
                    map.Set(index);
                    array[index] = new keyValue<TKeyType, TValueType>(key, getValue(key));
                }
            }
        }
        /// <summary>
        /// 唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public uniqueDictionary(list<keyValue<TKeyType, TValueType>> values, int size)
        {
            int count = values.count();
            if (count > size || size <= 0) log.Error.Throw(log.exceptionType.IndexOutOfRange);
            array = new keyValue<TKeyType, TValueType>[size];
            if (count != 0) fromArray(values.array, count, size);
        }
        /// <summary>
        /// 唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public uniqueDictionary(keyValue<TKeyType, TValueType>[] values, int size)
        {
            int count = values.length();
            if (count > size || size <= 0) log.Error.Throw(log.exceptionType.IndexOutOfRange);
            array = new keyValue<TKeyType, TValueType>[size];
            if (count != 0) fromArray(values, count, size);
        }
        /// <summary>
        /// 唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="count">数据数量</param>
        /// <param name="size">哈希容器尺寸</param>
        private unsafe void fromArray(keyValue<TKeyType, TValueType>[] values, int count, int size)
        {
            int length = ((size + 31) >> 5) << 2;
            byte* isValue = stackalloc byte[length];
            fixedMap map = new fixedMap(isValue, length, 0);
            do
            {
                keyValue<TKeyType, TValueType> value = values[--count];
                int index = value.Key.GetHashCode();
                if ((uint)index >= size) log.Error.Throw(log.exceptionType.IndexOutOfRange);
                if (map.Get(index)) log.Error.Throw(log.exceptionType.ErrorOperation);
                map.Set(index);
                array[index] = value;
            }
            while (count != 0);
        }
        /// <summary>
        /// 判断是否存在某键值
        /// </summary>
        /// <param name="key">待匹配键值</param>
        /// <returns>是否存在某键值</returns>
        public bool ContainsKey(TKeyType key)
        {
            int index = key.GetHashCode();
            return (uint)index < array.Length && key.Equals(array[index].Key);
        }
        /// <summary>
        /// 获取匹配数据
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>匹配数据,失败返回默认空值</returns>
        public TValueType Get(TKeyType key, TValueType nullValue)
        {
            int index = key.GetHashCode();
            return (uint)index < array.Length && key.Equals(array[index].Key) ? array[index].Value : nullValue;
        }
    }
}
