//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: Socket
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  Socket
//	User name:  C1400008
//	Location Time: 2015/7/16 15:41:51
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
    /// HTTP套接字
    /// </summary>
    public abstract class socketBase : requestForm.ILoadForm, IDisposable
    {
        /// <summary>
        /// 大数据缓冲区
        /// </summary>
        protected static readonly memoryPool bigBuffers = memoryPool.GetPool(fastCSharp.config.http.Default.BigBufferSize);
        /// <summary>
        /// 获取数据缓冲区
        /// </summary>
        /// <param name="length">缓冲区字节长度</param>
        /// <returns>数据缓冲区</returns>
        protected static memoryPool getMemoryPool(int length)
        {
            return length <= fastCSharp.config.appSetting.StreamBufferSize ? memoryPool.StreamBuffers : bigBuffers;
        }
        /// <summary>
        /// HTTP套接字数量
        /// </summary>
        protected static int newCount;
        /// <summary>
        /// HTTP套接字数量
        /// </summary>
        public static int NewCount
        {
            get { return newCount; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static int PoolCount
        {
            get { return typePool<socket>.Count(); }
        }
        /// <summary>
        /// HTTP头部接收超时队列
        /// </summary>
        internal static readonly timeoutQueue ReceiveTimeoutQueue = new timeoutQueue(fastCSharp.config.http.Default.ReceiveSeconds);
        /// <summary>
        /// WebSocket超时队列
        /// </summary>
        internal static readonly timeoutQueue WebSocketReceiveTimeoutQueue = new timeoutQueue(fastCSharp.config.http.Default.WebSocketReceiveSeconds);
        /// <summary>
        /// HTTP头部接收器
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        internal abstract class headerReceiver<socketType> where socketType : socketBase
        {
            /// <summary>
            /// HTTP套接字
            /// </summary>
            protected socketType socket;
            /// <summary>
            /// HTTP头部数据缓冲区
            /// </summary>
            protected byte[] buffer;
            /// <summary>
            /// 超时时间
            /// </summary>
            protected DateTime timeout;
            /// <summary>
            /// 接收数据结束位置
            /// </summary>
            public int ReceiveEndIndex;
            /// <summary>
            /// HTTP头部数据结束位置
            /// </summary>
            public int HeaderEndIndex;
            /// <summary>
            /// HTTP请求头部
            /// </summary>
            public requestHeader RequestHeader;
            /// <summary>
            /// HTTP头部接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public headerReceiver(socketType socket)
            {
                this.socket = socket;
                buffer = (RequestHeader = new requestHeader()).Buffer;
            }
            /// <summary>
            /// 接受头部换行数据
            /// </summary>
            protected abstract void receive();
            /// <summary>
            /// 接受头部数据处理
            /// </summary>
            protected unsafe void onReceive()
            {
                int searchEndIndex = ReceiveEndIndex - sizeof(int);
                if (HeaderEndIndex <= searchEndIndex)
                {
                    fixed (byte* dataFixed = buffer)
                    {
                        byte* start = dataFixed + HeaderEndIndex, searchEnd = dataFixed + searchEndIndex, end = dataFixed + ReceiveEndIndex;
                        *end = 13;
                        do
                        {
                            while (*start != 13) ++start;
                            if (start <= searchEnd)
                            {
                                if (*(int*)start == 0x0a0d0a0d)
                                {
                                    HeaderEndIndex = (int)(start - dataFixed);
                                    bool isParseHeader = RequestHeader.Parse(HeaderEndIndex, ReceiveEndIndex);
                                    if (RequestHeader.Host.Count != 0) socket.DomainServer = socket.servers.GetServer(RequestHeader.Host);
                                    if (isParseHeader && socket.DomainServer != null)
                                    {
                                        if (RequestHeader.IsHeaderError) socket.headerError();
                                        else if (RequestHeader.IsRangeError) socket.responseError(response.state.RangeNotSatisfiable416);
                                        else socket.request();
                                    }
                                    else socket.headerUnknown();
                                    return;
                                }
                                ++start;
                            }
                            else
                            {
                                HeaderEndIndex = (int)(start - dataFixed);
                                break;
                            }
                        }
                        while (true);
                    }
                }
                if (ReceiveEndIndex == fastCSharp.config.http.Default.HeaderBufferLength) socket.headerUnknown();
                else receive();
            }
        }
        /// <summary>
        /// 表单数据接收器
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        protected abstract class formIdentityReceiver<socketType> where socketType : socketBase
        {
            /// <summary>
            /// HTTP套接字
            /// </summary>
            protected socketType socket;
            /// <summary>
            /// 表单接收缓冲区
            /// </summary>
            protected byte[] buffer;
            /// <summary>
            /// 缓冲区内存池
            /// </summary>
            protected memoryPool memoryPool;
            /// <summary>
            /// 接收数据起始时间
            /// </summary>
            protected DateTime receiveStartTime;
            /// <summary>
            /// 表单数据内容长度
            /// </summary>
            protected int contentLength;
            /// <summary>
            /// 接收数据结束位置
            /// </summary>
            protected int receiveEndIndex;
            /// <summary>
            /// HTTP请求表单
            /// </summary>
            private requestForm form;
            /// <summary>
            /// HTTP请求表单加载
            /// </summary>
            protected requestForm.ILoadForm loadForm;
            /// <summary>
            /// HTTP表单接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public formIdentityReceiver(socketType socket)
            {
                this.socket = socket;
                form = socket.form;
            }
            /// <summary>
            /// 表单接收错误
            /// </summary>
            protected void receiveError()
            {
                try
                {
                    loadForm.OnGetForm(null);
                }
                finally
                {
                    if (memoryPool != null) memoryPool.Push(ref buffer);
                    socket.headerError();
                }
            }
            /// <summary>
            /// 解析表单数据
            /// </summary>
            protected void parse()
            {
                requestHeader header = socket.RequestHeader;
                if (header.PostType == requestHeader.postType.Json ? form.Parse(buffer, 0, receiveEndIndex, header.JsonEncoding) : form.Parse(buffer, receiveEndIndex))
                {
                    long identity = form.Identity = socket.identity;
                    try
                    {
                        loadForm.OnGetForm(form);
                        return;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    if (memoryPool != null) memoryPool.Push(ref buffer);
                    socket.ResponseError(identity, response.state.ServerError500);
                }
                else receiveError();
            }
        }
        /// <summary>
        /// 数据接收器
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        protected abstract class boundaryIdentityReceiver<socketType> where socketType : socketBase
        {
            /// <summary>
            /// 缓存文件名称前缀
            /// </summary>
            protected static readonly string cacheFileName = fastCSharp.config.pub.Default.CachePath + ((ulong)date.NowSecond.Ticks).toHex16();
            /// <summary>
            /// HTTP套接字
            /// </summary>
            protected socketType socket;
            /// <summary>
            /// 表单接收缓冲区
            /// </summary>
            internal byte[] Buffer;
            /// <summary>
            /// 接收数据起始时间
            /// </summary>
            protected DateTime receiveStartTime;
            /// <summary>
            /// 接受数据处理
            /// </summary>
            protected Action onReceiveData;
            /// <summary>
            /// 接受第一个分隔符处理
            /// </summary>
            private Action onReceiveFirstBoundary;
            /// <summary>
            /// 接收换行数据
            /// </summary>
            private Action onReceiveEnter;
            /// <summary>
            /// 接收表单值
            /// </summary>
            private Action onReceiveValue;
            /// <summary>
            /// HTTP请求表单
            /// </summary>
            private requestForm form;
            /// <summary>
            /// 当前处理表单值
            /// </summary>
            private requestForm.value formValue;
            /// <summary>
            /// 当前表单值文件流
            /// </summary>
            private fileStreamWriter fileStream;
            /// <summary>
            /// 文件流写入回调事件
            /// </summary>
            private pushPool<byte[]> onWriteFile;
            /// <summary>
            /// HTTP请求表单加载
            /// </summary>
            protected requestForm.ILoadForm loadForm;
            /// <summary>
            /// 数据分割符
            /// </summary>
            protected subArray<byte> boundary;
            /// <summary>
            /// 表单数据内容长度
            /// </summary>
            protected int contentLength;
            /// <summary>
            /// 接收数据结束位置
            /// </summary>
            protected int receiveEndIndex;
            /// <summary>
            /// 数据起始位置
            /// </summary>
            private int startIndex;
            /// <summary>
            /// 当前数据位置
            /// </summary>
            private int currentIndex;
            /// <summary>
            /// 当前接收数据字节长度
            /// </summary>
            protected int receiveLength;
            /// <summary>
            /// 表单值当前起始位置换行符标识
            /// </summary>
            private int valueEnterIndex;
            /// <summary>
            /// HTTP数据接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public boundaryIdentityReceiver(socketType socket)
            {
                this.socket = socket;
                form = socket.form;
                onReceiveFirstBoundary = onFirstBoundary;
                onReceiveEnter = onEnter;
                onReceiveValue = onValue;
                onWriteFile = onFile;
            }
            /// <summary>
            /// 开始接收表单数据
            /// </summary>
            protected abstract void receive();
            /// <summary>
            /// 数据接收错误
            /// </summary>
            protected void error()
            {
                try
                {
                    form.Clear();
                    loadForm.OnGetForm(null);
                }
                finally
                {
                    bigBuffers.Push(ref Buffer);
                    pub.Dispose(ref fileStream);
                    socket.headerError();
                }
            }
            /// <summary>
            /// 表单数据接收完成
            /// </summary>
            private void boundaryReceiverFinally()
            {
                if (receiveLength == contentLength)
                {
                    long identity = form.Identity = socket.identity;
                    try
                    {
                        pub.Dispose(ref fileStream);
                        form.SetFileValue();
                        loadForm.OnGetForm(form);
                        return;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    bigBuffers.Push(ref Buffer);
                    socket.ResponseError(identity, response.state.ServerError500);
                }
                else this.error();
            }
            /// <summary>
            /// 接收第一个分割符
            /// </summary>
            protected void onFirstBoundary()
            {
                if (receiveEndIndex >= boundary.Count + 4) checkFirstBoundary();
                else
                {
                    onReceiveData = onReceiveFirstBoundary;
                    receive();
                }
            }
            /// <summary>
            /// 检测第一个分隔符
            /// </summary>
            private unsafe void checkFirstBoundary()
            {
                int boundaryLength4 = boundary.Count + 4;
                fixed (byte* bufferFixed = Buffer, boundaryFixed = boundary.Array)
                {
                    if (*(short*)bufferFixed == '-' + ('-' << 8) && fastCSharp.unsafer.memory.Equal(boundaryFixed + boundary.StartIndex, bufferFixed + 2, boundary.Count))
                    {
                        int endValue = (int)*(short*)(bufferFixed + 2 + boundary.Count);
                        if (endValue == 0x0a0d)
                        {
                            startIndex = currentIndex = boundaryLength4;
                            onEnter();
                            return;
                        }
                        else if (((endValue ^ ('-' + ('-' << 8))) | (receiveEndIndex ^ boundaryLength4)) == 0)
                        {
                            boundaryReceiverFinally();
                            return;
                        }
                    }
                }
                error();
            }
            /// <summary>
            /// 查找换行处理
            /// </summary>
            private unsafe void onEnter()
            {
                int length = receiveEndIndex - currentIndex;
                if (length > sizeof(int)) checkEnter();
                else receiveEnter();
            }
            /// <summary>
            /// 继续接收换行
            /// </summary>
            private unsafe void receiveEnter()
            {
                int length = receiveEndIndex - startIndex;
                if (length >= fastCSharp.config.http.Default.HeaderBufferLength) error();
                else
                {
                    if (receiveEndIndex == bigBuffers.Size - sizeof(int))
                    {
                        fixed (byte* bufferFixed = Buffer)
                        {
                            unsafer.memory.Copy(bufferFixed + startIndex, bufferFixed, length);
                        }
                        currentIndex -= startIndex;
                        receiveEndIndex = length;
                        startIndex = 0;
                    }
                    onReceiveData = onReceiveEnter;
                    receive();
                }
            }
            /// <summary>
            /// 查找换行
            /// </summary>
            private unsafe void checkEnter()
            {
                int searchEndIndex = receiveEndIndex - sizeof(int);
                fixed (byte* dataFixed = Buffer)
                {
                    byte* start = dataFixed + currentIndex, searchEnd = dataFixed + searchEndIndex, end = dataFixed + receiveEndIndex;
                    *end = 13;
                    do
                    {
                        while (*start != 13) ++start;
                        if (start <= searchEnd)
                        {
                            if (*(int*)start == 0x0a0d0a0d)
                            {
                                currentIndex = (int)(start - dataFixed);
                                parseName();
                                return;
                            }
                            ++start;
                        }
                        else
                        {
                            currentIndex = (int)(start - dataFixed);
                            break;
                        }
                    }
                    while (true);
                }
                receiveEnter();
            }
            /// <summary>
            /// 解析表单名称
            /// </summary>
            private unsafe void parseName()
            {
                formValue.Null();
                try
                {
                    fixed (byte* dataFixed = Buffer)
                    {
                        byte* start = dataFixed + startIndex, end = dataFixed + currentIndex;
                        *end = (byte)';';
                        do
                        {
                            while (*start == ' ') ++start;
                            if (start == end) break;
                            if (*(int*)start == ('n' | ('a' << 8) | ('m' << 16) | ('e' << 24)))
                            {
                                formValue.Name = getFormNameValue(dataFixed, start += sizeof(int), end);
                                start += formValue.Name.Count + 3;
                            }
                            else if (((*(int*)start ^ ('f' | ('i' << 8) | ('l' << 16) | ('e' << 24)))
                                | (*(int*)(start + sizeof(int)) ^ ('n' | ('a' << 8) | ('m' << 16) | ('e' << 24)))) == 0)
                            {
                                formValue.FileName = getFormNameValue(dataFixed, start += sizeof(int) * 2, end);
                                start += formValue.FileName.Count + 3;
                            }
                            for (*end = (byte)';'; *start != ';'; ++start) ;
                        }
                        while (start++ != end);
                        *end = 13;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (formValue.Name.Array == null) this.error();
                else
                {
                    startIndex = valueEnterIndex = (currentIndex += 4);
                    onValue();
                }
            }
            /// <summary>
            /// 获取表单名称值
            /// </summary>
            /// <param name="dataFixed">数据</param>
            /// <param name="start">数据起始位置</param>
            /// <param name="end">数据结束位置</param>
            /// <returns>表单名称值,失败返回null</returns>
            private unsafe subArray<byte> getFormNameValue(byte* dataFixed, byte* start, byte* end)
            {
                while (*start == ' ') ++start;
                if (*start == '=')
                {
                    while (*++start == ' ') ;
                    if (*start == '"')
                    {
                        byte* valueStart = ++start;
                        for (*end = (byte)'"'; *start != '"'; ++start) ;
                        if (start != end)
                        {
                            byte[] value = new byte[start - valueStart];
                            System.Buffer.BlockCopy(Buffer, (int)(valueStart - dataFixed), value, 0, value.Length);
                            return subArray<byte>.Unsafe(value, 0, value.Length);
                        }
                    }
                }
                return default(subArray<byte>);
            }
            /// <summary>
            /// 接收表单值处理
            /// </summary>
            private void onValue()
            {
                if (valueEnterIndex >= 0 ? receiveEndIndex - valueEnterIndex >= (boundary.Count + 4) : (receiveEndIndex - currentIndex >= (boundary.Count + 6))) checkValue();
                else receiveValue();
            }
            /// <summary>
            /// 继续接收数据
            /// </summary>
            private unsafe void receiveValue()
            {
                try
                {
                    if (receiveEndIndex == bigBuffers.Size - sizeof(int))
                    {
                        if (startIndex == 0)
                        {
                            if (fileStream == null)
                            {
                                formValue.SaveFileName = loadForm.GetSaveFileName(formValue);
                                if (formValue.SaveFileName == null) formValue.SaveFileName = cacheFileName + ((ulong)CoreLibrary.PubPlus.Identity).toHex16();
                                fileStream = new fileStreamWriter(formValue.SaveFileName, FileMode.CreateNew, FileShare.None, FileOptions.None, false, null);
                            }
                            fileStream.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(Buffer, 0, valueEnterIndex > 0 ? valueEnterIndex : receiveEndIndex), PushPool = onWriteFile });
                            fileStream.WaitWriteBuffer();
                            return;
                        }
                        int length = receiveEndIndex - startIndex;
                        fixed (byte* bufferFixed = Buffer)
                        {
                            unsafer.memory.Copy(bufferFixed + startIndex, bufferFixed, length);
                        }
                        currentIndex -= startIndex;
                        valueEnterIndex -= startIndex;
                        receiveEndIndex = length;
                        startIndex = 0;
                    }
                    onReceiveData = onReceiveValue;
                    receive();
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                this.error();
            }
            /// <summary>
            /// 接收表单值处理
            /// </summary>
            private unsafe void checkValue()
            {
                int boundaryLength2 = boundary.Count + 2;
                fixed (byte* bufferFixed = Buffer, boundaryFixed = boundary.Array)
                {
                    byte* boundaryStart = boundaryFixed + boundary.StartIndex;
                    byte* start = bufferFixed + currentIndex, end = bufferFixed + receiveEndIndex, last = bufferFixed + valueEnterIndex;
                    *end-- = 13;
                    do
                    {
                        while (*start != 13) ++start;
                        if (start >= end) break;
                        if ((int)(start - last) == boundaryLength2 && *(short*)last == ('-') + ('-' << 8)
                            && fastCSharp.unsafer.memory.Equal(boundaryStart, last + 2, boundary.Count) && *(start + 1) == 10)
                        {
                            currentIndex = (int)(last - bufferFixed) - 2;
                            if (getValue())
                            {
                                startIndex = currentIndex = (int)(start - bufferFixed) + 2;
                                onEnter();
                            }
                            else error();
                            return;
                        }
                        last = *++start == 10 ? ++start : (bufferFixed - Buffer.Length);
                    }
                    while (true);
                    int hash = (*(int*)(end -= 3) ^ ('-') + ('-' << 8) + 0x0a0d0000);
                    if ((hash | (*(int*)(end -= boundary.Count + sizeof(int)) ^ 0x0a0d + ('-' << 16) + ('-' << 24))) == 0
                         && fastCSharp.unsafer.memory.Equal(boundaryStart, end + sizeof(int), boundary.Count))
                    {
                        currentIndex = (int)(end - bufferFixed);
                        if (getValue()) boundaryReceiverFinally();
                        else error();
                        return;
                    }
                    valueEnterIndex = (int)(last - bufferFixed);
                    currentIndex = (int)(start - bufferFixed);
                }
                receiveValue();
            }
            /// <summary>
            /// 文件流写入回调事件
            /// </summary>
            /// <param name="buffer">表单接收缓冲区</param>
            private unsafe void onFile(ref byte[] buffer)
            {
                if (fileStream.LastException == null)
                {
                    if (valueEnterIndex > 0)
                    {
                        fixed (byte* bufferFixed = buffer)
                        {
                            unsafer.memory.Copy(bufferFixed + valueEnterIndex, bufferFixed, receiveEndIndex -= valueEnterIndex);
                        }
                        valueEnterIndex = 0;
                    }
                    else
                    {
                        receiveEndIndex = 0;
                        valueEnterIndex = -buffer.Length;
                    }
                    startIndex = 0;
                    currentIndex = receiveEndIndex;
                    onReceiveData = onReceiveValue;
                    receive();
                }
                else error();
            }
            /// <summary>
            /// 获取表单值
            /// </summary>
            /// <returns>是否成功</returns>
            private unsafe bool getValue()
            {
                try
                {
                    if (fileStream == null)
                    {
                        byte[] value = new byte[currentIndex - startIndex];
                        System.Buffer.BlockCopy(Buffer, startIndex, value, 0, value.Length);
                        formValue.Value.UnsafeSet(value, 0, value.Length);
                    }
                    else
                    {
                        fileStream.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(Buffer, startIndex, currentIndex - startIndex) });
                        fileStream.Dispose();
                        fileStream = null;
                    }
                    if (formValue.FileName.Count == 0)
                    {
                        if (formValue.Name.Count == 1 && formValue.Name.array[formValue.Name.StartIndex] == fastCSharp.config.web.Default.QueryJsonName)
                        {
                            form.Parse(formValue.Value.array, formValue.Value.StartIndex, formValue.Value.Count, Encoding.UTF8);//showjim编码问题
                        }
                        else form.FormValues.Add(formValue);
                    }
                    else form.Files.Add(formValue);
                    return true;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                return false;
            }
        }
        /// <summary>
        /// 数据发送器
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        protected abstract class dataSender<socketType> where socketType : socketBase
        {
            /// <summary>
            /// HTTP套接字
            /// </summary>
            protected socketType socket;
            /// <summary>
            /// 发送数据起始时间
            /// </summary>
            protected DateTime sendStartTime;
            /// <summary>
            /// 正在发送的文件流
            /// </summary>
            protected FileStream fileStream;
            /// <summary>
            /// 待发送文件数据字节数
            /// </summary>
            protected long fileSize;
            /// <summary>
            /// 发送数据回调处理
            /// </summary>
            protected Action<bool> onSend;
            /// <summary>
            /// 缓冲区内存池
            /// </summary>
            protected memoryPool memoryPool;
            /// <summary>
            /// 发送数据缓冲区
            /// </summary>
            protected byte[] buffer;
            /// <summary>
            /// 发送数据起始位置
            /// </summary>
            protected int sendIndex;
            /// <summary>
            /// 发送数据结束位置
            /// </summary>
            protected int sendEndIndex;
            /// <summary>
            /// 已经发送数据长度
            /// </summary>
            protected long sendLength;
            /// <summary>
            /// 数据发送器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public dataSender(socketType socket)
            {
                this.socket = socket;
            }
            /// <summary>
            /// 发送数据完毕
            /// </summary>
            /// <param name="isSend">是否发送成功</param>
            protected void send(bool isSend)
            {
                if (memoryPool == null) buffer = null;
                else memoryPool.Push(ref buffer);
                onSend(isSend);
            }
            /// <summary>
            /// 读取文件并发送数据
            /// </summary>
            protected void readFile()
            {
                if (fileSize == 0) sendFile(true);
                else
                {
                    try
                    {
                        sendEndIndex = memoryPool.StreamBuffers.Size - sizeof(int);
                        if (sendEndIndex > fileSize) sendEndIndex = (int)fileSize;
                        if (fileStream.Read(buffer, 0, sendEndIndex) == sendEndIndex)
                        {
                            fileSize -= sendEndIndex;
                            sendIndex = 0;
                            sendFile();
                            return;
                        }
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    sendFile(false);
                }
            }
            /// <summary>
            /// 开始发送文件数据
            /// </summary>
            protected abstract void sendFile();
            /// <summary>
            /// 文件发送数据完毕
            /// </summary>
            /// <param name="isSend">是否发送成功</param>
            protected void sendFile(bool isSend)
            {
                pub.Dispose(ref fileStream);
                send(isSend);
            }
        }
        /// <summary>
        /// WebSocket请求接收器
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        protected abstract class webSocketIdentityReceiver<socketType> where socketType : socketBase
        {
            /// <summary>
            /// 操作编码
            /// </summary>
            public enum typeCode : byte
            {
                /// <summary>
                /// 连续消息片断
                /// </summary>
                Continuous = 0,
                /// <summary>
                /// 文本消息片断
                /// </summary>
                Text = 1,
                /// <summary>
                /// 二进制消息片断
                /// </summary>
                Binary = 2,
                /// <summary>
                /// 连接关闭
                /// </summary>
                Close = 8,
                /// <summary>
                /// 心跳检查的ping
                /// </summary>
                Ping = 9,
                /// <summary>
                /// 心跳检查的pong
                /// </summary>
                Pong = 10,
            }
            /// <summary>
            /// 请求信息
            /// </summary>
            private struct requestInfo
            {
                /// <summary>
                /// 请求编号
                /// </summary>
                public long Identity;
                /// <summary>
                /// AJAX回调函数
                /// </summary>
                public subString CallBack;
                /// <summary>
                /// 套接字请求编号
                /// </summary>
                public int SocketIdentity;
            }
            /// <summary>
            /// 关闭连接数据
            /// </summary>
            private static readonly byte[] closeData = new byte[] { 0x88, 0 };
            /// <summary>
            /// HTTP套接字
            /// </summary>
            protected socketType socket;
            /// <summary>
            /// 套接字访问锁
            /// </summary>
            private object socketLock = new object();
            /// <summary>
            /// 超时时间
            /// </summary>
            protected DateTime timeout;
            /// <summary>
            /// 开始接收数据
            /// </summary>
            protected Action receiveHandle;
            /// <summary>
            /// 表单接收缓冲区
            /// </summary>
            protected byte[] buffer;
            /// <summary>
            /// AJAX回调缓冲区
            /// </summary>
            private byte[] callBackBuffer;
            /// <summary>
            /// 接收数据结束位置
            /// </summary>
            protected int receiveEndIndex;
            /// <summary>
            /// 请求信息集合
            /// </summary>
            private list<requestInfo> requests = new list<requestInfo>();
            /// <summary>
            /// 当前请求编号
            /// </summary>
            private long currentIdentity;
            /// <summary>
            /// 请求编号访问锁
            /// </summary>
            private int identityLock;
            /// <summary>
            /// 套接字请求编号
            /// </summary>
            private int socketIdentity = 1;
            /// <summary>
            /// WebSocket请求接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public webSocketIdentityReceiver(socketType socket)
            {
                this.socket = socket;
                receiveHandle = receive;
                buffer = socket.Buffer;
                callBackBuffer = new byte[128];
                callBackBuffer[0] = 0x81;
            }
            /// <summary>
            /// 开始接收数据
            /// </summary>
            protected abstract void receive();
            /// <summary>
            /// 关闭连接
            /// </summary>
            protected void close()
            {
                Monitor.Enter(socketLock);
                try
                {
                    SocketError error = SocketError.Success;
                    socket.Socket.send(closeData, 0, 2, ref error);
                }
                catch { }
                finally
                {
                    Monitor.Exit(socketLock);
                    socket.webSocketEnd();
                }
            }
            /// <summary>
            /// 清除请求
            /// </summary>
            public void Clear()
            {
                Monitor.Enter(socketLock);
                ++socketIdentity;
                Monitor.Exit(socketLock);
            }
            /// <summary>
            /// 获取AJAX回调函数
            /// </summary>
            /// <param name="identity">操作标识</param>
            /// <returns>AJAX回调函数,失败返回null</returns>
            public subString GetCallBack(long identity)
            {
                interlocked.NoCheckCompareSetSleep0(ref identityLock);
                int count = requests.Count;
                if (count != 0)
                {
                    requestInfo[] requestArray = requests.array;
                    foreach (requestInfo request in requestArray)
                    {
                        if (request.Identity == identity)
                        {
                            identityLock = 0;
                            return request.CallBack;
                        }
                        if (--count == 0) break;
                    }
                }
                identityLock = 0;
                return default(subString);
            }
            /// <summary>
            /// 尝试解析数据
            /// </summary>
            protected unsafe void tryParse()
            {
                fixed (byte* bufferFixed = buffer)
                {
                    uint value = *(uint*)bufferFixed, code = value & 0xf;
                    if (code == (uint)(byte)typeCode.Close)
                    {
                        close();
                        return;
                    }
                    if ((value & 0xf0) == 0x80 && (code & 7) <= 2 && ((value & 0x8000) == 0x8000 || (value & 0xff00) == 0))
                    {
                        uint dataLength = ((value >> 8) & 0x7f);
                        if (dataLength <= 126)
                        {
                            uint dataIndex = (uint)((value & 0x8000) == 0 ? 2 : 6);
                            if (dataLength == 126)
                            {
                                dataLength = value >> 16;
                                dataIndex += 2;
                            }
                            uint packetLength = dataLength + dataIndex;
                            if (packetLength <= fastCSharp.config.http.Default.HeaderBufferLength)
                            {
                                if (receiveEndIndex >= packetLength)
                                {
                                    timeout = date.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                                    if (code == (uint)(byte)typeCode.Text && dataLength != 0)
                                    {
                                        byte* start = bufferFixed + dataIndex, end = start + ((dataLength + 3) & (uint.MaxValue - 3)), data = start;
                                        code = *(uint*)(start - sizeof(uint));
                                        do
                                        {
                                            *(uint*)data ^= code;
                                        }
                                        while ((data += sizeof(uint)) != end);
                                        for (data = start, *(end = start + dataLength) = (byte)'\n'; *data != '\n'; ++data) ;
                                        requestHeader requestHeader = socket.RequestHeader;
                                        if (requestHeader.SetWebSocketUrl(start, data))
                                        {
                                            subArray<byte> callBack = requestHeader.AjaxCallBackName;
                                            requestInfo request = new requestInfo { SocketIdentity = socketIdentity };
                                            if (data != end) ++data;
                                            dataLength = (uint)(int)(end - data);
                                            if (callBack.Count == 0)
                                            {
                                                if (dataLength != 0)
                                                {
                                                    try
                                                    {
                                                        requestHeader.WebSocketData = Encoding.UTF8.GetString(buffer, (int)(data - bufferFixed), (int)dataLength);
                                                    }
                                                    catch (Exception error)
                                                    {
                                                        request.Identity = long.MinValue;
                                                        log.Error.Add(error, null, false);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    string webSocketData = Encoding.UTF8.GetString(buffer, (int)(start - bufferFixed), (int)(end - start));
                                                    requestHeader.WebSocketData.UnsafeSet(webSocketData, (int)(data - start), (int)dataLength);
                                                    request.CallBack.UnsafeSet(webSocketData, callBack.StartIndex - requestHeader.EndIndex, callBack.Count);
                                                }
                                                catch (Exception error)
                                                {
                                                    request.Identity = long.MinValue;
                                                    log.Error.Add(error, null, false);
                                                }
                                            }
                                            if (request.Identity == 0) this.request(request);
                                            else response(callBack);
                                        }
                                    }
                                    if (receiveEndIndex > packetLength)
                                    {
                                        unsafer.memory.Copy(bufferFixed + packetLength, bufferFixed, receiveEndIndex -= (int)packetLength);
                                    }
                                }
                                receive();
                                return;
                            }
                        }
                    }
                }
                socket.webSocketEnd();
            }
            /// <summary>
            /// HTTP请求处理
            /// </summary>
            /// <param name="request">请求信息</param>
            private void request(requestInfo request)
            {
                interlocked.NoCheckCompareSetSleep0(ref identityLock);
                request.Identity = ++currentIdentity;
                try
                {
                    requests.Add(request);
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                    request.Identity = 0;
                }
                finally { identityLock = 0; }
                if (request.Identity != 0) socket.DomainServer.Server.Request(socket, request.Identity);
            }
            /// <summary>
            /// 错误输出
            /// </summary>
            /// <param name="callBack">AJAX回调函数</param>
            private unsafe void response(subArray<byte> callBack)
            {
                if (callBack.Count != 0 && callBack.Count <= 123)
                {
                    fixed (byte* bufferFixed = callBack.Array)
                    {
                        byte* start = bufferFixed + callBack.StartIndex;
                        *(short*)(start + callBack.Count) = '(' + (')' << 8);
                        *(short*)(start - sizeof(short)) = (short)(((callBack.Count + 2) << 8) + 0x81);
                        Monitor.Enter(socketLock);
                        try
                        {
                            SocketError error = SocketError.Success;
                            socket.Socket.send(callBack.Array, callBack.StartIndex - 2, callBack.Count + 4, ref error);
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                        }
                        finally { Monitor.Exit(socketLock); }
                    }
                }
            }
            /// <summary>
            /// 获取请求信息
            /// </summary>
            /// <param name="identity">HTTP操作标识</param>
            /// <returns>请求信息</returns>
            private unsafe requestInfo getRequest(long identity)
            {
                int index = 0;
                interlocked.NoCheckCompareSetSleep0(ref identityLock);
                int count = requests.Count;
                if (count != 0)
                {
                    requestInfo[] requestArray = requests.array;
                    foreach (requestInfo request in requestArray)
                    {
                        if (request.Identity == identity)
                        {
                            requests.Unsafer.AddLength(-1);
                            requestArray[index] = requestArray[requests.Count];
                            requestArray[requests.Count].CallBack.Null();
                            identityLock = 0;
                            return request;
                        }
                        if (--count == 0) break;
                        ++index;
                    }
                }
                identityLock = 0;
                return default(requestInfo);
            }
            /// <summary>
            /// 输出HTTP响应数据
            /// </summary>
            /// <param name="identity">HTTP操作标识</param>
            /// <param name="response">HTTP响应数据</param>
            public unsafe bool Response(long identity, ref response response)
            {
                fixed (byte* bufferFixed = callBackBuffer)
                {
                    Monitor.Enter(socketLock);
                    try
                    {
                        requestInfo request = getRequest(identity);
                        if (request.SocketIdentity != 0)
                        {
                            if (request.SocketIdentity == this.socketIdentity)
                            {
                                timeout = date.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                                if (response.State == http.response.state.Ok200)
                                {
                                    subArray<byte> body = response.Body;
                                    int length = body.Count;
                                    if (length == 0) this.response(request.CallBack);
                                    else
                                    {
                                        SocketError error = SocketError.Success;
                                        if (length > 125)
                                        {
                                            int bufferLength;
                                            if (length <= ushort.MaxValue)
                                            {
                                                *(bufferFixed + 1) = 126;
                                                *(ushort*)(bufferFixed + 2) = (ushort)length;
                                                bufferLength = 4;
                                            }
                                            else
                                            {
                                                *(bufferFixed + 1) = 127;
                                                *(long*)(bufferFixed + 2) = (long)length;
                                                bufferLength = 10;
                                            }
                                            if (socket.Socket.send(callBackBuffer, 0, bufferLength, ref error))
                                            {
                                                if (!socket.Socket.serverSend(body.Array, body.StartIndex, length, ref error)) socket.Socket.Close();
                                            }
                                        }
                                        else
                                        {
                                            *(bufferFixed + 1) = (byte)length;
                                            fixed (byte* bodyFixed = body.Array)
                                            {
                                                unsafer.memory.Copy(bodyFixed + body.StartIndex, bufferFixed + 2, length);
                                            }
                                            socket.Socket.send(callBackBuffer, 0, length + 2, ref error);
                                        }
                                    }
                                }
                                else if (response.State == http.response.state.NotChanged304) responseNotChanged(request.CallBack);
                                else this.response(request.CallBack);
                            }
                            return true;
                        }
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    finally
                    {
                        Monitor.Exit(socketLock);
                        response.Push(ref response);
                    }
                }
                return false;
            }
            /// <summary>
            /// 输出错误状态
            /// </summary>
            /// <param name="identity">操作标识</param>
            /// <param name="state">错误状态</param>
            public bool ResponseError(long identity, response.state state)
            {
                Monitor.Enter(socketLock);
                try
                {
                    requestInfo request = getRequest(identity);
                    if (request.SocketIdentity != 0)
                    {
                        if (request.SocketIdentity == this.socketIdentity)
                        {
                            timeout = date.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                            response(request.CallBack);
                        }
                        return true;
                    }
                }
                finally { Monitor.Exit(socketLock); }
                return false;
            }
            /// <summary>
            /// 错误输出
            /// </summary>
            /// <param name="callBack">AJAX回调函数</param>
            private unsafe void response(subString callBack)
            {
                if (callBack.Length != 0 && callBack.Length <= 125 - 4)
                {
                    fixed (byte* bufferFixed = callBackBuffer)
                    fixed (char* callBackFixed = callBack.value)
                    {
                        byte* start = bufferFixed + 2;
                        Monitor.Enter(socketLock);
                        try
                        {
                            *(bufferFixed + 1) = (byte)(callBack.Length + 2);
                            unsafer.String.WriteBytes(callBackFixed + callBack.StartIndex, callBack.Length, start);
                            *(short*)(start + callBack.Length) = '(' + (')' << 8);
                            SocketError error = SocketError.Success;
                            socket.Socket.send(callBackBuffer, 0, callBack.Length + 4, ref error);
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                        }
                        finally { Monitor.Exit(socketLock); }
                    }
                }
            }
            /// <summary>
            /// 输出空对象
            /// </summary>
            /// <param name="callBack">AJAX回调函数</param>
            private unsafe void responseNotChanged(subString callBack)
            {
                if (callBack.Length != 0 && callBack.Length + asynchronousMethod.ReturnParameterName.Length <= 125 - 9)
                {
                    fixed (byte* bufferFixed = callBackBuffer)
                    fixed (char* callBackFixed = callBack.value, returnFixed = asynchronousMethod.ReturnParameterName)
                    {
                        byte* start = bufferFixed + 2;
                        Monitor.Enter(socketLock);
                        try
                        {
                            *(bufferFixed + 1) = (byte)(callBack.Length + 7);
                            unsafer.String.WriteBytes(callBackFixed + callBack.StartIndex, callBack.Length, start);
                            *(short*)(start += callBack.Length) = '(' + ('{' << 8);
                            unsafer.String.WriteBytes(returnFixed, asynchronousMethod.ReturnParameterName.Length, start += sizeof(short));
                            *(start += asynchronousMethod.ReturnParameterName.Length) = (byte)':';
                            *(int*)(start + 1) = '{' + ('}' << 8) + ('}' << 16) + (')' << 24);
                            SocketError error = SocketError.Success;
                            socket.Socket.send(callBackBuffer, 0, callBack.Length + asynchronousMethod.ReturnParameterName.Length + 9, ref error);
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                        }
                        finally { Monitor.Exit(socketLock); }
                    }
                }
            }
        }
        /// <summary>
        /// 服务器类型
        /// </summary>
        private const string fastCSharpServer = @"Server: fastCSharp.http[C#]/1.0
";
        /// <summary>
        /// HTTP服务版本号
        /// </summary>
        private const string httpVersionString = "HTTP/1.1";
        /// <summary>
        /// HTTP每4秒最小表单数据接收字节数
        /// </summary>
        protected static readonly int minReceiveSizePerSecond4 = fastCSharp.config.http.Default.MinReceiveSizePerSecond * 4;
        /// <summary>
        /// 错误输出缓存数据
        /// </summary>
        protected static readonly byte[][] errorResponseDatas;
        /// <summary>
        /// HTTP服务版本号
        /// </summary>
        private static readonly byte[] httpVersion = httpVersionString.getBytes();
        /// <summary>
        /// 服务器类型
        /// </summary>
        private static readonly byte[] responseServer = fastCSharpServer.getBytes();
        /// <summary>
        /// 服务器类型
        /// </summary>
        private static readonly byte[] responseServerEnd = (fastCSharpServer + @"Content-Length: 0

").getBytes();
        /// <summary>
        /// 100 Continue确认
        /// </summary>
        private static readonly byte[] continue100 = (httpVersionString + fastCSharp.Enum<response.state, response.stateInfo>.Array(response.state.Continue100).Text + @"
").getBytes();
        /// <summary>
        /// WebSocket握手确认
        /// </summary>
        private static readonly byte[] webSocket101 = (httpVersionString + fastCSharp.Enum<response.state, response.stateInfo>.Array(response.state.WebSocket101).Text + @"Connection: Upgrade
Upgrade: WebSocket
" + fastCSharpServer + @"Sec-WebSocket-Accept: ").getBytes();
        /// <summary>
        /// WebSocket确认哈希值
        /// </summary>
        private static readonly byte[] webSocketKey = ("258EAFA5-E914-47DA-95CA-C5AB0DC85B11").getBytes();
        /// <summary>
        /// HTTP响应输出内容长度名称
        /// </summary>
        private static readonly byte[] contentLengthResponseName = (header.ContentLength + ": ").getBytes();
        /// <summary>
        /// HTTP响应输出日期名称
        /// </summary>
        private static readonly byte[] dateResponseName = (header.Date + ": ").getBytes();
        /// <summary>
        /// HTTP响应输出最后修改名称
        /// </summary>
        private static readonly byte[] lastModifiedResponseName = (header.LastModified + ": ").getBytes();
        /// <summary>
        /// 重定向名称
        /// </summary>
        private static readonly byte[] locationResponseName = (header.Location + ": ").getBytes();
        /// <summary>
        /// 缓存参数名称
        /// </summary>
        private static readonly byte[] cacheControlResponseName = (header.CacheControl + ": ").getBytes();
        /// <summary>
        /// 内容类型名称
        /// </summary>
        private static readonly byte[] contentTypeResponseName = (header.ContentType + ": ").getBytes();
        /// <summary>
        /// 内容压缩编码名称
        /// </summary>
        private static readonly byte[] contentEncodingResponseName = (header.ContentEncoding + ": ").getBytes();
        /// <summary>
        /// 缓存匹配标识名称
        /// </summary>
        private static readonly byte[] eTagResponseName = (header.ETag + @": """).getBytes();
        /// <summary>
        /// 内容描述名称
        /// </summary>
        private static readonly byte[] contentDispositionResponseName = (header.ContentDisposition + ": ").getBytes();
        /// <summary>
        /// 请求范围名称
        /// </summary>
        private static readonly byte[] rangeName = (header.AcceptRanges + @": bytes
" + header.ContentRange + ": bytes ").getBytes();
        /// <summary>
        /// HTTP响应输出保持连接
        /// </summary>
        private static readonly byte[] defaultKeepAlive = (header.Connection + @": Keep-Alive
").getBytes();
        /// <summary>
        /// HTTP响应输出Cookie名称
        /// </summary>
        private static readonly byte[] setCookieResponseName = (header.SetCookie + ": ").getBytes();
        /// <summary>
        /// Cookie域名
        /// </summary>
        private static readonly byte[] cookieDomainName = ("; Domain=").getBytes();
        /// <summary>
        /// Cookie有效路径
        /// </summary>
        private static readonly byte[] cookiePathName = ("; Path=").getBytes();
        /// <summary>
        /// Cookie有效期
        /// </summary>
        private static readonly byte[] cookieExpiresName = ("; Expires=").getBytes();
        /// <summary>
        /// Cookie安全标识
        /// </summary>
        private static readonly byte[] cookieSecureName = ("; Secure").getBytes();
        /// <summary>
        /// Cookie是否http only
        /// </summary>
        private static readonly byte[] cookieHttpOnlyName = ("; HttpOnly").getBytes();
        /// <summary>
        /// Cookie最小时间超时时间
        /// </summary>
        private static readonly byte[] pubMinTimeCookieExpires = pub.MinTime.toBytes();
        /// <summary>
        /// 最后一次生成的时间字节数组
        /// </summary>
        private static keyValue<long, byte[]> dateCache = new keyValue<long, byte[]>(0, new byte[(date.ToByteLength + 3) & (int.MaxValue - 3)]);
        /// <summary>
        /// 时间字节数组访问锁
        /// </summary>
        private static int dateCacheLock;
        /// <summary>
        /// 获取当前时间字节数组
        /// </summary>
        /// <param name="data">输出数据起始位置</param>
        private static unsafe void getDate(byte* data)
        {
            DateTime now = date.NowSecond;
            long second = now.Ticks / 10000000;
            fixed (byte* cacheFixed = dateCache.Value)
            {
                interlocked.NoCheckCompareSetSleep0(ref dateCacheLock);
                try
                {
                    if (dateCache.Key != second)
                    {
                        dateCache.Key = second;
                        date.ToBytes(now, cacheFixed);
                    }
                    unsafer.memory.Copy(cacheFixed, data, dateCache.Value.Length);
                }
                finally { dateCacheLock = 0; }
            }
        }
        /// <summary>
        /// 发送数据后关闭套接字
        /// </summary>
        protected Action<bool> sendClose;
        /// <summary>
        /// 发送数据后处理下一个请求
        /// </summary>
        protected Action<bool> sendNext;
        /// <summary>
        /// 发送数据后继续发送HTTP响应内容
        /// </summary>
        protected Action<bool> sendResponseBody;
        /// <summary>
        /// 当前输出HTTP响应
        /// </summary>
        protected response response;
        /// <summary>
        /// 当前输出HTTP响应字节数
        /// </summary>
        protected long responseSize;
        /// <summary>
        /// HTTP内容数据缓冲区
        /// </summary>
        internal byte[] Buffer;
        /// <summary>
        /// 获取HTTP请求头部
        /// </summary>
        internal abstract requestHeader RequestHeader { get; }
        /// <summary>
        /// HTTP请求表单
        /// </summary>
        protected requestForm form;
        /// <summary>
        /// HTTP服务器
        /// </summary>
        protected servers servers;
        /// <summary>
        /// 套接字
        /// </summary>
        internal Socket Socket;
        /// <summary>
        /// 超时标识
        /// </summary>
        protected int timeoutIdentity;
        /// <summary>
        /// 超时检测
        /// </summary>
        private Func<int, bool> checkTimeoutHandle;
        /// <summary>
        /// TCP调用套接字
        /// </summary>
        public commandSocket TcpCommandSocket { get; protected set; }
        /// <summary>
        /// 远程终结点
        /// </summary>
        internal EndPoint RemoteEndPoint
        {
            get { return Socket.RemoteEndPoint; }
        }
        /// <summary>
        /// 客户端IP地址
        /// </summary>
        protected int ipv4;
        /// <summary>
        /// 客户端IP地址
        /// </summary>
        protected ipv6Hash ipv6;
        /// <summary>
        /// 域名服务
        /// </summary>
        internal servers.domainServer DomainServer;
        /// <summary>
        /// 获取Session
        /// </summary>
        internal ISession Session { get { return DomainServer.Server.Session; } }
        /// <summary>
        /// 操作标识
        /// </summary>
        protected long identity;
        /// <summary>
        /// 是否加载表单
        /// </summary>
        protected byte isLoadForm;
        /// <summary>
        /// 是否正在处理下一个请求
        /// </summary>
        protected byte isNextRequest;
        /// <summary>
        /// HTTP套接字
        /// </summary>
        protected socketBase()
        {
            Buffer = new byte[fastCSharp.config.http.Default.HeaderBufferLength + sizeof(int)];
            form = new requestForm();
            sendClose = close;
            sendNext = next;
            sendResponseBody = responseBody;
            checkTimeoutHandle = checkTimeout;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            form.Clear();
            response.Push(ref response);
        }
        /// <summary>
        /// HTTP头部接收错误
        /// </summary>
        protected abstract void headerError();
        /// <summary>
        /// WebSocket结束
        /// </summary>
        protected abstract void webSocketEnd();
        /// <summary>
        /// 处理下一个请求
        /// </summary>
        /// <param name="isSend">是否发送成功</param>
        private void next(bool isSend)
        {
            response.Push(ref response);
            if (isSend)
            {
                form.Clear();
                if (RequestHeader.IsKeepAlive)
                {
                    isNextRequest = 1;
                    isLoadForm = 0;
                    receiveHeader();
                    return;
                }
            }
            headerError();
        }
        /// <summary>
        /// 开始接收头部数据
        /// </summary>
        protected abstract void receiveHeader();
        /// <summary>
        /// 未能识别的HTTP头部
        /// </summary>
        protected abstract void headerUnknown();
        /// <summary>
        /// 获取域名服务信息
        /// </summary>
        private void request()
        {
            long identity = this.identity;
            try
            {
                if (RequestHeader.IsWebSocket) responseWebSocket101();
                else DomainServer.Server.Request(this, identity);
                return;
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            ResponseError(identity, response.state.ServerError500);
        }
        /// <summary>
        /// 检测100 Continue确认
        /// </summary>
        protected bool check100Continue()
        {
            if (RequestHeader.Is100Continue)
            {
                RequestHeader.Is100Continue = false;
                try
                {
                    SocketError error = SocketError.Success;
                    if (Socket.send(continue100, 0, continue100.Length, ref error)) return true;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
            }
            else return true;
            return false;
        }
        /// <summary>
        /// 获取请求表单数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="loadForm">HTTP请求表单加载接口</param>
        internal abstract void GetForm(long identity, requestForm.ILoadForm loadForm);
        /// <summary>
        /// 输出HTTP响应数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="response">HTTP响应数据</param>
        public abstract bool Response(long identity, ref response response);
        /// <summary>
        /// 输出HTTP响应数据
        /// </summary>
        /// <param name="identity">HTTP操作标识</param>
        /// <param name="response">HTTP响应数据</param>
        internal unsafe bool Response(long identity, response response)
        {
            return Response(identity, ref response);
        }
        /// <summary>
        /// 输出错误状态
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <param name="state">错误状态</param>
        public bool ResponseError(long identity, response.state state)
        {
            if (identity >= 0)
            {
                if (Interlocked.CompareExchange(ref this.identity, identity + 1, identity) == identity)
                {
                    responseError(state);
                    return true;
                }
                return false;
            }
            return WebSocketResponseError(identity, state);
        }
        /// <summary>
        /// 输出错误状态
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <param name="state">错误状态</param>
        internal abstract bool WebSocketResponseError(long identity, response.state state);
        /// <summary>
        /// 输出错误状态
        /// </summary>
        /// <param name="state">错误状态</param>
        protected abstract void responseError(response.state state);
        /// <summary>
        /// 表单回调处理
        /// </summary>
        /// <param name="form">HTTP请求表单</param>
        public void OnGetForm(requestForm form)
        {
            if (form == null) responseHeader();
            else
            {
                response.Push(ref response);
                headerError();
            }
        }
        /// <summary>
        /// 根据HTTP请求表单值获取内存流最大字节数
        /// </summary>
        /// <param name="value">HTTP请求表单值</param>
        /// <returns>内存流最大字节数</returns>
        public int MaxMemoryStreamSize(fastCSharp.net.tcp.http.requestForm.value value) { return 0; }
        /// <summary>
        /// 根据HTTP请求表单值获取保存文件全称
        /// </summary>
        /// <param name="value">HTTP请求表单值</param>
        /// <returns>文件全称</returns>
        public string GetSaveFileName(fastCSharp.net.tcp.http.requestForm.value value) { return null; }
        /// <summary>
        /// HTTP响应头部输出
        /// </summary>
        protected unsafe void responseHeader()
        {
            try
            {
                requestHeader requestHeader = RequestHeader;
                if (response.Body.Count != 0)
                {
                    response.BodyFile = null;
                    if (requestHeader.IsKeepAlive && response.CanHeader && !requestHeader.IsRange)
                    {
                        subArray<byte> body = response.HeaderBody;
                        if (body.Count != 0)
                        {
                            responseSize = 0;
                            responseHeader(body, null);
                            return;
                        }
                    }
                }
                long bodySize = response.BodySize;
                byte* responseSizeFixed = stackalloc byte[24 * 4], bodySizeFixed = responseSizeFixed + 24;
                byte* rangeStartFixed = responseSizeFixed + 24 * 2, rangeEndFixed = responseSizeFixed + 24 * 3;
                byte* responseSizeWrite = responseSizeFixed, bodySizeWrite = responseSizeFixed;
                byte* rangeStartWrite = rangeStartFixed, rangeEndWrite = rangeEndFixed;
                if (requestHeader.IsRange)
                {
                    if (requestHeader.IsFormatRange || requestHeader.FormatRange(bodySize))
                    {
                        if (response.State == response.state.Ok200)
                        {
                            responseSize = requestHeader.RangeLength;
                            if (requestHeader.RangeStart != 0) rangeStartWrite = sizeToBytes(requestHeader.RangeStart, rangeStartFixed);
                            if (requestHeader.RangeEnd != bodySize - 1) rangeEndWrite = zeroSizeToBytes(requestHeader.RangeEnd, rangeEndFixed);
                            bodySizeWrite = zeroSizeToBytes(bodySize, bodySizeFixed);
                            response.State = response.state.PartialContent206;
                        }
                        else responseSize = bodySize;
                    }
                    else
                    {
                        response.State = http.response.state.RangeNotSatisfiable416;
                        responseSize = 0;
                    }
                }
                else responseSize = bodySize;
                responseSizeWrite = zeroSizeToBytes(responseSize, responseSizeFixed);
                response.stateInfo state = fastCSharp.Enum<response.state, response.stateInfo>.Array(response.State);
                if (state == null) state = fastCSharp.Enum<response.state, response.stateInfo>.Array(response.state.ServerError500);
                int index = httpVersion.Length + state.Bytes.Length + contentLengthResponseName.Length + (int)(responseSizeWrite - responseSizeFixed) + 2
                    + responseServer.Length + dateResponseName.Length + date.ToByteLength + 2 + 2;
                if (response.State == response.state.PartialContent206)
                {
                    index += rangeName.Length + (int)(rangeStartWrite - rangeStartFixed) + (int)(rangeEndWrite - rangeEndFixed) + (int)(bodySizeWrite - bodySizeFixed) + 2 + 2;
                }
                if (response.Location.Count != 0) index += locationResponseName.Length + response.Location.Count + 2;
                if (response.LastModified != null) index += lastModifiedResponseName.Length + response.LastModified.Length + 2;
                if (response.CacheControl != null) index += cacheControlResponseName.Length + response.CacheControl.Length + 2;
                if (response.ContentType != null) index += contentTypeResponseName.Length + response.ContentType.Length + 2;
                if (response.ContentEncoding != null) index += contentEncodingResponseName.Length + response.ContentEncoding.Length + 2;
                if (response.ETag != null) index += eTagResponseName.Length + response.ETag.Length + 2 + 1;
                if (response.ContentDisposition != null) index += contentDispositionResponseName.Length + response.ContentDisposition.Length + 2;
                if (requestHeader.IsKeepAlive) index += defaultKeepAlive.Length;
                int cookieCount = response.Cookies.Count;
                if (cookieCount != 0)
                {
                    index += (setCookieResponseName.Length + 3) * cookieCount;
                    foreach (cookie cookie in response.Cookies.array)
                    {
                        index += cookie.Name.Length + cookie.Value.length();
                        if (cookie.Domain.Count != 0) index += cookieDomainName.Length + cookie.Domain.Count;
                        if (cookie.Path != null) index += cookiePathName.Length + cookie.Path.Length;
                        if (cookie.Expires != DateTime.MinValue) index += cookieExpiresName.Length + date.ToByteLength;
                        if (cookie.IsSecure) index += cookieSecureName.Length;
                        if (cookie.IsHttpOnly) index += cookieHttpOnlyName.Length;
                        if (--cookieCount == 0) break;
                    }
                }
                int checkIndex = index;
                byte[] buffer;
                memoryPool memoryPool;
                if ((index += 3) <= (fastCSharp.config.http.Default.HeaderBufferLength + 4))
                {
                    buffer = this.Buffer;
                    memoryPool = null;
                }
                else
                {
                    memoryPool = getMemoryPool(index);
                    buffer = memoryPool.Get(index);
                }
                fixed (byte* bufferFixed = buffer)
                {
                    System.Buffer.BlockCopy(httpVersion, 0, buffer, 0, index = httpVersion.Length);
                    System.Buffer.BlockCopy(state.Bytes, 0, buffer, index, state.Bytes.Length);
                    System.Buffer.BlockCopy(contentLengthResponseName, 0, buffer, index += state.Bytes.Length, contentLengthResponseName.Length);
                    byte* write = bufferFixed + (index += contentLengthResponseName.Length);
                    index += (int)(responseSizeWrite - responseSizeFixed) + 2;
                    while (responseSizeWrite != responseSizeFixed) *write++ = *--responseSizeWrite;
                    *(short*)write = 0x0a0d;
                    if (response.State == response.state.PartialContent206)
                    {
                        System.Buffer.BlockCopy(rangeName, 0, buffer, index, rangeName.Length);
                        write = bufferFixed + (index += rangeName.Length);
                        index += (int)(rangeStartWrite - rangeStartFixed) + (int)(rangeEndWrite - rangeEndFixed) + (int)(bodySizeWrite - bodySizeFixed) + 2 + 2;
                        while (rangeStartWrite != rangeStartFixed) *write++ = *--rangeStartWrite;
                        *write++ = (byte)'-';
                        while (rangeEndWrite != rangeEndFixed) *write++ = *--rangeEndWrite;
                        *write++ = (byte)'/';
                        while (bodySizeWrite != bodySizeFixed) *write++ = *--bodySizeWrite;
                        *(short*)write = 0x0a0d;
                    }
                    if (response.Location.Count != 0)
                    {
                        System.Buffer.BlockCopy(locationResponseName, 0, buffer, index, locationResponseName.Length);
                        System.Buffer.BlockCopy(response.Location.Array, response.Location.StartIndex, buffer, index += locationResponseName.Length, response.Location.Count);
                        *(short*)(bufferFixed + (index += response.Location.Count)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.CacheControl != null)
                    {
                        System.Buffer.BlockCopy(cacheControlResponseName, 0, buffer, index, cacheControlResponseName.Length);
                        System.Buffer.BlockCopy(response.CacheControl, 0, buffer, index += cacheControlResponseName.Length, response.CacheControl.Length);
                        *(short*)(bufferFixed + (index += response.CacheControl.Length)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.ContentType != null)
                    {
                        System.Buffer.BlockCopy(contentTypeResponseName, 0, buffer, index, contentTypeResponseName.Length);
                        System.Buffer.BlockCopy(response.ContentType, 0, buffer, index += contentTypeResponseName.Length, response.ContentType.Length);
                        *(short*)(bufferFixed + (index += response.ContentType.Length)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.ContentEncoding != null)
                    {
                        System.Buffer.BlockCopy(contentEncodingResponseName, 0, buffer, index, contentEncodingResponseName.Length);
                        System.Buffer.BlockCopy(response.ContentEncoding, 0, buffer, index += contentEncodingResponseName.Length, response.ContentEncoding.Length);
                        *(short*)(bufferFixed + (index += response.ContentEncoding.Length)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.ETag != null)
                    {
                        System.Buffer.BlockCopy(eTagResponseName, 0, buffer, index, eTagResponseName.Length);
                        System.Buffer.BlockCopy(response.ETag, 0, buffer, index += eTagResponseName.Length, response.ETag.Length);
                        *(bufferFixed + (index += response.ETag.Length)) = (byte)'"';
                        *(short*)(bufferFixed + (++index)) = 0x0a0d;
                        index += 2;
                    }
                    if (response.ContentDisposition != null)
                    {
                        System.Buffer.BlockCopy(contentDispositionResponseName, 0, buffer, index, contentDispositionResponseName.Length);
                        System.Buffer.BlockCopy(response.ContentDisposition, 0, buffer, index += contentDispositionResponseName.Length, response.ContentDisposition.Length);
                        *(short*)(bufferFixed + (index += response.ContentDisposition.Length)) = 0x0a0d;
                        index += 2;
                    }
                    if ((cookieCount = response.Cookies.Count) != 0)
                    {
                        foreach (cookie cookie in response.Cookies.array)
                        {
                            System.Buffer.BlockCopy(setCookieResponseName, 0, buffer, index, setCookieResponseName.Length);
                            System.Buffer.BlockCopy(cookie.Name, 0, buffer, index += setCookieResponseName.Length, cookie.Name.Length);
                            *(bufferFixed + (index += cookie.Name.Length)) = (byte)'=';
                            ++index;
                            if (cookie.Value.length() != 0)
                            {
                                System.Buffer.BlockCopy(cookie.Value, 0, buffer, index, cookie.Value.Length);
                                index += cookie.Value.Length;
                            }
                            if (cookie.Domain.Count != 0)
                            {
                                System.Buffer.BlockCopy(cookieDomainName, 0, buffer, index, cookieDomainName.Length);
                                System.Buffer.BlockCopy(cookie.Domain.Array, cookie.Domain.StartIndex, buffer, index += cookieDomainName.Length, cookie.Domain.Count);
                                index += cookie.Domain.Count;
                            }
                            if (cookie.Path != null)
                            {
                                System.Buffer.BlockCopy(cookiePathName, 0, buffer, index, cookiePathName.Length);
                                System.Buffer.BlockCopy(cookie.Path, 0, buffer, index += cookiePathName.Length, cookie.Path.Length);
                                index += cookie.Path.Length;
                            }
                            if (cookie.Expires != DateTime.MinValue)
                            {
                                System.Buffer.BlockCopy(cookieExpiresName, 0, buffer, index, cookieExpiresName.Length);
                                index += cookieExpiresName.Length;
                                if (cookie.Expires == pub.MinTime)
                                {
                                    System.Buffer.BlockCopy(pubMinTimeCookieExpires, 0, buffer, index, pubMinTimeCookieExpires.Length);
                                }
                                else date.ToBytes(cookie.Expires, bufferFixed + index);
                                index += date.ToByteLength;
                            }
                            if (cookie.IsSecure)
                            {
                                System.Buffer.BlockCopy(cookieSecureName, 0, buffer, index, cookieSecureName.Length);
                                index += cookieSecureName.Length;
                            }
                            if (cookie.IsHttpOnly)
                            {
                                System.Buffer.BlockCopy(cookieHttpOnlyName, 0, buffer, index, cookieHttpOnlyName.Length);
                                index += cookieHttpOnlyName.Length;
                            }
                            *(short*)(bufferFixed + index) = 0x0a0d;
                            index += 2;
                            if (--cookieCount == 0) break;
                        }
                    }
                    System.Buffer.BlockCopy(responseServer, 0, buffer, index, responseServer.Length);
                    index += responseServer.Length;
                    if (requestHeader.IsKeepAlive)
                    {
                        System.Buffer.BlockCopy(defaultKeepAlive, 0, buffer, index, defaultKeepAlive.Length);
                        index += defaultKeepAlive.Length;
                    }
                    if (response.LastModified != null)
                    {
                        System.Buffer.BlockCopy(lastModifiedResponseName, 0, buffer, index, lastModifiedResponseName.Length);
                        System.Buffer.BlockCopy(response.LastModified, 0, buffer, index += lastModifiedResponseName.Length, response.LastModified.Length);
                        *(short*)(bufferFixed + (index += response.LastModified.Length)) = 0x0a0d;
                        index += 2;
                    }
                    System.Buffer.BlockCopy(dateResponseName, 0, buffer, index, dateResponseName.Length);
                    getDate(bufferFixed + (index += dateResponseName.Length));
                    *(int*)(bufferFixed + (index += date.ToByteLength)) = 0x0a0d0a0d;
                    index += 4;
                    //if (checkIndex != index) log.Default.Add("responseHeader checkIndex[" + checkIndex.toString() + "] != index[" + index.toString() + "]", true, false);
                    if (response.Body.Count != 0)
                    {
                        if (requestHeader.IsKeepAlive && response.CanHeader && (index + sizeof(int)) <= response.Body.StartIndex && !requestHeader.IsRange)
                        {
                            fixed (byte* bodyFixed = response.Body.array)
                            {
                                unsafer.memory.Copy(bufferFixed, bodyFixed + response.Body.StartIndex - index, index);
                                *(int*)bodyFixed = index;
                            }
                            responseSize = 0;
                            responseHeader(response.HeaderBody, null);
                            return;
                        }
                        if (buffer.Length - index >= (int)responseSize)
                        {
                            //if (response.Body.Length != responseSize) log.Default.Add("response.Body.Length[" + response.Body.Length.toString() + "] != responseSize[" + responseSize.toString() + "]", true, false);
                            System.Buffer.BlockCopy(response.Body.Array, response.State == response.state.PartialContent206 ? response.Body.StartIndex + (int)requestHeader.RangeStart : response.Body.StartIndex, buffer, index, (int)responseSize);
                            index += (int)responseSize;
                            responseSize = 0;
                        }
                        ////showjim
                        //else if (response.Body.Count > response.Body.array.Length)
                        //{
                        //    fixed (byte* headerBufferFixed = requestHeader.Buffer)
                        //    {
                        //        log.Error.Add(fastCSharp.String.DeSerialize(headerBufferFixed + requestHeader.Uri.StartIndex, -2 - requestHeader.Uri.Count) + fastCSharp.String.DeSerialize(bufferFixed, -index) + response.Body.Count.toString() + " > " + response.Body.array.Length.toString(), true, false);
                        //    }
                        //}
                    }
                }
                responseHeader(subArray<byte>.Unsafe(buffer, 0, index), memoryPool);
                return;
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
                fixed (byte* headerBufferFixed = RequestHeader.Buffer)
                {
                    log.Error.Add(@"responseSize[" + responseSize.toString() + "] body[" + response.Body.array.Length.toString() + "," + response.Body.StartIndex.toString() + "," + response.Body.Count.toString() + @"]
" + fastCSharp.String.DeSerialize(headerBufferFixed, -RequestHeader.EndIndex), true, false);
                }
            }
            headerError();
        }
        /// <summary>
        /// HTTP响应头部输出
        /// </summary>
        /// <param name="buffer">输出数据</param>
        /// <param name="memoryPool">内存池</param>
        protected abstract void responseHeader(subArray<byte> buffer, memoryPool memoryPool);
        /// <summary>
        /// 获取AJAX回调函数
        /// </summary>
        /// <param name="identity">操作标识</param>
        /// <returns>AJAX回调函数,失败返回null</returns>
        internal abstract subString GetWebSocketCallBack(long identity);
        /// <summary>
        /// WebSocket响应协议输出
        /// </summary>
        private unsafe void responseWebSocket101()
        {
            int index = webSocket101.Length;
            System.Buffer.BlockCopy(webSocket101, 0, Buffer, 0, index);
            subArray<byte> secWebSocketKey = RequestHeader.SecWebSocketKey;
            System.Buffer.BlockCopy(secWebSocketKey.Array, secWebSocketKey.StartIndex, Buffer, index, secWebSocketKey.Count);
            System.Buffer.BlockCopy(webSocketKey, 0, Buffer, index + secWebSocketKey.Count, webSocketKey.Length);
            byte[] acceptKey = pub.Sha1(Buffer, index, secWebSocketKey.Count + webSocketKey.Length);
            fixed (byte* bufferFixed = Buffer, acceptKeyFixed = acceptKey)
            {
                byte* write = bufferFixed + webSocket101.Length, keyEnd = acceptKeyFixed + 18, base64 = String.Base64.Byte;
                for (byte* read = acceptKeyFixed; read != keyEnd; read += 3)
                {
                    *write++ = *(base64 + (*read >> 2));
                    *write++ = *(base64 + (((*read << 4) | (*(read + 1) >> 4)) & 0x3f));
                    *write++ = *(base64 + (((*(read + 1) << 2) | (*(read + 2) >> 6)) & 0x3f));
                    *write++ = *(base64 + (*(read + 2) & 0x3f));
                }
                *write++ = *(base64 + (*keyEnd >> 2));
                *write++ = *(base64 + (((*keyEnd << 4) | (*(keyEnd + 1) >> 4)) & 0x3f));
                *write++ = *(base64 + ((*(keyEnd + 1) << 2) & 0x3f));
                *write++ = (byte)'=';
                *(int*)write = 0x0a0d0a0d;
            }
            SocketError error = SocketError.Success;
            if (Socket.send(Buffer, 0, index += 32, ref error))
            {
                Interlocked.Increment(ref identity);
                receiveWebSocket();
            }
            else ResponseError(identity, response.state.ServerError500);
        }
        /// <summary>
        /// 开始接收WebSocket数据
        /// </summary>
        protected abstract void receiveWebSocket();
        /// <summary>
        /// 发送HTTP响应内容
        /// </summary>
        /// <param name="isSend">是否发送成功</param>
        protected abstract void responseBody(bool isSend);
        /// <summary>
        /// 关闭套接字
        /// </summary>
        /// <param name="isSend">是否发送成功</param>
        private void close(bool isSend)
        {
            headerError();
        }
        /// <summary>
        /// 设置超时
        /// </summary>
        /// <param name="identity">超时标识</param>
        protected void setTimeout(int identity)
        {
            if (identity == timeoutIdentity) ReceiveTimeoutQueue.Add(Socket, checkTimeoutHandle, identity);
        }
        /// <summary>
        /// 超时检测
        /// </summary>
        /// <param name="identity">超时标识</param>
        /// <returns>是否超时</returns>
        private bool checkTimeout(int identity)
        {
            return identity == timeoutIdentity;
        }
        /// <summary>
        /// 关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        protected static void close(Socket socket)
        {
            if (socket != null)
            {
                if (!socket.Connected)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception error)
                    {
                        log.Default.Add(error, null, false);
                    }
                }
                socket.Close();
            }
        }
        /// <summary>
        /// 长度数字转换为字节串
        /// </summary>
        /// <param name="size">长度数字,不能为0</param>
        /// <param name="start">字节串起始位置</param>
        /// <returns>字节串结束位置</returns>
        private static unsafe byte* sizeToBytes(long size, byte* start)
        {
            if (size <= int.MaxValue)
            {
                int size32 = (int)size;
                for (*start++ = (byte)((size32 % 10) + '0'); (size32 /= 10) != 0; *start++ = (byte)((size32 % 10) + '0')) ;
            }
            else
            {
                for (*start++ = (byte)((size % 10) + '0'); (size /= 10) != 0; *start++ = (byte)((size % 10) + '0')) ;
            }
            return start;
        }
        /// <summary>
        /// 长度数字转换为字节串
        /// </summary>
        /// <param name="size">长度数字,可能为0</param>
        /// <param name="start">字节串起始位置</param>
        /// <returns>字节串结束位置</returns>
        private static unsafe byte* zeroSizeToBytes(long size, byte* start)
        {
            if (size == 0)
            {
                *start = (byte)'0';
                return start + 1;
            }
            return sizeToBytes(size, start);
        }
        static socketBase()
        {
            errorResponseDatas = new byte[Enum.GetMaxValue<response.state>(-1) + 1][];
            foreach (response.state type in System.Enum.GetValues(typeof(response.state)))
            {
                response.stateInfo state = fastCSharp.Enum<response.state, response.stateInfo>.Array((int)type);
                if (state != null && state.IsError)
                {
                    byte[] stateData = state.Bytes, responseData = new byte[httpVersion.Length + stateData.Length + responseServerEnd.Length];
                    System.Buffer.BlockCopy(httpVersion, 0, responseData, 0, httpVersion.Length);
                    int index = httpVersion.Length;
                    System.Buffer.BlockCopy(stateData, 0, responseData, index, stateData.Length);
                    index += stateData.Length;
                    System.Buffer.BlockCopy(responseServerEnd, 0, responseData, index, responseServerEnd.Length);
                    errorResponseDatas[(int)type] = responseData;
                }
            }
        }
    }
    /// <summary>
    /// HTTP套接字
    /// </summary>
    internal sealed class socket : socketBase
    {
        /// <summary>
        /// HTTP头部接收器
        /// </summary>
        internal sealed class headerReceiver : headerReceiver<socket>, IDisposable
        {
            /// <summary>
            /// 异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;
            /// <summary>
            /// HTTP头部接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public headerReceiver(socket socket)
                : base(socket)
            {
                Async = socketAsyncEventArgs.Get();
                Async.SocketFlags = System.Net.Sockets.SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += receive;
                Async.UserToken = this;
                Async.SetBuffer(buffer, 0, buffer.Length);
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= receive;
                socketAsyncEventArgs.Push(ref Async);
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
                    Async.SocketError = SocketError.Success;
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
            /// 开始接收数据(用于TCP调用)
            /// </summary>
            /// <param name="data">已接受数据</param>
            public unsafe void Receive(byte[] data)
            {
                timeout = date.NowSecond.AddTicks(ReceiveTimeoutQueue.CallbackTimeoutTicks);
                HeaderEndIndex = 0;
                fixed (byte* dataFixed = data, bufferFixed = buffer) *(int*)bufferFixed = *(int*)dataFixed;
                ReceiveEndIndex = sizeof(int);

                Async.SocketError = SocketError.Success;
                receive();
            }
            /// <summary>
            /// 接受头部换行数据
            /// </summary>
            protected override void receive()
            {
                try
                {
                    int timeoutIdentity = socket.timeoutIdentity;
                    Async.SetBuffer(ReceiveEndIndex, fastCSharp.config.http.Default.HeaderBufferLength - ReceiveEndIndex);
                    if (socket.Socket.ReceiveAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
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
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private unsafe void receive(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                int count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0) ReceiveEndIndex += count;
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
        private sealed class formIdentityReceiver : formIdentityReceiver<socket>, IDisposable
        {
            /// <summary>
            /// 异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;
            /// <summary>
            /// HTTP表单接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public formIdentityReceiver(socket socket)
                : base(socket)
            {
                Async = socketAsyncEventArgs.Get();
                Async.SocketFlags = System.Net.Sockets.SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += receive;
                Async.UserToken = this;
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= receive;
                socketAsyncEventArgs.Push(ref Async);
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
                    Async.SocketError = SocketError.Success;
                    Async.SetBuffer(buffer, 0, buffer.Length);
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
                    Async.SetBuffer(receiveEndIndex, contentLength - receiveEndIndex);
                    if (socket.Socket.ReceiveAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
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
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private unsafe void receive(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                int count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
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
        private sealed class boundaryIdentityReceiver : boundaryIdentityReceiver<socket>, IDisposable
        {
            /// <summary>
            /// 异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;
            /// <summary>
            /// HTTP数据接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public boundaryIdentityReceiver(socket socket)
                : base(socket)
            {
                Async = socketAsyncEventArgs.Get();
                Async.SocketFlags = System.Net.Sockets.SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += receive;
                Async.UserToken = this;
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= receive;
                socketAsyncEventArgs.Push(ref Async);
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
                    Async.SocketError = SocketError.Success;
                    Async.SetBuffer(Buffer, 0, Buffer.Length);
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
                    Async.SetBuffer(receiveEndIndex, bigBuffers.Size - receiveEndIndex - sizeof(int));
                    if (socket.Socket.ReceiveAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
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
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private unsafe void receive(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                int count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
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
        private sealed class dataSender : dataSender<socket>, IDisposable
        {
            /// <summary>
            /// 异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;
            /// <summary>
            /// 发送文件异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs FileAsync;
            /// <summary>
            /// 数据发送器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public dataSender(socket socket)
                : base(socket)
            {
                Async = socketAsyncEventArgs.Get();
                Async.SocketFlags = System.Net.Sockets.SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += send;
                Async.UserToken = this;

                FileAsync = socketAsyncEventArgs.Get();
                FileAsync.SocketFlags = System.Net.Sockets.SocketFlags.None;
                FileAsync.DisconnectReuseSocket = false;
                FileAsync.Completed += sendFile;
                FileAsync.UserToken = this;
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= send;
                socketAsyncEventArgs.Push(ref Async);
                FileAsync.Completed -= sendFile;
                socketAsyncEventArgs.Push(ref FileAsync);
            }
            /// <summary>
            /// 发送数据
            /// </summary>
            /// <param name="onSend">发送数据回调处理</param>
            /// <param name="buffer">发送数据缓冲区</param>
            /// <param name="pushBuffer">发送数据缓冲区回调处理</param>
            public unsafe void Send(Action<bool> onSend, subArray<byte> buffer, memoryPool memoryPool)
            {
                this.onSend = onSend;
                sendStartTime = date.NowSecond.AddTicks(date.SecondTicks);
                this.memoryPool = memoryPool;
                this.buffer = buffer.Array;
                sendIndex = buffer.StartIndex;
                sendLength = 0;
                sendEndIndex = sendIndex + buffer.Count;
                //showjim
                if (sendEndIndex > this.buffer.Length || sendEndIndex <= sendIndex)
                {
                    requestHeader requestHeader = socket.HeaderReceiver.RequestHeader;
                    fixed (byte* headerBufferFixed = requestHeader.Buffer)
                    {
                        log.Error.Add("buffer[" + this.buffer.Length.toString() + "] sendIndex[" + sendIndex.toString() + "] sendEndIndex[" + sendEndIndex.toString() + "] State[" + socket.response.State.ToString() + "] responseSize[" + socket.responseSize.toString() + "]" + fastCSharp.String.DeSerialize(headerBufferFixed + requestHeader.Uri.StartIndex, requestHeader.Uri.Count + 2), true, false);
                    }
                }

                Async.SocketError = SocketError.Success;
                Async.SetBuffer(this.buffer, 0, this.buffer.Length);
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
                    Async.SetBuffer(sendIndex, Math.Min(sendEndIndex - sendIndex, net.socket.MaxServerSendSize));
                    if (socket.Socket.SendAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
                }
                catch (Exception error)
                {
                    //showjim
                    log.Error.Add(error, "sendIndex[" + sendIndex.toString() + "] sendEndIndex[" + sendEndIndex.toString() + "]", false);
                }
                send(false);
            }
            /// <summary>
            /// 发送数据处理
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private unsafe void send(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                int count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
                    {
                        sendIndex += count;
                        sendLength += count;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (count <= 0) send(false);
                else if (sendIndex == sendEndIndex) send(true);
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
                        FileAsync.SetBuffer(buffer, 0, buffer.Length);
                        FileAsync.SocketError = SocketError.Success;
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
                    FileAsync.SetBuffer(sendIndex, sendEndIndex - sendIndex);
                    if (socket.Socket.SendAsync(FileAsync))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
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
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private unsafe void sendFile(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                int count = 0;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) > 0)
                    {
                        sendIndex += count;
                        sendLength += count;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (count <= 0) sendFile(false);
                else if (sendIndex == sendEndIndex) readFile();
                else if (date.NowSecond > sendStartTime && sendLength < minReceiveSizePerSecond4 * ((int)(date.NowSecond - sendStartTime).TotalSeconds >> 2)) sendFile(false);
                else sendFile();
            }
        }
        /// <summary>
        /// WebSocket请求接收器
        /// </summary>
        private unsafe sealed class webSocketIdentityReceiver : webSocketIdentityReceiver<socket>, IDisposable
        {
            /// <summary>
            /// 异步套接字操作
            /// </summary>
            public SocketAsyncEventArgs Async;
            /// <summary>
            /// WebSocket请求接收器
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            public webSocketIdentityReceiver(socket socket)
                : base(socket)
            {
                Async = socketAsyncEventArgs.Get();
                Async.SocketFlags = System.Net.Sockets.SocketFlags.None;
                Async.DisconnectReuseSocket = false;
                Async.Completed += receive;
                Async.UserToken = this;
                Async.SetBuffer(buffer, 0, fastCSharp.config.http.Default.HeaderBufferLength);
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                Async.Completed -= receive;
                socketAsyncEventArgs.Push(ref Async);
            }
            /// <summary>
            /// 开始接收请求数据
            /// </summary>
            public void Receive()
            {
                receiveEndIndex = 0;
                timeout = date.NowSecond.AddTicks(WebSocketReceiveTimeoutQueue.CallbackTimeoutTicks);
                Async.SocketError = SocketError.Success;
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
                    Async.SetBuffer(receiveEndIndex, fastCSharp.config.http.Default.HeaderBufferLength - receiveEndIndex);
                    if (socket.Socket.ReceiveAsync(Async))
                    {
                        socket.setTimeout(timeoutIdentity);
                        return;
                    }
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
            /// <param name="sender"></param>
            /// <param name="async">异步回调参数</param>
            private void receive(object sender, SocketAsyncEventArgs async)
            {
                ++socket.timeoutIdentity;
                int count = int.MinValue;
                try
                {
                    if (async.SocketError == SocketError.Success && (count = async.BytesTransferred) >= 0)
                    {
                        receiveEndIndex += count;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (count < 0) socket.webSocketEnd();
                else if (date.NowSecond >= timeout) close();
                else if (count == 0) fastCSharp.threading.timerTask.Default.Add(receiveHandle, date.NowSecond.AddSeconds(1), null);
                else if (receiveEndIndex >= 6) tryParse();
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
        /// 是否已经释放资源
        /// </summary>
        private int isDisposed;
        /// <summary>
        /// HTTP套接字
        /// </summary>
        private socket()
        {
            HeaderReceiver = new headerReceiver(this);
            sender = new dataSender(this);
        }
        /// <summary>
        /// 开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        private void start(servers servers, Socket socket)
        {
            this.servers = servers;
            Socket = socket;
            isLoadForm = isNextRequest = 0;
            DomainServer = null;
            form.Clear();
            response.Push(ref response);
            HeaderReceiver.RequestHeader.IsKeepAlive = false;
            HeaderReceiver.Receive();
        }
        /// <summary>
        /// 开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">TCP调用套接字</param>
        private void start(servers servers, commandSocket socket)
        {
            this.servers = servers;
            TcpCommandSocket = socket;
            Socket = socket.Socket;
            isLoadForm = isNextRequest = 0;
            DomainServer = null;
            form.Clear();
            response.Push(ref response);
            HeaderReceiver.RequestHeader.IsKeepAlive = false;
            HeaderReceiver.Receive(socket.receiveData);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1) Interlocked.Decrement(ref newCount);
            form.Clear();
            response.Push(ref response);
            base.Dispose();
        }
        /// <summary>
        /// HTTP头部接收错误
        /// </summary>
        protected override void headerError()
        {
            if (TcpCommandSocket == null)
            {
                if (ipv6.IsNull) server.SocketEnd(ipv4);
                else server.SocketEnd(ipv6);
                close(Socket);
                Socket = null;
            }
            else
            {
                TcpCommandSocket.PushPool();
                TcpCommandSocket = null;
            }
            form.Clear();
            typePool<socket>.Push(this);
        }
        /// <summary>
        /// HTTP代理结束
        /// </summary>
        internal void ProxyEnd()
        {
            Socket = null;
            if (ipv6.IsNull) server.SocketEnd(ipv4);
            else server.SocketEnd(ipv6);
            typePool<socket>.Push(this);
        }
        /// <summary>
        /// WebSocket结束
        /// </summary>
        protected override void webSocketEnd()
        {
            webSocketReceiver.Clear();
            if (ipv6.IsNull) server.SocketEnd(ipv4);
            else server.SocketEnd(ipv6);
            close(Socket);
            Socket = null;
            typePool<socket>.Push(this);
        }
        /// <summary>
        /// 未能识别的HTTP头部
        /// </summary>
        protected override void headerUnknown()
        {
            if (isNextRequest == 0)
            {
                try
                {
                    client client = servers.GetForwardClient();
                    if (client != null)
                    {
                        new forwardProxy(this, client).Start();
                        return;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
            }
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
        /// 开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="ip">客户端IP</param>
        internal static void Start(servers servers, Socket socket, ipv6Hash ip)
        {
            try
            {
                socket value = getSocket();
                value.ipv6 = ip;
                value.ipv4 = 0;
                value.start(servers, socket);
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
                server.SocketEnd(ip);
                close(socket);
            }
        }
        /// <summary>
        /// 开始处理新的请求
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="ip">客户端IP</param>
        internal static void Start(servers servers, Socket socket, int ip)
        {
            try
            {
                socket value = getSocket();
                value.ipv4 = ip;
                value.ipv6 = default(ipv6Hash);
                value.start(servers, socket);
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
                server.SocketEnd(ip);
                close(socket);
            }
        }
        /// <summary>
        /// 开始处理新的请求(用于TCP调用)
        /// </summary>
        /// <param name="servers">HTTP服务器</param>
        /// <param name="socket">TCP调用套接字</param>
        internal static void Start(servers servers, commandSocket socket)
        {
            getSocket().start(servers, socket);
        }
        /// <summary>
        /// 获取套接字
        /// </summary>
        /// <returns></returns>
        private static socket getSocket()
        {
            socket socket = typePool<socket>.Pop();
            if (socket != null) return socket;
            Interlocked.Increment(ref newCount);
            return new socket();
        }
    }
}
