//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: QuickSortPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Algorithm
//	File Name:  QuickSortPlus
//	User name:  C1400008
//	Location Time: 2015/7/8 16:10:16
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary.Algorithm
{
    /// <summary>
    ///     快速排序
    /// </summary>
    internal static class QuickSortPlus
    {
        /// <summary>
        ///     数组排序
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        public static void Sort<TValueType>(TValueType[] values, Func<TValueType, TValueType, int> comparer)
        {
            if (values != null && values.Length > 1)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                new SorterStruct<TValueType> {Array = values, Comparer = comparer}.Sort(0, values.Length - 1);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <returns>排序后的新数组</returns>
        public static TValueType[] GetSort<TValueType>(TValueType[] values, Func<TValueType, TValueType, int> comparer)
        {
            if (values.Length() != 0)
            {
                var sorter = new SorterStruct<TValueType> {Array = values.Copy(), Comparer = comparer};
                if (values.Length > 1)
                {
                    if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                    sorter.Sort(0, values.Length - 1);
                }
                return sorter.Array;
            }
            return values.NotNull();
        }

        /// <summary>
        ///     数组范围排序
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static void Sort<TValueType>(TValueType[] values, Func<TValueType, TValueType, int> comparer,
            int startIndex, int count)
        {
            var range = new ArrayPlus.RangeStruct(values.Length(), startIndex, count);
            if (range.GetCount > 1)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                new SorterStruct<TValueType> {Array = values, Comparer = comparer}.Sort(range.SkipCount,
                    range.EndIndex - 1);
            }
        }

        /// <summary>
        ///     数组范围排序
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static TValueType[] GetSort<TValueType>(TValueType[] values, Func<TValueType, TValueType, int> comparer,
            int startIndex, int count)
        {
            var range = new ArrayPlus.RangeStruct(values.Length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                var newValues = new TValueType[count];
                Array.Copy(values, range.SkipCount, newValues, 0, count);
                if (--count > 0)
                {
                    if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                    new SorterStruct<TValueType> {Array = newValues, Comparer = comparer}.Sort(0, count);
                }
                return newValues;
            }
            return values.NotNull();
        }

        /// <summary>
        ///     范围排序
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public static SubArrayStruct<TValueType> RangeSort<TValueType>
            (TValueType[] values, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            var range = new ArrayPlus.RangeStruct(values.Length(), skipCount, getCount);
            if ((getCount = range.GetCount) != 0)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                new RangeSorterStruct<TValueType>
                {
                    Array = values,
                    Comparer = comparer,
                    SkipCount = range.SkipCount,
                    GetEndIndex = range.EndIndex - 1
                }.Sort(0, values.Length - 1);
                return SubArrayStruct<TValueType>.Unsafe(values, range.SkipCount, getCount);
            }
            return default(SubArrayStruct<TValueType>);
        }

        /// <summary>
        ///     范围排序
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的新数据</returns>
        public static SubArrayStruct<TValueType> GetRangeSort<TValueType>
            (TValueType[] values, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            var range = new ArrayPlus.RangeStruct(values.Length(), skipCount, getCount);
            if ((getCount = range.GetCount) != 0)
            {
                if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                var sorter = new RangeSorterStruct<TValueType>
                {
                    Array = values.Copy(),
                    Comparer = comparer,
                    SkipCount = range.SkipCount,
                    GetEndIndex = range.EndIndex - 1
                };
                sorter.Sort(0, values.Length - 1);
                return SubArrayStruct<TValueType>.Unsafe(sorter.Array, range.SkipCount, getCount);
            }
            return default(SubArrayStruct<TValueType>);
        }

        /// <summary>
        ///     范围排序
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序范围数据数量</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public static SubArrayStruct<TValueType> RangeSort<TValueType>
            (TValueType[] values, int startIndex, int count, Func<TValueType, TValueType, int> comparer, int skipCount,
                int getCount)
        {
            var range = new ArrayPlus.RangeStruct(values.Length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                var getRange = new ArrayPlus.RangeStruct(count, skipCount, getCount);
                if ((getCount = getRange.GetCount) != 0)
                {
                    if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                    skipCount = range.SkipCount + getRange.SkipCount;
                    new RangeSorterStruct<TValueType>
                    {
                        Array = values,
                        Comparer = comparer,
                        SkipCount = skipCount,
                        GetEndIndex = skipCount + getCount - 1
                    }.Sort(range.SkipCount, range.SkipCount + --count);
                    return SubArrayStruct<TValueType>.Unsafe(values, skipCount, getCount);
                }
            }
            return default(SubArrayStruct<TValueType>);
        }

        /// <summary>
        ///     范围排序
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序范围数据数量</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public static SubArrayStruct<TValueType> GetRangeSort<TValueType>
            (TValueType[] values, int startIndex, int count, Func<TValueType, TValueType, int> comparer, int skipCount,
                int getCount)
        {
            var range = new ArrayPlus.RangeStruct(values.Length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                var getRange = new ArrayPlus.RangeStruct(count, skipCount, getCount);
                if ((getCount = getRange.GetCount) != 0)
                {
                    var newValues = new TValueType[count];
                    Array.Copy(values, range.SkipCount, newValues, 0, count);
                    return RangeSort(newValues, comparer, getRange.SkipCount, getCount);
                }
            }
            return default(SubArrayStruct<TValueType>);
        }

        /// <summary>
        ///     排序取Top N
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public static SubArrayStruct<TValueType> GetTop<TValueType>(TValueType[] values,
            Func<TValueType, TValueType, int> comparer, int count)
        {
            if (values == null) return default(SubArrayStruct<TValueType>);
            if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (count > 0)
            {
                if (count < values.Length)
                {
                    if (count <= values.Length >> 1) return GetTopBase(values, comparer, count);
                    values = GetRemoveTopBase(values, comparer, count);
                }
                else
                {
                    var newValues = new TValueType[values.Length];
                    Array.Copy(values, 0, newValues, 0, values.Length);
                    values = newValues;
                }
                return SubArrayStruct<TValueType>.Unsafe(values, 0, values.Length);
            }
            return default(SubArrayStruct<TValueType>);
        }

        /// <summary>
        ///     排序取Top N
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public static SubArrayStruct<TValueType> Top<TValueType>(TValueType[] values,
            Func<TValueType, TValueType, int> comparer, int count)
        {
            if (values == null) return default(SubArrayStruct<TValueType>);
            if (comparer == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (count > 0)
            {
                if (count < values.Length)
                {
                    if (count <= values.Length >> 1) return GetTopBase(values, comparer, count);
                    values = GetRemoveTopBase(values, comparer, count);
                }
                return SubArrayStruct<TValueType>.Unsafe(values, 0, values.Length);
            }
            return default(SubArrayStruct<TValueType>);
        }

        /// <summary>
        ///     排序取Top N
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        private static SubArrayStruct<TValueType> GetTopBase<TValueType>(TValueType[] values,
            Func<TValueType, TValueType, int> comparer, int count)
        {
            uint sqrtMod;
            var length = Math.Min(Math.Max(count << 2, count + (int) ((uint) values.Length).Sqrt(out sqrtMod)),
                values.Length);
            var newValues = new TValueType[length];
            int readIndex = values.Length - length, writeIndex = count;
            Array.Copy(values, readIndex, newValues, 0, length);
            var sort = new RangeSorterStruct<TValueType>
            {
                Array = newValues,
                Comparer = comparer,
                SkipCount = count - 1,
                GetEndIndex = count - 1
            };
            sort.Sort(0, --length);
            for (var maxValue = newValues[sort.GetEndIndex]; readIndex != 0;)
            {
                if (comparer(values[--readIndex], maxValue) < 0)
                {
                    newValues[writeIndex] = values[readIndex];
                    if (writeIndex == length)
                    {
                        sort.Sort(0, length);
                        writeIndex = count;
                        maxValue = newValues[sort.GetEndIndex];
                    }
                    else ++writeIndex;
                }
            }
            if (writeIndex != count) sort.Sort(0, writeIndex - 1);
            Array.Clear(newValues, count, newValues.Length - count);
            return SubArrayStruct<TValueType>.Unsafe(newValues, 0, count);
        }

        /// <summary>
        ///     排序去除Top N
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        private static TValueType[] GetRemoveTopBase<TValueType>(TValueType[] values,
            Func<TValueType, TValueType, int> comparer, int count)
        {
            var newValues = new TValueType[count];
            count = values.Length - count;
            uint sqrtMod;
            var length = Math.Min(Math.Max(count << 2, count + (int) ((uint) values.Length).Sqrt(out sqrtMod)),
                values.Length);
            var removeValues = new TValueType[length];
            int readIndex = values.Length - length,
                copyCount = length - count,
                removeIndex = copyCount,
                writeIndex = copyCount;
            Array.Copy(values, readIndex, removeValues, 0, length);
            var sort = new RangeSorterStruct<TValueType>
            {
                Array = removeValues,
                Comparer = comparer,
                SkipCount = copyCount,
                GetEndIndex = copyCount
            };
            sort.Sort(0, --length);
            Array.Copy(removeValues, 0, newValues, 0, copyCount);
            for (var maxValue = removeValues[copyCount]; readIndex != 0;)
            {
                if (comparer(values[--readIndex], maxValue) <= 0) newValues[writeIndex++] = values[readIndex];
                else
                {
                    removeValues[--removeIndex] = values[readIndex];
                    if (removeIndex == 0)
                    {
                        sort.Sort(0, length);
                        removeIndex = copyCount;
                        maxValue = removeValues[copyCount];
                        Array.Copy(removeValues, 0, newValues, writeIndex, copyCount);
                        writeIndex += copyCount;
                    }
                }
            }
            if (removeIndex != copyCount)
            {
                sort.Sort(removeIndex, length);
                Array.Copy(removeValues, removeIndex, newValues, writeIndex, copyCount - removeIndex);
            }
            return newValues;
        }

        /// <summary>
        ///     排序器
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        private struct SorterStruct<TValueType>
        {
            /// <summary>
            ///     待排序数组
            /// </summary>
            public TValueType[] Array;

            /// <summary>
            ///     排序比较器
            /// </summary>
            public Func<TValueType, TValueType, int> Comparer;

            /// <summary>
            ///     范围排序
            /// </summary>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置-1</param>
            public void Sort(int startIndex, int endIndex)
            {
                do
                {
                    TValueType leftValue = Array[startIndex], rightValue = Array[endIndex];
                    var average = (endIndex - startIndex) >> 1;
                    if (average == 0)
                    {
                        if (Comparer(leftValue, rightValue) > 0)
                        {
                            Array[startIndex] = rightValue;
                            Array[endIndex] = leftValue;
                        }
                        break;
                    }
                    int leftIndex = startIndex, rightIndex = endIndex;
                    var value = Array[average += startIndex];
                    if (Comparer(leftValue, value) <= 0)
                    {
                        if (Comparer(value, rightValue) > 0)
                        {
                            Array[rightIndex] = value;
                            if (Comparer(leftValue, rightValue) <= 0) Array[average] = value = rightValue;
                            else
                            {
                                Array[leftIndex] = rightValue;
                                Array[average] = value = leftValue;
                            }
                        }
                    }
                    else if (Comparer(leftValue, rightValue) <= 0)
                    {
                        Array[leftIndex] = value;
                        Array[average] = value = leftValue;
                    }
                    else
                    {
                        Array[rightIndex] = leftValue;
                        if (Comparer(value, rightValue) <= 0)
                        {
                            Array[leftIndex] = value;
                            Array[average] = value = rightValue;
                        }
                        else Array[leftIndex] = rightValue;
                    }
                    ++leftIndex;
                    --rightIndex;
                    do
                    {
                        while (Comparer(Array[leftIndex], value) < 0) ++leftIndex;
                        while (Comparer(value, Array[rightIndex]) < 0) --rightIndex;
                        if (leftIndex < rightIndex)
                        {
                            leftValue = Array[leftIndex];
                            Array[leftIndex] = Array[rightIndex];
                            Array[rightIndex] = leftValue;
                        }
                        else
                        {
                            if (leftIndex == rightIndex)
                            {
                                ++leftIndex;
                                --rightIndex;
                            }
                            break;
                        }
                    } while (++leftIndex <= --rightIndex);
                    if (rightIndex - startIndex <= endIndex - leftIndex)
                    {
                        if (startIndex < rightIndex) Sort(startIndex, rightIndex);
                        startIndex = leftIndex;
                    }
                    else
                    {
                        if (leftIndex < endIndex) Sort(leftIndex, endIndex);
                        endIndex = rightIndex;
                    }
                } while (startIndex < endIndex);
            }
        }

        /// <summary>
        ///     范围排序器(一般用于获取分页)
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        private struct RangeSorterStruct<TValueType>
        {
            /// <summary>
            ///     待排序数组
            /// </summary>
            public TValueType[] Array;

            /// <summary>
            ///     排序比较器
            /// </summary>
            public Func<TValueType, TValueType, int> Comparer;

            /// <summary>
            ///     最后一条记录位置-1
            /// </summary>
            public int GetEndIndex;

            /// <summary>
            ///     跳过数据数量
            /// </summary>
            public int SkipCount;

            /// <summary>
            ///     范围排序
            /// </summary>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置-1</param>
            public void Sort(int startIndex, int endIndex)
            {
                do
                {
                    TValueType leftValue = Array[startIndex], rightValue = Array[endIndex];
                    var average = (endIndex - startIndex) >> 1;
                    if (average == 0)
                    {
                        if (Comparer(leftValue, rightValue) > 0)
                        {
                            Array[startIndex] = rightValue;
                            Array[endIndex] = leftValue;
                        }
                        break;
                    }
                    average += startIndex;
                    //if (average > getEndIndex) average = getEndIndex;
                    //else if (average < skipCount) average = skipCount;
                    int leftIndex = startIndex, rightIndex = endIndex;
                    var value = Array[average];
                    if (Comparer(leftValue, value) <= 0)
                    {
                        if (Comparer(value, rightValue) > 0)
                        {
                            Array[rightIndex] = value;
                            if (Comparer(leftValue, rightValue) <= 0) Array[average] = value = rightValue;
                            else
                            {
                                Array[leftIndex] = rightValue;
                                Array[average] = value = leftValue;
                            }
                        }
                    }
                    else if (Comparer(leftValue, rightValue) <= 0)
                    {
                        Array[leftIndex] = value;
                        Array[average] = value = leftValue;
                    }
                    else
                    {
                        Array[rightIndex] = leftValue;
                        if (Comparer(value, rightValue) <= 0)
                        {
                            Array[leftIndex] = value;
                            Array[average] = value = rightValue;
                        }
                        else Array[leftIndex] = rightValue;
                    }
                    ++leftIndex;
                    --rightIndex;
                    do
                    {
                        while (Comparer(Array[leftIndex], value) < 0) ++leftIndex;
                        while (Comparer(value, Array[rightIndex]) < 0) --rightIndex;
                        if (leftIndex < rightIndex)
                        {
                            leftValue = Array[leftIndex];
                            Array[leftIndex] = Array[rightIndex];
                            Array[rightIndex] = leftValue;
                        }
                        else
                        {
                            if (leftIndex == rightIndex)
                            {
                                ++leftIndex;
                                --rightIndex;
                            }
                            break;
                        }
                    } while (++leftIndex <= --rightIndex);
                    if (rightIndex - startIndex <= endIndex - leftIndex)
                    {
                        if (startIndex < rightIndex && rightIndex >= SkipCount) Sort(startIndex, rightIndex);
                        if (leftIndex > GetEndIndex) break;
                        startIndex = leftIndex;
                    }
                    else
                    {
                        if (leftIndex < endIndex && leftIndex <= GetEndIndex) Sort(leftIndex, endIndex);
                        if (rightIndex < SkipCount) break;
                        endIndex = rightIndex;
                    }
                } while (startIndex < endIndex);
            }
        }
    }
}