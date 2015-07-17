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
using System.Diagnostics;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    ///     线程池线程
    /// </summary>
    internal sealed class ThreadPlus
    {
        /// <summary>
        ///     线程池线程集合
        /// </summary>
        private static readonly HashSet<Thread> Threads = HashSetPlus.CreateOnly<Thread>();

        /// <summary>
        ///     线程池线程集合访问锁
        /// </summary>
        private static int _threadLock;

        /// <summary>
        ///     线程池线程默认堆栈帧数
        /// </summary>
        private static int _defaultFrameCount;

        /// <summary>
        ///     线程句柄
        /// </summary>
        private readonly Thread _threadHandle;

        /// <summary>
        ///     线程池
        /// </summary>
        private readonly ThreadPoolPlus _threadPool;

        /// <summary>
        ///     等待事件
        /// </summary>
        private readonly EventWaitHandle _waitHandle;

        /// <summary>
        ///     应用程序退出处理
        /// </summary>
        private Action _domainUnload;

        /// <summary>
        ///     应用程序退出处理
        /// </summary>
        private Action<Exception> _onError;

        /// <summary>
        ///     任务委托
        /// </summary>
        private Action _task;

        /// <summary>
        ///     线程池线程
        /// </summary>
        /// <param name="threadPool">线程池</param>
        /// <param name="stackSize">堆栈大小</param>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal ThreadPlus(ThreadPoolPlus threadPool, int stackSize, Action task, Action domainUnload,
            Action<Exception> onError)
        {
            _task = task;
            _domainUnload = domainUnload;
            _onError = onError;
            _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);
            _threadPool = threadPool;
            _threadHandle = new Thread(Run, stackSize);
            _threadHandle.IsBackground = true;
            InterlockedPlus.NoCheckCompareSetSleep0(ref _threadLock);
            try
            {
                Threads.Add(_threadHandle);
            }
            finally
            {
                _threadLock = 0;
                _threadHandle.Start();
            }
        }

        /// <summary>
        ///     活动的线程池线程集合
        /// </summary>
        public static SubArrayStruct<StackTrace> StackTraces
        {
            get
            {
                Thread[] array;
                InterlockedPlus.NoCheckCompareSetSleep0(ref _threadLock);
                try
                {
                    array = Threads.getArray();
                }
                finally
                {
                    _threadLock = 0;
                }
                var stacks = new SubArrayStruct<StackTrace>();
                var currentId = Thread.CurrentThread.ManagedThreadId;
                foreach (var thread in array)
                {
                    if (thread.ManagedThreadId != currentId)
                    {
                        var isSuspend = false;
                        try
                        {
                            if ((thread.ThreadState | ThreadState.Suspended) != 0)
                            {
#pragma warning disable 618
                                thread.Suspend();
#pragma warning restore 618
                                isSuspend = true;
                            }
                            var stack = new StackTrace(thread, true);
                            if (stack.FrameCount != _defaultFrameCount) stacks.Add(stack);
                        }
                        catch (Exception error)
                        {
                            LogPlus.Default.Add(error, null, false);
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
        ///     线程ID
        /// </summary>
        public int ManagedThreadId
        {
            get { return _threadHandle.ManagedThreadId; }
        }

        /// <summary>
        ///     运行线程
        /// </summary>
        private void Run()
        {
#pragma warning disable 618
            if (_defaultFrameCount == 0) _defaultFrameCount = new StackTrace(_threadHandle, false).FrameCount;
#pragma warning restore 618
            do
            {
                if (_domainUnload != null) DomainUnloadPlus.Add(_domainUnload);
                try
                {
                    _task();
                }
                catch (Exception error)
                {
                    if (_onError != null)
                    {
                        try
                        {
                            _onError(error);
                        }
                        catch (Exception error1)
                        {
                            LogPlus.Error.Add(error1, null, false);
                        }
                    }
                    else LogPlus.Error.Add(error, null, false);
                }
                finally
                {
                    _task = null;
                    _onError = null;
                    if (_domainUnload != null)
                    {
                        DomainUnloadPlus.Remove(_domainUnload, false);
                        _domainUnload = null;
                    }
                }
                _threadPool.Push(this);
                _waitHandle.WaitOne();
            } while (_task != null);
            InterlockedPlus.NoCheckCompareSetSleep0(ref _threadLock);
            try
            {
                Threads.Remove(_threadHandle);
            }
            finally
            {
                _threadLock = 0;
                _waitHandle.Close();
            }
        }

        /// <summary>
        ///     结束线程
        /// </summary>
        internal void Stop()
        {
            _waitHandle.Set();
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void RunTask(Action task, Action domainUnload, Action<Exception> onError)
        {
            _domainUnload = domainUnload;
            _onError = onError;
            _task = task;
            _waitHandle.Set();
        }
    }
}