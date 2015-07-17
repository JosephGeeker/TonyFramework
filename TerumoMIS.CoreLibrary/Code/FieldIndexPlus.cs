//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FieldIndexPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  FieldIndexPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 16:21:02
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

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 字段索引
    /// </summary>
    internal sealed class FieldIndexPlus:MemberIndexPlus<FieldInfoPlus>
    {
        /// <summary>
        /// 字段信息
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        public fieldIndex(FieldInfo field, memberFilters filter, int index)
            : base(field, filter, index)
        {
            IsField = CanGet = true;
            CanSet = !field.IsInitOnly;
            Type = field.FieldType;
        }
    }
}
