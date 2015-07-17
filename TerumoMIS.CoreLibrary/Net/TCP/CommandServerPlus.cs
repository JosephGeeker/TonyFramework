//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CommandServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP
//	File Name:  CommandServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:20:30
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

namespace TerumoMIS.CoreLibrary.Net.TCP
{
    /// <summary>
    /// TCP调用服务端
    /// </summary>
    public abstract class CommandServerPlus : ServerPlus
    {
        /// <summary>
        /// 数据命令验证会话标识
        /// </summary>
        internal const int VerifyIdentity = 0x060C5113;
        /// <summary>
        /// 序号命令验证会话标识
        /// </summary>
        internal const int IdentityVerifyIdentity = 0x10035113;
        /// <summary>
        /// 空验证命令序号
        /// </summary>
        private const int nullVerifyCommandIdentity = -VerifyIdentity;
        /// <summary>
        /// 用户命令起始位置
        /// </summary>
        public const int CommandStartIndex = 128;
        /// <summary>
        /// 用户命令数据起始位置
        /// </summary>
        public const int CommandDataIndex = 0x20202000;
        /// <summary>
        /// 关闭命令
        /// </summary>
        public const int CloseIdentityCommand = CommandStartIndex - 1;
        /// <summary>
        /// 连接检测命令
        /// </summary>
        public const int CheckIdentityCommand = CloseIdentityCommand - 1;
        /// <summary>
        /// 负载均衡连接检测命令
        /// </summary>
        public const int LoadBalancingCheckIdentityCommand = CheckIdentityCommand - 1;
        /// <summary>
        /// 流合并命令
        /// </summary>
        public const int StreamMergeIdentityCommand = LoadBalancingCheckIdentityCommand - 1;
        /// <summary>
        /// TCP流回应命令
        /// </summary>
        public const int TcpStreamCommand = StreamMergeIdentityCommand - 1;
        /// <summary>
        /// 忽略分组命令
        /// </summary>
        public const int IgnoreGroupCommand = TcpStreamCommand - 1;
        /// <summary>
        /// 流式套接字错误返回长度值
        /// </summary>
        internal const int ErrorStreamReturnLength = int.MinValue;
        /// <summary>
        /// 关闭链接命令
        /// </summary>
        private static readonly byte[] closeCommandData = BitConverter.GetBytes(CloseIdentityCommand + CommandDataIndex);
        /// <summary>
        /// 流合并命令
        /// </summary>
        private static readonly byte[] streamMergeCommandData = BitConverter.GetBytes(StreamMergeIdentityCommand + CommandDataIndex);
        /// <summary>
        /// 连接检测命令
        /// </summary>
        private static readonly byte[] checkCommandData = BitConverter.GetBytes(CheckIdentityCommand + CommandDataIndex);
        /// <summary>
        /// 负载均衡连接检测命令
        /// </summary>
        private static readonly byte[] loadBalancingCheckCommandData = BitConverter.GetBytes(LoadBalancingCheckIdentityCommand + CommandDataIndex);
        /// <summary>
        /// TCP流回馈命令
        /// </summary>
        private static readonly byte[] tcpStreamCommandData = BitConverter.GetBytes(TcpStreamCommand + CommandDataIndex);
        /// <summary>
        /// 忽略分组命令
        /// </summary>
        private static readonly byte[] ignoreGroupCommandData = BitConverter.GetBytes(IgnoreGroupCommand + CommandDataIndex);
        /// <summary>
        /// 连接检测套接字
        /// </summary>
        private static readonly command mergeCheckCommand = new command(check, 0);
        /// <summary>
        /// 命令处理委托
        /// </summary>
        public struct command
        {
            /// <summary>
            /// 命令处理委托
            /// </summary>
            public Action<commandServer.socket, subArray<byte>> OnCommand;
            /// <summary>
            /// 最大参数数据长度,0表示不接受参数数据
            /// </summary>
            public int MaxDataLength;
            /// <summary>
            /// 命令处理委托
            /// </summary>
            /// <param name="onCommand">命令处理委托</param>
            /// <param name="maxDataLength">最大参数数据长度,0表示不接受参数数据</param>
            public command(Action<commandServer.socket, subArray<byte>> onCommand, int maxDataLength = int.MaxValue)
            {
                OnCommand = onCommand;
                MaxDataLength = maxDataLength;
            }
            /// <summary>
            /// 设置命令处理委托
            /// </summary>
            /// <param name="onCommand">命令处理委托</param>
            /// <param name="maxDataLength">最大参数数据长度,0表示不接受参数数据</param>
            public void Set(Action<commandServer.socket, subArray<byte>> onCommand, int maxDataLength = int.MaxValue)
            {
                OnCommand = onCommand;
                MaxDataLength = maxDataLength;
            }
        }
        /// <summary>
        /// 会话标识
        /// </summary>
        public struct streamIdentity
        {
            /// <summary>
            /// 请求标识
            /// </summary>
            public int Identity;
            /// <summary>
            /// 请求索引
            /// </summary>
            public int Index;
        }
        /// <summary>
        /// 服务器端调用
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        public abstract class socketCall
        {
            /// <summary>
            /// 套接字
            /// </summary>
            protected socket socket;
            /// <summary>
            /// 回话标识
            /// </summary>
            protected streamIdentity identity;
            /// <summary>
            /// 调用委托
            /// </summary>
            protected Action callHandle;
            /// <summary>
            /// 套接字重用标识
            /// </summary>
            protected int pushIdentity;
            /// <summary>
            /// 判断套接字是否有效
            /// </summary>
            protected int isVerify
            {
                get { return pushIdentity ^ socket.PushIdentity; }
            }
            /// <summary>
            /// 服务器端调用
            /// </summary>
            protected socketCall()
            {
                callHandle = call;
            }
            /// <summary>
            /// 调用处理
            /// </summary>
            protected abstract void call();
        }
        /// <summary>
        /// 服务器端调用
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        /// <typeparam name="inputParameterType">输入参数类型</typeparam>
        public abstract class socketCall<callType> : socketCall
            where callType : socketCall<callType>
        {
            /// <summary>
            /// 设置参数
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="identity">回话标识</param>
            /// <param name="inputParameter">输入参数</param>
            private void set(socket socket, streamIdentity identity)
            {
                this.socket = socket;
                this.identity = identity;
                pushIdentity = socket.PushIdentity;
            }
            /// <summary>
            /// 获取服务器端调用
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="identity"></param>
            /// <param name="inputParameter"></param>
            /// <returns></returns>
            public static Action Call(socket socket, fastCSharp.net.tcp.commandServer.streamIdentity identity)
            {
                callType value = fastCSharp.typePool<callType>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = fastCSharp.emit.constructor<callType>.New();
                    }
                    catch (Exception error)
                    {
                        fastCSharp.log.Error.Add(error, null, false);
                        return null;
                    }
                }
                value.set(socket, identity);
                return value.callHandle;
            }
        }
        /// <summary>
        /// 服务器端调用
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        /// <typeparam name="inputParameterType">输入参数类型</typeparam>
        public abstract class socketCall<callType ,inputParameterType> : socketCall
            where callType : socketCall<callType, inputParameterType>
        {
            /// <summary>
            /// 输入参数
            /// </summary>
            protected inputParameterType inputParameter;
            /// <summary>
            /// 设置参数
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="identity">回话标识</param>
            /// <param name="inputParameter">输入参数</param>
            private void set(socket socket, streamIdentity identity, inputParameterType inputParameter)
            {
                this.socket = socket;
                this.identity = identity;
                this.inputParameter = inputParameter;
                pushIdentity = socket.PushIdentity;
            }
            /// <summary>
            /// 获取服务器端调用
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="identity"></param>
            /// <param name="inputParameter"></param>
            /// <returns></returns>
            public static Action Call(socket socket, fastCSharp.net.tcp.commandServer.streamIdentity identity, inputParameterType inputParameter)
            {
                callType value = fastCSharp.typePool<callType>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = fastCSharp.emit.constructor<callType>.New();
                    }
                    catch (Exception error)
                    {
                        fastCSharp.log.Error.Add(error, null, false);
                        return null;
                    }
                }
                value.set(socket, identity, inputParameter);
                return value.callHandle;
            }
        }
        /// <summary>
        /// 服务器端调用
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        /// <typeparam name="serverType">服务器目标对象类型</typeparam>
        public abstract class serverCall<callType, serverType> : socketCall
            where callType : serverCall<callType, serverType>
        {
            /// <summary>
            /// 服务器目标对象
            /// </summary>
            protected serverType serverValue;
            /// <summary>
            /// 设置参数
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="serverValue">服务器目标对象</param>
            /// <param name="identity">回话标识</param>
            private void set(socket socket, serverType serverValue, streamIdentity identity)
            {
                this.socket = socket;
                this.serverValue = serverValue;
                this.identity = identity;
                pushIdentity = socket.PushIdentity;
            }
            /// <summary>
            /// 获取服务器端调用
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="serverValue"></param>
            /// <param name="identity"></param>
            /// <returns></returns>
            public static Action Call(socket socket, serverType serverValue, fastCSharp.net.tcp.commandServer.streamIdentity identity)
            {
                callType value = fastCSharp.typePool<callType>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = fastCSharp.emit.constructor<callType>.New();
                    }
                    catch (Exception error)
                    {
                        fastCSharp.log.Error.Add(error, null, false);
                        return null;
                    }
                }
                value.set(socket, serverValue, identity);
                return value.callHandle;
            }
        }
        /// <summary>
        /// 服务器端调用
        /// </summary>
        /// <typeparam name="socketType">套接字类型</typeparam>
        /// <typeparam name="serverType">服务器目标对象类型</typeparam>
        /// <typeparam name="inputParameterType">输入参数类型</typeparam>
        public abstract class serverCall<callType, serverType, inputParameterType> : serverCall<callType, serverType>
            where callType : serverCall<callType, serverType, inputParameterType>
        {
            /// <summary>
            /// 输入参数
            /// </summary>
            protected inputParameterType inputParameter;
            /// <summary>
            /// 设置参数
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="serverValue">服务器目标对象</param>
            /// <param name="identity">回话标识</param>
            /// <param name="inputParameter">输入参数</param>
            private void set(socket socket, serverType serverValue, streamIdentity identity, inputParameterType inputParameter)
            {
                this.socket = socket;
                this.serverValue = serverValue;
                this.identity = identity;
                this.inputParameter = inputParameter;
                pushIdentity = socket.PushIdentity;
            }
            /// <summary>
            /// 获取服务器端调用
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="serverValue"></param>
            /// <param name="identity"></param>
            /// <param name="inputParameter"></param>
            /// <returns></returns>
            public static Action Call(socket socket, serverType serverValue, fastCSharp.net.tcp.commandServer.streamIdentity identity, inputParameterType inputParameter)
            {
                callType value = fastCSharp.typePool<callType>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = fastCSharp.emit.constructor<callType>.New();
                    }
                    catch (Exception error)
                    {
                        fastCSharp.log.Error.Add(error, null, false);
                        return null;
                    }
                }
                value.set(socket, serverValue, identity, inputParameter);
                return value.callHandle;
            }
        }
        /// <summary>
        /// TCP流命令类型
        /// </summary>
        internal enum tcpStreamCommand : byte
        {
            /// <summary>
            /// 获取流字节长度
            /// </summary>
            GetLength,
            /// <summary>
            /// 设置流字节长度
            /// </summary>
            SetLength,
            /// <summary>
            /// 获取当前位置
            /// </summary>
            GetPosition,
            /// <summary>
            /// 设置当前位置
            /// </summary>
            SetPosition,
            /// <summary>
            /// 获取读取超时
            /// </summary>
            GetReadTimeout,
            /// <summary>
            /// 设置读取超时
            /// </summary>
            SetReadTimeout,
            /// <summary>
            /// 获取写入超时
            /// </summary>
            GetWriteTimeout,
            /// <summary>
            /// 设置写入超时
            /// </summary>
            SetWriteTimeout,
            /// <summary>
            /// 异步读取
            /// </summary>
            BeginRead,
            /// <summary>
            /// 读取字节序列
            /// </summary>
            Read,
            /// <summary>
            /// 读取字节
            /// </summary>
            ReadByte,
            /// <summary>
            /// 异步写入
            /// </summary>
            BeginWrite,
            /// <summary>
            /// 写入字节序列
            /// </summary>
            Write,
            /// <summary>
            /// 写入字节
            /// </summary>
            WriteByte,
            /// <summary>
            /// 设置流位置
            /// </summary>
            Seek,
            /// <summary>
            /// 清除缓冲区
            /// </summary>
            Flush,
            /// <summary>
            /// 关闭流
            /// </summary>
            Close
        }
        /// <summary>
        /// TCP流异步接口
        /// </summary>
        internal interface ITcpStreamCallback
        {
            /// <summary>
            /// TCP流异步回调
            /// </summary>
            /// <param name="tcpStreamAsyncResult">TCP流异步操作状态</param>
            /// <param name="parameter">TCP流参数</param>
            void Callback(tcpStreamAsyncResult tcpStreamAsyncResult, tcpStreamParameter parameter);
        }
        /// <summary>
        /// TCP流参数
        /// </summary>
        internal sealed class tcpStreamParameter
        {
            /// <summary>
            /// 命令集合索引
            /// </summary>
            public int Index;
            /// <summary>
            /// 命令序号
            /// </summary>
            public int Identity;
            /// <summary>
            /// 客户端索引
            /// </summary>
            public int ClientIndex;
            /// <summary>
            /// 客户端序号
            /// </summary>
            public int ClientIdentity;
            /// <summary>
            /// 位置参数
            /// </summary>
            public long Offset;
            /// <summary>
            /// 数据参数
            /// </summary>
            public subArray<byte> Data;
            /// <summary>
            /// 查找类型参数
            /// </summary>
            public SeekOrigin SeekOrigin;
            /// <summary>
            /// 命令类型
            /// </summary>
            public tcpStreamCommand Command;
            /// <summary>
            /// 客户端流是否存在
            /// </summary>
            public bool IsClientStream;
            /// <summary>
            /// 客户端命令是否成功
            /// </summary>
            public bool IsCommand;
            /// <summary>
            /// 缓冲区处理
            /// </summary>
            public Action<bool> PushClientBuffer
            {
                get
                {
                    if (Data.array != null && Data.array.Length == commandSocket.asyncBuffers.Size) return pushClientBuffer;
                    return null;
                }
            }
            /// <summary>
            /// 缓冲区处理
            /// </summary>
            private void pushClientBuffer(bool _)
            {
                commandSocket.asyncBuffers.Push(ref Data.array);
            }
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            internal unsafe void DeSerialize(fastCSharp.emit.dataDeSerializer deSerializer)
            {
                byte* start = deSerializer.Read;
                int bufferSize = *(int*)(start + (sizeof(int) * 5 + sizeof(long))), dataSize = sizeof(int) * 7 + sizeof(long) + ((bufferSize + 3) & (int.MaxValue - 3));
                if (deSerializer.VerifyRead(dataSize) && *(int*)(start + dataSize - sizeof(int)) == dataSize)
                {
                    Index = *(int*)start;
                    Identity = *(int*)(start + sizeof(int));
                    ClientIndex = *(int*)(start + sizeof(int) * 2);
                    ClientIdentity = *(int*)(start + sizeof(int) * 3);
                    Offset = *(long*)(start + sizeof(int) * 4);
                    SeekOrigin = (SeekOrigin)(*(start + (sizeof(int) * 4 + sizeof(long))));
                    Command = (tcpStreamCommand)(*(start + (sizeof(int) * 4 + sizeof(long) + 1)));
                    IsClientStream = *(start + (sizeof(int) * 4 + sizeof(long) + 2)) != 0;
                    IsCommand = *(start + (sizeof(int) * 4 + sizeof(long) + 3)) != 0;
                    if (bufferSize == 0) Data.UnsafeSet(nullValue<byte>.Array, 0, 0);
                    else
                    {
                        Data.UnsafeSet(new byte[bufferSize], 0, bufferSize);
                        fastCSharp.unsafer.memory.Copy(start + (sizeof(int) * 6 + sizeof(long)), Data.Array, bufferSize);
                    }
                }
            }
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static unsafe void deSerialize(fastCSharp.emit.dataDeSerializer deSerializer, ref tcpStreamParameter value)
            {
                //if (deSerializer.CheckNull() == 0) value = null;
                //else
                //{
                //    if (value == null) value = new tcpStreamParameter();
                //    value.DeSerialize(deSerializer);
                //}
                (value = new tcpStreamParameter()).DeSerialize(deSerializer);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            private unsafe void serialize(fastCSharp.emit.dataSerializer serializer)
            {
                unmanagedStream stream = serializer.Stream;
                int length = sizeof(int) * 7 + sizeof(long) + ((Data.Count + 3) & (int.MaxValue - 3));
                stream.PrepLength(length);
                unmanagedStream.unsafer unsafeStream = stream.Unsafer;
                unsafeStream.Write(Index);
                unsafeStream.Write(Identity);
                unsafeStream.Write(ClientIndex);
                unsafeStream.Write(ClientIdentity);
                unsafeStream.Write(Offset);
                unsafeStream.Write((byte)SeekOrigin);
                unsafeStream.Write((byte)Command);
                unsafeStream.Write(IsClientStream ? (byte)1 : (byte)0);
                unsafeStream.Write(IsCommand ? (byte)1 : (byte)0);
                unsafeStream.Write(Data.Count);
                if (Data.Count != 0)
                {
                    fixed (byte* dataFixed = Data.Array)
                    {
                        fastCSharp.unsafer.memory.Copy(dataFixed + Data.StartIndex, stream.CurrentData, Data.Count);
                    }
                    unsafeStream.AddLength((Data.Count + 3) & (int.MaxValue - 3));
                }
                unsafeStream.Write(length);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            [fastCSharp.emit.dataSerialize.custom]
            private static void serialize(fastCSharp.emit.dataSerializer serializer, tcpStreamParameter value)
            {
                value.serialize(serializer);
                //if (value == null) serializer.Stream.Write(fastCSharp.emit.binarySerializer.NullValue);
                //else value.serialize(serializer);
            }
            /// <summary>
            /// 空TCP流参数
            /// </summary>
            public static readonly tcpStreamParameter Null = new tcpStreamParameter();
        }
        /// <summary>
        /// TCP流异步操作状态
        /// </summary>
        internal class tcpStreamAsyncResult : IAsyncResult
        {
            /// <summary>
            /// TCP流异步接口
            /// </summary>
            public ITcpStreamCallback TcpStreamCallback;
            /// <summary>
            /// TCP流参数
            /// </summary>
            public tcpStreamParameter Parameter;
            /// <summary>
            /// 异步回调
            /// </summary>
            public AsyncCallback Callback;
            /// <summary>
            /// 用户定义的对象
            /// </summary>
            public object AsyncState { get; set; }
            /// <summary>
            /// 等待异步操作完成
            /// </summary>
            private EventWaitHandle asyncWaitHandle;
            /// <summary>
            /// 等待异步操作完成访问锁
            /// </summary>
            private int asyncWaitHandleLock;
            /// <summary>
            /// 等待异步操作是否完成
            /// </summary>
            public bool IsCallback;
            /// <summary>
            /// 等待异步操作完成
            /// </summary>
            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (asyncWaitHandle == null)
                    {
                        interlocked.NoCheckCompareSetSleep0(ref asyncWaitHandleLock);
                        try
                        {
                            if (asyncWaitHandle == null) asyncWaitHandle = new EventWaitHandle(IsCallback, EventResetMode.ManualReset);
                        }
                        finally { asyncWaitHandleLock = 0; }
                    }
                    return asyncWaitHandle;
                }
            }
            /// <summary>
            /// 是否同步完成
            /// </summary>
            public bool CompletedSynchronously
            {
                get { return false; }
            }
            /// <summary>
            /// 异步操作是否已完成
            /// </summary>
            public bool IsCompleted { get; set; }
            /// <summary>
            /// 异步回调
            /// </summary>
            /// <param name="parameter">TCP流参数</param>
            public void OnCallback(tcpStreamParameter parameter)
            {
                try
                {
                    TcpStreamCallback.Callback(this, parameter);
                }
                finally
                {
                    interlocked.NoCheckCompareSetSleep0(ref asyncWaitHandleLock);
                    IsCallback = true;
                    EventWaitHandle asyncWaitHandle = this.asyncWaitHandle;
                    asyncWaitHandleLock = 0;
                    if (asyncWaitHandle != null) asyncWaitHandle.Set();
                    if (Callback != null) Callback(this);
                }
            }
        }
        /// <summary>
        /// TCP流读取器
        /// </summary>
        private struct tcpStreamReceiver
        {
            /// <summary>
            /// 当前处理序号
            /// </summary>
            public int Identity;
            /// <summary>
            /// 异步状态
            /// </summary>
            public tcpStreamAsyncResult AsyncResult;
            /// <summary>
            /// TCP流参数
            /// </summary>
            public tcpStreamParameter Parameter;
            /// <summary>
            /// TCP流读取等待
            /// </summary>
            public EventWaitHandle ReceiveWait;
            /// <summary>
            /// 获取读取数据
            /// </summary>
            /// <param name="identity">当前处理序号</param>
            /// <param name="parameter">TCP流参数</param>
            /// <returns>是否成功</returns>
            public bool Get(int identity, ref tcpStreamParameter parameter)
            {
                if (identity == Identity)
                {
                    parameter = Parameter;
                    AsyncResult = null;
                    ++Identity;
                    Parameter = null;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 设置异步状态
            /// </summary>
            /// <param name="asyncResult">异步状态</param>
            public void SetAsyncResult(tcpStreamAsyncResult asyncResult)
            {
                if (asyncResult == null)
                {
                    if (ReceiveWait == null) ReceiveWait = new EventWaitHandle(false, EventResetMode.AutoReset);
                    else ReceiveWait.Reset();
                }
                else AsyncResult = asyncResult;
            }
            /// <summary>
            /// 取消读取
            /// </summary>
            /// <param name="isSetWait">是否设置结束状态</param>
            public void Cancel(bool isSetWait)
            {
                tcpStreamAsyncResult asyncResult = AsyncResult;
                ++Identity;
                Parameter = null;
                AsyncResult = null;
                if (asyncResult == null)
                {
                    if (isSetWait && ReceiveWait != null) ReceiveWait.Set();
                }
                else asyncResult.OnCallback(null);
            }
            /// <summary>
            /// 取消读取
            /// </summary>
            /// <param name="identity">当前处理序号</param>
            /// <param name="isSetWait">是否设置结束状态</param>
            /// <returns>是否成功</returns>
            public bool Cancel(int identity, bool isSetWait)
            {
                if (identity == Identity)
                {
                    Cancel(isSetWait);
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 设置TCP流参数
            /// </summary>
            /// <param name="parameter">TCP流参数</param>
            /// <param name="asyncResult">异步状态</param>
            /// <returns>是否成功</returns>
            public bool Set(tcpStreamParameter parameter, ref tcpStreamAsyncResult asyncResult)
            {
                if (Identity == parameter.Identity)
                {
                    asyncResult = AsyncResult;
                    if (AsyncResult == null)
                    {
                        Parameter = parameter;
                        ReceiveWait.Set();
                    }
                    else
                    {
                        ++Identity;
                        AsyncResult = null;
                    }
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// HTTP命令处理委托
        /// </summary>
        public struct httpCommand
        {
            /// <summary>
            /// HTTP命令处理委托
            /// </summary>
            public Action<http.socketBase> OnCommand;
            /// <summary>
            /// 最大参数数据长度,0表示不接受参数数据
            /// </summary>
            public int MaxDataLength;
            /// <summary>
            /// 是否仅支持POST调用
            /// </summary>
            public bool IsPostOnly;
            /// <summary>
            /// HTTP命令处理委托
            /// </summary>
            /// <param name="onCommand">命令处理委托</param>
            /// <param name="IsPostOnly">是否仅支持POST调用</param>
            /// <param name="maxDataLength">最大参数数据长度,0表示不接受参数数据</param>
            public httpCommand(Action<http.socketBase> onCommand, bool isPostOnly, int maxDataLength = int.MaxValue)
            {
                OnCommand = onCommand;
                MaxDataLength = maxDataLength;
                IsPostOnly = isPostOnly;
            }
            /// <summary>
            /// 设置命令处理委托
            /// </summary>
            /// <param name="onCommand">命令处理委托</param>
            /// <param name="IsPostOnly">是否仅支持POST调用</param>
            /// <param name="maxDataLength">最大参数数据长度,0表示不接受参数数据</param>
            public void Set(Action<http.socketBase> onCommand, bool isPostOnly, int maxDataLength = int.MaxValue)
            {
                OnCommand = onCommand;
                MaxDataLength = maxDataLength;
                IsPostOnly = isPostOnly;
            }
        }
        /// <summary>
        /// HTTP服务器
        /// </summary>
        internal sealed class httpServers : http.servers
        {
            /// <summary>
            /// HTTP服务
            /// </summary>
            public sealed class server : http.domainServer
            {
                /// <summary>
                /// HTTP服务器
                /// </summary>
                private httpServers httpServers;
                /// <summary>
                /// 客户端缓存时间(单位:秒)
                /// </summary>
                protected override int clientCacheSeconds
                {
                    get { return 0; }
                }
                /// <summary>
                /// 最大文件缓存字节数(单位KB)
                /// </summary>
                protected override int maxCacheFileSize
                {
                    get { return 0; }
                }
                /// <summary>
                /// 文件路径
                /// </summary>
                protected override int maxCacheSize
                {
                    get { return 0; }
                }
                /// <summary>
                /// HTTP服务
                /// </summary>
                /// <param name="httpServers">HTTP服务器</param>
                public server(httpServers httpServers)
                {
                    this.httpServers = httpServers;
                    Session = new http.session<object>();
                }
                /// <summary>
                /// 启动HTTP服务
                /// </summary>
                /// <param name="domains">域名信息集合</param>
                /// <param name="onStop">停止服务处理</param>
                /// <returns>是否启动成功</returns>
                public override bool Start(http.domain[] domains, Action onStop)
                {
                    return false;
                }
                /// <summary>
                /// HTTP请求处理
                /// </summary>
                /// <param name="socket">HTTP套接字</param>
                /// <param name="socketIdentity">套接字操作编号</param>
                public override void Request(http.socketBase socket, long socketIdentity)
                {
                    requestHeader request = socket.RequestHeader;
                    response response = null;
                    try
                    {
                        subArray<byte> commandName = request.Path;
                        if (commandName.Count != 0)
                        {
                            commandName.UnsafeSet(commandName.StartIndex + 1, commandName.Count - 1);
                            httpCommand command = default(httpCommand);
                            if (httpServers.commandServer.httpCommands.Get(commandName, ref command))
                            {
                                if (request.Method == web.http.methodType.GET)
                                {
                                    if (request.ContentLength == 0 && !command.IsPostOnly)
                                    {
                                        tcpBase.client clientUserInfo = new tcpBase.client();
                                        tcpBase.httpPage page = tcpBase.httpPage.Get(socket, this, socketIdentity, request, null);
                                        ((webPage.page)page).Response = (response = response.Get(true));
                                        clientUserInfo.UserInfo = page;
                                        socket.TcpCommandSocket.ClientUserInfo = clientUserInfo;
                                        command.OnCommand(socket);
                                        return;
                                    }
                                }
                                else if (request.PostType != http.requestHeader.postType.None && (uint)request.ContentLength <= command.MaxDataLength)
                                {
                                    socket.GetForm(socketIdentity, httpLoadForm.Get(socket, this, socketIdentity, request, command));
                                    return;
                                }
                                socket.ResponseError(socketIdentity, http.response.state.MethodNotAllowed405);
                                return;
                            }
                            else Console.Write("");
                        }
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    finally { response.Push(ref response); }
                    socket.ResponseError(socketIdentity, http.response.state.NotFound404);
                }
                /// <summary>
                /// 创建错误输出数据
                /// </summary>
                protected override void createErrorResponse() { }
            }
            /// <summary>
            /// TCP调用服务端
            /// </summary>
            private commandServer commandServer;
            /// <summary>
            /// 域名服务信息
            /// </summary>
            private new domainServer domainServer;
            /// <summary>
            /// HTTP服务器
            /// </summary>
            /// <param name="commandServer">TCP调用服务端</param>
            public httpServers(commandServer commandServer)
            {
                this.commandServer = commandServer;
                this.domainServer = new domainServer { Server = new server(this) };
            }
            /// <summary>
            /// 获取HTTP转发代理服务客户端
            /// </summary>
            /// <returns>HTTP转发代理服务客户端,失败返回null</returns>
            internal override client GetForwardClient() { return null; }
            /// <summary>
            /// 获取域名服务信息
            /// </summary>
            /// <param name="domain">域名</param>
            /// <returns>域名服务信息</returns>
            internal override unsafe domainServer GetServer(subArray<byte> domain) { return domainServer; }
        }
        /// <summary>
        /// HTTP表单数据加载处理
        /// </summary>
        private sealed class httpLoadForm : requestForm.ILoadForm
        {
            /// <summary>
            /// HTTP套接字
            /// </summary>
            private http.socketBase socket;
            /// <summary>
            /// HTTP服务
            /// </summary>
            private httpServers.server domainServer;
            /// <summary>
            /// 请求头部信息
            /// </summary>
            private http.requestHeader request;
            /// <summary>
            /// 套接字操作编号
            /// </summary>
            private long socketIdentity;
            /// <summary>
            /// HTTP命令处理委托
            /// </summary>
            private httpCommand command;
            /// <summary>
            /// 表单回调处理
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            public void OnGetForm(requestForm form)
            {
                http.socketBase socket = this.socket;
                response response = null;
                try
                {
                    if (form != null)
                    {
                        socketIdentity = form.Identity;
                        tcpBase.client clientUserInfo = new tcpBase.client();
                        tcpBase.httpPage page = tcpBase.httpPage.Get(socket, domainServer, socketIdentity, request, form);
                        ((webPage.page)page).Response = (response = response.Get(true));
                        clientUserInfo.UserInfo = page;
                        socket.TcpCommandSocket.ClientUserInfo = clientUserInfo;
                        command.OnCommand(socket);
                        return;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                finally
                {
                    this.socket = null;
                    domainServer = null;
                    request = null;
                    typePool<httpLoadForm>.Push(this);
                    response.Push(ref response);
                }
                socket.ResponseError(socketIdentity, http.response.state.ServerError500);
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
            /// 获取HTTP请求表单数据加载处理委托
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">请求头部信息</param>
            /// <param name="command">HTTP命令处理委托</param>
            /// <returns>HTTP请求表单数据加载处理委托</returns>
            internal static httpLoadForm Get
                (http.socketBase socket, httpServers.server domainServer, long socketIdentity, http.requestHeader request, httpCommand command)
            {
                httpLoadForm loadForm = typePool<httpLoadForm>.Pop() ?? new httpLoadForm();
                loadForm.socket = socket;
                loadForm.domainServer = domainServer;
                loadForm.socketIdentity = socketIdentity;
                loadForm.request = request;
                loadForm.command = command;
                return loadForm;
            }
        }
        /// <summary>
        /// 资源释放异常
        /// </summary>
        private static readonly Exception objectDisposedException = new ObjectDisposedException("tcpStream");
        /// <summary>
        /// 错误支持异常
        /// </summary>
        private static readonly Exception notSupportedException = new NotSupportedException();
        /// <summary>
        /// 错误操作异常
        /// </summary>
        private static readonly Exception invalidOperationException = new InvalidOperationException();
        /// <summary>
        /// IO异常
        /// </summary>
        private static readonly Exception ioException = new IOException();
        /// <summary>
        /// 空参数异常
        /// </summary>
        private static readonly Exception argumentNullException = new ArgumentNullException();
        /// <summary>
        /// 参数超出范围异常
        /// </summary>
        private static readonly Exception argumentOutOfRangeException = new ArgumentOutOfRangeException();
        /// <summary>
        /// 参数异常
        /// </summary>
        private static readonly Exception argumentException = new ArgumentException();
        /// <summary>
        /// TCP调用套接字
        /// </summary>
        /// <typeparam name="serverType">TCP调用服务端类型</typeparam>
        public sealed unsafe class socket : commandSocket<commandServer>
        {
            /// <summary>
            /// 异步回调
            /// </summary>
            /// <typeparam name="outputParameterType">输出参数类型</typeparam>
            /// <typeparam name="returnType">返回值类型</typeparam>
            public sealed class callback
            {
                /// <summary>
                /// 会话标识
                /// </summary>
                private streamIdentity identity;
                /// <summary>
                /// 异步套接字
                /// </summary>
                private socket socket;
                /// <summary>
                /// 异步回调
                /// </summary>
                private Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue, bool> onReturnHandle;
                /// <summary>
                /// 套接字重用标识
                /// </summary>
                private int pushIdentity;
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="isKeep">是否保持回调</param>
                private callback(byte isKeep)
                {
                    if (isKeep == 0) onReturnHandle = onReturn;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(fastCSharp.code.cSharp.asynchronousMethod.returnValue returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        socket socket = this.socket;
                        streamIdentity identity = this.identity;
                        this.socket = null;
                        typePool<callback>.Push(this);
                        return socket.SendStream(identity, returnValue);
                    }
                    this.socket = null;
                    typePool<callback>.Push(this);
                    return false;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onlyCallback(fastCSharp.code.cSharp.asynchronousMethod.returnValue returnValue)
                {
                    return this.socket.PushIdentity == pushIdentity && socket.SendStream(identity, returnValue);
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue, bool> Get(socket socket)
                {
                    callback value = typePool<callback>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new callback(0);
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue, bool> GetKeep(socket socket)
                {
                    try
                    {
                        callback value = new callback(1);
                        value.socket = socket;
                        value.identity = socket.identity;
                        value.pushIdentity = socket.PushIdentity;
                        return value.onlyCallback;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = false });
                    return null;
                }
            }
            /// <summary>
            /// 异步回调
            /// </summary>
            /// <typeparam name="outputParameterType">输出参数类型</typeparam>
            /// <typeparam name="returnType">返回值类型</typeparam>
            public sealed class callback<outputParameterType, returnType>
                where outputParameterType : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<returnType>
            {
                /// <summary>
                /// 会话标识
                /// </summary>
                private streamIdentity identity;
                /// <summary>
                /// 异步套接字
                /// </summary>
                private socket socket;
                /// <summary>
                /// 异步回调
                /// </summary>
                private Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>, bool> onReturnHandle;
                /// <summary>
                /// 输出参数
                /// </summary>
                private outputParameterType outputParameter;
                /// <summary>
                /// 套接字重用标识
                /// </summary>
                private int pushIdentity;
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="isKeep">是否保持回调</param>
                private callback(byte isKeep)
                {
                    if (isKeep == 0) onReturnHandle = onReturn;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        asynchronousMethod.returnValue<outputParameterType> outputParameter = new asynchronousMethod.returnValue<outputParameterType> { IsReturn = returnValue.IsReturn };
                        if (returnValue.IsReturn)
                        {
                            this.outputParameter.Return = returnValue.Value;
                            outputParameter.Value = this.outputParameter;
                        }
                        socket socket = this.socket;
                        streamIdentity identity = this.identity;
                        this.outputParameter = default(outputParameterType);
                        this.socket = null;
                        typePool<callback<outputParameterType, returnType>>.Push(this);
                        return socket.SendStream(identity, outputParameter);
                    }
                    this.outputParameter = default(outputParameterType);
                    this.socket = null;
                    typePool<callback<outputParameterType, returnType>>.Push(this);
                    return false;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onlyCallback(fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        asynchronousMethod.returnValue<outputParameterType> outputParameter = new asynchronousMethod.returnValue<outputParameterType> { IsReturn = returnValue.IsReturn };
                        if (returnValue.IsReturn)
                        {
                            this.outputParameter.Return = returnValue.Value;
                            outputParameter.Value = this.outputParameter;
                        }
                        return socket.SendStream(identity, outputParameter);
                    }
                    return false;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>, bool> Get(socket socket, outputParameterType outputParameter)
                {
                    callback<outputParameterType, returnType> value = typePool<callback<outputParameterType, returnType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new callback<outputParameterType, returnType>(0);
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.outputParameter = outputParameter;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>, bool> GetKeep(socket socket, outputParameterType outputParameter)
                {
                    try
                    {
                        callback<outputParameterType, returnType> value = new callback<outputParameterType, returnType>(1);
                        value.socket = socket;
                        value.outputParameter = outputParameter;
                        value.identity = socket.identity;
                        value.pushIdentity = socket.PushIdentity;
                        return value.onlyCallback;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = false });
                    return null;
                }
            }
            /// <summary>
            /// 异步回调
            /// </summary>
            /// <typeparam name="outputParameterType">输出参数类型</typeparam>
            /// <typeparam name="returnType">返回值类型</typeparam>
            public sealed class callbackJson<outputParameterType, returnType>
                where outputParameterType : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<returnType>
            {
                /// <summary>
                /// 会话标识
                /// </summary>
                private streamIdentity identity;
                /// <summary>
                /// 异步套接字
                /// </summary>
                private socket socket;
                /// <summary>
                /// 异步回调
                /// </summary>
                private Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>, bool> onReturnHandle;
                /// <summary>
                /// 输出参数
                /// </summary>
                private outputParameterType outputParameter;
                /// <summary>
                /// 套接字重用标识
                /// </summary>
                private int pushIdentity;
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="isKeep">是否保持回调</param>
                private callbackJson(byte isKeep)
                {
                    if (isKeep == 0) onReturnHandle = onReturn;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        asynchronousMethod.returnValue<outputParameterType> outputParameter = new asynchronousMethod.returnValue<outputParameterType> { IsReturn = returnValue.IsReturn };
                        if (returnValue.IsReturn)
                        {
                            this.outputParameter.Return = returnValue.Value;
                            outputParameter.Value = this.outputParameter;
                        }
                        socket socket = this.socket;
                        streamIdentity identity = this.identity;
                        this.outputParameter = default(outputParameterType);
                        this.socket = null;
                        typePool<callbackJson<outputParameterType, returnType>>.Push(this);
                        return socket.SendStreamJson(identity, outputParameter);
                    }
                    this.outputParameter = default(outputParameterType);
                    this.socket = null;
                    typePool<callbackJson<outputParameterType, returnType>>.Push(this);
                    return false;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onlyCallback(fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        asynchronousMethod.returnValue<outputParameterType> outputParameter = new asynchronousMethod.returnValue<outputParameterType> { IsReturn = returnValue.IsReturn };
                        if (returnValue.IsReturn)
                        {
                            this.outputParameter.Return = returnValue.Value;
                            outputParameter.Value = this.outputParameter;
                        }
                        return socket.SendStreamJson(identity, outputParameter);
                    }
                    return false;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>, bool> Get(socket socket, outputParameterType outputParameter)
                {
                    callbackJson<outputParameterType, returnType> value = typePool<callbackJson<outputParameterType, returnType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new callbackJson<outputParameterType, returnType>(0);
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.outputParameter = outputParameter;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>, bool> GetKeep(socket socket, outputParameterType outputParameter)
                {
                    try
                    {
                        callbackJson<outputParameterType, returnType> value = new callbackJson<outputParameterType, returnType>(1);
                        value.socket = socket;
                        value.outputParameter = outputParameter;
                        value.identity = socket.identity;
                        value.pushIdentity = socket.PushIdentity;
                        return value.onlyCallback;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = false });
                    return null;
                }
            }
            /// <summary>
            /// 验证函数异步回调
            /// </summary>
            /// <typeparam name="outputParameterType">输出参数类型</typeparam>
            public sealed class callback<outputParameterType>
                where outputParameterType : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
            {
                /// <summary>
                /// 会话标识
                /// </summary>
                private streamIdentity identity;
                /// <summary>
                /// 异步套接字
                /// </summary>
                private socket socket;
                /// <summary>
                /// 异步回调
                /// </summary>
                private Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool>, bool> onReturnHandle;
                /// <summary>
                /// 输出参数
                /// </summary>
                private outputParameterType outputParameter;
                /// <summary>
                /// 套接字重用标识
                /// </summary>
                private int pushIdentity;
                /// <summary>
                /// 异步回调
                /// </summary>
                private callback()
                {
                    onReturnHandle = onReturn;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        asynchronousMethod.returnValue<outputParameterType> outputParameter = new asynchronousMethod.returnValue<outputParameterType>();
                        if (returnValue.IsReturn)
                        {
                            try
                            {
                                if (returnValue.Value)
                                {
                                    outputParameter.IsReturn = this.socket.IsVerifyMethod = true;
                                    this.outputParameter.Return = returnValue.Value;
                                    outputParameter.Value = this.outputParameter;
                                }
                            }
                            catch (Exception error)
                            {
                                fastCSharp.log.Error.Add(error, null, true);
                            }
                        }
                        socket socket = this.socket;
                        streamIdentity identity = this.identity;
                        this.outputParameter = default(outputParameterType);
                        this.socket = null;
                        typePool<callback<outputParameterType>>.Push(this);
                        bool isReturn = socket.SendStream(identity, outputParameter);
                        if (!this.socket.IsVerifyMethod) log.Default.Add("TCP调用客户端验证失败 " + this.socket.Socket.RemoteEndPoint.ToString(), false, false);
                        return isReturn;
                    }
                    this.outputParameter = default(outputParameterType);
                    this.socket = null;
                    typePool<callback<outputParameterType>>.Push(this);
                    return false;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool>, bool> Get(socket socket, outputParameterType outputParameter)
                {
                    callback<outputParameterType> value = typePool<callback<outputParameterType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new callback<outputParameterType>();
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.outputParameter = outputParameter;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }
            }
            /// <summary>
            /// 验证函数异步回调
            /// </summary>
            /// <typeparam name="outputParameterType">输出参数类型</typeparam>
            public sealed class callbackJson<outputParameterType>
                where outputParameterType : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
            {
                /// <summary>
                /// 会话标识
                /// </summary>
                private streamIdentity identity;
                /// <summary>
                /// 异步套接字
                /// </summary>
                private socket socket;
                /// <summary>
                /// 异步回调
                /// </summary>
                private Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool>, bool> onReturnHandle;
                /// <summary>
                /// 输出参数
                /// </summary>
                private outputParameterType outputParameter;
                /// <summary>
                /// 套接字重用标识
                /// </summary>
                private int pushIdentity;
                /// <summary>
                /// 异步回调
                /// </summary>
                private callbackJson()
                {
                    onReturnHandle = onReturn;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        asynchronousMethod.returnValue<outputParameterType> outputParameter = new asynchronousMethod.returnValue<outputParameterType>();
                        if (returnValue.IsReturn)
                        {
                            try
                            {
                                if (returnValue.Value)
                                {
                                    outputParameter.IsReturn = this.socket.IsVerifyMethod = true;
                                    this.outputParameter.Return = returnValue.Value;
                                    outputParameter.Value = this.outputParameter;
                                }
                            }
                            catch (Exception error)
                            {
                                fastCSharp.log.Error.Add(error, null, true);
                            }
                        }
                        socket socket = this.socket;
                        streamIdentity identity = this.identity;
                        this.outputParameter = default(outputParameterType);
                        this.socket = null;
                        typePool<callbackJson<outputParameterType>>.Push(this);
                        bool isReturn = socket.SendStreamJson(identity, outputParameter);
                        if (!this.socket.IsVerifyMethod) log.Default.Add("TCP调用客户端验证失败 " + this.socket.Socket.RemoteEndPoint.ToString(), false, false);
                        return isReturn;
                    }
                    this.outputParameter = default(outputParameterType);
                    this.socket = null;
                    typePool<callbackJson<outputParameterType>>.Push(this);
                    return false;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool>, bool> Get(socket socket, outputParameterType outputParameter)
                {
                    callbackJson<outputParameterType> value = typePool<callbackJson<outputParameterType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new callbackJson<outputParameterType>();
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.outputParameter = outputParameter;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }
            }
            /// <summary>
            /// 异步回调
            /// </summary>
            /// <typeparam name="outputParameterType">输出参数类型</typeparam>
            /// <typeparam name="returnType">返回值类型</typeparam>
            public sealed class callbackHttp
            {
                /// <summary>
                /// HTTP页面
                /// </summary>
                private tcpBase.httpPage httpPage;
                /// <summary>
                /// 异步回调
                /// </summary>
                private Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue, bool> onReturnHandle;
                /// <summary>
                /// 异步回调
                /// </summary>
                private callbackHttp()
                {
                    onReturnHandle = onReturn;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(asynchronousMethod.returnValue returnValue)
                {
                    tcpBase.httpPage httpPage = this.httpPage;
                    this.httpPage = null;
                    bool isResponse = false;
                    try
                    {
                        typePool<callbackHttp>.Push(this);
                    }
                    finally
                    {
                        if (httpPage.Response(returnValue)) isResponse = true;
                    }
                    return isResponse;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="httpPage">HTTP页面</param>
                /// <returns>异步回调</returns>
                public static Func<asynchronousMethod.returnValue, bool> Get(tcpBase.httpPage httpPage)
                {
                    callbackHttp value = typePool<callbackHttp>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new callbackHttp();
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                        }
                        if (value == null)
                        {
                            httpPage.Socket.ResponseError(httpPage.SocketIdentity, http.response.state.ServerError500);
                            return null;
                        }
                    }
                    value.httpPage = httpPage;
                    return value.onReturnHandle;
                }
            }
            /// <summary>
            /// 异步回调
            /// </summary>
            /// <typeparam name="outputParameterType">输出参数类型</typeparam>
            /// <typeparam name="returnType">返回值类型</typeparam>
            public sealed class callbackHttp<outputParameterType, returnType>
                where outputParameterType : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<returnType>
            {
                /// <summary>
                /// HTTP页面
                /// </summary>
                private tcpBase.httpPage httpPage;
                /// <summary>
                /// 输出参数
                /// </summary>
                private outputParameterType outputParameter;
                /// <summary>
                /// 异步回调
                /// </summary>
                private Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>, bool> onReturnHandle;
                /// <summary>
                /// 异步回调
                /// </summary>
                private callbackHttp()
                {
                    onReturnHandle = onReturn;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType> returnValue)
                {
                    asynchronousMethod.returnValue<outputParameterType> outputParameter = new asynchronousMethod.returnValue<outputParameterType> { IsReturn = returnValue.IsReturn };
                    if (returnValue.IsReturn)
                    {
                        this.outputParameter.Return = returnValue.Value;
                        outputParameter.Value = this.outputParameter;
                    }
                    tcpBase.httpPage httpPage = this.httpPage;
                    this.outputParameter = default(outputParameterType);
                    this.httpPage = null;
                    bool isResponse = false;
                    try
                    {
                        typePool<callbackHttp<outputParameterType, returnType>>.Push(this);
                    }
                    finally
                    {
                        if (httpPage.Response(outputParameter)) isResponse = true;
                    }
                    return isResponse;
                }
                /// <summary>
                /// 异步回调
                /// </summary>
                /// <param name="httpPage">HTTP页面</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>, bool> Get(tcpBase.httpPage httpPage, outputParameterType outputParameter)
                {
                    callbackHttp<outputParameterType, returnType> value = typePool<callbackHttp<outputParameterType, returnType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new callbackHttp<outputParameterType, returnType>();
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                        }
                        if (value == null)
                        {
                            httpPage.Socket.ResponseError(httpPage.SocketIdentity, http.response.state.ServerError500);
                            return null;
                        }
                    }
                    value.httpPage = httpPage;
                    value.outputParameter = outputParameter;
                    return value.onReturnHandle;
                }
            }

            /// <summary>
            /// 套接字池下一个TCP调用套接字
            /// </summary>
            internal socket PoolNext;
            /// <summary>
            /// 套接字重用标识
            /// </summary>
            internal int PushIdentity;
            ///// <summary>
            ///// 是否已经设置流接收超时
            ///// </summary>
            //private int isStreamReceiveTimeout;
            /// <summary>
            /// 接收数据缓冲区起始位置
            /// </summary>
            private byte* receiveDataFixed;
            /// <summary>
            /// 接收数据结束位置
            /// </summary>
            private int receiveEndIndex;
            /// <summary>
            /// 接收数据起始位置
            /// </summary>
            private int receiveStartIndex;
            /// <summary>
            /// 验证超时
            /// </summary>
            private DateTime verifyTimeout;
            /// <summary>
            /// 默认HTTP内容编码
            /// </summary>
            internal override Encoding HttpEncoding
            {
                get { return commandSocketProxy.attribute.HttpEncoding; }
            }
            /// <summary>
            /// 客户端IP地址
            /// </summary>
            internal int Ipv4;
            /// <summary>
            /// 客户端IP地址
            /// </summary>
            internal ipv6Hash Ipv6;
            /// <summary>
            /// 创建输出数据并执行
            /// </summary>
            private Action buildOutputHandle;
            /// <summary>
            /// 当前处理命令
            /// </summary>
            private command command;
            /// <summary>
            /// 获取TCP调用客户端套接字类型
            /// </summary>
            private Action<bool> onSocketTypeHandle;
            ///// <summary>
            ///// 是否输出调试信息
            ///// </summary>
            //private bool isOutputDebug;
            /// <summary>
            /// 初始化同步套接字
            /// </summary>
            /// <param name="client">客户端信息</param>
            /// <param name="server">TCP调用服务</param>
            /// <param name="sendData">发送数据缓冲区</param>
            /// <param name="receiveData">接收数据缓冲区</param>
            internal socket(clientQueue<Socket>.clientInfo client, commandServer server, byte[] sendData, byte[] receiveData)
                : base(client.Client, sendData, receiveData, server, false)
            {
                Ipv4 = client.Ipv4;
                Ipv6 = client.Ipv6;
                buildOutputHandle = buildOutput;
                onSocketTypeHandle = onSocketType;
                //isOutputDebug = server.attribute.IsOutputDebug;
            }
            /// <summary>
            /// 重新设置套接字
            /// </summary>
            /// <param name="client">客户端信息</param>
            internal void SetSocket(clientQueue<Socket>.clientInfo client)
            {
                Socket = client.Client;
                Ipv4 = client.Ipv4;
                Ipv6 = client.Ipv6;
                IsVerifyMethod = false;
                //isStreamReceiveTimeout = 0;
                socketError = SocketError.Success;
                lastException = null;
                ClientUserInfo = null;
                LoadBalancingCheckIdentity = 0;
            }
            /// <summary>
            /// 关闭套接字连接
            /// </summary>
            protected override void dispose()
            {
                base.dispose();
                ClientUserInfo = null;
                interlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                try
                {
                    if (tcpStreamReceivers != null)
                    {
                        cancelTcpStream();
                        foreach (tcpStreamReceiver tcpStreamReceiver in tcpStreamReceivers)
                        {
                            if (tcpStreamReceiver.ReceiveWait != null)
                            {
                                tcpStreamReceiver.ReceiveWait.Set();
                                tcpStreamReceiver.ReceiveWait.Close();
                            }
                        }
                    }
                }
                finally { tcpStreamReceiveLock = 0; }
                interlocked.NoCheckCompareSetSleep0(ref outputLock);
                outputs.Clear();
                outputLock = 0;
            }
            /// <summary>
            /// 取消TCP流
            /// </summary>
            private void cancelTcpStream()
            {
                while (tcpStreamReceiveIndex != 0) tcpStreamReceivers[--tcpStreamReceiveIndex].Cancel(true);
                freeTcpStreamIndexs.Empty();
            }
            /// <summary>
            /// 临时数据缓冲区
            /// </summary>
            private static readonly byte[] closeData = new byte[sizeof(commandServer.streamIdentity) + sizeof(int)];

            static socket()
            {
                fixed (byte* dataFixed = closeData)
                {
                    *(streamIdentity*)dataFixed = new streamIdentity { Index = 0, Identity = int.MinValue };
                    *(int*)(dataFixed + sizeof(streamIdentity)) = 0;
                }
            }
            /// <summary>
            /// 关闭套接字
            /// </summary>
            protected override void close()
            {
                if (Socket != null)
                {
                    try
                    {
                        interlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                        try
                        {
                            cancelTcpStream();
                        }
                        finally { tcpStreamReceiveLock = 0; }
                        Socket.Send(closeData, 0, closeData.Length, SocketFlags.None, out socketError);
                    }
                    catch { }
                    finally { base.close(); }
                }
            }
            /// <summary>
            /// 关闭套接字
            /// </summary>
            internal void Close()
            {
                close();
                interlocked.CompareSetSleep1(ref isOutputBuilding);
                isOutputBuilding = 0;
            }
            /// <summary>
            /// TCP套接字添加到池
            /// </summary>
            internal override void PushPool()
            {
                commandSocketProxy.pushSocket(this);
            }
            /// <summary>
            /// 负载均衡联通测试标识
            /// </summary>
            internal int LoadBalancingCheckIdentity;
            /// <summary>
            /// 负载均衡联通测试
            /// </summary>
            /// <param name="identity">负载均衡联通测试标识</param>
            /// <returns>是否成功</returns>
            internal bool LoadBalancingCheck(int identity)
            {
                if (Socket != null && identity == LoadBalancingCheckIdentity)
                {
                    try
                    {

                        outputParameter output = outputParameter.Get(new streamIdentity { Index = 1 }, new asynchronousMethod.returnValue { IsReturn = true });
                        if (output != null) return pushOutput(output);
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                }
                return false;
            }
            /// <summary>
            /// 获取TCP调用客户端套接字类型
            /// </summary>
            internal void VerifySocketType()
            {
                int verifySeconds = commandSocketProxy.attribute.VerifySeconds;
                if (verifySeconds > 0) verifyTimeout = date.NowSecond.AddSeconds(verifySeconds + 1);
                else
                {
                    verifyTimeout = DateTime.MaxValue;
                    verifySeconds = config.tcpCommand.Default.DefaultTimeout;
                }
                if ((verifySeconds *= 1000) <= 0) verifySeconds = int.MaxValue;
                Socket.ReceiveTimeout = Socket.SendTimeout = verifySeconds;
                receive(onSocketTypeHandle, 0, sizeof(int), verifyTimeout);
            }
            /// <summary>
            /// 获取TCP调用客户端套接字类型
            /// </summary>
            /// <param name="isSocket">是否成功</param>
            private void onSocketType(bool isSocket)
            {
                if (isSocket)
                {
                    fixed (byte* receiveDataFixed = receiveData)
                    {
                        if (*(int*)receiveDataFixed == (commandSocketProxy.attribute.IsIdentityCommand ? commandServer.IdentityVerifyIdentity : commandServer.VerifyIdentity))
                        {
                            if (commandSocketProxy.isClientUserInfo) ClientUserInfo = new tcpBase.client();
                            if (commandSocketProxy.verify == null)
                            {
                                verifyMethod();
                                return;
                            }
                            try
                            {
                                if (commandSocketProxy.verify.Verify(this))
                                {
                                    verifyMethod();
                                    return;
                                }
                                log.Default.Add("TCP调用客户端验证失败 " + Socket.RemoteEndPoint.ToString(), false, false);
                            }
                            catch (Exception error)
                            {
                                log.Error.Add(error, "TCP调用客户端验证失败 " + Socket.RemoteEndPoint.ToString(), false);
                            }
                        }
                        else if (commandSocketProxy.attribute.IsHttpClient && fastCSharp.web.http.GetMethod(receiveDataFixed) != web.http.methodType.None)
                        {
                            http.socket.Start(commandSocketProxy.HttpServers, this);
                            return;
                        }
                        else if (*(int*)receiveDataFixed == (commandSocketProxy.attribute.IsIdentityCommand ? commandServer.VerifyIdentity : commandServer.IdentityVerifyIdentity))
                        {
                            log.Error.Add("TCP调用客户端命令模式不匹配" + Socket.RemoteEndPoint.ToString(), false, false);
                        }
                    }
                }
                commandSocketProxy.pushSocket(this);
            }
            /// <summary>
            /// 接收命令长度处理
            /// </summary>
            private Action<int> onReceiveStreamCommandLengthHandle;
            /// <summary>
            /// 接收命令处理
            /// </summary>
            private Action<int> onReceiveStreamCommandHandle;
            /// <summary>
            /// 执行命令委托
            /// </summary>
            private Action<memoryPool.pushSubArray> doStreamCommandHandle;
            /// <summary>
            /// 同步接收命令
            /// </summary>
            private Action receiveCommandHandle;
            /// <summary>
            /// 异步套接字方法验证
            /// </summary>
            private void verifyMethod()
            {
                if (commandSocketProxy.identityOnCommands == null ? commandSocketProxy.verifyCommand.Length == 0 : commandSocketProxy.verifyCommandIdentity == nullVerifyCommandIdentity) IsVerifyMethod = true;
                fixed (byte* dataFixed = sendData) *(int*)dataFixed = commandSocketProxy.attribute.IsIdentityCommand ? commandServer.IdentityVerifyIdentity : commandServer.VerifyIdentity;
                if (send(sendData, 0, sizeof(int)))
                {
                    if (commandSocketProxy.attribute.IsServerAsynchronousReceive)
                    {
                        if (onReceiveStreamCommandLengthHandle == null)
                        {
                            if (commandSocketProxy.identityOnCommands == null)
                            {
                                onReceiveStreamCommandLengthHandle = onReceiveStreamCommandLength;
                                onReceiveStreamCommandHandle = onReceiveStreamCommand;
                            }
                            else onReceiveStreamCommandLengthHandle = onReceiveStreamIdentityCommand;
                            doStreamCommandHandle = doStreamCommand;
                        }
                        receiveEndIndex = receiveStartIndex = 0;
                        receiveStreamCommand();
                    }
                    else
                    {
                        if (receiveCommandHandle == null) receiveCommandHandle = receiveCommand;
                        threadPool.TinyPool.FastStart(receiveCommandHandle, null, null);
                    }
                    return;
                }
                commandSocketProxy.pushSocket(this);
            }
            /// <summary>
            /// 同步接收命令
            /// </summary>
            private void receiveCommand()
            {
                try
                {
                    receiveStartIndex = 0;
                    fixed (byte* receiveDataFixed = receiveData)
                    {
                        this.receiveDataFixed = receiveDataFixed;
                        if (commandSocketProxy.identityOnCommands == null) receiveDataCommand();
                        else receiveIdentityCommand();
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                finally { commandSocketProxy.pushSocket(this); }
            }
            /// <summary>
            /// 接收命令
            /// </summary>
            private void receiveIdentityCommand()
            {
                command[] commands = commandSocketProxy.identityOnCommands;
                if (commandSocketProxy.verifyCommandIdentity == nullVerifyCommandIdentity)
                {
                    receiveEndIndex = 0;
                    IsVerifyMethod = true;
                }
                else
                {
                    if ((receiveEndIndex = tryReceive(0, sizeof(int) * 2 + sizeof(commandServer.streamIdentity), verifyTimeout)) >= sizeof(int) * 2 + sizeof(commandServer.streamIdentity))
                    {
                        if (*(int*)receiveDataFixed == commandSocketProxy.verifyCommandIdentity)
                        {
                            command = commands[commandSocketProxy.verifyCommandIdentity];
                            identity = *(commandServer.streamIdentity*)(receiveDataFixed + sizeof(int));
                            receiveStartIndex = sizeof(int) * 2 + sizeof(commandServer.streamIdentity);
                            doCommand(*(int*)(receiveDataFixed + (sizeof(int) + sizeof(commandServer.streamIdentity))));
                        }
                        else log.Error.Add(null, "TCP验证函数命令匹配失败 " + (*(int*)receiveDataFixed).toString() + "<>" + commandSocketProxy.verifyCommandIdentity.toString(), false);
                    }
                    else log.Error.Add(null, "TCP验证函数命令数据接受失败 " + receiveEndIndex.toString() + "<" + (sizeof(int) * 2 + sizeof(commandServer.streamIdentity)).toString(), false);
                }
                if (IsVerifyMethod)
                {
                    Socket.ReceiveTimeout = commandSocketProxy.receiveCommandTimeout == 0 ? -1 : commandSocketProxy.receiveCommandTimeout;
                    while (tryReceiveIdentityCommand())
                    {
                        byte* start = receiveDataFixed + receiveStartIndex;
                        int commandIdentity = *(int*)start;
                        if ((uint)commandIdentity < commands.Length)
                        {
                            command = commands[commandIdentity];
                            identity = *(commandServer.streamIdentity*)(start + sizeof(int));
                            receiveStartIndex += sizeof(int) * 2 + sizeof(commandServer.streamIdentity);
                            if (doCommand(*(int*)(start + (sizeof(int) + sizeof(commandServer.streamIdentity))))) continue;
                        }
                        log.Default.Add(commandSocketProxy.attribute.ServiceName + " 缺少命令处理委托 [" + commandIdentity.toString() + "]", false, false);
                        break;
                    }
                }
            }
            /// <summary>
            /// 接收命令
            /// </summary>
            /// <returns>是否成功</returns>
            private bool tryReceiveIdentityCommand()
            {
                int receiveLength = receiveEndIndex - receiveStartIndex;
                if (receiveLength >= sizeof(int) * 2 + sizeof(commandServer.streamIdentity)) return true;
                if (receiveLength != 0) Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                receiveEndIndex = tryReceive(receiveLength, sizeof(int) * 2 + sizeof(commandServer.streamIdentity), date.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks));
                if (receiveEndIndex >= sizeof(int) * 2 + sizeof(commandServer.streamIdentity))
                {
                    receiveStartIndex = 0;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 接收命令
            /// </summary>
            private void receiveDataCommand()
            {
                fastCSharp.stateSearcher.ascii<command> commands = commandSocketProxy.onCommands;
                if (commandSocketProxy.verifyCommand.Length == 0)
                {
                    receiveEndIndex = 0;
                    IsVerifyMethod = true;
                }
                else
                {
                    receiveStartIndex = commandSocketProxy.verifyCommand.Length + (sizeof(int) * 2 + sizeof(commandServer.streamIdentity));
                    if ((receiveEndIndex = tryReceive(0, receiveStartIndex, verifyTimeout)) >= receiveStartIndex && *(int*)receiveDataFixed == receiveStartIndex
                        && commandSocketProxy.verifyCommand.Equals(subArray<byte>.Unsafe(receiveData, sizeof(int), commandSocketProxy.verifyCommand.Length)))
                    {
                        byte* start = receiveDataFixed + receiveStartIndex;
                        command = commands.Get(commandSocketProxy.verifyCommand);
                        identity = *(commandServer.streamIdentity*)(start - (sizeof(int) + sizeof(commandServer.streamIdentity)));
                        doCommand(*(int*)(start - (sizeof(int))));
                    }
                    else log.Error.Add(null, "TCP验证函数命令匹配失败", false);
                }
                if (IsVerifyMethod)
                {
                    Socket.ReceiveTimeout = commandSocketProxy.receiveCommandTimeout == 0 ? -1 : commandSocketProxy.receiveCommandTimeout;
                    while (tryReceiveDataCommand())
                    {
                        byte* start = receiveDataFixed + receiveStartIndex;
                        int commandLength = *(int*)start;
                        if (commands.Get(subArray<byte>.Unsafe(receiveData, receiveStartIndex + sizeof(int), commandLength - (sizeof(int) * 2 + sizeof(commandServer.streamIdentity))), ref command))
                        {
                            start += commandLength;
                            receiveStartIndex += commandLength;
                            identity = *(commandServer.streamIdentity*)(start - (sizeof(int) + sizeof(commandServer.streamIdentity)));
                            if (doCommand(*(int*)(start - (sizeof(int))))) continue;
                        }
                        log.Default.Add(commandSocketProxy.attribute.ServiceName + " 缺少命令处理委托 " + subArray<byte>.Unsafe(receiveData, receiveStartIndex + sizeof(int), commandLength - (sizeof(int) * 2 + sizeof(commandServer.streamIdentity))).GetReverse().deSerialize(), false, false);
                        break;
                    }
                }
            }
            /// <summary>
            /// 接收命令
            /// </summary>
            /// <returns>是否成功</returns>
            private bool tryReceiveDataCommand()
            {
                int receiveLength = receiveEndIndex - receiveStartIndex;
                if (receiveLength >= sizeof(int) * 3 + sizeof(commandServer.streamIdentity))
                {
                    int commandLength = *(int*)(receiveDataFixed + receiveStartIndex);
                    if (receiveLength >= commandLength) return true;
                    Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                    receiveEndIndex = tryReceive(receiveLength, commandLength, date.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks));
                    if (receiveEndIndex >= commandLength)
                    {
                        receiveStartIndex = 0;
                        return true;
                    }
                }
                else
                {
                    if (receiveLength != 0) Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                    receiveEndIndex = tryReceive(receiveLength, sizeof(int) * 3 + sizeof(commandServer.streamIdentity), date.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks));
                    if (receiveEndIndex >= sizeof(int) * 3 + sizeof(commandServer.streamIdentity))
                    {
                        int commandLength = *(int*)receiveDataFixed;
                        if (receiveEndIndex >= commandLength)
                        {
                            receiveStartIndex = 0;
                            return true;
                        }
                    }
                }
                return false;
            }
            /// <summary>
            /// 接收命令数据并执行命令
            /// </summary>
            /// <param name="length">数据长度</param>
            private bool doCommand(int length)
            {
                if (length == 0)
                {
                    if (command.MaxDataLength == 0)
                    {
                        command.OnCommand(this, default(subArray<byte>));
                        return true;
                    }
                }
                else
                {
                    int dataLength = length > 0 ? length : -length;
                    if (dataLength <= command.MaxDataLength)
                    {
                        int receiveLength = receiveEndIndex - receiveStartIndex;
                        if (dataLength <= receiveData.Length)
                        {
                            if (dataLength > receiveLength)
                            {
                                if (receiveLength != 0) Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                                receiveEndIndex = tryReceive(receiveLength, dataLength, commandSocketProxy.getReceiveTimeout(dataLength));
                                if (receiveEndIndex < dataLength) return false;
                                receiveStartIndex = 0;
                            }
                            if (length >= 0)
                            {
                                subArray<byte> data = subArray<byte>.Unsafe(receiveData, receiveStartIndex, dataLength);
                                receiveStartIndex += dataLength;
                                command.OnCommand(this, data);
                            }
                            else
                            {
                                subArray<byte> data = stream.Deflate.GetDeCompressUnsafe(receiveData, receiveStartIndex, dataLength, fastCSharp.memoryPool.StreamBuffers);
                                receiveStartIndex += dataLength;
                                command.OnCommand(this, data);
                                fastCSharp.memoryPool.StreamBuffers.Push(ref data.array);
                            }
                            return true;
                        }
                        byte[] buffer = BigBuffers.Get(dataLength);
                        if (receiveLength != 0) Buffer.BlockCopy(receiveData, receiveStartIndex, buffer, 0, receiveLength);
                        if (receive(buffer, receiveLength, dataLength, commandSocketProxy.getReceiveTimeout(dataLength)))
                        {
                            if (length >= 0) command.OnCommand(this, subArray<byte>.Unsafe(buffer, 0, dataLength));
                            else
                            {
                                subArray<byte> data = stream.Deflate.GetDeCompressUnsafe(buffer, 0, dataLength, fastCSharp.memoryPool.StreamBuffers);
                                command.OnCommand(this, data);
                                fastCSharp.memoryPool.StreamBuffers.Push(ref data.array);
                            }
                            receiveStartIndex = receiveEndIndex = 0;
                            BigBuffers.Push(ref buffer);
                            return true;
                        }
                        BigBuffers.Push(ref buffer);
                    }
                    else
                    {
                        log.Default.Add("接收数据长度超限 " + (length > 0 ? length : -length).toString() + " > " + command.MaxDataLength.toString(), false, false);
                    }
                }
                return false;
            }
            /// <summary>
            /// 接收命令
            /// </summary>
            private void receiveStreamCommand()
            {
                //if (IsVerifyMethod)
                //{
                //    if (Socket == null)
                //    {
                //        commandSocketProxy.pushSocket(this);
                //        return;
                //    }
                //    Socket.ReceiveTimeout = commandSocketProxy.receiveCommandTimeout == 0 ? -1 : commandSocketProxy.receiveCommandTimeout;
                //}
                try
                {
                NEXT:
                    int receiveLength = receiveEndIndex - receiveStartIndex;
                    if (commandSocketProxy.identityOnCommands == null)
                    {
                        if (receiveLength >= sizeof(int) * 3 + sizeof(commandServer.streamIdentity))
                        {
                            if (receiveStreamCommandLength()) goto NEXT;
                        }
                        else
                        {
                            if (receiveLength != 0) Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                            tryReceive(onReceiveStreamCommandLengthHandle, receiveLength, (sizeof(int) * 3 + sizeof(commandServer.streamIdentity)) - receiveLength, IsVerifyMethod ? date.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks) : verifyTimeout);
                        }
                    }
                    else if (receiveLength >= sizeof(int) * 2 + sizeof(commandServer.streamIdentity))
                    {
                        if (receiveStreamIdentityCommand()) goto NEXT;
                    }
                    else
                    {
                        if (receiveLength != 0) Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                        tryReceive(onReceiveStreamCommandLengthHandle, receiveLength, (sizeof(int) * 2 + sizeof(commandServer.streamIdentity)) - receiveLength, IsVerifyMethod ? date.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks) : verifyTimeout);
                    }
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }
            /// <summary>
            /// 接收命令长度处理
            /// </summary>
            /// <param name="receiveEndIndex">接收数据结束位置</param>
            private void onReceiveStreamCommandLength(int receiveEndIndex)
            {
                try
                {
                    //if (isOutputDebug) DebugLog.Add(commandSocketProxy.attribute.ServiceName + ".onReceiveStreamCommandLength(" + receiveEndIndex.toString() + ")", false, false);
                    if (receiveEndIndex >= sizeof(int) * 3 + sizeof(commandServer.streamIdentity))
                    {
                        this.receiveEndIndex = receiveEndIndex;
                        receiveStartIndex = 0;
                        if (receiveStreamCommandLength()) receiveStreamCommand();
                        return;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }
            /// <summary>
            /// 接收命令长度处理
            /// </summary>
            /// <returns>是否继续处理下一个命令</returns>
            private bool receiveStreamCommandLength()
            {
                fixed (byte* receiveDataFixed = receiveData)
                {
                    int commandLength = *(int*)(receiveDataFixed + receiveStartIndex);
                    if ((uint)commandLength <= commandSocketProxy.maxCommandLength)
                    {
                        int receiveLength = receiveEndIndex - receiveStartIndex;
                        if (receiveLength >= commandLength)
                        {
                            this.receiveDataFixed = receiveDataFixed;
                            return getStreamCommand();
                        }
                        if (receiveLength != 0) fastCSharp.unsafer.memory.Copy(receiveDataFixed + receiveStartIndex, receiveDataFixed, receiveLength);
                        tryReceive(onReceiveStreamCommandHandle, receiveLength, commandLength, IsVerifyMethod ? date.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks) : verifyTimeout);
                        return false;
                    }
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }
            /// <summary>
            /// 接收命令处理
            /// </summary>
            /// <param name="receiveEndIndex">接收数据结束位置</param>
            private void onReceiveStreamCommand(int receiveEndIndex)
            {
                try
                {
                    //if (isOutputDebug) DebugLog.Add(commandSocketProxy.attribute.ServiceName + ".onReceiveStreamCommand(" + receiveEndIndex.toString() + ")", false, false);
                    fixed (byte* receiveDataFixed = receiveData)
                    {
                        if (receiveEndIndex >= *(int*)receiveDataFixed)
                        {
                            this.receiveEndIndex = receiveEndIndex;
                            this.receiveDataFixed = receiveDataFixed;
                            receiveStartIndex = 0;
                            if (getStreamCommand()) receiveStreamCommand();
                            return;
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }
            /// <summary>
            /// 获取命令委托
            /// </summary>
            /// <returns>是否继续处理下一个命令</returns>
            private bool getStreamCommand()
            {
                int commandLength = *(int*)(receiveDataFixed + receiveStartIndex);
                subArray<byte> commandData = subArray<byte>.Unsafe(receiveData, receiveStartIndex + sizeof(int), receiveStartIndex + commandLength - (sizeof(int) * 2 + sizeof(commandServer.streamIdentity)));
                if (IsVerifyMethod)
                {
                    if (commandSocketProxy.onCommands.Get(commandData, ref command))
                    {
                        receiveStartIndex += commandLength;
                        return getStreamIdentity();
                    }
                    log.Default.Add(commandSocketProxy.attribute.ServiceName + " 缺少命令处理委托 " + commandData.GetReverse().deSerialize(), true, false);
                }
                else if (commandSocketProxy.verifyCommand.Equals(commandData))
                {
                    command = commandSocketProxy.onCommands.Get(commandSocketProxy.verifyCommand);
                    receiveStartIndex += commandLength;
                    return getStreamIdentity();
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }
            /// <summary>
            /// 获取会话标识
            /// </summary>
            /// <returns>是否继续处理下一个命令</returns>
            private bool getStreamIdentity()
            {
                byte* start = receiveDataFixed + receiveStartIndex;
                int length = *(int*)(start - sizeof(int));
                if (length == 0)
                {
                    if (command.MaxDataLength == 0)
                    {
                        identity = *(commandServer.streamIdentity*)(start - (sizeof(int) + sizeof(commandServer.streamIdentity)));
                        command.OnCommand(this, default(subArray<byte>));
                        //if ((receiveEndIndex -= receiveStartIndex) != 0) unsafer.memory.Copy(receiveDataFixed + receiveStartIndex, receiveDataFixed, receiveEndIndex);
                        return true;
                    }
                }
                else
                {
                    int dataLength = length > 0 ? length : -length;
                    if (dataLength <= command.MaxDataLength)
                    {
                        int receiveLength = receiveEndIndex - receiveStartIndex;
                        identity = *(commandServer.streamIdentity*)(start - (sizeof(int) + sizeof(commandServer.streamIdentity)));
                        if (dataLength <= receiveLength)
                        {
                            if (length >= 0)
                            {
                                subArray<byte> data = subArray<byte>.Unsafe(receiveData, receiveStartIndex, dataLength);
                                receiveStartIndex += dataLength;
                                command.OnCommand(this, data);
                                return true;
                            }
                            else
                            {
                                subArray<byte> data = stream.Deflate.GetDeCompressUnsafe(receiveData, receiveStartIndex, dataLength, fastCSharp.memoryPool.StreamBuffers);
                                receiveStartIndex += dataLength;
                                return isDoStreamCommand(new memoryPool.pushSubArray { Value = data, PushPool = fastCSharp.memoryPool.StreamBuffers.PushHandle });
                            }
                        }
                        receiveStream(doStreamCommandHandle, receiveDataFixed, length, IsVerifyMethod ? commandSocketProxy.getReceiveTimeout(dataLength) : verifyTimeout);
                        return false;
                    }
                    log.Default.Add("接收数据长度超限 " + (length > 0 ? length : -length).toString() + " > " + command.MaxDataLength.toString(), false, false);
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }
            /// <summary>
            /// 执行命令委托
            /// </summary>
            /// <param name="data">输出数据</param>
            private void doStreamCommand(memoryPool.pushSubArray data)
            {
                try
                {
                    if (isDoStreamCommand(data)) receiveStreamCommand();
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }
            /// <summary>
            /// 执行命令委托
            /// </summary>
            /// <param name="data">输出数据</param>
            /// <returns>是否继续处理下一个命令</returns>
            private bool isDoStreamCommand(memoryPool.pushSubArray data)
            {
                byte[] buffer = data.Value.Array;
                if (buffer != null)
                {
                    command.OnCommand(this, data.Value);
                    data.Push();
                    return true;
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }
            /// <summary>
            /// 接收命令处理
            /// </summary>
            /// <param name="receiveEndIndex">接收数据结束位置</param>
            private void onReceiveStreamIdentityCommand(int receiveEndIndex)
            {
                try
                {
                    //if (isOutputDebug) DebugLog.Add(commandSocketProxy.attribute.ServiceName + ".onReceiveStreamIdentityCommand(" + receiveEndIndex.toString() + ")", false, false);
                    if (receiveEndIndex >= sizeof(int) * 2 + sizeof(commandServer.streamIdentity))
                    {
                        this.receiveEndIndex = receiveEndIndex;
                        receiveStartIndex = 0;
                        if (receiveStreamIdentityCommand()) receiveStreamCommand();
                        return;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }
            /// <summary>
            /// 接收命令处理
            /// </summary>
            /// <returns>是否继续处理下一个命令</returns>
            private bool receiveStreamIdentityCommand()
            {
                fixed (byte* receiveDataFixed = receiveData)
                {
                    int command = *(int*)(receiveDataFixed + receiveStartIndex);
                    if (IsVerifyMethod)
                    {
                        if ((uint)command < commandSocketProxy.identityOnCommands.Length)
                        {
                            this.command = commandSocketProxy.identityOnCommands[command];
                            if (this.command.OnCommand != null)
                            {
                                this.receiveDataFixed = receiveDataFixed;
                                receiveStartIndex += sizeof(int) * 2 + sizeof(commandServer.streamIdentity);
                                return getStreamIdentity();
                            }
                        }
                        log.Default.Add(commandSocketProxy.attribute.ServiceName + " 缺少命令处理委托 [" + command.toString() + "]", true, false);
                    }
                    else if (command == commandSocketProxy.verifyCommandIdentity)
                    {
                        this.receiveDataFixed = receiveDataFixed;
                        this.command = commandSocketProxy.identityOnCommands[command];
                        receiveStartIndex += sizeof(int) * 2 + sizeof(commandServer.streamIdentity);
                        return getStreamIdentity();
                    }
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }

            /// <summary>
            /// 数据读取器
            /// </summary>
            private sealed class streamReceiver
            {
                /// <summary>
                /// 回调委托
                /// </summary>
                public Action<memoryPool.pushSubArray> Callback;
                /// <summary>
                /// TCP客户端套接字
                /// </summary>
                public socket Socket;
                /// <summary>
                /// 读取数据回调操作
                /// </summary>
                private Action<bool> onReadCompressDataHandle;
                /// <summary>
                /// 读取数据回调操作
                /// </summary>
                private Action<bool> onReadDataHandle;
                /// <summary>
                /// 读取数据是否大缓存
                /// </summary>
                private bool isBigBuffer;
                /// <summary>
                /// 数据读取器
                /// </summary>
                public streamReceiver()
                {
                    onReadCompressDataHandle = onReadCompressData;
                    onReadDataHandle = onReadData;
                }
                /// <summary>
                /// 读取数据
                /// </summary>
                /// <param name="dataFixed">接收数据起始位置</param>
                /// <param name="length">数据长度</param>
                /// <param name="timeout">接收超时</param>
                public unsafe void Receive(byte* dataFixed, int length, DateTime timeout)
                {
                    isBigBuffer = false;
                    int dataLength = length >= 0 ? length : -length, receiveLength = Socket.receiveEndIndex - Socket.receiveStartIndex;
                    if (dataLength <= Socket.receiveData.Length)
                    {
                        unsafer.memory.Copy(dataFixed + Socket.receiveStartIndex, dataFixed, receiveLength);
                        Socket.receiveStartIndex = Socket.receiveEndIndex = 0;
                        if (length >= 0) Socket.receive(onReadDataHandle, receiveLength, dataLength - receiveLength, timeout);
                        else Socket.receive(onReadCompressDataHandle, receiveLength, dataLength - receiveLength, timeout);
                    }
                    else
                    {
                        byte[] data = BigBuffers.Get(dataLength);
                        isBigBuffer = true;
                        unsafer.memory.Copy(dataFixed + Socket.receiveStartIndex, data, receiveLength);
                        Socket.receiveStartIndex = Socket.receiveEndIndex = 0;
                        if (length >= 0) Socket.receive(onReadDataHandle, data, receiveLength, dataLength - receiveLength, timeout);
                        else Socket.receive(onReadCompressDataHandle, data, receiveLength, dataLength - receiveLength, timeout);
                    }
                }
                /// <summary>
                /// 读取数据回调操作
                /// </summary>
                /// <param name="isSocket">是否操作成功</param>
                private void onReadData(bool isSocket)
                {
                    byte[] data = Socket.currentReceiveData;
                    Socket.currentReceiveData = Socket.receiveData;
                    if (isSocket)
                    {
                        push(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(data, 0, Socket.currentReceiveEndIndex), PushPool = isBigBuffer ? BigBuffers.PushHandle : null });
                    }
                    else
                    {
                        try
                        {
                            if (isBigBuffer) BigBuffers.Push(ref data);
                            push(default(memoryPool.pushSubArray));
                        }
                        finally { Socket.close(); }
                    }
                }
                /// <summary>
                /// 读取数据回调操作
                /// </summary>
                /// <param name="isSocket">是否操作成功</param>
                private void onReadCompressData(bool isSocket)
                {
                    byte[] data = Socket.currentReceiveData;
                    Socket.currentReceiveData = Socket.receiveData;
                    if (isSocket)
                    {
                        onReadCompressData(subArray<byte>.Unsafe(data, 0, Socket.currentReceiveEndIndex));
                    }
                    else
                    {
                        try
                        {
                            if (isBigBuffer) BigBuffers.Push(ref data);
                            push(default(memoryPool.pushSubArray));
                        }
                        finally { Socket.close(); }
                    }
                }
                /// <summary>
                /// 读取数据回调操作
                /// </summary>
                private void onReadCompressData(subArray<byte> data)
                {
                    try
                    {
                        subArray<byte> newData = stream.Deflate.GetDeCompressUnsafe(data.Array, data.StartIndex, data.Count, fastCSharp.memoryPool.StreamBuffers);
                        push(new memoryPool.pushSubArray { Value = newData, PushPool = fastCSharp.memoryPool.StreamBuffers.PushHandle });
                        return;
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    finally
                    {
                        if (isBigBuffer) BigBuffers.Push(ref data.array);
                    }
                    try
                    {
                        push(default(memoryPool.pushSubArray));
                    }
                    finally { Socket.close(); }
                }
                /// <summary>
                /// 添加回调对象
                /// </summary>
                /// <param name="data">输出数据</param>
                private void push(memoryPool.pushSubArray data)
                {
                    Action<memoryPool.pushSubArray> callback = Callback;
                    Socket = null;
                    Callback = null;
                    try
                    {
                        typePool<streamReceiver>.Push(this);
                    }
                    finally
                    {
                        if (callback != null)
                        {
                            try
                            {
                                callback(data);
                            }
                            catch (Exception error)
                            {
                                fastCSharp.log.Error.Add(error, null, false);
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// 读取数据
            /// </summary>
            /// <param name="onReceive">接收数据处理委托</param>
            /// <param name="dataFixed">接收数据起始位置</param>
            /// <param name="length">数据长度</param>
            /// <param name="timeout">接收超时</param>
            private unsafe void receiveStream(Action<memoryPool.pushSubArray> onReceive, byte* dataFixed, int length, DateTime timeout)
            {
                streamReceiver streamReceiver = typePool<streamReceiver>.Pop();
                if (streamReceiver == null)
                {
                    try
                    {
                        streamReceiver = new streamReceiver();
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    if (streamReceiver == null)
                    {
                        onReceive(default(memoryPool.pushSubArray));
                        return;
                    }
                }
                streamReceiver.Callback = onReceive;
                streamReceiver.Socket = this;
                streamReceiver.Receive(dataFixed, length, timeout);
            }

            /// <summary>
            /// 输出信息队列集合
            /// </summary>
            private struct outputQueue
            {
                /// <summary>
                /// 第一个节点
                /// </summary>
                public output Head;
                /// <summary>
                /// 最后一个节点
                /// </summary>
                public output End;
                /// <summary>
                /// 清除输出信息
                /// </summary>
                public void Clear()
                {
                    Head = End = null;
                }
                /// <summary>
                /// 添加输出信息
                /// </summary>
                /// <param name="output"></param>
                public void Push(output output)
                {
                    if (Head == null) Head = End = output;
                    else
                    {
                        End.Next = output;
                        End = output;
                    }
                }
                /// <summary>
                /// 获取输出信息
                /// </summary>
                /// <returns></returns>
                public output Pop()
                {
                    if (Head == null) return null;
                    output command = Head;
                    Head = Head.Next;
                    command.Next = null;
                    return command;
                }
            }
            /// <summary>
            /// 输出信息队列集合
            /// </summary>
            private outputQueue outputs;
            /// <summary>
            /// 输出信息集合访问锁
            /// </summary>
            private int outputLock;
            /// <summary>
            /// 是否正在创建输出信息
            /// </summary>
            private int isOutputBuilding;
            /// <summary>
            /// 是否正在创建输出信息
            /// </summary>
            private byte isBuildOutput;
            /// <summary>
            /// 添加输出信息
            /// </summary>
            /// <param name="output">当前输出信息</param>
            /// <returns>是否成功加入输出队列</returns>
            private bool pushOutput(output output)
            {
                if (Socket != null)
                {
                    interlocked.NoCheckCompareSetSleep0(ref outputLock);
                    byte isBuildOutput = this.isBuildOutput;
                    outputs.Push(output);
                    this.isBuildOutput = 1;
                    outputLock = 0;
                    if (isBuildOutput == 0) threadPool.TinyPool.FastStart(buildOutputHandle, null, null);
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 创建输出数据并执行
            /// </summary>
            private unsafe void buildOutput()
            {
                interlocked.NoCheckCompareSetSleep0(ref isOutputBuilding);
                int bufferSize = BigBuffers.Size, bufferSize2 = bufferSize >> 1;
                using (unmanagedStream outputStream = new unmanagedStream((byte*)&bufferSize, sizeof(int)))
                {
                    outputBuilder outputBuilder = new outputBuilder { Socket = this, OutputStream = outputStream };
                    try
                    {
                    START:
                        byte[] buffer = sendData;
                        fixed (byte* dataFixed = buffer)
                        {
                            outputBuilder.Reset(dataFixed, buffer.Length);
                            do
                            {
                                interlocked.NoCheckCompareSetSleep0(ref outputLock);
                                output output = outputs.Pop();
                                if (output == null)
                                {
                                    if (outputStream.Length == 0)
                                    {
                                        isBuildOutput = 0;
                                        outputLock = 0;
                                        isOutputBuilding = 0;
                                        return;
                                    }
                                    outputLock = 0;
                                    outputBuilder.Send();
                                    if (sendData != buffer) goto START;
                                }
                                else
                                {
                                    outputLock = 0;
                                    outputBuilder.Build(output);
                                    if (outputStream.Length + outputBuilder.MaxOutputLength > bufferSize)
                                    {
                                        outputBuilder.Send();
                                        if (sendData != buffer) goto START;
                                    }
                                    if (outputs.Head == null && outputStream.Length <= bufferSize2) Thread.Sleep(0);
                                }
                            }
                            while (true);
                        }
                    }
                    catch (Exception error)
                    {
                        interlocked.NoCheckCompareSetSleep0(ref outputLock);
                        isBuildOutput = 0;
                        outputLock = 0;
                        isOutputBuilding = 0;
                        Socket.shutdown();
                        log.Error.Add(error, commandSocketProxy.attribute.ServiceName, false);
                    }
                }
            }
            /// <summary>
            /// 输出创建
            /// </summary>
            private struct outputBuilder
            {
                /// <summary>
                /// TCP客户端输出流处理套接字
                /// </summary>
                public socket Socket;
                /// <summary>
                /// 输出数据流
                /// </summary>
                public unmanagedStream OutputStream;
                /// <summary>
                /// 输出流数据起始位置
                /// </summary>
                private byte* dataFixed;
                /// <summary>
                /// 输出数据
                /// </summary>
                private subArray<byte> data;
                /// <summary>
                /// 输出流字节长度
                /// </summary>
                private int bufferLength;
                /// <summary>
                /// 最大输出长度
                /// </summary>
                public int MaxOutputLength;
                /// <summary>
                /// 重置输出流
                /// </summary>
                /// <param name="data">输出流数据起始位置</param>
                /// <param name="length">输出流字节长度</param>
                public void Reset(byte* data, int length)
                {
                    OutputStream.Reset(dataFixed = data, bufferLength = length);
                    OutputStream.Unsafer.SetLength(0);
                }
                /// <summary>
                /// 创建输出流
                /// </summary>
                /// <param name="output">输出</param>
                public void Build(output output)
                {
                    int streamLength = OutputStream.Length;
                    output.Build(OutputStream);
                    int outputLength = OutputStream.Length - streamLength;
                    if (outputLength > MaxOutputLength) MaxOutputLength = outputLength;
                }
                /// <summary>
                /// 发送数据
                /// </summary>
                public void Send()
                {
                    if (OutputStream.Length <= bufferLength)
                    {
                        data.UnsafeSet(Socket.sendData, 0, OutputStream.Length);
                        if (OutputStream.DataLength != bufferLength)
                        {
                            unsafer.memory.Copy(OutputStream.Data, dataFixed, OutputStream.Length);
                            OutputStream.Reset(dataFixed, bufferLength);
                        }
                        OutputStream.Unsafer.SetLength(0);
                    }
                    else
                    {
                        byte[] newOutputBuffer = OutputStream.GetSizeArray(bufferLength << 1);
                        fastCSharp.memoryPool.StreamBuffers.Push(ref Socket.sendData);
                        data.UnsafeSet(Socket.sendData = newOutputBuffer, 0, OutputStream.Length);
                    }
                    MaxOutputLength = 0;
                    //if (Socket.isOutputDebug) DebugLog.Add(Socket.commandSocketProxy.attribute.ServiceName + ".Send(" + data.Length.toString() + ")", false, false);
                    Socket.serverSend(data);
                }
            }

            /// <summary>
            /// 输出信息
            /// </summary>
            private abstract class output
            {
                /// <summary>
                /// 下一个输出信息
                /// </summary>
                public output Next;
                /// <summary>
                /// 会话标识
                /// </summary>
                public commandServer.streamIdentity Identity;
                /// <summary>
                /// 创建输出信息
                /// </summary>
                /// <param name="stream">命令内存流</param>
                public abstract void Build(unmanagedStream stream);
            }
            /// <summary>
            /// 输出信息
            /// </summary>
            private sealed class outputParameter : output
            {
                /// <summary>
                /// 输出参数
                /// </summary>
                public fastCSharp.code.cSharp.asynchronousMethod.returnValue OutputParameter;
                /// <summary>
                /// 创建输出信息
                /// </summary>
                /// <param name="stream">命令内存流</param>
                public override void Build(unmanagedStream stream)
                {
                    stream.PrepLength(sizeof(commandServer.streamIdentity) + sizeof(int));
                    byte* dataFixed = stream.CurrentData;
                    *(commandServer.streamIdentity*)dataFixed = Identity;
                    *(int*)(dataFixed + sizeof(commandServer.streamIdentity)) = OutputParameter.IsReturn ? 0 : ErrorStreamReturnLength;
                    stream.Unsafer.AddLength(sizeof(commandServer.streamIdentity) + sizeof(int));
                    Next = null;
                    typePool<outputParameter>.Push(this);
                }
                /// <summary>
                /// 获取输出信息
                /// </summary>
                /// <param name="identity">会话标识</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>输出信息</returns>
                public static outputParameter Get(commandServer.streamIdentity identity,
                    fastCSharp.code.cSharp.asynchronousMethod.returnValue outputParameter)
                {
                    outputParameter output = typePool<outputParameter>.Pop();
                    if (output == null)
                    {
                        try
                        {
                            output = new outputParameter();
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    output.Identity = identity;
                    output.OutputParameter = outputParameter;
                    return output;
                }
            }
            /// <summary>
            /// 发送数据
            /// </summary>
            /// <param name="identity">会话标识</param>
            /// <param name="value">返回值</param>
            /// <returns>是否成功加入输出队列</returns>
            public bool SendStream(commandServer.streamIdentity identity, fastCSharp.code.cSharp.asynchronousMethod.returnValue value)
            {
                outputParameter output = outputParameter.Get(identity, value);
                if (output != null) return pushOutput(output);
                close();
                return false;
            }
            /// <summary>
            /// 输出信息
            /// </summary>
            /// <typeparam name="outputParameterType">输出数据类型</typeparam>
            private sealed class outputParameter<outputParameterType> : output
            {
                /// <summary>
                /// 输出参数
                /// </summary>
                public outputParameterType OutputParameter;
                /// <summary>
                /// 创建输出信息
                /// </summary>
                /// <param name="stream">命令内存流</param>
                public override void Build(unmanagedStream stream)
                {
                    int streamLength = stream.Length;
                    stream.PrepLength(sizeof(commandServer.streamIdentity) + sizeof(int));
                    stream.Unsafer.AddLength(sizeof(commandServer.streamIdentity) + sizeof(int));
                    fastCSharp.emit.dataSerializer.Serialize(OutputParameter, stream);
                    int dataLength = stream.Length - streamLength - (sizeof(commandServer.streamIdentity) + sizeof(int));
                    byte* dataFixed = stream.Data + streamLength;
                    *(commandServer.streamIdentity*)dataFixed = Identity;
                    *(int*)(dataFixed + sizeof(commandServer.streamIdentity)) = dataLength;
                    OutputParameter = default(outputParameterType);
                    Next = null;
                    typePool<outputParameter<outputParameterType>>.Push(this);
                }
                /// <summary>
                /// 获取输出信息
                /// </summary>
                /// <param name="identity">会话标识</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>输出信息</returns>
                public static outputParameter<outputParameterType> Get
                    (commandServer.streamIdentity identity, outputParameterType outputParameter)
                {
                    outputParameter<outputParameterType> output = typePool<outputParameter<outputParameterType>>.Pop();
                    if (output == null)
                    {
                        try
                        {
                            output = new outputParameter<outputParameterType>();
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    output.Identity = identity;
                    output.OutputParameter = outputParameter;
                    return output;
                }
            }
            /// <summary>
            /// 发送数据
            /// </summary>
            /// <typeparam name="outputParameterType">输出数据类型</typeparam>
            /// <param name="identity">会话标识</param>
            /// <param name="outputParameter">返回值</param>
            /// <returns>是否成功加入输出队列</returns>
            public bool SendStream<outputParameterType>(commandServer.streamIdentity identity, asynchronousMethod.returnValue<outputParameterType> outputParameter)
            {
                if (outputParameter.IsReturn)
                {
                    outputParameter<outputParameterType> output = outputParameter<outputParameterType>.Get(identity, outputParameter.Value);
                    if (output != null) return pushOutput(output);
                    close();
                    return false;
                }
                return SendStream(identity, new asynchronousMethod.returnValue { IsReturn = false });
            }
            /// <summary>
            /// 发送数据
            /// </summary>
            /// <typeparam name="outputParameterType">输出数据类型</typeparam>
            /// <param name="identity">会话标识</param>
            /// <param name="outputParameter">返回值</param>
            /// <returns>是否成功加入输出队列</returns>
            public bool SendStreamJson<outputParameterType>(commandServer.streamIdentity identity, asynchronousMethod.returnValue<outputParameterType> outputParameter)
            {
                 return SendStream(identity, tcpBase.JsonToSerialize(outputParameter));
            }

            ///// <summary>
            ///// 设置流接收超时
            ///// </summary>
            //private void setStreamReceiveTimeout()
            //{
            //    if (IsVerifyMethod && isStreamReceiveTimeout == 0)
            //    {
            //        isStreamReceiveTimeout = 1;
            //        Socket.ReceiveTimeout = commandSocketProxy.receiveCommandTimeout == 0 ? -1 : commandSocketProxy.receiveCommandTimeout;
            //    }
            //}
            /// <summary>
            /// TCP参数流
            /// </summary>
            private class tcpStream : Stream, ITcpStreamCallback
            {
                /// <summary>
                /// 默认空TCP流异步操作状态
                /// </summary>
                private static readonly tcpStreamAsyncResult nullTcpStreamAsyncResult = new tcpStreamAsyncResult { Parameter = new tcpStreamParameter { Data = subArray<byte>.Unsafe(nullValue<byte>.Array, 0, 0) }, IsCompleted = true };
                /// <summary>
                /// TCP调用套接字
                /// </summary>
                private socket socket;
                /// <summary>
                /// TCP参数流
                /// </summary>
                private tcpBase.tcpStream stream;
                /// <summary>
                /// 套接字重用标识
                /// </summary>
                private int pushIdentity;
                /// <summary>
                /// 是否已经释放资源
                /// </summary>
                private int isDisposed;
                /// <summary>
                /// TCP参数流
                /// </summary>
                /// <param name="socket">TCP调用套接字</param>
                /// <param name="stream">TCP参数流</param>
                public tcpStream(socket socket, tcpBase.tcpStream stream)
                {
                    this.socket = socket;
                    this.stream = stream;
                    pushIdentity = socket.PushIdentity;
                }
                /// <summary>
                /// 发送命令获取客户端回馈
                /// </summary>
                /// <param name="parameter">TCP流参数</param>
                /// <returns>客户端回馈</returns>
                private tcpStreamParameter get(tcpStreamParameter parameter)
                {
                    if (pushIdentity == socket.PushIdentity)
                    {
                        parameter.Index = socket.getTcpStreamIndex(null);
                        parameter.Identity = socket.tcpStreamReceivers[parameter.Index].Identity;
                        parameter.ClientIndex = stream.ClientIndex;
                        parameter.ClientIdentity = stream.ClientIdentity;
                        try
                        {
                            socket.SendStream(new commandServer.streamIdentity(), new asynchronousMethod.returnValue<tcpStreamParameter> { IsReturn = true, Value = parameter });
                            tcpStreamParameter outputParameter = socket.waitTcpStream(parameter.Index, parameter.Identity);
                            if (outputParameter.IsClientStream) return outputParameter;
                            error();
                        }
                        finally
                        {
                            socket.cancelTcpStreamIndex(parameter.Index, parameter.Identity, false);
                        }
                    }
                    return tcpStreamParameter.Null;
                }
                /// <summary>
                /// 发送异步命令
                /// </summary>
                /// <param name="tcpStreamAsyncResult">TCP流异步操作状态</param>
                private void send(tcpStreamAsyncResult tcpStreamAsyncResult)
                {
                    if (pushIdentity == socket.PushIdentity)
                    {
                        tcpStreamParameter parameter = tcpStreamAsyncResult.Parameter;
                        parameter.Index = socket.getTcpStreamIndex(tcpStreamAsyncResult);
                        parameter.Identity = socket.tcpStreamReceivers[parameter.Index].Identity;
                        parameter.ClientIndex = stream.ClientIndex;
                        parameter.ClientIdentity = stream.ClientIdentity;
                        try
                        {
                            socket.SendStream(new commandServer.streamIdentity(), new asynchronousMethod.returnValue<tcpStreamParameter> { IsReturn = true, Value = parameter });
                            socket.setTcpStreamTimeout(parameter.Index, parameter.Identity);
                            return;
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                        }
                        socket.cancelTcpStreamIndex(parameter.Index, parameter.Identity, false);
                    }
                    throw ioException;
                }
                /// <summary>
                /// TCP流异步回调
                /// </summary>
                /// <param name="tcpStreamAsyncResult">TCP流异步操作状态</param>
                /// <param name="parameter">TCP流参数</param>
                public void Callback(tcpStreamAsyncResult tcpStreamAsyncResult, tcpStreamParameter parameter)
                {
                    if (parameter != null && parameter.IsClientStream)
                    {
                        if (parameter.IsCommand)
                        {
                            switch (tcpStreamAsyncResult.Parameter.Command)
                            {
                                case tcpStreamCommand.BeginRead:
                                    subArray<byte> data = parameter.Data;
                                    if (data.Count != 0)
                                    {
                                        subArray<byte> buffer = tcpStreamAsyncResult.Parameter.Data;
                                        Buffer.BlockCopy(data.Array, data.StartIndex, buffer.Array, buffer.StartIndex, data.Count);
                                    }
                                    tcpStreamAsyncResult.Parameter.Offset = data.Count;
                                    break;
                                case tcpStreamCommand.BeginWrite:
                                    tcpStreamAsyncResult.IsCompleted = true;
                                    break;
                            }
                        }
                    }
                    else Close();
                }
                /// <summary>
                /// 否支持读取
                /// </summary>
                public override bool CanRead
                {
                    get { return stream.CanRead; }
                }
                /// <summary>
                /// 否支持查找
                /// </summary>
                public override bool CanSeek
                {
                    get { return stream.CanSeek; }
                }
                /// <summary>
                /// 是否可以超时
                /// </summary>
                public override bool CanTimeout
                {
                    get { return stream.CanTimeout; }
                }
                /// <summary>
                /// 否支持写入
                /// </summary>
                public override bool CanWrite
                {
                    get { return stream.CanWrite; }
                }
                /// <summary>
                /// 流字节长度
                /// </summary>
                public override long Length
                {
                    get
                    {
                        if (isDisposed != 0) throw objectDisposedException;
                        tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.GetLength });
                        if (parameter.IsCommand) return parameter.Offset;
                        throw notSupportedException;
                    }
                }
                /// <summary>
                /// 当前位置
                /// </summary>
                public override long Position
                {
                    get
                    {
                        if (isDisposed != 0) throw objectDisposedException;
                        tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.GetPosition });
                        if (parameter.IsCommand) return parameter.Offset;
                        throw notSupportedException;
                    }
                    set
                    {
                        if (isDisposed != 0) throw objectDisposedException;
                        tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.SetPosition, Offset = value });
                        if (parameter.IsCommand) return;
                        throw notSupportedException;
                    }
                }
                /// <summary>
                /// 读超时毫秒
                /// </summary>
                public override int ReadTimeout
                {
                    get
                    {
                        if (isDisposed == 0)
                        {
                            tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.GetReadTimeout });
                            if (parameter.IsCommand) return (int)parameter.Offset;
                        }
                        throw invalidOperationException;
                    }
                    set
                    {
                        if (isDisposed == 0)
                        {
                            tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.SetReadTimeout, Offset = value });
                            if (parameter.IsCommand) return;
                        }
                        throw invalidOperationException;
                    }
                }
                /// <summary>
                /// 写超时毫秒
                /// </summary>
                public override int WriteTimeout
                {
                    get
                    {
                        if (isDisposed == 0)
                        {
                            tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.GetWriteTimeout });
                            if (parameter.IsCommand) return (int)parameter.Offset;
                        }
                        throw invalidOperationException;
                    }
                    set
                    {
                        if (isDisposed == 0)
                        {
                            tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.SetWriteTimeout, Offset = value });
                            if (parameter.IsCommand) return;
                        }
                        throw invalidOperationException;
                    }
                }
                /// <summary>
                /// 异步读取
                /// </summary>
                /// <param name="buffer">缓冲区</param>
                /// <param name="offset">起始位置</param>
                /// <param name="count">接收字节数</param>
                /// <param name="callback">异步回调</param>
                /// <param name="state">用户对象</param>
                /// <returns>异步读取结果</returns>
                public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!stream.CanRead) throw notSupportedException;
                    if (buffer == null || offset < 0 || count < 0 || offset + count > buffer.Length) throw argumentException;
                    if (count != 0)
                    {
                        tcpStreamAsyncResult result = new tcpStreamAsyncResult { TcpStreamCallback = this, Parameter = new tcpStreamParameter { Command = tcpStreamCommand.BeginRead, Data = subArray<byte>.Unsafe(buffer, offset, count) }, Callback = callback, AsyncState = state };
                        send(result);
                        return result;
                    }
                    callback(nullTcpStreamAsyncResult);
                    return nullTcpStreamAsyncResult;
                }
                /// <summary>
                /// 等待挂起的异步读取完成
                /// </summary>
                /// <param name="asyncResult">异步读取结果</param>
                /// <returns>读取的字节数</returns>
                public override int EndRead(IAsyncResult asyncResult)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    tcpStreamAsyncResult result = asyncResult as tcpStreamAsyncResult;
                    if (result == null) throw argumentNullException;
                    if (!result.IsCallback) result.AsyncWaitHandle.WaitOne();
                    if (result.IsCompleted) return (int)result.Parameter.Offset;
                    throw ioException;
                }
                /// <summary>
                /// 读取字节序列
                /// </summary>
                /// <param name="buffer">缓冲区</param>
                /// <param name="offset">起始位置</param>
                /// <param name="count">读取字节数</param>
                /// <returns>读取字节数</returns>
                public override unsafe int Read(byte[] buffer, int offset, int count)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!stream.CanRead) throw notSupportedException;
                    if (buffer == null) throw argumentNullException;
                    if (offset < 0 || count <= 0) throw argumentOutOfRangeException;
                    if (offset + count > buffer.Length) throw argumentException;
                    tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.Read, Offset = count });
                    if (parameter.IsCommand)
                    {
                        subArray<byte> data = parameter.Data;
                        if (data.Count != 0) Buffer.BlockCopy(data.Array, data.StartIndex, buffer, offset, data.Count);
                        return data.Count;
                    }
                    throw ioException;
                }
                /// <summary>
                /// 读取字节
                /// </summary>
                /// <returns>字节</returns>
                public override int ReadByte()
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!stream.CanRead) throw notSupportedException;
                    tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.ReadByte });
                    if (parameter.IsCommand) return (int)parameter.Offset;
                    throw ioException;
                }
                /// <summary>
                /// 异步写入
                /// </summary>
                /// <param name="buffer">缓冲区</param>
                /// <param name="offset">起始位置</param>
                /// <param name="count">接收字节数</param>
                /// <param name="callback">异步回调</param>
                /// <param name="state">用户对象</param>
                /// <returns>异步写入结果</returns>
                public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!stream.CanWrite) throw notSupportedException;
                    if (buffer == null || offset < 0 || count < 0 || offset + count > buffer.Length) throw argumentException;
                    if (count != 0)
                    {
                        tcpStreamAsyncResult result = new tcpStreamAsyncResult { TcpStreamCallback = this, Parameter = new tcpStreamParameter { Command = tcpStreamCommand.BeginWrite, Data = subArray<byte>.Unsafe(buffer, offset, count) }, Callback = callback, AsyncState = state };
                        send(result);
                        return result;
                    }
                    callback(nullTcpStreamAsyncResult);
                    return nullTcpStreamAsyncResult;
                }
                /// <summary>
                /// 结束异步写操作
                /// </summary>
                /// <param name="asyncResult">异步写入结果</param>
                /// <returns>写入的字节数</returns>
                public override void EndWrite(IAsyncResult asyncResult)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    tcpStreamAsyncResult result = asyncResult as tcpStreamAsyncResult;
                    if (result == null) throw argumentNullException;
                    if (!result.IsCallback) result.AsyncWaitHandle.WaitOne();
                    if (result.IsCompleted) return;
                    throw ioException;
                }
                /// <summary>
                /// 写入字节序列
                /// </summary>
                /// <param name="buffer">缓冲区</param>
                /// <param name="offset">起始位置</param>
                /// <param name="count">读取写入数</param>
                public override void Write(byte[] buffer, int offset, int count)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!stream.CanWrite) throw notSupportedException;
                    if (buffer == null) throw argumentNullException;
                    if (offset < 0 || count < 0) throw argumentOutOfRangeException;
                    if (offset + count > buffer.Length) throw argumentException;
                    if (count != 0)
                    {
                        tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.Write, Data = subArray<byte>.Unsafe(buffer, offset, count) });
                        if (parameter.IsCommand) return;
                        throw ioException;
                    }
                }
                /// <summary>
                /// 写入字节
                /// </summary>
                /// <param name="value">字节</param>
                public override void WriteByte(byte value)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!stream.CanWrite) throw notSupportedException;
                    tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.WriteByte, Offset = value });
                    if (parameter.IsCommand) return;
                    throw ioException;
                }
                /// <summary>
                /// 设置流位置
                /// </summary>
                /// <param name="offset">位置</param>
                /// <param name="origin">类型</param>
                /// <returns>流中的新位置</returns>
                public override long Seek(long offset, SeekOrigin origin)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.Seek, Offset = offset, SeekOrigin = origin });
                    if (parameter.IsCommand) return parameter.Offset;
                    throw notSupportedException;
                }
                /// <summary>
                /// 设置流长度
                /// </summary>
                /// <param name="value">字节长度</param>
                public override void SetLength(long value)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.SetLength, Offset = value });
                    if (parameter.IsCommand) return;
                    throw notSupportedException;
                }
                /// <summary>
                /// 清除缓冲区
                /// </summary>
                public override void Flush()
                {
                    if (isDisposed == 0)
                    {
                        tcpStreamParameter parameter = get(new tcpStreamParameter { Command = tcpStreamCommand.Flush });
                        if (parameter.IsCommand) return;
                        error();
                    }
                }
                /// <summary>
                /// 错误
                /// </summary>
                private void error()
                {
                    Close();
                    throw ioException;
                }
                /// <summary>
                /// 关闭流
                /// </summary>
                public override void Close()
                {
                    if (Interlocked.Increment(ref isDisposed) == 1)
                    {
                        get(new tcpStreamParameter { Command = tcpStreamCommand.Close });
                        base.Dispose();
                    }
                }
                /// <summary>
                /// 是否资源
                /// </summary>
                public new void Dispose()
                {
                    Close();
                }
            }
            /// <summary>
            /// TCP流读取器集合
            /// </summary>
            private tcpStreamReceiver[] tcpStreamReceivers;
            /// <summary>
            /// TCP流读取器索引
            /// </summary>
            private int tcpStreamReceiveIndex;
            /// <summary>
            /// TCP流读取器空闲索引集合
            /// </summary>
            private subArray<int> freeTcpStreamIndexs;
            /// <summary>
            /// TCP流读取器访问锁
            /// </summary>
            private int tcpStreamReceiveLock;
            /// <summary>
            /// 取消TCP流读取
            /// </summary>
            private Action<long> cancelTcpStreamHandle;
            /// <summary>
            /// 获取TCP流读取器索引
            /// </summary>
            /// <param name="tcpStreamAsyncResult">TCP流异步操作状态</param>
            /// <returns>TCP流读取器索引</returns>
            private int getTcpStreamIndex(tcpStreamAsyncResult tcpStreamAsyncResult)
            {
                int index = -1;
                interlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                try
                {
                    if (freeTcpStreamIndexs.Count == 0)
                    {
                        if (tcpStreamReceivers == null)
                        {
                            tcpStreamReceivers = new tcpStreamReceiver[4];
                            index = 0;
                            tcpStreamReceiveIndex = 1;
                        }
                        else
                        {
                            if (tcpStreamReceiveIndex == tcpStreamReceivers.Length)
                            {
                                tcpStreamReceiver[] newTcpStreamReceivers = new tcpStreamReceiver[tcpStreamReceiveIndex << 1];
                                tcpStreamReceivers.CopyTo(newTcpStreamReceivers, 0);
                                tcpStreamReceivers = newTcpStreamReceivers;
                            }
                            index = tcpStreamReceiveIndex++;
                        }
                    }
                    else index = freeTcpStreamIndexs.UnsafePop();
                    tcpStreamReceivers[index].SetAsyncResult(tcpStreamAsyncResult);
                }
                finally { tcpStreamReceiveLock = 0; }
                return index;
            }
            /// <summary>
            /// 取消TCP流读取
            /// </summary>
            /// <param name="indexIdentity">TCP流读取器索引+当前处理序号</param>
            private void cancelTcpStream(long indexIdentity)
            {
                cancelTcpStreamIndex((int)(indexIdentity >> 32), (int)indexIdentity, true);
            }
            /// <summary>
            /// 取消TCP流读取
            /// </summary>
            /// <param name="index">TCP流读取器索引</param>
            /// <param name="identity">当前处理序号</param>
            /// <param name="isSetWait">是否设置结束状态</param>
            private void cancelTcpStreamIndex(int index, int identity, bool isSetWait)
            {
                if (tcpStreamReceivers[index].Identity == identity)
                {
                    interlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                    try
                    {
                        if (tcpStreamReceivers[index].Cancel(identity, isSetWait)) freeTcpStreamIndexs.Add(index);
                    }
                    finally { tcpStreamReceiveLock = 0; }
                }
            }
            /// <summary>
            /// 等待TCP流读取
            /// </summary>
            /// <param name="index">TCP流读取器索引</param>
            /// <param name="identity">当前处理序号</param>
            /// <returns>读取的数据</returns>
            private tcpStreamParameter waitTcpStream(int index, int identity)
            {
                setTcpStreamTimeout(index, identity);
                EventWaitHandle receiveWait = tcpStreamReceivers[index].ReceiveWait;
                if (receiveWait.WaitOne())
                {
                    tcpStreamParameter parameter = tcpStreamParameter.Null;
                    interlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                    try
                    {
                        if (tcpStreamReceivers[index].Get(identity, ref parameter)) freeTcpStreamIndexs.Add(index);
                    }
                    finally { tcpStreamReceiveLock = 0; }
                    return parameter;
                }
                cancelTcpStreamIndex(index, identity, false);
                return tcpStreamParameter.Null;
            }
            /// <summary>
            /// 设置TCP流读取超时
            /// </summary>
            /// <param name="index">TCP流读取器索引</param>
            /// <param name="identity">当前处理序号</param>
            private void setTcpStreamTimeout(int index, int identity)
            {
                if (cancelTcpStreamHandle == null) cancelTcpStreamHandle = cancelTcpStream;
                threading.timerTask.Default.Add(cancelTcpStreamHandle, ((long)index << 32) + identity, date.NowSecond.AddSeconds(fastCSharp.config.tcpCommand.Default.TcpStreamTimeout), null);
            }
            /// <summary>
            /// TCP流回馈
            /// </summary>
            /// <param name="parameter">TCP流参数</param>
            internal void OnTcpStream(tcpStreamParameter parameter)
            {
                tcpStreamAsyncResult asyncResult = null;
                int index = parameter.Index;
                interlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                try
                {
                    if (tcpStreamReceivers[index].Set(parameter, ref asyncResult) && asyncResult != null) freeTcpStreamIndexs.Add(index);
                }
                finally
                {
                    tcpStreamReceiveLock = 0;
                    if (asyncResult != null) asyncResult.OnCallback(parameter);
                }
            }
            /// <summary>
            /// 获取TCP参数流
            /// </summary>
            /// <param name="stream">TCP参数流</param>
            /// <returns>字节流</returns>
            public Stream GetTcpStream(tcpBase.tcpStream stream)
            {
                return stream.IsStream ? new tcpStream(this, stream) : null;
            }
        }
        /// <summary>
        /// 最大命令长度
        /// </summary>
        protected int maxCommandLength;
        /// <summary>
        /// 验证命令
        /// </summary>
        protected byte[] verifyCommand = nullValue<byte>.Array;
        /// <summary>
        /// 验证命令序号
        /// </summary>
        protected int verifyCommandIdentity = nullVerifyCommandIdentity;
        /// <summary>
        /// 接收数据超时
        /// </summary>
        private int receiveTimeout;
        /// <summary>
        /// 发送数据超时
        /// </summary>
        private int sendTimeout;
        /// <summary>
        /// 每秒最低接收字节数
        /// </summary>
        private int minReceivePerSecond;
        /// <summary>
        /// 接收命令超时
        /// </summary>
        private int receiveCommandTimeout;
        /// <summary>
        /// 接收命令超时时钟周期
        /// </summary>
        private long receiveCommandTicks;
        /// <summary>
        /// 是否存在客户端标识
        /// </summary>
        protected bool isClientUserInfo;
        /// <summary>
        /// HTTP命令处理委托集合
        /// </summary>
        protected fastCSharp.stateSearcher.ascii<httpCommand> httpCommands;
        /// <summary>
        /// HTTP服务器
        /// </summary>
        internal httpServers HttpServers;
        /// <summary>
        /// 客户端套接字队列
        /// </summary>
        private clientQueue<Socket> clientQueue;
        /// <summary>
        /// TCP客户端验证接口
        /// </summary>
        private fastCSharp.code.cSharp.tcpBase.ITcpVerify verify;
        /// <summary>
        /// 命令处理委托集合
        /// </summary>
        protected fastCSharp.stateSearcher.ascii<command> onCommands;
        /// <summary>
        /// 序号识别命令处理委托集合
        /// </summary>
        protected command[] identityOnCommands;
        /// <summary>
        /// 套接字池第一个节点
        /// </summary>
        private socket socketPoolHead;
        /// <summary>
        /// 套接字池最后一个节点
        /// </summary>
        private socket socketPoolEnd;
        /// <summary>
        /// 套接字池访问锁
        /// </summary>
        private int socketPoolLock;
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        public commandServer(fastCSharp.code.cSharp.tcpServer attribute) : this(attribute, null) { }
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="verify">TCP客户端验证接口</param>
        public unsafe commandServer(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpVerify verify)
            : base(attribute)
        {
            if (attribute.SendBufferSize <= (sizeof(int) * 2 + sizeof(commandServer.streamIdentity))) attribute.SendBufferSize = Math.Max(sizeof(int) * 2 + sizeof(commandServer.streamIdentity), fastCSharp.config.appSetting.StreamBufferSize);
            if (attribute.ReceiveBufferSize <= maxCommandLength + (sizeof(int) * 3 + sizeof(commandServer.streamIdentity))) attribute.ReceiveBufferSize = Math.Max(maxCommandLength + (sizeof(int) * 3 + sizeof(commandServer.streamIdentity)), fastCSharp.config.appSetting.StreamBufferSize);
            if (attribute.ReceiveTimeout > 0)
            {
                receiveTimeout = attribute.ReceiveTimeout * 1000;
                if (receiveTimeout <= 0) receiveTimeout = int.MaxValue;
                sendTimeout = receiveTimeout;
            }
            else sendTimeout = config.tcpCommand.Default.DefaultTimeout * 1000;
            if (attribute.MinReceivePerSecond > 0)
            {
                minReceivePerSecond = attribute.MinReceivePerSecond << 10;
                if (minReceivePerSecond <= 0) minReceivePerSecond = int.MaxValue;
            }
            receiveCommandTimeout = (attribute.RecieveCommandMinutes > 0 ? attribute.RecieveCommandMinutes * 60 : config.tcpCommand.Default.DefaultTimeout) * 1000;
            if (receiveCommandTimeout <= 0) receiveCommandTimeout = int.MaxValue;
            receiveCommandTicks = date.MillisecondTicks * receiveCommandTimeout;

            loadBalancingCheckTicks = new TimeSpan(0, 0, Math.Max(attribute.LoadBalancingCheckSeconds - 2, 1)).Ticks;

            this.verify = verify;
            clientQueue = clientQueue<Socket>.Create(attribute.MaxActiveClientCount, attribute.MaxClientCount, closeSocket);
        }
        /// <summary>
        /// 负载均衡服务TCP服务调用配置
        /// </summary>
        private fastCSharp.code.cSharp.tcpServer loadBalancingAttribute;
        /// <summary>
        /// 负载均衡服务TCP验证方
        /// </summary>
        private tcpBase.ITcpClientVerifyMethod<commandLoadBalancingServer.commandClient> loadBalancingVerifyMethod;
        /// <summary>
        /// 负载均衡服务TCP验证方
        /// </summary>
        private Action<Exception> loadBalancingOnException;
        /// <summary>
        /// TCP调用服务添加负载均衡服务
        /// </summary>
        private Action loadBalancingHandle;
        /// <summary>
        /// 启动服务并添加到负载均衡服务
        /// </summary>
        /// <param name="attribute">负载均衡服务TCP服务调用配置</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        /// <param name="onException">异常处理</param>
        /// <returns>是否成功</returns>
        public bool StartLoadBalancing(fastCSharp.code.cSharp.tcpServer attribute
            , tcpBase.ITcpClientVerifyMethod<commandLoadBalancingServer.commandClient> verifyMethod = null
            , Action<Exception> onException = null)
        {
            if (Start())
            {
                if (attribute != null)
                {
                    attribute.IsLoadBalancing = false;
                    loadBalancingAttribute = attribute;
                    loadBalancingVerifyMethod = verifyMethod ?? new fastCSharp.net.tcp.verifyMethod();
                    loadBalancingOnException = onException;
                    loadBalancing();
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// TCP调用服务添加负载均衡服务
        /// </summary>
        private void loadBalancing()
        {
            if (isStart != 0)
            {
                try
                {
                    if (new commandLoadBalancingServer.commandClient(loadBalancingAttribute, loadBalancingVerifyMethod)
                        .NewServer(new fastCSharp.net.tcp.host { Host = attribute.Host, Port = attribute.Port }))
                    {
                        return;
                    }
                }
                catch (Exception error)
                {
                    if (loadBalancingOnException == null) log.Error.Add(error, null, false);
                    else loadBalancingOnException(error);
                }
                if (loadBalancingHandle == null) loadBalancingHandle = loadBalancing;
                fastCSharp.threading.timerTask.Default.Add(loadBalancingHandle, date.NowSecond.AddSeconds(1));
            }
        }
        /// <summary>
        /// 初始化序号识别命令处理委托集合
        /// </summary>
        /// <param name="count">命令数量</param>
        protected void setCommands(int count)
        {
            identityOnCommands = new command[count + CommandStartIndex];
            identityOnCommands[CloseIdentityCommand].Set(close, 0);
            identityOnCommands[CheckIdentityCommand].Set(check, 0);
            identityOnCommands[LoadBalancingCheckIdentityCommand].Set(loadBalancingCheck, 0);
            identityOnCommands[StreamMergeIdentityCommand].Set(mergeStreamIdentity);
            identityOnCommands[TcpStreamCommand].Set(tcpStream);
            identityOnCommands[IgnoreGroupCommand].Set(ignoreGroup);
        }
        /// <summary>
        /// 初始化命令处理委托集合
        /// </summary>
        /// <param name="count">命令数量</param>
        /// <param name="index">命令索引位置</param>
        /// <returns>命令处理委托集合</returns>
        protected keyValue<byte[][], command[]> getCommands(int count, out int index)
        {
            index = 6;
            byte[][] datas = new byte[count + index][];
            command[] commands = new command[count + index];
            datas[0] = closeCommandData;
            commands[0] = new command(close, 0);
            datas[1] = checkCommandData;
            commands[1] = new command(check, 0);
            datas[2] = loadBalancingCheckCommandData;
            commands[2] = new command(loadBalancingCheck, 0);
            datas[3] = streamMergeCommandData;
            commands[3] = new command(mergeStream);
            datas[4] = tcpStreamCommandData;
            commands[4] = new command(tcpStream);
            datas[5] = ignoreGroupCommandData;
            commands[5] = new command(ignoreGroup);
            return new keyValue<byte[][], command[]>(datas, commands);
        }
        /// <summary>
        /// 客户端请求处理
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        protected override void newSocket(Socket socket)
        {
            clientQueue<Socket> clientQueue = this.clientQueue;
            if (clientQueue != null)
            {
                clientQueue<Socket>.clientInfo client = clientQueue.NewClient(socket, socket);
                if ((int)client.Type >= (int)fastCSharp.net.clientQueue.socketType.Ipv4) newSocket(client);
            }
            else socket.shutdown();
            //else socket.Close();
        }
        /// <summary>
        /// 客户端请求处理
        /// </summary>
        /// <param name="client">客户端信息</param>
        private void newSocket(clientQueue<Socket>.clientInfo client)
        {
            socket commandSocket = null;
            try
            {
                interlocked.NoCheckCompareSetSleep0(ref socketPoolLock);
                if (socketPoolHead != null)
                {
                    commandSocket = socketPoolHead;
                    socketPoolHead = socketPoolHead.PoolNext;
                    commandSocket.PoolNext = null;
                }
                socketPoolLock = 0;
                if (commandSocket == null)
                {
                    memoryPool pool = fastCSharp.memoryPool.StreamBuffers;
                    byte[] sendData = pool.Size == attribute.SendBufferSize ? pool.Get() : new byte[attribute.SendBufferSize];
                    byte[] receiveData = pool.Size == attribute.ReceiveBufferSize ? pool.Get() : new byte[attribute.ReceiveBufferSize];
                    commandSocket = new socket(client, this, sendData, receiveData);
                }
                else commandSocket.SetSocket(client);
                client.Client = null;
                commandSocket.VerifySocketType();
                commandSocket = null;
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            finally
            {
                //if (client.Client != null) client.Client.Close();
                client.Client.shutdown();
                if (commandSocket != null) pushSocket(commandSocket);
            }
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        public override void Dispose()
        {
            fastCSharp.code.cSharp.tcpServer loadBalancingAttribute = this.loadBalancingAttribute;
            this.loadBalancingAttribute = null;
            if (loadBalancingAttribute != null)
            {
                try
                {
                    new commandLoadBalancingServer.commandClient(loadBalancingAttribute, loadBalancingVerifyMethod)
                        .RemoveServer(new fastCSharp.net.tcp.host { Host = attribute.Host, Port = attribute.Port });
                }
                catch (Exception error)
                {
                    if (loadBalancingOnException == null) log.Error.Add(error, null, false);
                    else loadBalancingOnException(error);
                }
            }
            base.Dispose();
            pub.Dispose(ref clientQueue);
            interlocked.NoCheckCompareSetSleep0(ref socketPoolLock);
            try
            {
                while (socketPoolHead != null)
                {
                    socketPoolEnd = socketPoolHead.PoolNext;
                    socketPoolHead.Dispose();
                    socketPoolHead = socketPoolEnd;
                }
            }
            finally
            {
                socketPoolHead = socketPoolEnd = null;
                socketPoolLock = 0;
            }
            pub.Dispose(ref onCommands);
            pub.Dispose(ref httpCommands);
        }
        /// <summary>
        /// 保存套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        protected void pushSocket(socket socket)
        {
            clientQueue<Socket>.clientInfo client = push(socket);
            if (client.Client != null) newSocket(client);
        }
        /// <summary>
        /// 保存套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <returns>下一个客户端</returns>
        private clientQueue<Socket>.clientInfo push(socket socket)
        {
            clientQueue<Socket>.clientInfo client = new clientQueue<Socket>.clientInfo { Ipv4 = socket.Ipv4, Ipv6 = socket.Ipv6 };
            socket.Close();
            interlocked.NoCheckCompareSetSleep0(ref socketPoolLock);
            //if (socketPool.IndexOf(socket) == -1)
            //{
            ++socket.PushIdentity;
            if (socketPoolHead == null) socketPoolHead = socketPoolEnd = socket;
            else
            {
                socketPoolEnd.PoolNext = socket;
                socketPoolEnd = socket;
            }
            socketPoolLock = 0;
            //}
            //else
            //{
            //    socketPoolLock = 0;
            //    fastCSharp.log.Error.Add("套接字客户端释放冲突 ", true, false);
            //}
            clientQueue<Socket> clientQueue = this.clientQueue;
            if (clientQueue != null)
            {
                client.Client = client.Ipv6.IsNull ? clientQueue.End(client.Ipv4) : clientQueue.End(client.Ipv6);
                if (client.Client != null)
                {
                    client.Type = client.Ipv6.IsNull ? fastCSharp.net.clientQueue.socketType.Ipv4 : fastCSharp.net.clientQueue.socketType.Ipv6;
                }
            }
            return client;
        }
        /// <summary>
        /// 获取客户端请求
        /// </summary>
        protected override void getSocket()
        {
            if (verify == null && (identityOnCommands == null ? verifyCommand.Length == 0 : verifyCommandIdentity == int.MinValue)) log.Error.Add("缺少TCP客户端验证接口或者方法", true, false);
            int bufferLength = maxCommandLength + sizeof(int);
            if (attribute.ReceiveBufferSize < bufferLength) attribute.ReceiveBufferSize = bufferLength;
            if (attribute.IsHttpClient) HttpServers = new httpServers(this);
            acceptSocket();
        }
        /// <summary>
        /// 获取接收数据超时时间
        /// </summary>
        /// <param name="length">接收数据字节长度</param>
        /// <returns>接收数据超时时间</returns>
        private DateTime getReceiveTimeout(int length)
        {
            return minReceivePerSecond == 0 ? DateTime.MaxValue : date.NowSecond.AddSeconds(length / minReceivePerSecond + 2);
        }
        /// <summary>
        /// 流合并命令处理
        /// </summary>
        /// <param name="socket">流套接字</param>
        /// <param name="data">输入数据</param>
        private unsafe void mergeStreamIdentity(socket socket, subArray<byte> data)
        {
            int isClose = 0;
            try
            {
                byte[] dataArray = data.Array;
                command command = default(command);
                fixed (byte* dataFixed = dataArray)
                {
                    int dataLength = data.Count;
                    byte* start = dataFixed + data.StartIndex;
                    do
                    {
                        int isCloseCommand = 0;
                        if (dataLength >= sizeof(int) * 2 + sizeof(commandServer.streamIdentity))
                        {
                            int commandIdentity = *(int*)start;
                            command.OnCommand = null;
                            if ((uint)commandIdentity < identityOnCommands.Length)
                            {
                                if (commandIdentity == CloseIdentityCommand)
                                {
                                    isCloseCommand = 1;
                                    command = mergeCheckCommand;
                                }
                                else command = identityOnCommands[commandIdentity];
                            }
                            if (command.OnCommand != null)
                            {
                                int length = *(int*)((start += sizeof(int) * 2 + sizeof(commandServer.streamIdentity)) - sizeof(int));
                                if ((uint)length <= command.MaxDataLength && (dataLength -= length + (sizeof(int) * 2 + sizeof(commandServer.streamIdentity))) >= 0)
                                {
                                    socket.identity = *(commandServer.streamIdentity*)(start - (sizeof(int) + sizeof(commandServer.streamIdentity)));
                                    if (isCloseCommand == 0) command.OnCommand(socket, subArray<byte>.Unsafe(dataArray, (int)(start - dataFixed), length));
                                    else isClose = 1;
                                    start += length;
                                    if (dataLength != 0) continue;
                                }
                            }
                        }
                        break;
                    }
                    while (true);
                }
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            if (isClose != 0) close(socket, default(subArray<byte>));
        }
        /// <summary>
        /// 流合并命令处理
        /// </summary>
        /// <param name="socket">流套接字</param>
        /// <param name="data">输入数据</param>
        private unsafe void mergeStream(socket socket, subArray<byte> data)
        {
            int isClose = 0;
            try
            {
                byte[] dataArray = data.Array;
                command command = default(command);
                fixed (byte* dataFixed = dataArray)
                {
                    int dataLength = data.Count;
                    byte* start = dataFixed + data.StartIndex;
                    do
                    {
                        int commandLength = *(int*)start, isCloseCommand = 0;// + (sizeof(commandServer.streamIdentity) - sizeof(int))
                        if ((uint)commandLength < maxCommandLength)
                        {
                            byte* commandIdentity = start + sizeof(int);
                            if (onCommands.Get(subArray<byte>.Unsafe(dataArray, (int)(commandIdentity - dataFixed), commandLength - (sizeof(int) * 2 + sizeof(commandServer.streamIdentity))), ref command))
                            {
                                if (((*(int*)commandIdentity ^ (CloseIdentityCommand + CommandDataIndex)) | (commandLength ^ (sizeof(int) * 3 + sizeof(commandServer.streamIdentity)))) == 0)
                                {
                                    command = mergeCheckCommand;
                                    isCloseCommand = 1;
                                }
                                int length = *(int*)((start += commandLength) - sizeof(int));
                                if ((uint)length <= command.MaxDataLength && (dataLength -= commandLength + length) >= 0)
                                {
                                    socket.identity = *(commandServer.streamIdentity*)(start - (sizeof(int) + sizeof(commandServer.streamIdentity)));
                                    if (isCloseCommand == 0) command.OnCommand(socket, subArray<byte>.Unsafe(dataArray, (int)(start - dataFixed), length));
                                    else isClose = 1;
                                    start += length;
                                    if (dataLength != 0) continue;
                                }
                            }
                        }
                        break;
                    }
                    while (true);
                }
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            if (isClose != 0) close(socket, default(subArray<byte>));
        }
        /// <summary>
        /// 关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private static void close(socket socket, subArray<byte> data)
        {
            socket.Close();
        }
        /// <summary>
        /// TCP流回馈
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private static void tcpStream(socket socket, subArray<byte> data)
        {
            socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = true });
            tcpStreamParameter parameter = fastCSharp.emit.dataDeSerializer.DeSerialize<commandServer.tcpStreamParameter>(data);
            if (parameter != null) socket.OnTcpStream(parameter);
        }

        /// <summary>
        /// 忽略分组事件
        /// </summary>
        public event Action<int> OnIgnoreGroup;
        /// <summary>
        /// 忽略分组
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private void ignoreGroup(socket socket, subArray<byte> data)
        {
            int groupId = 0;
            try
            {
                if (!fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref groupId)) groupId = 0;
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            finally
            {
                task.Tiny.Add(ignoreGroup, new keyValue<socket, int>(socket, groupId), null);
            }
        }
        /// <summary>
        /// 忽略分组
        /// </summary>
        /// <param name="socket">套接字+分组标识</param>
        private void ignoreGroup(keyValue<socket, int> socket)
        {
            if (OnIgnoreGroup != null) OnIgnoreGroup(socket.Value);
            ignoreGroup(socket.Value);
            fastCSharp.domainUnload.WaitTransaction();
            socket.Key.SendStream(socket.Key.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true });
        }
        /// <summary>
        /// 忽略分组
        /// </summary>
        /// <param name="groupId">分组标识</param>
        protected virtual void ignoreGroup(int groupId) { }
        /// <summary>
        /// 连接检测套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private static void check(socket socket, subArray<byte> data)
        {
            socket.SendStream(socket.identity, new asynchronousMethod.returnValue { IsReturn = true });
        }
        /// <summary>
        /// 最后一次负载均衡联通测试时间
        /// </summary>
        private DateTime loadBalancingCheckTime;
        /// <summary>
        /// 负载均衡联通测试时钟周期
        /// </summary>
        private long loadBalancingCheckTicks;
        /// <summary>
        /// 当前负载均衡套接字
        /// </summary>
        private socket loadBalancingSocket;
        /// <summary>
        /// 当前负载均衡套接字
        /// </summary>
        private socket nextLoadBalancingSocket;
        /// <summary>
        /// 负载均衡联通测试标识
        /// </summary>
        private int loadBalancingCheckIdentity;
        /// <summary>
        /// 负载均衡联通测试
        /// </summary>
        private Action loadBalancingCheckHandle;
        /// <summary>
        /// 负载均衡联通测试
        /// </summary>
        private void loadBalancingCheck()
        {
            if (loadBalancingSocket.LoadBalancingCheck(loadBalancingCheckIdentity))
            {
                if (loadBalancingCheckTime < date.NowSecond) loadBalancingCheckTime = date.NowSecond;
                fastCSharp.threading.timerTask.Default.Add(loadBalancingCheckHandle, loadBalancingCheckTime = loadBalancingCheckTime.AddTicks(loadBalancingCheckTicks), null);
            }
            else if (isStart == 0) loadBalancingSocket = nextLoadBalancingSocket = null;
            else
            {
                socket socket = nextLoadBalancingSocket;
                if (socket == null)
                {
                    loadBalancingSocket = null;
                    loadBalancing();
                }
                else
                {
                    nextLoadBalancingSocket = null;
                    loadBalancingSocket = socket;
                    socket.LoadBalancingCheckIdentity = ++loadBalancingCheckIdentity;
                    loadBalancingCheck();
                }
            }
        }
        /// <summary>
        /// 负载均衡连接检测套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private void loadBalancingCheck(socket socket, subArray<byte> data)
        {
            if (loadBalancingAttribute != null)
            {
                if (Interlocked.CompareExchange(ref loadBalancingSocket, socket, null) == null)
                {
                    if (loadBalancingCheckHandle == null) loadBalancingCheckHandle = loadBalancingCheck;
                    socket.LoadBalancingCheckIdentity = ++loadBalancingCheckIdentity;
                    fastCSharp.threading.threadPool.TinyPool.FastStart(loadBalancingCheckHandle, null, null);
                }
                else nextLoadBalancingSocket = socket;
            }
            check(socket, data);
        }
        /// <summary>
        /// 方法标识名称转TCP调用命令
        /// </summary>
        /// <param name="name">方法标识名称</param>
        /// <returns>TCP调用命令</returns>
        public unsafe static byte[] GetMethodKeyNameCommand(string name)
        {
            int length = name.Length, commandLength = (length + 3) & (int.MaxValue - 3);
            byte[] data = new byte[commandLength + sizeof(int)];
            fixed (byte* dataFixed = data)
            {
                *(int*)dataFixed = commandLength + sizeof(int) + sizeof(commandServer.streamIdentity) + sizeof(int);
                if ((length & 3) != 0) *(int*)(dataFixed + sizeof(int) + (length & (int.MaxValue - 3))) = 0x20202020;
                formatMethodKeyName(name, dataFixed + sizeof(int));
            }
            return data;
        }
        /// <summary>
        /// 格式化方法标识名称
        /// </summary>
        /// <param name="name">方法标识名称</param>
        /// <returns>方法标识名称</returns>
        protected internal unsafe static byte[] formatMethodKeyName(string name)
        {
            int length = name.Length;
            byte[] data = new byte[(length + 3) & (int.MaxValue - 3)];
            fixed (byte* dataFixed = data)
            {
                if ((length & 3) != 0) *(int*)(dataFixed + (length & (int.MaxValue - 3))) = 0x20202020;
                formatMethodKeyName(name, dataFixed);
            }
            return data;
        }
        /// <summary>
        /// 格式化方法标识名称
        /// </summary>
        /// <param name="name">方法标识名称</param>
        /// <param name="write">写入数据起始位置</param>
        protected internal unsafe static void formatMethodKeyName(string name, byte* write)
        {
            fixed (char* commandFixed = name)
            {
                for (char* start = commandFixed + name.Length, end = commandFixed; start != end; *write++ = (byte)*--start) ;
            }
        }
        ///// <summary>
        ///// 调试日志
        ///// </summary>
        //private static log debugLog;
        ///// <summary>
        ///// 调试日志访问锁
        ///// </summary>
        //private static int debugLogLock;
        ///// <summary>
        ///// 调试日志
        ///// </summary>
        //internal static log DebugLog
        //{
        //    get
        //    {
        //        if (debugLog == null)
        //        {
        //            interlocked.CompareSetSleep0NoCheck(ref debugLogLock);
        //            try
        //            {
        //                debugLog = new log(fastCSharp.config.appSetting.LogPath + "socketDebug.txt");
        //            }
        //            finally { debugLogLock = 0; }
        //        }
        //        return debugLog;
        //    }
        //}

        /// <summary>
        /// 关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        private static void closeSocket(Socket socket)
        {
            socket.Close();
        }
    }
}
