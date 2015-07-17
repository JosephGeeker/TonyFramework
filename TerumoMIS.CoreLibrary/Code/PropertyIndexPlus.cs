//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PropertyIndexPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  PropertyIndexPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:19:46
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System.Reflection;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 属性索引
    /// </summary>
    internal sealed class PropertyIndexPlus:MemberIndexPlus<PropertyInfo>
    {
        /// <summary>
        /// 属性信息
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        public PropertyIndexPlus(PropertyInfo property, MemberFiltersEnum filter, int index)
            : base(property, filter, index)
        {
            CanSet = property.CanWrite;
            CanGet = property.CanRead;
            Type = property.PropertyType;
        }
    }
}
