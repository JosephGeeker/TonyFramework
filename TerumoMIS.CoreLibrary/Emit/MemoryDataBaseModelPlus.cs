//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemoryDataBaseModelPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  MemoryDataBaseModelPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 14:56:54
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

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 内存数据库表格类型
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    internal abstract class MemoryDataBaseModelPlus<TValueType>:DataBaseModelPlus<TValueType>
    {
        /// <summary>
        /// 内存数据库表格模型配置
        /// </summary>
        internal static readonly fastCSharp.code.cSharp.memoryDatabaseModel Attribute;
        /// <summary>
        /// 内存数据库基本成员集合
        /// </summary>
        internal static readonly fastCSharp.code.memberMap MemberMap = fastCSharp.code.memberMap<TValueType>.Empty();
        /// <summary>
        /// 是否所有成员
        /// </summary>
        internal static readonly int IsAllMember;
        /// <summary>
        /// 设置自增标识
        /// </summary>
        internal static readonly Action<TValueType, int> SetIdentity;
        /// <summary>
        /// 自增标识获取器
        /// </summary>
        internal static readonly Func<TValueType, int> GetIdentity;
        /// <summary>
        /// 自增字段
        /// </summary>
        internal static readonly memoryDatabaseModel.fieldInfo Identity;
        /// <summary>
        /// 关键字集合
        /// </summary>
        internal static readonly memoryDatabaseModel.fieldInfo[] PrimaryKeys;
        /// <summary>
        /// 关键字集合
        /// </summary>
        internal static FieldInfo[] PrimaryKeyFields
        {
            get { return PrimaryKeys.getArray(value => value.Field); }
        }

        static MemoryDataBaseModelPlus()
        {
            Type type = typeof(TValueType);
            Attribute = fastCSharp.code.typeAttribute.GetAttribute<memoryDatabaseModel>(type, true, true) ?? memoryDatabaseModel.Default;
            code.fieldIndex[] fieldArray = fastCSharp.code.memberIndexGroup<TValueType>.GetFields(Attribute.MemberFilter);
            subArray<memoryDatabaseModel.fieldInfo> fields = new subArray<memoryDatabaseModel.fieldInfo>(), primaryKeys = new subArray<memoryDatabaseModel.fieldInfo>();
            memoryDatabaseModel.fieldInfo identity = default(memoryDatabaseModel.fieldInfo);
            int isCase = 0, isIdentity = 0;
            foreach (code.fieldIndex field in fieldArray)
            {
                Type memberType = field.Member.FieldType;
                if (!memberType.IsPointer && (!memberType.IsArray || memberType.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    dataMember memberAttribute = field.GetAttribute<dataMember>(true, true) ?? dataMember.NullDataMember;
                    if (memberAttribute.IsSetup)
                    {
                        fields.Add(new memoryDatabaseModel.fieldInfo(field, memberAttribute));
                        MemberMap.SetMember(field.MemberIndex);
                        if (isIdentity == 0)
                        {
                            if (memberAttribute != null && memberAttribute.IsIdentity)
                            {
                                identity = new memoryDatabaseModel.fieldInfo(field, memberAttribute);
                                isIdentity = 1;
                            }
                            else if (isCase == 0 && field.Member.Name == fastCSharp.config.memoryDatabase.Default.DefaultIdentityName)
                            {
                                identity = new memoryDatabaseModel.fieldInfo(field, memberAttribute);
                                isCase = 1;
                            }
                            else if (identity.Field == null && field.Member.Name.ToLower() == fastCSharp.config.memoryDatabase.Default.DefaultIdentityName) identity = new memoryDatabaseModel.fieldInfo(field, memberAttribute);
                        }
                        if (memberAttribute.PrimaryKeyIndex != 0) primaryKeys.Add(new memoryDatabaseModel.fieldInfo(field, memberAttribute));
                    }
                }
            }
            IsAllMember = fields.Count == fieldArray.Length ? 1 : 0;
            if ((Identity = identity).Field != null)
            {
                GetIdentity = getIdentityGetter32("GetMemoryDatabaseIdentity", identity.Field);
                SetIdentity = getIdentitySetter32("SetMemoryDatabaseIdentity", identity.Field);
            }
            PrimaryKeys = primaryKeys.ToArray();
        }
    }
}
