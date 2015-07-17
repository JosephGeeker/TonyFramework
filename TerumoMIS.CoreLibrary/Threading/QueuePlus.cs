﻿//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: QueuePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  QueuePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:32:03
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
    /// 任务队列
    /// </summary>
    public sealed class QueuePlus:TaskBasePlus
    {
        /// <summary>
        /// 当前执行任务集合
        /// </summary>
        private list<taskInfo> currentTasks = new list<taskInfo>();
        /// <summary>
        /// 执行任务
        /// </summary>
        private Action runHandle;
        /// <summary>
        /// 任务处理
        /// </summary>
        /// <param name="isDisposeWait">默认释放资源是否等待线程结束</param>
        /// <param name="threadPool">线程池</param>
        public queue(bool isDisposeWait = true, threadPool threadPool = null)
        {
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
                int threadCount = this.threadCount;
                try
                {
                    PushTasks.Add(task);
                    if (this.threadCount == 0)
                    {
                        this.threadCount = 1;
                        freeWaitHandle.Reset();
                    }
                }
                finally { taskLock = 0; }
                if (threadCount == 0)
                {
                    try
                    {
                        threadPool.FastStart(runHandle, null, null);
                        return true;
                    }
                    catch (Exception error)
                    {
                        interlocked.NoCheckCompareSetSleep0(ref taskLock);
                        this.threadCount = 0;
                        taskLock = 0;
                        log.Error.Add(error, null, false);
                    }
                }
                else return true;
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
        private void run()
        {
            do
            {
                interlocked.NoCheckCompareSetSleep0(ref taskLock);
                int taskCount = PushTasks.Count;
                if (taskCount == 0)
                {
                    threadCount = 0;
                    taskLock = 0;
                    freeWaitHandle.Set();
                    if (isStop != 0) freeWaitHandle.Close();
                    break;
                }
                list<taskInfo> runTasks = PushTasks;
                PushTasks = currentTasks;
                currentTasks = runTasks;
                taskLock = 0;
                taskInfo[] taskArray = runTasks.array;
                int index = 0;
                do
                {
                    taskArray[index++].RunClear();
                }
                while (index != taskCount);
                runTasks.Empty();
            }
            while (true);
        }
        /// <summary>
        /// 微型线程任务队列
        /// </summary>
        public static readonly queue Tiny = new queue();
        /// <summary>
        /// 默认任务队列
        /// </summary>
        public static readonly queue Default = new queue(true, threadPool.Default);
        static QueuePlus()
        {
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
