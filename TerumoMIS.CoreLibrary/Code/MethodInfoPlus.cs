//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MethodInfoPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  MethodInfoPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:11:24
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

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 成员方法
    /// </summary>
    public sealed class MethodInfoPlus:MemberInfoPlus
    {
        /// <summary>
        /// 成员方法信息
        /// </summary>
        public MethodInfo Method { get; private set; }
        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName
        {
            get { return Method.Name; }
        }
        /// <summary>
        /// 方法泛型名称
        /// </summary>
        public string MethodGenericName
        {
            get
            {
                return MethodName + GenericParameterName;
            }
        }
        /// <summary>
        /// 方法泛型名称
        /// </summary>
        public string StaticMethodGenericName
        {
            get { return MethodGenericName; }
        }
        /// <summary>
        /// 方法全称标识
        /// </summary>
        public string MethodKeyFullName
        {
            get
            {
                return Method.DeclaringType.fullName() + MethodKeyName;
            }
        }
        /// <summary>
        /// 方法标识
        /// </summary>
        public string MethodKeyName
        {
            get
            {
                return "(" + Parameters.joinString(',', value => value.ParameterRef + value.ParameterType.FullName) + ")" + GenericParameterName + MethodName;
            }
        }
        /// <summary>
        /// 返回值类型
        /// </summary>
        public MemberTypePlus ReturnType { get; private set; }
        /// <summary>
        /// 是否有返回值
        /// </summary>
        public bool IsReturn
        {
            get
            {
                return ReturnType.Type != typeof(void);
            }
        }
        /// <summary>
        /// 参数集合
        /// </summary>
        public parameterInfo[] Parameters { get; private set; }
        /// <summary>
        /// 泛型参数类型集合
        /// </summary>
        public MemberTypePlus[] GenericParameters { get; private set; }
        /// <summary>
        /// 泛型参数拼写
        /// </summary>
        private string _genericParameterName;
        /// <summary>
        /// 泛型参数拼写
        /// </summary>
        public string GenericParameterName
        {
            get
            {
                if (_genericParameterName == null)
                {
                    MemberTypePlus[] genericParameters = GenericParameters;
                    _genericParameterName = genericParameters.Length == 0 ? string.Empty : ("<" + genericParameters.joinString(',', value => value.FullName) + ">");
                }
                return _genericParameterName;
            }
        }
        /// <summary>
        /// 参数集合
        /// </summary>
        public parameterInfo[] OutputParameters { get; private set; }
        /// <summary>
        /// 成员方法
        /// </summary>
        /// <param name="method">成员方法信息</param>
        /// <param name="filter">选择类型</param>
        internal MethodInfoPlus(MethodInfo method, MemberFiltersEnum filter)
            : base(method, filter)
        {
            Method = method;
            ReturnType = Method.ReturnType;
            Parameters = parameterInfo.Get(method);
            OutputParameters = Parameters.getFindArray(value => value.Parameter.IsOut || value.Parameter.ParameterType.IsByRef);
            GenericParameters = method.GetGenericArguments().getArray(value => (MemberTypePlus)value);
        }

        /// <summary>
        /// 类型成员方法缓存
        /// </summary>
        private static readonly Dictionary<Type, MethodInfoPlus[]> MethodCache = DictionaryPlus.CreateOnly<Type, MethodInfoPlus[]>();
        /// <summary>
        /// 获取类型的成员方法集合
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>成员方法集合</returns>
        private static MethodInfoPlus[] GetMethods(Type type)
        {
            MethodInfoPlus[] methods;
            if (!MethodCache.TryGetValue(type, out methods))
            {
                var index = 0;
                MethodCache[type] = methods = ArrayPlus.Concat(
                    type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetArray(value => new MethodInfoPlus(value, MemberFiltersEnum.PublicStatic)),
                    type.GetMethods(BindingFlags.Public | BindingFlags.Instance).GetArray(value => new MethodInfoPlus(value, MemberFiltersEnum.PublicInstance)),
                    type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetArray(value => new MethodInfoPlus(value, MemberFiltersEnum.NonPublicStatic)),
                    type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).GetArray(value => new MethodInfoPlus(value, MemberFiltersEnum.NonPublicInstance)))
                    .Each(value => value.MemberIndex = index++);
            }
            return methods;
        }
        /// <summary>
        /// 获取匹配成员方法集合
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="filter">选择类型</param>
        /// <param name="isFilter">是否完全匹配选择类型</param>
        /// <returns>匹配的成员方法集合</returns>
        public static SubArrayStruct<MethodInfoPlus> GetMethods(Type type, MemberFiltersEnum filter, bool isFilter)
        {
            return GetMethods(type).getFind(value => isFilter ? (value.Filter & filter) == filter : ((value.Filter & filter) != 0));
        }
        /// <summary>
        /// 获取匹配成员方法集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="methods">成员方法集合</param>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类自定义属性</param>
        /// <param name="isInheritAttribute">自定义属性类型是否可继承</param>
        /// <returns>匹配成员方法集合</returns>
        private static MethodInfoPlus[] GetMethods<TAttributeType>
            (Type type, SubArrayStruct<MethodInfoPlus> methods, bool isAttribute, bool isBaseType, bool isInheritAttribute)
            where TAttributeType : IgnoreMemberPlus
        {
            if (isAttribute)
            {
                return methods.ToList().GetFindArray(value => value.IsAttribute<TAttributeType>(isBaseType, isInheritAttribute));
            }
            else
            {
                return methods.ToList().getFindArray(value => value.Method.DeclaringType == type && !value.IsIgnoreAttribute<TAttributeType>(isBaseType, isInheritAttribute));
            }
        }
        /// <summary>
        /// 获取匹配成员方法集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="filter">选择类型</param>
        /// <param name="isFilter">是否完全匹配选择类型</param>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类自定义属性</param>
        /// <param name="isInheritAttribute">自定义属性类型是否可继承</param>
        /// <returns>匹配成员方法集合</returns>
        public static MethodInfoPlus[] GetMethods<TAttributeType>(Type type, MemberFiltersEnum filter, bool isFilter, bool isAttribute, bool isBaseType, bool isInheritAttribute)
            where TAttributeType : IgnoreMemberPlus
        {
            return GetMethods<TAttributeType>(type, GetMethods(type, filter, isFilter), isAttribute, isBaseType, isInheritAttribute);
        }
    }
}
