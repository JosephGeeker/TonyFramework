//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: IndexSerializePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  IndexSerializePlus
//	User name:  C1400008
//	Location Time: 2015/7/13 14:30:22
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
    /// 二进制数据序列化设置（内存数据库专用）
    /// </summary>
    public sealed class IndexSerializePlus:BinarySerializePlus
    {
        /// <summary>
        /// 默认二进制数据序列化类型配置
        /// </summary>
        internal static readonly IndexSerializePlus Default = new IndexSerializePlus();
        /// <summary>
        /// 是否检测相同的引用成员
        /// </summary>
        public bool IsReferenceMember = true;
    }
}
