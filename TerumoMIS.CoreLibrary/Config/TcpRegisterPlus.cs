//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TcpRegisterPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  TcpRegisterPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:07:22
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
    /// TCP注册服务配置
    /// </summary>
    public sealed class TcpRegisterPlus
    {
        /// <summary>
        /// TCP注册服务名称
        /// </summary>
        private string serviceName = "fastCSharp.tcpResiter";
        /// <summary>
        /// TCP注册服务名称
        /// </summary>
        public string ServiceName
        {
            get { return serviceName.length() == 0 ? "fastCSharp.tcpResiter" : serviceName; }
        }
        /// <summary>
        /// TCP注册服务用户名
        /// </summary>
        public string Username;
        /// <summary>
        /// TCP注册服务密码
        /// </summary>
        public string Password;
        /// <summary>
        /// TCP注册服务依赖
        /// </summary>
        public string[] DependedOn;
        /// <summary>
        /// TCP注册服务启动后启动的进程
        /// </summary>
        public string[] OnStartProcesses;
        /// <summary>
        /// TCP服务注册起始端口号
        /// </summary>
        private int portStart = 9000;
        /// <summary>
        /// TCP服务注册起始端口号
        /// </summary>
        public int PortStart
        {
            get { return portStart; }
        }
        /// <summary>
        /// TCP服务注册验证
        /// </summary>
        public string Verify;
        /// <summary>
        /// TCP服务注册无响应超时秒数
        /// </summary>
        private int registerTimeoutSeconds = 10;
        /// <summary>
        /// TCP服务注册无响应超时秒数
        /// </summary>
        public int RegisterTimeoutSeconds
        {
            get { return registerTimeoutSeconds; }
        }
        /// <summary>
        /// TCP注册服务配置
        /// </summary>
        private TcpRegisterPlus()
        {
            pub.LoadConfig(this);
        }
        /// <summary>
        /// 默认TCP注册服务配置
        /// </summary>
        public static readonly tcpRegister Default = new tcpRegister();
    }
}
