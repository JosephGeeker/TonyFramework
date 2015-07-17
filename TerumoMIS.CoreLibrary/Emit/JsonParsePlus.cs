//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: JsonParsePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  JsonParsePlus
//	User name:  C1400008
//	Location Time: 2015/7/13 14:48:33
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// Json解析类型配置
    /// </summary>
    public sealed class JsonParsePlus:MemberFilterPlus.publicInstance
    {
        /// <summary>
        /// 默认解析所有成员
        /// </summary>
        internal static readonly JsonParsePlus AllMember = new JsonParsePlus { Filter = code.memberFilters.Instance, IsAllMember = true, IsBaseType = false };
        /// <summary>
        /// 是否解析所有成员
        /// </summary>
        public bool IsAllMember;
        /// <summary>
        /// 是否作用与派生类型
        /// </summary>
        public bool IsBaseType = true;
        /// <summary>
        /// Json解析成员配置
        /// </summary>
        public sealed class MemberPlus : IgnoreMemberPlus
        {
            /// <summary>
            /// 是否默认解析成员
            /// </summary>
            public bool IsDefault;
        }
        /// <summary>
        /// 自定义类型解析函数标识配置
        /// </summary>
        public sealed class CustomPlus : Attribute
        {
        }
    }
}
