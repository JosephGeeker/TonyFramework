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
using System.Reflection;
using System.Threading;
using TerumoMIS.CoreLibrary.Config;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    ///     任务处理基类
    /// </summary>
    public abstract class TaskBasePlus : IDisposable
    {
        /// <summary>
        ///     等待空闲事件
        /// </summary>
        protected readonly EventWaitHandle FreeWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset, null);

        /// <summary>
        ///     默认释放资源是否等待线程结束
        /// </summary>
        protected bool IsDisposeWait;

        /// <summary>
        ///     是否停止任务
        /// </summary>
        protected byte IsStop;

        /// <summary>
        ///     新任务集合
        /// </summary>
        internal ListPlus<TaskInfoStruct> PushTasks = new ListPlus<TaskInfoStruct>();

        /// <summary>
        ///     任务访问锁
        /// </summary>
        protected int TaskLock;

        /// <summary>
        ///     线程数量
        /// </summary>
        protected int ThreadCount;

        /// <summary>
        ///     线程池
        /// </summary>
        protected ThreadPoolPlus ThreadPool;

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            DomainUnloadPlus.Remove(Dispose, false);
            Dispose(IsDisposeWait);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <param name="isWait">是否等待线程结束</param>
        public void Dispose(bool isWait)
        {
            InterlockedPlus.NoCheckCompareSetSleep0(ref TaskLock);
            var threadCount = ThreadCount | PushTasks.Count;
            IsStop = 1;
            TaskLock = 0;
            if (isWait && threadCount != 0)
            {
                FreeWaitHandle.WaitOne();
                FreeWaitHandle.Close();
            }
        }

        /// <summary>
        ///     单线程添加任务后，等待所有线程空闲
        /// </summary>
        public void WaitFree()
        {
            InterlockedPlus.NoCheckCompareSetSleep0(ref TaskLock);
            var threadCount = ThreadCount | PushTasks.Count;
            TaskLock = 0;
            if (threadCount != 0) FreeWaitHandle.WaitOne();
        }
    }

    /// <summary>
    ///     任务处理类(适用于短小任务，因为处理阻塞)
    /// </summary>
    public sealed class TaskPlus : TaskBasePlus
    {
        /// <summary>
        ///     微型线程任务
        /// </summary>
        public static readonly TaskPlus Tiny = new TaskPlus(Config.PubPlus.Default.TinyThreadCount);

        /// <summary>
        ///     默认任务
        /// </summary>
        public static readonly TaskPlus Default = new TaskPlus(Config.PubPlus.Default.TaskThreadCount, true,
            ThreadPoolPlus.Default);

        /// <summary>
        ///     执行任务
        /// </summary>
        private readonly Action<TaskInfoStruct> _runHandle;

        static TaskPlus()
        {
            if (AppSettingPlus.IsCheckMemory) CheckMemoryPlus.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     任务处理
        /// </summary>
        /// <param name="count">线程数</param>
        /// <param name="isDisposeWait">默认释放资源是否等待线程结束</param>
        /// <param name="threadPool">线程池</param>
        public TaskPlus(int count, bool isDisposeWait = true, ThreadPoolPlus threadPool = null)
        {
            if (count <= 0 || count > Config.PubPlus.Default.TaskMaxThreadCount)
                LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            MaxThreadCount = count;
            _runHandle = Run;
            IsDisposeWait = isDisposeWait;
            ThreadPool = threadPool ?? ThreadPoolPlus.TinyPool;
            DomainUnloadPlus.Add(Dispose, false);
        }

        /// <summary>
        ///     最大线程数量
        /// </summary>
        public int MaxThreadCount { get; private set; }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <param name="task">任务信息</param>
        /// <returns>任务添加是否成功</returns>
        internal bool Add(TaskInfoStruct task)
        {
            InterlockedPlus.NoCheckCompareSetSleep0(ref TaskLock);
            if (IsStop == 0)
            {
                if (ThreadCount == MaxThreadCount)
                {
                    try
                    {
                        PushTasks.Add(task);
                    }
                    finally
                    {
                        TaskLock = 0;
                    }
                    return true;
                }
                if (ThreadCount == 0) FreeWaitHandle.Reset();
                ++ThreadCount;
                TaskLock = 0;
                try
                {
                    ThreadPool.FastStart(_runHandle, task, null, null);
                    return true;
                }
                catch (Exception error)
                {
                    InterlockedPlus.NoCheckCompareSetSleep0(ref TaskLock);
                    var count = --ThreadCount | PushTasks.Count;
                    TaskLock = 0;
                    if (count == 0) FreeWaitHandle.Set();
                    LogPlus.Error.Add(error, null, false);
                }
            }
            else TaskLock = 0;
            return false;
        }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <param name="run">任务执行委托</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        /// <returns>任务添加是否成功</returns>
        public bool Add(Action run, Action<Exception> onError = null)
        {
            return run != null && Add(new TaskInfoStruct {Call = run, OnError = onError});
        }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <typeparam name="TParameterType">执行参数类型</typeparam>
        /// <param name="run">任务执行委托</param>
        /// <param name="parameter">执行参数</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        /// <returns>任务添加是否成功</returns>
        public bool Add<TParameterType>(Action<TParameterType> run, TParameterType parameter,
            Action<Exception> onError = null)
        {
            return run != null &&
                   Add(new TaskInfoStruct {Call = RunPlus<TParameterType>.Create(run, parameter), OnError = onError});
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        /// <param name="task">任务信息</param>
        private void Run(TaskInfoStruct task)
        {
            task.Run();
            do
            {
                InterlockedPlus.NoCheckCompareSetSleep0(ref TaskLock);
                if (PushTasks.Count == 0)
                {
                    var threadCount = --ThreadCount;
                    TaskLock = 0;
                    if (threadCount == 0)
                    {
                        FreeWaitHandle.Set();
                        if (IsStop != 0) FreeWaitHandle.Close();
                    }
                    break;
                }
                task = PushTasks.Unsafer.PopReset();
                TaskLock = 0;
                task.Run();
            } while (true);
        }
    }
}