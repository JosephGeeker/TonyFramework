//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  ServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:39:32
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

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    /// <summary>
    /// Http服务
    /// </summary>
    internal class ServerPlus:TCP.ServerPlus
    {
        /// <summary>
        /// 客户端队列信息
        /// </summary>
        protected struct client
        {
            /// <summary>
            /// HTTP服务器
            /// </summary>
            public servers Server;
            /// <summary>
            /// 套接字
            /// </summary>
            public Socket Socket;
            /// <summary>
            /// SSL证书
            /// </summary>
            public X509Certificate Certificate;
        }
        /// <summary>
        /// 客户端队列
        /// </summary>
        protected static readonly clientQueue<client> clientQueue = clientQueue<client>.Create(fastCSharp.config.http.Default.IpActiveClientCount, fastCSharp.config.http.Default.IpClientCount, dispose);
        /// <summary>
        /// HTTP服务器
        /// </summary>
        protected servers servers;
        /// <summary>
        /// 已绑定域名数量
        /// </summary>
        internal int DomainCount;
        /// <summary>
        /// HTTP服务
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="host">TCP服务端口信息</param>
        public server(servers servers, host host)
            : base(new code.cSharp.tcpServer { Host = host.Host, Port = host.Port, IsServer = true })
        {
            this.servers = servers;
            DomainCount = 1;
        }
        /// <summary>
        /// 获取客户端请求
        /// </summary>
        protected override void getSocket()
        {
            acceptSocket();
        }
        /// <summary>
        /// 客户端请求处理
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        protected override void newSocket(Socket socket)
        {
            ipv6Hash ipv6 = default(ipv6Hash);
            int ipv4 = 0;
            clientQueue.socketType type = clientQueue.NewClient(new client { Server = servers, Socket = socket }, socket, ref ipv4, ref ipv6);
            if (type == fastCSharp.net.clientQueue.socketType.Ipv4) http.socket.Start(servers, socket, ipv4);
            else if (type == fastCSharp.net.clientQueue.socketType.Ipv6) http.socket.Start(servers, socket, ipv6);
        }
        /// <summary>
        /// 检测安全证书文件
        /// </summary>
        /// <param name="certificateFileName">安全证书文件</param>
        /// <returns>是否成功</returns>
        internal virtual bool CheckCertificate(string certificateFileName)
        {
            return certificateFileName == null;
        }
        /// <summary>
        /// 请求处理结束
        /// </summary>
        /// <param name="ipv4">客户端IP</param>
        internal static void SocketEnd(int ipv4)
        {
            client socket = clientQueue.End(ipv4);
            if (socket.Server != null) http.socket.Start(socket.Server, socket.Socket, ipv4);
        }
        /// <summary>
        /// 请求处理结束
        /// </summary>
        /// <param name="ipv6">客户端IP</param>
        internal static void SocketEnd(ipv6Hash ipv6)
        {
            client socket = clientQueue.End(ipv6);
            if (socket.Server != null) http.socket.Start(socket.Server, socket.Socket, ipv6);
        }
        /// <summary>
        /// 关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        private static void dispose(client socket)
        {
            socket.Socket.shutdown();
        }
        static ServerPlus()
        {
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
