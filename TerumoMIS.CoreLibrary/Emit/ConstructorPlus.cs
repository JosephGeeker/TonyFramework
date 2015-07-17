//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ConstructorPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  ConstructorPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 11:48:55
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

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 默认构造函数
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class ConstructorPlus:Attribute
    {
    }
    /// <summary>
    /// 默认构造函数
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public static class ConstructorPlus<TValueType>
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public static readonly Func<TValueType> New;
        /// <summary>
        /// 默认空值
        /// </summary>
        /// <returns>默认空值</returns>
        public static TValueType Default()
        {
            return default(TValueType);
        }

        static ConstructorPlus()
        {
            var type = typeof(TValueType);
            if (type.IsValueType || type.IsArray || type == typeof(string))
            {
                New = Default;
                return;
            }
            if (fastCSharp.code.typeAttribute.GetAttribute<ConstructorPlus>(type, false, true) != null)
            {
                foreach (fastCSharp.code.attributeMethod methodInfo in fastCSharp.code.attributeMethod.GetStatic(type))
                {
                    if (methodInfo.Method.ReflectedType == type && methodInfo.Method.GetParameters().Length == 0 && methodInfo.GetAttribute<ConstructorPlus>(true) != null)
                    {
                        New = (Func<TValueType>)Delegate.CreateDelegate(typeof(Func<TValueType>), methodInfo.Method);
                        return;
                    }
                }
            }
            if (!type.IsInterface && !type.IsAbstract)
            {
                var constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, NullValuePlus<Type>.Array, null);
                if (constructorInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("constructor", type, NullValuePlus<Type>.Array, type, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Newobj, constructorInfo);
                    generator.Emit(OpCodes.Ret);
                    New = (Func<TValueType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType>));
                }
            }
        }
    }
}
