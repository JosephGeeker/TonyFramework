//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FieldInfoPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  FieldInfoPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:44:33
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
    /// 字段信息扩展
    /// </summary>
    public static class FieldInfoPlus
    {
        /// <summary>
        /// 字段信息类型
        /// </summary>
        public static readonly Type RtFieldInfoType = typeof(FieldInfo).Assembly.GetType("System.Reflection.RtFieldInfo");
    }
}
