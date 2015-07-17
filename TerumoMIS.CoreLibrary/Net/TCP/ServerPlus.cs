//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP
//	File Name:  ServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:23:50
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
using System.Threading;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Net.TCP
{
    /// <summary>
    /// TCP调用服务端基类
    /// </summary>
    public abstract class ServerPlus : IDisposable
    {
        /// <summary>
        /// 配置信息
        /// </summary>
        protected internal fastCSharp.code.cSharp.tcpServer attribute;
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName
        {
            get { return attribute.ServiceName; }
        }
        /// <summary>
        /// TCP注册服务 客户端
        /// </summary>
        private fastCSharp.net.tcp.tcpRegister.client tcpRegisterClient;
        /// <summary>
        /// TCP监听服务器端套接字
        /// </summary>
        private Socket socket;
        /// <summary>
        /// 待处理的客户端集合
        /// </summary>
        private Socket[] newClients;
        /// <summary>
        /// 待处理的客户端数量
        /// </summary>
        private int newClientCount;
        /// <summary>
        /// 是否正在处理客户端集合
        /// </summary>
        private int isNewClientThread;
        /// <summary>
        /// 待处理的客户端集合访问锁
        /// </summary>
        private int newClientLock;
        /// <summary>
        /// 处理待处理客户端请求
        /// </summary>
        private Action waitSocketHandle;
        /// <summary>
        /// 是否已启动服务
        /// </summary>
        protected int isStart;
        /// <summary>
        /// 是否已启动服务
        /// </summary>
        public bool IsStart
        {
            get { return isStart != 0; }
        }
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        protected server(fastCSharp.code.cSharp.tcpServer attribute)
        {
            if (attribute == null) log.Error.Throw(log.exceptionType.Null);
            if (attribute.TcpRegisterName != null)
            {
                tcpRegisterClient = fastCSharp.net.tcp.tcpRegister.client.Get(attribute.TcpRegisterName);
                if (tcpRegisterClient == null) log.Error.Throw("TCP注册服务 " + attribute.TcpRegisterName + " 链接失败", true, false);
                fastCSharp.net.tcp.tcpRegister.registerState state = tcpRegisterClient.Register(attribute);
                if (state != fastCSharp.net.tcp.tcpRegister.registerState.Success) log.Error.Throw("TCP服务注册 " + attribute.ServiceName + " 失败 " + state.ToString(), true, false);
                log.Default.Add(attribute.ServiceName + " 注册 " + attribute.Host + ":" + attribute.Port.toString(), false, false);
            }
            if (!attribute.IsServer) log.Default.Add("配置未指明的TCP服务端 " + attribute.ServiceName, true, false);
            this.attribute = attribute;
        }
        /// <summary>
        /// 停止服务事件
        /// </summary>
        public event Action OnDisposed;
        /// <summary>
        /// 停止服务
        /// </summary>
        public virtual void Dispose()
        {
            if (Interlocked.CompareExchange(ref isStart, 0, 1) == 1)
            {
                log.Default.Add("停止服务 " + attribute.ServiceName + "[" + attribute.Host + ":" + attribute.Port.toString() + "]", true, false);
                fastCSharp.domainUnload.Remove(Dispose, false);
                if (tcpRegisterClient != null)
                {
                    tcpRegisterClient.RemoveRegister(attribute);
                    tcpRegisterClient = null;
                }
                pub.Dispose(ref this.socket);
                Socket[] sockets = null;
                interlocked.NoCheckCompareSetSleep0(ref newClientLock);
                if (newClients == null || newClientCount == 0) newClientLock = 0;
                else
                {
                    try
                    {
                        Array.Copy(newClients, sockets = new Socket[newClientCount], newClientCount);
                        Array.Clear(newClients, 0, newClientCount);
                        newClientCount = 0;
                    }
                    finally { newClientLock = 0; }
                    if (sockets != null)
                    {
                        foreach (Socket socket in sockets) socket.shutdown();
                    }
                }
                if (OnDisposed != null) OnDisposed();
            }
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns>是否成功</returns>
        protected bool start()
        {
            if (Interlocked.CompareExchange(ref isStart, 1, 0) == 0)
            {
                try
                {
                    socket = new Socket(attribute.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(new IPEndPoint(attribute.IpAddress, attribute.Port));
                    socket.Listen(int.MaxValue);
                    newClients = new Socket[16];
                    waitSocketHandle = waitSocket;
                }
                catch (Exception error)
                {
                    Dispose();
                    log.Error.ThrowReal(error, GetType().FullName + "服务器端口 " + attribute.Host + ":" + attribute.Port.toString() + " TCP连接失败)", false);
                }
                return isStart != 0;
            }
            return false;
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Start()
        {
            if (start())
            {
                threadPool.TinyPool.FastStart(getSocket, null, null);
                Thread.Sleep(0);
                fastCSharp.domainUnload.Add(Dispose);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取客户端请求
        /// </summary>
        protected abstract void getSocket();
        /// <summary>
        /// 获取客户端请求
        /// </summary>
        protected void acceptSocket()
        {
            while (isStart != 0)
            {
                try
                {
                    while (isStart != 0)
                    {
                        Socket socket = this.socket.Accept();
                        interlocked.NoCheckCompareSetSleep0(ref newClientLock);
                        int isNewClientThread = this.isNewClientThread;
                        if (newClientCount == this.newClients.Length)
                        {
                            try
                            {
                                Socket[] newClients = new Socket[newClientCount << 1];
                                this.newClients.CopyTo(newClients, 0);
                                newClients[newClientCount] = socket;
                                this.newClients = newClients;
                                this.isNewClientThread = 1;
                                ++newClientCount;
                            }
                            finally { newClientLock = 0; }
                        }
                        else
                        {
                            newClients[newClientCount] = socket;
                            this.isNewClientThread = 1;
                            ++newClientCount;
                            newClientLock = 0;
                        }
                        if (isNewClientThread == 0) threadPool.TinyPool.FastStart(waitSocketHandle, null, null);
                    }
                }
                catch (Exception error)
                {
                    if (isStart != 0)
                    {
                        log.Error.Add(error, null, false);
                        Thread.Sleep(1);
                    }
                }
            }
        }
        /// <summary>
        /// 处理待处理客户端请求
        /// </summary>
        private unsafe void waitSocket()
        {
            while (isStart != 0)
            {
                interlocked.NoCheckCompareSetSleep0(ref newClientLock);
                if (newClientCount == 0)
                {
                    isNewClientThread = 0;
                    newClientLock = 0;
                    return;
                }
                Socket socket = newClients[--newClientCount];
                newClientLock = 0;
                newSocket(socket);
            }
        }
        /// <summary>
        /// 客户端请求处理
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        protected abstract void newSocket(Socket socket);
    }
}
