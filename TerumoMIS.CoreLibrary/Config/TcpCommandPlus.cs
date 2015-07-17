//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TcpCommandPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  TcpCommandPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:06:28
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

namespace TerumoMIS.CoreLibrary.Config
{
    /// <summary>
    /// TCP调用配置
    /// </summary>
    public sealed class TcpCommandPlus
    {
        /// <summary>
        /// 命令套接字默认超时秒数
        /// </summary>
        private int defaultTimeout = 60;
        /// <summary>
        /// 命令套接字默认超时秒数
        /// </summary>
        public int DefaultTimeout
        {
            get { return defaultTimeout; }
        }
        /// <summary>
        /// 命令套接字大数据缓存字节数(单位:KB)
        /// </summary>
        private int bigBufferSize = 128;
        /// <summary>
        /// 命令套接字大数据缓存字节数
        /// </summary>
        public int BigBufferSize
        {
            get { return Math.Max(bigBufferSize << 10, fastCSharp.config.appSetting.StreamBufferSize); }
        }
        /// <summary>
        /// 命令套接字异步缓存字节数(单位:KB)
        /// </summary>
        private int asyncBufferSize = 0;
        /// <summary>
        /// 命令套接字异步缓存字节数
        /// </summary>
        public int AsyncBufferSize
        {
            get { return Math.Max(asyncBufferSize << 10, fastCSharp.config.appSetting.StreamBufferSize); }
        }
        /// <summary>
        /// 命令套接字客户端标识验证超时秒数
        /// </summary>
        private int clientVerifyTimeout = 15;
        /// <summary>
        /// 命令套接字客户端标识验证超时秒数
        /// </summary>
        public int ClientVerifyTimeout
        {
            get { return clientVerifyTimeout <= 0 ? 15 : clientVerifyTimeout; }
        }
        /// <summary>
        /// TCP流超时秒数
        /// </summary>
        private int tcpStreamTimeout = 60;
        /// <summary>
        /// TCP流超时秒数
        /// </summary>
        public int TcpStreamTimeout
        {
            get { return tcpStreamTimeout <= 0 ? 60 : tcpStreamTimeout; }
        }
        /// <summary>
        /// TCP调用配置
        /// </summary>
        private tcpCommand()
        {
            pub.LoadConfig(this);
        }
        /// <summary>
        /// 默认TCP调用配置
        /// </summary>
        public static readonly tcpCommand Default = new tcpCommand();
    }
}
