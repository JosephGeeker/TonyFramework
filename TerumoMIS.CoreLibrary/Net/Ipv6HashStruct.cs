//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: Ipv6HashPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net
//	File Name:  Ipv6HashPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:57:44
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
    /// Ipv6地址哈希
    /// </summary>
    public struct Ipv6HashStruct:IEquatable<Ipv6HashStruct>
    {
        /// <summary>
        /// IPv6地址
        /// </summary>
        private static readonly Func<IPAddress, ushort[]> ipAddress = fastCSharp.emit.pub.GetField<IPAddress, ushort[]>("m_Numbers");
        /// <summary>
        /// IP地址
        /// </summary>
        private ushort[] ip;
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsNull
        {
            get { return ip == null; }
        }
        /// <summary>
        /// IPv6地址哈希隐式转换
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>IPv6地址哈希</returns>
        public static implicit operator ipv6Hash(IPAddress ip)
        {
            return new ipv6Hash { ip = ipAddress(ip) };
        }
        /// <summary>
        /// 设置为空值
        /// </summary>
        internal void Null()
        {
            ip = null;
        }
        /// <summary>
        /// IPv6地址哈希值
        /// </summary>
        /// <returns>哈希值</returns>
        public override unsafe int GetHashCode()
        {
            if (ip != null)
            {
                fixed (ushort* ipFixed = ip)
                {
                    return *(int*)ipFixed ^ *(int*)(ipFixed + 2) ^ *(int*)(ipFixed + 4) ^ *(int*)(ipFixed + 6) ^ random.Hash;
                }
            }
            return 0;
        }
        /// <summary>
        /// IPv6地址哈希是否相等
        /// </summary>
        /// <param name="obj">IPv6地址哈希</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            return Equals((ipv6Hash)obj);
            //return obj != null & obj.GetType() == typeof(ipv6Hash) && Equals((ipv6Hash)obj);
        }
        /// <summary>
        /// IPv6地址哈希是否相等
        /// </summary>
        /// <param name="other">IPv6地址哈希</param>
        /// <returns>是否相等</returns>
        public unsafe bool Equals(ipv6Hash other)
        {
            fixed (ushort* ipFixed = ip, otherFixed = other.ip)
            {
                if (*(int*)ipFixed == *(int*)otherFixed)
                {
                    return ((*(int*)(ipFixed + 2) ^ *(int*)(otherFixed + 2))
                        | (*(int*)(ipFixed + 4) ^ *(int*)(otherFixed + 4))
                        | (*(int*)(ipFixed + 6) ^ *(int*)(otherFixed + 6))) == 0;
                }
            }
            return false;
        }
    }
}
