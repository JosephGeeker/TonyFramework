﻿//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ParameterInfoPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  ParameterInfoPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:18:15
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Reflection;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 参数信息
    /// </summary>
    public sealed partial class ParameterInfoPlus
    {
        /// <summary>
        /// 参数信息
        /// </summary>
        public ParameterInfo Parameter { get; private set; }
        /// <summary>
        /// 参数索引位置
        /// </summary>
        public int ParameterIndex { get; private set; }
        /// <summary>
        /// 参数类型
        /// </summary>
        public memberType ParameterType { get; private set; }
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName;
        /// <summary>
        /// 参数连接名称，最后一个参数不带逗号
        /// </summary>
        public string ParameterJoinName
        {
            get
            {
                return ParameterName + ParameterJoin;
            }
        }
        /// <summary>
        /// 带引用修饰的参数连接名称，最后一个参数不带逗号
        /// </summary>
        public string ParameterJoinRefName
        {
            get
            {
                return getRefName(ParameterJoinName);
            }
        }
        /// <summary>
        /// 带引用修饰的参数名称
        /// </summary>
        public string ParameterTypeRefName
        {
            get
            {
                return getRefName(ParameterType.FullName);
            }
        }
        /// <summary>
        /// 带引用修饰的参数名称
        /// </summary>
        public string ParameterRefName
        {
            get
            {
                return getRefName(ParameterName);
            }
        }
        /// <summary>
        /// 参数连接逗号，最后一个参数为null
        /// </summary>
        public string ParameterJoin { get; private set; }
        /// <summary>
        /// 是否引用参数
        /// </summary>
        public bool IsRef;
        /// <summary>
        /// 是否输出参数
        /// </summary>
        public bool IsOut { get; private set; }
        /// <summary>
        /// 是否输出参数
        /// </summary>
        public bool IsRefOrOut
        {
            get { return IsRef || IsOut; }
        }
        /// <summary>
        /// 参数引用前缀
        /// </summary>
        public string ParameterRef
        {
            get
            {
                return getRefName(null);
            }
        }
        /// <summary>
        /// 参数信息
        /// </summary>
        /// <param name="parameter">参数信息</param>
        /// <param name="index">参数索引位置</param>
        /// <param name="isLast">是否最后一个参数</param>
        private parameterInfo(ParameterInfo parameter, int index, bool isLast)
        {
            Parameter = parameter;
            ParameterIndex = index;
            Type parameterType = parameter.ParameterType;
            if (parameterType.IsByRef)
            {
                if (parameter.IsOut) IsOut = true;
                else IsRef = true;
                ParameterType = parameterType.GetElementType();
            }
            else ParameterType = parameterType;
            ParameterName = Parameter.Name;
            ParameterJoin = isLast ? null : ", ";
        }
        /// <summary>
        /// 参数信息
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="type">参数类型</param>
        public parameterInfo(string name, Type type)
        {
            ParameterName = name;
            ParameterType = type;
        }
        /// <summary>
        /// 获取带引用修饰的名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>带引用修饰的名称</returns>
        private string getRefName(string name)
        {
            if (IsOut) return "out " + name;
            if (IsRef) return "ref " + name;
            return name;
        }
        /// <summary>
        /// 获取方法参数信息集合
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>参数信息集合</returns>
        internal static parameterInfo[] Get(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.length() != 0)
            {
                int index = 0;
                return parameters.getArray(value => new parameterInfo(value, index, ++index == parameters.Length));
            }
            return nullValue<parameterInfo>.Array;
        }
        /// <summary>
        /// 获取方法参数信息集合
        /// </summary>
        /// <param name="parameters">参数信息集合</param>
        /// <returns>参数信息集合</returns>
        public static parameterInfo[] Get(parameterInfo[] parameters)
        {
            if (parameters.length() != 0)
            {
                parameterInfo parameter = parameters[parameters.Length - 1];
                parameters[parameters.Length - 1] = new parameterInfo(parameter.Parameter, parameter.ParameterIndex, true);
            }
            return parameters;
        }
    }
}
