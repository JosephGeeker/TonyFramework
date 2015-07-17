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
using System.Diagnostics;
using System.Reflection;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    /// 线程池
    /// </summary>
    public sealed class ThreadPoolPlus
    {
        /// <summary>
        /// 最低线程堆栈大小
        /// </summary>
        private const int minStackSize = 128 << 10;
        /// <summary>
        /// 线程堆栈大小
        /// </summary>
        private int stackSize;
        /// <summary>
        /// 线程集合
        /// </summary>
        private objectPool<thread> threads = objectPool<thread>.Create();
        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        private bool isDisposed;
        /// <summary>
        /// 线程池
        /// </summary>
        /// <param name="stackSize">线程堆栈大小</param>
        private threadPool(int stackSize = 1 << 20)
        {
            this.stackSize = stackSize < minStackSize ? minStackSize : stackSize;
            fastCSharp.domainUnload.Add(dispose);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        private void dispose()
        {
            isDisposed = true;
            disposePool();
        }
        /// <summary>
        /// 释放线程池
        /// </summary>
        private void disposePool()
        {
            foreach (array.value<thread> value in threads.GetClear(0)) value.Value.Stop();
        }
        /// <summary>
        /// 获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        private void start(Action task, Action domainUnload, Action<Exception> onError)
        {
            if (task == null) log.Error.Throw(null, "缺少 线程委托", false);
            if (isDisposed) log.Default.Real("线程池已经被释放", true, false);
            else
            {
                thread thread = threads.Pop();
                if (thread == null) new thread(this, stackSize, task, domainUnload, onError);
                else thread.RunTask(task, domainUnload, onError);
            }
        }
        /// <summary>
        /// 获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void FastStart(Action task, Action domainUnload, Action<Exception> onError)
        {
            thread thread = threads.Pop();
            if (thread == null) new thread(this, stackSize, task, domainUnload, onError);
            else thread.RunTask(task, domainUnload, onError);
        }
        /// <summary>
        /// 获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="parameterType">参数类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        internal void FastStart<parameterType>(Action<parameterType> task, parameterType parameter, Action domainUnload, Action<Exception> onError)
        {
            FastStart(run<parameterType>.Create(task, parameter), domainUnload, onError);
        }
        /// <summary>
        /// 获取一个线程并执行任务
        /// </summary>
        /// <param name="task">任务委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start(Action task, Action domainUnload = null, Action<Exception> onError = null)
        {
            if (task == null) log.Error.Throw(null, "缺少 线程委托", false);
            start(task, domainUnload, onError);
        }
        /// <summary>
        /// 获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="parameterType">参数类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start<parameterType>
            (Action<parameterType> task, parameterType parameter, Action domainUnload = null, Action<Exception> onError = null)
        {
            if (task == null) log.Error.Throw(null, "缺少 线程委托", false);
            start(run<parameterType>.Create(task, parameter), domainUnload, onError);
        }
        /// <summary>
        /// 获取一个线程并执行任务
        /// </summary>
        /// <typeparam name="parameterType">参数类型</typeparam>
        /// <typeparam name="returnType">返回值类型</typeparam>
        /// <param name="task">任务委托</param>
        /// <param name="parameter">线程参数</param>
        /// <param name="onReturn">返回值执行委托</param>
        /// <param name="domainUnload">应用程序退出处理</param>
        /// <param name="onError">应用程序退出处理</param>
        public void Start<parameterType, returnType>(Func<parameterType, returnType> task, parameterType parameter,
            Action<returnType> onReturn, Action domainUnload = null, Action<Exception> onError = null)
        {
            if (task == null) log.Error.Throw(null, "缺少 线程委托", false);
            start(run<parameterType, returnType>.Create(task, parameter, onReturn), domainUnload, onError);
        }
        /// <summary>
        /// 线程入池
        /// </summary>
        /// <param name="thread">线程池线程</param>
        internal void Push(thread thread)
        {
            if (isDisposed) thread.Stop();
            else
            {
                threads.Push(thread);
                if (isDisposed) disposePool();
            }
        }
        /// <summary>
        /// 检测日志输出
        /// </summary>
        public static void CheckLog()
        {
            subArray<StackTrace> threads = thread.StackTraces;
            log.Default.Add("活动线程数量 " + threads.Count.toString(), false, false);
            foreach (StackTrace value in threads)
            {
                try
                {
                    log.Default.Add(value.ToString(), false, false);
                }
                catch { }
            }
        }
        /// <summary>
        /// 微型线程池,堆栈大小可能只有128K
        /// </summary>
        public static readonly threadPool TinyPool = new threadPool(fastCSharp.config.appSetting.TinyThreadStackSize);
        /// <summary>
        /// 默认线程池
        /// </summary>
        public static readonly threadPool Default = fastCSharp.config.appSetting.ThreadStackSize != fastCSharp.config.appSetting.TinyThreadStackSize ? new threadPool(fastCSharp.config.appSetting.ThreadStackSize) : TinyPool;
        static ThreadPoolPlus()
        {
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
