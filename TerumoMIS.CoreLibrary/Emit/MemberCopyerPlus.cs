//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberCopyerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  MemberCopyerPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 14:55:37
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
    /// 成员复制
    /// </summary>
    internal static class MemberCopyerPlus
    {
        /// <summary>
        /// 动态函数
        /// </summary>
        public struct memberDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;
            /// <summary>
            /// 
            /// </summary>
            private ILGenerator generator;
            /// <summary>
            /// 是否值类型
            /// </summary>
            private bool isValueType;
            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="dynamicMethod"></param>
            public memberDynamicMethod(Type type, DynamicMethod dynamicMethod)
            {
                this.dynamicMethod = dynamicMethod;
                generator = dynamicMethod.GetILGenerator();
                isValueType = type.IsValueType;
            }
            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(fieldIndex field)
            {
                generator.Emit(OpCodes.Ldarg_0);
                if (isValueType) generator.Emit(OpCodes.Ldarga_S, 1);
                else
                {
                    generator.Emit(OpCodes.Ldind_Ref);
                    generator.Emit(OpCodes.Ldarg_1);
                }
                generator.Emit(OpCodes.Ldfld, field.Member);
                generator.Emit(OpCodes.Stfld, field.Member);
            }
            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void PushMemberMap(fieldIndex field)
            {
                Label isMember = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                generator.Emit(OpCodes.Callvirt, pub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, isMember);
                Push(field);
                generator.MarkLabel(isMember);
            }
            /// <summary>
            /// 创建成员复制委托
            /// </summary>
            /// <returns>成员复制委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
    }
    /// <summary>
    /// 成员复制
    /// </summary>
    /// <typeparam name="valueType">对象类型</typeparam>
    public static class memberCopyer<valueType>
    {
        /// <summary>
        /// 对象成员复制
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="readValue">被复制对象</param>
        /// <param name="memberMap">成员位图</param>
        public static void Copy(ref valueType value, valueType readValue, code.memberMap memberMap = null)
        {
            if (isValueCopy) value = readValue;
            else if (memberMap == null || memberMap.IsDefault) defaultCopyer(ref value, readValue);
            else defaultMemberCopyer(ref value, readValue, memberMap);
        }
        /// <summary>
        /// 对象成员复制
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="readValue">被复制对象</param>
        /// <param name="memberMap">成员位图</param>
        public static void Copy(valueType value, valueType readValue, code.memberMap memberMap = null)
        {
            if (memberMap == null || memberMap.IsDefault) defaultCopyer(ref value, readValue);
            else defaultMemberCopyer(ref value, readValue, memberMap);
        }
        /// <summary>
        /// 成员复制委托
        /// </summary>
        /// <param name="value"></param>
        /// <param name="copyValue"></param>
        private delegate void copyer(ref valueType value, valueType copyValue);
        /// <summary>
        /// 成员复制委托
        /// </summary>
        /// <param name="value"></param>
        /// <param name="copyValue"></param>
        /// <param name="memberMap">成员位图</param>
        private delegate void memberMapCopyer(ref valueType value, valueType copyValue, code.memberMap memberMap);
        /// <summary>
        /// 是否采用值类型复制模式
        /// </summary>
        private static readonly bool isValueCopy;
        /// <summary>
        /// 默认成员复制委托
        /// </summary>
        private static readonly copyer defaultCopyer;
        /// <summary>
        /// 默认成员复制委托
        /// </summary>
        private static readonly memberMapCopyer defaultMemberCopyer;
        /// <summary>
        /// 数组复制
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        private static void copyArray(ref valueType[] value, valueType[] readValue)
        {
            if (readValue != null)
            {
                if (readValue.Length == 0)
                {
                    if (value == null) value = nullValue<valueType>.Array;
                    return;
                }
                if (value == null || value.Length < readValue.Length) Array.Resize(ref value, readValue.Length);
                Array.Copy(readValue, 0, value, 0, readValue.Length);
            }
        }
        /// <summary>
        /// 数组复制
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        /// <param name="memberMap"></param>
        private static void copyArray(ref valueType[] value, valueType[] readValue, code.memberMap memberMap)
        {
            copyArray(ref value, readValue);
        }
        /// <summary>
        /// 自定义复制函数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        private static void customCopy(ref valueType value, valueType readValue)
        {
            defaultMemberCopyer(ref value, readValue, null);
        }
        /// <summary>
        /// 没有复制字段
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        private static void noCopy(ref valueType value, valueType readValue)
        {
        }
        /// <summary>
        /// 没有复制字段
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        /// <param name="memberMap"></param>
        private static void noCopy(ref valueType value, valueType readValue, code.memberMap memberMap)
        {
        }
        /// <summary>
        /// 对象浅复制
        /// </summary>
        private static readonly Func<valueType, object> memberwiseClone;
        /// <summary>
        /// 对象浅复制
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static valueType MemberwiseClone(valueType value)
        {
            return memberwiseClone != null ? (valueType)memberwiseClone(value) : value;
        }

        static memberCopyer()
        {
            Type type = typeof(valueType), refType = type.MakeByRefType();
            if (!type.IsValueType) memberwiseClone = (Func<valueType, object>)Delegate.CreateDelegate(typeof(Func<valueType, object>), typeof(valueType).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic));
            if (type.IsArray)
            {
                if (type.GetArrayRank() == 1)
                {
                    Type elementType = type.GetElementType();
                    defaultCopyer = (copyer)Delegate.CreateDelegate(typeof(copyer), elementType.GetMethod("copyArray", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { refType, type }, null));
                    defaultMemberCopyer = (memberMapCopyer)Delegate.CreateDelegate(typeof(memberMapCopyer), elementType.GetMethod("copyArray", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { refType, type, typeof(code.memberMap) }, null));
                    return;
                }
                defaultCopyer = noCopy;
                defaultMemberCopyer = noCopy;
                return;
            }
            if (type.IsEnum || type.IsPointer || type.IsInterface)
            {
                isValueCopy = true;
                return;
            }
            foreach (fastCSharp.code.attributeMethod methodInfo in fastCSharp.code.attributeMethod.GetStatic(type))
            {
                if (methodInfo.Method.ReturnType == typeof(void))
                {
                    ParameterInfo[] parameters = methodInfo.Method.GetParameters();
                    if (parameters.Length == 3 && parameters[0].ParameterType == refType && parameters[1].ParameterType == type && parameters[2].ParameterType == typeof(code.memberMap))
                    {
                        if (methodInfo.GetAttribute<memberCopy.custom>(true) != null)
                        {
                            defaultCopyer = customCopy;
                            defaultMemberCopyer = (memberMapCopyer)Delegate.CreateDelegate(typeof(memberMapCopyer), methodInfo.Method);
                            return;
                        }
                    }
                }
            }
            fieldIndex[] fields = memberIndexGroup<valueType>.GetFields();
            if (fields.Length == 0)
            {
                defaultCopyer = noCopy;
                defaultMemberCopyer = noCopy;
                return;
            }
            memberCopyer.memberDynamicMethod dynamicMethod = new memberCopyer.memberDynamicMethod(type, new DynamicMethod("memberCopyer", null, new Type[] { refType, type }, type, true));
            memberCopyer.memberDynamicMethod memberMapDynamicMethod = new memberCopyer.memberDynamicMethod(type, new DynamicMethod("memberMapCopyer", null, new Type[] { refType, type, typeof(code.memberMap) }, type, true));
            if (type.IsValueType)
            {
                foreach (fieldIndex field in fields)
                {
                    dynamicMethod.Push(field);
                    memberMapDynamicMethod.PushMemberMap(field);
                }
            }
            else
            {
                foreach (fieldIndex field in fields)
                {
                    dynamicMethod.Push(field);
                    memberMapDynamicMethod.PushMemberMap(field);
                }
            }
            defaultCopyer = (copyer)dynamicMethod.Create<copyer>();
            defaultMemberCopyer = (memberMapCopyer)memberMapDynamicMethod.Create<memberMapCopyer>();
        }
    }
}
