//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PhysicalServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.MemoryDataBase
//	File Name:  PhysicalServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:09:44
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
using TerumoMIS.CoreLibrary.Code.Csharper;

namespace TerumoMIS.CoreLibrary.MemoryDataBase
{
    /// <summary>
    /// 数据库物理层服务
    /// </summary>
    [TcpServerPlus(Service = "MemoryDataBasePhysical",IsIdentityCommand = true,IsServerAsynchronousReceive = false, IsClientAsynchronousReceive = false, VerifyMethodType =typeof(VerifyMethodPlus))]
    public partial class PhysicalServerPlus
    {
        /// <summary>
        /// 数据库物理层唯一标识
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct timeIdentity
        {
            /// <summary>
            /// 服务器启动时间
            /// </summary>
            public long TimeTick;
            /// <summary>
            /// 数据库物理层集合索引
            /// </summary>
            public int Index;
            /// <summary>
            /// 数据库物理层集合索引编号
            /// </summary>
            public int Identity;
            /// <summary>
            /// 转换成数据库物理层集合唯一标识
            /// </summary>
            /// <returns>数据库物理层集合唯一标识</returns>
            internal physicalSet.identity GetIdentity()
            {
                return new physicalSet.identity { Index = Index, Identity = Identity };
            }
        }
        /// <summary>
        /// 数据库物理层初始化信息
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct physicalIdentity
        {
            /// <summary>
            /// 数据库物理层唯一标识
            /// </summary>
            public timeIdentity Identity;
            /// <summary>
            /// 是否新建文件
            /// </summary>
            public bool IsLoader;
            /// <summary>
            /// 数据库文件是否成功打开
            /// </summary>
            public bool IsOpen
            {
                get { return Identity.TimeTick != 0; }
            }
        }
        /// <summary>
        /// 数据库物理层服务验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024)]
        protected virtual bool verify(string value)
        {
            if (fastCSharp.config.memoryDatabase.Default.PhysicalVerify == null && !fastCSharp.config.pub.Default.IsDebug)
            {
                fastCSharp.log.Error.Add("数据库物理层服务验证数据不能为空", false, true);
                return false;
            }
            return fastCSharp.config.memoryDatabase.Default.PhysicalVerify == value;
        }
        /// <summary>
        /// 打开数据库
        /// </summary>
        /// <param name="fileName">数据文件名</param>
        /// <returns>数据库物理层初始化信息</returns>
        [fastCSharp.code.cSharp.tcpServer(IsClientAsynchronous = true, IsClientSynchronous = false)]
        private physicalIdentity open(string fileName)
        {
            return physicalSet.Default.Open(fileName);
        }
        /// <summary>
        /// 关闭数据库文件
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        private void close(timeIdentity identity)
        {
            if (identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks) physicalSet.Default.Close(identity.GetIdentity(), false);
        }
        ///// <summary>
        ///// 关闭数据库文件
        ///// </summary>
        ///// <param name="identity">数据库物理层唯一标识</param>
        //[fastCSharp.code.cSharp.tcpServer]
        //private void waitClose(timeIdentity identity)
        //{
        //    if (identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks) physicalSet.Default.Close(identity.GetIdentity(), true);
        //}
        ///// <summary>
        ///// 关闭数据库文件
        ///// </summary>
        ///// <param name="identity">数据文件名</param>
        //[fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        //private void close(string fileName)
        //{
        //    physicalSet.identity identity = physicalSet.Default.GetIdentity(fileName);
        //    if (identity.IsValid) physicalSet.Default.Close(identity, false);
        //}
        ///// <summary>
        ///// 关闭数据库文件
        ///// </summary>
        ///// <param name="identity">数据文件名</param>
        //[fastCSharp.code.cSharp.tcpServer]
        //private void waitClose(string fileName)
        //{
        //    physicalSet.identity identity = physicalSet.Default.GetIdentity(fileName);
        //    if (identity.IsValid) physicalSet.Default.Close(identity, true);
        //}
        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="stream">文件头数据流</param>
        /// <returns>是否成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        private bool create(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream stream)
        {
            timeIdentity identity = getIdentity(ref stream.Buffer);
            return stream.Buffer.array != null && physicalSet.Default.Create(identity.GetIdentity(), stream.Buffer);
        }
        /// <summary>
        /// 数据库文件数据加载
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        [fastCSharp.code.cSharp.tcpServer(IsClientCallbackTask = false, IsClientAsynchronous = true, IsClientSynchronous = false)]
        private fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer loadHeader(timeIdentity identity)
        {
            if (identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks) return physicalSet.Default.LoadHeader(identity.GetIdentity());
            return default(fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer);
        }
        /// <summary>
        /// 数据库文件数据加载
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        [fastCSharp.code.cSharp.tcpServer(IsClientCallbackTask = false, IsClientAsynchronous = true, IsClientSynchronous = false)]
        private fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer load(timeIdentity identity)
        {
            if (identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks) return physicalSet.Default.Load(identity.GetIdentity());
            return default(fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer);
        }
        /// <summary>
        /// 数据库文件加载完毕
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="isLoaded">是否加载成功</param>
        /// <returns>是否加载成功</returns>
        [fastCSharp.code.cSharp.tcpServer]
        private bool loaded(timeIdentity identity, bool isLoaded)
        {
            return identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks && physicalSet.Default.Loaded(identity.GetIdentity(), isLoaded);
        }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="dataStream">日志数据</param>
        /// <returns>是否成功写入缓冲区</returns>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false, IsClientCallbackTask = false, IsClientAsynchronous = true)]
        private unsafe int append(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream)
        {
            timeIdentity identity = getIdentity(ref dataStream.Buffer);
            return dataStream.Buffer.array != null ? physicalSet.Default.Append(identity.GetIdentity(), dataStream.Buffer) : 0;
        }
        /// <summary>
        /// 等待缓存写入
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        [fastCSharp.code.cSharp.tcpServer]
        private void waitBuffer(timeIdentity identity)
        {
            if (identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks) physicalSet.Default.WaitBuffer(identity.GetIdentity());
        }
        /// <summary>
        /// 写入缓存
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <returns>是否成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        private bool flush(timeIdentity identity)
        {
            return identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks && physicalSet.Default.Flush(identity.GetIdentity());
        }
        /// <summary>
        /// 写入缓存
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
        /// <returns>是否成功</returns>
        [fastCSharp.code.cSharp.tcpServer]
        private bool flushFile(timeIdentity identity, bool isDiskFile)
        {
            return identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks && physicalSet.Default.FlushFile(identity.GetIdentity(), isDiskFile);
        }
        /// <summary>
        /// 获取数据库物理层唯一标识
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>数据库物理层唯一标识</returns>
        private static unsafe timeIdentity getIdentity(ref subArray<byte> data)
        {
            timeIdentity identity;
            fixed (byte* dataFixed = data.Array)
            {
                identity = *(timeIdentity*)(dataFixed + data.StartIndex);
                if (identity.TimeTick == CoreLibrary.PubPlus.StartTime.Ticks)
                {
                    data.UnsafeSet(data.StartIndex + sizeof(timeIdentity), data.Count - sizeof(timeIdentity));
                }
                else data.UnsafeSet(null, 0, 0);
            }
            return identity;
        }
    }
}
