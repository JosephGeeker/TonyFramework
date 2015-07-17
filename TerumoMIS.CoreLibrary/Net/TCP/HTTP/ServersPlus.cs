//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ServersPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  ServersPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:40:19
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

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    [TcpServerPlus(Service = "httpServer", IsIdentityCommand = true, VerifyMethodType = typeof(VerifyMethodPlus))]
    public sealed class ServersPlus:IDisposable
    {
        /// <summary>
        /// HTTP服务启动状态
        /// </summary>
        public enum startState
        {
            /// <summary>
            /// 未知状态
            /// </summary>
            Unknown,
            /// <summary>
            /// HTTP服务已经关闭
            /// </summary>
            Disposed,
            /// <summary>
            /// 主机名称合法
            /// </summary>
            HostError,
            /// <summary>
            /// 域名不合法
            /// </summary>
            DomainError,
            /// <summary>
            /// 域名冲突
            /// </summary>
            DomainExists,
            /// <summary>
            /// 证书文件匹配错误
            /// </summary>
            CertificateMatchError,
            /// <summary>
            /// 证书文件错误
            /// </summary>
            CertificateError,
            /// <summary>
            /// 程序集文件未找到
            /// </summary>
            NotFoundAssembly,
            /// <summary>
            /// 服务启动失败
            /// </summary>
            StartError,
            /// <summary>
            /// TCP监听服务启动失败
            /// </summary>
            TcpError,
            /// <summary>
            /// 启动成功
            /// </summary>
            Success,
        }
        /// <summary>
        /// 保存信息
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        private sealed class saveInfo
        {
            /// <summary>
            /// 域名服务信息
            /// </summary>
            public sealed class domainServer
            {
                /// <summary>
                /// 程序集文件名,包含路径
                /// </summary>
                public string AssemblyPath;
                /// <summary>
                /// 服务程序类型名称
                /// </summary>
                public string ServerType;
                /// <summary>
                /// 域名信息集合
                /// </summary>
                public domain[] Domains;
                /// <summary>
                /// 是否共享程序集
                /// </summary>
                public bool IsShareAssembly;
            }
            /// <summary>
            /// 域名服务信息集合
            /// </summary>
            [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
            public domainServer[] Domains;
            /// <summary>
            /// 转发服务端口信息
            /// </summary>
            public host ForwardHost;
        }
        /// <summary>
        /// 域名服务信息
        /// </summary>
        internal sealed class domainServer : IDisposable
        {
            /// <summary>
            /// HTTP服务器
            /// </summary>
            public servers Servers;
            /// <summary>
            /// 程序集文件名,包含路径
            /// </summary>
            public string AssemblyPath;
            /// <summary>
            /// 服务程序类型名称
            /// </summary>
            public string ServerType;
            /// <summary>
            /// 是否共享程序集
            /// </summary>
            public bool IsShareAssembly;
            /// <summary>
            /// 域名信息集合
            /// </summary>
            public keyValue<domain, int>[] Domains;
            /// <summary>
            /// 有效域名数量
            /// </summary>
            public int DomainCount;
            /// <summary>
            /// 域名服务
            /// </summary>
            public http.domainServer Server;
            /// <summary>
            /// 文件监视路径
            /// </summary>
            public string FileWatcherPath;
            /// <summary>
            /// 是否已经启动
            /// </summary>
            public bool IsStart;
            /// <summary>
            /// 删除文件监视路径
            /// </summary>
            public void RemoveFileWatcher()
            {
                createFlieTimeoutWatcher fileWatcher = Servers.fileWatcher;
                if (fileWatcher != null)
                {
                    string path = Interlocked.Exchange(ref FileWatcherPath, null);
                    if (path != null) fileWatcher.Remove(path);
                }
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                RemoveFileWatcher();
                pub.Dispose(ref Server);
            }
        }
        /// <summary>
        /// 域名搜索
        /// </summary>
        private unsafe sealed class domainSearcher : IDisposable
        {
            /// <summary>
            /// 字节数组搜索器
            /// </summary>
            private struct searcher
            {
                /// <summary>
                /// 状态集合
                /// </summary>
                private byte* state;
                /// <summary>
                /// 字节查找表
                /// </summary>
                private byte* bytes;
                /// <summary>
                /// 当前状态
                /// </summary>
                private byte* currentState;
                /// <summary>
                /// 查询矩阵单位尺寸类型
                /// </summary>
                private byte tableType;
                /// <summary>
                /// ASCII字节搜索器
                /// </summary>
                /// <param name="data">数据起始位置</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public searcher(pointer data)
                {
                    int stateCount = *data.Int;
                    currentState = state = data.Byte + sizeof(int);
                    bytes = state + stateCount * 3 * sizeof(int);
                    if (stateCount < 256) tableType = 0;
                    else if (stateCount < 65536) tableType = 1;
                    else tableType = 2;
                }
                /// <summary>
                /// 获取状态索引
                /// </summary>
                /// <param name="end">匹配起始位置</param>
                /// <param name="start">匹配结束位置</param>
                /// <returns>状态索引,失败返回-1</returns>
                private int search(byte* start, byte* end)
                {
                    int dotIndex = -1, value = 0;
                    currentState = state;
                    do
                    {
                        byte* prefix = currentState + *(int*)currentState;
                        int prefixSize = *(ushort*)(prefix - sizeof(ushort));
                        if (prefixSize != 0)
                        {
                            for (byte* endPrefix = prefix + prefixSize; prefix != endPrefix; ++prefix)
                            {
                                if (end == start) return dotIndex;
                                if ((uint)((value = *--end) - 'A') < 26) value |= 0x20;
                                if (value != *prefix) return dotIndex;
                            }
                        }
                        if (end == start) return *(int*)(currentState + sizeof(int) * 2);
                        if (value == '.' && (value = *(int*)(currentState + sizeof(int) * 2)) >= 0) dotIndex = value;
                        if (*(int*)(currentState + sizeof(int)) == 0) return dotIndex;
                        if ((uint)((value = *--end) - 'A') < 26) value |= 0x20;
                        int index = (int)*(bytes + value);
                        byte* table = currentState + *(int*)(currentState + sizeof(int));
                        if (tableType == 0)
                        {
                            if ((index = *(table + index)) == 0) return dotIndex;
                            currentState = state + index * 3 * sizeof(int);
                        }
                        else if (tableType == 1)
                        {
                            if ((index = (int)*(ushort*)(table + index * sizeof(ushort))) == 0) return dotIndex;
                            currentState = state + index * 3 * sizeof(int);
                        }
                        else
                        {
                            if ((index = *(int*)(table + index * sizeof(int))) == 0) return dotIndex;
                            currentState = state + index;
                        }
                    }
                    while (true);
                }
                /// <summary>
                /// 获取状态索引
                /// </summary>
                /// <param name="data">匹配状态</param>
                /// <returns>状态索引,失败返回-1</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public int Search(subArray<byte> data)
                {
                    fixed (byte* dataFixed = data.array)
                    {
                        byte* start = dataFixed + data.StartIndex;
                        return search(start, start + data.Count);
                    }
                }
                /// <summary>
                /// 获取状态索引
                /// </summary>
                /// <param name="data">匹配状态</param>
                /// <returns>状态索引,失败返回-1</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public int Search(byte[] data)
                {
                    fixed (byte* dataFixed = data) return search(dataFixed, dataFixed + data.Length);
                }
            }
            /// <summary>
            /// 域名信息集合
            /// </summary>
            private byte[][] domains;
            /// <summary>
            /// 域名服务信息集合
            /// </summary>
            public domainServer[] Servers { get; private set; }
            /// <summary>
            /// 域名搜索数据
            /// </summary>
            private pointer data;
            /// <summary>
            /// 域名搜索
            /// </summary>
            public domainSearcher() { }
            /// <summary>
            /// 域名搜索
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="servers">域名服务信息集合</param>
            private domainSearcher(byte[][] domains, domainServer[] servers)
            {
                this.domains = domains;
                Servers = servers;
                data = fastCSharp.stateSearcher.byteArray.Create(domains);
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                unmanaged.Free(ref data);
            }
            /// <summary>
            /// 获取域名服务信息
            /// </summary>
            /// <param name="domain">域名</param>
            /// <returns>域名服务信息</returns>
            public domainServer Get(subArray<byte> domain)
            {
                pointer data = this.data;
                if (domain.Count != 0 && data.Data != null)
                {
                    int index = new searcher(data).Search(domain);
                    if (index >= 0) return Servers[index];
                }
                return null;
            }
            /// <summary>
            /// 添加域名服务信息
            /// </summary>
            /// <param name="domain"></param>
            /// <param name="server"></param>
            /// <returns></returns>
            public domainSearcher Add(byte[] domain, domainServer server, out domainSearcher removeDomains)
            {
                byte[][] domains = this.domains;
                domainServer[] servers = Servers;
                pointer data = this.data;
                if (domain.Length != 0 && ((data.Data == null || new searcher(data).Search(domain) < 0)))
                {
                    byte[] reverseDomain = new byte[domain.Length];
                    fixed (byte* domainFixed = domain, reverseDomainFixed = reverseDomain)
                    {
                        for (byte* start = domainFixed, end = domainFixed + domain.Length, write = reverseDomainFixed + domain.Length; start != end; *--write = *start++) ;
                    }
                    domainSearcher searcher = new domainSearcher(domains.getAdd(reverseDomain), servers.getAdd(server));
                    removeDomains = this;
                    return searcher;
                }
                removeDomains = null;
                return this;
            }
            /// <summary>
            /// 删除域名服务信息
            /// </summary>
            /// <param name="domain"></param>
            /// <returns></returns>
            public domainSearcher Remove(byte[] domain, out domainSearcher removeDomains)
            {
                domainServer server;
                return Remove(domain, out removeDomains, out server);
            }
            /// <summary>
            /// 删除域名服务信息
            /// </summary>
            /// <param name="domain"></param>
            /// <param name="server">域名服务信息</param>
            /// <returns></returns>
            public domainSearcher Remove(byte[] domain, out domainSearcher removeDomains, out domainServer server)
            {
                byte[][] domains = this.domains;
                domainServer[] servers = Servers;
                pointer data = this.data;
                if (data.Data != null && domain.Length != 0)
                {
                    int index = new searcher(data).Search(domain);
                    if (index >= 0)
                    {
                        domainSearcher searcher = Default;
                        if (domains.Length != 1)
                        {
                            int length = domains.Length - 1;
                            byte[][] newDomains = new byte[length][];
                            domainServer[] newServers = new domainServer[length];
                            Array.Copy(domains, 0, newDomains, 0, index);
                            Array.Copy(servers, 0, newServers, 0, index);
                            Array.Copy(domains, index + 1, newDomains, index, length - index);
                            Array.Copy(servers, index + 1, newServers, index, length - index);
                            searcher = new domainSearcher(newDomains, newServers);
                        }
                        server = servers[index];
                        removeDomains = this;
                        return searcher;
                    }
                }
                server = null;
                removeDomains = null;
                return this;
            }
            /// <summary>
            /// 关闭所有域名服务
            /// </summary>
            public void Close()
            {
                foreach (domainServer domain in Servers) domain.Dispose();
            }
            /// <summary>
            /// 默认空域名搜索
            /// </summary>
            public static readonly domainSearcher Default = new domainSearcher();
        }
        /// <summary>
        /// 程序集信息缓存
        /// </summary>
        private static readonly Dictionary<hashString, Assembly> assemblyCache = dictionary.CreateHashString<Assembly>();
        /// <summary>
        /// 程序集信息访问锁
        /// </summary>
        private static readonly object assemblyLock = new object();
        /// <summary>
        /// 本地服务程序集运行目录
        /// </summary>
        private string serverPath = fastCSharp.config.web.Default.HttpServerPath;
        /// <summary>
        /// 域名搜索
        /// </summary>
        private domainSearcher domains = domainSearcher.Default;
        /// <summary>
        /// HTTP域名服务集合访问锁
        /// </summary>
        private readonly object domainLock = new object();
        /// <summary>
        /// TCP服务端口信息集合
        /// </summary>
        private Dictionary<host, server> hosts = dictionary.Create<host, server>();
        /// <summary>
        /// TCP服务端口信息集合访问锁
        /// </summary>
        private readonly object hostLock = new object();
        /// <summary>
        /// HTTP转发代理服务信息
        /// </summary>
        private fastCSharp.code.cSharp.tcpServer forwardHost;
        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        private int isDisposed;
        /// <summary>
        /// TCP调用服务器配置
        /// </summary>
        private fastCSharp.net.tcp.server server;
        /// <summary>
        /// TCP域名服务缓存文件名
        /// </summary>
        private string cacheFileName
        {
            get { return "httpServer_" + server.attribute.ServiceName + ".cache"; }
        }
        /// <summary>
        /// 文件监视器
        /// </summary>
        private createFlieTimeoutWatcher fileWatcher;
        /// <summary>
        /// 文件监视是否超时
        /// </summary>
        private int isFileWatcherTimeout;
        /// <summary>
        /// 缓存加载访问锁
        /// </summary>
        private int loadCacheLock;
        /// <summary>
        /// 是否加载缓存信息
        /// </summary>
        public bool IsLoadCache = true;
        /// <summary>
        /// 是否已经加载缓存信息
        /// </summary>
        public bool IsLoadedCache { get; private set; }
        /// <summary>
        /// 设置TCP服务端
        /// </summary>
        /// <param name="tcpServer">TCP服务端</param>
        public void SetTcpServer(fastCSharp.net.tcp.server tcpServer)
        {
            server = tcpServer;
            fileWatcher = new createFlieTimeoutWatcher(fastCSharp.config.processCopy.Default.CheckTimeoutSeconds, onFileWatcherTimeout, fastCSharp.diagnostics.processCopyServer.DefaultFileWatcherFilter);
            if (!fastCSharp.config.pub.Default.IsService && fastCSharp.config.processCopy.Default.WatcherPath != null)
            {
                try
                {
                    fileWatcher.Add(fastCSharp.config.processCopy.Default.WatcherPath);
                }
                catch (Exception error)
                {
                    log.Error.Add(error, fastCSharp.config.processCopy.Default.WatcherPath, false);
                }
            }
            if (IsLoadCache)
            {
                try
                {
                    string cacheFileName = this.cacheFileName;
                    if (File.Exists(cacheFileName))
                    {
                        interlocked.NoCheckCompareSetSleep0(ref loadCacheLock);
                        try
                        {
                            if (!IsLoadedCache)
                            {
                                saveInfo saveInfo = fastCSharp.emit.dataDeSerializer.DeSerialize<saveInfo>(File.ReadAllBytes(cacheFileName));
                                if (saveInfo.ForwardHost.Port != 0) setForward(saveInfo.ForwardHost);
                                if (saveInfo.Domains.length() != 0)
                                {
                                    foreach (saveInfo.domainServer domain in saveInfo.Domains)
                                    {
                                        try
                                        {
                                            start(domain.AssemblyPath, domain.ServerType, domain.Domains, domain.IsShareAssembly);
                                        }
                                        catch (Exception error)
                                        {
                                            log.Error.Add(error, null, false);
                                        }
                                    }
                                }
                                IsLoadedCache = true;
                            }
                        }
                        finally { loadCacheLock = 0; }
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
            }
        }
        /// <summary>
        /// 保存域名服务器参数集合
        /// </summary>
        private void save()
        {
            try
            {
                saveInfo saveInfo = new saveInfo();
                if (forwardHost != null)
                {
                    saveInfo.ForwardHost.Host = forwardHost.Host;
                    saveInfo.ForwardHost.Port = forwardHost.Port;
                }
                Monitor.Enter(domainLock);
                try
                {
                    saveInfo.Domains = domains.Servers.getHash().getArray(domain => new saveInfo.domainServer
                    {
                        AssemblyPath = domain.AssemblyPath,
                        ServerType = domain.ServerType,
                        IsShareAssembly = domain.IsShareAssembly,
                        Domains = domain.Domains.getFindArray(value => value.Value == 0, value => value.Key)
                    }).getFindArray(value => value.Domains.length() != 0);
                }
                finally { Monitor.Exit(domainLock); }
                File.WriteAllBytes(cacheFileName, fastCSharp.emit.dataSerializer.Serialize(saveInfo));
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                save();
                forwardHost = null;

                pub.Dispose(ref fileWatcher);

                Monitor.Enter(domainLock);
                domainSearcher domains = this.domains;
                this.domains = domainSearcher.Default;
                Monitor.Exit(domainLock);
                domains.Close();
                domains.Dispose();

                server[] servers = null;
                Monitor.Enter(hostLock);
                try
                {
                    servers = hosts.Values.getArray();
                    hosts = null;
                }
                finally
                {
                    Monitor.Exit(hostLock);
                    if (servers != null) foreach (server server in servers) server.Dispose();
                }
            }
        }
        /// <summary>
        /// HTTP服务验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024)]
        protected virtual bool verify(string value)
        {
            if (fastCSharp.config.http.Default.HttpVerify == null && !fastCSharp.config.pub.Default.IsDebug)
            {
                fastCSharp.log.Error.Add("HTTP服务验证数据不能为空", false, true);
                return false;
            }
            return fastCSharp.config.http.Default.HttpVerify == value;
        }
        /// <summary>
        /// 域名状态检测
        /// </summary>
        /// <param name="domain">域名信息</param>
        /// <param name="server">域名服务</param>
        /// <returns>域名状态</returns>
        private startState checkDomain(ref domain domain, domainServer server)
        {
            byte[] domainData = domain.Domain;
            if (domain.Host.Port == 0) domain.Host.Port = domain.CertificateFileName == null ? 80 : 443;
            if (domainData == null)
            {
                string domainString = domain.Host.Host;
                if (domainString.length() == 0) return servers.startState.DomainError;
                if (domain.Host.Port != (domain.CertificateFileName == null ? 80 : 443))
                {
                    domainString += ":" + domain.Host.Port.toString();
                }
                domain.Domain = domainData = domainString.getBytes();
                fastCSharp.log.Default.Add(domainString + " 缺少指定域名", false, false);
            }
            else if (domainData.Length == 0) return servers.startState.DomainError;
            else if (domain.Host.Port != (domain.CertificateFileName == null ? 80 : 443) && domainData.indexOf((byte)':') == -1)
            {
                domain.Domain = domainData = (domainData.deSerialize() + ":" + domain.Host.Port.toString()).getBytes();
            }
            if (!domain.Host.HostToIpAddress()) return servers.startState.HostError;
            if (domain.CertificateFileName != null && !File.Exists(domain.CertificateFileName))
            {
                fastCSharp.log.Error.Add("没有找到安全证书文件 " + domain.CertificateFileName, false, false);
                return servers.startState.CertificateError;
            }
            domainData.toLower();
            domainSearcher removeDomains = null;
            Monitor.Enter(domainLock);
            try
            {
                domains = domains.Add(domainData, server, out removeDomains);
            }
            finally
            {
                Monitor.Exit(domainLock);
                if (removeDomains != null) removeDomains.Dispose();
            }
            return removeDomains == null ? startState.DomainExists : servers.startState.Success;
        }
        /// <summary>
        /// 删除域名信息
        /// </summary>
        /// <param name="domain">域名信息</param>
        private void removeDomain(domain domain)
        {
            domainSearcher removeDomains = null;
            Monitor.Enter(domainLock);
            try
            {
                domains = domains.Remove(domain.Domain, out removeDomains);
            }
            finally
            {
                Monitor.Exit(domainLock);
                if (removeDomains != null) removeDomains.Dispose();
            }
        }
        /// <summary>
        /// 启动域名服务
        /// </summary>
        /// <param name="assemblyPath">程序集文件名,包含路径</param>
        /// <param name="serverType">服务程序类型名称</param>
        /// <param name="domain">域名信息</param>
        /// <param name="isShare">是否共享程序集</param>
        /// <returns>域名服务启动状态</returns>
        [fastCSharp.code.cSharp.tcpServer]
        private unsafe startState start(string assemblyPath, string serverType, domain domain, bool isShareAssembly)
        {
            return start(assemblyPath, serverType, new domain[] { domain }, isShareAssembly);
        }
        /// <summary>
        /// 启动域名服务
        /// </summary>
        /// <param name="assemblyPath">程序集文件名,包含路径</param>
        /// <param name="serverType">服务程序类型名称</param>
        /// <param name="domains">域名信息集合</param>
        /// <param name="isShareAssembly">是否共享程序集</param>
        /// <returns>域名服务启动状态</returns>
        [fastCSharp.code.cSharp.tcpServer]
        private unsafe startState start(string assemblyPath, string serverType, domain[] domains, bool isShareAssembly)
        {
            if (isDisposed != 0) return startState.Disposed;
            if (domains.length() == 0) return startState.DomainError;
            FileInfo assemblyFile = new FileInfo(assemblyPath);
            if (!File.Exists(assemblyPath))
            {
                log.Error.Add("未找到程序集 " + assemblyPath, false, false);
                return startState.NotFoundAssembly;
            }
            int domainCount = 0;
            startState state = startState.Unknown;
            domainServer server = new domainServer { AssemblyPath = assemblyPath, ServerType = serverType, Servers = this, IsShareAssembly = isShareAssembly };
            foreach (domain domain in domains)
            {
                if ((state = checkDomain(ref domains[domainCount], server)) != startState.Success) break;
                ++domainCount;
            }
            try
            {
                if (state == startState.Success)
                {
                    state = startState.StartError;
                    Assembly assembly = null;
                    DirectoryInfo directory = assemblyFile.Directory;
                    keyValue<domain, int>[] domainFlags = domains.getArray(value => new keyValue<domain, int>(value, 0));
                    hashString pathKey = assemblyPath;
                    Monitor.Enter(assemblyLock);
                    try
                    {
                        if (!isShareAssembly || !assemblyCache.TryGetValue(pathKey, out assembly))
                        {
                            string serverPath = this.serverPath + ((ulong)CoreLibrary.PubPlus.StartTime.Ticks).toHex16() + ((ulong)CoreLibrary.PubPlus.Identity).toHex16() + fastCSharp.directory.DirectorySeparator;
                            Directory.CreateDirectory(serverPath);
                            foreach (FileInfo file in directory.GetFiles()) file.CopyTo(serverPath + file.Name);
                            assembly = Assembly.LoadFrom(serverPath + assemblyFile.Name);
                            if (isShareAssembly) assemblyCache.Add(pathKey, assembly);
                        }
                    }
                    finally { Monitor.Exit(assemblyLock); }
                    server.Server = (http.domainServer)Activator.CreateInstance(assembly.GetType(serverType));
                    DirectoryInfo loadDirectory = directory;
                    do
                    {
                        string loadPath = loadDirectory.Name.toLower();
                        if (loadPath == "release" || loadPath == "bin" || loadPath == "debug")
                        {
                            loadDirectory = loadDirectory.Parent;
                        }
                        else break;
                    }
                    while (loadDirectory != null);
                    server.Server.LoadCheckPath = (loadDirectory ?? directory).FullName;
                    if (server.Server.Start(domains, server.RemoveFileWatcher))
                    {
                        fileWatcher.Add(directory.FullName);
                        server.FileWatcherPath = directory.FullName;
                        if ((state = start(domains)) == startState.Success)
                        {
                            server.DomainCount = domains.Length;
                            server.Domains = domainFlags;
                            server.IsStart = true;
                            log.Default.Add(@"domain success
" + domains.joinString(@"
", domain => domain.Host.Host + ":" + domain.Host.Port.toString() + "[" + domain.Domain.deSerialize() + "]"), false, false);
                            return startState.Success;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            foreach (domain domain in domains)
            {
                if (domainCount-- == 0) break;
                removeDomain(domain);
            }
            server.Dispose();
            return state;
        }
        /// <summary>
        /// 启动TCP服务
        /// </summary>
        /// <param name="domains">域名信息集合</param>
        /// <returns>HTTP服务启动状态</returns>
        private startState start(domain[] domains)
        {
            int hostCount = 0, startCount = 0;
            foreach (domain domain in domains)
            {
                if (!domain.IsOnlyHost)
                {
                    startState state = start(domain.Host, domain.CertificateFileName);
                    if (state != startState.Success) break;
                    ++startCount;
                }
                ++hostCount;
            }
            if (startCount != 0 && hostCount == domains.Length) return startState.Success;
            foreach (domain domain in domains)
            {
                if (hostCount-- == 0) break;
                if (!domain.IsOnlyHost) stop(domain.Host);
            }
            return startState.TcpError;
        }
        /// <summary>
        /// 启动TCP服务
        /// </summary>
        /// <param name="host">TCP服务端口信息</param>
        /// <param name="certificateFileName">安全证书文件</param>
        /// <returns>HTTP服务启动状态</returns>
        private startState start(host host, string certificateFileName)
        {
            startState state = startState.TcpError;
            server server = null;
            Monitor.Enter(hostLock);
            try
            {
                if (hosts.TryGetValue(host, out server))
                {
                    if (server.CheckCertificate(certificateFileName))
                    {
                        ++server.DomainCount;
                        return startState.Success;
                    }
                    server = null;
                    state = startState.CertificateMatchError;
                }
                else
                {
                    state = startState.CertificateError;
                    server = certificateFileName == null ? new server(this, host) : new sslServer(this, host, certificateFileName);
                    state = startState.TcpError;
                    if (server.Start())
                    {
                        hosts.Add(host, server);
                        return startState.Success;
                    }
                }
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            finally { Monitor.Exit(hostLock); }
            pub.Dispose(ref server);
            return state;
        }
        /// <summary>
        /// 停止域名服务
        /// </summary>
        /// <param name="domain">域名信息</param>
        [fastCSharp.code.cSharp.tcpServer]
        private unsafe void stop(domain domain)
        {
            domainSearcher removeDomains = null;
            domainServer domainServer = null;
            byte[] domainData = domain.Domain.toLower();
            Monitor.Enter(domainLock);
            try
            {
                domains = domains.Remove(domainData, out removeDomains, out domainServer);
            }
            finally
            {
                Monitor.Exit(domainLock);
                if (removeDomains != null) removeDomains.Dispose();
            }
            if (domainServer != null && domainServer.Domains != null)
            {
                for (int index = domainServer.Domains.Length; index != 0; )
                {
                    keyValue<domain, int> stopDomain = domainServer.Domains[--index];
                    if ((stopDomain.Value | (stopDomain.Key.Domain.Length ^ domainData.Length)) == 0
                        && unsafer.memory.Equal(stopDomain.Key.Domain, domainData, domainData.Length)
                        && Interlocked.CompareExchange(ref domainServer.Domains[index].Value, 1, 0) == 0)
                    {
                        if (!stopDomain.Key.IsOnlyHost) stop(stopDomain.Key.Host);
                        if (Interlocked.Decrement(ref domainServer.DomainCount) == 0) domainServer.Dispose();
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 停止域名服务
        /// </summary>
        /// <param name="domains">域名信息集合</param>
        [fastCSharp.code.cSharp.tcpServer]
        private unsafe void stop(domain[] domains)
        {
            if (domains != null)
            {
                foreach (domain domain in domains) stop(domain);
            }
        }
        ///// <summary>
        ///// 停止域名服务
        ///// </summary>
        ///// <param name="domainServer">域名服务</param>
        //private unsafe void stop(domainServer domainServer)
        //{
        //    if (domainServer != null && domainServer.Domains != null)
        //    {
        //        try
        //        {
        //            for (int index = domainServer.Domains.Length; index != 0; )
        //            {
        //                if (Interlocked.CompareExchange(ref domainServer.Domains[--index].Value, 1, 0) == 0)
        //                {
        //                    stop(domainServer.Domains[index].Key);
        //                    Interlocked.Decrement(ref domainServer.DomainCount);
        //                }
        //            }
        //        }
        //        catch (Exception error)
        //        {
        //            log.Default.Add(error, null, false);
        //        }
        //        finally
        //        {
        //            domainServer.Dispose();
        //        }
        //    }
        //}
        /// <summary>
        /// 停止TCP服务
        /// </summary>
        /// <param name="host">TCP服务端口信息</param>
        private void stop(host host)
        {
            server server;
            Monitor.Enter(hostLock);
            try
            {
                if (hosts.TryGetValue(host, out server))
                {
                    if (--server.DomainCount == 0) hosts.Remove(host);
                    else server = null;
                }
            }
            finally { Monitor.Exit(hostLock); }
            if (server != null) server.Dispose();
        }
        /// <summary>
        /// 文件监视超时处理
        /// </summary>
        private void onFileWatcherTimeout()
        {
            if (Interlocked.CompareExchange(ref isFileWatcherTimeout, 1, 0) == 0)
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    FileInfo file = new FileInfo(process.MainModule.FileName);
                    if (fastCSharp.config.processCopy.Default.WatcherPath == null)
                    {
                        ProcessStartInfo info = new ProcessStartInfo(file.FullName, null);
                        info.UseShellExecute = true;
                        info.WorkingDirectory = file.DirectoryName;
                        using (Process newProcess = Process.Start(info)) Environment.Exit(-1);
                    }
                    else
                    {
                        processCopyServer.Remove();
                        fileWatcherTimeout();
                    }
                }
            }
        }
        /// <summary>
        /// 文件监视超时处理
        /// </summary>
        private void fileWatcherTimeout()
        {
            if (processCopyServer.CopyStart())
            {
                Dispose();
                server.Dispose();
                Environment.Exit(-1);
            }
            else
            {
                timerTask.Default.Add(fileWatcherTimeout, date.NowSecond.AddSeconds(fastCSharp.config.processCopy.Default.CheckTimeoutSeconds), null);
            }
        }
        /// <summary>
        /// 设置HTTP转发代理服务信息
        /// </summary>
        /// <param name="host">HTTP转发代理服务信息</param>
        /// <returns>是否设置成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        private bool setForward(host host)
        {
            if (isDisposed == 0 && host.HostToIpAddress())
            {
                fastCSharp.code.cSharp.tcpServer tcpServer = new fastCSharp.code.cSharp.tcpServer { Host = host.Host, Port = host.Port };
                if (tcpServer.IpAddress != IPAddress.Any)
                {
                    forwardHost = tcpServer;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 清除HTTP转发代理服务信息
        /// </summary>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        private void removeForward()
        {
            forwardHost = null;
        }
        /// <summary>
        /// 获取HTTP转发代理服务客户端
        /// </summary>
        /// <returns>HTTP转发代理服务客户端,失败返回null</returns>
        internal virtual client GetForwardClient()
        {
            fastCSharp.code.cSharp.tcpServer host = forwardHost;
            if (host != null)
            {
                client client = new client(host, true);
                if (client.IsStart) return client;
                client.Dispose();
            }
            return null;
        }
        /// <summary>
        /// 获取域名服务信息
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>域名服务信息</returns>
        internal virtual domainServer GetServer(subArray<byte> domain)
        {
            domainServer server = domains.Get(domain);
            return server != null && server.IsStart ? server : null;
        }
    }
}
