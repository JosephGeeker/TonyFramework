//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemoryPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  MemoryPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 14:52:42
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 内存或字节数组
    /// </summary>
    public static class MemoryPlus
    {
        /// <summary>
        /// 填充字节
        /// </summary>
        /// <param name="src">串起始地址</param>
        /// <param name="value">字节值</param>
        /// <param name="length">字节数量</param>
        public unsafe static void Fill(void* src, byte value, int length)
        {
            if (src != null && length > 0) MemoryUnsafe.Fill(src, value, length);
        }
        /// <summary>
        /// 复制字符数组
        /// </summary>
        /// <param name="source">原串起始地址</param>
        /// <param name="destination">目标串起始地址</param>
        /// <param name="count">字符数量</param>
        public unsafe static void Copy(char* source, char* destination, int count)
        {
            if (source != null && destination != null && count > 0) MemoryUnsafe.Copy(source, (void*)destination, count << 1);
        }
        /// <summary>
        /// 复制字节数组
        /// </summary>
        /// <param name="source">原串起始地址</param>
        /// <param name="destination">目标串起始地址</param>
        /// <param name="length">字节长度</param>
        public unsafe static void Copy(void* source, void* destination, int length)
        {
            if (source != null && destination != null && length > 0) MemoryUnsafe.Copy(source, destination, length);
        }
        /// <summary>
        /// 字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>是否相等</returns>
        public static bool Equal(this byte[] left, byte[] right)
        {
            if (left == null) return right == null;
            return right != null && (left.Equals(right) || MemoryUnsafe.Equal(left, right));
        }
        /// <summary>
        /// 字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>是否相等</returns>
        public unsafe static bool Equal(this SubArrayStruct<byte> left, SubArrayStruct<byte> right)
        {
            if (left.Array == null) return right.Array == null;
            if (left.Count == right.Count)
            {
                fixed (byte* leftFixed = left.Array, rightFixed = right.Array)
                {
                    return MemoryUnsafe.Equal(leftFixed + left.StartIndex, rightFixed + right.StartIndex, left.Count);
                }
            }
            return false;
        }
        /// <summary>
        /// 字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="count">比较字节数</param>
        /// <returns>是否相等</returns>
        public static bool Equal(this byte[] left, byte[] right, int count)
        {
            if (left == null) return right == null;
            return right != null && left.Length >= count && right.Length >= count && count >= 0 && MemoryUnsafe.Equal(left, right, count);
        }
        /// <summary>
        /// 字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="count">比较字节数</param>
        /// <returns>是否相等</returns>
        public static unsafe bool Equal(this byte[] left, void* right, int count)
        {
            if (left == null) return right == null;
            return right != null && left.Length >= count && count >= 0 && MemoryUnsafe.Equal(left, right, count);
        }
        /// <summary>
        /// 字节数组比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="count">比较字节数</param>
        /// <returns>是否相等</returns>
        public static unsafe bool Equal(void* left, void* right, int count)
        {
            if (left != null && right != null)
            {
                return count > 0 ? MemoryUnsafe.Equal(left, right, count) : count == 0;
            }
            return left == null && right == null;
        }
        /// <summary>
        /// 查找字节
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <param name="value">字节值</param>
        /// <returns>字节位置,失败为-1</returns>
        public static int IndexOf(this byte[] data, byte value)
        {
            return data != null && data.Length > 0 ? MemoryUnsafe.IndexOf(data, value) : -1;
        }
        /// <summary>
        /// 查找字节
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <param name="value">字节值</param>
        /// <returns>字节位置</returns>
        public unsafe static byte* Find(void* start, void* end, byte value)
        {
            return start != null && end > start ? MemoryUnsafe.Find((byte*)start, (byte*)end, value) : null;
        }
        /// <summary>
        /// 字节替换
        /// </summary>
        /// <param name="value">字节数组</param>
        /// <param name="oldData">原字节</param>
        /// <param name="newData">目标字节</param>
        /// <returns>字节数组</returns>
        public static void Replace(this byte[] value, byte oldData, byte newData)
        {
            if (value != null && value.Length != 0) MemoryUnsafe.Replace(value, oldData, newData);
        }
        /// <summary>
        /// 大写转小写
        /// </summary>
        public unsafe static byte[] ToLower(this byte[] value)
        {
            if (value != null)
            {
                fixed (byte* valueFixed = value) MemoryUnsafe.ToLower(valueFixed, valueFixed + value.Length);
            }
            return value;
        }
        /// <summary>
        /// 大写转小写
        /// </summary>
        public unsafe static byte[] GetToLower(this byte[] value)
        {
            if (value.Length != 0)
            {
                byte[] newValue = new byte[value.Length];
                fixed (byte* valueFixed = value, newValueFixed = newValue) MemoryUnsafe.ToLower(valueFixed, valueFixed + value.Length, newValueFixed);
                return newValue;
            }
            return value;
        }
        /// <summary>
        /// 转16进制字符串(小写字母)
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>16进制字符串</returns>
        public unsafe static string ToLowerHex(this byte[] data)
        {
            if (data.Length != 0)
            {
                string value = StringPlus.FastAllocateString(data.Length << 1);
                fixed (byte* dataFixed = data)
                fixed (char* valueFixed = value)
                {
                    var write = valueFixed;
                    for (byte* start = dataFixed, end = dataFixed + data.Length; start != end; ++start)
                    {
                        var code = *start >> 4;
                        *write++ = (char)(code < 10 ? code + '0' : (code + ('0' + 'a' - '9' - 1)));
                        code = *start & 0xf;
                        *write++ = (char)(code < 10 ? code + '0' : (code + ('0' + 'a' - '9' - 1)));
                    }
                }
                return value;
            }
            return string.Empty;
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <param name="value">对象</param>
        /// <returns>字节数组</returns>
        public static SubArrayStruct<byte> Serialize<TValueType>(this TValueType value)
        {
            if (value != null)
            {
                using (MemoryStream memoryStream = new memoryStream.Get())
                {
                    (new BinaryFormatter()).Serialize(memoryStream, value);
                    return SubArrayStruct<byte>.Unsafe(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
                }
            }
            return default(SubArrayStruct<byte>);
        }
        /// <summary>
        /// 对象反序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <param name="data">字节数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">数据长度</param>
        /// <returns>对象</returns>
        public static TValueType DeSerialize<TValueType>(this byte[] data, int startIndex, int length)
        {
            var value = default(TValueType);
            if (data != null && data.Length != 0 && startIndex >= 0 && startIndex + length <= data.Length)
            {
                using (var memoryStream = new MemoryStream(data, startIndex, length))
                {
                    value = (TValueType)(new BinaryFormatter()).Deserialize(memoryStream);
                }
            }
            return value;
        }
        /// <summary>
        /// 对象反序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <param name="data">字节数组</param>
        /// <param name="defaultValue">默认空值</param>
        /// <returns>对象</returns>
        public static TValueType DeSerialize<TValueType>(this byte[] data, TValueType defaultValue)
        {
            var value = defaultValue;
            if (data != null && data.Length != 0)
            {
                using (var memoryStream = new MemoryStream(data))
                {
                    value = (TValueType)(new BinaryFormatter()).Deserialize(memoryStream);
                }
            }
            return value;
        }
        /// <summary>
        /// 对象反序列化
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <param name="data">字节数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">数据长度</param>
        /// <param name="defaultValue">默认空值</param>
        /// <returns>对象</returns>
        public static TValueType DeSerialize<TValueType>(this byte[] data, int startIndex, int length, TValueType defaultValue)
        {
            var value = defaultValue;
            if (data != null && data.Length != 0 && startIndex >= 0 && startIndex + length <= data.Length)
            {
                using (var memoryStream = new MemoryStream(data, startIndex, length))
                {
                    value = (TValueType)(new BinaryFormatter()).Deserialize(memoryStream);
                }
            }
            return value;
        }
    }
}
