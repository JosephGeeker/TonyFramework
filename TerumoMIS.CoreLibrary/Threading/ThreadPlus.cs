//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ThreadPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  ThreadPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:33:59
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

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    /// 线程池线程
    /// </summary>
    internal sealed class ThreadPlus
    {
        /// <summary>
        /// 线程池线程集合
        /// </summary>
        private static readonly HashSet<Thread> threads = hashSet.CreateOnly<Thread>();
        /// <summary>
        /// 线程池线程集合访问锁
        /// </summary>
        private static int threadLock;
        /// <summary>
        /// 线程池线程默认堆栈帧数
        /// </summary>
        private static int defaultFrameCount;
        /// <summary>
        /// 活动的线程池线程集合
        /// </summary>
        public static subArray<StackTrace> StackTraces
        {
            get
            {
                Thread[] array;
                interlocked.NoCheckCompareSetSleep0(ref threadLock);
                try
                {
                    array = threads.getArray();
                }
                finally { threadLock = 0; }
                subArray<StackTrace> stacks = new subArray<StackTrace>();
                int currentId = Thread.CurrentThread.ManagedThreadId;
                foreach (Thread thread in array)
                {
                    if (thread.ManagedThreadId != currentId)
                    {
                        bool isSuspend = false;
                        try
                        {
                            if ((thread.ThreadState | System.Threading.ThreadState.Suspended) != 0)
                            {
#pragma warning disable 618
                                thread.Suspend();
#pragma warning restore 618
                                isSuspend = true;
                            }
                            StackTrace stack = new StackTrace(thread, true);
                            if (stack.FrameCount != defaultFrameCount) stacks.Add(stack);
                        }
                        catch (Exception error)
                        {
                            log.Default.Add(error, null, false);
                        }
                        finally
                        {
#pragma warning disable 618
                            if (isSuspend) thread.Resume();
#pragma warning restore 618
                        }
                    }
                }
                return stacks;
            }
        }
        /// <summary>
        /// 线程池
        /// </summary>
        private readonly threadPool threadPool;
        /// <summary>
        /// 线程句柄
        /// </summary>
        private readonly Thread threadHandle;
        /// <summary>
        /// 等待事件
        /// </summary>
        private readonly EventWaitHandle waitHandle;
        /// <summary>
        /// 线程ID
        /// </summary>
        public int ManagedThreadId
        {
            get { return threadHandle.ManagedThreadId; }
        }
        /// <summary>
        /// 任务委托
        /// </summary>
        private Action task;
        /// <summary>
        /// 应用程序退出处理
        /// </summary>
        private Action domainUnload;
        /// <summary>
        /// 应用程序退出处理
        /// </summary>
        private Action<Exception> onError;
        /// <summary>
        /// 线程池线程
        /// </summary>
        /// <param name="threadPool">线程池</param>
        /// <param name="stackSize">堆栈大小</param>
        /// <param name="handle">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal thread(threadPool threadPool, int stackSize, Action task, Action domainUnload, Action<Exception> onError)
        {
            this.task = task;
            this.domainUnload = domainUnload;
            this.onError = onError;
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);
            this.threadPool = threadPool;
            threadHandle = new Thread(run, stackSize);
            threadHandle.IsBackground = true;
            interlocked.NoCheckCompareSetSleep0(ref threadLock);
            try
            {
                threads.Add(threadHandle);
            }
            finally
            {
                threadLock = 0;
                threadHandle.Start();
            }
        }
        /// <summary>
        /// 运行线程
        /// </summary>
        private void run()
        {
            if (defaultFrameCount == 0) defaultFrameCount = new System.Diagnostics.StackTrace(threadHandle, false).FrameCount;
            do
            {
                if (domainUnload != null) fastCSharp.domainUnload.Add(domainUnload);
                try
                {
                    task();
                }
                catch (Exception error)
                {
                    if (onError != null)
                    {
                        try
                        {
                            onError(error);
                        }
                        catch (Exception error1)
                        {
                            log.Error.Add(error1, null, false);
                        }
                    }
                    else log.Error.Add(error, null, false);
                }
                finally
                {
                    task = null;
                    onError = null;
                    if (domainUnload != null)
                    {
                        fastCSharp.domainUnload.Remove(domainUnload, false);
                        domainUnload = null;
                    }
                }
                threadPool.Push(this);
                waitHandle.WaitOne();
            }
            while (task != null);
            interlocked.NoCheckCompareSetSleep0(ref threadLock);
            try
            {
                threads.Remove(threadHandle);
            }
            finally
            {
                threadLock = 0;
                waitHandle.Close();
            }
        }
        /// <summary>
        /// 结束线程
        /// </summary>
        internal void Stop()
        {
            waitHandle.Set();
        }
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="handle">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void RunTask(Action task, Action domainUnload, Action<Exception> onError)
        {
            this.domainUnload = domainUnload;
            this.onError = onError;
            this.task = task;
            waitHandle.Set();
        }
    }
}
