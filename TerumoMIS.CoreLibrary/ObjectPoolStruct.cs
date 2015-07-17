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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 数组池
    /// </summary>
    /// <typeparam name="valueType"></typeparam>
    public struct ObjectPoolStruct<valueType>
        where valueType:class 
    {
        /// <summary>
        /// 释放资源委托
        /// </summary>
        internal static readonly Action<valueType> Dispose;
        /// <summary>
        /// 数据集合
        /// </summary>
        private IEnumeratorPlus<>.array.value<valueType>[] array;
        /// <summary>
        /// 数据数量
        /// </summary>
        internal int Count;
        /// <summary>
        /// 数据集合访问锁
        /// </summary>
        private int arrayLock;
        /// <summary>
        /// 集合更新访问锁
        /// </summary>
        private object newLock;
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value"></param>
        public void Push(valueType value)
        {
        PUSH:
            interlocked.NoCheckCompareSetSleep0(ref arrayLock);
            if (Count == array.Length)
            {
                int length = Count;
                arrayLock = 0;
                Monitor.Enter(newLock);
                if (length == array.Length)
                {
                    try
                    {
                        IEnumeratorPlus<>.array.value<valueType>[] newArray = new IEnumeratorPlus<>.array.value<valueType>[length << 1];
                        interlocked.NoCheckCompareSetSleep0(ref arrayLock);
                        System.Array.Copy(array, 0, newArray, 0, Count);
                        newArray[Count].Value = value;
                        array = newArray;
                        ++Count;
                        arrayLock = 0;
                    }
                    finally { Monitor.Exit(newLock); }
                    return;
                }
                Monitor.Exit(newLock);
                goto PUSH;
            }
            array[Count++].Value = value;
            arrayLock = 0;
        }
        /// <summary>
        /// 获取类型对象
        /// </summary>
        /// <returns>类型对象</returns>
        public valueType Pop()
        {
            interlocked.NoCheckCompareSetSleep0(ref arrayLock);
            if (Count == 0)
            {
                arrayLock = 0;
                return null;
            }
            valueType value = array[--Count].Free();
            arrayLock = 0;
            return value;
        }
        /// <summary>
        /// 清除数据集合
        /// </summary>
        /// <param name="count">保留数据数量</param>
        /// <returns>被清除的数据集合</returns>
        internal void Clear(int count)
        {
            if (Dispose == null)
            {
                interlocked.NoCheckCompareSetSleep0(ref arrayLock);
                int length = Count - count;
                if (length > 0)
                {
                    System.Array.Clear(array, count, length);
                    Count = count;
                }
                arrayLock = 0;
            }
            else
            {
                foreach (IEnumeratorPlus<>.array.value<valueType> value in GetClear(count))
                {
                    try
                    {
                        Dispose(value.Value);
                    }
                    catch (Exception error)
                    {
                        log.Default.Add(error, null, false);
                    }
                }
            }
        }
        /// <summary>
        /// 清除数据集合
        /// </summary>
        /// <param name="count">保留数据数量</param>
        /// <returns>被清除的数据集合</returns>
        internal IEnumeratorPlus<>.array.value<valueType>[] GetClear(int count)
        {
            interlocked.NoCheckCompareSetSleep0(ref arrayLock);
            int length = Count - count;
            if (length > 0)
            {
                IEnumeratorPlus<>.array.value<valueType>[] removeBuffers;
                try
                {
                    removeBuffers = new IEnumeratorPlus<>.array.value<valueType>[length];
                    System.Array.Copy(array, Count = count, removeBuffers, 0, length);
                    System.Array.Clear(array, count, length);
                }
                finally { arrayLock = 0; }
                return removeBuffers;
            }
            else arrayLock = 0;
            return nullValue<IEnumeratorPlus<>.array.value<valueType>>.Array;
        }
        /// <summary>
        /// 创建数组池
        /// </summary>
        /// <returns>数组池</returns>
        public static objectPool<valueType> Create()
        {
            return new objectPool<valueType> { array = new IEnumeratorPlus<>.array.value<valueType>[fastCSharp.config.appSetting.PoolSize], newLock = new object() };
        }
        /// <summary>
        /// 创建数组池
        /// </summary>
        /// <param name="size">容器初始化大小</param>
        /// <returns></returns>
        public static objectPool<valueType> Create(int size)
        {
            return new objectPool<valueType> { array = new IEnumeratorPlus<>.array.value<valueType>[size <= 0 ? fastCSharp.config.appSetting.PoolSize : size], newLock = new object() };
        }
        static objectPool()
        {
            Type type = typeof(valueType);
            if (typeof(IDisposable).IsAssignableFrom(type))
            {
                Dispose = (Action<valueType>)Delegate.CreateDelegate(typeof(Action<valueType>), type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null, nullValue<Type>.Array, null));
            }
        }
    }
}
