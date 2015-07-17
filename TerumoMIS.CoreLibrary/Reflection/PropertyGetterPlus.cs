//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PropertyGetter
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  PropertyGetter
//	User name:  C1400008
//	Location Time: 2015/7/16 14:49:53
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
    /// 属性获取器
    /// </summary>
    internal static class PropertyGetterPlus
    {
        /// <summary>
        /// 创建属性获取器
        /// </summary>
        /// <param name="targetType">目标对象类型</param>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>属性获取器,失败返回null</returns>
        private static object create(PropertyInfo property, bool nonPublic, Type targetType)
        {
            MethodInfo method = property.GetGetMethod(nonPublic);
            return method.GetParameters().Length == 0 ? Activator.CreateInstance(typeof(propertyGetter<,>).MakeGenericType(targetType, property.PropertyType), method) : null;
        }
        /// <summary>
        /// 创建属性获取器
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>属性获取器,失败返回null</returns>
        public static object Create(PropertyInfo property, bool nonPublic)
        {
            return property != null && property.CanRead ? create(property, nonPublic, property.DeclaringType) : null;
        }
        /// <summary>
        /// 创建属性获取器
        /// </summary>
        /// <typeparam name="targetType">目标对象类型</typeparam>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>属性获取器,失败返回null</returns>
        public static object Create<targetType>(PropertyInfo property, bool nonPublic)
        {
            return property != null && property.CanRead ? create(property, nonPublic, typeof(targetType)) : null;
        }
    }
    /// <summary>
    /// 属性获取器
    /// </summary>
    /// <typeparam name="valueType">属性值类型</typeparam>
    public static class propertyGetter<valueType>
    {
        /// <summary>
        /// 属性获取器 创建器
        /// </summary>
        public sealed class createPropertyGetter : createProperty<Func<object, valueType>>
        {
            /// <summary>
            /// 创建属性获取器
            /// </summary>
            /// <param name="property">属性信息</param>
            /// <param name="nonPublic">是否非公有属性</param>
            /// <returns>属性获取器,失败返回null</returns>
            public override Func<object, valueType> Create(PropertyInfo property, bool nonPublic)
            {
                object value = propertyGetter.Create(property, nonPublic);
                if (value != null) return ((IPropertyGetter<valueType>)value).Get;
                return null;
            }
        }
        /// <summary>
        /// 属性获取器 创建器
        /// </summary>
        public static readonly createPropertyGetter Creator = new createPropertyGetter();
        /// <summary>
        /// 属性获取器 创建器
        /// </summary>
        public sealed class createPropertyTargetGetter : createProperty<Func<valueType, object>>
        {
            /// <summary>
            /// 创建属性获取器
            /// </summary>
            /// <param name="property">属性信息</param>
            /// <param name="nonPublic">是否非公有属性</param>
            /// <returns>属性获取器,失败返回null</returns>
            public override Func<valueType, object> Create(PropertyInfo property, bool nonPublic)
            {
                object value = propertyGetter.Create<valueType>(property, nonPublic);
                if (value != null) return ((IPropertyTargetGetter<valueType>)value).Get;
                return null;
            }
            /// <summary>
            /// 属性获取器 创建器
            /// </summary>
            public static readonly createPropertyTargetGetter Default = new createPropertyTargetGetter();
        }
        /// <summary>
        /// 属性获取器 创建器
        /// </summary>
        /// <typeparam name="targetType">目标对象类型</typeparam>
        public sealed class createPropertyGetter<targetType> : createProperty<Func<targetType, valueType>>
        {
            /// <summary>
            /// 创建属性获取器
            /// </summary>
            /// <param name="property">属性信息</param>
            /// <param name="nonPublic">是否非公有属性</param>
            /// <returns>属性获取器,失败返回null</returns>
            public override Func<targetType, valueType> Create(PropertyInfo property, bool nonPublic)
            {
                object value = propertyGetter.Create<targetType>(property, nonPublic);
                if (value != null) return ((propertyGetter<targetType, valueType>)value).Getter;
                return null;
            }
            /// <summary>
            /// 属性获取器 创建器
            /// </summary>
            public static readonly createPropertyGetter<targetType> Default = new createPropertyGetter<targetType>();
        }
        /// <summary>
        /// 属性获取器 创建器
        /// </summary>
        public sealed class createPropertyStaticGetter : createProperty<Func<valueType>>
        {
            /// <summary>
            /// 创建属性获取器
            /// </summary>
            /// <param name="property">属性信息</param>
            /// <param name="nonPublic">是否非公有属性</param>
            /// <returns>属性获取器,失败返回null</returns>
            public override Func<valueType> Create(PropertyInfo property, bool nonPublic)
            {
                if (property != null && property.CanRead)
                {
                    return (Func<valueType>)Delegate.CreateDelegate(typeof(Func<valueType>), property.GetGetMethod(nonPublic));
                }
                return null;
            }
        }
        /// <summary>
        /// 属性获取器 创建器
        /// </summary>
        public static readonly createPropertyStaticGetter StaticCreator = new createPropertyStaticGetter();
    }
    /// <summary>
    /// 属性获取器接口
    /// </summary>
    /// <typeparam name="valueType">属性值类型</typeparam>
    internal interface IPropertyGetter<valueType>
    {
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <returns>属性值</returns>
        valueType Get(object value);
    }
    /// <summary>
    /// 属性获取器接口
    /// </summary>
    /// <typeparam name="targetType">属性值类型</typeparam>
    internal interface IPropertyTargetGetter<targetType>
    {
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <returns>属性值</returns>
        object Get(targetType value);
    }
    /// <summary>
    /// 属性获取器
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    /// <typeparam name="valueType">属性值类型</typeparam>
    internal sealed class propertyGetter<targetType, valueType> : IPropertyTargetGetter<targetType>, IPropertyGetter<valueType>
    {
        /// <summary>
        /// 属性获取器
        /// </summary>
        public Func<targetType, valueType> Getter;
        /// <summary>
        /// 属性获取器
        /// </summary>
        /// <param name="method">属性方法</param>
        public propertyGetter(MethodInfo method)
        {
            Getter = (Func<targetType, valueType>)Delegate.CreateDelegate(typeof(Func<targetType, valueType>), null, method);
        }
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <returns>属性值</returns>
        public valueType Get(object value)
        {
            return Getter((targetType)value);
        }
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <returns>属性值</returns>
        public object Get(targetType value)
        {
            return Getter(value);
        }
    }
}
