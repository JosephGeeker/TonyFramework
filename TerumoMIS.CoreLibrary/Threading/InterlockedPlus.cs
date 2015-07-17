//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: InterlockedPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  InterlockedPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 15:32:15
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    ///     原子操作扩张
    /// </summary>
    public static class InterlockedPlus
    {
        /// <summary>
        ///     将目标值从0改置为1,循环等待周期切换(适应于等待时间极短的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NoCheckCompareSetSleep0(ref int value)
        {
            while (Interlocked.CompareExchange(ref value, 1, 0) != 0) Thread.Sleep(0);
        }

        /// <summary>
        ///     将目标值从0改置为1,循环等待周期切换(适应于等待时间极短的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        public static void CompareSetSleep0(ref int value)
        {
            if (Interlocked.CompareExchange(ref value, 1, 0) != 0)
            {
                var time = DatePlus.NowSecond.AddSeconds(2);
                do
                {
                    Thread.Sleep(0);
                    if (Interlocked.CompareExchange(ref value, 1, 0) == 0) return;
                } while (DatePlus.NowSecond < time);
                LogPlus.Error.Add("可能出现死锁", true);
                while (Interlocked.CompareExchange(ref value, 1, 0) != 0) Thread.Sleep(0);
            }
        }

        /// <summary>
        ///     将目标值从0改置为1,循环等待周期切换(适应于等待时间较短的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        public static void CompareSetSleep1(ref int value)
        {
            if (Interlocked.CompareExchange(ref value, 1, 0) != 0)
            {
                Thread.Sleep(0);
                if (Interlocked.CompareExchange(ref value, 1, 0) != 0)
                {
                    var time = DatePlus.NowSecond.AddSeconds(2);
                    do
                    {
                        Thread.Sleep(1);
                        if (Interlocked.CompareExchange(ref value, 1, 0) == 0) return;
                    } while (DatePlus.NowSecond < time);
                    LogPlus.Default.Add("线程等待时间过长", true);
                    while (Interlocked.CompareExchange(ref value, 1, 0) != 0) Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        ///     将目标值从0改置为1,循环等待周期固定(适应于等待时间可能较长的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        /// <param name="logSeconds">输入日志秒数</param>
        public static void CompareSetSleep(ref int value, double logSeconds = 5)
        {
            if (Interlocked.CompareExchange(ref value, 1, 0) != 0)
            {
                var time = DatePlus.NowSecond.AddSeconds(logSeconds + 1);
                do
                {
                    Thread.Sleep(1);
                    if (Interlocked.CompareExchange(ref value, 1, 0) == 0) return;
                } while (DatePlus.NowSecond < time);
                LogPlus.Default.Add("线程等待时间过长", true);
                while (Interlocked.CompareExchange(ref value, 1, 0) != 0) Thread.Sleep(1);
            }
        }

        /// <summary>
        ///     等待单次运行
        /// </summary>
        /// <param name="runState">执行状态,0表示未运行,1表示运行中,2表示已经运行</param>
        /// <param name="run">执行委托</param>
        public static void WaitRunOnce(ref int runState, Action run)
        {
            var isRun = Interlocked.CompareExchange(ref runState, 1, 0);
            if (isRun == 0)
            {
                try
                {
                    run();
                }
                finally
                {
                    runState = 2;
                }
            }
            else if (isRun == 1)
            {
                while (runState == 1) Thread.Sleep(1);
            }
        }

        /// <summary>
        ///     字典
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        public struct DictionaryStruct<TKeyType, TValueType>
        {
            /// <summary>
            ///     字典
            /// </summary>
            private readonly Dictionary<TKeyType, TValueType> _values;

            /// <summary>
            ///     访问锁
            /// </summary>
            private int _valueLock;

            /// <summary>
            ///     字典
            /// </summary>
            /// <param name="values"></param>
            public DictionaryStruct(Dictionary<TKeyType, TValueType> values)
            {
                if (values == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                _values = values;
                _valueLock = 0;
            }

            /// <summary>
            ///     获取数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns>是否存在数据</returns>
            public bool TryGetValue(TKeyType key, out TValueType value)
            {
                NoCheckCompareSetSleep0(ref _valueLock);
                if (_values.TryGetValue(key, out value))
                {
                    _valueLock = 0;
                    return true;
                }
                _valueLock = 0;
                return false;
            }

            /// <summary>
            ///     设置数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void Set(TKeyType key, TValueType value)
            {
                NoCheckCompareSetSleep0(ref _valueLock);
                try
                {
                    _values[key] = value;
                }
                finally
                {
                    _valueLock = 0;
                }
            }

            /// <summary>
            ///     获取数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <param name="oldValue"></param>
            /// <returns>是否存在数据</returns>
            public bool Set(TKeyType key, TValueType value, out TValueType oldValue)
            {
                NoCheckCompareSetSleep0(ref _valueLock);
                if (_values.TryGetValue(key, out oldValue))
                {
                    try
                    {
                        _values[key] = value;
                    }
                    finally
                    {
                        _valueLock = 0;
                    }
                    return true;
                }
                try
                {
                    _values.Add(key, value);
                }
                finally
                {
                    _valueLock = 0;
                }
                return false;
            }
        }

        /// <summary>
        ///     字典
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        public struct LastDictionaryStruct<TKeyType, TValueType> where TKeyType : struct, IEquatable<TKeyType>
        {
            /// <summary>
            ///     字典
            /// </summary>
            private readonly Dictionary<TKeyType, TValueType> _values;

            /// <summary>
            ///     最后一次访问的关键字
            /// </summary>
            private TKeyType _lastKey;

            /// <summary>
            ///     最后一次访问的数据
            /// </summary>
            private TValueType _lastValue;

            /// <summary>
            ///     访问锁
            /// </summary>
            private int _valueLock;

            /// <summary>
            ///     字典
            /// </summary>
            /// <param name="values"></param>
            public LastDictionaryStruct(Dictionary<TKeyType, TValueType> values)
            {
                if (values == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                _values = values;
                _valueLock = 0;
                _lastKey = default(TKeyType);
                _lastValue = default(TValueType);
            }

            /// <summary>
            ///     是否空字典
            /// </summary>
            public bool IsNull
            {
                get { return _values == null; }
            }

            /// <summary>
            ///     获取数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns>是否存在数据</returns>
            public bool TryGetValue(TKeyType key, out TValueType value)
            {
                NoCheckCompareSetSleep0(ref _valueLock);
                if (_lastKey.Equals(key))
                {
                    value = _lastValue;
                    _valueLock = 0;
                    return true;
                }
                if (_values.TryGetValue(key, out value))
                {
                    _lastKey = key;
                    _lastValue = value;
                    _valueLock = 0;
                    return true;
                }
                _valueLock = 0;
                return false;
            }

            /// <summary>
            ///     设置数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void Set(TKeyType key, TValueType value)
            {
                NoCheckCompareSetSleep0(ref _valueLock);
                try
                {
                    _values[_lastKey = key] = _lastValue = value;
                }
                finally
                {
                    _valueLock = 0;
                }
            }
        }
    }
}