//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ArrayPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Unsafe
//	File Name:  ArrayPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:22:23
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary.Unsafe
{
    /// <summary>
    ///     数组扩展操作（非安全，请自行确保数据可靠性）
    /// </summary>
    public static class ArrayUnsafe
    {
        /// <summary>
        ///     移动数据块
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待处理数组</param>
        /// <param name="index">原始数据位置</param>
        /// <param name="writeIndex">目标数据位置</param>
        /// <param name="count">移动数据数量</param>
        public static void Move<TValueType>(TValueType[] array, int index, int writeIndex, int count)
        {
            var endIndex = index + count;
            if (index < writeIndex && endIndex > writeIndex)
            {
                for (var writeEndIndex = writeIndex + count;
                    endIndex != index;
                    array[--writeEndIndex] = array[--endIndex])
                {
                }
            }
            else Array.Copy(array, index, array, writeIndex, count);
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">移除数据位置</param>
        /// <returns>移除数据后的数组</returns>
        public static TValueType[] GetRemoveAt<TValueType>(TValueType[] array, int index)
        {
            var newValues = new TValueType[array.Length - 1];
            Array.Copy(array, 0, newValues, 0, index);
            Array.Copy(array, index + 1, newValues, index, array.Length - index - 1);
            return newValues;
        }
    }
}