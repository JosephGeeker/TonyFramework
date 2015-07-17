//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: WebPagePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  WebPagePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:37:03
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

namespace TerumoMIS.CoreLibrary.Code.Csharper
{
    /// <summary>
    /// Web页面
    /// </summary>
    public abstract class WebPagePlus:IgnoreMemberPlus
    {
        /// <summary>
        /// WEB页面
        /// </summary>
        public interface IWebPage
        {
            /// <summary>
            /// HTTP套接字接口设置
            /// </summary>
            socketBase Socket { set; }
            /// <summary>
            /// 域名服务设置
            /// </summary>
            domainServer DomainServer { set; }
            /// <summary>
            /// 根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            string GetSaveFileName(requestForm.value value);
            /// <summary>
            /// 套接字请求编号
            /// </summary>
            long SocketIdentity { get; }
        }
        /// <summary>
        /// WEB页面
        /// </summary>
        public abstract class page : IDisposable
        {
            /// <summary>
            /// 缓存标识处理
            /// </summary>
            public unsafe struct eTag
            {
                /// <summary>
                /// 起始字符
                /// </summary>
                private const uint startChar = ':' + 1;
                /// <summary>
                /// WEB页面
                /// </summary>
                private page page;
                /// <summary>
                /// 客户端缓存有效标识
                /// </summary>
                public subArray<byte> IfNoneMatch
                {
                    get { return page.requestHeader.IfNoneMatch; }
                }
                /// <summary>
                /// 当前数据位置
                /// </summary>
                private byte* Data;
                /// <summary>
                /// 缓存标识长度
                /// </summary>
                private int length;
                /// <summary>
                /// 判断长度是否匹配
                /// </summary>
                public bool IsLength
                {
                    get { return IfNoneMatch.Count == length; }
                }
                /// <summary>
                /// 是否处理网站配置版本
                /// </summary>
                private bool isServerVersion;
                /// <summary>
                /// 是否处理页面版本
                /// </summary>
                private bool isPageVersion;
                /// <summary>
                /// 缓存标识处理
                /// </summary>
                /// <param name="page">WEB页面</param>
                public eTag(page page) : this(page, true, true) { }
                /// <summary>
                /// 缓存标识处理
                /// </summary>
                /// <param name="page">WEB页面</param>
                /// <param name="isPageVersion">是否处理页面版本</param>
                /// <param name="isServerVersion">是否处理网站配置版本</param>
                public eTag(page page, bool isPageVersion, bool isServerVersion)
                {
                    this.page = page;
                    this.isServerVersion = isServerVersion;
                    this.isPageVersion = isPageVersion;
                    length = 0;
                    Data = null;
                    if (isServerVersion) AddLength(0);
                    if (isPageVersion) AddLength(0);
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(DateTime value)
                {
                    length += 12;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(ulong value)
                {
                    length += 12;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(long value)
                {
                    length += 12;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(uint value)
                {
                    length += 6;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(int value)
                {
                    length += 6;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(ushort value)
                {
                    length += 3;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(short value)
                {
                    length += 3;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(char value)
                {
                    length += 3;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(byte value)
                {
                    length += 2;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(sbyte value)
                {
                    length += 2;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(bool value)
                {
                    ++length;
                }
                /// <summary>
                /// 添加长度
                /// </summary>
                /// <param name="value">数据</param>
                public void AddLength(string value)
                {
                    if (value != null) length += (value.Length << 1) + value.Length;
                }
                /// <summary>
                /// 设置检测数据
                /// </summary>
                /// <param name="data">检测数据</param>
                public void SetCheckData(byte* data)
                {
                    this.Data = data;
                    if (isServerVersion) Check(page.DomainServer.WebConfig.ETagVersion);
                    if (isPageVersion) Check(page.ETagVersion);
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(DateTime value)
                {
                    Check((ulong)value.Ticks);
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(long value)
                {
                    Check((ulong)value);
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(ulong value)
                {
                    Check((uint)value);
                    Check((uint)(value >> 32));
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(int value)
                {
                    Check((uint)value);
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(uint value)
                {
                    if (Data != null)
                    {
                        if (*Data == (value & 0x3fU) + startChar
                            && Data[1] == ((value >> 6) & 0x3fU) + startChar
                            && Data[2] == ((value >> 12) & 0x3fU) + startChar
                            && Data[3] == ((value >> 18) & 0x3fU) + startChar
                            && Data[4] == ((value >> 24) & 0x3fU) + startChar
                            && Data[5] == (value >> 30) + startChar)
                        {
                            Data += 6;
                        }
                        else Data = null;
                    }
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(short value)
                {
                    Check((ushort)value);
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(char value)
                {
                    Check((ushort)value);
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(ushort value)
                {
                    if (Data != null)
                    {
                        if (*Data == (value & 0x3fU) + startChar
                            && Data[1] == ((value >> 6) & 0x3fU) + startChar
                            && Data[2] == (value >> 12) + startChar)
                        {
                            Data += 3;
                        }
                        else Data = null;
                    }
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(sbyte value)
                {
                    Check((byte)value);
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(byte value)
                {
                    if (Data != null)
                    {
                        if (*Data == (value & 0x3fU) + startChar
                            && Data[1] == (value >> 6) + startChar)
                        {
                            Data += 2;
                        }
                        else Data = null;
                    }
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(bool value)
                {
                    if (Data != null)
                    {
                        if (*Data == (value ? (byte)'1' : (byte)'0')) ++Data;
                        else Data = null;
                    }
                }
                /// <summary>
                /// 检测数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Check(string value)
                {
                    if (Data != null && value != null)
                    {
                        foreach (char code in value)
                        {
                            Check((ushort)code);
                            if (Data == null) break;
                        }
                    }
                }
                /// <summary>
                /// 检测数据结束
                /// </summary>
                /// <returns>当前数据位置</returns>
                public byte* Check()
                {
                    if (Data != null) page.notChanged304();
                    return Data;
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="response">HTTP响应输出</param>
                /// <returns>缓存数据</returns>
                public byte[] Set()
                {
                    byte[] eTag = new byte[length];
                    page.Response.SetETag(eTag);
                    return eTag;
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="data">数据</param>
                public void SetData(byte* data)
                {
                    this.Data = data;
                    if (isServerVersion) Set(page.DomainServer.WebConfig.ETagVersion);
                    if (isPageVersion) Set(page.ETagVersion);
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(DateTime value)
                {
                    Set((ulong)value.Ticks);
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(long value)
                {
                    Set((ulong)value);
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(ulong value)
                {
                    Set((uint)value);
                    Set((uint)(value >> 32));
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(int value)
                {
                    Set((uint)value);
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(uint value)
                {
                    *Data = (byte)((value & 0x3fU) + startChar);
                    Data[1] = (byte)(((value >> 6) & 0x3fU) + startChar);
                    Data[2] = (byte)(((value >> 12) & 0x3fU) + startChar);
                    Data[3] = (byte)(((value >> 18) & 0x3fU) + startChar);
                    Data[4] = (byte)(((value >> 24) & 0x3fU) + startChar);
                    Data[5] = (byte)((value >> 30) + startChar);
                    Data += 6;
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(short value)
                {
                    Set((ushort)value);
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(char value)
                {
                    Set((ushort)value);
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(ushort value)
                {
                    *Data = (byte)((value & 0x3fU) + startChar);
                    Data[1] = (byte)(((value >> 6) & 0x3fU) + startChar);
                    Data[2] = (byte)((value >> 12) + startChar);
                    Data += 3;
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(sbyte value)
                {
                    Set((byte)value);
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(byte value)
                {
                    *Data = (byte)((value & 0x3fU) + startChar);
                    Data[1] = (byte)((value >> 6) + startChar);
                    Data += 2;
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(bool value)
                {
                    *Data++ = value ? (byte)'1' : (byte)'0';
                }
                /// <summary>
                /// 设置数据
                /// </summary>
                /// <param name="value">数据</param>
                public void Set(string value)
                {
                    if (value != null)
                    {
                        foreach (char code in value) Set((ushort)code);
                    }
                }
            }
            /// <summary>
            /// 默认重定向路径
            /// </summary>
            private static readonly byte[] locationPath = new byte[] { (byte)'/' };
            /// <summary>
            /// Session名称
            /// </summary>
            private static readonly byte[] sessionName = fastCSharp.config.http.Default.SessionName.getBytes();
            /// <summary>
            /// HTTP套接字接口设置
            /// </summary>
            private socketBase socket;
            /// <summary>
            /// HTTP套接字接口设置
            /// </summary>
            public socketBase Socket
            {
                internal get { return socket; }
                set
                {
                    if (socket == null) socket = value;
                    else log.Error.Throw(log.exceptionType.ErrorOperation);
                }
            }
            /// <summary>
            /// 远程终结点
            /// </summary>
            public EndPoint RemoteEndPoint
            {
                get { return socket.RemoteEndPoint; }
            }
            /// <summary>
            /// 域名服务
            /// </summary>
            private domainServer domainServer;
            /// <summary>
            /// 域名服务
            /// </summary>
            public domainServer DomainServer
            {
                internal get { return domainServer; }
                set
                {
                    if (domainServer == null) domainServer = value;
                    else log.Error.Throw(log.exceptionType.ErrorOperation);
                }
            }
            /// <summary>
            /// 域名服务工作文件路径
            /// </summary>
            public string WorkPath { get { return domainServer.WorkPath; } }
            /// <summary>
            /// HTTP请求头部
            /// </summary>
            protected requestHeader requestHeader;
            /// <summary>
            /// 输出编码
            /// </summary>
            protected Encoding responseEncoding;
            /// <summary>
            /// 套接字请求编号
            /// </summary>
            public long SocketIdentity { get; protected internal set; }
            /// <summary>
            /// HTTP请求表单
            /// </summary>
            protected internal requestForm form;
            /// <summary>
            /// 会话标识
            /// </summary>
            private uint128 sessionId;
            /// <summary>
            /// HTTP响应输出
            /// </summary>
            public response Response;
            /// <summary>
            /// HTTP响应输出标识(用于终止同步ajax输出)
            /// </summary>
            public int ResponseIdentity { get; private set; }
            /// <summary>
            /// 是否异步调用
            /// </summary>
            public bool IsAsynchronous { get; internal set; }
            /// <summary>
            /// 异步调用标识
            /// </summary>
            protected int asynchronousIdentity;
            /// <summary>
            /// 客户端缓存版本号
            /// </summary>
            public virtual int ETagVersion
            {
                get { return 0; }
            }
            /// <summary>
            /// 是否支持压缩
            /// </summary>
            protected virtual bool isGZip { get { return true; } }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                Response = null;
                requestHeader requestHeader = this.requestHeader;
                this.requestHeader = null;
                socketBase socket = this.socket;
                this.socket = null;
                if (socket != null && requestHeader != null)
                {
                    socket.ResponseError(SocketIdentity, net.tcp.http.response.state.ServerError500);
                }
            }
            /// <summary>
            /// HTTP请求头部处理
            /// </summary>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头部</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>是否成功</returns>
            internal virtual bool LoadHeader(long socketIdentity, fastCSharp.net.tcp.http.requestHeader request, bool isPool) { return false; }
            /// <summary>
            /// 加载查询参数
            /// </summary>
            /// <param name="request">HTTP请求表单</param>
            /// <param name="isAjax">是否ajax请求</param>
            /// <returns>是否成功</returns>
            internal virtual void Load(fastCSharp.net.tcp.http.requestForm form, bool isAjax)
            {
                log.Error.Throw(log.exceptionType.ErrorOperation);
            }
            /// <summary>
            /// 清除当前请求数据
            /// </summary>
            protected virtual void clear()
            {
                ++ResponseIdentity;
                requestHeader = null;
                socket = null;
                if (IsAsynchronous)
                {
                    IsAsynchronous = false;
                    ++asynchronousIdentity;
                    fastCSharp.net.tcp.http.response.Push(ref Response);
                }
                else Response = null;
                domainServer = null;
                form = null;
                sessionId.Low = sessionId.High = 0;
            }
            /// <summary>
            /// 设置为异步调用模式(回调中需要cancelAsynchronous)
            /// </summary>
            protected internal void setAsynchronous()
            {
                IsAsynchronous = true;
            }
            /// <summary>
            /// 取消异步调用模式，获取HTTP响应
            /// </summary>
            /// <returns>HTTP响应</returns>
            protected internal response cancelAsynchronous()
            {
                if (IsAsynchronous)
                {
                    ++asynchronousIdentity;
                    IsAsynchronous = false;
                    return Response;
                }
                return null;
            }
            /// <summary>
            /// WEB页面回收
            /// </summary>
            internal abstract void PushPool();
            /// <summary>
            /// 重定向
            /// </summary>
            /// <param name="path">重定向地址</param>
            /// <param name="is302">是否临时重定向</param>
            protected void location(string path, bool is302 = true)
            {
                location(path.length() != 0 ? path.getBytes() : null, is302);
            }
            /// <summary>
            /// 重定向
            /// </summary>
            /// <param name="path">重定向地址</param>
            /// <param name="is302">是否临时重定向</param>
            protected void location(byte[] path, bool is302 = true)
            {
                if (requestHeader != null)
                {
                    response response = Response = response.Copy(Response);
                    response.State = is302 ? response.state.Found302 : response.state.MovedPermanently301;
                    if (path == null) path = locationPath;
                    response.Location.UnsafeSet(path, 0, path.Length);
                    if (socket.Response(SocketIdentity, ref response)) PushPool();
                }
            }
            /// <summary>
            /// 资源未修改
            /// </summary>
            protected void notChanged304()
            {
                if (socket.Response(SocketIdentity, fastCSharp.net.tcp.http.response.NotChanged304)) PushPool();
            }
            /// <summary>
            /// 服务器发生不可预期的错误
            /// </summary>
            protected internal void serverError500()
            {
                if (socket.ResponseError(SocketIdentity, net.tcp.http.response.state.ServerError500)) PushPool();
            }
            /// <summary>
            /// 请求资源不存在
            /// </summary>
            protected void NotFound404()
            {
                if (socket.ResponseError(SocketIdentity, net.tcp.http.response.state.NotFound404)) PushPool();
            }
            /// <summary>
            /// 任意客户端缓存有效标识处理
            /// </summary>
            /// <returns>是否需要继续加载</returns>
            protected bool anyIfNoneMatch()
            {
                if (requestHeader.IfNoneMatch.Count == 0)
                {
                    Response.SetETag(locationPath);
                    return true;
                }
                notChanged304();
                return false;
            }
            /// <summary>
            /// 客户端缓存有效标识匹配处理
            /// </summary>
            /// <param name="eTag">客户端缓存有效标识</param>
            /// <returns>是否需要继续加载</returns>
            protected unsafe bool loadIfNoneMatch(byte[] eTag)
            {
                if (eTag != null)
                {
                    subArray<byte> ifNoneMatch = requestHeader.IfNoneMatch;
                    if (ifNoneMatch.Count == eTag.Length)
                    {
                        fixed (byte* eTagFixed = eTag, ifNoneMatchFixed = ifNoneMatch.Array)
                        {
                            if (unsafer.memory.Equal(eTag, ifNoneMatchFixed + ifNoneMatch.StartIndex, eTag.Length))
                            {
                                notChanged304();
                                return false;
                            }
                        }
                    }
                    Response.SetETag(eTag);
                }
                return true;
            }
            /// <summary>
            /// 输出数据
            /// </summary>
            /// <param name="data"></param>
            protected void response(byte[] data)
            {
                Response.BodyStream.Write(data);
            }
            /// <summary>
            /// 输出数据
            /// </summary>
            /// <param name="data"></param>
            protected unsafe void response(subArray<byte> data)
            {
                Response.BodyStream.Write(data);
            }
            /// <summary>
            /// 输出HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            protected unsafe void response(char html)
            {
                unmanagedStream bodyStream = Response.BodyStream;
                if (responseEncoding.CodePage == Encoding.Unicode.CodePage) bodyStream.Write(html);
                else
                {
                    int count = responseEncoding.GetByteCount(&html, 1);
                    bodyStream.PrepLength(count);
                    responseEncoding.GetBytes(&html, 1, bodyStream.CurrentData, count);
                    bodyStream.Unsafer.AddLength(count);
                }
            }
            /// <summary>
            /// 输出字符串数据
            /// </summary>
            /// <param name="html">字符串</param>
            /// <param name="encoding">字符串编码</param>
            protected unsafe void response(subString value)
            {
                if (value.Length != 0)
                {
                    unmanagedStream bodyStream = Response.BodyStream;
                    fixed (char* valueFixed = value.value)
                    {
                        char* valueStart = valueFixed + value.StartIndex;
                        if (responseEncoding.CodePage == Encoding.Unicode.CodePage)
                        {
                            int count = value.Length << 1;
                            bodyStream.PrepLength(count);
                            unsafer.memory.Copy(valueStart, bodyStream.CurrentData, count);
                            bodyStream.Unsafer.AddLength(count);
                        }
                        else
                        {
                            int count = responseEncoding.GetByteCount(valueStart, value.Length);
                            bodyStream.PrepLength(count);
                            responseEncoding.GetBytes(valueStart, value.Length, bodyStream.CurrentData, count);
                            bodyStream.Unsafer.AddLength(count);
                        }
                    }
                }
            }
            /// <summary>
            /// 输出HTML片段
            /// </summary>
            /// <param name="html">HTML片段</param>
            protected void response(string html)
            {
                this.response((subString)html);
            }
            /// <summary>
            /// 输出结束
            /// </summary>
            /// <param name="response">HTTP响应输出</param>
            /// <returns>是否操作成功</returns>
            protected unsafe bool responseEnd(ref response response)
            {
                long identity = SocketIdentity;
                try
                {
                    byte[] buffer = response.Body.Array;
                    int length = response.BodyStream.Length;
                    if (requestHeader.IsRange && !requestHeader.FormatRange(length))
                    {
                        if (socket.ResponseError(SocketIdentity, response.state.RangeNotSatisfiable416))
                        {
                            PushPool();
                            return true;
                        }
                    }
                    else
                    {
                        if (buffer.Length >= length)
                        {
                            unsafer.memory.Copy(response.BodyStream.Data, buffer, length);
                            response.Body.UnsafeSet(0, length);
                        }
                        else
                        {
                            byte[] data = response.BodyStream.GetArray();
                            response.Body.UnsafeSet(data, 0, data.Length);
                            response.Buffer = buffer;
                        }
                        response.State = response.state.Ok200;
                        if (response.ContentType == null) response.ContentType = DomainServer.HtmlContentType;
                        if (requestHeader.IsGZip && isGZip)
                        {
                            if (isGZip)
                            {
                                if (!requestHeader.IsRange)
                                {
                                    subArray<byte> compressData = response.GetCompress(response.Body, fastCSharp.memoryPool.StreamBuffers);
                                    if (compressData.array != null)
                                    {
                                        Buffer.BlockCopy(compressData.array, 0, response.Body.array, 0, compressData.Count);
                                        response.Body.UnsafeSet(0, compressData.Count);
                                        response.ContentEncoding = response.GZipEncoding;
                                        fastCSharp.memoryPool.StreamBuffers.Push(ref compressData.array);
                                    }
                                }
                            }
                            else requestHeader.IsGZip = false;
                        }
                        response.NoStore();
                        if (socket.Response(identity, ref response))
                        {
                            PushPool();
                            return true;
                        }
                    }
                }
                finally { response.Push(ref response); }
                return false;
            }
            /// <summary>
            /// 获取请求会话标识
            /// </summary>
            /// <returns>请求会话标识</returns>
            private uint128 getSessionId()
            {
                if ((sessionId.Low | sessionId.High) == 0)
                {
                    sessionId = fastCSharp.net.tcp.http.session.FromCookie(requestHeader.GetCookie(sessionName));
                }
                return sessionId;
            }
            /// <summary>
            /// 获取Session值
            /// </summary>
            /// <returns>Session值</returns>
            private object getSession()
            {
                ISession session = socket.Session;
                if (session != null)
                {
                    uint128 sessionId = getSessionId();
                    if (sessionId.Low != 0) return session.Get(sessionId, null);
                }
                return null;
            }
            /// <summary>
            /// 获取Session值
            /// </summary>
            /// <typeparam name="valueType">值类型</typeparam>
            /// <returns>Session值</returns>
            public valueType GetSession<valueType>() where valueType : class
            {
                object value = getSession();
                return value != null ? value as valueType : null;
            }
            /// <summary>
            /// 获取Session值
            /// </summary>
            /// <typeparam name="valueType">值类型</typeparam>
            /// <param name="nullValue">默认空值</param>
            /// <returns>Session值</returns>
            public valueType GetSession<valueType>(valueType nullValue) where valueType : struct
            {
                object value = getSession();
                return value != null ? (valueType)value : nullValue;
            }
            /// <summary>
            /// 设置Session值
            /// </summary>
            /// <param name="value">值</param>
            /// <returns>是否设置成功</returns>
            public bool SetSession(object value)
            {
                ISession session = socket.Session;
                if (session != null)
                {
                    uint128 sessionId = getSessionId(), newSessionId = session.Set(sessionId, value);
                    if (!sessionId.Equals(newSessionId))
                    {
                        Response.Cookies.Add(new cookie(sessionName, newSessionId.ToHex(), DateTime.MinValue, requestHeader.Host, locationPath, false, true));
                        this.sessionId = newSessionId;
                    }
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 删除Session
            /// </summary>
            public void RemoveSession()
            {
                ISession session = socket.Session;
                if (session != null)
                {
                    uint128 sessionId = getSessionId();
                    if (sessionId.Low != 0)
                    {
                        session.Remove(sessionId);
                        Response.Cookies.Add(new cookie(sessionName, nullValue<byte>.Array, pub.MinTime, requestHeader.Host, locationPath, false, true));
                    }
                }
            }
            /// <summary>
            /// 设置Cookie
            /// </summary>
            /// <param name="cookie">Cookie</param>
            public void SetCookie(cookie cookie)
            {
                if (cookie != null && cookie.Name != null) Response.Cookies.Add(cookie);
            }
            /// <summary>
            /// 获取Cookie
            /// </summary>
            /// <param name="name">名称</param>
            /// <returns>值</returns>
            public string GetCookie(string name)
            {
                return requestHeader.GetCookie(name);
            }
            /// <summary>
            /// 获取Cookie
            /// </summary>
            /// <param name="name">名称</param>
            /// <returns>值</returns>
            public string GetCookie(byte[] name)
            {
                return requestHeader.GetCookieString(name);
            }
            /// <summary>
            /// 判断是否存在Cookie值
            /// </summary>
            /// <param name="name">名称</param>
            /// <returns>是否存在Cookie值</returns>
            public bool IsCookie(byte[] name)
            {
                return requestHeader.IsCookie(name);
            }
            ///// <summary>
            ///// 获取查询整数值
            ///// </summary>
            ///// <param name="name"></param>
            ///// <param name="nullValue"></param>
            ///// <returns></returns>
            //public int GetQueryInt(byte[] name, int nullValue)
            //{
            //    return requestHeader.GetQueryInt(name, nullValue);
            //}
            /// <summary>
            /// 设置内容类型
            /// </summary>
            public void JsContentType()
            {
                Response.SetJsContentType(DomainServer);
            }
        }
        /// <summary>
        /// WEB调用函数名称
        /// </summary>
        public string MethodName;
        /// <summary>
        /// 最大接收数据字节数(单位:MB)
        /// </summary>
        public int MaxPostDataSize = fastCSharp.config.http.Default.MaxPostDataSize;
        /// <summary>
        /// 内存流最大字节数(单位:KB)
        /// </summary>
        public int MaxMemoryStreamSize = fastCSharp.config.http.Default.MaxMemoryStreamSize;
        /// <summary>
        /// 是否使用WEB页面池
        /// </summary>
        public bool IsPool;
    }
}
