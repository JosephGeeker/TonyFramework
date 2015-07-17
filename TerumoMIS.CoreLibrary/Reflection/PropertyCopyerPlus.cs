//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PropertyCopyerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  PropertyCopyerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:48:32
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
    /// 属性复制器
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    public class PropertyCopyerPlus<targetType>:CreatePropertyPlus<Action<targetType,targetType>>
    {
        /// <summary>
        /// 创建属性设置器
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        /// <returns>属性设置器,失败返回null</returns>
        public override Action<targetType, targetType> Create(PropertyInfo property, bool nonPublic)
        {
            if (property != null && property.CanRead && property.CanWrite)
            {
                IPropertyCopyer<targetType> copyer = (IPropertyCopyer<targetType>)Activator.CreateInstance(typeof(propertyCopyer<,>).MakeGenericType(typeof(targetType), property.PropertyType), new object[] { property, nonPublic });
                if (copyer.IsCopyer) return copyer.Copy;
            }
            return null;
        }
        /// <summary>
        /// 属性设置器 创建器
        /// </summary>
        public static readonly propertyCopyer<targetType> Creator = new propertyCopyer<targetType>();
    }
    /// <summary>
    /// 属性复制器
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    internal interface IPropertyCopyer<targetType>
    {
        /// <summary>
        /// 是否属性复制器
        /// </summary>
        bool IsCopyer { get; }
        /// <summary>
        /// 属性复制
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="copyValue">被复制对象</param>
        void Copy(targetType value, targetType copyValue);
    }
    /// <summary>
    /// 属性复制器
    /// </summary>
    /// <typeparam name="targetType">目标对象类型</typeparam>
    /// <typeparam name="valueType">属性值类型</typeparam>
    internal sealed class propertyCopyer<targetType, valueType> : IPropertyCopyer<targetType>
    {
        /// <summary>
        /// 属性获取器
        /// </summary>
        public Func<targetType, valueType> Getter;
        /// <summary>
        /// 属性设置器
        /// </summary>
        public Action<targetType, valueType> Setter;
        /// <summary>
        /// 属性获取器
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="nonPublic">是否非公有属性</param>
        public propertyCopyer(PropertyInfo property, bool nonPublic)
        {
            MethodInfo getMethod = property.GetGetMethod(nonPublic);
            if (getMethod.GetParameters().Length == 0)
            {
                Getter = (Func<targetType, valueType>)Delegate.CreateDelegate(typeof(Func<targetType, valueType>), null, property.GetGetMethod(nonPublic));
                Setter = (Action<targetType, valueType>)Delegate.CreateDelegate(typeof(Action<targetType, valueType>), null, property.GetSetMethod(nonPublic));
            }
        }
        /// <summary>
        /// 是否属性复制器
        /// </summary>
        public bool IsCopyer
        {
            get { return Setter != null; }
        }
        /// <summary>
        /// 属性复制
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="copyValue">被复制对象</param>
        public void Copy(targetType value, targetType copyValue)
        {
            Setter(value, Getter(copyValue));
        }
    }
}
