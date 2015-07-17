//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ArrayPoolStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  ArrayPoolStruct
//	User name:  C1400008
//	Location Time: 2015/7/13 8:28:05
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 数组池
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public struct ArrayPoolStruct<TValueType>
    {
        /// <summary>
        /// 数据集合
        /// </summary>
        internal TValueType[] Array;
        /// <summary>
        /// 数据数量
        /// </summary>
        internal int Count;
        /// <summary>
        /// 数据集合访问锁
        /// </summary>
        private int _arrayLock;
        /// <summary>
        /// 集合更新访问锁
        /// </summary>
        private object _newLock;
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value"></param>
        public void Push(TValueType value)
        {
        PUSH:
            interlocked.NoCheckCompareSetSleep0(ref _arrayLock);
            if (Count == Array.Length)
            {
                var length = Count;
                _arrayLock = 0;
                Monitor.Enter(_newLock);
                if (length == Array.Length)
                {
                    try
                    {
                        var newArray = new TValueType[length << 1];
                        interlocked.NoCheckCompareSetSleep0(ref _arrayLock);
                        System.Array.Copy(Array, 0, newArray, 0, Count);
                        newArray[Count] = value;
                        Array = newArray;
                        ++Count;
                        _arrayLock = 0;
                    }
                    finally { Monitor.Exit(_newLock); }
                    return;
                }
                Monitor.Exit(_newLock);
                goto PUSH;
            }
            Array[Count++] = value;
            _arrayLock = 0;
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns>是否存在数据</returns>
        public bool TryGet(ref TValueType value)
        {
            interlocked.NoCheckCompareSetSleep0(ref _arrayLock);
            if (Count == 0)
            {
                _arrayLock = 0;
                return false;
            }
            value = Array[--Count];
            _arrayLock = 0;
            return true;
        }
        /// <summary>
        /// 清除数据集合
        /// </summary>
        /// <param name="count">保留数据数量</param>
        /// <param name="isClear">是否清除数据</param>
        internal void Clear(int count, bool isClear)
        {
            interlocked.NoCheckCompareSetSleep0(ref _arrayLock);
            var length = Count - count;
            if (length > 0)
            {
                if (isClear) System.Array.Clear(Array, count, length);
                Count = count;
            }
            _arrayLock = 0;
        }
        /// <summary>
        /// 清除数据集合
        /// </summary>
        /// <param name="count">保留数据数量</param>
        /// <param name="isClear">是否清除数据</param>
        /// <returns>被清除的数据集合</returns>
        internal TValueType[] GetClear(int count, bool isClear)
        {
            interlocked.NoCheckCompareSetSleep0(ref _arrayLock);
            var length = Count - count;
            if (length > 0)
            {
                TValueType[] removeBuffers;
                try
                {
                    removeBuffers = new TValueType[length];
                    System.Array.Copy(Array, Count = count, removeBuffers, 0, length);
                    if (isClear) System.Array.Clear(Array, count, length);
                }
                finally { _arrayLock = 0; }
                return removeBuffers;
            }
            _arrayLock = 0;
            return NullValuePlus<TValueType>.Array;
        }
        /// <summary>
        /// 创建数组池
        /// </summary>
        /// <returns>数组池</returns>
        public static ArrayPoolStruct<TValueType> Create()
        {
            return CreateBase(fastCSharp.config.appSetting.PoolSize);
        }
        /// <summary>
        /// 创建数组池
        /// </summary>
        /// <param name="size">容器初始化大小</param>
        /// <returns></returns>
        public static ArrayPoolStruct<TValueType> Create(int size)
        {
            return CreateBase(size <= 0 ? fastCSharp.config.appSetting.PoolSize : size);
        }
        /// <summary>
        /// 创建数组池
        /// </summary>
        /// <param name="size">容器初始化大小</param>
        /// <returns></returns>
        private static ArrayPoolStruct<TValueType> CreateBase(int size)
        {
            return new ArrayPoolStruct<TValueType> { Array = new TValueType[size], _newLock = new object() };
        }
    }
}
