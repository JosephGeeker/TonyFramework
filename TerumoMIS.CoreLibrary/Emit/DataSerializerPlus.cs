//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DataSerializerPlusPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  DataSerializerPlusPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 14:11:32
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 二进制数据序列化
    /// </summary>
    public unsafe sealed class DataSerializerPlus:BinarySerializerPlus
    {
        /// <summary>
        /// 真实类型
        /// </summary>
        public const int RealTypeValue = NullValue + 1;
        /// <summary>
        /// 配置参数
        /// </summary>
        public new sealed class ConfigPlus : BinarySerializerPlus.ConfigPlus
        {
            /// <summary>
            /// 是否检测引用类型对象的真实类型
            /// </summary>
            internal const int ObjectRealTypeValue = 2;
            /// <summary>
            /// 是否序列化成员位图
            /// </summary>
            public bool IsMemberMap;
            /// <summary>
            /// 是否检测引用类型对象的真实类型
            /// </summary>
            public bool IsRealType;
            /// <summary>
            /// 序列化头部数据
            /// </summary>
            internal override int HeaderValue
            {
                get
                {
                    int value = base.HeaderValue;
                    if (IsRealType) value += ObjectRealTypeValue;
                    return value;
                }
            }
        }
        /// <summary>
        /// 基本类型序列化函数
        /// </summary>
        internal sealed class SerializeMethodPlus : Attribute { }
        /// <summary>
        /// 基本类型序列化函数
        /// </summary>
        internal sealed class MemberSerializeMethodPlus : Attribute { }
        /// <summary>
        /// 基本类型序列化函数
        /// </summary>
        internal sealed class MemberMapSerializeMethodPlus : Attribute { }
        /// <summary>
        /// 二进制数据序列化
        /// </summary>
        internal static class TypeSerializerPlus
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
                /// <param name="name">成员类型</param>
                public MemberDynamicMethodStruct(Type type)
                {
                    _dynamicMethod = new DynamicMethod("DataSerializerPlus", null, new[] { typeof(DataSerializerPlus), type }, type, true);
                    _generator = _dynamicMethod.GetILGenerator();
                    _isValueType = type.IsValueType;
                }
                /// <summary>
                /// 添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(fieldInfo field)
                {
                    _generator.Emit(OpCodes.Ldarg_0);
                    if (_isValueType) _generator.Emit(OpCodes.Ldarga_S, 1);
                    else _generator.Emit(OpCodes.Ldarg_1);
                    _generator.Emit(OpCodes.Ldfld, field.Field);
                    MethodInfo method = DataSerializerPlus.getMemberSerializeMethod(field.Field.FieldType) ?? GetMemberSerializer(field.Field.FieldType);
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
                    _dynamicMethod = new DynamicMethod("dataMemberMapSerializer", null, new[] { typeof(memberMap), typeof(DataSerializerPlus), type }, type, true);
                    _generator = _dynamicMethod.GetILGenerator();
                    _isValueType = type.IsValueType;
                }
                /// <summary>
                /// 添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(fieldInfo field)
                {
                    Label end = _generator.DefineLabel();
                    _generator.Emit(OpCodes.Ldarg_0);
                    _generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                    _generator.Emit(OpCodes.Callvirt, pub.MemberMapIsMemberMethod);
                    _generator.Emit(OpCodes.Brfalse_S, end);

                    _generator.Emit(OpCodes.Ldarg_1);
                    if (_isValueType) _generator.Emit(OpCodes.Ldarga_S, 2);
                    else _generator.Emit(OpCodes.Ldarg_2);
                    _generator.Emit(OpCodes.Ldfld, field.Field);
                    MethodInfo method = DataSerializerPlus.getMemberMapSerializeMethod(field.Field.FieldType) ?? GetMemberSerializer(field.Field.FieldType);
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
            /// 获取字段成员集合
            /// </summary>
            /// <returns>字段成员集合</returns>
            public static fields<fieldInfo> GetFields(fieldIndex[] fieldIndexs, out int memberCountVerify)
            {
                SubArrayStruct<fieldInfo> fixedFields = new SubArrayStruct<fieldInfo>(fieldIndexs.Length), fields = new SubArrayStruct<fieldInfo>();
                SubArrayStruct<fieldIndex> jsonFields = new SubArrayStruct<fieldIndex>();
                fields.UnsafeSet(fixedFields.array, fixedFields.array.length(), 0);
                int fixedSize = 0;
                foreach (fieldIndex field in fieldIndexs)
                {
                    Type type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        dataSerialize.member attribute = field.GetAttribute<dataSerialize.member>(true, true);
                        if (attribute == null || attribute.IsSetup)
                        {
                            if (attribute != null && attribute.IsJson) jsonFields.Add(field);
                            else
                            {
                                fieldInfo value = new fieldInfo(field);
                                if (value.FixedSize == 0) fields.UnsafeAddExpand(value);
                                else
                                {
                                    fixedFields.Add(value);
                                    fixedSize += value.FixedSize;
                                }
                            }
                        }
                    }
                }
                memberCountVerify = fixedFields.Count + fields.Count + jsonFields.Count + 0x40000000;
                return new fields<fieldInfo> { FixedFields = fixedFields.Sort(fieldInfo.FixedSizeSort), Fields = fields, JsonFields = jsonFields, FixedSize = fixedSize };
            }
            /// <summary>
            /// 获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <returns>字段成员集合</returns>
            public static SubArrayStruct<memberIndex> GetMembers(fieldIndex[] fieldIndexs)
            {
                SubArrayStruct<memberIndex> fields = new SubArrayStruct<memberIndex>(fieldIndexs.Length);
                foreach (fieldIndex field in fieldIndexs)
                {
                    Type type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        dataSerialize.member attribute = field.GetAttribute<dataSerialize.member>(true, true);
                        if (attribute == null || attribute.IsSetup) fields.Add(field);
                    }
                }
                return fields;
            }
            /// <summary>
            /// 获取自定义序列化函数信息
            /// </summary>
            /// <param name="type"></param>
            /// <param name="isSerializer"></param>
            /// <returns></returns>
            public static MethodInfo GetCustom(Type type, bool isSerializer)
            {
                MethodInfo serializeMethod = null, deSerializeMethod = null;
                Type refType = type.MakeByRefType();
                foreach (fastCSharp.code.attributeMethod method in fastCSharp.code.attributeMethod.GetStatic(type))
                {
                    if (method.Method.ReturnType == typeof(void)
                        && method.GetAttribute<dataSerialize.custom>(true) != null)
                    {
                        ParameterInfo[] parameters = method.Method.GetParameters();
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == typeof(DataSerializerPlus))
                            {
                                if (parameters[1].ParameterType == type)
                                {
                                    if (deSerializeMethod != null) return isSerializer ? method.Method : deSerializeMethod;
                                    serializeMethod = method.Method;
                                }
                            }
                            else if (parameters[0].ParameterType == typeof(dataDeSerializer))
                            {
                                if (parameters[1].ParameterType == refType)
                                {
                                    if (serializeMethod != null) return isSerializer ? serializeMethod : method.Method;
                                    deSerializeMethod = method.Method;
                                }
                            }
                        }
                    }
                }
                return null;
            }

            /// <summary>
            /// 未知类型序列化调用函数信息集合
            /// </summary>
            private static interlocked.DictionaryPlus<Type, MethodInfo> memberSerializers = new interlocked.DictionaryPlus<Type,MethodInfo>(DictionaryPlus.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 未知类型枚举序列化委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>未知类型序列化委托调用函数信息</returns>
            public static MethodInfo GetMemberSerializer(Type type)
            {
                MethodInfo method;
                if (memberSerializers.TryGetValue(type, out method)) return method;
                if (type.IsArray)
                {
                    Type elementType = type.GetElementType();
                    if (elementType.IsValueType)
                    {
                        if (elementType.IsEnum)
                        {
                            var enumType = Enum.GetUnderlyingType(elementType);
                            if (enumType == typeof(uint)) method = enumUIntArrayMemberMethod;
                            else if (enumType == typeof(byte)) method = enumByteArrayMemberMethod;
                            else if (enumType == typeof(ulong)) method = enumULongArrayMemberMethod;
                            else if (enumType == typeof(ushort)) method = enumUShortArrayMemberMethod;
                            else if (enumType == typeof(long)) method = enumLongArrayMemberMethod;
                            else if (enumType == typeof(short)) method = enumShortArrayMemberMethod;
                            else if (enumType == typeof(sbyte)) method = enumSByteArrayMemberMethod;
                            else method = enumIntArrayMemberMethod;
                            method = method.MakeGenericMethod(elementType);
                        }
                        else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            method = nullableArrayMemberMethod.MakeGenericMethod(elementType.GetGenericArguments());
                        }
                        else method = structArrayMemberMethod.MakeGenericMethod(elementType);
                    }
                    else method = arrayMemberMethod.MakeGenericMethod(elementType);
                }
                else if (type.IsEnum)
                {
                    var enumType = Enum.GetUnderlyingType(type);
                    if (enumType == typeof(uint)) method = enumUIntMemberMethod;
                    else if (enumType == typeof(byte)) method = enumByteMemberMethod;
                    else if (enumType == typeof(ulong)) method = enumULongMemberMethod;
                    else if (enumType == typeof(ushort)) method = enumUShortMemberMethod;
                    else if (enumType == typeof(long)) method = enumLongMemberMethod;
                    else if (enumType == typeof(short)) method = enumShortMemberMethod;
                    else if (enumType == typeof(sbyte)) method = enumSByteMemberMethod;
                    else method = enumIntMemberMethod;
                    method = method.MakeGenericMethod(type);
                }
                else
                {
                    if (type.IsGenericType)
                    {
                        var genericType = type.GetGenericTypeDefinition();
                        if (genericType == typeof(Dictionary<,>) || genericType == typeof(SortedDictionary<,>) || genericType == typeof(SortedList<,>))
                        {
                            var parameterTypes = type.GetGenericArguments();
                            method = DictionaryPlusMemberMethod.MakeGenericMethod(type, parameterTypes[0], parameterTypes[1]);
                        }
                        else if (genericType == typeof(Nullable<>))
                        {
                            method = nullableMemberSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                        else if (genericType == typeof(KeyValuePair<,>))
                        {
                            method = keyValuePairSerializeMethod.MakeGenericMethod(type.GetGenericArguments());
                        }
                    }
                    if (method == null)
                    {
                        if (typeof(fastCSharp.code.cSharp.dataSerialize.ISerialize).IsAssignableFrom(type))
                        {
                            if (type.IsValueType) method = structISerializeMethod.MakeGenericMethod(type);
                            else method = memberClassISerializeMethod.MakeGenericMethod(type);
                        }
                        else if (type.IsValueType) method = structSerializeMethod.MakeGenericMethod(type);
                        else method = memberClassSerializeMethod.MakeGenericMethod(type);
                    }
                }
                memberSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            /// 真实类型序列化函数集合
            /// </summary>
            private static interlocked.DictionaryPlus<Type, Action<DataSerializerPlus, object>> realSerializers = new interlocked.DictionaryPlus<Type,Action<DataSerializerPlus,object>>(DictionaryPlus.CreateOnly<Type, Action<DataSerializerPlus, object>>());
            /// <summary>
            /// 获取真实类型序列化函数
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>真实类型序列化函数</returns>
            public static Action<DataSerializerPlus, object> GetRealSerializer(Type type)
            {
                Action<DataSerializerPlus, object> method;
                if (realSerializers.TryGetValue(type, out method)) return method;
                method = (Action<DataSerializerPlus, object>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, object>), realTypeObjectMethod.MakeGenericMethod(type));
                realSerializers.Set(type, method);
                return method;
            }

            /// <summary>
            /// 是否支持循环引用处理集合
            /// </summary>
            private static interlocked.DictionaryPlus<Type, bool> _isReferenceMembers = new interlocked.DictionaryPlus<Type,bool>(DictionaryPlus.CreateOnly<Type, bool>());
            /// <summary>
            /// 是否支持循环引用处理
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public static bool IsReferenceMember(Type type)
            {
                bool isReferenceMember;
                if (_isReferenceMembers.TryGetValue(type, out isReferenceMember)) return isReferenceMember;
                _isReferenceMembers.Set(type, isReferenceMember = (bool)IsReferenceMemberMethod.MakeGenericMethod(type).Invoke(null, null));
                return isReferenceMember;
            }
            /// <summary>
            /// 是否支持循环引用处理
            /// </summary>
            /// <typeparam name="TValueType"></typeparam>
            /// <returns></returns>
            private static bool IsReferenceMember<TValueType>()
            {
                return TypeSerializerPlus<TValueType>.IsReferenceMember;
            }
            /// <summary>
            /// 是否支持循环引用处理函数信息
            /// </summary>
            private static readonly MethodInfo IsReferenceMemberMethod = typeof(TypeSerializerPlus).GetMethod("isReferenceMember", BindingFlags.Static | BindingFlags.NonPublic);
        }
        /// <summary>
        /// 二进制数据序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        internal static class TypeSerializerPlus<TValueType>
        {
            /// <summary>
            /// 二进制数据序列化类型配置
            /// </summary>
            private static readonly DataSerializePlus Attribute;
            /// <summary>
            /// 序列化委托
            /// </summary>
            internal static readonly Action<DataSerializerPlus, TValueType> DefaultSerializer;
            /// <summary>
            /// 固定分组成员序列化
            /// </summary>
            private static readonly Action<DataSerializerPlus, TValueType> FixedMemberSerializer;
            /// <summary>
            /// 固定分组成员位图序列化
            /// </summary>
            private static readonly Action<memberMap, DataSerializerPlus, TValueType> FixedMemberMapSerializer;
            /// <summary>
            /// 成员序列化
            /// </summary>
            private static readonly Action<DataSerializerPlus, TValueType> MemberSerializer;
            /// <summary>
            /// 成员位图序列化
            /// </summary>
            private static readonly Action<memberMap, DataSerializerPlus, TValueType> MemberMapSerializer;
            /// <summary>
            /// JSON混合序列化位图
            /// </summary>
            private static readonly memberMap JsonMemberMap;
            /// <summary>
            /// JSON混合序列化成员索引集合
            /// </summary>
            private static readonly int[] JsonMemberIndexs;
            /// <summary>
            /// 序列化成员数量
            /// </summary>
            private static readonly int MemberCountVerify;
            /// <summary>
            /// 固定分组字节数
            /// </summary>
            private static readonly int FixedSize;
            /// <summary>
            /// 固定分组填充字节数
            /// </summary>
            private static readonly int FixedFillSize;
            /// <summary>
            /// 是否值类型
            /// </summary>
            private static readonly bool IsValueType;
            /// <summary>
            /// 是否支持循环引用处理
            /// </summary>
            internal static readonly bool IsReferenceMember;
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Serialize(DataSerializerPlus serializer, TValueType value)
            {
                if (IsValueType) StructSerialize(serializer, value);
                else ClassSerialize(serializer, value);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            internal static void ClassSerialize(DataSerializerPlus serializer, TValueType value)
            {
                if (DefaultSerializer == null)
                {
                    if (serializer.CheckPoint(value))
                    {
                        if (serializer.serializeConfig.IsRealType)
                        {
                            Type type = value.GetType();
                            if (type != typeof(TValueType))
                            {
                                if (serializer.CheckPoint(value))
                                {
                                    serializer.Stream.Write(fastCSharp.emit.DataSerializerPlus.RealTypeValue);
                                    TypeSerializerPlus.GetRealSerializer(type)(serializer, value);
                                }
                                return;
                            }
                        }
                        if (ConstructorPlus<TValueType>.New == null) serializer.Stream.Write(NullValue);
                        else MemberSerialize(serializer, value);
                    }
                }
                else DefaultSerializer(serializer, value);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void StructSerialize(DataSerializerPlus serializer, TValueType value)
            {
                if (DefaultSerializer == null) MemberSerialize(serializer, value);
                else DefaultSerializer(serializer, value);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            internal static void MemberSerialize(DataSerializerPlus serializer, TValueType value)
            {
                memberMap memberMap = Attribute.IsMemberMap ? serializer.SerializeMemberMap<TValueType>() : null;
                var stream = serializer.Stream;
                if (memberMap == null)
                {
                    stream.PrepLength(FixedSize);
                    stream.Unsafer.Write(MemberCountVerify);
                    FixedMemberSerializer(serializer, value);
                    stream.Unsafer.AddLength(FixedFillSize);
                    stream.PrepLength();
                    MemberSerializer(serializer, value);
                    if (JsonMemberMap == null)
                    {
                        if (Attribute.IsJson) stream.Write(0);
                    }
                    else
                    {
                        var buffer = UnmanagedPoolPlus.StreamBuffers.Get();
                        try
                        {
                            using (charStream jsonStream = serializer.ResetJsonStream(buffer.Data, UnmanagedPoolPlus.StreamBuffers.Size))
                            {
                                jsonSerializer.Serialize(value, jsonStream, stream, serializer.getJsonConfig(JsonMemberMap));
                            }
                        }
                        finally { UnmanagedPoolPlus.StreamBuffers.Push(ref buffer); }
                    }
                }
                else
                {
                    stream.PrepLength(FixedSize - sizeof(int));
                    int length = stream.OffsetLength;
                    FixedMemberMapSerializer(memberMap, serializer, value);
                    stream.Unsafer.AddLength((length - stream.OffsetLength) & 3);
                    stream.PrepLength();
                    MemberMapSerializer(memberMap, serializer, value);
                    if (JsonMemberMap == null || (memberMap = serializer.getJsonMemberMap<TValueType>(memberMap, JsonMemberIndexs)) == null)
                    {
                        if (Attribute.IsJson) stream.Write(0);
                    }
                    else
                    {
                        PointerStruct buffer = UnmanagedPoolPlus.StreamBuffers.Get();
                        try
                        {
                            using (charStream jsonStream = serializer.ResetJsonStream(buffer.Data, UnmanagedPoolPlus.StreamBuffers.Size))
                            {
                                jsonSerializer.Serialize(value, jsonStream, stream, serializer.getJsonConfig(memberMap));
                            }
                        }
                        finally { UnmanagedPoolPlus.StreamBuffers.Push(ref buffer); }
                    }
                }
            }
            /// <summary>
            /// 真实类型序列化
            /// </summary>
            /// <param name="serializer"></param>
            /// <param name="value"></param>
            internal static void RealTypeObject(DataSerializerPlus serializer, object value)
            {
                if (IsValueType)
                {
                    TypeSerializerPlus<remoteType>.StructSerialize(serializer, typeof(TValueType));
                    StructSerialize(serializer, (TValueType)value);
                }
                else
                {
                    if (ConstructorPlus<TValueType>.New == null) serializer.Stream.Write(NullValue);
                    else
                    {
                        TypeSerializerPlus<remoteType>.StructSerialize(serializer, typeof(TValueType));
                        if (DefaultSerializer == null)
                        {
                            if (serializer.CheckPoint(value)) MemberSerialize(serializer, (TValueType)value);
                        }
                        else DefaultSerializer(serializer, (TValueType)value);
                    }
                }
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void BaseSerialize<TChildType>(DataSerializerPlus serializer, TChildType value) where TChildType : TValueType
            {
                if (serializer.CheckPoint(value)) StructSerialize(serializer, value);
            }
            /// <summary>
            /// 找不到构造函数
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void NoConstructor(DataSerializerPlus serializer, TValueType value)
            {
                if (serializer.CheckPoint(value))
                {
                    if (serializer.serializeConfig.IsRealType) serializer.Stream.Write(BinarySerializerPlus.NullValue);
                    else
                    {
                        Type type = value.GetType();
                        if (type == typeof(TValueType)) serializer.Stream.Write(BinarySerializerPlus.NullValue);
                        else TypeSerializerPlus.GetRealSerializer(type)(serializer, value);
                    }
                }
            }
            /// <summary>
            /// 不支持对象转换null
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void ToNull(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write(NullValue);
            }
            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void EnumByte(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write((uint)PubPlus.EnumCastEnum<TValueType, byte>.ToInt(value));
            }
            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EnumSByte(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write((int)pub.enumCast<TValueType, sbyte>.ToInt(value));
            }
            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void enumShort(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write((int)pub.enumCast<TValueType, short>.ToInt(value));
            }
            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void enumUShort(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write((uint)pub.enumCast<TValueType, ushort>.ToInt(value));
            }
            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void enumInt(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write(pub.enumCast<TValueType, int>.ToInt(value));
            }
            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void enumUInt(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write(pub.enumCast<TValueType, uint>.ToInt(value));
            }
            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void enumLong(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write(pub.enumCast<TValueType, long>.ToInt(value));
            }
            /// <summary>
            /// 枚举值序列化
            /// </summary>
            /// <param name="serializer">二进制数据序列化</param>
            /// <param name="array">枚举值序列化</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe static void enumULong(DataSerializerPlus serializer, TValueType value)
            {
                serializer.Stream.Write(pub.enumCast<TValueType, ulong>.ToInt(value));
            }
            /// <summary>
            /// 获取字段成员集合
            /// </summary>
            /// <returns>字段成员集合</returns>
            public static SubArrayStruct<memberIndex> GetMembers()
            {
                if (FixedMemberSerializer == null) return default(SubArrayStruct<memberIndex>);
                return TypeSerializerPlus.GetMembers(memberIndexGroup<TValueType>.GetFields(Attribute.MemberFilter));
            }
            static TypeSerializerPlus()
            {
                Type type = typeof(TValueType), attributeType;
                MethodInfo methodInfo = DataSerializerPlus.getSerializeMethod(type);
                Attribute = type.customAttribute<dataSerialize>(out attributeType, true) ?? dataSerialize.Default;
                if (methodInfo != null)
                {
                    DynamicMethod dynamicMethod = new DynamicMethod("DataSerializerPlus", typeof(void), new Type[] { typeof(DataSerializerPlus), type }, true);
                    dynamicMethod.InitLocals = true;
                    ILGenerator generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    DefaultSerializer = (Action<DataSerializerPlus, TValueType>)dynamicMethod.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>));
                    IsValueType = true;
                    IsReferenceMember = false;
                    return;
                }
                if (type.IsArray)
                {
                    IsValueType = true;
                    if (type.GetArrayRank() == 1)
                    {
                        Type elementType = type.GetElementType();
                        if (!elementType.IsPointer)
                        {
                            if (elementType.IsValueType)
                            {
                                if (elementType.IsEnum)
                                {
                                    Type enumType = System.Enum.GetUnderlyingType(elementType);
                                    if (enumType == typeof(uint)) methodInfo = enumUIntArrayMethod;
                                    else if (enumType == typeof(byte)) methodInfo = enumByteArrayMethod;
                                    else if (enumType == typeof(ulong)) methodInfo = enumULongArrayMethod;
                                    else if (enumType == typeof(ushort)) methodInfo = enumUShortArrayMethod;
                                    else if (enumType == typeof(long)) methodInfo = enumLongArrayMethod;
                                    else if (enumType == typeof(short)) methodInfo = enumShortArrayMethod;
                                    else if (enumType == typeof(sbyte)) methodInfo = enumSByteArrayMethod;
                                    else methodInfo = enumIntArrayMethod;
                                    methodInfo = methodInfo.MakeGenericMethod(elementType);
                                    IsReferenceMember = false;
                                }
                                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    methodInfo = nullableArrayMethod.MakeGenericMethod(elementType = elementType.GetGenericArguments()[0]);
                                    IsReferenceMember = TypeSerializerPlus.IsReferenceMember(elementType);
                                }
                                else
                                {
                                    methodInfo = structArrayMethod.MakeGenericMethod(elementType);
                                    IsReferenceMember = TypeSerializerPlus.IsReferenceMember(elementType);
                                }
                            }
                            else
                            {
                                methodInfo = arrayMethod.MakeGenericMethod(elementType);
                                IsReferenceMember = TypeSerializerPlus.IsReferenceMember(elementType);
                            }
                            DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), methodInfo);
                            return;
                        }
                    }
                    DefaultSerializer = ToNull;
                    IsReferenceMember = false;
                    return;
                }
                if (type.IsEnum)
                {
                    Type enumType = System.Enum.GetUnderlyingType(type);
                    if (enumType == typeof(uint)) DefaultSerializer = enumUInt;
                    else if (enumType == typeof(byte)) DefaultSerializer = EnumByte;
                    else if (enumType == typeof(ulong)) DefaultSerializer = enumULong;
                    else if (enumType == typeof(ushort)) DefaultSerializer = enumUShort;
                    else if (enumType == typeof(long)) DefaultSerializer = enumLong;
                    else if (enumType == typeof(short)) DefaultSerializer = enumShort;
                    else if (enumType == typeof(sbyte)) DefaultSerializer = EnumSByte;
                    else DefaultSerializer = enumInt;
                    IsValueType = true;
                    IsReferenceMember = false;
                    return;
                }
                if (type.IsPointer)
                {
                    DefaultSerializer = ToNull;
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
                        DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), SubArrayStructSerializeMethod.MakeGenericMethod(parameterTypes));
                        IsReferenceMember = TypeSerializerPlus.IsReferenceMember(parameterTypes[0]);
                        IsValueType = true;
                        return;
                    }
                    if (genericType == typeof(DictionaryPlus<,>) || genericType == typeof(SortedDictionaryPlus<,>) || genericType == typeof(SortedList<,>))
                    {
                        DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), DictionaryPlusSerializeMethod.MakeGenericMethod(type, parameterTypes[0], parameterTypes[1]));
                        IsReferenceMember = TypeSerializerPlus.IsReferenceMember(parameterTypes[0]) || TypeSerializerPlus.IsReferenceMember(parameterTypes[1]);
                        IsValueType = true;
                        return;
                    }
                    if (genericType == typeof(Nullable<>))
                    {
                        DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), nullableSerializeMethod.MakeGenericMethod(parameterTypes));
                        IsReferenceMember = TypeSerializerPlus.IsReferenceMember(parameterTypes[0]);
                        IsValueType = true;
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), keyValuePairSerializeMethod.MakeGenericMethod(parameterTypes));
                        IsReferenceMember = TypeSerializerPlus.IsReferenceMember(parameterTypes[0]) || TypeSerializerPlus.IsReferenceMember(parameterTypes[1]);
                        IsValueType = true;
                        return;
                    }
                }
                if ((methodInfo = TypeSerializerPlus.GetCustom(type, true)) != null)
                {
                    DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), methodInfo);
                    IsReferenceMember = Attribute.IsReferenceMember;
                    IsValueType = true;
                    return;
                }
                if (type.IsAbstract || type.IsInterface || constructor<TValueType>.New == null)
                {
                    DefaultSerializer = NoConstructor;
                    IsValueType = IsReferenceMember = true;
                    return;
                }
                ConstructorInfo constructorInfo = null;
                Type argumentType = null;
                IsReferenceMember = Attribute.IsReferenceMember;
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType)
                    {
                        Type genericType = interfaceType.GetGenericTypeDefinition();
                        if (genericType == typeof(ICollection<>))
                        {
                            Type[] parameterTypes = interfaceType.GetGenericArguments();
                            argumentType = parameterTypes[0];
                            parameterTypes[0] = argumentType.MakeArrayType();
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);
                            if (constructorInfo != null) break;
                            parameterTypes[0] = typeof(IList<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);
                            if (constructorInfo != null) break;
                            parameterTypes[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);
                            if (constructorInfo != null) break;
                            parameterTypes[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);
                            if (constructorInfo != null) break;
                        }
                        else if (genericType == typeof(IDictionaryPlus<,>))
                        {
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { interfaceType }, null);
                            if (constructorInfo != null)
                            {
                                Type[] parameters = interfaceType.GetGenericArguments();
                                methodInfo = (type.IsValueType ? structDictionaryPlusMethod : classDictionaryPlusMethod).MakeGenericMethod(type, parameters[0], parameters[1]);
                                DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), methodInfo);
                                return;
                            }
                        }
                    }
                }
                if (constructorInfo != null)
                {
                    if (argumentType.IsValueType && argumentType.IsEnum)
                    {
                        Type enumType = System.Enum.GetUnderlyingType(argumentType);
                        if (enumType == typeof(uint)) methodInfo = type.IsValueType ? structEnumUIntCollectionMethod : classEnumUIntCollectionMethod;
                        else if (enumType == typeof(byte)) methodInfo = type.IsValueType ? structEnumByteCollectionMethod : classEnumByteCollectionMethod;
                        else if (enumType == typeof(ulong)) methodInfo = type.IsValueType ? structEnumULongCollectionMethod : classEnumULongCollectionMethod;
                        else if (enumType == typeof(ushort)) methodInfo = type.IsValueType ? structEnumUShortCollectionMethod : classEnumUShortCollectionMethod;
                        else if (enumType == typeof(long)) methodInfo = type.IsValueType ? structEnumLongCollectionMethod : classEnumLongCollectionMethod;
                        else if (enumType == typeof(short)) methodInfo = type.IsValueType ? structEnumShortCollectionMethod : classEnumShortCollectionMethod;
                        else if (enumType == typeof(sbyte)) methodInfo = type.IsValueType ? structEnumSByteCollectionMethod : classEnumSByteCollectionMethod;
                        else methodInfo = type.IsValueType ? structEnumIntCollectionMethod : classEnumIntCollectionMethod;
                        methodInfo = methodInfo.MakeGenericMethod(argumentType, type);
                    }
                    else methodInfo = (type.IsValueType ? structCollectionMethod : classCollectionMethod).MakeGenericMethod(argumentType, type);
                    DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), methodInfo);
                    return;
                }
                if (typeof(fastCSharp.code.cSharp.dataSerialize.ISerialize).IsAssignableFrom(type))
                {
                    methodInfo = type.IsValueType ? DataSerializerPlus.structISerializeMethod : DataSerializerPlus.classISerializeMethod;
                    DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), methodInfo.MakeGenericMethod(type));
                    IsValueType = true;
                }
                else
                {
                    if (type.IsValueType) IsValueType = true;
                    else if (Attribute != dataSerialize.Default && attributeType != type)
                    {
                        for (Type baseType = type.BaseType; baseType != typeof(object); baseType = baseType.BaseType)
                        {
                            dataSerialize baseAttribute = fastCSharp.code.typeAttribute.GetAttribute<dataSerialize>(baseType, false, true);
                            if (baseAttribute != null)
                            {
                                if (baseAttribute.IsBaseType)
                                {
                                    methodInfo = baseSerializeMethod.MakeGenericMethod(baseType, type);
                                    DefaultSerializer = (Action<DataSerializerPlus, TValueType>)Delegate.CreateDelegate(typeof(Action<DataSerializerPlus, TValueType>), methodInfo);
                                    return;
                                }
                                break;
                            }
                        }
                    }
                    fields<fieldInfo> fields = TypeSerializerPlus.GetFields(memberIndexGroup<TValueType>.GetFields(Attribute.MemberFilter), out MemberCountVerify);
                    FixedFillSize = -fields.FixedSize & 3;
                    FixedSize = (fields.FixedSize + (sizeof(int) + 3)) & (int.MaxValue - 3);
                    TypeSerializerPlus.MemberDynamicMethodStruct fixedDynamicMethod = new TypeSerializerPlus.MemberDynamicMethodStruct(type);
                    TypeSerializerPlus.MemberMapDynamicMethodStruct fixedMemberMapDynamicMethod = Attribute.IsMemberMap ? new TypeSerializerPlus.MemberMapDynamicMethodStruct(type) : default(TypeSerializerPlus.MemberMapDynamicMethodStruct);
                    foreach (fieldInfo member in fields.FixedFields)
                    {
                        fixedDynamicMethod.Push(member);
                        if (Attribute.IsMemberMap) fixedMemberMapDynamicMethod.Push(member);
                    }
                    FixedMemberSerializer = (Action<DataSerializerPlus, TValueType>)fixedDynamicMethod.Create<Action<DataSerializerPlus, TValueType>>();
                    if (Attribute.IsMemberMap) FixedMemberMapSerializer = (Action<memberMap, DataSerializerPlus, TValueType>)fixedMemberMapDynamicMethod.Create<Action<memberMap, DataSerializerPlus, TValueType>>();

                    TypeSerializerPlus.MemberDynamicMethodStruct dynamicMethod = new TypeSerializerPlus.MemberDynamicMethodStruct(type);
                    TypeSerializerPlus.MemberMapDynamicMethodStruct memberMapDynamicMethod = Attribute.IsMemberMap ? new TypeSerializerPlus.MemberMapDynamicMethodStruct(type) : default(TypeSerializerPlus.MemberMapDynamicMethodStruct);
                    foreach (fieldInfo member in fields.Fields)
                    {
                        dynamicMethod.Push(member);
                        if (Attribute.IsMemberMap) memberMapDynamicMethod.Push(member);
                    }
                    MemberSerializer = (Action<DataSerializerPlus, TValueType>)dynamicMethod.Create<Action<DataSerializerPlus, TValueType>>();
                    if (Attribute.IsMemberMap) MemberMapSerializer = (Action<memberMap, DataSerializerPlus, TValueType>)memberMapDynamicMethod.Create<Action<memberMap, DataSerializerPlus, TValueType>>();

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
        private DictionaryPlus<objectReference, int> points;
        /// <summary>
        /// 序列化配置参数
        /// </summary>
        private ConfigPlus serializeConfig;
        /// <summary>
        /// 是否支持循环引用处理
        /// </summary>
        private bool isReferenceMember;
        /// <summary>
        /// 是否检测数组引用
        /// </summary>
        private bool isReferenceArray;
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>序列化数据</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] serialize<valueType>(valueType value, ConfigPlus config)
        {
            binarySerializerConfig = serializeConfig = config;
            pointer buffer = UnmanagedPoolPlus.StreamBuffers.Get();
            try
            {
                Stream.Reset(buffer.Byte, UnmanagedPoolPlus.StreamBuffers.Size);
                using (Stream)
                {
                    serialize(value);
                    return Stream.GetArray();
                }
            }
            finally { UnmanagedPoolPlus.StreamBuffers.Push(ref buffer); }
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="stream">序列化输出缓冲区</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize<valueType>(valueType value, unmanagedStream stream, ConfigPlus config)
        {
            binarySerializerConfig = serializeConfig = config;
            this.Stream.From(stream);
            try
            {
                serialize(value);
            }
            finally { stream.From(this.Stream); }
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize<valueType>(valueType value)
        {
            isReferenceMember = TypeSerializerPlus<valueType>.IsReferenceMember;
            if (points == null && isReferenceMember) points = DictionaryPlus<objectReference>.Create<int>();
            isReferenceArray = true;
            memberMap = serializeConfig.MemberMap;
            streamStartIndex = Stream.OffsetLength;
            Stream.Write(serializeConfig.HeaderValue);
            TypeSerializerPlus<valueType>.Serialize(this, value);
            Stream.Write(Stream.OffsetLength - streamStartIndex);
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>序列化数据</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] codeSerialize<valueType>(valueType value, ConfigPlus config) where valueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            binarySerializerConfig = serializeConfig = config;
            pointer buffer = UnmanagedPoolPlus.StreamBuffers.Get();
            try
            {
                Stream.Reset(buffer.Byte, UnmanagedPoolPlus.StreamBuffers.Size);
                using (Stream)
                {
                    codeSerialize(value);
                    return Stream.GetArray();
                }
            }
            finally { UnmanagedPoolPlus.StreamBuffers.Push(ref buffer); }
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="stream">序列化输出缓冲区</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void codeSerialize<valueType>(valueType value, unmanagedStream stream, ConfigPlus config) where valueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            binarySerializerConfig = serializeConfig = config;
            this.Stream.From(stream);
            try
            {
                codeSerialize(value);
            }
            finally { stream.From(this.Stream); }
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void codeSerialize<valueType>(valueType value) where valueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            isReferenceMember = TypeSerializerPlus<valueType>.IsReferenceMember;
            if (points == null && isReferenceMember) points = DictionaryPlus<objectReference>.Create<int>();
            isReferenceArray = true;
            memberMap = serializeConfig.MemberMap;
            streamStartIndex = Stream.OffsetLength;
            Stream.Write(serializeConfig.HeaderValue);
            value.Serialize(this);
            Stream.Write(Stream.OffsetLength - streamStartIndex);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private new void free()
        {
            base.free();
            if (points != null) points.Clear();
            typePool<DataSerializerPlus>.Push(this);
        }
        /// <summary>
        /// 添加历史对象
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckPoint<valueType>(valueType value)
        {
            if (isReferenceMember)
            {
                int point;
                if (points.TryGetValue(new fastCSharp.objectReference { Value = value }, out point))
                {
                    Stream.Write(-point);
                    return false;
                }
                points[new fastCSharp.objectReference { Value = value }] = Stream.OffsetLength - streamStartIndex;
            }
            return true;
        }
        /// <summary>
        /// 添加历史对象
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool checkPoint<valueType>(valueType[] value)
        {
            if (value.Length == 0)
            {
                Stream.Write(0);
                isReferenceArray = true;
                return false;
            }
            if (isReferenceArray) return CheckPoint<valueType[]>(value);
            return isReferenceArray = true;
        }
        /// <summary>
        /// 判断成员索引是否有效
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMemberMap(int memberIndex)
        {
            return currentMemberMap.IsMember(memberIndex);
        }
        /// <summary>
        /// 逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(bool[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(bool[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(bool?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(bool?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(byte[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(byte[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(byte?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(byte?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(sbyte[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(sbyte[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(sbyte?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(sbyte?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(short[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(short[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(short?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(short?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(ushort[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ushort[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(ushort?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ushort?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(int[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(int[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(int?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(int?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(uint[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(uint[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(uint?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(uint?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>x
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(long[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(long[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(long?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(long?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(ulong[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ulong[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(ulong?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(ulong?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(float[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(float[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(float?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(float?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(double[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(double[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(double?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(double?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(decimal[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(decimal[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(decimal?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(decimal?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(char[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(char[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(char?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(char?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(DateTime[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(DateTime[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(DateTime?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(DateTime?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(Guid[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(Guid[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(Guid?[] value)
        {
            if (checkPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(Guid?[] value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(string value)
        {
            if (value.Length == 0) Stream.Write(0);
            else if (CheckPoint(value)) Serialize(Stream, value);
        }
        /// <summary>
        /// 字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(string value)
        {
            if (value == null) Stream.Write(NullValue);
            else serialize(value);
        }
        /// <summary>
        /// 字符串序列化
        /// </summary>
        /// <param name="array">字符串数组</param>
        [MemberSerializeMethodPlus]
        [MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberSerialize(string[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else serialize(array);
        }
        /// <summary>
        /// 字符串序列化
        /// </summary>
        /// <param name="array">字符串数组</param>
        [SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void serialize(string[] array)
        {
            if (checkPoint(array))
            {
                arrayMap arrayMap = new arrayMap(Stream, array.Length, array.Length);
                foreach (string value in array) arrayMap.Next(value != null);
                arrayMap.End(Stream);
                foreach (string value in array)
                {
                    if (value != null)
                    {
                        if (value.Length == 0) Stream.Write(0);
                        else if (CheckPoint(value)) Serialize(Stream, value);
                    }
                }
            }
        }

        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void structSerialize<valueType>(DataSerializerPlus serializer, valueType value) where valueType : struct
        {
            TypeSerializerPlus<valueType>.StructSerialize(serializer, value);
        }
        /// <summary>
        /// 对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo structSerializeMethod = typeof(DataSerializerPlus).GetMethod("structSerialize", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DictionaryPlusSerialize<DictionaryPlusType, keyType, valueType>(DictionaryPlusType value) where DictionaryPlusType : IDictionaryPlus<keyType, valueType>
        {
            if (CheckPoint(value))
            {
                int index = 0;
                keyType[] keys = new keyType[value.Count];
                valueType[] values = new valueType[keys.Length];
                foreach (KeyValuePair<keyType, valueType> keyValue in value)
                {
                    keys[index] = keyValue.Key;
                    values[index++] = keyValue.Value;
                }
                isReferenceArray = false;
                TypeSerializerPlus<keyType[]>.DefaultSerializer(this, keys);
                isReferenceArray = false;
                TypeSerializerPlus<valueType[]>.DefaultSerializer(this, values);
            }
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo DictionaryPlusSerializeMethod = typeof(DataSerializerPlus).GetMethod("DictionaryPlusSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DictionaryPlusMember<DictionaryPlusType, keyType, valueType>(DictionaryPlusType value) where DictionaryPlusType : IDictionaryPlus<keyType, valueType>
        {
            if (value == null) Stream.Write(NullValue);
            else DictionaryPlusSerialize<DictionaryPlusType, keyType, valueType>(value);
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo DictionaryPlusMemberMethod = typeof(DataSerializerPlus).GetMethod("DictionaryPlusMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void keyValuePairSerialize<keyType, valueType>(DataSerializerPlus serializer, KeyValuePair<keyType, valueType> value)
        {
            TypeSerializerPlus<keyValue<keyType, valueType>>.MemberSerialize(serializer, new keyValue<keyType, valueType>(value.Key, value.Value));
        }
        /// <summary>
        /// 字典序列化函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairSerializeMethod = typeof(DataSerializerPlus).GetMethod("keyValuePairSerialize", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SubArrayStructSerialize<valueType>(SubArrayStruct<valueType> value)
        {
            valueType[] array = value.ToArray();
            isReferenceArray = false;
            TypeSerializerPlus<valueType[]>.DefaultSerializer(this, array);
        }
        /// <summary>
        /// 数组序列化函数信息
        /// </summary>
        private static readonly MethodInfo SubArrayStructSerializeMethod = typeof(DataSerializerPlus).GetMethod("SubArrayStructSerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void nullableSerialize<valueType>(DataSerializerPlus serializer, Nullable<valueType> value) where valueType : struct
        {
            TypeSerializerPlus<valueType>.StructSerialize(serializer, value.Value);
        }
        /// <summary>
        /// 对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableSerializeMethod = typeof(DataSerializerPlus).GetMethod("nullableSerialize", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void nullableMemberSerialize<valueType>(DataSerializerPlus serializer, Nullable<valueType> value) where valueType : struct
        {
            if (value.HasValue) TypeSerializerPlus<valueType>.StructSerialize(serializer, value.Value);
            else serializer.Stream.Write(NullValue);
        }
        /// <summary>
        /// 对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberSerializeMethod = typeof(DataSerializerPlus).GetMethod("nullableMemberSerialize", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void baseSerialize<valueType, childType>(DataSerializerPlus serializer, childType value) where childType : valueType
        {
            TypeSerializerPlus<valueType>.BaseSerialize(serializer, value);
        }
        /// <summary>
        /// 对象序列化函数信息
        /// </summary>
        private static readonly MethodInfo baseSerializeMethod = typeof(DataSerializerPlus).GetMethod("baseSerialize", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 真实类型序列化
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="value"></param>
        private static void realTypeObject<valueType>(DataSerializerPlus serializer, object value)
        {
            TypeSerializerPlus<valueType>.RealTypeObject(serializer, value);
        }
        /// <summary>
        /// 真实类型序列化函数信息
        /// </summary>
        private static readonly MethodInfo realTypeObjectMethod = typeof(DataSerializerPlus).GetMethod("realTypeObject", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void structCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            isReferenceArray = false;
            TypeSerializerPlus<valueType[]>.DefaultSerializer(this, collection.getArray());
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structCollectionMethod = typeof(DataSerializerPlus).GetMethod("structCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void classCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classCollectionMethod = typeof(DataSerializerPlus).GetMethod("classCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void structDictionaryPlus<DictionaryPlusType, keyType, valueType>(DictionaryPlusType DictionaryPlus) where DictionaryPlusType : IDictionaryPlus<keyType, valueType>
        {
            keyType[] keys = new keyType[DictionaryPlus.Count];
            valueType[] values = new valueType[keys.Length];
            int index = 0;
            foreach (KeyValuePair<keyType, valueType> keyValue in DictionaryPlus)
            {
                keys[index] = keyValue.Key;
                values[index++] = keyValue.Value;
            }
            isReferenceArray = false;
            TypeSerializerPlus<keyType[]>.DefaultSerializer(this, keys);
            isReferenceArray = false;
            TypeSerializerPlus<valueType[]>.DefaultSerializer(this, values);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structDictionaryPlusMethod = typeof(DataSerializerPlus).GetMethod("structDictionaryPlus", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 集合转换
        /// </summary>
        /// <param name="collection">对象集合</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void classDictionaryPlus<DictionaryPlusType, keyType, valueType>(DictionaryPlusType DictionaryPlus) where DictionaryPlusType : IDictionaryPlus<keyType, valueType>
        {
            if (CheckPoint(DictionaryPlus)) structDictionaryPlus<DictionaryPlusType, keyType, valueType>(DictionaryPlus);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classDictionaryPlusMethod = typeof(DataSerializerPlus).GetMethod("classDictionaryPlus", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private unsafe void structEnumByteCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            int count = collection.Count, length = (count + 7) & (int.MaxValue - 3);
            Stream.PrepLength(length);
            byte* write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (valueType value in collection) *write++ = pub.enumCast<valueType, byte>.ToInt(value);
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumByteCollectionMethod = typeof(DataSerializerPlus).GetMethod("structEnumByteCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void classEnumByteCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structEnumByteCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumByteCollectionMethod = typeof(DataSerializerPlus).GetMethod("classEnumByteCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private unsafe void structEnumSByteCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            int count = collection.Count, length = (count + 7) & (int.MaxValue - 3);
            Stream.PrepLength(length);
            byte* write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (valueType value in collection) *(sbyte*)write++ = pub.enumCast<valueType, sbyte>.ToInt(value);
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumSByteCollectionMethod = typeof(DataSerializerPlus).GetMethod("structEnumSByteCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void classEnumSByteCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structEnumSByteCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumSByteCollectionMethod = typeof(DataSerializerPlus).GetMethod("classEnumSByteCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private unsafe void structEnumShortCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            int count = collection.Count, length = ((count * sizeof(short)) + 7) & (int.MaxValue - 3);
            Stream.PrepLength(length);
            byte* write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (valueType value in collection)
            {
                *(short*)write = pub.enumCast<valueType, short>.ToInt(value);
                write += sizeof(short);
            }
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumShortCollectionMethod = typeof(DataSerializerPlus).GetMethod("structEnumShortCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void classEnumShortCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structEnumShortCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumShortCollectionMethod = typeof(DataSerializerPlus).GetMethod("classEnumShortCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private unsafe void structEnumUShortCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            int count = collection.Count, length = ((count * sizeof(ushort)) + 7) & (int.MaxValue - 3);
            Stream.PrepLength(length);
            byte* write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (valueType value in collection)
            {
                *(ushort*)write = pub.enumCast<valueType, ushort>.ToInt(value);
                write += sizeof(ushort);
            }
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumUShortCollectionMethod = typeof(DataSerializerPlus).GetMethod("structEnumUShortCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void classEnumUShortCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structEnumUShortCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumUShortCollectionMethod = typeof(DataSerializerPlus).GetMethod("classEnumUShortCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private unsafe void structEnumIntCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            int count = collection.Count, length = (count + 1) * sizeof(int);
            Stream.PrepLength(length);
            byte* write = Stream.CurrentData;
            *(int*)write = count;
            foreach (valueType value in collection) *(int*)(write += sizeof(int)) = pub.enumCast<valueType, int>.ToInt(value);
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumIntCollectionMethod = typeof(DataSerializerPlus).GetMethod("structEnumIntCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void classEnumIntCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structEnumIntCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumIntCollectionMethod = typeof(DataSerializerPlus).GetMethod("classEnumIntCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private unsafe void structEnumUIntCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            int count = collection.Count, length = (count + 1) * sizeof(uint);
            Stream.PrepLength(length);
            byte* write = Stream.CurrentData;
            *(int*)write = count;
            foreach (valueType value in collection) *(uint*)(write += sizeof(uint)) = pub.enumCast<valueType, uint>.ToInt(value);
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumUIntCollectionMethod = typeof(DataSerializerPlus).GetMethod("structEnumUIntCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void classEnumUIntCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structEnumUIntCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumUIntCollectionMethod = typeof(DataSerializerPlus).GetMethod("classEnumUIntCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private unsafe void structEnumLongCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            int count = collection.Count, length = count * sizeof(long) + sizeof(int);
            Stream.PrepLength(length);
            byte* write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (valueType value in collection)
            {
                *(long*)write = pub.enumCast<valueType, long>.ToInt(value);
                write += sizeof(long);
            }
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumLongCollectionMethod = typeof(DataSerializerPlus).GetMethod("structEnumLongCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void classEnumLongCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structEnumLongCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumLongCollectionMethod = typeof(DataSerializerPlus).GetMethod("classEnumLongCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        private unsafe void structEnumULongCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            int count = collection.Count, length = count * sizeof(ulong) + sizeof(int);
            Stream.PrepLength(length);
            byte* write = Stream.CurrentData;
            *(int*)write = count;
            write += sizeof(int);
            foreach (valueType value in collection)
            {
                *(ulong*)write = pub.enumCast<valueType, ulong>.ToInt(value);
                write += sizeof(ulong);
            }
            Stream.Unsafer.AddLength(length);
            Stream.PrepLength();
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo structEnumULongCollectionMethod = typeof(DataSerializerPlus).GetMethod("structEnumULongCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合序列化
        /// </summary>
        /// <param name="collection">枚举集合序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void classEnumULongCollection<valueType, collectionType>(collectionType collection) where collectionType : ICollection<valueType>
        {
            if (CheckPoint(collection)) structEnumULongCollection<valueType, collectionType>(collection);
        }
        /// <summary>
        /// 集合序列化函数信息
        /// </summary>
        private static readonly MethodInfo classEnumULongCollectionMethod = typeof(DataSerializerPlus).GetMethod("classEnumULongCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMemberMethod = typeof(DataSerializerPlus).GetMethod("enumByteMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMemberMethod = typeof(DataSerializerPlus).GetMethod("enumSByteMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMemberMethod = typeof(DataSerializerPlus).GetMethod("enumShortMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMemberMethod = typeof(DataSerializerPlus).GetMethod("enumUShortMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntMemberMethod = typeof(DataSerializerPlus).GetMethod("enumIntMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntMemberMethod = typeof(DataSerializerPlus).GetMethod("enumUIntMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongMemberMethod = typeof(DataSerializerPlus).GetMethod("enumLongMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongMemberMethod = typeof(DataSerializerPlus).GetMethod("enumULongMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumByteArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                int length = (array.Length + 7) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                byte* write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (valueType value in array) *write++ = pub.enumCast<valueType, byte>.ToInt(value);
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMethod = typeof(DataSerializerPlus).GetMethod("enumByteArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumByteArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumByteArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumByteArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("enumByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumSByteArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                int length = (array.Length + 7) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                byte* write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (valueType value in array) *(sbyte*)write++ = pub.enumCast<valueType, sbyte>.ToInt(value);
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMethod = typeof(DataSerializerPlus).GetMethod("enumSByteArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumSByteArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumSByteArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("enumSByteArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumShortArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                int length = ((array.Length * sizeof(short)) + 7) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                byte* write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (valueType value in array)
                {
                    *(short*)write = pub.enumCast<valueType, short>.ToInt(value);
                    write += sizeof(short);
                }
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMethod = typeof(DataSerializerPlus).GetMethod("enumShortArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumShortArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumShortArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumShortArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("enumShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumUShortArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                int length = ((array.Length * sizeof(ushort)) + 7) & (int.MaxValue - 3);
                Stream.PrepLength(length);
                byte* write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (valueType value in array)
                {
                    *(ushort*)write = pub.enumCast<valueType, ushort>.ToInt(value);
                    write += sizeof(ushort);
                }
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMethod = typeof(DataSerializerPlus).GetMethod("enumUShortArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumUShortArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumUShortArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("enumUShortArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumIntArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                int length = (array.Length + 1) * sizeof(int);
                Stream.PrepLength(length);
                byte* write = Stream.CurrentData;
                *(int*)write = array.Length;
                foreach (valueType value in array) *(int*)(write += sizeof(int)) = pub.enumCast<valueType, int>.ToInt(value);
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMethod = typeof(DataSerializerPlus).GetMethod("enumIntArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumIntArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumIntArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumIntArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("enumIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumUIntArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                int length = (array.Length + 1) * sizeof(uint);
                Stream.PrepLength(length);
                byte* write = Stream.CurrentData;
                *(int*)write = array.Length;
                foreach (valueType value in array) *(uint*)(write += sizeof(uint)) = pub.enumCast<valueType, uint>.ToInt(value);
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMethod = typeof(DataSerializerPlus).GetMethod("enumUIntArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumUIntArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumUIntArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("enumUIntArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumLongArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                int length = array.Length * sizeof(long) + sizeof(int);
                Stream.PrepLength(length);
                byte* write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (valueType value in array)
                {
                    *(long*)write = pub.enumCast<valueType, long>.ToInt(value);
                    write += sizeof(long);
                }
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMethod = typeof(DataSerializerPlus).GetMethod("enumLongArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumLongArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumLongArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumLongArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("enumLongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        private unsafe void enumULongArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                int length = array.Length * sizeof(ulong) + sizeof(int);
                Stream.PrepLength(length);
                byte* write = Stream.CurrentData;
                *(int*)write = array.Length;
                write += sizeof(int);
                foreach (valueType value in array)
                {
                    *(ulong*)write = pub.enumCast<valueType, ulong>.ToInt(value);
                    write += sizeof(ulong);
                }
                Stream.Unsafer.AddLength(length);
                Stream.PrepLength();
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMethod = typeof(DataSerializerPlus).GetMethod("enumULongArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举数组序列化
        /// </summary>
        /// <param name="serializer">二进制数据序列化</param>
        /// <param name="array">枚举数组序列化</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void enumULongArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else enumULongArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo enumULongArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("enumULongArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void nullableArray<valueType>(Nullable<valueType>[] array) where valueType : struct
        {
            if (checkPoint(array))
            {
                arrayMap arrayMap = new arrayMap(Stream, array.Length);
                foreach (Nullable<valueType> value in array) arrayMap.Next(value.HasValue);
                arrayMap.End(Stream);

                foreach (Nullable<valueType> value in array)
                {
                    if (value.HasValue) TypeSerializerPlus<valueType>.StructSerialize(this, value.Value);
                }
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMethod = typeof(DataSerializerPlus).GetMethod("nullableArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void nullableArrayMember<valueType>(Nullable<valueType>[] array) where valueType : struct
        {
            if (array == null) Stream.Write(NullValue);
            else nullableArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("nullableArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void structArray<valueType>(valueType[] array)
        {
            if (checkPoint(array))
            {
                Stream.Write(array.Length);
                foreach (valueType value in array) TypeSerializerPlus<valueType>.StructSerialize(this, value);
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMethod = typeof(DataSerializerPlus).GetMethod("structArray", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void structArrayMember<valueType>(valueType[] array)
        {
            if (array == null) Stream.Write(NullValue);
            else structArray(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo structArrayMemberMethod = typeof(DataSerializerPlus).GetMethod("structArrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void array<valueType>(valueType[] array) where valueType : class
        {
            if (checkPoint(array))
            {
                arrayMap arrayMap = new arrayMap(Stream, array.Length);
                foreach (valueType value in array) arrayMap.Next(value != null);
                arrayMap.End(Stream);

                foreach (valueType value in array)
                {
                    if (value != null) TypeSerializerPlus<valueType>.ClassSerialize(this, value);
                }
            }
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayMethod = typeof(DataSerializerPlus).GetMethod("array", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void arrayMember<valueType>(valueType[] array) where valueType : class
        {
            if (array == null) Stream.Write(NullValue);
            else this.array(array);
        }
        /// <summary>
        /// 数组转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayMemberMethod = typeof(DataSerializerPlus).GetMethod("arrayMember", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 序列化接口
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void structISerialize<valueType>(valueType value) where valueType : struct, fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            value.Serialize(this);
        }
        /// <summary>
        /// 序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo structISerializeMethod = typeof(DataSerializerPlus).GetMethod("structISerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 序列化接口
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void classISerialize<valueType>(valueType value) where valueType : class, fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            value.Serialize(this);
        }
        /// <summary>
        /// 序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo classISerializeMethod = typeof(DataSerializerPlus).GetMethod("classISerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 序列化接口
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void memberClassISerialize<valueType>(valueType value) where valueType : class, fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            if (value == null) Stream.Write(NullValue);
            else value.Serialize(this);
        }
        /// <summary>
        /// 序列化接口函数信息
        /// </summary>
        private static readonly MethodInfo memberClassISerializeMethod = typeof(DataSerializerPlus).GetMethod("memberClassISerialize", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 引用类型成员序列化
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MemberClassSerialize<valueType>(valueType value) where valueType : class
        {
            if (value == null) Stream.Write(NullValue);
            else TypeSerializerPlus<valueType>.ClassSerialize(this, value);
        }
        /// <summary>
        /// 引用类型成员序列化函数信息
        /// </summary>
        private static readonly MethodInfo memberClassSerializeMethod = typeof(DataSerializerPlus).GetMethod("MemberClassSerialize", BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// 未知类型序列化
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MemberNullableSerialize<valueType>(Nullable<valueType> value) where valueType : struct
        {
            TypeSerializerPlus<valueType>.StructSerialize(this, value.Value);
        }
        /// <summary>
        /// 未知类型序列化
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MemberStructSerialize<valueType>(valueType value) where valueType : struct
        {
            TypeSerializerPlus<valueType>.StructSerialize(this, value);
        }

        /// <summary>
        /// 公共默认配置参数
        /// </summary>
        private static readonly ConfigPlus defaultConfig = new ConfigPlus { };
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="stream">序列化输出缓冲区</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<valueType>(valueType value, unmanagedStream stream, ConfigPlus config = null)
        {
            if (stream == null) log.Default.Throw(log.exceptionType.Null);
            if (value == null) stream.Write(BinarySerializerPlus.NullValue);
            else
            {
                DataSerializerPlus serializer = typePool<DataSerializerPlus>.Pop() ?? new DataSerializerPlus();
                try
                {
                    serializer.serialize<valueType>(value, stream, config ?? defaultConfig);
                }
                finally { serializer.free(); }
            }
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>序列化数据</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Serialize<valueType>(valueType value, ConfigPlus config = null)
        {
            if (value == null) return BitConverter.GetBytes(BinarySerializerPlus.NullValue);
            DataSerializerPlus serializer = typePool<DataSerializerPlus>.Pop() ?? new DataSerializerPlus();
            try
            {
                return serializer.serialize<valueType>(value, config ?? defaultConfig);
            }
            finally { serializer.free(); }
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="stream">序列化输出缓冲区</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CodeSerialize<valueType>(valueType value, unmanagedStream stream, ConfigPlus config = null) where valueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            if (stream == null) log.Default.Throw(log.exceptionType.Null);
            if (value == null) stream.Write(BinarySerializerPlus.NullValue);
            else
            {
                DataSerializerPlus serializer = typePool<DataSerializerPlus>.Pop() ?? new DataSerializerPlus();
                try
                {
                    serializer.codeSerialize<valueType>(value, stream, config ?? defaultConfig);
                }
                finally { serializer.free(); }
            }
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>序列化数据</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] CodeSerialize<valueType>(valueType value, ConfigPlus config = null) where valueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            if (value == null) return BitConverter.GetBytes(BinarySerializerPlus.NullValue);
            DataSerializerPlus serializer = typePool<DataSerializerPlus>.Pop() ?? new DataSerializerPlus();
            try
            {
                return serializer.codeSerialize<valueType>(value, config ?? defaultConfig);
            }
            finally { serializer.free(); }
        }
        /// <summary>
        /// 未知类型对象序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>序列化数据</returns>
        private static byte[] objectSerialize<valueType>(object value, ConfigPlus config)
        {
            DataSerializerPlus serializer = typePool<DataSerializerPlus>.Pop() ?? new DataSerializerPlus();
            try
            {
                return serializer.serialize<valueType>((valueType)value, config ?? defaultConfig);
            }
            finally { serializer.free(); }
        }
        /// <summary>
        /// 未知类型对象序列化
        /// </summary>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>序列化数据</returns>
        public static byte[] ObjectSerialize(object value, ConfigPlus config = null)
        {
            if (value == null) return BitConverter.GetBytes(BinarySerializerPlus.NullValue);
            Type type = value.GetType();
            Func<object, ConfigPlus, byte[]> serializer;
            if (!objectSerializes.TryGetValue(type, out serializer))
            {
                serializer = (Func<object, ConfigPlus, byte[]>)Delegate.CreateDelegate(typeof(Func<object, ConfigPlus, byte[]>), objectSerializeMethod.MakeGenericMethod(type));
                objectSerializes.Set(type, serializer);
            }
            return serializer(value, config);
        }
        /// <summary>
        /// 未知类型对象序列化
        /// </summary>
        private static interlocked.DictionaryPlus<Type, Func<object, ConfigPlus, byte[]>> objectSerializes = new interlocked.DictionaryPlus<Type,Func<object,ConfigPlus,byte[]>>(DictionaryPlus.CreateOnly<Type, Func<object, ConfigPlus, byte[]>>());
        /// <summary>
        /// 未知类型对象序列化
        /// </summary>
        private static readonly MethodInfo objectSerializeMethod = typeof(DataSerializerPlus).GetMethod("objectSerialize", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(object), typeof(ConfigPlus) }, null);
        /// <summary>
        /// 序列化数据流字段信息
        /// </summary>
        private static readonly FieldInfo serializeStreamField = typeof(DataSerializerPlus).GetField("Stream", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 基本类型转换函数
        /// </summary>
        private static readonly DictionaryPlus<Type, MethodInfo> serializeMethods;
        /// <summary>
        /// 获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getSerializeMethod(Type type)
        {
            MethodInfo method;
            if (serializeMethods.TryGetValue(type, out method))
            {
                serializeMethods.Remove(type);
                return method;
            }
            return null;
        }
        /// <summary>
        /// 基本类型转换函数
        /// </summary>
        private static readonly DictionaryPlus<Type, MethodInfo> memberSerializeMethods;
        /// <summary>
        /// 获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getMemberSerializeMethod(Type type)
        {
            MethodInfo method;
            return memberSerializeMethods.TryGetValue(type, out method) ? method : null;
        }
        /// <summary>
        /// 基本类型转换函数
        /// </summary>
        private static readonly DictionaryPlus<Type, MethodInfo> memberMapSerializeMethods;
        /// <summary>
        /// 获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getMemberMapSerializeMethod(Type type)
        {
            MethodInfo method;
            return memberMapSerializeMethods.TryGetValue(type, out method) ? method : null;
        }
        static DataSerializerPlus()
        {
            serializeMethods = DictionaryPlus.CreateOnly<Type, MethodInfo>();
            memberSerializeMethods = DictionaryPlus.CreateOnly<Type, MethodInfo>();
            memberMapSerializeMethods = DictionaryPlus.CreateOnly<Type, MethodInfo>();
            foreach (MethodInfo method in typeof(DataSerializerPlus).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                Type parameterType = null;
                if (method.customAttribute<serializeMethod>() != null)
                {
                    serializeMethods.Add(parameterType = method.GetParameters()[0].ParameterType, method);
                }
                if (method.customAttribute<memberSerializeMethod>() != null)
                {
                    if (parameterType == null) parameterType = method.GetParameters()[0].ParameterType;
                    memberSerializeMethods.Add(parameterType, method);
                }
                if (method.customAttribute<memberMapSerializeMethod>() != null)
                {
                    memberMapSerializeMethods.Add(parameterType ?? method.GetParameters()[0].ParameterType, method);
                }
            }
        }
    }
}
