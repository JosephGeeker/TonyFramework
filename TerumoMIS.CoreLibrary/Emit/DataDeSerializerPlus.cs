//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DataDeSerializerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  DataDeSerializerPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 11:54:01
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 二进制数据反序列化
    /// </summary>
    public unsafe sealed class DataDeSerializerPlus:BinaryDeSerializerPlus
    {
        /// <summary>
        /// 基本类型反序列化函数
        /// </summary>
        internal sealed class DeSerializeMethodPlus : Attribute { }
        /// <summary>
        /// 基本类型反序列化函数
        /// </summary>
        internal sealed class MemberDeSerializeMethodPlus : Attribute { }
        /// <summary>
        /// 基本类型反序列化函数
        /// </summary>
        internal sealed class MemberMapDeSerializeMethodPlus : Attribute { }
        /// <summary>
        /// 二进制数据反序列化
        /// </summary>
        internal static class TypeDeSerializerPlus
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            public struct MemberDynamicMethodStruct
            {
                /// <summary>
                /// 动态函数
                /// </summary>
                private DynamicMethod _dynamicMethod;
                /// <summary>
                /// 
                /// </summary>
                private ILGenerator _generator;
                /// <summary>
                /// 是否值类型
                /// </summary>
                private bool _isValueType;

                /// <summary>
                /// 动态函数
                /// </summary>
                /// <param name="type"></param>
                public MemberDynamicMethodStruct(Type type)
                {
                    _dynamicMethod = new DynamicMethod("DataDeSerializerPlus", null, new[] { typeof(DataDeSerializerPlus), type.MakeByRefType() }, type, true);
                    _generator = _dynamicMethod.GetILGenerator();
                    _isValueType = type.IsValueType;
                }
                /// <summary>
                /// 添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(BinarySerializerPlus.FieldInfoPlus field)
                {
                    _generator.Emit(OpCodes.Ldarg_0);
                    _generator.Emit(OpCodes.Ldarg_1);
                    if (!_isValueType) _generator.Emit(OpCodes.Ldind_Ref);
                    _generator.Emit(OpCodes.Ldflda, field.Field);
                    MethodInfo method = GetMemberDeSerializeMethod(field.Field.FieldType) ?? GetMemberDeSerializer(field.Field.FieldType);
                    _generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                }
                /// <summary>
                /// 创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<TDelegateType>()
                {
                    _generator.Emit(OpCodes.Ret);
                    return _dynamicMethod.CreateDelegate(typeof(TDelegateType));
                }
            }
            /// <summary>
            /// 动态函数
            /// </summary>
            public struct MemberMapDynamicMethodStruct
            {
                /// <summary>
                /// 动态函数
                /// </summary>
                private DynamicMethod _dynamicMethod;
                /// <summary>
                /// 
                /// </summary>
                private ILGenerator _generator;
                /// <summary>
                /// 是否值类型
                /// </summary>
                private bool _isValueType;

                /// <summary>
                /// 动态函数
                /// </summary>
                /// <param name="type"></param>
                public MemberMapDynamicMethodStruct(Type type)
                {
                    _dynamicMethod = new DynamicMethod("dataMemberMapDeSerializer", null, new[] { typeof(memberMap), typeof(DataDeSerializerPlus), type.MakeByRefType() }, type, true);
                    _generator = _dynamicMethod.GetILGenerator();
                    _isValueType = type.IsValueType;
                }
                /// <summary>
                /// 添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(BinarySerializerPlus.FieldInfoPlus field)
                {
                    var end = _generator.DefineLabel();
                    _generator.Emit(OpCodes.Ldarg_0);
                    _generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                    _generator.Emit(OpCodes.Callvirt, PubPlus.MemberMapIsMemberMethod);
                    _generator.Emit(OpCodes.Brfalse_S, end);

                    _generator.Emit(OpCodes.Ldarg_1);
                    _generator.Emit(OpCodes.Ldarg_2);
                    if (!_isValueType) _generator.Emit(OpCodes.Ldind_Ref);
                    _generator.Emit(OpCodes.Ldflda, field.Field);
                    var method = GetMemberMapDeSerializeMethod(field.Field.FieldType) ?? GetMemberDeSerializer(field.Field.FieldType);
                    _generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);

                    _generator.MarkLabel(end);
                }
                /// <summary>
                /// 创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<TDelegateType>()
                {
                    _generator.Emit(OpCodes.Ret);
                    return _dynamicMethod.CreateDelegate(typeof(TDelegateType));
                }
            }

            /// <summary>
            /// 未知类型反序列化调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> _MemberDeSerializers = new interlocked.dictionary<Type,MethodInfo>(DictionaryPlus.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 未知类型枚举反序列化委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>未知类型反序列化委托调用函数信息</returns>
            private static MethodInfo GetMemberDeSerializer(Type type)
            {
                MethodInfo method;
                if (_MemberDeSerializers.TryGetValue(type, out method)) return method;
                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    if (elementType.IsValueType)
                    {
                        if (elementType.IsEnum)
                        {
                            var enumType = Enum.GetUnderlyingType(elementType);
                            if (enumType == typeof(uint)) method = enumUIntArrayMemberMethod;
                            else if (enumType == typeof(byte)) method = enumByteArrayMemberMethod;
                            else if (enumType == typeof(ulong)) method = EnumULongArrayMemberMethod;
                            else if (enumType == typeof(ushort)) method = enumUShortArrayMemberMethod;
                            else if (enumType == typeof(long)) method = EnumLongArrayMemberMethod;
                            else if (enumType == typeof(short)) method = enumShortArrayMemberMethod;
                            else if (enumType == typeof(sbyte)) method = enumSByteArrayMemberMethod;
                            else method = enumIntArrayMemberMethod;
                            method = method.MakeGenericMethod(elementType);
                        }
                        else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            method = NullableArrayMemberMethod.MakeGenericMethod(elementType.GetGenericArguments());
                        }
                        else method = StructArrayMemberMethod.MakeGenericMethod(elementType);
                    }
                    else method = ArrayMemberMethod.MakeGenericMethod(elementType);
                }
                else if (type.IsEnum)
                {
                    var enumType = Enum.GetUnderlyingType(type);
                    if (enumType == typeof(uint)) method = enumUIntMethod;
                    else if (enumType == typeof(byte)) method = enumByteMemberMethod;
                    else if (enumType == typeof(ulong)) method = enumULongMethod;
                    else if (enumType == typeof(ushort)) method = enumUShortMemberMethod;
                    else if (enumType == typeof(long)) method = enumLongMethod;
                    else if (enumType == typeof(short)) method = enumShortMemberMethod;
                    else if (enumType == typeof(sbyte)) method = enumSByteMemberMethod;
                    else method = enumIntMethod;
                    method = method.MakeGenericMethod(type);
                }
                else
                {
                    if (type.IsGenericType)
                    {
                        var genericType = type.GetGenericTypeDefinition();
                        if (genericType == typeof(Dictionary<,>))
                        {
                            method = dictionaryMemberMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(Nullable<>))
                        {
                            method = nullableMemberDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(KeyValuePair<,>))
                        {
                            method = keyValuePairDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(SortedDictionary<,>))
                        {
                            method = sortedDictionaryMemberMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(SortedList<,>))
                        {
                            method = sortedListMemberMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                    }
                    if (method == null)
                    {
                        if (typeof(fastCSharp.code.cSharp.dataSerialize.ISerialize).IsAssignableFrom(type))
                        {
                            if (type.IsValueType) method = StructISerializeMethod.MakeGenericMethod(type);
                            else method = MemberClassISerializeMethod.MakeGenericMethod(type);
                        }
                        else if (type.IsValueType) method = structDeSerializeMethod.MakeGenericMethod(type);
                        else method = MemberClassDeSerializeMethod.MakeGenericMethod(type);
                    }
                }
                _MemberDeSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            /// 真实类型序列化函数集合
            /// </summary>
            private static interlocked.dictionary<Type, Func<DataDeSerializerPlus, object, object>> _realDeSerializers = new interlocked.dictionary<Type,Func<DataDeSerializerPlus,object,object>>(dictionary.CreateOnly<Type, Func<DataDeSerializerPlus, object, object>>());
            /// <summary>
            /// 获取真实类型序列化函数
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>真实类型序列化函数</returns>
            public static Func<DataDeSerializerPlus, object, object> GetRealDeSerializer(Type type)
            {
                Func<DataDeSerializerPlus, object, object> method;
                if (_realDeSerializers.TryGetValue(type, out method)) return method;
                method = (Func<DataDeSerializerPlus, object, object>)Delegate.CreateDelegate(typeof(Func<DataDeSerializerPlus, object, object>), realTypeObjectMethod.MakeGenericMethod(type));
                _realDeSerializers.Set(type, method);
                return method;
            }
        }
        /// <summary>
        /// 二进制数据反序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        internal static class TypeDeSerializerPlus<TValueType>
        {
            /// <summary>
            /// 二进制数据反序列化委托
            /// </summary>
            /// <param name="DeSerializer">二进制数据反序列化</param>
            /// <param name="value">目标数据</param>
            internal delegate void DeSerializeDele(DataDeSerializerPlus DeSerializer, ref TValueType value);
            /// <summary>
            /// 二进制数据反序列化委托
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            /// <param name="DeSerializer">二进制数据反序列化</param>
            /// <param name="value">目标数据</param>
            private delegate void MemberMapDeSerializeDele(memberMap memberMap, DataDeSerializerPlus DeSerializer, ref TValueType value);
            /// <summary>
            /// 二进制数据序列化类型配置
            /// </summary>
            private static readonly dataSerialize Attribute;
            /// <summary>
            /// 反序列化委托
            /// </summary>
            internal static readonly DeSerializeDele DefaultDeSerializer;
            /// <summary>
            /// 固定分组成员序列化
            /// </summary>
            private static readonly DeSerializeDele FixedMemberDeSerializer;
            /// <summary>
            /// 固定分组成员位图序列化
            /// </summary>
            private static readonly MemberMapDeSerializeDele FixedMemberMapDeSerializer;
            /// <summary>
            /// 成员序列化
            /// </summary>
            private static readonly DeSerializeDele MemberDeSerializer;
            /// <summary>
            /// 成员位图序列化
            /// </summary>
            private static readonly MemberMapDeSerializeDele MemberMapDeSerializer;
            /// <summary>
            /// JSON混合序列化位图
            /// </summary>
            private static readonly memberMap JsonMemberMap;
            /// <summary>
            /// JSON混合序列化成员索引集合
            /// </summary>
            private static readonly int[] JsonMemberIndexs;
            /// <summary>
            /// 固定分组填充字节数
            /// </summary>
            private static readonly int FixedFillSize;
            /// <summary>
            /// 序列化成员数量
            /// </summary>
            private static readonly int MemberCountVerify;
            /// <summary>
            /// 是否值类型
            /// </summary>
            private static readonly bool IsValueType;
            /// <summary>
            /// 是否支持循环引用处理
            /// </summary>
            internal static readonly bool IsReferenceMember;
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="DeSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void DeSerialize(DataDeSerializerPlus DeSerializer, ref TValueType value)
            {
                if (IsValueType) StructDeSerialize(DeSerializer, ref value);
                else ClassDeSerialize(DeSerializer, ref value);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void StructDeSerialize(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                if (DefaultDeSerializer == null) MemberDeSerialize(deSerializer, ref value);
                else DefaultDeSerializer(deSerializer, ref value);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void ClassDeSerialize(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                if (deSerializer.CheckPoint(ref value))
                    //if (!attribute.IsAttribute || DeSerializer.checkPoint(ref value))
                {
                    if (deSerializer.IsRealType()) RealTypeBase(deSerializer, ref value);
                    else ClassDeSerializeBase(deSerializer, ref value);
                }
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void ClassDeSerializeBase(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                if (DefaultDeSerializer == null)
                {
                    deSerializer.AddPoint(ref value);
                    MemberDeSerialize(deSerializer, ref value);
                }
                else DefaultDeSerializer(deSerializer, ref value);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            internal static void MemberDeSerialize(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                if (deSerializer.CheckMemberCount(MemberCountVerify))
                {
                    FixedMemberDeSerializer(deSerializer, ref value);
                    deSerializer.Read += FixedFillSize;
                    MemberDeSerializer(deSerializer, ref value);
                    if (Attribute.IsJson || JsonMemberMap != null) deSerializer.parseJson(ref value);
                }
                else if (Attribute.IsMemberMap)
                {
                    memberMap memberMap = deSerializer.CheckMemberMap<TValueType>();
                    if (memberMap != null)
                    {
                        byte* start = deSerializer.Read;
                        FixedMemberMapDeSerializer(memberMap, deSerializer, ref value);
                        deSerializer.Read += (int)(start - deSerializer.Read) & 3;
                        MemberMapDeSerializer(memberMap, deSerializer, ref value);
                        if (Attribute.IsJson) deSerializer.parseJson(ref value);
                        else if (JsonMemberMap != null)
                        {
                            foreach (int memberIndex in JsonMemberIndexs)
                            {
                                if (memberMap.IsMember(memberIndex))
                                {
                                    deSerializer.parseJson(ref value);
                                    return;
                                }
                            }
                        }
                    }
                }
                else deSerializer.Error(DeSerializeState.MemberMap);
            }

            /// <summary>
            /// 真实类型反序列化
            /// </summary>
            /// <param name="DeSerializer">二进制数据反序列化</param>
            /// <param name="value">值</param>
            /// <returns></returns>
            internal static void RealType(DataDeSerializerPlus DeSerializer, ref TValueType value)
            {
                if (IsValueType) StructDeSerialize(DeSerializer, ref value);
                else ClassDeSerializeBase(DeSerializer, ref value);
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void BaseDeSerialize<TChildType>(DataDeSerializerPlus deSerializer, ref TChildType value) where TChildType : TValueType
            {
                if (value == null) value = ConstructorPlus<TChildType>.New();
                TValueType newValue = value;
                ClassDeSerializeBase(deSerializer, ref newValue);
            }
            /// <summary>
            /// 找不到构造函数
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void NoConstructor(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                if (deSerializer._isObjectRealType) deSerializer.Error(DeSerializeState.NotNull);
                else RealTypeBase(deSerializer, ref value);
            }
            /// <summary>
            /// 真实类型
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void RealTypeBase(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                remoteType remoteType = default(remoteType);
                TypeDeSerializerPlus<remoteType>.StructDeSerialize(deSerializer, ref remoteType);
                if (deSerializer.state == DeSerializeState.Success)
                {
                    Type type = remoteType.Type;
                    if (value == null || type.IsValueType)
                    {
                        value = (TValueType)TypeDeSerializerPlus.GetRealDeSerializer(type)(deSerializer, value);
                    }
                    else TypeDeSerializerPlus.GetRealDeSerializer(type)(deSerializer, value);
                }
            }
            /// <summary>
            /// 不支持对象转换null
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void FromNullBase(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                if (value == null) throw new ArgumentNullException("value");
                deSerializer.checkNull();
                value = default(TValueType);
            }

            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">值</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void EnumByteBase(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                if (deSerializer == null) throw new ArgumentNullException("deSerializer");
                deSerializer.EnumByte(ref value);
            }

            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">值</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EnumSByte(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                deSerializer.EnumSByte(ref value);
            }

            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">值</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EnumShort(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                deSerializer.EnumShort(ref value);
            }

            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EnumUShort(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                deSerializer.EnumShort(ref value);
            }

            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void EnumInt(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                deSerializer.EnumInt(ref value);
            }

            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">值</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void EnumUInt(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                deSerializer.EnumUInt(ref value);
            }

            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value">值</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void EnumLong(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                deSerializer.EnumLong(ref value);
            }

            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="deSerializer">二进制数据反序列化</param>
            /// <param name="value"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void EnumULong(DataDeSerializerPlus deSerializer, ref TValueType value)
            {
                deSerializer.EnumULong(ref value);
            }
            static TypeDeSerializerPlus()
            {
                Type type = typeof(TValueType), attributeType;
                MethodInfo methodInfo = DataDeSerializerPlus.getDeSerializeMethod(type);
                Attribute = type.customAttribute<dataSerialize>(out attributeType, true) ?? dataSerialize.Default;
                if (methodInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("DataDeSerializerPlus", typeof(void), new[] { typeof(DataDeSerializerPlus), type.MakeByRefType() }, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    DefaultDeSerializer = (DeSerializeDele)dynamicMethod.CreateDelegate(typeof(DeSerializeDele));
                    IsReferenceMember = false;
                    IsValueType = true;
                    return;
                }
                if (type.IsArray)
                {
                    IsValueType = true;
                    if (type.GetArrayRank() == 1)
                    {
                        var elementType = type.GetElementType();
                        if (!elementType.IsPointer)
                        {
                            if (elementType.IsValueType)
                            {
                                if (elementType.IsEnum)
                                {
                                    var enumType = Enum.GetUnderlyingType(elementType);
                                    if (enumType == typeof(uint)) methodInfo = enumUIntArrayMethod.MakeGenericMethod(elementType);
                                    else if (enumType == typeof(byte)) methodInfo = enumByteArrayMethod.MakeGenericMethod(elementType);
                                    else if (enumType == typeof(ulong)) methodInfo = EnumULongArrayMethod.MakeGenericMethod(elementType);
                                    else if (enumType == typeof(ushort)) methodInfo = enumUShortArrayMethod.MakeGenericMethod(elementType);
                                    else if (enumType == typeof(long)) methodInfo = EnumLongArrayMethod.MakeGenericMethod(elementType);
                                    else if (enumType == typeof(short)) methodInfo = enumShortArrayMethod.MakeGenericMethod(elementType);
                                    else if (enumType == typeof(sbyte)) methodInfo = enumSByteArrayMethod.MakeGenericMethod(elementType);
                                    else methodInfo = enumIntArrayMethod.MakeGenericMethod(elementType);
                                    IsReferenceMember = false;
                                }
                                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    methodInfo = NullableArrayMethod.MakeGenericMethod(elementType = elementType.GetGenericArguments()[0]);
                                    IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(elementType);
                                }
                                else
                                {
                                    methodInfo = StructArrayMethod.MakeGenericMethod(elementType);
                                    IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(elementType);
                                }
                            }
                            else
                            {
                                methodInfo = ArrayMethod.MakeGenericMethod(elementType);
                                IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(elementType);
                            }
                            DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), methodInfo);
                            return;
                        }
                    }
                    DefaultDeSerializer = FromNullBase;
                    IsReferenceMember = false;
                    return;
                }
                if (type.IsEnum)
                {
                    var enumType = Enum.GetUnderlyingType(type);
                    if (enumType == typeof(uint)) DefaultDeSerializer = EnumUInt;
                    else if (enumType == typeof(byte)) DefaultDeSerializer = EnumByteBase;
                    else if (enumType == typeof(ulong)) DefaultDeSerializer = EnumULong;
                    else if (enumType == typeof(ushort)) DefaultDeSerializer = EnumUShort;
                    else if (enumType == typeof(long)) DefaultDeSerializer = EnumLong;
                    else if (enumType == typeof(short)) DefaultDeSerializer = EnumShort;
                    else if (enumType == typeof(sbyte)) DefaultDeSerializer = EnumSByte;
                    else DefaultDeSerializer = EnumInt;
                    IsReferenceMember = false;
                    IsValueType = true;
                    return;
                }
                if (type.IsPointer)
                {
                    DefaultDeSerializer = FromNullBase;
                    IsReferenceMember = false;
                    IsValueType = true;
                    return;
                }
                if (type.IsGenericType)
                {
                    Type genericType = type.GetGenericTypeDefinition();
                    Type[] parameterTypes = type.GetGenericArguments();
                    if (genericType == typeof(SubArrayStruct<>))
                    {
                        DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), subArrayDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[0]);
                        IsValueType = true;
                        return;
                    }
                    if (genericType == typeof(Dictionary<,>))
                    {
                        DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), dictionaryDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[0]) || dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[1]);
                        IsValueType = true;
                        return;
                    }
                    if (genericType == typeof(NullValuePlus<>))
                    {
                        DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), nullableDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[0]);
                        IsValueType = true;
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), keyValuePairDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[0]) || dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[1]);
                        IsValueType = true;
                        return;
                    }
                    if (genericType == typeof(SortedDictionary<,>))
                    {
                        DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), sortedDictionaryDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[0]) || dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[1]);
                        IsValueType = true;
                        return;
                    }
                    if (genericType == typeof(SortedList<,>))
                    {
                        DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), sortedListDeSerializeMethod.MakeGenericMethod(type.GetGenericArguments()));
                        IsReferenceMember = dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[0]) || dataSerializer.typeSerializer.IsReferenceMember(parameterTypes[1]);
                        IsValueType = true;
                        return;
                    }
                }
                if ((methodInfo = dataSerializer.typeSerializer.GetCustom(type, false)) != null)
                {
                    DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), methodInfo);
                    IsReferenceMember = Attribute.IsReferenceMember;
                    IsValueType = true;
                    return;
                }
                if (type.IsAbstract || type.IsInterface || ConstructorPlus<TValueType>.New == null)
                {
                    DefaultDeSerializer = NoConstructor;
                    IsValueType = IsReferenceMember = true;
                    return;
                }
                IsReferenceMember = Attribute.IsReferenceMember;
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType)
                    {
                        var genericType = interfaceType.GetGenericTypeDefinition();
                        if (genericType == typeof(ICollection<>))
                        {
                            var parameters = interfaceType.GetGenericArguments();
                            var argumentType = parameters[0];
                            parameters[0] = argumentType.MakeArrayType();
                            var constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo = (type.IsValueType ? structCollectionMethod : classCollectionMethod).MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IList<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo = (type.IsValueType ? structCollectionMethod : classCollectionMethod).MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo = (type.IsValueType ? structCollectionMethod : classCollectionMethod).MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                methodInfo = (type.IsValueType ? structCollectionMethod : classCollectionMethod).MakeGenericMethod(type, argumentType);
                                break;
                            }
                        }
                        else if (genericType == typeof(IDictionary<,>))
                        {
                            var constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { interfaceType }, null);
                            if (constructorInfo != null)
                            {
                                var parameters = interfaceType.GetGenericArguments();
                                methodInfo = (type.IsValueType ? structDictionaryDeSerializeMethod : classDictionaryDeSerializeMethod).MakeGenericMethod(type, parameters[0], parameters[1]);
                                break;
                            }
                        }
                    }
                }
                if (methodInfo != null)
                {
                    DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), methodInfo);
                    return;
                }
                if (typeof(fastCSharp.code.cSharp.dataSerialize.ISerialize).IsAssignableFrom(type))
                {
                    methodInfo = (type.IsValueType ? StructISerializeMethod : ClassISerializeMethod).MakeGenericMethod(type);
                    DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), methodInfo);
                    IsValueType = true;
                }
                else
                {
                    if (type.IsValueType) IsValueType = true;
                    else if (Attribute != dataSerialize.Default && attributeType != type)
                    {
                        for (var baseType = type.BaseType; baseType != typeof(object); baseType = baseType.BaseType)
                        {
                            dataSerialize baseAttribute = fastCSharp.code.typeAttribute.GetAttribute<dataSerialize>(baseType, false, true);
                            if (baseAttribute != null)
                            {
                                if (baseAttribute.IsBaseType)
                                {
                                    methodInfo = baseSerializeMethod.MakeGenericMethod(baseType, type);
                                    DefaultDeSerializer = (DeSerializeDele)Delegate.CreateDelegate(typeof(DeSerializeDele), methodInfo);
                                    return;
                                }
                                break;
                            }
                        }
                    }
                    BinarySerializerPlus.FieldsStruct<BinarySerializerPlus.FieldInfoPlus> fields = dataSerializer.typeSerializer.GetFields(memberIndexGroup<TValueType>.GetFields(Attribute.MemberFilter), out MemberCountVerify);
                    FixedFillSize = -fields.FixedSize & 3;
                    var fixedDynamicMethod = new TypeDeSerializerPlus.MemberDynamicMethodStruct(type);
                    var fixedMemberMapDynamicMethod = Attribute.IsMemberMap ? new TypeDeSerializerPlus.MemberMapDynamicMethodStruct(type) : default(TypeDeSerializerPlus.MemberMapDynamicMethodStruct);
                    foreach (BinarySerializerPlus.FieldInfoPlus member in fields.FixedFields)
                    {
                        fixedDynamicMethod.Push(member);
                        if (Attribute.IsMemberMap) fixedMemberMapDynamicMethod.Push(member);
                    }
                    FixedMemberDeSerializer = (DeSerializeDele)fixedDynamicMethod.Create<DeSerializeDele>();
                    if (Attribute.IsMemberMap) FixedMemberMapDeSerializer = (MemberMapDeSerializeDele)fixedMemberMapDynamicMethod.Create<MemberMapDeSerializeDele>();

                    TypeDeSerializerPlus.MemberDynamicMethodStruct dynamicMethod = new TypeDeSerializerPlus.MemberDynamicMethodStruct(type);
                    TypeDeSerializerPlus.MemberMapDynamicMethodStruct memberMapDynamicMethod = Attribute.IsMemberMap ? new TypeDeSerializerPlus.MemberMapDynamicMethodStruct(type) : default(TypeDeSerializerPlus.MemberMapDynamicMethodStruct);
                    foreach (BinarySerializerPlus.FieldInfoPlus member in fields.Fields)
                    {
                        dynamicMethod.Push(member);
                        if (Attribute.IsMemberMap) memberMapDynamicMethod.Push(member);
                    }
                    MemberDeSerializer = (DeSerializeDele)dynamicMethod.Create<DeSerializeDele>();
                    if (Attribute.IsMemberMap) MemberMapDeSerializer = (MemberMapDeSerializeDele)memberMapDynamicMethod.Create<MemberMapDeSerializeDele>();

                    if (fields.JsonFields.Count != 0)
                    {
                        JsonMemberMap = memberMap<TValueType>.New();
                        JsonMemberIndexs = new int[fields.JsonFields.Count];
                        int index = 0;
                        foreach (fieldIndex field in fields.JsonFields) JsonMemberMap.SetMember(JsonMemberIndexs[index++] = field.MemberIndex);
                    }
                }
            }
        }

        /// <summary>
        /// 历史对象指针位置
        /// </summary>
        private Dictionary<int, object> _points;
        /// <summary>
        /// 是否检测相同的引用成员
        /// </summary>
        private bool _isReferenceMember;
        /// <summary>
        /// 是否检测引用类型对象的真实类型
        /// </summary>
        private bool _isObjectRealType;
        /// <summary>
        /// 是否检测数组引用
        /// </summary>
        private bool _isReferenceArray;

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private DeSerializeState DeSerialize<TValueType>(byte[] data, byte* start, byte* end, ref TValueType value, config config)
        {
            DeSerializeConfig = config;
            Buffer = data;
            this.start = start;
            Read = start + sizeof(int);
            this.end = end;
            if ((*start & dataSerializer.config.MemberMapValue) == 0) isMemberMap = false;
            else
            {
                isMemberMap = true;
                memberMap = config.MemberMap;
            }
            _isObjectRealType = (*start & dataSerializer.config.ObjectRealTypeValue) != 0;
            _isReferenceMember = TypeDeSerializerPlus<TValueType>.IsReferenceMember;
            if (_points == null && _isReferenceMember) _points = DictionaryPlus.CreateInt<object>();
            _isReferenceArray = true;
            state = DeSerializeState.Success;
            TypeDeSerializerPlus<TValueType>.DeSerialize(this, ref value);
            checkState();
            return config.State = state;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private DeSerializeState CodeDeSerialize<TValueType>(byte[] data, byte* start, byte* end, ref TValueType value, config config) where TValueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            DeSerializeConfig = config;
            Buffer = data;
            this.start = start;
            Read = start + sizeof(int);
            this.end = end;
            if ((*start & dataSerializer.config.MemberMapValue) == 0) isMemberMap = false;
            else
            {
                isMemberMap = true;
                memberMap = config.MemberMap;
            }
            _isObjectRealType = (*start & dataSerializer.config.ObjectRealTypeValue) != 0;
            _isReferenceMember = TypeDeSerializerPlus<TValueType>.IsReferenceMember;
            if (_points == null && _isReferenceMember) _points = DictionaryPlus.CreateInt<object>();
            if (value == null) value = ConstructorPlus<TValueType>.New();
            _isReferenceArray = true;
            state = DeSerializeState.Success;
            value.DeSerialize(this);
            checkState();
            return config.State = state;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private new void FreeBase()
        {
            Free();
            if (_points != null) _points.Clear();
            typePool<DataDeSerializerPlus>.Push(this);
        }
        /// <summary>
        /// 获取历史对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckPoint<TValueType>(ref TValueType value)
        {
            if (_isReferenceMember && *(int*)Read < 0)
            {
                object pointValue;
                if (_points.TryGetValue(*(int*)Read, out pointValue))
                {
                    value = (TValueType)pointValue;
                    Read += sizeof(int);
                    return false;
                }
                if (*(int*)Read != fastCSharp.emit.dataSerializer.RealTypeValue)
                {
                    Error(DeSerializeState.NoPoint);
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 添加历史对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddPoint<TValueType>(ref TValueType value)
        {
            if (value == null) value = ConstructorPlus<TValueType>.New();
            if (_isReferenceMember) _points.Add((int)(start - Read), value);
        }
        /// <summary>
        /// 添加历史对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPoint<TValueType>(TValueType value)
        {
            if (_isReferenceMember) _points.Add((int)(start - Read), value);
        }
        /// <summary>
        /// 是否真实类型处理
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsRealType()
        {
            if (_isObjectRealType && *(int*)Read == fastCSharp.emit.dataSerializer.RealTypeValue)
            {
                Read += sizeof(int);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 判断成员索引是否有效
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMemberMap(int memberIndex)
        {
            return memberMap.IsMember(memberIndex);
        }
        /// <summary>
        /// 创建数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="length"></param>
        private void CreateArray<TValueType>(ref TValueType[] array, int length)
        {
            if (array == null) throw new ArgumentNullException("array");
            array = new TValueType[length];
            if (_isReferenceArray)
            {
                if (_isReferenceMember) _points.Add((int)(start - Read), array);
            }
            else _isReferenceArray = true;
        }
        /// <summary>
        /// 数组反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns>数组长度</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int DeSerializeArray<TValueType>(ref TValueType[] value)
        {
            if (_isReferenceArray && !CheckPoint(ref value)) return 0;
            if (*(int*)Read != 0) return *(int*)Read;
            _isReferenceArray = true;
            value = NullValuePlus<TValueType>.Array;
            Read += sizeof(int);
            return 0;
        }
        /// <summary>
        /// 逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref bool[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref bool[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref bool?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length >> 1);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref bool?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref byte[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (((length + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref byte[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref byte?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref byte?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref sbyte[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (((length + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref sbyte[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref sbyte?[] value)
        {
            var length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize((sbyte*)Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref sbyte?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref short[] value)
        {
            var length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length * sizeof(short)) + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref short[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref short?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref short?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref ushort[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (((length * sizeof(ushort) + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref ushort[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref ushort?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref ushort?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref int[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref int[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref int?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref int?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref uint[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref uint[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref uint?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref uint?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref long[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(long) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref long[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref long?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref long?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref ulong[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(ulong) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref ulong[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref ulong?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref ulong?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref float[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(float) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref float[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref float?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref float?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref double[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(double) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref double[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref double?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref double?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref decimal[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(decimal) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref decimal[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref decimal?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref decimal?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref char[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (((length * sizeof(char) + (3 + sizeof(int))) & (int.MaxValue - 3)) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref char[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref char?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref char?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref DateTime[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(DateTime) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref DateTime[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref DateTime?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref DateTime?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref Guid[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if (length * sizeof(Guid) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref Guid[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref Guid?[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                if ((((length + (31 + 32)) >> 5) << 2) <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    Read = DeSerialize(Read + sizeof(int), value);
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref Guid?[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref string value)
        {
            if (CheckPoint(ref value))
            {
                int length = *(int*)Read;
                if ((length & 1) == 0)
                {
                    if (length != 0)
                    {
                        int dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                        if (dataLength <= (int)(end - Read))
                        {
                            value = new string((char*)(Read + sizeof(int)), 0, length >> 1);
                            if (_isReferenceMember) _points.Add((int)(start - Read), value);
                            Read += dataLength;
                        }
                        else Error(DeSerializeState.IndexOutOfRange);
                    }
                    else
                    {
                        value = string.Empty;
                        Read += sizeof(int);
                    }
                }
                else
                {
                    int dataLength = ((length >>= 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                    if (dataLength <= (int)(end - Read))
                    {
                        value = fastCSharp.String.FastAllocateString(length);
                        if (_isReferenceMember) _points.Add((int)(start - Read), value);
                        fixed (char* valueFixed = value)
                        {
                            char* write = valueFixed;
                            byte* readStart = Read + sizeof(int), readEnd = readStart + length;
                            do
                            {
                                *write++ = (char)*readStart++;
                            }
                            while (readStart != readEnd);
                        }
                        Read += dataLength;
                    }
                    else Error(DeSerializeState.IndexOutOfRange);
                }
            }
        }
        /// <summary>
        /// 字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref string value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }
        /// <summary>
        /// 字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [DeSerializeMethodPlus]
        private void DeSerialize(ref string[] value)
        {
            int length = DeSerializeArray(ref value);
            if (length != 0)
            {
                int mapLength = ((length + (31 + 32)) >> 5) << 2;
                if (mapLength <= (int)(end - Read))
                {
                    CreateArray(ref value, length);
                    arrayMap arrayMap = new arrayMap(Read + sizeof(int));
                    Read += mapLength;
                    for (int index = 0; index != value.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) value[index] = null;
                        else
                        {
                            DeSerialize(ref value[index]);
                            if (state != DeSerializeState.Success) return;
                        }
                    }
                    if (Read <= end) return;
                }
                Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 字符串反序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [MemberDeSerializeMethodPlus]
        [MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberDeSerialize(ref string[] value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else DeSerialize(ref value);
        }

        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void structDeSerialize<valueType>(DataDeSerializerPlus DeSerializer, ref valueType value) where valueType : struct
        {
            TypeDeSerializerPlus<valueType>.StructDeSerialize(DeSerializer, ref value);
        }
        /// <summary>
        /// 对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo structDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("structDeSerialize", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void keyValuePairDeSerialize<keyType, valueType>(DataDeSerializerPlus DeSerializer, ref KeyValuePair<keyType, valueType> value)
        {
            keyValue<keyType, valueType> keyValue = default(keyValue<keyType, valueType>);
            TypeDeSerializerPlus<keyValue<keyType, valueType>>.MemberDeSerialize(DeSerializer, ref keyValue);
            value = new KeyValuePair<keyType,valueType>(keyValue.Key, keyValue.Value);
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("keyValuePairDeSerialize", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void dictionaryArrayDeSerialize<keyType, valueType>(IDictionary<keyType, valueType> value)
        {
            if (_isReferenceMember) _points.Add((int)(start - Read), value);
            keyType[] keys = null;
            _isReferenceArray = false;
            TypeDeSerializerPlus<keyType[]>.DefaultDeSerializer(this, ref keys);
            if (state == DeSerializeState.Success)
            {
                valueType[] values = null;
                _isReferenceArray = false;
                TypeDeSerializerPlus<valueType[]>.DefaultDeSerializer(this, ref values);
                if (state == DeSerializeState.Success)
                {
                    int index = 0;
                    foreach (valueType nextValue in values) value.Add(keys[index++], nextValue);
                }
            }
        }
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void dictionaryDeSerialize<keyType, valueType>(ref Dictionary<keyType, valueType> value)
        {
            dictionaryArrayDeSerialize(value = dictionary.CreateAny<keyType, valueType>());
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("dictionaryDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void dictionaryMember<keyType, valueType>(ref Dictionary<keyType, valueType> value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else dictionaryDeSerialize(ref value);
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMemberMethod = typeof(DataDeSerializerPlus).GetMethod("dictionaryMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void sortedDictionaryDeSerialize<keyType, valueType>(ref SortedDictionary<keyType, valueType> value)
        {
            dictionaryArrayDeSerialize(value = new SortedDictionary<keyType, valueType>());
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedDictionaryDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("sortedDictionaryDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void sortedDictionaryMember<keyType, valueType>(ref SortedDictionary<keyType, valueType> value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else sortedDictionaryDeSerialize(ref value);
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedDictionaryMemberMethod = typeof(DataDeSerializerPlus).GetMethod("sortedDictionaryMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void sortedListDeSerialize<keyType, valueType>(ref SortedList<keyType, valueType> value)
        {
            dictionaryArrayDeSerialize(value = new SortedList<keyType, valueType>());
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedListDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("sortedListDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void sortedListMember<keyType, valueType>(ref SortedList<keyType, valueType> value)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else sortedListDeSerialize(ref value);
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo sortedListMemberMethod = typeof(DataDeSerializerPlus).GetMethod("sortedListMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void nullableDeSerialize<valueType>(ref Nullable<valueType> value) where valueType : struct
        {
            valueType newValue = value.HasValue ? value.Value : default(valueType);
            TypeDeSerializerPlus<valueType>.StructDeSerialize(this, ref newValue);
            value = newValue;
        }
        /// <summary>
        /// 对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("nullableDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void subArrayDeSerialize<valueType>(ref subArray<valueType> value)
        {
            valueType[] array = null;
            _isReferenceArray = false;
            TypeDeSerializerPlus<valueType[]>.DefaultDeSerializer(this, ref array);
            value.UnsafeSet(array, 0, array.Length);
        }
        /// <summary>
        /// 数组对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo subArrayDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("subArrayDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void nullableMemberDeSerialize<valueType>(ref Nullable<valueType> value) where valueType : struct
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else nullableDeSerialize(ref value);
        }
        /// <summary>
        /// 对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("nullableMemberDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 基类反序列化
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void baseSerialize<valueType, childType>(DataDeSerializerPlus DeSerializer, ref childType value) where childType : valueType
        {
            TypeDeSerializerPlus<valueType>.BaseDeSerialize(DeSerializer, ref value);
        }
        /// <summary>
        /// 基类反序列化函数信息
        /// </summary>
        private static readonly MethodInfo baseSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("baseSerialize", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 真实类型反序列化
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="objectValue"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object realTypeObject<valueType>(DataDeSerializerPlus DeSerializer, object objectValue)
        {
            valueType value = (valueType)objectValue;
            TypeDeSerializerPlus<valueType>.RealType(DeSerializer, ref value);
            return value;
        }
        /// <summary>
        /// 基类反序列化函数信息
        /// </summary>
        private static readonly MethodInfo realTypeObjectMethod = typeof(DataDeSerializerPlus).GetMethod("realTypeObject", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void collection<valueType, argumentType>(ref valueType value) where valueType : ICollection<argumentType>
        {
            argumentType[] values = null;
            _isReferenceArray = false;
            TypeDeSerializerPlus<argumentType[]>.DefaultDeSerializer(this, ref values);
            if (state == DeSerializeState.Success)
            {
                foreach (argumentType nextValue in values) value.Add(nextValue);
            }
        }
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void structCollection<valueType, argumentType>(ref valueType value) where valueType : ICollection<argumentType>
        {
            value = ConstructorPlus<valueType>.New();
            collection<valueType, argumentType>(ref value);
        }
        /// <summary>
        /// 集合反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structCollectionMethod = typeof(DataDeSerializerPlus).GetMethod("structCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void classCollection<valueType, argumentType>(ref valueType value) where valueType : ICollection<argumentType>
        {
            if (CheckPoint(ref value))
            {
                value = ConstructorPlus<valueType>.New();
                if (_isReferenceMember) _points.Add((int)(start - Read), value);
                collection<valueType, argumentType>(ref value);
            }
        }
        /// <summary>
        /// 集合反序列化函数信息
        /// </summary>
        private static readonly MethodInfo classCollectionMethod = typeof(DataDeSerializerPlus).GetMethod("classCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void dictionaryConstructorDeSerialize<dictionaryType, keyType, valueType>(ref dictionaryType value) where dictionaryType : IDictionary<keyType, valueType>
        {
            keyType[] keys = null;
            _isReferenceArray = false;
            TypeDeSerializerPlus<keyType[]>.DefaultDeSerializer(this, ref keys);
            if (state == DeSerializeState.Success)
            {
                valueType[] values = null;
                _isReferenceArray = false;
                TypeDeSerializerPlus<valueType[]>.DefaultDeSerializer(this, ref values);
                if (state == DeSerializeState.Success)
                {
                    int index = 0;
                    foreach (valueType nextValue in values) value.Add(keys[index++], nextValue);
                }
            }
        }
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void structDictionaryDeSerialize<dictionaryType, keyType, valueType>(ref dictionaryType value) where dictionaryType : IDictionary<keyType, valueType>
        {
            value = ConstructorPlus<dictionaryType>.New();
            dictionaryConstructorDeSerialize<dictionaryType, keyType, valueType>(ref value);
        }
        /// <summary>
        /// 集合反序列化函数信息
        /// </summary>
        private static readonly MethodInfo structDictionaryDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("structDictionaryDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="DeSerializer">二进制数据反序列化</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void classDictionaryDeSerialize<dictionaryType, keyType, valueType>(ref dictionaryType value) where dictionaryType : IDictionary<keyType, valueType>
        {
            if (CheckPoint(ref value))
            {
                value = ConstructorPlus<dictionaryType>.New();
                if (_isReferenceMember) _points.Add((int)(start - Read), value);
                dictionaryConstructorDeSerialize<dictionaryType, keyType, valueType>(ref value);
            }
        }
        /// <summary>
        /// 集合反序列化函数信息
        /// </summary>
        private static readonly MethodInfo classDictionaryDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("classDictionaryDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumByteMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumSByteMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumShortMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumUShortMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntMethod = typeof(DataDeSerializerPlus).GetMethod("enumInt", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntMethod = typeof(DataDeSerializerPlus).GetMethod("enumUInt", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumLongMethod = typeof(DataDeSerializerPlus).GetMethod("enumLong", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumULongMethod = typeof(DataDeSerializerPlus).GetMethod("enumULong", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumByteArray<valueType>(ref valueType[] array)
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                int dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    byte* data = Read + sizeof(int);
                    for (int index = 0; index != array.Length; array[index++] = pub.enumCast<valueType, byte>.FromInt(*data++)) ;
                    Read += dataLength;
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMethod = typeof(DataDeSerializerPlus).GetMethod("enumByteArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumByteArrayMember<valueType>(ref valueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumByteArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumSByteArray<valueType>(ref valueType[] array)
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                int dataLength = (length + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    byte* data = Read + sizeof(int);
                    for (int index = 0; index != array.Length; array[index++] = pub.enumCast<valueType, sbyte>.FromInt((sbyte)*data++)) ;
                    Read += dataLength;
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMethod = typeof(DataDeSerializerPlus).GetMethod("enumSByteArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumSByteArrayMember<valueType>(ref valueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumSByteArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumSByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumShortArray<valueType>(ref valueType[] array)
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                int dataLength = ((length << 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    short* data = (short*)(Read + sizeof(int));
                    for (int index = 0; index != array.Length; array[index++] = pub.enumCast<valueType, short>.FromInt(*data++)) ;
                    Read += dataLength;
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMethod = typeof(DataDeSerializerPlus).GetMethod("enumShortArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumShortArrayMember<valueType>(ref valueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumShortArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumUShortArray<valueType>(ref valueType[] array)
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                int dataLength = ((length << 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                if (dataLength <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    ushort* data = (ushort*)(Read + sizeof(int));
                    for (int index = 0; index != array.Length; array[index++] = pub.enumCast<valueType, ushort>.FromInt(*data++)) ;
                    Read += dataLength;
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMethod = typeof(DataDeSerializerPlus).GetMethod("enumUShortArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumUShortArrayMember<valueType>(ref valueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumUShortArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumUShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumIntArray<valueType>(ref valueType[] array)
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    Read += sizeof(int);
                    for (int index = 0; index != array.Length; Read += sizeof(int)) array[index++] = pub.enumCast<valueType, int>.FromInt(*(int*)Read);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMethod = typeof(DataDeSerializerPlus).GetMethod("enumIntArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumIntArrayMember<valueType>(ref valueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumIntArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumUIntArray<valueType>(ref valueType[] array)
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    Read += sizeof(int);
                    for (int index = 0; index != array.Length; Read += sizeof(uint)) array[index++] = pub.enumCast<valueType, uint>.FromInt(*(uint*)Read);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMethod = typeof(DataDeSerializerPlus).GetMethod("enumUIntArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumUIntArrayMember<valueType>(ref valueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else enumUIntArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumUIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void EnumLongArray<TValueType>(ref TValueType[] array)
        {
            var length = DeSerializeArray(ref array);
            if (length != 0)
            {
                if (length * sizeof(long) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    Read += sizeof(int);
                    for (int index = 0; index != array.Length; Read += sizeof(long)) array[index++] = PubPlus.EnumCastEnum<TValueType, long>.FromInt(*(long*)Read);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo EnumLongArrayMethod = typeof(DataDeSerializerPlus).GetMethod("enumLongArray", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void EnumLongArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlusPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else EnumLongArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo EnumLongArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumLongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void EnumULongArray<TValueType>(ref TValueType[] array)
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                if (length * sizeof(ulong) + sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    Read += sizeof(int);
                    for (int index = 0; index != array.Length; Read += sizeof(ulong)) array[index++] = PubPlus.EnumCastEnum<TValueType, ulong>.FromInt(*(ulong*)Read);
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo EnumULongArrayMethod = typeof(DataDeSerializerPlus).GetMethod("enumULongArray", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void EnumULongArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlusPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else EnumULongArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo EnumULongArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("enumULongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void NullableArray<TValueType>(ref Nullable<TValueType>[] array) where TValueType : struct
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                int mapLength = ((length + (31 + 32)) >> 5) << 2;
                if (mapLength <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    arrayMap arrayMap = new arrayMap(Read + sizeof(int));
                    Read += mapLength;
                    for (int index = 0; index != array.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) array[index] = null;
                        else
                        {
                            TValueType value = default(TValueType);
                            TypeDeSerializerPlus<TValueType>.StructDeSerialize(this, ref value);
                            if (state != DeSerializeState.Success) return;
                            array[index] = value;
                        }
                    }
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo NullableArrayMethod = typeof(DataDeSerializerPlus).GetMethod("nullableArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void NullableArrayMember<TValueType>(ref Nullable<TValueType>[] array) where TValueType : struct
        {
            if (*(int*)Read == BinarySerializerPlusPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else NullableArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo NullableArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("nullableArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void StructArray<TValueType>(ref TValueType[] array)
        {
            int length = DeSerializeArray(ref array);
            if (length != 0)
            {
                if ((length + 1) * sizeof(int) <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    Read += sizeof(int);
                    for (int index = 0; index != array.Length; ++index)
                    {
                        TypeDeSerializerPlus<TValueType>.StructDeSerialize(this, ref array[index]);
                        if (state != DeSerializeState.Success) return;
                    }
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo StructArrayMethod = typeof(DataDeSerializerPlus).GetMethod("structArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StructArrayMember<TValueType>(ref TValueType[] array)
        {
            if (*(int*)Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else StructArray(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo StructArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("structArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void Array<TValueType>(ref TValueType[] array) where TValueType : class
        {
            var length = DeSerializeArray(ref array);
            if (length != 0)
            {
                var mapLength = ((length + (31 + 32)) >> 5) << 2;
                if (mapLength <= (int)(end - Read))
                {
                    CreateArray(ref array, length);
                    arrayMap arrayMap = new arrayMap(Read + sizeof(int));
                    Read += mapLength;
                    for (var index = 0; index != array.Length; ++index)
                    {
                        if (arrayMap.Next() == 0) array[index] = null;
                        else
                        {
                            TypeDeSerializerPlus<TValueType>.ClassDeSerialize(this, ref array[index]);
                            if (state != DeSerializeState.Success) return;
                        }
                    }
                }
                else Error(DeSerializeState.IndexOutOfRange);
            }
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo ArrayMethod = typeof(DataDeSerializerPlus).GetMethod("array", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ArrayMember<TValueType>(ref TValueType[] array) where TValueType : class
        {
            if (*(int*)Read == BinarySerializerPlusPlus.NullValue)
            {
                Read += sizeof(int);
                array = null;
            }
            else this.Array(ref array);
        }
        /// <summary>
        /// 数组反序列化函数信息
        /// </summary>
        private static readonly MethodInfo ArrayMemberMethod = typeof(DataDeSerializerPlus).GetMethod("arrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StructISerialize<TValueType>(ref TValueType value) where TValueType : struct, fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            value.DeSerialize(this);
        }
        /// <summary>
        /// 序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo StructISerializeMethod = typeof(DataDeSerializerPlus).GetMethod("structISerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClassISerialize<TValueType>(ref TValueType value) where TValueType : class, fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            if (value == null) value = ConstructorPlus<TValueType>.New();
            value.DeSerialize(this);
        }
        /// <summary>
        /// 序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo ClassISerializeMethod = typeof(DataDeSerializerPlus).GetMethod("classISerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 序列化接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberClassISerialize<TValueType>(ref TValueType value) where TValueType : class, fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            if (*(int*)Read == BinarySerializerPlusPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else
            {
                if (value == null) value = ConstructorPlus<TValueType>.New();
                value.DeSerialize(this);
            }
        }
        /// <summary>
        /// 序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo MemberClassISerializeMethod = typeof(DataDeSerializerPlus).GetMethod("memberClassISerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 引用类型成员反序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MemberClassDeSerialize<TValueType>(ref TValueType value) where TValueType : class
        {
            MemberClassDeSerializeBase(ref value);
            return state == DeSerializeState.Success;
        }
        /// <summary>
        /// 引用类型成员反序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MemberClassDeSerializeBase<TValueType>(ref TValueType value) where TValueType : class
        {
            if (*(int*)Read == BinarySerializerPlusPlus.NullValue)
            {
                Read += sizeof(int);
                value = null;
            }
            else TypeDeSerializerPlus<TValueType>.ClassDeSerialize(this, ref value);
        }
        /// <summary>
        /// 序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo MemberClassDeSerializeMethod = typeof(DataDeSerializerPlus).GetMethod("memberClassDeSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 未知类型反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MemberStructDeSerialize<TValueType>(ref TValueType value)
        {
            TypeDeSerializerPlus<TValueType>.StructDeSerialize(this, ref value);
            return state == DeSerializeState.Success;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValueType DeSerialize<TValueType>(byte[] data, config config = null)
        {
            if (data != null)
            {
                fixed (byte* dataFixed = data) return DeSerialize<TValueType>(data, dataFixed, data.Length, config);
            }
            if (config != null) config.State = DeSerializeState.UnknownData;
            return default(TValueType);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValueType DeSerialize<TValueType>(SubArrayStruct<byte> data, config config = null)
        {
            if (data.Count != 0)
            {
                fixed (byte* dataFixed = data.array) return DeSerialize<TValueType>(data.array, dataFixed + data.StartIndex, data.Count, config);
            }
            if (config != null) config.State = DeSerializeState.UnknownData;
            return default(TValueType);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValueType DeSerialize<TValueType>(UnmanagedStreamPlus data, int startIndex = 0, config config = null)
        {
            if (data != null && startIndex >= 0)
            {
                return DeSerialize<TValueType>(null, data.Data + startIndex, data.Length - startIndex, config);
            }
            if (config != null) config.State = DeSerializeState.UnknownData;
            return default(TValueType);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TValueType DeSerialize<TValueType>(byte* data, int size, config config = null)
        {
            return DeSerialize<TValueType>(null, data, size, config);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TValueType DeSerialize<TValueType>(byte[] buffer, byte* data, int size, config config)
        {
            if (config == null) config = defaultConfig;
            int length = size - sizeof(int);
            if (length >= 0 && data != null)
            {
                if (config.IsFullData)
                {
                    if ((size & 3) == 0)
                    {
                        if (length != 0)
                        {
                            byte* end = data + length;
                            if (*(int*)end == length)
                            {
                                if ((*(uint*)data & BinarySerializerPlusPlus.ConfigPlus.HeaderMapAndValue) == BinarySerializerPlusPlus.ConfigPlus.HeaderMapValue)
                                {
                                    TValueType value = default(TValueType);
                                    DataDeSerializerPlus DeSerializer = typePool<DataDeSerializerPlus>.Pop() ?? new DataDeSerializerPlus();
                                    try
                                    {
                                        return DeSerializer.DeSerialize<TValueType>(buffer, data, end, ref value, config) == DeSerializeState.Success ? value : default(TValueType);
                                    }
                                    finally { DeSerializer.FreeBase(); }
                                }
                                config.State = DeSerializeState.HeaderError;
                                return default(TValueType);
                            }
                            config.State = DeSerializeState.EndVerify;
                            return default(TValueType);
                        }
                        if (*(int*)data == dataSerializer.NullValue)
                        {
                            config.State = DeSerializeState.Success;
                            return default(TValueType);
                        }
                    }
                }
                else
                {
                    if ((*(uint*)data & BinarySerializerPlus.config.HeaderMapAndValue) == BinarySerializerPlus.config.HeaderMapValue)
                    {
                        TValueType value = default(TValueType);
                        DataDeSerializerPlus DeSerializer = typePool<DataDeSerializerPlus>.Pop() ?? new DataDeSerializerPlus();
                        try
                        {
                            return DeSerializer.DeSerialize<TValueType>(buffer, data, data + length, ref value, config) == DeSerializeState.Success ? value : default(TValueType);
                        }
                        finally { DeSerializer.FreeBase(); }
                    }
                    if (*(int*)data == dataSerializer.NullValue)
                    {
                        config.State = DeSerializeState.Success;
                        config.DataLength = sizeof(int);
                        return default(TValueType);
                    }
                    config.State = DeSerializeState.HeaderError;
                    return default(TValueType);
                }
            }
            config.State = DeSerializeState.UnknownData;
            return default(TValueType);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DeSerialize<TValueType>(byte[] data, ref TValueType value, config config = null)
        {
            if (data != null)
            {
                fixed (byte* dataFixed = data) return DeSerialize<TValueType>(data, dataFixed, data.Length, ref value, config);
            }
            if (config != null) config.State = DeSerializeState.UnknownData;
            return false;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DeSerialize<TValueType>(SubArrayStruct<byte> data, ref TValueType value, config config = null)
        {
            if (data.Count != 0)
            {
                fixed (byte* dataFixed = data.array) return DeSerialize<TValueType>(data.array, dataFixed + data.StartIndex, data.Count, ref value, config);
            }
            if (config != null) config.State = DeSerializeState.UnknownData;
            return false;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DeSerialize<TValueType>(UnmanagedStreamPlus data, ref TValueType value, int startIndex = 0, config config = null)
        {
            if (data != null && startIndex >= 0)
            {
                return DeSerialize<TValueType>(null, data.Data + startIndex, data.Length - startIndex, ref value, config);
            }
            if (config != null) config.State = DeSerializeState.UnknownData;
            return false;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        public static bool DeSerialize<TValueType>(byte* data, int size, ref TValueType value, config config = null)
        {
            return DeSerialize(null, data, size, ref value, config);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        private static bool DeSerialize<TValueType>(byte[] buffer, byte* data, int size, ref TValueType value, config config)
        {
            if (config == null) config = defaultConfig;
            int length = size - sizeof(int);
            if (length >= 0)
            {
                if (config.IsFullData)
                {
                    if ((size & 3) == 0)
                    {
                        if (length != 0)
                        {
                            byte* end = data + length;
                            if (*(int*)end == length)
                            {
                                if ((*(uint*)data & BinarySerializerPlus.config.HeaderMapAndValue) == BinarySerializerPlus.config.HeaderMapValue)
                                {
                                    DataDeSerializerPlus DeSerializer = typePool<DataDeSerializerPlus>.Pop() ?? new DataDeSerializerPlus();
                                    try
                                    {
                                        return DeSerializer.DeSerialize<TValueType>(buffer, data, end, ref value, config) == DeSerializeState.Success;
                                    }
                                    finally { DeSerializer.FreeBase(); }
                                }
                                config.State = DeSerializeState.HeaderError;
                                return false;
                            }
                            config.State = DeSerializeState.EndVerify;
                            return false;
                        }
                        if (*(int*)data == dataSerializer.NullValue)
                        {
                            config.State = DeSerializeState.Success;
                            value = default(TValueType);
                            return true;
                        }
                    }
                }
                else
                {
                    if ((*(uint*)data & BinarySerializerPlus.config.HeaderMapAndValue) == BinarySerializerPlus.config.HeaderMapValue)
                    {
                        DataDeSerializerPlus DeSerializer = typePool<DataDeSerializerPlus>.Pop() ?? new DataDeSerializerPlus();
                        try
                        {
                            return DeSerializer.DeSerialize<TValueType>(buffer, data, data + length, ref value, config) == DeSerializeState.Success;
                        }
                        finally { DeSerializer.FreeBase(); }
                    }
                    if (*(int*)data == dataSerializer.NullValue)
                    {
                        config.State = DeSerializeState.Success;
                        config.DataLength = sizeof(int);
                        value = default(TValueType);
                        return true;
                    }
                    config.State = DeSerializeState.HeaderError;
                    return false;
                }
            }
            config.State = DeSerializeState.UnknownData;
            return false;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        public static bool CodeDeSerialize<TValueType>(byte[] data, ref TValueType value, config config = null) where TValueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            return CodeDeSerialize<TValueType>(SubArrayStruct<byte>.Unsafe(data, 0, data.Length), ref value, config);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        public static bool CodeDeSerialize<TValueType>(SubArrayStruct<byte> data, ref TValueType value, config config = null) where TValueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            if (config == null) config = defaultConfig;
            int length = data.Count - sizeof(int);
            if (length >= 0)
            {
                if (config.IsFullData)
                {
                    if ((data.Count & 3) == 0)
                    {
                        fixed (byte* dataFixed = data.array)
                        {
                            byte* start = dataFixed + data.StartIndex;
                            if (length != 0)
                            {
                                byte* end = start + length;
                                if (*(int*)end == length)
                                {
                                    if ((*(uint*)start & BinarySerializerPlusPlus.ConfigPlus.HeaderMapAndValue) == BinarySerializerPlusPlus.ConfigPlus.HeaderMapValue)
                                    {
                                        DataDeSerializerPlus DeSerializer = typePool<DataDeSerializerPlus>.Pop() ?? new DataDeSerializerPlus();
                                        try
                                        {
                                            return DeSerializer.CodeDeSerialize<TValueType>(data.array, start, end, ref value, config) == DeSerializeState.Success;
                                        }
                                        finally { DeSerializer.FreeBase(); }
                                    }
                                    config.State = DeSerializeState.HeaderError;
                                    return false;
                                }
                                config.State = DeSerializeState.EndVerify;
                                return false;
                            }
                            if (*(int*)start == dataSerializer.NullValue)
                            {
                                config.State = DeSerializeState.Success;
                                value = default(TValueType);
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    fixed (byte* dataFixed = data.array)
                    {
                        var start = dataFixed + data.StartIndex;
                        if ((*(uint*)start & BinarySerializerPlus.config.HeaderMapAndValue) == BinarySerializerPlus.config.HeaderMapValue)
                        {
                            DataDeSerializerPlus DeSerializer = typePool<DataDeSerializerPlus>.Pop() ?? new DataDeSerializerPlus();
                            try
                            {
                                return DeSerializer.CodeDeSerialize<TValueType>(data.array, start, start + length, ref value, config) == DeSerializeState.Success;
                            }
                            finally { DeSerializer.FreeBase(); }
                        }
                        if (*(int*)start == dataSerializer.NullValue)
                        {
                            config.State = DeSerializeState.Success;
                            config.DataLength = sizeof(int);
                            value = default(TValueType);
                            return true;
                        }
                        config.State = DeSerializeState.HeaderError;
                        return false;
                    }
                }
            }
            config.State = DeSerializeState.UnknownData;
            return false;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="data"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static object DeSerializeType<TValueType>(SubArrayStruct<byte> data, config config)
        {
            return DeSerialize<TValueType>(data, config);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static object DeSerializeType<TValueType>(Type type, SubArrayStruct<byte> data, config config = null)
        {
            if (type == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            Func<SubArrayStruct<byte>, config, object> parse;
            if (!DeSerializeTypes.TryGetValue(type, out parse))
            {
                parse = (Func<SubArrayStruct<byte>, config, object>)Delegate.CreateDelegate(typeof(Func<SubArrayStruct<byte>, config, object>), DeSerializeTypeMethod.MakeGenericMethod(type));
                DeSerializeTypes.Set(type, parse);
            }
            return parse(data, config);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        private static interlocked.dictionary<Type, Func<SubArrayStruct<byte>, config, object>> DeSerializeTypes = new interlocked.dictionary<Type, Func<SubArrayStruct<byte>, config, object>>(DictionaryPlus.CreateOnly<Type, Func<SubArrayStruct<byte>, config, object>>());
        /// <summary>
        /// 反序列化函数信息
        /// </summary>
        private static readonly MethodInfo DeSerializeTypeMethod = typeof(DataDeSerializerPlus).GetMethod("DeSerializeType", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(SubArrayStruct<byte>), typeof(config) }, null);
        /// <summary>
        /// 基本类型反序列化函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> DeSerializeMethods;
        /// <summary>
        /// 获取基本类型反序列化函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>反序列化函数</returns>
        private static MethodInfo GetDeSerializeMethod(Type type)
        {
            MethodInfo method;
            if (DeSerializeMethods.TryGetValue(type, out method))
            {
                DeSerializeMethods.Remove(type);
                return method;
            }
            return null;
        }
        /// <summary>
        /// 基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> MemberDeSerializeMethods;
        /// <summary>
        /// 获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo GetMemberDeSerializeMethod(Type type)
        {
            MethodInfo method;
            return MemberDeSerializeMethods.TryGetValue(type, out method) ? method : null;
        }
        /// <summary>
        /// 基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> MemberMapDeSerializeMethods;
        /// <summary>
        /// 获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo GetMemberMapDeSerializeMethod(Type type)
        {
            MethodInfo method;
            return MemberMapDeSerializeMethods.TryGetValue(type, out method) ? method : null;
        }
        static DataDeSerializerPlus()
        {
            DeSerializeMethods = DictionaryPlus.CreateOnly<Type, MethodInfo>();
            MemberDeSerializeMethods = DictionaryPlus.CreateOnly<Type, MethodInfo>();
            MemberMapDeSerializeMethods = DictionaryPlus.CreateOnly<Type, MethodInfo>();
            foreach (MethodInfo method in typeof(DataDeSerializerPlus).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                Type parameterType = null;
                if (method.customAttribute<DeSerializeMethod>() != null)
                {
                    DeSerializeMethods.Add(parameterType = method.GetParameters()[0].ParameterType.GetElementType(), method);
                }
                if (method.customAttribute<MemberDeSerializeMethod>() != null)
                {
                    if (parameterType == null) parameterType = method.GetParameters()[0].ParameterType.GetElementType();
                    MemberDeSerializeMethods.Add(parameterType, method);
                }
                if (method.customAttribute<memberMapDeSerializeMethod>() != null)
                {
                    MemberMapDeSerializeMethods.Add(parameterType ?? method.GetParameters()[0].ParameterType.GetElementType(), method);
                }
            }
        }
    }
}
