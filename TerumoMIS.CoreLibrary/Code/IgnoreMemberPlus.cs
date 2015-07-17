//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: IgnoreMemberPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  IgnoreMemberPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 15:18:55
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    ///     禁止安装属性
    /// </summary>
    public abstract class IgnoreMemberPlus : Attribute
    {
        /// <summary>
        ///     是否禁止当前安装
        /// </summary>
        public bool IsIgnoreCurrent;

        /// <summary>
        ///     是否安装
        /// </summary>
        public bool IsSetup
        {
            get { return !IsIgnoreCurrent; }
        }
    }
}