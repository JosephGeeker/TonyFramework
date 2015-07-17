﻿//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HeaderPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Web
//	File Name:  HeaderPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:16:14
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

namespace TerumoMIS.CoreLibrary.Web
{
    /// <summary>
    ///     Http头部名称数据
    /// </summary>
    public static class HeaderPlus
    {
        /// <summary>
        ///     允许的编码方式
        /// </summary>
        public const string AcceptEncoding = "Accept-Encoding";

        /// <summary>
        ///     允许的请求范围方式
        /// </summary>
        public const string AcceptRanges = "Accept-Ranges";

        /// <summary>
        ///     缓存参数
        /// </summary>
        public const string CacheControl = "Cache-Control";

        /// <summary>
        ///     连接状态
        /// </summary>
        public const string Connection = "Connection";

        /// <summary>
        ///     内容描述名称
        /// </summary>
        public const string ContentDisposition = "Content-Disposition";

        /// <summary>
        ///     压缩编码名称
        /// </summary>
        public const string ContentEncoding = "Content-Encoding";

        /// <summary>
        ///     内容长度名称
        /// </summary>
        public const string ContentLength = "Content-Length";

        /// <summary>
        ///     内容请求范围名称
        /// </summary>
        public const string ContentRange = "Content-Range";

        /// <summary>
        ///     内容类型名称
        /// </summary>
        public const string ContentType = "Content-Type";

        /// <summary>
        ///     Cookie
        /// </summary>
        public const string Cookie = "Cookie";

        /// <summary>
        ///     时间
        /// </summary>
        public const string Date = "Date";

        /// <summary>
        ///     缓存匹配标识
        /// </summary>
        public const string ETag = "ETag";

        /// <summary>
        ///     100 Continue确认名称
        /// </summary>
        public const string Expect = "Expect";

        /// <summary>
        ///     主机名称
        /// </summary>
        public const string Host = "Host";

        /// <summary>
        ///     文档时间修改标识
        /// </summary>
        public const string IfModifiedSince = "If-Modified-Since";

        /// <summary>
        ///     文档匹配标识
        /// </summary>
        public const string IfNoneMatch = "If-None-Match";

        /// <summary>
        ///     最后修改时间
        /// </summary>
        public const string LastModified = "Last-Modified";

        /// <summary>
        ///     重定向
        /// </summary>
        public const string Location = "Location";

        /// <summary>
        ///     来源页面名称
        /// </summary>
        public const string Origin = "Origin";

        /// <summary>
        ///     请求字节范围
        /// </summary>
        public const string Range = "Range";

        /// <summary>
        ///     来源页面名称
        /// </summary>
        public const string Referer = "Referer";

        /// <summary>
        ///     WebSocket确认连接值
        /// </summary>
        public const string SecWebSocketKey = "Sec-WebSocket-Key";

        /// <summary>
        ///     WebSocket来源页面名称
        /// </summary>
        public const string SecWebSocketOrigin = "Sec-WebSocket-Origin";

        /// <summary>
        ///     输出Cookie
        /// </summary>
        public const string SetCookie = "Set-Cookie";

        /// <summary>
        ///     传输编码
        /// </summary>
        public const string TransferEncoding = "Transfer-Encoding";

        /// <summary>
        ///     协议升级支持名称
        /// </summary>
        public const string Upgrade = "Upgrade";

        /// <summary>
        ///     浏览器参数名称
        /// </summary>
        public const string UserAgent = "User-Agent";

        /// <summary>
        ///     转发名称
        /// </summary>
        public const string XProwardedFor = "X-Prowarded-For";

        /// <summary>
        ///     允许的编码方式
        /// </summary>
        internal static readonly byte[] AcceptEncodingBytes = AcceptEncoding.GetBytes();

        /// <summary>
        ///     缓存参数
        /// </summary>
        internal static readonly byte[] CacheControlBytes = CacheControl.GetBytes();

        /// <summary>
        ///     连接状态
        /// </summary>
        public static readonly byte[] ConnectionBytes = Connection.GetBytes();

        /// <summary>
        ///     压缩编码名称
        /// </summary>
        public static readonly byte[] ContentEncodingBytes = ContentEncoding.GetBytes();

        /// <summary>
        ///     内容长度名称
        /// </summary>
        public static readonly byte[] ContentLengthBytes = ContentLength.GetBytes();

        /// <summary>
        ///     内容类型名称
        /// </summary>
        public static readonly byte[] ContentTypeBytes = ContentType.GetBytes();

        /// <summary>
        ///     Cookie
        /// </summary>
        internal static readonly byte[] CookieBytes = Cookie.GetBytes();

        /// <summary>
        ///     100 Continue确认名称
        /// </summary>
        internal static readonly byte[] ExpectBytes = Expect.GetBytes();

        /// <summary>
        ///     主机名称
        /// </summary>
        internal static readonly byte[] HostBytes = Host.GetBytes();

        /// <summary>
        ///     文档时间修改标识
        /// </summary>
        internal static readonly byte[] IfModifiedSinceBytes = IfModifiedSince.GetBytes();

        /// <summary>
        ///     文档匹配标识
        /// </summary>
        internal static readonly byte[] IfNoneMatchBytes = IfNoneMatch.GetBytes();

        /// <summary>
        ///     重定向
        /// </summary>
        internal static readonly byte[] LocationBytes = Location.GetBytes();

        /// <summary>
        ///     来源页面名称
        /// </summary>
        internal static readonly byte[] OriginBytes = Origin.GetBytes();

        /// <summary>
        ///     请求字节范围
        /// </summary>
        internal static readonly byte[] RangeBytes = Range.GetBytes();

        /// <summary>
        ///     来源页面名称
        /// </summary>
        internal static readonly byte[] RefererBytes = Referer.GetBytes();

        /// <summary>
        ///     WebSocket确认连接值
        /// </summary>
        internal static readonly byte[] SecWebSocketKeyBytes = SecWebSocketKey.GetBytes();

        /// <summary>
        ///     WebSocket来源页面名称
        /// </summary>
        internal static readonly byte[] SecWebSocketOriginBytes = SecWebSocketOrigin.GetBytes();

        /// <summary>
        ///     传输编码名称
        /// </summary>
        public static readonly byte[] TransferEncodingBytes = TransferEncoding.GetBytes();

        /// <summary>
        ///     协议升级支持名称
        /// </summary>
        internal static readonly byte[] UpgradeBytes = Upgrade.GetBytes();

        /// <summary>
        ///     浏览器参数名称
        /// </summary>
        internal static readonly byte[] UserAgentBytes = UserAgent.GetBytes();

        /// <summary>
        ///     转发名称
        /// </summary>
        internal static readonly byte[] XProwardedForBytes = XProwardedFor.GetBytes();
    }
}