//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ConverterPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  ConverterPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:40:32
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
    /// 类型转换函数
    /// </summary>
    public static class ConverterPlus
    {
        /// <summary>
        /// 数组转换器
        /// </summary>
        private interface IArrayConverter
        {
            /// <summary>
            /// 数组转换
            /// </summary>
            /// <param name="arrayObject">原数组</param>
            /// <returns>目标数组</returns>
            object Get(object arrayObject);
        }
        /// <summary>
        /// 数组转换器
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <typeparam name="convertType">目标类型</typeparam>
        private sealed class arrayConverter<valueType, convertType> : IArrayConverter
        {
            /// <summary>
            /// 类型转换器
            /// </summary>
            private Func<object, object> converter;
            /// <summary>
            /// 数组转换器
            /// </summary>
            /// <param name="converter">类型转换器</param>
            public arrayConverter(Func<object, object> converter)
            {
                this.converter = converter;
            }
            /// <summary>
            /// 数组转换
            /// </summary>
            /// <param name="arrayObject">原数组</param>
            /// <returns>目标数组</returns>
            public object Get(object arrayObject)
            {
                if (arrayObject == null) return null;
                valueType[] array = (valueType[])arrayObject;
                convertType[] newArray = new convertType[array.Length];
                int index = 0;
                foreach (valueType value in array) newArray[index++] = (convertType)converter(value);
                return newArray;
            }
        }
        /// <summary>
        /// 获取数组类型转换函数
        /// </summary>
        /// <param name="type">原类型</param>
        /// <param name="convertType">目标类型</param>
        /// <returns>数组类型转换函数,失败返回null</returns>
        public static Func<object, object> GetArray(Type type, Type convertType)
        {
            Func<object, object> value = Get(type, convertType);
            if (value != null)
            {
                return ((IArrayConverter)Activator.CreateInstance(typeof(arrayConverter<,>).MakeGenericType(type, convertType), value)).Get;
            }
            return null;
        }
        /// <summary>
        /// 获取类型转换函数
        /// </summary>
        /// <param name="type">原类型</param>
        /// <param name="convertType">目标类型</param>
        /// <returns>类型转换函数,失败返回null</returns>
        public static Func<object, object> Get(Type type, Type convertType)
        {
            if (type == convertType) return getSelf;
            MethodInfo method = getMethod(type, convertType);
            if (method != null) return staticMethodDelegate.Create(method);
            if (typeof(IConvertible).IsAssignableFrom(type))
            {
                Func<object, object> func;
                if (getConvertibles.TryGetValue(convertType, out func)) return func;
                return staticMethodDelegate.Create(getConvertibleMethodInfo.MakeGenericMethod(convertType));
            }
            return null;
        }
        /// <summary>
        /// 获取类型转换函数
        /// </summary>
        /// <param name="type">原类型</param>
        /// <param name="convertType">目标类型</param>
        /// <returns>类型转换函数,失败返回null</returns>
        private static MethodInfo getMethod(Type type, Type convertType)
        {
            if (type.isInherit(convertType)) return getBaseMethodInfo.MakeGenericMethod(type, convertType);
            if (convertType.isInherit(type)) return getMethodInfo.MakeGenericMethod(type, convertType);
            MethodInfo method = type.getImplicitMethod(convertType);
            if (method != null) return method;
            if (convertType.IsEnum)
            {
                return (getEnumMethods.TryGetValue(type, out method) ? method : getEnumMethodInfo).MakeGenericMethod(convertType);
            }
            return null;
        }
        /// <summary>
        /// 无类型转换
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getSelf(object value)
        {
            return value;
        }
        /// <summary>
        /// 转换为基类函数
        /// </summary>
        /// <typeparam name="valueType">原类型</typeparam>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getBase<valueType, convertType>(object value) where valueType : convertType
        {
            return (convertType)(valueType)value;
        }
        /// <summary>
        /// 转换为子类函数
        /// </summary>
        /// <typeparam name="valueType">原类型</typeparam>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object get<valueType, convertType>(object value) where convertType : valueType
        {
            return (convertType)(valueType)value;
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getConvertible<convertType>(object value)
        {
            return ((IConvertible)value).ToType(typeof(convertType), null);
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnumByte<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), ((IConvertible)value).ToByte(null));
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnumSByte<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), ((IConvertible)value).ToSByte(null));
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnumUShort<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), ((IConvertible)value).ToUInt16(null));
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnumShort<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), ((IConvertible)value).ToInt16(null));
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnumUInt<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), ((IConvertible)value).ToUInt32(null));
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnumInt<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), ((IConvertible)value).ToInt32(null));
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnumULong<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), ((IConvertible)value).ToUInt64(null));
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnumLong<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), ((IConvertible)value).ToInt64(null));
        }
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        /// <typeparam name="convertType">目标类型</typeparam>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getEnum<convertType>(object value)
        {
            return System.Enum.ToObject(typeof(convertType), value);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getBool(object value)
        {
            return ((IConvertible)value).ToBoolean(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getByte(object value)
        {
            return ((IConvertible)value).ToByte(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getChar(object value)
        {
            return ((IConvertible)value).ToChar(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getDateTime(object value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getDecimal(object value)
        {
            return ((IConvertible)value).ToDecimal(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getDouble(object value)
        {
            return ((IConvertible)value).ToDouble(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getShort(object value)
        {
            return ((IConvertible)value).ToInt16(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getInt(object value)
        {
            return ((IConvertible)value).ToInt32(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getLong(object value)
        {
            return ((IConvertible)value).ToInt64(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getSByte(object value)
        {
            return ((IConvertible)value).ToSByte(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getFloat(object value)
        {
            return ((IConvertible)value).ToSingle(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getString(object value)
        {
            return ((IConvertible)value).ToString(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getUShort(object value)
        {
            return ((IConvertible)value).ToUInt16(null);
        }
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getUInt(object value)
        {
            return ((IConvertible)value).ToUInt32(null);
        }
        /// <summary>
        /// <summary>
        /// 转换接口函数
        /// </summary>
        /// <param name="value">原数据</param>
        /// <returns>目标数据</returns>
        private static object getULong(object value)
        {
            return ((IConvertible)value).ToUInt64(null);
        }
        /// 转换为基类函数
        /// </summary>
        private static readonly MethodInfo getBaseMethodInfo;
        /// <summary>
        /// 转换为子类函数
        /// </summary>
        private static readonly MethodInfo getMethodInfo;
        /// <summary>
        /// 转换接口函数
        /// </summary>
        private static readonly MethodInfo getConvertibleMethodInfo;
        /// <summary>
        /// 转换接口函数集合
        /// </summary>
        private static readonly Dictionary<Type, Func<object, object>> getConvertibles;
        /// <summary>
        /// 转换枚举函数
        /// </summary>
        private static readonly MethodInfo getEnumMethodInfo;
        /// <summary>
        /// 转换枚举函数集合
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> getEnumMethods;
        static converter()
        {
            getConvertibles = dictionary.CreateOnly<Type, Func<object, object>>();
            getConvertibles.Add(typeof(bool), getBool);
            getConvertibles.Add(typeof(byte), getByte);
            getConvertibles.Add(typeof(char), getChar);
            getConvertibles.Add(typeof(DateTime), getDateTime);
            getConvertibles.Add(typeof(decimal), getDecimal);
            getConvertibles.Add(typeof(double), getDouble);
            getConvertibles.Add(typeof(short), getShort);
            getConvertibles.Add(typeof(int), getInt);
            getConvertibles.Add(typeof(long), getLong);
            getConvertibles.Add(typeof(sbyte), getSByte);
            getConvertibles.Add(typeof(float), getFloat);
            getConvertibles.Add(typeof(string), getString);
            getConvertibles.Add(typeof(ushort), getUShort);
            getConvertibles.Add(typeof(uint), getUInt);
            getConvertibles.Add(typeof(ulong), getULong);

            Dictionary<string, MethodInfo> methods = typeof(converter).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).getDictionary(value => value.Name);
            getBaseMethodInfo = methods["getBase"];
            getMethodInfo = methods["get"];
            getConvertibleMethodInfo = methods["getConvertible"];
            getEnumMethodInfo = methods["getEnum"];

            getEnumMethods = dictionary.CreateOnly<Type, MethodInfo>();
            getEnumMethods.Add(typeof(byte), methods["getEnumByte"]);
            getEnumMethods.Add(typeof(sbyte), methods["getEnumSByte"]);
            getEnumMethods.Add(typeof(short), methods["getEnumShort"]);
            getEnumMethods.Add(typeof(ushort), methods["getEnumUShort"]);
            getEnumMethods.Add(typeof(int), methods["getEnumInt"]);
            getEnumMethods.Add(typeof(uint), methods["getEnumUInt"]);
            getEnumMethods.Add(typeof(long), methods["getEnumLong"]);
            getEnumMethods.Add(typeof(ulong), methods["getEnumULong"]);
        }
    }
}
