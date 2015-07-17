//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: UnmanagedPoolPlusPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  UnmanagedPoolPlusPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 8:15:26
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 非托管内存池
    /// </summary>
    public unsafe abstract class UnmanagedPoolPlus
    {
        /// <summary>
        /// 缓冲区尺寸
        /// </summary>
        public int Size { get; protected set; }
        /// <summary>
        /// 非托管内存数量
        /// </summary>
        protected abstract int Count { get; }
        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        public void Clear()
        {
            ClearBase(0);
        }
        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public void Clear(int count)
        {
            ClearBase(count <= 0 ? 0 : count);
        }
        /// <summary>
        /// 清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        protected abstract void ClearBase(int count);
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns>缓冲区,失败返回null</returns>
        public abstract PointerStruct TryGet();
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区,失败返回null</returns>
        public PointerStruct TryGet(int minSize)
        {
            return minSize <= Size ? TryGet() : new PointerStruct();
        }
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        public PointerStruct Get()
        {
            var data = TryGet();
            return data.Data != null ? data : UnmanagedPlus.Get(Size, false);
        }
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区</returns>
        public PointerStruct Get(int minSize)
        {
            return minSize <= Size ? Get() : UnmanagedPlus.Get(minSize, false);
        }
        /// <summary>
        /// 保存缓冲区
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        public abstract void Push(ref PointerStruct buffer);
        ///// <summary>
        ///// 保存缓冲区
        ///// </summary>
        ///// <param name="data">缓冲区</param>
        //public abstract void Push(byte* data);
        /// <summary>
        /// 数组模式内存池
        /// </summary>
        private sealed class ArrayPoolPlus : UnmanagedPoolPlus
        {
            /// <summary>
            /// 缓冲区集合
            /// </summary>
            private ArrayPoolStruct<PointerStruct> pool;
            /// <summary>
            /// 非托管内存数量
            /// </summary>
            protected override int Count { get { return pool.Count; } }
            /// <summary>
            /// 内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public ArrayPoolPlus(int size)
            {
                pool = ArrayPoolStruct<PointerStruct>.Create();
                Size = size;
            }
            /// <summary>
            /// 清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void ClearBase(int count)
            {
                foreach (PointerStruct pointerStruct in pool.GetClear(count, false))
                {
                    try
                    {
                        UnmanagedPlus.Free(pointerStruct.Data);
                    }
                    catch (Exception error)
                    {
                        LogPlus.Error.Add(error, null, false);
                    }
                }
            }
            /// <summary>
            /// 获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override PointerStruct TryGet()
            {
                var value = default(PointerStruct);
                pool.TryGet(ref value);
                return value;
            }
            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="buffer">缓冲区</param>
            public override void Push(ref PointerStruct buffer)
            {
                void* data = buffer.Data;
                buffer.Data = null;
                if (data != null) pool.Push(new PointerStruct { Data = data });
            }
            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="data">缓冲区</param>
            public void Push(void* data)
            {
                if (data != null) pool.Push(new PointerStruct { Data = data });
            }
        }
        /// <summary>
        /// 纠错模式内存池
        /// </summary>
        private sealed class DebugPoolPlus : UnmanagedPoolPlus
        {
            /// <summary>
            /// 缓冲区集合
            /// </summary>
            private HashSet<PointerStruct> _buffers;
            /// <summary>
            /// 非托管内存数量
            /// </summary>
            protected override int Count { get { return _buffers.Count; } }
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
                _buffers = hashSet.CreatePointerStruct();
                Size = size;
            }
            /// <summary>
            /// 清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void ClearBase(int count)
            {
                interlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                PointerStruct[] removeBuffers;
                try
                {
                    removeBuffers = _buffers.getArray();
                    _buffers.Clear();
                }
                finally { _bufferLock = 0; }
                foreach (var pointerStruct in removeBuffers)
                {
                    try
                    {
                        UnmanagedPlus.Free(pointerStruct.Data);
                    }
                    catch (Exception error)
                    {
                        LogPlus.Error.Add(error, null, false);
                    }
                }
            }
            /// <summary>
            /// 获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override PointerStruct TryGet()
            {
                var buffer = new PointerStruct();
                interlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                foreach (var data in _buffers)
                {
                    buffer = data;
                    break;
                }
                if (buffer.Data != null) _buffers.Remove(buffer);
                _bufferLock = 0;
                return buffer;
            }
            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="buffer">缓冲区</param>
            public override void Push(ref PointerStruct buffer)
            {
                var data = buffer.Data;
                buffer.Data = null;
                if (data != null)
                {
                    bool isAdd, isMax = false;
                    interlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                    try
                    {
                        if ((isAdd = _buffers.Add(new PointerStruct { Data = data })) && (isMax = _buffers.Count > _maxCount))
                        {
                            _maxCount <<= 1;
                        }
                    }
                    finally { _bufferLock = 0; }
                    if (isAdd)
                    {
                        if (isMax)
                        {
                            LogPlus.Default.Add("非托管内存池扩展实例数量 byte*(" + Size + ")[" + _buffers.Count + "]");
                        }
                    }
                    else LogPlus.Error.Add("内存池释放冲突 " + Size, true);
                }
            }
            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="data">缓冲区</param>
            public void Push(void* data)
            {
                if (data != null)
                {
                    bool isAdd, isMax = false;
                    interlocked.NoCheckCompareSetSleep0(ref _bufferLock);
                    try
                    {
                        if ((isAdd = buffers.Add(new PointerStruct { Data = data })) && (isMax = buffers.Count > _maxCount))
                        {
                            _maxCount <<= 1;
                        }
                    }
                    finally { _bufferLock = 0; }
                    if (isAdd)
                    {
                        if (isMax)
                        {
                            LogPlus.Default.Add("非托管内存池扩展实例数量 byte*(" + Size + ")[" + buffers.Count.toString() + "]");
                        }
                    }
                    else LogPlus.Error.Add("内存池释放冲突 " + Size, true);
                }
            }
        }
        /// <summary>
        /// 内存池
        /// </summary>
        private static readonly Dictionary<int, UnmanagedPoolPlus> Pools;
        /// <summary>
        /// 内存池访问锁
        /// </summary>
        private static int _poolLock;
        /// <summary>
        /// 获取内存池[反射引用于 fastCSharp.UnmanagedPoolPlusProxy]
        /// </summary>
        /// <param name="size">缓冲区尺寸</param>
        /// <returns>内存池</returns>
        internal static UnmanagedPoolPlus GetPool(int size)
        {
            if (size <= 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            UnmanagedPoolPlus pool;
            interlocked.NoCheckCompareSetSleep0(ref _poolLock);
            if (Pools.TryGetValue(size, out pool)) _poolLock = 0;
            else
            {
                try
                {
                    Pools.Add(size, pool = config.appSetting.IsPoolDebug ? (UnmanagedPoolPlus)new DebugPoolPlus(size) : new ArrayPoolPlus(size));
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
            foreach (var pool in Pools.Values) pool.ClearBase(count);
            _poolLock = 0;
        }
        /// <summary>
        /// 默认临时缓冲区
        /// </summary>
        public static readonly UnmanagedPoolPlus TinyBuffers;
        /// <summary>
        /// 默认流缓冲区
        /// </summary>
        public static readonly UnmanagedPoolPlus StreamBuffers;
        /// <summary>
        /// 获取临时缓冲区
        /// </summary>
        /// <param name="length">缓冲区字节长度</param>
        /// <returns>临时缓冲区</returns>
        public static UnmanagedPoolPlus GetDefaultPool(int length)
        {
            return length <= UnmanagedStreamBasePlus.DefaultLength ? TinyBuffers : StreamBuffers;
        }
        /// <summary>
        /// 获取所有非托管内存的数量
        /// </summary>
        /// <returns></returns>
        public static int TotalCount
        {
            get
            {
                interlocked.NoCheckCompareSetSleep0(ref _poolLock);
                var count = Pools.Values.Aggregate(0, (current, pool) => current + current);
                _poolLock = 0;
                return count;
            }
        }

        static UnmanagedPoolPlus()
        {
            Pools = dictionary.CreateInt<UnmanagedPoolPlus>();
            Pools.Add(UnmanagedStreamBasePlus.DefaultLength, TinyBuffers = config.appSetting.IsPoolDebug ? (UnmanagedPoolPlus)new DebugPoolPlus(UnmanagedPlusStreamBase.DefaultLength) : new ArrayPoolPlus(UnmanagedPlusStreamBase.DefaultLength));
            Pools.Add(config.appSetting.StreamBufferSize, StreamBuffers = config.appSetting.IsPoolDebug ? (UnmanagedPoolPlus)new DebugPoolPlus(config.appSetting.StreamBufferSize) : new ArrayPoolPlus(config.appSetting.StreamBufferSize));
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
