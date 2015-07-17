//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: AttributeMethodStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  AttributeMethodStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 10:55:44
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
using System.Threading;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Threading;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 自定义函数属性信息
    /// </summary>
    public struct AttributeMethodStruct
    {
        /// <summary>
        /// 函数信息
        /// </summary>
        public MethodInfo Method;
        /// <summary>
        /// 自定义属性集合
        /// </summary>
        private object[] _attributes;
        /// <summary>
        /// 获取自定义属性集合
        /// </summary>
        /// <typeparam name="TAttributeType"></typeparam>
        /// <returns></returns>
        internal IEnumerable<TAttributeType> Attributes<TAttributeType>() where TAttributeType : Attribute
        {
            return _attributes.Where(value => typeof(TAttributeType).IsAssignableFrom(value.GetType())).Cast<TAttributeType>();
        }

        /// <summary>
        /// 根据成员属性获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性</returns>
        public TAttributeType GetAttribute<TAttributeType>(bool isInheritAttribute) where TAttributeType : Attribute
        {
            TAttributeType value = null;
            var minDepth = int.MaxValue;
            foreach (var attribute in Attributes<TAttributeType>())
            {
                if (isInheritAttribute)
                {
                    var depth = 0;
                    for (var type = attribute.GetType(); type != typeof(TAttributeType); type = type.BaseType) ++depth;
                    if (depth < minDepth)
                    {
                        if (depth == 0) return attribute;
                        minDepth = depth;
                        value = attribute;
                    }
                }
                else if (attribute.GetType() == typeof(TAttributeType)) return attribute;
            }
            return value;
        }
        /// <summary>
        /// 自定义属性函数信息集合
        /// </summary>
        private static InterlockedPlus.DictionaryStruct<Type, AttributeMethodStruct[]> _methods = new InterlockedPlus.DictionaryStruct<Type, AttributeMethodStruct[]>(DictionaryPlus.CreateOnly<Type, AttributeMethodStruct[]>());
        /// <summary>
        /// 自定义属性函数信息集合访问锁
        /// </summary>
        private static readonly object CreateLock = new object();
        /// <summary>
        /// 根据类型获取自定义属性函数信息集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>自定义属性函数信息集合</returns>
        public static AttributeMethodStruct[] Get(Type type)
        {
            AttributeMethodStruct[] values;
            if (_methods.TryGetValue(type, out values)) return values;
            Monitor.Enter(CreateLock);
            try
            {
                if (_methods.TryGetValue(type, out values)) return values;
                var array = default(SubArrayStruct<AttributeMethodStruct>);
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attributes = method.GetCustomAttributes(true);
                    if (attributes.Length != 0) array.Add(new AttributeMethodStruct { Method = method, _attributes = attributes });
                }
                _methods.Set(type, values = array.ToArray());
            }
            finally { Monitor.Exit(CreateLock); }
            return values;
        }
        /// <summary>
        /// 自定义属性函数信息集合
        /// </summary>
        private static InterlockedPlus.DictionaryStruct<Type, AttributeMethodStruct[]> _staticMethods = new InterlockedPlus.DictionaryStruct<Type, AttributeMethodStruct[]>(DictionaryPlus.CreateOnly<Type, AttributeMethodStruct[]>());
        /// <summary>
        /// 自定义属性函数信息集合访问锁
        /// </summary>
        private static readonly object CreateStaticLock = new object();
        /// <summary>
        /// 根据类型获取自定义属性函数信息集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>自定义属性函数信息集合</returns>
        public static AttributeMethodStruct[] GetStatic(Type type)
        {
            AttributeMethodStruct[] values;
            if (_staticMethods.TryGetValue(type, out values)) return values;
            Monitor.Enter(CreateStaticLock);
            try
            {
                if (_staticMethods.TryGetValue(type, out values)) return values;
                var array = default(SubArrayStruct<AttributeMethodStruct>);
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    object[] attributes = method.GetCustomAttributes(true);
                    if (attributes.Length != 0) array.Add(new AttributeMethodStruct { Method = method, _attributes = attributes });
                }
                _staticMethods.Set(type, values = array.ToArray());
            }
            finally { Monitor.Exit(CreateStaticLock); }
            return values;
        }
    }
}
