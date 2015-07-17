//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberIndexGroupStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  MemberIndexGroupStruct
//	User name:  C1400008
//	Location Time: 2015/7/13 16:55:06
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 成员索引分组
    /// </summary>
    public struct MemberIndexGroupStruct
    {
        /// <summary>
        /// 成员索引分组集合
        /// </summary>
        private static readonly Dictionary<Type, MemberIndexGroupStruct> Cache = DictionaryPlus.CreateOnly<Type, MemberIndexGroupStruct>();
        /// <summary>
        /// 成员索引分组集合访问锁
        /// </summary>
        private static readonly object CacheLock = new object();
        /// <summary>
        /// 公有动态字段
        /// </summary>
        internal readonly MemberIndexPlus[] PublicFields;
        /// <summary>
        /// 非公有动态字段
        /// </summary>
        internal readonly MemberIndexPlus[] NonPublicFields;
        /// <summary>
        /// 公有动态属性
        /// </summary>
        internal readonly MemberIndexPlus[] PublicProperties;
        /// <summary>
        /// 非公有动态属性
        /// </summary>
        internal readonly MemberIndexPlus[] NonPublicProperties;
        /// <summary>
        /// 所有成员数量
        /// </summary>
        public readonly int MemberCount;
        /// <summary>
        /// 成员索引分组
        /// </summary>
        /// <param name="type">对象类型</param>
        private MemberIndexGroupStruct(Type type)
        {
            var index = 0;
            if (type.IsEnum)
            {
                PublicFields = type.GetFields(BindingFlags.Public | BindingFlags.Static).GetArray(member => new FieldIndexPlus(member, MemberFiltersEnum.PublicStaticField, index++));
                NonPublicFields = NullValuePlus<FieldIndexPlus>.Array;
                PublicProperties = NonPublicProperties = NullValuePlus<PropertyIndexPlus>.Array;
            }
            else
            {
                var group = new MemberIndexPlus.GroupStruct(type);
                if (type.getTypeName() == null)
                {
                    PublicFields = group.PublicFields.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal)).GetArray(value => new FieldIndexPlus(value, MemberFiltersEnum.PublicInstanceField, index++));
                    NonPublicFields = group.NonPublicFields.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal)).GetArray(value => new FieldIndexPlus(value, MemberFiltersEnum.NonPublicInstanceField, index++));
                    PublicProperties = group.PublicProperties.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal)).GetArray(value => new PropertyIndexPlus(value, MemberFiltersEnum.PublicInstanceProperty, index++));
                    NonPublicProperties = group.NonPublicProperties.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal)).GetArray(value => new PropertyIndexPlus(value, MemberFiltersEnum.NonPublicInstanceProperty, index++));
                }
                else
                {
                    PublicFields = NonPublicFields = NullValuePlus<FieldIndexPlus>.Array;
                    PublicProperties = NonPublicProperties = NullValuePlus<PropertyIndexPlus>.Array;
                }
            }
            MemberCount = index;
        }
        /// <summary>
        /// 获取成员索引集合
        /// </summary>
        /// <param name="isValue">成员匹配委托</param>
        /// <returns>成员索引集合</returns>
        private MemberIndexPlus[] Get(Func<MemberIndexPlus, bool> isValue)
        {
            return ArrayPlus.Concat(PublicFields.GetFindArray(isValue), NonPublicFields.GetFindArray(isValue), PublicProperties.GetFindArray(isValue), NonPublicProperties.GetFindArray(isValue));
        }
        /// <summary>
        /// 根据类型获取成员信息集合
        /// </summary>
        /// <param name="filter">选择类型</param>
        /// <returns>成员信息集合</returns>
        public MemberIndexPlus[] Find(MemberFiltersEnum filter)
        {
            return Get(value => (value.Filter & filter) != 0);
        }
        /// <summary>
        /// 根据类型获取成员信息集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="filter">成员选择</param>
        /// <returns>成员信息集合</returns>
        internal MemberIndexPlus[] Find<TAttributeType>(TAttributeType filter) where TAttributeType : MemberFilterPlus
        {
            return Find(filter.MemberFilter).GetFindArray(value => filter.IsAttribute ? value.IsAttribute<TAttributeType>(filter.IsBaseTypeAttribute, filter.IsInheritAttribute) : !value.IsIgnoreAttribute<TAttributeType>(filter.IsBaseTypeAttribute, filter.IsInheritAttribute));
        }
        /// <summary>
        /// 根据类型获取成员信息集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="filter">成员选择</param>
        /// <returns>成员信息集合</returns>
        internal MemberIndexPlus[] Find<TAttributeType>(MemberFilterPlus filter) where TAttributeType : IgnoreMemberPlus
        {
            return Find(filter.MemberFilter).GetFindArray(value => filter.IsAttribute ? value.IsAttribute<TAttributeType>(filter.IsBaseTypeAttribute, filter.IsInheritAttribute) : !value.IsIgnoreAttribute<TAttributeType>(filter.IsBaseTypeAttribute, filter.IsInheritAttribute));
        }
        /// <summary>
        /// 根据类型获取成员索引分组
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>成员索引分组</returns>
        public static MemberIndexGroupStruct Get(Type type)
        {
            MemberIndexGroupStruct value;
            Monitor.Enter(CacheLock);
            try
            {
                if (!Cache.TryGetValue(type, out value)) Cache.Add(type, value = new MemberIndexGroupStruct(type));
            }
            finally { Monitor.Exit(CacheLock); }
            return value;
        }
    }
    /// <summary>
    /// 成员索引分组
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    internal static class MemberIndexGroupPlus<TValueType>
    {
        /// <summary>
        /// 成员索引分组
        /// </summary>
        public static readonly MemberIndexGroupStruct Group = MemberIndexGroupStruct.Get(typeof(TValueType));
        /// <summary>
        /// 所有成员数量
        /// </summary>
        public static readonly int MemberCount = Group.MemberCount;
        /// <summary>
        /// 字段成员数量
        /// </summary>
        public static readonly int FieldCount = Group.PublicFields.Length + Group.NonPublicFields.Length;
        /// <summary>
        /// 成员集合
        /// </summary>
        public static MemberIndexPlus[] GetAllMembers()
        {
            var members = new SubArrayStruct<MemberIndexPlus>(MemberCount);
            members.Add(Group.PublicFields.toGeneric<MemberIndexPlus>());
            members.Add(Group.NonPublicFields.toGeneric<MemberIndexPlus>());
            members.Add(Group.PublicProperties.toGeneric<MemberIndexPlus>());
            members.Add(Group.NonPublicProperties.toGeneric<MemberIndexPlus>());
            return members.ToArray();
        }
        /// <summary>
        /// 获取字段集合
        /// </summary>
        /// <param name="memberFilter">成员选择类型</param>
        /// <returns></returns>
        public static FieldIndexPlus[] GetFields(MemberFiltersEnum memberFilter = MemberFiltersEnum.InstanceField)
        {
            if ((memberFilter & MemberFiltersEnum.PublicInstanceField) == 0)
            {
                if ((memberFilter & MemberFiltersEnum.NonPublicInstanceField) == 0) return NullValuePlus<FieldIndexPlus>.Array;
                return Group.NonPublicFields;
            }
            else if ((memberFilter & MemberFiltersEnum.NonPublicInstanceField) == 0) return Group.PublicFields;
            return Group.PublicFields.Concat(Group.NonPublicFields);
        }
        /// <summary>
        /// 获取属性集合
        /// </summary>
        /// <param name="memberFilter">成员选择类型</param>
        /// <returns></returns>
        public static PropertyIndexPlus[] GetProperties(MemberFiltersEnum memberFilter = MemberFiltersEnum.InstanceField)
        {
            if ((memberFilter & MemberFiltersEnum.PublicInstanceProperty) == 0)
            {
                if ((memberFilter & MemberFiltersEnum.NonPublicInstanceProperty) == 0) return NullValuePlus<PropertyIndexPlus>.Array;
                return Group.NonPublicProperties;
            }
            else if ((memberFilter & MemberFiltersEnum.NonPublicInstanceProperty) == 0) return Group.PublicProperties;
            return Group.PublicProperties.Concat(Group.NonPublicProperties);
        }
    }
}
