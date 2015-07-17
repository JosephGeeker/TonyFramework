//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: StaticMethodDelegatePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  StaticMethodDelegatePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:51:42
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
    /// 静态方法委托
    /// </summary>
    public static class StaticMethodDelegatePlus
    {
        /// <summary>
        /// 创建方法委托
        /// </summary>
        /// <typeparam name="parameterType1">参数1类型</typeparam>
        /// <typeparam name="parameterType2">参数2类型</typeparam>
        /// <typeparam name="returnType">返回值类型</typeparam>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Func<parameterType1, parameterType2, returnType> Create<parameterType1, parameterType2, returnType>(MethodInfo method)
        {
            if (method != null)
            {
                try
                {
                    return (Func<parameterType1, parameterType2, returnType>)Delegate.CreateDelegate(typeof(Func<parameterType1, parameterType2, returnType>), method);
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
        /// <typeparam name="parameterType1">参数1类型</typeparam>
        /// <typeparam name="parameterType2">参数2类型</typeparam>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Func<parameterType1, parameterType2, object> Create2<parameterType1, parameterType2>(MethodInfo method)
        {
            if (method != null && method.ReturnType != typeof(void))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.length() == 2)
                {
                    try
                    {
                        return ((IStaticMethodDelegate<parameterType1, parameterType2>)Activator.CreateInstance(typeof(staticMethodDelegate<,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, method.ReturnType), method)).Invoke;
                    }
                    catch (Exception error)
                    {
                        fastCSharp.log.Error.Add(error, method.fullName(), true);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 创建方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Func<object, object> Create(MethodInfo method)
        {
            if (method != null && method.ReturnType != typeof(void))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.length() == 1)
                {
                    try
                    {
                        return ((IStaticMethodDelegate)Activator.CreateInstance(typeof(staticMethodDelegate<,>).MakeGenericType(parameters[0].ParameterType, method.ReturnType), method)).Invoke;
                    }
                    catch (Exception error)
                    {
                        fastCSharp.log.Error.Add(error, method.fullName(), true);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 创建方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Func<parameterType, object> Create<parameterType>(MethodInfo method)
        {
            if (method != null && method.ReturnType != typeof(void))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.length() == 1)
                {
                    try
                    {
                        return ((IStaticMethodDelegate<parameterType>)Activator.CreateInstance(typeof(staticMethodDelegate<,>).MakeGenericType(parameters[0].ParameterType, method.ReturnType), method)).Invoke;
                    }
                    catch (Exception error)
                    {
                        fastCSharp.log.Error.Add(error, method.fullName(), true);
                    }
                }
            }
            return null;
        }
    }
    /// <summary>
    /// 方法委托
    /// </summary>
    internal interface IStaticMethodDelegate
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        object Invoke(object parameter);
    }
    /// <summary>
    /// 方法委托
    /// </summary>
    internal interface IStaticMethodDelegate<parameterType> : IStaticMethodDelegate
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        object Invoke(parameterType parameter);
    }
    /// <summary>
    /// 1个参数的方法委托
    /// </summary>
    /// <typeparam name="parameterType">参数类型</typeparam>
    /// <typeparam name="returnType">返回值类型</typeparam>
    internal sealed class staticMethodDelegate<parameterType, returnType> : IStaticMethodDelegate<parameterType>
    {
        /// <summary>
        /// 方法委托
        /// </summary>
        private Func<parameterType, returnType> func;
        /// <summary>
        /// 方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        public staticMethodDelegate(MethodInfo method)
        {
            this.func = (Func<parameterType, returnType>)Delegate.CreateDelegate(typeof(Func<parameterType, returnType>), method);
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        public object Invoke(parameterType parameter)
        {
            return func(parameter);
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        public object Invoke(object parameter)
        {
            return func((parameterType)parameter);
        }
    }
    /// <summary>
    /// 方法委托
    /// </summary>
    internal interface IStaticMethodDelegate<parameterType1, parameterType2>
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="parameter1">参数1</param>
        /// <param name="parameter2">参数2</param>
        /// <returns>返回值</returns>
        object Invoke(parameterType1 parameter1, parameterType2 parameter2);
    }
    /// <summary>
    /// 2个参数的方法委托
    /// </summary>
    /// <typeparam name="parameterType1">参数1类型</typeparam>
    /// <typeparam name="parameterType2">参数2类型</typeparam>
    /// <typeparam name="returnType">返回值类型</typeparam>
    internal sealed class staticMethodDelegate<parameterType1, parameterType2, returnType> : IStaticMethodDelegate<parameterType1, parameterType2>
    {
        /// <summary>
        /// 方法委托
        /// </summary>
        private Func<parameterType1, parameterType2, returnType> func;
        /// <summary>
        /// 方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        public staticMethodDelegate(MethodInfo method)
        {
            this.func = (Func<parameterType1, parameterType2, returnType>)Delegate.CreateDelegate(typeof(Func<parameterType1, parameterType2, returnType>), method);
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="parameter1">参数1</param>
        /// <param name="parameter2">参数2</param>
        /// <returns>返回值</returns>
        public object Invoke(parameterType1 parameter1, parameterType2 parameter2)
        {
            return func(parameter1, parameter2);
        }
    }
}
