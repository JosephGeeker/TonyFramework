//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: {Remember
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  _Remember
//	User name:  C1400008
//	Location Time: 2015/7/16 16:38:32
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
        /// TCP服务
        /// </summary>
        public partial class tcpRegister
        {
            /// <summary>
            /// 命令序号记忆数据
            /// </summary>
            private static keyValue<hashString, int>[] _identityCommandNames_()
            {
                keyValue<hashString, int>[] names = new keyValue<hashString, int>[7];
                names[0].Set(@"(string)verify", 0);
                names[1].Set(@"(fastCSharp.net.tcp.tcpRegister.clientId,fastCSharp.net.tcp.tcpRegister.service)register", 1);
                names[2].Set(@"(fastCSharp.net.tcp.tcpRegister.clientId,System.Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.pollResult>,bool>)poll", 2);
                names[3].Set(@"(fastCSharp.net.tcp.tcpRegister.clientId,string)removeRegister", 3);
                names[4].Set(@"(out int)getServices", 4);
                names[5].Set(@"()register", 5);
                names[6].Set(@"(fastCSharp.net.tcp.tcpRegister.clientId)removeRegister", 6);
                return names;
            }
        }
}
namespace fastCSharp.tcpServer
{

        /// <summary>
        /// TCP服务
        /// </summary>
        public partial class httpServer
        {
            /// <summary>
            /// 命令序号记忆数据
            /// </summary>
            private static keyValue<hashString, int>[] _identityCommandNames_()
            {
                keyValue<hashString, int>[] names = new keyValue<hashString, int>[7];
                names[0].Set(@"(string)verify", 0);
                names[1].Set(@"(fastCSharp.net.tcp.host)setForward", 1);
                names[2].Set(@"(fastCSharp.net.tcp.http.domain[])stop", 2);
                names[3].Set(@"(fastCSharp.net.tcp.http.domain)stop", 3);
                names[4].Set(@"(string,string,fastCSharp.net.tcp.http.domain[],bool)start", 4);
                names[5].Set(@"(string,string,fastCSharp.net.tcp.http.domain,bool)start", 5);
                names[6].Set(@"()removeForward", 6);
                return names;
            }
        }
}
namespace fastCSharp.tcpServer
{

        /// <summary>
        /// TCP服务
        /// </summary>
        public partial class processCopy
        {
            /// <summary>
            /// 命令序号记忆数据
            /// </summary>
            private static keyValue<hashString, int>[] _identityCommandNames_()
            {
                keyValue<hashString, int>[] names = new keyValue<hashString, int>[4];
                names[0].Set(@"(string)verify", 0);
                names[1].Set(@"(fastCSharp.diagnostics.processCopyServer.copyer)guard", 1);
                names[2].Set(@"(fastCSharp.diagnostics.processCopyServer.copyer)copyStart", 2);
                names[3].Set(@"(fastCSharp.diagnostics.processCopyServer.copyer)remove", 3);
                return names;
            }
        }
}
namespace fastCSharp.tcpServer
{

        /// <summary>
        /// TCP服务
        /// </summary>
        public partial class memoryDatabasePhysical
        {
            /// <summary>
            /// 命令序号记忆数据
            /// </summary>
            private static keyValue<hashString, int>[] _identityCommandNames_()
            {
                keyValue<hashString, int>[] names = new keyValue<hashString, int>[11];
                names[0].Set(@"(string)verify", 0);
                names[1].Set(@"(string)open", 1);
                names[2].Set(@"(fastCSharp.memoryDatabase.physicalServer.timeIdentity)close", 2);
                names[3].Set(@"(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream)create", 3);
                names[4].Set(@"(fastCSharp.memoryDatabase.physicalServer.timeIdentity)load", 4);
                names[5].Set(@"(fastCSharp.memoryDatabase.physicalServer.timeIdentity,bool)loaded", 5);
                names[6].Set(@"(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream)append", 6);
                names[7].Set(@"(fastCSharp.memoryDatabase.physicalServer.timeIdentity)waitBuffer", 7);
                names[8].Set(@"(fastCSharp.memoryDatabase.physicalServer.timeIdentity)flush", 8);
                names[9].Set(@"(fastCSharp.memoryDatabase.physicalServer.timeIdentity,bool)flushFile", 9);
                names[10].Set(@"(fastCSharp.memoryDatabase.physicalServer.timeIdentity)loadHeader", 10);
                return names;
            }
        }
}
namespace fastCSharp.tcpServer
{

        /// <summary>
        /// TCP服务
        /// </summary>
        public partial class fileBlock
        {
            /// <summary>
            /// 命令序号记忆数据
            /// </summary>
            private static keyValue<hashString, int>[] _identityCommandNames_()
            {
                keyValue<hashString, int>[] names = new keyValue<hashString, int>[5];
                names[0].Set(@"(string)verify", 0);
                names[1].Set(@"(fastCSharp.io.fileBlockStream.index,ref fastCSharp.code.cSharp.tcpBase.subByteArrayEvent,System.Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>,bool>)read", 1);
                names[2].Set(@"(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream)write", 2);
                names[3].Set(@"()waitBuffer", 3);
                names[4].Set(@"(bool)flush", 4);
                return names;
            }
        }
}
