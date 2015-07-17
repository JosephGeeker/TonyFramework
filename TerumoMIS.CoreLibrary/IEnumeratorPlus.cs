//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: IEnumeratorPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  IEnumeratorPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 16:54:25
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 枚举器
    /// </summary>
    /// <typeparam name="valueType"></typeparam>
    static class IEnumeratorPlus<valueType>
    {
        /// <summary>
        /// 空枚举器
        /// </summary>
        private struct empty : IEnumerator<valueType>
        {
            /// <summary>
            /// 当前数据元素
            /// </summary>
            valueType IEnumerator<valueType>.Current
            {
                get
                {
                    log.Error.Throw(log.exceptionType.IndexOutOfRange);
                    return default(valueType);
                }
            }
            /// <summary>
            /// 当前数据元素
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    log.Error.Throw(log.exceptionType.IndexOutOfRange);
                    return default(valueType);
                }
            }
            /// <summary>
            /// 转到下一个数据元素
            /// </summary>
            /// <returns>是否存在下一个数据元素</returns>
            public bool MoveNext()
            {
                return false;
            }
            /// <summary>
            /// 重置枚举器状态
            /// </summary>
            public void Reset() { }
            /// <summary>
            /// 释放枚举器
            /// </summary>
            public void Dispose() { }
        }
        /// <summary>
        /// 空枚举实例
        /// </summary>
        internal static readonly IEnumerator<valueType> Empty = new empty();
        /// <summary>
        /// 数组枚举器
        /// </summary>
        internal struct array : IEnumerator<valueType>
        {
            /// <summary>
            /// 被枚举数组
            /// </summary>
            private valueType[] values;
            /// <summary>
            /// 当前位置
            /// </summary>
            private int currentIndex;
            /// <summary>
            /// 结束位置
            /// </summary>
            private int endIndex;
            /// <summary>
            /// 起始位置
            /// </summary>
            private int startIndex;
            /// <summary>
            /// 数组枚举器
            /// </summary>
            /// <param name="value">双向动态数据</param>
            public array(collection<valueType> value)
            {
                values = value.array;
                startIndex = value.StartIndex;
                endIndex = value.EndIndex;
                currentIndex = startIndex - 1;
                if (endIndex == 0) endIndex = values.Length;
            }
            /// <summary>
            /// 数组枚举器
            /// </summary>
            /// <param name="value">单向动态数据</param>
            public array(list<valueType> value)
            {
                values = value.array;
                startIndex = 0;
                endIndex = value.Count;
                currentIndex = -1;
            }
            /// <summary>
            /// 数组枚举器
            /// </summary>
            /// <param name="value">数组子串</param>
            public array(subArray<valueType> value)
            {
                values = value.array;
                startIndex = value.StartIndex;
                endIndex = value.EndIndex;
                currentIndex = startIndex - 1;
            }
            /// <summary>
            /// 当前数据元素
            /// </summary>
            valueType IEnumerator<valueType>.Current
            {
                get { return values[currentIndex]; }
            }
            /// <summary>
            /// 当前数据元素
            /// </summary>
            object IEnumerator.Current
            {
                get { return values[currentIndex]; }
            }
            /// <summary>
            /// 转到下一个数据元素
            /// </summary>
            /// <returns>是否存在下一个数据元素</returns>
            public bool MoveNext()
            {
                if (++currentIndex != endIndex) return true;
                --currentIndex;
                return false;
            }
            /// <summary>
            /// 重置枚举器状态
            /// </summary>
            public void Reset()
            {
                currentIndex = startIndex - 1;
            }
            /// <summary>
            /// 释放枚举器
            /// </summary>
            public void Dispose()
            {
                values = null;
            }
        }
    }
}
