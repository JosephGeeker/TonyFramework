//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DynamicArrayPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  DynamicArrayPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 16:13:28
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using TerumoMIS.CoreLibrary.Reflection;
using TerumoMIS.CoreLibrary.Threading;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 动态数组信息
    /// </summary>
    internal static class DynamicArrayPlus
    {
        /// <summary>
        /// 是否需要清除数组
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>需要清除数组</returns>
        public static bool IsClearArray(Type type)
        {
            if (type.IsPointer) return false;
            if (type.IsClass || type.IsInterface) return true;
            if (type.IsEnum) return false;
            if (type.IsValueType)
            {
                bool isClear;
                InterlockedPlus.CompareSetSleep(ref _isClearArrayLock);
                try
                {
                    if (IsClearArrayCache.TryGetValue(type, out isClear)) return isClear;
                    IsClearArrayCache.Add(type, isClear = IsClearArrayBase(type));
                }
                finally { _isClearArrayLock = 0; }
                return isClear;
            }
            else
            {
                LogPlus.Default.Add(type.FullName);
                return true;
            }
        }
        /// <summary>
        /// 是否需要清除数组
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>需要清除数组</returns>
        private static bool IsClearArrayBase(Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldType = field.FieldType;
                if (fieldType != type && !fieldType.IsPointer)
                {
                    if (fieldType.IsClass || fieldType.IsInterface) return true;
                    if (!fieldType.IsEnum)
                    {
                        if (fieldType.IsValueType)
                        {
                            bool isClear;
                            if (!IsClearArrayCache.TryGetValue(fieldType, out isClear))
                            {
                                IsClearArrayCache.Add(fieldType, isClear = IsClearArrayBase(fieldType));
                            }
                            if (isClear) return true;
                        }
                        else
                        {
                            LogPlus.Default.Add(fieldType.fullName());
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 是否需要清除数组缓存 访问锁
        /// </summary>
        private static int _isClearArrayLock;
        /// <summary>
        /// 是否需要清除数组缓存信息
        /// </summary>
        private static readonly Dictionary<Type, bool> IsClearArrayCache = DictionaryPlus.CreateOnly<Type, bool>();
    }
    /// <summary>
    /// 动态数组基类
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public abstract class DynamicArrayPlus<TValueType>
    {
        /// <summary>
        /// 是否需要清除数组
        /// </summary>
        internal static readonly bool IsClearArray = DynamicArrayPlus.IsClearArray(typeof(TValueType));
        /// <summary>
        /// 创建新数组
        /// </summary>
        /// <param name="length">数组长度</param>
        /// <returns>数组</returns>
        internal static TValueType[] GetNewArray(int length)
        {
            if (length > 0 && (length & (length - 1)) != 0) length = 1 << ((uint) length).Bits();
            if (length <= 0) length = int.MaxValue;
            return new TValueType[length];
        }

        /// <summary>
        /// 数据数组
        /// </summary>
        protected internal TValueType[] Array;
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get { return false; } }
        /// 数据数量
        /// 
        protected abstract int ValueCount { get; }

        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        public abstract void Add(TValueType[] values, int index, int count);
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(IEnumerable<TValueType> values)
        {
            if (values != null) Add(values.getSubArray());
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(TValueType[] values)
        {
            if (values != null) Add(values, 0, values.Length);
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="value">数据集合</param>
        public void Add(SubArrayStruct<TValueType> value)
        {
            if (value.Count != 0) Add(value.Array, value.StartIndex, value.Count);
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="value">数据集合</param>
        public void Add(ListPlus<TValueType> value)
        {
            if (value.Count != 0) Add(value.Array, 0, value.Count);
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="value">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        public void Add(ListPlus<TValueType> value, int index, int count)
        {
            if (value.Count != 0) Add(value.Array, index, count);
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>是否存在移除数据</returns>
        public bool Remove(TValueType value)
        {
            var index = IndexOf(value);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 判断是否存在匹配
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>是否存在匹配</returns>
        public bool Any(Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            return IndexOfBase(isValue) != -1;
        }
        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public abstract int IndexOf(TValueType value);
        /// <summary>
        /// 判断是否存在数据
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>是否存在数据</returns>
        public bool Contains(TValueType value)
        {
            return IndexOf(value) != -1;
        }
        /// <summary>
        /// 获取第一个匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值,失败为 default(valueType)</returns>
        public TValueType FirstOrDefault(Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            int index = IndexOfBase(isValue);
            return index != -1 ? Array[index] : default(TValueType);
        }
        /// <summary>
        /// 获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配值集合</returns>
        protected abstract TValueType[] GetFindArray(Func<TValueType, bool> isValue, FixedMapStruct map);
        /// <summary>
        /// 获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值集合</returns>
        public unsafe TValueType[] GetFindArray(Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            var length = ValueCount;
            if (length != 0)
            {
                var pool = MemoryPoolPlus.GetDefaultPool(length = ((length + 31) >> 5) << 2);
                var data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        Array.Clear(data, 0, length);
                        return GetFindArray(isValue, new FixedMapStruct(dataFixed));
                    }
                }
                finally { pool.Push(ref data); }
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 获取匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public abstract int IndexOf(Func<TValueType, bool> isValue);
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public abstract TValueType GetRemoveAt(int index);
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public abstract void RemoveAt(int index);
        /// <summary>
        /// 移除匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        public void RemoveFirst(Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            var index = IndexOf(isValue);
            if (index != -1) RemoveAt(index);
        }
        /// <summary>
        /// 移除匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>被移除的数据元素,失败返回default(valueType)</returns>
        public TValueType GetRemoveFirst(Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            int index = IndexOf(isValue);
            return index != -1 ? GetRemoveAt(index) : default(TValueType);
        }
        /// <summary>
        /// 获取数组中的匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>数组中的匹配位置,失败为-1</returns>
        protected abstract int IndexOfBase(Func<TValueType, bool> isValue);
        /// <summary>
        /// 替换数据
        /// </summary>
        /// <param name="value">新数据</param>
        /// <param name="isValue">数据匹配器</param>
        public void ReplaceFirst(TValueType value, Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            int index = IndexOfBase(isValue);
            if (index != -1) Array[index] = value;
        }
        /// <summary>
        /// 获取数据范围
        /// </summary>
        /// <typeparam name="TArrayType">目标数据类型</typeparam>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>目标数据</returns>
        protected abstract TArrayType[] GetRange<TArrayType>(int index, int count, Func<TValueType, TArrayType> getValue);
        /// <summary>
        /// 获取数据分页
        /// </summary>
        /// <typeparam name="TArrayType">目标数据类型</typeparam>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>目标数据</returns>
        public TArrayType[] GetPage<TArrayType>(int pageSize, int currentPage, Func<TValueType, TArrayType> getValue)
        {
            if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            var page = new ArrayPlus.PageStruct(ValueCount, pageSize, currentPage);
            return page.SkipCount < page.Count ? GetRange(page.SkipCount, page.CurrentPageSize, getValue) : NullValuePlus<TArrayType>.Array;
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public abstract bool Max(Func<TValueType, TValueType, int> comparer, out TValueType value);
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public abstract bool Max<TKeyType>
            (Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, out TValueType value);
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public TValueType Max<TKeyType>(Func<TValueType, TKeyType> getKey, TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return Max(getKey, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public TValueType Max<TKeyType>
            (Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TValueType nullValue)
        {
            TValueType value;
            return Max(getKey, comparer, out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最大键值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public TKeyType MaxKey<TKeyType>(Func<TValueType, TKeyType> getKey, TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return Max(getKey, (left, right) => left.CompareTo(right), out value) ? getKey(value) : nullValue;
        }
        /// <summary>
        /// 获取最大键值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public TKeyType MaxKey<TKeyType>
            (Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TKeyType nullValue)
        {
            TValueType value;
            return Max(getKey, comparer, out value) ? getKey(value) : nullValue;
        }
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public abstract bool Min(Func<TValueType, TValueType, int> comparer, out TValueType value);
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public abstract bool Min<TKeyType>
            (Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, out TValueType value);
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public TValueType Min<TKeyType>(Func<TValueType, TKeyType> getKey, TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return Min(getKey, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public TValueType Min<TKeyType>
            (Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TValueType nullValue)
        {
            TValueType value;
            return Min(getKey, comparer, out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最小键值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public TKeyType MinKey<TKeyType>(Func<TValueType, TKeyType> getKey, TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return Min(getKey, (left, right) => left.CompareTo(right), out value) ? getKey(value) : nullValue;
        }
        /// <summary>
        /// 获取最小键值
        /// </summary>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public TKeyType MinKey<TKeyType>
            (Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TKeyType nullValue)
        {
            TValueType value;
            return Min(getKey, comparer, out value) ? getKey(value) : nullValue;
        }
        /// <summary>
        /// 转换数据集合
        /// </summary>
        /// <typeparam name="TArrayType">目标数据类型</typeparam>
        /// <param name="getValue">数据转换器</param>
        /// <returns>数据集合</returns>
        public abstract TArrayType[] GetArray<TArrayType>(Func<TValueType, TArrayType> getValue);
        /// <summary>
        /// 转换键值对集合
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <param name="getKey">键值获取器</param>
        /// <returns>键值对数组</returns>
        public abstract KeyValueStruct<TKeyType, TValueType>[] GetKeyValueArray<TKeyType>(Func<TValueType, TKeyType> getKey);
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <param name="toString">字符串转换器</param>
        /// <returns>字符串</returns>
        public string JoinString(Func<TValueType, string> toString)
        {
            return string.Concat(GetArray(toString));
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接串</param>
        /// <returns>字符串</returns>
        public string JoinString(string join, Func<TValueType, string> toString)
        {
            return string.Join(join, GetArray(toString));
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接字符</param>
        /// <returns>字符串</returns>
        public string JoinString(char join, Func<TValueType, string> toString)
        {
            return GetArray(toString).JoinString(join);
        }
    }
}
