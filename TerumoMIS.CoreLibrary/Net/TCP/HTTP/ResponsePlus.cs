//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ResponsePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  ResponsePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:38:21
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    /// <summary>
    /// Http响应
    /// </summary>
    public sealed partial class ResponsePlus:IDisposable
    {
        /// <summary>
        /// HTTP响应状态
        /// </summary>
        public sealed class stateInfo : Attribute
        {
            /// <summary>
            /// 编号
            /// </summary>
            public int Number;
            /// <summary>
            /// 状态输出文本
            /// </summary>
            public string Text;
            /// <summary>
            /// 状态输出字节
            /// </summary>
            private byte[] bytes;
            /// <summary>
            /// 状态输出字节
            /// </summary>
            internal byte[] Bytes
            {
                get
                {
                    if (bytes == null) bytes = Text.getBytes();
                    return bytes;
                }
            }
            /// <summary>
            /// 是否错误状态类型
            /// </summary>
            public bool IsError;
        }
        /// <summary>
        /// HTTP状态类型
        /// </summary>
        public enum state
        {
            /// <summary>
            /// 未知状态
            /// </summary>
            Unknown,
            /// <summary>
            /// 允许客户端继续发送数据
            /// </summary>
            [stateInfo(Number = 100, Text = @" 100 Continue
")]
            Continue100,
            /// <summary>
            /// WebSocket握手
            /// </summary>
            [stateInfo(Number = 101, Text = @" 101 Switching Protocols
")]
            WebSocket101,
            /// <summary>
            /// 客户端请求成功
            /// </summary>
            [stateInfo(Number = 200, Text = @" 200 OK
")]
            Ok200,
            /// <summary>
            /// 成功处理了Range头的GET请求
            /// </summary>
            [stateInfo(Number = 206, Text = @" 206 Partial Content
")]
            PartialContent206,
            /// <summary>
            /// 永久重定向
            /// </summary>
            [stateInfo(Number = 301, Text = @" 301 Moved Permanently
")]
            MovedPermanently301,
            /// <summary>
            /// 临时重定向
            /// </summary>
            [stateInfo(Number = 302, Text = @" 302 Found
")]
            Found302,
            /// <summary>
            /// 资源未修改
            /// </summary>
            [stateInfo(Number = 304, Text = @" 304 Not Changed
")]
            NotChanged304,
            /// <summary>
            /// 客户端请求有语法错误，不能被服务器所理解
            /// </summary>
            [stateInfo(IsError = true, Number = 400, Text = @" 400 Bad Request
")]
            BadRequest400,
            /// <summary>
            /// 请求未经授权，这个状态代码必须和WWW-Authenticate报头域一起使用
            /// </summary>
            [stateInfo(IsError = true, Number = 401, Text = @" 401 Unauthorized
")]
            Unauthorized401,
            /// <summary>
            /// 服务器收到请求，但是拒绝提供服务
            /// WWW-Authenticate响应报头域必须被包含在401（未授权的）响应消息中，客户端收到401响应消息时候，并发送Authorization报头域请求服务器对其进行验证时，服务端响应报头就包含该报头域。
            /// eg：WWW-Authenticate:Basic realm="Basic Auth Test!"  可以看出服务器对请求资源采用的是基本验证机制。
            /// </summary>
            [stateInfo(IsError = true, Number = 403, Text = @" 403 Forbidden
")]
            Forbidden403,
            /// <summary>
            /// 请求资源不存在
            /// </summary>
            [stateInfo(IsError = true, Number = 404, Text = @" 404 Not Found
")]
            NotFound404,
            /// <summary>
            /// 不允许使用的方法
            /// </summary>
            [stateInfo(IsError = true, Number = 405, Text = @" 405 Method Not Allowed
")]
            MethodNotAllowed405,
            /// <summary>
            /// Request Timeout
            /// </summary>
            [stateInfo(IsError = true, Number = 408, Text = @" 408 Request Timeout
")]
            RequestTimeout408,
            /// <summary>
            /// Range请求无效
            /// </summary>
            [stateInfo(IsError = true, Number = 416, Text = @" 416 Request Range Not Satisfiable
")]
            RangeNotSatisfiable416,
            /// <summary>
            /// 服务器发生不可预期的错误
            /// </summary>
            [stateInfo(IsError = true, Number = 500, Text = @" 500 Internal Server Error
")]
            ServerError500,
            /// <summary>
            /// 服务器当前不能处理客户端的请求，一段时间后可能恢复正常
            /// </summary>
            [stateInfo(IsError = true, Number = 503, Text = @" 503 Server Unavailable
")]
            ServerUnavailable503,
        }
        /// <summary>
        /// 资源未修改
        /// </summary>
        internal static readonly response NotChanged304 = new response { State = response.state.NotChanged304 };
        /// <summary>
        /// Range请求无效
        /// </summary>
        internal static readonly response RangeNotSatisfiable416 = new response { State = response.state.RangeNotSatisfiable416 };
        /// <summary>
        /// 空页面输出
        /// </summary>
        internal static readonly response Blank = new response
        {
            State = response.state.Ok200,
            CacheControl = ("public, max-age=9999999").getBytes(),
            LastModified = ("Mon, 20 Apr 1981 08:03:16 GMT").getBytes()
        };
        /// <summary>
        /// 默认内容类型头部
        /// </summary>
        internal static readonly byte[] HtmlContentType = ("text/html; charset=" + fastCSharp.config.appSetting.Encoding.WebName).getBytes();
        /// <summary>
        /// 默认内容类型头部
        /// </summary>
        internal static readonly byte[] JsContentType = ("application/x-javascript; charset=" + fastCSharp.config.appSetting.Encoding.WebName).getBytes();
        /// <summary>
        /// ZIP文件输出类型
        /// </summary>
        private static readonly byte[] zipContentType = fastCSharp.web.contentTypeInfo.GetContentType("zip");
        /// <summary>
        /// 文本文件输出类型
        /// </summary>
        private static readonly byte[] textContentType = fastCSharp.web.contentTypeInfo.GetContentType("txt");
        /// <summary>
        /// GZIP压缩响应头部
        /// </summary>
        internal static readonly byte[] GZipEncoding = ("gzip").getBytes();
        /// <summary>
        /// 非缓存参数输出
        /// </summary>
        private static readonly byte[] noStoreBytes = ("public, no-store").getBytes();
        /// <summary>
        /// 缓存过期
        /// </summary>
        private static readonly byte[] zeroAgeBytes = ("public, max-age=0").getBytes();
        /// <summary>
        /// GZIP压缩响应头部字节尺寸
        /// </summary>
        internal static readonly int GZipSize = header.ContentEncoding.Length + GZipEncoding.Length + 2;
        /// <summary>
        /// Cookie集合
        /// </summary>
        internal list<cookie> Cookies = new list<cookie>();
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public byte[] LastModified;
        /// <summary>
        /// 缓存匹配标识
        /// </summary>
        internal byte[] ETag;
        /// <summary>
        /// 输出内容类型
        /// </summary>
        public byte[] ContentType;
        /// <summary>
        /// 输出内容压缩编码
        /// </summary>
        internal byte[] ContentEncoding;
        /// <summary>
        /// 重定向
        /// </summary>
        internal subArray<byte> Location;
        /// <summary>
        /// 缓存参数
        /// </summary>
        public byte[] CacheControl;
        /// <summary>
        /// 内容描述
        /// </summary>
        public byte[] ContentDisposition;
        /// <summary>
        /// 输出内容
        /// </summary>
        internal subArray<byte> Body;
        /// <summary>
        /// 获取包含HTTP头部的输出内容
        /// </summary>
        internal subArray<byte> HeaderBody
        {
            get
            {
                int size = unsafer.memory.GetInt(Body.array);
                return size == 0 ? default(subArray<byte>) : subArray<byte>.Unsafe(Body.array, Body.StartIndex - size, Body.Count + size);
            }
        }
        /// <summary>
        /// 输出内容数组
        /// </summary>
        public byte[] BodyData
        {
            get { return Body.Array; }
        }
        /// <summary>
        /// 输出缓存流
        /// </summary>
        internal unmanagedStream BodyStream;
        /// <summary>
        /// JSON输出流
        /// </summary>
        private charStream jsonStream;
        /// <summary>
        /// 临时缓存区
        /// </summary>
        internal byte[] Buffer;
        /// <summary>
        /// 输出内容重定向文件
        /// </summary>
        private string bodyFile;
        /// <summary>
        /// 输出内容重定向文件
        /// </summary>
        public string BodyFile
        {
            get { return bodyFile; }
            set
            {
                bodyFile = value;
                if (value != null) Body.UnsafeSetLength(0);
            }
        }
        /// <summary>
        /// 输出内容长度
        /// </summary>
        public long BodySize
        {
            get
            {
                if (BodyFile == null) return Body.Count;
                try
                {
                    return new FileInfo(BodyFile).Length;
                }
                catch (Exception error)
                {
                    log.Default.Add(error, BodyFile, false);
                    return 0;
                }
            }
        }
        /// <summary>
        /// HTTP响应状态
        /// </summary>
        public state State;
        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        private int isDisposed;
        /// <summary>
        /// 是否可以覆盖HTTP预留头部
        /// </summary>
        internal bool CanHeader;
        /// <summary>
        /// 输出数据是否一次性(不可重用)
        /// </summary>
        private bool isBodyOnlyOnce;
        /// <summary>
        /// 是否使用HTTP响应池
        /// </summary>
        private bool isPool;
        /// <summary>
        /// HTTP响应
        /// </summary>
        private response() { }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1) Interlocked.Decrement(ref newCount);
            pub.Dispose(ref BodyStream);
        }
        /// <summary>
        /// 清除数据
        /// </summary>
        public void Clear()
        {
            if (isBodyOnlyOnce)
            {
                isBodyOnlyOnce = false;
                Body.UnsafeSet(Buffer, 0, 0);
                Buffer = nullValue<byte>.Array;
            }
            else
            {
                byte[] buffer = Buffer, data = Body.Array;
                if (buffer.Length > data.Length)
                {
                    Body.UnsafeSet(buffer, 0, 0);
                    Buffer = data;
                }
                else Body.UnsafeSetLength(0);
            }
            State = state.ServerError500;
            CanHeader = false;
            bodyFile = null;
            Location.Null();
            LastModified = CacheControl = ContentType = ContentEncoding = ETag = ContentDisposition = null;
            Cookies.Empty();
            BodyStream.Clear();
        }
        /// <summary>
        /// 设置输出数据
        /// </summary>
        /// <param name="data">输出数据</param>
        /// <param name="isBodyData">输出数据是否一次性(不可重用)</param>
        public void SetBody(subArray<byte> data, bool isBodyOnlyOnce = true, bool canHeader = false)
        {
            CanHeader = canHeader;
            if (data.Count == 0) Body.UnsafeSetLength(0);
            else
            {
                if (!this.isBodyOnlyOnce) Buffer = Body.Array;
                Body = data;
                this.isBodyOnlyOnce = isBodyOnlyOnce;
            }
        }
        /// <summary>
        /// 获取JSON序列化输出缓冲区
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe charStream ResetJsonStream(void* data, int size)
        {
            if (jsonStream == null) return jsonStream = new charStream((char*)data, size >> 1);
            jsonStream.Reset((byte*)data, size);
            return jsonStream;
        }
        /// <summary>
        /// 设置非缓存参数输出
        /// </summary>
        internal void NoStore()
        {
            if (LastModified == null && CacheControl == null && ETag == null) CacheControl = noStoreBytes;
        }
        ///// <summary>
        ///// 设置缓存过期
        ///// </summary>
        //public void ZeroAge()
        //{
        //    CacheControl = zeroAgeBytes;
        //}
        /// <summary>
        /// 设置缓存匹配标识
        /// </summary>
        /// <param name="eTag">缓存匹配标识</param>
        public void SetETag(byte[] eTag)
        {
            ETag = eTag;
            if (CacheControl == null) CacheControl = zeroAgeBytes;
        }
        /// <summary>
        /// 设置js内容类型
        /// </summary>
        /// <param name="domainServer">域名服务</param>
        internal void SetJsContentType(domainServer domainServer)
        {
            ContentType = domainServer.JsContentType;
        }
        /// <summary>
        /// 设置zip内容类型
        /// </summary>
        public void SetZipContentType()
        {
            ContentType = zipContentType;
        }
        /// <summary>
        /// 设置文本内容类型
        /// </summary>
        public void SetTextContentType()
        {
            ContentType = textContentType;
        }
        /// <summary>
        /// 获取压缩数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>压缩数据,失败返回null</returns>
        internal static subArray<byte> GetCompress(subArray<byte> data, memoryPool memoryPool = null, int seek = 0)
        {
            if (data.Count > GZipSize)
            {
                subArray<byte> compressData = stream.GZip.GetCompress(data.Array, data.StartIndex, data.Count, seek, memoryPool);
                if (compressData.Count != 0)
                {
                    if (compressData.Count + GZipSize < data.Count) return compressData;
                    if (memoryPool != null) memoryPool.Push(ref compressData.array);
                }
            }
            return default(subArray<byte>);
        }
        /// <summary>
        /// HTTP响应数量
        /// </summary>
        private static int newCount;
        /// <summary>
        /// HTTP响应数量
        /// </summary>
        public static int NewCount
        {
            get { return newCount; }
        }
        /// <summary>
        /// 获取HTTP响应
        /// </summary>
        /// <param name="isPool">是否使用HTTP响应池</param>
        /// <returns>HTTP响应</returns>
        public static response Get(bool isPool = false)
        {
            if (isPool)
            {
                response response = typePool<response>.Pop();
                if (response != null) return response;
                Interlocked.Increment(ref newCount);
                return new response { BodyStream = new unmanagedStream(), isPool = true, Body = subArray<byte>.Unsafe(nullValue<byte>.Array, 0, 0), Buffer = nullValue<byte>.Array };
            }
            return new response { isPool = false, Body = subArray<byte>.Unsafe(nullValue<byte>.Array, 0, 0), Buffer = nullValue<byte>.Array };
        }
        /// <summary>
        /// 复制HTTP响应
        /// </summary>
        /// <param name="response">HTTP响应</param>
        /// <returns>HTTP响应</returns>
        internal static response Copy(response response)
        {
            response value = Get(true);
            if (response != null)
            {
                value.CacheControl = response.CacheControl;
                value.ContentEncoding = response.ContentEncoding;
                value.ContentType = response.ContentType;
                value.ETag = response.ETag;
                value.LastModified = response.LastModified;
                value.ContentDisposition = response.ContentDisposition;
                int count = response.Cookies.Count;
                if (count != 0) value.Cookies.Add(response.Cookies.array, 0, count);
            }
            return value;
        }
        /// <summary>
        /// 添加到HTTP响应池
        /// </summary>
        /// <param name="response">HTTP响应</param>
        internal static void Push(ref response response)
        {
            response value = Interlocked.Exchange(ref response, null);
            if (value != null && value.isPool)
            {
                value.Clear();
                typePool<response>.Push(value);
            }
        }
    }
}
