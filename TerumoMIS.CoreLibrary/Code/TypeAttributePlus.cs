//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TypeAttributePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  TypeAttributePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:34:02
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
    /// 类型自定义属性信息
    /// </summary>
    public static class TypeAttributePlus
    {
        /// <summary>
        /// 自定义属性信息集合
        /// </summary>
        private static interlocked.dictionary<Type, object[]> attributes = new interlocked.dictionary<Type, object[]>(dictionary.CreateOnly<Type, object[]>());
        /// <summary>
        /// 根据类型获取自定义属性信息集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>自定义属性信息集合</returns>
        private static object[] get(Type type)
        {
            object[] values;
            if (attributes.TryGetValue(type, out values)) return values;
            attributes.Set(type, values = type.GetCustomAttributes(false));
            return values;
        }
        /// <summary>
        /// 获取自定义属性集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <typeparam name="attributeType"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<attributeType> GetAttributes<attributeType>(Type type) where attributeType : Attribute
        {
            foreach (object value in get(type))
            {
                if (typeof(attributeType).IsAssignableFrom(value.GetType())) yield return (attributeType)value;
            }
        }
        /// <summary>
        /// 根据类型获取自定义属性
        /// </summary>
        /// <typeparam name="attributeType">自定义属性类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性</returns>
        public static attributeType GetAttribute<attributeType>(Type type, bool isBaseType, bool isInheritAttribute)
            where attributeType : Attribute
        {
            while (type != null && type != typeof(object))
            {
                foreach (attributeType attribute in GetAttributes<attributeType>(type))
                {
                    if (isInheritAttribute || attribute.GetType() == typeof(attributeType)) return (attributeType)attribute;
                }
                if (isBaseType) type = type.BaseType;
                else return null;
            }
            return null;
        }
    }
}
