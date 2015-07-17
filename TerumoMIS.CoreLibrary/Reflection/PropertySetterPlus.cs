//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PropertySetter
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  PropertySetter
//	User name:  C1400008
//	Location Time: 2015/7/16 14:50:40
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

namespace TerumoMIS.CoreLibrary.Reflection
{
    /// <summary>
    /// 属性设置器 
    /// </summary>
    internal static class PropertySetterPlus
    {
        /// <summary>
        /// 创建属性设置器
        /// </summary>
        /// <param name="targetType">目标对象类型</param>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>属性设置器,失败返回null</returns>
        private static object create(PropertyInfo property, bool nonPublic, Type targetType)
        {
            MethodInfo method = property.GetSetMethod(nonPublic);
            return method.GetParameters().Length == 1 ? Activator.CreateInstance(typeof(propertySetter<,>).MakeGenericType(targetType, property.PropertyType), method) : null;
        }
        /// <summary>
        /// 创建属性设置器
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>属性设置器,失败返回null</returns>
        public static object Create(PropertyInfo property, bool nonPublic)
        {
            return property != null && property.CanWrite ? create(property, nonPublic, property.DeclaringType) : null;
        }
        /// <summary>
        /// 创建属性设置器
        /// </summary>
        /// <typeparam name="targetType">目标对象类型</typeparam>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>属性设置器,失败返回null</returns>
        public static object Create<targetType>(PropertyInfo property, bool nonPublic)
        {
            return property != null && property.CanWrite ? create(property, nonPublic, typeof(targetType)) : null;
        }
    }
    /// <summary>
    /// 属性设置器
    /// </summary>
    /// <typeparam name="valueType">属性值类型</typeparam>
    public static class propertySetter<valueType>
    {
        /// <summary>
        /// 属性设置器 创建器
        /// </summary>
        public sealed class createPropertySetter : createProperty<Action<object, valueType>>
        {
            /// <summary>
            /// 创建属性设置器
            /// </summary>
            /// <param name="property">属性信息</param>
            /// <param name="nonPublic">是否非公有属性</param>
            /// <returns>属性设置器,失败返回null</returns>
            public override Action<object, valueType> Create(PropertyInfo property, bool nonPublic)
            {
                object value = propertySetter.Create(property, nonPublic);
                if (value != null) return ((IPropertySetter<valueType>)value).Set;
                return null;
            }
        }
        /// <summary>
        /// 属性设置器 创建器
        /// </summary>
        public static readonly createPropertySetter Creator = new createPropertySetter();
        /// <summary>
        /// 属性设置器 创建器
        /// </summary>
        public sealed class createPropertyTargetSetter : createProperty<Action<valueType, object>>
        {
            /// <summary>
            /// 创建属性设置器
            /// </summary>
            /// <param name="property">属性信息</param>
            /// <param name="nonPublic">是否非公有属性</param>
            /// <returns>属性设置器,失败返回null</returns>
            public override Action<valueType, object> Create(PropertyInfo property, bool nonPublic)
            {
                object value = propertySetter.Create<valueType>(property, nonPublic);
                if (value != null) return ((IPropertyTargetSetter<valueType>)value).Set;
                return null;
            }
            /// <summary>
            /// 属性设置器 创建器
            /// </summary>
            public static readonly createPropertyTargetSetter Default = new createPropertyTargetSetter();
        }
        /// <summary>
        /// 属性设置器 创建器
        /// </summary>
        public sealed class createPropertySetter<targetType> : createProperty<Action<targetType, valueType>>
        {
            /// <summary>
            /// 创建属性设置器
            /// </summary>
            /// <param name="property">属性信息</param>
            /// <param name="nonPublic">是否非公有属性</param>
            /// <returns>属性设置器,失败返回null</returns>
            public override Action<targetType, valueType> Create(PropertyInfo property, bool nonPublic)
            {
                object value = propertySetter.Create<targetType>(property, nonPublic);
                if (value != null) return ((propertySetter<targetType, valueType>)value).Setter;
                return null;
            }
            /// <summary>
            /// 属性设置器 创建器
            /// </summary>
            public static readonly createPropertySetter<targetType> Default = new createPropertySetter<targetType>();
        }
        /// <summary>
        /// 属性设置器 创建器
        /// </summary>
        public sealed class createPropertyStaticSetter : createProperty<Action<valueType>>
        {
            /// <summary>
            /// 创建属性设置器
            /// </summary>
            /// <param name="property">属性信息</param>
            /// <param name="nonPublic">是否非公有属性</param>
            /// <returns>属性设置器,失败返回null</returns>
            public override Action<valueType> Create(PropertyInfo property, bool nonPublic)
            {
                if (property != null && property.CanWrite)
                {
                    return (Action<valueType>)Delegate.CreateDelegate(typeof(Action<valueType>), property.GetGetMethod(nonPublic));
                }
                return null;
            }
        }
        /// <summary>
        /// 属性设置器 创建器
        /// </summary>
        public static readonly createPropertyStaticSetter StaticCreator = new createPropertyStaticSetter();
    }
    /// <summary>
    /// 属性设置器接口
    /// </summary>
    /// <typeparam name="valueType">属性值类型</typeparam>
    internal interface IPropertySetter<valueType>
    {
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="instance">目标对象</param>
        /// <param name="value">属性值</param>
        /// <returns>属性值</returns>
        void Set(object instance, valueType value);
    }
    /// <summary>
    /// 属性设置器接口
    /// </summary>
    /// <typeparam name="targetType">属性值类型</typeparam>
    internal interface IPropertyTargetSetter<targetType>
    {
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="instance">目标对象</param>
        /// <param name="value">属性值</param>
        /// <returns>属性值</returns>
        void Set(targetType instance, object value);
    }
    /// <summary>
    /// 属性设置器
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    /// <typeparam name="valueType">属性值类型</typeparam>
    internal sealed class propertySetter<targetType, valueType> : IPropertyTargetSetter<targetType>, IPropertySetter<valueType>
    {
        /// <summary>
        /// 属性获取器
        /// </summary>
        public Action<targetType, valueType> Setter;
        /// <summary>
        /// 属性获取器
        /// </summary>
        /// <param name="method">属性方法</param>
        public propertySetter(MethodInfo method)
        {
            Setter = (Action<targetType, valueType>)Delegate.CreateDelegate(typeof(Action<targetType, valueType>), null, method);
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="instance">目标对象</param>
        /// <param name="value">属性值</param>
        /// <returns>属性值</returns>
        public void Set(object instance, valueType value)
        {
            Setter((targetType)instance, value);
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="instance">目标对象</param>
        /// <param name="value">属性值</param>
        /// <returns>属性值</returns>
        public void Set(targetType instance, object value)
        {
            Setter(instance, (valueType)value);
        }
    }
}
