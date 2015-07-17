//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HashSetPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  HashSetPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 9:04:40
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// Hash表扩展操作
    /// </summary>
    public static class HashSetPlus
    {
        /// <summary>
        /// 创建HASH表
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>HASH表</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<TValueType> CreateOnly<TValueType>() where TValueType : class
        {
            return new HashSet<TValueType>();
        }

        /// <summary>
        /// 创建HASH表
        /// </summary>
        /// <returns>HASH表</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<PointerStruct> CreatePointer()
        {
#if MONO
            return new HashSet<PointerStruct>(equalityComparer.Pointer);
#else
            return new HashSet<PointerStruct>();
#endif
        }

        /// <summary>
        /// 创建HASH表
        /// </summary>
        /// <returns>HASH表</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<SubStringStruct> CreateSubString()
        {
#if MONO
                    return new HashSet<subString>(equalityComparer.SubString);
#else
            return new HashSet<SubStringStruct>();
#endif
        }

        /// <summary>
        /// 创建HASH表
        /// </summary>
        /// <returns>HASH表</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<hashString> CreateHashString()
        {
#if MONO
            return new HashSet<hashString>(equalityComparer.HashString);
#else
            return new HashSet<hashString>();
#endif
        }
    }
    /// <summary>
    /// HASH表扩展操作
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public static class HashSetPlus<TValueType>
    {
        /// <summary>
        /// 是否值类型
        /// </summary>
        private static readonly bool IsValueType = typeof(TValueType).IsValueType;

        /// <summary>
        /// 创建字典
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <returns>字典</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<TValueType> Create()
        {
#if MONO
            if (isValueType) return new HashSet<keyType, valueType>(equalityComparer.comparer<valueType>.Default);
#endif
            return new HashSet<TValueType>();
        }
    }
}
