//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CreatePropertyPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  CreatePropertyPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:42:27
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
    /// 属性创建器
    /// </summary>
    /// <typeparam name="valueType">创建目标类型</typeparam>
    public abstract class CreatePropertyPlus<valueType>
    {
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>目标,失败返回null</returns>
        public abstract valueType Create(PropertyInfo property, bool nonPublic);
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <param name="name">属性名称</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>目标,失败返回null</returns>
        public valueType Create(Type type, string name, bool nonPublic)
        {
            if (type != null && name != null)
            {
                return Create(type.GetProperty(name, BindingFlags.Instance | (nonPublic ? BindingFlags.NonPublic : BindingFlags.Public)), nonPublic);
            }
            return default(valueType);
        }
        /// <summary>
        /// 创建目标
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeName">类型全名</param>
        /// <param name="name">属性名称</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>目标,失败返回null</returns>
        public valueType Create(Assembly assembly, string typeName, string name, bool nonPublic)
        {
            return assembly != null && typeName != null ? Create(assembly.GetType(typeName), name, nonPublic) : default(valueType);
        }
    }
}
