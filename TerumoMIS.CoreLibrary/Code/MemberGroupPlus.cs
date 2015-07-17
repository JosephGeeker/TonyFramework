//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberGroupPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  MemberGroupPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:01:31
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

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 动态成员分组
    /// </summary>
    /// <typeparam name="TValueType">目标类型</typeparam>
    internal sealed class MemberGroupPlus<TValueType>
    {
        /// <summary>
        /// 公有动态字段
        /// </summary>
        private FieldInfoPlus<TValueType>[] _publicFields;
        /// <summary>
        /// 非公有动态字段
        /// </summary>
        private FieldInfoPlus<TValueType>[] _nonPublicFields;
        /// <summary>
        /// 公有动态属性
        /// </summary>
        private propertyInfo<TValueType>[] _publicProperties;
        /// <summary>
        /// 非公有动态属性
        /// </summary>
        private propertyInfo<TValueType>[] _nonPublicProperties;
        /// <summary>
        /// 成员数量
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// 成员集合
        /// </summary>
        public MemberInfoPlus[] Members
        {
            get
            {
                var members = new SubArrayStruct<MemberInfoPlus>(Count);
                members.Add(_publicFields.toGeneric<MemberInfoPlus>());
                members.Add(_nonPublicFields.toGeneric<MemberInfoPlus>());
                members.Add(_publicProperties.toGeneric<MemberInfoPlus>());
                members.Add(_nonPublicProperties.toGeneric<MemberInfoPlus>());
                return members.ToArray();
            }
        }
        /// <summary>
        /// 动态成员分组
        /// </summary>
        /// <param name="publicFields">公有动态字段</param>
        /// <param name="nonPublicFields">非公有动态字段</param>
        /// <param name="publicProperties">公有动态属性</param>
        /// <param name="nonPublicProperties">非公有动态属性</param>
        /// <param name="filter">成员选择</param>
        private MemberGroupPlus(SubArrayStruct<FieldInfoPlus<TValueType>> publicFields, SubArrayStruct<FieldInfoPlus<TValueType>> nonPublicFields
            , SubArrayStruct<propertyInfo<TValueType>> publicProperties, SubArrayStruct<propertyInfo<TValueType>> nonPublicProperties
            , MemberFiltersEnum filter)
        {
            _publicFields = (filter & MemberFiltersEnum.PublicInstanceField) != 0 ? publicFields.ToArray().notNull() : NullValuePlus<FieldInfoPlus<TValueType>>.Array;
            _nonPublicFields = (filter & MemberFiltersEnum.NonPublicInstanceField) != 0 ? nonPublicFields.ToArray().notNull() : NullValuePlus<FieldInfoPlus<TValueType>>.Array;
            _publicProperties = (filter & MemberFiltersEnum.PublicInstanceProperty) != 0 ? publicProperties.ToArray().notNull() : NullValuePlus<propertyInfo<TValueType>>.Array;
            _nonPublicProperties = (filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0 ? nonPublicProperties.ToArray().notNull() : NullValuePlus<propertyInfo<TValueType>>.Array;
            Count = _publicFields.Length + _nonPublicFields.Length
                + _publicProperties.Length + _nonPublicProperties.Length;
        }
        /// <summary>
        /// 获取成员名称与成员值集合
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="filter">成员选择</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>成员名称与成员值集合</returns>
        public SubArrayStruct<KeyValueStruct<MemberInfoPlus, object>> GetMemberValue(TValueType value
            , MemberFiltersEnum filter, MemberMapPlus memberMap)
        {
            return isStruct ? GetMemberValueValue(value, filter, memberMap) : GetMemberValue(value, filter, memberMap);
        }
        /// <summary>
        /// 引用类型获取成员名称与成员值集合
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="filter">成员选择</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>成员名称与成员值集合</returns>
        private SubArrayStruct<KeyValueStruct<MemberInfoPlus, object>> GetMemberValue(TValueType value, MemberFiltersEnum filter, MemberMapPlus<TValueType> memberMap)
        {
            var count = 0;
            var values = new KeyValueStruct<MemberInfoPlus, object>[Count];
            if (memberMap.IsDefault)
            {
                if ((filter & MemberFiltersEnum.PublicInstanceField) != 0)
                {
                    foreach (var field in _publicFields)
                    {
                        values[count++].Set(field, field.Getter(value));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceField) != 0)
                {
                    foreach (var field in _nonPublicFields)
                    {
                        values[count++].Set(field, field.Getter(value));
                    }
                }
                if ((filter & MemberFiltersEnum.PublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _publicProperties)
                    {
                        values[count++].Set(property, property.Getter(value));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _nonPublicProperties)
                    {
                        values[count++].Set(property, property.Getter(value));
                    }
                }
            }
            else
            {
                if ((filter & MemberFiltersEnum.PublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _publicFields)
                    {
                        if (memberMap.IsMember(field.MemberIndex))
                            values[count++].Set(field, field.Getter(value));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _nonPublicFields)
                    {
                        if (memberMap.IsMember(field.MemberIndex))
                            values[count++].Set(field, field.Getter(value));
                    }
                }
                if ((filter & MemberFiltersEnum.PublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _publicProperties)
                    {
                        if (memberMap.IsMember(property.MemberIndex))
                            values[count++].Set(property, property.Getter(value));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _nonPublicProperties)
                    {
                        if (memberMap.IsMember(property.MemberIndex))
                            values[count++].Set(property, property.Getter(value));
                    }
                }
            }
            return SubArrayStruct<KeyValueStruct<MemberInfoPlus, object>>.Unsafe(values, 0, count);
        }
        /// <summary>
        /// 值类型获取成员名称与成员值集合
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="filter">成员选择</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>成员名称与成员值集合</returns>
        private SubArrayStruct<KeyValueStruct<MemberInfoPlus, object>> GetMemberValueValue(TValueType value, MemberFiltersEnum filter, MemberMapPlus<TValueType> memberMap)
        {
            KeyValueStruct<MemberInfoPlus, object>[] values = new KeyValueStruct<MemberInfoPlus, object>[Count];
            object objectValue = value;
            int count = 0;
            if (memberMap.IsDefault)
            {
                if ((filter & MemberFiltersEnum.PublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _publicFields)
                    {
                        values[count++].Set(field, field.ValueGetter(objectValue));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _nonPublicFields)
                    {
                        values[count++].Set(field, field.ValueGetter(objectValue));
                    }
                }
                if ((filter & MemberFiltersEnum.PublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _publicProperties)
                    {
                        values[count++].Set(property, property.ValueGetter(objectValue));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _nonPublicProperties)
                    {
                        values[count++].Set(property, property.ValueGetter(objectValue));
                    }
                }
            }
            else
            {
                if ((filter & MemberFiltersEnum.PublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _publicFields)
                    {
                        if (memberMap.IsMember(field.MemberIndex))
                            values[count++].Set(field, field.ValueGetter(objectValue));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _nonPublicFields)
                    {
                        if (memberMap.IsMember(field.MemberIndex))
                            values[count++].Set(field, field.ValueGetter(objectValue));
                    }
                }
                if ((filter & MemberFiltersEnum.PublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _publicProperties)
                    {
                        if (memberMap.IsMember(property.MemberIndex))
                            values[count++].Set(property, property.ValueGetter(objectValue));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _nonPublicProperties)
                    {
                        if (memberMap.IsMember(property.MemberIndex))
                            values[count++].Set(property, property.ValueGetter(objectValue));
                    }
                }
            }
            return SubArrayStruct<KeyValueStruct<MemberInfoPlus, object>>.Unsafe(values, 0, count);
        }
        /// <summary>
        /// 引用类型设置成员值
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="values">成员值数组</param>
        /// <param name="isValueMap">成员值设置位图</param>
        public void SetMember(TValueType value, object[] values, fixedMap isValueMap)
        {
            foreach (FieldInfoPlus<TValueType> field in _publicFields)
            {
                if (isValueMap.Get(field.MemberIndex)) field.Setter(value, values[field.MemberIndex]);
            }
            foreach (FieldInfoPlus<TValueType> field in _nonPublicFields)
            {
                if (isValueMap.Get(field.MemberIndex)) field.Setter(value, values[field.MemberIndex]);
            }
            foreach (propertyInfo<TValueType> property in _publicProperties)
            {
                if (isValueMap.Get(property.MemberIndex)) property.Setter(value, values[property.MemberIndex]);
            }
            foreach (propertyInfo<TValueType> property in _nonPublicProperties)
            {
                if (isValueMap.Get(property.MemberIndex)) property.Setter(value, values[property.MemberIndex]);
            }
        }
        /// <summary>
        /// 值类型设置成员值
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="values">成员值数组</param>
        /// <param name="isValueMap">成员值设置位图</param>
        /// <returns>目标对象</returns>
        public TValueType SetMemberValue(TValueType value, object[] values, fixedMap isValueMap)
        {
            object objectValue = value;
            //object[] setValues = new object[1];
            foreach (FieldInfoPlus<TValueType> field in _publicFields)
            {
                if (isValueMap.Get(field.MemberIndex)) field.ValueSetter(objectValue, values[field.MemberIndex]);
            }
            foreach (FieldInfoPlus<TValueType> field in _nonPublicFields)
            {
                if (isValueMap.Get(field.MemberIndex)) field.ValueSetter(objectValue, values[field.MemberIndex]);
            }
            foreach (propertyInfo<TValueType> property in _publicProperties)
            {
                if (isValueMap.Get(property.MemberIndex))
                {
                    //setValues[0] = values[property.MemberIndex];
                    //property.SetValue(objectValue, setValues);
                    property.ValueSetter(objectValue, values[property.MemberIndex]);
                }
            }
            foreach (propertyInfo<TValueType> property in _nonPublicProperties)
            {
                if (isValueMap.Get(property.MemberIndex))
                {
                    //setValues[0] = values[property.MemberIndex];
                    //property.SetValue(objectValue, setValues);
                    property.ValueSetter(objectValue, values[property.MemberIndex]);
                }
            }
            return (TValueType)objectValue;
        }
        /// <summary>
        /// 引用类型成员复制
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="copyValue">被复制对象</param>
        /// <param name="filter">成员选择</param>
        /// <param name="memberMap">成员位图</param>
        public void Copy(TValueType value, TValueType copyValue, MemberFiltersEnum filter, MemberMapPlus<TValueType> memberMap)
        {
            if (memberMap.IsDefault)
            {
                if ((filter & MemberFiltersEnum.PublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _publicFields) field.Setter(value, field.Getter(copyValue));
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _nonPublicFields) field.Setter(value, field.Getter(copyValue));
                }
                if ((filter & MemberFiltersEnum.PublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _publicProperties) property.Copyer(value, copyValue);
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _nonPublicProperties) property.Copyer(value, copyValue);
                }
            }
            else
            {
                if ((filter & MemberFiltersEnum.PublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _publicFields)
                    {
                        if (memberMap.IsMember(field.MemberIndex)) field.Setter(value, field.Getter(copyValue));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _nonPublicFields)
                    {
                        if (memberMap.IsMember(field.MemberIndex)) field.Setter(value, field.Getter(copyValue));
                    }
                }
                if ((filter & MemberFiltersEnum.PublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _publicProperties)
                    {
                        if (memberMap.IsMember(property.MemberIndex)) property.Copyer(value, copyValue);
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _nonPublicProperties)
                    {
                        if (memberMap.IsMember(property.MemberIndex)) property.Copyer(value, copyValue);
                    }
                }
            }
        }
        /// <summary>
        /// 值类型成员复制
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="copyValue">被复制对象</param>
        /// <param name="filter">成员选择</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>目标对象</returns>
        public TValueType CopyValue(TValueType value, TValueType copyValue, MemberFiltersEnum filter, MemberMapPlus<TValueType> memberMap)
        {
            if (_publicFields.Length + _nonPublicFields.Length == allPublicFields.Length + allNonPublicFields.Length
                && (filter & MemberFiltersEnum.InstanceField) == MemberFiltersEnum.InstanceField && memberMap.IsDefault) return copyValue;
            object objectValue = value, objectCopyValue = copyValue;
            //object[] setValues = new object[1];
            if (memberMap.IsDefault)
            {
                if ((filter & MemberFiltersEnum.PublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _publicFields) field.ValueSetter(objectValue, field.ValueGetter(objectCopyValue));
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _nonPublicFields) field.ValueSetter(objectValue, field.ValueGetter(objectCopyValue));
                }
                if ((filter & MemberFiltersEnum.PublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _publicProperties)
                    {
                        //setValues[0] = property.GetValue(objectCopyValue);
                        //property.SetValue(objectValue, setValues);
                        property.ValueSetter(objectValue, property.ValueGetter(objectCopyValue));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _nonPublicProperties)
                    {
                        //setValues[0] = property.GetValue(objectCopyValue);
                        //property.SetValue(objectValue, setValues);
                        property.ValueSetter(objectValue, property.ValueGetter(objectCopyValue));
                    }
                }
            }
            else
            {
                if ((filter & MemberFiltersEnum.PublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _publicFields)
                    {
                        if (memberMap.IsMember(field.MemberIndex)) field.ValueSetter(objectValue, field.ValueGetter(objectCopyValue));
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceField) != 0)
                {
                    foreach (FieldInfoPlus<TValueType> field in _nonPublicFields)
                    {
                        if (memberMap.IsMember(field.MemberIndex)) field.ValueSetter(objectValue, field.ValueGetter(objectCopyValue));
                    }
                }
                if ((filter & MemberFiltersEnum.PublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _publicProperties)
                    {
                        if (memberMap.IsMember(property.MemberIndex))
                        {
                            //setValues[0] = property.GetValue(objectCopyValue);
                            //property.SetValue(objectValue, setValues);
                            property.ValueSetter(objectValue, property.ValueGetter(objectCopyValue));
                        }
                    }
                }
                if ((filter & MemberFiltersEnum.NonPublicInstanceProperty) != 0)
                {
                    foreach (propertyInfo<TValueType> property in _nonPublicProperties)
                    {
                        if (memberMap.IsMember(property.MemberIndex))
                        {
                            //setValues[0] = property.GetValue(objectCopyValue);
                            //property.SetValue(objectValue, setValues);
                            property.ValueSetter(objectValue, property.ValueGetter(objectCopyValue));
                        }
                    }
                }
            }
            return (TValueType)objectValue;
        }
        /// <summary>
        /// 是否值类型
        /// </summary>
        private readonly static bool isStruct = typeof(TValueType).isStruct();
        /// <summary>
        /// 公有动态字段
        /// </summary>
        private readonly static FieldInfoPlus<TValueType>[] allPublicFields;
        /// <summary>
        /// 非公有动态字段
        /// </summary>
        private readonly static FieldInfoPlus<TValueType>[] allNonPublicFields;
        /// <summary>
        /// 公有动态属性
        /// </summary>
        private readonly static propertyInfo<TValueType>[] allPublicProperties;
        /// <summary>
        /// 非公有动态属性
        /// </summary>
        private readonly static propertyInfo<TValueType>[] allNonPublicProperties;
        /// <summary>
        /// 成员集合
        /// </summary>
        public static MemberInfoPlus[] GetAllMembers()
        {
            SubArrayStruct<MemberInfoPlus> members = new SubArrayStruct<MemberInfoPlus>(memberIndexGroup<TValueType>.MemberCount);
            members.Add(allPublicFields.toGeneric<MemberInfoPlus>());
            members.Add(allNonPublicFields.toGeneric<MemberInfoPlus>());
            members.Add(allPublicProperties.toGeneric<MemberInfoPlus>());
            members.Add(allNonPublicProperties.toGeneric<MemberInfoPlus>());
            return members.ToArray();
        }
        /// <summary>
        /// 成员判定器
        /// </summary>
        /// <param name="value">成员信息</param>
        /// <returns>true</returns>
        private static bool defaultIsValue(MemberInfoPlus value)
        {
            return true;
        }
        /// <summary>
        /// 获取动态成员分组
        /// </summary>
        /// <typeparam name="attributeType">自定义属性类型</typeparam>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <param name="isValue">成员判定器</param>
        /// <param name="filter">成员选择</param>
        /// <returns>动态成员分组</returns>
        public static memberGroup<TValueType> Create<attributeType>(bool isAttribute, bool isBaseType, bool isInheritAttribute
            , Func<MemberInfoPlus, bool> isValue, MemberFiltersEnum filter)
            where attributeType : fastCSharp.code.ignoreOld
        {
            if (isValue == null) isValue = defaultIsValue;
            if (isAttribute)
            {
                return new memberGroup<TValueType>(allPublicFields.getFind(value => isValue(value) && value.IsAttribute<attributeType>(isBaseType, isInheritAttribute))
                    , allNonPublicFields.getFind(value => isValue(value) && value.IsAttribute<attributeType>(isBaseType, isInheritAttribute))
                    , allPublicProperties.getFind(value => isValue(value) && value.IsAttribute<attributeType>(isBaseType, isInheritAttribute))
                    , allNonPublicProperties.getFind(value => isValue(value) && value.IsAttribute<attributeType>(isBaseType, isInheritAttribute))
                    , filter);
            }
            else
            {
                return new memberGroup<TValueType>(allPublicFields.getFind(value => isValue(value) && !value.IsIgnoreAttribute<attributeType>(isBaseType, isInheritAttribute))
                    , allNonPublicFields.getFind(value => isValue(value) && !value.IsIgnoreAttribute<attributeType>(isBaseType, isInheritAttribute))
                    , allPublicProperties.getFind(value => isValue(value) && !value.IsIgnoreAttribute<attributeType>(isBaseType, isInheritAttribute))
                    , allNonPublicProperties.getFind(value => isValue(value) && !value.IsIgnoreAttribute<attributeType>(isBaseType, isInheritAttribute))
                    , filter);
            }
        }
        /// <summary>
        /// 获取动态成员分组
        /// </summary>
        /// <param name="isValue">成员判定器</param>
        /// <returns>动态成员分组</returns>
        public static memberGroup<TValueType> Create(Func<MemberInfoPlus, bool> isValue)
        {
            if (isValue == null) isValue = defaultIsValue;
            return new memberGroup<TValueType>(allPublicFields.getFind(value => !value.IsIgnore && isValue(value))
                    , allNonPublicFields.getFind(value => !value.IsIgnore && isValue(value))
                    , allPublicProperties.getFind(value => !value.IsIgnore && isValue(value))
                    , allNonPublicProperties.getFind(value => !value.IsIgnore && isValue(value))
                    , MemberFiltersEnum.Instance);
        }
        /// <summary>
        /// 获取成员值委托
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>获取成员值委托</returns>
        public static Func<TValueType, object> GetValue(MemberInfoPlus member)
        {
            FieldInfoPlus<TValueType> field = member as FieldInfoPlus<TValueType>;
            if (field != null) return isStruct ? field.GetValue : field.Getter;
            propertyInfo<TValueType> property = (propertyInfo<TValueType>)member;
            return isStruct ? property.GetValue : property.Getter;
        }
        /// <summary>
        /// 获取成员值委托
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <param name="convertType">成员转换类型</param>
        /// <returns>获取成员值委托</returns>
        public static Func<TValueType, object> GetConvertValue(MemberInfoPlus member, Type convertType)
        {
            Func<object, object> converter = reflection.converter.Get(member.MemberType.Type, convertType);
            return converter != null ? new converter { GetValue = GetValue(member), Converter = converter }.Get : GetValue(member);
        }
        /// <summary>
        /// 设置成员值委托
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>设置成员值委托</returns>
        public static Action<TValueType, object> SetValue(MemberInfoPlus member)
        {
            FieldInfoPlus<TValueType> field = member as FieldInfoPlus<TValueType>;
            if (field != null) return isStruct ? field.SetValue : field.Setter;
            propertyInfo<TValueType> property = (propertyInfo<TValueType>)member;
            return isStruct ? property.SetValue : property.Setter;
        }
        /// <summary>
        /// 类型转换委托
        /// </summary>
        private sealed class converter
        {
            /// <summary>
            /// 成员值获取委托
            /// </summary>
            public Func<TValueType, object> GetValue;
            /// <summary>
            /// 类型转换委托
            /// </summary>
            public Func<object, object> Converter;
            /// <summary>
            /// 成员值类型转换委托
            /// </summary>
            /// <param name="value">目标对象</param>
            /// <returns>返回值</returns>
            public object Get(TValueType value)
            {
                return Converter(GetValue(value));
            }
        }
        static memberGroup()
        {
            Type type = typeof(TValueType);
            if (type.getTypeName() == null)
            {
                memberIndexGroup group = memberIndexGroup.Get(type);
                allPublicFields = group.PublicFields.getArray(value => new FieldInfoPlus<TValueType>(value, isStruct));
                allNonPublicFields = group.NonPublicFields.getArray(value => new FieldInfoPlus<TValueType>(value, isStruct));
                allPublicProperties = group.PublicProperties.getArray(value => new propertyInfo<TValueType>(value, isStruct));
                allNonPublicProperties = group.NonPublicProperties.getArray(value => new propertyInfo<TValueType>(value, isStruct));
            }
            else
            {
                allPublicFields = allNonPublicFields = nullValue<FieldInfoPlus<TValueType>>.Array;
                allPublicProperties = allNonPublicProperties = nullValue<propertyInfo<TValueType>>.Array;
            }
        }
    }
}
