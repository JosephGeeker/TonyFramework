//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SqlModelPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  SqlModelPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 15:11:52
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 数据库表格模型
    /// </summary>
    internal static class SqlModelPlus
    {
        /// <summary>
        /// 数据列验证动态函数
        /// </summary>
        public struct verifyDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;
            /// <summary>
            /// 
            /// </summary>
            private ILGenerator generator;
            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public verifyDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelVerify", typeof(bool), new Type[] { type, typeof(fastCSharp.code.memberMap), typeof(fastCSharp.emit.sqlTable.sqlTool) }, type, true);
                generator = dynamicMethod.GetILGenerator();
            }
            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(fastCSharp.code.cSharp.sqlModel.fieldInfo field)
            {
                Label end = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, field.MemberMapIndex);
                generator.Emit(OpCodes.Callvirt, pub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, end);
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Call, sqlColumn.verifyDynamicMethod.GetTypeVerifyer(field.DataType));
                    generator.Emit(OpCodes.Brtrue_S, end);
                }
                else if (field.DataType == typeof(string))
                {
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    MethodInfo castMethod = pub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Ldc_I4, field.DataMember.MaxStringLength);
                    generator.Emit(field.DataMember.IsAscii ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    generator.Emit(field.DataMember.IsNull ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    generator.Emit(OpCodes.Callvirt, fastCSharp.emit.sqlTable.sqlTool.StringVerifyMethod);
                    generator.Emit(OpCodes.Brtrue_S, end);
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    MethodInfo castMethod = pub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Brtrue_S, end);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Callvirt, fastCSharp.emit.sqlTable.sqlTool.NullVerifyMethod);
                }
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ret);
                generator.MarkLabel(end);
            }
            /// <summary>
            /// 创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
        /// <summary>
        /// 数据库模型设置动态函数
        /// </summary>
        public struct setDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;
            /// <summary>
            /// 
            /// </summary>
            private ILGenerator generator;
            /// <summary>
            /// 
            /// </summary>
            private LocalBuilder indexMember;
            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public setDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelSet", null, new Type[] { typeof(DbDataReader), type, typeof(fastCSharp.code.memberMap) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                indexMember = generator.DeclareLocal(typeof(int));
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Stloc_0);
            }
            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(fastCSharp.code.cSharp.sqlModel.fieldInfo field)
            {
                Label notMember = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Ldc_I4, field.MemberMapIndex);
                generator.Emit(OpCodes.Callvirt, pub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, notMember);
                if (field.DataReaderMethod == null)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldflda, field.Field);
                    generator.Emit(OpCodes.Ldloca_S, indexMember);
                    generator.Emit(OpCodes.Call, sqlColumn.setDynamicMethod.GetTypeSetter(field.DataType));
                }
                else
                {
                    if (field.DataType == field.NullableDataType && (field.DataType.IsValueType || !field.DataMember.IsNull))
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldloc_0);
                        generator.Emit(OpCodes.Callvirt, field.DataReaderMethod);
                        MethodInfo castMethod = pub.GetCastMethod(field.DataType, field.Field.FieldType);
                        if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                        generator.Emit(OpCodes.Stfld, field.Field);
                    }
                    else
                    {
                        Label notNull = generator.DefineLabel(), end = generator.DefineLabel();
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldloc_0);
                        generator.Emit(OpCodes.Callvirt, pub.DataReaderIsDBNullMethod);
                        generator.Emit(OpCodes.Brfalse_S, notNull);

                        generator.Emit(OpCodes.Ldarg_1);
                        if (field.DataType == field.NullableDataType)
                        {
                            generator.Emit(OpCodes.Ldnull);
                            generator.Emit(OpCodes.Stfld, field.Field);
                        }
                        else
                        {
                            generator.Emit(OpCodes.Ldflda, field.Field);
                            generator.Emit(OpCodes.Initobj, field.Field.FieldType);
                        }
                        generator.Emit(OpCodes.Br_S, end);
                        generator.MarkLabel(notNull);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldloc_0);
                        generator.Emit(OpCodes.Callvirt, field.DataReaderMethod);
                        if (field.DataType == field.NullableDataType)
                        {
                            MethodInfo castMethod = pub.GetCastMethod(field.DataType, field.Field.FieldType);
                            if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                        }
                        else generator.Emit(OpCodes.Newobj, pub.NullableConstructors[field.DataType]);
                        generator.Emit(OpCodes.Stfld, field.Field);
                        generator.MarkLabel(end);
                    }
                    generator.Emit(OpCodes.Ldloc_0);
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Add);
                    generator.Emit(OpCodes.Stloc_0);
                }
                generator.MarkLabel(notMember);
            }
            /// <summary>
            /// 创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
        /// <summary>
        /// 数据列转换数组动态函数
        /// </summary>
        public struct toArrayDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;
            /// <summary>
            /// 
            /// </summary>
            private ILGenerator generator;
            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public toArrayDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelToArray", null, new Type[] { type, typeof(object[]), pub.RefIntType }, type, true);
                generator = dynamicMethod.GetILGenerator();
            }
            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(fastCSharp.code.cSharp.sqlModel.fieldInfo field)
            {
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, sqlColumn.toArrayDynamicMethod.GetTypeToArray(field.DataType));
                }
                else
                {
                    if (field.DataType == field.NullableDataType)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldfld, field.Field);
                        MethodInfo castMethod = pub.GetCastMethod(field.Field.FieldType, field.DataType);
                        if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                        if (field.DataType.IsValueType) generator.Emit(OpCodes.Box, field.DataType);
                        generator.Emit(OpCodes.Stelem_Ref);
                    }
                    else
                    {
                        Label end = generator.DefineLabel();
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldflda, field.Field);
                        generator.Emit(OpCodes.Call, pub.GetNullableHasValue(field.NullableDataType));
                        generator.Emit(OpCodes.Brtrue_S, end);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldflda, field.Field);
                        generator.Emit(OpCodes.Call, pub.GetNullableValue(field.NullableDataType));
                        generator.Emit(OpCodes.Box, field.DataType);
                        generator.Emit(OpCodes.Stelem_Ref);
                        generator.MarkLabel(end);
                    }
                }
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldind_I4);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Add);
                generator.Emit(OpCodes.Stind_I4);
            }
            /// <summary>
            /// 创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
        /// <summary>
        /// 添加数据动态函数
        /// </summary>
        public struct insertDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;
            /// <summary>
            /// 
            /// </summary>
            private ILGenerator generator;
            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public insertDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelInsert", null, new Type[] { typeof(charStream), typeof(fastCSharp.code.memberMap), type, typeof(constantConverter) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                generator.DeclareLocal(typeof(int));
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Stloc_0);
            }
            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(fastCSharp.code.cSharp.sqlModel.fieldInfo field)
            {
                Label end = generator.DefineLabel(), isNext = generator.DefineLabel(), insert = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, field.MemberMapIndex);
                generator.Emit(OpCodes.Callvirt, pub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, end);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Brtrue_S, isNext);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Br_S, insert);
                generator.MarkLabel(isNext);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4_S, (byte)',');
                generator.Emit(OpCodes.Callvirt, pub.CharStreamWriteCharMethod);
                generator.MarkLabel(insert);
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Call, sqlColumn.insertDynamicMethod.GetTypeInsert(field.DataType));
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    MethodInfo castMethod = pub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Callvirt, pub.GetSqlConverterMethod(field.DataType));
                }
                generator.MarkLabel(end);
            }
            /// <summary>
            /// 创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
        /// <summary>
        /// 更新数据动态函数
        /// </summary>
        public struct updateDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;
            /// <summary>
            /// 
            /// </summary>
            private ILGenerator generator;
            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public updateDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelUpdate", null, new Type[] { typeof(charStream), typeof(fastCSharp.code.memberMap), type, typeof(constantConverter) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                generator.DeclareLocal(typeof(int));
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Stloc_0);
            }
            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(fastCSharp.code.cSharp.sqlModel.fieldInfo field)
            {
                Label end = generator.DefineLabel(), isNext = generator.DefineLabel(), update = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, field.MemberMapIndex);
                generator.Emit(OpCodes.Callvirt, pub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, end);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Brtrue_S, isNext);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Br_S, update);
                generator.MarkLabel(isNext);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4_S, (byte)',');
                generator.Emit(OpCodes.Callvirt, pub.CharStreamWriteCharMethod);
                generator.MarkLabel(update);
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Call, sqlColumn.updateDynamicMethod.GetTypeUpdate(field.DataType));
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name + "=");
                    generator.Emit(OpCodes.Callvirt, pub.CharStreamWriteNotNullMethod);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    MethodInfo castMethod = pub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Callvirt, pub.GetSqlConverterMethod(field.DataType));
                }
                generator.MarkLabel(end);
            }
            /// <summary>
            /// 创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
        /// <summary>
        /// 关键字条件动态函数
        /// </summary>
        public struct primaryKeyWhereDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;
            /// <summary>
            /// 
            /// </summary>
            private ILGenerator generator;
            /// <summary>
            /// 
            /// </summary>
            private bool isNextMember;
            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public primaryKeyWhereDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelPrimaryKeyWhere", null, new Type[] { typeof(charStream), type, typeof(constantConverter) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                isNextMember = false;
            }
            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(fastCSharp.code.cSharp.sqlModel.fieldInfo field)
            {
                if (isNextMember)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldstr, " and ");
                    generator.Emit(OpCodes.Call, pub.CharStreamWriteNotNullMethod);
                }
                else isNextMember = true;
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Call, sqlColumn.updateDynamicMethod.GetTypeUpdate(field.DataType));
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name + "=");
                    generator.Emit(OpCodes.Call, pub.CharStreamWriteNotNullMethod);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    MethodInfo castMethod = pub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Callvirt, pub.GetSqlConverterMethod(field.DataType));
                }
            }
            /// <summary>
            /// 创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
    }
    /// <summary>
    /// 数据库表格模型
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public abstract class sqlModel<valueType> : databaseModel<valueType>
    {
        /// <summary>
        /// 数据列验证
        /// </summary>
        internal static class verify
        {
            /// <summary>
            /// 数据验证
            /// </summary>
            private static readonly Func<valueType, fastCSharp.code.memberMap, fastCSharp.emit.sqlTable.sqlTool, bool> verifyer;
            /// <summary>
            /// 数据验证
            /// </summary>
            /// <param name="value"></param>
            /// <param name="memberMap"></param>
            /// <param name="sqlTool"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool Verify(valueType value, fastCSharp.code.memberMap memberMap, fastCSharp.emit.sqlTable.sqlTool sqlTool)
            {
                return verifyer == null || verifyer(value, memberMap, sqlTool);
            }
            static verify()
            {
                if (attribute != null)
                {
                    subArray<fastCSharp.code.cSharp.sqlModel.fieldInfo> verifyFields = Fields.getFind(value => value.IsVerify);
                    if (verifyFields.Count != 0)
                    {
                        sqlModel.verifyDynamicMethod dynamicMethod = new sqlModel.verifyDynamicMethod(typeof(valueType));
                        foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo member in verifyFields) dynamicMethod.Push(member);
                        verifyer = (Func<valueType, fastCSharp.code.memberMap, fastCSharp.emit.sqlTable.sqlTool, bool>)dynamicMethod.Create<Func<valueType, fastCSharp.code.memberMap, fastCSharp.emit.sqlTable.sqlTool, bool>>();
                    }
                }
            }
        }
        /// <summary>
        /// 数据库模型设置
        /// </summary>
        internal static class set
        {
            /// <summary>
            /// 默认数据列设置
            /// </summary>
            private static readonly Action<DbDataReader, valueType, fastCSharp.code.memberMap> setter;
            /// <summary>
            /// 设置字段值
            /// </summary>
            /// <param name="reader">字段读取器物理存储</param>
            /// <param name="value">目标数据</param>
            /// <param name="memberMap">成员位图</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Set(DbDataReader reader, valueType value, fastCSharp.code.memberMap memberMap)
            {
                if (setter != null) setter(reader, value, memberMap);
            }

            static set()
            {
                if (attribute != null)
                {
                    sqlModel.setDynamicMethod dynamicMethod = new sqlModel.setDynamicMethod(typeof(valueType));
                    foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo member in Fields) dynamicMethod.Push(member);
                    setter = (Action<DbDataReader, valueType, fastCSharp.code.memberMap>)dynamicMethod.Create<Action<DbDataReader, valueType, fastCSharp.code.memberMap>>();
                }
            }
        }
        /// <summary>
        /// 数据列转换数组
        /// </summary>
        internal static class toArray
        {
            /// <summary>
            /// 导入数据列集合
            /// </summary>
            private static keyValue<string, Type>[] dataColumns;
            /// <summary>
            /// 导入数据列集合
            /// </summary>
            internal static keyValue<string, Type>[] DataColumns
            {
                get
                {
                    if (dataColumns == null)
                    {
                        subArray<keyValue<string, Type>> columns = new subArray<keyValue<string, Type>>(Fields.Length);
                        foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo field in Fields)
                        {
                            if (field.IsSqlColumn) columns.Add(sqlColumn.toArrayDynamicMethod.GetDataColumns(field.DataType)(field.Field.Name));
                            else columns.Add(new keyValue<string, Type>(field.Field.Name, field.DataType));
                        }
                        dataColumns = columns.ToArray();
                    }
                    return dataColumns;
                }
            }
            /// <summary>
            /// 数据列转换数组
            /// </summary>
            /// <param name="values">目标数组</param>
            /// <param name="value">数据列</param>
            /// <param name="index">当前读取位置</param>
            private delegate void writer(valueType value, object[] values, ref int index);
            /// <summary>
            /// 数据列转换数组
            /// </summary>
            private static readonly writer defaultWriter;
            /// <summary>
            /// 数据列转换数组
            /// </summary>
            /// <param name="values">目标数组</param>
            /// <param name="value">数据列</param>
            /// <param name="index">当前读取位置</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ToArray(valueType value, object[] values, ref int index)
            {
                if (defaultWriter != null) defaultWriter(value, values, ref index);
            }

            static toArray()
            {
                if (attribute != null)
                {
                    sqlModel.toArrayDynamicMethod dynamicMethod = new sqlModel.toArrayDynamicMethod(typeof(valueType));
                    foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo member in Fields) dynamicMethod.Push(member);
                    defaultWriter = (writer)dynamicMethod.Create<writer>();
                }
            }
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        internal static class insert
        {
            /// <summary>
            /// 获取逗号分割的列名集合
            /// </summary>
            /// <param name="sqlStream"></param>
            /// <param name="memberMap"></param>
            public static void GetColumnNames(charStream sqlStream, fastCSharp.code.memberMap memberMap)
            {
                int isNext = 0;
                foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo member in Fields)
                {
                    if (memberMap.IsMember(member.MemberMapIndex) || member == Identity || member.DataMember.PrimaryKeyIndex != 0)
                    {
                        if (isNext == 0) isNext = 1;
                        else sqlStream.Write(',');
                        if (member.IsSqlColumn) sqlStream.WriteNotNull(sqlColumn.insertDynamicMethod.GetColumnNames(member.Field.FieldType)(member.Field.Name));
                        else sqlStream.WriteNotNull(member.Field.Name);
                    }
                }
            }
            /// <summary>
            /// 获取插入数据SQL表达式
            /// </summary>
            private static readonly Action<charStream, fastCSharp.code.memberMap, valueType, constantConverter> inserter;
            /// <summary>
            /// 获取插入数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="memberMap">成员位图</param>
            /// <param name="value">数据</param>
            /// <param name="converter">SQL常量转换</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Insert(charStream sqlStream, fastCSharp.code.memberMap memberMap, valueType value, constantConverter converter)
            {
                if (inserter != null) inserter(sqlStream, memberMap, value, converter);
            }
            static insert()
            {
                if (attribute != null)
                {
                    sqlModel.insertDynamicMethod dynamicMethod = new sqlModel.insertDynamicMethod(typeof(valueType));
                    foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo member in Fields) dynamicMethod.Push(member);
                    inserter = (Action<charStream, fastCSharp.code.memberMap, valueType, constantConverter>)dynamicMethod.Create<Action<charStream, fastCSharp.code.memberMap, valueType, constantConverter>>();
                }
            }
        }
        /// <summary>
        /// 数据列更新SQL流
        /// </summary>
        internal static class update
        {
            /// <summary>
            /// 获取更新数据SQL表达式
            /// </summary>
            private static readonly Action<charStream, fastCSharp.code.memberMap, valueType, constantConverter> updater;
            /// <summary>
            /// 获取更新数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="memberMap">更新成员位图</param>
            /// <param name="value">数据</param>
            /// <param name="converter">SQL常量转换</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Update(charStream sqlStream, fastCSharp.code.memberMap memberMap, valueType value, constantConverter converter)
            {
                if (updater != null) updater(sqlStream, memberMap, value, converter);
            }

            static update()
            {
                if (attribute != null)
                {
                    sqlModel.updateDynamicMethod dynamicMethod = new sqlModel.updateDynamicMethod(typeof(valueType));
                    foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo member in Fields) dynamicMethod.Push(member);
                    updater = (Action<charStream, fastCSharp.code.memberMap, valueType, constantConverter>)dynamicMethod.Create<Action<charStream, fastCSharp.code.memberMap, valueType, constantConverter>>();
                }
            }
        }
        /// <summary>
        /// 关键字条件
        /// </summary>
        internal static class primaryKeyWhere
        {
            /// <summary>
            /// 关键字条件SQL流
            /// </summary>
            private static readonly Action<charStream, valueType, constantConverter> where;
            /// <summary>
            /// 关键字条件SQL流
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Where(charStream sqlStream, valueType value, constantConverter converter)
            {
                if (where != null) where(sqlStream, value, converter);
            }

            static primaryKeyWhere()
            {
                if (attribute != null && PrimaryKeys.Length != 0)
                {
                    sqlModel.primaryKeyWhereDynamicMethod dynamicMethod = new sqlModel.primaryKeyWhereDynamicMethod(typeof(valueType));
                    foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo member in PrimaryKeys) dynamicMethod.Push(member);
                    where = (Action<charStream, valueType, constantConverter>)dynamicMethod.Create<Action<charStream, valueType, constantConverter>>();
                }
            }
        }
        /// <summary>
        /// 数据库表格模型配置
        /// </summary>
        private static readonly fastCSharp.code.cSharp.sqlModel attribute;
        /// <summary>
        /// 字段集合
        /// </summary>
        internal static readonly fastCSharp.code.cSharp.sqlModel.fieldInfo[] Fields;
        /// <summary>
        /// 自增字段
        /// </summary>
        internal static readonly fastCSharp.code.cSharp.sqlModel.fieldInfo Identity;
        /// <summary>
        /// 关键字字段集合
        /// </summary>
        internal static readonly fastCSharp.code.cSharp.sqlModel.fieldInfo[] PrimaryKeys;
        /// <summary>
        /// SQL数据成员
        /// </summary>
        internal static readonly fastCSharp.code.memberMap MemberMap;
        /// <summary>
        /// SQL数据成员
        /// </summary>
        public static fastCSharp.code.memberMap CopyMemberMap
        {
            get { return MemberMap.Copy(); }
        }
        /// <summary>
        /// 分组数据成员位图
        /// </summary>
        private static keyValue<fastCSharp.code.memberMap, int>[] groupMemberMaps;
        /// <summary>
        /// 分组数据成员位图访问锁
        /// </summary>
        private static int groupMemberMapLock;
        /// <summary>
        /// 自增标识获取器
        /// </summary>
        public static readonly Func<valueType, long> GetIdentity;
        /// <summary>
        /// 自增标识获取器
        /// </summary>
        public static readonly Func<valueType, int> GetIdentity32;
        /// <summary>
        /// 设置自增标识
        /// </summary>
        internal static readonly Action<valueType, long> SetIdentity;
        /// <summary>
        /// 获取以逗号分割的名称集合
        /// </summary>
        /// <param name="sqlStream"></param>
        /// <param name="memberMap"></param>
        internal static void GetNames(charStream sqlStream, fastCSharp.code.memberMap memberMap)
        {
            int isNext = 0;
            foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo field in Fields)
            {
                if (memberMap.IsMember(field.MemberMapIndex))
                {
                    if (isNext == 0) isNext = 1;
                    else sqlStream.Write(',');
                    if (field.IsSqlColumn) sqlStream.WriteNotNull(field.GetSqlColumnName());
                    else sqlStream.WriteNotNull(field.Field.Name);
                }
            }
        }
        /// <summary>
        /// 获取表格信息
        /// </summary>
        /// <param name="type">SQL绑定类型</param>
        /// <param name="sqlTable">SQL表格信息</param>
        /// <returns>表格信息</returns>
        internal static MemoryDataBaseTablePlus.table GetTable(Type type, sqlTable sqlTable)
        {
            client client = connection.GetConnection(sqlTable.ConnectionType).Client;
            MemoryDataBaseTablePlus.table table = new MemoryDataBaseTablePlus.table { Columns = new columnCollection { Name = sqlTable.GetTableName(type) } };
            column[] columns = new column[Fields.Length];
            column[] primaryKeyColumns = new column[PrimaryKeys.Length];
            int index = 0, primaryKeyIndex = 0;
            foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo member in Fields)
            {
                column column = client.GetColumn(member.Field.Name, member.Field.FieldType, member.DataMember);
                columns[index++] = column;
                if (Identity == member) table.Identity = column;
                if (member.DataMember.PrimaryKeyIndex != 0) primaryKeyColumns[primaryKeyIndex++] = column;
            }
            table.Columns.Columns = columns;
            if (primaryKeyColumns.Length != 0)
            {
                table.PrimaryKey = new columnCollection
                {
                    Columns = PrimaryKeys.getArray(value => primaryKeyColumns.firstOrDefault(column => column.Name == value.Field.Name))
                };
            }
            table.Columns.Name = sqlTable.GetTableName(type);
            return table;
        }
        /// <summary>
        /// 获取分组数据成员位图
        /// </summary>
        /// <param name="group">分组</param>
        /// <returns>分组数据成员位图</returns>
        private static fastCSharp.code.memberMap getGroupMemberMap(int group)
        {
            if (groupMemberMaps == null)
            {
                subArray<keyValue<fastCSharp.code.memberMap, int>> memberMaps = new subArray<keyValue<code.memberMap, int>>();
                memberMaps.Add(new keyValue<fastCSharp.code.memberMap, int>(MemberMap, 0));
                interlocked.NoCheckCompareSetSleep0(ref groupMemberMapLock);
                if (groupMemberMaps == null)
                {
                    try
                    {
                        foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo field in Fields)
                        {
                            if (field.DataMember.Group != 0)
                            {
                                int index = memberMaps.Count;
                                foreach (keyValue<fastCSharp.code.memberMap, int> memberMap in memberMaps.array)
                                {
                                    if (memberMap.Value == field.DataMember.Group || --index == 0) break;
                                }
                                if (index == 0)
                                {
                                    fastCSharp.code.memberMap memberMap = fastCSharp.code.memberMap<valueType>.New();
                                    memberMaps.Add(new keyValue<fastCSharp.code.memberMap, int>(memberMap, field.DataMember.Group));
                                    memberMap.SetMember(field.MemberMapIndex);
                                }
                                else memberMaps.array[memberMaps.Count - index].Key.SetMember(field.MemberMapIndex);
                            }
                        }
                        if (memberMaps.Count != 1)
                        {
                            fastCSharp.code.memberMap memberMap = memberMaps.array[0].Key = fastCSharp.code.memberMap<valueType>.New();
                            foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo field in Fields)
                            {
                                if (field.DataMember.Group == 0) memberMap.SetMember(field.MemberMapIndex);
                            }
                        }
                        groupMemberMaps = memberMaps.ToArray();
                    }
                    finally { groupMemberMapLock = 0; }
                }
                else groupMemberMapLock = 0;
            }
            foreach (keyValue<fastCSharp.code.memberMap, int> memberMap in groupMemberMaps)
            {
                if (memberMap.Value == group) return memberMap.Key;
            }
            log.Error.Add(typeof(valueType).fullName() + " 缺少缓存分组 " + group.toString(), false, false);
            return null;
        }
        /// <summary>
        /// 获取分组数据成员位图
        /// </summary>
        /// <param name="group">分组</param>
        /// <returns>分组数据成员位图</returns>
        public static fastCSharp.code.memberMap GetCacheMemberMap(int group)
        {
            fastCSharp.code.memberMap memberMap = getGroupMemberMap(group);
            if (memberMap != null)
            {
                memberMap = memberMap.Copy();
                if (Identity != null) memberMap.SetMember(Identity.MemberMapIndex);
                else if (PrimaryKeys.Length != 0)
                {
                    foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo field in PrimaryKeys) memberMap.SetMember(field.MemberMapIndex);
                }
                return memberMap;
            }
            return null;
        }
        /// <summary>
        /// 获取自增标识获取器
        /// </summary>
        /// <param name="baseIdentity"></param>
        /// <returns></returns>
        public static Func<valueType, int> IdentityGetter(int baseIdentity)
        {
            if (baseIdentity == 0) return GetIdentity32;
            DynamicMethod dynamicMethod = new DynamicMethod("GetIdentity32_" + baseIdentity.toString(), typeof(int), new Type[] { typeof(valueType) }, typeof(valueType), true);
            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, Identity.Field);
            if (Identity.Field.FieldType != typeof(int) && Identity.Field.FieldType != typeof(uint)) generator.Emit(OpCodes.Conv_I4);
            generator.Emit(OpCodes.Ldc_I4, baseIdentity);
            generator.Emit(OpCodes.Sub);
            generator.Emit(OpCodes.Ret);
            return (Func<valueType, int>)dynamicMethod.CreateDelegate(typeof(Func<valueType, int>));
        }
        static sqlModel()
        {
            Type type = typeof(valueType);
            attribute = fastCSharp.code.typeAttribute.GetAttribute<fastCSharp.code.cSharp.sqlModel>(type, true, true) ?? fastCSharp.code.cSharp.sqlModel.Default;
            Fields = fastCSharp.code.cSharp.sqlModel.GetFields(fastCSharp.code.memberIndexGroup<valueType>.GetFields(attribute.MemberFilter)).ToArray();
            Identity = fastCSharp.code.cSharp.sqlModel.GetIdentity(Fields);
            PrimaryKeys = fastCSharp.code.cSharp.sqlModel.GetPrimaryKeys(Fields).ToArray();
            MemberMap = fastCSharp.code.memberMap<valueType>.New();
            foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo field in Fields) MemberMap.SetMember(field.MemberMapIndex);
            if (Identity != null)
            {
                DynamicMethod dynamicMethod = new DynamicMethod("GetSqlIdentity", typeof(long), new Type[] { type }, type, true);
                ILGenerator generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, Identity.Field);
                if (Identity.Field.FieldType != typeof(long) && Identity.Field.FieldType != typeof(ulong)) generator.Emit(OpCodes.Conv_I8);
                generator.Emit(OpCodes.Ret);
                GetIdentity = (Func<valueType, long>)dynamicMethod.CreateDelegate(typeof(Func<valueType, long>));

                dynamicMethod = new DynamicMethod("SetSqlIdentity", null, new Type[] { type, typeof(long) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                if (Identity.Field.FieldType != typeof(long) && Identity.Field.FieldType != typeof(ulong)) generator.Emit(OpCodes.Conv_I4);
                generator.Emit(OpCodes.Stfld, Identity.Field);
                generator.Emit(OpCodes.Ret);
                SetIdentity = (Action<valueType, long>)dynamicMethod.CreateDelegate(typeof(Action<valueType, long>));

                GetIdentity32 = getIdentityGetter32("GetSqlIdentity32", Identity.Field);
            }
        }
    }
}
