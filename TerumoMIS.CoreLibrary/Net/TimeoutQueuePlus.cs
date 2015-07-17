//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TimeoutQueuePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net
//	File Name:  TimeoutQueuePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:01:08
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

namespace TerumoMIS.CoreLibrary.Net
{
    /// <summary>
    /// 超时队列
    /// </summary>
    public sealed class TimeoutQueuePlus
    {
        /// <summary>
        /// 回调信息
        /// </summary>
        private struct callback
        {
            /// <summary>
            /// 超时时间
            /// </summary>
            public DateTime Timeout;
            /// <summary>
            /// 套接字
            /// </summary>
            private Socket socket;
            /// <summary>
            /// 是否超时判断
            /// </summary>
            private Func<int, bool> isTimeout;
            /// <summary>
            /// 超时判断标识
            /// </summary>
            private int identity;
            /// <summary>
            /// 设置回调信息
            /// </summary>
            /// <param name="timeout">超时时间</param>
            /// <param name="socket">套接字</param>
            /// <param name="isTimeout">是否超时判断</param>
            /// <param name="idnetity">超时判断标识</param>
            public void Set(DateTime timeout, Socket socket, Func<int, bool> isTimeout, int idnetity)
            {
                Timeout = timeout;
                this.socket = socket;
                this.isTimeout = isTimeout;
                this.identity = idnetity;
            }
            /// <summary>
            /// 设置回调信息
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="isTimeout">是否超时判断</param>
            /// <param name="idnetity">超时判断标识</param>
            private void set(Socket socket, Func<int, bool> isTimeout, int idnetity)
            {
                this.socket = socket;
                this.isTimeout = isTimeout;
                this.identity = idnetity;
            }
            /// <summary>
            /// 超时检测
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool Check(ref callback value)
            {
                if (date.NowSecond >= Timeout)
                {
                    value.set(socket, isTimeout, identity);
                    Clear();
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 超时检测
            /// </summary>
            public void Check()
            {
                if (isTimeout(identity)) socket.Shutdown(SocketShutdown.Both);
                Clear();
            }
            /// <summary>
            /// 清除回调信息
            /// </summary>
            public void Clear()
            {
                socket = null;
                isTimeout = null;
            }
            /// <summary>
            /// 修改超时时间
            /// </summary>
            /// <param name="ticks"></param>
            public void AddTimeout(long ticks)
            {
                Timeout = Timeout.AddTicks(ticks);
            }
        }
        /// <summary>
        /// 超时时钟周期
        /// </summary>
        private long timeoutTicks;
        /// <summary>
        /// 超时时钟周期
        /// </summary>
        public long CallbackTimeoutTicks
        {
            get { return timeoutTicks + date.SecondTicks; }
        }
        /// <summary>
        /// 回调信息集合
        /// </summary>
        private callback[] sockets;
        /// <summary>
        /// 当前处理超时集合
        /// </summary>
        private readonly callback[] timeoutSockets;
        /// <summary>
        /// 超时检测
        /// </summary>
        private readonly Action checkHandle;
        /// <summary>
        /// 回调信息集合起始位置
        /// </summary>
        private int startIndex;
        /// <summary>
        /// 回调信息集合结束位置
        /// </summary>
        private int endIndex;
        /// <summary>
        /// 回调信息集合访问锁
        /// </summary>
        private int socketLock;
        /// <summary>
        /// 是否正在处理超时集合
        /// </summary>
        private int isTask;
        /// <summary>
        /// 超时秒数
        /// </summary>
        public int TimeoutSeconds
        {
            set
            {
                long newTicks = date.SecondTicks * value;
                interlocked.NoCheckCompareSetSleep0(ref socketLock);
                long ticks = newTicks - timeoutTicks;
                timeoutTicks = newTicks;
                for (int index = startIndex, end = startIndex > endIndex ? sockets.Length : endIndex; index != end; sockets[index++].AddTimeout(newTicks)) ;
                if (startIndex > endIndex)
                {
                    for (int index = 0; index != endIndex; sockets[index++].AddTimeout(newTicks)) ;
                }
                socketLock = 0;
            }
        }
        /// <summary>
        /// 超时队列
        /// </summary>
        /// <param name="seconds">超时秒数</param>
        public timeoutQueue(int seconds)
        {
            timeoutTicks = date.SecondTicks * seconds;
            sockets = new callback[256];
            timeoutSockets = new callback[256];
            checkHandle = check;
        }
        /// <summary>
        /// 添加超时回调信息
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="isTimeout">是否超时判断</param>
        /// <param name="idnetity">超时判断标识</param>
        public void Add(Socket socket, Func<int, bool> isTimeout, int idnetity = 0)
        {
            int isError = 0, isTask;
            DateTime timeout = date.NowSecond.AddTicks(timeoutTicks);
            interlocked.NoCheckCompareSetSleep0(ref socketLock);
            isTask = this.isTask;
            sockets[endIndex].Set(timeout, socket, isTimeout, idnetity);
            if (++endIndex == sockets.Length) endIndex = 0;
            this.isTask = 1;
            if (endIndex == startIndex)
            {
                try
                {
                    callback[] newSockets = new callback[sockets.Length << 1];
                    if (startIndex == 0) Array.Copy(sockets, 0, newSockets, 0, sockets.Length);
                    else
                    {
                        Array.Copy(sockets, startIndex, newSockets, 0, idnetity = sockets.Length - startIndex);
                        Array.Copy(sockets, 0, newSockets, idnetity, startIndex);
                    }
                    endIndex = sockets.Length;
                    startIndex = 0;
                    sockets = newSockets;
                }
                catch (Exception error)
                {
                    if (endIndex == 0) endIndex = sockets.Length - 1;
                    else --endIndex;
                    isError = 1;
                    this.isTask = isTask;
                    sockets[endIndex].Clear();
                    log.Error.Add(error, null, false);
                }
                finally { socketLock = 0; }
            }
            else socketLock = 0;
            if (isError == 0)
            {
                if (isTask == 0)
                {
                    DateTime maxTimeout = date.NowSecond.AddTicks(date.MinutesTicks);
                    fastCSharp.threading.timerTask.Default.Add(checkHandle, maxTimeout < timeout ? maxTimeout : timeout);
                }
            }
            else socket.Shutdown(SocketShutdown.Both);
        }
        /// <summary>
        /// 超时检测
        /// </summary>
        private void check()
        {
            int count = 0;
            do
            {
                interlocked.NoCheckCompareSetSleep0(ref socketLock);
                if (startIndex > endIndex)
                {
                    while (sockets[startIndex].Check(ref timeoutSockets[count]))
                    {
                        ++count;
                        if (++startIndex == sockets.Length)
                        {
                            startIndex = 0;
                            break;
                        }
                        if (count == timeoutSockets.Length) break;
                    }
                }
                else
                {
                    if (startIndex == endIndex)
                    {
                        isTask = 0;
                        socketLock = 0;
                        return;
                    }
                    while (sockets[startIndex].Check(ref timeoutSockets[count]))
                    {
                        ++count;
                        if (++startIndex == endIndex)
                        {
                            startIndex = endIndex = 0;
                            break;
                        }
                        if (count == timeoutSockets.Length) break;
                    }
                }
                socketLock = 0;
                if (count == 0)
                {
                    DateTime maxTimeout = date.NowSecond.AddTicks(date.MinutesTicks), timeout = sockets[startIndex].Timeout;
                    fastCSharp.threading.timerTask.Default.Add(checkHandle, maxTimeout < timeout ? maxTimeout : timeout);
                    return;
                }
                while (count != 0) timeoutSockets[--count].Check();
            }
            while (true);
        }
    }
}
