//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SslServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  SslServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:42:19
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    /// <summary>
    /// 基于安全连接的Http服务
    /// </summary>
    internal sealed class SslServerPlus:ServerPlus
    {
        /// <summary>
        /// SSL证书
        /// </summary>
        private X509Certificate certificate;
        /// <summary>
        /// SSL证书文件内容
        /// </summary>
        private byte[] certificateFileData;
        /// <summary>
        /// HTTP服务
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="host">TCP服务端口信息</param>
        /// <param name="certificateFileName">安全证书文件</param>
        public sslServer(servers servers, host host, string certificateFileName)
            : base(servers, host)
        {
            certificateFileData = File.ReadAllBytes(certificateFileName);
            certificate = X509Certificate.CreateFromCertFile(certificateFileName);
        }
        /// <summary>
        /// 客户端请求处理
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        protected override void newSocket(Socket socket)
        {
            ipv6Hash ipv6 = default(ipv6Hash);
            int ipv4 = 0;
            clientQueue.socketType type = clientQueue.NewClient(new client { Server = servers, Socket = socket, Certificate = certificate }, socket, ref ipv4, ref ipv6);
            if (type == fastCSharp.net.clientQueue.socketType.Ipv4) sslStream.Start(servers, socket, ipv4, certificate);
            else if (type == fastCSharp.net.clientQueue.socketType.Ipv6) sslStream.Start(servers, socket, ipv6, certificate);
        }
        /// <summary>
        /// 检测安全证书文件
        /// </summary>
        /// <param name="certificateFileName">安全证书文件</param>
        /// <returns>是否成功</returns>
        internal override bool CheckCertificate(string certificateFileName)
        {
            return certificateFileName != null && certificateFileData.equal(File.ReadAllBytes(certificateFileName));
        }
        /// <summary>
        /// 请求处理结束
        /// </summary>
        /// <param name="ipv4">客户端IP</param>
        internal new static void SocketEnd(int ipv4)
        {
            client socket = clientQueue.End(ipv4);
            if (socket.Server != null) sslStream.Start(socket.Server, socket.Socket, ipv4, socket.Certificate);
        }
        /// <summary>
        /// 请求处理结束
        /// </summary>
        /// <param name="ipv6">客户端IP</param>
        internal new static void SocketEnd(ipv6Hash ipv6)
        {
            client socket = clientQueue.End(ipv6);
            if (socket.Server != null) sslStream.Start(socket.Server, socket.Socket, ipv6, socket.Certificate);
        }
    }
}
