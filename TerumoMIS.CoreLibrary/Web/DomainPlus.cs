//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DomainPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Web
//	File Name:  DomainPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:14:35
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

namespace TerumoMIS.CoreLibrary.Web
{
    /// <summary>
    /// 域名参数及相关操作
    /// </summary>
    public static class DomainPlus
    {
        /// <summary>
        /// 顶级域名唯一哈希
        /// </summary>
        private struct topDomain : IEquatable<topDomain>
        {
            /// <summary>
            /// 顶级域名
            /// </summary>
            public subArray<byte> Name;
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="name">顶级域名</param>
            /// <returns>顶级域名唯一哈希</returns>
            public static implicit operator topDomain(string name) { return subArray<byte>.Unsafe(name.getBytes(), 0, name.Length); }
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="name">顶级域名</param>
            /// <returns>顶级域名唯一哈希</returns>
            public static implicit operator topDomain(subArray<byte> name) { return new topDomain { Name = name }; }
            /// <summary>
            /// 获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Count < 3) return 0;
                byte[] key = Name.Array;
                uint code = (uint)(key[Name.StartIndex] << 8) + (uint)key[Name.StartIndex + 2];
                return (int)(((code >> 4) ^ code) & ((1U << 5) - 1));
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public unsafe bool Equals(topDomain other)
            {
                return Name.equal(other.Name);
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((topDomain)obj);
            }
        }
        /// <summary>
        /// 顶级域名集合
        /// </summary>
        private static readonly uniqueHashSet<topDomain> topDomains = new uniqueHashSet<topDomain>(new topDomain[] { "arpa", "com", "edu", "gov", "int", "mil", "net", "org", "biz", "name", "info", "pro", "museum", "aero", "coop" }, 30);
        /// <summary>
        /// 域名分解器
        /// </summary>
        private unsafe struct domainParser
        {
            /// <summary>
            /// 域名数据
            /// </summary>
            public byte[] Data;
            /// <summary>
            /// 域名数据
            /// </summary>
            public byte* DataFixed;
            /// <summary>
            /// 域名数据结束
            /// </summary>
            public byte* DataEnd;
            /// <summary>
            /// 根据URL地址获取主域名
            /// </summary>
            /// <param name="start">URL起始位置</param>
            /// <returns>主域名</returns>
            public subArray<byte> GetMainDomainByUrl(byte* start)
            {
                byte* end = DataEnd, domain = unsafer.memory.Find(start, end, (byte)':');
                if (domain != null && *(short*)++domain == '/' + ('/' << 8) && (domain += sizeof(short)) < end)
                {
                    byte* next = unsafer.memory.Find(domain, end, (byte)'/');
                    if (next == null) return getMainDomain(domain, end);
                    if (domain != next) return getMainDomain(domain, next);

                }
                return default(subArray<byte>);
            }
            /// <summary>
            /// 获取主域名
            /// </summary>
            /// <param name="start">域名起始位置</param>
            /// <returns>主域名</returns>
            public subArray<byte> GetMainDomain(byte* start)
            {
                return getMainDomain(start, DataEnd);
            }
            /// <summary>
            /// 获取主域名
            /// </summary>
            /// <param name="domain">域名起始位置</param>
            /// <param name="end">域名结束位置</param>
            /// <returns>主域名</returns>
            private subArray<byte> getMainDomain(byte* domain, byte* end)
            {
                byte* next = unsafer.memory.Find(domain, end, (byte)':');
                if (next != null) end = next;
                if (domain != end)
                {
                    for (next = domain; next != end; ++next)
                    {
                        if (((uint)(*next - '0') >= 10 && *next != '.'))
                        {
                            byte* dot1 = unsafer.memory.FindLast(domain, end, (byte)'.');
                            if (dot1 != null && domain != dot1)
                            {
                                byte* dot2 = unsafer.memory.FindLast(domain, dot1, (byte)'.');
                                if (dot2 != null)
                                {
                                    if (topDomains.Contains(subArray<byte>.Unsafe(Data, (int)(dot1 - DataFixed) + 1, (int)(end - dot1) - 1))
                                        || !topDomains.Contains(subArray<byte>.Unsafe(Data, (int)(dot2 - DataFixed) + 1, (int)(dot1 - dot2) - 1)))
                                    {
                                        domain = dot2 + 1;
                                    }
                                    else if (domain != dot2 && (dot1 = unsafer.memory.FindLast(domain, dot2, (byte)'.')) != null)
                                    {
                                        domain = dot1 + 1;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    return subArray<byte>.Unsafe(Data, (int)(domain - DataFixed), (int)(end - domain));
                }
                return default(subArray<byte>);
            }
        }
        /// <summary>
        /// 根据URL地址获取主域名
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>主域名</returns>
        public unsafe static subArray<byte> GetMainDomainByUrl(subArray<byte> url)
        {
            if (url.Count != 0)
            {
                fixed (byte* urlFixed = url.Array)
                {
                    byte* urlStart = urlFixed + url.StartIndex;
                    return new domainParser { Data = url.Array, DataFixed = urlFixed, DataEnd = urlStart + url.Count }.GetMainDomainByUrl(urlStart);
                }
            }
            return default(subArray<byte>);
        }
        /// <summary>
        /// 根据域名获取主域名
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>主域名</returns>
        public unsafe static subArray<byte> GetMainDomain(subArray<byte> domain)
        {
            if (domain.Count != 0)
            {
                fixed (byte* domainFixed = domain.Array)
                {
                    byte* domainStart = domainFixed + domain.StartIndex;
                    return new domainParser { Data = domain.Array, DataFixed = domainFixed, DataEnd = domainStart + domain.Count }.GetMainDomain(domainStart);
                }
            }
            return default(subArray<byte>);
        }
    }
}
