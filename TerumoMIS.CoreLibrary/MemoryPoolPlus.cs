//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemoryPoolPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  MemoryPoolPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 11:11:44
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 内存池
    /// </summary>
    public abstract class MemoryPoolPlus
    {
        /// <summary>
        /// 缓冲数组子串
        /// </summary>
        public struct PushSubArrayStruct
        {
            /// <summary>
            /// 数组子串
            /// </summary>
            internal SubArrayStruct<byte> Value;
            /// <summary>
            /// 数组子串
            /// </summary>
            public SubArrayStruct<byte> SubArray
            {
                get { return Value; }
            }
            /// <summary>
            /// 数组
            /// </summary>
            public byte[] Array
            {
                get { return Value.Array; }
            }
            /// <summary>
            /// 数组子串入池处理
            /// </summary>
            internal pushPool<byte[]> PushPool;
            /// <summary>
            /// 数组子串入池处理
            /// </summary>
            public void Push()
            {
                if (PushPool == null) Value.array = null;
                else
                {
                    try
                    {
                        PushPool(ref Value.array);
                    }
                    catch (Exception error)
                    {
                        LogPlus.Error.Add(error, null, false);
                    }
                }
            }
            /// <summary>
            /// 缓冲数组子串
            /// </summary>
            /// <param name="value">数组子串</param>
            /// <param name="pushPool">数组子串入池处理</param>
            public PushSubArrayStruct(SubArrayStruct<byte> value, pushPool<byte[]> pushPool)
            {
                Value = value;
                PushPool = pushPool;
            }
        }
        /// <summary>
        /// 缓冲区尺寸
        /// </summary>
        public int Size { get; protected set; }
        /// <summary>
        /// 保存缓冲区调用委托
        /// </summary>
        public pushPool<byte[]> PushHandle { get; protected set; }
        /// <summary>
        /// 保存缓冲区调用委托
        /// </summary>
        public Action<SubArrayStruct<byte>> PushSubArray { get; protected set; }
        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        public void Clear()
        {
            clear(0);
        }
        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public void Clear(int count)
        {
            clear(count <= 0 ? 0 : count);
        }
        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        protected abstract void clear(int count);
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns>缓冲区,失败返回null</returns>
        public abstract byte[] TryGet();
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区,失败返回null</returns>
        public byte[] TryGet(int minSize)
        {
            return minSize <= Size ? TryGet() : null;
        }
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        public byte[] Get()
        {
            byte[] data = TryGet();
            return data ?? new byte[Size];
        }
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="isNew">是否新建缓冲区</param>
        /// <returns>缓冲区</returns>
        public byte[] Get(out bool isNew)
        {
            byte[] data = TryGet();
            if (data == null)
            {
                isNew = true;
                return new byte[Size];
            }
            isNew = false;
            return data;
        }
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区</returns>
        public byte[] Get(int minSize)
        {
            return minSize <= Size ? Get() : new byte[minSize];
        }
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <param name="isNew">是否新建缓冲区</param>
        /// <returns>缓冲区</returns>
        public byte[] Get(int minSize, out bool isNew)
        {
            if (minSize <= Size) return Get(out isNew);
            isNew = true;
            return new byte[minSize];
        }
        /// <summary>
        /// 保存缓冲区
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        public abstract void Push(ref byte[] buffer);
        /// <summary>
        /// 保存缓冲区
        /// </summary>
        /// <param name="buffer"></param>
        public void Push(SubArrayStruct<byte> buffer)
        {
            buffer.UnsafeSet(0, 0);
            Push(ref buffer._array);
        }
        /// <summary>
        /// 数组模式内存池
        /// </summary>
        private sealed class ArrayPoolPlus : MemoryPoolPlus
        {
            /// <summary>
            /// 缓冲区集合
            /// </summary>
            private objectPool<byte[]> pool;
            /// <summary>
            /// 内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public ArrayPoolPlus(int size)
            {
                pool = objectPool<byte[]>.Create();
                Size = size;
                PushHandle = Push;
                PushSubArray = Push;
            }
            /// <summary>
            /// 清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void clear(int count)
            {
                pool.Clear(count);
            }
            /// <summary>
            /// 获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override byte[] TryGet()
            {
                return pool.Pop();
            }
            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="buffer">缓冲区</param>
            public override void Push(ref byte[] buffer)
            {
                byte[] data = Interlocked.Exchange(ref buffer, null);
                if (data != null && data.Length == Size) pool.Push(data);
            }
        }
        /// <summary>
        /// 纠错模式内存池
        /// </summary>
        private sealed class DebugPoolPlus : MemoryPoolPlus
        {
            /// <summary>
            /// 缓冲区集合
            /// </summary>
            private HashSet<byte[]> _buffers;
            /// <summary>
            /// 缓冲区集合访问锁
            /// </summary>
            private int _bufferLock;
            /// <summary>
            /// 当前最大缓冲区数量
            /// </summary>
            private int _maxCount = config.appSetting.PoolSize;
            /// <summary>
            /// 内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public DebugPoolPlus(int size)
            {
                _buffers = hashSet.CreateOnly<byte[]>();
                Size = size;
                PushHandle = Push;
                PushSubArray = Push;
            }
            /// <summary>
            /// 清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void clear(int count)
            {
                interlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                _buffers.Clear();
                _bufferLock = 0;
            }
            /// <summary>
            /// 获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override byte[] TryGet()
            {
                byte[] buffer = null;
                interlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                foreach (var data in _buffers)
                {
                    buffer = data;
                    break;
                }
                if (buffer != null) _buffers.Remove(buffer);
                _bufferLock = 0;
                return buffer;
            }
            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="buffer">缓冲区</param>
            public override void Push(ref byte[] buffer)
            {
                var data = Interlocked.Exchange(ref buffer, null);
                if (data != null && data.Length == Size)
                {
                    bool isAdd, isMax = false;
                    interlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                    try
                    {
                        if ((isAdd = _buffers.Add(data)) && (isMax = _buffers.Count > _maxCount))
                        {
                            _maxCount <<= 1;
                        }
                    }
                    finally { _bufferLock = 0; }
                    if (isAdd)
                    {
                        if (isMax)
                        {
                            LogPlus.Default.Add("内存池扩展实例数量 byte[" + _buffers.Count + "][" + Size + "]");
                        }
                    }
                    else LogPlus.Error.Add("内存池释放冲突 " + Size, true);
                }
            }
        }
        /// <summary>
        /// 内存池
        /// </summary>
        private static readonly Dictionary<int, MemoryPoolPlus> Pools;
        /// <summary>
        /// 内存池访问锁
        /// </summary>
        private static int _poolLock;
        /// <summary>
        /// 获取内存池[反射引用于 fastCSharp.memoryPoolProxy]
        /// </summary>
        /// <param name="size">缓冲区尺寸</param>
        /// <returns>内存池</returns>
        internal static MemoryPoolPlus GetPool(int size)
        {
            if (size <= 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            MemoryPoolPlus pool;
            interlocked.NoCheckCompareSetSleep0(ref _poolLock);
            if (Pools.TryGetValue(size, out pool)) _poolLock = 0;
            else
            {
                try
                {
                    Pools.Add(size, pool = config.appSetting.IsPoolDebug ? (MemoryPoolPlus)new DebugPoolPlus(size) : new ArrayPoolPlus(size));
                }
                finally { _poolLock = 0; }
            }
            return pool;
        }
        /// <summary>
        /// 清除内存池
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public static void ClearPool(int count = 0)
        {
            if (count <= 0) count = 0;
            interlocked.NoCheckCompareSetSleep0(ref _poolLock);
            foreach (MemoryPoolPlus pool in Pools.Values) pool.clear(count);
            _poolLock = 0;
        }
        /// <summary>
        /// 默认临时缓冲区
        /// </summary>
        public static readonly MemoryPoolPlus TinyBuffers;
        /// <summary>
        /// 默认流缓冲区
        /// </summary>
        public static readonly MemoryPoolPlus StreamBuffers;
        /// <summary>
        /// 获取临时缓冲区
        /// </summary>
        /// <param name="length">缓冲区字节长度</param>
        /// <returns>临时缓冲区</returns>
        public static MemoryPoolPlus GetDefaultPool(int length)
        {
            return length <= UnmanagedStreamBasePlus.DefaultLength ? TinyBuffers : StreamBuffers;
        }

        static MemoryPoolPlus()
        {
            Pools = dictionary.CreateInt<memoryPool>();
            Pools.Add(UnmanagedStreamBasePlus.DefaultLength, TinyBuffers = config.appSetting.IsPoolDebug ? (MemoryPoolPlus)new DebugPoolPlus(UnmanagedStreamBasePlus.DefaultLength) : new ArrayPoolPlus(UnmanagedStreamBasePlus.DefaultLength));
            Pools.Add(config.appSetting.StreamBufferSize, StreamBuffers = config.appSetting.IsPoolDebug ? (MemoryPoolPlus)new DebugPoolPlus(config.appSetting.StreamBufferSize) : new ArrayPoolPlus(config.appSetting.StreamBufferSize));
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
