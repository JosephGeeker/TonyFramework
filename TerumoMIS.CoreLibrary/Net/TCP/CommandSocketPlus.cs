//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CommandSocketPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP
//	File Name:  CommandSocketPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:21:24
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Net.TCP
{
    /// <summary>
    /// TCP调用套接字
    /// </summary>
    public abstract class CommandSocketPlus:SocketPlus
    {
        /// <summary>
        /// 大数据缓存
        /// </summary>
        internal static readonly memoryPool BigBuffers = memoryPool.GetPool(fastCSharp.config.tcpCommand.Default.BigBufferSize);
        /// <summary>
        /// 异步(包括流式)缓冲区
        /// </summary>
        protected internal static readonly memoryPool asyncBuffers = memoryPool.GetPool(fastCSharp.config.tcpCommand.Default.AsyncBufferSize);
        /// <summary>
        /// 发送命令缓冲区
        /// </summary>
        protected byte[] sendData;
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        protected internal byte[] receiveData;
        /// <summary>
        /// 当前处理会话标识
        /// </summary>
        protected internal commandServer.streamIdentity identity;
        /// <summary>
        /// 当前处理会话标识
        /// </summary>
        public commandServer.streamIdentity Identity
        {
            get { return identity; }
        }
        /// <summary>
        /// 客户端标识
        /// </summary>
        public tcpBase.client ClientUserInfo { get; protected internal set; }
        /// <summary>
        /// 是否通过验证方法
        /// </summary>
        public bool IsVerifyMethod;
        /// <summary>
        /// 默认HTTP内容编码
        /// </summary>
        internal virtual Encoding HttpEncoding { get { return null; } }
        /// <summary>
        /// TCP客户端套接字
        /// </summary>
        /// <param name="socket">TCP套接字</param>
        /// <param name="sendData">发送数据缓冲区</param>
        /// <param name="receiveData">接收数据缓冲区</param>
        /// <param name="isErrorDispose">操作错误是否自动调用析构函数</param>
        protected commandSocket(Socket socket, byte[] sendData, byte[] receiveData, bool isErrorDispose)
            : base(socket, isErrorDispose)
        {
            this.sendData = sendData;
            currentReceiveData = this.receiveData = receiveData;
        }
        ///// <summary>
        ///// TCP客户端套接字
        ///// </summary>
        ///// <param name="sendData">接收数据缓冲区</param>
        ///// <param name="receiveData">发送数据缓冲区</param>
        //protected commandSocket(byte[] sendData, byte[] receiveData)
        //    : base(false)
        //{
        //    this.sendData = sendData;
        //    currentReceiveData = this.receiveData = receiveData;
        //}
        /// <summary>
        /// 关闭套接字连接
        /// </summary>
        protected override void dispose()
        {
            fastCSharp.memoryPool.StreamBuffers.Push(ref sendData);
            fastCSharp.memoryPool.StreamBuffers.Push(ref receiveData);
        }
        /// <summary>
        /// TCP套接字添加到池
        /// </summary>
        internal abstract void PushPool();
        ///// <summary>
        ///// 获取数据
        ///// </summary>
        ///// <param name="dataFixed">数据起始位置</param>
        ///// <param name="startIndex">接收数据起始位置</param>
        ///// <param name="endIndex">接收数据结束位置</param>
        ///// <param name="length">数据长度</param>
        ///// <param name="timeout">超时时间</param>
        ///// <returns>数据,失败返回null</returns>
        //protected unsafe internal memoryPool.pushSubArray getData(byte* dataFixed, int startIndex, int endIndex, int length, DateTime timeout)
        //{
        //    int dataLength = length > 0 ? length : -length, receiveLength = endIndex - startIndex;
        //    if (dataLength <= receiveData.Length)
        //    {
        //        if (dataLength >= receiveLength)
        //        {
        //            if (dataLength == receiveLength)
        //            {
        //                if (length > 0) return new memoryPool.pushSubArray { SubArray = subArray<byte>.Unsafe(receiveData, startIndex, dataLength) };
        //                return stream.Deflate.GetDeCompressUnsafe(receiveData, startIndex, dataLength);
        //            }
        //            else
        //            {
        //                unsafer.memory.Copy(dataFixed + startIndex, dataFixed, receiveLength);
        //                if (tryReceive(receiveLength, dataLength, timeout) == dataLength)
        //                {
        //                    if (length > 0) return new memoryPool.pushSubArray { SubArray = subArray<byte>.Unsafe(receiveData, 0, dataLength) };
        //                    return stream.Deflate.GetDeCompressUnsafe(receiveData, 0, dataLength);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        byte[] data = BigBuffers.Get(dataLength);
        //        if (receive(data, receiveLength, dataLength, timeout))
        //        {
        //            unsafer.memory.Copy(dataFixed + startIndex, data, receiveLength);
        //            if (length > 0) return new memoryPool.pushSubArray { SubArray = subArray<byte>.Unsafe(data, 0, dataLength), PushPool = BigBuffers.PushHandle };
        //            memoryPool.pushSubArray newData = stream.Deflate.GetDeCompressUnsafe(data, 0, dataLength);
        //            BigBuffers.Push(ref data);
        //            return newData;
        //        }
        //        else BigBuffers.Push(ref data);
        //    }
        //    return default(memoryPool.pushSubArray);
        //}
        ///// <summary>
        ///// 数据读取器
        ///// </summary>
        //private sealed class receiver
        //{
        //    /// <summary>
        //    /// 回调委托
        //    /// </summary>
        //    public action<memoryPool.pushSubArray> Callback;
        //    /// <summary>
        //    /// TCP客户端套接字
        //    /// </summary>
        //    public commandSocket Socket;
        //    /// <summary>
        //    /// 读取数据回调操作
        //    /// </summary>
        //    private action<bool> onReadCompressDataHandle;
        //    /// <summary>
        //    /// 读取数据回调操作
        //    /// </summary>
        //    private action<bool> onReadDataHandle;
        //    /// <summary>
        //    ///读取数据是否大缓存
        //    /// </summary>
        //    private bool isBigBuffer;
        //    /// <summary>
        //    /// 数据读取器
        //    /// </summary>
        //    public receiver()
        //    {
        //        onReadCompressDataHandle = onReadCompressData;
        //        onReadDataHandle = onReadData;
        //    }
        //    /// <summary>
        //    /// 读取数据
        //    /// </summary>
        //    /// <param name="dataFixed">接收数据起始位置</param>
        //    /// <param name="startIndex">接收数据起始位置</param>
        //    /// <param name="endIndex">接收数据结束位置</param>
        //    /// <param name="length">数据长度</param>
        //    /// <param name="timeout">接收超时</param>
        //    public unsafe void Receive(byte* dataFixed, int startIndex, int endIndex, int length, DateTime timeout)
        //    {
        //        isBigBuffer = false;
        //        int dataLength = length >= 0 ? length : -length, receiveLength = endIndex - startIndex;
        //        if (dataLength <= Socket.receiveData.Length)
        //        {
        //            if (dataLength >= receiveLength)
        //            {
        //                if (receiveLength == dataLength)
        //                {
        //                    if (length >= 0) push(new memoryPool.pushSubArray { SubArray = subArray<byte>.Unsafe(Socket.receiveData, startIndex, dataLength) });
        //                    else onReadCompressData(subArray<byte>.Unsafe(Socket.receiveData, startIndex, dataLength));
        //                }
        //                else
        //                {
        //                    unsafer.memory.Copy(dataFixed + startIndex, dataFixed, receiveLength);
        //                    if (length >= 0) Socket.receive(onReadDataHandle, receiveLength, dataLength - receiveLength, timeout);
        //                    else Socket.receive(onReadCompressDataHandle, receiveLength, dataLength - receiveLength, timeout);
        //                }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    push(default(memoryPool.pushSubArray));
        //                }
        //                finally { Socket.error(); }
        //            }
        //        }
        //        else
        //        {
        //            byte[] data = BigBuffers.Get(dataLength);
        //            isBigBuffer = true;
        //            unsafer.memory.Copy(dataFixed + startIndex, data, receiveLength);
        //            if (length >= 0) Socket.receive(onReadDataHandle, data, receiveLength, dataLength - receiveLength, timeout);
        //            else Socket.receive(onReadCompressDataHandle, data, receiveLength, dataLength - receiveLength, timeout);
        //        }
        //    }
        //    /// <summary>
        //    /// 读取数据回调操作
        //    /// </summary>
        //    /// <param name="isSocket">是否操作成功</param>
        //    private void onReadData(bool isSocket)
        //    {
        //        byte[] data = Socket.currentReceiveData;
        //        Socket.currentReceiveData = Socket.receiveData;
        //        if (isSocket)
        //        {
        //            push(new memoryPool.pushSubArray { SubArray = subArray<byte>.Unsafe(data, 0, Socket.currentReceiveEndIndex), PushPool = isBigBuffer ? BigBuffers.PushHandle : null });
        //        }
        //        else
        //        {
        //            try
        //            {
        //                if (isBigBuffer) BigBuffers.Push(ref data);
        //                push(default(memoryPool.pushSubArray));
        //            }
        //            finally { Socket.error(); }
        //        }
        //    }
        //    /// <summary>
        //    /// 读取数据回调操作
        //    /// </summary>
        //    /// <param name="isSocket">是否操作成功</param>
        //    private void onReadCompressData(bool isSocket)
        //    {
        //        byte[] data = Socket.currentReceiveData;
        //        Socket.currentReceiveData = Socket.receiveData;
        //        if (isSocket)
        //        {
        //            onReadCompressData(subArray<byte>.Unsafe(data, 0, Socket.currentReceiveEndIndex));
        //        }
        //        else
        //        {
        //            try
        //            {
        //                if (isBigBuffer) BigBuffers.Push(ref data);
        //                push(default(memoryPool.pushSubArray));
        //            }
        //            finally { Socket.error(); }
        //        }
        //    }
        //    /// <summary>
        //    /// 读取数据回调操作
        //    /// </summary>
        //    private void onReadCompressData(subArray<byte> data)
        //    {
        //        try
        //        {
        //            push(stream.Deflate.GetDeCompressUnsafe(data.Array, data.StartIndex, data.Length));
        //            return;
        //        }
        //        catch (Exception error)
        //        {
        //            log.Default.Add(error, null, false);
        //        }
        //        finally
        //        {
        //            if (isBigBuffer) BigBuffers.Push(ref data.array);
        //        }
        //        try
        //        {
        //            push(default(memoryPool.pushSubArray));
        //        }
        //        finally { Socket.error(); }
        //    }
        //    /// <summary>
        //    /// 添加回调对象
        //    /// </summary>
        //    /// <param name="data">输出数据</param>
        //    private void push(memoryPool.pushSubArray data)
        //    {
        //        action<memoryPool.pushSubArray> callback = Callback;
        //        Socket = null;
        //        Callback = null;
        //        try
        //        {
        //            typePool<receiver>.Push(this);
        //        }
        //        finally
        //        {
        //            if (callback != null)
        //            {
        //                try
        //                {
        //                    callback(data);
        //                }
        //                catch (Exception error)
        //                {
        //                    fastCSharp.log.Default.Add(error, null, false);
        //                }
        //            }
        //        }
        //    }
        //}
        ///// <summary>
        ///// 读取数据
        ///// </summary>
        ///// <param name="onReceive">接收数据处理委托</param>
        ///// <param name="dataFixed">接收数据起始位置</param>
        ///// <param name="startIndex">接收数据起始位置</param>
        ///// <param name="endIndex">接收数据结束位置</param>
        ///// <param name="length">数据长度</param>
        ///// <param name="timeout">接收超时</param>
        //protected unsafe void receive(action<memoryPool.pushSubArray> onReceive, byte* dataFixed, int startIndex, int endIndex, int length, DateTime timeout)
        //{
        //    receiver receiver = typePool<receiver>.Pop();
        //    if (receiver == null)
        //    {
        //        try
        //        {
        //            receiver = new receiver();
        //        }
        //        catch (Exception error)
        //        {
        //            log.Default.Add(error, null, false);
        //        }
        //        if (receiver == null)
        //        {
        //            onReceive(default(memoryPool.pushSubArray));
        //            return;
        //        }
        //    }
        //    receiver.Callback = onReceive;
        //    receiver.Socket = this;
        //    receiver.Receive(dataFixed, startIndex, endIndex, length, timeout);
        //}
    }
    /// <summary>
    /// TCP调用套接字
    /// </summary>
    /// <typeparam name="socketType">TCP调用类型</typeparam>
    public abstract class commandSocket<commandSocketType> : commandSocket
        where commandSocketType : class, IDisposable
    {
        /// <summary>
        /// TCP调用代理
        /// </summary>
        protected internal commandSocketType commandSocketProxy;
        /// <summary>
        /// TCP客户端套接字
        /// </summary>
        /// <param name="socket">TCP套接字</param>
        /// <param name="sendData">发送数据缓冲区</param>
        /// <param name="receiveData">接收数据缓冲区</param>
        /// <param name="commandSocketProxy">TCP调用类型</param>
        /// <param name="isErrorDispose">操作错误是否自动调用析构函数</param>
        protected commandSocket(Socket socket, byte[] sendData, byte[] receiveData, commandSocketType commandSocketProxy, bool isErrorDispose)
            : base(socket, sendData, receiveData, isErrorDispose)
        {
            this.commandSocketProxy = commandSocketProxy;
        }
        ///// <summary>
        ///// TCP客户端套接字
        ///// </summary>
        ///// <param name="receiveData">接收数据缓冲区</param>
        ///// <param name="commandSocketProxy">TCP调用类型</param>
        //protected commandSocket(byte[] receiveData, commandSocketType commandSocketProxy)
        //    : base(receiveData)
        //{
        //    this.commandSocketProxy = commandSocketProxy;
        //}
    }
}
