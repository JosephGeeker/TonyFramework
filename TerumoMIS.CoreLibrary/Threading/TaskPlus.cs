//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TaskPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  TaskPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:32:50
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
    /// 任务处理基类
    /// </summary>
    public abstract class taskBase : IDisposable
    {
        /// <summary>
        /// 线程池
        /// </summary>
        protected threadPool threadPool;
        /// <summary>
        /// 新任务集合
        /// </summary>
        internal list<taskInfo> PushTasks = new list<taskInfo>();
        /// <summary>
        /// 等待空闲事件
        /// </summary>
        protected readonly EventWaitHandle freeWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset, null);
        /// <summary>
        /// 任务访问锁
        /// </summary>
        protected int taskLock;
        /// <summary>
        /// 线程数量
        /// </summary>
        protected int threadCount;
        /// <summary>
        /// 默认释放资源是否等待线程结束
        /// </summary>
        protected bool isDisposeWait;
        /// <summary>
        /// 是否停止任务
        /// </summary>
        protected byte isStop;
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            fastCSharp.domainUnload.Remove(Dispose, false);
            Dispose(isDisposeWait);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="isWait">是否等待线程结束</param>
        public void Dispose(bool isWait)
        {
            interlocked.NoCheckCompareSetSleep0(ref taskLock);
            int threadCount = this.threadCount | PushTasks.Count;
            isStop = 1;
            taskLock = 0;
            if (isWait && threadCount != 0)
            {
                freeWaitHandle.WaitOne();
                freeWaitHandle.Close();
            }
        }
        /// <summary>
        /// 单线程添加任务后，等待所有线程空闲
        /// </summary>
        public void WaitFree()
        {
            interlocked.NoCheckCompareSetSleep0(ref taskLock);
            int threadCount = this.threadCount | PushTasks.Count;
            taskLock = 0;
            if (threadCount != 0) freeWaitHandle.WaitOne();
        }
    }
    /// <summary>
    /// 任务处理类(适用于短小任务，因为处理阻塞)
    /// </summary>
    public sealed class task : taskBase
    {
        /// <summary>
        /// 最大线程数量
        /// </summary>
        public int MaxThreadCount { get; private set; }
        /// <summary>
        /// 执行任务
        /// </summary>
        private Action<taskInfo> runHandle;
        /// <summary>
        /// 任务处理
        /// </summary>
        /// <param name="count">线程数</param>
        /// <param name="isDisposeWait">默认释放资源是否等待线程结束</param>
        /// <param name="threadPool">线程池</param>
        public task(int count, bool isDisposeWait = true, threadPool threadPool = null)
        {
            if (count <= 0 || count > config.pub.Default.TaskMaxThreadCount) fastCSharp.log.Error.Throw(log.exceptionType.IndexOutOfRange);
            MaxThreadCount = count;
            runHandle = run;
            this.isDisposeWait = isDisposeWait;
            this.threadPool = threadPool ?? fastCSharp.threading.threadPool.TinyPool;
            fastCSharp.domainUnload.Add(Dispose, false);
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task">任务信息</param>
        /// <returns>任务添加是否成功</returns>
        internal bool Add(taskInfo task)
        {
            interlocked.NoCheckCompareSetSleep0(ref taskLock);
            if (isStop == 0)
            {
                if (threadCount == MaxThreadCount)
                {
                    try
                    {
                        PushTasks.Add(task);
                    }
                    finally { taskLock = 0; }
                    return true;
                }
                if (threadCount == 0) freeWaitHandle.Reset();
                ++threadCount;
                taskLock = 0;
                try
                {
                    threadPool.FastStart(runHandle, task, null, null);
                    return true;
                }
                catch (Exception error)
                {
                    interlocked.NoCheckCompareSetSleep0(ref taskLock);
                    int count = --this.threadCount | PushTasks.Count;
                    taskLock = 0;
                    if (count == 0) freeWaitHandle.Set();
                    log.Error.Add(error, null, false);
                }
            }
            else taskLock = 0;
            return false;
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="run">任务执行委托</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        /// <returns>任务添加是否成功</returns>
        public bool Add(Action run, Action<Exception> onError = null)
        {
            return run != null && Add(new taskInfo { Call = run, OnError = onError });
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <typeparam name="parameterType">执行参数类型</typeparam>
        /// <param name="run">任务执行委托</param>
        /// <param name="parameter">执行参数</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        /// <returns>任务添加是否成功</returns>
        public bool Add<parameterType>(Action<parameterType> run, parameterType parameter, Action<Exception> onError = null)
        {
            return run != null && Add(new taskInfo { Call = run<parameterType>.Create(run, parameter), OnError = onError });
        }
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="task">任务信息</param>
        private void run(taskInfo task)
        {
            task.Run();
            do
            {
                interlocked.NoCheckCompareSetSleep0(ref taskLock);
                if (PushTasks.Count == 0)
                {
                    int threadCount = --this.threadCount;
                    taskLock = 0;
                    if (threadCount == 0)
                    {
                        freeWaitHandle.Set();
                        if (isStop != 0) freeWaitHandle.Close();
                    }
                    break;
                }
                task = PushTasks.Unsafer.PopReset();
                taskLock = 0;
                task.Run();
            }
            while (true);
        }
        /// <summary>
        /// 微型线程任务
        /// </summary>
        public static readonly task Tiny = new task(config.pub.Default.TinyThreadCount);
        /// <summary>
        /// 默认任务
        /// </summary>
        public static readonly task Default = new task(config.pub.Default.TaskThreadCount, true, threadPool.Default);
        static task()
        {
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
