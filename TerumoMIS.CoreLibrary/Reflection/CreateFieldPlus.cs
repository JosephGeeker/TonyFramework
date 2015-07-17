//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CreateFieldPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  CreateFieldPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:41:21
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
    /// 字段创建
    /// </summary>
    /// <typeparam name="valueType">创建目标类型</typeparam>
    public abstract class CreateFieldPlus<valueType>
    {
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>目标</returns>
        public abstract valueType Create(FieldInfo field);
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <param name="name">字段名称</param>
        /// <param name="nonPublic">是否非公共字段</param>
        /// <returns>目标</returns>
        public valueType Create(Type type, string name, bool nonPublic)
        {
            if (type != null && name != null)
            {
                return Create(type.GetField(name, BindingFlags.Instance | (nonPublic ? BindingFlags.NonPublic : BindingFlags.Public)));
            }
            return default(valueType);
        }
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="assembly">程序及信息</param>
        /// <param name="typeName">类型全名</param>
        /// <param name="name">字段名称</param>
        /// <param name="nonPublic">是否非公共字段</param>
        /// <returns>目标</returns>
        public valueType Create(Assembly assembly, string typeName, string name, bool nonPublic)
        {
            return assembly != null && typeName != null ? Create(assembly.GetType(typeName), name, nonPublic) : default(valueType);
        }
    }
    /// <summary>
    /// 字段创建器
    /// </summary>
    /// <typeparam name="valueType">创建目标类型</typeparam>
    /// <typeparam name="parameterType">参数类型</typeparam>
    public abstract class createField<valueType, parameterType>
    {
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>目标</returns>
        public abstract valueType Create(FieldInfo field);
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <param name="name">字段名称</param>
        /// <param name="nonPublic">是否非公共字段</param>
        /// <returns>目标</returns>
        public valueType Create(Type type, string name, bool nonPublic)
        {
            if (type != null && name != null)
            {
                return Create(type.GetField(name, BindingFlags.Instance | (nonPublic ? BindingFlags.NonPublic : BindingFlags.Public)));
            }
            return default(valueType);
        }
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="assembly">程序及信息</param>
        /// <param name="typeName">类型全名</param>
        /// <param name="name">字段名称</param>
        /// <param name="nonPublic">是否非公共字段</param>
        /// <returns>目标</returns>
        public valueType Create(Assembly assembly, string typeName, string name, bool nonPublic)
        {
            return assembly != null && typeName != null ? Create(assembly.GetType(typeName), name, nonPublic) : default(valueType);
        }
    }
}
