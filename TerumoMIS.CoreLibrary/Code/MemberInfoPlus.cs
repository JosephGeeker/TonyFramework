//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberInfoPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  MemberInfoPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 16:34:53
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
using TerumoMIS.CoreLibrary.Emit;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 成员信息
    /// </summary>
    public partial class MemberInfoPlus:MemberIndexPlus
    {
        /// <summary>
        /// 成员类型
        /// </summary>
        public MemberTypePlus MemberType { get; private set; }
        /// <summary>
        /// 成员名称
        /// </summary>
        public string MemberName { get; private set; }
        /// <summary>
        /// 成员信息
        /// </summary>
        /// <param name="type">成员类型</param>
        /// <param name="name">成员名称</param>
        /// <param name="index">成员编号</param>
        public MemberInfoPlus(MemberTypePlus type, string name, int index)
            : base(index)
        {
            MemberType = type;
            MemberName = name;
        }
        /// <summary>
        /// 成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        internal MemberInfoPlus(MemberIndexPlus member)
            : base(member)
        {
            MemberType = member.Type;
            MemberName = Member.Name;
            if (CanGet && CanSet)
            {
                DataMemberPlus sqlMember = GetAttribute<DataMemberPlus>(true, false);
                if (sqlMember != null && sqlMember.DataType != null) MemberType = new MemberTypePlus(MemberType, sqlMember.DataType);
            }
        }
        /// <summary>
        /// 成员信息
        /// </summary>
        /// <param name="method">成员方法信息</param>
        /// <param name="filter">选择类型</param>
        protected MemberInfoPlus(MethodInfo method, MemberFiltersEnum filter)
            : base(method, filter, 0)
        {
            Member = method;
            MemberName = method.Name;
        }
        /// <summary>
        /// 类型成员集合缓存
        /// </summary>
        private static readonly Dictionary<Type, MemberInfoPlus[]> MemberCache = DictionaryPlus.CreateOnly<Type, MemberInfoPlus[]>();
        /// <summary>
        /// 根据类型获取成员信息集合
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>成员信息集合</returns>
        private static MemberInfoPlus[] GetMembers(Type type)
        {
            MemberInfoPlus[] members;
            if (!MemberCache.TryGetValue(type, out members))
            {
                memberIndexGroup group = memberIndexGroup.Get(type);
                MemberCache[type] = members =
                    ArrayPlus.Concat(group.PublicFields.getArray(value => new memberInfo(value)),
                        group.NonPublicFields.getArray(value => new memberInfo(value)),
                        group.PublicProperties.getArray(value => new memberInfo(value)),
                        group.NonPublicProperties.getArray(value => new memberInfo(value)));
            }
            return members;
        }

        /// <summary>
        /// 根据类型获取成员信息集合
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filter">选择类型</param>
        /// <returns>成员信息集合</returns>
        public static MemberInfoPlus[] GetMembers(Type type, MemberFiltersEnum filter)
        {
            return GetMembers(type).GetFindArray(value => (value.Filter & filter) != 0);
        }

        /// <summary>
        /// 根据类型获取成员信息集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type"></param>
        /// <param name="filter">选择类型</param>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>成员信息集合</returns>
        public static MemberInfoPlus[] GetMembers<TAttributeType>(Type type, MemberFiltersEnum filter, bool isAttribute, bool isBaseType, bool isInheritAttribute)
            where TAttributeType : IgnoreMemberPlus
        {
            return Find<MemberInfoPlus, TAttributeType>(GetMembers(type, filter), isAttribute, isBaseType, isInheritAttribute);
        }
    }
    /// <summary>
    /// 成员信息
    /// </summary>
    /// <typeparam name="TMemberType">成员类型</typeparam>
    internal abstract class MemberInfoPlus<TMemberType> : MemberInfoPlus where TMemberType : MemberInfo
    {
        /// <summary>
        /// 成员信息
        /// </summary>
        public new TMemberType Member;
        /// <summary>
        /// 成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        protected MemberInfoPlus(MemberIndexPlus<TMemberType> member)
            : base(member)
        {
            Member = member.Member;
        }
    }
}
