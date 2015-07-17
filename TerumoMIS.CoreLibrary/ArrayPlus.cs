//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ArrayPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  ArrayPlus
//	User name:  C1400008
//	Location Time: 2015/7/8 16:15:31
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using TerumoMIS.CoreLibrary.Algorithm;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 数组扩展操作
    /// </summary>
    public static partial class ArrayPlus
    {
        /// <summary>
        /// 数组数据
        /// </summary>
        public struct ValueStruct<TValueType> where TValueType : class
        {
            /// <summary>
            /// 数据对象
            /// </summary>
            public TValueType Value;
            /// <summary>
            /// 释放数据对象
            /// </summary>
            /// <returns>缓冲区</returns>
            public TValueType Free()
            {
                var value = Value;
                Value = null;
                return value;
            }
        }

        #region 数据记录范围
        /// <summary>
        /// 数据记录范围
        /// </summary>
        public struct RangeStruct
        {
            /// <summary>
            /// 数据总量
            /// </summary>
            private int _count;
            /// <summary>
            /// 起始位置
            /// </summary>
            private int _startIndex;
            /// <summary>
            /// 跳过记录数
            /// </summary>
            public int SkipCount
            {
                get { return _startIndex; }
            }
            /// <summary>
            /// 结束位置
            /// </summary>
            private int _endIndex;
            /// <summary>
            /// 结束位置
            /// </summary>
            public int EndIndex
            {
                get { return _endIndex; }
            }
            /// <summary>
            /// 获取记录数
            /// </summary>
            public int GetCount
            {
                get { return _endIndex - _startIndex; }
            }

            /// <summary>
            /// 数据记录范围
            /// </summary>
            /// <param name="count">数据总量</param>
            /// <param name="skipCount">跳过记录数</param>
            /// <param name="getCount">获取记录数</param>
            public RangeStruct(int count, int skipCount, int getCount)
            {
                _count = count < 0 ? 0 : count;
                if (skipCount < count && getCount != 0)
                {
                    if (getCount > 0)
                    {
                        if (skipCount >= 0)
                        {
                            _startIndex = skipCount;
                            if ((_endIndex = skipCount + getCount) > count) _endIndex = count;
                        }
                        else
                        {
                            _startIndex = 0;
                            if ((_endIndex = skipCount + getCount) > count) _endIndex = count;
                            else if (_endIndex < 0) _endIndex = 0;
                        }
                    }
                    else
                    {
                        _startIndex = skipCount >= 0 ? skipCount : 0;
                        _endIndex = count;
                    }
                }
                else _startIndex = _endIndex = 0;
            }
        }
        #endregion

        #region 分页记录范围
        /// <summary>
        /// 分页记录范围
        /// </summary>
        public struct PageStruct
        {
            /// <summary>
            /// 数据总量
            /// </summary>
            private int _count;
            /// <summary>
            /// 数据总量
            /// </summary>
            public int Count
            {
                get { return _count; }
            }
            /// <summary>
            /// 分页总数
            /// </summary>
            private int _pageCount;
            /// <summary>
            /// 分页总数
            /// </summary>
            public int PageCount
            {
                get { return _pageCount; }
            }
            /// <summary>
            /// 当前页号
            /// </summary>
            private int _currentPage;
            /// <summary>
            /// 当前页号
            /// </summary>
            public int CurrentPage
            {
                get { return _currentPage; }
            }
            /// <summary>
            /// 分页尺寸
            /// </summary>
            private int _pageSize;
            /// <summary>
            /// 分页尺寸
            /// </summary>
            public int PageSize
            {
                get { return _pageSize; }
            }
            /// <summary>
            /// 跳过记录数
            /// </summary>
            private int _skipCount;
            /// <summary>
            /// 跳过记录数
            /// </summary>
            public int SkipCount
            {
                get { return _skipCount; }
            }
            /// <summary>
            /// 逆序跳过记录数
            /// </summary>
            public int DescSkipCount
            {
                get { return _count - _skipCount - _currentPageSize; }
            }
            /// <summary>
            /// 当前页记录数
            /// </summary>
            private int _currentPageSize;
            /// <summary>
            /// 当前页记录数
            /// </summary>
            public int CurrentPageSize
            {
                get { return _currentPageSize; }
            }
            /// <summary>
            /// 分页记录范围
            /// </summary>
            /// <param name="count">数据总量</param>
            /// <param name="pageSize">分页尺寸</param>
            /// <param name="currentPage">页号</param>
            public PageStruct(int count, int pageSize, int currentPage)
            {
                _pageSize = pageSize > 0 ? pageSize : config.pub.Default.PageSize;
                _count = count < 0 ? 0 : count;
                _pageCount = (_count + _pageSize - 1) / _pageSize;
                if (_pageCount < 0) _pageCount = 0;
                _currentPage = currentPage > 0 ? (currentPage <= _pageCount ? currentPage : (_pageCount == 0 ? 1 : _pageCount)) : 1;
                _skipCount = (_currentPage - 1) * _pageSize;
                _currentPageSize = Math.Min(_skipCount + _pageSize, _count) - _skipCount;
            }
        }
        #endregion

        /// <summary>
        /// 获取数组长度
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>null为0</returns>
        public static int Length<TValueType>(this TValueType[] array)
        {
            return array != null ? array.Length : 0;
        }
        /// <summary>
        /// 空值转0长度数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>非空数组</returns>
        public static TValueType[] NotNull<TValueType>(this TValueType[] array)
        {
            return array ?? NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 根据索引位置获取数组元素值
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <param name="index">索引位置</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>数组元素值</returns>
        public static TValueType Get<TValueType>(this TValueType[] array, int index, TValueType nullValue)
        {
            return array != null && (uint)index < (uint)array.Length ? array[index] : nullValue;
        }
        /// <summary>
        /// 获取最后一个值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>最后一个值,失败为default(valueType)</returns>
        public static TValueType LastOrDefault<TValueType>(this TValueType[] array)
        {
            return array != null && array.Length != 0 ? array[array.Length - 1] : default(TValueType);
        }
        /// <summary>
        /// 获取第一个匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>第一个匹配值,失败为default(valueType)</returns>
        public static TValueType FirstOrDefault<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                foreach (TValueType value in array)
                {
                    if (isValue != null && isValue(value)) return value;
                }
            }
            return default(TValueType);
        }
        /// <summary>
        /// 获取第一个匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="index">起始位置</param>
        /// <returns>第一个匹配值,失败为default(valueType)</returns>
        public static TValueType FirstOrDefault<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue, int index)
        {
            if (array != null && (uint)index < (uint)array.Length)
            {
                if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                while (index != array.Length)
                {
                    if (isValue != null && isValue(array[index])) return array[index];
                    ++index;
                }
            }
            return default(TValueType);
        }
        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public static int IndexOf<TValueType>(this TValueType[] array, TValueType value)
        {
            return array != null ? Array.IndexOf(array, value) : -1;
        }
        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public static int IndexOf<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (array.Length() != 0)
            {
                for (var index = 0; index != array.Length; ++index)
                {
                    if (isValue(array[index])) return index;
                }
            }
            return -1;
        }
        #region 二分查找匹配值位置
        /// <summary>
        /// 二分查找匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryIndexOf<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return BinaryIndexOf(values, value, (left, right) => left.CompareTo(right));
        }
        /// <summary>
        /// 二分查找匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryIndexOf<TValueType>(this TValueType[] values, TValueType value
            , Func<TValueType, TValueType, int> comparer)
        {
            if (values != null && values.Length != 0)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);

                int average, start = 0, length = values.Length, cmp;
                if (comparer != null && comparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp < 0) length = average;
                        else if (cmp == 0) return average;
                        else start = average + 1;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp > 0) length = average;
                        else if (cmp == 0) return average;
                        else start = average + 1;
                    }
                }
            }
            return -1;
        }
        /// <summary>
        /// 二分查找匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer, bool isAscending)
        {
            return BinaryIndexOf(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }
        /// <summary>
        /// 二分查找匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer, Func<TValueType, TValueType, int> orderComparer)
        {
            if (values != null && values.Length != 0)
            {
                if (comparer == null || orderComparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);

                int average, start = 0, length = values.Length, cmp;
                if (orderComparer != null && orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp < 0) length = average;
                        else if (cmp == 0) return average;
                        else start = average + 1;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp > 0) length = average;
                        else if (cmp == 0) return average;
                        else start = average + 1;
                    }
                }
            }
            return -1;
        }
        #endregion

        #region 二分查找第一个匹配值位置
        /// <summary>
        /// 二分查找第一个匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryFirstIndexOf<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return BinaryFirstIndexOf(values, value, (left, right) => left.CompareTo(right));
        }
        /// <summary>
        /// 二分查找第一个匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryFirstIndexOf<TValueType>(this TValueType[] values, TValueType value, Func<TValueType, TValueType, int> comparer)
        {
            int index = -1;
            if (values != null && values.Length != 0)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);

                int average, start = 0, length = values.Length, cmp;
                if (comparer != null && comparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp <= 0)
                        {
                            length = average;
                            if (cmp == 0) index = average;
                        }
                        else start = average + 1;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp >= 0)
                        {
                            length = average;
                            if (cmp == 0) index = average;
                        }
                        else start = average + 1;
                    }
                }
            }
            return index;
        }
        /// <summary>
        /// 二分查找第一个匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryFirstIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , bool isAscending)
        {
            return BinaryFirstIndexOf(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }
        /// <summary>
        /// 二分查找第一个匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryFirstIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , Func<TValueType, TValueType, int> orderComparer)
        {
            var index = -1;
            if (values != null && values.Length != 0)
            {
                if (comparer == null || orderComparer == null) log.Error.Throw(log.exceptionType.Null);

                int average, start = 0, length = values.Length, cmp;
                if (orderComparer != null && orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp <= 0)
                        {
                            length = average;
                            if (cmp == 0) index = average;
                        }
                        else start = average + 1;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp >= 0)
                        {
                            length = average;
                            if (cmp == 0) index = average;
                        }
                        else start = average + 1;
                    }
                }
            }
            return index;
        }
        #endregion

        #region 二分查找最后一个匹配值位置
        /// <summary>
        /// 二分查找最后一个匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryLastIndexOf<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return BinaryLastIndexOf(values, value, (left, right) => left.CompareTo(right));
        }
        /// <summary>
        /// 二分查找最后一个匹配值位置
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryLastIndexOf<TValueType>(this TValueType[] values, TValueType value, Func<TValueType, TValueType, int> comparer)
        {
            int index = -1;
            if (values != null && values.Length != 0)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);

                int average, start = 0, length = values.Length, cmp;
                if (comparer != null && comparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp >= 0)
                        {
                            start = average + 1;
                            if (cmp == 0) index = average;
                        }
                        else length = average;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp <= 0)
                        {
                            start = average + 1;
                            if (cmp == 0) index = average;
                        }
                        else length = average;
                    }
                }
            }
            return index;
        }
        /// <summary>
        /// 二分查找最后一个匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryLastIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , bool isAscending)
        {
            return BinaryLastIndexOf(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }
        /// <summary>
        /// 二分查找最后一个匹配值位置
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值位置,失败返回-1</returns>
        public static int BinaryLastIndexOf<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , Func<TValueType, TValueType, int> orderComparer)
        {
            var index = -1;
            if (values != null && values.Length != 0)
            {
                if (comparer == null || orderComparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);

                int average, start = 0, length = values.Length, cmp;
                if (orderComparer != null && orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp >= 0)
                        {
                            start = average + 1;
                            if (cmp == 0) index = average;
                        }
                        else length = average;
                    }
                }
                else
                {
                    while (start < length)
                    {
                        average = start + ((length - start) >> 1);
                        cmp = comparer(value, values[average]);
                        if (cmp <= 0)
                        {
                            start = average + 1;
                            if (cmp == 0) index = average;
                        }
                        else length = average;
                    }
                }
            }
            return index;
        }
        #endregion

        #region 二分查找匹配值之后的位置(用于查找插入值的位置)
        /// <summary>
        /// 二分查找匹配值之后的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值之后的位置,失败返回-1</returns>
        public static int BinaryIndexOfThan<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return BinaryIndexOfThan(values, value, (left, right) => left.CompareTo(right));
        }
        /// <summary>
        /// 二分查找匹配值之后的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值之后的位置,失败返回-1</returns>
        public static int BinaryIndexOfThan<TValueType>(this TValueType[] values, TValueType value, Func<TValueType, TValueType, int> comparer)
        {
            var index = 0;
            if (values == null) index = -1;
            else if (values.Length != 0)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);

                int average, length = values.Length;
                if (comparer != null && comparer(values[0], values[length - 1]) <= 0)
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) >= 0) index = average + 1;
                        else length = average;
                    }
                }
                else
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) <= 0) index = average + 1;
                        else length = average;
                    }
                }
            }
            return index;
        }
        /// <summary>
        /// 二分查找匹配值之后的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值之后的位置,失败返回-1</returns>
        public static int BinaryIndexOfThan<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , bool isAscending)
        {
            return BinaryIndexOfThan(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }
        /// <summary>
        /// 二分查找匹配值之后的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值之后的位置,失败返回-1</returns>
        public static int BinaryIndexOfThan<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , Func<TValueType, TValueType, int> orderComparer)
        {
            var index = 0;
            if (values == null) index = -1;
            else if (values.Length != 0)
            {
                if (comparer == null || orderComparer == null) log.Error.Throw(log.exceptionType.Null);

                int average, length = values.Length;
                if (orderComparer != null && orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) >= 0) index = average + 1;
                        else length = average;
                    }
                }
                else
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) <= 0) index = average + 1;
                        else length = average;
                    }
                }
            }
            return index;
        }
        #endregion

        #region 二分查找匹配值之前的位置(用于查找插入值的位置)
        /// <summary>
        /// 二分查找匹配值之前的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <returns>匹配值之前的位置,失败返回-1</returns>
        public static int BinaryIndexOfLess<TValueType>(this TValueType[] values, TValueType value)
            where TValueType : IComparable<TValueType>
        {
            return BinaryIndexOfLess(values, value, (left, right) => left.CompareTo(right));
        }
        /// <summary>
        /// 二分查找匹配值之前的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">数组值比较器</param>
        /// <returns>匹配值之前的位置,失败返回-1</returns>
        public static int BinaryIndexOfLess<TValueType>(this TValueType[] values, TValueType value, Func<TValueType, TValueType, int> comparer)
        {
            int index = 0;
            if (values == null) index = -1;
            else if (values.Length != 0)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);

                int average, length = values.Length;
                if (comparer != null && comparer(values[0], values[length - 1]) <= 0)
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) > 0) index = average + 1;
                        else length = average;
                    }
                }
                else
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) < 0) index = average + 1;
                        else length = average;
                    }
                }
            }
            return index;
        }
        /// <summary>
        /// 二分查找匹配值之前的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="isAscending">是否升序</param>
        /// <returns>匹配值之前的位置,失败返回-1</returns>
        public static int BinaryIndexOfLess<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , bool isAscending)
        {
            return BinaryIndexOfLess(values, value, comparer, (left, right) => isAscending ? -1 : 1);
        }
        /// <summary>
        /// 二分查找匹配值之前的位置(用于查找插入值的位置)
        /// </summary>
        /// <typeparam name="TKeyType">查找值类型</typeparam>
        /// <typeparam name="TValueType">数组值类型</typeparam>
        /// <param name="value">匹配值</param>
        /// <param name="values">匹配数组</param>
        /// <param name="comparer">查找值比较器</param>
        /// <param name="orderComparer">数组值比较器</param>
        /// <returns>匹配值之前的位置,失败返回-1</returns>
        public static int BinaryIndexOfLess<TKeyType, TValueType>(this TValueType[] values, TKeyType value
            , Func<TKeyType, TValueType, int> comparer
            , Func<TValueType, TValueType, int> orderComparer)
        {
            var index = 0;
            if (values == null) index = -1;
            else if (values.Length != 0)
            {
                if (comparer == null || orderComparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);

                int average, length = values.Length;
                if (orderComparer != null && orderComparer(values[0], values[length - 1]) <= 0)
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) > 0) index = average + 1;
                        else length = average;
                    }
                }
                else
                {
                    while (index < length)
                    {
                        average = index + ((length - index) >> 1);
                        if (comparer(value, values[average]) < 0) index = average + 1;
                        else length = average;
                    }
                }
            }
            return index;
        }
        #endregion
        /// <summary>
        /// 判断是否存在匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">匹配值</param>
        /// <returns>是否存在匹配值</returns>
        public static bool Any<TValueType>(this TValueType[] array, TValueType value)
        {
            return array != null && Array.IndexOf(array, value) != -1;
        }
        /// <summary>
        /// 判断是否存在匹配值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>是否存在匹配值</returns>
        public static bool Any<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                return Enumerable.Any(array, value => isValue != null && isValue(value));
            }
            return false;
        }
        /// <summary>
        /// 获取匹配数量
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数量</returns>
        public static int Count<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            var count = 0;
            if (array != null)
            {
                if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                count += Enumerable.Count(array, value => isValue != null && isValue(value));
            }
            return count;
        }
        /// <summary>
        /// 复制数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待复制数组</param>
        /// <returns>复制后的新数组</returns>
        public static TValueType[] Copy<TValueType>(this TValueType[] array)
        {
            if (array.Length() != 0)
            {
                TValueType[] newValues = new TValueType[array.Length];
                Array.Copy(array, 0, newValues, 0, array.Length);
                return newValues;
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 翻转数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>翻转后的数组</returns>
        public static TValueType[] Reverse<TValueType>(this TValueType[] array)
        {
            if (array.Length() != 0)
            {
                Array.Reverse(array);
                return array;
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">添加的数据</param>
        /// <returns>添加数据的数组</returns>
        public static TValueType[] GetAdd<TValueType>(this TValueType[] array, TValueType value)
        {
            if (array != null)
            {
                var newValues = new TValueType[array.Length + 1];
                Array.Copy(array, 0, newValues, 0, array.Length);
                newValues[array.Length] = value;
                return newValues;
            }
            return new TValueType[] { value };
        }
        /// <summary>
        /// 获取前端子段集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">原数组</param>
        /// <param name="count">数量</param>
        /// <returns>子段集合</returns>
        public static subArray<TValueType> Left<TValueType>(this TValueType[] array, int count)
        {
            return array != null ? subArray<TValueType>.Unsafe(array, 0, count <= array.Length ? count : array.Length) : default(subArray<TValueType>);
        }
        /// <summary>
        /// 获取子段集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <returns>子段集合</returns>
        public static subArray<TValueType> Sub<TValueType>(this TValueType[] array, int index)
        {
            return array != null ? Sub(array, index, -1) : default(subArray<TValueType>);
        }
        /// <summary>
        /// 获取子段集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子段集合</returns>
        public static subArray<TValueType> Sub<TValueType>(this TValueType[] array, int index, int count)
        {
            if (array != null)
            {
                RangeStruct range = new RangeStruct(array.Length, index, count);
                return subArray<TValueType>.Unsafe(array, range.SkipCount, range.GetCount);
            }
            return default(subArray<TValueType>);
        }
        /// <summary>
        /// 取子集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>子集合</returns>
        public static TValueType[] GetSub<TValueType>(this TValueType[] array, int index, int count)
        {
            return array.Sub(index, count).GetArray();
        }
        /// <summary>
        /// 获取分页字段数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TArrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <param name="getValue">数据转换器</param>
        /// <returns>分页字段数组</returns>
        public static TArrayType[] GetPage<TValueType, TArrayType>
            (this TValueType[] array, int pageSize, int currentPage, Func<TValueType, TArrayType> getValue)
        {
            var page = new PageStruct(array.Length(), pageSize, currentPage);
            return array.Sub(page.SkipCount, page.CurrentPageSize).GetArray(getValue);
        }
        /// <summary>
        /// 获取分页字段数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="arrayType">目标数组类型</typeparam>
        /// <param name="array"></param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <param name="getValue"></param>
        /// <returns>分页字段数组</returns>
        public static TValueType[] GetPage<TValueType>(this TValueType[] array, int pageSize, int currentPage)
        {
            var page = new PageStruct(array.Length(), pageSize, currentPage);
            return array.GetSub(page.SkipCount, page.CurrentPageSize);
        }
        /// <summary>
        /// 移除第一个匹配数据数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">待移除的数据</param>
        /// <returns>移除数据后的数组</returns>
        public static TValueType[] RemoveFirst<TValueType>(this TValueType[] array, TValueType value) where TValueType : IComparable<TValueType>
        {
            if (array != null)
            {
                int index = Array.IndexOf(array, value);
                if (index != -1) return unsafer.array.GetRemoveAt(array, index);
                return array;
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 移除第一个匹配数据数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>移除数据后的数组</returns>
        public static TValueType[] RemoveFirst<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (array != null)
            {
                int index = 0;
                while (isValue != null && (index != array.Length && !isValue(array[index]))) ++index;
                if (index != array.Length) return unsafer.array.GetRemoveAt(array, index);
            }
            return array.NotNull();
        }
        /// <summary>
        /// 替换数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="value">新值</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>替换数据后的数组</returns>
        public static TValueType[] ReplaceFirst<TValueType>(this TValueType[] array, TValueType value, Func<TValueType, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) log.Error.Throw(log.exceptionType.Null);
                for (var index = 0; index != array.Length; ++index)
                {
                    if (isValue != null && isValue(array[index])) array[index] = value;
                }
            }
            return array.NotNull();
        }
        /// <summary>
        /// 移动数据块
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待处理数组</param>
        /// <param name="index">原始数据位置</param>
        /// <param name="writeIndex">目标数据位置</param>
        /// <param name="count">移动数据数量</param>
        public static void Move<TValueType>(this TValueType[] array, int index, int writeIndex, int count)
        {
            if (count > 0)
            {
                var writeEndIndex = writeIndex + count;
                if (index >= 0 && writeEndIndex <= array.Length)
                {
                    int endIndex = index + count;
                    if (index < writeIndex && endIndex > writeIndex)
                    {
                        while (endIndex != index) array[--writeEndIndex] = array[--endIndex];
                    }
                    else if (writeIndex >= 0 && endIndex <= array.Length) Array.Copy(array, index, array, writeIndex, count);
                    else LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
                }
                else LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            }
            else if (count != 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
        }
        /// <summary>
        /// 转换成数组子串
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <returns>单向列表</returns>
        public static subArray<TValueType> ToSubArray<TValueType>(this TValueType[] array)
        {
            if (array == null) return default(subArray<TValueType>);
            return subArray<TValueType>.Unsafe(array, 0, array.Length);
        }
        /// <summary>
        /// 转换成数组子串
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>单向列表</returns>
        public static subArray<TValueType> ToSubArray<TValueType>(this TValueType[] array, int index, int length)
        {
            return new subArray<TValueType>(array, index, length);
        }
        /// <summary>
        /// 根据集合内容返回单向列表
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <returns>单向列表</returns>
        public static list<TValueType> ToList<TValueType>(this TValueType[] array)
        {
            return new list<TValueType>(array, true);
        }
        /// <summary>
        /// 根据集合内容返回双向列表
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <returns>双向列表</returns>
        public static collection<TValueType> ToCollection<TValueType>(this TValueType[] array)
        {
            return new collection<TValueType>(array, true);
        }
        /// <summary>
        /// 转换HASH
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="array">值集合</param>
        /// <returns>HASH</returns>
        public static HashSet<TValueType> GetHash<TValueType>(this TValueType[] array)
        {
            if (array != null)
            {
                HashSet<TValueType> hash = HashSetPlus<TValueType>.Create();
                foreach (var value in array) hash.Add(value);
                return hash;
            }
            return null;
        }
        /// <summary>
        /// 数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TArrayType">目标数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合</returns>
        public static subArray<TArrayType> Distinct<TValueType, TArrayType>(this TValueType[] array, Func<TValueType, TArrayType> getValue)
        {
            if (array != null)
            {
                if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                TArrayType[] newValues = new TArrayType[array.Length];
                HashSet<TValueType> hash = HashSetPlus<TValueType>.Create();
                int count = 0;
                foreach (TValueType value in array)
                {
                    if (!hash.Contains(value))
                    {
                        if (getValue != null) newValues[count++] = getValue(value);
                        hash.Add(value);
                    }
                }
                return subArray<TArrayType>.Unsafe(newValues, 0, count);
            }
            return default(subArray<TArrayType>);
        }
        /// <summary>
        /// 转换HASH
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>HASH</returns>
        public static HashSet<TValueType> GetHash<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                HashSet<TValueType> hash = HashSetPlus<TValueType>.Create();
                foreach (TValueType value in array.Where(value => isValue != null && isValue(value)))
                {
                    hash.Add(value);
                }
                return hash;
            }
            return null;
        }
        /// <summary>
        /// 转换HASH
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="THashType">目标数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>HASH</returns>
        public static HashSet<THashType> GetHash<TValueType, THashType>(this TValueType[] array, Func<TValueType, THashType> getValue)
        {
            if (array != null)
            {
                if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                HashSet<THashType> hash = HashSetPlus<THashType>.Create();
                foreach (var value in array) if (getValue != null) hash.Add(getValue(value));
                return hash;
            }
            return null;
        }
        /// <summary>
        /// 将数组转化为字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">默认值类型</typeparam>
        /// <param name="array">待转化的数组</param>
        /// <param name="defaultValue">默认值数组</param>
        /// <returns>数组转化后字典</returns>
        public static Dictionary<TKeyType, TValueType> GetDictionary<TKeyType, TValueType>(this TKeyType[] array, TValueType[] defaultValue)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                Dictionary<TKeyType, TValueType> dictionary = DictionaryPlus<TKeyType>.Create<TValueType>(array.Length << 1);
                if (defaultValue.Length() == array.Length)
                {
                    var index = 0;
                    foreach (var key in array) dictionary.Add(key, defaultValue[index++]);
                }
                return dictionary;
            }
            return null;
        }
        /// <summary>
        /// 将数组转化为字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">默认值类型</typeparam>
        /// <param name="array">待转化的数组</param>
        /// <param name="defaultValue">默认值数组</param>
        /// <returns>数组转化后字典</returns>
        public static Dictionary<TKeyType, TValueType> GetDictionary<TKeyType, TValueType>(this TKeyType[] array, TValueType defaultValue)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                Dictionary<TKeyType, TValueType> dictionary = DictionaryPlus<TKeyType>.Create<TValueType>(array.Length << 1);
                foreach (var key in array) dictionary.Add(key, defaultValue);
                return dictionary;
            }
            return null;
        }
        /// <summary>
        /// 将键值数组转化为字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">默认值类型</typeparam>
        /// <param name="array">键值数组</param>
        /// <returns>数组转化后字典</returns>
        public static Dictionary<TKeyType, TValueType> GetDictionary<TKeyType, TValueType>(this KeyValueStruct<TKeyType, TValueType>[] array)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                Dictionary<TKeyType, TValueType> dictionary = DictionaryPlus<TKeyType>.Create<TValueType>(array.Length << 1);
                foreach (KeyValueStruct<TKeyType, TValueType> value in array) dictionary.Add(value.Key, value.Value);
                return dictionary;
            }
            return null;
        }
        /// <summary>
        /// 数据分组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">分组键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <returns>分组数据</returns>
        public static Dictionary<TKeyType, list<TValueType>> Group<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                if (getKey == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                Dictionary<TKeyType, list<TValueType>> newValues = DictionaryPlus<TKeyType>.Create<list<TValueType>>(array.Length << 1);
                list<TValueType> list;
                foreach (TValueType value in array)
                {
                    TKeyType key = getKey(value);
                    if (!newValues.TryGetValue(key, out list)) newValues[key] = list = new list<TValueType>();
                    list.Add(value);
                }
                return newValues;
            }
            return null;
        }
        /// <summary>
        /// 分组计数
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">分组键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>分组计数</returns>
        public static Dictionary<TKeyType, int> GroupCount<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey)
            where TKeyType : IEquatable<TKeyType>
        {
            if (array != null)
            {
                if (getKey == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                Dictionary<TKeyType, int> dictionary = DictionaryPlus<TKeyType>.Create<int>(array.Length);
                int count;
                foreach (var key in array.Select(value => getKey != null ? getKey(value) : default(TKeyType)))
                {
                    if (dictionary.TryGetValue(key, out count)) dictionary[key] = count + 1;
                    else dictionary.Add(key, 1);
                }
                return dictionary;
            }
            return null;
        }
        /// <summary>
        /// HASH统计
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>HASH统计</returns>
        public static Dictionary<TValueType, int> ValueCount<TValueType>(this TValueType[] array)
        {
            if (array != null)
            {
                int count;
                Dictionary<TValueType, int> dictionary = fastCSharp.dictionary.CreateAny<TValueType, int>(array.Length << 1);
                foreach (TValueType value in array)
                {
                    if (dictionary.TryGetValue(value, out count)) dictionary[value] = ++count;
                    else dictionary.Add(value, 1);
                }
                return dictionary;
            }
            return null;
        }
        /// <summary>
        /// 求交集
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="left">左侧数据</param>
        /// <param name="right">右侧数据</param>
        /// <returns>数据交集</returns>
        public static subArray<TValueType> Intersect<TValueType>(this TValueType[] left, TValueType[] right)
        {
            int leftLength = left.Length();
            if (leftLength != 0)
            {
                int rightLength = right.Length();
                if (rightLength != 0)
                {
                    TValueType[] min = leftLength <= rightLength ? left : right, values = new TValueType[min.Length];
                    HashSet<TValueType> hash = HashSetPlus<TValueType>.Create();
                    int count = 0;
                    foreach (TValueType value in leftLength <= rightLength ? right : left)
                    {
                        if (hash.Contains(value)) values[count++] = value;
                    }
                    return subArray<TValueType>.Unsafe(values, 0, count);
                }
            }
            return default(subArray<TValueType>);
        }
        /// <summary>
        /// 遍历foreach
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="method">调用函数</param>
        /// <returns>数组数据</returns>
        public static TValueType[] Each<TValueType>(this TValueType[] array, Action<TValueType> method)
        {
            if (array != null)
            {
                if (method == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                foreach (var value in array) if (method != null) method(value);
            }
            return array.NotNull();
        }
        /// <summary>
        /// 根据集合内容返回新的数据集合
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <typeparam name="TArrayType">返回数组类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>数据集合</returns>
        public static IEnumerable<TArrayType> GetEnumerable<TValueType, TArrayType>(this TValueType[] values, Func<TValueType, TArrayType> getValue)
        {
            if (values != null)
            {
                foreach (TValueType value in values) yield return getValue(value);
            }
        }
        /// <summary>
        /// 数据转换
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TArrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public static TArrayType[] GetArray<TValueType, TArrayType>(this TValueType[] array, Func<TValueType, TArrayType> getValue)
        {
            if (array.Length() != 0)
            {
                if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                var newValues = new TArrayType[array.Length];
                var index = 0;
                foreach (var value in array) newValues[index++] = getValue(value);
                return newValues;
            }
            return NullValuePlus<TArrayType>.Array;
        }
        /// <summary>
        /// 获取键值对数组
        /// </summary>
        /// <typeparam name="TKeyType">键类型</typeparam>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="array">键值对数组</param>
        /// <param name="getKey">键值获取器</param>
        /// <returns>键值对数组</returns>
        public static KeyValueStruct<TKeyType, TValueType>[] GetKeyValueArray<TKeyType, TValueType>(this TValueType[] array, Func<TValueType, TKeyType> getKey)
        {
            if (array.Length() != 0)
            {
                if (getKey == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                KeyValueStruct<TKeyType, TValueType>[] newValues = new KeyValueStruct<TKeyType, TValueType>[array.Length];
                int index = 0;
                foreach (TValueType value in array) newValues[index++].Set(getKey(value), value);
                return newValues;
            }
            return NullValuePlus<KeyValueStruct<TKeyType, TValueType>>.Array;
        }
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <typeparam name="TValueType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <returns>目标数组</returns>
        public static TValueType[] ToArray<TValueType>(this Array array)
        {
            return array != null ? array as TValueType[] : NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 数据转换
        /// </summary>
        /// <typeparam name="TValueType">目标数组类型</typeparam>
        /// <typeparam name="TArrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">一次处理两个数据</param>
        /// <returns>目标数组</returns>
        public static TArrayType[] GetArray<TValueType, TArrayType>(this TValueType[] array, Func<TValueType, TValueType, TArrayType> getValue)
        {
            if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (array.Length() != 0)
            {
                int length = array.Length, index = (length + 1) >> 1;
                var newValues = new TArrayType[index];
                if ((length & 1) != 0)
                    if (getValue != null) newValues[--index] = getValue(array[--length], default(TValueType));
                while (--index >= 0)
                {
                    var right = array[--length];
                    if (getValue != null) newValues[index] = getValue(array[--length], right);
                }
                return newValues;
            }
            return NullValuePlus<TArrayType>.Array;
        }
        /// <summary>
        /// 连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <returns>连接后的数组</returns>
        private static TValueType[] GetArray<TValueType>(TValueType[][] array)
        {
            var length = 0;
            foreach (TValueType[] value in array)
            {
                if (value != null) length += value.Length;
            }
            if (length != 0)
            {
                var newValues = new TValueType[length];
                length = 0;
                foreach (var value in array.Where(value => value != null))
                {
                    value.CopyTo(newValues, length);
                    length += value.Length;
                }
                return newValues;
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <returns>连接后的数组</returns>
        public static TValueType[] ToArray<TValueType>(this TValueType[][] array)
        {
            if (array.Length() != 0)
            {
                return array.Length == 1 ? array[0].NotNull() : GetArray(array);
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <returns>连接后的数组</returns>
        public static TValueType[] GetArrayBase<TValueType>(this TValueType[][] array)
        {
            if (array.Length() != 0)
            {
                if (array.Length != 1) return GetArray(array);
                return array[0].Copy();
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <param name="addValues">数组集合</param>
        /// <returns>连接后的数组</returns>
        public static TValueType[] Concat<TValueType>(this TValueType[] array, TValueType[] addValues)
        {
            return GetArrayBase(new TValueType[][] { array, addValues });
        }
        /// <summary>
        /// 连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组集合</param>
        /// <returns>连接后的数组</returns>
        public static TValueType[] Concat<TValueType>(params TValueType[][] array)
        {
            return array.GetArrayBase();
        }
        /// <summary>
        /// 连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TArrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public static TArrayType[] Concat<TValueType, TArrayType>(this TValueType[] array, Func<TValueType, TArrayType[]> getValue)
        {
            if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            return array.GetArray(value => getValue != null ? getValue(value) : new TArrayType[] { }).ToArray();
        }
        /// <summary>
        /// 连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TArrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public static subArray<TArrayType> Concat<TValueType, TArrayType>(this TValueType[] array, Func<TValueType, subArray<TArrayType>> getValue)
        {
            if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (array != null)
            {
                subArray<TArrayType> values = new subArray<TArrayType>();
                foreach (var value in array) if (getValue != null) values.Add(getValue(value));
                return values;
            }
            return default(subArray<TArrayType>);
        }
        /// <summary>
        /// 分割数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="count">子数组长度</param>
        /// <returns>分割后的数组集合</returns>
        public static subArray<TValueType>[] Split<TValueType>(this TValueType[] array, int count)
        {
            if (array != null && array.Length != 0 && count > 0)
            {
                var length = (array.Length + count - 1) / count;
                subArray<TValueType>[] newValues = new subArray<TValueType>[length];
                int index = (--length) * count, lastCount = array.Length - index;
                if (lastCount != 0) newValues[length--].UnsafeSet(array, index, lastCount);
                while (index != 0) newValues[length--].UnsafeSet(array, index -= count, count);
                return newValues;
            }
            return NullValuePlus<subArray<TValueType>>.Array;
        }
        /// <summary>
        /// 分割数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="count">子数组长度</param>
        /// <returns>分割后的数组集合</returns>
        public static TValueType[][] GetSplit<TValueType>(this TValueType[] array, int count)
        {
            if (array != null && count > 0)
            {
                if (count < array.Length)
                {
                    int length = (array.Length + count - 1) / count, copyIndex = 0;
                    var newValues = new TValueType[length--][];
                    for (var index = 0; index != length; ++index, copyIndex += count)
                    {
                        Array.Copy(array, copyIndex, newValues[index] = new TValueType[count], 0, count);
                    }
                    Array.Copy(array, copyIndex, newValues[length] = new TValueType[count = array.Length - copyIndex], 0, count);
                    return newValues;
                }
                if (array.Length != 0) return new TValueType[][] { array };
            }
            return NullValuePlus<TValueType[]>.Array;
        }
        /// <summary>
        /// 转换键值对集合
        /// </summary>
        /// <typeparam name="TKeyType">键值类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">键值数组</param>
        /// <param name="values">数组数据</param>
        /// <returns>键值对数组</returns>
        public static KeyValueStruct<TKeyType, TValueType>[] GetKeyValue<TKeyType, TValueType>(this TKeyType[] array, TValueType[] values)
        {
            int length = array.Length();
            if (length != values.Length()) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            if (length != 0)
            {
                KeyValueStruct<TKeyType, TValueType>[] newValues = new KeyValueStruct<TKeyType, TValueType>[array.Length];
                int index = 0;
                foreach (TKeyType key in array)
                {
                    newValues[index].Set(key, values[index]);
                    ++index;
                }
                return newValues;
            }
            return NullValuePlus<KeyValueStruct<TKeyType, TValueType>>.Array;
        }
        /// <summary>
        /// 获取匹配集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配集合</returns>
        public static subArray<TValueType> GetFind<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (array != null)
            {
                var length = array.Length;
                if (length != 0)
                {
                    var newValues = new TValueType[array.Length < sizeof(int) ? sizeof(int) : length];
                    length = 0;
                    foreach (var value in array)
                    {
                        if (isValue != null && isValue(value)) newValues[length++] = value;
                    }
                    return subArray<TValueType>.Unsafe(newValues, 0, length);
                }
            }
            return default(subArray<TValueType>);
        }
        /// <summary>
        /// 获取匹配数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数组</returns>
        public unsafe static TValueType[] GetFindArray<TValueType>(this TValueType[] array, Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            var length = array.Length();
            if (length != 0)
            {
                memoryPool pool = fastCSharp.memoryPool.GetDefaultPool(length = ((length + 31) >> 5) << 2);
                byte[] data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        Array.Clear(data, 0, length);
                        return GetFindArray(array, isValue, new fixedMap(dataFixed));
                    }
                }
                finally { pool.Push(ref data); }
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 获取匹配数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配数组</returns>
        private static TValueType[] GetFindArray<TValueType>(TValueType[] array, Func<TValueType, bool> isValue, fixedMap map)
        {
            var length = 0;
            for (var index = 0; index != array.Length; ++index)
            {
                if (isValue(array[index]))
                {
                    ++length;
                    map.Set(index);
                }
            }
            if (length != 0)
            {
                var newValues = new TValueType[length];
                for (var index = array.Length; length != 0; )
                {
                    if (map.Get(--index)) newValues[--length] = array[index];
                }
                return newValues;
            }
            return nullValue<TValueType>.Array;
        }
        /// <summary>
        /// 获取匹配数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TArrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>匹配数组</returns>
        public unsafe static TArrayType[] GetFindArray<TValueType, TArrayType>
            (this TValueType[] array, Func<TValueType, bool> isValue, Func<TValueType, TArrayType> getValue)
        {
            if (isValue == null || getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            int length = array.Length();
            if (length != 0)
            {
                memoryPool pool = fastCSharp.memoryPool.GetDefaultPool(length = ((length + 31) >> 5) << 2);
                byte[] data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        Array.Clear(data, 0, length);
                        return GetFindArray(array, isValue, getValue, new fixedMap(dataFixed));
                    }
                }
                finally { pool.Push(ref data); }
            }
            return NullValuePlus<TArrayType>.Array;
        }
        /// <summary>
        /// 获取匹配数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TArrayType">目标数组类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="getValue">数据获取器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配数组</returns>
        private static TArrayType[] GetFindArray<TValueType, TArrayType>
            (TValueType[] array, Func<TValueType, bool> isValue, Func<TValueType, TArrayType> getValue, fixedMap map)
        {
            var length = 0;
            for (int index = 0; index != array.Length; ++index)
            {
                if (isValue(array[index]))
                {
                    ++length;
                    map.Set(index);
                }
            }
            if (length != 0)
            {
                var newValues = new TArrayType[length];
                for (var index = array.Length; length != 0; )
                {
                    if (map.Get(--index)) newValues[--length] = getValue(array[index]);
                }
                return newValues;
            }
            return NullValuePlus<TArrayType>.Array;
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <returns>排序后的数组</returns>
        public static TValueType[] Sort<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer)
        {
            QuickSortPlus.Sort(array, comparer);
            return array.NotNull();
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <returns>排序后的新数组</returns>
        public static TValueType[] GetSort<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer)
        {
            return QuickSortPlus.GetSort(array, comparer);
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        public static TValueType[] Sort<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer, int startIndex, int count)
        {
            QuickSortPlus.Sort(array, comparer, startIndex, count);
            return array.NotNull();
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的新数组</returns>
        public static TValueType[] GetSort<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer, int startIndex, int count)
        {
            return QuickSortPlus.GetSort(array, comparer, startIndex, count);
        }
        /// <summary>
        /// 范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static subArray<TValueType> RangeSort<TValueType>
            (this TValueType[] array, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            return QuickSortPlus.RangeSort(array, comparer, skipCount, getCount);
        }
        /// <summary>
        /// 范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static subArray<TValueType> GetRangeSort<TValueType>
            (this TValueType[] array, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            return QuickSortPlus.GetRangeSort(array, comparer, skipCount, getCount);
        }
        /// <summary>
        /// 范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">结束位置</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序范围数组</returns>
        public static subArray<TValueType> RangeSort<TValueType>
            (this TValueType[] array, int startIndex, int count, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            return QuickSortPlus.RangeSort(array, startIndex, count, comparer, skipCount, getCount);
        }
        /// <summary>
        /// 范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">结束位置</param>
        /// <param name="comparer">比较器</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序范围数组</returns>
        public static subArray<TValueType> GetRangeSort<TValueType>
            (this TValueType[] array, int startIndex, int count, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            return QuickSortPlus.GetRangeSort(array, startIndex, count, comparer, skipCount, getCount);
        }
        /// <summary>
        /// 分页排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static subArray<TValueType> PageSort<TValueType>
            (this TValueType[] array, Func<TValueType, TValueType, int> comparer, int pageSize, int currentPage)
        {
            PageStruct page = new PageStruct(array.Length(), pageSize, currentPage);
            return QuickSortPlus.RangeSort(array, comparer, page.SkipCount, page.CurrentPageSize);
        }
        /// <summary>
        /// 分页排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="comparer">比较器</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static subArray<TValueType> GetPageSort<TValueType>
            (this TValueType[] array, Func<TValueType, TValueType, int> comparer, int pageSize, int currentPage)
        {
            var page = new PageStruct(array.Length(), pageSize, currentPage);
            return QuickSortPlus.GetRangeSort(array, comparer, page.SkipCount, page.CurrentPageSize);
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool Max<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer, out TValueType value)
        {
            if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (array.Length() != 0)
            {
                value = array[0];
                foreach (var nextValue in array)
                {
                    if (comparer != null && comparer(nextValue, value) > 0) value = nextValue;
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool Max<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, out TValueType value)
        {
            if (getKey == null || comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (array.Length() != 0)
            {
                value = array[0];
                if (array.Length != 1)
                {
                    if (getKey != null)
                    {
                        var key = getKey(value);
                        foreach (var nextValue in array)
                        {
                            var nextKey = getKey(nextValue);
                            if (comparer != null && comparer(nextKey, key) > 0)
                            {
                                value = nextValue;
                                key = nextKey;
                            }
                        }
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType Max<TValueType>(this TValueType[] array, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            TValueType value;
            return Max(array, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType Max<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey, TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return Max(array, getKey, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType Max<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TValueType nullValue)
        {
            TValueType value;
            return Max(array, getKey, comparer, out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最大键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public static TKeyType MaxKey<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey, TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return Max(array, getKey, (left, right) => left.CompareTo(right), out value) ? getKey(value) : nullValue;
        }
        /// <summary>
        /// 获取最大键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大键值,失败返回默认空值</returns>
        public static TKeyType MaxKey<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TKeyType nullValue)
        {
            TValueType value;
            return Max(array, getKey, comparer, out value) ? getKey(value) : nullValue;
        }
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool Min<TValueType>(this TValueType[] array, Func<TValueType, TValueType, int> comparer, out TValueType value)
        {
            if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (array.Length() != 0)
            {
                value = array[0];
                foreach (TValueType nextValue in array)
                {
                    if (comparer(nextValue, value) < 0) value = nextValue;
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool Min<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, out TValueType value)
        {
            if (getKey == null || comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (array.Length() != 0)
            {
                value = array[0];
                if (array.Length != 1)
                {
                    if (getKey != null)
                    {
                        var key = getKey(value);
                        foreach (var nextValue in array)
                        {
                            var nextKey = getKey(nextValue);
                            if (comparer != null && comparer(nextKey, key) < 0)
                            {
                                value = nextValue;
                                key = nextKey;
                            }
                        }
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType Min<TValueType>(this TValueType[] array, TValueType nullValue)
            where TValueType : IComparable<TValueType>
        {
            TValueType value;
            return Min(array, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType Min<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey, TValueType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return Min(array, getKey, (left, right) => left.CompareTo(right), out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType Min<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TValueType nullValue)
        {
            TValueType value;
            return Min(array, getKey, comparer, out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取最小键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public static TKeyType MinKey<TValueType, TKeyType>(this TValueType[] array, Func<TValueType, TKeyType> getKey, TKeyType nullValue)
            where TKeyType : IComparable<TKeyType>
        {
            TValueType value;
            return Min(array, getKey, (left, right) => left.CompareTo(right), out value) ? getKey(value) : nullValue;
        }
        /// <summary>
        /// 获取最小键值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TKeyType">比较键类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="getKey">获取键值</param>
        /// <param name="comparer">比较器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小键值,失败返回默认空值</returns>
        public static TKeyType MinKey<TValueType, TKeyType>
            (this TValueType[] array, Func<TValueType, TKeyType> getKey, Func<TKeyType, TKeyType, int> comparer, TKeyType nullValue)
        {
            TValueType value;
            return Min(array, getKey, comparer, out value) ? getKey(value) : nullValue;
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <returns>字符串</returns>
        public static string JoinString<TValueType>(this TValueType[] array, Func<TValueType, string> toString)
        {
            return string.Concat(array.GetArray(toString));
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接串</param>
        /// <returns>字符串</returns>
        public static string JoinString<TValueType>(this TValueType[] array, string join, Func<TValueType, string> toString)
        {
            return string.Join(join, array.GetArray(toString));
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据集合</param>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接字符</param>
        /// <returns>字符串</returns>
        public static string JoinString<TValueType>(this TValueType[] array, char join, Func<TValueType, string> toString)
        {
            return array.GetArray(toString).JoinString(join);
        }
    }
}
