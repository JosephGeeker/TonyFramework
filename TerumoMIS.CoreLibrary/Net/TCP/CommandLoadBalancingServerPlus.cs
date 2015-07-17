//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CommandLoadBalancingServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP
//	File Name:  CommandLoadBalancingServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:19:46
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

namespace TerumoMIS.CoreLibrary.Net.TCP
{
    /// <summary>
    /// TCP调用负载均衡服务端
    /// </summary>
    public abstract class CommandLoadBalancingServerPlus
    {
        /// <summary>
        /// 添加TCP调用服务端命令索引位置
        /// </summary>
        internal const int NewServerCommandIndex = tcp.commandServer.CommandStartIndex + 1;
        /// <summary>
        /// 移除TCP调用服务端命令索引位置
        /// </summary>
        internal const int RemoveServerCommandIndex = NewServerCommandIndex + 1;
        /// <summary>
        /// 最大错误间隔时钟周期
        /// </summary>
        protected static readonly long maxErrorTimeTicks = new TimeSpan(0, 0, 2).Ticks;
        /// <summary>
        /// 验证接口
        /// </summary>
        protected fastCSharp.code.cSharp.tcpBase.ITcpClientVerify _verify_;
        /// <summary>
        /// TCP调用服务端信息
        /// </summary>
        protected struct serverInfo
        {
            /// <summary>
            /// TCP调用套接字
            /// </summary>
            public commandServer.socket Socket;
            /// <summary>
            /// TCP调用套接字回话标识
            /// </summary>
            public commandServer.streamIdentity Identity;
            /// <summary>
            /// TCP调用服务端端口信息
            /// </summary>
            public host Host;
        }
        /// <summary>
        /// TCP调用负载均衡客户端
        /// </summary>
        public sealed class commandClient : IDisposable
        {
            /// <summary>
            /// 添加TCP调用服务端回调处理
            /// </summary>
            private sealed class serverReturn
            {
                /// <summary>
                /// 创建完成回调处理
                /// </summary>
                public Action<bool> OnReturn;
                /// <summary>
                /// 创建完成回调处理
                /// </summary>
                /// <param name="returnValue">返回值</param>
                public void OnNewServer(fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> returnValue)
                {
                    OnReturn(returnValue.IsReturn && returnValue.Value);
                }
            }
            /// <summary>
            /// TCP调用客户端
            /// </summary>
            private commandClient<commandClient> client;
            /// <summary>
            /// TCP调用客户端
            /// </summary>
            /// <param name="attribute">TCP调用服务器端配置信息</param>
            public commandClient(fastCSharp.code.cSharp.tcpServer attribute) : this(attribute, null) { }
            /// <summary>
            /// TCP调用客户端
            /// </summary>
            /// <param name="attribute">TCP调用服务器端配置信息</param>
            /// <param name="verifyMethod">TCP验证方法</param>
            public commandClient(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<commandClient> verifyMethod)
            {
                if (attribute == null) log.Error.Throw(log.exceptionType.Null);
                client = new commandClient<commandClient>(attribute, 1024, verifyMethod ?? new fastCSharp.net.tcp.verifyMethod(), this);
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                pub.Dispose(ref client);
            }
            /// <summary>
            /// TCP调用客户端验证
            /// </summary>
            /// <param name="verify">验证字符串</param>
            /// <returns>是否验证成功</returns>
            public bool Verify(string verify)
            {
                keyValue<tcp.commandClient.streamCommandSocket, fastCSharp.code.cSharp.asynchronousMethod.waitCall<bool>> wait = getWait(true);
                if (wait.Value != null)
                {
                    try
                    {
                        wait.Key.Get(wait.Value.OnReturn, tcp.commandServer.CommandStartIndex, fastCSharp.config.tcpRegister.Default.Verify, 1024, false, false, false);
                        fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> value = wait.Value.Value;
                        return value.IsReturn && value.Value;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                }
                return false;
            }
            /// <summary>
            /// 添加TCP调用服务端
            /// </summary>
            /// <param name="host">TCP调用服务端端口信息</param>
            /// <returns>是否添加成功</returns>
            public bool NewServer(host host)
            {
                keyValue<tcp.commandClient.streamCommandSocket, fastCSharp.code.cSharp.asynchronousMethod.waitCall<bool>> wait = getWait(false);
                if (wait.Value != null)
                {
                    try
                    {
                        wait.Key.Get(wait.Value.OnReturn, NewServerCommandIndex, host, 1024, false, false, false);
                        fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> value = wait.Value.Value;
                        return value.IsReturn && value.Value;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                }
                return false;
            }
            /// <summary>
            /// 添加TCP调用服务端
            /// </summary>
            /// <param name="host">TCP调用服务端端口信息</param>
            /// <param name="onReturn">创建完成回调处理</param>
            public void NewServer(host host, Action<bool> onReturn)
            {
                try
                {
                    client.StreamSocket.Get(onReturn == null ? null : (Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool>>)new serverReturn { OnReturn = onReturn }.OnNewServer, NewServerCommandIndex, host, 1024, false, true, false);
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (onReturn != null) onReturn(false);
            }
            /// <summary>
            /// 移除TCP调用服务端
            /// </summary>
            /// <param name="host">TCP调用服务端端口信息</param>
            /// <returns>是否移除成功</returns>
            public bool RemoveServer(host host)
            {
                keyValue<tcp.commandClient.streamCommandSocket, fastCSharp.code.cSharp.asynchronousMethod.waitCall<bool>> wait = getWait(false);
                if (wait.Value != null)
                {
                    try
                    {
                        wait.Key.Get(wait.Value.OnReturn, RemoveServerCommandIndex, host, 1024, false, false, false);
                        fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> value = wait.Value.Value;
                        return value.IsReturn && value.Value;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                }
                return false;
            }
            /// <summary>
            /// 获取同步等待调用
            /// </summary>
            /// <param name="isVerify">是否验证调用</param>
            /// <returns>TCP客户端套接字+同步等待调用</returns>
            private keyValue<tcp.commandClient.streamCommandSocket, fastCSharp.code.cSharp.asynchronousMethod.waitCall<bool>> getWait(bool isVerify)
            {
                try
                {
                    if (client != null)
                    {
                        tcp.commandClient.streamCommandSocket socket = isVerify ? client.VerifyStreamSocket : client.StreamSocket;
                        if (socket != null)
                        {
                            return new keyValue<tcp.commandClient.streamCommandSocket, code.cSharp.asynchronousMethod.waitCall<bool>>(socket, fastCSharp.code.cSharp.asynchronousMethod.waitCall<bool>.Get());
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                return default(keyValue<tcp.commandClient.streamCommandSocket, fastCSharp.code.cSharp.asynchronousMethod.waitCall<bool>>);
            }
        }
    }
    /// <summary>
    /// TCP调用负载均衡服务端
    /// </summary>
    /// <typeparam name="clientType">TCP调用客户端类型</typeparam>
    public abstract class commandLoadBalancingServer<clientType> : commandLoadBalancingServer, IDisposable where clientType : class, IDisposable
    {
        /// <summary>
        /// TCP调用负载均衡服务端
        /// </summary>
        private sealed class commandServer : fastCSharp.net.tcp.commandServer
        {
            /// <summary>
            /// TCP调用负载均衡服务端目标对象
            /// </summary>
            private readonly commandLoadBalancingServer<clientType> server;
            /// <summary>
            /// 自定义TCP调用服务端验证
            /// </summary>
            private readonly Func<subArray<byte>, bool> isVerify;
            /// <summary>
            /// TCP调用负载均衡服务端
            /// </summary>
            /// <param name="server">TCP调用负载均衡服务端目标对象</param>
            public commandServer(commandLoadBalancingServer<clientType> server) : this(server, null) { }
            /// <summary>
            /// TCP调用负载均衡服务端
            /// </summary>
            /// <param name="server">TCP调用负载均衡服务端目标对象</param>
            /// <param name="isVerify">自定义TCP调用服务端验证</param>
            public commandServer(commandLoadBalancingServer<clientType> server, Func<subArray<byte>, bool> isVerify)
                : base(server.attribute)
            {
                this.server = server;
                this.isVerify = isVerify;
                setCommands(3);
                identityOnCommands[verifyCommandIdentity = tcp.commandServer.CommandStartIndex].Set(verify, 1024);
                identityOnCommands[commandLoadBalancingServer.NewServerCommandIndex].Set(newServer, 1024);
                identityOnCommands[commandLoadBalancingServer.RemoveServerCommandIndex].Set(removeServer, 1024);
            }
            /// <summary>
            /// TCP调用服务端验证
            /// </summary>
            /// <param name="socket">TCP调用套接字</param>
            /// <param name="data">参数序列化数据</param>
            private void verify(socket socket, subArray<byte> data)
            {
                try
                {
                    string inputParameter = null;
                    if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                    {
                        bool isVerify = false;
                        if (this.isVerify == null)
                        {
                            if (fastCSharp.config.tcpRegister.Default.Verify == null && !fastCSharp.config.pub.Default.IsDebug)
                            {
                                log.Error.Add("TCP服务注册验证数据不能为空", false, true);
                            }
                            else isVerify = fastCSharp.config.tcpRegister.Default.Verify == inputParameter;
                        }
                        else isVerify = this.isVerify(data);
                        socket.IsVerifyMethod = true;
                        socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = true, Value = isVerify });
                        return;
                    }
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
            }
            /// <summary>
            /// 添加TCP调用服务端
            /// </summary>
            /// <param name="socket">TCP调用套接字</param>
            /// <param name="data">参数序列化数据</param>
            private void newServer(socket socket, subArray<byte> data)
            {
                try
                {
                    host host = new host();
                    if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref host))
                    {
                        fastCSharp.threading.threadPool.TinyPool.FastStart(server.newServerHandle, new serverInfo { Socket = socket, Identity = socket.Identity, Host = host }, null, null);
                        return;
                    }
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
            }
            /// <summary>
            /// 移除TCP调用服务端
            /// </summary>
            /// <param name="socket">TCP调用套接字</param>
            /// <param name="data">参数序列化数据</param>
            private void removeServer(socket socket, subArray<byte> data)
            {
                try
                {
                    host host = new host();
                    if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref host))
                    {
                        socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = true, Value = server.removeServer(host) });
                        return;
                    }
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
            }
        }
        /// <summary>
        /// TCP调用服务器信息
        /// </summary>
        public struct clientIdentity
        {
            /// <summary>
            /// TCP调用客户端
            /// </summary>
            public clientType Client;
            /// <summary>
            /// TCP调用客户端索引
            /// </summary>
            public int Index;
            /// <summary>
            /// 验证编号
            /// </summary>
            public int Identity;
            /// <summary>
            /// 设置TCP调用客户端
            /// </summary>
            /// <param name="client">TCP调用客户端</param>
            /// <param name="index">TCP调用客户端索引</param>
            internal void Set(clientType client, int index)
            {
                Client = client;
                Index = index;
            }
            /// <summary>
            /// 重置TCP调用客户端
            /// </summary>
            /// <param name="client">TCP调用客户端</param>
            internal void Set(clientType client)
            {
                Client = client;
                ++Identity;
            }
            /// <summary>
            /// 移除TCP调用客户端
            /// </summary>
            /// <returns>TCP调用客户端</returns>
            internal clientType GetRemove()
            {
                clientType client = Client;
                Client = null;
                ++Identity;
                return client;
            }
            /// <summary>
            /// 移除TCP调用客户端
            /// </summary>
            internal void Remove()
            {
                Client = null;
                ++Identity;
            }
        }
        /// <summary>
        /// TCP调用服务器信息
        /// </summary>
        public struct clientHost
        {
            /// <summary>
            /// TCP调用服务器信息
            /// </summary>
            public clientIdentity Client;
            /// <summary>
            /// 最后响应时间
            /// </summary>
            public DateTime LastTime;
            /// <summary>
            /// TCP调用端口信息
            /// </summary>
            public host Host;
            /// <summary>
            /// 当前处理数量
            /// </summary>
            public int Count;
            /// <summary>
            /// 设置TCP调用客户端
            /// </summary>
            /// <param name="host">TCP调用端口信息</param>
            /// <param name="client">TCP调用客户端</param>
            /// <param name="index">TCP调用客户端索引</param>
            internal void Set(host host, clientType client, int index)
            {
                Client.Set(client, index);
                Host = host;
                Count = 0;
            }
            /// <summary>
            /// 重置TCP调用客户端
            /// </summary>
            /// <param name="host">TCP调用端口信息</param>
            /// <param name="client">TCP调用客户端</param>
            internal void Set(host host, clientType client)
            {
                Client.Client = client;
                Host = host;
                Count = 0;
            }
            /// <summary>
            /// 重置TCP调用客户端
            /// </summary>
            /// <param name="host">TCP调用端口信息</param>
            /// <param name="client">TCP调用客户端</param>
            /// <returns>TCP调用客户端+未完成处理数量</returns>
            internal keyValue<clientType, int> ReSet(host host, clientType client)
            {
                if (Client.Client != null && Host.Equals(host))
                {
                    clientType removeClient = Client.Client;
                    int count = Count;
                    Client.Set(client);
                    Count = 0;
                    return new keyValue<clientType, int>(removeClient, count);
                }
                return default(keyValue<clientType, int>);
            }
            /// <summary>
            /// 移除TCP调用客户端
            /// </summary>
            /// <param name="host">TCP调用端口信息</param>
            /// <returns>TCP调用客户端+未完成处理数量</returns>
            internal keyValue<clientType, int> Remove(host host)
            {
                if (Client.Client != null && Host.Equals(host)) return Remove();
                return default(keyValue<clientType, int>);
            }
            /// <summary>
            /// 移除TCP调用客户端
            /// </summary>
            /// <returns>TCP调用客户端+未完成处理数量</returns>
            internal keyValue<clientType, int> Remove()
            {
                clientType client = Client.GetRemove();
                int count = Count;
                return new keyValue<clientType, int>(client, count);
            }
            /// <summary>
            /// 移除TCP调用客户端
            /// </summary>
            /// <param name="client">TCP调用客户端</param>
            /// <returns>TCP调用端口信息+未完成处理数量</returns>
            internal keyValue<host, int> Remove(clientType client)
            {
                if (Client.Client == client)
                {
                    Client.Remove();
                    return new keyValue<host, int>(Host, Count);
                }
                return default(keyValue<host, int>);
            }
            /// <summary>
            /// 测试当前处理数量
            /// </summary>
            /// <param name="count">最大处理数量</param>
            /// <returns>是否测试成功</returns>
            internal bool TryCount(int count)
            {
                if (Count <= count && Client.Client != null)
                {
                    ++Count;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// TCP调用客户端调用结束
            /// </summary>
            /// <param name="identity">验证编号</param>
            /// <returns>是否验证成功</returns>
            internal int End(int identity)
            {
                if (Client.Identity == identity)
                {
                    LastTime = date.NowSecond;
                    --Count;
                    return 1;
                }
                return 0;
            }
            /// <summary>
            /// TCP调用客户端调用错误
            /// </summary>
            /// <param name="identity">验证编号</param>
            /// <returns>开始错误时间</returns>
            internal keyValue<clientType, keyValue<host, int>> Error(int identity)
            {
                if (Client.Identity == identity)
                {
                    clientType client = Client.GetRemove();
                    return new keyValue<clientType, keyValue<host, int>>(client, new keyValue<host, int>(Host, Count - 1));
                }
                return default(keyValue<clientType, keyValue<host, int>>);
            }
            /// <summary>
            /// 超时检测
            /// </summary>
            /// <param name="timeout">超时时间</param>
            /// <returns>TCP调用客户端</returns>
            internal clientType CheckTimeout(DateTime timeout)
            {
                return LastTime < timeout ? Client.Client : null;
            }
        }
        /// <summary>
        /// TCP调用服务器端配置信息
        /// </summary>
        private fastCSharp.code.cSharp.tcpServer attribute;
        /// <summary>
        /// 验证函数接口
        /// </summary>
        protected fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<clientType> _verifyMethod_;
        /// <summary>
        /// TCP调用客户端集合
        /// </summary>
        private clientHost[] clients = new clientHost[sizeof(int)];
        /// <summary>
        /// TCP调用客户端空闲索引
        /// </summary>
        private list<int> freeIndexs = new list<int>();
        /// <summary>
        /// 已使用的TCP调用客户端数量(包括空闲索引)
        /// </summary>
        private int currentCount;
        /// <summary>
        /// 当前访问TCP调用客户端索引
        /// </summary>
        private int currentIndex;
        /// <summary>
        /// 当前调用总数
        /// </summary>
        private int callCount;
        /// <summary>
        /// TCP调用客户端访问锁
        /// </summary>
        private int clientLock;
        /// <summary>
        /// 最后一次检测时间
        /// </summary>
        private DateTime checkTime;
        /// <summary>
        /// 检测任务
        /// </summary>
        private Action checkHandle;
        /// <summary>
        /// 移除TCP调用客户端集合
        /// </summary>
        private keyValue<clientType, int>[] removeClients = nullValue<keyValue<clientType, int>>.Array;
        /// <summary>
        /// 是否启动检测任务
        /// </summary>
        private byte isCheckTask;
        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        private byte isDisposed;
        /// <summary>
        /// 添加TCP调用服务端
        /// </summary>
        private Action<serverInfo> newServerHandle;
        /// <summary>
        /// TCP调用负载均衡服务端
        /// </summary>
        private commandLoadBalancingServer()
        {
            checkHandle = check;
        }
        /// <summary>
        /// TCP调用负载均衡服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">验证函数接口</param>
        /// <param name="verify">验证接口</param>
        protected commandLoadBalancingServer(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<clientType> verifyMethod, fastCSharp.code.cSharp.tcpBase.ITcpClientVerify verify)
        {
        }
        /// <summary>
        /// TCP调用负载均衡服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">验证函数接口</param>
        /// <param name="verify">验证接口</param>
        protected commandLoadBalancingServer(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<clientType> verifyMethod)
            : this()
        {
            this.attribute = attribute;
            _verifyMethod_ = verifyMethod;
        }
        /// <summary>
        /// TCP调用负载均衡服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">验证接口</param>
        protected commandLoadBalancingServer(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerify verify)
            : this()
        {
            this.attribute = attribute;
            _verify_ = verify;
            newServerHandle = newServer;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            isDisposed = 1;
            Monitor.Enter(serverLock);
            pub.Dispose(ref server);
            Monitor.Exit(serverLock);
            int count = 0;
            clientType[] clients;
            do
            {
                clients = new clientType[this.clients.Length];
                interlocked.NoCheckCompareSetSleep0(ref clientLock);
                currentIndex = callCount = 0;
                if (currentCount <= clients.Length)
                {
                    while (count != currentCount) clients[count++] = this.clients[count++].Client.GetRemove();
                    freeIndexs.Clear();
                    currentCount = 0;
                    clientLock = 0;
                    break;
                }
                else clientLock = 0;
            }
            while (true);
            while (count != 0) pub.Dispose(ref clients[--count]);
        }
        /// <summary>
        /// TCP调用负载均衡服务端
        /// </summary>
        private commandServer server;
        /// <summary>
        /// TCP调用负载均衡服务端访问锁
        /// </summary>
        private object serverLock = new object();
        /// <summary>
        /// 超时检测时钟周期
        /// </summary>
        private long checkTicks;
        /// <summary>
        /// 启动负载均衡服务
        /// </summary>
        /// <param name="isVerify">自定义TCP调用服务端验证</param>
        /// <returns>是否成功</returns>
        public bool StartLoadBalancingServer(Func<subArray<byte>, bool> isVerify = null)
        {
            attribute.IsLoadBalancing = false;
            checkTicks = new TimeSpan(0, 0, Math.Max(attribute.LoadBalancingCheckSeconds + 2, 2)).Ticks;
            Monitor.Enter(serverLock);
            try
            {
                if (server == null)
                {
                    server = new commandServer(this, isVerify);
                    if (server.Start()) return true;
                    pub.Dispose(ref server);
                }
            }
            finally { Monitor.Exit(serverLock); }
            return false;
        }
        /// <summary>
        /// 获取一个TCP调用客户端
        /// </summary>
        /// <returns>TCP调用服务器信息</returns>
        protected clientIdentity _getClient_()
        {
            if (isDisposed == 0)
            {
                interlocked.NoCheckCompareSetSleep0(ref clientLock);
                int count = currentCount - freeIndexs.Count;
                if (count != 0)
                {
                    int callCount = this.callCount / count + 1, index = currentIndex;
                    do
                    {
                        if (clients[currentIndex].TryCount(callCount))
                        {
                            ++this.callCount;
                            clientIdentity value = clients[currentIndex].Client;
                            clientLock = 0;
                            return value;
                        }
                    }
                    while (++currentIndex != currentCount);
                    for (currentIndex = 0; currentIndex != index; ++currentIndex)
                    {
                        if (clients[currentIndex].TryCount(callCount))
                        {
                            ++this.callCount;
                            clientIdentity value = clients[currentIndex].Client;
                            clientLock = 0;
                            return value;
                        }
                    }
                }
                clientLock = 0;
            }
            return default(clientIdentity);
        }
        /// <summary>
        /// TCP调用客户端调用结束
        /// </summary>
        /// <param name="client">TCP调用服务器信息</param>
        /// <param name="isReturn">是否回调成功</param>
        protected void _end_(ref clientIdentity client, bool isReturn)
        {
            if (isDisposed == 0)
            {
                if (isReturn)
                {
                    interlocked.NoCheckCompareSetSleep0(ref clientLock);
                    callCount -= clients[client.Index].End(client.Identity);
                    clientLock = 0;
                }
                else
                {
                    interlocked.NoCheckCompareSetSleep0(ref clientLock);
                    keyValue<clientType, keyValue<host, int>> errorClient = clients[client.Index].Error(client.Identity);
                    if (errorClient.Key == null) clientLock = 0;
                    else
                    {
                        callCount -= errorClient.Value.Value;
                        try
                        {
                            freeIndexs.Add(client.Index);
                        }
                        finally
                        {
                            clientLock = 0;
                            pub.Dispose(ref errorClient.Key);

                            host host = errorClient.Value.Key;
                            bool isCreate = newServer(host);
                            if (isCreate)
                            {
                                tryCheck();
                                log.Default.Add("恢复TCP调用服务端[调用错误] " + host.Host + ":" + host.Port.toString(), false, false);
                            }
                            else log.Default.Add("移除TCP调用服务端[调用错误] " + host.Host + ":" + host.Port.toString(), false, false);
                        }
                    }
                }
            }
            client.Identity = int.MinValue;
            client.Client = null;
        }
        /// <summary>
        /// 创建TCP调用客户端
        /// </summary>
        /// <param name="client">TCP调用服务器端配置信息</param>
        /// <returns>TCP调用客户端</returns>
        protected abstract clientType _createClient_(fastCSharp.code.cSharp.tcpServer attribute);
        /// <summary>
        /// 获取负载均衡联通最后检测时间
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>负载均衡联通最后检测时间</returns>
        protected abstract DateTime _loadBalancingCheckTime_(clientType client);
        /// <summary>
        /// 负载均衡超时检测
        /// </summary>
        /// <param name="client">TCP调用客户端</param>
        /// <returns>TCP调用客户端是否可用</returns>
        protected abstract bool _loadBalancingCheck_(clientType client);
        /// <summary>
        /// 添加TCP调用服务端
        /// </summary>
        /// <param name="server">TCP调用服务端信息</param>
        private void newServer(serverInfo server)
        {
            bool isCreate = newServer(server.Host);
            if (isCreate)
            {
                tryCheck();
                log.Default.Add("添加TCP调用服务端 " + server.Host.Host + ":" + server.Host.Port.toString(), false, false);
            }
            server.Socket.SendStream(server.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = true, Value = isCreate });
        }
        /// <summary>
        /// 添加TCP调用服务端
        /// </summary>
        /// <param name="host">TCP服务端口信息</param>
        /// <param name="isCheckTask">是否启动检测任务</param>
        /// <returns>是否添加成功</returns>
        private bool newServer(host host)
        {
            if (isDisposed == 0)
            {
                try
                {
                    fastCSharp.code.cSharp.tcpServer attribute = fastCSharp.emit.memberCopyer<fastCSharp.code.cSharp.tcpServer>.MemberwiseClone(this.attribute);
                    attribute.IsLoadBalancing = true;
                    attribute.Host = host.Host;
                    attribute.Port = host.Port;
                    clientType client = _createClient_(attribute);
                    if (client != null)
                    {
                        interlocked.NoCheckCompareSetSleep0(ref clientLock);
                        for (int index = 0; index != currentCount; ++index)
                        {
                            keyValue<clientType, int> removeClient = this.clients[index].ReSet(host, client);
                            if (removeClient.Key != null)
                            {
                                callCount -= removeClient.Value;
                                clientLock = 0;
                                pub.Dispose(ref removeClient.Key);
                                return true;
                            }
                        }
                        if (freeIndexs.Count == 0)
                        {
                            if (currentCount == this.clients.Length)
                            {
                                try
                                {
                                    clientHost[] clients = new clientHost[currentCount << 1];
                                    this.clients.CopyTo(clients, 0);
                                    clients[currentCount].Set(host, client, currentCount);
                                    this.clients = clients;
                                    ++currentCount;
                                    client = null;
                                }
                                finally
                                {
                                    clientLock = 0;
                                    pub.Dispose(ref client);
                                }
                            }
                            else
                            {
                                clients[currentCount].Set(host, client, currentCount);
                                ++currentCount;
                                clientLock = 0;
                            }
                        }
                        else
                        {
                            clients[freeIndexs.Unsafer.Pop()].Set(host, client);
                            clientLock = 0;
                        }
                        return true;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
            }
            return false;
        }
        /// <summary>
        /// 移除TCP调用服务端
        /// </summary>
        /// <param name="host">TCP调用服务端端口信息</param>
        /// <returns>是否移除成功</returns>
        private bool removeServer(host host)
        {
            if (isDisposed == 0)
            {
                interlocked.NoCheckCompareSetSleep0(ref clientLock);
                for (int index = 0; index != currentCount; ++index)
                {
                    keyValue<clientType, int> removeClient = this.clients[index].Remove(host);
                    if (removeClient.Key != null)
                    {
                        callCount -= removeClient.Value;
                        try
                        {
                            freeIndexs.Add(index);
                        }
                        finally
                        {
                            clientLock = 0;
                            pub.Dispose(ref removeClient.Key);
                        }
                        log.Default.Add("移除TCP调用服务端 " + host.Host + ":" + host.Port.toString(), false, false);
                        return true;
                    }
                }
                clientLock = 0;
            }
            return false;
        }
        /// <summary>
        /// 添加检测任务
        /// </summary>
        private void tryCheck()
        {
            interlocked.NoCheckCompareSetSleep0(ref clientLock);
            if (isCheckTask == 0)
            {
                isCheckTask = 1;
                clientLock = 0;
                addCheck();
            }
            else clientLock = 0;
        }
        /// <summary>
        /// 添加检测任务
        /// </summary>
        private void addCheck()
        {
            if (checkTime < date.NowSecond) checkTime = date.NowSecond;
            fastCSharp.threading.timerTask.Default.Add(checkHandle, checkTime = checkTime.AddSeconds(1), null);
        }
        /// <summary>
        /// 检测任务
        /// </summary>
        private void check()
        {
            if (isDisposed == 0)
            {
                int count = 0;
                DateTime now = date.NowSecond.AddTicks(-checkTicks);
                interlocked.NoCheckCompareSetSleep0(ref clientLock);
                try
                {
                    if (removeClients.Length < currentCount) removeClients = new keyValue<clientType, int>[clients.Length];
                    for (int index = 0; index != currentCount; ++index)
                    {
                        clientType client = clients[index].CheckTimeout(now);
                        if (client != null && _loadBalancingCheckTime_(client) < now) removeClients[count++].Set(client, index);
                    }
                }
                finally { clientLock = 0; }
                while (count != 0)
                {
                    bool isClient = false;
                    clientType client = removeClients[--count].Key;
                    try
                    {
                        isClient = _loadBalancingCheck_(client);
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    if (!isClient)
                    {
                        int index = removeClients[count].Value;
                        interlocked.NoCheckCompareSetSleep0(ref clientLock);
                        keyValue<host, int> host = clients[index].Remove(client);
                        if (host.Key.Host == null) clientLock = 0;
                        else
                        {
                            callCount -= host.Value;
                            try
                            {
                                freeIndexs.Add(index);
                            }
                            finally { clientLock = 0; }
                            pub.Dispose(ref client);

                            if (newServer(host.Key))
                            {
                                log.Default.Add("恢复TCP调用服务端[检测超时] " + host.Key.Host + ":" + host.Key.Port.toString(), false, false);
                            }
                            else log.Default.Add("移除TCP调用服务端[检测超时] " + host.Key.Host + ":" + host.Key.Port.toString(), false, false);
                        }
                    }
                }
                interlocked.NoCheckCompareSetSleep0(ref clientLock);
                if (currentCount == freeIndexs.Count)
                {
                    isCheckTask = 0;
                    clientLock = 0;
                }
                else
                {
                    clientLock = 0;
                    addCheck();
                }
            }
        }
    }
}
