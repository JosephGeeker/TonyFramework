﻿//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ICollectionStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  ICollectionStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 16:52:06
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
    /// ICollection泛型转换
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public struct ICollectionStruct<valueType>:ICollection<valueType>
    {
        /// <summary>
        /// ICollection数据集合
        /// </summary>
        private ICollection collection;
        /// <summary>
        /// ICollection泛型转换
        /// </summary>
        /// <param name="collection">ICollection数据集合</param>
        public iCollection(ICollection collection)
        {
            this.collection = collection;
        }
        /// <summary>
        /// 数据数量
        /// </summary>
        public int Count
        {
            get { return collection != null ? collection.Count : 0; }
        }
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get { return true; } }
        /// <summary>
        /// 枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator<valueType> IEnumerable<valueType>.GetEnumerator()
        {
            if (collection != null)
            {
                foreach (valueType value in collection) yield return value;
            }
        }
        /// <summary>
        /// 枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<valueType>)this).GetEnumerator();
        }
        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void Clear()
        {
            log.Error.Throw(log.exceptionType.ErrorOperation);
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Add(valueType value)
        {
            log.Error.Throw(log.exceptionType.ErrorOperation);
        }
        /// <summary>
        /// 移除数据(不可用)
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>是否存在移除数据</returns>
        public bool Remove(valueType value)
        {
            log.Error.Throw(log.exceptionType.ErrorOperation);
            return false;
        }
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <param name="values">目标数据</param>
        /// <param name="index">目标位置</param>
        public void CopyTo(valueType[] values, int index)
        {
            if (collection != null) collection.CopyTo(values, index);
        }
        /// <summary>
        /// 判断是否存在数据
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>是否存在数据</returns>
        public bool Contains(valueType value)
        {
            if (collection != null)
            {
                foreach (valueType nextValue in collection)
                {
                    if (nextValue.Equals(value)) return true;
                }
            }
            return false;
        }
    }
    /// <summary>
    /// 集合相关扩展
    /// </summary>
    public static partial class iCollection
    {
        /// <summary>
        /// ICollection泛型转换
        /// </summary>
        /// <param name="value">数据集合</param>
        /// <returns>泛型数据集合</returns>
        public static iCollection<valueType> toGeneric<valueType>(this ICollection value)
        {
            return new iCollection<valueType>(value);
        }
        /// <summary>
        /// 空值转数组
        /// </summary>
        /// <typeparam name="valueType">数据集合类型</typeparam>
        /// <param name="value">数据集合</param>
        /// <returns>非空数组</returns>
        public static ICollection<valueType> notNull<valueType>(this ICollection<valueType> value)
        {
            return value != null ? value : nullValue<valueType>.Array;
        }
        /// <summary>
        /// 获取数据数量
        /// </summary>
        /// <param name="value">数据集合</param>
        /// <returns>null为0</returns>
        public static int count<valueType>(this ICollection<valueType> value)
        {
            return value != null ? value.Count : 0;
        }
        /// <summary>
        /// 根据集合内容返回数组
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <param name="values">数据集合</param>
        /// <returns>数组</returns>
        public static valueType[] getArray<valueType>(this ICollection<valueType> values)
        {
            if (values.count() != 0)
            {
                valueType[] newValues = new valueType[values.Count];
                int index = 0;
                foreach (valueType value in values) newValues[index++] = value;
                if (index != newValues.Length) Array.Resize(ref newValues, index);
                return newValues;
            }
            return nullValue<valueType>.Array;
        }
        /// <summary>
        /// 根据集合内容返回数组
        /// </summary>
        /// <typeparam name="valueType">枚举值类型</typeparam>
        /// <typeparam name="arrayType">返回数组类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>数组</returns>
        public static arrayType[] getArray<valueType, arrayType>(this ICollection<valueType> values, Func<valueType, arrayType> getValue)
        {
            if (values.count() != 0)
            {
                if (getValue == null) log.Error.Throw(log.exceptionType.Null);
                arrayType[] newValues = new arrayType[values.Count];
                int index = 0;
                foreach (valueType value in values) newValues[index++] = getValue(value);
                if (index != newValues.Length) Array.Resize(ref newValues, index);
                return newValues;
            }
            return nullValue<arrayType>.Array;
        }
        /// <summary>
        /// 获取键值对数组
        /// </summary>
        /// <typeparam name="keyType">键类型</typeparam>
        /// <typeparam name="valueType">值类型</typeparam>
        /// <param name="values">键值对集合</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>键值对数组</returns>
        public static keyValue<keyType, valueType>[] getKeyValueArray<keyType, valueType>(this ICollection<valueType> values, Func<valueType, keyType> getKey)
        {
            if (values.count() != 0)
            {
                if (getKey == null) log.Error.Throw(log.exceptionType.Null);
                keyValue<keyType, valueType>[] newValues = new keyValue<keyType, valueType>[values.Count];
                int index = 0;
                foreach (valueType value in values) newValues[index++].Set(getKey(value), value);
                if (index != newValues.Length) Array.Resize(ref newValues, index);
                return newValues;
            }
            return nullValue<keyValue<keyType, valueType>>.Array;
        }

        /// <summary>
        /// 键值对转换
        /// </summary>
        /// <typeparam name="keyType">键类型</typeparam>
        /// <typeparam name="valueType">值类型</typeparam>
        /// <param name="values">键值对集合</param>
        /// <returns>键值对数组</returns>
        public static keyValue<keyType, valueType>[] getKeyValueArray<keyType, valueType>(this ICollection<KeyValuePair<keyType, valueType>> values)
        {
            if (values.count() != 0)
            {
                keyValue<keyType, valueType>[] newValues = new keyValue<keyType, valueType>[values.Count];
                int index = 0;
                foreach (KeyValuePair<keyType, valueType> value in values) newValues[index++].Set(value.Key, value.Value);
                if (index != newValues.Length) Array.Resize(ref newValues, index);
                return newValues;
            }
            return nullValue<keyValue<keyType, valueType>>.Array;
        }
        /// <summary>
        /// 根据集合内容返回单向动态数组
        /// </summary>
        /// <typeparam name="valueType">枚举值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <returns>单向动态数组</returns>
        public static list<valueType> getList<valueType>(this ICollection<valueType> values)
        {
            return values != null ? new list<valueType>(getArray(values), true) : null;
        }
        /// <summary>
        /// 根据集合内容返回单向列表
        /// </summary>
        /// <typeparam name="valueType">枚举值类型</typeparam>
        /// <typeparam name="arrayType">返回数组类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>单向列表</returns>
        public static list<arrayType> getList<valueType, arrayType>(this ICollection<valueType> values, Func<valueType, arrayType> getValue)
        {
            return values != null ? new list<arrayType>(getArray(values, getValue), true) : null;
        }
        /// <summary>
        /// 根据集合内容返回双向列表
        /// </summary>
        /// <typeparam name="valueType">枚举值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <returns>双向列表</returns>
        public static collection<valueType> getCollection<valueType>(this ICollection<valueType> values)
        {
            return values != null ? new collection<valueType>(getArray(values), true) : null;
        }
        /// <summary>
        /// 根据集合内容返回双向列表
        /// </summary>
        /// <typeparam name="valueType">枚举值类型</typeparam>
        /// <typeparam name="arrayType">返回数组类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>双向列表</returns>
        public static collection<arrayType> getCollection<valueType, arrayType>
            (this ICollection<valueType> values, Func<valueType, arrayType> getValue)
        {
            return values != null ? new collection<arrayType>(getArray(values, getValue), true) : null;
        }
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <param name="values">数据集合</param>
        /// <param name="array">目标数组</param>
        /// <param name="index">目标数组起始位置</param>
        public static void copyTo<valueType>(this ICollection<valueType> values, valueType[] array, int index)
        {
            if (values.count() != 0)
            {
                array.range range = new array.range(array.length(), index, values.Count);
                if (range.GetCount == values.Count)
                {
                    foreach (valueType value in values) array[index++] = value;
                }
                else log.Error.Throw(log.exceptionType.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <param name="values">数据集合</param>
        /// <param name="array">目标数组</param>
        /// <param name="index">目标数组起始位置</param>
        /// <param name="count">复制数据数量</param>
        public static void copyTo<valueType>(this ICollection<valueType> values, valueType[] array, int index, int count)
        {
            if (values.count() != 0)
            {
                if (count > values.Count) count = values.Count;
                array.range range = new array.range(array.length(), index, count);
                if (range.GetCount == count)
                {
                    foreach (valueType value in values) array[index++] = value;
                }
                else log.Error.Throw(log.exceptionType.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 查找符合条件的记录集合
        /// </summary>
        /// <typeparam name="valueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public static subArray<valueType> getFind<valueType>(this ICollection<valueType> values, Func<valueType, bool> isValue)
        {
            if (values.count() != 0)
            {
                if (isValue == null) log.Error.Throw(log.exceptionType.Null);
                valueType[] value = new valueType[values.Count];
                int count = 0;
                foreach (valueType nextValue in values)
                {
                    if (isValue(nextValue)) value[count++] = nextValue;
                }
                return subArray<valueType>.Unsafe(value, 0, count);
            }
            return default(subArray<valueType>);
        }
        /// <summary>
        /// 查找符合条件的记录集合
        /// </summary>
        /// <typeparam name="valueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public static list<valueType> getFindList<valueType>(this ICollection<valueType> values, Func<valueType, bool> isValue)
        {
            return getFind(values, isValue).ToList();
        }
        /// <summary>
        /// 查找符合条件的记录集合
        /// </summary>
        /// <typeparam name="valueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public static valueType[] getFindArray<valueType>(this ICollection<valueType> values, Func<valueType, bool> isValue)
        {
            return values != null ? getFind(values, isValue).ToArray().notNull() : nullValue<valueType>.Array;
        }
        /// <summary>
        /// 分组计数
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <typeparam name="keyType">分组键值类型</typeparam>
        /// <param name="values">数据集合</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>分组计数</returns>
        public static Dictionary<keyType, int> groupCount<valueType, keyType>
            (this ICollection<valueType> values, Func<valueType, keyType> getKey)
            where keyType : IEquatable<keyType>
        {
            return values.groupCount(getKey, values.count());
        }
        /// <summary>
        /// 将集合分隔成多个集合
        /// </summary>
        /// <typeparam name="valueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="split">分隔值</param>
        /// <returns>分隔后的值集合</returns>
        public static subArray<subArray<valueType>> getSplit<valueType>(this ICollection<valueType> values, valueType split)
            where valueType : IComparable<valueType>
        {
            if (values != null)
            {
                subArray<subArray<valueType>> value = new subArray<subArray<valueType>>();
                subArray<valueType> newValue = new subArray<valueType>();
                foreach (valueType nextValue in values)
                {
                    if (nextValue.CompareTo(split) == 0)
                    {
                        value.Add(newValue);
                        newValue.Null();
                    }
                    else newValue.Add(nextValue);
                }
                if (newValue.Count != 0) value.Add(newValue);
                return value;
            }
            return default(subArray<subArray<valueType>>);
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <returns>字符串</returns>
        public static string joinString<valueType>(this ICollection<valueType> values, Func<valueType, string> toString)
        {
            return string.Concat(values.getArray(toString));
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接串</param>
        /// <returns>字符串</returns>
        public static string joinString<valueType>(this ICollection<valueType> values, string join, Func<valueType, string> toString)
        {
            return string.Join(join, values.getArray(toString));
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接字符</param>
        /// <returns>字符串</returns>
        public static string joinString<valueType>(this ICollection<valueType> values, char join, Func<valueType, string> toString)
        {
            return values.getArray(toString).joinString(join);
        }
    }
}
