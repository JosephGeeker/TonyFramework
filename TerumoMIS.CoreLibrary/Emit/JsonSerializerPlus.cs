//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: JsonSerializerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  JsonSerializerPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 14:53:49
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
    /// Json序列化
    /// </summary>
    public unsafe sealed class JsonSerializerPlus
    {
        /// <summary>
        /// 警告提示状态
        /// </summary>
        public enum warning : byte
        {
            /// <summary>
            /// 正常
            /// </summary>
            None,
            /// <summary>
            /// 缺少循环引用设置函数名称
            /// </summary>
            LessSetLoop,
            /// <summary>
            /// 缺少循环引用获取函数名称
            /// </summary>
            LessGetLoop,
            /// <summary>
            /// 成员位图类型不匹配
            /// </summary>
            MemberMap,
        }
        /// <summary>
        /// 配置参数
        /// </summary>
        public sealed class config
        {
            /// <summary>
            /// 循环引用设置函数名称
            /// </summary>
            public string SetLoopObject;
            /// <summary>
            /// 循环引用获取函数名称
            /// </summary>
            public string GetLoopObject;
            /// <summary>
            /// 循环引用检测深度,0表示实时检测
            /// </summary>
            public int CheckLoopDepth;
            /// <summary>
            /// 成员位图
            /// </summary>
            public memberMap MemberMap;
            /// <summary>
            /// 警告提示状态
            /// </summary>
            public warning Warning { get; internal set; }
            /// <summary>
            /// 最小时间是否输出为null
            /// </summary>
            public bool IsDateTimeMinNull = true;
            /// <summary>
            /// 时间是否转换成字符串
            /// </summary>
            public bool IsDateTimeToString;
            /// <summary>
            /// 是否将object转换成真实类型输出
            /// </summary>
            public bool IsObject;
            /// <summary>
            /// Dictionary[string,]是否转换成对象输出
            /// </summary>
            public bool IsStringDictionaryToObject = true;
            /// <summary>
            /// Dictionary是否转换成对象模式输出
            /// </summary>
            public bool IsDictionaryToObject;
            /// <summary>
            /// 是否输出客户端视图绑定类型
            /// </summary>
            public bool IsViewClientType;
            /// <summary>
            /// 超出最大有效精度的long/ulong是否转换成字符串
            /// </summary>
            public bool IsMaxNumberToString;
            /// <summary>
            /// 成员位图类型不匹配是否输出错误信息
            /// </summary>
            public bool IsMemberMapErrorLog = true;
            /// <summary>
            /// 成员位图类型不匹配时是否使用默认输出
            /// </summary>
            public bool IsMemberMapErrorToDefault = true;
        }
        /// <summary>
        /// 基本转换类型
        /// </summary>
        private sealed class toJsonMethod : Attribute { }
        /// <summary>
        /// 对象转换JSON字符串静态信息
        /// </summary>
        private static class typeToJsoner
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            public struct memberDynamicMethod
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
                /// 是否第一个字段
                /// </summary>
                private byte isFirstMember;
                /// <summary>
                /// 是否值类型
                /// </summary>
                private bool isValueType;
                /// <summary>
                /// 动态函数
                /// </summary>
                /// <param name="type"></param>
                /// <param name="name">成员类型</param>
                public memberDynamicMethod(Type type)
                {
                    dynamicMethod = new DynamicMethod("jsonSerializer", null, new Type[] { typeof(jsonSerializer), type }, type, true);
                    generator = dynamicMethod.GetILGenerator();
                    generator.DeclareLocal(typeof(charStream));

                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, jsonStreamField);
                    generator.Emit(OpCodes.Stloc_0);

                    isFirstMember = 1;
                    isValueType = type.IsValueType;
                }
                /// <summary>
                /// 添加成员
                /// </summary>
                /// <param name="name">成员名称</param>
                private void push(string name)
                {
                    if (isFirstMember == 0)
                    {
                        generator.Emit(OpCodes.Ldloc_0);
                        generator.Emit(OpCodes.Ldstr, "," + fastCSharp.web.ajax.QuoteString + name + fastCSharp.web.ajax.QuoteString + ":");
                        generator.Emit(OpCodes.Call, pub.CharStreamWriteNotNullMethod);
                    }
                    else
                    {
                        generator.Emit(OpCodes.Ldloc_0);
                        generator.Emit(OpCodes.Ldstr, fastCSharp.web.ajax.QuoteString + name + fastCSharp.web.ajax.QuoteString + ":");
                        generator.Emit(OpCodes.Call, pub.CharStreamWriteNotNullMethod);

                        isFirstMember = 0;
                    }
                    generator.Emit(OpCodes.Ldarg_0);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 1);
                    else generator.Emit(OpCodes.Ldarg_1);
                }
                /// <summary>
                /// 添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(fieldIndex field)
                {
                    push(field.Member.Name);
                    generator.Emit(OpCodes.Ldfld, field.Member);
                    MethodInfo method = getMemberMethodInfo(field.Member.FieldType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                }
                /// <summary>
                /// 添加属性
                /// </summary>
                /// <param name="property">属性信息</param>
                /// <param name="method">函数信息</param>
                public void Push(propertyIndex property, MethodInfo method)
                {
                    push(property.Member.Name);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                    method = getMemberMethodInfo(property.Member.PropertyType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                }
                /// <summary>
                /// 创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<delegateType>()
                {
                    generator.Emit(OpCodes.Ret);
                    return dynamicMethod.CreateDelegate(typeof(delegateType));
                }
            }
            /// <summary>
            /// 动态函数
            /// </summary>
            public struct memberMapDynamicMethod
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
                /// 是否值类型
                /// </summary>
                private bool isValueType;
                /// <summary>
                /// 动态函数
                /// </summary>
                /// <param name="type"></param>
                /// <param name="name">成员类型</param>
                public memberMapDynamicMethod(Type type)
                {
                    dynamicMethod = new DynamicMethod("jsonMemberMapSerializer", null, new Type[] { typeof(memberMap), typeof(jsonSerializer), type, typeof(charStream) }, type, true);
                    generator = dynamicMethod.GetILGenerator();

                    generator.DeclareLocal(typeof(int));
                    generator.Emit(OpCodes.Ldc_I4_0);
                    generator.Emit(OpCodes.Stloc_0);

                    isValueType = type.IsValueType;
                }
                /// <summary>
                /// 添加成员
                /// </summary>
                /// <param name="name">成员名称</param>
                private void push(string name, int memberIndex, Label end)
                {
                    Label next = generator.DefineLabel(), value = generator.DefineLabel();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, memberIndex);
                    generator.Emit(OpCodes.Callvirt, pub.MemberMapIsMemberMethod);
                    generator.Emit(OpCodes.Brfalse_S, end);

                    generator.Emit(OpCodes.Ldloc_0);
                    generator.Emit(OpCodes.Brtrue_S, next);

                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Stloc_0);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldstr, fastCSharp.web.ajax.QuoteString + name + fastCSharp.web.ajax.QuoteString + ":");
                    generator.Emit(OpCodes.Call, pub.CharStreamWriteNotNullMethod);
                    generator.Emit(OpCodes.Br_S, value);

                    generator.MarkLabel(next);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldstr, "," + fastCSharp.web.ajax.QuoteString + name + fastCSharp.web.ajax.QuoteString + ":");
                    generator.Emit(OpCodes.Call, pub.CharStreamWriteNotNullMethod);

                    generator.MarkLabel(value);
                    generator.Emit(OpCodes.Ldarg_1);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 2);
                    else generator.Emit(OpCodes.Ldarg_2);
                }
                /// <summary>
                /// 添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(fieldIndex field)
                {
                    Label end = generator.DefineLabel();
                    push(field.Member.Name, field.MemberIndex, end);
                    generator.Emit(OpCodes.Ldfld, field.Member);
                    MethodInfo method = getMemberMethodInfo(field.Member.FieldType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                    generator.MarkLabel(end);
                }
                /// <summary>
                /// 添加属性
                /// </summary>
                /// <param name="property">属性信息</param>
                /// <param name="method">函数信息</param>
                public void Push(propertyIndex property, MethodInfo method)
                {
                    Label end = generator.DefineLabel();
                    push(property.Member.Name, property.MemberIndex, end);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                    method = getMemberMethodInfo(property.Member.PropertyType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                    generator.MarkLabel(end);
                }
                /// <summary>
                /// 创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<delegateType>()
                {
                    generator.Emit(OpCodes.Ret);
                    return dynamicMethod.CreateDelegate(typeof(delegateType));
                }
            }
            /// <summary>
            /// 获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <param name="typeAttribute">类型配置</param>
            /// <returns>字段成员集合</returns>
            public static subArray<fieldIndex> GetFields(fieldIndex[] fields, jsonSerialize typeAttribute)
            {
                subArray<fieldIndex> values = new subArray<fieldIndex>(fields.Length);
                foreach (fieldIndex field in fields)
                {
                    Type type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        jsonSerialize.member attribute = field.GetAttribute<jsonSerialize.member>(true, true);
                        if (typeAttribute.IsAllMember ? (attribute == null || attribute.IsSetup) : (attribute != null && attribute.IsSetup)) values.Add(field);
                    }
                }
                return values;
            }
            /// <summary>
            /// 获取属性成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <param name="typeAttribute">类型配置</param>
            /// <param name="fields">字段成员集合</param>
            /// <returns>属性成员集合</returns>
            public static subArray<keyValue<propertyIndex, MethodInfo>> GetProperties(propertyIndex[] properties, jsonSerialize typeAttribute)
            {
                subArray<keyValue<propertyIndex, MethodInfo>> values = new subArray<keyValue<propertyIndex, MethodInfo>>(properties.Length);
                foreach (propertyIndex property in properties)
                {
                    if (property.Member.CanRead)
                    {
                        Type type = property.Member.PropertyType;
                        if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !property.IsIgnore)
                        {
                            jsonSerialize.member attribute = property.GetAttribute<jsonSerialize.member>(true, true);
                            if (typeAttribute.IsAllMember ? (attribute == null || attribute.IsSetup) : (attribute != null && attribute.IsSetup))
                            {
                                MethodInfo method = property.Member.GetGetMethod(true);
                                if (method != null && method.GetParameters().Length == 0) values.Add(new keyValue<propertyIndex, MethodInfo>(property, method));
                            }
                        }
                    }
                }
                return values;
            }
            /// <summary>
            /// 获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <returns>字段成员集合</returns>
            public static subArray<memberIndex> GetMembers(fieldIndex[] fieldIndexs, propertyIndex[] properties, jsonSerialize typeAttribute)
            {
                subArray<memberIndex> members = new subArray<memberIndex>(fieldIndexs.Length + properties.Length);
                foreach (fieldIndex field in fieldIndexs)
                {
                    Type type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        jsonSerialize.member attribute = field.GetAttribute<jsonSerialize.member>(true, true);
                        if (typeAttribute.IsAllMember ? (attribute == null || attribute.IsSetup) : (attribute != null && attribute.IsSetup)) members.Add(field);
                    }
                }
                foreach (propertyIndex property in properties)
                {
                    if (property.Member.CanRead && property.Member.CanWrite)
                    {
                        Type type = property.Member.PropertyType;
                        if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !property.IsIgnore)
                        {
                            jsonSerialize.member attribute = property.GetAttribute<jsonSerialize.member>(true, true);
                            if (typeAttribute.IsAllMember ? (attribute == null || attribute.IsSetup) : (attribute != null && attribute.IsSetup))
                            {
                                MethodInfo method = property.Member.GetGetMethod(true);
                                if (method != null && method.GetParameters().Length == 0) members.Add(property);
                            }
                        }
                    }
                }
                return members;
            }
            /// <summary>
            /// 获取成员转换函数信息
            /// </summary>
            /// <param name="type">成员类型</param>
            /// <returns>成员转换函数信息</returns>
            private static MethodInfo getMemberMethodInfo(Type type)
            {
                MethodInfo methodInfo = jsonSerializer.getToJsonMethod(type);
                if (methodInfo != null) return methodInfo;
                if (type.IsArray) return GetArrayToJsoner(type.GetElementType());
                if (type.IsEnum) return GetEnumToJsoner(type);
                if (type.IsGenericType)
                {
                    Type genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(Dictionary<,>)) return GetDictionaryToJsoner(type);
                    if (genericType == typeof(Nullable<>)) return nullableToJsonMethod.MakeGenericMethod(type.GetGenericArguments());
                    if (genericType == typeof(KeyValuePair<,>)) return keyValuePairToJsonMethod.MakeGenericMethod(type.GetGenericArguments());
                }
                if ((methodInfo = GetCustomToJsoner(type)) != null) return methodInfo;
                if ((methodInfo = GetIEnumerableToJsoner(type)) != null) return methodInfo;
                return GetTypeToJsoner(type);
            }

            /// <summary>
            /// object转换调用委托信息集合
            /// </summary>
            private static interlocked.dictionary<Type, Action<jsonSerializer, object>> objectToJsoners = new interlocked.dictionary<Type,Action<jsonSerializer,object>>(fastCSharp.dictionary.CreateOnly<Type, Action<jsonSerializer, object>>());
            /// <summary>
            /// 获取object转换调用委托信息
            /// </summary>
            /// <param name="type">真实类型</param>
            /// <returns>object转换调用委托信息</returns>
            public static Action<jsonSerializer, object> GetObjectToJsoner(Type type)
            {
                Action<jsonSerializer, object> method;
                if (objectToJsoners.TryGetValue(type, out method)) return method;
                method = (Action<jsonSerializer, object>)Delegate.CreateDelegate(typeof(Action<jsonSerializer, object>), toJsonObjectMethod.MakeGenericMethod(type));
                objectToJsoners.Set(type, method);
                return method;
            }
            /// <summary>
            /// 数组转换调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> arrayToJsoners = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取数组转换委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>数组转换委托调用函数信息</returns>
            public static MethodInfo GetArrayToJsoner(Type type)
            {
                MethodInfo method;
                if (arrayToJsoners.TryGetValue(type, out method)) return method;
                arrayToJsoners.Set(type, method = arrayMethod.MakeGenericMethod(type));
                return method;
            }
            /// <summary>
            /// 枚举集合转换调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> enumerableToJsoners = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取枚举集合转换委托调用函数信息
            /// </summary>
            /// <param name="type">枚举类型</param>
            /// <returns>枚举集合转换委托调用函数信息</returns>
            public static MethodInfo GetIEnumerableToJsoner(Type type)
            {
                MethodInfo method;
                if (enumerableToJsoners.TryGetValue(type, out method)) return method;
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType)
                    {
                        Type genericType = interfaceType.GetGenericTypeDefinition();
                        if (genericType == typeof(IEnumerable<>))
                        {
                            Type[] parameters = interfaceType.GetGenericArguments();
                            Type argumentType = parameters[0];
                            parameters[0] = typeof(IList<>).MakeGenericType(argumentType);
                            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                method = (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                method = (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                method = (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = argumentType.MakeArrayType();
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                method = (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(type, argumentType);
                                break;
                            }
                        }
                        else if (genericType == typeof(IDictionary<,>))
                        {
                            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { interfaceType }, null);
                            if (constructorInfo != null)
                            {
                                method = (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(type, typeof(KeyValuePair<,>).MakeGenericType(interfaceType.GetGenericArguments()));
                                break;
                            }
                        }
                    }
                }
                enumerableToJsoners.Set(type, method);
                return method;
            }
            /// <summary>
            /// 字典转换调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> dictionaryToJsoners = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取字典转换委托调用函数信息
            /// </summary>
            /// <param name="type">枚举类型</param>
            /// <returns>字典转换委托调用函数信息</returns>
            public static MethodInfo GetDictionaryToJsoner(Type type)
            {
                MethodInfo method;
                if (dictionaryToJsoners.TryGetValue(type, out method)) return method;
                Type[] types = type.GetGenericArguments();
                if (types[0] == typeof(string)) method = stringDictionaryMethod.MakeGenericMethod(types[1]);
                else method = dictionaryMethod.MakeGenericMethod(types);
                dictionaryToJsoners.Set(type, method);
                return method;
            }
            /// <summary>
            /// 枚举转换调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> enumToJsoners = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取枚举转换委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>枚举转换委托调用函数信息</returns>
            public static MethodInfo GetEnumToJsoner(Type type)
            {
                MethodInfo method;
                if (enumToJsoners.TryGetValue(type, out method)) return method;
                enumToJsoners.Set(type, method = enumToStringMethod.MakeGenericMethod(type));
                return method;
            }
            /// <summary>
            /// 未知类型转换调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> typeToJsoners = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 未知类型枚举转换委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>未知类型转换委托调用函数信息</returns>
            public static MethodInfo GetTypeToJsoner(Type type)
            {
                MethodInfo method;
                if (typeToJsoners.TryGetValue(type, out method)) return method;
                if (type.IsValueType)
                {
                    Type nullType = type.nullableType();
                    if (nullType == null) method = memberToJsonMethod.MakeGenericMethod(type);
                    else method = nullableMemberToJsonMethod.MakeGenericMethod(nullType);
                }
                else method = classToJsonMethod.MakeGenericMethod(type);
                typeToJsoners.Set(type, method);
                return method;
            }
            /// <summary>
            /// 自定义转换调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> customToJsoners = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 自定义枚举转换委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>自定义转换委托调用函数信息</returns>
            public static MethodInfo GetCustomToJsoner(Type type)
            {
                MethodInfo method;
                if (customToJsoners.TryGetValue(type, out method)) return method;
                foreach (fastCSharp.code.attributeMethod methodInfo in fastCSharp.code.attributeMethod.GetStatic(type))
                {
                    if (methodInfo.Method.ReturnType == typeof(void))
                    {
                        ParameterInfo[] parameters = methodInfo.Method.GetParameters();
                        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(jsonSerializer) && parameters[1].ParameterType == type)
                        {
                            if (methodInfo.GetAttribute<jsonSerialize.custom>(true) != null)
                            {
                                method = methodInfo.Method;
                                break;
                            }
                        }
                    }
                }
                customToJsoners.Set(type, method);
                return method;
            }
        }
        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        internal static class typeToJsoner<valueType>
        {
            /// <summary>
            /// 成员转换
            /// </summary>
            private static readonly Action<jsonSerializer, valueType> memberToJsoner;
            /// <summary>
            /// 成员转换
            /// </summary>
            private static readonly Action<memberMap, jsonSerializer, valueType, charStream> memberMapToJsoner;
            /// <summary>
            /// 转换委托
            /// </summary>
            private static readonly Action<jsonSerializer, valueType> defaultToJsoner;
            /// <summary>
            /// JSON序列化类型配置
            /// </summary>
            private static readonly jsonSerialize attribute;
            /// <summary>
            /// 客户端视图类型名称
            /// </summary>
            private static readonly string viewClientTypeName;
            /// <summary>
            /// 是否值类型
            /// </summary>
            private static readonly bool isValueType;
            /// <summary>
            /// 对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void ToJson(jsonSerializer toJsoner, valueType value)
            {
                if (isValueType) StructToJson(toJsoner, value);
                else toJson(toJsoner, value);
            }
            /// <summary>
            /// 对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void StructToJson(jsonSerializer toJsoner, valueType value)
            {
                if (defaultToJsoner == null) MemberToJson(toJsoner, value);
                else defaultToJsoner(toJsoner, value);
            }
            /// <summary>
            /// 引用类型对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void ClassToJson(jsonSerializer toJsoner, valueType value)
            {
                if (defaultToJsoner == null)
                {
                    if (toJsoner.push(value))
                    {
                        MemberToJson(toJsoner, value);
                        toJsoner.pop();
                    }
                }
                else defaultToJsoner(toJsoner, value);
            }
            /// <summary>
            /// 引用类型对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void toJson(jsonSerializer toJsoner, valueType value)
            {
                if (value == null) fastCSharp.web.ajax.WriteNull(toJsoner.JsonStream);
                else ClassToJson(toJsoner, value);
            }
            /// <summary>
            /// 值类型对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void MemberToJson(jsonSerializer toJsoner, valueType value)
            {
                charStream jsonStream = toJsoner.JsonStream;
                config config = toJsoner.toJsonConfig;
                byte isView;
                if (viewClientTypeName != null && config.IsViewClientType)
                {
                    jsonStream.WriteNotNull(viewClientTypeName);
                    isView = 1;
                }
                else
                {
                    jsonStream.PrepLength(2);
                    jsonStream.Unsafer.Write('{');
                    isView = 0;
                }
                memberMap memberMap = config.MemberMap;
                if (memberMap == null) memberToJsoner(toJsoner, value);
                else if (memberMap.Type == memberMap<valueType>.Type)
                {
                    config.MemberMap = null;
                    try
                    {
                        memberMapToJsoner(memberMap, toJsoner, value, toJsoner.JsonStream);
                    }
                    finally { config.MemberMap = memberMap; }
                }
                else
                {
                    config.Warning = warning.MemberMap;
                    if (config.IsMemberMapErrorLog) log.Error.Add("Json序列化成员位图类型匹配失败", true, true);
                    if (config.IsMemberMapErrorToDefault) memberToJsoner(toJsoner, value);
                }
                if (isView == 0) jsonStream.Write('}');
                else
                {
                    jsonStream.PrepLength(2);
                    jsonStream.Unsafer.Write('}');
                    jsonStream.Unsafer.Write(')');
                }
            }
            /// <summary>
            /// 数组转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="array">数组对象</param>
            internal static void Array(jsonSerializer toJsoner, valueType[] array)
            {
                charStream jsonStream = toJsoner.JsonStream;
                jsonStream.Write('[');
                byte isFirst = 1;
                if (isValueType)
                {
                    foreach (valueType value in array)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        StructToJson(toJsoner, value);
                        isFirst = 0;
                    }
                }
                else
                {
                    foreach (valueType value in array)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        toJson(toJsoner, value);
                        isFirst = 0;
                    }
                }
                jsonStream.Write(']');
            }
            /// <summary>
            /// 枚举集合转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="values">枚举集合</param>
            internal static void Enumerable(jsonSerializer toJsoner, IEnumerable<valueType> values)
            {
                charStream jsonStream = toJsoner.JsonStream;
                jsonStream.Write('[');
                byte isFirst = 1;
                if (isValueType)
                {
                    foreach (valueType value in values)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        StructToJson(toJsoner, value);
                        isFirst = 0;
                    }
                }
                else
                {
                    foreach (valueType value in values)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        toJson(toJsoner, value);
                        isFirst = 0;
                    }
                }
                jsonStream.Write(']');
            }
            /// <summary>
            /// 字典转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            internal static void Dictionary<dictionaryValueType>(jsonSerializer toJsoner, Dictionary<valueType, dictionaryValueType> dictionary)
            {
                charStream jsonStream = toJsoner.JsonStream;
                byte isFirst = 1;
                if (toJsoner.toJsonConfig.IsDictionaryToObject)
                {
                    jsonStream.Write('{');
                    foreach (KeyValuePair<valueType, dictionaryValueType> value in dictionary)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        typeToJsoner<valueType>.ToJson(toJsoner, value.Key);
                        jsonStream.Write(':');
                        typeToJsoner<dictionaryValueType>.ToJson(toJsoner, value.Value);
                        isFirst = 0;
                    }
                    jsonStream.Write('}');
                }
                else
                {
                    jsonStream.Write('[');
                    foreach (KeyValuePair<valueType, dictionaryValueType> value in dictionary)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        KeyValuePair(toJsoner, value);
                        isFirst = 0;
                    }
                    jsonStream.Write(']');
                }
            }
            /// <summary>
            /// 字典转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="dictionary">字典</param>
            internal static void StringDictionary(jsonSerializer toJsoner, Dictionary<string, valueType> dictionary)
            {
                charStream jsonStream = toJsoner.JsonStream;
                jsonStream.Write('{');
                byte isFirst = 1;
                if (isValueType)
                {
                    foreach (KeyValuePair<string, valueType> value in dictionary)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        fastCSharp.web.ajax.ToString(value.Key, jsonStream);
                        jsonStream.Write(':');
                        StructToJson(toJsoner, value.Value);
                        isFirst = 0;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, valueType> value in dictionary)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        fastCSharp.web.ajax.ToString(value.Key, jsonStream);
                        jsonStream.Write(':');
                        toJson(toJsoner, value.Value);
                        isFirst = 0;
                    }
                }
                jsonStream.Write('}');
            }
            /// <summary>
            /// 字典转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            internal static void KeyValuePair<dictionaryValueType>(jsonSerializer toJsoner, KeyValuePair<valueType, dictionaryValueType> value)
            {
                charStream jsonStream = toJsoner.JsonStream;
                jsonStream.PrepLength(21);
                byte* data = (byte*)jsonStream.CurrentChar;
                *(char*)data = '{';
                *(char*)(data + sizeof(char)) = fastCSharp.web.ajax.Quote;
                *(char*)(data + sizeof(char) * 2) = 'K';
                *(char*)(data + sizeof(char) * 3) = 'e';
                *(char*)(data + sizeof(char) * 4) = 'y';
                *(char*)(data + sizeof(char) * 5) = fastCSharp.web.ajax.Quote;
                *(char*)(data + sizeof(char) * 6) = ':';
                jsonStream.Unsafer.AddLength(7);
                typeToJsoner<valueType>.ToJson(toJsoner, value.Key);
                jsonStream.PrepLength(12);
                data = (byte*)jsonStream.CurrentChar;
                *(char*)data = ',';
                *(char*)(data + sizeof(char)) = fastCSharp.web.ajax.Quote;
                *(char*)(data + sizeof(char) * 2) = 'V';
                *(char*)(data + sizeof(char) * 3) = 'a';
                *(char*)(data + sizeof(char) * 4) = 'l';
                *(char*)(data + sizeof(char) * 5) = 'u';
                *(char*)(data + sizeof(char) * 6) = 'e';
                *(char*)(data + sizeof(char) * 7) = fastCSharp.web.ajax.Quote;
                *(char*)(data + sizeof(char) * 8) = ':';
                jsonStream.Unsafer.AddLength(9);
                typeToJsoner<dictionaryValueType>.ToJson(toJsoner, value.Value);
                jsonStream.Write('}');
            }
            /// <summary>
            /// 不支持多维数组
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void arrayManyRank(jsonSerializer toJsoner, valueType value)
            {
                fastCSharp.web.ajax.WriteArray(toJsoner.JsonStream);
            }
            /// <summary>
            /// 不支持对象转换null
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void toNull(jsonSerializer toJsoner, valueType value)
            {
                fastCSharp.web.ajax.WriteNull(toJsoner.JsonStream);
            }
            /// <summary>
            /// 枚举转换字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void enumToString(jsonSerializer toJsoner, valueType value)
            {
                jsonSerializer.enumToString(toJsoner, value);
            }
            /// <summary>
            /// 获取字段成员集合
            /// </summary>
            /// <returns>字段成员集合</returns>
            public static subArray<memberIndex> GetMembers()
            {
                if (memberToJsoner == null) return default(subArray<memberIndex>);
                return typeToJsoner.GetMembers(memberIndexGroup<valueType>.GetFields(attribute.MemberFilter), memberIndexGroup<valueType>.GetProperties(attribute.MemberFilter), attribute);
            }
            static typeToJsoner()
            {
                Type type = typeof(valueType);
                MethodInfo methodInfo = jsonSerializer.getToJsonMethod(type);
                if (methodInfo != null)
                {
                    DynamicMethod dynamicMethod = new DynamicMethod("toJsoner", typeof(void), new Type[] { typeof(jsonSerializer), type }, true);
                    dynamicMethod.InitLocals = true;
                    ILGenerator generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    defaultToJsoner = (Action<jsonSerializer, valueType>)dynamicMethod.CreateDelegate(typeof(Action<jsonSerializer, valueType>));
                    isValueType = true;
                    return;
                }
                if (type.IsArray)
                {
                    if (type.GetArrayRank() == 1) defaultToJsoner = (Action<jsonSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<jsonSerializer, valueType>), typeToJsoner.GetArrayToJsoner(type.GetElementType()));
                    else defaultToJsoner = arrayManyRank;
                    isValueType = true;
                    return;
                }
                if (type.IsEnum)
                {
                    defaultToJsoner = enumToString;
                    isValueType = true;
                    return;
                }
                if (type.IsPointer)
                {
                    defaultToJsoner = toNull;
                    isValueType = true;
                    return;
                }
                if (type.IsGenericType)
                {
                    Type genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(Dictionary<,>))
                    {
                        defaultToJsoner = (Action<jsonSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<jsonSerializer, valueType>), typeToJsoner.GetDictionaryToJsoner(type));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(Nullable<>))
                    {
                        defaultToJsoner = (Action<jsonSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<jsonSerializer, valueType>), nullableToJsonMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        defaultToJsoner = (Action<jsonSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<jsonSerializer, valueType>), keyValuePairToJsonMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                }
                if ((methodInfo = typeToJsoner.GetCustomToJsoner(type)) != null
                    || (methodInfo = typeToJsoner.GetIEnumerableToJsoner(type)) != null)
                {
                    defaultToJsoner = (Action<jsonSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<jsonSerializer, valueType>), methodInfo);
                    isValueType = true;
                }
                else
                {
                    Type attributeType;
                    attribute = type.customAttribute<jsonSerialize>(out attributeType, true) ?? jsonSerialize.AllMember;
                    if (type.IsValueType) isValueType = true;
                    else if (attribute != jsonSerialize.AllMember && attributeType != type)
                    {
                        for (Type baseType = type.BaseType; baseType != typeof(object); baseType = baseType.BaseType)
                        {
                            jsonSerialize baseAttribute = fastCSharp.code.typeAttribute.GetAttribute<jsonSerialize>(baseType, false, true);
                            if (baseAttribute != null)
                            {
                                if (baseAttribute.IsBaseType)
                                {
                                    methodInfo = baseToJsonMethod.MakeGenericMethod(baseType, type);
                                    defaultToJsoner = (Action<jsonSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<jsonSerializer, valueType>), methodInfo);
                                    return;
                                }
                                break;
                            }
                        }
                    }
                    fastCSharp.code.cSharp.webView.clientType clientType = fastCSharp.code.typeAttribute.GetAttribute<fastCSharp.code.cSharp.webView.clientType>(type, true, true);
                    if (clientType != null)
                    {
                        if (clientType.MemberName == null) viewClientTypeName = "new " + clientType.Name + "({";
                        else viewClientTypeName = clientType.Name + ".Get({";
                    }
                    typeToJsoner.memberDynamicMethod dynamicMethod = new typeToJsoner.memberDynamicMethod(type);
                    typeToJsoner.memberMapDynamicMethod memberMapDynamicMethod = new typeToJsoner.memberMapDynamicMethod(type);
                    subArray<fieldIndex> fields = typeToJsoner.GetFields(memberIndexGroup<valueType>.GetFields(attribute.MemberFilter), attribute);
                    foreach (fieldIndex member in fields)
                    {
                        dynamicMethod.Push(member);
                        memberMapDynamicMethod.Push(member);
                    }
                    subArray<keyValue<propertyIndex, MethodInfo>> properties = typeToJsoner.GetProperties(memberIndexGroup<valueType>.GetProperties(attribute.MemberFilter), attribute);
                    foreach (keyValue<propertyIndex, MethodInfo> member in properties)
                    {
                        dynamicMethod.Push(member.Key, member.Value);
                        memberMapDynamicMethod.Push(member.Key, member.Value);
                    }
                    memberToJsoner = (Action<jsonSerializer, valueType>)dynamicMethod.Create<Action<jsonSerializer, valueType>>();
                    memberMapToJsoner = (Action<memberMap, jsonSerializer, valueType, charStream>)memberMapDynamicMethod.Create<Action<memberMap, jsonSerializer, valueType, charStream>>();
                }
            }
        }
        /// <summary>
        /// 配置参数
        /// </summary>
        private config toJsonConfig;
        /// <summary>
        /// Json字符串输出缓冲区
        /// </summary>
        internal readonly charStream JsonStream = new charStream((char*)fastCSharp.emit.pub.PuzzleValue, 1);
        /// <summary>
        /// 对象编号
        /// </summary>
        private Dictionary<objectReference, string> objectIndexs;
        /// <summary>
        /// 祖先节点集合
        /// </summary>
        private object[] forefather;
        /// <summary>
        /// 祖先节点数量
        /// </summary>
        private int forefatherCount;
        /// <summary>
        /// 循环检测深度
        /// </summary>
        private int checkLoopDepth;
        /// <summary>
        /// 是否调用循环引用处理函数
        /// </summary>
        private bool isLoopObject;
        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>Json字符串</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string toJson<valueType>(valueType value, config config)
        {
            toJsonConfig = config;
            pointer buffer = fastCSharp.unmanagedPool.StreamBuffers.Get();
            try
            {
                JsonStream.Reset((byte*)buffer.Char, fastCSharp.unmanagedPool.StreamBuffers.Size);
                using (JsonStream)
                {
                    toJson(value);
                    return fastCSharp.web.ajax.FormatJavascript(JsonStream);
                }
            }
            finally
            {
                fastCSharp.unmanagedPool.StreamBuffers.Push(ref buffer);
            }
        }
        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="jsonStream">Json输出缓冲区</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson<valueType>(valueType value, charStream jsonStream, config config)
        {
            toJsonConfig = config;
            JsonStream.From(jsonStream);
            try
            {
                toJson(value);
            }
            finally { jsonStream.From(JsonStream); }
        }
        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson<valueType>(valueType value)
        {
            if (toJsonConfig.GetLoopObject == null || toJsonConfig.SetLoopObject == null)
            {
                if (toJsonConfig.GetLoopObject != null) toJsonConfig.Warning = warning.LessSetLoop;
                else if (toJsonConfig.SetLoopObject != null) toJsonConfig.Warning = warning.LessGetLoop;
                else toJsonConfig.Warning = warning.None;
                isLoopObject = false;
                if (toJsonConfig.CheckLoopDepth <= 0)
                {
                    checkLoopDepth = 0;
                    if (forefather == null) forefather = new object[sizeof(int)];
                }
                else checkLoopDepth = toJsonConfig.CheckLoopDepth;
            }
            else
            {
                isLoopObject = true;
                if (objectIndexs == null) objectIndexs = dictionary<objectReference>.Create<string>();
                checkLoopDepth = toJsonConfig.CheckLoopDepth <= 0 ? fastCSharp.config.appSetting.JsonDepth : toJsonConfig.CheckLoopDepth;
            }
            typeToJsoner<valueType>.ToJson(this, value);
        }
        /// <summary>
        /// 进入对象节点
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <returns>是否继续处理对象</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool push<valueType>(valueType value)
        {
            if (checkLoopDepth == 0)
            {
                if (forefatherCount != 0)
                {
                    int count = forefatherCount;
                    object objectValue = value;
                    foreach (object arrayValue in forefather)
                    {
                        if (arrayValue == objectValue)
                        {
                            fastCSharp.web.ajax.WriteObject(JsonStream);
                            return false;
                        }
                        if (--count == 0) break;
                    }
                }
                if (forefatherCount == forefather.Length)
                {
                    object[] newValues = new object[forefatherCount << 1];
                    forefather.CopyTo(newValues, 0);
                    forefather = newValues;
                }
                forefather[forefatherCount++] = value;
            }
            else
            {
                if (--checkLoopDepth == 0) fastCSharp.log.Default.Throw(log.exceptionType.IndexOutOfRange);
                if (isLoopObject)
                {
                    string index;
                    if (objectIndexs.TryGetValue(new objectReference { Value = value }, out index))
                    {
                        JsonStream.PrepLength(toJsonConfig.GetLoopObject.Length + index.Length + 2);
                        JsonStream.WriteNotNull(toJsonConfig.GetLoopObject);
                        JsonStream.Unsafer.Write('(');
                        JsonStream.WriteNotNull(index);
                        JsonStream.Unsafer.Write(')');
                        return false;
                    }
                    objectIndexs.Add(new objectReference { Value = value }, index = objectIndexs.Count.toString());
                    JsonStream.PrepLength(toJsonConfig.SetLoopObject.Length + index.Length + 4);
                    JsonStream.WriteNotNull(toJsonConfig.SetLoopObject);
                    JsonStream.Unsafer.Write('(');
                    JsonStream.WriteNotNull(index);
                    JsonStream.Unsafer.Write(',');
                }
            }
            return true;
        }
        /// <summary>
        /// 进入对象节点
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <returns>是否继续处理对象</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool pushArray<valueType>(valueType value)
        {
            if (checkLoopDepth == 0)
            {
                if (forefatherCount != 0)
                {
                    int count = forefatherCount;
                    object objectValue = value;
                    foreach (object arrayValue in forefather)
                    {
                        if (arrayValue == objectValue)
                        {
                            fastCSharp.web.ajax.WriteObject(JsonStream);
                            return false;
                        }
                        if (--count == 0) break;
                    }
                }
                if (forefatherCount == forefather.Length)
                {
                    object[] newValues = new object[forefatherCount << 1];
                    forefather.CopyTo(newValues, 0);
                    forefather = newValues;
                }
                forefather[forefatherCount++] = value;
            }
            else
            {
                if (--checkLoopDepth == 0) fastCSharp.log.Default.Throw(log.exceptionType.IndexOutOfRange);
                if (isLoopObject)
                {
                    string index;
                    if (objectIndexs.TryGetValue(new objectReference { Value = value }, out index))
                    {
                        JsonStream.PrepLength(toJsonConfig.GetLoopObject.Length + index.Length + 5);
                        JsonStream.WriteNotNull(toJsonConfig.GetLoopObject);
                        JsonStream.Unsafer.Write('(');
                        JsonStream.WriteNotNull(index);
                        JsonStream.WriteNotNull(",[])");
                        return false;
                    }
                    objectIndexs.Add(new objectReference { Value = value }, index = objectIndexs.Count.toString());
                    JsonStream.PrepLength(toJsonConfig.SetLoopObject.Length + index.Length + 4);
                    JsonStream.WriteNotNull(toJsonConfig.SetLoopObject);
                    JsonStream.Unsafer.Write('(');
                    JsonStream.WriteNotNull(index);
                    JsonStream.Unsafer.Write(',');
                }
            }
            return true;
        }
        /// <summary>
        /// 退出对象节点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void pop()
        {
            if (checkLoopDepth == 0) forefather[--forefatherCount] = null;
            else
            {
                ++checkLoopDepth;
                if (isLoopObject) JsonStream.Write(')');
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        private void free()
        {
            toJsonConfig = null;
            if (objectIndexs != null) objectIndexs.Clear();
            if (forefatherCount != 0)
            {
                Array.Clear(forefather, 0, forefatherCount);
                forefatherCount = 0;
            }
            typePool<jsonSerializer>.Push(this);
        }
        /// <summary>
        /// 逻辑值转换
        /// </summary>
        /// <param name="value">逻辑值</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(bool value)
        {
            if (value)
            {
                JsonStream.PrepLength(4);
                byte* data = (byte*)JsonStream.CurrentChar;
                *(char*)data = 't';
                *(char*)(data + sizeof(char)) = 'r';
                *(char*)(data + sizeof(char) * 2) = 'u';
                *(char*)(data + sizeof(char) * 3) = 'e';
                JsonStream.Unsafer.AddLength(4);
            }
            else
            {
                JsonStream.PrepLength(5);
                byte* data = (byte*)JsonStream.CurrentChar;
                *(char*)data = 'f';
                *(char*)(data + sizeof(char)) = 'a';
                *(char*)(data + sizeof(char) * 2) = 'l';
                *(char*)(data + sizeof(char) * 3) = 's';
                *(char*)(data + sizeof(char) * 4) = 'e';
                JsonStream.Unsafer.AddLength(5);
            }
        }
        /// <summary>
        /// 逻辑值转换
        /// </summary>
        /// <param name="value">逻辑值</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(bool? value)
        {
            if (value.HasValue) toJson((bool)value);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(byte value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(byte? value)
        {
            if (value.HasValue) fastCSharp.web.ajax.ToString((byte)value, JsonStream);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(sbyte value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(sbyte? value)
        {
            if (value.HasValue) fastCSharp.web.ajax.ToString((sbyte)value, JsonStream);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(short value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(short? value)
        {
            if (value.HasValue) fastCSharp.web.ajax.ToString((short)value, JsonStream);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(ushort value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(ushort? value)
        {
            if (value.HasValue) fastCSharp.web.ajax.ToString((ushort)value, JsonStream);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(int value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(int? value)
        {
            if (value.HasValue) fastCSharp.web.ajax.ToString((int)value, JsonStream);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(uint value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(uint? value)
        {
            if (value.HasValue) fastCSharp.web.ajax.ToString((uint)value, JsonStream);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(long value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream, toJsonConfig.IsMaxNumberToString);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(long? value)
        {
            if (value.HasValue) fastCSharp.web.ajax.ToString((long)value, JsonStream, toJsonConfig.IsMaxNumberToString);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(ulong value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream, toJsonConfig.IsMaxNumberToString);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(ulong? value)
        {
            if (value.HasValue) fastCSharp.web.ajax.ToString((ulong)value, JsonStream, toJsonConfig.IsMaxNumberToString);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(float value)
        {
            if (float.IsNaN(value)) fastCSharp.web.ajax.WriteNaN(JsonStream);
            else JsonStream.WriteNotNull(value.ToString());
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(float? value)
        {
            if (value.HasValue) toJson(value.Value);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(double value)
        {
            if (double.IsNaN(value)) fastCSharp.web.ajax.WriteNaN(JsonStream);
            else JsonStream.WriteNotNull(value.ToString());
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(double? value)
        {
            if (value.HasValue) toJson(value.Value);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(decimal value)
        {
            JsonStream.WriteNotNull(value.ToString());
        }
        /// <summary>
        /// 数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(decimal? value)
        {
            if (value.HasValue) JsonStream.WriteNotNull(((decimal)value).ToString());
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 字符转换
        /// </summary>
        /// <param name="value">字符</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(char value)
        {
            JsonStream.PrepLength(3);
            byte* data = (byte*)JsonStream.CurrentChar;
            *(char*)data = fastCSharp.web.ajax.Quote;
            *(char*)(data + sizeof(char)) = value == fastCSharp.web.ajax.Quote ? ' ' : value;
            *(char*)(data + sizeof(char) * 2) = fastCSharp.web.ajax.Quote;
            JsonStream.Unsafer.AddLength(3);
        }
        /// <summary>
        /// 字符转换
        /// </summary>
        /// <param name="value">字符</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(char? value)
        {
            if (value.HasValue) toJson((char)value);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 时间转换
        /// </summary>
        /// <param name="value">时间</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(DateTime value)
        {
            if (toJsonConfig.IsDateTimeMinNull) fastCSharp.web.ajax.ToString(value, JsonStream);
            else if (toJsonConfig.IsDateTimeToString)
            {
                JsonStream.PrepLength(fastCSharp.date.SqlMillisecondSize + 2);
                JsonStream.Unsafer.Write('"');
                fastCSharp.date.ToSqlMillisecond((DateTime)value, JsonStream);
                JsonStream.Unsafer.Write('"');
            }
            else fastCSharp.web.ajax.ToStringNotNull(value, JsonStream);
        }
        /// <summary>
        /// 时间转换
        /// </summary>
        /// <param name="value">时间</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(DateTime? value)
        {
            if (value.HasValue) toJson((DateTime)value);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// Guid转换
        /// </summary>
        /// <param name="value">Guid</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(Guid value)
        {
            fastCSharp.web.ajax.ToString(value, JsonStream);
        }
        /// <summary>
        /// Guid转换
        /// </summary>
        /// <param name="value">Guid</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(Guid? value)
        {
            if (value.HasValue) toJson((Guid)value);
            else fastCSharp.web.ajax.WriteNull(JsonStream);
        }
        /// <summary>
        /// 字符串转换
        /// </summary>
        /// <param name="value">字符串</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(string value)
        {
            if (value == null) fastCSharp.web.ajax.WriteNull(JsonStream);
            else
            {
                fixed (char* valueFixed = value) toJson(valueFixed, value.Length);
            }
        }
        /// <summary>
        /// 字符串转换
        /// </summary>
        /// <param name="value">字符串</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(subString value)
        {
            if (value.value == null) fastCSharp.web.ajax.WriteNull(JsonStream);
            else
            {
                fixed (char* valueFixed = value.value) toJson(valueFixed + value.StartIndex, value.Length);
            }
        }
        /// <summary>
        /// 字符串转换
        /// </summary>
        /// <param name="value">JSON节点</param>
        [toJsonMethod]
        private void toJson(jsonNode value)
        {
            switch (value.Type)
            {
                case jsonNode.type.Null:
                    fastCSharp.web.ajax.WriteNull(JsonStream);
                    return;
                case jsonNode.type.Dictionary:
                    subArray<keyValue<jsonNode, jsonNode>> dictionary = value.Dictionary;
                    JsonStream.Write('{');
                    if (dictionary.Count != 0)
                    {
                        int count = dictionary.Count;
                        foreach (keyValue<jsonNode, jsonNode> keyValue in dictionary.array)
                        {
                            if (count != dictionary.Count) JsonStream.Write(',');
                            toJson(keyValue.Key);
                            JsonStream.Write(':');
                            toJson(keyValue.Value);
                            if (--count == 0) break;
                        }
                    }
                    JsonStream.Write('}');
                    return;
                case jsonNode.type.List:
                    subArray<jsonNode> list = value.List;
                     JsonStream.Write('[');
                     if (list.Count != 0)
                    {
                        int count = list.Count;
                        foreach (jsonNode node in list.array)
                        {
                            if (count != list.Count) JsonStream.Write(',');
                            toJson(node);
                            if (--count == 0) break;
                        }
                    }
                    JsonStream.Write(']');
                    return;
                case jsonNode.type.String:
                    subString subString = value.String;
                    fixed (char* valueFixed = subString.value) toJson(valueFixed + subString.StartIndex, subString.Length);
                    return;
                case jsonNode.type.QuoteString:
                    JsonStream.PrepLength(value.String.Length + 2);
                    JsonStream.Unsafer.Write((char)value.Int64);
                    JsonStream.Write(value.String);
                    JsonStream.Unsafer.Write((char)value.Int64);
                    return;
                case jsonNode.type.NumberString:
                    if ((int)value.Int64 == 0) JsonStream.Write(value.String);
                    else
                    {
                        JsonStream.PrepLength(value.String.Length + 2);
                        JsonStream.Unsafer.Write((char)value.Int64);
                        JsonStream.Write(value.String);
                        JsonStream.Unsafer.Write((char)value.Int64);
                    }
                    return;
                case jsonNode.type.Bool:
                    toJson((byte)value.Int64 != 0);
                    return;
                case jsonNode.type.DateTimeTick:
                    toJson(new DateTime(value.Int64));
                    return;
                case jsonNode.type.NaN:
                    fastCSharp.web.ajax.WriteNaN(JsonStream);
                    return;
            }
        }
        /// <summary>
        /// 字符串转换
        /// </summary>
        /// <param name="value">字符串</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(object value)
        {
            if (value == null) fastCSharp.web.ajax.WriteNull(JsonStream);
            else if (toJsonConfig.IsObject)
            {
                Type type = value.GetType();
                if (type == typeof(object)) fastCSharp.web.ajax.WriteObject(JsonStream);
                else typeToJsoner.GetObjectToJsoner(type)(this, value);
            }
            else fastCSharp.web.ajax.WriteObject(JsonStream);
        }
        /// <summary>
        /// 字符串转换
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="length">字符串长度</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(char* start, int length)
        {
            JsonStream.PrepLength(length + 2);
            char* data = JsonStream.CurrentChar;
            *data = fastCSharp.web.ajax.Quote;
            for (char* end = start + length; start != end; ++start) *++data = *start == fastCSharp.web.ajax.Quote ? ' ' : *start;
            *(data + 1) = fastCSharp.web.ajax.Quote;
            JsonStream.Unsafer.AddLength(length + 2);
        }
        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="type">类型</param>
        [toJsonMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void toJson(Type type)
        {
            if (type == null) fastCSharp.web.ajax.WriteNull(JsonStream);
            else typeToJsoner<remoteType>.ToJson(this, new remoteType(type));
        }

        /// <summary>
        /// 值类型对象转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void nullableToJson<valueType>(jsonSerializer toJsoner, Nullable<valueType> value) where valueType : struct
        {
            if (value.HasValue) typeToJsoner<valueType>.StructToJson(toJsoner, value.Value);
            else fastCSharp.web.ajax.WriteNull(toJsoner.JsonStream);
        }
        /// <summary>
        /// 值类型对象转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableToJsonMethod = typeof(jsonSerializer).GetMethod("nullableToJson", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 字典转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void keyValuePairToJson<keyValue, valueType>(jsonSerializer toJsoner, KeyValuePair<keyValue, valueType> value)
        {
            typeToJsoner<keyValue>.KeyValuePair(toJsoner, value);
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairToJsonMethod = typeof(jsonSerializer).GetMethod("keyValuePairToJson", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 引用类型对象转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void classToJson<valueType>(jsonSerializer toJsoner, valueType value)
        {
            if (value == null) fastCSharp.web.ajax.WriteNull(toJsoner.JsonStream);
            else typeToJsoner<valueType>.ClassToJson(toJsoner, value);
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo classToJsonMethod = typeof(jsonSerializer).GetMethod("classToJson", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 值类型对象转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void memberToJson<valueType>(jsonSerializer toJsoner, valueType value)
        {
            typeToJsoner<valueType>.MemberToJson(toJsoner, value);
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo memberToJsonMethod = typeof(jsonSerializer).GetMethod("memberToJson", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 值类型对象转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void nullableMemberToJson<valueType>(jsonSerializer toJsoner, Nullable<valueType> value) where valueType : struct
        {
            if (value.HasValue) typeToJsoner<valueType>.MemberToJson(toJsoner, value.Value);
            else fastCSharp.web.ajax.WriteNull(toJsoner.JsonStream);
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberToJsonMethod = typeof(jsonSerializer).GetMethod("nullableMemberToJson", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 字符串转换
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumToString<valueType>(jsonSerializer toJsoner, valueType value)
        {
            string stringValue = value.ToString();
            char charValue = stringValue[0];
            if ((uint)(charValue - '1') < 10 || charValue == '-') toJsoner.JsonStream.WriteNotNull(stringValue);
            else fastCSharp.web.ajax.ToString(stringValue, toJsoner.JsonStream);
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo enumToStringMethod = typeof(jsonSerializer).GetMethod("enumToString", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// object转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void toJsonObject<valueType>(jsonSerializer toJsoner, object value)
        {
            typeToJsoner<valueType>.ToJson(toJsoner, (valueType)value);
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo toJsonObjectMethod = typeof(jsonSerializer).GetMethod("toJsonObject", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void array<valueType>(valueType[] array)
        {
            if (array == null) fastCSharp.web.ajax.WriteNull(JsonStream);
            else if (push(array))
            {
                typeToJsoner<valueType>.Array(this, array);
                pop();
            }
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayMethod = typeof(jsonSerializer).GetMethod("array", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合转换
        /// </summary>
        /// <param name="values">枚举集合</param>
        private static void structEnumerable<valueType, elementType>(jsonSerializer serializer, valueType value) where valueType : IEnumerable<elementType>
        {
            typeToJsoner<elementType>.Enumerable(serializer, value);
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo structEnumerableMethod = typeof(jsonSerializer).GetMethod("structEnumerable", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举集合转换
        /// </summary>
        /// <param name="values">枚举集合</param>
        private void enumerable<valueType, elementType>(valueType value) where valueType : IEnumerable<elementType>
        {
            if (value == null) fastCSharp.web.ajax.WriteNull(JsonStream);
            else if (pushArray(value))
            {
                typeToJsoner<elementType>.Enumerable(this, value);
                pop();
            }
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo enumerableMethod = typeof(jsonSerializer).GetMethod("enumerable", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典转换
        /// </summary>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void dictionary<valueType, dictionaryValueType>(Dictionary<valueType, dictionaryValueType> dictionary)
        {
            if (dictionary == null) fastCSharp.web.ajax.WriteNull(JsonStream);
            else if (push(dictionary))
            {
                typeToJsoner<valueType>.Dictionary(this, dictionary);
                pop();
            }
        }
        /// <summary>
        /// 字典转换函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMethod = typeof(jsonSerializer).GetMethod("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 字典转换
        /// </summary>
        /// <param name="dictionary">字典</param>
        private void stringDictionary<valueType>(Dictionary<string, valueType> dictionary)
        {
            if (dictionary == null) fastCSharp.web.ajax.WriteNull(JsonStream);
            else if (push(dictionary))
            {
                if (toJsonConfig.IsStringDictionaryToObject) typeToJsoner<valueType>.StringDictionary(this, dictionary);
                else typeToJsoner<string>.Dictionary<valueType>(this, dictionary);
                pop();
            }
        }
        /// <summary>
        /// 字符串字典转换函数信息
        /// </summary>
        private static readonly MethodInfo stringDictionaryMethod = typeof(jsonSerializer).GetMethod("stringDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 基类转换
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void baseToJson<valueType, childType>(jsonSerializer toJsoner, childType value) where childType : valueType
        {
            typeToJsoner<valueType>.ClassToJson(toJsoner, value);
        }
        /// <summary>
        /// 基类转换函数信息
        /// </summary>
        private static readonly MethodInfo baseToJsonMethod = typeof(jsonSerializer).GetMethod("baseToJson", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="jsonStream"></param>
        /// <param name="stream"></param>
        /// <param name="config"></param>
        internal static void Serialize<valueType>(valueType value, charStream jsonStream, unmanagedStream stream, config config)
        {
            ToJson(value, jsonStream, config);
            stream.PrepLength(sizeof(int) + (jsonStream.Length << 1));
            stream.Unsafer.AddLength(sizeof(int));
            int index = stream.Length;
            web.ajax.FormatJavascript(jsonStream, stream);
            int length = stream.Length - index;
            *(int*)(stream.Data + index - sizeof(int)) = length;
            if ((length & 2) != 0) stream.Write(' ');
        }

        /// <summary>
        /// 公共默认配置参数
        /// </summary>
        private static readonly config defaultConfig = new config { CheckLoopDepth = fastCSharp.config.appSetting.JsonDepth };
        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="jsonStream">Json输出缓冲区</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToJson<valueType>(valueType value, charStream jsonStream, config config = null)
        {
            if (jsonStream == null) log.Default.Throw(log.exceptionType.Null);
            jsonSerializer toJsoner = typePool<jsonSerializer>.Pop() ?? new jsonSerializer();
            try
            {
                toJsoner.toJson<valueType>(value, jsonStream, config ?? defaultConfig);
            }
            finally { toJsoner.free(); }
        }
        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>Json字符串</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToJson<valueType>(valueType value, config config = null)
        {
            jsonSerializer toJsoner = typePool<jsonSerializer>.Pop() ?? new jsonSerializer();
            try
            {
                return toJsoner.toJson<valueType>(value, config ?? defaultConfig);
            }
            finally { toJsoner.free(); }
        }
        /// <summary>
        /// 未知类型对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>Json字符串</returns>
        private static string objectToJson<valueType>(object value, config config)
        {
            jsonSerializer toJsoner = typePool<jsonSerializer>.Pop() ?? new jsonSerializer();
            try
            {
                return toJsoner.toJson<valueType>((valueType)value, config ?? defaultConfig);
            }
            finally { toJsoner.free(); }
        }
        /// <summary>
        /// 未知类型对象转换JSON字符串
        /// </summary>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>Json字符串</returns>
        public static string ObjectToJson(object value, config config = null)
        {
            if (value == null) return fastCSharp.web.ajax.Null;
            Type type = value.GetType();
            Func<object, config, string> toJson;
            if (!objectToJsons.TryGetValue(type, out toJson))
            {
                objectToJsons.Set(type, toJson = (Func<object, config, string>)Delegate.CreateDelegate(typeof(Func<object, config, string>), objectToJsonMethod.MakeGenericMethod(type)));
            }
            return toJson(value, config);
        }
        /// <summary>
        /// 未知类型对象转换JSON字符串
        /// </summary>
        private static interlocked.dictionary<Type, Func<object, config, string>> objectToJsons = new interlocked.dictionary<Type,Func<object,config,string>>(fastCSharp.dictionary.CreateOnly<Type, Func<object, config, string>>());
        /// <summary>
        /// 未知类型对象转换JSON字符串函数信息
        /// </summary>
        private static readonly MethodInfo objectToJsonMethod = typeof(jsonSerializer).GetMethod("objectToJson", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(object), typeof(config) }, null);
        /// <summary>
        /// 获取Json字符串输出缓冲区属性方法信息
        /// </summary>
        private static readonly FieldInfo jsonStreamField = typeof(jsonSerializer).GetField("JsonStream", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> toJsonMethods;
        /// <summary>
        /// 获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getToJsonMethod(Type type)
        {
            MethodInfo method;
            return toJsonMethods.TryGetValue(type, out method) ? method : null;
        }
        static jsonSerializer()
        {
            toJsonMethods = fastCSharp.dictionary.CreateOnly<Type, MethodInfo>();
            foreach (MethodInfo method in typeof(jsonSerializer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (method.customAttribute<toJsonMethod>() != null)
                {
                    toJsonMethods.Add(method.GetParameters()[0].ParameterType, method);
                }
            }
        }
    }
}
