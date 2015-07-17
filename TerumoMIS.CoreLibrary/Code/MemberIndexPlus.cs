//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberIndexPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  MemberIndexPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 16:23:56
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 成员索引
    /// </summary>
    public abstract class MemberIndexPlus
    {
        /// <summary>
        /// 动态成员分组
        /// </summary>
        internal struct GroupStruct
        {
            /// <summary>
            /// 类型深度
            /// </summary>
            private struct TypeDepthStruct
            {
                /// <summary>
                /// 成员信息
                /// </summary>
                private MemberInfo _member;
                /// <summary>
                /// 类型深度
                /// </summary>
                public int Depth;
                /// <summary>
                /// 是否字段
                /// </summary>
                private bool _isField;
                /// <summary>
                /// 是否共有成员
                /// </summary>
                private bool _isPublic;
                /// <summary>
                /// 共有字段成员
                /// </summary>
                public FieldInfo PublicField
                {
                    get
                    {
                        return _isPublic && _isField ? (FieldInfo)_member : null;
                    }
                }
                /// <summary>
                /// 非共有字段成员
                /// </summary>
                public FieldInfo NonPublicField
                {
                    get
                    {
                        return !_isPublic && _isField ? (FieldInfo)_member : null;
                    }
                }
                /// <summary>
                /// 共有属性成员
                /// </summary>
                public PropertyInfo PublicProperty
                {
                    get
                    {
                        return _isPublic && !_isField ? (PropertyInfo)_member : null;
                    }
                }
                /// <summary>
                /// 非共有属性成员
                /// </summary>
                public PropertyInfo NonPublicProperty
                {
                    get
                    {
                        return !_isPublic && !_isField ? (PropertyInfo)_member : null;
                    }
                }
                /// <summary>
                /// 类型深度
                /// </summary>
                /// <param name="type">类型</param>
                /// <param name="field">成员字段</param>
                /// <param name="isPublic">是否共有成员</param>
                public TypeDepthStruct(Type type, FieldInfo field, bool isPublic)
                {
                    Type memberType = field.DeclaringType;
                    _member = field;
                    _isField = true;
                    _isPublic = isPublic;
                    for (Depth = 0; type != memberType; ++Depth) if (type != null) type = type.BaseType;
                }
                /// <summary>
                /// 类型深度
                /// </summary>
                /// <param name="type">类型</param>
                /// <param name="property">成员属性</param>
                /// <param name="isPublic">是否共有成员</param>
                public TypeDepthStruct(Type type, PropertyInfo property, bool isPublic)
                {
                    var memberType = property.DeclaringType;
                    _member = property;
                    _isField = false;
                    _isPublic = isPublic;
                    for (Depth = 0; type != memberType; ++Depth) if (type != null) type = type.BaseType;
                }
            }
            /// <summary>
            /// 公有动态字段
            /// </summary>
            public FieldInfo[] PublicFields;
            /// <summary>
            /// 非公有动态字段
            /// </summary>
            public FieldInfo[] NonPublicFields;
            /// <summary>
            /// 公有动态属性
            /// </summary>
            public PropertyInfo[] PublicProperties;
            /// <summary>
            /// 非公有动态属性
            /// </summary>
            public PropertyInfo[] NonPublicProperties;
            /// <summary>
            /// 动态成员分组
            /// </summary>
            /// <param name="type">目标类型</param>
            public GroupStruct(Type type)
            {
                var members = DictionaryPlus.CreateHashString<TypeDepthStruct>();
                TypeDepthStruct oldMember;
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var member = new TypeDepthStruct(type, field, true);
                    HashStringStruct nameKey = field.Name;
                    if (!members.TryGetValue(nameKey, out oldMember) || member.Depth < oldMember.Depth) members[nameKey] = member;
                }
                foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.Name[0] != '<')
                    {
                        var member = new TypeDepthStruct(type, field, false);
                        HashStringStruct nameKey = field.Name;
                        if (!members.TryGetValue(nameKey, out oldMember) || member.Depth < oldMember.Depth) members[nameKey] = member;
                    }
                }
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var member = new TypeDepthStruct(type, property, true);
                    HashStringStruct nameKey = property.Name;
                    if (!members.TryGetValue(nameKey, out oldMember) || member.Depth < oldMember.Depth) members[nameKey] = member;
                }
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var member = new TypeDepthStruct(type, property, false);
                    HashStringStruct nameKey = property.Name;
                    if (!members.TryGetValue(nameKey, out oldMember) || member.Depth < oldMember.Depth) members[nameKey] = member;
                }
                PublicFields = members.Values.getArray(value => value.PublicProperty).getFindArray(value => value != null);
                NonPublicFields = members.Values.getArray(value => value.NonPublicField).getFindArray(value => value != null);
                PublicProperties = members.Values.getArray(value => value.PublicProperty).getFindArray(value => value != null);
                NonPublicProperties = members.Values.getArray(value => value.NonPublicProperty).getFindArray(value => value != null);
            }
        }
        /// <summary>
        /// 成员信息
        /// </summary>
        public MemberInfo Member { get; protected set; }
        /// <summary>
        /// 自定义属性集合
        /// </summary>
        private object[] _attributes;
        /// <summary>
        /// 自定义属性集合(包括基类成员属性)
        /// </summary>
        private object[] _baseAttributes;
        /// <summary>
        /// 成员类型
        /// </summary>
        public Type Type { get; protected set; }
        /// <summary>
        /// 成员编号
        /// </summary>
        public int MemberIndex { get; protected set; }
        /// <summary>
        /// 选择类型
        /// </summary>
        internal MemberFiltersEnum Filter;
        /// <summary>
        /// 是否字段
        /// </summary>
        internal bool IsField;
        /// <summary>
        /// 是否可赋值
        /// </summary>
        public bool CanSet { get; protected set; }
        /// <summary>
        /// 是否可读取
        /// </summary>
        public bool CanGet { get; protected set; }
        /// <summary>
        /// 是否忽略该成员
        /// </summary>
        private bool? _isIgnore;
        /// <summary>
        /// 是否忽略该成员
        /// </summary>
        public bool IsIgnore
        {
            get
            {
                if (_isIgnore == null) _isIgnore = Member != null && GetAttribute<IgnorePlus>(true, false) != null;
                return (bool)_isIgnore;
            }
        }
        /// <summary>
        /// 成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        protected MemberIndexPlus(MemberIndexPlus member)
        {
            Member = member.Member;
            Type = member.Type;
            MemberIndex = member.MemberIndex;
            Filter = member.Filter;
            IsField = member.IsField;
            CanSet = member.CanSet;
            CanGet = member.CanGet;
            _isIgnore = member._isIgnore;
        }
        /// <summary>
        /// 成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        protected MemberIndexPlus(MemberInfo member, MemberFiltersEnum filter, int index)
        {
            Member = member;
            MemberIndex = index;
            Filter = filter;
        }
        /// <summary>
        /// 成员信息
        /// </summary>
        /// <param name="index">成员编号</param>
        protected MemberIndexPlus(int index)
        {
            MemberIndex = index;
            IsField = CanSet = CanSet = true;
            Filter = MemberFiltersEnum.PublicInstance;
        }
        /// <summary>
        /// 获取自定义属性集合
        /// </summary>
        /// <typeparam name="TAttributeType"></typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <returns></returns>
        internal IEnumerable<TAttributeType> Attributes<TAttributeType>(bool isBaseType) where TAttributeType : Attribute
        {
            if (Member != null)
            {
                object[] values;
                if (isBaseType)
                {
                    if (_baseAttributes == null)
                    {
                        _baseAttributes = Member.GetCustomAttributes(true);
                        if (_baseAttributes.Length == 0) _attributes = _baseAttributes;
                    }
                    values = _baseAttributes;
                }
                else
                {
                    if (_attributes == null) _attributes = Member.GetCustomAttributes(false);
                    values = _attributes;
                }
                foreach (var value in values)
                {
                    if (typeof(TAttributeType).IsAssignableFrom(value.GetType())) yield return (TAttributeType)value;
                }
            }
        }
        /// <summary>
        /// 根据成员属性获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性</returns>
        internal TAttributeType GetAttribute<TAttributeType>(bool isBaseType, bool isInheritAttribute) where TAttributeType : Attribute
        {
            TAttributeType value = null;
            var minDepth = int.MaxValue;
            foreach (var attribute in Attributes<TAttributeType>(isBaseType))
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
        /// 获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性,失败返回null</returns>
        public TAttributeType GetSetupAttribute<TAttributeType>(bool isBaseType, bool isInheritAttribute) where TAttributeType : IgnoreMemberPlus
        {
            if (!IsIgnore)
            {
                var value = GetAttribute<TAttributeType>(isBaseType, isInheritAttribute);
                if (value != null && value.IsSetup) return value;
            }
            return null;
        }
        /// <summary>
        /// 判断是否存在自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>是否存在自定义属性</returns>
        internal bool IsAttribute<TAttributeType>(bool isBaseType, bool isInheritAttribute) where TAttributeType : IgnoreMemberPlus
        {
            return GetSetupAttribute<TAttributeType>(isBaseType, isInheritAttribute) != null;
        }
        /// <summary>
        /// 判断是否忽略自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>是否忽略自定义属性</returns>
        internal bool IsIgnoreAttribute<TAttributeType>(bool isBaseType, bool isInheritAttribute) where TAttributeType : IgnoreMemberPlus
        {
            if (IsIgnore) return true;
            TAttributeType value = GetAttribute<TAttributeType>(isBaseType, isInheritAttribute);
            return value != null && !value.IsSetup;
        }

        /// <summary>
        /// 根据类型获取成员信息集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <typeparam name="TMemberType"></typeparam>
        /// <param name="members">待匹配的成员信息集合</param>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>成员信息集合</returns>
        protected static TMemberType[] Find<TMemberType, TAttributeType>(TMemberType[] members, bool isAttribute, bool isBaseType, bool isInheritAttribute)
            where TMemberType : MemberIndexPlus
            where TAttributeType : IgnoreMemberPlus
        {
            return members.GetFindArray(value => isAttribute ? value.IsAttribute<TAttributeType>(isBaseType, isInheritAttribute) : !value.IsIgnoreAttribute<TAttributeType>(isBaseType, isInheritAttribute));
        }
    }
    /// <summary>
    /// 成员索引
    /// </summary>
    /// <typeparam name="TMemberType">成员类型</typeparam>
    internal abstract class MemberIndexPlus<TMemberType> : MemberIndexPlus where TMemberType : MemberInfo
    {
        /// <summary>
        /// 成员信息
        /// </summary>
        public new TMemberType Member;
        /// <summary>
        /// 成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        protected MemberIndexPlus(TMemberType member, MemberFiltersEnum filter, int index)
            : base(member, filter, index)
        {
            Member = member;
        }
    }
}
