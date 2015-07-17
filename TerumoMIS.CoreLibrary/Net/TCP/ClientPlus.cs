//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ClientPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP
//	File Name:  ClientPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:13:01
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Net.TCP
{
    /// <summary>
    /// Tcp客户端
    /// </summary>
    public class ClientPlus:IDisposable
    {
        /// <summary>
        /// 最后一次连接是否没有指定IP地址
        /// </summary>
        private static bool isAnyIpAddress;
        /// <summary>
        /// 配置信息
        /// </summary>
        protected fastCSharp.code.cSharp.tcpServer attribute;
        /// <summary>
        /// TCP客户端
        /// </summary>
        protected Socket tcpClient;
        /// <summary>
        /// 套接字
        /// </summary>
        private net.socket netSocket;
        /// <summary>
        /// 套接字
        /// </summary>
        public net.socket NetSocket
        {
            get
            {
                if (tcpClient == null) netSocket = new net.socket(tcpClient, true);
                return netSocket;
            }
        }
        /// <summary>
        /// 是否正在释放资源
        /// </summary>
        protected bool isDispose;
        /// <summary>
        /// 是否启动TCP客户端
        /// </summary>
        public bool IsStart
        {
            get
            {
                return tcpClient != null;
            }
        }
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="isStart">是否启动客户端</param>
        public client(fastCSharp.code.cSharp.tcpServer attribute, bool isStart)
        {
            this.attribute = attribute;
            if (isStart) start();
        }
        /// <summary>
        /// 启动客户端链接
        /// </summary>
        protected virtual void start()
        {
            tcpClient = Create(attribute);
        }
        /// <summary>
        /// 停止客户端链接
        /// </summary>
        public virtual void Dispose()
        {
            if (!isDispose)
            {
                isDispose = true;
                dispose();
            }
        }
        /// <summary>
        /// 停止客户端链接
        /// </summary>
        protected void dispose()
        {
            //if (tcpClient != null) tcpClient.Close();
            tcpClient.shutdown();
            tcpClient = null;
            netSocket = null;
        }
        /// <summary>
        /// 创建TCP客户端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <returns>TCP客户端,失败返回null</returns>
        internal static Socket Create(fastCSharp.code.cSharp.tcpServer attribute)
        {
            Socket socket = null;
            try
            {
                if (attribute.IpAddress == IPAddress.Any)
                {
                    if (!isAnyIpAddress) log.Error.Add("客户端TCP连接失败(" + attribute.ServiceName + " " + attribute.Host + ":" + attribute.Port.toString() + ")", true, false);
                    isAnyIpAddress = true;
                    return null;
                }
                isAnyIpAddress = false;
                socket = new Socket(attribute.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(attribute.IpAddress, attribute.Port);
                return socket;
            }
            catch (Exception error)
            {
                log.Error.Add(error, "客户端TCP连接失败(" + attribute.ServiceName + " " + attribute.IpAddress.ToString() + ":" + attribute.Port.toString() + ")", false);
                log.Error.Add("客户端TCP连接失败(" + attribute.ServiceName + " " + attribute.IpAddress.ToString() + ":" + attribute.Port.toString() + ")", true, false);
                if (socket != null) socket.Close();
            }
            return null;
        }
    }
}
