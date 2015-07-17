//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: BinarySerializePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  BinarySerializePlus
//	User name:  C1400008
//	Location Time: 2015/7/13 11:06:47
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using TerumoMIS.CoreLibrary.Code;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    ///     二进制序列化配置
    /// </summary>
    public abstract class BinarySerializePlus : MemberFilterPlus.InstanceFieldPlus
    {
        /// <summary>
        ///     是否作用于未知派生类型
        /// </summary>
        public bool IsBaseType = true;

        /// <summary>
        ///     当没有JSON序列化成员时是否预留JSON序列化标记
        /// </summary>
        public bool IsJson;

        /// <summary>
        ///     二进制数据序列化成员配置
        /// </summary>
        public sealed class MemberPlus : IgnoreMemberPlus
        {
            /// <summary>
            ///     是否采用JSON混合序列化
            /// </summary>
            public bool IsJson;
        }
    }
}