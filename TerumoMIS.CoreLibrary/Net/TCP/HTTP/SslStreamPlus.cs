//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SslStreamPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  SslStreamPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:43:33
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Emit;

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    /// <summary>
    /// Http安全流
    /// </summary>
   internal sealed class SslStreamPlus:SocketBasePlus
    {
       /// <summary>
        /// HTTP头部接收器
        /// </summary>
        internal sealed class headerReceiver : socketBase.headerReceiver<sslStream>
        {
            /// <summary>
            /// 接受头部换行数据
            /// </summary>
            private AsyncCallback receiveCallback;
            /// <summary>
            /// HTTP头部接收器
            /// </summary>
            /// <param name="sslStream">HTTP安全流</param>
            public headerReceiver(sslStream sslStream)
                : base(sslStream)
            {
                receiveCallback = receive;
            }
            /// <summary>
            /// 开始接收数据
            /// </summary>
            public void Receive()
            {
                timeout = date.NowSecond.AddTicks(ReceiveTimeoutQueue.CallbackTimeoutTicks);
                if (socket.isNextRequest == 0)
                {
                    ReceiveEndIndex = HeaderEndIndex = 0;
                    receive();
                }
                else
                {
                    if ((ReceiveEndIndex -= (HeaderEndIndex + sizeof(int))) > 0)
                    {
                        System.Buffer.BlockCopy(buffer, HeaderEndIndex + sizeof(int), buffer, 0, ReceiveEndIndex);
                        HeaderEndIndex = 0;
                        onReceive();
                    }
                    else
                    {
                        ReceiveEndIndex = HeaderEndIndex = 0;
                        receive();
                    }
                }
            }
            /// <summary>
            /// 接受头部换行数据
            /// </summary>
            protected override void receive()
            {
                try
                {
                    int timeoutIdentity = socket.timeoutIdentity;
                    socket.SslStream.BeginRead(buffer, ReceiveEndIndex,  fastCSharp.config.http.Default.HeaderBufferLength - ReceiveEndIndex, receiveCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                socket.headerError();
            }
            /// <summary>
            /// 接受头部换行数据
            /// </summary>
            /// <param name="result">异步操作状态</param>
            private unsafe void receive(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                int count = 0;
                try
                {
                    if ((count = socket.SslStream.EndRead(result)) > 0) ReceiveEndIndex += count;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (count <= 0 || date.NowSecond >= timeout) socket.headerError();
                else onReceive();
            }
        }
        /// <summary>
        /// 表单数据接收器
        /// </summary>
        private sealed class formIdentityReceiver : formIdentityReceiver<sslStream>
        {
            /// <summary>
            /// 接收表单数据处理
            /// </summary>
            private AsyncCallback receiveCallback;
            /// <summary>
            /// HTTP表单接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public formIdentityReceiver(sslStream socket)
                : base(socket)
            {
                receiveCallback = receive;
            }
            /// <summary>
            /// 开始接收表单数据
            /// </summary>
            /// <param name="loadForm">HTTP请求表单加载接口</param>
            public void Receive(requestForm.ILoadForm loadForm)
            {
                this.loadForm = loadForm;
                headerReceiver headerReceiver = socket.HeaderReceiver;
                contentLength = headerReceiver.RequestHeader.ContentLength;
                if (contentLength < socket.Buffer.Length)
                {
                    buffer = socket.Buffer;
                    memoryPool = null;
                }
                else
                {
                    memoryPool = getMemoryPool(contentLength + 1);
                    buffer = memoryPool.Get(contentLength + 1);
                }
                receiveEndIndex = headerReceiver.ReceiveEndIndex - headerReceiver.HeaderEndIndex - sizeof(int);
                System.Buffer.BlockCopy(headerReceiver.RequestHeader.Buffer, headerReceiver.HeaderEndIndex + sizeof(int), buffer, 0, receiveEndIndex);
                headerReceiver.ReceiveEndIndex = headerReceiver.HeaderEndIndex;

                if (receiveEndIndex == contentLength) parse();
                else
                {
                    receiveStartTime = date.NowSecond.AddTicks(date.SecondTicks);
                    receive();
                }
            }
            /// <summary>
            /// 开始接收表单数据
            /// </summary>
            private void receive()
            {
                try
                {
                    int timeoutIdentity = socket.timeoutIdentity;
                    socket.SslStream.BeginRead(buffer, receiveEndIndex, contentLength - receiveEndIndex, receiveCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                receiveError();
            }
            /// <summary>
            /// 接收表单数据处理
            /// </summary>
            /// <param name="result">异步操作状态</param>
            private unsafe void receive(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                int count = 0;
                try
                {
                    if ((count = socket.SslStream.EndRead(result)) > 0)
                    {
                        receiveEndIndex += count;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (count <= 0) receiveError();
                else if (receiveEndIndex == contentLength) parse();
                else if (date.NowSecond > receiveStartTime && receiveEndIndex < minReceiveSizePerSecond4 * ((int)(date.NowSecond - receiveStartTime).TotalSeconds >> 2)) receiveError();
                else receive();
            }
        }
        /// <summary>
        /// 数据接收器
        /// </summary>
        private sealed class boundaryIdentityReceiver : boundaryIdentityReceiver<sslStream>
        {
            /// <summary>
            /// 接收表单数据处理
            /// </summary>
            private AsyncCallback receiveCallback;
            /// <summary>
            /// HTTP数据接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public boundaryIdentityReceiver(sslStream socket)
                : base(socket)
            {
                receiveCallback = receive;
            }
            /// <summary>
            /// 开始接收表单数据
            /// </summary>
            /// <param name="loadForm">HTTP请求表单加载接口</param>
            public void Receive(requestForm.ILoadForm loadForm)
            {
                this.loadForm = loadForm;
                try
                {
                    Buffer = bigBuffers.Get();
                    headerReceiver headerReceiver = socket.HeaderReceiver;
                    boundary = headerReceiver.RequestHeader.Boundary;
                    receiveLength = receiveEndIndex = headerReceiver.ReceiveEndIndex - headerReceiver.HeaderEndIndex - sizeof(int);
                    System.Buffer.BlockCopy(headerReceiver.RequestHeader.Buffer, headerReceiver.HeaderEndIndex + sizeof(int), Buffer, 0, receiveEndIndex);
                    headerReceiver.ReceiveEndIndex = headerReceiver.HeaderEndIndex;
                    contentLength = headerReceiver.RequestHeader.ContentLength;

                    receiveStartTime = date.NowSecond.AddTicks(date.SecondTicks);
                    onFirstBoundary();
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                this.error();
            }
            /// <summary>
            /// 开始接收表单数据
            /// </summary>
            protected override void receive()
            {
                try
                {
                    int timeoutIdentity = socket.timeoutIdentity;
                    socket.SslStream.BeginRead(Buffer, receiveEndIndex, bigBuffers.Size - receiveEndIndex - sizeof(int), receiveCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                this.error();
            }
            /// <summary>
            /// 接收表单数据处理
            /// </summary>
            /// <param name="result">异步操作状态</param>
            private unsafe void receive(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                int count = 0;
                try
                {
                    if ((count = socket.SslStream.EndRead(result)) > 0)
                    {
                        receiveEndIndex += count;
                        receiveLength += count;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (count <= 0 || receiveLength > contentLength
                    || (date.NowSecond > receiveStartTime && receiveLength < minReceiveSizePerSecond4 * ((int)(date.NowSecond - receiveStartTime).TotalSeconds >> 2)))
                {
                    error();
                }
                else onReceiveData();
            }
        }
        /// <summary>
        /// 数据发送器
        /// </summary>
        private sealed class dataSender : dataSender<sslStream>
        {
            /// <summary>
            /// 发送数据处理
            /// </summary>
            private AsyncCallback sendCallback;
            /// <summary>
            /// 发送文件数据处理
            /// </summary>
            private AsyncCallback sendFileCallback;
            /// <summary>
            /// 当前发送字节数
            /// </summary>
            private int sendSize;
            /// <summary>
            /// 数据发送器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public dataSender(sslStream socket)
                : base(socket)
            {
                sendCallback = send;
                sendFileCallback = sendFile;
            }
            /// <summary>
            /// 发送数据
            /// </summary>
            /// <param name="onSend">发送数据回调处理</param>
            /// <param name="buffer">发送数据缓冲区</param>
            /// <param name="pushBuffer">发送数据缓冲区回调处理</param>
            public void Send(Action<bool> onSend, subArray<byte> buffer, memoryPool memoryPool)
            {
                this.onSend = onSend;
                sendStartTime = date.NowSecond.AddTicks(date.SecondTicks);
                this.memoryPool = memoryPool;
                this.buffer = buffer.Array;
                sendIndex = buffer.StartIndex;
                sendLength = 0;
                sendEndIndex = sendIndex + buffer.Count;

                send();
            }
            /// <summary>
            /// 开始发送数据
            /// </summary>
            private void send()
            {
                try
                {
                    int timeoutIdentity = socket.timeoutIdentity;
                    socket.SslStream.BeginWrite(buffer, sendIndex, sendSize = Math.Min(sendEndIndex - sendIndex, net.socket.MaxServerSendSize), sendCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                send(false);
            }
            /// <summary>
            /// 发送数据处理
            /// </summary>
            /// <param name="async">异步调用状态</param>
            private unsafe void send(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                try
                {
                    socket.SslStream.EndWrite(result);
                    sendIndex += sendSize;
                    sendLength += sendSize;
                }
                catch (Exception error)
                {
                    send(false);
                    log.Error.Add(error, null, false);
                    return;
                }
                if (sendIndex == sendEndIndex) send(true);
                else if (date.NowSecond > sendStartTime && sendLength < minReceiveSizePerSecond4 * ((int)(date.NowSecond - sendStartTime).TotalSeconds >> 2)) send(false);
                else send();
            }
            /// <summary>
            /// 发送文件数据
            /// </summary>
            /// <param name="onSend">发送数据回调处理</param>
            /// <param name="fileName">文件名称</param>
            /// <param name="seek">起始位置</param>
            /// <param name="size">发送字节长度</param>
            public void SendFile(Action<bool> onSend, string fileName, long seek, long size)
            {
                try
                {
                    fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, fastCSharp.config.appSetting.StreamBufferSize, FileOptions.SequentialScan);
                    if (fileStream.Length >= seek + size)
                    {
                        if (seek != 0) fileStream.Seek(seek, SeekOrigin.Begin);
                        this.onSend = onSend;
                        sendStartTime = date.NowSecond.AddTicks(date.SecondTicks);
                        fileSize = size;
                        sendLength = 0;

                        memoryPool = net.socket.ServerSendBuffers;
                        buffer = memoryPool.Get();
                        readFile();
                        return;
                    }
                    else fileStream.Dispose();
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                onSend(false);
            }
            /// <summary>
            /// 开始发送文件数据
            /// </summary>
            protected override void sendFile()
            {
                try
                {
                    int timeoutIdentity = socket.timeoutIdentity;
                    socket.SslStream.BeginWrite(buffer, sendIndex, sendSize = sendEndIndex - sendIndex, sendFileCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                sendFile(false);
            }
            /// <summary>
            /// 发送文件数据处理
            /// </summary>
            /// <param name="result">异步操作状态</param>
            private unsafe void sendFile(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                try
                {
                    socket.SslStream.EndWrite(result);
                    sendIndex += sendSize;
                    sendLength += sendSize;
                }
                catch (Exception error)
                {
                    sendFile(false);
                    log.Error.Add(error, null, false);
                    return;
                }
                if (sendIndex == sendEndIndex) readFile();
                else if (date.NowSecond > sendStartTime && sendLength < minReceiveSizePerSecond4 * ((int)(date.NowSecond - sendStartTime).TotalSeconds >> 2)) sendFile(false);
                else sendFile();
            }
        }
        /// <summary>
        /// WebSocket请求接收器
        /// </summary>
        private unsafe sealed class webSocketIdentityReceiver : webSocketIdentityReceiver<sslStream>
        {
            /// <summary>
            /// WebSocket请求数据处理
            /// </summary>
            private AsyncCallback receiveCallback;
            /// <summary>
            /// WebSocket请求接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public webSocketIdentityReceiver(sslStream socket)
                : base(socket)
            {
                receiveCallback = receive;
            }
            /// <summary>
            /// 开始接收请求数据
            /// </summary>
            public void Receive()
            {
                receiveEndIndex = 0;
                timeout = date.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                receive();
            }
            /// <summary>
            /// 开始接收数据
            /// </summary>
            protected override void receive()
            {
                try
                {
                    int timeoutIdentity = socket.timeoutIdentity;
                    socket.SslStream.BeginRead(buffer, receiveEndIndex, fastCSharp.config.http.Default.HeaderBufferLength - receiveEndIndex, receiveCallback, this);
                    socket.setTimeout(timeoutIdentity);
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                socket.webSocketEnd();
            }
            /// <summary>
            /// WebSocket请求数据处理
            /// </summary>
            /// <param name="result">异步调用状态</param>
            private void receive(IAsyncResult result)
            {
                ++socket.timeoutIdentity;
                int count = int.MinValue;
                try
                {
                    if ((count = socket.SslStream.EndRead(result)) >= 0)
                    {
                        receiveEndIndex += count;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (count < 0) socket.webSocketEnd();
                else if(date.NowSecond >= timeout) close();
                else if (count == 0) fastCSharp.threading.timerTask.Default.Add(receiveHandle, date.NowSecond.AddSeconds(1), null);
                else if (receiveEndIndex >= 6) JsonParserPlus.typeParser<>.tryParse();
                else receive();
            }
        }
        /// <summary>
        /// HTTP头部接收器
        /// </summary>
        internal headerReceiver HeaderReceiver;
        /// <summary>
        /// 获取HTTP请求头部
        /// </summary>
        internal override requestHeader RequestHeader
        {
            get { return HeaderReceiver.RequestHeader; }
        }
        /// <summary>
        /// 表单数据接收器
        /// </summary>
        private formIdentityReceiver formReceiver;
        /// <summary>
        /// 数据接收器
        /// </summary>
        private boundaryIdentityReceiver boundaryReceiver;
        /// <summary>
        /// 数据发送器
        /// </summary>
        private dataSender sender;
        /// <summary>
        /// WebSocket请求接收器
        /// </summary>
        private webSocketIdentityReceiver webSocketReceiver;
        /// <summary>
        /// 身份验证完成处理
        /// </summary>
        private AsyncCallback authenticateCallback;
        /// <summary>
        /// 网络流
        /// </summary>
        private NetworkStream networkStream;
        /// <summary>
        /// 安全网络流
        /// </summary>
        internal SslStream SslStream;
        /// <summary>
        /// HTTP安全流
        /// </summary>
        private sslStream()
        {
            HeaderReceiver = new headerReceiver(this);
            sender = new dataSender(this);
            authenticateCallback = onAuthenticate;
        }
        /// <summary>
        /// 开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="certificate">SSL证书</param>
        private void start(servers servers, Socket socket, X509Certificate certificate)
        {
            this.servers = servers;
            Socket = socket;
            try
            {
                SslStream = new SslStream(networkStream = new NetworkStream(socket, true), false);
                SslStream.BeginAuthenticateAsServer(certificate, false, SslProtocols.Tls, false, authenticateCallback, this);
                return;
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            headerError();
        }
        /// <summary>
        /// 身份验证完成处理
        /// </summary>
        /// <param name="result">异步操作状态</param>
        private void onAuthenticate(IAsyncResult result)
        {
            try
            {
                SslStream.EndAuthenticateAsServer(result);
                isLoadForm = isNextRequest = 0;
                DomainServer = null;
                form.Clear();
                response.Push(ref response);
                HeaderReceiver.RequestHeader.IsKeepAlive = false;
                HeaderReceiver.Receive();
                return;
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            headerError();
        }
        /// <summary>
        /// HTTP头部接收错误
        /// </summary>
        protected override void headerError()
        {
            if (ipv6.IsNull) sslServer.SocketEnd(ipv4);
            else sslServer.SocketEnd(ipv6);
            close();
            form.Clear();
            typePool<sslStream>.Push(this);
        }
        /// <summary>
        /// WebSocket结束
        /// </summary>
        protected override void webSocketEnd()
        {
            webSocketReceiver.Clear();
            if (ipv6.IsNull) sslServer.SocketEnd(ipv4);
            else sslServer.SocketEnd(ipv6);
            close();
            typePool<sslStream>.Push(this);
        }
        /// <summary>
        /// 未能识别的HTTP头部
        /// </summary>
        protected override void headerUnknown()
        {
            responseError(http.response.state.BadRequest400);
        }
        /// <summary>
        /// 开始接收头部数据
        /// </summary>
        protected override void receiveHeader()
        {
            HeaderReceiver.Receive();
        }
        /// <summary>
        /// 获取AJAX回调函数
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <returns>AJAX回调函数,失败返回null</returns>
        internal override subString GetWebSocketCallBack(long identity)
        {
            return webSocketReceiver.GetCallBack(identity);
        }
        /// <summary>
        /// 输出错误状态
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <param name="state">错误状态</param>
        internal override bool WebSocketResponseError(long identity, response.state state)
        {
            return webSocketReceiver.ResponseError(identity, state);
        }
        /// <summary>
        /// 开始接收WebSocket数据
        /// </summary>
        protected override void receiveWebSocket()
        {
            if (webSocketReceiver == null) webSocketReceiver = new webSocketIdentityReceiver(this);
            webSocketReceiver.Receive();
        }
        /// <summary>
        /// 获取请求表单数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="loadForm">HTTP请求表单加载接口</param>
        internal override void GetForm(long identity, requestForm.ILoadForm loadForm)
        {
            if (Interlocked.CompareExchange(ref this.identity, identity + 1, identity) == identity)
            {
                if (isLoadForm == 0)
                {
                    isLoadForm = 1;
                    if (check100Continue())
                    {
                        requestHeader.postType type = HeaderReceiver.RequestHeader.PostType;
                        if (type == requestHeader.postType.Json || type == requestHeader.postType.Form)
                        {
                            if (formReceiver == null) formReceiver = new formIdentityReceiver(this);
                            formReceiver.Receive(loadForm);
                        }
                        else
                        {
                            if (boundaryReceiver == null) boundaryReceiver = new boundaryIdentityReceiver(this);
                            boundaryReceiver.Receive(loadForm);
                        }
                        return;
                    }
                }
                else log.Error.Add("表单已加载", true, true);
            }
            loadForm.OnGetForm(null);
        }
        /// <summary>
        /// HTTP响应头部输出
        /// </summary>
        /// <param name="buffer">输出数据</param>
        /// <param name="memoryPool">内存池</param>
        protected override void responseHeader(subArray<byte> buffer, memoryPool memoryPool)
        {
            if (responseSize == 0)
            {
                response.Push(ref response);
                sender.Send(sendNext, buffer, memoryPool);
            }
            else sender.Send(sendResponseBody, buffer, memoryPool);
        }
        /// <summary>
        /// 输出HTTP响应数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="response">HTTP响应数据</param>
        public override unsafe bool Response(long identity, ref response response)
        {
            if (identity >= 0)
            {
                if (Interlocked.CompareExchange(ref this.identity, identity + 1, identity) == identity)
                {
                    this.response = response;
                    response = null;
                    if (this.response.LastModified != null)
                    {
                        subArray<byte> ifModifiedSince = HeaderReceiver.RequestHeader.IfModifiedSince;
                        if (ifModifiedSince.Count == this.response.LastModified.Length)
                        {
                            fixed (byte* buffer = ifModifiedSince.Array)
                            {
                                if (unsafer.memory.Equal(this.response.LastModified, buffer + ifModifiedSince.StartIndex, ifModifiedSince.Count))
                                {
                                    response.Push(ref this.response);
                                    this.response = response.NotChanged304;
                                }
                            }
                        }
                    }
                    if (boundaryReceiver != null) bigBuffers.Push(ref boundaryReceiver.Buffer);
                    if (HeaderReceiver.RequestHeader.Method == fastCSharp.web.http.methodType.POST && isLoadForm == 0)
                    {
                        if (HeaderReceiver.RequestHeader.PostType == requestHeader.postType.Json || HeaderReceiver.RequestHeader.PostType == requestHeader.postType.Form)
                        {
                            if (formReceiver == null) formReceiver = new formIdentityReceiver(this);
                            formReceiver.Receive(this);
                        }
                        else
                        {
                            if (boundaryReceiver == null) boundaryReceiver = new boundaryIdentityReceiver(this);
                            boundaryReceiver.Receive(this);
                        }
                    }
                    else responseHeader();
                    return true;
                }
                response.Push(ref response);
                return false;
            }
            return webSocketReceiver.Response(identity, ref response);
        }
        /// <summary>
        /// 发送HTTP响应内容
        /// </summary>
        /// <param name="isSend">是否发送成功</param>
        protected override void responseBody(bool isSend)
        {
            if (isSend)
            {
                if (response.BodyFile == null)
                {
                    subArray<byte> body = response.Body;
                    if (response.State == response.state.PartialContent206)
                    {
                        body.UnsafeSet(body.StartIndex + (int)HeaderReceiver.RequestHeader.RangeStart, (int)responseSize);
                    }
                    sender.Send(sendNext, body, null);
                }
                else sender.SendFile(sendNext, response.BodyFile, response.State == response.state.PartialContent206 ? HeaderReceiver.RequestHeader.RangeStart : 0, responseSize);
            }
            else headerError();
        }
        /// <summary>
        /// 输出错误状态
        /// </summary>
        /// <param name="state">错误状态</param>
        protected override void responseError(response.state state)
        {
            if (boundaryReceiver != null) bigBuffers.Push(ref boundaryReceiver.Buffer);
            if (DomainServer != null)
            {
                response = DomainServer.Server.GetErrorResponseData(state, HeaderReceiver.RequestHeader.IsGZip);
                if (response != null)
                {
                    if (state != http.response.state.NotFound404 || HeaderReceiver.RequestHeader.Method != web.http.methodType.GET)
                    {
                        HeaderReceiver.RequestHeader.IsKeepAlive = false;
                    }
                    responseHeader();
                    return;
                }
            }
            byte[] data = errorResponseDatas[(int)state];
            if (data != null)
            {
                if (state == http.response.state.NotFound404 && HeaderReceiver.RequestHeader.Method == web.http.methodType.GET)
                {
                    sender.Send(sendNext, subArray<byte>.Unsafe(data, 0, data.Length), null);
                }
                else
                {
                    HeaderReceiver.RequestHeader.IsKeepAlive = false;
                    sender.Send(sendClose, subArray<byte>.Unsafe(data, 0, data.Length), null);
                }
            }
            else headerError();
        }
        /// <summary>
        /// 关闭套接字
        /// </summary>
        private void close()
        {
            pub.Dispose(ref SslStream);
            pub.Dispose(ref networkStream);
            close(Socket);
            Socket = null;
        }
        /// <summary>
        /// 开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="ip">客户端IP</param>
        /// <param name="certificate">SSL证书</param>
        internal static void Start(servers servers, Socket socket, ipv6Hash ip, X509Certificate certificate)
        {
            try
            {
                sslStream value = typePool<sslStream>.Pop() ?? new sslStream();
                value.ipv6 = ip;
                value.ipv4 = 0;
                value.start(servers, socket, certificate);
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
                sslServer.SocketEnd(ip);
                close(socket);
            }
        }
        /// <summary>
        /// 开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="ip">客户端IP</param>
        /// <param name="certificate">SSL证书</param>
        internal static void Start(servers servers, Socket socket, int ip, X509Certificate certificate)
        {
            try
            {
                sslStream value = typePool<sslStream>.Pop() ?? new sslStream();
                value.ipv4 = ip;
                value.ipv6 = default(ipv6Hash);
                value.start(servers, socket, certificate);
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
                sslServer.SocketEnd(ip);
                close(socket);
            }
        }
    }
}
