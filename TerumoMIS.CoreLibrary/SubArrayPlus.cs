//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SubArrayPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  SubArrayPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 10:54:18
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using TerumoMIS.CoreLibrary.Algorithm;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 数组子串
    /// </summary>
    public struct SubArrayStruct<TValueType> : IList<TValueType>
    {
        /// <summary>
        /// 原数组
        /// </summary>
        internal TValueType[] _array;
        /// <summary>
        /// 原数组
        /// </summary>
        public TValueType[] Array
        {
            get { return _array; }
        }
        /// <summary>
        /// 设置或获取值
        /// </summary>
        /// <param name="index">位置</param>
        /// <returns>数据值</returns>
        public TValueType this[int index]
        {
            get
            {
                if ((uint)index < (uint)_length) return _array[_startIndex + index];
                LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
                return default(TValueType);
            }
            set
            {
                if ((uint)index < (uint)_length) _array[_startIndex + index] = value;
                else LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 原数组中的起始位置
        /// </summary>
        private int _startIndex;
        /// <summary>
        /// 原数组中的起始位置
        /// </summary>
        public int StartIndex
        {
            get { return _startIndex; }
        }
        /// <summary>
        /// 长度
        /// </summary>
        private int _length;
        /// <summary>
        /// 长度
        /// </summary>
        public int Count
        {
            get
            {
                return _length;
            }
        }
        /// <summary>
        /// 只读
        /// </summary>
        public bool IsReadOnly { get { return true; } }
        /// <summary>
        /// 数据结束位置
        /// </summary>
        internal int EndIndex
        {
            get
            {
                return _startIndex + _length;
            }
        }
        /// <summary>
        /// 最后空闲长度
        /// </summary>
        internal int FreeLength
        {
            get
            {
                return _array.Length - _startIndex - _length;
            }
        }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="size">容器大小</param>
        public SubArrayStruct(int size)
        {
            _array = size > 0 ? new TValueType[size] : null;
            _startIndex = _length = 0;
        }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="value">数组</param>
        public SubArrayStruct(TValueType[] value)
        {
            _array = value;
            _startIndex = 0;
            _length = value == null ? 0 : value.Length;
        }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="value">数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public SubArrayStruct(TValueType[] value, int startIndex, int length)
        {
            if (value == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (value != null)
            {
                var range = new ArrayPlus.RangeStruct(value.Length, startIndex, length);
                if (range.GetCount != length) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
                if (range.GetCount != 0)
                {
                    _array = value;
                    _startIndex = range.SkipCount;
                    _length = range.GetCount;
                }
                else
                {
                    _array = NullValuePlus<TValueType>.Array;
                    _startIndex = _length = 0;
                }
            }
            _array = new TValueType[] {};
            _startIndex = 0;
            _length = 0;
        }
        /// <summary>
        /// 强制类型转换
        /// </summary>
        /// <param name="value">单向动态数组</param>
        /// <returns>数组数据</returns>
        public static explicit operator TValueType[](SubArrayStruct<TValueType> value)
        {
            return value.ToArray();
        }
        /// <summary>
        /// 强制类型转换
        /// </summary>
        /// <param name="value">数组数据</param>
        /// <returns>单向动态数组</returns>
        public static explicit operator SubArrayStruct<TValueType>(TValueType[] value)
        {
            return new SubArrayStruct<TValueType>(value);
        }
        /// <summary>
        /// 枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator<TValueType> IEnumerable<TValueType>.GetEnumerator()
        {
            if (_length != 0) return new iEnumerator<TValueType>.array(this);
            return iEnumerator<TValueType>.Empty;
        }
        /// <summary>
        /// 枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_length != 0) return new iEnumerator<TValueType>.array(this);
            return iEnumerator<TValueType>.Empty;
        }
        /// <summary>
        /// 反转枚举
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TValueType> ReverseEnumerable()
        {
            for (int endIndex = _startIndex + _length; endIndex > _startIndex; ) yield return _array[--endIndex];
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <returns>数组</returns>
        public TValueType[] ToArray()
        {
            if (_length == 0) return NullValuePlus<TValueType>.Array;
            return _length == _array.Length ? _array : GetArrayBase();
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <returns>数组</returns>
        public TValueType[] GetArray()
        {
            return _length != 0 ? GetArrayBase() : NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <returns>数组</returns>
        private TValueType[] GetArrayBase()
        {
            TValueType[] newArray = new TValueType[_length];
            System.Array.Copy(_array, _startIndex, newArray, 0, _length);
            return newArray;
        }
        /// <summary>
        /// 反转数组
        /// </summary>
        /// <returns></returns>
        public SubArrayStruct<TValueType> Reverse()
        {
            for (int index = _startIndex, endIndex = index + _length, middle = index + (_length >> 1); index != middle; )
            {
                var value = _array[index];
                _array[index++] = _array[--endIndex];
                _array[endIndex] = value;
            }
            return this;
        }
        /// <summary>
        /// 获取反转数组
        /// </summary>
        /// <returns></returns>
        public TValueType[] GetReverse()
        {
            if (_length == 0) return nullValue<TValueType>.Array;
            TValueType[] newArray = new TValueType[_length];
            if (_startIndex == 0)
            {
                int index = _length;
                foreach (TValueType value in _array)
                {
                    newArray[--index] = value;
                    if (index == 0) break;
                }
            }
            else
            {
                int index = _length, copyIndex = _startIndex;
                do
                {
                    newArray[--index] = _array[copyIndex++];
                }
                while (index != 0);
            }
            return newArray;
        }
        /// <summary>
        /// 置空并释放数组
        /// </summary>
        public void Null()
        {
            _array = null;
            _startIndex = _length = 0;
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="value">数组,不能为null</param>
        /// <param name="startIndex">起始位置,必须合法</param>
        /// <param name="length">长度,必须合法</param>
        public void UnsafeSet(TValueType[] value, int startIndex, int length)
        {
            _array = value;
            _startIndex = startIndex;
            _length = length;
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="startIndex">起始位置,必须合法</param>
        /// <param name="length">长度,必须合法</param>
        public void UnsafeSet(int startIndex, int length)
        {
            _startIndex = startIndex;
            _length = length;
        }
        /// <summary>
        /// 设置数据长度
        /// </summary>
        /// <param name="length">长度,必须合法</param>
        public void UnsafeSetLength(int length)
        {
            _length = length;
        }
        /// <summary>
        /// 设置数据容器长度
        /// </summary>
        /// <param name="count">数据长度</param>
        private void setLength(int count)
        {
            TValueType[] newArray = DynamicArrayPlus<TValueType>.GetNewArray(count);
            System.Array.Copy(_array, _startIndex, newArray, _startIndex, _length);
            _array = newArray;
        }
        /// <summary>
        /// 长度设为0
        /// </summary>
        public void Empty()
        {
            _startIndex = _length = 0;
        }
        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void Clear()
        {
            if (_array != null)
            {
                if (DynamicArrayPlus<TValueType>.IsClearArray) System.Array.Clear(_array, 0, _array.Length);
                Empty();
            }
        }
        /// <summary>
        /// 获取匹配数量
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>获取匹配数量</returns>
        public int GetCount(Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (_length != 0)
            {
                var count = 0;
                if (_startIndex == 0)
                {
                    int index = _length;
                    foreach (var value in _array)
                    {
                        if (isValue != null && isValue(value)) ++count;
                        if (--index == 0) break;
                    }
                }
                else
                {
                    int index = _startIndex, endIndex = _startIndex + _length;
                    do
                    {
                        if (isValue != null && isValue(_array[index])) ++count;
                    }
                    while (++index != endIndex);
                }
                return count;
            }
            return 0;
        }
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
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public int IndexOf(TValueType value)
        {
            if (_length != 0)
            {
                var index = System.Array.IndexOf(_array, value, _startIndex, _length);
                if (index >= 0) return index - _startIndex;
            }
            return -1;
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
        /// 获取获取数组中的匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>数组中的匹配位置,失败为-1</returns>
        private int IndexOfBase(Func<TValueType, bool> isValue)
        {
            if (_length != 0)
            {
                if (_startIndex == 0)
                {
                    int index = 0;
                    foreach (TValueType value in _array)
                    {
                        if (isValue(value)) return index;
                        if (++index == _length) break;
                    }
                }
                else
                {
                    int index = _startIndex, endIndex = _startIndex + _length;
                    do
                    {
                        if (isValue(_array[index])) return index;
                    }
                    while (++index != endIndex);
                }
            }
            return -1;
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>是否存在移除数据</returns>
        public bool Remove(TValueType value)
        {
            int index = IndexOf(value);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public void RemoveAt(int index)
        {
            if ((uint)index < (uint)_length)
            {
                fastCSharp.unsafer.array.Move(_array, _startIndex + index + 1, _startIndex + index, --_length - index);
                _array[_startIndex + _length] = default(TValueType);
            }
            else LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
        }
        /// <summary>
        /// 移除数据并使用最后一个数据移动到当前位置
        /// </summary>
        /// <param name="index">数据位置</param>
        public void RemoveAtEnd(int index)
        {
            if ((uint)index < (uint)_length) _array[_startIndex + index] = _array[_startIndex + --_length];
            else LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
        }
        /// <summary>
        /// 移除所有后端匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        private void RemoveEnd(Func<TValueType, bool> isValue)
        {
            for (int index = _startIndex + _length; index != _startIndex; --_length)
            {
                if (!isValue(_array[--index])) return;
            }
        }
        /// <summary>
        /// 移除匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns></returns>
        public SubArrayStruct<TValueType> Remove(Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (_length != 0)
            {
                RemoveEnd(isValue);
                int index = IndexOfBase(isValue);
                if (index != -1)
                {
                    for (int start = index, endIndex = _startIndex + _length; ++start != endIndex; )
                    {
                        if (isValue != null && !isValue(_array[start])) _array[index++] = _array[start];
                    }
                    _length = index - _startIndex;
                }
            }
            return this;
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据</param>
        public void Insert(int index, TValueType value)
        {
            if ((uint)index <= (uint)_length)
            {
                if (index != _length)
                {
                    if (_startIndex + _length != _array.Length)
                    {
                        fastCSharp.unsafer.array.Move(_array, _startIndex + index, _startIndex + index + 1, _length - index);
                        _array[_startIndex + index] = value;
                        ++_length;
                    }
                    else
                    {
                        TValueType[] values = DynamicArrayPlus<TValueType>.GetNewArray(_array.Length << 1);
                        System.Array.Copy(_array, _startIndex, values, _startIndex, index);
                        values[_startIndex + index] = value;
                        System.Array.Copy(_array, _startIndex + index, values, _startIndex + index + 1, _length++ - index);
                        _array = values;
                    }
                }
                else Add(value);
            }
            else LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
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
            return index != -1 ? _array[index] : default(TValueType);
        }

        /// <summary>
        /// 获取最后一个值
        /// </summary>
        /// <returns>最后一个值,失败为default(valueType)</returns>
        public TValueType LastOrDefault()
        {
            return _length != 0 ? _array[_startIndex + _length - 1] : default(TValueType);
        }
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <param name="values">目标数据</param>
        /// <param name="index">目标位置</param>
        public void CopyTo(TValueType[] values, int index)
        {
            if (index >= 0 && _length + index <= values.Length)
            {
                if (_length != 0) System.Array.Copy(_array, _startIndex, values, index, _length);
            }
            else LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
        }
        /// <summary>
        /// 转换成子集合(不清除数组)
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子集合</returns>
        public SubArrayStruct<TValueType> Sub(int index, int count)
        {
            var range = new ArrayPlus.RangeStruct(_length, index, count < 0 ? _length - index : count);
            if (range.GetCount > 0)
            {
                _startIndex += range.SkipCount;
                _length = range.GetCount;
            }
            return this;
        }
        /// <summary>
        /// 转换成子集合(不清除数组)
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量,小于0表示所有</param>
        /// <returns>子集合</returns>
        public SubArrayStruct<TValueType> GetSub(int index, int count)
        {
            var range = new ArrayPlus.RangeStruct(_length, index, count < 0 ? _length - index : count);
            if (range.GetCount > 0)
            {
                return Unsafe(_array, _startIndex + range.SkipCount, range.GetCount);
            }
            return default(SubArrayStruct<TValueType>);
        }
        /// <summary>
        /// 获取分页字段数组
        /// </summary>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页字段数组</returns>
        public SubArrayStruct<TValueType> Page(int pageSize, int currentPage)
        {
            var page = new ArrayPlus.RangeStruct(_length, pageSize, currentPage);
            return SubArrayStruct<TValueType>.Unsafe(_array, _startIndex + page.SkipCount, page.CurrentPageSize);
        }
        /// <summary>
        /// 获取分页字段数组
        /// </summary>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页字段数组</returns>
        public SubArrayStruct<TValueType> PageDesc(int pageSize, int currentPage)
        {
            var page = new ArrayPlus.RangeStruct(_length, pageSize, currentPage);
            return SubArrayStruct<TValueType>.Unsafe(_array, _startIndex + page.DescSkipCount, page.CurrentPageSize).Reverse();
        }
        /// <summary>
        /// 获取分页字段数组
        /// </summary>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页字段数组</returns>
        public TValueType[] GetPageDesc(int pageSize, int currentPage)
        {
            var page = new ArrayPlus.RangeStruct(_length, pageSize, currentPage);
            return SubArrayStruct<TValueType>.Unsafe(_array, _startIndex + page.DescSkipCount, page.CurrentPageSize).GetReverse();
        }
        /// <summary>
        /// 增加数据长度
        /// </summary>
        /// <param name="length">数据长度</param>
        private void AddToLength(int length)
        {
            if (_array == null) _array = new TValueType[length < sizeof(int) ? sizeof(int) : length];
            else if (length > _array.Length) setLength(length);
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Add(TValueType value)
        {
            if (_array == null)
            {
                _array = new TValueType[sizeof(int)];
                _array[0] = value;
                _length = 1;
            }
            else
            {
                int index = _startIndex + _length;
                if (index == _array.Length)
                {
                    if (index == 0) _array = new TValueType[sizeof(int)];
                    else setLength(index << 1);
                }
                _array[index] = value;
                ++_length;
            }
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据</param>
        internal void UnsafeAdd(TValueType value)
        {
            _array[_startIndex + _length++] = value;
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(ICollection<TValueType> values)
        {
            var count = values.Count;
            if (count != 0)
            {
                int index = _startIndex + _length;
                AddToLength(index + count);
                foreach (TValueType value in values) _array[index++] = value;
                _length += count;
            }
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <returns></returns>
        public SubArrayStruct<TValueType> Add(TValueType[] values)
        {
            if (values != null && values.Length != 0) AddBase(values, 0, values.Length);
            return this;
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        public void Add(TValueType[] values, int index, int count)
        {
            var range = new ArrayPlus.RangeStruct(values.Length, index, count);
            if ((count = range.GetCount) != 0) AddBase(values, range.SkipCount, count);
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(SubArrayStruct<TValueType> values)
        {
            if (values.Count != 0) AddBase(values.Array, values.StartIndex, values.Count);
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        private void AddBase(TValueType[] values, int index, int count)
        {
            var newLength = _startIndex + _length + count;
            AddToLength(newLength);
            System.Array.Copy(values, index, _array, _startIndex + _length, count);
            _length += count;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据</param>
        internal void UnsafeAddExpand(TValueType value)
        {
            _array[--_startIndex] = value;
            ++_length;
        }
        /// <summary>
        /// 弹出数据
        /// </summary>
        /// <returns></returns>
        public TValueType Pop()
        {
            if (_length == 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            return _array[_startIndex + --_length];
        }
        /// <summary>
        /// 弹出数据
        /// </summary>
        /// <returns></returns>
        internal TValueType UnsafePop()
        {
            return _array[_startIndex + --_length];
        }
        /// <summary>
        /// 弹出数据
        /// </summary>
        /// <returns></returns>
        internal TValueType UnsafePopReset()
        {
            var index = _startIndex + --_length;
            var value = _array[index];
            _array[index] = default(TValueType);
            return value;
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <typeparam name="TArrayType">数组类型</typeparam>
        /// <param name="getValue">数据获取委托</param>
        /// <returns>数组</returns>
        public TArrayType[] GetArray<TArrayType>(Func<TValueType, TArrayType> getValue)
        {
            //if (array != null)
            //{
            if (_length != 0)
            {
                if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                var newArray = new TArrayType[_length];
                for (int count = 0, index = _startIndex, endIndex = _startIndex + _length; index != endIndex; ++index)
                {
                    if (getValue != null) newArray[count++] = getValue(_array[index]);
                }
                return newArray;
            }
            return NullValuePlus<TArrayType>.Array;
            //}
            //return null;
        }
        /// <summary>
        /// 获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值集合</returns>
        public unsafe TValueType[] GetFindArray(Func<TValueType, bool> isValue)
        {
            if (isValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (_length != 0)
            {
                var length = ((_length + 31) >> 5) << 2;
                memoryPool pool = fastCSharp.memoryPool.GetDefaultPool(length);
                byte[] data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        System.Array.Clear(data, 0, length);
                        return GetFindArray(isValue, new fixedMap(dataFixed));
                    }
                }
                finally { pool.Push(ref data); }
            }
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配值集合</returns>
        private TValueType[] GetFindArray(Func<TValueType, bool> isValue, fixedMap map)
        {
            if (_startIndex == 0)
            {
                int count = 0, index = 0;
                foreach (TValueType value in _array)
                {
                    if (isValue(value))
                    {
                        ++count;
                        map.Set(index);
                    }
                    if (++index == _length) break;
                }
                if (count != 0)
                {
                    var values = new TValueType[count];
                    for (index = _length; count != 0; values[--count] = _array[index])
                    {
                        while (!map.Get(--index)) ;
                    }
                    return values;
                }
            }
            else
            {
                int count = 0, index = _startIndex, endIndex = _startIndex + _length;
                do
                {
                    if (isValue(_array[index]))
                    {
                        ++count;
                        map.Set(index - _startIndex);
                    }
                }
                while (++index != endIndex);
                if (count != 0)
                {
                    var values = new TValueType[count];
                    for (index = _length; count != 0; values[--count] = _array[_startIndex + index])
                    {
                        while (!map.Get(--index)) ;
                    }
                    return values;
                }
            }
            return nullValue<TValueType>.Array;
        }
        /// <summary>
        /// 转换为单向动态数组
        /// </summary>
        /// <returns>单向动态数组</returns>
        public list<TValueType> ToList()
        {
            return _length != 0 ? new list<TValueType>(this, true) : null;
        }
        /// <summary>
        /// 转换为单向动态数组
        /// </summary>
        /// <returns>单向动态数组</returns>
        public collection<TValueType> ToCollection()
        {
            return _length != 0 ? new collection<TValueType>(this, true) : null;
        }
        /// <summary>
        /// 连接数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TArrayType">目标数组类型</typeparam>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public SubArrayStruct<TArrayType> Concat<TArrayType>(Func<TValueType, SubArrayStruct<TArrayType>> getValue)
        {
            if (getValue == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (_length != 0)
            {
                var values = new SubArrayStruct<TArrayType>();
                if (_startIndex == 0)
                {
                    int index = _length;
                    foreach (var value in _array)
                    {
                        if (getValue != null) values.Add(getValue(value));
                        if (--index == 0) break;
                    }
                }
                else
                {
                    int index = _startIndex, endIndex = _startIndex + _length;
                    do
                    {
                        if (getValue != null) values.Add(getValue(_array[index]));
                    }
                    while (++index != endIndex);
                }
                return values;
            }
            return default(SubArrayStruct<TArrayType>);
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns>排序后的数组</returns>
        public SubArrayStruct<TValueType> Sort(Func<TValueType, TValueType, int> comparer)
        {
            QuickSortPlus.Sort(_array, comparer, _startIndex, _length);
            return this;
        }
        /// <summary>
        /// 数组子串(非安全,请自行确保数据可靠性)
        /// </summary>
        /// <param name="value">数组,不能为null</param>
        /// <param name="startIndex">起始位置,必须合法</param>
        /// <param name="length">长度,必须合法</param>
        /// <returns>数组子串</returns>
        public static SubArrayStruct<TValueType> Unsafe(TValueType[] value, int startIndex, int length)
        {
            return new SubArrayStruct<TValueType> { _array = value, _startIndex = startIndex, _length = length };
        }
    }
}
