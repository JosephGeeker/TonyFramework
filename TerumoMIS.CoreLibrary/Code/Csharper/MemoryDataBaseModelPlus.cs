//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemoryDataBaseModelPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  MemoryDataBaseModelPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:41:56
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

namespace TerumoMIS.CoreLibrary.Code.Csharper
{
    /// <summary>
    /// 内存数据库表格模型配置
    /// </summary>
    public class MemoryDataBaseModelPlus:DataModelPlus
    {
        /// <summary>
        /// 默认空属性
        /// </summary>
        internal static readonly memoryDatabaseModel Default = new memoryDatabaseModel();
        /// <summary>
        /// 字段信息
        /// </summary>
        internal struct fieldInfo
        {
            /// <summary>
            /// 字段信息
            /// </summary>
            public FieldInfo Field;
            /// <summary>
            /// 数据库成员信息
            /// </summary>
            public dataMember DataMember;
            /// <summary>
            /// 成员位图索引
            /// </summary>
            public int MemberMapIndex;
            /// <summary>
            /// 字段信息
            /// </summary>
            /// <param name="field"></param>
            /// <param name="attribute"></param>
            public fieldInfo(fieldIndex field, dataMember attribute)
            {
                Field = field.Member;
                DataMember = attribute;
                MemberMapIndex = field.MemberIndex;
            }
        }
    }
}
