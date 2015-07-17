//==============================================================
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
using System.Reflection;
using TerumoMIS.CoreLibrary.Config;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    ///     任务队列
    /// </summary>
    public sealed class QueuePlus : TaskBasePlus
    {
        /// <summary>
        ///     微型线程任务队列
        /// </summary>
        public static readonly QueuePlus Tiny = new QueuePlus();

        /// <summary>
        ///     默认任务队列
        /// </summary>
        public static readonly QueuePlus Default = new QueuePlus(true, ThreadPoolPlus.Default);

        /// <summary>
        ///     执行任务
        /// </summary>
        private readonly Action _runHandle;

        /// <summary>
        ///     当前执行任务集合
        /// </summary>
        private ListPlus<TaskInfoStruct> _currentTasks = new ListPlus<TaskInfoStruct>();

        static QueuePlus()
        {
            if (AppSettingPlus.IsCheckMemory) CheckMemoryPlus.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     任务处理
        /// </summary>
        /// <param name="isDisposeWait">默认释放资源是否等待线程结束</param>
        /// <param name="threadPool">线程池</param>
        public QueuePlus(bool isDisposeWait = true, ThreadPoolPlus threadPool = null)
        {
            _runHandle = Run;
            IsDisposeWait = isDisposeWait;
            ThreadPool = threadPool ?? ThreadPoolPlus.TinyPool;
            DomainUnloadPlus.Add(Dispose, false);
        }

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
                var threadCount = ThreadCount;
                try
                {
                    PushTasks.Add(task);
                    if (ThreadCount == 0)
                    {
                        ThreadCount = 1;
                        FreeWaitHandle.Reset();
                    }
                }
                finally
                {
                    TaskLock = 0;
                }
                if (threadCount == 0)
                {
                    try
                    {
                        ThreadPoolPlus.FastStart(_runHandle, null, null);
                        return true;
                    }
                    catch (Exception error)
                    {
                        InterlockedPlus.NoCheckCompareSetSleep0(ref TaskLock);
                        ThreadCount = 0;
                        TaskLock = 0;
                        LogPlus.Error.Add(error, null, false);
                    }
                }
                else return true;
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
        private void Run()
        {
            do
            {
                InterlockedPlus.NoCheckCompareSetSleep0(ref TaskLock);
                var taskCount = PushTasks.Count;
                if (taskCount == 0)
                {
                    ThreadCount = 0;
                    TaskLock = 0;
                    FreeWaitHandle.Set();
                    if (IsStop != 0) FreeWaitHandle.Close();
                    break;
                }
                var runTasks = PushTasks;
                PushTasks = _currentTasks;
                _currentTasks = runTasks;
                TaskLock = 0;
                var taskArray = runTasks.Array;
                var index = 0;
                do
                {
                    taskArray[index++].RunClear();
                } while (index != taskCount);
                runTasks.Empty();
            } while (true);
        }
    }
}