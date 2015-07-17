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
using TerumoMIS.CoreLibrary.Config;
using TerumoMIS.CoreLibrary.Threading;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     非托管内存池
    /// </summary>
    public abstract unsafe class UnmanagedPoolPlus
    {
        /// <summary>
        ///     内存池
        /// </summary>
        private static readonly Dictionary<int, UnmanagedPoolPlus> Pools;

        /// <summary>
        ///     内存池访问锁
        /// </summary>
        private static int _poolLock;

        /// <summary>
        ///     默认临时缓冲区
        /// </summary>
        public static readonly UnmanagedPoolPlus TinyBuffers;

        /// <summary>
        ///     默认流缓冲区
        /// </summary>
        public static readonly UnmanagedPoolPlus StreamBuffers;

        static UnmanagedPoolPlus()
        {
            Pools = DictionaryPlus.CreateInt<UnmanagedPoolPlus>();
            Pools.Add(UnmanagedStreamBasePlus.DefaultLength,
                TinyBuffers =
                    AppSettingPlus.IsPoolDebug
                        ? (UnmanagedPoolPlus) new DebugPoolPlus(UnmanagedStreamBasePlus.DefaultLength)
                        : new ArrayPoolPlus(UnmanagedStreamBasePlus.DefaultLength));
            Pools.Add(AppSettingPlus.StreamBufferSize,
                StreamBuffers =
                    AppSettingPlus.IsPoolDebug
                        ? (UnmanagedPoolPlus) new DebugPoolPlus(AppSettingPlus.StreamBufferSize)
                        : new ArrayPoolPlus(AppSettingPlus.StreamBufferSize));
            if (AppSettingPlus.IsCheckMemory) CheckMemoryPlus.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     缓冲区尺寸
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        ///     非托管内存数量
        /// </summary>
        protected abstract int Count { get; }

        /// <summary>
        ///     获取所有非托管内存的数量
        /// </summary>
        /// <returns></returns>
        public static int TotalCount
        {
            get
            {
                InterlockedPlus.NoCheckCompareSetSleep0(ref _poolLock);
                var count = Pools.Values.Aggregate(0, (current, pool) => current + current);
                _poolLock = 0;
                return count;
            }
        }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        public void Clear()
        {
            ClearBase(0);
        }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public void Clear(int count)
        {
            ClearBase(count <= 0 ? 0 : count);
        }

        /// <summary>
        ///     清除缓冲区集合
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        protected abstract void ClearBase(int count);

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <returns>缓冲区,失败返回null</returns>
        public abstract PointerStruct TryGet();

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区,失败返回null</returns>
        public PointerStruct TryGet(int minSize)
        {
            return minSize <= Size ? TryGet() : new PointerStruct();
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        public PointerStruct Get()
        {
            var data = TryGet();
            return data.Data != null ? data : UnmanagedPlus.Get(Size, false);
        }

        /// <summary>
        ///     获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区</returns>
        public PointerStruct Get(int minSize)
        {
            return minSize <= Size ? Get() : UnmanagedPlus.Get(minSize, false);
        }

        /// <summary>
        ///     保存缓冲区
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        public abstract void Push(ref PointerStruct buffer);

        /// <summary>
        ///     获取内存池[反射引用于 fastCSharp.UnmanagedPoolPlusProxy]
        /// </summary>
        /// <param name="size">缓冲区尺寸</param>
        /// <returns>内存池</returns>
        internal static UnmanagedPoolPlus GetPool(int size)
        {
            if (size <= 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            UnmanagedPoolPlus pool;
            InterlockedPlus.NoCheckCompareSetSleep0(ref _poolLock);
            if (Pools.TryGetValue(size, out pool)) _poolLock = 0;
            else
            {
                try
                {
                    Pools.Add(size,
                        pool =
                            AppSettingPlus.IsPoolDebug
                                ? (UnmanagedPoolPlus) new DebugPoolPlus(size)
                                : new ArrayPoolPlus(size));
                }
                finally
                {
                    _poolLock = 0;
                }
            }
            return pool;
        }

        /// <summary>
        ///     清除内存池
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public static void ClearPool(int count = 0)
        {
            if (count <= 0) count = 0;
            InterlockedPlus.NoCheckCompareSetSleep0(ref _poolLock);
            foreach (var pool in Pools.Values) pool.ClearBase(count);
            _poolLock = 0;
        }

        /// <summary>
        ///     获取临时缓冲区
        /// </summary>
        /// <param name="length">缓冲区字节长度</param>
        /// <returns>临时缓冲区</returns>
        public static UnmanagedPoolPlus GetDefaultPool(int length)
        {
            return length <= UnmanagedStreamBasePlus.DefaultLength ? TinyBuffers : StreamBuffers;
        }

        ///// <summary>
        ///// 保存缓冲区
        ///// </summary>
        ///// <param name="data">缓冲区</param>
        //public abstract void Push(byte* data);
        /// <summary>
        ///     数组模式内存池
        /// </summary>
        private class ArrayPoolPlus : UnmanagedPoolPlus
        {
            /// <summary>
            ///     缓冲区集合
            /// </summary>
            private ArrayPoolStruct<PointerStruct> _pool;

            /// <summary>
            ///     内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public ArrayPoolPlus(int size)
            {
                _pool = ArrayPoolStruct<PointerStruct>.Create();
                Size = size;
            }

            /// <summary>
            ///     非托管内存数量
            /// </summary>
            protected override int Count
            {
                get { return _pool.Count; }
            }

            /// <summary>
            ///     清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void ClearBase(int count)
            {
                foreach (var pointerStruct in _pool.GetClear(count, false))
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
            ///     获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override PointerStruct TryGet()
            {
                var value = default(PointerStruct);
                _pool.TryGet(ref value);
                return value;
            }

            /// <summary>
            ///     保存缓冲区
            /// </summary>
            /// <param name="buffer">缓冲区</param>
            public override void Push(ref PointerStruct buffer)
            {
                var data = buffer.Data;
                buffer.Data = null;
                if (data != null) _pool.Push(new PointerStruct {Data = data});
            }

            /// <summary>
            ///     保存缓冲区
            /// </summary>
            /// <param name="data">缓冲区</param>
            public virtual void Push(void* data)
            {
                if (data != null) _pool.Push(new PointerStruct {Data = data});
            }
        }

        /// <summary>
        ///     纠错模式内存池
        /// </summary>
        private class DebugPoolPlus : UnmanagedPoolPlus
        {
            /// <summary>
            ///     缓冲区集合
            /// </summary>
            private readonly HashSet<PointerStruct> _buffers;

            /// <summary>
            ///     缓冲区集合访问锁
            /// </summary>
            private int _bufferLock;

            /// <summary>
            ///     当前最大缓冲区数量
            /// </summary>
            private int _maxCount = AppSettingPlus.PoolSize;

            /// <summary>
            ///     内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public DebugPoolPlus(int size)
            {
                _buffers = HashSetPlus.CreatePointer();
                Size = size;
            }

            /// <summary>
            ///     非托管内存数量
            /// </summary>
            protected override int Count
            {
                get { return _buffers.Count; }
            }

            /// <summary>
            ///     清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void ClearBase(int count)
            {
                InterlockedPlus.NoCheckCompareSetSleep0(ref _bufferLock);
                PointerStruct[] removeBuffers;
                try
                {
                    removeBuffers = _buffers.getArray();
                    _buffers.Clear();
                }
                finally
                {
                    _bufferLock = 0;
                }
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
            ///     获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override PointerStruct TryGet()
            {
                var buffer = new PointerStruct();
                InterlockedPlus.NoCheckCompareSetSleep0(ref _bufferLock);
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
            ///     保存缓冲区
            /// </summary>
            /// <param name="buffer">缓冲区</param>
            public override void Push(ref PointerStruct buffer)
            {
                var data = buffer.Data;
                buffer.Data = null;
                if (data != null)
                {
                    bool isAdd, isMax = false;
                    InterlockedPlus.NoCheckCompareSetSleep0(ref _bufferLock);
                    try
                    {
                        if ((isAdd = _buffers.Add(new PointerStruct {Data = data})) &&
                            (isMax = _buffers.Count > _maxCount))
                        {
                            _maxCount <<= 1;
                        }
                    }
                    finally
                    {
                        _bufferLock = 0;
                    }
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
            ///     保存缓冲区
            /// </summary>
            /// <param name="data">缓冲区</param>
            public virtual void Push(void* data)
            {
                if (data != null)
                {
                    bool isAdd, isMax = false;
                    InterlockedPlus.NoCheckCompareSetSleep0(ref _bufferLock);
                    try
                    {
                        if ((isAdd = _buffers.Add(new PointerStruct {Data = data})) &&
                            (isMax = _buffers.Count > _maxCount))
                        {
                            _maxCount <<= 1;
                        }
                    }
                    finally
                    {
                        _bufferLock = 0;
                    }
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
        }
    }
}