//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ArrayHeapPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  ArrayHeapPlus
//	User name:  C1400008
//	Location Time: 2015/7/8 16:51:48
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 数组模拟最小堆
    /// </summary>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class ArrayHeapPlus<TKeyType,TValueType> where TKeyType:IComparable<TKeyType>
    {
        /// <summary>
        /// 默认数组长度
        /// </summary>
        private const int DefaultArrayLength = 256;
        /// <summary>
        /// 数据数组
        /// </summary>
        private KeyValueStruct<TKeyType, TValueType>[] _array;
        /// <summary>
        /// 最小堆索引
        /// </summary>
        private int[] _heap;
        /// <summary>
        /// 数据数量
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// 根节点索引
        /// </summary>
        private int _bootIndex;
        /// <summary>
        /// 数组模拟最小堆
        /// </summary>
        public ArrayHeapPlus()
        {
            _array = new KeyValueStruct<TKeyType, TValueType>[DefaultArrayLength];
            _heap = new int[DefaultArrayLength];
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">数据值</param>
        public void Add(TKeyType key, TValueType value)
        {
            Push(key, value);
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">数据值</param>
        public unsafe void Push(TKeyType key, TValueType value)
        {
            int index = Count;
            if (index == _array.Length)
            {
                KeyValueStruct<TKeyType, TValueType>[] newArray = new KeyValueStruct<TKeyType, TValueType>[index << 1];
                _array.CopyTo(newArray, 0);
                var newHeap = new int[index << 1];
                _heap.CopyTo(newHeap, 0);
                _heap = newHeap;
                _array = newArray;
            }
            _array[index].Set(key, value);
            if (index == 0) _heap[_bootIndex = 1] = 0;
            else
            {
                fixed (int* heapFixed = _heap)
                {
                    if ((index >> 1) == _bootIndex)
                    {
                        heapFixed[index] = heapFixed[_bootIndex];
                        _bootIndex = index;
                    }
                    if ((index & 1) == 0 || key.CompareTo(_array[index ^ 1].Key) < 0)
                    {
                        int heapIndex = index | 1, xor = 1;
                        heapFixed[heapIndex] = index;
                        while (xor != _bootIndex)
                        {
                            var cmpHeapIndex = heapIndex ^ (xor <<= 1);
                            if ((heapIndex & xor) == 0 || key.CompareTo(_array[heapFixed[cmpHeapIndex]].Key) < 0)
                            {
                                heapFixed[heapIndex = (int)(((uint)heapIndex + (uint)cmpHeapIndex) >> 1)] = index;
                            }
                            else break;
                        }
                    }
                }
            }
            ++Count;
        }
        /// <summary>
        /// 弹出堆顶数据
        /// </summary>
        /// <returns>堆顶数据</returns>
        public KeyValueStruct<TKeyType, TValueType> Pop()
        {
            if (Count == 0) throw new IndexOutOfRangeException();
            var index = _heap[_bootIndex];
            var value = _array[index];
            RemoveTop(index);
            return value;
        }
        /// <summary>
        /// 删除堆顶数据
        /// </summary>
        /// <param name="index">堆顶数据索引位置</param>
        private unsafe void RemoveTop(int index)
        {
            if (--Count == 0) _array[index].Set(default(TKeyType), default(TValueType));
            else
            {
                fixed (int* heapFixed = _heap)
                {
                    if (index != Count)
                    {
                        TKeyType key = (_array[index] = _array[Count]).Key, cmpKey = _array[index ^ 1].Key;
                        var heapIndex = index | 1;
                        if (key.CompareTo(cmpKey) > 0)
                        {
                            index ^= 1;
                            key = cmpKey;
                            heapFixed[heapIndex] = index;
                        }
                        for (var xor = 1; xor != _bootIndex; heapFixed[heapIndex] = index)
                        {
                            var cmpHeapIndex = heapIndex ^ (xor <<= 1);
                            heapIndex = (int)(((uint)heapIndex + (uint)cmpHeapIndex) >> 1);
                            if (heapIndex <= Count)
                            {
                                var cmpIndex = heapFixed[cmpHeapIndex];
                                if (key.CompareTo(cmpKey = _array[cmpIndex].Key) > 0)
                                {
                                    index = cmpIndex;
                                    key = cmpKey;
                                }
                            }
                        }
                    }
                    _array[Count].Set(default(TKeyType), default(TValueType));
                    if (_bootIndex == Count) _bootIndex >>= 1;
                    else
                    {
                        var heapIndex = (index = Count) | 1;
                        if (heapFixed[heapIndex] == index)
                        {
                            TKeyType key;
                            var xor = 1;
                            if ((index & 1) == 0)
                            {
                                while ((heapIndex & (xor <<= 1)) == 0) heapIndex += xor >> 1;
                                index = heapFixed[heapIndex ^= xor];
                                xor >>= 1;
                            }
                            else heapFixed[heapIndex] = (index ^= 1);
                            for (key = _array[index].Key; xor != _bootIndex; heapFixed[heapIndex] = index)
                            {
                                var cmpHeapIndex = heapIndex ^ (xor <<= 1);
                                heapIndex = (int)(((uint)heapIndex + (uint)cmpHeapIndex) >> 1);
                                if (heapFixed[heapIndex] == Count)
                                {
                                    if (cmpHeapIndex < heapIndex)
                                    {
                                        var cmpIndex = heapFixed[cmpHeapIndex];
                                        TKeyType cmpKey = _array[cmpIndex].Key;
                                        if (key.CompareTo(cmpKey) > 0)
                                        {
                                            index = cmpIndex;
                                            key = cmpKey;
                                        }
                                    }
                                }
                                else break;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取堆顶数据,不弹出
        /// </summary>
        /// <returns>堆顶数据</returns>
        public KeyValueStruct<TKeyType, TValueType> Top()
        {
            if (Count == 0) throw new IndexOutOfRangeException();
            return _array[_heap[_bootIndex]];
        }
        /// <summary>
        /// 获取堆顶数据,不弹出
        /// </summary>
        /// <returns>堆顶数据</returns>
        internal KeyValueStruct<TKeyType, TValueType> UnsafeTop()
        {
            return _array[_heap[_bootIndex]];
        }
        /// <summary>
        /// 删除堆顶数据
        /// </summary>
        internal void RemoveTop()
        {
            RemoveTop(_heap[_bootIndex]);
        }
    }
}
