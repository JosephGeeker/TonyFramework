//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberInfoPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  MemberInfoPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:45:59
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
    /// 成员属性相关操作
    /// </summary>
    public static class MemberInfoPlus
    {
        /// <summary>
        /// 根据成员属性获取自定义属性
        /// </summary>
        /// <typeparam name="attributeType">自定义属性类型</typeparam>
        /// <param name="member">成员属性</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <returns>自定义属性</returns>
        public static attributeType customAttribute<attributeType>(this MemberInfo member, bool isBaseType = false) where attributeType : Attribute
        {
            attributeType value = null;
            if (member != null)
            {
                foreach (object attribute in member.GetCustomAttributes(typeof(attributeType), isBaseType))
                {
                    if (attribute.GetType() == typeof(attributeType)) return attribute as attributeType;
                }
            }
            return value;
        }
    }
}
