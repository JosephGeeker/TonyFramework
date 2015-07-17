//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DictionaryPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  DictionaryPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 8:53:33
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 字典扩展操作
    /// </summary>
    public static class DictionaryPlus
    {
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> CreateOnly<TKeyType, TValueType>()
             where TKeyType : class
        {
            return new Dictionary<TKeyType, TValueType>();
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="capacity">初始化容器尺寸</param>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> CreateOnly<TKeyType, TValueType>(int capacity)
             where TKeyType : class
        {
            return new Dictionary<TKeyType, TValueType>(capacity);
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> Create<TKeyType, TValueType>()
             where TKeyType : struct, IEquatable<TKeyType>
        {
#if MONO
            return new Dictionary<keyType, valueType>(equalityComparer.comparer<keyType>.Default);
#else
            return new Dictionary<TKeyType, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="capacity">初始化容器尺寸</param>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> CreateAny<TKeyType, TValueType>(int capacity)
        {
#if MONO
            return new Dictionary<keyType, valueType>(capacity, equalityComparer.comparer<keyType>.Default);
#else
            return new Dictionary<TKeyType, TValueType>(capacity);
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> CreateAny<TKeyType, TValueType>()
        {
#if MONO
            return new Dictionary<keyType, valueType>(equalityComparer.comparer<keyType>.Default);
#else
            return new Dictionary<TKeyType, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<short, TValueType> CreateShort<TValueType>()
        {
#if MONO
            return new Dictionary<short, valueType>(equalityComparer.Short);
#else
            return new Dictionary<short, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, TValueType> CreateInt<TValueType>()
        {
#if MONO
            return new Dictionary<int, valueType>(equalityComparer.Int);
#else
            return new Dictionary<int, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="capacity">初始化容器尺寸</param>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, TValueType> CreateInt<TValueType>(int capacity)
        {
#if MONO
            return new Dictionary<int, valueType>(capacity, equalityComparer.Int);
#else
            return new Dictionary<int, TValueType>(capacity);
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<ulong, TValueType> CreateULong<TValueType>()
        {
#if MONO
            return new Dictionary<ulong, valueType>(equalityComparer.ULong);
#else
            return new Dictionary<ulong, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<Uint128Struct, TValueType> CreateUInt128<TValueType>()
        {
#if MONO
            return new Dictionary<uint128, valueType>(equalityComparer.UInt128);
#else
            return new Dictionary<Uint128Struct, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<PointerStruct, TValueType> CreatePointer<TValueType>()
        {
#if MONO
            return new Dictionary<pointer, valueType>(equalityComparer.Pointer);
#else
            return new Dictionary<PointerStruct, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<SubStringStruct, TValueType> CreateSubString<TValueType>()
        {
#if MONO
                    return new Dictionary<subString, valueType>(equalityComparer.SubString);
#else
            return new Dictionary<SubStringStruct, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<HashBytesStruct, TValueType> CreateHashBytes<TValueType>()
        {
#if MONO
            return new Dictionary<hashBytes, valueType>(equalityComparer.HashBytes);
#else
            return new Dictionary<HashBytesStruct, TValueType>();
#endif
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<HashStringStruct, TValueType> CreateHashString<TValueType>()
        {
#if MONO
            return new Dictionary<hashBytes, valueType>(equalityComparer.HashString);
#else
            return new Dictionary<HashStringStruct, TValueType>();
#endif
        }
    }
    /// <summary>
    /// 字典扩展操作
    /// </summary>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public static class DictionaryPlus<TKeyType> where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 是否值类型
        /// </summary>
        private static readonly bool IsValueType = typeof(TKeyType).IsValueType;
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> Create<TValueType>()
        {
#if MONO
            if (isValueType) return new Dictionary<keyType, valueType>(equalityComparer.comparer<keyType>.Default);
#endif
            return new Dictionary<TKeyType, TValueType>();
        }
        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="capacity">初始化容器尺寸</param>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKeyType, TValueType> Create<TValueType>(int capacity)
        {
#if MONO
            if (isValueType) return new Dictionary<keyType, valueType>(capacity, equalityComparer.comparer<keyType>.Default);
#endif
            return new Dictionary<TKeyType, TValueType>(capacity);
        }
    }
}
