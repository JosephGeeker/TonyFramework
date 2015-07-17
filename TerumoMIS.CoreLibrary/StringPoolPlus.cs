//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: StringPoolPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  StringPoolPlus
//	User name:  C1400008
//	Location Time: 2015/7/17 14:09:32
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
    /// 快速字符串池
    /// </summary>
    internal abstract class StringPoolPlus
    {
        /// <summary>
        /// 缓冲区集合访问锁
        /// </summary>
        protected int bufferLock;
        /// <summary>
        /// 缓冲区尺寸(单位字符)
        /// </summary>
        public int Size { get; protected set; }
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
        public abstract string TryGet();
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">字符串长度</param>
        /// <returns>缓冲区,失败返回null</returns>
        public string TryGet(int minSize)
        {
            return minSize <= Size ? TryGet() : null;
        }
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        public string Get()
        {
            string value = TryGet();
            return value ?? String.FastAllocateString(Size);
        }
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <param name="minSize">数据字节长度</param>
        /// <returns>缓冲区</returns>
        public string Get(int minSize)
        {
            return minSize <= Size ? Get() : String.FastAllocateString(minSize);
        }
        /// <summary>
        /// 保存缓冲区
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        public abstract void Push(ref string buffer);
        /// <summary>
        /// 数组模式内存池
        /// </summary>
        private sealed class arrayPool : stringPool
        {
            /// <summary>
            /// 缓冲区
            /// </summary>
            private struct buffer
            {
                /// <summary>
                /// 缓冲区
                /// </summary>
                public string Buffer;
                /// <summary>
                /// 释放缓冲区
                /// </summary>
                /// <returns>缓冲区</returns>
                public string Free()
                {
                    string buffer = Buffer;
                    Buffer = null;
                    return buffer;
                }
            }
            /// <summary>
            /// 缓冲区集合
            /// </summary>
            private buffer[] buffers;
            /// <summary>
            /// 缓冲区集合更新访问锁
            /// </summary>
            private readonly object newBufferLock = new object();
            /// <summary>
            /// 缓冲区数量
            /// </summary>
            private int count;
            /// <summary>
            /// 内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public arrayPool(int size)
            {
                buffers = new buffer[config.appSetting.PoolSize];
                Size = size;
            }
            /// <summary>
            /// 清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void clear(int count)
            {
                interlocked.CompareSetSleep0(ref bufferLock);
                int length = this.count - count;
                if (length > 0) Array.Clear(buffers, this.count = count, length);
                bufferLock = 0;
            }
            /// <summary>
            /// 获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override string TryGet()
            {
                interlocked.CompareSetSleep0(ref bufferLock);
                if (count == 0)
                {
                    bufferLock = 0;
                    return null;
                }
                string buffer = buffers[--count].Free();
                bufferLock = 0;
                return buffer;
            }
            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="buffer">缓冲区</param>
            public override void Push(ref string buffer)
            {
                string value = Interlocked.Exchange(ref buffer, null);
                if (value != null && value.Length == Size)
                {
                PUSH:
                    interlocked.CompareSetSleep0(ref bufferLock);
                    if (count == buffers.Length)
                    {
                        int length = count;
                        bufferLock = 0;
                        Monitor.Enter(newBufferLock);
                        if (length == buffers.Length)
                        {
                            try
                            {
                                buffer[] newBuffers = new buffer[length << 1];
                                interlocked.CompareSetSleep0(ref bufferLock);
                                Array.Copy(buffers, 0, newBuffers, 0, count);
                                newBuffers[count].Buffer = value;
                                buffers = newBuffers;
                                ++count;
                                bufferLock = 0;
                            }
                            finally { Monitor.Exit(newBufferLock); }
                            return;
                        }
                        Monitor.Exit(newBufferLock);
                        goto PUSH;
                    }
                    buffers[count++].Buffer = value;
                    bufferLock = 0;
                }
            }
        }
        /// <summary>
        /// 纠错模式内存池
        /// </summary>
        private sealed class debugPool : stringPool
        {
            /// <summary>
            /// 缓冲区
            /// </summary>
            private struct buffer : IEquatable<buffer>
            {
                /// <summary>
                /// 缓冲区
                /// </summary>
                public string Buffer;
                /// <summary>
                /// 判断缓冲区是否同一个实例
                /// </summary>
                /// <param name="other"></param>
                /// <returns></returns>
                public bool Equals(buffer other)
                {
                    return Object.ReferenceEquals(Buffer, other.Buffer);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                public override int GetHashCode()
                {
                    return Buffer.GetHashCode();
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="obj"></param>
                /// <returns></returns>
                public override bool Equals(object obj)
                {
                    return Equals((buffer)obj);
                }
                /// <summary>
                /// 释放缓冲区
                /// </summary>
                /// <returns>缓冲区</returns>
                public string Free()
                {
                    string buffer = Buffer;
                    Buffer = null;
                    return buffer;
                }
            }
            /// <summary>
            /// 缓冲区集合
            /// </summary>
            private HashSet<buffer> buffers;
            /// <summary>
            /// 当前最大缓冲区数量
            /// </summary>
            private int maxCount = unmanagedStreamBase.DefaultLength;
            /// <summary>
            /// 内存池
            /// </summary>
            /// <param name="size">缓冲区尺寸</param>
            public debugPool(int size)
            {
                buffers = new HashSet<buffer>();
                Size = size;
            }
            /// <summary>
            /// 清除缓冲区集合
            /// </summary>
            /// <param name="count">保留清除缓冲区数量</param>
            protected override void clear(int count)
            {
                interlocked.CompareSetSleep0(ref bufferLock);
                buffers.Clear();
                bufferLock = 0;
            }
            /// <summary>
            /// 获取缓冲区
            /// </summary>
            /// <returns>缓冲区,失败返回null</returns>
            public override string TryGet()
            {
                buffer buffer = new buffer();
                interlocked.CompareSetSleep0(ref bufferLock);
                foreach (buffer data in buffers)
                {
                    buffer = data;
                    break;
                }
                if (buffer.Buffer != null) buffers.Remove(buffer);
                bufferLock = 0;
                return buffer.Buffer;
            }
            /// <summary>
            /// 保存缓冲区
            /// </summary>
            /// <param name="buffer">缓冲区</param>
            public override void Push(ref string buffer)
            {
                string value = Interlocked.Exchange(ref buffer, null);
                if (value != null && value.Length == Size)
                {
                    bool isAdd, isMax = false;
                    interlocked.CompareSetSleep0(ref bufferLock);
                    try
                    {
                        if ((isAdd = buffers.Add(new buffer { Buffer = value })) && (isMax = buffers.Count > maxCount))
                        {
                            maxCount <<= 1;
                        }
                    }
                    finally { bufferLock = 0; }
                    if (isAdd)
                    {
                        if (isMax)
                        {
                            log.Default.Add("快速字符串池 string(" + Size.toString() + ")[" + buffers.Count.toString() + "]", false, false);
                        }
                    }
                    else log.Default.Add("快速字符串池释放冲突 " + Size.toString(), true, false);
                }
            }
        }
        /// <summary>
        /// 内存池
        /// </summary>
        private static readonly Dictionary<int, stringPool> pools;
        /// <summary>
        /// 内存池访问锁
        /// </summary>
        private static int poolLock;
        /// <summary>
        /// 获取内存池[反射引用于 fastCSharp.stringPoolProxy]
        /// </summary>
        /// <param name="size">缓冲区尺寸</param>
        /// <returns>内存池</returns>
        internal static stringPool GetPool(int size)
        {
            if (size <= 0) log.Default.Throw(log.exceptionType.IndexOutOfRange);
            stringPool pool;
            interlocked.CompareSetSleep0(ref poolLock);
            if (pools.TryGetValue(size, out pool)) poolLock = 0;
            else
            {
                try
                {
                    pools.Add(size, pool = config.appSetting.IsPoolDebug ? (stringPool)new debugPool(size) : new arrayPool(size));
                }
                finally { poolLock = 0; }
            }
            return pool;
        }
#if MONO
        /// <summary>
        /// 清除内存池
        /// </summary>
        public static void ClearPool() { ClearPool(0); }
        /// <summary>
        /// 清除内存池
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public static void ClearPool(int count)
#else
        /// <summary>
        /// 清除内存池
        /// </summary>
        /// <param name="count">保留清除缓冲区数量</param>
        public static void ClearPool(int count = 0)
#endif
        {
            if (count <= 0) count = 0;
            interlocked.CompareSetSleep0(ref poolLock);
            foreach (stringPool pool in pools.Values) pool.clear(count);
            poolLock = 0;
        }
        ///// <summary>
        ///// 默认临时缓冲区
        ///// </summary>
        //public static readonly stringPool TinyBuffers;
        ///// <summary>
        ///// 默认流缓冲区
        ///// </summary>
        //public static readonly stringPool StreamBuffers;
        ///// <summary>
        ///// 获取临时缓冲区
        ///// </summary>
        ///// <param name="length">缓冲区字节长度</param>
        ///// <returns>临时缓冲区</returns>
        //public static stringPool GetDefaultPool(int length)
        //{
        //    return length <= unmanagedStreamBase.DefaultLength ? TinyBuffers : StreamBuffers;
        //}

        static StringPoolPlus()
        {
#if MONO
            pools = new Dictionary<int, stringPool>(equalityComparer.Int);
#else
            pools = new Dictionary<int, stringPool>();
#endif
            //pools.Add(unmanagedStreamBase.DefaultLength, TinyBuffers = config.appSetting.IsPoolDebug ? (stringPool)new debugPool(unmanagedStreamBase.DefaultLength) : new arrayPool(unmanagedStreamBase.DefaultLength));
            //pools.Add(config.appSetting.StreamBufferSize, StreamBuffers = config.appSetting.IsPoolDebug ? (stringPool)new debugPool(config.appSetting.StreamBufferSize) : new arrayPool(config.appSetting.StreamBufferSize));
        }
    }
}
