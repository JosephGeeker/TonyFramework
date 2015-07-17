//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MethodDelegatePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  MethodDelegatePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:46:44
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Reflection
{
    /// <summary>
    /// 成员方法委托
    /// </summary>
    public static class MethodDelegatePlus
    {
        /// <summary>
        /// 创建方法委托
        /// </summary>
        /// <typeparam name="parameterType">参数类型</typeparam>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Action<object, parameterType> Create<parameterType>(MethodInfo method)
        {
            if (method != null)
            {
                try
                {
                    return ((IMethodDelegate<parameterType>)Activator.CreateInstance(typeof(methodDelegate<,>).MakeGenericType(method.DeclaringType, typeof(parameterType)), method)).Invoke;
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, method.fullName(), true);
                }
            }
            return null;
        }
        /// <summary>
        /// 创建方法委托
        /// </summary>
        /// <typeparam name="目标对象类型">参数类型</typeparam>
        /// <typeparam name="parameterType">参数类型</typeparam>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Action<targetType, parameterType> Create<targetType, parameterType>(MethodInfo method)
        {
            if (method != null)
            {
                try
                {
                    return ((IMethodDelegate<targetType, parameterType>)Activator.CreateInstance(typeof(methodDelegate<,>).MakeGenericType(method.DeclaringType, typeof(parameterType)), method)).Invoke;
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, method.fullName(), true);
                }
            }
            return null;
        }
    }
    /// <summary>
    /// 方法委托
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    public static class methodTargetDelegate<targetType>
    {
        /// <summary>
        /// 创建方法委托
        /// </summary>
        /// <typeparam name="parameterType">参数类型</typeparam>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Action<targetType, parameterType> Create<parameterType>(MethodInfo method)
        {
            if (method != null)
            {
                try
                {
                    return (Action<targetType, parameterType>)Delegate.CreateDelegate(typeof(Action<targetType, parameterType>), null, method);
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, method.fullName(), true);
                }
            }
            return null;
        }
        /// <summary>
        /// 创建方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <param name="parameterType">参数类型</param>
        /// <returns>方法委托</returns>
        public static Action<targetType, object> Create(MethodInfo method, Type parameterType)
        {
            if (method != null && parameterType != null)
            {
                try
                {
                    return ((IMethodTargetDelegate<targetType>)Activator.CreateInstance(typeof(methodDelegate<,>).MakeGenericType(method.DeclaringType, parameterType), method)).Invoke;
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, method.fullName() + " + " + parameterType.FullName, true);
                }
            }
            return null;
        }
    }
    /// <summary>
    /// 方法委托接口
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    internal interface IMethodTargetDelegate<targetType>
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="parameter">参数值</param>
        void Invoke(targetType target, object parameter);
    }
    /// <summary>
    /// 方法委托接口
    /// </summary>
    /// <typeparam name="parameterType">参数类型</typeparam>
    internal interface IMethodDelegate<parameterType>
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="parameter">参数值</param>
        void Invoke(object target, parameterType parameter);
    }
    /// <summary>
    /// 方法委托接口
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    /// <typeparam name="parameterType">参数类型</typeparam>
    internal interface IMethodDelegate<targetType, parameterType>
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="parameter">参数值</param>
        void Invoke(targetType target, parameterType parameter);
    }
    /// <summary>
    /// 方法委托
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    /// <typeparam name="parameterType">参数类型</typeparam>
    internal sealed class methodDelegate<targetType, parameterType> : IMethodTargetDelegate<targetType>, IMethodDelegate<parameterType>, IMethodDelegate<targetType, parameterType>
    {
        /// <summary>
        /// 方法委托
        /// </summary>
        private Action<targetType, parameterType> method;
        /// <summary>
        /// 方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        public methodDelegate(MethodInfo method)
        {
            this.method = (Action<targetType, parameterType>)Delegate.CreateDelegate(typeof(Action<targetType, parameterType>), null, method);
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="parameter">参数值</param>
        public void Invoke(targetType target, object parameter)
        {
            method(target, (parameterType)parameter);
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="parameter">参数值</param>
        public void Invoke(object target, parameterType parameter)
        {
            method((targetType)target, parameter);
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="parameter">参数值</param>
        public void Invoke(targetType target, parameterType parameter)
        {
            method(target, parameter);
        }
    }
}
