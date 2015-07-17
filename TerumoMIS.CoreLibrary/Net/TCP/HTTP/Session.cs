//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: Session
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  Session
//	User name:  C1400008
//	Location Time: 2015/7/16 15:41:27
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
    /// 会话标识接口
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// 删除Session
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        void Remove(uint128 sessionId);
        /// <summary>
        /// 设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        uint128 Set(uint128 sessionId, object value);
        /// <summary>
        /// 获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        object Get(uint128 sessionId, object nullValue);
    }
    /// <summary>
    /// 会话标识接口
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public interface ISession<valueType> : ISession
    {
        /// <summary>
        /// 设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        uint128 Set(uint128 sessionId, valueType value);
        /// <summary>
        /// 获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        valueType Get(uint128 sessionId, valueType nullValue);
    }
    /// <summary>
    /// 会话标识服务客户端接口
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public interface ISessionClient<valueType>
    {
        /// <summary>
        /// 设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        asynchronousMethod.returnValue<uint128> Set(uint128 sessionId, valueType value);
        /// <summary>
        /// 获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>是否存在返回值</returns>
        asynchronousMethod.returnValue<bool> tryGet(uint128 sessionId, out valueType value);
        /// <summary>
        /// 删除Session
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        asynchronousMethod.returnValue Remove(uint128 sessionId);
    }
    /// <summary>
    /// 会话标识
    /// </summary>
    public abstract unsafe class session
    {
        /// <summary>
        /// 随机数高位
        /// </summary>
        private static ulong highRandom = fastCSharp.random.Default.SecureNextULong();
        /// <summary>
        /// 超时时钟周期
        /// </summary>
        protected long timeoutTicks;
        /// <summary>
        /// 超时刷新时钟周期
        /// </summary>
        protected long refreshTicks;
        /// <summary>
        /// 超时检测
        /// </summary>
        protected Action refresh;
        /// <summary>
        /// Session集合访问锁
        /// </summary>
        protected int sessionLock;
        /// <summary>
        /// 会话标识
        /// </summary>
        protected session() { }
        /// <summary>
        /// 会话标识
        /// </summary>
        /// <param name="timeoutMinutes">超时分钟数</param>
        /// <param name="refreshMinutes">超时刷新分钟数</param>
        protected session(int timeoutMinutes, int refreshMinutes)
        {
            timeoutTicks = new TimeSpan(0, timeoutMinutes, 0).Ticks;
            refreshTicks = new TimeSpan(0, refreshMinutes, 0).Ticks;
            timerTask.Default.Add(refresh = refreshTimeout, date.NowSecond.AddTicks(refreshTicks), null);
        }
        /// <summary>
        /// 超时检测
        /// </summary>
        protected virtual void refreshTimeout() { }
        /// <summary>
        /// Session服务端注册验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024)]
        protected virtual bool verify(string value)
        {
            if (fastCSharp.config.http.Default.SessionVerify == null && !fastCSharp.config.pub.Default.IsDebug)
            {
                log.Error.Add("Session服务注册验证数据不能为空", false, true);
                return false;
            }
            return fastCSharp.config.http.Default.SessionVerify == value;
        }
        /// <summary>
        /// 新建一个会话标识
        /// </summary>
        /// <returns>会话标识</returns>
        protected unsafe static uint128 newSessionId()
        {
            ulong low = fastCSharp.random.Default.SecureNextULongNotZero();
            highRandom ^= low;
            return new uint128 { Low = low, High = highRandom = (highRandom << 11) | (highRandom >> 53) };
        }
        /// <summary>
        /// 从Cookie
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static unsafe uint128 FromCookie(subArray<byte> data)
        {
            if (data.Count == 32)
            {
                uint128 sessionId = new uint128();
                fixed (byte* dataFixed = data.array) sessionId.ParseHex(dataFixed + data.StartIndex);
                return sessionId;
            }
            return new uint128 { High = 1 };
        }
    }
    /// <summary>
    /// 会话标识
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public class session<valueType> : session, ISession<valueType>
    {
        /// <summary>
        /// Session值
        /// </summary>
        private struct value
        {
            /// <summary>
            /// 超时时间
            /// </summary>
            public DateTime Timeout;
            /// <summary>
            /// Session值集合
            /// </summary>
            public valueType Value;
        }
        /// <summary>
        /// Session集合
        /// </summary>
        private Dictionary<uint128, value> values = dictionary.CreateUInt128<value>();
        /// <summary>
        /// 超时检测Session集合
        /// </summary>
        private uint128[] refreshValues = new uint128[256];
        /// <summary>
        /// 会话标识
        /// </summary>
        public session() : base(fastCSharp.config.http.Default.SessionMinutes, fastCSharp.config.http.Default.SessionRefreshMinutes) { }
        /// <summary>
        /// 会话标识
        /// </summary>
        /// <param name="timeoutMinutes">超时分钟数</param>
        /// <param name="refreshMinutes">超时刷新分钟数</param>
        public session(int timeoutMinutes, int refreshMinutes) : base(timeoutMinutes, refreshMinutes) { }
        /// <summary>
        /// 超时检测
        /// </summary>
        protected unsafe override void refreshTimeout()
        {
            DateTime time = date.NowSecond;
            interlocked.NoCheckCompareSetSleep0(ref sessionLock);
            try
            {
                if (refreshValues.Length < this.values.Count) refreshValues = new uint128[Math.Max(refreshValues.Length << 1, this.values.Count)];
                fixed (uint128* refreshFixed = refreshValues)
                {
                    uint128* write = refreshFixed;
                    foreach (KeyValuePair<uint128, value> values in this.values)
                    {
                        if (time >= values.Value.Timeout) *write++ = values.Key;
                    }
                    while (write != refreshFixed) this.values.Remove(*--write);
                }
            }
            finally
            {
                sessionLock = 0;
                timerTask.Default.Add(refresh, date.NowSecond.AddTicks(refreshTicks), null);
            }
        }
        /// <summary>
        /// 设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        public uint128 Set(uint128 sessionId, object value)
        {
            return Set(sessionId, (valueType)value);
        }
        /// <summary>
        /// 设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        public unsafe uint128 Set(uint128 sessionId, valueType value)
        {
            DateTime timeout = date.NowSecond.AddTicks(timeoutTicks);
            if (sessionId.Low == 0)
            {
                do
                {
                    sessionId = newSessionId();
                    interlocked.NoCheckCompareSetSleep0(ref sessionLock);
                    if (this.values.ContainsKey(sessionId)) sessionLock = 0;
                    else
                    {
                        try
                        {
                            this.values.Add(sessionId, new value { Timeout = timeout, Value = value });
                            break;
                        }
                        finally { sessionLock = 0; }
                    }
                }
                while (true);
            }
            else
            {
                interlocked.NoCheckCompareSetSleep0(ref sessionLock);
                try
                {
                    this.values[sessionId] = new value { Timeout = timeout, Value = value };
                }
                finally { sessionLock = 0; }
            }
            return sessionId;
        }
        /// <summary>
        /// 获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        public object Get(uint128 sessionId, object nullValue)
        {
            valueType value;
            return tryGet(sessionId, out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        public valueType Get(uint128 sessionId, valueType nullValue)
        {
            valueType value;
            return tryGet(sessionId, out value) ? value : nullValue;
        }
        /// <summary>
        /// 获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>是否存在返回值</returns>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        protected bool tryGet(uint128 sessionId, out valueType value)
        {
            DateTime timeout = date.NowSecond.AddTicks(timeoutTicks);
            value session;
            interlocked.NoCheckCompareSetSleep0(ref sessionLock);
            if (this.values.TryGetValue(sessionId, out session))
            {
                sessionLock = 0;
                value = session.Value;
                return true;
            }
            //try
            //{
            //    this.values[sessionId] = new value { Timeout = timeout, Value = session.Value };
            //}
            //finally { sessionLock = 0; }
            sessionLock = 0;
            value = session.Value;
            return true;
        }
        /// <summary>
        /// 删除Session
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false)]
        public void Remove(uint128 sessionId)
        {
            interlocked.NoCheckCompareSetSleep0(ref sessionLock);
            try
            {
                this.values.Remove(sessionId);
            }
            finally { sessionLock = 0; }
        }
    }
    /// <summary>
    /// 会话标识服务客户端
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public sealed class sessionClient<valueType> : session, ISession<valueType>
    {
        /// <summary>
        /// 会话标识服务客户端
        /// </summary>
        private ISessionClient<valueType> client;
        /// <summary>
        /// 会话标识服务客户端
        /// </summary>
        /// <param name="client">会话标识服务客户端</param>
        public sessionClient(ISessionClient<valueType> client)
        {
            this.client = client;
        }
        /// <summary>
        /// 设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        public uint128 Set(uint128 sessionId, object value)
        {
            return Set(sessionId, (valueType)value);
        }
        /// <summary>
        /// 设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        public uint128 Set(uint128 sessionId, valueType value)
        {
            return client.Set(sessionId, value);
        }
        /// <summary>
        /// 获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        public object Get(uint128 sessionId, object nullValue)
        {
            valueType value;
            return client.tryGet(sessionId, out value).Value ? value : nullValue;
        }
        /// <summary>
        /// 获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        public valueType Get(uint128 sessionId, valueType nullValue)
        {
            valueType value;
            return client.tryGet(sessionId, out value).Value ? value : nullValue;
        }
        /// <summary>
        /// 删除Session
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        public void Remove(uint128 sessionId)
        {
            client.Remove(sessionId);
        }
    }
}
