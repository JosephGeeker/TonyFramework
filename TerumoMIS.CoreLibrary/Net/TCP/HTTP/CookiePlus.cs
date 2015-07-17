//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CookiePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  CookiePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:32:57
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
    /// Http相应Cookie
    /// </summary>
    public sealed partial class CookiePlus
    {
        /// <summary>
        /// 名称
        /// </summary>
        public byte[] Name;
        /// <summary>
        /// 值
        /// </summary>
        public byte[] Value;
        /// <summary>
        /// 有效域名
        /// </summary>
        public subArray<byte> Domain;
        /// <summary>
        /// 有效路径
        /// </summary>
        public byte[] Path;
        /// <summary>
        /// 超时时间
        /// </summary>
        public DateTime Expires = DateTime.MinValue;
        /// <summary>
        /// 是否安全
        /// </summary>
        public bool IsSecure;
        /// <summary>
        /// 是否HTTP Only
        /// </summary>
        public bool IsHttpOnly;
        /// <summary>
        /// HTTP响应Cookie
        /// </summary>
        public cookie() { }
        /// <summary>
        /// HTTP响应Cookie
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        public cookie(string name, string value)
        {
            if (name.length() != 0) Name = name.getBytes();
            if (value.length() != 0) Value = value.getBytes();
        }
        /// <summary>
        /// HTTP响应Cookie
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="domain">有效域名</param>
        /// <param name="path">有效路径</param>
        /// <param name="isSecure">是否安全</param>
        /// <param name="isHttpOnly">是否HTTP Only</param>
        public cookie(string name, string value, string domain, string path, bool isSecure, bool isHttpOnly)
        {
            if (name.length() != 0) Name = name.getBytes();
            if (value.length() != 0) Value = value.getBytes();
            if (domain.length() != 0)
            {
                byte[] data = domain.getBytes();
                Domain = subArray<byte>.Unsafe(data, 0, data.Length);
            }
            if (path.length() != 0) Path = path.getBytes();
            IsSecure = isSecure;
            IsHttpOnly = isHttpOnly;
        }
        /// <summary>
        /// HTTP响应Cookie
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="expires">超时时间,DateTime.MinValue表示忽略</param>
        /// <param name="domain">有效域名</param>
        /// <param name="path">有效路径</param>
        /// <param name="isSecure">是否安全</param>
        /// <param name="isHttpOnly">是否HTTP Only</param>
        public cookie(string name, string value, DateTime expires
            , string domain, string path, bool isSecure, bool isHttpOnly)
            : this(name, value, domain, path, isSecure, isHttpOnly)
        {
            Expires = expires;
        }
        /// <summary>
        /// HTTP响应Cookie
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="expires">超时时间,DateTime.MinValue表示忽略</param>
        /// <param name="domain">有效域名</param>
        /// <param name="path">有效路径</param>
        /// <param name="isSecure">是否安全</param>
        /// <param name="isHttpOnly">是否HTTP Only</param>
        internal CookiePlus(byte[] name, byte[] value, DateTime expires, subArray<byte> domain, byte[] path, bool isSecure, bool isHttpOnly)
        {
            Name = name;
            Value = value;
            Expires = expires;
            Domain = domain;
            Path = path;
            IsSecure = isSecure;
            IsHttpOnly = isHttpOnly;
        }
    }
}
