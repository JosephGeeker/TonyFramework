//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SqlModelPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  SqlModelPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 15:16:36
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
using TerumoMIS.CoreLibrary.Emit;

namespace TerumoMIS.CoreLibrary.Code.Csharper
{
    /// <summary>
    /// 数据库表格模型配置
    /// </summary>
    public class SqlModelPlus:DataModelPlus
    {
        /// <summary>
        /// 默认空属性
        /// </summary>
        internal static readonly sqlModel<> Default = new sqlModel();
        /// <summary>
        /// 空自增列成员索引
        /// </summary>
        internal const int NullIdentityMemberIndex = -1;

        /// <summary>
        /// 字段信息
        /// </summary>
        public class fieldInfo
        {
            /// <summary>
            /// 字段信息
            /// </summary>
            public FieldInfo Field { get; private set; }
            /// <summary>
            /// 可空类型数据库数据类型
            /// </summary>
            internal Type NullableDataType;
            /// <summary>
            /// 数据库数据类型
            /// </summary>
            internal Type DataType;
            /// <summary>
            /// 数据库成员信息
            /// </summary>
            public dataMember DataMember { get; private set; }
            /// <summary>
            /// 数据读取函数
            /// </summary>
            internal MethodInfo DataReaderMethod;
            /// <summary>
            /// 成员位图索引
            /// </summary>
            internal int MemberMapIndex;
            /// <summary>
            /// 是否数据列
            /// </summary>
            internal bool IsSqlColumn;
            /// <summary>
            /// 是否有效字段
            /// </summary>
            internal bool IsField;
            /// <summary>
            /// 是否需要验证
            /// </summary>
            internal bool IsVerify
            {
                get
                {
                    if (IsSqlColumn)
                    {
                        bool isVerify;
                        if (verifyTypes.TryGetValue(DataType, out isVerify)) return isVerify;
                        isVerify = typeof(sqlColumn<>.verify).MakeGenericType(DataType).GetField("verifyer", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) != null
                            || typeof(sqlColumn<>).MakeGenericType(DataType).GetField("custom", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) != null;
                        verifyTypes.Set(DataType, isVerify);
                        return isVerify;
                    }
                    if (!DataMember.IsDefaultMember)
                    {
                        if (DataType == typeof(string)) return DataMember.MaxStringLength > 0;
                        return DataType.IsClass && !DataMember.IsNull;
                    }
                    return false;
                }
            }
            /// <summary>
            /// 获取数据列名称
            /// </summary>
            internal Func<string, string> getSqlColumnName;
            /// <summary>
            /// 获取数据列名称
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public string GetSqlColumnName()
            {
                if (getSqlColumnName == null) getSqlColumnName = sqlColumn.insertDynamicMethod.GetColumnNames(Field.FieldType);
                return getSqlColumnName(Field.Name);
            }
            /// <summary>
            /// 字段信息
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="attribute">数据库成员信息</param>
            internal fieldInfo(fieldIndex field, dataMember attribute)
            {
                Field = field.Member;
                MemberMapIndex = field.MemberIndex;
                DataMember = dataMember.FormatSql(attribute, Field.FieldType, ref IsSqlColumn);
                if ((NullableDataType = DataMember.DataType) == null) NullableDataType = Field.FieldType;
                if ((DataReaderMethod = fastCSharp.emit.pub.GetDataReaderMethod(DataType = NullableDataType.nullableType() ?? NullableDataType)) == null)
                {
                    if (IsSqlColumn && isSqlColumn(DataType)) IsField = true;
                }
                else IsField = true;
            }
            /// <summary>
            /// 数据列验证类型集合
            /// </summary>
            private static interlocked.dictionary<Type, bool> verifyTypes = new interlocked.dictionary<Type, bool>(dictionary.CreateOnly<Type, bool>());
            /// <summary>
            /// 是否有效数据列
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            private static bool isSqlColumn(Type type)
            {
                bool isType;
                if (sqlColumnTypes.TryGetValue(type, out isType)) return isType;
                isType = typeof(sqlColumn<>.set).MakeGenericType(type).GetField("defaultSetter", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) != null
                    || typeof(sqlColumn<>).MakeGenericType(type).GetField("custom", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) != null;
                sqlColumnTypes.Set(type, isType);
                return isType;
            }
            /// <summary>
            /// 数据列类型集合
            /// </summary>
            private static interlocked.dictionary<Type, bool> sqlColumnTypes = new interlocked.dictionary<Type, bool>(dictionary.CreateOnly<Type, bool>());
        }
        /// <summary>
        /// 获取字段成员集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns>字段成员集合</returns>
        internal static subArray<fieldInfo> GetFields(fieldIndex[] fields)
        {
            subArray<fieldInfo> values = new subArray<fieldInfo>(fields.Length);
            foreach (fieldIndex field in fields)
            {
                Type type = field.Member.FieldType;
                if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    dataMember attribute = field.GetAttribute<dataMember>(true, true);
                    if (attribute == null || attribute.IsSetup)
                    {
                        fieldInfo fieldInfo = new fieldInfo(field, attribute);
                        if (fieldInfo.IsField) values.Add(fieldInfo);
                    }
                }
            }
            return values;
        }
        /// <summary>
        /// 获取关键字集合
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        internal static subArray<fieldInfo> GetPrimaryKeys(fieldInfo[] fields)
        {
            return fields.getFind(value => value.DataMember.PrimaryKeyIndex != 0)
                .Sort((left, right) =>
                {
                    int value = left.DataMember.PrimaryKeyIndex - right.DataMember.PrimaryKeyIndex;
                    return value == 0 ? left.Field.Name.CompareTo(right.Field.Name) : value;
                });
        }
        /// <summary>
        /// 获取自增标识
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static fieldInfo GetIdentity(fieldInfo[] fields)
        {
            fieldInfo identity = null;
            int isCase = 0;
            foreach (fieldInfo field in fields)
            {
                if (field.DataMember.IsIdentity) return field;
                if (isCase == 0 && field.Field.Name == fastCSharp.config.sql.Default.DefaultIdentityName)
                {
                    identity = field;
                    isCase = 1;
                }
                else if (identity == null && field.Field.Name.ToLower() == fastCSharp.config.sql.Default.DefaultIdentityName) identity = field;
            }
            return identity;
        }

        /// <summary>
        /// 获取数据库成员信息集合 
        /// </summary>
        /// <param name="type">数据库绑定类型</param>
        /// <param name="database">数据库配置</param>
        /// <returns>数据库成员信息集合</returns>
        internal static keyValue<memberIndex, dataMember>[] GetMemberIndexs<attributeType>(Type type, attributeType database)
             where attributeType : memberFilter
        {//showjim
            return GetMembers(memberIndexGroup.Get(type).Find<dataMember>(database));
        }
        /// <summary>
        /// 获取数据库成员信息集合
        /// </summary>
        /// <typeparam name="memberType">成员类型</typeparam>
        /// <param name="members">成员集合</param>
        /// <returns>数据库成员信息集合</returns>
        public static keyValue<memberType, dataMember>[] GetMembers<memberType>(memberType[] members) where memberType : memberIndex
        {
            return members.getFind(value => value.CanSet && value.CanGet)
                .GetArray(value => new keyValue<memberType, dataMember>(value, dataMember.Get(value)));
        }
    }
}
