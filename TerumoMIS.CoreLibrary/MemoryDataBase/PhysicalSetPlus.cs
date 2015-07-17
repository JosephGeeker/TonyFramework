//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PhysicalSetPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.MemoryDataBase
//	File Name:  PhysicalSetPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:11:41
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

namespace TerumoMIS.CoreLibrary.MemoryDataBase
{
    /// <summary>
    /// 数据库物理层集合
    /// </summary>
    internal sealed class PhysicalSetPlus:IDisposable
    {
        /// <summary>
        /// 错误索引
        /// </summary>
        private const int errorIndex = int.MinValue;
        /// <summary>
        /// 数据库物理层集合唯一标识
        /// </summary>
        public struct identity
        {
            /// <summary>
            /// 数据库物理层集合索引
            /// </summary>
            public int Index;
            /// <summary>
            /// 数据库物理层集合索引编号
            /// </summary>
            public int Identity;
            /// <summary>
            /// 索引是否有效
            /// </summary>
            public bool IsValid
            {
                get { return Index != errorIndex; }
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="value">比较值</param>
            /// <returns>0表示相等</returns>
            public int Equals(identity value)
            {
                return (Index ^ value.Index) | (Identity ^ value.Identity);
            }
            /// <summary>
            /// 索引无效是设置索引
            /// </summary>
            /// <param name="value">目标值</param>
            /// <returns>是否成功</returns>
            public bool SetIfNull(identity value)
            {
                if (Index == errorIndex)
                {
                    Index = value.Index;
                    Identity = value.Identity;
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 数据库物理层集合索引编号
        /// </summary>
        private struct physicalInfo
        {
            /// <summary>
            /// 数据文件名
            /// </summary>
            public hashString FileName;
            /// <summary>
            /// 数据库物理层
            /// </summary>
            public physical Physical;
            /// <summary>
            /// 索引编号
            /// </summary>
            public int Identity;
            /// <summary>
            /// 设置数据库物理层
            /// </summary>
            /// <param name="fileName">数据文件名</param>
            /// <param name="physical">数据库物理层</param>
            public void Set(string fileName, physical physical)
            {
                FileName = fileName;
                Physical = physical;
            }
            /// <summary>
            /// 清除数据库物理层
            /// </summary>
            /// <param name="identity">索引编号</param>
            public void Clear()
            {
                Physical = null;
                FileName.Null();
                ++Identity;
            }
            /// <summary>
            /// 关闭数据库物理层
            /// </summary>
            /// <param name="isWait">是否等待结束</param>
            public void Close(bool isWait)
            {
                if (Physical != null)
                {
                    if (isWait) Physical.Dispose();
                    else fastCSharp.threading.threadPool.TinyPool.Start(Physical.Dispose);
                }
            }
        }
        /// <summary>
        /// 数据库物理层集合
        /// </summary>
        private physicalInfo[] physicals = new physicalInfo[255];
        /// <summary>
        /// 数据库物理层文件名与索引集合
        /// </summary>
        private readonly Dictionary<hashString, int> fileNameIndexs = dictionary.CreateHashString<int>();
        /// <summary>
        /// 数据库物理层空闲索引集合
        /// </summary>
        private subArray<int> freeIndexs;
        /// <summary>
        /// 数据库物理层集合访问锁
        /// </summary>
        private int physicalLock;
        /// <summary>
        /// 数据库物理层最大索引号
        /// </summary>
        private int maxIndex;
        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        private int isDisposed;
        /// <summary>
        /// 数据库物理层集合
        /// </summary>
        private physicalSet()
        {
            fastCSharp.domainUnload.Add(Dispose);
        }
        /// <summary>
        /// 获取一个可用的集合索引
        /// </summary>
        /// <returns>集合索引</returns>
        private int newIndex()
        {
            if (freeIndexs.Count != 0) return freeIndexs.UnsafePop();
            if (maxIndex == physicals.Length)
            {
                physicalInfo[] newPhysicals = new physicalInfo[maxIndex << 1];
                Array.Copy(physicals, 0, newPhysicals, 0, maxIndex);
                physicals = newPhysicals;
            }
            return maxIndex++;
        }
        /// <summary>
        /// 获取数据库物理层集合唯一标识
        /// </summary>
        /// <param name="fileName">数据文件名</param>
        /// <returns>数据库物理层集合唯一标识</returns>
        internal identity GetIdentity(string fileName)
        {
            int index;
            identity identity = new identity { Index = errorIndex };
            hashString key = fileName;
            if (fileNameIndexs.TryGetValue(key, out index))
            {
                identity.Identity = physicals[index].Identity;
                int nextIndex;
                if (fileNameIndexs.TryGetValue(key, out nextIndex) && index == nextIndex) identity.Index = index;
            }
            return identity;
        }
        /// <summary>
        /// 打开数据库
        /// </summary>
        /// <param name="fileName">数据文件名</param>
        /// <returns>数据库物理层初始化信息</returns>
        internal physicalServer.physicalIdentity Open(string fileName)
        {
            physicalServer.physicalIdentity physicalInfo = new physicalServer.physicalIdentity { Identity = new physicalServer.timeIdentity { TimeTick = 0, Index = -1 } };
            if (isDisposed == 0)
            {
                hashString key = fileName;
                interlocked.NoCheckCompareSetSleep0(ref physicalLock);
                try
                {
                    if (!fileNameIndexs.ContainsKey(key)) fileNameIndexs.Add(key, physicalInfo.Identity.Index = newIndex());
                }
                finally { physicalLock = 0; }
                if (physicalInfo.Identity.Index != -1)
                {
                    try
                    {
                        physical physical = new physical(fileName, false);
                        if (!physical.IsDisposed)
                        {
                            interlocked.NoCheckCompareSetSleep0(ref physicalLock);
                            physicals[physicalInfo.Identity.Index].Set(fileName, physical);
                            physicalLock = 0;
                            physicalInfo.Identity.Identity = physicals[physicalInfo.Identity.Index].Identity;
                            physicalInfo.Identity.TimeTick = CoreLibrary.PubPlus.StartTime.Ticks;
                            physicalInfo.IsLoader = physical.IsLoader;
                        }
                    }
                    finally
                    {
                        if (physicalInfo.Identity.TimeTick == 0)
                        {
                            interlocked.NoCheckCompareSetSleep0(ref physicalLock);
                            try
                            {
                                fileNameIndexs.Remove(key);
                            }
                            finally { physicalLock = 0; }
                        }
                    }
                }
            }
            return physicalInfo;
        }
        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="header">文件头数据</param>
        /// <returns>是否创建成功</returns>
        internal bool Create(identity identity, subArray<byte> header)
        {
            physicalInfo physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                if (physical.Physical.Create(header)) return true;
                Close(identity, false);
            }
            return false;
        }
        /// <summary>
        /// 关闭数据库文件
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="isWait">是否等待关闭</param>
        internal void Close(identity identity, bool isWait)
        {
            interlocked.NoCheckCompareSetSleep0(ref physicalLock);
            physicalInfo physical = physicals[identity.Index];
            try
            {
                if (physical.Identity == identity.Identity)
                {
                    physicals[identity.Index].Clear();
                    fileNameIndexs.Remove(physical.FileName);
                    freeIndexs.Add(identity.Index);
                }
            }
            finally { physicalLock = 0; }
            if (physical.Identity == identity.Identity) physical.Close(isWait);
        }
        /// <summary>
        /// 数据库文件头数据加载
        /// </summary>
        /// <param name="identity"></param>
        /// <returns>文件数据,null表示失败</returns>
        internal subArray<byte> LoadHeader(identity identity)
        {
            physicalInfo physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                subArray<byte> data = physical.Physical.LoadHeader();
                if (data.array != null) return data;
                Close(identity, false);
            }
            return default(subArray<byte>);
        }
        /// <summary>
        /// 数据库文件数据加载
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <returns>文件数据,空数组表示结束,null表示失败</returns>
        internal subArray<byte> Load(identity identity)
        {
            physicalInfo physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                subArray<byte> data = physical.Physical.Load();
                if (data.array != null) return data;
                Close(identity, false);
            }
            return default(subArray<byte>);
        }
        /// <summary>
        /// 数据库文件加载完毕
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <returns>是否加载成功</returns>
        internal bool Loaded(identity identity, bool isLoaded)
        {
            physicalInfo physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                if (physical.Physical.Loaded(isLoaded)) return true;
                Close(identity, false);
            }
            return false;
        }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="data">日志数据</param>
        /// <returns>是否成功写入缓冲区</returns>
        internal int Append(identity identity, subArray<byte> data)
        {
            physicalInfo physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                int value = physical.Physical.Append(data);
                if (value != 0) return value;
                Close(identity, false);
            }
            return 0;
        }
        /// <summary>
        /// 等待缓存写入
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        internal void WaitBuffer(identity identity)
        {
            physicalInfo physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity) physical.Physical.WaitBuffer();
        }
        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <returns>是否成功</returns>
        internal bool Flush(identity identity)
        {
            physicalInfo physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                if (physical.Physical.Flush()) return true;
                Close(identity, false);
            }
            return false;
        }
        /// <summary>
        /// 刷新写入文件缓存区
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
        /// <returns>是否成功</returns>
        internal bool FlushFile(identity identity, bool isDiskFile)
        {
            physicalInfo physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                if (physical.Physical.FlushFile(isDiskFile)) return true;
                Close(identity, false);
            }
            return false;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                fastCSharp.domainUnload.Remove(Dispose, false);
                interlocked.NoCheckCompareSetSleep0(ref physicalLock);
                physicalInfo[] physicals = this.physicals;
                try
                {
                    this.physicals = new physicalInfo[this.physicals.Length];
                    fileNameIndexs.Clear();
                    freeIndexs.Empty();
                    while (maxIndex != 0) physicals[--maxIndex].Close(false);
                }
                finally { physicalLock = 0; }
                physicals = null;
            }
        }
        /// <summary>
        /// 数据库物理层集合
        /// </summary>
        public static readonly physicalSet Default = new physicalSet();
        static physicalSet()
        {
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
