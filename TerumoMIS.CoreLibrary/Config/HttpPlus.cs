//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HttpPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  HttpPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:01:34
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary.Config
{
    /// <summary>
    ///     Http服务相关参数
    /// </summary>
    public sealed class HttpPlus
    {
        /// <summary>
        ///     默认HTTP服务相关参数
        /// </summary>
        public static readonly HttpPlus Default = new HttpPlus();

        /// <summary>
        ///     大数据缓存字节数(单位:KB)
        /// </summary>
        private readonly int _bigBufferSize = 64;

        /// <summary>
        ///     HTTP头部最大项数
        /// </summary>
        private readonly int _maxHeaderCount = 32;

        /// <summary>
        ///     HTTP内存流最大字节数(单位:KB)
        /// </summary>
        private readonly int _maxMemoryStreamSize = 64;

        /// <summary>
        ///     HTTP服务名称
        /// </summary>
        private readonly string _serviceName = "TmphCoreLib.HttpServer";

        /// <summary>
        ///     Session超时分钟数
        /// </summary>
        private readonly int _sessionMinutes = 60;

        /// <summary>
        ///     Session名称
        /// </summary>
        private readonly string _sessionName = "TmphCoreLibSession";

        /// <summary>
        ///     Session超时刷新分钟数
        /// </summary>
        private readonly int _sessionRefreshMinutes = 10;

        /// <summary>
        ///     Session服务名称
        /// </summary>
        private readonly string _sessionServiceName = "TmphCoreLib.HttpSessionServer";

        /// <summary>
        ///     WebSocket超时
        /// </summary>
        private readonly int _webSocketReceiveSeconds = 60;

        /// <summary>
        ///     HTTP头部缓存数据大小
        /// </summary>
        private readonly int headerBufferLength = 1 << 10;

        /// <summary>
        ///     HTTP连接每IP最大活动连接数量,小于等于0表示不限
        /// </summary>
        private readonly int ipActiveClientCount = 256;

        /// <summary>
        ///     域名转IP地址缓存数量(小于等于0表示不限)
        /// </summary>
        private readonly int ipAddressCacheCount = 1 << 10;

        /// <summary>
        ///     域名转IP地址缓存时间
        /// </summary>
        private readonly int ipAddressTimeoutMinutes = 60;

        /// <summary>
        ///     HTTP连接每IP最大连接数量,小于等于0表示不限
        /// </summary>
        private readonly int ipClientCount = 1024;

        /// <summary>
        ///     HTTP最大接收数据字节数(单位:MB)
        /// </summary>
        private readonly int maxPostDataSize = 4;

        /// <summary>
        ///     HTTP每秒最小表单数据接收字节数(单位KB)
        /// </summary>
        private readonly int minReceiveSizePerSecond = 8;

        /// <summary>
        ///     套接字接收超时
        /// </summary>
        private readonly int receiveSeconds = 15;

        /// <summary>
        ///     HTTP服务验证
        /// </summary>
        public string HttpVerify;

        /// <summary>
        ///     HTTP服务启动后启动的进程
        /// </summary>
        public string[] OnStartProcesses;

        /// <summary>
        ///     HTTP服务密码
        /// </summary>
        public string ServicePassword;

        /// <summary>
        ///     HTTP服务用户名
        /// </summary>
        public string ServiceUsername;

        /// <summary>
        ///     Session服务密码
        /// </summary>
        public string SessionServicePassword;

        /// <summary>
        ///     Session服务用户名
        /// </summary>
        public string SessionServiceUsername;

        /// <summary>
        ///     Session服务验证
        /// </summary>
        public string SessionVerify;

        /// <summary>
        ///     HTTP服务相关参数
        /// </summary>
        private HttpPlus()
        {
            PubPlus.LoadConfig(this);
        }

        /// <summary>
        ///     HTTP服务名称
        /// </summary>
        public string ServiceName
        {
            get { return _serviceName.Length == 0 ? "TmphCoreLib.HttpServer" : _serviceName; }
        }

        /// <summary>
        ///     HTTP连接每IP最大连接数量,0表示不限
        /// </summary>
        public int IpClientCount
        {
            get { return ipClientCount; }
        }

        /// <summary>
        ///     HTTP连接每IP最大活动连接数量,0表示不限
        /// </summary>
        public int IpActiveClientCount
        {
            get { return ipActiveClientCount; }
        }

        /// <summary>
        ///     HTTP头部缓存数据大小
        /// </summary>
        public int HeaderBufferLength
        {
            get
            {
                return headerBufferLength >= (1 << 10) && headerBufferLength < (1 << 15) ? headerBufferLength : (1 << 10);
            }
        }

        /// <summary>
        ///     HTTP头部最大项数
        /// </summary>
        public int MaxHeaderCount
        {
            get { return _maxHeaderCount; }
        }

        /// <summary>
        ///     HTTP每秒最小表单数据接收字节数
        /// </summary>
        public int MinReceiveSizePerSecond
        {
            get { return minReceiveSizePerSecond > 0 ? minReceiveSizePerSecond << 10 : 0; }
        }

        /// <summary>
        ///     套接字接收超时
        /// </summary>
        public int ReceiveSeconds
        {
            get { return receiveSeconds; }
        }

        /// <summary>
        ///     WebSocket超时
        /// </summary>
        public int WebSocketReceiveSeconds
        {
            get { return _webSocketReceiveSeconds; }
        }

        /// <summary>
        ///     HTTP最大接收数据字节数(单位:MB)
        /// </summary>
        public int MaxPostDataSize
        {
            get { return maxPostDataSize > 0 ? maxPostDataSize : 4; }
        }

        /// <summary>
        ///     HTTP内存流最大字节数(单位:KB)
        /// </summary>
        public int MaxMemoryStreamSize
        {
            get { return _maxMemoryStreamSize >= 0 ? _maxMemoryStreamSize : 64; }
        }

        /// <summary>
        ///     大数据缓存字节数
        /// </summary>
        public int BigBufferSize
        {
            get
            {
                return Math.Max(Math.Max(_bigBufferSize << 10, headerBufferLength << 1), AppSettingPlus.StreamBufferSize);
            }
        }

        /// <summary>
        ///     Session名称
        /// </summary>
        public string SessionName
        {
            get { return _sessionName ?? "TmphCoreLibSession"; }
        }

        /// <summary>
        ///     Session服务名称
        /// </summary>
        public string SessionServiceName
        {
            get { return _sessionServiceName.Length == 0 ? "TmphCoreLib.HttpSessionServer" : _sessionServiceName; }
        }

        /// <summary>
        ///     Session超时分钟数
        /// </summary>
        public int SessionMinutes
        {
            get { return _sessionMinutes; }
        }

        /// <summary>
        ///     Session超时刷新分钟数
        /// </summary>
        public int SessionRefreshMinutes
        {
            get { return _sessionRefreshMinutes > _sessionMinutes ? _sessionMinutes : _sessionRefreshMinutes; }
        }

        /// <summary>
        ///     域名转IP地址缓存时间
        /// </summary>
        public int IpAddressTimeoutMinutes
        {
            get { return Math.Max(ipAddressTimeoutMinutes, 1); }
        }

        /// <summary>
        ///     域名转IP地址缓存时间
        /// </summary>
        public int IpAddressCacheCount
        {
            get { return ipAddressCacheCount; }
        }
    }
}