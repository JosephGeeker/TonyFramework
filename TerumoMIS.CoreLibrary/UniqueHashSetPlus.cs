//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: UniqueHashSetPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  UniqueHashSetPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 17:01:27
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     唯一静态哈希
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class UniqueHashSetPlus<TValueType>
        where TValueType : struct, IEquatable<TValueType>
    {
        /// <summary>
        ///     哈希数据数组
        /// </summary>
        private readonly TValueType[] _array;

        /// <summary>
        ///     唯一静态哈希
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public unsafe UniqueHashSetPlus(IReadOnlyCollection<TValueType> values, int size)
        {
            if (values.Count > size || size <= 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            _array = new TValueType[size];
            var length = ((size + 31) >> 5) << 2;
            byte* isValue = stackalloc byte[length];
            var map = new FixedMapStruct(isValue, length);
            foreach (var value in values)
            {
                var index = value.GetHashCode();
                if ((uint) index >= size) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
                if (map.Get(index)) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
                map.Set(index);
                _array[index] = value;
            }
        }

        /// <summary>
        ///     判断是否存在某值
        /// </summary>
        /// <param name="value">待匹配值</param>
        /// <returns>是否存在某值</returns>
        public bool Contains(TValueType value)
        {
            var index = value.GetHashCode();
            return (uint) index < _array.Length && value.Equals(_array[index]);
        }
    }
}