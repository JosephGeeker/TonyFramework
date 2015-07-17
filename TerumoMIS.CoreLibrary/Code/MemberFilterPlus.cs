//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberFilterPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  MemberFilterPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 15:23:50
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
    ///     成员选择
    /// </summary>
    public abstract class MemberFilterPlus : Attribute
    {
        /// <summary>
        ///     成员是否匹配自定义属性类型
        /// </summary>
        public bool IsAttribute;

        /// <summary>
        ///     是否搜索父类自定义属性
        /// </summary>
        public bool IsBaseTypeAttribute;

        /// <summary>
        ///     成员匹配自定义属性是否可继承
        /// </summary>
        public bool IsInheritAttribute = true;

        /// <summary>
        ///     成员选择类型
        /// </summary>
        public abstract MemberFiltersEnum MemberFilter { get; }

        /// <summary>
        ///     默认公有动态成员
        /// </summary>
        public abstract class InstancePlus : MemberFilterPlus
        {
            /// <summary>
            ///     成员选择类型
            /// </summary>
            public MemberFiltersEnum Filter = MemberFiltersEnum.Instance;

            /// <summary>
            ///     成员选择类型
            /// </summary>
            public override MemberFiltersEnum MemberFilter
            {
                get { return Filter & MemberFiltersEnum.Instance; }
            }

            /// <summary>
            ///     获取字段选择
            /// </summary>
            internal BindingFlags FieldBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & MemberFiltersEnum.PublicInstanceField) == MemberFiltersEnum.PublicInstanceField)
                        flags |= BindingFlags.Public;
                    if ((Filter & MemberFiltersEnum.NonPublicInstanceField) == MemberFiltersEnum.NonPublicInstanceField)
                        flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }

            /// <summary>
            ///     获取属性选择
            /// </summary>
            internal BindingFlags PropertyBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & MemberFiltersEnum.PublicInstanceProperty) == MemberFiltersEnum.PublicInstanceProperty)
                        flags |= BindingFlags.Public;
                    if ((Filter & MemberFiltersEnum.NonPublicInstanceProperty) ==
                        MemberFiltersEnum.NonPublicInstanceProperty) flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }
        }

        /// <summary>
        ///     默认公有动态成员
        /// </summary>
        public abstract class PublicInstancePlus : MemberFilterPlus
        {
            /// <summary>
            ///     成员选择类型
            /// </summary>
            public MemberFiltersEnum Filter = MemberFiltersEnum.PublicInstance;

            /// <summary>
            ///     成员选择类型
            /// </summary>
            public override MemberFiltersEnum MemberFilter
            {
                get { return Filter & MemberFiltersEnum.Instance; }
            }

            /// <summary>
            ///     获取字段选择
            /// </summary>
            internal BindingFlags FieldBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & MemberFiltersEnum.PublicInstanceField) == MemberFiltersEnum.PublicInstanceField)
                        flags |= BindingFlags.Public;
                    if ((Filter & MemberFiltersEnum.NonPublicInstanceField) == MemberFiltersEnum.NonPublicInstanceField)
                        flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }

            /// <summary>
            ///     获取属性选择
            /// </summary>
            internal BindingFlags PropertyBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & MemberFiltersEnum.PublicInstanceProperty) == MemberFiltersEnum.PublicInstanceProperty)
                        flags |= BindingFlags.Public;
                    if ((Filter & MemberFiltersEnum.NonPublicInstanceProperty) ==
                        MemberFiltersEnum.NonPublicInstanceProperty) flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }
        }

        /// <summary>
        ///     默认公有动态字段成员
        /// </summary>
        public abstract class PublicInstanceFieldPlus : MemberFilterPlus
        {
            /// <summary>
            ///     成员选择类型
            /// </summary>
            public MemberFiltersEnum Filter = MemberFiltersEnum.PublicInstanceField;

            /// <summary>
            ///     成员选择类型
            /// </summary>
            public override MemberFiltersEnum MemberFilter
            {
                get { return Filter & MemberFiltersEnum.Instance; }
            }

            /// <summary>
            ///     获取字段选择
            /// </summary>
            internal BindingFlags FieldBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & MemberFiltersEnum.PublicInstanceField) == MemberFiltersEnum.PublicInstanceField)
                        flags |= BindingFlags.Public;
                    if ((Filter & MemberFiltersEnum.NonPublicInstanceField) == MemberFiltersEnum.NonPublicInstanceField)
                        flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }
        }

        /// <summary>
        ///     默认动态字段成员
        /// </summary>
        public abstract class InstanceFieldPlus : MemberFilterPlus
        {
            /// <summary>
            ///     成员选择类型
            /// </summary>
            public MemberFiltersEnum Filter = MemberFiltersEnum.InstanceField;

            /// <summary>
            ///     成员选择类型
            /// </summary>
            public override MemberFiltersEnum MemberFilter
            {
                get { return Filter & MemberFiltersEnum.Instance; }
            }

            /// <summary>
            ///     获取字段选择
            /// </summary>
            public BindingFlags FieldBindingFlags
            {
                get
                {
                    var flags = BindingFlags.Default;
                    if ((Filter & MemberFiltersEnum.PublicInstanceField) == MemberFiltersEnum.PublicInstanceField)
                        flags |= BindingFlags.Public;
                    if ((Filter & MemberFiltersEnum.NonPublicInstanceField) == MemberFiltersEnum.NonPublicInstanceField)
                        flags |= BindingFlags.NonPublic;
                    return flags == BindingFlags.Default ? flags : (flags | BindingFlags.Instance);
                }
            }
        }
    }
}