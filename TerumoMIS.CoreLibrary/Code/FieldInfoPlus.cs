//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FieldInfoPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  FieldInfoPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 16:22:11
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
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Code
{
    internal sealed class FieldInfoPlus<TValueType>:MemberInfoPlus
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
        /// 字段信息
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <param name="isStruct">目标类型是否结构体</param>
        public FieldInfoPlus(FieldIndexPlus field, bool isStruct)
            : base(field)
        {
            FieldInfo fieldInfo = field.Member;
            Type memberType = fieldInfo.FieldType;
            if (isStruct)
            {
                DynamicMethod dynamicMethod = new DynamicMethod("fastCSharpGetField_" + fieldInfo.Name, typeof(object), new Type[] { typeof(object) }, fieldInfo.DeclaringType);
                ILGenerator generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Unbox, typeof(TValueType));
                generator.Emit(OpCodes.Ldfld, fieldInfo);
                generator.Emit(memberType.IsClass || memberType.IsInterface ? OpCodes.Castclass : OpCodes.Box, memberType);
                generator.Emit(OpCodes.Ret);
                ValueGetter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));

                dynamicMethod = new DynamicMethod("fastCSharpSetField_" + fieldInfo.Name, null, new Type[] { typeof(object), typeof(object) }, fieldInfo.DeclaringType);
                generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Unbox, typeof(TValueType));
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(memberType.IsClass || memberType.IsInterface ? OpCodes.Castclass : OpCodes.Unbox_Any, memberType);
                generator.Emit(OpCodes.Stfld, fieldInfo);
                generator.Emit(OpCodes.Ret);
                ValueSetter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
            }
            else
            {
                DynamicMethod dynamicMethod = new DynamicMethod("fastCSharpGetField_" + fieldInfo.Name, typeof(object), new Type[] { typeof(TValueType) }, fieldInfo.DeclaringType);
                ILGenerator generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, fieldInfo);
                generator.Emit(memberType.IsClass || memberType.IsInterface ? OpCodes.Castclass : OpCodes.Box, memberType);
                generator.Emit(OpCodes.Ret);
                Getter = (Func<TValueType, object>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, object>));

                dynamicMethod = new DynamicMethod("fastCSharpSetField_" + fieldInfo.Name, null, new Type[] { typeof(TValueType), typeof(object) }, fieldInfo.DeclaringType);
                generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(memberType.IsClass || memberType.IsInterface ? OpCodes.Castclass : OpCodes.Unbox_Any, memberType);
                generator.Emit(OpCodes.Stfld, fieldInfo);
                generator.Emit(OpCodes.Ret);
                Setter = (Action<TValueType, object>)dynamicMethod.CreateDelegate(typeof(Action<TValueType, object>));
            }
            //Getter = fieldGetValue.Creator.Create(field.Member);
            //Setter = fieldSetValue.Creator.Create(field.Member);
        }
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
        /// <returns>成员值</returns>
        public void SetValue(TValueType value, object memberValue)
        {
            ValueSetter(value, memberValue);
        }
    }
}
