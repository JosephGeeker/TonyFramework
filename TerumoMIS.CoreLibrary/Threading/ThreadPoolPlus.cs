//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ThreadPoolPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  ThreadPoolPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:34:45
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
    ///     线程池
    /// </summary>
    public sealed class ThreadPoolPlus
    {
        /// <summary>
        ///     最低线程堆栈大小
        /// </summary>
        private const int MinStackSize = 128 << 10;

        /// <summary>
        ///     微型线程池,堆栈大小可能只有128K
        /// </summary>
        public static readonly ThreadPoolPlus TinyPool = new ThreadPoolPlus(AppSettingPlus.TinyThreadStackSize);

        /// <summary>
        ///     默认线程池
        /// </summary>
        public static readonly ThreadPoolPlus Default = AppSettingPlus.ThreadStackSize !=
                                                        AppSettingPlus.TinyThreadStackSize
            ? new ThreadPoolPlus(AppSettingPlus.ThreadStackSize)
            : TinyPool;

        /// <summary>
        ///     线程堆栈大小
        /// </summary>
        private readonly int _stackSize;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        ///     线程集合
        /// </summary>
        private ObjectPoolStruct<ThreadPlus> _threads = ObjectPoolStruct<ThreadPlus>.Create();

        static ThreadPoolPlus()
        {
            if (AppSettingPlus.IsCheckMemory) CheckMemoryPlus.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     线程池
        /// </summary>
        /// <param name="stackSize">线程堆栈大小</param>
        private ThreadPoolPlus(int stackSize = 1 << 20)
        {
            _stackSize = stackSize < MinStackSize ? MinStackSize : stackSize;
            DomainUnloadPlus.Add(Dispose);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        private void Dispose()
        {
            _isDisposed = true;
            DisposePool();
        }

        /// <summary>
        ///     释放线程池
        /// </summary>
        private void DisposePool()
        {
            foreach (var value in _threads.GetClear(0)) value.Value.Stop();
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        private void StartBase(Action task, Action domainUnload, Action<Exception> onError)
        {
            if (task == null) LogPlus.Error.Throw(null, "缺少 线程委托", false);
            if (_isDisposed) LogPlus.Default.Real("线程池已经被释放", true);
            else
            {
                var thread = _threads.Pop();
                // ReSharper disable once ObjectCreationAsStatement
                if (thread == null) new ThreadPlus(this, _stackSize, task, domainUnload, onError);
                else thread.RunTask(task, domainUnload, onError);
            }
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void FastStart(Action task, Action domainUnload, Action<Exception> onError)
        {
            var thread = _threads.Pop();
            // ReSharper disable once ObjectCreationAsStatement
            if (thread == null) new ThreadPlus(this, _stackSize, task, domainUnload, onError);
            else thread.RunTask(task, domainUnload, onError);
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="TParameterType">参数类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void FastStart<TParameterType>(Action<TParameterType> task, TParameterType parameter,
            Action domainUnload, Action<Exception> onError)
        {
            FastStart(RunPlus<TParameterType>.Create(task, parameter), domainUnload, onError);
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start(Action task, Action domainUnload = null, Action<Exception> onError = null)
        {
            if (task == null) LogPlus.Error.Throw(null, "缺少 线程委托", false);
            StartBase(task, domainUnload, onError);
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="TParameterType">参数类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start<TParameterType>
            (Action<TParameterType> task, TParameterType parameter, Action domainUnload = null,
                Action<Exception> onError = null)
        {
            if (task == null) LogPlus.Error.Throw(null, "缺少线程委托", false);
            StartBase(RunPlus<TParameterType>.Create(task, parameter), domainUnload, onError);
        }

        /// <summary>
        ///     获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="TParameterType">参数类型</typeparam>
        /// <typeparam name="TReturnType">返回值类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="onReturn">返回值执行委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start<TParameterType, TReturnType>(Func<TParameterType, TReturnType> task, TParameterType parameter,
            Action<TReturnType> onReturn, Action domainUnload = null, Action<Exception> onError = null)
        {
            if (task == null) LogPlus.Error.Throw(null, "缺少线程委托", false);
            StartBase(run<TParameterType, TReturnType>.Create(task, parameter, onReturn), domainUnload, onError);
        }

        /// <summary>
        ///     线程入池
        /// </summary>
        /// <param name="thread">线程池线程</param>
        internal void Push(ThreadPlus thread)
        {
            if (_isDisposed) thread.Stop();
            else
            {
                _threads.Push(thread);
                if (_isDisposed) DisposePool();
            }
        }

        /// <summary>
        ///     检测日志输出
        /// </summary>
        public static void CheckLogPlus()
        {
            var threads = ThreadPlus.StackTraces;
            LogPlus.Default.Add("活动线程数量 " + threads.Count);
            foreach (var value in threads)
            {
                try
                {
                    LogPlus.Default.Add(value.ToString());
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}