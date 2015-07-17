//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TcpBasePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  TcpBasePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:46:10
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using fastCSharp.code;

namespace TerumoMIS.CoreLibrary.Code.Csharper
{
    /// <summary>
    /// TCP调用配置基类
    /// </summary>
    public abstract partial class TcpBasePlus:IgnoreMemberPlus
    {
        /// <summary>
        /// 泛型类型服务器端调用类型名称
        /// </summary>
        public const string GenericTypeServerName = "tcpServer";
        /// <summary>
        /// TCP客户端验证接口
        /// </summary>
        public interface ITcpClientVerify
        {
            /// <summary>
            /// TCP客户端验证
            /// </summary>
            /// <param name="socket">TCP调用客户端套接字</param>
            /// <returns>是否通过验证</returns>
            bool Verify(fastCSharp.net.tcp.commandClient.socket socket);
        }
        /// <summary>
        /// TCP客户端验证函数接口(tcpCall)
        /// </summary>
        /// <typeparam name="clientType">TCP客户端类型</typeparam>
        public interface ITcpClientVerifyMethod
        {
            /// <summary>
            /// TCP客户端验证
            /// </summary>
            /// <returns>是否通过验证</returns>
            bool Verify();
        }
        /// <summary>
        /// TCP客户端验证函数接口(tcpServer)
        /// </summary>
        /// <typeparam name="clientType">TCP客户端类型</typeparam>
        public interface ITcpClientVerifyMethod<clientType>
        {
            /// <summary>
            /// TCP客户端验证
            /// </summary>
            /// <param name="client">TCP调用客户端</param>
            /// <returns>是否通过验证</returns>
            bool Verify(clientType client);
        }
        ///// <summary>
        ///// TCP客户端验证
        ///// </summary>
        ///// <typeparam name="clientType">TCP客户端类型</typeparam>
        ///// <param name="client">TCP客户端</param>
        ///// <param name="verify">TCP客户端验证接口</param>
        ///// <returns>验证是否成功</returns>
        //public static bool Verify<clientType>(clientType client, ITcpClientVerifyMethod<clientType> verify)
        //{
        //    if (verify == null) return true;
        //    try
        //    {
        //        return verify.Verify(client);
        //    }
        //    catch (Exception error)
        //    {
        //        log.Default.Add(error, null, false);
        //    }
        //    return false;
        //}
        /// <summary>
        /// TCP服务器端同步验证客户端接口
        /// </summary>
        public interface ITcpVerify : ITcpClientVerify
        {
            /// <summary>
            /// TCP客户端同步验证
            /// </summary>
            /// <param name="socket">同步套接字</param>
            /// <returns>是否通过验证</returns>
            bool Verify(fastCSharp.net.tcp.commandServer.socket socket);
        }
        /// <summary>
        /// 客户端标识
        /// </summary>
        public sealed class client
        {
            /// <summary>
            /// 客户端用户信息
            /// </summary>
            public object UserInfo;
            /// <summary>
            /// HTTP页面
            /// </summary>
            public httpPage HttpPage
            {
                get { return (httpPage)UserInfo; }
            }
        }
        /// <summary>
        /// HTTP页面
        /// </summary>
        public sealed class httpPage : webPage.page
        {
            /// <summary>
            /// HTTP请求头部
            /// </summary>
            public fastCSharp.net.tcp.http.requestHeader RequestHeader
            {
                get { return requestHeader; }
            }
            /// <summary>
            /// HTTP请求表单
            /// </summary>
            public fastCSharp.net.tcp.http.requestForm Form
            {
                get { return form; }
            }
            /// <summary>
            /// WEB页面回收
            /// </summary>
            internal override void PushPool()
            {
                clear();
                typePool<httpPage>.Push(this);
            }
            /// <summary>
            /// 参数反序列化
            /// </summary>
            /// <typeparam name="valueType">参数类型</typeparam>
            /// <param name="value">参数值</param>
            /// <returns>是否成功</returns>
            public unsafe bool DeSerialize<valueType>(ref valueType value)
            {
                if (form != null && form.Json != null)
                {
                    if (form.Json.Length != 0
                        && !fastCSharp.emit.jsonParser.Parse(form.Json, ref value))
                    {
                        return false;
                    }
                }
                else
                {
                    subString queryJson = requestHeader.QueryJson;
                    if (queryJson.Length != 0
                        && !fastCSharp.emit.jsonParser.Parse(queryJson, ref value))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 输出
            /// </summary>
            /// <param name="returnValue">是否调用成功</param>
            /// <returns>是否输出成功</returns>
            public new bool Response(asynchronousMethod.returnValue returnValue)
            {
                socketBase socket = Socket;
                response response = null;
                long identity = SocketIdentity;
                try
                {
                    base.Response = (response = fastCSharp.net.tcp.http.response.Copy(base.Response));
                    base.response(returnValue.IsReturn ? fastCSharp.web.ajax.Object : fastCSharp.web.ajax.Null);
                    if (responseEnd(ref response)) return true;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                finally { response.Push(ref response); }
                if (socket.ResponseError(identity, net.tcp.http.response.state.ServerError500)) PushPool();
                return false;
            }
            /// <summary>
            /// 输出
            /// </summary>
            /// <typeparam name="outputParameter">输出数据类型</typeparam>
            /// <param name="returnValue">输出数据</param>
            /// <returns>是否输出成功</returns>
            public new unsafe bool Response<outputParameter>(asynchronousMethod.returnValue<outputParameter> returnValue)
            {
                socketBase socket = Socket;
                response response = null;
                long identity = SocketIdentity;
                try
                {
                    base.Response = (response = fastCSharp.net.tcp.http.response.Copy(base.Response));
                    if (returnValue.IsReturn)
                    {
                        if (returnValue.Value == null) base.response(fastCSharp.web.ajax.Object);
                        else
                        {
                            pointer buffer = fastCSharp.unmanagedPool.StreamBuffers.Get();
                            try
                            {
                                using (charStream jsonStream = response.ResetJsonStream(buffer.Data, fastCSharp.unmanagedPool.StreamBuffers.Size))
                                {
                                    if (responseEncoding.CodePage == Encoding.Unicode.CodePage)
                                    {
                                        fastCSharp.emit.jsonSerializer.Serialize(returnValue.Value, jsonStream, response.BodyStream, null);
                                    }
                                    else
                                    {
                                        fastCSharp.emit.jsonSerializer.ToJson(returnValue.Value, jsonStream);
                                        base.response(fastCSharp.web.ajax.FormatJavascript(jsonStream));
                                    }
                                }
                            }
                            finally { fastCSharp.unmanagedPool.StreamBuffers.Push(ref buffer); }
                        }
                    }
                    else base.response(fastCSharp.web.ajax.Null);
                    if (responseEnd(ref response)) return true;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                finally { response.Push(ref response); }
                if (socket.ResponseError(identity, net.tcp.http.response.state.ServerError500)) PushPool();
                return false;
            }
            /// <summary>
            /// 获取HTTP页面
            /// </summary>
            /// <param name="socket">HTTP套接字接口设置</param>
            /// <param name="domainServer">域名服务</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="requestHeader">HTTP请求头部信息</param>
            /// <param name="form">HTTP表单</param>
            /// <returns>HTTP页面</returns>
            internal static httpPage Get(fastCSharp.net.tcp.http.socketBase socket, domainServer domainServer
                , long socketIdentity, requestHeader requestHeader, requestForm form)
            {
                httpPage httpPage = typePool<httpPage>.Pop() ?? new httpPage();
                httpPage.Socket = socket;
                httpPage.DomainServer = domainServer;
                httpPage.SocketIdentity = socketIdentity;
                httpPage.requestHeader = requestHeader;
                httpPage.responseEncoding = socket.TcpCommandSocket.HttpEncoding ?? domainServer.ResponseEncoding;
                httpPage.form = form;
                return httpPage;
            }
        }
        /// <summary>
        /// 泛型函数信息
        /// </summary>
        public struct genericMethod : IEquatable<genericMethod>
        {
            /// <summary>
            /// 泛型参数数量
            /// </summary>
            public int ArgumentCount;
            /// <summary>
            /// 函数名称
            /// </summary>
            public string Name;
            /// <summary>
            /// 参数名称集合
            /// </summary>
            public string[] ParameterTypeNames;
            /// <summary>
            /// 哈希值
            /// </summary>
            public int HashCode;
            /// <summary>
            /// 泛型函数信息
            /// </summary>
            /// <param name="method">泛型函数信息</param>
            public genericMethod(MethodInfo method)
            {
                Name = method.Name;
                ArgumentCount = method.GetGenericArguments().Length;
                ParameterTypeNames = parameterInfo.Get(method).getArray(value => value.ParameterRef + value.ParameterType.FullName);
                HashCode = Name.GetHashCode() ^ ArgumentCount;
                setHashCode();
            }
            /// <summary>
            /// 泛型函数信息
            /// </summary>
            /// <param name="name">函数名称</param>
            /// <param name="argumentCount">泛型参数数量</param>
            /// <param name="typeNames">参数名称集合</param>
            public genericMethod(string name, int argumentCount, params string[] typeNames)
            {
                Name = name;
                ArgumentCount = argumentCount;
                ParameterTypeNames = typeNames;
                HashCode = Name.GetHashCode() ^ ArgumentCount;
                setHashCode();
            }
            /// <summary>
            /// 计算哈希值
            /// </summary>
            private void setHashCode()
            {
                foreach (string name in ParameterTypeNames) HashCode ^= name.GetHashCode();
            }
            /// <summary>
            /// 哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return HashCode;
            }
            /// <summary>
            /// 比较是否相等
            /// </summary>
            /// <param name="other">比较对象</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object other)
            {
                return Equals((genericMethod)other);
                //return other != null && other.GetType() == typeof(genericMethod) && Equals((genericMethod)other);
            }
            /// <summary>
            /// 比较是否相等
            /// </summary>
            /// <param name="other">比较对象</param>
            /// <returns>是否相等</returns>
            public bool Equals(genericMethod other)
            {
                if (HashCode == other.HashCode && Name == other.Name
                    && ParameterTypeNames.Length == other.ParameterTypeNames.Length)
                {
                    int index = 0;
                    foreach (string name in other.ParameterTypeNames)
                    {
                        if (ParameterTypeNames[index++] != name) return false;
                    }
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 泛型回调委托
        /// </summary>
        /// <typeparam name="valueType">返回值类型</typeparam>
        private sealed class genericParameterCallback<valueType>
        {
            /// <summary>
            /// 回调处理
            /// </summary>
            private Func<asynchronousMethod.returnValue<object>, bool> callback;
            /// <summary>
            /// 回调处理
            /// </summary>
            /// <param name="value">返回值</param>
            /// <returns></returns>
            private bool onReturn(asynchronousMethod.returnValue<valueType> value)
            {
                return callback(new asynchronousMethod.returnValue<object> { IsReturn = value.IsReturn, Value = value.IsReturn ? (object)value.Value : null });
            }
            /// <summary>
            /// 泛型回调委托
            /// </summary>
            /// <param name="callback">回调处理</param>
            /// <returns>泛型回调委托</returns>
            public static Func<asynchronousMethod.returnValue<valueType>, bool> Create(Func<asynchronousMethod.returnValue<object>, bool> callback)
            {
                return new genericParameterCallback<valueType> { callback = callback }.onReturn;
            }
        }
        /// <summary>
        /// 负载均衡回调
        /// </summary>
        /// <typeparam name="returnType">返回值类型</typeparam>
        public abstract class loadBalancingCallback<returnType>
        {
            /// <summary>
            /// 回调委托
            /// </summary>
            protected Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>> _onReturn_;
            /// <summary>
            /// 回调委托
            /// </summary>
            protected Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>> _onReturnHandle_;
            /// <summary>
            /// 错误尝试次数
            /// </summary>
            protected int _tryCount_;
            /// <summary>
            /// 负载均衡回调
            /// </summary>
            protected loadBalancingCallback()
            {
                _onReturnHandle_ = onReturnValue;
            }
            /// <summary>
            /// TCP客户端回调
            /// </summary>
            /// <param name="returnValue">返回值</param>
            private void onReturnValue(fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType> returnValue)
            {
                if (returnValue.IsReturn || --_tryCount_ <= 0) _push_(returnValue);
                else
                {
                    System.Threading.Thread.Sleep(1);
                    _call_();
                }
            }
            /// <summary>
            /// TCP客户端调用
            /// </summary>
            protected abstract void _call_();
            /// <summary>
            /// 添加到回调池负载均衡回调
            /// </summary>
            /// <param name="returnValue">返回值</param>
            protected void _push_(fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType> returnValue)
            {
                Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>> onReturn = this._onReturn_;
                this._onReturn_ = null;
                _push_(returnValue.IsReturn);
                onReturn(returnValue);
            }
            /// <summary>
            /// 添加到回调池负载均衡回调
            /// </summary>
            /// <param name="isReturn">是否回调成功</param>
            protected abstract void _push_(bool isReturn);
        }
        /// <summary>
        /// 负载均衡回调
        /// </summary>
        public abstract class loadBalancingCallback
        {
            /// <summary>
            /// 回调委托
            /// </summary>
            protected Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue> _onReturn_;
            /// <summary>
            /// 回调委托
            /// </summary>
            protected Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue> _onReturnHandle_;
            /// <summary>
            /// 错误尝试次数
            /// </summary>
            protected int _tryCount_;
            /// <summary>
            /// 负载均衡回调
            /// </summary>
            protected loadBalancingCallback()
            {
                _onReturnHandle_ = onReturnValue;
            }
            /// <summary>
            /// TCP客户端回调
            /// </summary>
            /// <param name="returnValue">返回值</param>
            private void onReturnValue(fastCSharp.code.cSharp.asynchronousMethod.returnValue returnValue)
            {
                if (returnValue.IsReturn || --_tryCount_ <= 0) _push_(returnValue);
                else
                {
                    System.Threading.Thread.Sleep(1);
                    _call_();
                }
            }
            /// <summary>
            /// TCP客户端调用
            /// </summary>
            protected abstract void _call_();
            /// <summary>
            /// 添加到回调池负载均衡回调
            /// </summary>
            /// <param name="returnValue">返回值</param>
            protected void _push_(fastCSharp.code.cSharp.asynchronousMethod.returnValue returnValue)
            {
                Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue> onReturn = this._onReturn_;
                this._onReturn_ = null;
                _push_(returnValue.IsReturn);
                onReturn(returnValue);
            }
            /// <summary>
            /// 添加到回调池负载均衡回调
            /// </summary>
            /// <param name="isReturn">是否回调成功</param>
            protected abstract void _push_(bool isReturn);
        }
        /// <summary>
        /// 命令序号记忆数据
        /// </summary>
        public struct rememberIdentityCommeand
        {
            /// <summary>
            /// 命令序号集合
            /// </summary>
            public int[] Identitys;
            /// <summary>
            /// 命令名称集合
            /// </summary>
            public string[] Names;
        }
        /// <summary>
        /// 参数序列化标识
        /// </summary>
        private const uint parameterSerializeValue = 0x10030000;
        /// <summary>
        /// TCP参数流
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct tcpStream
        {
            /// <summary>
            /// 客户端索引
            /// </summary>
            public int ClientIndex;
            /// <summary>
            /// 客户端序号
            /// </summary>
            public int ClientIdentity;
            /// <summary>
            /// 否支持读取
            /// </summary>
            public bool CanRead;
            /// <summary>
            /// 否支持查找
            /// </summary>
            public bool CanSeek;
            /// <summary>
            /// 是否可以超时
            /// </summary>
            public bool CanTimeout;
            /// <summary>
            /// 否支持写入
            /// </summary>
            public bool CanWrite;
            /// <summary>
            /// 是否有效
            /// </summary>
            public bool IsStream;
        }
        /// <summary>
        /// 字节数组缓冲区反序列化事件
        /// </summary>
        public struct subByteArrayEvent
        {
            /// <summary>
            /// 字节数组缓冲区
            /// </summary>
            public subArray<byte> Buffer;
            /// <summary>
            /// 反序列化事件
            /// </summary>
            public Action<subArray<byte>> Event;
            /// <summary>
            /// 反序列化事件
            /// </summary>
            private void callEvent()
            {
                if (Event != null) Event(Buffer);
            }
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="buffer">字节数组缓冲区</param>
            /// <returns>内存数据流</returns>
            public static implicit operator subByteArrayEvent(subArray<byte> buffer) { return new subByteArrayEvent { Buffer = buffer }; }
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="deSerializer">序列化数据</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static void deSerialize(fastCSharp.emit.dataDeSerializer deSerializer, ref subByteArrayEvent value)
            {
                deSerializer.DeSerialize(ref value.Buffer);
                value.callEvent();
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static void serialize(fastCSharp.emit.dataSerializer serializer, subByteArrayEvent value)
            {
                if (value.Buffer.array == null) serializer.Stream.Write(fastCSharp.emit.binarySerializer.NullValue);
                else fastCSharp.emit.binarySerializer.Serialize(serializer.Stream, value.Buffer);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="toJsoner"></param>
            [fastCSharp.emit.jsonSerialize.custom]
            private static void toJson(fastCSharp.emit.jsonSerializer toJsoner, subByteArrayEvent value)
            {
                using (unmanagedStream dataStream = new unmanagedStream(toJsoner.JsonStream))
                {
                    try
                    {
                        if (value.Buffer.array == null) dataStream.Write(fastCSharp.emit.binarySerializer.NullValue);
                        else fastCSharp.emit.binarySerializer.Serialize(dataStream, value.Buffer);
                    }
                    finally { toJsoner.JsonStream.From(dataStream); }
                }
            }
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">参数</param>
            [fastCSharp.emit.jsonParse.custom]
            private unsafe static void parseJson(fastCSharp.emit.jsonParser parser, ref subByteArrayEvent value)
            {
                byte* read = emit.binaryDeSerializer.DeSerialize((byte*)parser.Current, (byte*)parser.End, parser.Buffer, ref value.Buffer);
                if (read == null) parser.Error(emit.jsonParser.parseState.CrashEnd);
                else
                {
                    parser.Current = (char*)read;
                    value.callEvent();
                }
            }
        }
        /// <summary>
        /// 字节数组缓冲区(反序列化数据必须立即使用,否则可能脏数据)
        /// </summary>
        public struct subByteArrayBuffer
        {
            /// <summary>
            /// 字节数组缓冲区
            /// </summary>
            public subArray<byte> Buffer;
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="buffer">字节数组缓冲区</param>
            /// <returns>内存数据流</returns>
            public static implicit operator subByteArrayBuffer(subArray<byte> buffer) { return new subByteArrayBuffer { Buffer = buffer }; }
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="buffer">内存数据流</param>
            /// <returns>字节数组缓冲区</returns>
            public static implicit operator subArray<byte>(subByteArrayBuffer buffer) { return buffer.Buffer; }
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="deSerializer">序列化数据</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static void deSerialize(fastCSharp.emit.dataDeSerializer deSerializer, ref subByteArrayBuffer value)
            {
                deSerializer.DeSerialize(ref value.Buffer);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static void serialize(fastCSharp.emit.dataSerializer serializer, subByteArrayBuffer value)
            {
                if (value.Buffer.array == null) serializer.Stream.Write(fastCSharp.emit.binarySerializer.NullValue);
                else fastCSharp.emit.binarySerializer.Serialize(serializer.Stream, value.Buffer);
            }
            /// <summary>
            /// 对象转换成JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换成JSON字符串</param>
            /// <param name="value">参数</param>
            [fastCSharp.emit.jsonSerialize.custom]
            private static void toJson(fastCSharp.emit.jsonSerializer toJsoner, subByteArrayBuffer value)
            {
                fastCSharp.unsafer.memory.ToJson(toJsoner.JsonStream, value.Buffer);
            }
            /// <summary>
            /// 对象转换成JSON字符串
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">参数</param>
            [fastCSharp.emit.jsonParse.custom]
            private unsafe static void parseJson(fastCSharp.emit.jsonParser parser, ref subByteArrayBuffer value)
            {
                byte[] buffer = null;
                fastCSharp.emit.jsonParser.typeParser<byte>.Array(parser, ref buffer);
                if (buffer == null) value.Buffer.Null();
                else value.Buffer.UnsafeSet(buffer, 0, buffer.Length);
            }
        }
        /// <summary>
        /// 内存数据流(序列化输入流,反序列化字节数组)(反序列化数据必须立即使用,否则可能脏数据)
        /// </summary>
        public struct subByteUnmanagedStream
        {
            /// <summary>
            /// 内存数据流
            /// </summary>
            public unmanagedStream Stream;
            /// <summary>
            /// 内存数据流
            /// </summary>
            public subArray<byte> Buffer;
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="stream">内存数据流</param>
            /// <returns>内存数据流</returns>
            public static implicit operator subByteUnmanagedStream(unmanagedStream stream) { return new subByteUnmanagedStream { Stream = stream }; }
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="stream">内存数据流</param>
            /// <returns>内存数据流</returns>
            public static implicit operator subArray<byte>(subByteUnmanagedStream stream) { return stream.Buffer; }
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="deSerializer">序列化数据</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static void deSerialize(fastCSharp.emit.dataDeSerializer deSerializer, ref subByteUnmanagedStream value)
            {
                deSerializer.DeSerialize(ref value.Buffer);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static unsafe void serialize(fastCSharp.emit.dataSerializer serializer, subByteUnmanagedStream value)
            {
                if (value.Stream == null) serializer.Stream.Write(fastCSharp.emit.binarySerializer.NullValue);
                else
                {
                    unmanagedStream stream = serializer.Stream;
                    int streamLength = value.Stream.Length, length = ((streamLength + 3) & (int.MaxValue - 3));
                    stream.PrepLength(length + sizeof(int));
                    byte* data = stream.CurrentData;
                    *(int*)data = streamLength;
                    unsafer.memory.Copy(value.Stream.Data, data + sizeof(int), length);
                    stream.Unsafer.AddLength(length + sizeof(int));
                }
            }
            /// <summary>
            /// 对象转换成JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换成JSON字符串</param>
            /// <param name="value">参数</param>
            [fastCSharp.emit.jsonSerialize.custom]
            private static void toJson(fastCSharp.emit.jsonSerializer toJsoner, subByteUnmanagedStream value)
            {
                fastCSharp.unsafer.memory.ToJson(toJsoner.JsonStream, value.Buffer);
            }
            /// <summary>
            /// 对象转换成JSON字符串
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">参数</param>
            [fastCSharp.emit.jsonParse.custom]
            private unsafe static void parseJson(fastCSharp.emit.jsonParser parser, ref subByteUnmanagedStream value)
            {
                byte[] buffer = null;
                fastCSharp.emit.jsonParser.typeParser<byte>.Array(parser, ref buffer);
                if (buffer == null) value.Buffer.Null();
                else value.Buffer.UnsafeSet(buffer, 0, buffer.Length);
            }
        }
        /// <summary>
        /// JSON参数
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        public struct parameterJsonToSerialize<valueType>
        {
            /// <summary>
            /// 参数值
            /// </summary>
            public valueType Return;
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static unsafe void deSerialize(fastCSharp.emit.dataDeSerializer deSerializer, ref parameterJsonToSerialize<valueType> value)
            {
                int length = *(int*)deSerializer.Read;
                if (length == fastCSharp.emit.binarySerializer.NullValue)
                {
                    value.Return = default(valueType);
                    return;
                }
                if ((length & 1) == 0 && length > 0)
                {
                    byte* start = deSerializer.Read;
                    if (deSerializer.VerifyRead((length + (2 + sizeof(int))) & (int.MaxValue - 3))
                        && fastCSharp.emit.jsonParser.Parse((char*)(start + sizeof(int)), length >> 1, ref value.Return, null, deSerializer.Buffer))
                    {
                        return;
                    }
                }
                deSerializer.Error(emit.binaryDeSerializer.deSerializeState.IndexOutOfRange);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static unsafe void serialize(fastCSharp.emit.dataSerializer serializer, parameterJsonToSerialize<valueType> value)
            {
                if (value.Return == null) serializer.Stream.Write(fastCSharp.emit.binarySerializer.NullValue);
                else
                {
                    pointer buffer = fastCSharp.unmanagedPool.StreamBuffers.Get();
                    try
                    {
                        using (charStream jsonStream = serializer.ResetJsonStream(buffer.Data, fastCSharp.unmanagedPool.StreamBuffers.Size))
                        {
                            fastCSharp.emit.jsonSerializer.Serialize(value.Return, jsonStream, serializer.Stream, null);
                        }
                    }
                    finally { fastCSharp.unmanagedPool.StreamBuffers.Push(ref buffer); }
                }
            }
        }
        /// <summary>
        /// JSON反序列化
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <param name="value">目标对象</param>
        /// <param name="data">序列化数据</param>
        /// <returns>是否成功</returns>
        public static bool JsonDeSerialize<valueType>(ref valueType value, subArray<byte> data)
        {
            parameterJsonToSerialize<valueType> json = new parameterJsonToSerialize<valueType> { Return = value };
            if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref json))
            {
                value = json.Return;
                return true;
            }
            return false;
        }
        /// <summary>
        /// JSON转换序列化
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <param name="value">目标对象</param>
        /// <returns>序列化对象</returns>
        internal static asynchronousMethod.returnValue<parameterJsonToSerialize<valueType>> JsonToSerialize<valueType>(asynchronousMethod.returnValue<valueType> value)
        {
            if (value.IsReturn) return new parameterJsonToSerialize<valueType> { Return = value.Value };
            return new asynchronousMethod.returnValue<parameterJsonToSerialize<valueType>> { IsReturn = false };
        }
        static tcpBase()
        {
            copyFields = typeof(tcpBase).GetFields(BindingFlags.Instance | BindingFlags.Public)
                .getFindArray(field => field.Name != "Service" && field.Name != "IsAttribute" && field.Name != "IsBaseTypeAttribute" && field.Name != "IsInheritAttribute");
        }
        /// <summary>
        /// 成员是否匹配自定义属性类型(有效域为class)
        /// </summary>
        public bool IsAttribute = true;
        /// <summary>
        /// 是否搜索父类自定义属性(有效域为class)
        /// </summary>
        public bool IsBaseTypeAttribute;
        /// <summary>
        /// 成员匹配自定义属性是否可继承(有效域为class)
        /// </summary>
        public bool IsInheritAttribute = true;
        /// <summary>
        /// 服务名称(服务配置)
        /// </summary>
        public string Service;
        /// <summary>
        /// TCP注册服务名称(服务配置)
        /// </summary>
        public string TcpRegister;
        /// <summary>
        /// TCP注册服务名称
        /// </summary>
        public virtual string TcpRegisterName
        {
            get { return TcpRegister; }
        }
        /// <summary>
        /// 是否只允许一个TCP服务实例(服务配置)
        /// </summary>
        public bool IsSingleRegister = true;
        /// <summary>
        /// 是否预申请TCP服务实例(服务配置)
        /// </summary>
        public bool IsPerpleRegister = true;
        /// <summary>
        /// 验证类(服务配置),必须继承接口fastCSharp.code.cSharp.tcpBase.ITcpVerify或fastCSharp.code.cSharp.tcpBase.ITcpVerifyAsynchronous
        /// </summary>
        public Type VerifyType;
        /// <summary>
        /// 客户端验证方法类(服务配置),必须继承接口fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<>
        /// </summary>
        public Type VerifyMethodType;
        /// <summary>
        /// 主机名称或者IP地址(服务配置)
        /// </summary>
        public string Host;
        /// <summary>
        /// 主机名称或者IP地址(服务配置)
        /// </summary>
        private string host;
        /// <summary>
        /// IP地址
        /// </summary>
        private IPAddress ipAddress;
        /// <summary>
        /// IP地址
        /// </summary>
        internal IPAddress IpAddress
        {
            get
            {
                if (ipAddress == null || host != Host)
                {
                    ipAddress = HostToIpAddress(host = Host) ?? IPAddress.Any;
                }
                return ipAddress;
            }
        }
        /// <summary>
        /// 监听端口(服务配置)
        /// </summary>
        public int Port;
        /// <summary>
        /// 服务器端是否异步接收数据(服务配置)
        /// </summary>
        public bool IsServerAsynchronousReceive = true;
        /// <summary>
        /// 服务器端函数是否显示异步回调(服务配置),(返回值必须为void，最后一个参数必须为回调委托action(fastCSharp.code.cSharp.methodInfo.asynchronousReturn))
        /// </summary>
        public bool IsServerAsynchronousCallback;
        /// <summary>
        /// 服务器端模拟异步调用是否使用任务池(有效域为方法,对于同步服务器端的验证调用无效)
        /// </summary>
        public bool IsServerAsynchronousTask = true;
        /// <summary>
        /// 客户端是否异步接收数据(服务配置)
        /// </summary>
        public bool IsClientAsynchronousReceive = false;
        /// <summary>
        /// 客户端回调是否使用任务池(有效域为方法)。警告：如果设置为false，在回调中不能调用远程同步方法，否则死锁
        /// </summary>
        public bool IsClientCallbackTask = true;
        /// <summary>
        /// 客户端是否提供同步调用(有效域为方法)
        /// </summary>
        public bool IsClientSynchronous = true;
        /// <summary>
        /// 客户端是否提供异步调用(有效域为方法)
        /// </summary>
        public bool IsClientAsynchronous;
        /// <summary>
        /// 客户端异步回调是否公用输入参数(有效域为方法)
        /// </summary>
        public bool IsClientAsynchronousReturnInputParameter;
        /// <summary>
        /// 是否保持异步回调(有效域为方法)
        /// </summary>
        public bool IsKeepCallback;
        /// <summary>
        /// 分组标识，0标识无分组(有效域为方法)
        /// </summary>
        public int GroupId;
        /// <summary>
        /// 服务名称
        /// </summary>
        public virtual string ServiceName
        {
            get { return Service; }
        }
        /// <summary>
        /// 输入参数最大数据长度,0表示不限(有效域为方法)
        /// </summary>
        public int InputParameterMaxLength;
        /// <summary>
        /// 命令序号(有效域为方法)(不能重复,比如使用枚举),IsIdentityCommand=true时有效
        /// </summary>
        public int CommandIentity = int.MaxValue;
        /// <summary>
        /// 是否验证方法(有效域为方法),一个TCP调用服务只能指定一个验证方法,且返回值类型必须为bool
        /// </summary>
        public bool IsVerifyMethod;
        /// <summary>
        /// 是否采用命令序号识别函数,用于接口稳定的网络服务(服务配置)
        /// </summary>
        public bool IsIdentityCommand;
        /// <summary>
        /// 是否生成命令序号记忆代码(服务配置)，IsIdentityCommand=true时有效
        /// </summary>
        public bool IsRememberIdentityCommeand = true;
        /// <summary>
        /// 是否支持HTTP客户端(服务配置)
        /// </summary>
        public bool IsHttpClient;
        /// <summary>
        /// HTTP调用名称(有效域为方法)
        /// </summary>
        public string HttpName;
        /// <summary>
        /// HTTP调用是否仅支持POST(有效域为方法)
        /// </summary>
        public bool IsHttpPostOnly = true;
        /// <summary>
        /// 默认HTTP编码名称(服务配置)
        /// </summary>
        public string HttpEncodingName;
        /// <summary>
        /// 默认HTTP编码
        /// </summary>
        private asynchronousMethod.returnValue<Encoding> httpEncoding;
        /// <summary>
        /// 默认HTTP编码
        /// </summary>
        internal Encoding HttpEncoding
        {
            get
            {
                if (!httpEncoding.IsReturn)
                {
                    if (HttpEncodingName != null)
                    {
                        try
                        {
                            httpEncoding = Encoding.GetEncoding(HttpEncodingName);
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, HttpEncodingName, false);
                        }
                    }
                    httpEncoding.IsReturn = true;
                }
                return httpEncoding.Value;
            }
        }
        /// <summary>
        /// 客户端连接检测时间(服务配置),0表示不检测(单位:秒)
        /// </summary>
        public int ClientCheckSeconds = 50;
        /// <summary>
        /// 服务器端发送数据缓冲区初始字节数(服务配置)
        /// </summary>
        public int SendBufferSize = fastCSharp.config.appSetting.StreamBufferSize;
        /// <summary>
        /// 服务器端接收数据缓冲区字节数(服务配置)
        /// </summary>
        public int ReceiveBufferSize = fastCSharp.config.appSetting.StreamBufferSize;
        /// <summary>
        /// 是否使用JSON序列化(服务配置)
        /// </summary>
        public bool IsJsonSerialize;
        /// <summary>
        /// 是否支持HTTP客户端 或者 是否使用JSON序列化
        /// </summary>
        public bool IsHttpClientOrJsonSerialize
        {
            get
            {
                return IsHttpClient || IsJsonSerialize;
            }
        }
        /// <summary>
        /// 是否分割客户端代码(服务配置)
        /// </summary>
        public bool IsSegmentation;
        /// <summary>
        /// 客户端代码复制到目标目录(服务配置)
        /// </summary>
        public string ClientSegmentationCopyPath;
        /// <summary>
        /// 验证超时秒数(服务配置)
        /// </summary>
        public int VerifySeconds = 20;
        /// <summary>
        /// 每秒最低接收数据(单位:KB)(服务配置)
        /// </summary>
        public int MinReceivePerSecond;
        /// <summary>
        /// 接收命令超时分钟数(服务配置)
        /// </summary>
        public int RecieveCommandMinutes;
        /// <summary>
        /// 接收数据超时的秒数(服务配置)
        /// </summary>
        public int ReceiveTimeout;
        /// <summary>
        /// 每IP最大客户端连接数,小于等于0表示不限(服务配置)
        /// </summary>
        public int MaxClientCount;
        /// <summary>
        /// 每IP最大活动客户端连接数,小于等于0表示不限(服务配置)
        /// </summary>
        public int MaxActiveClientCount;
        /// <summary>
        /// 是否压缩数据(服务配置)
        /// </summary>
        public bool IsCompress;
        ///// <summary>
        ///// 是否输出调试信息
        ///// </summary>
        //public bool IsOutputDebug;
        /// <summary>
        /// 复制TCP服务配置字段集合
        /// </summary>
        private static readonly FieldInfo[] copyFields;
        /// <summary>
        /// 复制TCP服务配置
        /// </summary>
        /// <param name="value">TCP服务配置</param>
        public void CopyFrom(tcpBase value)
        {
            foreach (FieldInfo field in copyFields) field.SetValue(this, field.GetValue(value));
        }
        /// <summary>
        /// 获取泛型参数集合
        /// </summary>
        /// <param name="_"></param>
        /// <param name="types">泛型参数集合</param>
        /// <returns>泛型参数集合</returns>
        public static fastCSharp.code.remoteType[] GetGenericParameters(int _, params Type[] types)
        {
            return types.getArray(type => new fastCSharp.code.remoteType(type));
        }
        /// <summary>
        /// 获取泛型回调委托
        /// </summary>
        /// <typeparam name="valueType">返回值类型</typeparam>
        /// <param name="callback">回调委托</param>
        /// <returns>泛型回调委托</returns>
        private static object getGenericParameterCallback<valueType>(Func<asynchronousMethod.returnValue<object>, bool> callback)
        {
            return callback != null ? genericParameterCallback<valueType>.Create(callback) : null;
        }
        /// <summary>
        /// 获取泛型回调委托函数信息
        /// </summary>
        private static readonly MethodInfo getGenericParameterCallbackMethod = typeof(tcpBase).GetMethod("getGenericParameterCallback", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 获取泛型回调委托
        /// </summary>
        /// <param name="type">返回值类型</param>
        /// <param name="callback">回调委托</param>
        /// <returns>泛型回调委托</returns>
        public static object GetGenericParameterCallback(fastCSharp.code.remoteType type, Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<object>, bool> callback)
        {
            return ((Func<Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<object>, bool>, object>)Delegate.CreateDelegate(typeof(Func<Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<object>, bool>, object>), getGenericParameterCallbackMethod.MakeGenericMethod(type.Type)))(callback);
        }
        /// <summary>
        /// 主机名称转换成IP地址
        /// </summary>
        /// <param name="host">主机名称</param>
        /// <returns>IP地址</returns>
        internal static IPAddress HostToIpAddress(string host)
        {
            if (host.length() != 0)
            {
                IPAddress ipAddress;
                if (!IPAddress.TryParse(host, out ipAddress))
                {
                    try
                    {
                        ipAddress = Dns.GetHostEntry(host).AddressList[0];
                    }
                    catch (Exception error)
                    {
                        log.Default.Add(error, host, true);
                    }
                }
                return ipAddress;
            }
            return null;
        }
    }
}
namespace fastCSharp.code
{
    /// <summary>
    /// 参数信息
    /// </summary>
    public sealed partial class parameterInfo
    {
        /// <summary>
        /// 流参数名称
        /// </summary>
        public string StreamParameterName
        {
            get { return ParameterName; }
        }
    }
}
