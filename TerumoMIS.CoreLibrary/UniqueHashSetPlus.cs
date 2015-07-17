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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 唯一静态哈希
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public sealed class UniqueHashSetPlus<valueType>
        where valueType:struct ,IEquatable<valueType>
    {
        /// <summary>
        /// 哈希数据数组
        /// </summary>
        private valueType[] array;
        /// <summary>
        /// 唯一静态哈希
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public unsafe uniqueHashSet(valueType[] values, int size)
        {
            if (values.length() > size || size <= 0) log.Error.Throw(log.exceptionType.IndexOutOfRange);
            array = new valueType[size];
            int length = ((size + 31) >> 5) << 2;
            byte* isValue = stackalloc byte[length];
            fixedMap map = new fixedMap(isValue, length, 0);
            foreach (valueType value in values)
            {
                int index = value.GetHashCode();
                if ((uint)index >= size) log.Error.Throw(log.exceptionType.IndexOutOfRange);
                if (map.Get(index)) log.Error.Throw(log.exceptionType.ErrorOperation);
                map.Set(index);
                array[index] = value;
            }
        }
        /// <summary>
        /// 判断是否存在某值
        /// </summary>
        /// <param name="value">待匹配值</param>
        /// <returns>是否存在某值</returns>
        public bool Contains(valueType value)
        {
            int index = value.GetHashCode();
            return (uint)index < array.Length && value.Equals(array[index]);
        }
    }
}
