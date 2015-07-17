//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: RunPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  RunPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 16:59:47
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

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 泛型任务信息
    /// </summary>
    /// <typeparam name="parameterType">任务执行参数类型</typeparam>
    public sealed class RunPlus<parameterType>
    {
        /// <summary>
        /// 任务执行委托
        /// </summary>
        private Action<parameterType> func;
        /// <summary>
        /// 任务执行参数
        /// </summary>
        private parameterType parameter;
        /// <summary>
        /// 执行任务
        /// </summary>
        private Action action;
        /// <summary>
        /// 泛型任务信息
        /// </summary>
        private run()
        {
            action = call;
        }
        /// <summary>
        /// 执行任务
        /// </summary>
        private void call()
        {
            Action<parameterType> func = this.func;
            parameterType parameter = this.parameter;
            this.func = null;
            this.parameter = default(parameterType);
            try
            {
                typePool<run<parameterType>>.Push(this);
            }
            finally
            {
                func(parameter);
            }
        }
        /// <summary>
        /// 泛型任务信息
        /// </summary>
        /// <param name="action">任务执行委托</param>
        /// <param name="parameter">任务执行参数</param>
        /// <returns>泛型任务信息</returns>
        public static Action Create(Action<parameterType> action, parameterType parameter)
        {
            run<parameterType> run = typePool<run<parameterType>>.Pop();
            if (run == null)
            {
                try
                {
                    run = new run<parameterType>();
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (run == null) return null;
            }
            run.func = action;
            run.parameter = parameter;
            return run.action;
        }
    }
    /// <summary>
    /// 泛型任务信息
    /// </summary>
    /// <typeparam name="parameterType">任务执行参数类型</typeparam>
    /// <typeparam name="returnType">返回值类型</typeparam>
    public sealed class run<parameterType, returnType>
    {
        /// <summary>
        /// 任务执行委托
        /// </summary>
        private Func<parameterType, returnType> func;
        /// <summary>
        /// 返回值执行委托
        /// </summary>
        private Action<returnType> onReturn;
        /// <summary>
        /// 任务执行参数
        /// </summary>
        private parameterType parameter;
        /// <summary>
        /// 执行任务
        /// </summary>
        private Action action;
        /// <summary>
        /// 泛型任务信息
        /// </summary>
        private run()
        {
            action = call;
        }
        /// <summary>
        /// 执行任务
        /// </summary>
        private void call()
        {
            Func<parameterType, returnType> func = this.func;
            Action<returnType> onReturn = this.onReturn;
            parameterType parameter = this.parameter;
            this.func = null;
            this.onReturn = null;
            this.parameter = default(parameterType);
            try
            {
                typePool<run<parameterType, returnType>>.Push(this);
            }
            finally
            {
                onReturn(func(parameter));
            }
        }
        /// <summary>
        /// 泛型任务信息
        /// </summary>
        /// <param name="func">任务执行委托</param>
        /// <param name="parameter">任务执行参数</param>
        /// <param name="onReturn">返回值执行委托</param>
        /// <returns>泛型任务信息</returns>
        public static Action Create(Func<parameterType, returnType> func, parameterType parameter, Action<returnType> onReturn)
        {
            run<parameterType, returnType> run = typePool<run<parameterType, returnType>>.Pop();
            if (run == null)
            {
                try
                {
                    run = new run<parameterType, returnType>();
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (run == null) return null;
            }
            run.func = func;
            run.parameter = parameter;
            run.onReturn = onReturn;
            return run.action;
        }
    }
}
