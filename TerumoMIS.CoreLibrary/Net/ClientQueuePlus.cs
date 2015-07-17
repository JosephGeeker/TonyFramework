//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ClientQueuePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net
//	File Name:  ClientQueuePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:56:03
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

namespace TerumoMIS.CoreLibrary.Net
{
    /// <summary>
    /// 客户端队列
    /// </summary>
    public abstract class ClientQueuePlus
    {
        /// <summary>
        /// 套接字类型
        /// </summary>
        public enum socketType
        {
            /// <summary>
            /// 客户端错误
            /// </summary>
            Error,
            /// <summary>
            /// 队列
            /// </summary>
            Queue,
            /// <summary>
            /// IPv4
            /// </summary>
            Ipv4,
            /// <summary>
            /// IPv4
            /// </summary>
            Ipv6,
        }
        /// <summary>
        /// 每IP最大活动连接数量,等于0表示不限
        /// </summary>
        protected int maxActiveCount;
        /// <summary>
        /// 每IP最大连接数量,等于0表示不限
        /// </summary>
        protected int maxCount;
        /// <summary>
        /// IPv4客户端连接 访问锁
        /// </summary>
        protected int ipv4Lock;
        /// <summary>
        /// IPv4客户端连接 访问锁
        /// </summary>
        protected int ipv6Lock;
        /// <summary>
        /// 客户端队列
        /// </summary>
        /// <param name="maxActiveCount">每IP最大活动连接数量,等于0表示不限</param>
        /// <param name="maxCount">每IP最大连接数量,等于0表示不限</param>
        protected clientQueue(int maxActiveCount, int maxCount)
        {
            this.maxActiveCount = maxActiveCount;
            this.maxCount = maxCount;
        }
    }
    /// <summary>
    /// 客户端队列
    /// </summary>
    /// <typeparam name="clientType">客户端实例类型</typeparam>
    public class clientQueue<clientType> : clientQueue, IDisposable
    {
        /// <summary>
        /// 客户端空队列
        /// </summary>
        private sealed class nullQueue : clientQueue<clientType>
        {
            /// <summary>
            /// 添加客户端
            /// </summary>
            /// <param name="client">客户端</param>
            /// <param name="socket">套接字</param>
            /// <param name="ipv4">ipv4地址</param>
            /// <param name="ipv6">ipv6地址</param>
            /// <returns>套接字操作类型</returns>
            public override socketType NewClient(clientType client, Socket socket, ref int ipv4, ref ipv6Hash ipv6)
            {
                ipv4 = 0;
                return socketType.Ipv4;
            }
            /// <summary>
            /// 请求处理结束
            /// </summary>
            /// <param name="ipv4">ipv4地址</param>
            /// <returns>下一个客户端</returns>
            public override clientType End(int ipv4)
            {
                return default(clientType);
            }
            /// <summary>
            /// 请求处理结束
            /// </summary>
            /// <param name="ipv6">ipv6地址</param>
            /// <returns>下一个客户端</returns>
            public override clientType End(ipv6Hash ipv6)
            {
                return default(clientType);
            }
            /// <summary>
            /// 客户端空队列
            /// </summary>
            private nullQueue() : base(0, 0, null) { }
            /// <summary>
            /// 客户端空队列
            /// </summary>
            public static readonly nullQueue Default = new nullQueue();

        }
        /// <summary>
        /// 客户端连接数量
        /// </summary>
        private sealed class count
        {
            /// <summary>
            /// 等待处理的客户端
            /// </summary>
            public subArray<clientType> Sockets;
            /// <summary>
            /// 户端连接数量
            /// </summary>
            public int Count;
            /// <summary>
            /// 获取HTTP客户端连接数量
            /// </summary>
            /// <returns>HTTP客户端连接数量</returns>
            public static count Get()
            {
                count count = typePool<count>.Pop() ?? new count();
                count.Count = 1;
                return count;
            }
        }
        /// <summary>
        /// 客户端信息
        /// </summary>
        public struct clientInfo
        {
            /// <summary>
            /// 客户端
            /// </summary>
            public clientType Client;
            /// <summary>
            /// IPv6地址
            /// </summary>
            public ipv6Hash Ipv6;
            /// <summary>
            /// IPv4地址
            /// </summary>
            public int Ipv4;
            /// <summary>
            /// 套接字类型
            /// </summary>
            public socketType Type;
        }
        /// <summary>
        /// IPv4客户端连接数量信息
        /// </summary>
        private Dictionary<int, count> ipv4Count;
        /// <summary>
        /// IPv6客户端连接数量信息
        /// </summary>
        private Dictionary<ipv6Hash, count> ipv6Count;
        /// <summary>
        /// 释放客户端
        /// </summary>
        private Action<clientType> disposeClient;
        /// <summary>
        /// 客户端队列
        /// </summary>
        /// <param name="maxActiveCount">每IP最大活动连接数量,等于0表示不限</param>
        /// <param name="maxCount">每IP最大连接数量,等于0表示不限</param>
        /// <param name="disposeClient">释放客户端</param>
        protected clientQueue(int maxActiveCount, int maxCount, Action<clientType> disposeClient)
            : base(maxActiveCount, maxCount)
        {
            this.disposeClient = disposeClient;
            ipv4Count = dictionary.CreateInt<count>();
            ipv6Count = dictionary.Create<ipv6Hash, count>();
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (disposeClient != null)
            {
                count[] ipv4Sockets = null, ipv6Sockets = null;
                interlocked.NoCheckCompareSetSleep0(ref ipv4Lock);
                try
                {
                    ipv4Sockets = ipv4Count.Values.getArray();
                    ipv4Count.Clear();
                }
                finally { ipv4Lock = 0; }
                interlocked.NoCheckCompareSetSleep0(ref ipv6Lock);
                try
                {
                    ipv6Sockets = ipv6Count.Values.getArray();
                    ipv6Count.Clear();
                }
                finally { ipv6Lock = 0; }
                dispose(ipv4Sockets);
                dispose(ipv6Sockets);
            }
        }
        /// <summary>
        /// 释放客户端
        /// </summary>
        /// <param name="counts">客户端集合</param>
        private void dispose(count[] counts)
        {
            foreach (count count in counts)
            {
                int index = count.Sockets.Count;
                if (index != 0)
                {
                    foreach (clientType client in count.Sockets.array)
                    {
                        try
                        {
                            disposeClient(client);
                        }
                        catch { }
                        if (--index == 0) break;
                    }
                    Array.Clear(count.Sockets.array, 0, count.Sockets.Count);
                    count.Sockets.Empty();
                }
                typePool<count>.Push(count);
            }
        }
        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="socket">套接字</param>
        /// <param name="ipv4">ipv4地址</param>
        /// <param name="ipv6">ipv6地址</param>
        /// <returns>套接字操作类型</returns>
        public virtual socketType NewClient(clientType client, Socket socket, ref int ipv4, ref ipv6Hash ipv6)
        {
            socketType type = socketType.Error;
            try
            {
                IPEndPoint ipEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                count count;
                if (ipEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (!(ipv6 = ipEndPoint.Address).IsNull)
                    {
                        interlocked.NoCheckCompareSetSleep0(ref ipv6Lock);
                        if (ipv6Count.TryGetValue(ipv6, out count))
                        {
                            if (count.Count < maxActiveCount)
                            {
                                ++count.Count;
                                ipv6Lock = 0;
                                type = socketType.Ipv6;
                            }
                            else if (count.Count < maxCount)
                            {
                                try
                                {
                                    count.Sockets.Add(client);
                                    ++count.Count;
                                }
                                finally { ipv6Lock = 0; }
                                type = socketType.Queue;
                            }
                            else ipv6Lock = 0;
                        }
                        else
                        {
                            try
                            {
                                ipv6Count.Add(ipv6, count.Get());
                            }
                            finally { ipv6Lock = 0; }
                            type = socketType.Ipv6;
                        }
                    }
                }
                else
                {
#pragma warning disable 618
                    ipv4 = (int)ipEndPoint.Address.Address;
#pragma warning restore 618
                    int ipKey = ipv4 ^ random.Hash;
                    interlocked.NoCheckCompareSetSleep0(ref ipv4Lock);
                    if (ipv4Count.TryGetValue(ipKey, out count))
                    {
                        if (count.Count < maxActiveCount)
                        {
                            ++count.Count;
                            ipv4Lock = 0;
                            type = socketType.Ipv4;
                        }
                        else if (count.Count < maxCount)
                        {
                            try
                            {
                                count.Sockets.Add(client);
                                ++count.Count;
                            }
                            finally { ipv4Lock = 0; }
                            type = socketType.Queue;
                        }
                        else ipv4Lock = 0;
                    }
                    else
                    {
                        try
                        {
                            ipv4Count.Add(ipKey, count.Get());
                        }
                        finally { ipv4Lock = 0; }
                        type = socketType.Ipv4;
                    }
                }
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            if (type == socketType.Error) socket.Close();
            return type;
        }
        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="socket">套接字</param>
        /// <returns>客户端信息</returns>
        public clientInfo NewClient(clientType client, Socket socket)
        {
            clientInfo clientInfo = new clientInfo { Client = client };
            clientInfo.Type = NewClient(client, socket, ref clientInfo.Ipv4, ref clientInfo.Ipv6);
            return clientInfo;
        }
        /// <summary>
        /// 请求处理结束
        /// </summary>
        /// <param name="ipv4">ipv4地址</param>
        /// <returns>下一个客户端</returns>
        public virtual clientType End(int ipv4)
        {
            clientType socket = default(clientType);
            count count;
            int ipKey = ipv4 ^ random.Hash;
            interlocked.NoCheckCompareSetSleep0(ref ipv4Lock);
            if (ipv4Count.TryGetValue(ipKey, out count))
            {
                if (count.Count <= maxActiveCount)
                {
                    if (--count.Count == 0)
                    {
                        try
                        {
                            ipv4Count.Remove(ipKey);
                        }
                        finally { ipv4Lock = 0; }
                        typePool<count>.Push(ref count);
                    }
                    else ipv4Lock = 0;
                }
                else
                {
                    socket = count.Sockets.UnsafePopReset();
                    --count.Count;
                    ipv4Lock = 0;
                }
            }
            else ipv4Lock = 0;
            return socket;
        }
        /// <summary>
        /// 请求处理结束
        /// </summary>
        /// <param name="ipv6">ipv6地址</param>
        /// <returns>下一个客户端</returns>
        public virtual clientType End(ipv6Hash ipv6)
        {
            clientType socket = default(clientType);
            count count;
            interlocked.NoCheckCompareSetSleep0(ref ipv6Lock);
            if (ipv6Count.TryGetValue(ipv6, out count))
            {
                if (count.Count <= maxActiveCount)
                {
                    if (--count.Count == 0)
                    {
                        try
                        {
                            ipv6Count.Remove(ipv6);
                        }
                        finally { ipv6Lock = 0; }
                        typePool<count>.Push(ref count);
                    }
                    else ipv6Lock = 0;
                }
                else
                {
                    socket = count.Sockets.UnsafePopReset();
                    --count.Count;
                    ipv6Lock = 0;
                }
            }
            else ipv6Lock = 0;
            return socket;
        }
        /// <summary>
        /// 客户端套接字队列
        /// </summary>
        /// <param name="maxActiveCount">每IP最大活动连接数量,小于等于0表示不限</param>
        /// <param name="maxCount">每IP最大连接数量,小于等于0表示不限</param>
        /// <param name="disposeClient">释放客户端</param>
        public static clientQueue<clientType> Create(int maxActiveCount, int maxCount, Action<clientType> disposeClient)
        {
            if (maxActiveCount <= 0) return nullQueue.Default;
            if (disposeClient == null) log.Error.Throw(log.exceptionType.Null);
            return new clientQueue<clientType>(maxActiveCount, maxCount < maxActiveCount ? maxActiveCount : maxCount, disposeClient);
        }
    }
}
