//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: NumberToCharStream
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  NumberToCharStream
//	User name:  C1400008
//	Location Time: 2015/7/13 15:00:40
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
    /// 数字转换成字符串
    /// </summary>
    internal static class NumberToCharStreamPlus
    {
        /// <summary>
        /// 动态函数
        /// </summary>
        public struct numberDynamicMethod
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
            /// 
            /// </summary>
            private Label indexLable;
            /// <summary>
            /// 
            /// </summary>
            private Label nextLable;
            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="arrayType"></param>
            public numberDynamicMethod(Type type, Type arrayType)
            {
                dynamicMethod = new DynamicMethod("numberJoinChar", null, new Type[] { typeof(charStream), arrayType, typeof(int), typeof(int), typeof(char), typeof(bool) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                generator.DeclareLocal(typeof(int));

                indexLable = generator.DefineLabel();
                nextLable = generator.DefineLabel();
                Label toString = generator.DefineLabel();

                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Br_S, indexLable);

                generator.MarkLabel(nextLable);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Beq_S, toString);

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_S, 4);
                generator.Emit(OpCodes.Callvirt, pub.CharStreamWriteCharMethod);

                generator.MarkLabel(toString);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="method"></param>
            public void JoinChar(MethodInfo method, Type type)
            {
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloc_0);
                if (type == typeof(int) || type == typeof(uint)) generator.Emit(OpCodes.Ldelem_I4);
                else if (type == typeof(byte) || type == typeof(sbyte)) generator.Emit(OpCodes.Ldelem_I1);
                else if (type == typeof(long) || type == typeof(ulong)) generator.Emit(OpCodes.Ldelem_I8);
                else if (type == typeof(short) || type == typeof(ushort)) generator.Emit(OpCodes.Ldelem_I2);
                else generator.Emit(OpCodes.Ldelem, type);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, method);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="method"></param>
            public void JoinCharNull(MethodInfo method, Type type)
            {
                Label writeNull = generator.DefineLabel(), end = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldelem, type);
                generator.Emit(OpCodes.Call, pub.GetNullableHasValue(type));
                generator.Emit(OpCodes.Brfalse_S, writeNull);

                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldelema, type);
                generator.Emit(OpCodes.Call, pub.GetNullableValue(type));
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, method);
                generator.Emit(OpCodes.Br_S, end);

                generator.MarkLabel(writeNull);
                generator.Emit(OpCodes.Ldarg_S, 5);
                generator.Emit(OpCodes.Brfalse_S, end);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, pub.CharStreamWriteNullMethod);
                generator.MarkLabel(end);
            }
            /// <summary>
            /// 创建成员转换委托
            /// </summary>
            /// <returns>成员转换委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Add);
                generator.Emit(OpCodes.Stloc_0);

                generator.MarkLabel(indexLable);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldarg_3);
                generator.Emit(OpCodes.Bne_Un_S, nextLable);

                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        private unsafe static string joinString(string[] array, char join)
        {
            int length = 0;
            foreach (string nextString in array)
            {
                if (nextString != null) length += nextString.Length;
            }
            string value = fastCSharp.String.FastAllocateString(length + array.Length - 1);
            fixed (char* valueFixed = value)
            {
                char* write = valueFixed;
                foreach (string nextString in array)
                {
                    if (write != valueFixed) *write++ = join;
                    if (nextString != null)
                    {
                        unsafer.String.Copy(nextString, write);
                        write += nextString.Length;
                    }
                }
            }
            return value;
        }
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        private unsafe static string joinNullString(string[] array, char join)
        {
            int length = 0;
            foreach (string nextString in array) length += nextString == null ? 4 : nextString.Length;
            string value = fastCSharp.String.FastAllocateString(length + array.Length - 1);
            fixed (char* valueFixed = value)
            {
                char* write = valueFixed;
                foreach (string nextString in array)
                {
                    if (write != valueFixed) *write++ = join;
                    if (nextString == null)
                    {
                        *(int*)write = 'n' + ('u' << 16);
                        *(int*)(write + 2) = 'l' + ('l' << 16);
                        write += 4;
                    }
                    else
                    {
                        unsafer.String.Copy(nextString, write);
                        write += nextString.Length;
                    }
                }
            }
            return value;
        }
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string structJoinChar<valueType>(valueType[] array, char join, bool isNull) where valueType : struct
        {
            if (array.Length == 1)
            {
                foreach (valueType value in array) return value.ToString();
            }
            string[] stringArray = new string[array.Length];
            int index = 0;
            foreach (valueType value in array) stringArray[index++] = value.ToString();
            return joinString(stringArray, join);
        }
        /// <summary>
        /// 连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo StructJoinCharMethod = typeof(numberToCharStream).GetMethod("structJoinChar", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string structSubArrayJoinChar<valueType>(subArray<valueType> subArray, char join, bool isNull) where valueType : struct
        {
            valueType[] array = subArray.array;
            if (subArray.Count == 1) return array[subArray.StartIndex].ToString();
            string[] stringArray = new string[subArray.Count];
            int index = 0, startIndex = subArray.StartIndex, endIndex = startIndex + subArray.Count;
            do
            {
                stringArray[index++] = array[startIndex++].ToString();
            }
            while (startIndex != endIndex);
            return joinString(stringArray, join);
        }
        /// <summary>
        /// 连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo StructSubArrayJoinCharMethod = typeof(numberToCharStream).GetMethod("structSubArrayJoinChar", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string nullableJoinChar<valueType>(Nullable<valueType>[] array, char join, bool isNull) where valueType : struct
        {
            if (array.Length == 1)
            {
                foreach (Nullable<valueType> value in array) return value.HasValue ? value.Value.ToString() : (isNull ? fastCSharp.web.ajax.Null : string.Empty);
            }
            string[] stringArray = new string[array.Length];
            int index = 0;
            foreach (Nullable<valueType> value in array) stringArray[index++] = value.HasValue ? value.Value.ToString() : null;
            return isNull ? joinNullString(stringArray, join) : joinString(stringArray, join);
        }
        /// <summary>
        /// 连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo NullableJoinCharMethod = typeof(numberToCharStream).GetMethod("nullableJoinChar", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string nullableSubArrayJoinChar<valueType>(subArray<Nullable<valueType>> subArray, char join, bool isNull) where valueType : struct
        {
            Nullable<valueType>[] array = subArray.array;
            if (subArray.Count == 1)
            {
                Nullable<valueType> value = array[subArray.StartIndex];
                return value.HasValue ? value.Value.ToString() : (isNull ? fastCSharp.web.ajax.Null : string.Empty);
            }
            string[] stringArray = new string[subArray.Count];
            int index = 0, startIndex = subArray.StartIndex, endIndex = startIndex + subArray.Count;
            do
            {
                Nullable<valueType> value = array[startIndex++];
                stringArray[index++] = value.HasValue ? value.Value.ToString() : null;
            }
            while (startIndex != endIndex);
            return isNull ? joinNullString(stringArray, join) : joinString(stringArray, join);
        }
        /// <summary>
        /// 连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo NullableSubArrayJoinCharMethod = typeof(numberToCharStream).GetMethod("nullableSubArrayJoinChar", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string classJoinChar<valueType>(valueType[] array, char join, bool isNull) where valueType : class
        {
            if (array.Length == 1)
            {
                foreach (valueType value in array) return value == null ? (isNull ? fastCSharp.web.ajax.Null : string.Empty) : value.ToString();
            }
            string[] stringArray = new string[array.Length];
            int index = 0;
            foreach (valueType value in array) stringArray[index++] = value == null ? null : value.ToString();
            return isNull ? joinNullString(stringArray, join) : joinString(stringArray, join);
        }
        /// <summary>
        /// 连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo ClassJoinCharMethod = typeof(numberToCharStream).GetMethod("classJoinChar", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string classSubArrayJoinChar<valueType>(subArray<valueType> subArray, char join, bool isNull) where valueType : class
        {
            valueType[] array = subArray.array;
            if (subArray.Count == 1)
            {
                valueType value = array[subArray.StartIndex];
                return value == null ? (isNull ? fastCSharp.web.ajax.Null : string.Empty) : value.ToString();
            }
            string[] stringArray = new string[subArray.Count];
            int index = 0, startIndex = subArray.StartIndex, endIndex = startIndex + subArray.Count;
            do
            {
                valueType value = array[startIndex++];
                stringArray[index++] = value == null ? null : value.ToString();
            }
            while (startIndex != endIndex);
            return isNull ? joinNullString(stringArray, join) : joinString(stringArray, join);
        }
        /// <summary>
        /// 连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo ClassSubArrayJoinCharMethod = typeof(numberToCharStream).GetMethod("classSubArrayJoinChar", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private static string stringJoinChar(string[] array, char join, bool isNull)
        {
            if (array.Length == 1)
            {
                foreach (string value in array) return value == null ? (isNull ? fastCSharp.web.ajax.Null : string.Empty) : value.ToString();
            }
            return isNull ? joinNullString(array, join) : joinString(array, join);
        }
        /// <summary>
        /// 连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo StringJoinCharMethod = typeof(numberToCharStream).GetMethod("stringJoinChar", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        private unsafe static string stringSubArrayJoinChar(subArray<string> subArray, char join, bool isNull)
        {
            string[] array = subArray.array;
            if (subArray.Count == 1)
            {
                string value = array[subArray.StartIndex];
                return value == null ? (isNull ? fastCSharp.web.ajax.Null : string.Empty) : value.ToString();
            }
            int startIndex = subArray.StartIndex, length = 0, endIndex = startIndex + subArray.Count;
            if (isNull)
            {
                do
                {
                    string nextString = array[startIndex++];
                    length += nextString == null ? 4 : nextString.Length;
                }
                while (startIndex != endIndex);
                string value = fastCSharp.String.FastAllocateString(length + subArray.Count - 1);
                fixed (char* valueFixed = value)
                {
                    char* write = valueFixed;
                    startIndex = subArray.StartIndex;
                    do
                    {
                        string nextString = array[startIndex++];
                        if (write != valueFixed) *write++ = join;
                        if (nextString == null)
                        {
                            *(int*)write = 'n' + ('u' << 16);
                            *(int*)(write + 2) = 'l' + ('l' << 16);
                            write += 4;
                        }
                        else
                        {
                            unsafer.String.Copy(nextString, write);
                            write += nextString.Length;
                        }
                    }
                    while (startIndex != endIndex);
                }
                return value;
            }
            else
            {
                do
                {
                    string nextString = array[startIndex++];
                    if (nextString != null) length += nextString.Length;
                }
                while (startIndex != endIndex);
                string value = fastCSharp.String.FastAllocateString(length + subArray.Count - 1);
                fixed (char* valueFixed = value)
                {
                    char* write = valueFixed;
                    startIndex = subArray.StartIndex;
                    do
                    {
                        string nextString = array[startIndex++];
                        if (write != valueFixed) *write++ = join;
                        if (nextString != null)
                        {
                            unsafer.String.Copy(nextString, write);
                            write += nextString.Length;
                        }
                    }
                    while (startIndex != endIndex);
                }
                return value;
            }
        }
        /// <summary>
        /// 连接字符串集合函数信息
        /// </summary>
        public static readonly MethodInfo StringSubArrayJoinCharMethod = typeof(numberToCharStream).GetMethod("stringSubArrayJoinChar", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 数值转换调用函数信息集合
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> toStringMethods;
        /// <summary>
        /// 获取数值转换委托调用函数信息
        /// </summary>
        /// <param name="type">数值类型</param>
        /// <returns>数值转换委托调用函数信息</returns>
        public static MethodInfo GetToStringMethod(Type type)
        {
            MethodInfo method;
            return toStringMethods.TryGetValue(type, out method) ? method : null;
        }
        static numberToCharStream()
        {
            toStringMethods = dictionary.CreateOnly<Type, MethodInfo>();
            foreach (MethodInfo method in typeof(fastCSharp.number).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (method.Name == "ToString")
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 2 && parameters[1].ParameterType == typeof(charStream))
                    {
                        toStringMethods.Add(parameters[0].ParameterType, method);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 数字转换成字符串
    /// </summary>
    /// <typeparam name="valueType"></typeparam>
    internal static class numberToCharStream<valueType>
    {
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        internal static readonly Action<charStream, valueType[], int, int, char, bool> NumberJoinChar;
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        private static readonly Func<valueType[], char, bool, string> otherJoinChar;
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        private static readonly Func<subArray<valueType>, char, bool, string> subArrayJoinChar;
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        public unsafe static string JoinString(valueType[] array, char join, bool isNull = false)
        {
            if (array.length() == 0) return isNull ? fastCSharp.web.ajax.Null : string.Empty;
            if (NumberJoinChar == null) return otherJoinChar(array, join, isNull);
            pointer buffer = unmanagedPool.StreamBuffers.Get();
            try
            {
                using (charStream stream = new charStream(buffer.Char, unmanagedPool.StreamBuffers.Size >> 1))
                {
                    NumberJoinChar(stream, array, 0, array.Length, join, isNull);
                    return new string(stream.Char, 0, stream.Length);
                }
            }
            finally { unmanagedPool.StreamBuffers.Push(ref buffer); }
        }
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array"></param>
        /// <param name="join"></param>
        /// <param name="isNull"></param>
        /// <returns></returns>
        public unsafe static string JoinString(subArray<valueType> array, char join, bool isNull = false)
        {
            if (array.Count == 0) return isNull ? fastCSharp.web.ajax.Null : string.Empty;
            if (NumberJoinChar == null) return subArrayJoinChar(array, join, isNull);
            pointer buffer = unmanagedPool.StreamBuffers.Get();
            try
            {
                using (charStream stream = new charStream(buffer.Char, unmanagedPool.StreamBuffers.Size >> 1))
                {
                    NumberJoinChar(stream, array.array, array.StartIndex, array.Count, join, isNull);
                    return new string(stream.Char, 0, stream.Length);
                }
            }
            finally { unmanagedPool.StreamBuffers.Push(ref buffer); }
        }
        static numberToCharStream()
        {
            Type type = typeof(valueType);
            if (type.IsValueType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type[] parameterTypes = type.GetGenericArguments();
                    MethodInfo method = numberToCharStream.GetToStringMethod(parameterTypes[0]);
                    if (method == null)
                    {
                        otherJoinChar = (Func<valueType[], char, bool, string>)Delegate.CreateDelegate(typeof(Func<valueType[], char, bool, string>), numberToCharStream.NullableJoinCharMethod.MakeGenericMethod(parameterTypes));
                        subArrayJoinChar = (Func<subArray<valueType>, char, bool, string>)Delegate.CreateDelegate(typeof(Func<subArray<valueType>, char, bool, string>), numberToCharStream.NullableSubArrayJoinCharMethod.MakeGenericMethod(parameterTypes));
                    }
                    else
                    {
                        numberToCharStream.numberDynamicMethod dynamicMethod = new numberToCharStream.numberDynamicMethod(type, typeof(valueType[]));
                        dynamicMethod.JoinCharNull(method, type);
                        NumberJoinChar = (Action<charStream, valueType[], int, int, char, bool>)dynamicMethod.Create<Action<charStream, valueType[], int, int, char, bool>>();
                    }
                }
                else
                {
                    MethodInfo method = numberToCharStream.GetToStringMethod(type);
                    if (method == null)
                    {
                        otherJoinChar = (Func<valueType[], char, bool, string>)Delegate.CreateDelegate(typeof(Func<valueType[], char, bool, string>), numberToCharStream.StructJoinCharMethod.MakeGenericMethod(type));
                        subArrayJoinChar = (Func<subArray<valueType>, char, bool, string>)Delegate.CreateDelegate(typeof(Func<subArray<valueType>, char, bool, string>), numberToCharStream.StructSubArrayJoinCharMethod.MakeGenericMethod(type));
                    }
                    else
                    {
                        numberToCharStream.numberDynamicMethod dynamicMethod = new numberToCharStream.numberDynamicMethod(type, typeof(valueType[]));
                        dynamicMethod.JoinChar(method, type);
                        NumberJoinChar = (Action<charStream, valueType[], int, int, char, bool>)dynamicMethod.Create<Action<charStream, valueType[], int, int, char, bool>>();
                    }
                }
            }
            else
            {
                MethodInfo method, subArrayMethod;
                if (type == typeof(string))
                {
                    method = numberToCharStream.StringJoinCharMethod;
                    subArrayMethod = numberToCharStream.StringSubArrayJoinCharMethod;
                }
                else
                {
                    method = numberToCharStream.ClassJoinCharMethod.MakeGenericMethod(type);
                    subArrayMethod = numberToCharStream.ClassSubArrayJoinCharMethod.MakeGenericMethod(type);
                }
                otherJoinChar = (Func<valueType[], char, bool, string>)Delegate.CreateDelegate(typeof(Func<valueType[], char, bool, string>), method);
                subArrayJoinChar = (Func<subArray<valueType>, char, bool, string>)Delegate.CreateDelegate(typeof(Func<subArray<valueType>, char, bool, string>), subArrayMethod);
            }
        }
    }
}
