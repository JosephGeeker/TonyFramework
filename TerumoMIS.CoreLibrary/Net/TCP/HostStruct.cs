//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HostStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP
//	File Name:  HostStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 15:22:34
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Emit;

namespace TerumoMIS.CoreLibrary.Net.TCP
{
    /// <summary>
    /// TCP服务端口信息
    /// </summary>
    [DataSerializePlus(IsReferenceMember = false,IsMemberMap = false)]
    public struct HostStruct:IEquatable<HostStruct>
    {
        /// <summary>
        /// 主机名称或者IP地址
        /// </summary>
        public string Host;
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port;
        /// <summary>
        /// 主机名称转换成IP地址
        /// </summary>
        /// <returns>是否转换成功</returns>
        public bool HostToIpAddress()
        {
            IPAddress ipAddress = fastCSharp.code.cSharp.tcpBase.HostToIpAddress(Host);
            if (ipAddress == null) return false;
            Host = ipAddress.ToString();
            return true;
        }
        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns>哈希值</returns>
        public override int GetHashCode()
        {
            return Host == null ? Port : (Host.GetHashCode() ^ Port);
        }
        /// <summary>
        /// 判断是否TCP服务端口信息
        /// </summary>
        /// <param name="other">TCP服务端口信息</param>
        /// <returns>是否同一TCP服务端口信息</returns>
        public override bool Equals(object other)
        {
            return Equals((host)other);
            //return other != null && other.GetType() == typeof(host) && Equals((host)other);
        }
        /// <summary>
        /// 判断是否TCP服务端口信息
        /// </summary>
        /// <param name="other">TCP服务端口信息</param>
        /// <returns>是否同一TCP服务端口信息</returns>
        public bool Equals(host other)
        {
            return Host == other.Host && Port == other.Port;
        }
    }
}
