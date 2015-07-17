//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TypePoolPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  TypePoolPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 17:04:36
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
    /// 类型对象池
    /// </summary>
    public static class TypePoolPlus
    {
        /// <summary>
        /// 对象池操作
        /// </summary>
        internal struct pool
        {
            /// <summary>
            /// 清除对象池+保留对象数量
            /// </summary>
            public Action<int> Clear;
        }
        /// <summary>
        /// 类型对象池操作集合
        /// </summary>
        private static readonly Dictionary<Type, pool> pools = dictionary.CreateOnly<Type, pool>();
        /// <summary>
        /// 类型对象池操作集合访问锁
        /// </summary>
        private static int poolLock;
        /// <summary>
        /// 添加类型对象池
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="pool">类型对象池</param>
        internal static void Add(Type type, pool pool)
        {
            interlocked.NoCheckCompareSetSleep0(ref poolLock);
            try
            {
                pools.Add(type, pool);
            }
            finally { poolLock = 0; }
        }
        /// <summary>
        /// 清除类型对象池
        /// </summary>
        /// <param name="count">保留对象数量</param>
        public static void ClearPool(int count = 0)
        {
            if (count <= 0) count = 0;
            interlocked.NoCheckCompareSetSleep0(ref poolLock);
            foreach (pool pool in pools.Values) pool.Clear(count);
            poolLock = 0;
        }
        static typePool()
        {
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
    /// <summary>
    /// 类型对象池
    /// </summary>
    /// <typeparam name="valueType">对象类型</typeparam>
    public static class typePool<valueType> where valueType : class
    {
        /// <summary>
        /// 类型对象池
        /// </summary>
        private static objectPool<valueType> pool;
        /// <summary>
        /// 类型对象池
        /// </summary>
        private static readonly HashSet<valueType> debugPool;
        /// <summary>
        /// 类型对象池访问锁
        /// </summary>
        private static int poolLock;
        /// <summary>
        /// 类型对象池对象数量
        /// </summary>
        private static int poolCount;

        /// <summary>
        /// 添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        public static void Push(valueType value)
        {
            if (value != null) push(value);
        }
        /// <summary>
        /// 添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        public static void Push(ref valueType value)
        {
            valueType pushValue = Interlocked.Exchange(ref value, null);
            if (pushValue != null) push(pushValue);
        }
        /// <summary>
        /// 添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        private static void push(valueType value)
        {
            if (fastCSharp.config.appSetting.IsPoolDebug)
            {
                bool isAdd, isMax = false;
                interlocked.NoCheckCompareSetSleep0(ref poolLock);
                try
                {
                    if ((isAdd = debugPool.Add(value)) && (isMax = debugPool.Count > poolCount))
                    {
                        poolCount <<= 1;
                    }
                }
                finally { poolLock = 0; }
                if (isAdd)
                {
                    if (isMax)
                    {
                        log.Default.Add("类型对象池扩展实例数量 " + typeof(valueType).fullName() + "[" + debugPool.Count.toString() + "]", false, false);
                    }
                }
                else log.Error.Add("对象池释放冲突 " + typeof(valueType).fullName(), true, false);
            }
            else pool.Push(value);
        }
        /// <summary>
        /// 获取类型对象
        /// </summary>
        /// <returns>类型对象</returns>
        public static valueType Pop()
        {
            if (fastCSharp.config.appSetting.IsPoolDebug)
            {
                valueType value = null;
                interlocked.NoCheckCompareSetSleep0(ref poolLock);
                foreach (valueType poolValue in debugPool)
                {
                    value = poolValue;
                    break;
                }
                if (value != null) debugPool.Remove(value);
                poolLock = 0;
                return value;
            }
            return pool.Pop();
        }
        /// <summary>
        /// 对象数量
        /// </summary>
        /// <returns>对象数量</returns>
        public static int Count()
        {
            return fastCSharp.config.appSetting.IsPoolDebug ? debugPool.Count : pool.Count;
        }
        /// <summary>
        /// 清除对象池
        /// </summary>
        /// <param name="count">保留对象数量</param>
        internal static void Clear(int count = 0)
        {
            if (fastCSharp.config.appSetting.IsPoolDebug) clearDebug(0);
            else clear(0);
        }
        /// <summary>
        /// 清除对象池
        /// </summary>
        /// <param name="count">保留对象数量</param>
        private static void clear(int count)
        {
            pool.Clear(count);
        }
        /// <summary>
        /// 清除对象池
        /// </summary>
        /// <param name="count">保留对象数量</param>
        private static void clearDebug(int count)
        {
            ClearDebug(debugPool, ref poolLock, count);
        }
        /// <summary>
        /// 清除对象池
        /// </summary>
        /// <param name="pool">类型对象池</param>
        /// <param name="poolLock">类型对象池访问锁</param>
        /// <param name="count">保留对象数量</param>
        internal static void ClearDebug(HashSet<valueType> pool, ref int poolLock, int count)
        {
            valueType[] removeValues = null;
            interlocked.NoCheckCompareSetSleep0(ref poolLock);
            int removeCount = pool.Count - count;
            if (removeCount > 0)
            {
                try
                {
                    removeValues = new valueType[removeCount];
                    foreach (valueType value in pool)
                    {
                        removeValues[--removeCount] = value;
                        if (removeCount == 0) break;
                    }
                    foreach (valueType value in removeValues) pool.Remove(value);
                }
                finally { poolLock = 0; }
                Action<valueType> dispose = objectPool<valueType>.Dispose;
                if (dispose != null)
                {
                    foreach (valueType value in removeValues)
                    {
                        try
                        {
                            dispose(value);
                        }
                        catch (Exception error)
                        {
                            log.Default.Add(error, null, false);
                        }
                    }
                }
            }
            else poolLock = 0;
        }
        static typePool()
        {
            Type type = typeof(valueType);
            if (fastCSharp.config.appSetting.IsPoolDebug)
            {
                debugPool = hashSet.CreateOnly<valueType>();
                typePool.Add(type, new typePool.pool { Clear = clearDebug });
                poolCount = fastCSharp.config.appSetting.PoolSize;
            }
            else
            {
                pool = objectPool<valueType>.Create();
                typePool.Add(type, new typePool.pool { Clear = clear });
            }
        }
    }
    /// <summary>
    /// 类型对象池
    /// </summary>
    /// <typeparam name="markType">标识类型</typeparam>
    /// <typeparam name="valueType">对象类型</typeparam>
    public abstract class typePool<markType, valueType> where valueType : class
    {
        /// <summary>
        /// 添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        public void Push(valueType value)
        {
            if (value != null) push(value);
        }
        /// <summary>
        /// 添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        public void Push(ref valueType value)
        {
            valueType pushValue = Interlocked.Exchange(ref value, null);
            if (pushValue != null) push(pushValue);
        }
        /// <summary>
        /// 添加类型对象
        /// </summary>
        /// <param name="value">类型对象</param>
        protected abstract void push(valueType value);
        /// <summary>
        /// 获取类型对象
        /// </summary>
        /// <returns>类型对象</returns>
        public abstract valueType Pop();
        /// <summary>
        /// 数组模式对象池
        /// </summary>
        private sealed class arrayPool : typePool<markType, valueType>
        {
            /// <summary>
            /// 类型对象池
            /// </summary>
            private objectPool<valueType> pool;
            /// <summary>
            /// 数组模式对象池
            /// </summary>
            public arrayPool()
            {
                pool = objectPool<valueType>.Create();
                typePool.Add(typeof(typePool<markType, valueType>), new typePool.pool { Clear = clear });
            }
            /// <summary>
            /// 添加类型对象
            /// </summary>
            /// <param name="value">类型对象</param>
            protected override void push(valueType value)
            {
                pool.Push(value);
            }
            /// <summary>
            /// 清除对象池
            /// </summary>
            /// <param name="count">保留对象数量</param>
            private void clear(int count)
            {
                pool.Clear(count);
            }
            /// <summary>
            /// 获取类型对象
            /// </summary>
            /// <returns>类型对象</returns>
            public override valueType Pop()
            {
                return pool.Pop();
            }
        }
        /// <summary>
        /// 纠错模式对象池
        /// </summary>
        private sealed class debugPool : typePool<markType, valueType>
        {
            /// <summary>
            /// 类型对象池
            /// </summary>
            private HashSet<valueType> pool;
            /// <summary>
            /// 类型对象池访问锁
            /// </summary>
            private int poolLock;
            /// <summary>
            /// 当前最大缓冲区数量
            /// </summary>
            private int maxCount = config.appSetting.PoolSize;
            /// <summary>
            /// 纠错模式对象池
            /// </summary>
            public debugPool()
            {
                pool = hashSet.CreateOnly<valueType>();
                typePool.Add(typeof(typePool<markType, valueType>), new typePool.pool { Clear = clear });
            }
            /// <summary>
            /// 添加类型对象
            /// </summary>
            /// <param name="value">类型对象</param>
            protected override void push(valueType value)
            {
                bool isAdd, isMax = false;
                interlocked.NoCheckCompareSetSleep0(ref poolLock);
                try
                {
                    if ((isAdd = pool.Add(value)) && (isMax = pool.Count > maxCount))
                    {
                        maxCount <<= 1;
                    }
                }
                finally { poolLock = 0; }
                if (isAdd)
                {
                    if (isMax)
                    {
                        log.Default.Add("类型对象池扩展实例数量 " + typeof(valueType).fullName() + "[" + pool.Count.toString() + "]@" + typeof(markType).fullName(), false, false);
                    }
                }
                else log.Error.Add("对象池释放冲突 " + typeof(markType).fullName() + " -> " + typeof(valueType).fullName(), true, false);
            }
            /// <summary>
            /// 清除对象池
            /// </summary>
            /// <param name="count">保留对象数量</param>
            private void clear(int count)
            {
                typePool<valueType>.ClearDebug(pool, ref poolLock, count);
            }
            /// <summary>
            /// 获取类型对象
            /// </summary>
            /// <returns>类型对象</returns>
            public override valueType Pop()
            {
                valueType value = null;
                interlocked.NoCheckCompareSetSleep0(ref poolLock);
                foreach (valueType poolValue in pool)
                {
                    value = poolValue;
                    break;
                }
                if (value != null) pool.Remove(value);
                poolLock = 0;
                return value;
            }
        }
        /// <summary>
        /// 类型对象池
        /// </summary>
        public static typePool<markType, valueType> Default = config.appSetting.IsPoolDebug ? (typePool<markType, valueType>)new debugPool() : new arrayPool();
    }
}
