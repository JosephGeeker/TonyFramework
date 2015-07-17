//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberTypePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  MemberTypePlus
//	User name:  C1400008
//	Location Time: 2015/7/13 16:35:42
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TerumoMIS.CoreLibrary.Threading;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 成员类型
    /// </summary>
    public sealed partial class MemberTypePlus
    {
        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// SQL类型
        /// </summary>
        private Type _sqlType;
        /// <summary>
        /// 自定义类型名称
        /// </summary>
        private string _name;
        /// <summary>
        /// 类型名称
        /// </summary>
        private string _typeName;
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName
        {
            get
            {
                if (_typeName == null) _typeName = _name ?? (Type != null ? Type.Name: null);
                return _typeName;
            }
        }
        /// <summary>
        /// 类型名称
        /// </summary>
        private string _typeOnlyName;
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeOnlyName
        {
            get
            {
                if (_typeOnlyName == null) _typeOnlyName = _name == null ? (Type != null ? Type.onlyName() : null) : _name;
                return _typeOnlyName;
            }
        }
        /// <summary>
        /// 类型全名
        /// </summary>
        private string _fullName;
        /// <summary>
        /// 类型全名
        /// </summary>
        public string FullName
        {
            get
            {
                if (_fullName == null) _fullName = Type != null ? Type.fullName() : TypeName;
                return _fullName;
            }
        }
        /// <summary>
        /// 是否引用类型
        /// </summary>
        private bool? isNull;
        /// <summary>
        /// 是否引用类型
        /// </summary>
        public bool IsNull
        {
            get { return isNull == null ? Type == null || Type.isNull() : (bool)isNull; }
        }
        /// <summary>
        /// 是否object
        /// </summary>
        internal bool IsObject
        {
            get { return Type == typeof(object); }
        }
        /// <summary>
        /// 是否字符串
        /// </summary>
        public bool IsString
        {
            get { return Type == typeof(string); }
        }
        /// <summary>
        /// 是否字符串
        /// </summary>
        public bool IsSubString
        {
            get { return Type == typeof(SubStringStruct); }
        }
        /// <summary>
        /// 是否字符类型(包括可空类型)
        /// </summary>
        public bool IsChar
        {
            get { return Type == typeof(char) || Type == typeof(char?); }
        }
        /// <summary>
        /// 是否逻辑类型(包括可空类型)
        /// </summary>
        public bool IsBool
        {
            get { return Type == typeof(bool) || Type == typeof(bool?); }
        }
        /// <summary>
        /// 是否时间类型(包括可空类型)
        /// </summary>
        public bool IsDateTime
        {
            get { return Type == typeof(DateTime) || Type == typeof(DateTime?); }
        }
        /// <summary>
        /// 是否数字类型(包括可空类型)
        /// </summary>
        public bool IsDecimal
        {
            get { return Type == typeof(decimal) || Type == typeof(decimal?); }
        }
        /// <summary>
        /// 是否Guid类型(包括可空类型)
        /// </summary>
        public bool IsGuid
        {
            get { return Type == typeof(Guid) || Type == typeof(Guid?); }
        }
        /// <summary>
        /// 是否字节数组
        /// </summary>
        public bool IsByteArray
        {
            get { return Type == typeof(byte[]); }
        }
        /// <summary>
        /// 是否值类型(排除可空类型)
        /// </summary>
        public bool IsStruct
        {
            get { return Type.isStruct() && Type.nullableType() == null; }
        }
        /// <summary>
        /// 是否数组或者接口
        /// </summary>
        public bool IsArrayOrInterface
        {
            get
            {
                return Type.IsArray || Type.IsInterface;
            }
        }
        /// <summary>
        /// 是否字典
        /// </summary>
        public bool IsDictionary
        {
            get
            {
                return Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            }
        }
        /// <summary>
        /// 是否流
        /// </summary>
        public bool IsStream
        {
            get { return Type == typeof(Stream); }
        }
        /// <summary>
        /// 成员类型
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="isNull">是否引用类型</param>
        public MemberTypePlus(string name, bool isNull)
        {
            _name = name;
            this.isNull = isNull;
        }
        /// <summary>
        /// 成员类型
        /// </summary>
        /// <param name="type">类型</param>
        private MemberTypePlus(Type type)
        {
            Type = type;
        }
        /// <summary>
        /// 成员类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="sqlType">SQL类型</param>
        internal MemberTypePlus(Type type, Type sqlType)
            : this(type)
        {
            _sqlType = sqlType;
        }
        /// <summary>
        /// 空类型
        /// </summary>
        internal static readonly MemberTypePlus Null = new MemberTypePlus(null);
        /// <summary>
        /// 成员类型隐式转换集合
        /// </summary>
        private static readonly Dictionary<Type, MemberTypePlus> Types = DictionaryPlus.CreateOnly<Type, MemberTypePlus>();
        /// <summary>
        /// 隐式转换集合转换锁
        /// </summary>
        private static int _typeLock;
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="value">成员类型</param>
        /// <returns>类型</returns>
        public static implicit operator Type(MemberTypePlus value)
        {
            return value != null ? value.Type : null;
        }
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>成员类型</returns>
        public static implicit operator MemberTypePlus(Type type)
        {
            if (type == null) return Null;
            MemberTypePlus value;
            InterlockedPlus.CompareSetSleep(ref _typeLock);
            try
            {
                if (!Types.TryGetValue(type, out value)) Types.Add(type, value = new MemberTypePlus(type));
            }
            finally { _typeLock = 0; }
            return value;
        }
        /// <summary>
        /// 数组构造信息
        /// </summary>
        internal ConstructorInfo ArrayConstructor { get; private set; }
        /// <summary>
        /// 列表数组构造信息
        /// </summary>
        internal ConstructorInfo ListConstructor { get; private set; }
        /// <summary>
        /// 集合构造信息
        /// </summary>
        internal ConstructorInfo CollectionConstructor { get; private set; }
        /// <summary>
        /// 可枚举泛型构造信息
        /// </summary>
        internal ConstructorInfo EnumerableConstructor { get; private set; }
        /// <summary>
        /// 枚举基类类型
        /// </summary>
        public MemberTypePlus EnumUnderlyingType
        {
            get { return Type.GetEnumUnderlyingType(); }
        }
        /// <summary>
        /// 可枚举泛型类型
        /// </summary>
        private MemberTypePlus _enumerableType;
        /// <summary>
        /// 可枚举泛型类型
        /// </summary>
        public MemberTypePlus EnumerableType
        {
            get
            {
                if (_enumerableType == null)
                {
                    if (!IsString)
                    {
                        Type value = Type.getGenericInterface(typeof(IEnumerable<>));
                        if (value != null)
                        {
                            if (Type.IsInterface)
                            {
                                var interfaceType = Type.GetGenericTypeDefinition();
                                if (interfaceType == typeof(IEnumerable<>) || interfaceType == typeof(ICollection<>)
                                    || interfaceType == typeof(IList<>))
                                {
                                    _enumerableArgumentType = value.GetGenericArguments()[0];
                                    _enumerableType = value;
                                }
                            }
                            else if (Type.IsArray)
                            {
                                _enumerableArgumentType = value.GetGenericArguments()[0];
                                _enumerableType = value;
                            }
                            else
                            {
                                var enumerableArgumentType = value.GetGenericArguments()[0];
                                var parameters = new Type[1];
                                parameters[0] = enumerableArgumentType.MakeArrayType();
                                ArrayConstructor = Type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                                if (ArrayConstructor != null)
                                {
                                    _enumerableArgumentType = enumerableArgumentType;
                                    _enumerableType = value;
                                }
                                else
                                {
                                    parameters[0] = typeof(IList<>).MakeGenericType(enumerableArgumentType);
                                    ListConstructor = Type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                                    if (ListConstructor != null)
                                    {
                                        _enumerableArgumentType = enumerableArgumentType;
                                        _enumerableType = value;
                                    }
                                    else
                                    {
                                        parameters[0] = typeof(ICollection<>).MakeGenericType(enumerableArgumentType);
                                        CollectionConstructor = Type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                                        if (CollectionConstructor != null)
                                        {
                                            _enumerableArgumentType = enumerableArgumentType;
                                            _enumerableType = value;
                                        }
                                        else
                                        {
                                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(enumerableArgumentType);
                                            EnumerableConstructor = Type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null);
                                            if (EnumerableConstructor != null)
                                            {
                                                _enumerableArgumentType = enumerableArgumentType;
                                                _enumerableType = value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (_enumerableType == null) _enumerableType = Null;
                }
                return _enumerableType.Type != null ? _enumerableType : null;
            }
        }
        /// <summary>
        /// 是否可枚举类型
        /// </summary>
        public bool IsEnumerable
        {
            get
            {
                return EnumerableType != null;
            }
        }
        /// <summary>
        /// 可枚举泛型参数类型
        /// </summary>
        private MemberTypePlus _enumerableArgumentType;
        /// <summary>
        /// 可枚举泛型参数类型
        /// </summary>
        public MemberTypePlus EnumerableArgumentType
        {
            get
            {
                return EnumerableType != null ? _enumerableArgumentType : null;
            }
        }
        /// <summary>
        /// 可控类型的值类型
        /// </summary>
        private MemberTypePlus _nullType;
        /// <summary>
        /// 可控类型的值类型
        /// </summary>
        public MemberTypePlus NullType
        {
            get
            {
                if (_nullType == null) _nullType = (MemberTypePlus)Type.nullableType();
                return _nullType.Type != null ? _nullType : null;
            }
        }
        /// <summary>
        /// 非可控类型为null
        /// </summary>
        public MemberTypePlus NotNullType
        {
            get { return NullType != null ? _nullType : this; }
        }
        /// <summary>
        /// 非可控类型为null
        /// </summary>
        public string StructNotNullType
        {
            get
            {
                if (NotNullType.Type.IsEnum) return NotNullType.Type.GetEnumUnderlyingType().fullName();
                return NotNullType.FullName;
            }
        }
        /// <summary>
        /// 结构体非可空类型
        /// </summary>
        private string _structType;
        /// <summary>
        /// 结构体非可空类型
        /// </summary>
        public string StructType
        {
            get
            {
                if (_structType == null)
                {
                    Type type = Type.nullableType();
                    _structType = type == null ? fullName : type.fullName();
                }
                return _structType;
            }
        }
        /// <summary>
        /// 是否拥有静态转换函数
        /// </summary>
        private bool? _isTryParse;
        /// <summary>
        /// 是否拥有静态转换函数
        /// </summary>
        internal bool IsTryParse
        {
            get
            {
                if (_isTryParse == null) _isTryParse = (Type.nullableType() ?? Type).getTryParse() != null;
                return (bool)_isTryParse;
            }
        }
        /// <summary>
        /// 键值对键类型
        /// </summary>
        internal MemberTypePlus pairKeyType;
        /// <summary>
        /// 键值对键类型
        /// </summary>
        public MemberTypePlus PairKeyType
        {
            get
            {
                if (pairKeyType == null)
                {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        pairKeyType = Type.GetGenericArguments()[0];
                    }
                    else pairKeyType = Null;
                }
                return pairKeyType.Type != null ? pairKeyType : null;
            }
        }
        /// <summary>
        /// 键值对值类型
        /// </summary>
        internal MemberTypePlus pairValueType;
        /// <summary>
        /// 键值对值类型
        /// </summary>
        public MemberTypePlus PairValueType
        {
            get
            {
                if (pairValueType == null)
                {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        pairValueType = Type.GetGenericArguments()[1];
                    }
                    else pairValueType = Null;
                }
                return pairValueType.Type != null ? pairValueType : null;
            }
        }
        /// <summary>
        /// 键值对键类型
        /// </summary>
        internal MemberTypePlus keyValueKeyType;
        /// <summary>
        /// 键值对键类型
        /// </summary>
        public MemberTypePlus KeyValueKeyType
        {
            get
            {
                if (keyValueKeyType == null)
                {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(KeyValueStruct<,>))
                    {
                        keyValueKeyType = Type.GetGenericArguments()[0];
                    }
                    else keyValueKeyType = Null;
                }
                return keyValueKeyType.Type != null ? keyValueKeyType : null;
            }
        }
        /// <summary>
        /// 键值对键类型
        /// </summary>
        internal MemberTypePlus keyValueValueType;
        /// <summary>
        /// 键值对键类型
        /// </summary>
        public MemberTypePlus KeyValueValueType
        {
            get
            {
                if (keyValueValueType == null)
                {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(KeyValueStruct<,>))
                    {
                        keyValueValueType = Type.GetGenericArguments()[1];
                    }
                    else keyValueValueType = Null;
                }
                return keyValueValueType.Type != null ? keyValueValueType : null;
            }
        }
        /// <summary>
        /// 泛型参数集合
        /// </summary>
        private MemberTypePlus[] _genericParameters;
        /// <summary>
        /// 泛型参数集合
        /// </summary>
        internal MemberTypePlus[] GenericParameters
        {
            get
            {
                if (_genericParameters == null)
                {
                    _genericParameters = Type.IsGenericType ? Type.GetGenericArguments().GetArray(value => (MemberTypePlus)value) : NullValuePlus<MemberTypePlus>.Array;
                }
                return _genericParameters;
            }
        }
        /// <summary>
        /// 泛型参数名称
        /// </summary>
        public string GenericParameterNames
        {
            get
            {
                return GenericParameters.JoinString(',', value => value.FullName);
            }
        }
    }
}
