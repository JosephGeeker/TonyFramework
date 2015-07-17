//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DataMemberPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  DataMemberPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 14:06:56
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using TerumoMIS.CoreLibrary.Code;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 数据库成员信息
    /// </summary>
    public sealed class DataMemberPlus:IgnoreMemberPlus
    {
        /// <summary>
        /// 数据库成员信息空值
        /// </summary>
        internal static readonly DataMemberPlus NullDataMember = new DataMemberPlus();
        /// <summary>
        /// 数据库类型
        /// </summary>
        public Type DataType;
        /// <summary>
        /// 数据库成员类型
        /// </summary>
        private MemberTypePlus _dataMemberTypePlus;
        /// <summary>
        /// 数据库成员类型
        /// </summary>
        public MemberTypePlus DataMemberTypePlus
        {
            get
            {
                if (DataType == null) return null;
                if (_dataMemberTypePlus == null) _dataMemberTypePlus = DataType;
                return _dataMemberTypePlus;
            }
        }
        /// <summary>
        /// 枚举真实类型
        /// </summary>
        public MemberTypePlus EnumType { get; private set; }
        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIdentity;
        /// <summary>
        /// 主键索引,0标识非主键
        /// </summary>
        public int PrimaryKeyIndex;
        /// <summary>
        /// 分组标识
        /// </summary>
        public int Group;
        /// <summary>
        /// 是否允许空值
        /// </summary>
        public bool IsNull;
        /// <summary>
        /// 字符串是否ASCII
        /// </summary>
        public bool IsAscii;
        /// <summary>
        /// 是否固定长度
        /// </summary>
        public bool IsFixedLength;
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue;
        /// <summary>
        /// 新增字段时的计算子查询
        /// </summary>
        public string UpdateValue;
        /// <summary>
        /// 字符串最大长度验证
        /// </summary>
        public int MaxStringLength;
        /// <summary>
        /// 是否数据库成员信息空值
        /// </summary>
        internal bool IsDefaultMember
        {
            get
            {
                return this == NullDataMember;
            }
        }

        /// <summary>
        /// 格式化数据库成员信息
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="isSqlColumn"></param>
        /// <returns></returns>
        internal static DataMemberPlus FormatSql(DataMemberPlus value, Type type, ref bool isSqlColumn)
        {
            if (type.IsEnum)
            {
                Type enumType = Enum.GetUnderlyingType(type);
                if (value == null) return new DataMemberPlus { DataType = Enum.GetUnderlyingType(type) };
                if (value.DataType == null) value.DataType = Enum.GetUnderlyingType(type);
                else if (enumType != value.DataType) value.EnumType = enumType;
                return value;
            }
            Type nullableType = null;
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(fastCSharp.sql.jsonMember<>))
                {
                    if (value == null) return new DataMemberPlus { DataType = typeof(string) };
                    value.DataType = typeof(string);
                    return value;
                }
                if (genericType == typeof(fastCSharp.sql.fileBlockMember<>))
                {
                    if (value == null) return new DataMemberPlus { DataType = typeof(fastCSharp.io.fileBlockStream.index) };
                    value.DataType = typeof(fastCSharp.io.fileBlockStream.index);
                    return value;
                }
                if (genericType == typeof(Nullable<>)) nullableType = type.GetGenericArguments()[0];
            }
            else if (fastCSharp.code.typeAttribute.GetAttribute<SqlColumnPlus>(type, false, false) != null)
            {
                isSqlColumn = true;
                return NullDataMember;
            }
            if (value == null || value.DataType == null)
            {
                DataMemberPlus sqlMember = fastCSharp.code.typeAttribute.GetAttribute<DataMemberPlus>(type, false, false);
                if (sqlMember != null && sqlMember.DataType != null)
                {
                    if (value == null) value = new DataMemberPlus();
                    value.DataType = sqlMember.DataType;
                    if (sqlMember.DataType.IsValueType && sqlMember.DataType.IsGenericType && sqlMember.DataType.GetGenericTypeDefinition() == typeof(Nullable<>)) value.IsNull = true;
                }
            }
            if (value == null)
            {
                if (nullableType == null)
                {
                    Type dataType = type.formCSharpType().toCSharpType();
                    if (dataType != type) value = new DataMemberPlus { DataType = dataType };
                }
                else
                {
                    value = new DataMemberPlus { IsNull = true };
                    Type dataType = nullableType.formCSharpType().toCSharpType();
                    if (dataType != nullableType) value.DataType = dataType.toNullableType();
                }
            }
            return value ?? NullDataMember;
        }

        /// <summary>
        /// 获取数据库成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>数据库成员信息</returns>
        internal static DataMemberPlus Get(MemberIndexPlus member)
        {
            var value = member.GetAttribute<DataMemberPlus>(true, false);
            if (value == null || value.DataType == null)
            {
                if (member.Type.IsEnum)
                {
                    if (value == null) value = new DataMemberPlus();
                    value.DataType = Enum.GetUnderlyingType(member.Type);
                }
                else
                {
                    DataMemberPlus sqlMember = fastCSharp.code.typeAttribute.GetAttribute<DataMemberPlus>(member.Type, false, false);
                    if (sqlMember != null && sqlMember.DataType != null)
                    {
                        if (value == null) value = new DataMemberPlus();
                        value.DataType = sqlMember.DataType;
                        if (sqlMember.DataType.nullableType() != null) value.IsNull = true;
                    }
                }
            }
            else if (member.Type.IsEnum)
            {
                Type enumType = Enum.GetUnderlyingType(member.Type);
                if (enumType != value.DataType) value.EnumType = enumType;
            }
            if (value == null)
            {
                Type nullableType = member.Type.nullableType();
                if (nullableType == null)
                {
                    if (fastCSharp.code.typeAttribute.GetAttribute<SqlColumnPlus>(member.Type, false, false) == null)
                    {
                        Type dataType = member.Type.formCSharpType().toCSharpType();
                        if (dataType != member.Type)
                        {
                            value = new DataMemberPlus();
                            value.DataType = dataType;
                        }
                    }
                }
                else
                {
                    value = new DataMemberPlus();
                    value.IsNull = true;
                    Type dataType = nullableType.formCSharpType().toCSharpType();
                    if (dataType != nullableType) value.DataType = dataType.toNullableType();
                }
            }
            return value ?? NullDataMember;
        }
    }
}
