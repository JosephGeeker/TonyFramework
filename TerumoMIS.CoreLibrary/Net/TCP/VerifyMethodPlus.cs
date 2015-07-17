//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: VerifyMethodPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP
//	File Name:  VerifyMethodPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:27:09
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Code.Csharper;

namespace TerumoMIS.CoreLibrary.Net.TCP
{
    /// <summary>
    /// 默认TCP调用验证函数
    /// </summary>
    public sealed class verifyMethod : fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<fastCSharp.net.tcp.commandLoadBalancingServer.commandClient>
        , fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<tcpClient.tcpRegister>
        , fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<tcpClient.memoryDatabasePhysical>
        , fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<tcpClient.fileBlock>
        , fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<tcpClient.httpServer>
        , fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<tcpClient.processCopy>
    {
        /// <summary>
        /// 负载均衡服务客户端验证
        /// </summary>
        /// <param name="client">负载均衡服务客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(fastCSharp.net.tcp.commandLoadBalancingServer.commandClient client)
        {
            return client.Verify(fastCSharp.config.tcpRegister.Default.Verify);
        }
        /// <summary>
        /// TCP注册服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(tcpClient.tcpRegister client)
        {
            return client.verify(fastCSharp.config.tcpRegister.Default.Verify).Value;
        }
        /// <summary>
        /// 数据库物理层服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(tcpClient.memoryDatabasePhysical client)
        {
            return client.verify(fastCSharp.config.memoryDatabase.Default.PhysicalVerify).Value;
        }
        /// <summary>
        /// 文件分块服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(tcpClient.fileBlock client)
        {
            return client.verify(fastCSharp.config.fileBlock.Default.Verify).Value;
        }
        /// <summary>
        /// HTTP服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(tcpClient.httpServer client)
        {
            return client.verify(fastCSharp.config.http.Default.HttpVerify).Value;
        }
        /// <summary>
        /// 进程复制重启服务客户端验证
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>是否通过验证</returns>
        public bool Verify(tcpClient.processCopy client)
        {
            return client.verify(fastCSharp.config.processCopy.Default.Verify).Value;
        }
    }
}
