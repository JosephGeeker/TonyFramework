//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TmphCoreLib
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  TmphCoreLib
//	User name:  C1400008
//	Location Time: 2015/7/16 16:36:40
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

namespace TerumoMIS.CoreLibrary
{
    public partial class tcpRegister : fastCSharp.code.cSharp.tcpServer.ITcpServer
    {
        internal static class tcpServer
        {
            public static bool verify(fastCSharp.net.tcp.tcpRegister _value_, string value)
            {

                return _value_.verify(value);
            }
            public static fastCSharp.net.tcp.tcpRegister.registerResult register(fastCSharp.net.tcp.tcpRegister _value_, fastCSharp.net.tcp.tcpRegister.clientId client, fastCSharp.net.tcp.tcpRegister.service service)
            {

                return _value_.register(client, service);
            }
            public static void poll(fastCSharp.net.tcp.tcpRegister _value_, fastCSharp.net.tcp.tcpRegister.clientId client, System.Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.pollResult>, bool> onRegisterChanged)
            {
                _value_.poll(client, onRegisterChanged);
            }
            public static void removeRegister(fastCSharp.net.tcp.tcpRegister _value_, fastCSharp.net.tcp.tcpRegister.clientId client, string serviceName)
            {
                _value_.removeRegister(client, serviceName);
            }
            public static fastCSharp.net.tcp.tcpRegister.services[] getServices(fastCSharp.net.tcp.tcpRegister _value_, out int version)
            {

                return _value_.getServices(out version);
            }
            public static fastCSharp.net.tcp.tcpRegister.clientId register(fastCSharp.net.tcp.tcpRegister _value_)
            {

                return _value_.register();
            }
            public static void removeRegister(fastCSharp.net.tcp.tcpRegister _value_, fastCSharp.net.tcp.tcpRegister.clientId client)
            {
                _value_.removeRegister(client);
            }
        }
    }
}
namespace fastCSharp.tcpServer
{

    /// <summary>
    /// TCP服务
    /// </summary>
    public partial class tcpRegister : fastCSharp.net.tcp.commandServer
    {
        /// <summary>
        /// TCP服务目标对象
        /// </summary>
        private readonly fastCSharp.net.tcp.tcpRegister _value_;
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        public tcpRegister() : this(null, null) { }
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public tcpRegister(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.net.tcp.tcpRegister value)
            : base(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("tcpRegister", typeof(fastCSharp.net.tcp.tcpRegister)))
        {
            _value_ = value ?? new fastCSharp.net.tcp.tcpRegister();
            _value_.SetTcpServer(this);
            setCommands(7);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3);
            identityOnCommands[4 + 128].Set(_M4);
            identityOnCommands[5 + 128].Set(_M5, 0);
            identityOnCommands[6 + 128].Set(_M6);
        }

        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o0 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s0 : fastCSharp.net.tcp.commandServer.serverCall<_s0, fastCSharp.net.tcp.tcpRegister, _i0>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.net.tcp.tcpRegister/**/.tcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s0>.Push(this);
            }
        }
        private void _M0(socket socket, subArray<byte> data)
        {
            try
            {
                _i0 inputParameter = new _i0();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public fastCSharp.net.tcp.tcpRegister.clientId client;
            public fastCSharp.net.tcp.tcpRegister.service service;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o1 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.net.tcp.tcpRegister.registerResult>
        {
            public fastCSharp.net.tcp.tcpRegister.registerResult Ret;
            public fastCSharp.net.tcp.tcpRegister.registerResult Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s1 : fastCSharp.net.tcp.commandServer.serverCall<_s1, fastCSharp.net.tcp.tcpRegister, _i1>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o1> get()
            {
                try
                {

                    fastCSharp.net.tcp.tcpRegister.registerResult Return =
                    fastCSharp.net.tcp.tcpRegister/**/.tcpServer.register(serverValue, inputParameter.client, inputParameter.service);
                    return new _o1
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o1> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s1>.Push(this);
            }
        }
        private void _M1(socket socket, subArray<byte> data)
        {
            try
            {
                _i1 inputParameter = new _i1();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s1/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public fastCSharp.net.tcp.tcpRegister.clientId client;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o2 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.net.tcp.tcpRegister.pollResult>
        {
            public fastCSharp.net.tcp.tcpRegister.pollResult Ret;
            public fastCSharp.net.tcp.tcpRegister.pollResult Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        private void _M2(socket socket, subArray<byte> data)
        {
            try
            {
                _i2 inputParameter = new _i2();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.pollResult>, bool> callbackReturn = fastCSharp.net.tcp.commandServer.socket.callback<_o2, fastCSharp.net.tcp.tcpRegister.pollResult>.GetKeep(socket, new _o2());
                    if (callbackReturn != null)
                    {
                        fastCSharp.net.tcp.tcpRegister/**/.tcpServer.poll(_value_, inputParameter.client, callbackReturn);
                        return;
                    }
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i3
        {
            public fastCSharp.net.tcp.tcpRegister.clientId client;
            public string serviceName;
        }
        sealed class _s3 : fastCSharp.net.tcp.commandServer.serverCall<_s3, fastCSharp.net.tcp.tcpRegister, _i3>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.net.tcp.tcpRegister/**/.tcpServer.removeRegister(serverValue, inputParameter.client, inputParameter.serviceName);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s3>.Push(this);
            }
        }
        private void _M3(socket socket, subArray<byte> data)
        {
            try
            {
                _i3 inputParameter = new _i3();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s3/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i4
        {
            public int version;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o4 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.net.tcp.tcpRegister.services[]>
        {
            public int version;
            public fastCSharp.net.tcp.tcpRegister.services[] Ret;
            public fastCSharp.net.tcp.tcpRegister.services[] Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s4 : fastCSharp.net.tcp.commandServer.serverCall<_s4, fastCSharp.net.tcp.tcpRegister, _i4>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o4> get()
            {
                try
                {

                    fastCSharp.net.tcp.tcpRegister.services[] Return =
                    fastCSharp.net.tcp.tcpRegister/**/.tcpServer.getServices(serverValue, out inputParameter.version);
                    return new _o4
                    {
                        version = inputParameter.version,
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o4> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s4>.Push(this);
            }
        }
        private void _M4(socket socket, subArray<byte> data)
        {
            try
            {
                _i4 inputParameter = new _i4();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s4/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o5 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.net.tcp.tcpRegister.clientId>
        {
            public fastCSharp.net.tcp.tcpRegister.clientId Ret;
            public fastCSharp.net.tcp.tcpRegister.clientId Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s5 : fastCSharp.net.tcp.commandServer.serverCall<_s5, fastCSharp.net.tcp.tcpRegister>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o5> get()
            {
                try
                {

                    fastCSharp.net.tcp.tcpRegister.clientId Return =
                    fastCSharp.net.tcp.tcpRegister/**/.tcpServer.register(serverValue);
                    return new _o5
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o5> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s5>.Push(this);
            }
        }
        private void _M5(socket socket, subArray<byte> data)
        {
            try
            {
                {
                    _s5/**/.Call(socket, _value_, socket.Identity)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i6
        {
            public fastCSharp.net.tcp.tcpRegister.clientId client;
        }
        sealed class _s6 : fastCSharp.net.tcp.commandServer.serverCall<_s6, fastCSharp.net.tcp.tcpRegister, _i6>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.net.tcp.tcpRegister/**/.tcpServer.removeRegister(serverValue, inputParameter.client);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s6>.Push(this);
            }
        }
        private void _M6(socket socket, subArray<byte> data)
        {
            try
            {
                _i6 inputParameter = new _i6();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s6/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
    }
}
namespace fastCSharp.tcpClient
{

    public class tcpRegister : IDisposable
    {
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public fastCSharp.net.tcp.commandClient<tcpRegister> _TcpClient_ { get; private set; }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public tcpRegister() : this(null, null) { }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public tcpRegister(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<tcpRegister> verifyMethod)
        {
            _TcpClient_ = new fastCSharp.net.tcp.commandClient<tcpRegister>(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("tcpRegister", typeof(fastCSharp.net.tcp.tcpRegister)), 24, verifyMethod ?? new fastCSharp.net.tcp.verifyMethod(), this);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            fastCSharp.net.tcp.commandClient<tcpRegister> client = _TcpClient_;
            _TcpClient_ = null;
            fastCSharp.pub.Dispose(ref client);
        }



        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> verify(string value)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.tcpRegister/**/._o0> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.tcpRegister/**/._o0>.Get();
            if (_wait_ != null)
            {

                verify(value, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o0> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o0 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void verify(string value, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._i0 _inputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._i0
                    {
                        value = value,
                    };

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o0 _outputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o0> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.registerResult> register(fastCSharp.net.tcp.tcpRegister.clientId client, fastCSharp.net.tcp.tcpRegister.service service)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.tcpRegister/**/._o1> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.tcpRegister/**/._o1>.Get();
            if (_wait_ != null)
            {

                register(client, service, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o1> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o1 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.registerResult> { IsReturn = false };
        }
        private void register(fastCSharp.net.tcp.tcpRegister.clientId client, fastCSharp.net.tcp.tcpRegister.service service, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o1>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._i1 _inputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._i1
                    {
                        client = client,
                        service = service,
                    };

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o1 _outputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._o1();
                    _socket_.Get(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o1> { IsReturn = false });
        }
        public fastCSharp.net.tcp.commandClient.streamCommandSocket.keepCallback poll(fastCSharp.net.tcp.tcpRegister.clientId client, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.pollResult>> _onReturn_)
        {
            Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o2>> _onOutput_ = null;
            _onOutput_ = fastCSharp.code.cSharp.asynchronousMethod.callReturn<fastCSharp.net.tcp.tcpRegister.pollResult, fastCSharp.tcpServer/**/.tcpRegister/**/._o2>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {

                return poll(client, _onOutput_, true);
            }
            return null;
        }
        private fastCSharp.net.tcp.commandClient.streamCommandSocket.keepCallback poll(fastCSharp.net.tcp.tcpRegister.clientId client, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o2>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._i2 _inputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._i2
                    {
                        client = client,
                    };

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o2 _outputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._o2();

                    return _socket_.Get(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, true);
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o2> { IsReturn = false });
            return null;
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue removeRegister(fastCSharp.net.tcp.tcpRegister.clientId client, string serviceName)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                removeRegister(client, serviceName, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void removeRegister(fastCSharp.net.tcp.tcpRegister.clientId client, string serviceName, Action<bool> _onReturn_)
        {

            removeRegister(client, serviceName, _onReturn_, true);
        }
        private void removeRegister(fastCSharp.net.tcp.tcpRegister.clientId client, string serviceName, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._i3 _inputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._i3
                    {
                        client = client,
                        serviceName = serviceName,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.tcpRegister/**/._i3>(_onReturn_, 3 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.services[]> getServices(out int version)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.tcpRegister/**/._o4> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.tcpRegister/**/._o4>.Get();
            if (_wait_ != null)
            {

                getServices(out version, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o4> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o4 _outputParameterValue_ = _outputParameter_.Value;
                    version = _outputParameterValue_.version;
                    return _outputParameterValue_.Return;
                }
            }
            version = default(int);
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.services[]> { IsReturn = false };
        }
        private void getServices(out int version, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o4>> _onReturn_, bool _isTask_)
        {
            version = default(int);
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._i4 _inputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._i4
                    {
                    };

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o4 _outputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._o4();
                    _socket_.Get(_onReturn_, 4 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o4> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.clientId> register()
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.tcpRegister/**/._o5> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.tcpRegister/**/._o5>.Get();
            if (_wait_ != null)
            {

                register(_wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o5> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o5 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.tcpRegister.clientId> { IsReturn = false };
        }
        private void register(Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o5>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._o5 _outputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._o5();
                    _socket_.Get(_onReturn_, 5 + 128, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.tcpRegister/**/._o5> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue removeRegister(fastCSharp.net.tcp.tcpRegister.clientId client)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                removeRegister(client, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void removeRegister(fastCSharp.net.tcp.tcpRegister.clientId client, Action<bool> _onReturn_)
        {

            removeRegister(client, _onReturn_, true);
        }
        private void removeRegister(fastCSharp.net.tcp.tcpRegister.clientId client, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.tcpRegister/**/._i6 _inputParameter_ = new fastCSharp.tcpServer/**/.tcpRegister/**/._i6
                    {
                        client = client,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.tcpRegister/**/._i6>(_onReturn_, 6 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
    }
}
namespace fastCSharp.net.tcp.http
{
    public partial class servers : fastCSharp.code.cSharp.tcpServer.ITcpServer
    {
        internal static class tcpServer
        {
            public static bool verify(fastCSharp.net.tcp.http.servers _value_, string value)
            {

                return _value_.verify(value);
            }
            public static bool setForward(fastCSharp.net.tcp.http.servers _value_, fastCSharp.net.tcp.host host)
            {

                return _value_.setForward(host);
            }
            public static void stop(fastCSharp.net.tcp.http.servers _value_, fastCSharp.net.tcp.http.domain[] domains)
            {
                _value_.stop(domains);
            }
            public static void stop(fastCSharp.net.tcp.http.servers _value_, fastCSharp.net.tcp.http.domain domain)
            {
                _value_.stop(domain);
            }
            public static fastCSharp.net.tcp.http.servers.startState start(fastCSharp.net.tcp.http.servers _value_, string assemblyPath, string serverType, fastCSharp.net.tcp.http.domain[] domains, bool isShareAssembly)
            {

                return _value_.start(assemblyPath, serverType, domains, isShareAssembly);
            }
            public static fastCSharp.net.tcp.http.servers.startState start(fastCSharp.net.tcp.http.servers _value_, string assemblyPath, string serverType, fastCSharp.net.tcp.http.domain domain, bool isShareAssembly)
            {

                return _value_.start(assemblyPath, serverType, domain, isShareAssembly);
            }
            public static void removeForward(fastCSharp.net.tcp.http.servers _value_)
            {
                _value_.removeForward();
            }
        }
    }
}
namespace fastCSharp.tcpServer
{

    /// <summary>
    /// TCP服务
    /// </summary>
    public partial class httpServer : fastCSharp.net.tcp.commandServer
    {
        /// <summary>
        /// TCP服务目标对象
        /// </summary>
        private readonly fastCSharp.net.tcp.http.servers _value_;
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        public httpServer() : this(null, null) { }
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public httpServer(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.net.tcp.http.servers value)
            : base(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("httpServer", typeof(fastCSharp.net.tcp.http.servers)))
        {
            _value_ = value ?? new fastCSharp.net.tcp.http.servers();
            _value_.SetTcpServer(this);
            setCommands(7);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3);
            identityOnCommands[4 + 128].Set(_M4);
            identityOnCommands[5 + 128].Set(_M5);
            identityOnCommands[6 + 128].Set(_M6, 0);
        }

        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o0 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s0 : fastCSharp.net.tcp.commandServer.serverCall<_s0, fastCSharp.net.tcp.http.servers, _i0>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.net.tcp.http.servers/**/.tcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s0>.Push(this);
            }
        }
        private void _M0(socket socket, subArray<byte> data)
        {
            try
            {
                _i0 inputParameter = new _i0();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public fastCSharp.net.tcp.host host;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o1 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s1 : fastCSharp.net.tcp.commandServer.serverCall<_s1, fastCSharp.net.tcp.http.servers, _i1>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o1> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.net.tcp.http.servers/**/.tcpServer.setForward(serverValue, inputParameter.host);
                    return new _o1
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o1> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s1>.Push(this);
            }
        }
        private void _M1(socket socket, subArray<byte> data)
        {
            try
            {
                _i1 inputParameter = new _i1();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s1/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public fastCSharp.net.tcp.http.domain[] domains;
        }
        sealed class _s2 : fastCSharp.net.tcp.commandServer.serverCall<_s2, fastCSharp.net.tcp.http.servers, _i2>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.net.tcp.http.servers/**/.tcpServer.stop(serverValue, inputParameter.domains);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s2>.Push(this);
            }
        }
        private void _M2(socket socket, subArray<byte> data)
        {
            try
            {
                _i2 inputParameter = new _i2();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s2/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i3
        {
            public fastCSharp.net.tcp.http.domain domain;
        }
        sealed class _s3 : fastCSharp.net.tcp.commandServer.serverCall<_s3, fastCSharp.net.tcp.http.servers, _i3>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.net.tcp.http.servers/**/.tcpServer.stop(serverValue, inputParameter.domain);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s3>.Push(this);
            }
        }
        private void _M3(socket socket, subArray<byte> data)
        {
            try
            {
                _i3 inputParameter = new _i3();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s3/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i4
        {
            public string assemblyPath;
            public string serverType;
            public fastCSharp.net.tcp.http.domain[] domains;
            public bool isShareAssembly;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o4 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.net.tcp.http.servers.startState>
        {
            public fastCSharp.net.tcp.http.servers.startState Ret;
            public fastCSharp.net.tcp.http.servers.startState Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s4 : fastCSharp.net.tcp.commandServer.serverCall<_s4, fastCSharp.net.tcp.http.servers, _i4>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o4> get()
            {
                try
                {

                    fastCSharp.net.tcp.http.servers.startState Return =
                    fastCSharp.net.tcp.http.servers/**/.tcpServer.start(serverValue, inputParameter.assemblyPath, inputParameter.serverType, inputParameter.domains, inputParameter.isShareAssembly);
                    return new _o4
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o4> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s4>.Push(this);
            }
        }
        private void _M4(socket socket, subArray<byte> data)
        {
            try
            {
                _i4 inputParameter = new _i4();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s4/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i5
        {
            public string assemblyPath;
            public string serverType;
            public fastCSharp.net.tcp.http.domain domain;
            public bool isShareAssembly;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o5 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.net.tcp.http.servers.startState>
        {
            public fastCSharp.net.tcp.http.servers.startState Ret;
            public fastCSharp.net.tcp.http.servers.startState Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s5 : fastCSharp.net.tcp.commandServer.serverCall<_s5, fastCSharp.net.tcp.http.servers, _i5>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o5> get()
            {
                try
                {

                    fastCSharp.net.tcp.http.servers.startState Return =
                    fastCSharp.net.tcp.http.servers/**/.tcpServer.start(serverValue, inputParameter.assemblyPath, inputParameter.serverType, inputParameter.domain, inputParameter.isShareAssembly);
                    return new _o5
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o5> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s5>.Push(this);
            }
        }
        private void _M5(socket socket, subArray<byte> data)
        {
            try
            {
                _i5 inputParameter = new _i5();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s5/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        sealed class _s6 : fastCSharp.net.tcp.commandServer.serverCall<_s6, fastCSharp.net.tcp.http.servers>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.net.tcp.http.servers/**/.tcpServer.removeForward(serverValue);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s6>.Push(this);
            }
        }
        private void _M6(socket socket, subArray<byte> data)
        {
            try
            {
                {
                    _s6/**/.Call(socket, _value_, socket.Identity)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
    }
}
namespace fastCSharp.tcpClient
{

    public class httpServer : IDisposable
    {
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public fastCSharp.net.tcp.commandClient<httpServer> _TcpClient_ { get; private set; }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public httpServer() : this(null, null) { }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public httpServer(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<httpServer> verifyMethod)
        {
            _TcpClient_ = new fastCSharp.net.tcp.commandClient<httpServer>(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("httpServer", typeof(fastCSharp.net.tcp.http.servers)), 24, verifyMethod ?? new fastCSharp.net.tcp.verifyMethod(), this);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            fastCSharp.net.tcp.commandClient<httpServer> client = _TcpClient_;
            _TcpClient_ = null;
            fastCSharp.pub.Dispose(ref client);
        }



        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> verify(string value)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.httpServer/**/._o0> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.httpServer/**/._o0>.Get();
            if (_wait_ != null)
            {

                verify(value, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o0> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._o0 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void verify(string value, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._i0 _inputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._i0
                    {
                        value = value,
                    };

                    fastCSharp.tcpServer/**/.httpServer/**/._o0 _outputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o0> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> setForward(fastCSharp.net.tcp.host host)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.httpServer/**/._o1> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.httpServer/**/._o1>.Get();
            if (_wait_ != null)
            {

                setForward(host, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o1> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._o1 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void setForward(fastCSharp.net.tcp.host host, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o1>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._i1 _inputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._i1
                    {
                        host = host,
                    };

                    fastCSharp.tcpServer/**/.httpServer/**/._o1 _outputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._o1();
                    _socket_.Get(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o1> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue stop(fastCSharp.net.tcp.http.domain[] domains)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                stop(domains, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void stop(fastCSharp.net.tcp.http.domain[] domains, Action<bool> _onReturn_)
        {

            stop(domains, _onReturn_, true);
        }
        private void stop(fastCSharp.net.tcp.http.domain[] domains, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._i2 _inputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._i2
                    {
                        domains = domains,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.httpServer/**/._i2>(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue stop(fastCSharp.net.tcp.http.domain domain)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                stop(domain, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void stop(fastCSharp.net.tcp.http.domain domain, Action<bool> _onReturn_)
        {

            stop(domain, _onReturn_, true);
        }
        private void stop(fastCSharp.net.tcp.http.domain domain, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._i3 _inputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._i3
                    {
                        domain = domain,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.httpServer/**/._i3>(_onReturn_, 3 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.http.servers.startState> start(string assemblyPath, string serverType, fastCSharp.net.tcp.http.domain[] domains, bool isShareAssembly)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.httpServer/**/._o4> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.httpServer/**/._o4>.Get();
            if (_wait_ != null)
            {

                start(assemblyPath, serverType, domains, isShareAssembly, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o4> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._o4 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.http.servers.startState> { IsReturn = false };
        }
        private void start(string assemblyPath, string serverType, fastCSharp.net.tcp.http.domain[] domains, bool isShareAssembly, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o4>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._i4 _inputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._i4
                    {
                        assemblyPath = assemblyPath,
                        serverType = serverType,
                        domains = domains,
                        isShareAssembly = isShareAssembly,
                    };

                    fastCSharp.tcpServer/**/.httpServer/**/._o4 _outputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._o4();
                    _socket_.Get(_onReturn_, 4 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o4> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.http.servers.startState> start(string assemblyPath, string serverType, fastCSharp.net.tcp.http.domain domain, bool isShareAssembly)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.httpServer/**/._o5> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.httpServer/**/._o5>.Get();
            if (_wait_ != null)
            {

                start(assemblyPath, serverType, domain, isShareAssembly, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o5> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._o5 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.net.tcp.http.servers.startState> { IsReturn = false };
        }
        private void start(string assemblyPath, string serverType, fastCSharp.net.tcp.http.domain domain, bool isShareAssembly, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o5>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.httpServer/**/._i5 _inputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._i5
                    {
                        assemblyPath = assemblyPath,
                        serverType = serverType,
                        domain = domain,
                        isShareAssembly = isShareAssembly,
                    };

                    fastCSharp.tcpServer/**/.httpServer/**/._o5 _outputParameter_ = new fastCSharp.tcpServer/**/.httpServer/**/._o5();
                    _socket_.Get(_onReturn_, 5 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.httpServer/**/._o5> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue removeForward()
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                removeForward(_wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void removeForward(Action<bool> _onReturn_)
        {

            removeForward(_onReturn_, true);
        }
        private void removeForward(Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    _socket_.Call(_onReturn_, 6 + 128, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
    }
}
namespace fastCSharp.diagnostics
{
    public partial class processCopyServer : fastCSharp.code.cSharp.tcpServer.ITcpServer
    {
        internal static class tcpServer
        {
            public static bool verify(fastCSharp.diagnostics.processCopyServer _value_, string value)
            {

                return _value_.verify(value);
            }
            public static void guard(fastCSharp.diagnostics.processCopyServer _value_, fastCSharp.diagnostics.processCopyServer.copyer copyer)
            {
                _value_.guard(copyer);
            }
            public static void copyStart(fastCSharp.diagnostics.processCopyServer _value_, fastCSharp.diagnostics.processCopyServer.copyer copyer)
            {
                _value_.copyStart(copyer);
            }
            public static void remove(fastCSharp.diagnostics.processCopyServer _value_, fastCSharp.diagnostics.processCopyServer.copyer copyer)
            {
                _value_.remove(copyer);
            }
        }
    }
}
namespace fastCSharp.tcpServer
{

    /// <summary>
    /// TCP服务
    /// </summary>
    public partial class processCopy : fastCSharp.net.tcp.commandServer
    {
        /// <summary>
        /// TCP服务目标对象
        /// </summary>
        private readonly fastCSharp.diagnostics.processCopyServer _value_;
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        public processCopy() : this(null, null) { }
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public processCopy(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.diagnostics.processCopyServer value)
            : base(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("processCopy", typeof(fastCSharp.diagnostics.processCopyServer)))
        {
            _value_ = value ?? new fastCSharp.diagnostics.processCopyServer();
            _value_.SetTcpServer(this);
            setCommands(4);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3);
        }

        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o0 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s0 : fastCSharp.net.tcp.commandServer.serverCall<_s0, fastCSharp.diagnostics.processCopyServer, _i0>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.diagnostics.processCopyServer/**/.tcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s0>.Push(this);
            }
        }
        private void _M0(socket socket, subArray<byte> data)
        {
            try
            {
                _i0 inputParameter = new _i0();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public fastCSharp.diagnostics.processCopyServer.copyer copyer;
        }
        sealed class _s1 : fastCSharp.net.tcp.commandServer.serverCall<_s1, fastCSharp.diagnostics.processCopyServer, _i1>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.diagnostics.processCopyServer/**/.tcpServer.guard(serverValue, inputParameter.copyer);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s1>.Push(this);
            }
        }
        private void _M1(socket socket, subArray<byte> data)
        {
            try
            {
                _i1 inputParameter = new _i1();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s1/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public fastCSharp.diagnostics.processCopyServer.copyer copyer;
        }
        sealed class _s2 : fastCSharp.net.tcp.commandServer.serverCall<_s2, fastCSharp.diagnostics.processCopyServer, _i2>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.diagnostics.processCopyServer/**/.tcpServer.copyStart(serverValue, inputParameter.copyer);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s2>.Push(this);
            }
        }
        private void _M2(socket socket, subArray<byte> data)
        {
            try
            {
                _i2 inputParameter = new _i2();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s2/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i3
        {
            public fastCSharp.diagnostics.processCopyServer.copyer copyer;
        }
        sealed class _s3 : fastCSharp.net.tcp.commandServer.serverCall<_s3, fastCSharp.diagnostics.processCopyServer, _i3>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.diagnostics.processCopyServer/**/.tcpServer.remove(serverValue, inputParameter.copyer);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s3>.Push(this);
            }
        }
        private void _M3(socket socket, subArray<byte> data)
        {
            try
            {
                _i3 inputParameter = new _i3();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s3/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
    }
}
namespace fastCSharp.tcpClient
{

    public class processCopy : IDisposable
    {
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public fastCSharp.net.tcp.commandClient<processCopy> _TcpClient_ { get; private set; }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public processCopy() : this(null, null) { }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public processCopy(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<processCopy> verifyMethod)
        {
            _TcpClient_ = new fastCSharp.net.tcp.commandClient<processCopy>(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("processCopy", typeof(fastCSharp.diagnostics.processCopyServer)), 24, verifyMethod ?? new fastCSharp.net.tcp.verifyMethod(), this);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            fastCSharp.net.tcp.commandClient<processCopy> client = _TcpClient_;
            _TcpClient_ = null;
            fastCSharp.pub.Dispose(ref client);
        }



        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> verify(string value)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.processCopy/**/._o0> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.processCopy/**/._o0>.Get();
            if (_wait_ != null)
            {

                verify(value, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.processCopy/**/._o0> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.processCopy/**/._o0 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void verify(string value, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.processCopy/**/._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.processCopy/**/._i0 _inputParameter_ = new fastCSharp.tcpServer/**/.processCopy/**/._i0
                    {
                        value = value,
                    };

                    fastCSharp.tcpServer/**/.processCopy/**/._o0 _outputParameter_ = new fastCSharp.tcpServer/**/.processCopy/**/._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.processCopy/**/._o0> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue guard(fastCSharp.diagnostics.processCopyServer.copyer copyer)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                guard(copyer, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void guard(fastCSharp.diagnostics.processCopyServer.copyer copyer, Action<bool> _onReturn_)
        {

            guard(copyer, _onReturn_, true);
        }
        private void guard(fastCSharp.diagnostics.processCopyServer.copyer copyer, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.processCopy/**/._i1 _inputParameter_ = new fastCSharp.tcpServer/**/.processCopy/**/._i1
                    {
                        copyer = copyer,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.processCopy/**/._i1>(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue copyStart(fastCSharp.diagnostics.processCopyServer.copyer copyer)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                copyStart(copyer, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void copyStart(fastCSharp.diagnostics.processCopyServer.copyer copyer, Action<bool> _onReturn_)
        {

            copyStart(copyer, _onReturn_, true);
        }
        private void copyStart(fastCSharp.diagnostics.processCopyServer.copyer copyer, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.processCopy/**/._i2 _inputParameter_ = new fastCSharp.tcpServer/**/.processCopy/**/._i2
                    {
                        copyer = copyer,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.processCopy/**/._i2>(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue remove(fastCSharp.diagnostics.processCopyServer.copyer copyer)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                remove(copyer, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void remove(fastCSharp.diagnostics.processCopyServer.copyer copyer, Action<bool> _onReturn_)
        {

            remove(copyer, _onReturn_, true);
        }
        private void remove(fastCSharp.diagnostics.processCopyServer.copyer copyer, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.processCopy/**/._i3 _inputParameter_ = new fastCSharp.tcpServer/**/.processCopy/**/._i3
                    {
                        copyer = copyer,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.processCopy/**/._i3>(_onReturn_, 3 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
    }
}
namespace fastCSharp.memoryDatabase
{
    public partial class physicalServer
    {
        internal static class tcpServer
        {
            public static bool verify(fastCSharp.memoryDatabase.physicalServer _value_, string value)
            {

                return _value_.verify(value);
            }
            public static fastCSharp.memoryDatabase.physicalServer.physicalIdentity open(fastCSharp.memoryDatabase.physicalServer _value_, string fileName)
            {

                return _value_.open(fileName);
            }
            public static void close(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.memoryDatabase.physicalServer.timeIdentity identity)
            {
                _value_.close(identity);
            }
            public static bool create(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream stream)
            {

                return _value_.create(stream);
            }
            public static fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer load(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.memoryDatabase.physicalServer.timeIdentity identity)
            {

                return _value_.load(identity);
            }
            public static bool loaded(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, bool isLoaded)
            {

                return _value_.loaded(identity, isLoaded);
            }
            public static int append(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream)
            {

                return _value_.append(dataStream);
            }
            public static void waitBuffer(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.memoryDatabase.physicalServer.timeIdentity identity)
            {
                _value_.waitBuffer(identity);
            }
            public static bool flush(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.memoryDatabase.physicalServer.timeIdentity identity)
            {

                return _value_.flush(identity);
            }
            public static bool flushFile(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, bool isDiskFile)
            {

                return _value_.flushFile(identity, isDiskFile);
            }
            public static fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer loadHeader(fastCSharp.memoryDatabase.physicalServer _value_, fastCSharp.memoryDatabase.physicalServer.timeIdentity identity)
            {

                return _value_.loadHeader(identity);
            }
        }
    }
}
namespace fastCSharp.tcpServer
{

    /// <summary>
    /// TCP服务
    /// </summary>
    public partial class memoryDatabasePhysical : fastCSharp.net.tcp.commandServer
    {
        /// <summary>
        /// TCP服务目标对象
        /// </summary>
        private readonly fastCSharp.memoryDatabase.physicalServer _value_;
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        public memoryDatabasePhysical() : this(null, null) { }
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public memoryDatabasePhysical(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.memoryDatabase.physicalServer value)
            : base(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("memoryDatabasePhysical", typeof(fastCSharp.memoryDatabase.physicalServer)))
        {
            _value_ = value ?? new fastCSharp.memoryDatabase.physicalServer();
            setCommands(11);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3);
            identityOnCommands[4 + 128].Set(_M4);
            identityOnCommands[5 + 128].Set(_M5);
            identityOnCommands[6 + 128].Set(_M6);
            identityOnCommands[7 + 128].Set(_M7);
            identityOnCommands[8 + 128].Set(_M8);
            identityOnCommands[9 + 128].Set(_M9);
            identityOnCommands[10 + 128].Set(_M10);
        }

        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o0 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s0 : fastCSharp.net.tcp.commandServer.serverCall<_s0, fastCSharp.memoryDatabase.physicalServer, _i0>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s0>.Push(this);
            }
        }
        private void _M0(socket socket, subArray<byte> data)
        {
            try
            {
                _i0 inputParameter = new _i0();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public string fileName;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o1 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.memoryDatabase.physicalServer.physicalIdentity>
        {
            public fastCSharp.memoryDatabase.physicalServer.physicalIdentity Ret;
            public fastCSharp.memoryDatabase.physicalServer.physicalIdentity Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s1 : fastCSharp.net.tcp.commandServer.serverCall<_s1, fastCSharp.memoryDatabase.physicalServer, _i1>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o1> get()
            {
                try
                {

                    fastCSharp.memoryDatabase.physicalServer.physicalIdentity Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.open(serverValue, inputParameter.fileName);
                    return new _o1
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o1> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s1>.Push(this);
            }
        }
        private void _M1(socket socket, subArray<byte> data)
        {
            try
            {
                _i1 inputParameter = new _i1();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s1/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public fastCSharp.memoryDatabase.physicalServer.timeIdentity identity;
        }
        sealed class _s2 : fastCSharp.net.tcp.commandServer.serverCall<_s2, fastCSharp.memoryDatabase.physicalServer, _i2>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.close(serverValue, inputParameter.identity);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s2>.Push(this);
            }
        }
        private void _M2(socket socket, subArray<byte> data)
        {
            try
            {
                _i2 inputParameter = new _i2();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s2/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i3
        {
            public fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream stream;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o3 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s3 : fastCSharp.net.tcp.commandServer.serverCall<_s3, fastCSharp.memoryDatabase.physicalServer, _i3>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o3> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.create(serverValue, inputParameter.stream);
                    return new _o3
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o3> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s3>.Push(this);
            }
        }
        private void _M3(socket socket, subArray<byte> data)
        {
            try
            {
                _i3 inputParameter = new _i3();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s3/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i4
        {
            public fastCSharp.memoryDatabase.physicalServer.timeIdentity identity;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o4 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer>
        {
            public fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer Ret;
            public fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s4 : fastCSharp.net.tcp.commandServer.serverCall<_s4, fastCSharp.memoryDatabase.physicalServer, _i4>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o4> get()
            {
                try
                {

                    fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.load(serverValue, inputParameter.identity);
                    return new _o4
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o4> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s4>.Push(this);
            }
        }
        private void _M4(socket socket, subArray<byte> data)
        {
            try
            {
                _i4 inputParameter = new _i4();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s4/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i5
        {
            public fastCSharp.memoryDatabase.physicalServer.timeIdentity identity;
            public bool isLoaded;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o5 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s5 : fastCSharp.net.tcp.commandServer.serverCall<_s5, fastCSharp.memoryDatabase.physicalServer, _i5>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o5> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.loaded(serverValue, inputParameter.identity, inputParameter.isLoaded);
                    return new _o5
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o5> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s5>.Push(this);
            }
        }
        private void _M5(socket socket, subArray<byte> data)
        {
            try
            {
                _i5 inputParameter = new _i5();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s5/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i6
        {
            public fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o6 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<int>
        {
            public int Ret;
            public int Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s6 : fastCSharp.net.tcp.commandServer.serverCall<_s6, fastCSharp.memoryDatabase.physicalServer, _i6>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o6> get()
            {
                try
                {

                    int Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.append(serverValue, inputParameter.dataStream);
                    return new _o6
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o6> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s6>.Push(this);
            }
        }
        private void _M6(socket socket, subArray<byte> data)
        {
            try
            {
                _i6 inputParameter = new _i6();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s6/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i7
        {
            public fastCSharp.memoryDatabase.physicalServer.timeIdentity identity;
        }
        sealed class _s7 : fastCSharp.net.tcp.commandServer.serverCall<_s7, fastCSharp.memoryDatabase.physicalServer, _i7>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.waitBuffer(serverValue, inputParameter.identity);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s7>.Push(this);
            }
        }
        private void _M7(socket socket, subArray<byte> data)
        {
            try
            {
                _i7 inputParameter = new _i7();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s7/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i8
        {
            public fastCSharp.memoryDatabase.physicalServer.timeIdentity identity;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o8 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s8 : fastCSharp.net.tcp.commandServer.serverCall<_s8, fastCSharp.memoryDatabase.physicalServer, _i8>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o8> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.flush(serverValue, inputParameter.identity);
                    return new _o8
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o8> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s8>.Push(this);
            }
        }
        private void _M8(socket socket, subArray<byte> data)
        {
            try
            {
                _i8 inputParameter = new _i8();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s8/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i9
        {
            public fastCSharp.memoryDatabase.physicalServer.timeIdentity identity;
            public bool isDiskFile;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o9 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s9 : fastCSharp.net.tcp.commandServer.serverCall<_s9, fastCSharp.memoryDatabase.physicalServer, _i9>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o9> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.flushFile(serverValue, inputParameter.identity, inputParameter.isDiskFile);
                    return new _o9
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o9> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s9>.Push(this);
            }
        }
        private void _M9(socket socket, subArray<byte> data)
        {
            try
            {
                _i9 inputParameter = new _i9();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s9/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i10
        {
            public fastCSharp.memoryDatabase.physicalServer.timeIdentity identity;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o10 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer>
        {
            public fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer Ret;
            public fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s10 : fastCSharp.net.tcp.commandServer.serverCall<_s10, fastCSharp.memoryDatabase.physicalServer, _i10>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o10> get()
            {
                try
                {

                    fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer Return =
                    fastCSharp.memoryDatabase.physicalServer/**/.tcpServer.loadHeader(serverValue, inputParameter.identity);
                    return new _o10
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o10> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s10>.Push(this);
            }
        }
        private void _M10(socket socket, subArray<byte> data)
        {
            try
            {
                _i10 inputParameter = new _i10();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s10/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
    }
}
namespace fastCSharp.tcpClient
{

    public class memoryDatabasePhysical : IDisposable
    {
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public fastCSharp.net.tcp.commandClient<memoryDatabasePhysical> _TcpClient_ { get; private set; }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public memoryDatabasePhysical() : this(null, null) { }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public memoryDatabasePhysical(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<memoryDatabasePhysical> verifyMethod)
        {
            _TcpClient_ = new fastCSharp.net.tcp.commandClient<memoryDatabasePhysical>(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("memoryDatabasePhysical", typeof(fastCSharp.memoryDatabase.physicalServer)), 24, verifyMethod ?? new fastCSharp.net.tcp.verifyMethod(), this);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            fastCSharp.net.tcp.commandClient<memoryDatabasePhysical> client = _TcpClient_;
            _TcpClient_ = null;
            fastCSharp.pub.Dispose(ref client);
        }



        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> verify(string value)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o0> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o0>.Get();
            if (_wait_ != null)
            {

                verify(value, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o0> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o0 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void verify(string value, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i0 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i0
                    {
                        value = value,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o0 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o0> { IsReturn = false });
        }
        public void open(string fileName, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.memoryDatabase.physicalServer.physicalIdentity>> _onReturn_)
        {
            Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o1>> _onOutput_ = null;
            _onOutput_ = fastCSharp.code.cSharp.asynchronousMethod.callReturn<fastCSharp.memoryDatabase.physicalServer.physicalIdentity, fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o1>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                open(fileName, _onOutput_, true);
            }
        }
        private void open(string fileName, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o1>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i1 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i1
                    {
                        fileName = fileName,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o1 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o1();
                    _socket_.Get(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o1> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue close(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                close(identity, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void close(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<bool> _onReturn_)
        {

            close(identity, _onReturn_, true);
        }
        private void close(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i2 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i2
                    {
                        identity = identity,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i2>(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> create(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream stream)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o3> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o3>.Get();
            if (_wait_ != null)
            {

                create(stream, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o3> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o3 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void create(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream stream, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o3>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i3 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i3
                    {
                        stream = stream,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o3 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o3();
                    _socket_.Get(_onReturn_, 3 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o3> { IsReturn = false });
        }
        public void load(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer>> _onReturn_)
        {
            Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o4>> _onOutput_ = null;
            _onOutput_ = fastCSharp.code.cSharp.asynchronousMethod.callReturn<fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer, fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o4>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                load(identity, _onOutput_, false);
            }
        }
        private void load(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o4>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i4 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i4
                    {
                        identity = identity,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o4 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o4();
                    _socket_.Get(_onReturn_, 4 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o4> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> loaded(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, bool isLoaded)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o5> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o5>.Get();
            if (_wait_ != null)
            {

                loaded(identity, isLoaded, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o5> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o5 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void loaded(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, bool isLoaded, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o5>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i5 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i5
                    {
                        identity = identity,
                        isLoaded = isLoaded,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o5 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o5();
                    _socket_.Get(_onReturn_, 5 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o5> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<int> append(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6>.Get();
            if (_wait_ != null)
            {

                append(dataStream, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<int> { IsReturn = false };
        }
        public void append(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<int>> _onReturn_)
        {
            Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6>> _onOutput_ = null;
            _onOutput_ = fastCSharp.code.cSharp.asynchronousMethod.callReturn<int, fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                append(dataStream, _onOutput_, false);
            }
        }
        private void append(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i6 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i6
                    {
                        dataStream = dataStream,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6();
                    _socket_.Get(_onReturn_, 6 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o6> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue waitBuffer(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                waitBuffer(identity, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void waitBuffer(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<bool> _onReturn_)
        {

            waitBuffer(identity, _onReturn_, true);
        }
        private void waitBuffer(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i7 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i7
                    {
                        identity = identity,
                    };
                    _socket_.Call<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i7>(_onReturn_, 7 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> flush(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o8> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o8>.Get();
            if (_wait_ != null)
            {

                flush(identity, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o8> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o8 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void flush(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o8>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i8 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i8
                    {
                        identity = identity,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o8 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o8();
                    _socket_.Get(_onReturn_, 8 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o8> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> flushFile(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, bool isDiskFile)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o9> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o9>.Get();
            if (_wait_ != null)
            {

                flushFile(identity, isDiskFile, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o9> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o9 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void flushFile(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, bool isDiskFile, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o9>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i9 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i9
                    {
                        identity = identity,
                        isDiskFile = isDiskFile,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o9 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o9();
                    _socket_.Get(_onReturn_, 9 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o9> { IsReturn = false });
        }
        public void loadHeader(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer>> _onReturn_)
        {
            Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o10>> _onOutput_ = null;
            _onOutput_ = fastCSharp.code.cSharp.asynchronousMethod.callReturn<fastCSharp.code.cSharp.tcpBase.subByteArrayBuffer, fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o10>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                loadHeader(identity, _onOutput_, false);
            }
        }
        private void loadHeader(fastCSharp.memoryDatabase.physicalServer.timeIdentity identity, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o10>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i10 _inputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._i10
                    {
                        identity = identity,
                    };

                    fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o10 _outputParameter_ = new fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o10();
                    _socket_.Get(_onReturn_, 10 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.memoryDatabasePhysical/**/._o10> { IsReturn = false });
        }
    }
}
namespace fastCSharp.io
{
    public partial class fileBlockServer
    {
        internal static class tcpServer
        {
            public static bool verify(fastCSharp.io.fileBlockServer _value_, string value)
            {

                return _value_.verify(value);
            }
            public static void read(fastCSharp.io.fileBlockServer _value_, fastCSharp.io.fileBlockStream.index index, ref fastCSharp.code.cSharp.tcpBase.subByteArrayEvent buffer, System.Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>, bool> onReturn)
            {
                _value_.read(index, ref buffer, onReturn);
            }
            public static long write(fastCSharp.io.fileBlockServer _value_, fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream)
            {

                return _value_.write(dataStream);
            }
            public static void waitBuffer(fastCSharp.io.fileBlockServer _value_)
            {
                _value_.waitBuffer();
            }
            public static bool flush(fastCSharp.io.fileBlockServer _value_, bool isDiskFile)
            {

                return _value_.flush(isDiskFile);
            }
        }
    }
}
namespace fastCSharp.tcpServer
{

    /// <summary>
    /// TCP服务
    /// </summary>
    public partial class fileBlock : fastCSharp.net.tcp.commandServer
    {
        /// <summary>
        /// TCP服务目标对象
        /// </summary>
        private readonly fastCSharp.io.fileBlockServer _value_;
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        public fileBlock() : this(null, null) { }
        /// <summary>
        /// TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public fileBlock(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.io.fileBlockServer value)
            : base(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("fileBlock", typeof(fastCSharp.io.fileBlockServer)))
        {
            _value_ = value ?? new fastCSharp.io.fileBlockServer();
            setCommands(5);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3, 0);
            identityOnCommands[4 + 128].Set(_M4);
        }

        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o0 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s0 : fastCSharp.net.tcp.commandServer.serverCall<_s0, fastCSharp.io.fileBlockServer, _i0>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.io.fileBlockServer/**/.tcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o0> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s0>.Push(this);
            }
        }
        private void _M0(socket socket, subArray<byte> data)
        {
            try
            {
                _i0 inputParameter = new _i0();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public fastCSharp.io.fileBlockStream.index index;
            public fastCSharp.code.cSharp.tcpBase.subByteArrayEvent buffer;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o1 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>
        {
            public fastCSharp.code.cSharp.tcpBase.subByteArrayEvent buffer;
            public fastCSharp.code.cSharp.tcpBase.subByteArrayEvent Ret;
            public fastCSharp.code.cSharp.tcpBase.subByteArrayEvent Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        private void _M1(socket socket, subArray<byte> data)
        {
            try
            {
                _i1 inputParameter = new _i1();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>, bool> callbackReturn = fastCSharp.net.tcp.commandServer.socket.callback<_o1, fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>.Get(socket, new _o1());
                    if (callbackReturn != null)
                    {
                        fastCSharp.io.fileBlockServer/**/.tcpServer.read(_value_, inputParameter.index, ref inputParameter.buffer, callbackReturn);
                        return;
                    }
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o2 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<long>
        {
            public long Ret;
            public long Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s2 : fastCSharp.net.tcp.commandServer.serverCall<_s2, fastCSharp.io.fileBlockServer, _i2>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o2> get()
            {
                try
                {

                    long Return =
                    fastCSharp.io.fileBlockServer/**/.tcpServer.write(serverValue, inputParameter.dataStream);
                    return new _o2
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o2> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s2>.Push(this);
            }
        }
        private void _M2(socket socket, subArray<byte> data)
        {
            try
            {
                _i2 inputParameter = new _i2();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s2/**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        sealed class _s3 : fastCSharp.net.tcp.commandServer.serverCall<_s3, fastCSharp.io.fileBlockServer>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue get()
            {
                try
                {

                    fastCSharp.io.fileBlockServer/**/.tcpServer.waitBuffer(serverValue);
                    return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s3>.Push(this);
            }
        }
        private void _M3(socket socket, subArray<byte> data)
        {
            try
            {
                {
                    fastCSharp.threading.task.Tiny.Add(_s3/**/.Call(socket, _value_, socket.Identity));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _i4
        {
            public bool isDiskFile;
        }
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        internal struct _o4 : fastCSharp.code.cSharp.asynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;
            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        sealed class _s4 : fastCSharp.net.tcp.commandServer.serverCall<_s4, fastCSharp.io.fileBlockServer, _i4>
        {
            private fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o4> get()
            {
                try
                {

                    bool Return =
                    fastCSharp.io.fileBlockServer/**/.tcpServer.flush(serverValue, inputParameter.isDiskFile);
                    return new _o4
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, true);
                }
                return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<_o4> { IsReturn = false };
            }
            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                fastCSharp.typePool<_s4>.Push(this);
            }
        }
        private void _M4(socket socket, subArray<byte> data)
        {
            try
            {
                _i4 inputParameter = new _i4();
                if (fastCSharp.emit.dataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    fastCSharp.threading.task.Tiny.Add(_s4/**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                fastCSharp.log.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false });
        }
    }
}
namespace fastCSharp.tcpClient
{

    public class fileBlock : IDisposable
    {
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public fastCSharp.net.tcp.commandClient<fileBlock> _TcpClient_ { get; private set; }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        public fileBlock() : this(null, null) { }
        /// <summary>
        /// TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public fileBlock(fastCSharp.code.cSharp.tcpServer attribute, fastCSharp.code.cSharp.tcpBase.ITcpClientVerifyMethod<fileBlock> verifyMethod)
        {
            _TcpClient_ = new fastCSharp.net.tcp.commandClient<fileBlock>(attribute ?? fastCSharp.code.cSharp.tcpServer.GetConfig("fileBlock", typeof(fastCSharp.io.fileBlockServer)), 24, verifyMethod ?? new fastCSharp.net.tcp.verifyMethod(), this);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            fastCSharp.net.tcp.commandClient<fileBlock> client = _TcpClient_;
            _TcpClient_ = null;
            fastCSharp.pub.Dispose(ref client);
        }



        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> verify(string value)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.fileBlock/**/._o0> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.fileBlock/**/._o0>.Get();
            if (_wait_ != null)
            {

                verify(value, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o0> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.fileBlock/**/._o0 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void verify(string value, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.fileBlock/**/._i0 _inputParameter_ = new fastCSharp.tcpServer/**/.fileBlock/**/._i0
                    {
                        value = value,
                    };

                    fastCSharp.tcpServer/**/.fileBlock/**/._o0 _outputParameter_ = new fastCSharp.tcpServer/**/.fileBlock/**/._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o0> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent> read(fastCSharp.io.fileBlockStream.index index, ref fastCSharp.code.cSharp.tcpBase.subByteArrayEvent buffer)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.fileBlock/**/._o1> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.fileBlock/**/._o1>.Get();
            if (_wait_ != null)
            {

                read(index, ref buffer, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o1> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.fileBlock/**/._o1 _outputParameterValue_ = _outputParameter_.Value;
                    buffer = _outputParameterValue_.buffer;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent> { IsReturn = false };
        }
        private void read(fastCSharp.io.fileBlockStream.index index, ref fastCSharp.code.cSharp.tcpBase.subByteArrayEvent buffer, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o1>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.fileBlock/**/._i1 _inputParameter_ = new fastCSharp.tcpServer/**/.fileBlock/**/._i1
                    {
                        index = index,
                        buffer = buffer,
                    };

                    fastCSharp.tcpServer/**/.fileBlock/**/._o1 _outputParameter_ = new fastCSharp.tcpServer/**/.fileBlock/**/._o1();
                    _outputParameter_.buffer = _inputParameter_.buffer;
                    _socket_.Get(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o1> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<long> write(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.fileBlock/**/._o2> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.fileBlock/**/._o2>.Get();
            if (_wait_ != null)
            {

                write(dataStream, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o2> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.fileBlock/**/._o2 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<long> { IsReturn = false };
        }
        public void write(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<long>> _onReturn_)
        {
            Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o2>> _onOutput_ = null;
            _onOutput_ = fastCSharp.code.cSharp.asynchronousMethod.callReturn<long, fastCSharp.tcpServer/**/.fileBlock/**/._o2>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                write(dataStream, _onOutput_, false);
            }
        }
        private void write(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o2>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.fileBlock/**/._i2 _inputParameter_ = new fastCSharp.tcpServer/**/.fileBlock/**/._i2
                    {
                        dataStream = dataStream,
                    };

                    fastCSharp.tcpServer/**/.fileBlock/**/._o2 _outputParameter_ = new fastCSharp.tcpServer/**/.fileBlock/**/._o2();
                    _socket_.Get(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o2> { IsReturn = false });
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue waitBuffer()
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall.Get();
            if (_wait_ != null)
            {

                waitBuffer(_wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue { IsReturn = false };
        }
        public void waitBuffer(Action<bool> _onReturn_)
        {

            waitBuffer(_onReturn_, true);
        }
        private void waitBuffer(Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    _socket_.Call(_onReturn_, 3 + 128, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
        public fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> flush(bool isDiskFile)
        {
            fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.fileBlock/**/._o4> _wait_ = fastCSharp.code.cSharp.asynchronousMethod.waitCall<fastCSharp.tcpServer/**/.fileBlock/**/._o4>.Get();
            if (_wait_ != null)
            {

                flush(isDiskFile, _wait_.OnReturn, false);
                fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o4> _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {

                    fastCSharp.tcpServer/**/.fileBlock/**/._o4 _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new fastCSharp.code.cSharp.asynchronousMethod.returnValue<bool> { IsReturn = false };
        }
        private void flush(bool isDiskFile, Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o4>> _onReturn_, bool _isTask_)
        {
            try
            {
                fastCSharp.net.tcp.commandClient.streamCommandSocket _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {

                    fastCSharp.tcpServer/**/.fileBlock/**/._i4 _inputParameter_ = new fastCSharp.tcpServer/**/.fileBlock/**/._i4
                    {
                        isDiskFile = isDiskFile,
                    };

                    fastCSharp.tcpServer/**/.fileBlock/**/._o4 _outputParameter_ = new fastCSharp.tcpServer/**/.fileBlock/**/._o4();
                    _socket_.Get(_onReturn_, 4 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                fastCSharp.log.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(new fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.tcpServer/**/.fileBlock/**/._o4> { IsReturn = false });
        }
    }
}
