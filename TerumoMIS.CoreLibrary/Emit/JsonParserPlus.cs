//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: JsonParserPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  JsonParserPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 14:50:39
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using TerumoMIS.CoreLibrary.Code;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// Json解析器
    /// </summary>
    public unsafe sealed class JsonParserPlus
    {
        /// <summary>
        /// 解析状态
        /// </summary>
        public enum ParseStateEnum : byte
        {
            /// <summary>
            /// 成功
            /// </summary>
            Success,
            /// <summary>
            /// 成员位图类型错误
            /// </summary>
            MemberMap,
            /// <summary>
            /// Json字符串参数为空
            /// </summary>
            NullJson,
            /// <summary>
            /// 解析目标对象参数为空
            /// </summary>
            NullValue,
            /// <summary>
            /// 非正常意外结束
            /// </summary>
            CrashEnd,
            /// <summary>
            /// 未能识别的注释
            /// </summary>
            UnknownNote,
            /// <summary>
            /// /**/注释缺少回合
            /// </summary>
            NoteNotRound,
            /// <summary>
            /// null值解析失败
            /// </summary>
            NotNull,
            /// <summary>
            /// 逻辑值解析错误
            /// </summary>
            NotBool,
            /// <summary>
            /// 非数字解析错误
            /// </summary>
            NotNumber,
            /// <summary>
            /// 16进制数字解析错误
            /// </summary>
            NotHex,
            /// <summary>
            /// 字符解析错误
            /// </summary>
            NotChar,
            /// <summary>
            /// 字符串解析失败
            /// </summary>
            NotString,
            /// <summary>
            /// 字符串被换行截断
            /// </summary>
            StringEnter,
            /// <summary>
            /// 时间解析错误
            /// </summary>
            NotDateTime,
            /// <summary>
            /// Guid解析错误
            /// </summary>
            NotGuid,
            /// <summary>
            /// 不支持多维数组
            /// </summary>
            ArrayManyRank,
            /// <summary>
            /// 数组解析错误
            /// </summary>
            NotArray,
            /// <summary>
            /// 数组数据解析错误
            /// </summary>
            NotArrayValue,
            ///// <summary>
            ///// 不支持指针
            ///// </summary>
            //Pointer,
            /// <summary>
            /// 找不到构造函数
            /// </summary>
            NoConstructor,
            /// <summary>
            /// 非枚举字符
            /// </summary>
            NotEnumChar,
            /// <summary>
            /// 没有找到匹配的枚举值
            /// </summary>
            NoFoundEnumValue,
            /// <summary>
            /// 对象解析错误
            /// </summary>
            NotObject,
            /// <summary>
            /// 没有找到成员名称
            /// </summary>
            NotFoundName,
            /// <summary>
            /// 没有找到冒号
            /// </summary>
            NotFoundColon,
            /// <summary>
            /// 忽略值解析错误
            /// </summary>
            UnknownValue,
            /// <summary>
            /// 字典解析错误
            /// </summary>
            NotDictionary,
            /// <summary>
            /// 类型解析错误
            /// </summary>
            ErrorType,
        }
        /// <summary>
        /// 配置参数
        /// </summary>
        public sealed class ConfigPlus
        {
            /// <summary>
            /// Json字符串
            /// </summary>
            internal SubStringStruct Json;
            /// <summary>
            /// 当前解析位置
            /// </summary>
            internal int CurrentIndex;
            /// <summary>
            /// 成员位图
            /// </summary>
            public MemberMapPlus MemberMap;
            /// <summary>
            /// 解析状态
            /// </summary>
            public ParseStateEnum State { get; internal set; }
            /// <summary>
            /// 自定义构造函数
            /// </summary>
            public Func<Type, object> Constructor;
            /// <summary>
            /// 成员选择
            /// </summary>
            public MemberFiltersEnum MemberFilter = MemberFiltersEnum.PublicInstance;
            /// <summary>
            /// 是否获取Json字符串与当前解析位置信息
            /// </summary>
            public bool IsGetJson = true;
            /// <summary>
            /// 对象解析结束后是否检测最后的空格符
            /// </summary>
            public bool IsEndSpace = true;
            /// <summary>
            /// 是否临时字符串(可修改)
            /// </summary>
            public bool IsTempString;
            /// <summary>
            /// 是否强制匹配枚举值
            /// </summary>
            public bool IsMatchEnum;
        }
        /// <summary>
        /// 名称状态查找器
        /// </summary>
        internal struct StateSearcherStruct
        {
            /// <summary>
            /// Json解析器
            /// </summary>
            private JsonParserPlus _parser;
            /// <summary>
            /// 状态集合
            /// </summary>
            private byte* _state;
            /// <summary>
            /// ASCII字符查找表
            /// </summary>
            private byte* _charsAscii;
            /// <summary>
            /// 特殊字符串查找表
            /// </summary>
            private byte* _charStart;
            /// <summary>
            /// 特殊字符串查找表结束位置
            /// </summary>
            private byte* charEnd;
            /// <summary>
            /// 当前状态
            /// </summary>
            private byte* currentState;
            /// <summary>
            /// 特殊字符起始值
            /// </summary>
            private int charIndex;
            /// <summary>
            /// 查询矩阵单位尺寸类型
            /// </summary>
            private byte tableType;
            /// <summary>
            /// 名称查找器
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="data">数据起始位置</param>
            internal StateSearcherStruct(jsonParser parser, pointer data)
            {
                this._parser = parser;
                if (data.Data == null)
                {
                    _state = _charsAscii = _charStart = charEnd = currentState = null;
                    charIndex = 0;
                    tableType = 0;
                }
                else
                {
                    int stateCount = *data.Int;
                    currentState = _state = data.Byte + sizeof(int);
                    _charsAscii = _state + stateCount * 3 * sizeof(int);
                    _charStart = _charsAscii + 128 * sizeof(ushort);
                    charIndex = *(ushort*)_charStart;
                    _charStart += sizeof(ushort) * 2;
                    charEnd = _charStart + *(ushort*)(_charStart - sizeof(ushort)) * sizeof(ushort);
                    if (stateCount < 256) tableType = 0;
                    else if (stateCount < 65536) tableType = 1;
                    else tableType = 2;
                }
            }
            /// <summary>
            /// 获取特殊字符索引值
            /// </summary>
            /// <param name="value">特殊字符</param>
            /// <returns>索引值,匹配失败返回0</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int getCharIndex(char value)
            {
                char* current = fastCSharp.stateSearcher.chars.GetCharIndex((char*)_charStart, (char*)charEnd, value);
                return current == null ? 0 : (charIndex + (int)(current - (char*)_charStart));
                //char* charStart = (char*)this.charStart, charEnd = (char*)this.charEnd, current = charStart + ((int)(charEnd - charStart) >> 1);
                //while (*current != value)
                //{
                //    if (value < *current)
                //    {
                //        if (current == charStart) return 0;
                //        charEnd = current;
                //        current = charStart + ((int)(charEnd - charStart) >> 1);
                //    }
                //    else
                //    {
                //        if ((charStart = current + 1) == charEnd) return 0;
                //        current = charStart + ((int)(charEnd - charStart) >> 1);
                //    }
                //}
                //return charIndex + (int)(current - (char*)this.charStart);
            }
            /// <summary>
            /// 根据字符串查找目标索引
            /// </summary>
            /// <returns>目标索引,null返回-1</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal int SearchString()
            {
                char value = _parser.searchQuote();
                if (_parser.state != ParseStateEnum.Success || _state == null) return -1;
                currentState = _state;
                return searchString(value);
            }
            /// <summary>
            /// 获取名称索引
            /// </summary>
            /// <param name="isQuote">名称是否带引号</param>
            /// <returns>名称索引,失败返回-1</returns>
            internal int SearchName(ref bool isQuote)
            {
                char value = _parser.getFirstName();
                if (_state == null) return -1;
                if (_parser.quote != 0)
                {
                    isQuote = true;
                    currentState = _state;
                    return searchString(value);
                }
                if (_parser.state != ParseStateEnum.Success) return -1;
                isQuote = false;
                currentState = _state;
                do
                {
                    char* prefix = (char*)(currentState + *(int*)currentState);
                    if (*prefix != 0)
                    {
                        if (value != *prefix) return -1;
                        while (*++prefix != 0)
                        {
                            if (_parser.getNextName() != *prefix) return -1;
                        }
                        value = _parser.getNextName();
                    }
                    if (value == 0) return _parser.state == ParseStateEnum.Success ? *(int*)(currentState + sizeof(int) * 2) : -1;
                    if (*(int*)(currentState + sizeof(int)) == 0) return -1;
                    int index = value < 128 ? (int)*(ushort*)(_charsAscii + (value << 1)) : getCharIndex(value);
                    byte* table = currentState + *(int*)(currentState + sizeof(int));
                    if (tableType == 0)
                    {
                        if ((index = *(table + index)) == 0) return -1;
                        currentState = _state + index * 3 * sizeof(int);
                    }
                    else if (tableType == 1)
                    {
                        if ((index = (int)*(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                        currentState = _state + index * 3 * sizeof(int);
                    }
                    else
                    {
                        if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                        currentState = _state + index;
                    }
                    value = _parser.getNextName();
                }
                while (true);
            }
            /// <summary>
            /// 根据字符串查找目标索引
            /// </summary>
            /// <param name="value">第一个字符</param>
            /// <returns>目标索引,null返回-1</returns>
            internal int searchString(char value)
            {
                do
                {
                    char* prefix = (char*)(currentState + *(int*)currentState);
                    if (*prefix != 0)
                    {
                        if (value != *prefix) return -1;
                        while (*++prefix != 0)
                        {
                            if (_parser.nextStringChar() != *prefix) return -1;
                        }
                        value = _parser.nextStringChar();
                    }
                    if (value == 0) return _parser.state == ParseStateEnum.Success ? *(int*)(currentState + sizeof(int) * 2) : -1;
                    if (*(int*)(currentState + sizeof(int)) == 0) return -1;
                    int index = value < 128 ? (int)*(ushort*)(_charsAscii + (value << 1)) : getCharIndex(value);
                    byte* table = currentState + *(int*)(currentState + sizeof(int));
                    if (tableType == 0)
                    {
                        if ((index = *(table + index)) == 0) return -1;
                        currentState = _state + index * 3 * sizeof(int);
                    }
                    else if (tableType == 1)
                    {
                        if ((index = (int)*(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                        currentState = _state + index * 3 * sizeof(int);
                    }
                    else
                    {
                        if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                        currentState = _state + index;
                    }
                    value = _parser.nextStringChar();
                }
                while (true);
            }
            /// <summary>
            /// 根据枚举字符串查找目标索引
            /// </summary>
            /// <returns>目标索引,null返回-1</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal int SearchFlagEnum()
            {
                char value = _parser.searchEnumQuote();
                if (_state == null) return -1;
                currentState = _state;
                return flagEnum(value);
            }
            /// <summary>
            /// 根据枚举字符串查找目标索引
            /// </summary>
            /// <returns>目标索引,null返回-1</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal int NextFlagEnum()
            {
                char value = _parser.searchNextEnum();
                if (_state == null) return -1;
                currentState = _state;
                return flagEnum(value);
            }
            /// <summary>
            /// 根据枚举字符串查找目标索引
            /// </summary>
            /// <param name="value">当前字符</param>
            /// <returns>目标索引,null返回-1</returns>
            private int flagEnum(char value)
            {
                do
                {
                    char* prefix = (char*)(currentState + *(int*)currentState);
                    if (*prefix != 0)
                    {
                        if (value != *prefix) return -1;
                        while (*++prefix != 0)
                        {
                            if (_parser.nextEnumChar() != *prefix) return -1;
                        }
                        value = _parser.nextEnumChar();
                    }
                    if (value == 0 || value == ',') return _parser.state == ParseStateEnum.Success ? *(int*)(currentState + sizeof(int) * 2) : -1;
                    if (*(int*)(currentState + sizeof(int)) == 0) return -1;
                    int index = value < 128 ? (int)*(ushort*)(_charsAscii + (value << 1)) : getCharIndex(value);
                    byte* table = currentState + *(int*)(currentState + sizeof(int));
                    if (tableType == 0)
                    {
                        if ((index = *(table + index)) == 0) return -1;
                        currentState = _state + index * 3 * sizeof(int);
                    }
                    else if (tableType == 1)
                    {
                        if ((index = (int)*(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                        currentState = _state + index * 3 * sizeof(int);
                    }
                    else
                    {
                        if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                        currentState = _state + index;
                    }
                    value = _parser.nextEnumChar();
                }
                while (true);
            }
            /// <summary>
            /// 泛型定义类型成员名称查找数据
            /// </summary>
            private static interlocked.dictionary<Type, pointer> genericDefinitionMemberSearchers = new interlocked.dictionary<Type,pointer>(fastCSharp.dictionary.CreateOnly<Type, pointer>());
            /// <summary>
            /// 泛型定义类型成员名称查找数据创建锁
            /// </summary>
            private static readonly object genericDefinitionMemberSearcherCreateLock = new object();
            /// <summary>
            /// 获取泛型定义成员名称查找数据
            /// </summary>
            /// <param name="type">泛型定义类型</param>
            /// <param name="names">成员名称集合</param>
            /// <returns>泛型定义成员名称查找数据</returns>
            internal static pointer GetGenericDefinitionMember(Type type, string[] names)
            {
                pointer data;
                if (genericDefinitionMemberSearchers.TryGetValue(type = type.GetGenericTypeDefinition(), out data)) return data;
                Monitor.Enter(genericDefinitionMemberSearcherCreateLock);
                if (genericDefinitionMemberSearchers.TryGetValue(type, out data))
                {
                    Monitor.Exit(genericDefinitionMemberSearcherCreateLock);
                    return data;
                }
                try
                {
                    genericDefinitionMemberSearchers.Set(type, data = fastCSharp.stateSearcher.chars.Create(names));
                }
                finally { Monitor.Exit(genericDefinitionMemberSearcherCreateLock); }
                return data;
            }
        }
        /// <summary>
        /// 类型解析器静态信息
        /// </summary>
        internal static class typeParser
        {
            /// <summary>
            /// 获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <param name="typeAttribute">类型配置</param>
            /// <returns>字段成员集合</returns>
            public static subArray<fieldIndex> GetFields(fieldIndex[] fields, jsonParse typeAttribute, ref fieldIndex defaultMember)
            {
                subArray<fieldIndex> values = new subArray<fieldIndex>(fields.Length);
                foreach (fieldIndex field in fields)
                {
                    Type type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        jsonParse.member attribute = field.GetAttribute<jsonParse.member>(true, true);
                        if (typeAttribute.IsAllMember ? (attribute == null || attribute.IsSetup) : (attribute != null && attribute.IsSetup))
                        {
                            if (attribute != null && attribute.IsDefault) defaultMember = field;
                            values.Add(field);
                        }
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
            public static subArray<keyValue<propertyIndex, MethodInfo>> GetProperties(propertyIndex[] properties, jsonParse typeAttribute)
            {
                subArray<keyValue<propertyIndex, MethodInfo>> values = new subArray<keyValue<propertyIndex, MethodInfo>>(properties.Length);
                foreach (propertyIndex property in properties)
                {
                    if (property.Member.CanWrite)
                    {
                     Type   type = property.Member.PropertyType;
                        if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !property.IsIgnore)
                        {
                            jsonParse.member attribute = property.GetAttribute<jsonParse.member>(true, true);
                            if (typeAttribute.IsAllMember ? (attribute == null || attribute.IsSetup) : (attribute != null && attribute.IsSetup))
                            {
                                MethodInfo method = property.Member.GetSetMethod(true);
                                if (method != null && method.GetParameters().Length == 1)
                                {
                                    values.Add(new keyValue<propertyIndex, MethodInfo>(property, method));
                                }
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
            public static subArray<memberIndex> GetMembers(fieldIndex[] fieldIndexs, propertyIndex[] properties, jsonParse typeAttribute)
            {
                subArray<memberIndex> members = new subArray<memberIndex>(fieldIndexs.Length + properties.Length);
                foreach (fieldIndex field in fieldIndexs)
                {
                    Type type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        jsonParse.member attribute = field.GetAttribute<jsonParse.member>(true, true);
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
                            jsonParse.member attribute = property.GetAttribute<jsonParse.member>(true, true);
                            if (typeAttribute.IsAllMember ? (attribute == null || attribute.IsSetup) : (attribute != null && attribute.IsSetup))
                            {
                                MethodInfo method = property.Member.GetSetMethod(true);
                                if (method != null && method.GetParameters().Length == 1) members.Add(property);
                            }
                        }
                    }
                }
                return members;
            }

            /// <summary>
            /// 创建解析委托函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="field"></param>
            /// <returns>解析委托函数</returns>
            public static DynamicMethod CreateDynamicMethod(Type type, FieldInfo field)
            {
                DynamicMethod dynamicMethod = new DynamicMethod("jsonParser" + field.Name, null, new Type[] { typeof(jsonParser), type.MakeByRefType() }, type, true);
                ILGenerator generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                if (!type.IsValueType) generator.Emit(OpCodes.Ldind_Ref);
                generator.Emit(OpCodes.Ldflda, field);
                MethodInfo methodInfo = getMemberMethodInfo(field.FieldType);
                generator.Emit(methodInfo.IsFinal || !methodInfo.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, methodInfo);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod;
            }
            /// <summary>
            /// 创建解析委托函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="property"></param>
            /// <param name="propertyMethod"></param>
            /// <returns>解析委托函数</returns>
            public static DynamicMethod CreateDynamicMethod(Type type, PropertyInfo property, MethodInfo propertyMethod)
            {
                DynamicMethod dynamicMethod = new DynamicMethod("jsonParser" + property.Name, null, new Type[] { typeof(jsonParser), type.MakeByRefType() }, type, true);
                ILGenerator generator = dynamicMethod.GetILGenerator();
                Type memberType = property.PropertyType;
                LocalBuilder loadMember = generator.DeclareLocal(memberType);
                MethodInfo methodInfo = getMemberMethodInfo(memberType);
                if (!memberType.IsValueType)
                {
                    generator.Emit(OpCodes.Ldloca_S, loadMember);
                    generator.Emit(OpCodes.Initobj, memberType);
                }
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldloca_S, loadMember);
                generator.Emit(methodInfo.IsFinal || !methodInfo.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, methodInfo);

                generator.Emit(OpCodes.Ldarg_1);
                if (!type.IsValueType) generator.Emit(OpCodes.Ldind_Ref);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(propertyMethod.IsFinal || !propertyMethod.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, propertyMethod);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod;
            }
            /// <summary>
            /// 获取成员转换函数信息
            /// </summary>
            /// <param name="type">成员类型</param>
            /// <returns>成员转换函数信息</returns>
            private static MethodInfo getMemberMethodInfo(Type type)
            {
                MethodInfo methodInfo = jsonParser.getParseMethod(type);
                if (methodInfo != null) return methodInfo;
                if (type.IsArray) return GetArrayParser(type.GetElementType());
                if (type.IsEnum) return GetEnumParser(type);
                if (type.IsGenericType)
                {
                    Type genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(Dictionary<,>)) return GetDictionaryParser(type);
                    if (genericType == typeof(Nullable<>))
                    {
                        Type[] parameterTypes = type.GetGenericArguments();
                        return (parameterTypes[0].IsEnum ? nullableEnumParseMethod : nullableParseMethod).MakeGenericMethod(parameterTypes);
                    }
                    if (genericType == typeof(KeyValuePair<,>)) return keyValuePairParseMethod.MakeGenericMethod(type.GetGenericArguments());
                }
                if ((methodInfo = GetCustomParser(type)) != null) return methodInfo;
                if (type.IsAbstract || type.IsInterface) return GetNoConstructorParser(type);
                if ((methodInfo = GetIEnumerableConstructorParser(type)) != null) return methodInfo;
                if (type.IsValueType) return GetValueTypeParser(type);
                return GetTypeParser(type);
            }

            /// <summary>
            /// 数组解析调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> arrayParsers = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取数组解析委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>数组解析委托调用函数信息</returns>
            public static MethodInfo GetArrayParser(Type type)
            {
                MethodInfo method;
                if (arrayParsers.TryGetValue(type, out method)) return method;
                arrayParsers.Set(type, method = arrayMethod.MakeGenericMethod(type));
                return method;
            }
            /// <summary>
            /// 缺少构造函数解析调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> noConstructorParsers = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取缺少构造函数委托调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>缺少构造函数委托调用函数信息</returns>
            public static MethodInfo GetNoConstructorParser(Type type)
            {
                MethodInfo method;
                if (noConstructorParsers.TryGetValue(type, out method)) return method;
                method = checkNoConstructorMethod.MakeGenericMethod(type);
                noConstructorParsers.Set(type, method);
                return method;
            }
            /// <summary>
            /// 获取枚举构造调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> enumerableConstructorParsers = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取枚举构造调用函数信息
            /// </summary>
            /// <param name="type">集合类型</param>
            /// <returns>枚举构造调用函数信息</returns>
            public static MethodInfo GetIEnumerableConstructorParser(Type type)
            {
                MethodInfo method;
                if (enumerableConstructorParsers.TryGetValue(type, out method)) return method;
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
                                method = listConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                method = collectionConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                method = enumerableConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = argumentType.MakeArrayType();
                            constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                            if (constructorInfo != null)
                            {
                                method = arrayConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                        }
                        else if (genericType == typeof(IDictionary<,>))
                        {
                            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { interfaceType }, null);
                            if (constructorInfo != null)
                            {
                                Type[] parameters = interfaceType.GetGenericArguments();
                                method = dictionaryConstructorMethod.MakeGenericMethod(type, parameters[0], parameters[1]);
                                break;
                            }
                        }
                    }
                }
                enumerableConstructorParsers.Set(type, method);
                return method;
            }
            /// <summary>
            /// 枚举解析调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> enumParsers = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取枚举解析调用函数信息
            /// </summary>
            /// <param name="type">枚举类型</param>
            /// <returns>枚举解析调用函数信息</returns>
            public static MethodInfo GetEnumParser(Type type)
            {
                MethodInfo method;
                if (enumParsers.TryGetValue(type, out method)) return method;
                Type enumType = Enum.GetUnderlyingType(type);
                if (fastCSharp.code.typeAttribute.GetAttribute<FlagsAttribute>(type, false, false) == null)
                {
                    if (enumType == typeof(uint)) method = enumUIntMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(byte)) method = enumByteMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(ulong)) method = enumULongMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(ushort)) method = enumUShortMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(long)) method = enumLongMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(short)) method = enumShortMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(sbyte)) method = enumSByteMethod.MakeGenericMethod(type);
                    else method = enumIntMethod.MakeGenericMethod(type);
                }
                else
                {
                    if (enumType == typeof(uint)) method = enumUIntFlagsMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(byte)) method = enumByteFlagsMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(ulong)) method = enumULongFlagsMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(ushort)) method = enumUShortFlagsMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(long)) method = enumLongFlagsMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(short)) method = enumShortFlagsMethod.MakeGenericMethod(type);
                    else if (enumType == typeof(sbyte)) method = enumSByteFlagsMethod.MakeGenericMethod(type);
                    else method = enumIntFlagsMethod.MakeGenericMethod(type);
                }
                enumParsers.Set(type, method);
                return method;
            }
            /// <summary>
            /// 值类型解析调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> valueTypeParsers = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取值类型解析调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>值类型解析调用函数信息</returns>
            public static MethodInfo GetValueTypeParser(Type type)
            {
                MethodInfo method;
                if (valueTypeParsers.TryGetValue(type, out method)) return method;
                Type nullType = type.nullableType();
                if (nullType == null) method = structParseMethod.MakeGenericMethod(type);
                else method = nullableMemberParseMethod.MakeGenericMethod(nullType);
                valueTypeParsers.Set(type, method);
                return method;
            }
            /// <summary>
            /// 引用类型解析调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> typeParsers = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取引用类型解析调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>引用类型解析调用函数信息</returns>
            public static MethodInfo GetTypeParser(Type type)
            {
                MethodInfo method;
                if (typeParsers.TryGetValue(type, out method)) return method;
                if (type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, nullValue<Type>.Array, null) == null) method = checkNoConstructorMethod.MakeGenericMethod(type);
                else method = typeParseMethod.MakeGenericMethod(type);
                typeParsers.Set(type, method);
                return method;
            }
            /// <summary>
            /// 字典解析调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> dictionaryParsers = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 获取字典解析调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>字典解析调用函数信息</returns>
            public static MethodInfo GetDictionaryParser(Type type)
            {
                MethodInfo method;
                if (dictionaryParsers.TryGetValue(type, out method)) return method;
                method = dictionaryMethod.MakeGenericMethod(type.GetGenericArguments());
                dictionaryParsers.Set(type, method);
                return method;
            }
            /// <summary>
            /// 自定义解析调用函数信息集合
            /// </summary>
            private static interlocked.dictionary<Type, MethodInfo> customParsers = new interlocked.dictionary<Type,MethodInfo>(fastCSharp.dictionary.CreateOnly<Type, MethodInfo>());
            /// <summary>
            /// 自定义解析委托调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>自定义解析委托调用函数信息</returns>
            public static MethodInfo GetCustomParser(Type type)
            {
                MethodInfo method;
                if (customParsers.TryGetValue(type, out method)) return method;
                Type refType = type.MakeByRefType();
                foreach (fastCSharp.code.attributeMethod methodInfo in fastCSharp.code.attributeMethod.GetStatic(type))
                {
                    if (methodInfo.Method.ReturnType == typeof(void))
                    {
                        ParameterInfo[] parameters = methodInfo.Method.GetParameters();
                        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(jsonParser) && parameters[1].ParameterType == refType)
                        {
                            if (methodInfo.GetAttribute<jsonParse.custom>(true) != null)
                            {
                                method = methodInfo.Method;
                                break;
                            }
                        }
                    }
                }
                customParsers.Set(type, method);
                return method;
            }
        }
        /// <summary>
        /// 类型解析器
        /// </summary>
        /// <typeparam name="valueType">目标类型</typeparam>
        internal static class typeParser<valueType>
        {
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal class enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                protected static readonly valueType[] enumValues;
                /// <summary>
                /// 枚举名称查找数据
                /// </summary>
                protected static readonly pointer enumSearcher;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                protected static void parse(jsonParser parser, ref valueType value)
                {
                    int index = new StateSearcherStruct(parser, enumSearcher).SearchString();
                    if (index != -1) value = enumValues[index];
                    else if (parser.parseConfig.IsMatchEnum) parser.state = ParseStateEnum.NoFoundEnumValue;
                    else parser.searchStringEnd();
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="index">第一个枚举索引</param>
                /// <param name="nextIndex">第二个枚举索引</param>
                protected static void getIndex(jsonParser parser, ref valueType value, out int index, ref int nextIndex)
                {
                    StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                    if ((index = searcher.SearchFlagEnum()) == -1)
                    {
                        if (parser.parseConfig.IsMatchEnum)
                        {
                            parser.state = ParseStateEnum.NoFoundEnumValue;
                            return;
                        }
                        else
                        {
                            do
                            {
                                if (parser.state != ParseStateEnum.Success || parser.quote == 0) return;
                            }
                            while ((index = searcher.NextFlagEnum()) == -1);
                        }
                    }
                    do
                    {
                        if (parser.quote == 0)
                        {
                            value = enumValues[index];
                            return;
                        }
                        if ((nextIndex = searcher.NextFlagEnum()) != -1) break;
                        if (parser.state != ParseStateEnum.Success) return;
                    }
                    while (true);
                }
                static enumBase()
                {
                    Dictionary<hashString, valueType> values = ((valueType[])Enum.GetValues(typeof(valueType))).getDictionary(value => (hashString)value.ToString());
                    enumValues = values.getArray(value => value.Value);
                    enumSearcher = fastCSharp.stateSearcher.chars.Create(values.getArray(value => value.Key.ToString()));
                }
            }
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal sealed class enumByte : enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                private static readonly pointer enumInts;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        byte intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, byte>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        byte intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, byte>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                            byte intValue = enumInts.Byte[index];
                            intValue |= enumInts.Byte[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != ParseStateEnum.Success) return;
                                if (index != -1) intValue |= enumInts.Byte[index];
                            }
                            value = pub.enumCast<valueType, byte>.FromInt(intValue);
                        }
                    }
                }
                static enumByte()
                {
                    enumInts = unmanaged.Get(enumValues.Length * sizeof(byte), false);
                    byte* data = enumInts.Byte;
                    foreach (valueType value in enumValues) *(byte*)data++ = pub.enumCast<valueType, byte>.ToInt(value);
                }
            }
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal sealed class enumSByte : enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                private static readonly pointer enumInts;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        sbyte intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, sbyte>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        sbyte intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, sbyte>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                            sbyte intValue = enumInts.SByte[index];
                            intValue |= enumInts.SByte[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != ParseStateEnum.Success) return;
                                if (index != -1) intValue |= enumInts.SByte[index];
                            }
                            value = pub.enumCast<valueType, sbyte>.FromInt(intValue);
                        }
                    }
                }
                static enumSByte()
                {
                    enumInts = unmanaged.Get(enumValues.Length * sizeof(sbyte), false);
                    sbyte* data = enumInts.SByte;
                    foreach (valueType value in enumValues) *(sbyte*)data++ = pub.enumCast<valueType, sbyte>.ToInt(value);
                }
            }
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal sealed class enumShort : enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                private static readonly pointer enumInts;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        short intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, short>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        short intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, short>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                            short intValue = enumInts.Short[index];
                            intValue |= enumInts.Short[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != ParseStateEnum.Success) return;
                                if (index != -1) intValue |= enumInts.Short[index];
                            }
                            value = pub.enumCast<valueType, short>.FromInt(intValue);
                        }
                    }
                }
                static enumShort()
                {
                    enumInts = unmanaged.Get(enumValues.Length * sizeof(short), false);
                    short* data = enumInts.Short;
                    foreach (valueType value in enumValues) *(short*)data++ = pub.enumCast<valueType, short>.ToInt(value);
                }
            }
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal sealed class enumUShort : enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                private static readonly pointer enumInts;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        ushort intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, ushort>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        ushort intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, ushort>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                            ushort intValue = enumInts.UShort[index];
                            intValue |= enumInts.UShort[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != ParseStateEnum.Success) return;
                                if (index != -1) intValue |= enumInts.UShort[index];
                            }
                            value = pub.enumCast<valueType, ushort>.FromInt(intValue);
                        }
                    }
                }
                static enumUShort()
                {
                    enumInts = unmanaged.Get(enumValues.Length * sizeof(ushort), false);
                    ushort* data = enumInts.UShort;
                    foreach (valueType value in enumValues) *(ushort*)data++ = pub.enumCast<valueType, ushort>.ToInt(value);
                }
            }
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal sealed class enumInt : enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                private static readonly pointer enumInts;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        int intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, int>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        int intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, int>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                            int intValue = enumInts.Int[index];
                            intValue |= enumInts.Int[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != ParseStateEnum.Success) return;
                                if (index != -1) intValue |= enumInts.Int[index];
                            }
                            value = pub.enumCast<valueType, int>.FromInt(intValue);
                        }
                    }
                }
                static enumInt()
                {
                    enumInts = unmanaged.Get(enumValues.Length * sizeof(int), false);
                    int* data = enumInts.Int;
                    foreach (valueType value in enumValues) *(int*)data++ = pub.enumCast<valueType, int>.ToInt(value);
                }
            }
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal sealed class enumUInt : enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                private static readonly pointer enumInts;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        uint intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, uint>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        uint intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, uint>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                            uint intValue = enumInts.UInt[index];
                            intValue |= enumInts.UInt[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != ParseStateEnum.Success) return;
                                if (index != -1) intValue |= enumInts.UInt[index];
                            }
                            value = pub.enumCast<valueType, uint>.FromInt(intValue);
                        }
                    }
                }
                static enumUInt()
                {
                    enumInts = unmanaged.Get(enumValues.Length * sizeof(uint), false);
                    uint* data = enumInts.UInt;
                    foreach (valueType value in enumValues) *(uint*)data++ = pub.enumCast<valueType, uint>.ToInt(value);
                }
            }
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal sealed class enumLong : enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                private static readonly pointer enumInts;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        long intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, long>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        long intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, long>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                            long intValue = enumInts.Long[index];
                            intValue |= enumInts.Long[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != ParseStateEnum.Success) return;
                                if (index != -1) intValue |= enumInts.Long[index];
                            }
                            value = pub.enumCast<valueType, long>.FromInt(intValue);
                        }
                    }
                }
                static enumLong()
                {
                    enumInts = unmanaged.Get(enumValues.Length * sizeof(long), false);
                    long* data = enumInts.Long;
                    foreach (valueType value in enumValues) *(long*)data++ = pub.enumCast<valueType, long>.ToInt(value);
                }
            }
            /// <summary>
            /// 枚举值解析
            /// </summary>
            internal sealed class enumULong : enumBase
            {
                /// <summary>
                /// 枚举值集合
                /// </summary>
                private static readonly pointer enumInts;
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        ulong intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, ulong>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }
                /// <summary>
                /// 枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(jsonParser parser, ref valueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        ulong intValue = 0;
                        parser.Parse(ref intValue);
                        value = pub.enumCast<valueType, ulong>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            StateSearcherStruct searcher = new StateSearcherStruct(parser, enumSearcher);
                            ulong intValue = enumInts.ULong[index];
                            intValue |= enumInts.ULong[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != ParseStateEnum.Success) return;
                                if (index != -1) intValue |= enumInts.ULong[index];
                            }
                            value = pub.enumCast<valueType, ulong>.FromInt(intValue);
                        }
                    }
                }
                static enumULong()
                {
                    enumInts = unmanaged.Get(enumValues.Length * sizeof(ulong), false);
                    ulong* data = enumInts.ULong;
                    foreach (valueType value in enumValues) *(ulong*)data++ = pub.enumCast<valueType, ulong>.ToInt(value);
                }
            }
            /// <summary>
            /// 解析委托
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            internal delegate void tryParse(jsonParser parser, ref valueType value);
            /// <summary>
            /// 成员解析器过滤
            /// </summary>
            private struct tryParseFilter
            {
                /// <summary>
                /// 成员解析器
                /// </summary>
                public tryParse TryParse;
                /// <summary>
                /// 成员位图索引
                /// </summary>
                public int MemberMapIndex;
                /// <summary>
                /// 成员选择
                /// </summary>
                public fastCSharp.code.memberFilters Filter;
                /// <summary>
                /// 成员解析器
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Call(jsonParser parser, ref valueType value)
                {
                    if ((parser.parseConfig.MemberFilter & Filter) == Filter) TryParse(parser, ref value);
                    else parser.ignore();
                }
                /// <summary>
                /// 成员解析器
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="memberMap">成员位图</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Call(jsonParser parser, memberMap memberMap, ref valueType value)
                {
                    if ((parser.parseConfig.MemberFilter & Filter) == Filter)
                    {
                        TryParse(parser, ref value);
                        memberMap.SetMember(MemberMapIndex);
                    }
                    else parser.ignore();
                }
            }
            /// <summary>
            /// Json解析类型配置
            /// </summary>
            private static readonly jsonParse attribute;
            /// <summary>
            /// 解析委托
            /// </summary>
            internal static readonly tryParse DefaultParser;
            /// <summary>
            /// 是否值类型
            /// </summary>
            private static readonly bool isValueType;
            /// <summary>
            /// 成员解析器集合
            /// </summary>
            private static tryParseFilter[] memberParsers;
            /// <summary>
            /// 成员名称查找数据
            /// </summary>
            private static pointer memberSearcher;
            /// <summary>
            /// 引用类型对象解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Parse(jsonParser parser, ref valueType value)
            {
                if (DefaultParser == null)
                {
                    if (isValueType) ParseValue(parser, ref value);
                    else parseClass(parser, ref value);
                }
                else DefaultParser(parser, ref value);
            }
            /// <summary>
            /// 值类型对象解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void ParseValue(jsonParser parser, ref valueType value)
            {
                if (parser.searchObject()) ParseMembers(parser, ref value);
                else value = default(valueType);
            }
            /// <summary>
            /// 引用类型对象解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void ParseClass(jsonParser parser, ref valueType value)
            {
                if (DefaultParser == null) parseClass(parser, ref value);
                else DefaultParser(parser, ref value);
            }
            /// <summary>
            /// 引用类型对象解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void parseClass(jsonParser parser, ref valueType value)
            {
                if (parser.searchObject())
                {
                    if (value == null) value = fastCSharp.emit.constructor<valueType>.New();
                    ParseMembers(parser, ref value);
                }
                else value = default(valueType);
            }
            /// <summary>
            /// 数据成员解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            internal static void ParseMembers(jsonParser parser, ref valueType value)
            {
                StateSearcherStruct searcher = new StateSearcherStruct(parser, memberSearcher);
                if (parser.isFirstObject())
                {
                    ConfigPlus config = parser.parseConfig;
                    memberMap memberMap = config.MemberMap;
                    bool isQuote = false;
                    int index = searcher.SearchName(ref isQuote);
                    if (memberMap == null)
                    {
                        if (index != -1)
                        {
                            if (parser.searchColon() == 0) return;
                            memberParsers[index].Call(parser, ref value);
                        }
                        else
                        {
                            if (isQuote) parser.searchStringEnd();
                            else parser.searchNameEnd();
                            if (parser.state != ParseStateEnum.Success || parser.searchColon() == 0) return;
                            parser.ignore();
                        }
                        while (parser.state == ParseStateEnum.Success && parser.isNextObject())
                        {
                            if ((index = searcher.SearchName(ref isQuote)) != -1)
                            {
                                if (parser.searchColon() == 0) return;
                                memberParsers[index].Call(parser, ref value);
                            }
                            else
                            {
                                if (isQuote) parser.searchStringEnd();
                                else parser.searchNameEnd();
                                if (parser.state != ParseStateEnum.Success || parser.searchColon() == 0) return;
                                parser.ignore();
                            }
                        }
                    }
                    else if (memberMap.Type == memberMap<valueType>.Type)
                    {
                        try
                        {
                            memberMap.Empty();
                            config.MemberMap = null;
                            if (index != -1)
                            {
                                if (parser.searchColon() == 0) return;
                                memberParsers[index].Call(parser, memberMap, ref value);
                            }
                            else
                            {
                                if (isQuote) parser.searchStringEnd();
                                else parser.searchNameEnd();
                                if (parser.state != ParseStateEnum.Success || parser.searchColon() == 0) return;
                                parser.ignore();
                            }
                            while (parser.state == ParseStateEnum.Success && parser.isNextObject())
                            {
                                if ((index = searcher.SearchName(ref isQuote)) != -1)
                                {
                                    if (parser.searchColon() == 0) return;
                                    memberParsers[index].Call(parser, memberMap, ref value);
                                }
                                else
                                {
                                    if (isQuote) parser.searchStringEnd();
                                    else parser.searchNameEnd();
                                    if (parser.state != ParseStateEnum.Success || parser.searchColon() == 0) return;
                                    parser.ignore();
                                }
                            }
                        }
                        finally { config.MemberMap = memberMap; }
                    }
                    else parser.state = ParseStateEnum.MemberMap;
                }
            }
            /// <summary>
            /// 不支持多维数组
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void arrayManyRank(jsonParser parser, ref valueType value)
            {
                parser.state = ParseStateEnum.ArrayManyRank;
            }
            /// <summary>
            /// 数组解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Array(jsonParser parser, ref valueType[] values)
            {
                int count = ArrayIndex(parser, ref values);
                if (count != -1 && count != values.Length) System.Array.Resize(ref values, count);
            }
            /// <summary>
            /// 数组解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            /// <returns>数据数量,-1表示失败</returns>
            internal static int ArrayIndex(jsonParser parser, ref valueType[] values)
            {
                parser.searchArray(ref values);
                if (parser.state != ParseStateEnum.Success || values == null) return -1;
                int index = 0;
                if (parser.isFirstArrayValue())
                {
                    do
                    {
                        if (index == values.Length)
                        {
                            valueType value = default(valueType);
                            Parse(parser, ref value);
                            if (parser.state != ParseStateEnum.Success) return -1;
                            valueType[] newValues = new valueType[index == 0 ? sizeof(int) : (index << 1)];
                            values.CopyTo(newValues, 0);
                            newValues[index++] = value;
                            values = newValues;
                        }
                        else
                        {
                            Parse(parser, ref values[index]);
                            if (parser.state != ParseStateEnum.Success) return -1;
                            ++index;
                        }
                    }
                    while (parser.isNextArrayValue());
                }
                return parser.state == ParseStateEnum.Success ? index : -1;
            }
            /// <summary>
            /// 忽略数据
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void ignore(jsonParser parser, ref valueType value)
            {
                parser.ignore();
            }
            /// <summary>
            /// 找不到构造函数
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void noConstructor(jsonParser parser, ref valueType value)
            {
                parser.checkNoConstructor(ref value);
            }
            /// <summary>
            /// 获取字段成员集合
            /// </summary>
            /// <returns>字段成员集合</returns>
            public static subArray<memberIndex> GetMembers()
            {
                if (memberParsers == null) return default(subArray<memberIndex>);
                return typeParser.GetMembers(memberIndexGroup<valueType>.GetFields(attribute.MemberFilter), memberIndexGroup<valueType>.GetProperties(attribute.MemberFilter), attribute);
            }
            /// <summary>
            /// 创建成员解析集合
            /// </summary>
            private static void createMembers()
            {
                Type type = typeof(valueType);
                fieldIndex defaultMember = null;
                subArray<fieldIndex> fields = typeParser.GetFields(memberIndexGroup<valueType>.GetFields(attribute.MemberFilter), attribute, ref defaultMember);
                subArray<keyValue<propertyIndex, MethodInfo>> properties = typeParser.GetProperties(memberIndexGroup<valueType>.GetProperties(attribute.MemberFilter), attribute);
                tryParseFilter[] parsers = new tryParseFilter[fields.Count + properties.Count + (defaultMember == null ? 0 : 1)];
                memberMap.type memberMapType = memberMap<valueType>.Type;
                string[] names = new string[parsers.Length];
                int index = 0;
                foreach (fieldIndex member in fields)
                {
                    tryParseFilter tryParse = parsers[index] = new tryParseFilter
                    {
                        TryParse = (tryParse)typeParser.CreateDynamicMethod(type, member.Member).CreateDelegate(typeof(tryParse)),
                        MemberMapIndex = member.MemberIndex,
                        Filter = member.Member.IsPublic ? code.memberFilters.PublicInstanceField : code.memberFilters.NonPublicInstanceField
                    };
                    names[index++] = member.Member.Name;
                    if (member == defaultMember)
                    {
                        parsers[index] = tryParse;
                        names[index++] = string.Empty;
                    }
                }
                foreach (keyValue<propertyIndex, MethodInfo> member in properties)
                {
                    parsers[index] = new tryParseFilter
                    {
                        TryParse = (tryParse)typeParser.CreateDynamicMethod(type, member.Key.Member, member.Value).CreateDelegate(typeof(tryParse)),
                        MemberMapIndex = member.Key.MemberIndex,
                        Filter = member.Value.IsPublic ? code.memberFilters.PublicInstanceProperty : code.memberFilters.NonPublicInstanceProperty
                    };
                    names[index++] = member.Key.Member.Name;
                }
                if (type.IsGenericType) memberSearcher = StateSearcherStruct.GetGenericDefinitionMember(type, names);
                else memberSearcher = fastCSharp.stateSearcher.chars.Create(names);
                memberParsers = parsers;
            }
            static typeParser()
            {
                Type type = typeof(valueType);
                MethodInfo methodInfo = jsonParser.getParseMethod(type);
                if (methodInfo != null)
                {
                    DynamicMethod dynamicMethod = new DynamicMethod("jsonParser", typeof(void), new Type[] { typeof(jsonParser), type.MakeByRefType() }, true);
                    dynamicMethod.InitLocals = true;
                    ILGenerator generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    DefaultParser = (tryParse)dynamicMethod.CreateDelegate(typeof(tryParse));
                    return;
                }
                if (type.IsArray)
                {
                    if (type.GetArrayRank() == 1) DefaultParser = (tryParse)Delegate.CreateDelegate(typeof(tryParse), typeParser.GetArrayParser(type.GetElementType()));
                    else DefaultParser = arrayManyRank;
                    return;
                }
                if (type.IsEnum)
                {
                    Type enumType = Enum.GetUnderlyingType(type);
                    if (fastCSharp.code.typeAttribute.GetAttribute<FlagsAttribute>(type, false, false) == null)
                    {
                        if (enumType == typeof(uint)) DefaultParser = enumUInt.Parse;
                        else if (enumType == typeof(byte)) DefaultParser = enumByte.Parse;
                        else if (enumType == typeof(ulong)) DefaultParser = enumULong.Parse;
                        else if (enumType == typeof(ushort)) DefaultParser = enumUShort.Parse;
                        else if (enumType == typeof(long)) DefaultParser = enumLong.Parse;
                        else if (enumType == typeof(short)) DefaultParser = enumShort.Parse;
                        else if (enumType == typeof(sbyte)) DefaultParser = enumSByte.Parse;
                        else DefaultParser = enumInt.Parse;
                    }
                    else
                    {
                        if (enumType == typeof(uint)) DefaultParser = enumUInt.ParseFlags;
                        else if (enumType == typeof(byte)) DefaultParser = enumByte.ParseFlags;
                        else if (enumType == typeof(ulong)) DefaultParser = enumULong.ParseFlags;
                        else if (enumType == typeof(ushort)) DefaultParser = enumUShort.ParseFlags;
                        else if (enumType == typeof(long)) DefaultParser = enumLong.ParseFlags;
                        else if (enumType == typeof(short)) DefaultParser = enumShort.ParseFlags;
                        else if (enumType == typeof(sbyte)) DefaultParser = enumSByte.ParseFlags;
                        else DefaultParser = enumInt.ParseFlags;
                    }
                    return;
                }
                if (type.IsPointer)
                {
                    DefaultParser = ignore;
                    return;
                }
                if (type.IsGenericType)
                {
                    Type genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(Dictionary<,>))
                    {
                        DefaultParser = (tryParse)Delegate.CreateDelegate(typeof(tryParse), typeParser.GetDictionaryParser(type));
                        return;
                    }
                    if (genericType == typeof(Nullable<>))
                    {
                        Type[] parameterTypes = type.GetGenericArguments();
                        DefaultParser = (tryParse)Delegate.CreateDelegate(typeof(tryParse), (parameterTypes[0].IsEnum ? nullableEnumParseMethod : nullableParseMethod).MakeGenericMethod(parameterTypes));
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        DefaultParser = (tryParse)Delegate.CreateDelegate(typeof(tryParse), keyValuePairParseMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                }
                if ((methodInfo = typeParser.GetCustomParser(type)) != null)
                {
                    DefaultParser = (tryParse)Delegate.CreateDelegate(typeof(tryParse), methodInfo);
                }
                else
                {
                    Type attributeType;
                    attribute = type.customAttribute<jsonParse>(out attributeType, true) ?? jsonParse.AllMember;
                    if (type.IsAbstract || type.IsInterface) DefaultParser = noConstructor;
                    else if ((methodInfo = typeParser.GetIEnumerableConstructorParser(type)) != null)
                    {
                        DefaultParser = (tryParse)Delegate.CreateDelegate(typeof(tryParse), methodInfo);
                    }
                    else if (constructor<valueType>.New == null) DefaultParser = noConstructor;
                    else
                    {
                        if (type.IsValueType) isValueType = true;
                        else if (attribute != jsonParse.AllMember && attributeType != type)
                        {
                            for (Type baseType = type.BaseType; baseType != typeof(object); baseType = baseType.BaseType)
                            {
                                jsonParse baseAttribute = fastCSharp.code.typeAttribute.GetAttribute<jsonParse>(baseType, false, true);
                                if (baseAttribute != null)
                                {
                                    if (baseAttribute.IsBaseType)
                                    {
                                        methodInfo = baseParseMethod.MakeGenericMethod(baseType, type);
                                        DefaultParser = (tryParse)Delegate.CreateDelegate(typeof(tryParse), methodInfo);
                                        return;
                                    }
                                    break;
                                }
                            }
                        }
                        createMembers();
                    }
                }
            }
        }
        /// <summary>
        /// 解析类型
        /// </summary>
        private sealed class parseMethod : Attribute { }
        /// <summary>
        /// 配置参数
        /// </summary>
        private ConfigPlus parseConfig;
        /// <summary>
        /// Json字符串
        /// </summary>
        private string json;
        /// <summary>
        /// 二进制缓冲区
        /// </summary>
        internal byte[] Buffer { get; private set; }
        /// <summary>
        /// Json字符串起始位置
        /// </summary>
        private char* jsonFixed;
        /// <summary>
        /// 当前解析位置
        /// </summary>
        internal char* Current;
        /// <summary>
        /// 解析结束位置
        /// </summary>
        private char* end;
        /// <summary>
        /// 解析结束位置
        /// </summary>
        internal char* End
        {
            get { return end; }
        }
        /// <summary>
        /// 最后一个字符
        /// </summary>
        private char endChar;
        /// <summary>
        /// 当前字符串引号
        /// </summary>
        private char quote;
        /// <summary>
        /// 解析状态
        /// </summary>
        private ParseStateEnum state;
        /// <summary>
        /// 是否以空格字符结束
        /// </summary>
        private bool isEndSpace;
        /// <summary>
        /// 是否以10进制数字字符结束
        /// </summary>
        private bool isEndDigital;
        /// <summary>
        /// 是否以16进制数字字符结束
        /// </summary>
        private bool isEndHex;
        /// <summary>
        /// 是否以数字字符结束
        /// </summary>
        private bool isEndNumber;
        /// <summary>
        /// Json解析器
        /// </summary>
        private jsonParser() { }
        /// <summary>
        /// Json解析
        /// </summary>
        /// <typeparam name="valueType">目标类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="value">目标数据</param>
        /// <param name="config">配置参数</param>
        /// <returns>解析状态</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ParseStateEnum parse<valueType>(subString json, ref valueType value, ConfigPlus config)
        {
            fixed (char* jsonFixed = (this.json = json.value))
            {
                Current = (this.jsonFixed = jsonFixed) + json.StartIndex;
                this.parseConfig = config;
                endChar = *((end = Current + json.Length) - 1);
                ParseStateEnum state = parse(ref value);
                if (state != ParseStateEnum.Success && config.IsGetJson) config.Json = json;
                return state;
            }
        }
        /// <summary>
        /// Json解析
        /// </summary>
        /// <typeparam name="valueType">目标类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="length">Json长度</param>
        /// <param name="value">目标数据</param>
        /// <param name="config">配置参数</param>
        /// <returns>解析状态</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ParseStateEnum parse<valueType>(char* json, int length, ref valueType value, ConfigPlus config, byte[] buffer)
        {
            parseConfig = config;
            Buffer = buffer;
            endChar = *((end = (jsonFixed = Current = json) + length) - 1);
            return parse(ref value);
        }
        /// <summary>
        /// Json解析
        /// </summary>
        /// <typeparam name="valueType">目标类型</typeparam>
        /// <param name="value">目标数据</param>
        /// <returns>解析状态</returns>
        private ParseStateEnum parse<valueType>(ref valueType value)
        {
            isEndSpace = endChar < SpaceMapSize && spaceMap.Get(endChar);
            isEndDigital = (uint)(endChar - '0') < 10;
            isEndHex = isEndDigital || (uint)((endChar | 0x20) - 'a') < 6;
            isEndNumber = isEndHex || (endChar < NumberMapSize && numberMap.Get(endChar));
            state = ParseStateEnum.Success;
            typeParser<valueType>.Parse(this, ref value);
            if (state == ParseStateEnum.Success)
            {
                if (Current == end || !parseConfig.IsEndSpace) return parseConfig.State = ParseStateEnum.Success;
                space();
                if (state == ParseStateEnum.Success)
                {
                    if (Current == end) return parseConfig.State = ParseStateEnum.Success;
                    state = ParseStateEnum.CrashEnd;
                }
            }
            if (parseConfig.IsGetJson)
            {
                parseConfig.State = state;
                parseConfig.CurrentIndex = (int)(Current - jsonFixed);
            }
            return state;
        }
        /// <summary>
        /// 释放Json解析器
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void free()
        {
            json = null;
            parseConfig = null;
            Buffer = null;
            typePool<jsonParser>.Push(this);
        }
        /// <summary>
        /// 设置错误解析状态
        /// </summary>
        /// <param name="state"></param>
        internal void Error(ParseStateEnum state)
        {
            this.state = state;
        }
        /// <summary>
        /// 扫描空格字符
        /// </summary>
        private void space()
        {
        SPACE:
            if (isEndSpace)
            {
                while (Current != end && *Current < SpaceMapSize && spaceMap.Get(*Current)) ++Current;
            }
            else
            {
                while (*Current < SpaceMapSize && spaceMap.Get(*Current)) ++Current;
            }
            if (Current == end || *Current != '/') return;
            if (++Current == end)
            {
                state = ParseStateEnum.UnknownNote;
                return;
            }
            if (*Current == '/')
            {
                if (endChar == '\n')
                {
                    while (*++Current != '\n') ;
                    ++Current;
                }
                else
                {
                    do
                    {
                        if (++Current == end) return;
                    }
                    while (*Current != '\n');
                }
                goto SPACE;
            }
            if (*Current != '*')
            {
                state = ParseStateEnum.UnknownNote;
                return;
            }
            if (++Current == end)
            {
                state = ParseStateEnum.NoteNotRound;
                return;
            }
            if (endChar == '*')
            {
                do
                {
                    while (*Current != '*') ++Current;
                    if (++Current == end)
                    {
                        state = ParseStateEnum.NoteNotRound;
                        return;
                    }
                    if (*Current == '/')
                    {
                        ++Current;
                        goto SPACE;
                    }
                }
                while (true);
            }
            if (endChar == '/')
            {
                do
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.NoteNotRound;
                        return;
                    }
                    while (*Current != '/') ++Current;
                    if (*(Current - 1) == '*')
                    {
                        ++Current;
                        goto SPACE;
                    }
                    if (++Current == end)
                    {
                        state = ParseStateEnum.NoteNotRound;
                        return;
                    }
                }
                while (true);
            }
            do
            {
                while (*Current != '*')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.NoteNotRound;
                        return;
                    }
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.NoteNotRound;
                    return;
                }
                if (*Current == '/')
                {
                    if (++Current == end) return;
                    goto SPACE;
                }
            }
            while (true);
        }
        /// <summary>
        /// 是否null
        /// </summary>
        /// <returns>是否null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isNull()
        {
            if (*Current == 'n')
            {
                if ((int)(end - Current) < 4 || ((*(Current + 1) ^ 'u') | (*(int*)(Current + 2) ^ ('l' + ('l' << 16)))) != 0)
                {
                    state = ParseStateEnum.NotNull;
                    return false;
                }
                Current += 4;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 是否非数字NaN
        /// </summary>
        /// <returns>是否非数字NaN</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isNaN()
        {
            if (*Current == 'N')
            {
                if ((int)(end - Current) < 3 || (*(int*)(Current + 1) ^ ('a' + ('N' << 16))) != 0)
                {
                    state = ParseStateEnum.NotNumber;
                    return false;
                }
                Current += 3;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 解析10进制数字
        /// </summary>
        /// <param name="value">第一位数字</param>
        /// <returns>数字</returns>
        private uint parseUInt32(uint value)
        {
            uint number;
            if (isEndDigital)
            {
                do
                {
                    if ((number = (uint)(*Current - '0')) > 9) return value;
                    value *= 10;
                    value += number;
                    if (++Current == end) return value;
                }
                while (true);
            }
            while ((number = (uint)(*Current - '0')) < 10)
            {
                value *= 10;
                ++Current;
                value += (byte)number;
            }
            return value;
        }
        /// <summary>
        /// 解析16进制数字
        /// </summary>
        /// <param name="value">数值</param>
        private void parseHex32(ref uint value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                {
                    state = ParseStateEnum.NotHex;
                    return;
                }
                number += 10;
            }
            value = number;
            if (++Current == end) return;
            if (isEndHex)
            {
                do
                {
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return;
                        number += 10;
                    }
                    value <<= 4;
                    value += number;
                }
                while (++Current != end);
                return;
            }
            do
            {
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return;
                    number += 10;
                }
                value <<= 4;
                ++Current;
                value += number;
            }
            while (true);
        }
        /// <summary>
        /// 逻辑值解析
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>解析状态</returns>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref bool value)
        {
            if (*Current != 'f') goto NOTFALSE;
        FALSE:
            if (((int)(end - Current)) < 5
                || ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) != 0)
            {
                state = ParseStateEnum.NotBool;
                return;
            }
            Current += 5;
            value = false;
            return;
        NOTFALSE:
            if (*Current != 't') goto NOTTRUE;
        TRUE:
            if ((int)(end - Current) < 4 || ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
            {
                state = ParseStateEnum.NotBool;
                return;
            }
            Current += 4;
            value = true;
            return;
        NOTTRUE:
            uint number = (uint)(*Current - '0');
            if (number < 2)
            {
                ++Current;
                value = number != 0;
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == 'f') goto FALSE;
            if (*Current == 't') goto TRUE;
            if ((number = (uint)(*Current - '0')) < 2)
            {
                ++Current;
                value = number != 0;
                return;
            }
            if (*Current == '"' || *Current == '\'')
            {
                quote = *Current;
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if (*Current == 'f')
                {
                    if (((int)(end - Current)) < 5
                        || ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) != 0)
                    {
                        state = ParseStateEnum.NotBool;
                        return;
                    }
                    Current += 5;
                    value = false;
                }
                else if (*Current == 't')
                {
                    if ((int)(end - Current) < 4 || ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
                    {
                        state = ParseStateEnum.NotBool;
                        return;
                    }
                    Current += 4;
                    value = true;
                }
                else if ((number = (uint)(*Current - '0')) < 2)
                {
                    ++Current;
                    value = number != 0;
                }
                else state = ParseStateEnum.NotBool;
                if (state == ParseStateEnum.Success)
                {
                    if (Current == end) state = ParseStateEnum.CrashEnd;
                    else if (*Current == quote) ++Current;
                    else state = ParseStateEnum.NotBool;
                }
                return;
            }
            state = ParseStateEnum.NotBool;
        }
        /// <summary>
        /// 逻辑值解析
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>解析状态</returns>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref bool? value)
        {
            if (*Current != 'f') goto NOTFALSE;
        FALSE:
            if (((int)(end - Current)) < 5
                || ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) != 0)
            {
                state = ParseStateEnum.NotBool;
                return;
            }
            Current += 5;
            value = false;
            return;
        NOTFALSE:
            if (*Current != 't') goto NOTTRUE;
        TRUE:
            if ((int)(end - Current) < 4 || ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
            {
                state = ParseStateEnum.NotBool;
                return;
            }
            Current += 4;
            value = true;
            return;
        NOTTRUE:
            uint number = (uint)(*Current - '0');
            if (number < 2)
            {
                ++Current;
                value = number != 0;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == 'f') goto FALSE;
            if (*Current == 't') goto TRUE;
            if ((number = (uint)(*Current - '0')) < 2)
            {
                ++Current;
                value = number != 0;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            if (*Current == '"' || *Current == '\'')
            {
                quote = *Current;
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if (*Current == 'f')
                {
                    if (((int)(end - Current)) < 5
                        || ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) != 0)
                    {
                        state = ParseStateEnum.NotBool;
                        return;
                    }
                    Current += 5;
                    value = false;
                }
                else if (*Current == 't')
                {
                    if ((int)(end - Current) < 4 || ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
                    {
                        state = ParseStateEnum.NotBool;
                        return;
                    }
                    Current += 4;
                    value = true;
                }
                else if ((number = (uint)(*Current - '0')) < 2)
                {
                    ++Current;
                    value = number != 0;
                }
                else state = ParseStateEnum.NotBool;
                if (state == ParseStateEnum.Success)
                {
                    if (Current == end) state = ParseStateEnum.CrashEnd;
                    else if (*Current == quote) ++Current;
                    else state = ParseStateEnum.NotBool;
                }
                return;
            }
            state = ParseStateEnum.NotBool;
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref byte value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                space();
                if (state != ParseStateEnum.Success) return;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = ParseStateEnum.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = (byte)number;
                        }
                        else value = (byte)parseUInt32(number);
                        if (state == ParseStateEnum.Success)
                        {
                            if (Current == end) state = ParseStateEnum.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = ParseStateEnum.NotNumber;
                        }
                        return;
                    }
                    state = ParseStateEnum.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = (byte)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = (byte)number;
                return;
            }
            value = (byte)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref byte? value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                space();
                if (state != ParseStateEnum.Success) return;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (isNull())
                    {
                        value = null;
                        return;
                    }
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = ParseStateEnum.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = (byte)number;
                        }
                        else value = (byte)parseUInt32(number);
                        if (state == ParseStateEnum.Success)
                        {
                            if (Current == end) state = ParseStateEnum.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = ParseStateEnum.NotNumber;
                        }
                        return;
                    }
                    state = ParseStateEnum.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = (byte)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = (byte)number;
                return;
            }
            value = (byte)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref sbyte value)
        {
            int sign = 0;
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = ParseStateEnum.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                            }
                            else value = sign == 0 ? (sbyte)(byte)parseUInt32(number) : (sbyte)-(int)parseUInt32(number);
                            if (state == ParseStateEnum.Success)
                            {
                                if (Current == end) state = ParseStateEnum.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = ParseStateEnum.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                return;
            }
            value = sign == 0 ? (sbyte)(byte)parseUInt32(number) : (sbyte)-(int)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref sbyte? value)
        {
            int sign = 0;
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = ParseStateEnum.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (isNull())
                        {
                            value = null;
                            return;
                        }
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                            }
                            else value = sign == 0 ? (sbyte)(byte)parseUInt32(number) : (sbyte)-(int)parseUInt32(number);
                            if (state == ParseStateEnum.Success)
                            {
                                if (Current == end) state = ParseStateEnum.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = ParseStateEnum.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                return;
            }
            value = sign == 0 ? (sbyte)(byte)parseUInt32(number) : (sbyte)-(int)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref ushort value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                space();
                if (state != ParseStateEnum.Success) return;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = ParseStateEnum.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = (ushort)number;
                        }
                        else value = (ushort)parseUInt32(number);
                        if (state == ParseStateEnum.Success)
                        {
                            if (Current == end) state = ParseStateEnum.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = ParseStateEnum.NotNumber;
                        }
                        return;
                    }
                    state = ParseStateEnum.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = (ushort)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = (ushort)number;
                return;
            }
            value = (ushort)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref ushort? value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                space();
                if (state != ParseStateEnum.Success) return;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (isNull())
                    {
                        value = null;
                        return;
                    }
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = ParseStateEnum.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = (ushort)number;
                        }
                        else value = (ushort)parseUInt32(number);
                        if (state == ParseStateEnum.Success)
                        {
                            if (Current == end) state = ParseStateEnum.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = ParseStateEnum.NotNumber;
                        }
                        return;
                    }
                    state = ParseStateEnum.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = (ushort)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = (ushort)number;
                return;
            }
            value = (ushort)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref short value)
        {
            int sign = 0;
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = ParseStateEnum.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                            }
                            else value = sign == 0 ? (short)(ushort)parseUInt32(number) : (short)-(int)parseUInt32(number);
                            if (state == ParseStateEnum.Success)
                            {
                                if (Current == end) state = ParseStateEnum.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = ParseStateEnum.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                return;
            }
            value = sign == 0 ? (short)(ushort)parseUInt32(number) : (short)-(int)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref short? value)
        {
            int sign = 0;
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = ParseStateEnum.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (isNull())
                        {
                            value = null;
                            return;
                        }
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                            }
                            else value = sign == 0 ? (short)(ushort)parseUInt32(number) : (short)-(int)parseUInt32(number);
                            if (state == ParseStateEnum.Success)
                            {
                                if (Current == end) state = ParseStateEnum.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = ParseStateEnum.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                return;
            }
            value = sign == 0 ? (short)(ushort)parseUInt32(number) : (short)-(int)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref uint value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                space();
                if (state != ParseStateEnum.Success) return;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = ParseStateEnum.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = number;
                        }
                        else value = parseUInt32(number);
                        if (state == ParseStateEnum.Success)
                        {
                            if (Current == end) state = ParseStateEnum.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = ParseStateEnum.NotNumber;
                        }
                        return;
                    }
                    state = ParseStateEnum.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = number;
                return;
            }
            value = parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref uint? value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                space();
                if (state != ParseStateEnum.Success) return;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (isNull())
                    {
                        value = null;
                        return;
                    }
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = ParseStateEnum.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = number;
                        }
                        else value = parseUInt32(number);
                        if (state == ParseStateEnum.Success)
                        {
                            if (Current == end) state = ParseStateEnum.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = ParseStateEnum.NotNumber;
                        }
                        return;
                    }
                    state = ParseStateEnum.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = number;
                return;
            }
            value = parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref int value)
        {
            int sign = 0;
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = ParseStateEnum.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (int)number : -(int)number;
                            }
                            else value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
                            if (state == ParseStateEnum.Success)
                            {
                                if (Current == end) state = ParseStateEnum.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = ParseStateEnum.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (int)number : -(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (int)number : -(int)number;
                return;
            }
            value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref int? value)
        {
            int sign = 0;
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = ParseStateEnum.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (isNull())
                        {
                            value = null;
                            return;
                        }
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (int)number : -(int)number;
                            }
                            else value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
                            if (state == ParseStateEnum.Success)
                            {
                                if (Current == end) state = ParseStateEnum.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = ParseStateEnum.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (int)number : -(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (int)number : -(int)number;
                return;
            }
            value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
        }
        /// <summary>
        /// 解析10进制数字
        /// </summary>
        /// <param name="value">第一位数字</param>
        /// <returns>数字</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong parseUInt64(uint value)
        {
            char* end32 = Current + 8;
            if (end32 > end) end32 = end;
            uint number;
            do
            {
                if ((number = (uint)(*Current - '0')) > 9) return value;
                value *= 10;
                value += number;
            }
            while (++Current != end32);
            if (Current == end) return value;
            ulong value64 = value;
            if (isEndDigital)
            {
                do
                {
                    if ((number = (uint)(*Current - '0')) > 9) return value64;
                    value64 *= 10;
                    value64 += number;
                    if (++Current == end) return value64;
                }
                while (true);
            }
            while ((number = (uint)(*Current - '0')) < 10)
            {
                value64 *= 10;
                ++Current;
                value64 += (byte)number;
            }
            return value64;
        }
        /// <summary>
        /// 解析16进制数字
        /// </summary>
        /// <returns>数字</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong parseHex64()
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                {
                    state = ParseStateEnum.NotHex;
                    return 0;
                }
                number += 10;
            }
            if (++Current == end) return number;
            uint high = number;
            char* end32 = Current + 7;
            if (end32 > end) end32 = end;
            do
            {
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return high;
                    number += 10;
                }
                high <<= 4;
                high += number;
            }
            while (++Current != end32);
            if (Current == end) return high;
            char* start = Current;
            ulong low = number;
            if (isEndHex)
            {
                do
                {
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                        {
                            return low | (ulong)high << ((int)((byte*)Current - (byte*)start) << 1);
                        }
                        number += 10;
                    }
                    low <<= 4;
                    low += number;
                }
                while (++Current != end);
                return low | (ulong)high << ((int)((byte*)Current - (byte*)start) << 1);
            }
            do
            {
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                    {
                        return low | (ulong)high << ((int)((byte*)Current - (byte*)start) << 1);
                    }
                    number += 10;
                }
                low <<= 4;
                ++Current;
                low += number;
            }
            while (true);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref ulong value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                space();
                if (state != ParseStateEnum.Success) return;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = ParseStateEnum.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            value = parseHex64();
                        }
                        else value = parseUInt64(number);
                        if (state == ParseStateEnum.Success)
                        {
                            if (Current == end) state = ParseStateEnum.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = ParseStateEnum.NotNumber;
                        }
                        return;
                    }
                    state = ParseStateEnum.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                value = parseHex64();
                return;
            }
            value = parseUInt64(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref ulong? value)
        {
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                space();
                if (state != ParseStateEnum.Success) return;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (isNull())
                    {
                        value = null;
                        return;
                    }
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = ParseStateEnum.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            value = parseHex64();
                        }
                        else value = parseUInt64(number);
                        if (state == ParseStateEnum.Success)
                        {
                            if (Current == end) state = ParseStateEnum.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = ParseStateEnum.NotNumber;
                        }
                        return;
                    }
                    state = ParseStateEnum.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                value = parseHex64();
                return;
            }
            value = parseUInt64(number);
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref long value)
        {
            int sign = 0;
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = ParseStateEnum.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                value = (long)parseHex64();
                            }
                            else value = (long)parseUInt64(number);
                            if (state == ParseStateEnum.Success)
                            {
                                if (Current == end) state = ParseStateEnum.CrashEnd;
                                else if (*Current == quote)
                                {
                                    if (sign != 0) value = -value;
                                    ++Current;
                                }
                                else state = ParseStateEnum.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (long)(int)number : -(long)(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                value = (long)parseHex64();
                if (sign != 0) value = -value;
                return;
            }
            value = (long)parseUInt64(number);
            if (sign != 0) value = -value;
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref long? value)
        {
            int sign = 0;
            uint number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = ParseStateEnum.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (isNull())
                        {
                            value = null;
                            return;
                        }
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = ParseStateEnum.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = ParseStateEnum.CrashEnd;
                                    return;
                                }
                                value = (long)parseHex64();
                            }
                            else value = (long)parseUInt64(number);
                            if (state == ParseStateEnum.Success)
                            {
                                if (Current == end) state = ParseStateEnum.CrashEnd;
                                else if (*Current == quote)
                                {
                                    if (sign != 0) value = -value;
                                    ++Current;
                                }
                                else state = ParseStateEnum.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = ParseStateEnum.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (long)(int)number : -(long)(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                long hexValue = (long)parseHex64();
                value = sign == 0 ? hexValue : -hexValue;
                return;
            }
            long value64 = (long)parseUInt64(number);
            value = sign == 0 ? value64 : -value64;
        }
        /// <summary>
        /// 查找数字结束位置
        /// </summary>
        /// <returns>数字结束位置,失败返回null</returns>
        private char* searchNumber()
        {
            if (*Current >= NumberMapSize || !numberMap.Get(*Current))
            {
                space();
                if (state != ParseStateEnum.Success) return null;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return null;
                }
                if (*Current == '"' || *Current == '\'')
                {
                    quote = *Current;
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return null;
                    }
                    char* stringEnd = Current;
                    if (endChar == quote)
                    {
                        while (*stringEnd != quote) ++stringEnd;
                    }
                    else
                    {
                        while (*stringEnd != quote)
                        {
                            if (++stringEnd == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return null;
                            }
                        }
                    }
                    return stringEnd;
                }
                if (*Current >= NumberMapSize || !numberMap.Get(*Current))
                {
                    if (isNaN()) return jsonFixed;
                    state = ParseStateEnum.NotNumber;
                    return null;
                }
            }
            char* numberEnd = Current;
            if (isEndNumber)
            {
                while (++numberEnd != end && *numberEnd < NumberMapSize && numberMap.Get(*numberEnd)) ;
            }
            else
            {
                while (*++numberEnd < NumberMapSize && numberMap.Get(*numberEnd)) ;
            }
            quote = (char)0;
            return numberEnd;
        }
        /// <summary>
        /// 查找数字结束位置
        /// </summary>
        /// <returns>数字结束位置,失败返回null</returns>
        private char* searchNumberNull()
        {
            if (*Current >= NumberMapSize || !numberMap.Get(*Current))
            {
                if (isNull()) return jsonFixed;
                space();
                if (state != ParseStateEnum.Success) return null;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return null;
                }
                if (*Current == '"' || *Current == '\'')
                {
                    quote = *Current;
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return null;
                    }
                    char* stringEnd = Current;
                    if (endChar == quote)
                    {
                        while (*stringEnd != quote) ++stringEnd;
                    }
                    else
                    {
                        while (*stringEnd != quote)
                        {
                            if (++stringEnd == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return null;
                            }
                        }
                    }
                    return stringEnd;
                }
                if (*Current >= NumberMapSize || !numberMap.Get(*Current))
                {
                    if (isNull() || isNaN()) return jsonFixed;
                    state = ParseStateEnum.NotNumber;
                    return null;
                }
            }
            char* numberEnd = Current;
            if (isEndNumber)
            {
                while (++numberEnd != end && *numberEnd < NumberMapSize && numberMap.Get(*numberEnd)) ;
            }
            else
            {
                while (*++numberEnd < NumberMapSize && numberMap.Get(*numberEnd)) ;
            }
            quote = (char)0;
            return numberEnd;
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref float value)
        {
            char* end = searchNumber();
            if (end != null)
            {
                if (end == jsonFixed) value = float.NaN;
                else
                {
                    string number = this.json == null ? new string(Current, 0, (int)(end - Current)) : this.json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    if (float.TryParse(number, out value))
                    {
                        Current = end;
                        if (quote != 0) ++Current;
                    }
                    else state = ParseStateEnum.NotNumber;
                }
            }
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref float? value)
        {
            char* end = searchNumberNull();
            if (end != null)
            {
                if (end == jsonFixed)
                {
                    if (*(Current - 1) == 'l') value = null;
                    else value = float.NaN;
                }
                else
                {
                    string number = this.json == null ? new string(Current, 0, (int)(end - Current)) : this.json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    float parseValue;
                    if (float.TryParse(number, out parseValue))
                    {
                        Current = end;
                        value = parseValue;
                        if (quote != 0) ++Current;
                    }
                    else state = ParseStateEnum.NotNumber;
                }
            }
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref double value)
        {
            char* end = searchNumber();
            if (end != null)
            {
                if (end == jsonFixed) value = double.NaN;
                else
                {
                    string number = this.json == null ? new string(Current, 0, (int)(end - Current)) : this.json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    if (double.TryParse(number, out value))
                    {
                        Current = end;
                        if (quote != 0) ++Current;
                    }
                    else state = ParseStateEnum.NotNumber;
                }
            }
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref double? value)
        {
            char* end = searchNumberNull();
            if (end != null)
            {
                if (end == jsonFixed)
                {
                    if (*(Current - 1) == 'l') value = null;
                    else value = double.NaN;
                }
                else
                {
                    string number = this.json == null ? new string(Current, 0, (int)(end - Current)) : this.json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    double parseValue;
                    if (double.TryParse(number, out parseValue))
                    {
                        Current = end;
                        value = parseValue;
                        if (quote != 0) ++Current;
                    }
                    else state = ParseStateEnum.NotNumber;
                }
            }
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref decimal value)
        {
            char* end = searchNumber();
            if (end != null)
            {
                if (end == jsonFixed) state = ParseStateEnum.NotNumber;
                else
                {
                    string number = this.json == null ? new string(Current, 0, (int)(end - Current)) : this.json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    if (decimal.TryParse(number, out value))
                    {
                        Current = end;
                        if (quote != 0) ++Current;
                    }
                    else state = ParseStateEnum.NotNumber;
                }
            }
        }
        /// <summary>
        /// 数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref decimal? value)
        {
            char* end = searchNumberNull();
            if (end != null)
            {
                if (end == jsonFixed)
                {
                    if (*(Current - 1) == 'l') value = null;
                    else state = ParseStateEnum.NotNumber;
                }
                else
                {
                    string number = this.json == null ? new string(Current, 0, (int)(end - Current)) : this.json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    decimal parseValue;
                    if (decimal.TryParse(number, out parseValue))
                    {
                        Current = end;
                        value = parseValue;
                        if (quote != 0) ++Current;
                    }
                    else state = ParseStateEnum.NotNumber;
                }
            }
        }
        /// <summary>
        /// 字符解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref char value)
        {
            if (*Current != '"') goto NOTQUOTES;
        QUOTES:
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if ((value = *Current) == '\\')
            {
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                value = *Current == 'n' ? '\n' : (*Current == 'r' ? '\r' : *Current);
            }
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '"')
            {
                ++Current;
                return;
            }
            state = ParseStateEnum.NotChar;
            return;
        NOTQUOTES:
            if (*Current != '\'') goto NOTQUOTE;
        QUOTE:
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if ((value = *Current) == '\\')
            {
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                value = *Current == 'n' ? '\n' : (*Current == 'r' ? '\r' : *Current);
            }
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '\'')
            {
                ++Current;
                return;
            }
        NOTQUOTE:
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '"') goto QUOTES;
            if (*Current == '\'') goto QUOTE;
            state = ParseStateEnum.NotChar;
        }
        /// <summary>
        /// 字符解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref char? value)
        {
            if (*Current != '"') goto NOTQUOTES;
        QUOTES:
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '\\')
            {
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                value = *Current == 'n' ? '\n' : (*Current == 'r' ? '\r' : *Current);
            }
            else value = *Current;
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '"')
            {
                ++Current;
                return;
            }
            state = ParseStateEnum.NotChar;
            return;
        NOTQUOTES:
            if (*Current != '\'') goto NOTQUOTE;
        QUOTE:
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '\\')
            {
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                value = *Current == 'n' ? '\n' : (*Current == 'r' ? '\r' : *Current);
            }
            else value = *Current;
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '\'')
            {
                ++Current;
                return;
            }
        NOTQUOTE:
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '"') goto QUOTES;
            if (*Current == '\'') goto QUOTE;
            if (isNull())
            {
                value = null;
                return;
            }
            state = ParseStateEnum.NotChar;
        }
        /// <summary>
        /// 时间解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref DateTime value)
        {
            if (*Current != 'n') goto NOTDATETIME;
        DATETIME:
            int count = (int)(end - Current);
            if (count > 9 && *(int*)(Current + 1) == ('e' + ('w' << 16)))
            {
                if (((*(int*)(Current + 3) ^ (' ' + ('D' << 16))) | (*(int*)(Current + 5) ^ ('a' + ('t' << 16))) | (*(int*)(Current + 7) ^ ('e' + ('(' << 16)))) == 0)
                {
                    long millisecond = 0;
                    Current += 9;
                    Parse(ref millisecond);
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if (*Current == ')')
                    {
                        value = fastCSharp.config.appSetting.JavascriptMinTime.AddTicks(millisecond * date.MillisecondTicks);
                        ++Current;
                        return;
                    }
                }
            }
            else if (count >= 4 && ((*(Current + 1) ^ 'u') | (*(int*)(Current + 2) ^ ('l' + ('l' << 16)))) == 0)
            {
                value = DateTime.MinValue;
                Current += 4;
                return;
            }
            state = ParseStateEnum.NotDateTime;
            return;
        NOTDATETIME:
            if (*Current == '\'' || *Current == '"') goto STRING;
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == 'n') goto DATETIME;
            if (*Current != '\'' && *Current != '"')
            {
                state = ParseStateEnum.NotDateTime;
                return;
            }
        STRING:
            string timeString = parseString();
            if (timeString != null)
            {
                DateTime parseTime;
                if (DateTime.TryParse(timeString, out parseTime))
                {
                    value = parseTime;
                    return;
                }
            }
            state = ParseStateEnum.NotDateTime;
        }
        /// <summary>
        /// 时间解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref DateTime? value)
        {
            if (*Current != 'n') goto NOTDATETIME;
        DATETIME:
            int count = (int)(end - Current);
            if (count > 9 && *(int*)(Current + 1) == ('e' + ('w' << 16)))
            {
                if (((*(int*)(Current + 3) ^ (' ' + ('D' << 16))) | (*(int*)(Current + 5) ^ ('a' + ('t' << 16))) | (*(int*)(Current + 7) ^ ('e' + ('(' << 16)))) == 0)
                {
                    long millisecond = 0;
                    Current += 9;
                    Parse(ref millisecond);
                    if (state != ParseStateEnum.Success) return;
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if (*Current == ')')
                    {
                        value = fastCSharp.config.appSetting.JavascriptMinTime.AddTicks(millisecond * date.MillisecondTicks);
                        ++Current;
                        return;
                    }
                }
            }
            else if (count >= 4 && ((*(Current + 1) ^ 'u') | (*(int*)(Current + 2) ^ ('l' + ('l' << 16)))) == 0)
            {
                value = null;
                Current += 4;
                return;
            }
            state = ParseStateEnum.NotDateTime;
            return;
        NOTDATETIME:
            if (*Current == '\'' || *Current == '"') goto STRING;
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == 'n') goto DATETIME;
            if (*Current != '\'' && *Current != '"')
            {
                state = ParseStateEnum.NotDateTime;
                return;
            }
        STRING:
            string timeString = parseString();
            if (timeString != null)
            {
                DateTime parseTime;
                if (DateTime.TryParse(timeString, out parseTime))
                {
                    value = parseTime;
                    return;
                }
            }
            state = ParseStateEnum.NotDateTime;
        }
        /// <summary>
        /// Guid解析
        /// </summary>
        /// <param name="value">数据</param>
        private void parse(ref Guid value)
        {
            if (end - Current < 38)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            quote = *Current;
            guid guid = new guid();
            guid.Byte3 = (byte)parseHex2();
            guid.Byte2 = (byte)parseHex2();
            guid.Byte1 = (byte)parseHex2();
            guid.Byte0 = (byte)parseHex2();
            if (*++Current != '-')
            {
                state = ParseStateEnum.NotGuid;
                return;
            }
            guid.Byte45 = (ushort)parseHex4();
            if (*++Current != '-')
            {
                state = ParseStateEnum.NotGuid;
                return;
            }
            guid.Byte67 = (ushort)parseHex4();
            if (*++Current != '-')
            {
                state = ParseStateEnum.NotGuid;
                return;
            }
            guid.Byte8 = (byte)parseHex2();
            guid.Byte9 = (byte)parseHex2();
            if (*++Current != '-')
            {
                state = ParseStateEnum.NotGuid;
                return;
            }
            guid.Byte10 = (byte)parseHex2();
            guid.Byte11 = (byte)parseHex2();
            guid.Byte12 = (byte)parseHex2();
            guid.Byte13 = (byte)parseHex2();
            guid.Byte14 = (byte)parseHex2();
            guid.Byte15 = (byte)parseHex2();
            if (*++Current == quote)
            {
                value = guid.Value;
                ++Current;
                return;
            }
            state = ParseStateEnum.NotGuid;
        }
        /// <summary>
        /// Guid解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref Guid value)
        {
            if (*Current == '\'' || *Current == '"')
            {
                parse(ref value);
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '\'' || *Current == '"')
            {
                parse(ref value);
                return;
            }
            state = ParseStateEnum.NotGuid;
        }
        /// <summary>
        /// Guid解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref Guid? value)
        {
            if (*Current == '\'' || *Current == '"')
            {
                Guid guid = new Guid();
                parse(ref guid);
                value = guid;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '\'' || *Current == '"')
            {
                Guid guid = new Guid();
                parse(ref guid);
                value = guid;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            state = ParseStateEnum.NotGuid;
        }
        /// <summary>
        /// 查找字符串中的转义符
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void searchEscape()
        {
            if (endChar == quote)
            {
                while (*Current != quote && *Current != '\\')
                {
                    if (*Current == '\n')
                    {
                        state = ParseStateEnum.StringEnter;
                        return;
                    }
                    ++Current;
                }
            }
            else
            {
                while (*Current != quote && *Current != '\\')
                {
                    if (*Current == '\n')
                    {
                        state = ParseStateEnum.StringEnter;
                        return;
                    }
                    if (++Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// 解析16进制字符
        /// </summary>
        /// <returns>字符</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint parseHex4()
        {
            uint code = (uint)(*++Current - '0'), number = (uint)(*++Current - '0');
            if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
            if (number > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
            code <<= 12;
            code += (number << 8);
            if ((number = (uint)(*++Current - '0')) > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
            code += (number << 4);
            number = (uint)(*++Current - '0');
            return code + (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number);
        }
        /// <summary>
        /// 解析16进制字符
        /// </summary>
        /// <returns>字符</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint parseHex2()
        {
            uint code = (uint)(*++Current - '0'), number = (uint)(*++Current - '0');
            if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
            return (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number) + (code << 4);
        }
        /// <summary>
        /// 字符串转义解析
        /// </summary>
        /// <returns>写入结束位置</returns>
        private char* parseEscape()
        {
            char* write = Current;
            do
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return null;
                    }
                    *write++ = (char)parseHex4();
                }
                else if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return null;
                    }
                    *write++ = (char)parseHex2();
                }
                else
                {
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return null;
                    }
                    *write++ = *Current < escapeCharSize ? escapeCharData.Char[*Current] : *Current;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return null;
                }
                if (endChar == quote)
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = ParseStateEnum.StringEnter;
                            return null;
                        }
                        *write++ = *Current++;
                    }
                }
                else
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = ParseStateEnum.StringEnter;
                            return null;
                        }
                        *write++ = *Current++;
                        if (Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return null;
                        }
                    }
                }
                if (*Current == quote)
                {
                    ++Current;
                    return write;
                }
            }
            while (true);
        }
        /// <summary>
        /// 获取转义后的字符串长度
        /// </summary>
        /// <returns>字符串长度</returns>
        private int parseEscapeSize()
        {
            char* start = Current;
            int length = 0;
            do
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return 0;
                    }
                    length += 5;
                    Current += 5;
                }
                else if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return 0;
                    }
                    length += 3;
                    Current += 3;
                }
                else
                {
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return 0;
                    }
                    ++length;
                    ++Current;
                }
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return 0;
                }
                if (endChar == quote)
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = ParseStateEnum.StringEnter;
                            return 0;
                        }
                        ++Current;
                    }
                }
                else
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = ParseStateEnum.StringEnter;
                            return 0;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return 0;
                        }
                    }
                }
                if (*Current == quote)
                {
                    length = (int)(Current - start) - length;
                    Current = start;
                    return length;
                }
            }
            while (true);
        }
        /// <summary>
        /// 字符串转义解析
        /// </summary>
        /// <param name="write">当前写入位置</param>
        private void parseEscapeUnsafe(char* write)
        {
            do
            {
                if (*++Current == 'u') *write++ = (char)parseHex4();
                else if (*Current == 'x') *write++ = (char)parseHex2();
                else *write++ = *Current < escapeCharSize ? escapeCharData.Char[*Current] : *Current;
                while (*++Current != quote && *Current != '\\') *write++ = *Current;
                if (*Current == quote)
                {
                    ++Current;
                    return;
                }
            }
            while (true);
        }
        /// <summary>
        /// 字符串解析
        /// </summary>
        /// <returns>字符串,失败返回null</returns>
        private string parseString()
        {
            quote = *Current;
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return null;
            }
            char* start = Current;
            searchEscape();
            if (state != ParseStateEnum.Success) return null;
            if (*Current == quote) return new string(start, 0, (int)(Current++ - start));
            if (parseConfig.IsTempString)
            {
                char* writeEnd = parseEscape();
                return writeEnd != null ? new string(start, 0, (int)(writeEnd - start)) : null;
            }
            return parseEscape(start);
        }
        /// <summary>
        /// 字符串解析
        /// </summary>
        /// <param name="start"></param>
        /// <returns>字符串,失败返回null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string parseEscape(char* start)
        {
            int size = parseEscapeSize();
            if (size != 0)
            {
                int left = (int)(Current - start);
                string value = fastCSharp.String.FastAllocateString(left + size);
                fixed (char* valueFixed = value)
                {
                    fastCSharp.unsafer.memory.Copy((void*)start, valueFixed, left << 1);
                    parseEscapeUnsafe(valueFixed + left);
                    return value;
                }
            }
            return null;
        }
        /// <summary>
        /// 查找枚举数字
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isEnumNumber()
        {
            if ((uint)(*Current - '0') < 10) return true;
            space();
            if (state != ParseStateEnum.Success) return false;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            return (uint)(*Current - '0') < 10;
        }
        /// <summary>
        /// 查找枚举数字
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isEnumNumberFlag()
        {
            if ((uint)(*Current - '0') < 10 || *Current == '-') return true;
            space();
            if (state != ParseStateEnum.Success) return false;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            return (uint)(*Current - '0') < 10 || *Current == '-';
        }
        /// <summary>
        /// 查找字符串引号并返回第一个字符
        /// </summary>
        /// <returns>第一个字符,0表示null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char searchQuote()
        {
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextStringChar();
            }
            if (isNull()) return quote = (char)0;
            space();
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return (char)0;
            }
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextStringChar();
            }
            if (isNull()) return quote = (char)0;
            state = ParseStateEnum.NotString;
            return (char)0;
        }
        /// <summary>
        /// 读取下一个字符
        /// </summary>
        /// <returns>字符,结束或者错误返回0</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char nextStringChar()
        {
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return (char)0;
            }
            if (*Current == quote)
            {
                ++Current;
                return quote = (char)0;
            }
            if (*Current == '\\')
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return (char)0;
                    }
                    return (char)parseHex4();
                }
                if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return (char)0;
                    }
                    return (char)parseHex2();
                }
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return (char)0;
                }
                return *Current < escapeCharSize ? escapeCharData.Char[*Current] : *Current;
            }
            if (*Current == '\n')
            {
                state = ParseStateEnum.StringEnter;
                return (char)0;
            }
            return *Current;
        }
        ///// <summary>
        ///// 判断是否字符串结束引号
        ///// </summary>
        ///// <returns>是否字符串结束</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private bool isNextStringQuote()
        //{
        //    if (++current == end)
        //    {
        //        state = parseState.CrashEnd;
        //        return false;
        //    }
        //    if (*current == quote)
        //    {
        //        ++current;
        //        quote = (char)0;
        //        return true;
        //    }
        //    if (*current == '\\')
        //    {
        //        if (*++current == 'u')
        //        {
        //            if ((int)(end - current) < 5) state = parseState.CrashEnd;
        //            else current += 4;
        //        }
        //        else if (*current == 'x')
        //        {
        //            if ((int)(end - current) < 3) state = parseState.CrashEnd;
        //            else current += 2;
        //        }
        //        else if (current == end) state = parseState.CrashEnd;
        //        else ++current;
        //    }
        //    else if (*current == '\n') state = parseState.StringEnter;
        //    return false;
        //}
        /// <summary>
        /// 查找字符串直到结束
        /// </summary>
        private void searchStringEnd()
        {
            if (quote != 0 && state == ParseStateEnum.Success)
            {
                ++Current;
                do
                {
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    if (endChar == quote)
                    {
                        while (*Current != quote && *Current != '\\')
                        {
                            if (*Current == '\n')
                            {
                                state = ParseStateEnum.StringEnter;
                                return;
                            }
                            ++Current;
                        }
                    }
                    else
                    {
                        while (*Current != quote && *Current != '\\')
                        {
                            if (*Current == '\n')
                            {
                                state = ParseStateEnum.StringEnter;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                        }
                    }
                    if (*Current == quote)
                    {
                        ++Current;
                        return;
                    }
                    if (*++Current == 'u')
                    {
                        if ((int)(end - Current) < 5)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        Current += 5;
                    }
                    else if (*Current == 'x')
                    {
                        if ((int)(end - Current) < 3)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        Current += 3;
                    }
                    else
                    {
                        if (Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        ++Current;
                    }
                }
                while (true);
            }
        }
        /// <summary>
        /// 字符串解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref string value)
        {
            if (*Current == '\'' || *Current == '"')
            {
                value = parseString();
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '\'' || *Current == '"')
            {
                value = parseString();
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            state = ParseStateEnum.NotString;
        }
        /// <summary>
        /// 字符串解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref subString value)
        {
            if (*Current == '\'' || *Current == '"') goto STRING;
            if (isNull())
            {
                value.Null();
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current != '\'' && *Current != '"')
            {
                if (isNull())
                {
                    value.Null();
                    return;
                }
                state = ParseStateEnum.NotString;
                return;
            }
        STRING:
            quote = *Current;
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            char* start = Current;
            searchEscape();
            if (state != ParseStateEnum.Success) return;
            if (*Current == quote)
            {
                if (this.json == null) value = new string(start, 0, (int)(Current++ - start));
                else value.UnsafeSet(this.json, (int)(start - jsonFixed), (int)(Current++ - start));
                return;
            }
            if (parseConfig.IsTempString && this.json != null)
            {
                char* writeEnd = parseEscape();
                if (writeEnd != null) value.UnsafeSet(this.json, (int)(start - jsonFixed), (int)(writeEnd - start));
            }
            else
            {
                string newValue = parseEscape(start);
                if (newValue != null) value.UnsafeSet(newValue, 0, newValue.Length);
            }
        }
        /// <summary>
        /// JSON节点解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        private void parse(ref jsonNode value)
        {
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            switch (*Current)
            {
                case '"':
                case '\'':
                    parseStringNode(ref value);
                    return;
                case '{':
                    subArray<keyValue<jsonNode, jsonNode>> dictionary = parseDictionaryNode();
                    if (state == ParseStateEnum.Success) value.SetDictionary(dictionary);
                    return;
                case '[':
                    subArray<jsonNode> list = parseListNode();
                    if (state == ParseStateEnum.Success) value.SetList(list);
                    {
                        value.Type = jsonNode.type.List;
                    }
                    return;
                case 'n':
                    if (*(Current + 1) == 'u')
                    {
                        if ((int)(end - Current) < 4 || (*(int*)(Current + 2) ^ ('l' + ('l' << 16))) != 0) state = ParseStateEnum.NotNull;
                        else
                        {
                            value.Type = jsonNode.type.Null;
                            Current += 4;
                        }
                        return;
                    }
                    if ((int)(end - Current) > 9 && *(int*)(Current + 1) == ('e' + ('w' << 16))
                        && ((*(int*)(Current + 3) ^ (' ' + ('D' << 16))) | (*(int*)(Current + 5) ^ ('a' + ('t' << 16))) | (*(int*)(Current + 7) ^ ('e' + ('(' << 16)))) == 0)
                    {
                        long millisecond = 0;
                        Current += 9;
                        Parse(ref millisecond);
                        if (state != ParseStateEnum.Success) return;
                        if (Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                        if (*Current == ')')
                        {
                            value.Int64 = fastCSharp.config.appSetting.JavascriptMinTime.Ticks + millisecond * date.MillisecondTicks;
                            value.Type = jsonNode.type.DateTimeTick;
                            ++Current;
                            return;
                        }
                    }
                    break;
                case 't':
                    if ((int)(end - Current) < 4 || ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0) state = ParseStateEnum.NotBool;
                    else
                    {
                        Current += 4;
                        value.Int64 = 1;
                        value.Type = jsonNode.type.Bool;
                    }
                    return;
                case 'f':
                    if (((int)(end - Current)) < 5
                        || ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) != 0) state = ParseStateEnum.NotBool;
                    else
                    {
                        Current += 5;
                        value.Int64 = 0;
                        value.Type = jsonNode.type.Bool;
                    }
                    return;
                default:
                    char* numberEnd = searchNumber();
                    if (numberEnd != null)
                    {
                        if (numberEnd == jsonFixed) value.Type = jsonNode.type.NaN;
                        else
                        {
                            if (json == null) value.String = new string(Current, 0, (int)(numberEnd - Current));
                            else value.String.UnsafeSet(this.json, (int)(Current - jsonFixed), (int)(numberEnd - Current));
                            Current = numberEnd;
                            if (quote != 0) ++Current;
                            value.SetNumberString(quote);
                        }
                        return;
                    }
                    break;
            }
            state = ParseStateEnum.UnknownValue;
        }
        /// <summary>
        /// 解析字符串节点
        /// </summary>
        /// <param name="value"></param>
        private void parseStringNode(ref jsonNode value)
        {
            quote = *Current;
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            char* start = Current;
            searchEscape();
            if (state != ParseStateEnum.Success) return;
            if (*Current == quote)
            {
                if (this.json == null) value.String = new string(start, 0, (int)(Current++ - start));
                else value.String.UnsafeSet(this.json, (int)(start - jsonFixed), (int)(Current++ - start));
                value.Type = jsonNode.type.String;
                return;
            }
            if (this.json != null)
            {
                char* escapeStart = Current;
                searchEscapeEnd();
                if (state == ParseStateEnum.Success)
                {
                    value.String.UnsafeSet(this.json, (int)(start - jsonFixed), (int)(Current - start));
                    value.SetQuoteString((int)(escapeStart - start), quote, parseConfig.IsTempString);
                    ++Current;
                }
            }
            else
            {
                string newValue = parseEscape(start);
                if (newValue != null)
                {
                    value.String.UnsafeSet(newValue, 0, newValue.Length);
                    value.Type = jsonNode.type.String;
                }
            }
        }
        /// <summary>
        /// 查找转义字符串结束位置
        /// </summary>
        private void searchEscapeEnd()
        {
            do
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    Current += 5;
                }
                else if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    Current += 3;
                }
                else
                {
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    ++Current;
                }
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if (endChar == quote)
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = ParseStateEnum.StringEnter;
                            return;
                        }
                        ++Current;
                    }
                }
                else
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = ParseStateEnum.StringEnter;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                    }
                }
                if (*Current == quote) return;
            }
            while (true);
        }
        /// <summary>
        /// 字符串转义解析
        /// </summary>
        /// <param name="value"></param>
        /// <param name="escapeIndex">未解析字符串起始位置</param>
        /// <param name="quote">字符串引号</param>
        /// <returns></returns>
        private subString parseQuoteString(subString value, int escapeIndex, char quote, int isTempString)
        {
            fixed (char* jsonFixed = value.value)
            {
                char* start = jsonFixed + value.StartIndex;
                end = start + value.Length;
                this.quote = quote;
                Current = start + escapeIndex;
                endChar = *(end - 1);
                if (isTempString == 0)
                {
                    string newValue = parseEscape(start);
                    if (newValue != null) return newValue;
                }
                else
                {
                    char* writeEnd = parseEscape();
                    if (writeEnd != null) return subString.Unsafe(value.value, (int)(start - jsonFixed), (int)(writeEnd - start));
                }
            }
            return default(subString);
        }
        /// <summary>
        /// 解析列表节点
        /// </summary>
        /// <returns></returns>
        private subArray<jsonNode> parseListNode()
        {
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return default(subArray<jsonNode>);
            }
            subArray<jsonNode> value = default(subArray<jsonNode>);
            while (isNextArrayValue())
            {
                jsonNode node = default(jsonNode);
                parse(ref node);
                if (state != ParseStateEnum.Success) return default(subArray<jsonNode>);
                value.Add(node);
            }
            return value;
        }
        /// <summary>
        /// 解析字典节点
        /// </summary>
        /// <returns></returns>
        private subArray<keyValue<jsonNode, jsonNode>> parseDictionaryNode()
        {
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return default(subArray<keyValue<jsonNode, jsonNode>>);
            }
            subArray<keyValue<jsonNode, jsonNode>> value = default(subArray<keyValue<jsonNode, jsonNode>>);
            if (isFirstObject())
            {
                do
                {
                    jsonNode name = default(jsonNode);
                    if (*Current == '"' || *Current == '\'') parseStringNode(ref name);
                    else
                    {
                        char* nameStart = Current;
                        searchNameEnd();
                        if (this.json == null) name.String = new string(nameStart, 0, (int)(Current - nameStart));
                        else name.String.UnsafeSet(this.json, (int)(nameStart - jsonFixed), (int)(Current - nameStart));
                        name.Type = jsonNode.type.String;
                    }
                    if (state != ParseStateEnum.Success || searchColon() == 0) return default(subArray<keyValue<jsonNode, jsonNode>>);
                    jsonNode node = default(jsonNode);
                    parse(ref node);
                    if (state != ParseStateEnum.Success) return default(subArray<keyValue<jsonNode, jsonNode>>);
                    value.Add(new keyValue<jsonNode, jsonNode>(name, node));
                }
                while (isNextObject());
            }
            return value;
        }
        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="value">数据</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void parse(ref object value)
        {
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            ignore();
            if (state == ParseStateEnum.Success) value = new object();
        }
        /// <summary>
        /// 类型解析
        /// </summary>
        /// <param name="type">类型</param>
        [parseMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Parse(ref Type type)
        {
            if (*Current == '{')
            {
                remoteType remoteType = default(remoteType);
                typeParser<remoteType>.Parse(this, ref remoteType);
                if (!remoteType.TryGet(out type)) state = ParseStateEnum.ErrorType;
                return;
            }
            if (isNull())
            {
                type = null;
                return;
            }
            space();
            if (*Current == '{')
            {
                remoteType remoteType = default(remoteType);
                typeParser<remoteType>.Parse(this, ref remoteType);
                if (!remoteType.TryGet(out type)) state = ParseStateEnum.ErrorType;
                return;
            }
            if (isNull())
            {
                type = null;
                return;
            }
            state = ParseStateEnum.ErrorType;
        }
        /// <summary>
        /// 查找枚举引号并返回第一个字符
        /// </summary>
        /// <returns>第一个字符,0表示null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char searchEnumQuote()
        {
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextEnumChar();
            }
            if (isNull()) return quote = (char)0;
            space();
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return (char)0;
            }
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextEnumChar();
            }
            if (isNull()) return quote = (char)0;
            state = ParseStateEnum.NotEnumChar;
            return (char)0;
        }
        /// <summary>
        /// 获取下一个枚举字符
        /// </summary>
        /// <returns>下一个枚举字符,0表示null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char nextEnumChar()
        {
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return (char)0;
            }
            if (*Current == quote)
            {
                ++Current;
                return quote = (char)0;
            }
            if (*Current == '\\' || *Current == '\n')
            {
                state = ParseStateEnum.NotEnumChar;
                return (char)0;
            }
            return *Current;
        }
        /// <summary>
        /// 查找下一个枚举字符
        /// </summary>
        /// <returns>下一个枚举字符,0表示null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char searchNextEnum()
        {
            do
            {
                if (*Current == ',')
                {
                    do
                    {
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return (char)0;
                        }
                    }
                    while (*Current == ' ');
                    if (*Current == quote)
                    {
                        ++Current;
                        return quote = (char)0;
                    }
                    if (*Current == '\\' || *Current == '\n')
                    {
                        state = ParseStateEnum.NotEnumChar;
                        return (char)0;
                    }
                    return *Current;
                }
                if (*Current == quote)
                {
                    ++Current;
                    return quote = (char)0;
                }
                if (*Current == '\\' || *Current == '\n')
                {
                    state = ParseStateEnum.NotEnumChar;
                    return (char)0;
                }
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return (char)0;
                }
            }
            while (true);
        }
        /// <summary>
        /// 查找数组起始位置
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <param name="value">目标数组</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void searchArray<valueType>(ref valueType[] value)
        {
            if (*Current == '[')
            {
                ++Current;
                value = nullValue<valueType>.Array;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (*Current == '[')
            {
                ++Current;
                value = nullValue<valueType>.Array;
                return;
            }
            state = ParseStateEnum.CrashEnd;
        }
        /// <summary>
        /// 是否存在下一个数组数据
        /// </summary>
        /// <returns>是否存在下一个数组数据</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isFirstArrayValue()
        {
            if (*Current == ']')
            {
                ++Current;
                return false;
            }
            space();
            if (state != ParseStateEnum.Success) return false;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            if (*Current == ']')
            {
                ++Current;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 是否存在下一个数组数据
        /// </summary>
        /// <returns>是否存在下一个数组数据</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isNextArrayValue()
        {
            if (*Current == ',')
            {
                ++Current;
                return true;
            }
            if (*Current == ']')
            {
                ++Current;
                return false;
            }
            space();
            if (state != ParseStateEnum.Success) return false;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            if (*Current == ',')
            {
                ++Current;
                return true;
            }
            if (*Current == ']')
            {
                ++Current;
                return false;
            }
            state = ParseStateEnum.NotArrayValue;
            return false;
        }
        /// <summary>
        /// 自定义构造函数数据解析
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void noConstructor<valueType>(ref valueType value)
        {
            if (value == null)
            {
                object newValue = parseConfig.Constructor(typeof(valueType));
                if (newValue == null)
                {
                    state = ParseStateEnum.NoConstructor;
                    return;
                }
                value = (valueType)newValue;
            }
            typeParser<valueType>.ParseClass(this, ref value);
        }
        /// <summary>
        /// 查找对象起始位置
        /// </summary>
        /// <returns>是否查找到</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool searchObject()
        {
            if (*Current == '{')
            {
                ++Current;
                return true;
            }
            if (isNull()) return false;
            space();
            if (state != ParseStateEnum.Success) return false;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            if (*Current == '{')
            {
                ++Current;
                return true;
            }
            if (isNull()) return false;
            state = ParseStateEnum.NotObject;
            return false;
        }
        /// <summary>
        /// 判断是否存在第一个成员
        /// </summary>
        /// <returns>是否存在第一个成员</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isFirstObject()
        {
            if (*Current < NameMapSize && nameStartMap.Get(*Current)) return true;
            if (*Current == '}')
            {
                ++Current;
                return false;
            }
            space();
            if (state != ParseStateEnum.Success) return false;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            if (*Current < NameMapSize && nameStartMap.Get(*Current)) return true;
            if (*Current == '}')
            {
                ++Current;
                return false;
            }
            state = ParseStateEnum.NotFoundName;
            return false;
        }
        /// <summary>
        /// 判断是否存在下一个成员
        /// </summary>
        /// <returns>是否存在下一个成员</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isNextObject()
        {
            if (*Current != ',') goto NOTNEXT;
        NEXT:
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            if (*Current < NameMapSize && nameStartMap.Get(*Current)) return true;
            space();
            if (state != ParseStateEnum.Success) return false;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            if (*Current < NameMapSize && nameStartMap.Get(*Current)) return true;
            state = ParseStateEnum.NotFoundName;
            return false;
        NOTNEXT:
            if (*Current == '}')
            {
                ++Current;
                return false;
            }
            space();
            if (state != ParseStateEnum.Success) return false;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return false;
            }
            if (*Current == ',') goto NEXT;
            if (*Current == '}')
            {
                ++Current;
                return false;
            }
            state = ParseStateEnum.NotObject;
            return false;
        }
        /// <summary>
        /// 查找冒号
        /// </summary>
        /// <returns>是否找到</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte searchColon()
        {
            if (*Current == ':')
            {
                ++Current;
                return 1;
            }
            space();
            if (state != ParseStateEnum.Success) return 0;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return 0;
            }
            if (*Current == ':')
            {
                ++Current;
                return 1;
            }
            state = ParseStateEnum.NotFoundColon;
            return 0;
        }
        /// <summary>
        /// 获取成员名称第一个字符
        /// </summary>
        /// <returns>第一个字符,0表示失败</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char getFirstName()
        {
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextStringChar();
            }
            quote = (char)0;
            return *Current;
        }
        /// <summary>
        /// 获取成员名称下一个字符
        /// </summary>
        /// <returns>第一个字符,0表示失败</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char getNextName()
        {
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return (char)0;
            }
            return *Current < NameMapSize && nameMap.Get(*Current) ? *Current : (char)0;
        }
        /// <summary>
        /// 查找名称直到结束
        /// </summary>
        private void searchNameEnd()
        {
            if (state == ParseStateEnum.Success)
            {
                while (++Current != end && *Current < NameMapSize && nameMap.Get(*Current)) ;
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
            }
        }
        /// <summary>
        /// 忽略对象
        /// </summary>
        private void ignore()
        {
            space();
            if (state != ParseStateEnum.Success) return;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            switch (*Current)
            {
                case '"':
                case '\'':
                    ignoreString();
                    return;
                case '{':
                    ignoreObject();
                    return;
                case '[':
                    ignoreArray();
                    return;
                case 'n':
                    if ((int)(end - Current) >= 4)
                    {
                        if (*(Current + 1) == 'u')
                        {
                            if (*(int*)(Current + 2) == ('l') + (('l') << 16))
                            {
                                Current += 4;
                                return;
                            }
                        }
                        else if ((int)(end - Current) > 9 && ((*(int*)(Current + 1) ^ ('e' + ('w' << 16))) | (*(int*)(Current + 3) ^ (' ' + ('D' << 16))) | (*(int*)(Current + 5) ^ ('a' + ('t' << 16))) | (*(int*)(Current + 7) ^ ('e' + ('(' << 16)))) == 0)
                        {
                            Current += 9;
                            ignoreNumber();
                            if (state != ParseStateEnum.Success) return;
                            if (Current == end)
                            {
                                state = ParseStateEnum.CrashEnd;
                                return;
                            }
                            if (*Current == ')')
                            {
                                ++Current;
                                return;
                            }
                        }
                    }
                    break;
                case 't':
                    if ((int)(end - Current) >= 4 && ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) == 0)
                    {
                        Current += 4;
                        return;
                    }
                    break;
                case 'f':
                    if (((int)(end - Current)) >= 5 && ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) == 0)
                    {
                        Current += 5;
                        return;
                    }
                    break;
                default:
                    ignoreNumber();
                    return;
            }
            state = ParseStateEnum.UnknownValue;
        }
        /// <summary>
        /// 忽略字符串
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ignoreString()
        {
            quote = *Current;
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            char* start = Current;
            searchEscape();
            if (state != ParseStateEnum.Success) return;
            if (*Current == quote)
            {
                ++Current;
                return;
            }
            do
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    Current += 5;
                }
                else if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    Current += 3;
                }
                else
                {
                    if (Current == end)
                    {
                        state = ParseStateEnum.CrashEnd;
                        return;
                    }
                    ++Current;
                }
                if (Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
                if (endChar == quote)
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = ParseStateEnum.StringEnter;
                            return;
                        }
                        ++Current;
                    }
                }
                else
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = ParseStateEnum.StringEnter;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = ParseStateEnum.CrashEnd;
                            return;
                        }
                    }
                }
                if (*Current == quote)
                {
                    ++Current;
                    return;
                }
            }
            while (true);
        }
        /// <summary>
        /// 忽略对象
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ignoreObject()
        {
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (isFirstObject())
            {
                if (*Current == '\'' || *Current == '"') ignoreString();
                else ignoreName();
                if (state != ParseStateEnum.Success || searchColon() == 0) return;
                ignore();
                while (state == ParseStateEnum.Success && isNextObject())
                {
                    if (*Current == '\'' || *Current == '"') ignoreString();
                    else ignoreName();
                    if (state != ParseStateEnum.Success || searchColon() == 0) return;
                    ignore();
                }
            }
        }
        /// <summary>
        /// 忽略成员名称
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ignoreName()
        {
            do
            {
                if (++Current == end)
                {
                    state = ParseStateEnum.CrashEnd;
                    return;
                }
            }
            while (*Current < NameMapSize && nameMap.Get(*Current));
        }
        /// <summary>
        /// 忽略数组
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ignoreArray()
        {
            if (++Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return;
            }
            if (isFirstArrayValue())
            {
                do
                {
                    ignore();
                }
                while (isNextArrayValue());
            }
        }
        /// <summary>
        /// 忽略数字
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ignoreNumber()
        {
            if (*Current < NumberMapSize && numberMap.Get(*Current))
            {
                while (++Current != end && *Current < NumberMapSize && numberMap.Get(*Current)) ;
                return;
            }
            state = ParseStateEnum.NotNumber;
        }
        /// <summary>
        /// 查找字典起始位置
        /// </summary>
        /// <returns>是否查找到</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte searchDictionary()
        {
            if (*Current == '{')
            {
                ++Current;
                return 1;
            }
            if (*Current == '[')
            {
                ++Current;
                return 2;
            }
            if (isNull()) return 0;
            space();
            if (state != ParseStateEnum.Success) return 0;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return 0;
            }
            if (*Current == '{')
            {
                ++Current;
                return 1;
            }
            if (*Current == '[')
            {
                ++Current;
                return 2;
            }
            if (isNull()) return 0;
            state = ParseStateEnum.NotObject;
            return 0;
        }
        /// <summary>
        /// 对象是否结束
        /// </summary>
        /// <returns>对象是否结束</returns>
        private byte isDictionaryObjectEnd()
        {
            if (*Current == '}')
            {
                ++Current;
                return 1;
            }
            space();
            if (state != ParseStateEnum.Success) return 1;
            if (Current == end)
            {
                state = ParseStateEnum.CrashEnd;
                return 1;
            }
            if (*Current == '}')
            {
                ++Current;
                return 1;
            }
            return 0;
        }
        ///// <summary>
        ///// TCP调用名称解析
        ///// </summary>
        ///// <param name="name">名称</param>
        ///// <returns>是否成功</returns>
        //internal bool TcpParameterName(string name)
        //{
        //    if (end - current > name.Length + 5 && *(int*)current == '{' + ('"' << 16))
        //    {
        //        fixed (char* nameFixed = name)
        //        {
        //            if (fastCSharp.unsafer.memory.Equal((byte*)(current + 2), nameFixed, name.Length << 1))
        //            {
        //                if (*(int*)(current += name.Length + 2) == '"' + (':' << 16))
        //                {
        //                    current += 2;
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// TCP调用对象结束
        ///// </summary>
        //internal void TcpObjectEnd()
        //{
        //    if (state == parseState.Success)
        //    {
        //        if (*current == '}') ++current;
        //        else state = parseState.CrashEnd;
        //    }
        //}

        /// <summary>
        /// 引用类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void typeParse<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.ParseClass(parser, ref value);
        }
        /// <summary>
        /// 引用类型对象解析函数信息
        /// </summary>
        private static readonly MethodInfo typeParseMethod = typeof(jsonParser).GetMethod("typeParse", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void structParse<valueType>(jsonParser parser, ref valueType value) where valueType : struct
        {
            if (parser.searchObject()) typeParser<valueType>.ParseMembers(parser, ref value);
            else value = default(valueType);
        }
        /// <summary>
        /// 集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo structParseMethod = typeof(jsonParser).GetMethod("structParse", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void nullableParse<valueType>(ref Nullable<valueType> value) where valueType : struct
        {
            if (searchObject())
            {
                --Current;
                valueType newValue = value.HasValue ? value.Value : default(valueType);
                typeParser<valueType>.Parse(this, ref newValue);
                value = newValue;
            }
            else value = null;
        }
        /// <summary>
        /// 集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo nullableParseMethod = typeof(jsonParser).GetMethod("nullableParse", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 是否null
        /// </summary>
        /// <returns>是否null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool tryNull()
        {
            if (isNull()) return true;
            space();
            return isNull();
        }
        /// <summary>
        /// 值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void nullableEnumParse<valueType>(jsonParser parser, ref Nullable<valueType> value) where valueType : struct
        {
            if (parser.tryNull()) value = null;
            else
            {
                valueType newValue = value.HasValue ? value.Value : default(valueType);
                typeParser<valueType>.DefaultParser(parser, ref newValue);
                value = newValue;
            }
        }
        /// <summary>
        /// 集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo nullableEnumParseMethod = typeof(jsonParser).GetMethod("nullableEnumParse", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void keyValuePairParse<keyType, valueType>(jsonParser parser, ref KeyValuePair<keyType, valueType> value)
        {
            if (parser.searchObject())
            {
                keyValue<keyType, valueType> keyValue = new keyValue<keyType,valueType>();
                typeParser<keyValue<keyType, valueType>>.ParseMembers(parser, ref keyValue);
                value = new KeyValuePair<keyType, valueType>(keyValue.Key, keyValue.Value);
            }
            else value = new KeyValuePair<keyType, valueType>();
        }
        /// <summary>
        /// 值类型对象解析函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairParseMethod = typeof(jsonParser).GetMethod("keyValuePairParse", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void nullableMemberParse<valueType>(jsonParser parser, ref Nullable<valueType> value) where valueType : struct
        {
            if (parser.searchObject())
            {
                valueType newValue = value.HasValue ? value.Value : default(valueType);
                typeParser<valueType>.ParseMembers(parser, ref newValue);
                value = newValue;
            }
            else value = null;
        }
        /// <summary>
        /// 集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberParseMethod = typeof(jsonParser).GetMethod("nullableMemberParse", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 基类转换
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void baseParse<valueType, childType>(jsonParser parser, ref childType value) where childType : valueType
        {
            valueType newValue = value;
            if (value == null)
            {
                if (parser.searchObject())
                {
                    newValue = fastCSharp.emit.constructor<childType>.New();
                    typeParser<valueType>.ParseMembers(parser, ref newValue);
                    value = (childType)newValue;
                }
            }
            else typeParser<valueType>.ParseClass(parser, ref newValue);
        }
        /// <summary>
        /// 基类转换函数信息
        /// </summary>
        private static readonly MethodInfo baseParseMethod = typeof(jsonParser).GetMethod("baseParse", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 找不到构造函数
        /// </summary>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void checkNoConstructor<valueType>(ref valueType value)
        {
            if (value == null)
            {
                Func<Type, object> constructor = parseConfig.Constructor;
                if (constructor == null)
                {
                    state = ParseStateEnum.NoConstructor;
                    return;
                }
            }
            noConstructor(ref value);
        }
        /// <summary>
        /// 找不到构造函数解析函数信息
        /// </summary>
        private static readonly MethodInfo checkNoConstructorMethod = typeof(jsonParser).GetMethod("checkNoConstructor", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumByte<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumByte.Parse(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMethod = typeof(jsonParser).GetMethod("enumByte", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumSByte<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumSByte.Parse(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMethod = typeof(jsonParser).GetMethod("enumSByte", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumShort<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumShort.Parse(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMethod = typeof(jsonParser).GetMethod("enumShort", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumUShort<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumUShort.Parse(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMethod = typeof(jsonParser).GetMethod("enumUShort", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumInt<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumInt.Parse(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumIntMethod = typeof(jsonParser).GetMethod("enumInt", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumUInt<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumUInt.Parse(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntMethod = typeof(jsonParser).GetMethod("enumUInt", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumLong<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumLong.Parse(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumLongMethod = typeof(jsonParser).GetMethod("enumLong", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumULong<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumULong.Parse(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumULongMethod = typeof(jsonParser).GetMethod("enumULong", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumByteFlags<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumByte.ParseFlags(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumByteFlagsMethod = typeof(jsonParser).GetMethod("enumByteFlags", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumSByteFlags<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumSByte.ParseFlags(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteFlagsMethod = typeof(jsonParser).GetMethod("enumSByteFlags", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumShortFlags<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumShort.ParseFlags(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumShortFlagsMethod = typeof(jsonParser).GetMethod("enumShortFlags", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumUShortFlags<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumUShort.ParseFlags(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortFlagsMethod = typeof(jsonParser).GetMethod("enumUShortFlags", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumIntFlags<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumInt.ParseFlags(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumIntFlagsMethod = typeof(jsonParser).GetMethod("enumIntFlags", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumUIntFlags<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumUInt.ParseFlags(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntFlagsMethod = typeof(jsonParser).GetMethod("enumUIntFlags", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumLongFlags<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumLong.ParseFlags(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumLongFlagsMethod = typeof(jsonParser).GetMethod("enumLongFlags", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumULongFlags<valueType>(jsonParser parser, ref valueType value)
        {
            typeParser<valueType>.enumULong.ParseFlags(parser, ref value);
        }
        /// <summary>
        /// 枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumULongFlagsMethod = typeof(jsonParser).GetMethod("enumULongFlags", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 数组解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void array<valueType>(jsonParser parser, ref valueType[] values)
        {
            typeParser<valueType>.Array(parser, ref values);
        }
        /// <summary>
        /// 数组解析函数信息
        /// </summary>
        private static readonly MethodInfo arrayMethod = typeof(jsonParser).GetMethod("array", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 字典解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="dictionary">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void dictionary<valueType, dictionaryValueType>(ref Dictionary<valueType, dictionaryValueType> dictionary)
        {
            byte type = searchDictionary();
            if (type == 0) dictionary = null;
            else
            {
                dictionary = fastCSharp.dictionary.CreateAny<valueType, dictionaryValueType>();
                if (type == 1)
                {
                    if (isDictionaryObjectEnd() == 0)
                    {
                        valueType key = default(valueType);
                        dictionaryValueType value = default(dictionaryValueType);
                        do
                        {
                            typeParser<valueType>.Parse(this, ref key);
                            if (state != ParseStateEnum.Success || searchColon() == 0) return;
                            typeParser<dictionaryValueType>.Parse(this, ref value);
                            if (state != ParseStateEnum.Success) return;
                            dictionary.Add(key, value);
                        }
                        while (isNextObject());
                    }
                }
                else if (isFirstArrayValue())
                {
                    keyValue<valueType, dictionaryValueType> value = default(keyValue<valueType, dictionaryValueType>);
                    do
                    {
                        typeParser<keyValue<valueType, dictionaryValueType>>.ParseValue(this, ref value);
                        if (state != ParseStateEnum.Success) return;
                        dictionary.Add(value.Key, value.Value);
                    }
                    while (isNextArrayValue());
                }
            }
        }
        /// <summary>
        /// 字典解析函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMethod = typeof(jsonParser).GetMethod("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void dictionaryConstructor<dictionaryType, keyType, valueType>(jsonParser parser, ref dictionaryType value)
        {
            KeyValuePair<keyType, valueType>[] values = null;
            int count = typeParser<KeyValuePair<keyType, valueType>>.ArrayIndex(parser, ref values);
            if (count == -1) value = default(dictionaryType);
            else
            {
                Dictionary<keyType, valueType> dictionary = fastCSharp.dictionary.CreateAny<keyType, valueType>(count);
                if (count != 0)
                {
                    foreach (KeyValuePair<keyType, valueType> keyValue in values)
                    {
                        dictionary.Add(keyValue.Key, keyValue.Value);
                        if (--count == 0) break;
                    }
                }
                value = pub.dictionaryConstructor<dictionaryType, keyType, valueType>.Constructor(dictionary);
            }
        }
        /// <summary>
        /// 集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryConstructorMethod = typeof(jsonParser).GetMethod("dictionaryConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void listConstructor<valueType, argumentType>(jsonParser parser, ref valueType value)
        {
            argumentType[] values = null;
            int count = typeParser<argumentType>.ArrayIndex(parser, ref values);
            if (count == -1) value = default(valueType);
            else value = pub.listConstructor<valueType, argumentType>.Constructor(subArray<argumentType>.Unsafe(values, 0, count));
        }
        /// <summary>
        /// 集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo listConstructorMethod = typeof(jsonParser).GetMethod("listConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void collectionConstructor<valueType, argumentType>(jsonParser parser, ref valueType value)
        {
            argumentType[] values = null;
            int count = typeParser<argumentType>.ArrayIndex(parser, ref values);
            if (count == -1) value = default(valueType);
            else value = value = pub.collectionConstructor<valueType, argumentType>.Constructor(subArray<argumentType>.Unsafe(values, 0, count));
        }
        /// <summary>
        /// 集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo collectionConstructorMethod = typeof(jsonParser).GetMethod("collectionConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enumerableConstructor<valueType, argumentType>(jsonParser parser, ref valueType value)
        {
            argumentType[] values = null;
            int count = typeParser<argumentType>.ArrayIndex(parser, ref values);
            if (count == -1) value = default(valueType);
            else value = pub.enumerableConstructor<valueType, argumentType>.Constructor(subArray<argumentType>.Unsafe(values, 0, count));
        }
        /// <summary>
        /// 集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo enumerableConstructorMethod = typeof(jsonParser).GetMethod("enumerableConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void arrayConstructor<valueType, argumentType>(jsonParser parser, ref valueType value)
        {
            argumentType[] values = null;
            typeParser<argumentType>.Array(parser, ref values);
            if (parser.state == ParseStateEnum.Success)
            {
                if (values == null) value = default(valueType);
                else value = pub.arrayConstructor<valueType, argumentType>.Constructor(values);
            }
        }
        /// <summary>
        /// 数组构造解析函数信息
        /// </summary>
        private static readonly MethodInfo arrayConstructorMethod = typeof(jsonParser).GetMethod("arrayConstructor", BindingFlags.Static | BindingFlags.NonPublic);
        
        /// <summary>
        /// 公共默认配置参数
        /// </summary>
        private static readonly ConfigPlus defaultConfig = new ConfigPlus { IsGetJson = false };
        /// <summary>
        /// Json解析
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="config">配置参数</param>
        /// <returns>目标数据</returns>
        public static valueType Parse<valueType>(subString json, ConfigPlus config = null)
        {
            if (json.Length == 0)
            {
                if (config != null) config.State = ParseStateEnum.NullJson;
                return default(valueType);
            }
            valueType value = default(valueType);
            jsonParser parser = typePool<jsonParser>.Pop() ?? new jsonParser();
            try
            {
                return parser.parse<valueType>(json, ref value, config ?? defaultConfig) == ParseStateEnum.Success ? value : default(valueType);
            }
            finally { parser.free(); }
        }
        /// <summary>
        /// Json解析
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="value">目标数据</param>
        /// <param name="config">配置参数</param>
        /// <returns>是否解析成功</returns>
        public static bool Parse<valueType>(subString json, ref valueType value, ConfigPlus config = null)
        {
            if (json.Length == 0)
            {
                if (config != null) config.State = ParseStateEnum.NullJson;
                return false;
            }
            jsonParser parser = typePool<jsonParser>.Pop() ?? new jsonParser();
            try
            {
                return parser.parse<valueType>(json, ref value, config ?? defaultConfig) == ParseStateEnum.Success;
            }
            finally { parser.free(); }
        }
        /// <summary>
        /// Json解析
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="length">Json长度</param>
        /// <param name="value">目标数据</param>
        /// <param name="config">配置参数</param>
        /// <returns>是否解析成功</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Parse<valueType>(char* json, int length, ref valueType value, ConfigPlus config = null, byte[] buffer = null)
        {
            jsonParser parser = typePool<jsonParser>.Pop() ?? new jsonParser();
            try
            {
                return parser.parse<valueType>(json, length, ref value, config ?? defaultConfig, buffer) == ParseStateEnum.Success;
            }
            finally { parser.free(); }
        }
        /// <summary>
        /// Json解析
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="config">配置参数</param>
        /// <returns>目标数据</returns>
        private static object parseType<valueType>(subString json, ConfigPlus config)
        {
            return Parse<valueType>(json, config);
        }
        /// <summary>
        /// Json解析
        /// </summary>
        /// <param name="type">目标数据类型</param>
        /// <param name="json">Json字符串</param>
        /// <param name="config">配置参数</param>
        /// <returns>目标数据</returns>
        public static object ParseType(Type type, subString json, ConfigPlus config = null)
        {
            if (type == null) log.Error.Throw(log.exceptionType.Null);
            Func<subString, ConfigPlus, object> parse;
            if (!parseTypes.TryGetValue(type, out parse))
            {
                parse = (Func<subString, ConfigPlus, object>)Delegate.CreateDelegate(typeof(Func<subString, ConfigPlus, object>), parseTypeMethod.MakeGenericMethod(type));
                parseTypes.Set(type, parse);
            }
            return parse(json, config);
        }
        /// <summary>
        /// Json解析
        /// </summary>
        private static interlocked.dictionary<Type, Func<subString, ConfigPlus, object>> parseTypes = new interlocked.dictionary<Type,Func<subString,ConfigPlus,object>>(fastCSharp.dictionary.CreateOnly<Type, Func<subString, config, object>>());
        /// <summary>
        /// Json解析函数信息
        /// </summary>
        private static readonly MethodInfo parseTypeMethod = typeof(jsonParser).GetMethod("parseType", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(subString), typeof(ConfigPlus) }, null);
        /// <summary>
        /// 字符串转义解析
        /// </summary>
        /// <param name="value"></param>
        /// <param name="escapeIndex">未解析字符串起始位置</param>
        /// <param name="quote">字符串引号</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static subString ParseQuoteString(subString value, int escapeIndex, char quote, int isTempString)
        {
            jsonParser parser = typePool<jsonParser>.Pop() ?? new jsonParser();
            try
            {
                return parser.parseQuoteString(value, escapeIndex, quote, isTempString);
            }
            finally { parser.free(); }
        }
        /// <summary>
        /// 基本类型解析函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> parseMethods;
        /// <summary>
        /// 获取基本类型解析函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>解析函数</returns>
        private static MethodInfo getParseMethod(Type type)
        {
            MethodInfo method;
            return parseMethods.TryGetValue(type, out method) ? method : null;
        }
        /// <summary>
        /// 空格字符位图尺寸
        /// </summary>
        internal const int SpaceMapSize = 128 + 64;
        /// <summary>
        /// 空格字符位图
        /// </summary>
        private static readonly fixedMap spaceMap;
        /// <summary>
        /// 数字字符位图尺寸
        /// </summary>
        internal const int NumberMapSize = 128;
        /// <summary>
        /// 数字字符位图
        /// </summary>
        private static readonly fixedMap numberMap;
        /// <summary>
        /// 键值字符位图尺寸
        /// </summary>
        internal const int NameMapSize = 128;
        /// <summary>
        /// 键值字符位图
        /// </summary>
        private static readonly fixedMap nameMap;
        /// <summary>
        /// 键值开始字符位图
        /// </summary>
        private static readonly fixedMap nameStartMap;
        /// <summary>
        /// 转义字符集合尺寸
        /// </summary>
        private const int escapeCharSize = 128;
        /// <summary>
        /// 转义字符集合
        /// </summary>
        private static readonly pointer escapeCharData;
        static JsonParserPlus()
        {
            int dataIndex = 0;
            pointer[] datas = unmanaged.Get(true, SpaceMapSize >> 3, NumberMapSize >> 3, NameMapSize >> 3, NameMapSize >> 3, escapeCharSize * sizeof(char));
            spaceMap = new fixedMap(datas[dataIndex++]);
            numberMap = new fixedMap(datas[dataIndex++]);
            nameMap = new fixedMap(datas[dataIndex++]);
            nameStartMap = new fixedMap(datas[dataIndex++]);
            escapeCharData = datas[dataIndex++];

            foreach (char value in " \r\n\t") spaceMap.Set(value);
            spaceMap.Set(160);

            numberMap.Set('0', 10);
            //numberMap.Set('a');
            numberMap.Set('e');
            numberMap.Set('E');
            //numberMap.Set('N');
            numberMap.Set('+');
            numberMap.Set('-');
            numberMap.Set('.');

            nameMap.Set('0', 10);
            nameMap.Set('A', 26);
            nameMap.Set('a', 26);
            nameMap.Set('_');

            nameStartMap.Set('A', 26);
            nameStartMap.Set('a', 26);
            nameStartMap.Set('_');
            nameStartMap.Set('\'');
            nameStartMap.Set('"');

            char* escapeCharDataChar = escapeCharData.Char;
            for (int value = 0; value != escapeCharSize; ++value) escapeCharDataChar[value] = (char)value;
            escapeCharDataChar['0'] = (char)0;
            escapeCharDataChar['B'] = escapeCharDataChar['b'] = '\b';
            escapeCharDataChar['F'] = escapeCharDataChar['f'] = '\f';
            escapeCharDataChar['N'] = escapeCharDataChar['n'] = '\n';
            escapeCharDataChar['R'] = escapeCharDataChar['r'] = '\r';
            escapeCharDataChar['T'] = escapeCharDataChar['t'] = '\t';
            escapeCharDataChar['V'] = escapeCharDataChar['v'] = '\v';

            parseMethods = fastCSharp.dictionary.CreateOnly<Type, MethodInfo>();
            foreach (MethodInfo method in typeof(jsonParser).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (method.customAttribute<parseMethod>() != null)
                {
                    parseMethods.Add(method.GetParameters()[0].ParameterType.GetElementType(), method);
                }
            }
        }
    }
}
