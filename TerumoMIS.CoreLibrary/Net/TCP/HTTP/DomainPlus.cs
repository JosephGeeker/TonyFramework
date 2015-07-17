//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DomainPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  DomainPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:33:51
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

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    /// <summary>
    /// 域名信息
    /// </summary>
    public sealed class DomainPlus:IEquatable<DomainPlus>
    {
        /// <summary>
        /// 域名
        /// </summary>
        public byte[] Domain;
        /// <summary>
        /// TCP服务端口信息
        /// </summary>
        public host Host;
        /// <summary>
        /// HASH值
        /// </summary>
        private int hashCode;
        /// <summary>
        /// 域名是否全名,否则表示泛域名后缀
        /// </summary>
        public bool IsFullName = true;
        /// <summary>
        /// 安全证书文件
        /// </summary>
        public string CertificateFileName;
        /// <summary>
        /// 是否仅用于内网IP映射
        /// </summary>
        public bool IsOnlyHost;
        /// <summary>
        /// 域名信息
        /// </summary>
        public domain() { }
        /// <summary>
        /// 域名信息
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="host">TCP服务端口信息</param>
        /// <param name="isFullName">域名是否全名,否则表示泛域名后缀</param>
        public domain(string domain, host host, bool isFullName = true)
        {
            Domain = domain.getBytes();
            Host = host;
            IsFullName = isFullName;
        }
        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns>哈希值</returns>
        public override int GetHashCode()
        {
            if (hashCode == 0)
            {
                hashCode = Host.GetHashCode() ^ algorithm.hashCode.GetHashCode(Domain) ^ (IsFullName ? int.MinValue : 0);
                if (hashCode == 0) hashCode = int.MaxValue;
            }
            return hashCode;
        }
        /// <summary>
        /// 判断是否TCP服务端口信息
        /// </summary>
        /// <param name="other">TCP服务端口信息</param>
        /// <returns>是否同一TCP服务端口信息</returns>
        public override bool Equals(object other)
        {
            return Equals((domain)other);
        }
        /// <summary>
        /// 判断是否TCP服务端口信息
        /// </summary>
        /// <param name="other">TCP服务端口信息</param>
        /// <returns>是否同一TCP服务端口信息</returns>
        public bool Equals(domain other)
        {
            return other != null && GetHashCode() == other.GetHashCode()
                && (IsFullName ? other.IsFullName : !other.IsFullName)
                && Domain.equal(other.Domain) && Host.Equals(other.Host);
        }
    }
}
