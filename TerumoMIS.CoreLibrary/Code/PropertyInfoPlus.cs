//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PropertyInfoPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  PropertyInfoPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:21:30
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 属性信息
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    internal sealed class PropertyInfoPlus<TValueType>:MemberInfoPlus
    {
        /// <summary>
        /// 引用类型成员获取器
        /// </summary>
        public Func<TValueType, object> Getter;
        /// <summary>
        /// 引用类型成员设置器
        /// </summary>
        public Action<TValueType, object> Setter;
        /// <summary>
        /// 值类型成员获取器
        /// </summary>
        public Func<object, object> ValueGetter;
        /// <summary>
        /// 值类型成员设置器
        /// </summary>
        public Action<object, object> ValueSetter;
        /// <summary>
        /// 引用类型成员复制器
        /// </summary>
        public Action<TValueType, TValueType> Copyer;
        /// <summary>
        /// 获取值类型成员值
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">目标数据</param>
        /// <returns>成员值</returns>
        public object GetValue(TValueType value)
        {
            return ValueGetter(value);
        }

        /// <summary>
        /// 设置值类型成员值
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">目标数据</param>
        /// <param name="memberValue">成员值</param>
        /// <returns>成员值</returns>
        public void SetValue(TValueType value, object memberValue)
        {
            if (memberValue == null) throw new ArgumentNullException("memberValue");
            ValueSetter(value, memberValue);
        }

        /// <summary>
        /// 属性信息
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="isStruct">目标类型是否结构体</param>
        public PropertyInfoPlus(PropertyIndexPlus property, bool isStruct)
            : base(property)
        {
            var propertyInfo = property.Member;
            var memberType = propertyInfo.PropertyType;
            if (isStruct)
            {
                if (CanGet)
                {
                    var methodInfo = propertyInfo.GetGetMethod(true);
                    var dynamicMethod = new DynamicMethod("TerumoMISGetProperty_" + propertyInfo.Name, typeof(object), new[] { typeof(object) }, propertyInfo.DeclaringType);
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Unbox, typeof(TValueType));
                    generator.Emit(methodInfo.IsFinal || !methodInfo.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, methodInfo);
                    generator.Emit(memberType.IsClass || memberType.IsInterface ? OpCodes.Castclass : OpCodes.Box, memberType);
                    generator.Emit(OpCodes.Ret);
                    ValueGetter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
                }
                if (CanSet)
                {
                    var methodInfo = propertyInfo.GetSetMethod(true);
                    var dynamicMethod = new DynamicMethod("TerumoMISProperty_" + propertyInfo.Name, null, new[] { typeof(object), typeof(object) }, propertyInfo.DeclaringType);
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Unbox, typeof(TValueType));
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(memberType.IsClass || memberType.IsInterface ? OpCodes.Castclass : OpCodes.Unbox_Any, memberType);
                    generator.Emit(methodInfo.IsFinal || !methodInfo.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    ValueSetter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
                }
            }
            else
            {
                if (CanGet)
                {
                    Getter = propertyGetter<TValueType>.createPropertyTargetGetter.Default.Create(propertyInfo, true);
                    if (Getter == null) CanGet = false;
                }
                if (CanSet)
                {
                    Setter = propertySetter<TValueType>.createPropertyTargetSetter.Default.Create(propertyInfo, true);
                    if (Setter == null) CanSet = false;
                    else if (CanGet) Copyer = propertyCopyer<TValueType>.Creator.Create(propertyInfo, true);
                }
            }
        }
    }
}
