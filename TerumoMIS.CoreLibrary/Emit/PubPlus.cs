//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PubPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  PubPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 10:40:49
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 公共类型
    /// </summary>
    public static partial class PubPlus
    {
        /// <summary>
        /// LGD
        /// </summary>
        internal const int PuzzleValue = 0x10035113;
        /// <summary>
        /// int引用参数类型
        /// </summary>
        internal static readonly Type RefIntType = typeof(int).MakeByRefType();
        /// <summary>
        /// 内存字符流写入字符串方法信息
        /// </summary>
        internal static readonly MethodInfo CharStreamWriteNotNullMethod = typeof(charStream).GetMethod("WriteNotNull", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
        /// <summary>
        /// 内存字符流写入字符方法信息
        /// </summary>
        internal static readonly MethodInfo CharStreamWriteCharMethod = typeof(charStream).GetMethod("Write", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(char) }, null);
        /// <summary>
        /// 字符流写入null方法信息
        /// </summary>
        internal static readonly MethodInfo CharStreamWriteNullMethod = typeof(fastCSharp.web.ajax).GetMethod("WriteNull", BindingFlags.Public | BindingFlags.Static);
        /// <summary>
        /// 内存流安全写入Int32方法信息
        /// </summary>
        internal static readonly MethodInfo UnmanagedStreamUnsafeWriteIntMethod = typeof(UnmanagedStreamPlus).GetMethod("UnsafeWrite", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);
        /// <summary>
        /// 内存流写入Int32方法信息
        /// </summary>
        internal static readonly MethodInfo UnmanagedStreamWriteIntMethod = typeof(UnmanagedStreamPlus).GetMethod("Write", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(int) }, null);
        /// <summary>
        /// 判断成员位图是否匹配成员索引
        /// </summary>
        internal static readonly MethodInfo MemberMapIsMemberMethod = memberMap.IsMemberMethod;

        /// <summary>
        /// 创建构造函数委托
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameterType">参数类型</param>
        /// <returns>构造函数委托</returns>
        public static Delegate CreateConstructor(Type type, Type parameterType)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("constructor", type, new Type[] { parameterType }, type, true);
            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg, 0);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { parameterType }, null));
            generator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(parameterType, type));
        }
        /// <summary>
        /// 获取数值转换委托调用函数信息
        /// </summary>
        /// <param name="type">数值类型</param>
        /// <returns>数值转换委托调用函数信息</returns>
        internal static MethodInfo GetNumberToCharStreamMethod(Type type)
        {
            return numberToCharStream.GetToStringMethod(type);
        }

        /// <summary>
        /// 可空类型是否为空判断函数信息集合
        /// </summary>
        private static interlocked.dictionary<Type, MethodInfo> _nullableHasValues = new interlocked.dictionary<Type,MethodInfo>(dictionary.CreateOnly<Type, MethodInfo>());
        /// <summary>
        /// 获取可空类型是否为空判断函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>可空类型是否为空判断函数信息</returns>
        internal static MethodInfo GetNullableHasValue(Type type)
        {
            MethodInfo method;
            if (_nullableHasValues.TryGetValue(type, out method)) return method;
            method = type.GetProperty("HasValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod();
            _nullableHasValues.Set(type, method);
            return method;
        }

        /// <summary>
        /// 可空类型获取数据函数信息集合
        /// </summary>
        private static interlocked.dictionary<Type, MethodInfo> _nullableValues = new interlocked.dictionary<Type,MethodInfo>(dictionary.CreateOnly<Type, MethodInfo>());
        /// <summary>
        /// 获取可空类型获取数据函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>可空类型获取数据函数信息</returns>
        internal static MethodInfo GetNullableValue(Type type)
        {
            MethodInfo method;
            if (_nullableValues.TryGetValue(type, out method)) return method;
            method = type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public).GetGetMethod();
            _nullableValues.Set(type, method);
            return method;
        }

        /// <summary>
        /// SQL常量转换函数信息集合
        /// </summary>
        private static interlocked.dictionary<Type, MethodInfo> _sqlConverterMethods = new interlocked.dictionary<Type,MethodInfo>(dictionary.CreateOnly<Type, MethodInfo>());
        /// <summary>
        /// 获取SQL常量转换函数信息
        /// </summary>
        /// <param name="type">数值类型</param>
        /// <returns>SQL常量转换函数信息</returns>
        internal static MethodInfo GetSqlConverterMethod(Type type)
        {
            MethodInfo method;
            if (_sqlConverterMethods.TryGetValue(type, out method)) return method;
            method = typeof(constantConverter).GetMethod("convertConstant", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(charStream), type }, null)
                ?? constantConverter.ConvertConstantStringMethod.MakeGenericMethod(type);
            _sqlConverterMethods.Set(type, method);
            return method;
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        private struct CastTypeStruct : IEquatable<CastTypeStruct>
        {
            /// <summary>
            /// 原始类型
            /// </summary>
            public Type FromType;
            /// <summary>
            /// 目标类型
            /// </summary>
            public Type ToType;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equals(CastTypeStruct other)
            {
                return FromType == other.FromType && ToType == other.ToType;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return FromType.GetHashCode() ^ ToType.GetHashCode();
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return Equals((CastTypeStruct)obj);
            }
        }
        /// <summary>
        /// 类型转换函数集合
        /// </summary>
        private static interlocked.dictionary<CastTypeStruct, MethodInfo> castMethods = new interlocked.dictionary<CastTypeStruct,MethodInfo>(dictionary<castType>.Create<MethodInfo>());
        /// <summary>
        /// 获取类型转换函数
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        internal static MethodInfo GetCastMethod(Type fromType, Type toType)
        {
            if (fromType == toType) return null;
            if (fromType == typeof(int))
            {
                if (toType == typeof(uint)) return null;
            }
            else if (fromType == typeof(long))
            {
                if (toType == typeof(ulong)) return null;
            }
            else if (fromType == typeof(byte))
            {
                if (toType == typeof(sbyte)) return null;
            }
            else if (fromType == typeof(short))
            {
                if (toType == typeof(ushort)) return null;
            }
            var castType = new CastTypeStruct { FromType = fromType, ToType = toType };
            MethodInfo method;
            if (castMethods.TryGetValue(castType, out method)) return method;
            if (!toType.IsPrimitive)
            {
                var castParameterTypes = new Type[] { fromType };
                method = toType.GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, castParameterTypes, null)
                    ?? toType.GetMethod("op_Explicit", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, castParameterTypes, null);
            }
            if (method == null && !fromType.IsPrimitive)
            {
                foreach (MethodInfo methodInfo in fromType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (methodInfo.ReturnType == toType && (methodInfo.Name == "op_Implicit" || methodInfo.Name == "op_Explicit") && methodInfo.GetParameters()[0].ParameterType == fromType)
                    {
                        method = methodInfo;
                        break;
                    }
                }
            }
            castMethods.Set(castType, method);
            return method;
        }

        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <typeparam name="TValueType">枚举类型</typeparam>
        /// <typeparam name="TIntType">枚举值数字类型</typeparam>
        public static class EnumCastEnum<TValueType, TIntType>
        {
            /// <summary>
            /// 枚举转数字委托
            /// </summary>
            public static readonly Func<TValueType, TIntType> ToInt;
            /// <summary>
            /// 数字转枚举委托
            /// </summary>
            public static readonly Func<TIntType, TValueType> FromInt;

            static EnumCastEnum()
            {
                DynamicMethod dynamicMethod = new DynamicMethod("To" + typeof(TIntType).FullName, typeof(TIntType), new[] { typeof(TValueType) }, typeof(TValueType), true);
                ILGenerator generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ret);
                ToInt = (Func<TValueType, TIntType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, TIntType>));

                dynamicMethod = new DynamicMethod("From" + typeof(TIntType).FullName, typeof(TValueType), new[] { typeof(TIntType) }, typeof(TValueType), true);
                generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ret);
                FromInt = (Func<TIntType, TValueType>)dynamicMethod.CreateDelegate(typeof(Func<TIntType, TValueType>));
            }
        }
        /// <summary>
        /// 集合构造函数
        /// </summary>
        /// <typeparam name="TDictionaryType">集合类型</typeparam>
        /// <typeparam name="TKeyType">枚举值类型</typeparam>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        internal static class DictionaryConstructorPlus<TDictionaryType, TKeyType, TValueType>
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            public static readonly Func<IDictionary<TKeyType, TValueType>, TDictionaryType> Constructor = (Func<IDictionary<TKeyType, TValueType>, TDictionaryType>)CreateConstructor(typeof(TDictionaryType), typeof(IDictionary<,>).MakeGenericType(typeof(TKeyType), typeof(TValueType)));
        }
        /// <summary>
        /// 集合构造函数
        /// </summary>
        /// <typeparam name="TValueType">集合类型</typeparam>
        /// <typeparam name="TArgumentType">枚举值类型</typeparam>
        internal static class ListConstructorPlus<TValueType, TArgumentType>
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            public static readonly Func<IList<TArgumentType>, TValueType> Constructor = (Func<IList<TArgumentType>, TValueType>)CreateConstructor(typeof(TValueType), typeof(IList<>).MakeGenericType(typeof(TArgumentType)));
        }
        /// <summary>
        /// 集合构造函数
        /// </summary>
        /// <typeparam name="TValueType">集合类型</typeparam>
        /// <typeparam name="TArgumentType">枚举值类型</typeparam>
        internal static class CollectionConstructorPlus<TValueType, TArgumentType>
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            public static readonly Func<ICollection<TArgumentType>, TValueType> Constructor = (Func<ICollection<TArgumentType>, TValueType>)CreateConstructor(typeof(TValueType), typeof(ICollection<>).MakeGenericType(typeof(TArgumentType)));
        }
        /// <summary>
        /// 集合构造函数
        /// </summary>
        /// <typeparam name="TValueType">集合类型</typeparam>
        /// <typeparam name="TArgumentType">枚举值类型</typeparam>
        public static class EnumerableConstructorEnum<TValueType, TArgumentType>
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            public static readonly Func<IEnumerable<TArgumentType>, TValueType> Constructor = (Func<IEnumerable<TArgumentType>, TValueType>)CreateConstructor(typeof(TValueType), typeof(IEnumerable<>).MakeGenericType(typeof(TArgumentType)));
        }
        /// <summary>
        /// 集合构造函数
        /// </summary>
        /// <typeparam name="TValueType">集合类型</typeparam>
        /// <typeparam name="TArgumentType">枚举值类型</typeparam>
        internal static class ArrayConstructorStruct<TValueType, TArgumentType>
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            public static readonly Func<TArgumentType[], TValueType> Constructor = (Func<TArgumentType[], TValueType>)CreateConstructor(typeof(TValueType), typeof(TArgumentType).MakeArrayType());
        }
        
        /// <summary>
        /// 判断数据是否为空
        /// </summary>
        internal static readonly MethodInfo DataReaderIsDBNullMethod = typeof(DbDataReader).GetMethod("IsDBNull", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
        /// <summary>
        /// 基本类型设置函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> DataReaderMethods;
        /// <summary>
        /// 获取基本类型设置函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>设置函数</returns>
        internal static MethodInfo GetDataReaderMethod(Type type)
        {
            MethodInfo method;
            return DataReaderMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        /// 获取字段成员集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TMemberAttribute"></typeparam>
        /// <param name="memberFilter"></param>
        /// <param name="isAllMember"></param>
        /// <returns>字段成员集合</returns>
        internal static SubArrayStruct<FieldInfo> GetFields<TValueType, TMemberAttribute>(memberFilters memberFilter, bool isAllMember)
            where TMemberAttribute : ignoreMember
        {
            fieldIndex[] fieldIndexs = fastCSharp.code.memberIndexGroup<TValueType>.GetFields(memberFilter);
            subArray<FieldInfo> fields = new subArray<FieldInfo>(fieldIndexs.Length);
            foreach (fieldIndex field in fieldIndexs)
            {
                Type type = field.Member.FieldType;
                if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    TMemberAttribute attribute = field.GetAttribute<TMemberAttribute>(true, true);
                    if (isAllMember ? (attribute == null || attribute.IsSetup) : (attribute != null && attribute.IsSetup))
                    {
                        fields.Add(field.Member);
                    }
                }
            }
            return fields;
        }
        /// <summary>
        /// 获取字段成员集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="memberFilter"></param>
        /// <returns>字段成员集合</returns>
        internal static KeyValueStruct<FieldInfo, int>[] GetFieldIndexs<TValueType>(memberFilters memberFilter)
        {
            return fastCSharp.code.memberIndexGroup<TValueType>.GetFields(memberFilter)
                .getArray(value => new KeyValueStruct<FieldInfo, int>(value.Member, value.MemberIndex));
        }

        /// <summary>
        /// 创建获取字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TFieldType"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static Func<TValueType, TFieldType> GetField<TValueType, TFieldType>(string fieldName)
        {
            var field = typeof(TValueType).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) LogPlus.Error.Throw(typeof(TValueType).fullName() + " 未找到字段成员 " + fieldName, true, false);
            return GetField<TValueType, TFieldType>(field);
        }
        /// <summary>
        /// 创建获取字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TFieldType"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Func<TValueType, TFieldType> GetField<TValueType, TFieldType>(FieldInfo field)
        {
            if (field.ReflectedType != typeof(TValueType) || !typeof(TFieldType).IsAssignableFrom(field.FieldType)) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
            DynamicMethod dynamicMethod = new DynamicMethod("get_" + field.Name, typeof(TFieldType), new[] { typeof(TValueType) }, typeof(TValueType), true);
            ILGenerator generator = dynamicMethod.GetILGenerator();
            if (typeof(TValueType).IsValueType) generator.Emit(OpCodes.Ldarga_S, 0);
            else generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, field);
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType, TFieldType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, TFieldType>));
        }
        /// <summary>
        /// 获取字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TFieldType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public delegate TFieldType GetFieldDele<TValueType, out TFieldType>(ref TValueType value);
        /// <summary>
        /// 创建获取字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TFieldType"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static GetFieldDele<TValueType, TFieldType> GetFieldStruct<TValueType, TFieldType>(string fieldName) where TValueType : struct
        {
            DynamicMethod dynamicMethod = new DynamicMethod("getRef_" + fieldName, typeof(TFieldType), new[] { typeof(TValueType).MakeByRefType() }, typeof(TValueType), true);
            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, typeof(TValueType).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
            generator.Emit(OpCodes.Ret);
            return (GetFieldDele<TValueType, TFieldType>)dynamicMethod.CreateDelegate(typeof(GetFieldDele<TValueType, TFieldType>));
        }
        /// <summary>
        /// 创建设置字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="dynamicMethod"></param>
        /// <param name="fieldName"></param>
        private static void GetSetField<TValueType>(DynamicMethod dynamicMethod, string fieldName)
        {
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, typeof(TValueType).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
            generator.Emit(OpCodes.Ret);
        }
        /// <summary>
        /// 创建设置字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TFieldType"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static Action<TValueType, TFieldType> SetField<TValueType, TFieldType>(string fieldName) where TValueType : class
        {
            DynamicMethod dynamicMethod = new DynamicMethod("set_" + fieldName, null, new[] { typeof(TValueType), typeof(TFieldType) }, typeof(TValueType), true);
            GetSetField<TValueType>(dynamicMethod, fieldName);
            return (Action<TValueType, TFieldType>)dynamicMethod.CreateDelegate(typeof(Action<TValueType, TFieldType>));
        }
        /// <summary>
        /// 设置字段值委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TFieldType"></typeparam>
        /// <param name="value"></param>
        /// <param name="fieldValue"></param>
        public delegate void SetFieldDele<TValueType, in TFieldType>(ref TValueType value, TFieldType fieldValue);
        /// <summary>
        /// 创建设置字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TFieldType"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static SetFieldDele<TValueType, TFieldType> SetFieldStruct<TValueType, TFieldType>(string fieldName) where TValueType : struct
        {
            var dynamicMethod = new DynamicMethod("set_" + fieldName, null, new Type[] { typeof(TValueType).MakeByRefType(), typeof(TFieldType) }, typeof(TValueType), true);
            GetSetField<TValueType>(dynamicMethod, fieldName);
            return (SetFieldDele<TValueType, TFieldType>)dynamicMethod.CreateDelegate(typeof(SetFieldDele<TValueType, TFieldType>));
        }

        /// <summary>
        /// 获取静态属性值
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <param name="name"></param>
        /// <param name="nonPublic"></param>
        /// <returns></returns>
        public static Func<TValueType> GetStaticProperty<TValueType>(Assembly assembly, string typeName, string name, bool nonPublic)
        {
            var type = assembly.GetType(typeName);
            var dynamicMethod = new DynamicMethod("get_" + name, typeof(TValueType), NullValuePlus<Type>.Array, type, true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Call, type.GetProperty(name, BindingFlags.Static | (nonPublic ? BindingFlags.NonPublic : BindingFlags.Public)).GetGetMethod(nonPublic));
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType>));
        }
        /// <summary>
        /// 获取静态属性值
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <param name="name"></param>
        /// <param name="nonPublic"></param>
        /// <returns></returns>
        public static Func<object, TValueType> GetProperty<TValueType>(Assembly assembly, string typeName, string name, bool nonPublic)
        {
            var type = assembly.GetType(typeName);
            var dynamicMethod = new DynamicMethod("get_" + name, typeof(TValueType), new[] { typeof(object) }, type, true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            var method = type.GetProperty(name, BindingFlags.Instance | (nonPublic ? BindingFlags.NonPublic : BindingFlags.Public)).GetGetMethod(nonPublic);
            generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
            generator.Emit(OpCodes.Ret);
            return (Func<object, TValueType>)dynamicMethod.CreateDelegate(typeof(Func<object, TValueType>));
        }

        /// <summary>
        /// 创建函数委托
        /// </summary>
        /// <typeparam name="TValueType1"></typeparam>
        /// <typeparam name="TValueType2"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Action<TValueType1, TValueType2> GetAction<TValueType1, TValueType2>(MethodInfo method)
        {
            var dynamicMethod = new DynamicMethod(method.Name, null, new[] { typeof(TValueType1), typeof(TValueType2) }, method.DeclaringType, true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
            generator.Emit(OpCodes.Ret);
            return (Action<TValueType1, TValueType2>)dynamicMethod.CreateDelegate(typeof(Action<TValueType1, TValueType2>));
        }
        /// <summary>
        /// 创建函数委托
        /// </summary>
        /// <typeparam name="TValueType1"></typeparam>
        /// <typeparam name="TValueType2"></typeparam>
        /// <typeparam name="TReturnType"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Func<TValueType1, TValueType2, TReturnType> GetStaticFunc<TValueType1, TValueType2, TReturnType>(MethodInfo method)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(method.Name, typeof(TReturnType), new[] { typeof(TValueType1), typeof(TValueType2) }, method.DeclaringType, true);
            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType1, TValueType2, TReturnType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType1, TValueType2, TReturnType>));
        }

        /// <summary>
        /// 可空类型构造函数
        /// </summary>
        internal static readonly Dictionary<Type, ConstructorInfo> NullableConstructors;
        static PubPlus()
        {
            var intType = new Type[] { typeof(int) };
            DataReaderMethods = DictionaryPlus.CreateOnly<Type, MethodInfo>();
            DataReaderMethods.Add(typeof(bool), typeof(DbDataReader).GetMethod("GetBoolean", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(byte), typeof(DbDataReader).GetMethod("GetByte", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(char), typeof(DbDataReader).GetMethod("GetChar", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(DateTime), typeof(DbDataReader).GetMethod("GetDateTime", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(decimal), typeof(DbDataReader).GetMethod("GetDecimal", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(double), typeof(DbDataReader).GetMethod("GetDouble", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(float), typeof(DbDataReader).GetMethod("GetFloat", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(Guid), typeof(DbDataReader).GetMethod("GetGuid", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(short), typeof(DbDataReader).GetMethod("GetInt16", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(int), typeof(DbDataReader).GetMethod("GetInt32", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(long), typeof(DbDataReader).GetMethod("GetInt64", BindingFlags.Public | BindingFlags.Instance, null, intType, null));
            DataReaderMethods.Add(typeof(string), typeof(DbDataReader).GetMethod("GetString", BindingFlags.Public | BindingFlags.Instance, null, intType, null));

            NullableConstructors = DictionaryPlus.CreateOnly<Type, ConstructorInfo>();
            NullableConstructors.Add(typeof(bool), typeof(bool?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(bool) }, null));
            NullableConstructors.Add(typeof(byte), typeof(byte?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(byte) }, null));
            NullableConstructors.Add(typeof(char), typeof(char?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(char) }, null));
            NullableConstructors.Add(typeof(DateTime), typeof(DateTime?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(DateTime) }, null));
            NullableConstructors.Add(typeof(decimal), typeof(decimal?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(decimal) }, null));
            NullableConstructors.Add(typeof(double), typeof(double?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(double) }, null));
            NullableConstructors.Add(typeof(float), typeof(float?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(float) }, null));
            NullableConstructors.Add(typeof(Guid), typeof(Guid?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(Guid) }, null));
            NullableConstructors.Add(typeof(short), typeof(short?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(short) }, null));
            NullableConstructors.Add(typeof(int), typeof(int?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(int) }, null));
            NullableConstructors.Add(typeof(long), typeof(long?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new [] { typeof(long) }, null));
        }
    }
}
