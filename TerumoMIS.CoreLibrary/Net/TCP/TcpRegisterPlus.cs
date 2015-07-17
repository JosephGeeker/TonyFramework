//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TcpRegisterPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP
//	File Name:  TcpRegisterPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:25:03
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Code.Csharper;

namespace TerumoMIS.CoreLibrary.Net.TCP
{
    /// <summary>
    /// TCP注册服务
    /// </summary>
    [TcpServerPlus(Service = "TcpRegister", IsIdentityCommand = true, VerifyMethodType = typeof(VerifyMethodPlus))]
    public partial class TcpRegisterPlus
    {
        /// <summary>
        /// TCP注册服务 客户端
        /// </summary>
        public sealed class client : IDisposable
        {
            /// <summary>
            /// TCP注册服务名称
            /// </summary>
            private string serviceName;
            /// <summary>
            /// TCP服务配置信息
            /// </summary>
            private fastCSharp.code.cSharp.tcpServer attribute;
            /// <summary>
            /// TCP服务端标识
            /// </summary>
            private clientId clientId;
            /// <summary>
            /// TCP注册服务客户端
            /// </summary>
            private tcpClient.tcpRegister registerClient;
            /// <summary>
            /// TCP服务信息
            /// </summary>
            private Dictionary<hashString, services> services = dictionary.CreateHashString<services>();
            /// <summary>
            /// TCP服务信息访问锁
            /// </summary>
            private int servicesLock;
            /// <summary>
            /// TCP注册服务访问锁
            /// </summary>
            private readonly object clientLock = new object();
            ///// <summary>
            ///// 创建TCP注册服务客户端失败是否输出日志
            ///// </summary>
            //private bool isNewClientErrorLog;
            /// <summary>
            /// 启动TCP注册服务客户端
            /// </summary>
            private Action startHandle;
            /// <summary>
            /// 客户端轮询
            /// </summary>
            private Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<pollResult>> pollHandle;
            /// <summary>
            /// 客户端保持回调
            /// </summary>
            private CommandLoadBalancingServerPlus.commandClient.streamCommandSocket.keepCallback pollKeep;
            /// <summary>
            /// TCP注册服务客户端
            /// </summary>
            /// <param name="serviceName">TCP注册服务服务名称</param>
            public client(string serviceName)
            {
                attribute = fastCSharp.config.pub.LoadConfig(new fastCSharp.code.cSharp.tcpServer(), serviceName);
                attribute.IsIdentityCommand = true;
                attribute.TcpRegister = null;
                registerClient = new tcpClient.tcpRegister(attribute, null);
                this.serviceName = serviceName;
                //isNewClientErrorLog = true;
                startHandle = start;
                pollHandle = poll;
                start();
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                Monitor.Enter(clientLock);
                try
                {
                    if (registerClient != null) close();
                    pub.Dispose(ref registerClient);
                }
                finally { Monitor.Exit(clientLock); }
                interlocked.NoCheckCompareSetSleep0(ref servicesLock);
                foreach (services services in this.services.Values) services.Clients = null;
                servicesLock = 0;
            }
            /// <summary>
            /// 关闭客户端
            /// </summary>
            private void close()
            {
                if (clientId.Tick != 0)
                {
                    try
                    {
                        registerClient.removeRegister(clientId);
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    clientId.Tick = 0;
                }
                pub.Dispose(ref pollKeep);
            }
            /// <summary>
            /// 启动TCP注册服务客户端
            /// </summary>
            private void start()
            {
                bool isStart = false;
                Monitor.Enter(clientLock);
                try
                {
                    if (registerClient != null)
                    {
                        close();
                        if ((clientId = registerClient.register().Value).Tick != 0)
                        {
                            services[] services = registerClient.getServices(out clientId.Version).Value;
                            if (services != null)
                            {
                                newServices(services);
                                if ((pollKeep = registerClient.poll(clientId, pollHandle)) != null) isStart = true;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                finally { Monitor.Exit(clientLock); }
                log.Default.Add("TCP注册客户端启动 " + (isStart ? "成功" : "失败"), false, false);
                if (!isStart && registerClient != null)
                {
                    fastCSharp.threading.timerTask.Default.Add(startHandle, fastCSharp.date.NowSecond.AddSeconds(2), null);
                }
            }
            /// <summary>
            /// TCP服务信息集合更新
            /// </summary>
            /// <param name="services">TCP服务信息集合</param>
            private void newServices(services[] services)
            {
                services cacheServices;
                interlocked.NoCheckCompareSetSleep0(ref servicesLock);
                try
                {
                    foreach (services service in services)
                    {
                        hashString name = service.Name;
                        if (this.services.TryGetValue(name, out cacheServices)) cacheServices.Copy(service);
                        else
                        {
                            service.SetClient();
                            this.services.Add(name, service);
                        }
                    }
                }
                finally { servicesLock = 0; }
            }
            /// <summary>
            /// 客户端轮询
            /// </summary>
            /// <param name="result">轮询结果</param>
            private void poll(fastCSharp.code.cSharp.asynchronousMethod.returnValue<pollResult> result)
            {
                if (result.IsReturn)
                {
                    switch (result.Value.State)
                    {
                        case pollState.RegisterChange:
                            Monitor.Enter(clientLock);
                            try
                            {
                                if (clientId.Version < result.Value.Version)
                                {
                                    clientId.Version = result.Value.Version;
                                    newServices(result.Value.Services);
                                }
                            }
                            finally { Monitor.Exit(clientLock); }
                            break;
                        case pollState.VersionError:
                            services[] services = null;
                            Monitor.Enter(clientLock);
                            try
                            {
                                services = registerClient.getServices(out clientId.Version).Value;
                            }
                            finally
                            {
                                Monitor.Exit(clientLock);
                                if (services != null) newServices(services);
                            }
                            break;
                        case pollState.ClientError:
                            Monitor.Enter(clientLock);
                            try
                            {
                                close();
                            }
                            finally { Monitor.Exit(clientLock); }
                            break;
                        case pollState.NewClient:
                            log.Default.Add(serviceName + " 轮询客户端冲突", false, false);
                            break;
                        default:
                            if (result.Value.State != pollState.Check)
                            {
                                log.Error.Add("不可识别的轮询状态 " + result.Value.State.ToString(), false, false);
                            }
                            break;
                    }
                }
                else start();
            }
            /// <summary>
            /// 注册TCP服务端
            /// </summary>
            /// <param name="attribute">TCP服务配置</param>
            /// <returns>是否注册成功</returns>
            public registerState Register(fastCSharp.code.cSharp.tcpServer attribute)
            {
                registerResult result = new registerResult { State = registerState.NoClient };
                Monitor.Enter(clientLock);
                try
                {
                    result = registerClient.register(clientId, new service { Host = new host { Host = attribute.Host, Port = attribute.Port }, Name = attribute.ServiceName, IsSingle = attribute.IsSingleRegister, IsPerp = attribute.IsPerpleRegister }).Value;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                finally { Monitor.Exit(clientLock); }
                if (result.State == registerState.Success)
                {
                    attribute.Host = result.Service.Host.Host;
                    attribute.Port = result.Service.Host.Port;
                }
                return result.State;
            }
            /// <summary>
            /// 删除注册TCP服务端
            /// </summary>
            /// <param name="attribute">TCP服务配置</param>
            public void RemoveRegister(fastCSharp.code.cSharp.tcpServer attribute)
            {
                Monitor.Enter(clientLock);
                try
                {
                    registerClient.removeRegister(clientId, attribute.ServiceName);
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                finally { Monitor.Exit(clientLock); }
            }
            /// <summary>
            /// 绑定TCP调用客户端
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            public void Register(CommandLoadBalancingServerPlus.commandClient commandClient)
            {
                hashString name = commandClient.ServiceName;
                interlocked.NoCheckCompareSetSleep0(ref servicesLock);
                try
                {
                    if (!this.services.TryGetValue(name, out commandClient.TcpRegisterServices))
                    {
                        services.Add(name, commandClient.TcpRegisterServices = new services { Hosts = nullValue<host>.Array });
                    }
                    commandClient.TcpRegisterServices.AddClient(commandClient);
                }
                finally { servicesLock = 0; }
            }
            /// <summary>
            /// 删除TCP调用客户端
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            internal void Remove(CommandLoadBalancingServerPlus.commandClient commandClient)
            {
                services services = commandClient.TcpRegisterServices;
                if (services != null)
                {
                    commandClient.TcpRegisterServices = null;
                    interlocked.NoCheckCompareSetSleep0(ref servicesLock);
                    services.RemoveClient(commandClient);
                    servicesLock = 0;
                }
            }
            /// <summary>
            /// 获取TCP服务端口信息
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            /// <returns>TCP服务端口信息是否更新</returns>
            internal bool GetHost(CommandLoadBalancingServerPlus.commandClient commandClient)
            {
                services services = commandClient.TcpRegisterServices;
                if (services == null)
                {
                    fastCSharp.code.cSharp.tcpServer attribute = commandClient.Attribute;
                    attribute.Port = 0;
                    return true;
                }
                interlocked.NoCheckCompareSetSleep0(ref servicesLock);
                bool isHost = services.GetHost(commandClient);
                servicesLock = 0;
                return isHost;
            }
            /// <summary>
            /// TCP注册服务客户端缓存
            /// </summary>
            private static Dictionary<hashString, client> clients = dictionary.CreateHashString<client>();
            /// <summary>
            /// TCP注册服务客户端 访问锁
            /// </summary>
            private static readonly object clientsLock = new object();
            /// <summary>
            /// 关闭TCP注册服务客户端
            /// </summary>
            private static void disposeClients()
            {
                client[] clientArray = null;
                Monitor.Enter(clientsLock);
                try
                {
                    clientArray = clients.Values.getArray();
                    clients = null;
                }
                finally { Monitor.Exit(clientsLock); }
                foreach (client client in clientArray) client.Dispose();
            }
            /// <summary>
            /// 获取TCP注册服务客户端
            /// </summary>
            /// <param name="serviceName">服务名称</param>
            /// <returns>TCP注册服务客户端,失败返回null</returns>
            public static client Get(string serviceName)
            {
                if (serviceName.length() != 0)
                {
                    int count = int.MinValue;
                    client client = null;
                    hashString nameKey = serviceName;
                    Monitor.Enter(clientsLock);
                    try
                    {
                        if (clients != null && !clients.TryGetValue(nameKey, out client))
                        {
                            try
                            {
                                client = new client(serviceName);
                            }
                            catch (Exception error)
                            {
                                log.Error.Add(error, null, false);
                            }
                            if (client != null)
                            {
                                count = clients.Count;
                                clients.Add(nameKey, client);
                            }
                        }
                    }
                    finally { Monitor.Exit(clientsLock); }
                    if (count == 0) fastCSharp.domainUnload.Add(disposeClients);
                    return client;
                }
                return null;
            }
        }
        /// <summary>
        /// TCP服务端标识
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct clientId : IEquatable<clientId>
        {
            /// <summary>
            /// TCP注册服务进程时间
            /// </summary>
            public long Tick;
            /// <summary>
            /// 进程级唯一编号
            /// </summary>
            public int Identity;
            /// <summary>
            /// 注册信息版本
            /// </summary>
            public int Version;
            /// <summary>
            /// 获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return (int)(Tick >> 32) ^ (int)Tick ^ Identity;
            }
            /// <summary>
            /// 判断是否同一TCP服务端
            /// </summary>
            /// <param name="other">TCP服务端</param>
            /// <returns>是否同一TCP服务端</returns>
            public override bool Equals(object other)
            {
                return Equals((clientId)other);
            }
            /// <summary>
            /// 判断是否同一TCP服务端
            /// </summary>
            /// <param name="other">TCP服务端</param>
            /// <returns>是否同一TCP服务端</returns>
            public bool Equals(clientId other)
            {
                return Tick == other.Tick && Identity == other.Identity;
            }
        }
        /// <summary>
        /// TCP服务信息
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct service
        {
            /// <summary>
            /// TCP服务名称标识
            /// </summary>
            public string Name;
            /// <summary>
            /// 端口信息集合
            /// </summary>
            public host Host;
            /// <summary>
            /// 是否只允许一个TCP服务实例
            /// </summary>
            public bool IsSingle;
            /// <summary>
            /// 是否预申请服务
            /// </summary>
            public bool IsPerp;
        }
        /// <summary>
        /// TCP服务信息集合
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public sealed class services
        {
            /// <summary>
            /// 最后一次未找到注册的服务名称
            /// </summary>
            private static string errorServiceName;
            /// <summary>
            /// TCP服务名称标识
            /// </summary>
            public string Name;
            /// <summary>
            /// 端口信息集合
            /// </summary>
            public host[] Hosts;
            /// <summary>
            /// 是否只允许一个TCP服务实例
            /// </summary>
            public bool IsSingle;
            /// <summary>
            /// 端口信息更新版本
            /// </summary>
            [fastCSharp.code.ignore]
            internal int Version;
            /// <summary>
            /// TCP调用客户端集合
            /// </summary>
            [fastCSharp.code.ignore]
            internal list<CommandLoadBalancingServerPlus.commandClient> Clients;
            /// <summary>
            /// 当前端口信息位置
            /// </summary>
            [fastCSharp.code.ignore]
            private int hostIndex;
            /// <summary>
            /// 获取TCP服务端口信息
            /// </summary>
            /// <param name="commandClient">TCP调用客户端</param>
            /// <returns>是否更新</returns>
            internal bool GetHost(CommandLoadBalancingServerPlus.commandClient commandClient)
            {
                fastCSharp.code.cSharp.tcpServer attribute = commandClient.Attribute;
                if (attribute != null)
                {
                    commandClient.TcpRegisterServicesVersion = Version;
                    if (Hosts.Length == 0)
                    {
                        attribute.Port = 0;
                        if (errorServiceName != attribute.ServiceName) log.Error.Add(attribute.ServiceName + " 未找到注册服务信息", false, false);
                        errorServiceName = attribute.ServiceName;
                    }
                    else
                    {
                        if (errorServiceName == attribute.ServiceName) errorServiceName = null;
                        host host;
                        int index = hostIndex;
                        if (index < Hosts.Length)
                        {
                            ++hostIndex;
                            host = Hosts[index];
                        }
                        else
                        {
                            hostIndex = 1;
                            host = Hosts[0];
                        }
                        if (attribute.Host != host.Host || attribute.Port != host.Port)
                        {
                            attribute.Host = host.Host;
                            attribute.Port = host.Port;
                        }
                    }
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 添加TCP调用客户端
            /// </summary>
            /// <param name="client">TCP调用客户端</param>
            internal void AddClient(CommandLoadBalancingServerPlus.commandClient client)
            {
                if (Clients == null) Clients = new list<CommandLoadBalancingServerPlus.commandClient>(sizeof(int));
                Clients.Add(client);
            }
            /// <summary>
            /// 删除TCP调用客户端
            /// </summary>
            /// <param name="removeClient">TCP调用客户端</param>
            internal void RemoveClient(CommandLoadBalancingServerPlus.commandClient removeClient)
            {
                if (Clients != null && Clients.Count != 0)
                {
                    int count = Clients.Count;
                    CommandLoadBalancingServerPlus.commandClient[] clientArray = Clients.array;
                    foreach (CommandLoadBalancingServerPlus.commandClient client in clientArray)
                    {
                        if (client == removeClient)
                        {
                            count = Clients.Count - count;
                            Clients.Unsafer.AddLength(-1);
                            clientArray[count] = clientArray[Clients.Count];
                            clientArray[Clients.Count] = null;
                            return;
                        }
                        if (--count == 0) break;
                    }
                }
            }
            /// <summary>
            /// 复制TCP服务信息
            /// </summary>
            /// <param name="services">TCP服务信息集合</param>
            internal void Copy(services services)
            {
                Hosts = services.Hosts ?? nullValue<host>.Array;
                IsSingle = services.IsSingle;
                hostIndex = 0;
                ++Version;
            }
            /// <summary>
            /// 客户端初始化设置
            /// </summary>
            internal void SetClient()
            {
                if (Hosts == null) Hosts = nullValue<host>.Array;
                Version = 1;
            }
            /// <summary>
            /// 默认空TCP服务信息集合
            /// </summary>
            internal static readonly services Null = new services();
        }
        /// <summary>
        /// 注册状态
        /// </summary>
        public enum registerState
        {
            /// <summary>
            /// 客户端不可用
            /// </summary>
            NoClient,
            /// <summary>
            /// 客户端标识错误
            /// </summary>
            ClientError,
            /// <summary>
            /// 单例服务冲突
            /// </summary>
            SingleError,
            /// <summary>
            /// TCP服务端口信息不合法
            /// </summary>
            HostError,
            /// <summary>
            /// TCP服务端口信息已存在
            /// </summary>
            HostExists,
            /// <summary>
            /// 没有可用的端口号
            /// </summary>
            PortError,
            /// <summary>
            /// TCP服务信息检测被更新,需要重试
            /// </summary>
            ServiceChange,
            /// <summary>
            /// 注册成功
            /// </summary>
            Success,
        }
        /// <summary>
        /// 注册结果
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct registerResult
        {
            /// <summary>
            /// 注册状态
            /// </summary>
            public registerState State;
            /// <summary>
            /// 注册成功的TCP服务信息
            /// </summary>
            public service Service;
        }
        /// <summary>
        /// 轮询状态
        /// </summary>
        public enum pollState
        {
            /// <summary>
            /// 客户端标识错误
            /// </summary>
            ClientError,
            /// <summary>
            /// 检测是否在线
            /// </summary>
            Check,
            /// <summary>
            /// TCP服务信息版本号不匹配
            /// </summary>
            VersionError,
            /// <summary>
            /// 轮询TCP客户端冲突
            /// </summary>
            NewClient,
            /// <summary>
            /// TCP服务端注册更新
            /// </summary>
            RegisterChange,
        }
        /// <summary>
        /// 轮询结果
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct pollResult
        {
            /// <summary>
            /// TCP服务端注册信息
            /// </summary>
            public services[] Services;
            /// <summary>
            /// TCP服务端注册版本号
            /// </summary>
            public int Version;
            /// <summary>
            /// 轮询状态
            /// </summary>
            public pollState State;
            /// <summary>
            /// 检测是否在线
            /// </summary>
            internal static readonly pollResult Check = new pollResult { State = pollState.Check };
        }
        /// <summary>
        /// 缓存信息
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        private struct cache
        {
            /// <summary>
            /// TCP服务信息集合
            /// </summary>
            public keyValue<string, services>[] ServiceCache;
            /// <summary>
            /// TCP服务端标识信息集合
            /// </summary>
            public Dictionary<host, clientId> HostClients;
            /// <summary>
            /// TCP服务端口信息集合
            /// </summary>
            public keyValue<string, int>[] HostPorts;
            /// <summary>
            /// 轮询TCP服务端集合
            /// </summary>
            public clientId[] clients;
        }
        /// <summary>
        /// 客户端状态
        /// </summary>
        private sealed class clientState
        {
            /// <summary>
            /// 轮询委托
            /// </summary>
            public Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<pollResult>, bool> Poll;
            /// <summary>
            /// 最后响应时间
            /// </summary>
            public DateTime Time = date.NowSecond;
            /// <summary>
            /// 判断客户端是否有效
            /// </summary>
            public bool IsClient
            {
                get
                {
                    return Poll == null ? Time.AddSeconds(fastCSharp.config.tcpRegister.Default.RegisterTimeoutSeconds) >= date.NowSecond : Poll(pollResult.Check);
                }
            }
        }
        /// <summary>
        /// TCP服务信息集合
        /// </summary>
        private Dictionary<hashString, services> serviceCache;
        /// <summary>
        /// 预申请服务集合
        /// </summary>
        private Dictionary<hashString, keyValue<clientId, service>> perpServices = dictionary.CreateHashString<keyValue<clientId, service>>();
        /// <summary>
        /// TCP服务信息 访问锁
        /// </summary>
        private readonly object serviceLock = new object();
        /// <summary>
        /// TCP服务信息 版本号
        /// </summary>
        private int serviceVersion;
        /// <summary>
        /// 轮询TCP服务端集合
        /// </summary>
        private Dictionary<clientId, clientState> clients;
        /// <summary>
        /// TCP服务端信息集合
        /// </summary>
        private Dictionary<host, clientId> hostClients;
        /// <summary>
        /// TCP服务端口信息集合
        /// </summary>
        private Dictionary<hashString, int> hostPorts;
        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private string cacheFile;
        /// <summary>
        /// TCP服务注册通知轮询TCP服务端
        /// </summary>
        private Action<keyValue<services, int>> onRegisterHandle;
        /// <summary>
        /// TCP服务注册通知轮询TCP服务端
        /// </summary>
        private Action<keyValue<subArray<services>, int>> onRegistersHandle;
        /// <summary>
        /// 设置TCP服务端
        /// </summary>
        /// <param name="tcpServer">TCP服务端</param>
        public void SetTcpServer(fastCSharp.net.tcp.server tcpServer)
        {
            onRegisterHandle = onRegister;
            onRegistersHandle = onRegister;
            cacheFile = fastCSharp.config.pub.Default.CachePath + tcpServer.ServiceName + @".cache";
            fromCacheFile();
        }
        /// <summary>
        /// TCP服务端注册验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024)]
        protected virtual bool verify(string value)
        {
            if (fastCSharp.config.tcpRegister.Default.Verify == null && !fastCSharp.config.pub.Default.IsDebug)
            {
                log.Error.Add("TCP服务注册验证数据不能为空", false, true);
                return false;
            }
            return fastCSharp.config.tcpRegister.Default.Verify == value;
        }
        /// <summary>
        /// TCP服务端注册
        /// </summary>
        /// <returns>TCP服务端标识</returns>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        private clientId register()
        {
            clientId clientId = new clientId { Tick = CoreLibrary.PubPlus.StartTime.Ticks, Identity = CoreLibrary.PubPlus.Identity32, Version = int.MinValue };
            clientState state = new clientState();
            Monitor.Enter(serviceLock);
            try
            {
                clients.Add(clientId, state);
            }
            finally { Monitor.Exit(serviceLock); }
            return clientId;
        }
        /// <summary>
        /// 获取TCP服务信息集合
        /// </summary>
        /// <param name="version">TCP服务信息 版本号</param>
        /// <returns>TCP服务信息集合</returns>
        [fastCSharp.code.cSharp.tcpServer]
        private services[] getServices(out int version)
        {
            services[] services = null;
            Monitor.Enter(serviceLock);
            try
            {
                version = serviceVersion;
                services = serviceCache.Values.getArray();
            }
            finally { Monitor.Exit(serviceLock); }
            return services;
        }
        /// <summary>
        /// 获取客户端状态
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private clientState getClient(clientId client)
        {
            clientState state;
            Monitor.Enter(serviceLock);
            if (clients.TryGetValue(client, out state))
            {
                Monitor.Exit(serviceLock);
                state.Time = date.NowSecond;
            }
            else Monitor.Exit(serviceLock);
            return state;
        }
        /// <summary>
        /// 注册TCP服务信息
        /// </summary>
        /// <param name="client">TCP服务端标识</param>
        /// <param name="service">TCP服务信息</param>
        /// <returns>注册状态</returns>
        [fastCSharp.code.cSharp.tcpServer]
        private registerResult register(clientId client, service service)
        {
            clientState state = getClient(client);
            if (state == null) return new registerResult { State = registerState.ClientError };
            if (!service.Host.HostToIpAddress()) return new registerResult { State = registerState.HostError };
            services oldService, newService = new services { Name = service.Name };
            int version = int.MinValue, hostCount = 0;
            hashString serviceName = service.Name;
            Monitor.Enter(serviceLock);
            try
            {
                if (serviceCache.TryGetValue(serviceName, out oldService))
                {
                    if (oldService.IsSingle || service.IsSingle)
                    {
                        foreach (host host in oldService.Hosts)
                        {
                            clientId oldClient;
                            if (hostClients.TryGetValue(host, out oldClient))
                            {
                                clientState oldState;
                                if (clients.TryGetValue(oldClient, out oldState))
                                {
                                    if (oldState.IsClient) oldService.Hosts[hostCount++] = host;
                                    else fastCSharp.threading.threadPool.TinyPool.FastStart(removeRegister, oldClient, null, null);
                                }
                            }
                        }
                        if (hostCount != 0)
                        {
                            if (hostCount != oldService.Hosts.Length) Array.Resize(ref oldService.Hosts, hostCount);
                            newService = oldService;
                            version = ++serviceVersion;
                            if (service.IsPerp)
                            {
                                if (service.Host.Port == 0)
                                {
                                    service.Host.Port = getPort(client, service.Host.Host, true);
                                    if (service.Host.Port == 0) return new registerResult { State = registerState.PortError };
                                }
                                perpServices[serviceName] = new keyValue<clientId, tcpRegister.service>(client, service);
                                return new registerResult { State = registerState.Success, Service = service };
                            }
                            return new registerResult { State = registerState.SingleError };
                        }
                        oldService.Hosts = nullValue<host>.Array;
                    }
                    if (service.Host.Port == 0)
                    {
                        service.Host.Port = getPort(client, service.Host.Host, false);
                        if (service.Host.Port == 0) return new registerResult { State = registerState.PortError };
                    }
                    else
                    {
                        if (hostClients.ContainsKey(service.Host)) return new registerResult { State = registerState.HostExists };
                        hostClients.Add(service.Host, client);
                    }
                    newService.Hosts = new host[oldService.Hosts.Length + 1];
                    Array.Copy(oldService.Hosts, 0, newService.Hosts, 1, oldService.Hosts.Length);
                    newService.Hosts[0] = service.Host;
                    serviceCache[serviceName] = newService;
                    version = ++serviceVersion;
                }
                else
                {
                    if (service.Host.Port == 0)
                    {
                        service.Host.Port = getPort(client, service.Host.Host, false);
                        if (service.Host.Port == 0) return new registerResult { State = registerState.PortError };
                    }
                    else
                    {
                        if (hostClients.ContainsKey(service.Host)) return new registerResult { State = registerState.HostExists };
                        hostClients.Add(service.Host, client);
                    }
                    newService.Hosts = new host[] { service.Host };
                    newService.IsSingle = service.IsSingle;
                    serviceCache.Add(serviceName, newService);
                    version = ++serviceVersion;
                }
            }
            finally
            {
                Monitor.Exit(serviceLock);
                if (version != int.MinValue) fastCSharp.threading.queue.Tiny.Add(onRegisterHandle, new keyValue<services, int>(newService, version), null);
            }
            return new registerResult { State = registerState.Success, Service = service };
        }
        /// <summary>
        /// 获取TCP服务端口号
        /// </summary>
        /// <param name="client">TCP服务端标识</param>
        /// <param name="host">主机IP地址</param>
        /// <returns>TCP服务端口号</returns>
        private int getPort(clientId client, string ipAddress, bool isPerp)
        {
            host host = new host { Host = ipAddress };
            hashString ipKey = ipAddress;
            if (!hostPorts.TryGetValue(ipKey, out host.Port)) host.Port = fastCSharp.config.tcpRegister.Default.PortStart;
            int startPort = host.Port;
            while (hostClients.ContainsKey(host)) ++host.Port;
            if (host.Port >= 65536)
            {
                host.Port = fastCSharp.config.tcpRegister.Default.PortStart;
                while (host.Port != startPort && hostClients.ContainsKey(host)) ++host.Port;
                if (host.Port == startPort) return 0;
            }
            hostPorts[ipKey] = host.Port + 1;
            if (!isPerp) hostClients.Add(host, client);
            return host.Port;
        }
        /// <summary>
        /// TCP服务注册通知轮询TCP服务端
        /// </summary>
        /// <param name="serviceVersion">TCP服务信息</param>
        private void onRegister(keyValue<services, int> serviceVersion)
        {
            callPoll(new pollResult { State = pollState.RegisterChange, Services = new services[] { serviceVersion.Key }, Version = serviceVersion.Value });
        }
        /// <summary>
        /// TCP服务注册通知轮询TCP服务端
        /// </summary>
        /// <param name="result">TCP服务信息</param>
        /// <param name="isCheckTime">是否检测轮询时间</param>
        private void callPoll(pollResult result)
        {
            KeyValuePair<clientId, clientState>[] clients = null;
            Monitor.Enter(serviceLock);
            try
            {
                clients = this.clients.getArray();
            }
            finally { Monitor.Exit(serviceLock); }
            if (clients.Length != 0)
            {
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<pollResult> returnValue = new fastCSharp.code.cSharp.asynchronousMethod.returnValue<pollResult>
                {
                    IsReturn = true,
                    Value = result
                };
                foreach (KeyValuePair<clientId, clientState> client in clients)
                {
                    try
                    {
                        if (!(client.Value.Poll == null ? client.Value.IsClient : client.Value.Poll(returnValue))) removeRegister(client.Key);
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                }
            }
            if (result.State == pollState.RegisterChange) fastCSharp.threading.task.Tiny.Add(saveCacheFile);
        }
        /// <summary>
        /// TCP服务端轮询
        /// </summary>
        /// <param name="client">TCP服务端标识</param>
        /// <param name="onRegisterChanged">TCP服务注册通知委托</param>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousCallback = true, IsClientAsynchronous = true, IsClientSynchronous = false, IsKeepCallback = true)]
        private void poll(clientId client, Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<pollResult>, bool> onRegisterChanged)
        {
            clientState state = getClient(client);
            if (state == null) onRegisterChanged(new pollResult { State = pollState.ClientError });
            else
            {
                pollResult value = new pollResult();
                Monitor.Enter(serviceLock);
                if (client.Version != serviceVersion) value.State = pollState.VersionError;
                else
                {
                    Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<pollResult>, bool> poll = state.Poll;
                    state.Poll = onRegisterChanged;
                    onRegisterChanged = poll;
                    value.State = pollState.NewClient;
                }
                Monitor.Exit(serviceLock);
                if (onRegisterChanged != null) onRegisterChanged(value);
            }
        }
        /// <summary>
        /// 注销TCP服务信息
        /// </summary>
        /// <param name="client">TCP服务端标识</param>
        /// <param name="serviceName">TCP服务名称</param>
        [fastCSharp.code.cSharp.tcpServer]
        private void removeRegister(clientId client, string serviceName)
        {
            clientState state = getClient(client);
            if (state != null)
            {
                services services;
                keyValue<clientId, service> service = default(keyValue<clientId, service>);
                hashString nameKey = serviceName;
                int version = int.MinValue;
                bool isPerp = false;
                Monitor.Enter(serviceLock);
                try
                {
                    if (serviceCache.TryGetValue(nameKey, out services))
                    {
                        if (perpServices.TryGetValue(nameKey, out service))
                        {
                            perpServices.Remove(nameKey);
                            isPerp = true;
                        }
                        host[] hosts = removeRegister(client, services);
                        if (hosts != services.Hosts)
                        {
                            if (isPerp) version = 0;
                            else version = ++serviceVersion;
                            services.Hosts = hosts;
                        }
                    }
                }
                finally { Monitor.Exit(serviceLock); }
                if (isPerp)
                {
                    service.Value.IsPerp = false;
                    try
                    {
                        if (register(service.Key, service.Value).State == registerState.Success) isPerp = false;
                    }
                    finally
                    {
                        if (isPerp && version == 0)
                        {
                            Monitor.Enter(serviceLock);
                            version = ++serviceVersion;
                            Monitor.Exit(serviceLock);
                            fastCSharp.threading.queue.Tiny.Add(onRegisterHandle, new keyValue<services, int>(services, version), null);
                        }
                    }
                }
                else if (version != int.MinValue) fastCSharp.threading.queue.Tiny.Add(onRegisterHandle, new keyValue<services, int>(services, version), null);
            }
        }
        /// <summary>
        /// 注销TCP服务信息
        /// </summary>
        /// <param name="client">TCP服务端标识</param>
        /// <param name="serviceName">TCP服务信息</param>
        /// <returns>注销操作之后的TCP服务端口信息集合</returns>
        private unsafe host[] removeRegister(clientId client, services service)
        {
            int count = (service.Hosts.Length + 7) >> 3, index = 0;
            byte* isRemove = stackalloc byte[count];
            fixedMap removeMap = new fixedMap(isRemove, count);
            count = 0;
            foreach (host host in service.Hosts)
            {
                if (client.Equals(hostClients[host])) removeMap.Set(index);
                else ++count;
                ++index;
            }
            if (count != service.Hosts.Length)
            {
                hashString serviceName = service.Name;
                if (count == 0)
                {
                    serviceCache.Remove(serviceName);
                    foreach (host host in service.Hosts) hostClients.Remove(host);
                    return null;
                }
                host[] hosts = new host[count];
                count = index = 0;
                foreach (host host in service.Hosts)
                {
                    if (removeMap.Get(index++)) hostClients.Remove(host);
                    else hosts[count++] = host;
                }
                service.Hosts = hosts;
                serviceCache[serviceName] = service;
            }
            return service.Hosts;
        }
        /// <summary>
        /// 注销TCP服务信息
        /// </summary>
        /// <param name="client">TCP服务端标识</param>
        [fastCSharp.code.cSharp.tcpServer]
        private void removeRegister(clientId client)
        {
            clientState state = getClient(client);
            if (state != null)
            {
                subArray<services> removeServices;
                subArray<keyValue<clientId, service>> removePerpServices = new subArray<keyValue<clientId, service>>();
                keyValue<clientId, service> perpService;
                int version = int.MinValue;
                Monitor.Enter(serviceLock);
                try
                {
                    removeServices = new subArray<services>(serviceCache.Count);
                    foreach (services service in serviceCache.Values.getArray())
                    {
                        host[] hosts = removeRegister(client, service);
                        if (hosts != service.Hosts)
                        {
                            removeServices.UnsafeAdd(new services { Name = service.Name, Hosts = hosts, IsSingle = service.IsSingle });
                            if (perpServices.TryGetValue(service.Name, out perpService))
                            {
                                perpServices.Remove(service.Name);
                                removePerpServices.Add(perpService);
                            }
                        }
                    }
                    if (removeServices.Count != 0) version = ++serviceVersion;
                    clients.Remove(client);
                }
                finally { Monitor.Exit(serviceLock); }
                if (version != int.MinValue) fastCSharp.threading.task.Tiny.Add(onRegistersHandle, new keyValue<subArray<services>, int>(removeServices, version), null);
                if (removePerpServices.Count != 0)
                {
                    foreach (keyValue<clientId, service> removePerpService in removePerpServices)
                    {
                        perpService.Value = removePerpService.Value;
                        perpService.Value.IsPerp = false;
                        register(removePerpService.Key, perpService.Value);
                    }
                }
            }
        }
        /// <summary>
        /// TCP服务注册通知轮询TCP服务端
        /// </summary>
        /// <param name="serviceVersion">TCP服务信息</param>
        private void onRegister(keyValue<subArray<services>, int> serviceVersion)
        {
            callPoll(new pollResult { State = pollState.RegisterChange, Services = serviceVersion.Key.ToArray(), Version = serviceVersion.Value });
        }
        /// <summary>
        /// 保存TCP服务信息集合到缓存文件
        /// </summary>
        private unsafe void saveCacheFile()
        {
            cache cache = new cache();
            byte[] buffer = fastCSharp.memoryPool.StreamBuffers.Get();
            try
            {
                fixed (byte* bufferFixed = buffer)
                {
                    using (unmanagedStream stream = new unmanagedStream(bufferFixed, buffer.Length))
                    {
                        Monitor.Enter(serviceLock);
                        try
                        {
                            cache.ServiceCache = serviceCache.getArray(value => new keyValue<string, services>(value.Key.ToString(), value.Value));
                            cache.HostClients = hostClients;
                            cache.HostPorts = hostPorts.getArray(value => new keyValue<string, int>(value.Key.ToString(), value.Value));
                            cache.clients = clients.Keys.getArray();
                            fastCSharp.emit.dataSerializer.Serialize(cache, stream);
                            if (stream.Data == bufferFixed)
                            {
                                using (FileStream file = new FileStream(cacheFile, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    file.Write(buffer, 0, stream.Length);
                                }
                            }
                            else File.WriteAllBytes(cacheFile, stream.GetArray());
                        }
                        finally { Monitor.Exit(serviceLock); }
                    }
                }
            }
            finally { fastCSharp.memoryPool.StreamBuffers.Push(ref buffer); }
        }
        /// <summary>
        /// 从缓存文件恢复TCP服务信息集合
        /// </summary>
        private void fromCacheFile()
        {
            cache cache = new cache();
            if (File.Exists(cacheFile))
            {
                int isCache = 0;
                try
                {
                    if (fastCSharp.emit.dataDeSerializer.DeSerialize(File.ReadAllBytes(cacheFile), ref cache)) isCache = 1;
                    else cache = new cache();
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (isCache == 0)
                {
                    try
                    {
                        File.Delete(cacheFile);
                    }
                    catch (Exception error)
                    {
                        log.Default.Add(error, null, false);
                    }
                }
            }
            Monitor.Enter(serviceLock);
            try
            {
                serviceCache = dictionary.CreateHashString<services>();
                if (cache.ServiceCache != null)
                {
                    foreach (keyValue<string, services> value in cache.ServiceCache) serviceCache.Add(value.Key, value.Value);
                }
                hostClients = cache.HostClients ?? dictionary<host>.Create<clientId>();
                hostPorts = dictionary.CreateHashString<int>();
                if (cache.HostPorts != null)
                {
                    foreach (keyValue<string, int> value in cache.HostPorts) hostPorts.Add(value.Key, value.Value);
                }
                clients = dictionary<clientId>.Create<clientState>();
                if (cache.clients != null) foreach (clientId client in cache.clients) clients.Add(client, new clientState { Time = DateTime.MinValue });
            }
            finally { Monitor.Exit(serviceLock); }
        }
    }
}
