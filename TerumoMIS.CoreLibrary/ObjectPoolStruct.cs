//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ObjectPoolStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  ObjectPoolStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 16:57:21
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Reflection;
using System.Threading;
using TerumoMIS.CoreLibrary.Config;
using TerumoMIS.CoreLibrary.Threading;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     数组池
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    public struct ObjectPoolStruct<TValueType>
        where TValueType : class
    {
        /// <summary>
        ///     释放资源委托
        /// </summary>
        internal static readonly Action<TValueType> Dispose;

        /// <summary>
        ///     数据集合
        /// </summary>
        private ArrayPlus.ValueStruct<TValueType>[] _array;

        /// <summary>
        ///     数据集合访问锁
        /// </summary>
        private int _arrayLock;

        /// <summary>
        ///     集合更新访问锁
        /// </summary>
        private object _newLock;

        /// <summary>
        ///     数据数量
        /// </summary>
        internal int Count;

        static ObjectPoolStruct()
        {
            var type = typeof (TValueType);
            if (typeof (IDisposable).IsAssignableFrom(type))
            {
                Dispose =
                    (Action<TValueType>)
                        Delegate.CreateDelegate(typeof (Action<TValueType>),
                            type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null,
                                NullValuePlus<Type>.Array, null));
            }
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="value"></param>
        public void Push(TValueType value)
        {
            PUSH:
            InterlockedPlus.NoCheckCompareSetSleep0(ref _arrayLock);
            if (Count == _array.Length)
            {
                var length = Count;
                _arrayLock = 0;
                Monitor.Enter(_newLock);
                if (length == _array.Length)
                {
                    try
                    {
                        var newArray = new ArrayPlus.ValueStruct<TValueType>[length << 1];
                        InterlockedPlus.NoCheckCompareSetSleep0(ref _arrayLock);
                        Array.Copy(_array, 0, newArray, 0, Count);
                        newArray[Count].Value = value;
                        _array = newArray;
                        ++Count;
                        _arrayLock = 0;
                    }
                    finally
                    {
                        Monitor.Exit(_newLock);
                    }
                    return;
                }
                Monitor.Exit(_newLock);
                goto PUSH;
            }
            _array[Count++].Value = value;
            _arrayLock = 0;
        }

        /// <summary>
        ///     获取类型对象
        /// </summary>
        /// <returns>类型对象</returns>
        public TValueType Pop()
        {
            InterlockedPlus.NoCheckCompareSetSleep0(ref _arrayLock);
            if (Count == 0)
            {
                _arrayLock = 0;
                return null;
            }
            var value = _array[--Count].Free();
            _arrayLock = 0;
            return value;
        }

        /// <summary>
        ///     清除数据集合
        /// </summary>
        /// <param name="count">保留数据数量</param>
        /// <returns>被清除的数据集合</returns>
        internal void Clear(int count)
        {
            if (Dispose == null)
            {
                InterlockedPlus.NoCheckCompareSetSleep0(ref _arrayLock);
                var length = Count - count;
                if (length > 0)
                {
                    Array.Clear(_array, count, length);
                    Count = count;
                }
                _arrayLock = 0;
            }
            else
            {
                foreach (var value in GetClear(count))
                {
                    try
                    {
                        Dispose(value.Value);
                    }
                    catch (Exception error)
                    {
                        LogPlus.Default.Add(error, null, false);
                    }
                }
            }
        }

        /// <summary>
        ///     清除数据集合
        /// </summary>
        /// <param name="count">保留数据数量</param>
        /// <returns>被清除的数据集合</returns>
        internal ArrayPlus.ValueStruct<TValueType>[] GetClear(int count)
        {
            InterlockedPlus.NoCheckCompareSetSleep0(ref _arrayLock);
            var length = Count - count;
            if (length > 0)
            {
                ArrayPlus.ValueStruct<TValueType>[] removeBuffers;
                try
                {
                    removeBuffers = new ArrayPlus.ValueStruct<TValueType>[length];
                    Array.Copy(_array, Count = count, removeBuffers, 0, length);
                    Array.Clear(_array, count, length);
                }
                finally
                {
                    _arrayLock = 0;
                }
                return removeBuffers;
            }
            _arrayLock = 0;
            return NullValuePlus<ArrayPlus.ValueStruct<TValueType>>.Array;
        }

        /// <summary>
        ///     创建数组池
        /// </summary>
        /// <returns>数组池</returns>
        public static ObjectPoolStruct<TValueType> Create()
        {
            return new ObjectPoolStruct<TValueType>
            {
                _array = new ArrayPlus.ValueStruct<TValueType>[AppSettingPlus.PoolSize],
                _newLock = new object()
            };
        }

        /// <summary>
        ///     创建数组池
        /// </summary>
        /// <param name="size">容器初始化大小</param>
        /// <returns></returns>
        public static ObjectPoolStruct<TValueType> Create(int size)
        {
            return new ObjectPoolStruct<TValueType>
            {
                _array = new ArrayPlus.ValueStruct<TValueType>[size <= 0 ? AppSettingPlus.PoolSize : size],
                _newLock = new object()
            };
        }
    }
}