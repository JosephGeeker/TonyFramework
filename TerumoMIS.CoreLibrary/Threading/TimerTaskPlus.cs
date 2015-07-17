//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TimerTaskPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  TimerTaskPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:35:43
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Reflection;
using System.Timers;
using TerumoMIS.CoreLibrary.Config;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    ///     定时任务信息
    /// </summary>
    public sealed class TimerTaskPlus : IDisposable
    {
        /// <summary>
        ///     默认定时任务
        /// </summary>
        public static readonly TimerTaskPlus Default = new TimerTaskPlus(null);

        /// <summary>
        ///     任务处理线程池
        /// </summary>
        private readonly ThreadPoolPlus _threadPool;

        /// <summary>
        ///     线程池任务
        /// </summary>
        private readonly Action _threadTaskHandle;

        /// <summary>
        ///     最近时间
        /// </summary>
        private DateTime _nearTime = DateTime.MaxValue;

        /// <summary>
        ///     已排序任务
        /// </summary>
        private ArrayHeapPlus<DateTime, TaskInfoStruct> _taskHeap = new ArrayHeapPlus<DateTime, TaskInfoStruct>();

        /// <summary>
        ///     任务访问锁
        /// </summary>
        private int _taskLock;

        /// <summary>
        ///     定时器
        /// </summary>
        private Timer _timer = new Timer();

        static TimerTaskPlus()
        {
            if (AppSettingPlus.IsCheckMemory) CheckMemoryPlus.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     定时任务信息
        /// </summary>
        /// <param name="threadPool">任务处理线程池</param>
        public TimerTaskPlus(ThreadPoolPlus threadPool)
        {
            _threadPool = threadPool ?? threadPool.TinyPool;
            _timer.Elapsed += OnTimer;
            _timer.AutoReset = false;
            _threadTaskHandle = ThreadTask;
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            _taskHeap = null;
        }

        /// <summary>
        ///     添加新任务
        /// </summary>
        /// <param name="task">任务信息</param>
        /// <param name="runTime">执行时间</param>
        private void AddBase(TaskInfoStruct task, DateTime runTime)
        {
            var isThread = false;
            InterlockedPlus.NoCheckCompareSetSleep0(ref _taskLock);
            try
            {
                _taskHeap.Push(runTime, task);
                if (runTime < _nearTime)
                {
                    _timer.Stop();
                    _nearTime = runTime;
                    var time = (runTime - DatePlus.Now).TotalMilliseconds;
                    if (time > 0)
                    {
                        _timer.Interval = time;
                        _timer.Start();
                    }
                    else isThread = true;
                }
            }
            finally
            {
                _taskLock = 0;
            }
            if (isThread) _threadPool.FastStart(_threadTaskHandle, null, null);
        }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <param name="run">任务执行委托</param>
        /// <param name="runTime">执行时间</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        public void Add(Action run, DateTime runTime, Action<Exception> onError = null)
        {
            if (run != null)
            {
                if (runTime > DatePlus.Now) AddBase(new TaskInfoStruct {Run = run, OnError = onError}, runTime);
                else _threadPool.FastStart(run, null, onError);
            }
        }

        /// <summary>
        ///     添加任务
        /// </summary>
        /// <typeparam name="TParameterType">执行参数类型</typeparam>
        /// <param name="run">任务执行委托</param>
        /// <param name="parameter">执行参数</param>
        /// <param name="runTime">执行时间</param>
        /// <param name="onError">任务执行出错委托,停止任务参数null</param>
        public void Add<TParameterType>
            (Action<TParameterType> run, TParameterType parameter, DateTime runTime, Action<Exception> onError)
        {
            if (run != null)
            {
                if (runTime > DatePlus.Now)
                    AddBase(
                        new TaskInfoStruct {Run = RunPlus<TParameterType>.Create(run, parameter), OnError = onError},
                        runTime);
                else _threadPool.FastStart(run, parameter, null, onError);
            }
        }

        /// <summary>
        ///     线程池任务
        /// </summary>
        private void ThreadTask()
        {
            var now = DatePlus.Now;
            InterlockedPlus.NoCheckCompareSetSleep0(ref _taskLock);
            try
            {
                while (_taskHeap.Count != 0)
                {
                    var task = _taskHeap.UnsafeTop();
                    if (task.Key <= now)
                    {
                        _threadPool.FastStart(task.Value.Run, null, task.Value.OnError);
                        _taskHeap.RemoveTop();
                    }
                    else
                    {
                        _nearTime = task.Key;
                        _timer.Interval = Math.Max((task.Key - now).TotalMilliseconds, 1);
                        _timer.Start();
                        break;
                    }
                }
                if (_taskHeap.Count == 0) _nearTime = DateTime.MaxValue;
            }
            catch (Exception error)
            {
                LogPlus.Error.Add(error, null, false);
                _timer.Interval = 1;
                _timer.Start();
            }
            finally
            {
                _taskLock = 0;
            }
        }

        /// <summary>
        ///     触发定时任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            ThreadTask();
        }

        /// <summary>
        ///     任务信息
        /// </summary>
        private struct TaskInfoStruct
        {
            /// <summary>
            ///     错误处理
            /// </summary>
            public Action<Exception> OnError;

            /// <summary>
            ///     任务委托
            /// </summary>
            public Action Run;
        }
    }
}