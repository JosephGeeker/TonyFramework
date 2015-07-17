//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FieldSetValuePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  FieldSetValuePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:45:15
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Code.Csharper;

namespace TerumoMIS.CoreLibrary.Reflection
{
    /// <summary>
    /// 字段设置器
    /// </summary>
    public static class FieldSetValuePlus
    {
        /// <summary>
        /// 域初始化属性设置器
        /// </summary>
        private static readonly Func<object, bool> getDomainInitialized = propertyGetter<bool>.Creator.Create(typeof(RuntimeFieldHandle).Assembly, "System.RuntimeType", "DomainInitialized", true);
        /// <summary>
        /// 字段值设置方法信息
        /// </summary>
        private static readonly MethodInfo runtimeFieldHandleSetValue = typeof(RuntimeFieldHandle).GetMethod("SetValue", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// .NET4.0字段值设置方法信息
        /// </summary>
        private static readonly MethodInfo runtimeFieldHandleSetValue4 = typeof(RuntimeFieldHandle).GetMethod("SetValue", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 字段值设置委托
        /// </summary>
        /// <param name="instance">目标对象</param>
        /// <param name="object">字段值</param>
        /// <param name="fieldType">字段值类型</param>
        /// <param name="fieldAttributes">字段属性描述</param>
        /// <param name="declaringType">定义字段的类型</param>
        /// <param name="domainInitialized">域初始化状态</param>
        /// <returns>字段值</returns>
        private delegate void SetValueDelegate(object instance, object value, RuntimeTypeHandle fieldType, FieldAttributes fieldAttributes, RuntimeTypeHandle declaringType, ref bool domainInitialized);
        /// <summary>
        /// 字段设置器接口
        /// </summary>
        private interface IFieldSetValue
        {
            /// <summary>
            /// 设置字段值
            /// </summary>
            /// <param name="instance">目标对象</param>
            /// <param name="value">字段值</param>
            void SetObject(object instance, object value);
        }
        /// <summary>
        /// .NET4.0字段设置器接口
        /// </summary>
        /// <typeparam name="valueType">字段值类型</typeparam>
        private interface IFieldSetValue<valueType> : IFieldSetValue
        {
            /// <summary>
            /// 设置字段值
            /// </summary>
            /// <param name="instance">目标对象</param>
            /// <param name="value">字段值</param>
            void SetValue(object instance, valueType value);
        }
        /// <summary>
        /// 字段设置器
        /// </summary>
        /// <typeparam name="valueType">字段值类型</typeparam>
        private sealed class setter<valueType> : IFieldSetValue
        {
            /// <summary>
            /// 字段值设置委托
            /// </summary>
            private SetValueDelegate SetValueDelegate;
            /// <summary>
            /// 字段值类型
            /// </summary>
            private RuntimeTypeHandle fieldType;
            /// <summary>
            /// 字段属性描述
            /// </summary>
            private FieldAttributes fieldAttributes;
            /// <summary>
            /// 定义字段的类型
            /// </summary>
            private RuntimeTypeHandle declaringType;
            /// <summary>
            /// 域初始化状态
            /// </summary>
            private bool domainInitialized;
            /// <summary>
            /// 字段设置器
            /// </summary>
            /// <param name="field">字段信息</param>
            public setter(FieldInfo field)
            {
                SetValueDelegate = (fieldSetValue.SetValueDelegate)Delegate.CreateDelegate(typeof(fieldSetValue.SetValueDelegate), field.FieldHandle, fieldSetValue.runtimeFieldHandleSetValue);
                fieldType = field.FieldType.TypeHandle;
                fieldAttributes = field.Attributes;
                declaringType = field.DeclaringType.TypeHandle;
                domainInitialized = fieldSetValue.getDomainInitialized(field.DeclaringType);
            }
            /// <summary>
            /// 设置字段值
            /// </summary>
            /// <param name="instance">目标对象</param>
            /// <param name="value">字段值</param>
            public void SetValue(object instance, valueType value)
            {
                SetObject(instance, value);
            }
            /// <summary>
            /// 设置字段值
            /// </summary>
            /// <param name="instance">目标对象</param>
            /// <param name="value">字段值</param>
            public void SetObject(object instance, object value)
            {
                SetValueDelegate(instance, value, fieldType, fieldAttributes, declaringType, ref domainInitialized);
            }
        }
        /// <summary>
        /// .NET4.0字段设置器
        /// </summary>
        /// <typeparam name="valueType">字段值类型</typeparam>
        /// <typeparam name="RtFieldInfoType">字段类型</typeparam>
        /// <typeparam name="RuntimeType">运行时类型</typeparam>
        private sealed class setter<valueType, RtFieldInfoType, RuntimeType> : IFieldSetValue<valueType>
            where RtFieldInfoType : FieldInfo
            where RuntimeType : Type
        {
            /// <summary>
            /// 字段值设置委托
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="instance">目标对象</param>
            /// <param name="object">字段值</param>
            /// <param name="fieldType">字段值类型</param>
            /// <param name="fieldAttributes">字段属性描述</param>
            /// <param name="declaringType">定义字段的类型</param>
            /// <param name="domainInitialized">域初始化状态</param>
            /// <returns>字段值</returns>
            private delegate void setValue(RtFieldInfoType field, object instance, object value, RuntimeType fieldType, FieldAttributes fieldAttributes, RuntimeType declaringType, ref bool domainInitialized);
            /// <summary>
            /// 字段值设置委托
            /// </summary>
            private setValue SetValueDelegate;
            /// <summary>
            /// 字段信息
            /// </summary>
            private RtFieldInfoType field;
            /// <summary>
            /// 字段值类型
            /// </summary>
            private RuntimeType fieldType;
            /// <summary>
            /// 字段属性描述
            /// </summary>
            private FieldAttributes fieldAttributes;
            /// <summary>
            /// 定义字段的类型
            /// </summary>
            private RuntimeType declaringType;
            /// <summary>
            /// 域初始化状态
            /// </summary>
            private bool domainInitialized;
            /// <summary>
            /// 字段设置器
            /// </summary>
            /// <param name="field">字段信息</param>
            public setter(FieldInfo field)
            {
                SetValueDelegate = (setValue)Delegate.CreateDelegate(typeof(setValue), fieldSetValue.runtimeFieldHandleSetValue4);
                this.field = (RtFieldInfoType)field;
                fieldType = (RuntimeType)field.FieldType;
                fieldAttributes = field.Attributes;
                declaringType = (RuntimeType)field.DeclaringType;
                domainInitialized = fieldSetValue.getDomainInitialized(field.DeclaringType);
            }
            /// <summary>
            /// 设置字段值
            /// </summary>
            /// <param name="instance">目标对象</param>
            /// <param name="value">字段值</param>
            public void SetValue(object instance, valueType value)
            {
                SetObject(instance, value);
            }
            /// <summary>
            /// 设置字段值
            /// </summary>
            /// <param name="instance">目标对象</param>
            /// <param name="value">字段值</param>
            public void SetObject(object instance, object value)
            {
                SetValueDelegate(field, instance, value, fieldType, fieldAttributes, declaringType, ref domainInitialized);
            }
        }
        /// <summary>
        /// 字段设置器 创建器
        /// </summary>
        public sealed class createFieldSetValue : createField<Action<object, object>>
        {
            /// <summary>
            /// 创建字段设置器
            /// </summary>
            /// <typeparam name="valueType">返回值类型</typeparam>
            /// <param name="field">字段信息</param>
            /// <returns>字段设置器</returns>
            public override Action<object, object> Create(FieldInfo field)
            {
                if (field != null)
                {
                    if (runtimeFieldHandleSetValue != null) return ((IFieldSetValue)Activator.CreateInstance(typeof(setter<>).MakeGenericType(field.FieldType), field)).SetObject;
                    return ((IFieldSetValue)Activator.CreateInstance(typeof(setter<,,>).MakeGenericType(field.FieldType, MemoryDataBaseModelPlus.fieldInfo.RtFieldInfoType, type.RuntimeType), field)).SetObject;
                }
                return null;
            }
        }
        /// <summary>
        /// 字段设置器 创建器
        /// </summary>
        public static readonly createFieldSetValue Creator = new createFieldSetValue();
        /// <summary>
        /// 字段设置器 创建器
        /// </summary>
        public sealed class createFieldSetValue<valueType> : createField<Action<object, valueType>>
        {
            /// <summary>
            /// 创建字段设置器
            /// </summary>
            /// <typeparam name="valueType">返回值类型</typeparam>
            /// <param name="field">字段信息</param>
            /// <returns>字段设置器</returns>
            public override Action<object, valueType> Create(FieldInfo field)
            {
                if (field != null)
                {
                    if (runtimeFieldHandleSetValue != null) return new setter<valueType>(field).SetValue;
                    return ((IFieldSetValue<valueType>)Activator.CreateInstance(typeof(setter<,,>).MakeGenericType(field.FieldType, MemoryDataBaseModelPlus.fieldInfo.RtFieldInfoType, type.RuntimeType), field)).SetValue;
                }
                return null;
            }
            /// <summary>
            /// 字段设置器 创建器
            /// </summary>
            public static readonly createFieldSetValue<valueType> Default = new createFieldSetValue<valueType>();
        }
    }
}
