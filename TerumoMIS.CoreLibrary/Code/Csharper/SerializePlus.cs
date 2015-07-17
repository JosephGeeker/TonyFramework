//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SerializePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  SerializePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:42:45
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

namespace TerumoMIS.CoreLibrary.Code.Csharper
{
    /// <summary>
    /// 序列化代码生成自定义属性
    /// </summary>
    public sealed partial class SerializePlus:SerializePlusBase
    {
        /// <summary>
        /// 默认空属性
        /// </summary>
        public static readonly serialize SerializeAttribute = new serialize();
        /// <summary>
        /// 序列化转换泛型标识
        /// </summary>
        public interface ISerializeGeneric { }
        /// <summary>
        /// 序列化接口
        /// </summary>
        public interface ISerializeBase
        {
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            void Serialize(unmanagedStream stream);
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            bool DeSerialize(subArray<byte> data);
        }
        /// <summary>
        /// 序列化接口
        /// </summary>
        public interface ISerialize : ISerializeBase
        {
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            void Serialize(unmanagedStreamSerializer serializer);
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置</param>
            bool DeSerialize(byte[] data, int startIndex, out int endIndex);
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            /// <returns>是否成功</returns>
            bool DeSerialize(dataDeSerializer deSerializer);
        }
        /// <summary>
        /// 序列化接口
        /// </summary>
        /// <typeparam name="memberType">成员位图类型</typeparam>
        public interface ISerialize<memberType> : ISerialize where memberType : IMemberMap<memberType>
        {
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            /// <param name="memberMap">成员位图接口</param>
            void Serialize(unmanagedStream stream, memberType memberMap);
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置</param>
            /// <param name="memberMap">成员位图类型</param>
            bool DeSerialize(byte[] data, int startIndex, out int endIndex, out memberType memberMap);
            ///// <summary>
            ///// 反序列化
            ///// </summary>
            ///// <param name="deSerializer">对象反序列化器</param>
            ///// <param name="memberMap">成员位图类型</param>
            ///// <returns>是否成功</returns>
            //bool DeSerialize(dataDeSerializer deSerializer, out memberType memberMap);
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        public new abstract unsafe class unmanagedStreamSerializer : serializeBase.unmanagedStreamSerializer
        {
            /// </summary>
            /// 成员位图接口
            /// </summary>
            protected internal IMemberMap memberMap;
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="stream">序列化流</param>
            /// <param name="memberCount">长远数量</param>
            protected unmanagedStreamSerializer(bool isReferenceMember, unmanagedStream stream, int memberCount)
                : base(isReferenceMember, stream, memberCount)
            {
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="stream">序列化流</param>
            /// <param name="memberFilter">成员选择</param>
            protected unmanagedStreamSerializer(bool isReferenceMember, unmanagedStream stream, code.memberFilters memberFilter)
                : base(isReferenceMember, stream, memberFilter)
            {
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="parentSerializer">序列化</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            protected unmanagedStreamSerializer(unmanagedStreamSerializer parentSerializer, bool isReferenceMember)
                : base(parentSerializer, isReferenceMember)
            {
            }
            /// <summary>
            /// 序列化版本号与成员位图
            /// </summary>
            /// <param name="version">版本号</param>
            protected internal abstract void versionMemerMap(int version);
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="value">序列化接口数据</param>
            protected void iSerializeNoPoint<valueType>(valueType value) where valueType : ISerialize
            {
                Type type = value.GetType();
                if (type == typeof(valueType)) dataStream.Write(fastCSharp.emit.binarySerializer.NullValue);
                else new fastCSharp.code.remoteType(type).Serialize(this);
                value.Serialize(this);
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="array">数组数据</param>
            protected void iSerializeArrayNoPoint<valueType>(valueType[] array) where valueType : ISerialize
            {
                arrayMap(array, dataStream);
                if (isReferenceMember)
                {
                    foreach (valueType nextValue in array)
                    {
                        if (checkPoint(nextValue)) iSerializeNoPoint(nextValue);
                    }
                }
                else
                {
                    foreach (valueType nextValue in array)
                    {
                        if (nextValue != null) iSerializeNoPoint(nextValue);
                    }
                }
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="array">数组数据</param>
            protected void iSerializeArrayNotNullNoPoint<valueType>(valueType[] array) where valueType : ISerialize
            {
                dataStream.Write(array.Length);
                foreach (valueType nextValue in array) nextValue.Serialize(this);
            }
            /// <summary>
            /// 未知类型数组序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="array">数组数据</param>
            protected void unknownArrayNull<valueType>(Nullable<valueType>[] array) where valueType : struct
            {
                reflectionDataSerializer reflectionDataSerializer = new reflectionDataSerializer(this, serializeBase.serializer<valueType, serialize>.serializeAttribute.IsReferenceMember, null);
                unknownArrayNull(reflectionDataSerializer, array);
                SetPoint(reflectionDataSerializer);
            }
            /// <summary>
            /// 未知类型数组序列化
            /// </summary>
            /// <typeparam name="serializer">对象序列化器</typeparam>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="array">数组数据</param>
            internal void unknownArrayNull<valueType>(unmanagedStreamSerializer serializer, Nullable<valueType>[] array)
                where valueType : struct
            {
                int length = sizeof(int) + (((array.Length + 31) >> 5) << 2);
                dataStream.PrepLength(length);
                unmanagedStream.unsafer unsafeStream = dataStream.Unsafer;
                byte* write = dataStream.CurrentData;
                *(int*)write = array.Length;
                fixedMap nullMap = new fixedMap(write += sizeof(int));
                unsafeStream.AddLength(length);
                length = 0;
                foreach (Nullable<valueType> nextValue in array)
                {
                    if (nextValue == null) nullMap.Set(length);
                    ++length;
                }
                dataStream.PrepLength();
                serializer.memberMap = default(memberMap<valueType>);
                foreach (Nullable<valueType> nextValue in array)
                {
                    if (nextValue != null) dataSerialize<valueType>.GetVersionMemerMap(serializer, nextValue.Value);
                }
            }
            /// <summary>
            /// 未知类型数组序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="array">数组数据</param>
            protected internal void unknownArray<valueType>(valueType[] array)
            {
                arrayMap(array, dataStream);
                foreach (valueType nextValue in array)
                {
                    if (nextValue != null) unknown(nextValue);
                }
            }
            /// <summary>
            /// 未知类型数据序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="value">未知类型数据</param>
            protected void unknown<valueType>(valueType value)
            {
                if (value != null)
                {
                    if (!isReferenceMember || checkPoint(value)) unknownNoPoint(value);
                }
            }
            /// <summary>
            /// 未知类型数据序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="value">未知类型数据</param>
            protected void unknownNoPoint<valueType>(valueType value)
            {
                Type type = value.GetType();
                if (type == typeof(valueType))
                {
                    dataStream.Write(fastCSharp.emit.binarySerializer.NullValue);
                    unknownNotNull(value);
                }
                else
                {
                    (new fastCSharp.code.remoteType(type)).Serialize(this);
                    ((Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), unknownNotNullMethod.MakeGenericMethod(unknownValue.GetGenericType(type))))(this, unknownValue.Converter(value, type));
                }
            }
            /// <summary>
            /// 未知值类型数据序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="value">值类型数据</param>
            protected void unknownNotNull<valueType>(valueType value)
            {
                reflectionDataSerializer serializer = new reflectionDataSerializer(this, serializeBase.serializer<valueType, serialize>.serializeAttribute.IsReferenceMember, default(memberMap<valueType>));
                dataSerialize<valueType>.GetVersionMemerMap(serializer, value);
                SetPoint(serializer);
            }
            /// <summary>
            /// 未知值类型数组序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="array">数组数据</param>
            protected void unknownArrayNotNull<valueType>(valueType[] array)
            {
                reflectionDataSerializer reflectionDataSerializer = new reflectionDataSerializer(this, serializeBase.serializer<valueType, serialize>.serializeAttribute.IsReferenceMember, null);
                unknownArrayNotNull(reflectionDataSerializer, array);
                SetPoint(reflectionDataSerializer);
            }
            /// <summary>
            /// 未知值类型数组序列化
            /// </summary>
            /// <typeparam name="serializer">对象序列化器</typeparam>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="array">数组数据</param>
            internal void unknownArrayNotNull<valueType>(unmanagedStreamSerializer serializer, valueType[] array)
            {
                dataStream.Write(array.Length);
                serializer.memberMap = default(memberMap<valueType>);
                foreach (valueType value in array) dataSerialize<valueType>.GetVersionMemerMap(serializer, value);
            }
            /// <summary>
            /// 未知类型数据序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="value">未知类型数据</param>
            protected void unknownNull<valueType>(Nullable<valueType> value) where valueType : struct
            {
                if (value != null) unknownNotNull(value.Value);
            }
            /// <summary>
            /// 未知值类型数据序列化
            /// </summary>
            /// <typeparam name="serializer">对象序列化器</typeparam>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="value">值类型数据</param>
            private static void unknownNotNullObject<valueType>(unmanagedStreamSerializer serializer, object value)
            {
                serializer.unknownNotNull((valueType)value);
            }
            /// <summary>
            /// 未知值类型数据序列化 函数信息
            /// </summary>
            private static readonly MethodInfo unknownNotNullMethod = typeof(unmanagedStreamSerializer).GetMethod("unknownNotNullObject", BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            /// 序列化逻辑值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">逻辑值</param>
            public static void GetBool(unmanagedStreamSerializer serializer, bool value)
            {
                serializer.dataStream.Write(value ? (byte)1 : (byte)0);
            }
            /// <summary>
            /// 序列化逻辑值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">逻辑值</param>
            public static void GetBool(unmanagedStreamSerializer serializer, bool? value)
            {
                GetBool(serializer, (bool)value);
            }
            /// <summary>
            /// 序列化逻辑值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="array">逻辑值数组</param>
            public unsafe static void GetBoolArray(unmanagedStreamSerializer serializer, bool[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, array);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public unsafe static void GetByte(unmanagedStreamSerializer serializer, byte value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public static void GetByte(unmanagedStreamSerializer serializer, byte? value)
            {
                GetByte(serializer, (byte)value);
            }
            /// <summary>
            /// 序列化字节数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字节数组</param>
            public static void GetByteArray(unmanagedStreamSerializer serializer, byte[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public unsafe static void GetSByte(unmanagedStreamSerializer serializer, sbyte value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public static void GetSByte(unmanagedStreamSerializer serializer, sbyte? value)
            {
                GetSByte(serializer, (sbyte)value);
            }
            /// <summary>
            /// 序列化字节数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字节数组</param>
            public unsafe static void GetSByteArray(unmanagedStreamSerializer serializer, sbyte[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public unsafe static void GetShort(unmanagedStreamSerializer serializer, short value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public static void GetShort(unmanagedStreamSerializer serializer, short? value)
            {
                GetShort(serializer, (short)value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public unsafe static void GetShortArray(unmanagedStreamSerializer serializer, short[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public unsafe static void GetUShort(unmanagedStreamSerializer serializer, ushort value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public static void GetUShort(unmanagedStreamSerializer serializer, ushort? value)
            {
                GetUShort(serializer, (ushort)value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public unsafe static void GetUShortArray(unmanagedStreamSerializer serializer, ushort[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public unsafe static void GetInt(unmanagedStreamSerializer serializer, int value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public static void GetInt(unmanagedStreamSerializer serializer, int? value)
            {
                GetInt(serializer, (int)value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public unsafe static void GetIntArray(unmanagedStreamSerializer serializer, int[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public unsafe static void GetUInt(unmanagedStreamSerializer serializer, uint value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public static void GetUInt(unmanagedStreamSerializer serializer, uint? value)
            {
                GetUInt(serializer, (uint)value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public unsafe static void GetUIntArray(unmanagedStreamSerializer serializer, uint[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public unsafe static void GetLong(unmanagedStreamSerializer serializer, long value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public static void GetLong(unmanagedStreamSerializer serializer, long? value)
            {
                GetLong(serializer, (long)value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public unsafe static void GetLongArray(unmanagedStreamSerializer serializer, long[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public unsafe static void GetULong(unmanagedStreamSerializer serializer, ulong value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            public static void GetULong(unmanagedStreamSerializer serializer, ulong? value)
            {
                GetULong(serializer, (ulong)value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public unsafe static void GetULongArray(unmanagedStreamSerializer serializer, ulong[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化字符
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符</param>
            public unsafe static void GetChar(unmanagedStreamSerializer serializer, char value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化字符
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符</param>
            public static void GetChar(unmanagedStreamSerializer serializer, char? value)
            {
                GetChar(serializer, (char)value);
            }
            /// <summary>
            /// 序列化字符数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符数组</param>
            public unsafe static void GetCharArray(unmanagedStreamSerializer serializer, char[] value)
            {
                fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化日期值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">日期值</param>
            public static void GetDateTime(unmanagedStreamSerializer serializer, DateTime value)
            {
                GetLong(serializer, value.Ticks);
            }
            /// <summary>
            /// 序列化日期值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">日期值</param>
            public static void GetDateTime(unmanagedStreamSerializer serializer, DateTime? value)
            {
                GetLong(serializer, ((DateTime)value).Ticks);
            }
            /// <summary>
            /// 序列化日期值数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">日期值数组</param>
            public static void GetDateTimeArray(unmanagedStreamSerializer serializer, DateTime[] value)
            {
                fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            public unsafe static void GetFloat(unmanagedStreamSerializer serializer, float value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            public static void GetFloat(unmanagedStreamSerializer serializer, float? value)
            {
                GetFloat(serializer, (float)value);
            }
            /// <summary>
            /// 序列化浮点值数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值数组</param>
            public unsafe static void GetFloatArray(unmanagedStreamSerializer serializer, float[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            public unsafe static void GetDouble(unmanagedStreamSerializer serializer, double value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            public static void GetDouble(unmanagedStreamSerializer serializer, double? value)
            {
                GetDouble(serializer, (double)value);
            }
            /// <summary>
            /// 序列化浮点值数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值数组</param>
            public unsafe static void GetDoubleArray(unmanagedStreamSerializer serializer, double[] value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            public unsafe static void GetDecimal(unmanagedStreamSerializer serializer, decimal value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            public static void GetDecimal(unmanagedStreamSerializer serializer, decimal? value)
            {
                GetDecimal(serializer, (decimal)value);
            }
            /// <summary>
            /// 序列化浮点值数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值数组</param>
            public static void GetDecimalArray(unmanagedStreamSerializer serializer, decimal[] value)
            {
                fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化Guid值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">Guid值</param>
            public unsafe static void GetGuid(unmanagedStreamSerializer serializer, Guid value)
            {
                serializer.dataStream.Write(value);
            }
            /// <summary>
            /// 序列化Guid值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">Guid值</param>
            public static void GetGuid(unmanagedStreamSerializer serializer, Guid? value)
            {
                GetGuid(serializer, (Guid)value);
            }
            /// <summary>
            /// 序列化Guid数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">Guid数组</param>
            public unsafe static void GetGuidArray(unmanagedStreamSerializer serializer, Guid[] value)
            {
                fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化字符串
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符串</param>
            public unsafe static void GetString(unmanagedStreamSerializer serializer, string value)
            {
                if (value.Length == 0) serializer.dataStream.Write(0);
                else fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, value);
            }
            /// <summary>
            /// 序列化字符串数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="array">字符串数组</param>
            public unsafe static void GetStringArray(unmanagedStreamSerializer serializer, string[] array)
            {
                serializer.stringArrayNoPoint(array);
            }
            /// <summary>
            /// 可空类型
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">可空类型数据</param>
            private static void getNullType<valueType>(unmanagedStreamSerializer serializer, Nullable<valueType> value) where valueType : struct
            {
                serializer.memberMap = default(memberMap<valueType>);
                dataSerialize<valueType>.GetVersionMemerMap(serializer, value.Value);
            }
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">序列化接口数据</param>
            private static void getISerializeType<valueType>(unmanagedStreamSerializer serializer, valueType value) where valueType : ISerialize
            {
                value.Serialize(serializer);
            }
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">序列化接口数据</param>
            private static void getISerializeTypeObjectNotNull<valueType>(unmanagedStreamSerializer serializer, object value) where valueType : ISerialize
            {
                ((valueType)value).Serialize(serializer);
            }
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">序列化接口数据</param>
            private static void getISerializeTypeObject<valueType>(unmanagedStreamSerializer serializer, object value) where valueType : ISerialize
            {
                serializer.iSerializeNoPoint((valueType)value);
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组数据</param>
            private static void getISerializeTypeArrayNotNull<valueType>(unmanagedStreamSerializer serializer, valueType[] value) where valueType : ISerialize
            {
                serializer.iSerializeArrayNotNullNoPoint(value);
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组数据</param>
            private static void getISerializeTypeArray<valueType>(unmanagedStreamSerializer serializer, valueType[] value) where valueType : ISerialize
            {
                serializer.iSerializeArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组数据</param>
            private static void getISerializeTypeArrayNotNullObject<valueType>(unmanagedStreamSerializer serializer, object value) where valueType : ISerialize
            {
                serializer.iSerializeArrayNotNullNoPoint((valueType[])value);
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组数据</param>
            private static void getISerializeTypeArrayObject<valueType>(unmanagedStreamSerializer serializer, object value) where valueType : ISerialize
            {
                serializer.iSerializeArrayNoPoint((valueType[])value);
            }
            /// <summary>
            /// 可空对象数组序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            /// <param name="array">对象数组</param>
            public unsafe static void GetBoolNullArray(unmanagedStreamSerializer serializer, bool?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, array);
            }
            /// <summary>
            /// 可空对象数组序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            /// <param name="array">对象数组</param>
            private static void getArrayNull<nullType>(unmanagedStreamSerializer serializer, Nullable<nullType>[] array)
                where nullType : struct
            {
                serializer.unknownArrayNull<nullType>(serializer, array);
            }
            /// <summary>
            /// 可空对象数组序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            /// <param name="array">对象数组</param>
            private unsafe static void getArrayNullObject<nullType>(unmanagedStreamSerializer serializer, object array)
                where nullType : struct
            {
                serializer.unknownArrayNull<nullType>(serializer, (Nullable<nullType>[])array);
            }
            /// <summary>
            /// 序列化逻辑值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">逻辑值</param>
            private unsafe static void getBool(unmanagedStreamSerializer serializer, object value)
            {
                *(bool*)serializer.write = (bool)value;
                serializer.write += sizeof(bool);
            }
            /// <summary>
            /// 序列化逻辑值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">逻辑值</param>
            private unsafe static void getBoolNull(unmanagedStreamSerializer serializer, object value)
            {
                *(bool*)serializer.write = (bool)(bool?)value;
                serializer.write += sizeof(bool);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getByte(unmanagedStreamSerializer serializer, object value)
            {
                *(byte*)serializer.write = (byte)value;
                serializer.write += sizeof(byte);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getByteNull(unmanagedStreamSerializer serializer, object value)
            {
                *(byte*)serializer.write = (byte)(byte?)value;
                serializer.write += sizeof(byte);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getSByte(unmanagedStreamSerializer serializer, object value)
            {
                *(sbyte*)serializer.write = (sbyte)value;
                serializer.write += sizeof(sbyte);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getSByteNull(unmanagedStreamSerializer serializer, object value)
            {
                *(sbyte*)serializer.write = (sbyte)(sbyte?)value;
                serializer.write += sizeof(sbyte);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getShort(unmanagedStreamSerializer serializer, object value)
            {
                *(short*)serializer.write = (short)value;
                serializer.write += sizeof(short);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getShortNull(unmanagedStreamSerializer serializer, object value)
            {
                *(short*)serializer.write = (short)(short?)value;
                serializer.write += sizeof(short);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getUShort(unmanagedStreamSerializer serializer, object value)
            {
                *(ushort*)serializer.write = (ushort)value;
                serializer.write += sizeof(ushort);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getUShortNull(unmanagedStreamSerializer serializer, object value)
            {
                *(ushort*)serializer.write = (ushort)(ushort?)value;
                serializer.write += sizeof(ushort);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getInt(unmanagedStreamSerializer serializer, object value)
            {
                *(int*)serializer.write = (int)value;
                serializer.write += sizeof(int);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getIntNull(unmanagedStreamSerializer serializer, object value)
            {
                *(int*)serializer.write = (int)(int?)value;
                serializer.write += sizeof(int);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getUInt(unmanagedStreamSerializer serializer, object value)
            {
                *(uint*)serializer.write = (uint)value;
                serializer.write += sizeof(uint);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getUIntNull(unmanagedStreamSerializer serializer, object value)
            {
                *(uint*)serializer.write = (uint)(uint?)value;
                serializer.write += sizeof(uint);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getLong(unmanagedStreamSerializer serializer, object value)
            {
                *(long*)serializer.write = (long)value;
                serializer.write += sizeof(long);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getLongNull(unmanagedStreamSerializer serializer, object value)
            {
                *(long*)serializer.write = (long)(long?)value;
                serializer.write += sizeof(long);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getULong(unmanagedStreamSerializer serializer, object value)
            {
                *(ulong*)serializer.write = (ulong)value;
                serializer.write += sizeof(ulong);
            }
            /// <summary>
            /// 序列化整数值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数值</param>
            private unsafe static void getULongNull(unmanagedStreamSerializer serializer, object value)
            {
                *(ulong*)serializer.write = (ulong)(ulong?)value;
                serializer.write += sizeof(ulong);
            }
            /// <summary>
            /// 序列化字符
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符</param>
            private unsafe static void getChar(unmanagedStreamSerializer serializer, object value)
            {
                *(char*)serializer.write = (char)value;
                serializer.write += sizeof(char);
            }
            /// <summary>
            /// 序列化字符
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符</param>
            private unsafe static void getCharNull(unmanagedStreamSerializer serializer, object value)
            {
                *(char*)serializer.write = (char)(char?)value;
                serializer.write += sizeof(char);
            }
            /// <summary>
            /// 序列化日期值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">日期值</param>
            private unsafe static void getDateTime(unmanagedStreamSerializer serializer, object value)
            {
                *(long*)serializer.write = ((DateTime)value).Ticks;
                serializer.write += sizeof(long);
            }
            /// <summary>
            /// 序列化日期值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">日期值</param>
            private unsafe static void getDateTimeNull(unmanagedStreamSerializer serializer, object value)
            {
                *(long*)serializer.write = ((DateTime)(DateTime?)value).Ticks;
                serializer.write += sizeof(long);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            private unsafe static void getFloat(unmanagedStreamSerializer serializer, object value)
            {
                *(float*)serializer.write = (float)value;
                serializer.write += sizeof(float);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            private unsafe static void getFloatNull(unmanagedStreamSerializer serializer, object value)
            {
                *(float*)serializer.write = (float)(float?)value;
                serializer.write += sizeof(float);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            private unsafe static void getDouble(unmanagedStreamSerializer serializer, object value)
            {
                *(double*)serializer.write = (double)value;
                serializer.write += sizeof(double);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            private unsafe static void getDoubleNull(unmanagedStreamSerializer serializer, object value)
            {
                *(double*)serializer.write = (double)(double?)value;
                serializer.write += sizeof(double);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            private unsafe static void getDecimal(unmanagedStreamSerializer serializer, object value)
            {
                *(decimal*)serializer.write = (decimal)value;
                serializer.write += sizeof(decimal);
            }
            /// <summary>
            /// 序列化浮点值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点值</param>
            private unsafe static void getDecimalNull(unmanagedStreamSerializer serializer, object value)
            {
                *(decimal*)serializer.write = (decimal)(decimal?)value;
                serializer.write += sizeof(decimal);
            }
            /// <summary>
            /// 序列化Guid值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">Guid值</param>
            private unsafe static void getGuid(unmanagedStreamSerializer serializer, object value)
            {
                *(Guid*)serializer.write = (Guid)value;
                serializer.write += sizeof(Guid);
            }
            /// <summary>
            /// 序列化Guid值
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">Guid值</param>
            private unsafe static void getGuidNull(unmanagedStreamSerializer serializer, object value)
            {
                *(Guid*)serializer.write = (Guid)(Guid?)value;
                serializer.write += sizeof(Guid);
            }
            /// <summary>
            /// 可空类型
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">可空类型数据</param>
            private static void getNullTypeObject<valueType>(unmanagedStreamSerializer serializer, object value) where valueType : struct
            {
                serializer.memberMap = default(memberMap<valueType>);
                dataSerialize<valueType>.GetVersionMemerMap(serializer, ((Nullable<valueType>)value).Value);
            }
            /// <summary>
            /// 序列化逻辑值数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">逻辑值数组</param>
            private static void getBoolArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                GetBoolArray(serializer, (bool[])value);
            }
            /// <summary>
            /// 序列化逻辑值数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">逻辑值数组</param>
            private static void getBoolNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                GetBoolNullArray(serializer, (bool?[])value);
            }
            /// <summary>
            /// 字节数组序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            /// <param name="array">字节数组</param>
            public static void GetByteNullArray(unmanagedStreamSerializer serializer, byte?[] array)
            {
                serializer.nullArrayNoPoint(array);
            }
            /// <summary>
            /// 序列化字节数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字节数组</param>
            private static void getByteNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((byte?[])value);
            }
            /// <summary>
            /// 序列化字节数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字节数组</param>
            private unsafe static void getSByteArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (sbyte[])value);
            }
            /// <summary>
            /// 序列化字节数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字节数组</param>
            public static void GetSByteNullArray(unmanagedStreamSerializer serializer, sbyte?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化字节数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字节数组</param>
            private static void getSByteNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((sbyte?[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private unsafe static void getShortArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (short[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public static void GetShortNullArray(unmanagedStreamSerializer serializer, short?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private static void getShortNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((short?[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private unsafe static void getUShortArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (ushort[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public static void GetUShortNullArray(unmanagedStreamSerializer serializer, ushort?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private static void getUShortNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((ushort?[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private unsafe static void getIntArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (int[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public static void GetIntNullArray(unmanagedStreamSerializer serializer, int?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private static void getIntNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((int?[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private unsafe static void getUIntArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (uint[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public static void GetUIntNullArray(unmanagedStreamSerializer serializer, uint?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private static void getUIntNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((uint?[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private unsafe static void getLongArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (long[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public static void GetLongNullArray(unmanagedStreamSerializer serializer, long?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private static void getLongNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((long?[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private unsafe static void getULongArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (ulong[])value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            public static void GetULongNullArray(unmanagedStreamSerializer serializer, ulong?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化整数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">整数数组</param>
            private static void getULongNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((ulong?[])value);
            }
            /// <summary>
            /// 序列化字符数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符数组</param>
            private unsafe static void getCharArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, (char[])value);
            }
            /// <summary>
            /// 序列化字符数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符数组</param>
            public static void GetCharNullArray(unmanagedStreamSerializer serializer, char?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化字符数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符数组</param>
            private static void getCharNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((char?[])value);
            }
            /// <summary>
            /// 序列化时间数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">时间数组</param>
            private static void getDateTimeArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, (DateTime[])value);
            }
            /// <summary>
            /// 序列化时间数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">时间数组</param>
            public static void GetDateTimeNullArray(unmanagedStreamSerializer serializer, DateTime?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化时间数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">时间数组</param>
            private static void getDateTimeNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((DateTime?[])value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            private unsafe static void getFloatArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (float[])value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            public static void GetFloatNullArray(unmanagedStreamSerializer serializer, float?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            private static void getFloatNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((float?[])value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            private unsafe static void getDoubleArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.binarySerializer.Serialize(serializer.dataStream, (double[])value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            public static void GetDoubleNullArray(unmanagedStreamSerializer serializer, double?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            private static void getDoubleNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((double?[])value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            private static void getDecimalArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, (decimal[])value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            public static void GetDecimalNullArray(unmanagedStreamSerializer serializer, decimal?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化浮点数数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">浮点数数组</param>
            private static void getDecimalNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((decimal?[])value);
            }
            /// <summary>
            /// 序列化Guid数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">Guid数组</param>
            private unsafe static void getGuidArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                fastCSharp.emit.dataSerializer.Serialize(serializer.dataStream, (Guid[])value);
            }
            /// <summary>
            /// 序列化Guid数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">Guid数组</param>
            public static void GetGuidNullArray(unmanagedStreamSerializer serializer, Guid?[] value)
            {
                serializer.nullArrayNoPoint(value);
            }
            /// <summary>
            /// 序列化Guid数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">Guid数组</param>
            private static void getGuidNullArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                serializer.nullArrayNoPoint((Guid?[])value);
            }
            /// <summary>
            /// 序列化字节数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字节数组</param>
            private static void getByteArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                GetByteArray(serializer, (byte[])value);
            }
            /// <summary>
            /// 序列化字符串
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符串</param>
            private static void getStringObject(unmanagedStreamSerializer serializer, object value)
            {
                GetString(serializer, (string)value);
            }
            /// <summary>
            /// 序列化字符串数组
            /// </summary>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">字符串数组</param>
            private static void getStringArrayObject(unmanagedStreamSerializer serializer, object value)
            {
                GetStringArray(serializer, (string[])value);
            }
            /// <summary>
            /// 序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组</param>
            private static void getArray<valueType>(unmanagedStreamSerializer serializer, valueType[] value)
            {
                serializer.unknownArray<valueType>(value);
            }
            /// <summary>
            /// 序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组</param>
            private static void getArrayObject<valueType>(unmanagedStreamSerializer serializer, object value)
            {
                serializer.unknownArray<valueType>((valueType[])value);
            }
            /// <summary>
            /// 序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组</param>
            private static void getArrayNotNull<valueType>(unmanagedStreamSerializer serializer, valueType[] value)
            {
                serializer.unknownArrayNotNull<valueType>(serializer, value);
            }
            /// <summary>
            /// 序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组</param>
            private static void getArrayNotNullObject<valueType>(unmanagedStreamSerializer serializer, object value)
            {
                serializer.unknownArrayNotNull<valueType>(serializer, (valueType[])value);
            }
            /// <summary>
            /// 获取 成员对象序列化
            /// </summary>
            /// <param name="type">成员类型</param>
            /// <returns>成员对象序列化</returns>
            private static Action<unmanagedStreamSerializer, object> getMemberGetter(Type type)
            {
                memberType memberType = type;
                if (type.isStruct())
                {
                    if (memberType.IsISerialize)
                    {
                        return (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod("getISerializeTypeObjectNotNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type));
                    }
                    Type nullType = type.nullableType();
                    if (nullType != null)
                    {
                        if (nullType == typeof(bool)) return getBoolNull;
                        if (nullType == typeof(byte)) return getByteNull;
                        if (nullType == typeof(sbyte)) return getSByteNull;
                        if (nullType == typeof(short)) return getShortNull;
                        if (nullType == typeof(ushort)) return getUShortNull;
                        if (nullType == typeof(int)) return getIntNull;
                        if (nullType == typeof(uint)) return getUIntNull;
                        if (nullType == typeof(long)) return getLongNull;
                        if (nullType == typeof(ulong)) return getULongNull;
                        if (nullType == typeof(DateTime)) return getDateTimeNull;
                        if (nullType == typeof(char)) return getCharNull;
                        if (nullType == typeof(float)) return getFloatNull;
                        if (nullType == typeof(double)) return getDoubleNull;
                        if (nullType == typeof(decimal)) return getDecimalNull;
                        if (nullType == typeof(Guid)) return getGuidNull;
                        return (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod("getNullTypeObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(nullType));
                    }
                    if (type == typeof(bool)) return getBool;
                    if (type == typeof(byte)) return getByte;
                    if (type == typeof(sbyte)) return getSByte;
                    if (type == typeof(short)) return getShort;
                    if (type == typeof(ushort)) return getUShort;
                    if (type == typeof(int)) return getInt;
                    if (type == typeof(uint)) return getUInt;
                    if (type == typeof(long)) return getLong;
                    if (type == typeof(ulong)) return getULong;
                    if (type == typeof(DateTime)) return getDateTime;
                    if (type == typeof(char)) return getChar;
                    if (type == typeof(float)) return getFloat;
                    if (type == typeof(double)) return getDouble;
                    if (type == typeof(decimal)) return getDecimal;
                    if (type == typeof(Guid)) return getGuid;
                    return getObjectByType(type, true);
                }
                if (memberType.IsISerialize)
                {
                    return (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod("getISerializeTypeObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type));
                }
                if (type.IsArray)
                {
                    Type enumerableType = memberType.EnumerableArgumentType;
                    if (((memberType)enumerableType).IsISerialize)
                    {
                        if (enumerableType.isStruct())
                        {
                            return (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod("getISerializeTypeArrayNotNullObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                        }
                        return (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod("getISerializeTypeArrayObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                    }
                    Type nullType = enumerableType.nullableType();
                    if (nullType != null)
                    {
                        if (nullType == typeof(bool)) return getBoolNullArrayObject;
                        if (nullType == typeof(byte)) return getByteNullArrayObject;
                        if (nullType == typeof(sbyte)) return getSByteNullArrayObject;
                        if (nullType == typeof(short)) return getShortNullArrayObject;
                        if (nullType == typeof(ushort)) return getUShortNullArrayObject;
                        if (nullType == typeof(int)) return getIntNullArrayObject;
                        if (nullType == typeof(uint)) return getUIntNullArrayObject;
                        if (nullType == typeof(long)) return getLongNullArrayObject;
                        if (nullType == typeof(ulong)) return getULongNullArrayObject;
                        if (nullType == typeof(DateTime)) return getDateTimeNullArrayObject;
                        if (nullType == typeof(char)) return getCharNullArrayObject;
                        if (nullType == typeof(float)) return getFloatNullArrayObject;
                        if (nullType == typeof(double)) return getDoubleNullArrayObject;
                        if (nullType == typeof(decimal)) return getDecimalNullArrayObject;
                        if (nullType == typeof(Guid)) return getGuidNullArrayObject;
                        return (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod("getArrayNullObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                    }
                    if (enumerableType == typeof(bool)) return getBoolArrayObject;
                    if (enumerableType == typeof(byte)) return getByteArrayObject;
                    if (enumerableType == typeof(sbyte)) return getSByteArrayObject;
                    if (enumerableType == typeof(short)) return getShortArrayObject;
                    if (enumerableType == typeof(ushort)) return getUShortArrayObject;
                    if (enumerableType == typeof(int)) return getIntArrayObject;
                    if (enumerableType == typeof(uint)) return getUIntArrayObject;
                    if (enumerableType == typeof(long)) return getLongArrayObject;
                    if (enumerableType == typeof(ulong)) return getULongArrayObject;
                    if (enumerableType == typeof(DateTime)) return getDateTimeArrayObject;
                    if (enumerableType == typeof(char)) return getCharArrayObject;
                    if (enumerableType == typeof(float)) return getFloatArrayObject;
                    if (enumerableType == typeof(double)) return getDoubleArrayObject;
                    if (enumerableType == typeof(decimal)) return getDecimalArrayObject;
                    if (enumerableType == typeof(Guid)) return getGuidArrayObject;
                    if (enumerableType == typeof(string)) return unmanagedStreamSerializer.getStringArrayObject;
                    if (enumerableType.isStruct())
                    {
                        return (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod("getArrayNotNullObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                    }
                    return (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod("getArrayObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                }
                if (type == typeof(string)) return unmanagedStreamSerializer.getStringObject;
                return getObjectByType(type, false);
            }
            /// <summary>
            /// 获取 成员对象序列化
            /// </summary>
            /// <param name="type">成员类型</param>
            /// <returns>成员对象序列化</returns>
            public static Action<unmanagedStreamSerializer, object> GetMemberGetter(Type type)
            {
                Action<unmanagedStreamSerializer, object> getMember;
                interlocked.CompareSetSleep(ref getMemberLock);
                try
                {
                    if (!getMemberTypes.TryGetValue(type, out getMember)) getMemberTypes.Add(type, getMember = getMemberGetter(type));
                }
                finally { getMemberLock = 0; }
                return getMember;
            }
            /// <summary>
            /// 成员对象序列化器访问锁
            /// </summary>
            private static int getMemberLock;
            /// <summary>
            /// 成员对象序列化器集合
            /// </summary>
            private static readonly Dictionary<Type, Action<unmanagedStreamSerializer, object>> getMemberTypes = dictionary.CreateOnly<Type, Action<unmanagedStreamSerializer, object>>();
            /// <summary>
            /// 未知类型序列化
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数据对象</param>
            private static void getObjectNotNull<valueType>(unmanagedStreamSerializer serializer, object value)
            {
                serializer.memberMap = default(memberMap<valueType>);
                dataSerialize<valueType>.GetVersionMemerMap(serializer, (valueType)value);
            }
            /// <summary>
            /// 未知类型序列化
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数据对象</param>
            private static void getObject<valueType>(unmanagedStreamSerializer serializer, object value)
            {
                serializer.unknownNoPoint<valueType>((valueType)value);
            }
            /// <summary>
            /// 获取 未知类型序列化器
            /// </summary>
            /// <param name="type">未知类型</param>
            /// <param name="isStruct">是否值类型</param>
            /// <returns>未知类型序列化器</returns>
            private static Action<unmanagedStreamSerializer, object> getObjectByType(Type type, bool isStruct)
            {
                Action<unmanagedStreamSerializer, object> getObject;
                interlocked.CompareSetSleep(ref getObjectLock);
                try
                {
                    if (!getObjectTypes.TryGetValue(type, out getObject))
                    {
                        getObjectTypes.Add(type, getObject = (Action<unmanagedStreamSerializer, object>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, object>), typeof(unmanagedStreamSerializer).GetMethod(isStruct ? "getObjectNotNull" : "getObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type)));
                    }
                }
                finally { getObjectLock = 0; }
                return getObject;
            }
            /// <summary>
            /// 未知类型序列化器访问锁
            /// </summary>
            private static int getObjectLock;
            /// <summary>
            /// 未知类型序列化器集合
            /// </summary>
            private static readonly Dictionary<Type, Action<unmanagedStreamSerializer, object>> getObjectTypes = dictionary.CreateOnly<Type, Action<unmanagedStreamSerializer, object>>();
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        public abstract unsafe class dataSerializer : unmanagedStreamSerializer
        {
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="stream">序列化流</param>
            /// <param name="memberCount">长远数量</param>
            protected dataSerializer(bool isReferenceMember, unmanagedStream stream, int memberCount)
                : base(isReferenceMember, stream, memberCount)
            {
                memberMap = new memberMap.NullMemberMap(memberCount);
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="stream">序列化流</param>
            /// <param name="memberMap">成员位图接口</param>
            /// <param name="memberFilter">成员选择</param>
            protected dataSerializer(bool isReferenceMember, unmanagedStream stream, IMemberMap memberMap, code.memberFilters memberFilter)
                : base(isReferenceMember, stream, memberFilter)
            {
                this.memberMap = memberMap;
                stream.Write((int)fastCSharp.code.cSharp.serializeVersion.serialize);
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="parentSerializer">序列化</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="memberMap">成员位图接口</param>
            protected dataSerializer(unmanagedStreamSerializer parentSerializer, bool isReferenceMember, IMemberMap memberMap)
                : base(parentSerializer, isReferenceMember)
            {
                this.memberMap = memberMap;
            }
            /// <summary>
            /// 序列化版本号与成员位图
            /// </summary>
            /// <param name="version">版本号</param>
            protected internal override void versionMemerMap(int version)
            {
                dataStream.Write(version);
                memberMap.Serialize(dataStream);
            }
            /// <summary>
            /// 序列化结束
            /// </summary>
            public void Finally()
            {
                dataStream.Write(dataStream.OffsetLength - streamStartIndex);
            }
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="value">序列化接口数据</param>
            protected void iSerialize<valueType>(valueType value) where valueType : ISerialize
            {
                if (value != null)
                {
                    if (!isReferenceMember || checkPoint(value)) iSerializeNoPoint(value);
                }
            }
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="value">序列化接口数据</param>
            protected void iSerializeNotNull<valueType>(valueType value) where valueType : ISerialize
            {
                value.Serialize(this);
            }
            /// <summary>
            /// 字节数组序列化
            /// </summary>
            /// <param name="value">字节数组</param>
            protected void byteArray(byte[] value)
            {
                if (value != null)
                {
                    if (!isReferenceMember || value.Length == 0 || checkPointNotNull(value)) fastCSharp.emit.binarySerializer.Serialize(dataStream, value);
                }
            }
        }
        /// <summary>
        /// 对象反序列化
        /// </summary>
        public unsafe abstract class dataDeSerializer : serializeBase.deSerializer
        {
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="dataStart">序列化数据起始位置</param>
            /// <param name="dataEnd">序列化数据结束位置</param>
            protected dataDeSerializer(bool isReferenceMember, byte* dataStart, byte* dataEnd)
                : base(isReferenceMember, dataStart, dataEnd)
            {
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            protected dataDeSerializer(dataDeSerializer parentDeSerializer, bool isReferenceMember)
                : base(parentDeSerializer, isReferenceMember)
            {
            }
            /// <summary>
            /// 数据结束标识检测
            /// </summary>
            /// <returns>是否合法结束</returns>
            protected internal bool checkEnd()
            {
                if (Read > dataEnd)
                {
                    fastCSharp.log.Default.Add("数据解析错误", true, false);
                    return false;
                }
                if (*(int*)Read != (int)(Read - dataStart))
                {
                    fastCSharp.log.Default.Add("数据结束标识不匹配 " + ((int)(Read - dataStart)).toString() + " != " + (*(int*)Read).toString(), true, false);
                    return false;
                }
                Read += sizeof(int);
                return true;
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <returns>数组数据</returns>
            protected valueType[] iSerializeArrayNoPoint<valueType>() where valueType : ISerialize
            {
                valueType[] array = createArray<valueType>();
                fixedMap nullMap = new fixedMap(Read);
                Read += ((array.Length + 31) >> 5) << 2;
                for (int index = 0; index != array.Length; ++index)
                {
                    if (!nullMap.Get(index)) array[index] = iSerialize<valueType>();
                }
                return array;
            }
            /// <summary>
            /// 序列化接口数组(仅用于代码生成)
            /// </summary>
            /// <typeparam name="valueType"></typeparam>
            /// <param name="value"></param>
            /// <returns></returns>
            public valueType iSerializeNotNull<valueType>(object value)
            {
                fastCSharp.log.Error.Throw(log.exceptionType.ErrorOperation);
                return default(valueType);
            }
            /// <summary>
            /// 序列化接口数据
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <returns>序列化接口数据</returns>
            private valueType iSerialize<valueType>() where valueType : ISerialize
            {
                if (isReferenceMember)
                {
                    object reference = getPoint();
                    if (reference != null) return (valueType)reference;
                }
                return iSerializeNoPoint<valueType>(null);
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <returns>数组数据</returns>
            protected valueType[] iSerializeArrayNotNullNoPoint<valueType>() where valueType : ISerialize
            {
                valueType[] array = createArray<valueType>();
                iSerializeArrayNotNullNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <returns>字节数组</returns>
            protected byte[] byteArrayNoPoint()
            {
                byte[] data = createArray<byte>(1);
                unsafer.memory.Copy(Read, data, data.Length);
                Read += data.Length;
                if ((data.Length & 3) != 0) Read += -data.Length & 3;
                return data;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected bool[] boolArrayNoPoint()
            {
                bool[] data = createArray<bool>();
                deSerializeNoPoint(data);
                return data;
            }
            /// <summary>
            /// 反序列化字符串数组
            /// </summary>
            /// <returns>字符串数组</returns>
            protected string[] stringArrayNoPoint()
            {
                string[] array = new string[*(int*)Read];
                if (isReferenceMember) points.Add(-(int)(Read - dataStart), array);
                Read += sizeof(int);
                stringArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化未知类型数组
            /// </summary>
            /// <returns>数组数据</returns>
            protected valueType[] unknownArray<valueType>()
            {
                valueType[] array = createArray<valueType>();
                unknownArray(array);
                return array;
            }
            /// <summary>
            /// 反序列化值类型数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <returns>值类型数组<returns>
            protected Nullable<valueType>[] unknownArrayNull<valueType>() where valueType : struct
            {
                Nullable<valueType>[] array = createArray<Nullable<valueType>>();
                unknownArrayNull(array);
                return array;
            }
            /// <summary>
            /// 反序列化值类型数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="array">值类型数组</param>
            protected valueType[] unknownArrayNotNull<valueType>()
            {
                valueType[] array = createArray<valueType>();
                unknownArrayNotNull(array);
                return array;
            }
            /// <summary>
            /// 反序列化值类型数据
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="array">值类型数据</param>
            protected Nullable<valueType> unknownNull<valueType>() where valueType : struct
            {
                return unknownNotNull<valueType>();
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="array">数组数据</param>
            /// <param name="newValue">获取新数据委托</param>
            protected void iSerializeArrayNoPoint<valueType>(valueType[] array, Func<valueType> newValue) where valueType : ISerialize
            {
                fixedMap nullMap = new fixedMap(Read);
                Read += ((array.Length + 31) >> 5) << 2;
                for (int index = 0; index != array.Length; ++index)
                {
                    if (!nullMap.Get(index)) array[index] = iSerialize<valueType>(newValue);
                }
            }
            /// <summary>
            /// 序列化接口数据
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="newValue">获取新数据委托</param>
            /// <returns>序列化接口数据</returns>
            protected valueType iSerialize<valueType>(Func<valueType> newValue) where valueType : ISerialize
            {
                if (isReferenceMember)
                {
                    object reference = getPoint();
                    if (reference != null) return (valueType)reference;
                }
                return iSerializeNoPoint(newValue);
            }
            /// <summary>
            /// 序列化接口数据
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="newValue">获取新数据委托</param>
            /// <returns>序列化接口数据</returns>
            protected valueType iSerializeNoPoint<valueType>(Func<valueType> newValue) where valueType : ISerialize
            {
                valueType value;
                int point = -(int)(Read - dataStart);
                if (*(int*)Read == fastCSharp.emit.binarySerializer.NullValue)
                {
                    Read += sizeof(int);
                    value = newValue != null ? newValue() : fastCSharp.emit.constructor<valueType>.New();
                }
                else
                {
                    fastCSharp.code.remoteType remoteType = new fastCSharp.code.remoteType();
                    if (remoteType.DeSerialize(this))
                    {
                        Type type;
                        if (remoteType.TryGet(out type))
                        {
                            if (!type.isInherit(typeof(valueType))) log.Default.Throw(type.fullName() + " 不继承 " + typeof(valueType).fullName() + " ,无法反序列化", false, false);
                            value = (valueType)fastCSharp.emit.constructor.Get(type);
                        }
                        else
                        {
                            log.Default.Add("未能加载类型 " + remoteType.ToString(), false, true);
                            value = newValue != null ? newValue() : fastCSharp.emit.constructor<valueType>.New();
                        }
                    }
                    else
                    {
                        log.Default.Throw("remoteType 反序列化失败", true, false);
                        value = default(valueType);
                    }
                }
                if (isReferenceMember) points.Add(point, value);
                value.DeSerialize(this);
                return value;
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="array">数组数据</param>
            protected void iSerializeArrayNotNullNoPoint<valueType>(valueType[] array) where valueType : ISerialize
            {
                for (int index = 0; index != array.Length; ++index) array[index] = iSerializeNotNull<valueType>();
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <returns>序列化接口数据</returns>
            protected valueType iSerializeNotNull<valueType>() where valueType : ISerialize
            {
                valueType value = default(valueType);
                value.DeSerialize(this);
                return value;
            }
            /// <summary>
            /// 反序列化值类型数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="array">值类型数组</param>
            protected void unknownArrayNull<valueType>(Nullable<valueType>[] array) where valueType : struct
            {
                fixedMap nullMap = new fixedMap(Read);
                Read += ((array.Length + 31) >> 5) << 2;
                for (int index = 0; index != array.Length; ++index)
                {
                    if (!nullMap.Get(index)) array[index] = unknownNotNull<valueType>();
                }
            }
            /// <summary>
            /// 反序列化值类型数据
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            protected valueType unknownNotNull<valueType>()
            {
                return serialize.deSerialize<valueType>.GetVersionMemerMap(this, (int)(Read - dataStart));
            }
            /// <summary>
            /// 反序列化未知类型数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void unknownArray<valueType>(valueType[] array)
            {
                fixedMap nullMap = new fixedMap(Read);
                Read += ((array.Length + 31) >> 5) << 2;
                for (int index = 0; index != array.Length; ++index)
                {
                    if (!nullMap.Get(index)) array[index] = unknown<valueType>();
                }
            }
            /// <summary>
            /// 反序列化未知类型数据
            /// </summary>
            /// <returns>数组数据</returns>
            protected valueType unknown<valueType>()
            {
                if (isReferenceMember)
                {
                    object reference = getPoint();
                    if (reference != null) return (valueType)reference;
                }
                return unknownNoPoint<valueType>();
            }
            /// <summary>
            /// 反序列化未知类型数据
            /// </summary>
            /// <returns>数组数据</returns>
            protected valueType unknownNoPoint<valueType>()
            {
                int point = (int)(Read - dataStart);
                if (*(int*)Read == fastCSharp.emit.binarySerializer.NullValue)
                {
                    Read += sizeof(int);
                    return serialize.deSerialize<valueType>.GetVersionMemerMap(this, point);
                }
                fastCSharp.code.remoteType remoteType = new fastCSharp.code.remoteType();
                if (remoteType.DeSerialize(this))
                {
                    Type type = remoteType.Type;
                    if (typeof(valueType) != typeof(object) && !type.isInherit(typeof(valueType))) log.Default.Throw(type.fullName() + " 不继承 " + typeof(valueType).fullName() + " ,无法反序列化", false, false);
                    return (valueType)unknownValue.GetValue(typeof(valueType), type, ((Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), unknownNotNullMethod.MakeGenericMethod(unknownValue.GetGenericType(type))))(this));
                }
                log.Default.Throw("remoteType 反序列化失败", true, false);
                return default(valueType);
            }
            /// <summary>
            /// 反序列化值类型数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="array">值类型数组</param>
            protected void unknownArrayNotNull<valueType>(valueType[] array)
            {
                for (int index = 0; index != array.Length; ++index) array[index] = unknownNotNull<valueType>();
            }
            /// <summary>
            /// 创建数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="length">数组长度</param>
            /// <returns>数组</returns>
            protected static valueType[] newArray<valueType>(int length)
            {
                return new valueType[length];
            }
            /// <summary>
            /// 反序列化值类型数据
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <returns>反序列化数据</returns>
            private static object unknownNotNullObject<valueType>(dataDeSerializer serializer)
            {
                return serializer.unknownNotNull<valueType>();
            }
            /// <summary>
            /// 未知值类型数据序列化 函数信息
            /// </summary>
            protected static readonly MethodInfo unknownNotNullMethod = typeof(dataDeSerializer).GetMethod("unknownNotNullObject", BindingFlags.Static | BindingFlags.NonPublic);
            /// <summary>
            /// 反序列化逻辑值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>逻辑值</returns>
            public unsafe static bool GetBool(dataDeSerializer serializer)
            {
                return *serializer.Read++ != 0;
            }
            /// <summary>
            /// 反序列化逻辑值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>逻辑值</returns>
            public unsafe static bool? GetBoolNull(dataDeSerializer serializer)
            {
                return *serializer.Read++ != 0;
            }
            /// <summary>
            /// 反序列化逻辑值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>逻辑值数组</returns>
            public unsafe static bool[] GetBoolArray(dataDeSerializer serializer)
            {
                return serializer.boolArrayNoPoint();
            }
            /// <summary>
            /// 可空对象数组反序列化
            /// </summary>
            /// <param name="serializer">对象反序列化器</param>
            /// <returns>可空对象数组</returns>
            public unsafe static bool?[] GetBoolNullArray(dataDeSerializer serializer)
            {
                return serializer.boolNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static byte GetByte(dataDeSerializer serializer)
            {
                return *serializer.Read++;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static byte? GetByteNull(dataDeSerializer serializer)
            {
                return *serializer.Read++;
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字节数组</returns>
            public static byte[] GetByteArray(dataDeSerializer serializer)
            {
                return serializer.byteArrayNoPoint();
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字节数组</returns>
            public static byte?[] GetByteNullArray(dataDeSerializer serializer)
            {
                return serializer.byteNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static sbyte GetSByte(dataDeSerializer serializer)
            {
                return (sbyte)*serializer.Read++;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static sbyte? GetSByteNull(dataDeSerializer serializer)
            {
                return (sbyte?)(sbyte)*serializer.Read++;
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字节数组</returns>
            public static unsafe sbyte[] GetSByteArray(dataDeSerializer serializer)
            {
                sbyte[] array = serializer.createArray<sbyte>(sizeof(sbyte));
                fixed (sbyte* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(sbyte));
                return array;
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字节数组</returns>
            public static sbyte?[] GetSByteNullArray(dataDeSerializer serializer)
            {
                return serializer.sByteNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static short GetShort(dataDeSerializer serializer)
            {
                short value = *(short*)serializer.Read;
                serializer.Read += sizeof(short);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static short? GetShortNull(dataDeSerializer serializer)
            {
                short value = *(short*)serializer.Read;
                serializer.Read += sizeof(short);
                return value;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static unsafe short[] GetShortArray(dataDeSerializer serializer)
            {
                short[] array = serializer.createArray<short>(sizeof(short));
                fixed (short* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(short));
                return array;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static short?[] GetShortNullArray(dataDeSerializer serializer)
            {
                return serializer.shortNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static ushort GetUShort(dataDeSerializer serializer)
            {
                ushort value = *(ushort*)serializer.Read;
                serializer.Read += sizeof(ushort);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static ushort? GetUShortNull(dataDeSerializer serializer)
            {
                ushort value = *(ushort*)serializer.Read;
                serializer.Read += sizeof(ushort);
                return value;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static unsafe ushort[] GetUShortArray(dataDeSerializer serializer)
            {
                ushort[] array = serializer.createArray<ushort>(sizeof(ushort));
                fixed (ushort* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(ushort));
                return array;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static ushort?[] GetUShortNullArray(dataDeSerializer serializer)
            {
                return serializer.uShortNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static int GetInt(dataDeSerializer serializer)
            {
                int value = *(int*)serializer.Read;
                serializer.Read += sizeof(int);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static int? GetIntNull(dataDeSerializer serializer)
            {
                int value = *(int*)serializer.Read;
                serializer.Read += sizeof(int);
                return value;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static unsafe int[] GetIntArray(dataDeSerializer serializer)
            {
                int[] array = serializer.createArray<int>(sizeof(int));
                fixed (int* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(int));
                return array;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static int?[] GetIntNullArray(dataDeSerializer serializer)
            {
                return serializer.intNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static uint GetUInt(dataDeSerializer serializer)
            {
                uint value = *(uint*)serializer.Read;
                serializer.Read += sizeof(uint);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static uint? GetUIntNull(dataDeSerializer serializer)
            {
                uint value = *(uint*)serializer.Read;
                serializer.Read += sizeof(uint);
                return value;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static unsafe uint[] GetUIntArray(dataDeSerializer serializer)
            {
                uint[] array = serializer.createArray<uint>(sizeof(uint));
                fixed (uint* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(uint));
                return array;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static uint?[] GetUIntNullArray(dataDeSerializer serializer)
            {
                return serializer.uIntNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static long GetLong(dataDeSerializer serializer)
            {
                long value = *(long*)serializer.Read;
                serializer.Read += sizeof(long);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static long? GetLongNull(dataDeSerializer serializer)
            {
                long value = *(long*)serializer.Read;
                serializer.Read += sizeof(long);
                return value;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static unsafe long[] GetLongArray(dataDeSerializer serializer)
            {
                long[] array = serializer.createArray<long>(sizeof(long));
                fixed (long* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(long));
                return array;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static long?[] GetLongNullArray(dataDeSerializer serializer)
            {
                return serializer.longNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static ulong GetULong(dataDeSerializer serializer)
            {
                ulong value = *(ulong*)serializer.Read;
                serializer.Read += sizeof(ulong);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            public unsafe static ulong? GetULongNull(dataDeSerializer serializer)
            {
                ulong value = *(ulong*)serializer.Read;
                serializer.Read += sizeof(ulong);
                return value;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static unsafe ulong[] GetULongArray(dataDeSerializer serializer)
            {
                ulong[] array = serializer.createArray<ulong>(sizeof(ulong));
                fixed (ulong* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(ulong));
                return array;
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            public static ulong?[] GetULongNullArray(dataDeSerializer serializer)
            {
                return serializer.uLongNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化字符
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符</returns>
            public unsafe static char GetChar(dataDeSerializer serializer)
            {
                char value = *(char*)serializer.Read;
                serializer.Read += sizeof(char);
                return value;
            }
            /// <summary>
            /// 反序列化字符
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符</returns>
            public unsafe static char? GetCharNull(dataDeSerializer serializer)
            {
                char value = *(char*)serializer.Read;
                serializer.Read += sizeof(char);
                return (char?)value;
            }
            /// <summary>
            /// 反序列化字符数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符数组</returns>
            public static unsafe char[] GetCharArray(dataDeSerializer serializer)
            {
                char[] array = serializer.createArray<char>(sizeof(char));
                fixed (char* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(char));
                return array;
            }
            /// <summary>
            /// 反序列化字符数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符数组</returns>
            public static char?[] GetCharNullArray(dataDeSerializer serializer)
            {
                return serializer.charNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化日期值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>日期值</returns>
            public unsafe static DateTime GetDateTime(dataDeSerializer serializer)
            {
                long value = *(long*)serializer.Read;
                serializer.Read += sizeof(long);
                return new DateTime(value);
            }
            /// <summary>
            /// 反序列化日期值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>日期值</returns>
            public unsafe static DateTime? GetDateTimeNull(dataDeSerializer serializer)
            {
                long value = *(long*)serializer.Read;
                serializer.Read += sizeof(long);
                return new DateTime(value);
            }
            /// <summary>
            /// 反序列化日期值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>日期值数组</returns>
            public static DateTime[] GetDateTimeArray(dataDeSerializer serializer)
            {
                return serializer.dateTimeArrayNoPoint();
            }
            /// <summary>
            /// 反序列化日期值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>日期值数组</returns>
            public static DateTime?[] GetDateTimeNullArray(dataDeSerializer serializer)
            {
                return serializer.dateTimeNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            public unsafe static float GetFloat(dataDeSerializer serializer)
            {
                float value = *(float*)serializer.Read;
                serializer.Read += sizeof(float);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            public unsafe static float? GetFloatNull(dataDeSerializer serializer)
            {
                float value = *(float*)serializer.Read;
                serializer.Read += sizeof(float);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            public static unsafe float[] GetFloatArray(dataDeSerializer serializer)
            {
                float[] array = serializer.createArray<float>(sizeof(float));
                fixed (float* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(float));
                return array;
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            public static float?[] GetFloatNullArray(dataDeSerializer serializer)
            {
                return serializer.floatNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            public unsafe static double GetDouble(dataDeSerializer serializer)
            {
                double value = *(double*)serializer.Read;
                serializer.Read += sizeof(double);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            public unsafe static double? GetDoubleNull(dataDeSerializer serializer)
            {
                double value = *(double*)serializer.Read;
                serializer.Read += sizeof(double);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            public static unsafe double[] GetDoubleArray(dataDeSerializer serializer)
            {
                double[] array = serializer.createArray<double>(sizeof(double));
                fixed (double* arrayFixed = array) serializer.deSerializeNoPoint(arrayFixed, array.Length * sizeof(double));
                return array;
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            public static double?[] GetDoubleNullArray(dataDeSerializer serializer)
            {
                return serializer.doubleNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            public unsafe static decimal GetDecimal(dataDeSerializer serializer)
            {
                decimal value = *(decimal*)serializer.Read;
                serializer.Read += sizeof(decimal);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            public unsafe static decimal? GetDecimalNull(dataDeSerializer serializer)
            {
                decimal value = *(decimal*)serializer.Read;
                serializer.Read += sizeof(decimal);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            public static decimal[] GetDecimalArray(dataDeSerializer serializer)
            {
                return serializer.decimalArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            public static decimal?[] GetDecimalNullArray(dataDeSerializer serializer)
            {
                return serializer.decimalNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化Guid值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>Guid值</returns>
            public unsafe static Guid GetGuid(dataDeSerializer serializer)
            {
                Guid value = *(Guid*)serializer.Read;
                serializer.Read += sizeof(Guid);
                return value;
            }
            /// <summary>
            /// 反序列化Guid值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>Guid值</returns>
            public unsafe static Guid? GetGuidNull(dataDeSerializer serializer)
            {
                Guid value = *(Guid*)serializer.Read;
                serializer.Read += sizeof(Guid);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            public static Guid[] GetGuidArray(dataDeSerializer serializer)
            {
                return serializer.guidArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            public static Guid?[] GetGuidNullArray(dataDeSerializer serializer)
            {
                return serializer.guidNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化字符串
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <param name="value">字符串</param>
            public unsafe static string GetString(dataDeSerializer serializer)
            {
                return serializer.getStringNoPoint();
            }
            /// <summary>
            /// 反序列化字符串数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符串数组</returns>
            public static string[] GetStringArray(dataDeSerializer serializer)
            {
                return serializer.stringArrayNoPoint();
            }
            /// <summary>
            /// 可空类型
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>可空类型数据</returns>
            private static Nullable<valueType> getNullType<valueType>(dataDeSerializer serializer) where valueType : struct
            {
                return serializer.unknownNull<valueType>();
            }
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <returns>序列化接口数据</returns>
            private static valueType getISerializeType<valueType>(dataDeSerializer serializer) where valueType : ISerialize
            {
                valueType value = fastCSharp.emit.constructor<valueType>.New();
                value.DeSerialize(serializer);
                return value;
            }
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <returns>序列化接口数据</returns>
            private static object getISerializeTypeObjectNotNull<valueType>(dataDeSerializer serializer) where valueType : ISerialize
            {
                valueType value = fastCSharp.emit.constructor<valueType>.New();
                value.DeSerialize(serializer);
                return value;
            }
            /// <summary>
            /// 序列化接口
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <returns>序列化接口数据</returns>
            private static object getISerializeTypeObject<valueType>(dataDeSerializer serializer) where valueType : ISerialize
            {
                return serializer.iSerializeNoPoint<valueType>(null);
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <returns>数组数据</returns>
            private static valueType[] getISerializeTypeArrayNotNull<valueType>(dataDeSerializer serializer) where valueType : ISerialize
            {
                return serializer.iSerializeArrayNotNullNoPoint<valueType>();
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组数据</param>
            private static valueType[] getISerializeTypeArray<valueType>(dataDeSerializer serializer) where valueType : ISerialize
            {
                return serializer.iSerializeArrayNoPoint<valueType>();
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <returns>数组数据</returns>
            private static object getISerializeTypeArrayNotNullObject<valueType>(dataDeSerializer serializer) where valueType : ISerialize
            {
                return serializer.iSerializeArrayNotNullNoPoint<valueType>();
            }
            /// <summary>
            /// 序列化接口数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">序列化器</param>
            /// <param name="value">数组数据</param>
            private static object getISerializeTypeArrayObject<valueType>(dataDeSerializer serializer) where valueType : ISerialize
            {
                return serializer.iSerializeArrayNoPoint<valueType>();
            }
            /// <summary>
            /// 反序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>数组</returns>
            private static valueType[] getArray<valueType>(dataDeSerializer serializer)
            {
                return serializer.unknownArray<valueType>();
            }
            /// <summary>
            /// 反序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>数组</returns>
            private static Nullable<valueType>[] getArrayNull<valueType>(dataDeSerializer serializer) where valueType : struct
            {
                return serializer.unknownArrayNull<valueType>();
            }
            /// <summary>
            /// 反序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>数组</returns>
            private static valueType[] getArrayNotNull<valueType>(dataDeSerializer serializer)
            {
                return serializer.unknownArrayNotNull<valueType>();
            }
            /// <summary>
            /// 反序列化逻辑值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>逻辑值</returns>
            private unsafe static object getBool(dataDeSerializer serializer)
            {
                return *serializer.Read++ != 0;
            }
            /// <summary>
            /// 反序列化逻辑值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>逻辑值</returns>
            private unsafe static object getBoolNull(dataDeSerializer serializer)
            {
                return (bool?)(*serializer.Read++ != 0);
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getByte(dataDeSerializer serializer)
            {
                return *serializer.Read++;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getByteNull(dataDeSerializer serializer)
            {
                return (byte?)*serializer.Read++;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getSByte(dataDeSerializer serializer)
            {
                return (sbyte)*serializer.Read++;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getSByteNull(dataDeSerializer serializer)
            {
                return (sbyte?)(sbyte)*serializer.Read++;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getShort(dataDeSerializer serializer)
            {
                short value = *(short*)serializer.Read;
                serializer.Read += sizeof(short);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getShortNull(dataDeSerializer serializer)
            {
                short value = *(short*)serializer.Read;
                serializer.Read += sizeof(short);
                return (short?)value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getUShort(dataDeSerializer serializer)
            {
                ushort value = *(ushort*)serializer.Read;
                serializer.Read += sizeof(ushort);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getUShortNull(dataDeSerializer serializer)
            {
                ushort value = *(ushort*)serializer.Read;
                serializer.Read += sizeof(ushort);
                return (ushort?)value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getInt(dataDeSerializer serializer)
            {
                int value = *(int*)serializer.Read;
                serializer.Read += sizeof(int);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getIntNull(dataDeSerializer serializer)
            {
                int value = *(int*)serializer.Read;
                serializer.Read += sizeof(int);
                return (int?)value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getUInt(dataDeSerializer serializer)
            {
                uint value = *(uint*)serializer.Read;
                serializer.Read += sizeof(uint);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getUIntNull(dataDeSerializer serializer)
            {
                uint value = *(uint*)serializer.Read;
                serializer.Read += sizeof(uint);
                return (uint?)value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getLong(dataDeSerializer serializer)
            {
                long value = *(long*)serializer.Read;
                serializer.Read += sizeof(long);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getLongNull(dataDeSerializer serializer)
            {
                long value = *(long*)serializer.Read;
                serializer.Read += sizeof(long);
                return (long?)value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getULong(dataDeSerializer serializer)
            {
                ulong value = *(ulong*)serializer.Read;
                serializer.Read += sizeof(ulong);
                return value;
            }
            /// <summary>
            /// 反序列化整数值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数值</returns>
            private unsafe static object getULongNull(dataDeSerializer serializer)
            {
                ulong value = *(ulong*)serializer.Read;
                serializer.Read += sizeof(ulong);
                return (ulong?)value;
            }
            /// <summary>
            /// 反序列化字符
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符</returns>
            private unsafe static object getChar(dataDeSerializer serializer)
            {
                char value = *(char*)serializer.Read;
                serializer.Read += sizeof(char);
                return value;
            }
            /// <summary>
            /// 反序列化字符
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符</returns>
            private unsafe static object getCharNull(dataDeSerializer serializer)
            {
                char value = *(char*)serializer.Read;
                serializer.Read += sizeof(char);
                return (char?)value;
            }
            /// <summary>
            /// 反序列化日期值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>日期值</returns>
            private unsafe static object getDateTime(dataDeSerializer serializer)
            {
                long value = *(long*)serializer.Read;
                serializer.Read += sizeof(long);
                return new DateTime(value);
            }
            /// <summary>
            /// 反序列化日期值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>日期值</returns>
            private unsafe static object getDateTimeNull(dataDeSerializer serializer)
            {
                long value = *(long*)serializer.Read;
                serializer.Read += sizeof(long);
                return (DateTime?)new DateTime(value);
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            private unsafe static object getFloat(dataDeSerializer serializer)
            {
                float value = *(float*)serializer.Read;
                serializer.Read += sizeof(float);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            private unsafe static object getFloatNull(dataDeSerializer serializer)
            {
                float value = *(float*)serializer.Read;
                serializer.Read += sizeof(float);
                return (float?)value;
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            private unsafe static object getDouble(dataDeSerializer serializer)
            {
                double value = *(double*)serializer.Read;
                serializer.Read += sizeof(double);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            private unsafe static object getDoubleNull(dataDeSerializer serializer)
            {
                double value = *(double*)serializer.Read;
                serializer.Read += sizeof(double);
                return (double?)value;
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            private unsafe static object getDecimal(dataDeSerializer serializer)
            {
                decimal value = *(decimal*)serializer.Read;
                serializer.Read += sizeof(decimal);
                return value;
            }
            /// <summary>
            /// 反序列化浮点值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值</returns>
            private unsafe static object getDecimalNull(dataDeSerializer serializer)
            {
                decimal value = *(decimal*)serializer.Read;
                serializer.Read += sizeof(decimal);
                return (decimal?)value;
            }
            /// <summary>
            /// 反序列化Guid值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>Guid值</returns>
            private unsafe static object getGuid(dataDeSerializer serializer)
            {
                Guid value = *(Guid*)serializer.Read;
                serializer.Read += sizeof(Guid);
                return value;
            }
            /// <summary>
            /// 反序列化Guid值
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>Guid值</returns>
            private unsafe static object getGuidNull(dataDeSerializer serializer)
            {
                Guid value = *(Guid*)serializer.Read;
                serializer.Read += sizeof(Guid);
                return (Guid?)value;
            }
            /// <summary>
            /// 可空类型
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>可空类型数据</returns>
            private static object getNullTypeObject<valueType>(dataDeSerializer serializer) where valueType : struct
            {
                return (Nullable<valueType>)(serialize.deSerialize<valueType>.GetVersionMemerMap(serializer, (int)(serializer.Read - serializer.dataStart)));
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字节数组</returns>
            private static object getSByteArrayObject(dataDeSerializer serializer)
            {
                return GetSByteArray(serializer);
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字节数组</returns>
            private static object getSByteNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.sByteNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getShortArrayObject(dataDeSerializer serializer)
            {
                return GetShortArray(serializer);
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getShortNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.shortNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getUShortArrayObject(dataDeSerializer serializer)
            {
                return GetUShortArray(serializer);
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getUShortNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.uShortNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getIntArrayObject(dataDeSerializer serializer)
            {
                return GetIntArray(serializer);
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getIntNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.intNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getUIntArrayObject(dataDeSerializer serializer)
            {
                return GetUIntArray(serializer);
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getUIntNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.uIntNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getLongArrayObject(dataDeSerializer serializer)
            {
                return GetLongArray(serializer);
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getLongNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.longNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getULongArrayObject(dataDeSerializer serializer)
            {
                return GetULongArray(serializer);
            }
            /// <summary>
            /// 反序列化整数数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>整数数组</returns>
            private static object getULongNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.uLongNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化字符数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符数组</returns>
            private static object getCharArrayObject(dataDeSerializer serializer)
            {
                return GetCharArray(serializer);
            }
            /// <summary>
            /// 反序列化字符数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符数组</returns>
            private static object getCharNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.charNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化时间数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>时间数组</returns>
            private static object getDateTimeArrayObject(dataDeSerializer serializer)
            {
                return serializer.dateTimeArrayNoPoint();
            }
            /// <summary>
            /// 反序列化时间数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>时间数组</returns>
            private static object getDateTimeNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.dateTimeNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            private static unsafe object getFloatArrayObject(dataDeSerializer serializer)
            {
                return GetFloatArray(serializer);
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            private static object getFloatNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.floatNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            private static object getDoubleArrayObject(dataDeSerializer serializer)
            {
                return GetDoubleArray(serializer);
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            private static object getDoubleNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.doubleNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            private static object getDecimalArrayObject(dataDeSerializer serializer)
            {
                return serializer.decimalArrayNoPoint();
            }
            /// <summary>
            /// 反序列化浮点值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>浮点值数组</returns>
            private static object getDecimalNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.decimalNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化Guid数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>Guid数组</returns>
            private static object getGuidArrayObject(dataDeSerializer serializer)
            {
                return serializer.guidArrayNoPoint();
            }
            /// <summary>
            /// 反序列化Guid数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>Guid数组</returns>
            private static object getGuidNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.guidNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化逻辑值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>逻辑值数组</returns>
            private static object getBoolArrayObject(dataDeSerializer serializer)
            {
                return serializer.boolArrayNoPoint();
            }
            /// <summary>
            /// 反序列化逻辑值数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>逻辑值数组</returns>
            private static object getBoolNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.boolNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字节数组</returns>
            private static object getByteArrayObject(dataDeSerializer serializer)
            {
                return serializer.byteArrayNoPoint();
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字节数组</returns>
            private static object getByteNullArrayObject(dataDeSerializer serializer)
            {
                return serializer.byteNullArrayNoPoint();
            }
            /// <summary>
            /// 反序列化字符串
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符串</returns>
            private static object getStringObject(dataDeSerializer serializer)
            {
                return serializer.getStringNoPoint();
            }
            /// <summary>
            /// 反序列化字符串数组
            /// </summary>
            /// <param name="serializer">反序列化器</param>
            /// <returns>字符串数组</returns>
            private static object getStringArrayObject(dataDeSerializer serializer)
            {
                return GetStringArray(serializer);
            }
            /// <summary>
            /// 反序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>数组</returns>
            private static object getArrayObject<valueType>(dataDeSerializer serializer)
            {
                return serializer.unknownArray<valueType>();
            }
            /// <summary>
            /// 反序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>数组</returns>
            private static object getArrayNullObject<valueType>(dataDeSerializer serializer) where valueType : struct
            {
                return serializer.unknownArrayNull<valueType>();
            }
            /// <summary>
            /// 反序列化数组
            /// </summary>
            /// <typeparam name="valueType">数组数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>数组</returns>
            private static object getArrayNotNullObject<valueType>(dataDeSerializer serializer)
            {
                return serializer.unknownArrayNotNull<valueType>();
            }
            /// <summary>
            /// 获取 成员对象反序列化
            /// </summary>
            /// <param name="type">成员类型</param>
            /// <param name="member">成员信息</param>
            /// <returns>成员对象反序列化</returns>
            private static Func<dataDeSerializer, object> getMemberGetter(Type type, code.memberInfo member)
            {
                memberType memberType = type;
                if (type.isStruct())
                {
                    if (memberType.IsISerialize)
                    {
                        return (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod("getISerializeTypeObjectNotNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type));
                    }
                    Type nullType = type.nullableType();
                    if (nullType != null)
                    {
                        if (nullType == typeof(bool)) return getBoolNull;
                        if (nullType == typeof(byte)) return getByteNull;
                        if (nullType == typeof(sbyte)) return getSByteNull;
                        if (nullType == typeof(short)) return getShortNull;
                        if (nullType == typeof(ushort)) return getUShortNull;
                        if (nullType == typeof(int)) return getIntNull;
                        if (nullType == typeof(uint)) return getUIntNull;
                        if (nullType == typeof(long)) return getLongNull;
                        if (nullType == typeof(ulong)) return getULongNull;
                        if (nullType == typeof(DateTime)) return getDateTimeNull;
                        if (nullType == typeof(char)) return getCharNull;
                        if (nullType == typeof(float)) return getFloatNull;
                        if (nullType == typeof(double)) return getDoubleNull;
                        if (nullType == typeof(decimal)) return getDecimalNull;
                        if (nullType == typeof(Guid)) return getGuidNull;
                        return (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod("getNullTypeObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(nullType));
                    }
                    if (type == typeof(bool)) return getBool;
                    if (type == typeof(byte)) return getByte;
                    if (type == typeof(sbyte)) return getSByte;
                    if (type == typeof(short)) return getShort;
                    if (type == typeof(ushort)) return getUShort;
                    if (type == typeof(int)) return getInt;
                    if (type == typeof(uint)) return getUInt;
                    if (type == typeof(long)) return getLong;
                    if (type == typeof(ulong)) return getULong;
                    if (type == typeof(DateTime)) return getDateTime;
                    if (type == typeof(char)) return getChar;
                    if (type == typeof(float)) return getFloat;
                    if (type == typeof(double)) return getDouble;
                    if (type == typeof(decimal)) return getDecimal;
                    if (type == typeof(Guid)) return getGuid;
                    return getObjectByType(type, true);
                }
                if (memberType.IsISerialize)
                {
                    return (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod("getISerializeTypeObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type));
                }
                if (type.IsArray)
                {
                    Type enumerableType = memberType.EnumerableArgumentType;
                    if (((memberType)enumerableType).IsISerialize)
                    {
                        if (enumerableType.isStruct())
                        {
                            return (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod("getISerializeTypeArrayNotNullObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                        }
                        return (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod("getISerializeTypeArrayObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                    }
                    Type nullType = enumerableType.nullableType();
                    if (nullType != null)
                    {
                        if (nullType == typeof(bool)) return getBoolNullArrayObject;
                        if (nullType == typeof(byte)) return getByteNullArrayObject;
                        if (nullType == typeof(sbyte)) return getSByteNullArrayObject;
                        if (nullType == typeof(short)) return getShortNullArrayObject;
                        if (nullType == typeof(ushort)) return getUShortNullArrayObject;
                        if (nullType == typeof(int)) return getIntNullArrayObject;
                        if (nullType == typeof(uint)) return getUIntNullArrayObject;
                        if (nullType == typeof(long)) return getLongNullArrayObject;
                        if (nullType == typeof(ulong)) return getULongNullArrayObject;
                        if (nullType == typeof(DateTime)) return getDateTimeNullArrayObject;
                        if (nullType == typeof(char)) return getCharNullArrayObject;
                        if (nullType == typeof(float)) return getFloatNullArrayObject;
                        if (nullType == typeof(double)) return getDoubleNullArrayObject;
                        if (nullType == typeof(decimal)) return getDecimalNullArrayObject;
                        if (nullType == typeof(Guid)) return getGuidNullArrayObject;
                        return (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod("getArrayNullObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                    }
                    if (enumerableType == typeof(bool)) return getBoolArrayObject;
                    if (enumerableType == typeof(byte)) return getByteArrayObject;
                    if (enumerableType == typeof(sbyte)) return getSByteArrayObject;
                    if (enumerableType == typeof(short)) return getShortArrayObject;
                    if (enumerableType == typeof(ushort)) return getUShortArrayObject;
                    if (enumerableType == typeof(int)) return getIntArrayObject;
                    if (enumerableType == typeof(uint)) return getUIntArrayObject;
                    if (enumerableType == typeof(long)) return getLongArrayObject;
                    if (enumerableType == typeof(ulong)) return getULongArrayObject;
                    if (enumerableType == typeof(DateTime)) return getDateTimeArrayObject;
                    if (enumerableType == typeof(char)) return getCharArrayObject;
                    if (enumerableType == typeof(float)) return getFloatArrayObject;
                    if (enumerableType == typeof(double)) return getDoubleArrayObject;
                    if (enumerableType == typeof(decimal)) return getDecimalArrayObject;
                    if (enumerableType == typeof(Guid)) return getGuidArrayObject;
                    if (enumerableType == typeof(string)) return getStringArrayObject;
                    if (enumerableType.isStruct())
                    {
                        return (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod("getArrayNotNullObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                    }
                    return (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod("getArrayObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(((memberType)type).EnumerableArgumentType));
                }
                if (type == typeof(string)) return getStringObject;
                return getObjectByType(type, false);
            }
            /// <summary>
            /// 获取 成员对象反序列化
            /// </summary>
            /// <param name="type">成员类型</param>
            /// <returns>成员对象反序列化</returns>
            internal static Func<dataDeSerializer, object> GetMemberGetter(Type type)//code.memberInfo member
            {
                Func<dataDeSerializer, object> getMember;
                interlocked.CompareSetSleep0(ref getMemberLock);
                try
                {
                    if (!getMemberTypes.TryGetValue(type, out getMember)) getMemberTypes.Add(type, getMember = getMemberGetter(type, null));
                }
                finally { getMemberLock = 0; }
                return getMember;
            }
            /// <summary>
            /// 成员对象反序列化器访问锁
            /// </summary>
            private static int getMemberLock;
            /// <summary>
            /// 成员对象反序列化器集合
            /// </summary>
            private static readonly Dictionary<Type, Func<dataDeSerializer, object>> getMemberTypes = dictionary.CreateOnly<Type, Func<dataDeSerializer, object>>();
            /// <summary>
            /// 未知类型反序列化
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>数据对象</returns>
            private static object getObjectNotNull<valueType>(dataDeSerializer serializer)
            {
                return serialize.deSerialize<valueType>.GetVersionMemerMap(serializer, (int)(serializer.Read - serializer.dataStart));
            }
            /// <summary>
            /// 未知类型反序列化
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="serializer">反序列化器</param>
            /// <returns>数据对象</returns>
            private static object getObject<valueType>(dataDeSerializer serializer)
            {
                return serializer.unknownNoPoint<valueType>();
            }
            /// <summary>
            /// 获取 未知类型反序列化器
            /// </summary>
            /// <param name="type">未知类型</param>
            /// <param name="isStruct">是否值类型</param>
            /// <returns>未知类型反序列化器</returns>
            private static Func<dataDeSerializer, object> getObjectByType(Type type, bool isStruct)
            {
                Func<dataDeSerializer, object> getObject;
                interlocked.CompareSetSleep(ref getObjectLock);
                try
                {
                    if (!getObjectTypes.TryGetValue(type, out getObject))
                    {
                        getObjectTypes.Add(type, getObject = (Func<dataDeSerializer, object>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, object>), typeof(dataDeSerializer).GetMethod(isStruct ? "getObjectNotNull" : "getObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type)));
                    }
                }
                finally { getObjectLock = 0; }
                return getObject;
            }
            /// <summary>
            /// 未知类型反序列化器访问锁
            /// </summary>
            private static int getObjectLock;
            /// <summary>
            /// 未知类型反序列化器集合
            /// </summary>
            private static readonly Dictionary<Type, Func<dataDeSerializer, object>> getObjectTypes = dictionary.CreateOnly<Type, Func<dataDeSerializer, object>>();
        }
        /// <summary>
        /// 对象反序列化
        /// </summary>
        public new unsafe abstract class deSerializer : dataDeSerializer
        {
            /// <summary>
            /// 序列化版本号
            /// </summary>
            protected int dataVersion;
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="dataStart">序列化数据起始位置</param>
            /// <param name="dataEnd">序列化数据结束位置</param>
            protected deSerializer(bool isReferenceMember, byte* dataStart, byte* dataEnd)
                : base(isReferenceMember, dataStart, dataEnd)
            {
            }
            /// <summary>
            /// 对象集合反序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="dataStart">序列化数据起始位置</param>
            /// <param name="dataEnd">序列化数据结束位置</param>
            /// <param name="version">序列化格式版本号</param>
            protected deSerializer(bool isReferenceMember, byte* dataStart, byte* dataEnd, serializeVersion version)
                : base(isReferenceMember, dataStart, dataEnd)
            {
                if (*(int*)Read != (int)version)
                {
                    fastCSharp.log.Default.Throw("序列化格式版本号不匹配 fastCSharp.code.cSharp.serializeVersion." + version.ToString() + "[" + ((int)version).toString() + "] != " + (*(int*)Read).toString(), true, false);
                }
                Read += sizeof(int);
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            public deSerializer(dataDeSerializer parentDeSerializer, bool isReferenceMember)
                : base(parentDeSerializer, isReferenceMember)
            {
            }
            /// <summary>
            /// 版本号+成员位图接口
            /// </summary>
            protected abstract bool versionMemberMap();
            /// <summary>
            /// 数据结束标识检测
            /// </summary>
            /// <returns>是否合法结束</returns>
            protected internal bool checkDataEnd()
            {
                if (Read != dataEnd)
                {
                    fastCSharp.log.Default.Add("数据解析错误", true, false);
                    return false;
                }
                Read += sizeof(int);
                return true;
            }
            /// <summary>
            /// 反序列化字节数组
            /// </summary>
            /// <returns>字节数组</returns>
            protected byte[] byteArray()
            {
                if (isReferenceMember)
                {
                    object reference = getPoint();
                    if (reference != null) return (byte[])reference;
                }
                return byteArrayNoPoint();
            }
        }
        /// <summary>
        /// 对象序列化器(反射模式)
        /// </summary>
        internal sealed class reflectionDataSerializer : dataSerializer
        {
            /// <summary>
            /// 对象序列化器
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="stream">序列化流</param>
            /// <param name="memberMap">成员位图接口</param>
            /// <param name="memberFilter">成员选择</param>
            public reflectionDataSerializer(bool isReferenceMember, unmanagedStream stream, IMemberMap memberMap, code.memberFilters memberFilter)
                : base(isReferenceMember, stream, memberMap, memberFilter)
            {
            }
            /// <summary>
            /// 对象序列化器
            /// </summary>
            /// <param name="parentSerializer">序列化</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="memberMap">成员位图接口</param>
            public reflectionDataSerializer(unmanagedStreamSerializer parentSerializer, bool isReferenceMember, IMemberMap memberMap)
                : base(parentSerializer, isReferenceMember, memberMap)
            {
            }
        }
        /// <summary>
        /// 对象序列化(反射模式)
        /// </summary>
        public static class dataSerialize
        {
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="value">数据对象</param>
            /// <param name="filter">成员选择,默认为公共字段成员</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>序列化数据</returns>
            public static byte[] Get<valueType>(valueType value
                    , code.memberFilters filter = defaultMemberFilter, memberMap<valueType> memberMap = default(memberMap<valueType>))
            {
                return dataSerialize<valueType>.Get(value, filter, memberMap);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="stream">内存数据流</param>
            /// <param name="value">数据对象</param>
            /// <param name="filter">成员选择,默认为公共字段成员</param>
            /// <param name="memberMap">成员位图</param>
            public static void Get<valueType>(unmanagedStream stream, valueType value
                    , code.memberFilters filter = defaultMemberFilter, memberMap<valueType> memberMap = default(memberMap<valueType>))
            {
                dataSerialize<valueType>.Get(stream, value, filter, memberMap);
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="value">数据对象</param>
            /// <param name="filter">成员选择,默认为公共字段成员</param>
            /// <returns>序列化数据</returns>
            public static byte[] Get(object value, code.memberFilters filter = defaultMemberFilter)
            {
                return value != null ? getter(value.GetType())(value, filter) : NullValueData;
            }
            /// <summary>
            /// 获取 对象序列化委托
            /// </summary>
            /// <param name="type">对象类型</param>
            /// <returns>对象序列化委托</returns>
            private static Func<object, code.memberFilters, byte[]> getter(Type type)
            {
                Func<object, code.memberFilters, byte[]> value;
                interlocked.CompareSetSleep(ref getterLock);
                try
                {
                    if (!getters.TryGetValue(type, out value))
                    {
                        getters.Add(type, value = (Func<object, code.memberFilters, byte[]>)Delegate.CreateDelegate(typeof(Func<object, code.memberFilters, byte[]>), typeof(dataSerialize<>).MakeGenericType(type).GetMethod("getObject", BindingFlags.Static | BindingFlags.NonPublic)));
                    }
                }
                finally { getterLock = 0; }
                return value;
            }
            /// <summary>
            /// 对象序列化委托 访问锁
            /// </summary>
            private static int getterLock;
            /// <summary>
            /// 对象序列化委托 集合
            /// </summary>
            private static readonly Dictionary<Type, Func<object, code.memberFilters, byte[]>> getters = dictionary.CreateOnly<Type, Func<object, code.memberFilters, byte[]>>();
        }
        /// <summary>
        /// 对象序列化(反射模式)
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        /// <typeparam name="serializeType">序列化类型</typeparam>
        /// <typeparam name="reflectionSerializerType">反射序列化器类型</typeparam>
        internal abstract class serializer<valueType, serializerType, reflectionSerializerType> : serializeBase.serializer<valueType, serializerType>
            where serializerType : serializeBase
        {
            /// <summary>
            /// 对象序列化
            /// </summary>
            protected static Action<reflectionSerializerType, valueType> getter;
            /// <summary>
            /// 成员对象序列化 集合
            /// </summary>
            protected static Action<reflectionSerializerType, object>[] memberGetters;
            /// <summary>
            /// 成员序列化类型转换器集合
            /// </summary>
            protected static Func<object, object>[] converters;
            /// <summary>
            /// 未知类型成员对象序列化
            /// </summary>
            protected static Action<reflectionSerializerType, object> unknownMemberGetter;
            /// <summary>
            /// 未知类型成员序列化类型转换器
            /// </summary>
            protected static Func<object, object> unknownConverter;
            /// <summary>
            /// 初始化成员序列化器
            /// </summary>
            /// <param name="getter">成员序列化器获取器</param>
            protected static void setMemberGetter(Func<Type, Action<reflectionSerializerType, object>> getter)
            {
                if (isUnknownValue)
                {
                    keyValue<Action<reflectionSerializerType, object>, Func<object, object>> memberGetter = getMemberGetter(unknownField, getter);
                    unknownMemberGetter = memberGetter.Key;
                    unknownConverter = memberGetter.Value;
                }
                else
                {
                    memberGetters = new Action<reflectionSerializerType, object>[memberCount];
                    converters = new Func<object, object>[memberCount];
                    foreach (code.memberInfo member in sortMembers)
                    {
                        keyValue<Action<reflectionSerializerType, object>, Func<object, object>> memberGetter = getMemberGetter(member, getter);
                        memberGetters[member.MemberIndex] = memberGetter.Key;
                        converters[member.MemberIndex] = memberGetter.Value;
                    }
                }
            }
            /// <summary>
            /// 获取成员序列化器
            /// </summary>
            /// <param name="member">成员信息</param>
            /// <param name="getter">成员序列化器获取器</param>
            /// <returns>成员序列化器</returns>
            private static keyValue<Action<reflectionSerializerType, object>, Func<object, object>> getMemberGetter
                (code.memberInfo member, Func<Type, Action<reflectionSerializerType, object>> getter)
            {
                Action<reflectionSerializerType, object> memberGetter = null;
                Func<object, object> converter = null;
                bool isArray = false;
                if (member.MemberType.Type.IsArray)
                {
                    memberType genericType = member.MemberType.Type.GetElementType();
                    if (genericType.SerializeType.Type != genericType.Type)
                    {
                        isArray = true;
                        memberGetter = getter(genericType.SerializeType.Type.MakeArrayType());
                        converter = reflection.converter.GetArray(genericType.Type, genericType.SerializeType.Type);
                    }
                }
                if (!isArray)
                {
                    if (member.MemberType.SerializeType.Type.IsArray)
                    {
                        memberType genericType = member.MemberType.SerializeType.Type.GetElementType();
                        if (genericType.SerializeType.Type != genericType.Type)
                        {
                            isArray = true;
                            memberGetter = getter(genericType.SerializeType.Type.MakeArrayType());
                            converter = new converter2
                            {
                                Converter1 = reflection.converter.Get(member.MemberType.Type, member.MemberType.SerializeType.Type),
                                Converter2 = reflection.converter.GetArray(genericType.Type, genericType.SerializeType.Type)
                            }.Convert;
                        }
                    }
                }
                if (!isArray)
                {
                    memberGetter = getter(member.MemberType.SerializeType);
                    if (member.MemberType.SerializeType.Type != member.MemberType.Type)
                    {
                        converter = reflection.converter.Get(member.MemberType.Type, member.MemberType.SerializeType.Type);
                    }
                }
                return new keyValue<Action<reflectionSerializerType, object>, Func<object, object>>(memberGetter, converter);
            }
        }
        /// <summary>
        /// 对象序列化(反射模式)
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        /// <typeparam name="serializeType">序列化类型</typeparam>
        internal abstract class dataSerialize<valueType, serializeType> : serializer<valueType, serializeType, unmanagedStreamSerializer>
            where serializeType : serializeBase
        {
            /// <summary>
            /// 是否未知类型泛型包装
            /// </summary>
            protected new static bool isUnknownValue = serializer<valueType>.isUnknownValue;
            static dataSerialize()
            {
                if (!isUnknownValue)
                {
                    bool isSerialize = typeof(serializeType) == typeof(serialize);
                    if (isSerialize && isISerialize)
                    {
                        if (isSerialize) getter = (Action<unmanagedStreamSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, valueType>), typeof(unmanagedStreamSerializer).GetMethod("getISerializeType", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeof(valueType)));
                        return;
                    }
                    Type type = typeof(valueType);
                    if (isStruct)
                    {
                        if (nullType != null)
                        {
                            if (nullType == typeof(bool))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, bool?>)unmanagedStreamSerializer.GetBool;
                                return;
                            }
                            if (nullType == typeof(byte))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, byte?>)unmanagedStreamSerializer.GetByte;
                                return;
                            }
                            if (nullType == typeof(sbyte))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, sbyte?>)unmanagedStreamSerializer.GetSByte;
                                return;
                            }
                            if (nullType == typeof(short))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, short?>)unmanagedStreamSerializer.GetShort;
                                return;
                            }
                            if (nullType == typeof(ushort))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, ushort?>)unmanagedStreamSerializer.GetUShort;
                                return;
                            }
                            if (nullType == typeof(int))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, int?>)unmanagedStreamSerializer.GetInt;
                                return;
                            }
                            if (nullType == typeof(uint))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, uint?>)unmanagedStreamSerializer.GetUInt;
                                return;
                            }
                            if (nullType == typeof(long))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, long?>)unmanagedStreamSerializer.GetLong;
                                return;
                            }
                            if (nullType == typeof(ulong))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, ulong?>)unmanagedStreamSerializer.GetULong;
                                return;
                            }
                            if (nullType == typeof(char))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, char?>)unmanagedStreamSerializer.GetChar;
                                return;
                            }
                            if (nullType == typeof(DateTime))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, DateTime?>)unmanagedStreamSerializer.GetDateTime;
                                return;
                            }
                            if (nullType == typeof(float))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, float?>)unmanagedStreamSerializer.GetFloat;
                                return;
                            }
                            if (nullType == typeof(double))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, double?>)unmanagedStreamSerializer.GetDouble;
                                return;
                            }
                            if (nullType == typeof(decimal))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, decimal?>)unmanagedStreamSerializer.GetDecimal;
                                return;
                            }
                            if (nullType == typeof(Guid))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, Guid?>)unmanagedStreamSerializer.GetGuid;
                                return;
                            }
                            getter = (Action<unmanagedStreamSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, valueType>), typeof(unmanagedStreamSerializer).GetMethod("getNullType", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(nullType));
                            return;
                        }
                        if (type == typeof(bool))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, bool>)unmanagedStreamSerializer.GetBool;
                            return;
                        }
                        if (type == typeof(byte))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, byte>)unmanagedStreamSerializer.GetByte;
                            return;
                        }
                        if (type == typeof(sbyte))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, sbyte>)unmanagedStreamSerializer.GetSByte;
                            return;
                        }
                        if (type == typeof(short))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, short>)unmanagedStreamSerializer.GetShort;
                            return;
                        }
                        if (type == typeof(ushort))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, ushort>)unmanagedStreamSerializer.GetUShort;
                            return;
                        }
                        if (type == typeof(int))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, int>)unmanagedStreamSerializer.GetInt;
                            return;
                        }
                        if (type == typeof(uint))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, uint>)unmanagedStreamSerializer.GetUInt;
                            return;
                        }
                        if (type == typeof(long))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, long>)unmanagedStreamSerializer.GetLong;
                            return;
                        }
                        if (type == typeof(ulong))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, ulong>)unmanagedStreamSerializer.GetULong;
                            return;
                        }
                        if (type == typeof(char))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, char>)unmanagedStreamSerializer.GetChar;
                            return;
                        }
                        if (type == typeof(DateTime))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, DateTime>)unmanagedStreamSerializer.GetDateTime;
                            return;
                        }
                        if (type == typeof(float))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, float>)unmanagedStreamSerializer.GetFloat;
                            return;
                        }
                        if (type == typeof(double))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, double>)unmanagedStreamSerializer.GetDouble;
                            return;
                        }
                        if (type == typeof(decimal))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, decimal>)unmanagedStreamSerializer.GetDecimal;
                            return;
                        }
                        if (type == typeof(Guid))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, Guid>)unmanagedStreamSerializer.GetGuid;
                            return;
                        }
                        setMemberGetter(unmanagedStreamSerializer.GetMemberGetter);
                        return;
                    }
                    if (type.IsArray)
                    {
                        Type enumerableType = ((memberType)type).EnumerableArgumentType;
                        if (((memberType)enumerableType).IsISerialize)
                        {
                            if (enumerableType.isStruct())
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, valueType>), typeof(unmanagedStreamSerializer).GetMethod("getISerializeTypeArrayNotNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                                return;
                            }
                            getter = (Action<unmanagedStreamSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, valueType>), typeof(unmanagedStreamSerializer).GetMethod("getISerializeTypeArray", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                            return;
                        }
                        Type nullType = enumerableType.nullableType();
                        if (nullType != null)
                        {
                            if (nullType == typeof(bool))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, bool?[]>)unmanagedStreamSerializer.GetBoolNullArray;
                                return;
                            }
                            if (nullType == typeof(byte))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, byte?[]>)unmanagedStreamSerializer.GetByteNullArray;
                                return;
                            }
                            if (nullType == typeof(sbyte))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, sbyte?[]>)unmanagedStreamSerializer.GetSByteNullArray;
                                return;
                            }
                            if (nullType == typeof(short))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, short?[]>)unmanagedStreamSerializer.GetShortNullArray;
                                return;
                            }
                            if (nullType == typeof(ushort))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, ushort?[]>)unmanagedStreamSerializer.GetUShortNullArray;
                                return;
                            }
                            if (nullType == typeof(int))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, int?[]>)unmanagedStreamSerializer.GetIntNullArray;
                                return;
                            }
                            if (nullType == typeof(uint))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, uint?[]>)unmanagedStreamSerializer.GetUIntNullArray;
                                return;
                            }
                            if (nullType == typeof(long))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, long?[]>)unmanagedStreamSerializer.GetLongNullArray;
                                return;
                            }
                            if (nullType == typeof(ulong))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, ulong?[]>)unmanagedStreamSerializer.GetULongNullArray;
                                return;
                            }
                            if (nullType == typeof(char))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, char?[]>)unmanagedStreamSerializer.GetCharNullArray;
                                return;
                            }
                            if (nullType == typeof(DateTime))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, DateTime?[]>)unmanagedStreamSerializer.GetDateTimeNullArray;
                                return;
                            }
                            if (nullType == typeof(float))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, float?[]>)unmanagedStreamSerializer.GetFloatNullArray;
                                return;
                            }
                            if (nullType == typeof(double))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, double?[]>)unmanagedStreamSerializer.GetDoubleNullArray;
                                return;
                            }
                            if (nullType == typeof(decimal))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, decimal?[]>)unmanagedStreamSerializer.GetDecimalNullArray;
                                return;
                            }
                            if (nullType == typeof(Guid))
                            {
                                getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, Guid?[]>)unmanagedStreamSerializer.GetGuidNullArray;
                                return;
                            }
                            getter = (Action<unmanagedStreamSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, valueType>), typeof(unmanagedStreamSerializer).GetMethod("getArrayNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                            return;
                        }
                        if (enumerableType == typeof(bool))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, bool[]>)unmanagedStreamSerializer.GetBoolArray;
                            return;
                        }
                        if (enumerableType == typeof(byte))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, byte[]>)unmanagedStreamSerializer.GetByteArray;
                            return;
                        }
                        if (enumerableType == typeof(sbyte))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, sbyte[]>)unmanagedStreamSerializer.GetSByteArray;
                            return;
                        }
                        if (enumerableType == typeof(short))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, short[]>)unmanagedStreamSerializer.GetShortArray;
                            return;
                        }
                        if (enumerableType == typeof(ushort))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, ushort[]>)unmanagedStreamSerializer.GetUShortArray;
                            return;
                        }
                        if (enumerableType == typeof(int))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, int[]>)unmanagedStreamSerializer.GetIntArray;
                            return;
                        }
                        if (enumerableType == typeof(uint))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, uint[]>)unmanagedStreamSerializer.GetUIntArray;
                            return;
                        }
                        if (enumerableType == typeof(long))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, long[]>)unmanagedStreamSerializer.GetLongArray;
                            return;
                        }
                        if (enumerableType == typeof(ulong))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, ulong[]>)unmanagedStreamSerializer.GetULongArray;
                            return;
                        }
                        if (enumerableType == typeof(char))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, char[]>)unmanagedStreamSerializer.GetCharArray;
                            return;
                        }
                        if (enumerableType == typeof(DateTime))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, DateTime[]>)unmanagedStreamSerializer.GetDateTimeArray;
                            return;
                        }
                        if (enumerableType == typeof(float))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, float[]>)unmanagedStreamSerializer.GetFloatArray;
                            return;
                        }
                        if (enumerableType == typeof(double))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, double[]>)unmanagedStreamSerializer.GetDoubleArray;
                            return;
                        }
                        if (enumerableType == typeof(decimal))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, decimal[]>)unmanagedStreamSerializer.GetDecimalArray;
                            return;
                        }
                        if (enumerableType == typeof(Guid))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, Guid[]>)unmanagedStreamSerializer.GetGuidArray;
                            return;
                        }
                        if (enumerableType == typeof(string))
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, string[]>)unmanagedStreamSerializer.GetStringArray;
                            return;
                        }
                        if (enumerableType.isStruct())
                        {
                            getter = (Action<unmanagedStreamSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, valueType>), typeof(unmanagedStreamSerializer).GetMethod("getArrayNotNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                            return;
                        }
                        getter = (Action<unmanagedStreamSerializer, valueType>)Delegate.CreateDelegate(typeof(Action<unmanagedStreamSerializer, valueType>), typeof(unmanagedStreamSerializer).GetMethod("getArray", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                        return;
                    }
                    if (type == typeof(string))
                    {
                        getter = (Action<unmanagedStreamSerializer, valueType>)(Delegate)(Action<unmanagedStreamSerializer, string>)unmanagedStreamSerializer.GetString;
                        return;
                    }
                    setMemberGetter(unmanagedStreamSerializer.GetMemberGetter);
                }
            }
        }
        /// <summary>
        /// 对象序列化(反射模式)
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        internal sealed class dataSerialize<valueType> : dataSerialize<valueType, serialize>
        {
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="value">数据对象</param>
            /// <param name="filter">成员选择</param>
            /// <returns>序列化数据</returns>
            private static byte[] getObject(object value, code.memberFilters filter)
            {
                return Get((valueType)value, filter, default(memberMap<valueType>));
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="value">数据对象</param>
            /// <param name="filter">成员选择</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>序列化数据</returns>
            public unsafe static byte[] Get(valueType value, code.memberFilters filter, memberMap<valueType> memberMap)
            {
                pointer buffer = fastCSharp.unmanagedPool.StreamBuffers.Get();
                try
                {
                    using (unmanagedStream stream = new unmanagedStream(buffer.Byte, fastCSharp.unmanagedPool.StreamBuffers.Size))
                    {
                        Get(stream, value, filter, memberMap);
                        return stream.GetArray();
                    }
                }
                finally { fastCSharp.unmanagedPool.StreamBuffers.Push(ref buffer); }
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="stream">内存数据流</param>
            /// <param name="value">数据对象</param>
            /// <param name="filter">成员选择</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>序列化数据</returns>
            public static void Get(unmanagedStream stream, valueType value, code.memberFilters filter, memberMap<valueType> memberMap)
            {
                reflectionDataSerializer serializer = new reflectionDataSerializer(serializeAttribute.IsReferenceMember, stream, memberMap, filter);
                if ((!isStruct || nullType != null) && value == null) stream.Write(fastCSharp.emit.binarySerializer.NullValue);
                else
                {
                    if (!isStruct) serializer.checkPointReferenceMember(value);
                    if (getter != null) getter(serializer, value);
                    else GetVersionMemerMap(serializer, value);
                }
                serializer.Finally();
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            /// <param name="value">数据对象</param>
            public static void GetVersionMemerMap(unmanagedStreamSerializer serializer, valueType value)
            {
                if (isUnknownValue) getUnknownMember(serializer, value);
                else
                {
                    serializer.memberMap = getSerializeMemberMap(serializer.memberFilter, serializer.memberMap);
                    serializer.versionMemerMap(serializeAttribute.Version);
                    getMember(serializer, value);
                    serializer.memberMap.PushPool();
                }
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            /// <param name="value">数据对象</param>
            private unsafe static void getMember(unmanagedStreamSerializer serializer, valueType value)
            {
                subArray<keyValue<code.memberInfo, object>> memberValues = memberGroup.GetMemberValue(value, serializer.memberMap.IsDefault ? code.memberFilters.Instance : serializer.memberFilter, (memberMap<valueType>)serializer.memberMap);
                byte* isValue = stackalloc byte[memberMapSize];
                fixedMap isValueMap = new fixedMap(isValue, memberMapSize);
                keyValue<object, object>[] values = new keyValue<object, object>[memberCount];
                int memberValueCount = memberValues.Count;
                if (memberValueCount != 0)
                {
                    foreach (keyValue<code.memberInfo, object> memberValue in memberValues.Array)
                    {
                        int memberIndex = memberValue.Key.MemberIndex;
                        Func<object, object> converter = converters[memberIndex];
                        isValueMap.Set(memberIndex);
                        values[memberIndex].Set(memberValue.Value, converter == null ? memberValue.Value : converter(memberValue.Value));
                        if (--memberValueCount == 0) break;
                    }
                }
                unmanagedStream dataStream = serializer.dataStream;
                unmanagedStream.unsafer unsafeStream = dataStream.Unsafer;
                int length = serializer.memberMap.SerializeSize;
                dataStream.PrepLength(length + serializeSize);
                serializer.write = dataStream.CurrentData;
                fixedMap nullMap = new fixedMap(serializer.write);
                fastCSharp.unsafer.memory.Fill(serializer.write, (uint)0, length >> 2);
                serializer.write += length;
                for (int* memberIndexStart = memberSort.Int, memberIndexEnd = memberIndexStart + memberGroup.Count; memberIndexStart != memberIndexEnd; ++memberIndexStart)
                {
                    int memberIndex = *memberIndexStart;
                    if (isValueMap.Get(memberIndex))
                    {
                        object objectValue = values[memberIndex].Value;
                        if (objectValue == null) nullMap.Set(memberIndex);
                        else if (isMemberSerializeMap.Get(memberIndex)) memberGetters[memberIndex](serializer, objectValue);
                    }
                }
                unsafeStream.AddLength(((int)(serializer.write - dataStream.CurrentData) + 3) & (int.MaxValue - 3));
                byte* copyNullMap = stackalloc byte[length];
                fastCSharp.unsafer.memory.Copy(nullMap.Map, copyNullMap, length);
                nullMap = new fixedMap(copyNullMap);
                dataStream.PrepLength();
                for (int* memberIndexStart = memberSort.Int, memberIndexEnd = memberIndexStart + memberGroup.Count; memberIndexStart != memberIndexEnd; ++memberIndexStart)
                {
                    int memberIndex = *memberIndexStart;
                    if (!isMemberSerializeMap.Get(memberIndex) && isValueMap.Get(memberIndex) && !nullMap.Get(memberIndex))
                    {
                        keyValue<object, object> objectValue = values[memberIndex];
                        if (!isNullMap.Get(memberIndex) || serializer.checkPointReferenceMember(objectValue.Key))
                        {
                            memberGetters[memberIndex](serializer, objectValue.Value);
                        }
                    }
                }
            }
            /// <summary>
            /// 对象序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            /// <param name="value">数据对象</param>
            private unsafe static void getUnknownMember(unmanagedStreamSerializer serializer, valueType value)
            {
                object memberValue = unknownField.ValueGetter(value), converterValue = unknownConverter == null ? memberValue : unknownConverter(memberValue);
                unmanagedStream dataStream = serializer.dataStream;
                unmanagedStream.unsafer unsafeStream = dataStream.Unsafer;
                dataStream.PrepLength(sizeof(int) + serializeSize);
                serializer.write = dataStream.CurrentData;
                if (converterValue == null)
                {
                    *(int*)serializer.write = 1;
                    unsafeStream.AddLength(sizeof(int));
                }
                else
                {
                    *(int*)serializer.write = 0;
                    if (isUnknownMemberSerialize)
                    {
                        serializer.write += sizeof(int);
                        unknownMemberGetter(serializer, converterValue);
                        unsafeStream.AddLength(((int)(serializer.write - dataStream.CurrentData) + 3) & (int.MaxValue - 3));
                    }
                    else
                    {
                        unsafeStream.AddLength(sizeof(int));
                        unknownMemberGetter(serializer, converterValue);
                    }
                }
                dataStream.PrepLength();
            }
            static dataSerialize()
            {
                if (dataSerialize<valueType, serialize>.isUnknownValue) setMemberGetter(unmanagedStreamSerializer.GetMemberGetter);
            }
        }
        /// <summary>
        /// 对象反序列化器(反射模式)
        /// </summary>
        internal abstract unsafe class reflectionDeSerializer : deSerializer
        {
            /// <summary>
            /// 版本号
            /// </summary>
            protected int version;
            /// <summary>
            /// 对象反序列化器
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="dataStart">序列化数据起始位置</param>
            /// <param name="dataEnd">序列化数据结束位置</param>
            /// <param name="version">版本号</param>
            protected reflectionDeSerializer(bool isReferenceMember, byte* dataStart, byte* dataEnd, int version)
                : base(isReferenceMember, dataStart, dataEnd, serializeVersion.serialize)
            {
                this.version = version;
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="version">版本号</param>
            protected reflectionDeSerializer(dataDeSerializer parentDeSerializer, bool isReferenceMember, int version)
                : base(parentDeSerializer, isReferenceMember)
            {
                this.version = version;
            }
        }
        /// <summary>
        /// 对象反序列化器(反射模式)
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        internal unsafe sealed class reflectionDeSerializer<valueType> : reflectionDeSerializer
        {
            /// <summary>
            /// 成员位图
            /// </summary>
            internal memberMap<valueType> MemberMap = default(memberMap<valueType>);
            /// <summary>
            /// 对象反序列化器
            /// </summary>
            /// <param name="dataStart">序列化数据起始位置</param>
            /// <param name="dataEnd">序列化数据结束位置</param>
            public reflectionDeSerializer(byte* dataStart, byte* dataEnd)
                : base(serializeBase.serializer<valueType, serialize>.serializeAttribute.IsReferenceMember, dataStart, dataEnd, serializeBase.serializer<valueType, serialize>.serializeAttribute.Version)
            {
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="parentDeSerializer">反序列化</param>
            /// <param name="version">版本号</param>
            public reflectionDeSerializer(dataDeSerializer parentDeSerializer, int version) : base(parentDeSerializer, serializeBase.serializer<valueType, serialize>.serializeAttribute.IsReferenceMember, version) { }
            /// <summary>
            /// 版本号+成员位图接口
            /// </summary>
            protected override bool versionMemberMap()
            {
                if (version != dataVersion)
                {
                    fastCSharp.log.Default.Add("序列化版本号错误 " + version.toString() + " != " + dataVersion.toString(), true, false);
                    return false;
                }
                Read = MemberMap.DeSerialize(Read += sizeof(int));
                return Read != null;
            }
            /// <summary>
            /// 版本号+成员位图接口
            /// </summary>
            internal bool VersionMemberMap()
            {
                this.dataVersion = *(int*)Read;
                return versionMemberMap();
            }
        }
        /// <summary>
        /// 对象反序列化(反射模式)
        /// </summary>
        public static class deSerialize
        {
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置</param>
            /// <returns>反序列化数据</returns>
            public static valueType Get<valueType>(byte[] data)
            {
                valueType value = fastCSharp.emit.constructor<valueType>.New();
                int endIndex;
                memberMap<valueType> memberMap;
                if (!Get(ref value, data, 0, out endIndex, out memberMap)) value = default(valueType);
                memberMap.PushPool();
                return value;
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置</param>
            /// <returns>反序列化数据</returns>
            public static unsafe valueType Get<valueType>(subArray<byte> data)
            {
                fixed (byte* dataFixed = data.Array)
                {
                    byte* dataStart = dataFixed + data.StartIndex;
                    return get<valueType>(dataStart, dataStart + data.Count);
                }
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="dataStart">数据起始位置</param>
            /// <param name="dataEnd">数据结束位置</param>
            /// <returns>反序列化数据</returns>
            public static unsafe valueType Get<valueType>(byte* dataStart, byte* dataEnd)
            {
                return dataStart != null && dataEnd != null && dataEnd > dataStart ? get<valueType>(dataStart, dataEnd) : default(valueType);
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="dataStart">数据起始位置</param>
            /// <param name="dataEnd">数据结束位置</param>
            /// <returns>反序列化数据</returns>
            private static unsafe valueType get<valueType>(byte* dataStart, byte* dataEnd)
            {
                valueType value = fastCSharp.emit.constructor<valueType>.New();
                byte* read;
                memberMap<valueType> memberMap;
                if (!deSerialize<valueType>.Get(ref value, dataStart, dataEnd, out read, out memberMap) || read > dataEnd) value = default(valueType);
                memberMap.PushPool();
                return value;
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置</param>
            /// <returns>反序列化数据</returns>
            public static unsafe valueType Get<valueType>(byte[] data, int startIndex, out int endIndex)
            {
                valueType value = fastCSharp.emit.constructor<valueType>.New();
                memberMap<valueType> memberMap;
                if (!Get(ref value, data, startIndex, out endIndex, out memberMap)) value = default(valueType);
                memberMap.PushPool();
                return value;
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="value">目标对象</param>
            /// <param name="data">序列化数据</param>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>是否成功</returns>
            public static unsafe bool Get<valueType>(ref valueType value, byte[] data, int startIndex, out int endIndex, out memberMap<valueType> memberMap)
            {
                fixed (byte* dataFixed = data)
                {
                    byte* read;
                    if (deSerialize<valueType>.Get(ref value, dataFixed + startIndex, dataFixed + data.Length, out read, out memberMap))
                    {
                        endIndex = (int)(read - dataFixed);
                        return true;
                    }
                }
                endIndex = -1;
                return false;
            }
        }
        /// <summary>
        /// 对象反序列化(反射模式)
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        /// <typeparam name="serializeType">序列化类型</typeparam>
        internal abstract class dataDeSerialize<valueType, serializeType> : serializeBase.serializer<valueType, serializeType>
            where serializeType : serializeBase
        {
            /// <summary>
            /// 对象反序列化
            /// </summary>
            protected static Func<dataDeSerializer, valueType> getter;
            /// <summary>
            /// 成员位图
            /// </summary>
            protected static memberMap<valueType> andMemberMap = default(memberMap<valueType>);
            /// <summary>
            /// 默认成员值集合
            /// </summary>
            protected static object[] defaultValues;
            /// <summary>
            /// 成员对象反序列化 集合
            /// </summary>
            protected static Func<dataDeSerializer, object>[] memberGetters;
            /// <summary>
            /// 成员反序列化类型转换器集合
            /// </summary>
            protected static Func<object, object>[] converters;
            /// <summary>
            /// 初始化成员序列化器
            /// </summary>
            private static void setMemberGetter()
            {
                defaultValues = new object[memberCount];
                memberGetters = new Func<dataDeSerializer, object>[memberCount];
                converters = new Func<object, object>[memberCount];
                foreach (code.memberInfo member in sortMembers)
                {
                    andMemberMap.SetMember(member.MemberIndex);
                    defaultValues[member.MemberIndex] = constructor.GetNull(member.MemberType);

                    keyValue<Func<dataDeSerializer, object>, Func<object, object>> memberGetter = getMemberGetter(member);
                    memberGetters[member.MemberIndex] = memberGetter.Key;
                    converters[member.MemberIndex] = memberGetter.Value;
                }
            }
            /// <summary>
            /// 获取成员对象反序列化器
            /// </summary>
            /// <param name="member">成员信息</param>
            /// <returns>成员对象反序列化器</returns>
            protected static keyValue<Func<dataDeSerializer, object>, Func<object, object>> getMemberGetter(code.memberInfo member)
            {
                Func<dataDeSerializer, object> memberGetter = null;
                Func<object, object> converter = null;
                bool isArray = false;
                if (member.MemberType.Type.IsArray)
                {
                    memberType genericType = member.MemberType.Type.GetElementType();
                    if (genericType.SerializeType.Type != genericType.Type)
                    {
                        isArray = true;
                        memberGetter = dataDeSerializer.GetMemberGetter(genericType.SerializeType.Type.MakeArrayType());
                        converter = reflection.converter.GetArray(genericType.SerializeType.Type, genericType.Type);
                    }
                }
                if (!isArray)
                {
                    if (member.MemberType.SerializeType.Type.IsArray)
                    {
                        memberType genericType = member.MemberType.SerializeType.Type.GetElementType();
                        if (genericType.SerializeType.Type != genericType.Type)
                        {
                            isArray = true;
                            memberGetter = dataDeSerializer.GetMemberGetter(genericType.SerializeType.Type.MakeArrayType());
                            converter = new converter2
                            {
                                Converter1 = reflection.converter.GetArray(genericType.SerializeType.Type, genericType.Type),
                                Converter2 = reflection.converter.Get(member.MemberType.SerializeType.Type, member.MemberType.Type)
                            }.Convert;
                        }
                    }
                }
                if (!isArray)
                {
                    memberGetter = dataDeSerializer.GetMemberGetter(member.MemberType.SerializeType);
                    if (member.MemberType.SerializeType.Type != member.MemberType.Type)
                    {
                        converter = reflection.converter.Get(member.MemberType.SerializeType.Type, member.MemberType.Type);
                    }
                }
                return new keyValue<Func<dataDeSerializer, object>, Func<object, object>>(memberGetter, converter);
            }
            static dataDeSerialize()
            {
                if (!isUnknownValue)
                {
                    bool isSerialize = typeof(serializeType) == typeof(serialize);
                    if (isSerialize && isISerialize)
                    {
                        if (isSerialize) getter = (Func<dataDeSerializer, valueType>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, valueType>), typeof(dataDeSerializer).GetMethod("getISerializeType", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeof(valueType)));
                        return;
                    }
                    Type type = typeof(valueType);
                    if (isStruct)
                    {
                        if (nullType != null)
                        {
                            if (nullType == typeof(bool))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, bool?>)dataDeSerializer.GetBoolNull;
                                return;
                            }
                            if (nullType == typeof(byte))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, byte?>)dataDeSerializer.GetByteNull;
                                return;
                            }
                            if (nullType == typeof(sbyte))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, sbyte?>)dataDeSerializer.GetSByteNull;
                                return;
                            }
                            if (nullType == typeof(short))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, short?>)dataDeSerializer.GetShortNull;
                                return;
                            }
                            if (nullType == typeof(ushort))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, ushort?>)dataDeSerializer.GetUShortNull;
                                return;
                            }
                            if (nullType == typeof(int))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, int?>)dataDeSerializer.GetIntNull;
                                return;
                            }
                            if (nullType == typeof(uint))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, uint?>)dataDeSerializer.GetUIntNull;
                                return;
                            }
                            if (nullType == typeof(long))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, long?>)dataDeSerializer.GetLongNull;
                                return;
                            }
                            if (nullType == typeof(ulong))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, ulong?>)dataDeSerializer.GetULongNull;
                                return;
                            }
                            if (nullType == typeof(char))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, char?>)dataDeSerializer.GetCharNull;
                                return;
                            }
                            if (nullType == typeof(DateTime))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, DateTime?>)dataDeSerializer.GetDateTimeNull;
                                return;
                            }
                            if (nullType == typeof(float))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, float?>)dataDeSerializer.GetFloatNull;
                                return;
                            }
                            if (nullType == typeof(double))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, double?>)dataDeSerializer.GetDoubleNull;
                                return;
                            }
                            if (nullType == typeof(decimal))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, decimal?>)dataDeSerializer.GetDecimalNull;
                                return;
                            }
                            if (nullType == typeof(Guid))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, Guid?>)dataDeSerializer.GetGuidNull;
                                return;
                            }
                            getter = (Func<dataDeSerializer, valueType>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, valueType>), typeof(dataDeSerializer).GetMethod("getNullType", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(nullType));
                            return;
                        }
                        if (type == typeof(bool))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, bool>)dataDeSerializer.GetBool;
                            return;
                        }
                        if (type == typeof(byte))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, byte>)dataDeSerializer.GetByte;
                            return;
                        }
                        if (type == typeof(sbyte))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, sbyte>)dataDeSerializer.GetSByte;
                            return;
                        }
                        if (type == typeof(short))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, short>)dataDeSerializer.GetShort;
                            return;
                        }
                        if (type == typeof(ushort))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, ushort>)dataDeSerializer.GetUShort;
                            return;
                        }
                        if (type == typeof(int))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, int>)dataDeSerializer.GetInt;
                            return;
                        }
                        if (type == typeof(uint))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, uint>)dataDeSerializer.GetUInt;
                            return;
                        }
                        if (type == typeof(long))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, long>)dataDeSerializer.GetLong;
                            return;
                        }
                        if (type == typeof(ulong))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, ulong>)dataDeSerializer.GetULong;
                            return;
                        }
                        if (type == typeof(char))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, char>)dataDeSerializer.GetChar;
                            return;
                        }
                        if (type == typeof(DateTime))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, DateTime>)dataDeSerializer.GetDateTime;
                            return;
                        }
                        if (type == typeof(float))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, float>)dataDeSerializer.GetFloat;
                            return;
                        }
                        if (type == typeof(double))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, double>)dataDeSerializer.GetDouble;
                            return;
                        }
                        if (type == typeof(decimal))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, decimal>)dataDeSerializer.GetDecimal;
                            return;
                        }
                        if (type == typeof(Guid))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, Guid>)dataDeSerializer.GetGuid;
                            return;
                        }
                        setMemberGetter();
                        return;
                    }

                    if (type.IsArray)
                    {
                        Type enumerableType = ((memberType)type).EnumerableArgumentType;
                        if (((memberType)enumerableType).IsISerialize)
                        {
                            if (enumerableType.isStruct())
                            {
                                getter = (Func<dataDeSerializer, valueType>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, valueType>), typeof(dataDeSerializer).GetMethod("getISerializeTypeArrayNotNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                                return;
                            }
                            getter = (Func<dataDeSerializer, valueType>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, valueType>), typeof(dataDeSerializer).GetMethod("getISerializeTypeArray", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                            return;
                        }
                        Type nullType = enumerableType.nullableType();
                        if (nullType != null)
                        {
                            if (nullType == typeof(bool))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, bool?[]>)dataDeSerializer.GetBoolNullArray;
                                return;
                            }
                            if (nullType == typeof(byte))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, byte?[]>)dataDeSerializer.GetByteNullArray;
                                return;
                            }
                            if (nullType == typeof(sbyte))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, sbyte?[]>)dataDeSerializer.GetSByteNullArray;
                                return;
                            }
                            if (nullType == typeof(short))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, short?[]>)dataDeSerializer.GetShortNullArray;
                                return;
                            }
                            if (nullType == typeof(ushort))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, ushort?[]>)dataDeSerializer.GetUShortNullArray;
                                return;
                            }
                            if (nullType == typeof(int))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, int?[]>)dataDeSerializer.GetIntNullArray;
                                return;
                            }
                            if (nullType == typeof(uint))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, uint?[]>)dataDeSerializer.GetUIntNullArray;
                                return;
                            }
                            if (nullType == typeof(long))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, long?[]>)dataDeSerializer.GetLongNullArray;
                                return;
                            }
                            if (nullType == typeof(ulong))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, ulong?[]>)dataDeSerializer.GetULongNullArray;
                                return;
                            }
                            if (nullType == typeof(char))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, char?[]>)dataDeSerializer.GetCharNullArray;
                                return;
                            }
                            if (nullType == typeof(DateTime))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, DateTime?[]>)dataDeSerializer.GetDateTimeNullArray;
                                return;
                            }
                            if (nullType == typeof(float))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, float?[]>)dataDeSerializer.GetFloatNullArray;
                                return;
                            }
                            if (nullType == typeof(double))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, double?[]>)dataDeSerializer.GetDoubleNullArray;
                                return;
                            }
                            if (nullType == typeof(decimal))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, decimal?[]>)dataDeSerializer.GetDecimalNullArray;
                                return;
                            }
                            if (nullType == typeof(Guid))
                            {
                                getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, Guid?[]>)dataDeSerializer.GetGuidNullArray;
                                return;
                            }
                            getter = (Func<dataDeSerializer, valueType>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, valueType>), typeof(dataDeSerializer).GetMethod("getArrayNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                            return;
                        }
                        if (enumerableType == typeof(bool))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, bool[]>)dataDeSerializer.GetBoolArray;
                            return;
                        }
                        if (enumerableType == typeof(byte))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, byte[]>)dataDeSerializer.GetByteArray;
                            return;
                        }
                        if (enumerableType == typeof(sbyte))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, sbyte[]>)dataDeSerializer.GetSByteArray;
                            return;
                        }
                        if (enumerableType == typeof(short))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, short[]>)dataDeSerializer.GetShortArray;
                            return;
                        }
                        if (enumerableType == typeof(ushort))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, ushort[]>)dataDeSerializer.GetUShortArray;
                            return;
                        }
                        if (enumerableType == typeof(int))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, int[]>)dataDeSerializer.GetIntArray;
                            return;
                        }
                        if (enumerableType == typeof(uint))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, uint[]>)dataDeSerializer.GetUIntArray;
                            return;
                        }
                        if (enumerableType == typeof(long))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, long[]>)dataDeSerializer.GetLongArray;
                            return;
                        }
                        if (enumerableType == typeof(ulong))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, ulong[]>)dataDeSerializer.GetULongArray;
                            return;
                        }
                        if (enumerableType == typeof(char))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, char[]>)dataDeSerializer.GetCharArray;
                            return;
                        }
                        if (enumerableType == typeof(DateTime))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, DateTime[]>)dataDeSerializer.GetDateTimeArray;
                            return;
                        }
                        if (enumerableType == typeof(float))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, float[]>)dataDeSerializer.GetFloatArray;
                            return;
                        }
                        if (enumerableType == typeof(double))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, double[]>)dataDeSerializer.GetDoubleArray;
                            return;
                        }
                        if (enumerableType == typeof(decimal))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, decimal[]>)dataDeSerializer.GetDecimalArray;
                            return;
                        }
                        if (enumerableType == typeof(Guid))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, Guid[]>)dataDeSerializer.GetGuidArray;
                            return;
                        }
                        if (enumerableType == typeof(string))
                        {
                            getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, string[]>)dataDeSerializer.GetStringArray;
                            return;
                        }
                        if (enumerableType.isStruct())
                        {
                            getter = (Func<dataDeSerializer, valueType>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, valueType>), typeof(dataDeSerializer).GetMethod("getArrayNotNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                            return;
                        }
                        getter = (Func<dataDeSerializer, valueType>)Delegate.CreateDelegate(typeof(Func<dataDeSerializer, valueType>), typeof(dataDeSerializer).GetMethod("getArray", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumerableType));
                        return;
                    }
                    if (type == typeof(string))
                    {
                        getter = (Func<dataDeSerializer, valueType>)(Delegate)(Func<dataDeSerializer, string>)dataDeSerializer.GetString;
                        return;
                    }
                    setMemberGetter();
                }
            }
        }
        /// <summary>
        /// 对象反序列化(反射模式)
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        internal sealed class deSerialize<valueType> : dataDeSerialize<valueType, serialize>
        {
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="value">数据对象</param>
            /// <param name="dataStart">数据起始位置</param>
            /// <param name="dataEnd">数据结束位置</param>
            /// <param name="read">读取结束位置</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>是否成功</returns>
            public unsafe static bool Get(ref valueType value, byte* dataStart, byte* dataEnd, out byte* read, out memberMap<valueType> memberMap)
            {
                reflectionDeSerializer<valueType> deSerializer = new reflectionDeSerializer<valueType>(dataStart, dataEnd);
                if (*(int*)deSerializer.Read == fastCSharp.emit.binarySerializer.NullValue)
                {
                    deSerializer.Read += sizeof(int);
                    value = default(valueType);
                    memberMap = default(memberMap<valueType>);
                }
                else
                {
                    if (getter != null)
                    {
                        value = getter(deSerializer);
                        memberMap = deSerializer.MemberMap;
                        //(memberMap = default(memberMap<valueType>)).CopyFrom(deSerializer.MemberMap);
                    }
                    else
                    {
                        if (!isStruct && deSerializer.isReferenceMember) deSerializer.points.Add(-sizeof(int), value);
                        if (deSerializer.VersionMemberMap())
                        {
                            getMember(deSerializer, ref value);
                            memberMap = deSerializer.MemberMap;
                        }
                        else
                        {
                            read = null;
                            memberMap = default(memberMap<valueType>);
                            return false;
                        }
                    }
                }
                if (deSerializer.checkEnd())
                {
                    read = deSerializer.Read;
                    return true;
                }
                read = null;
                return false;
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="parentDeSerializer">对象反序列化器</param>
            /// <param name="point">历史对象指针</param>
            /// <returns>数据对象</returns>
            public unsafe static valueType GetVersionMemerMap(dataDeSerializer parentDeSerializer, int point)
            {
                reflectionDeSerializer<valueType> deSerializer = new reflectionDeSerializer<valueType>(parentDeSerializer, serializeAttribute.Version);
                if (isUnknownValue || deSerializer.VersionMemberMap())
                {
                    valueType value = get(deSerializer, point);
                    parentDeSerializer.SetReadPoint(deSerializer);
                    return value;
                }
                return default(valueType);
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            /// <param name="point">历史对象指针</param>
            /// <returns>数据对象</returns>
            private static valueType get(reflectionDeSerializer<valueType> deSerializer, int point)
            {
                if (getter != null) return getter(deSerializer);
                if (!isStruct)
                {
                    valueType value = fastCSharp.emit.constructor<valueType>.New();
                    if (deSerializer.isReferenceMember) deSerializer.points.Add(-point, value);
                    getMember(deSerializer, ref value);
                    return value;
                }
                if (isUnknownValue) return getUnknownMember(deSerializer);
                else
                {
                    valueType value = default(valueType);
                    getMember(deSerializer, ref value);
                    return value;
                }
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            /// <param name="value">数据对象</param>
            private unsafe static void getMember(reflectionDeSerializer<valueType> deSerializer, ref valueType value)
            {
                byte* isValueFixed = stackalloc byte[memberMapSize];
                fixedMap isValueMap = new fixedMap(isValueFixed, memberMapSize);
                object[] values = new object[memberCount];
                memberMap<valueType> memberMap = andMemberMap.Copy();
                if (!deSerializer.MemberMap.IsDefault) memberMap.And(deSerializer.MemberMap);
                fixedMap nullMap = new fixedMap(deSerializer.Read);
                deSerializer.Read += memberMap.SerializeSize;
                object reference;
                for (int* memberIndexStart = memberSort.Int, memberIndexEnd = memberIndexStart + memberGroup.Count; memberIndexStart != memberIndexEnd; ++memberIndexStart)
                {
                    int memberIndex = *memberIndexStart;
                    if (memberMap.IsMember(memberIndex))
                    {
                        if (nullMap.Get(memberIndex)) values[memberIndex] = defaultValues[memberIndex];
                        else if (isMemberSerializeMap.Get(memberIndex))
                        {
                            Func<object, object> converter = converters[memberIndex];
                            reference = memberGetters[memberIndex](deSerializer);
                            values[memberIndex] = converter == null ? reference : converter(reference);
                        }
                        isValueMap.Set(memberIndex);
                    }
                }
                int length = (int)(deSerializer.Read - nullMap.Map);
                if ((length & 3) != 0) deSerializer.Read += -length & 3;
                bool isReferenceMember = deSerializer.isReferenceMember;
                for (int* memberIndexStart = memberSort.Int, memberIndexEnd = memberIndexStart + memberGroup.Count; memberIndexStart != memberIndexEnd; ++memberIndexStart)
                {
                    int memberIndex = *memberIndexStart;
                    if (!isMemberSerializeMap.Get(memberIndex) && memberMap.IsMember(memberIndex) && !nullMap.Get(memberIndex))
                    {
                        bool isNull = isNullMap.Get(memberIndex);
                        if (isNull)
                        {
                            if (isReferenceMember)
                            {
                                length = *(int*)deSerializer.Read;
                                if (deSerializer.points.TryGetValue(length, out reference)) deSerializer.Read += sizeof(int);
                                else reference = memberGetters[memberIndex](deSerializer);
                            }
                            else reference = memberGetters[memberIndex](deSerializer);
                        }
                        else reference = memberGetters[memberIndex](deSerializer);
                        Func<object, object> converter = converters[memberIndex];
                        values[memberIndex] = converter == null ? reference : converter(reference);
                    }
                }
                memberMap.PushPool();
                if (isStruct) value = memberGroup.SetMemberValue(value, values, isValueMap);
                else memberGroup.SetMember(value, values, isValueMap);
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            /// <returns>目标对象</returns>
            private unsafe static valueType getUnknownMember(reflectionDeSerializer<valueType> deSerializer)
            {
                byte* isNull = deSerializer.Read;
                deSerializer.Read += sizeof(int);
                object memberValue;
                if (*(int*)isNull == 0)
                {
                    memberValue = unknownMemberGetter(deSerializer);
                    if (unknownConverter != null) memberValue = unknownConverter(memberValue);
                    if (isUnknownMemberSerialize)
                    {
                        int length = (int)(deSerializer.Read - isNull);
                        if ((length & 3) != 0) deSerializer.Read += -length & 3;
                    }
                }
                else memberValue = unknownDefaultValue;
                object value = default(valueType);
                unknownField.ValueSetter(value, memberValue);
                return (valueType)value;
            }
            /// <summary>
            /// 默认成员值集合
            /// </summary>
            private static object unknownDefaultValue;
            /// <summary>
            /// 成员对象反序列化 集合
            /// </summary>
            private static Func<dataDeSerializer, object> unknownMemberGetter;
            /// <summary>
            /// 成员反序列化类型转换器集合
            /// </summary>
            private static Func<object, object> unknownConverter;
            /// <summary>
            /// 初始化未知类型成员序列化器
            /// </summary>
            private static void setUnknownMemberGetter()
            {
                unknownDefaultValue = constructor.GetNull(unknownField.MemberType);
                keyValue<Func<dataDeSerializer, object>, Func<object, object>> values = getMemberGetter(unknownField);
                unknownMemberGetter = values.Key;
                unknownConverter = values.Value;
            }
            static deSerialize()
            {
                if (isUnknownValue) setUnknownMemberGetter();
            }
        }
        /// <summary>
        /// 序列化版本号(不能小于0)
        /// </summary>
        public int Version;
        /// <summary>
        /// 是否支持序列化到流
        /// </summary>
        public bool IsStreamSerialize;
        /// <summary>
        /// 是否用于测试用例
        /// </summary>
        public bool IsTestCase;
        /// <summary>
        /// 是否只需要基本序列化代码
        /// </summary>
        public bool IsBaseSerialize;
#if MONO
#else
        /// <summary>
        /// 序列化代码生成
        /// </summary
        public sealed partial class cSharp
        {
            /// <summary>
            /// 未知的序列化类型
            /// </summary>
            public readonly static HashSet<Type> UnknownSerializeTypes = hashSet.CreateOnly<Type>();
            /// <summary>
            /// 未知序列化类型
            /// </summary>
            /// <param name="type">类型</param>
            internal static void UnknownSerialize(Type type)
            {
                if (type != null && type.FullName != null && type.customAttribute<serialize>() == null)
                {
                    if (type != typeof(object)) UnknownSerializeTypes.Add(type);
                }
            }
        }
#endif
    }
}
namespace fastCSharp.code
{
    /// <summary>
    /// 成员类型
    /// </summary>
    public unsafe partial class memberType
    {
        /// <summary>
        /// 序列化类型
        /// </summary>
        private memberType serializeType;
        /// <summary>
        /// 序列化类型
        /// </summary>
        public memberType SerializeType
        {
            get
            {
                if (serializeType == null)
                {
                    cSharp.serialize serialize = Type.customAttribute<cSharp.serialize>();
                    if (serialize != null && serialize.SerializeType != null)
                    {
                        Type type = serialize.SerializeType;
                        if (Type.IsGenericType)
                        {
                            if (type.IsArray)
                            {
                                if (type.GetElementType() == typeof(cSharp.serialize.ISerializeGeneric)) type = Type.GetGenericArguments()[0].MakeArrayType();
                            }
                            else
                            {
                                Type[] types = type.GetGenericArguments();
                                int index = 0;
                                foreach (Type nextType in Type.GetGenericArguments())
                                {
                                    while (types[index] != typeof(cSharp.serialize.ISerializeGeneric)) ++index;
                                    types[index] = nextType;
                                }
                                type = type.GetGenericTypeDefinition().MakeGenericType(types);
                            }
                        }
                        serializeType = type;
                    }
                    else if (Type.IsEnum) serializeType = System.Enum.GetUnderlyingType(Type);
                    else serializeType = Type;
                }
                return serializeType;
            }
        }
        /// <summary>
        /// 序列化类型是否一致
        /// </summary>
        public bool IsSerializeType
        {
            get
            {
                return SerializeType.Type == Type;
            }
        }
        /// <summary>
        /// 序列化类型是否不一致,或者枚举类型
        /// </summary>
        public bool NotSerializeTypeEnum
        {
            get
            {
                return !IsSerializeType || Type.IsEnum;
            }
        }
        /// <summary>
        /// 序列化基本字节数
        /// </summary>
        private int serializeSize;
        /// <summary>
        /// 序列化基本字节数
        /// </summary>
        public int SerializeSize
        {
            get
            {
                if (serializeSize == 0
                    && !memorySerializeSizes.TryGetValue(SerializeType.Type.nullableType() ?? SerializeType, out serializeSize))
                {
                    serializeSize = sizeof(int);
                }
                return serializeSize;
            }
        }
        /// <summary>
        /// 是否值类型序列化
        /// </summary>
        private bool? isMemberSerialize;
        /// <summary>
        /// 是否值类型序列化
        /// </summary>
        public bool IsMemberSerialize
        {
            get
            {
                if (isMemberSerialize == null) isMemberSerialize = memorySerializeSizes.ContainsKey(SerializeType.Type.nullableType() ?? SerializeType);
                return (bool)isMemberSerialize;
            }
        }
        /// <summary>
        /// 是否内存复制序列化
        /// </summary>
        public bool IsSerializeBlockCopy
        {
            get
            {
                return blockCopySerializeTypes.Contains(SerializeType.Type.nullableType() ?? SerializeType);
            }
        }
        /// <summary>
        /// 是否继承接口fastCSharp.code.cSharp.serialize.ISerialize
        /// </summary>
        private bool? isISerialize;
        /// <summary>
        /// 是否继承接口fastCSharp.code.cSharp.serialize.ISerialize
        /// </summary>
        public bool IsISerialize
        {
            get
            {
                //SerializeType.Type.IsGenericParameter
                if (isISerialize == null) isISerialize = typeof(fastCSharp.code.cSharp.serialize.ISerialize).IsAssignableFrom(SerializeType.Type);
                return (bool)isISerialize;
            }
        }
        /// <summary>
        /// 未知序列化类型
        /// </summary>
        private bool? serializeUnknown;
        /// <summary>
        /// 未知序列化类型
        /// </summary>
        private bool SerializeUnknown
        {
            get
            {
                if (serializeUnknown == null)
                {
                    serializeUnknown = !(IsMemberSerialize || SerializeType.IsString || SerializeType.Type.IsArray);
                }
                return (bool)serializeUnknown;
            }
        }
#if MONO
#else
        /// <summary>
        /// 未知序列化类型
        /// </summary>
        private bool? isSerializeUnknown;
        /// <summary>
        /// 未知序列化类型
        /// </summary>
        public bool IsSerializeUnknown
        {
            get
            {
                if (isSerializeUnknown == null)
                {
                    if (SerializeUnknown)
                    {
                        if (SerializeType.IsISerialize) isSerializeUnknown = false;
                        else
                        {
                            fastCSharp.code.cSharp.serialize.cSharp.UnknownSerialize(SerializeType.Type.IsArray ? SerializeType.EnumerableArgumentType.SerializeType.Type : SerializeType.Type);
                            isSerializeUnknown = true;
                        }
                    }
                    else isSerializeUnknown = false;
                }
                return (bool)isSerializeUnknown;
            }
        }
#endif
        /// <summary>
        /// 是否序列化数组
        /// </summary>
        public bool IsSerializeArray
        {
            get { return Type.IsArray && !IsByteArray; }
        }
        /// <summary>
        /// 是否序列化未知类型数组
        /// </summary>
        public bool IsSerializeArrayUnknown
        {
            get
            {
                return !IsMemberSerialize && !IsString && !IsISerialize;
            }
        }
        /// <summary>
        /// 序列化基本字节数
        /// </summary>
        private static readonly Dictionary<Type, int> memorySerializeSizes;
        /// <summary>
        /// 序列化基本字节数
        /// </summary>
        private static readonly HashSet<Type> blockCopySerializeTypes = new HashSet<Type>(new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float), typeof(double) });
        static memberType()
        {
            memorySerializeSizes = dictionary.CreateOnly<Type, int>();
            memorySerializeSizes.Add(typeof(bool), sizeof(bool));
            memorySerializeSizes.Add(typeof(byte), sizeof(byte));
            memorySerializeSizes.Add(typeof(sbyte), sizeof(sbyte));
            memorySerializeSizes.Add(typeof(short), sizeof(short));
            memorySerializeSizes.Add(typeof(ushort), sizeof(ushort));
            memorySerializeSizes.Add(typeof(int), sizeof(int));
            memorySerializeSizes.Add(typeof(uint), sizeof(uint));
            memorySerializeSizes.Add(typeof(long), sizeof(long));
            memorySerializeSizes.Add(typeof(ulong), sizeof(ulong));
            memorySerializeSizes.Add(typeof(char), sizeof(char));
            memorySerializeSizes.Add(typeof(DateTime), sizeof(long));
            memorySerializeSizes.Add(typeof(float), sizeof(float));
            memorySerializeSizes.Add(typeof(double), sizeof(double));
            memorySerializeSizes.Add(typeof(decimal), sizeof(decimal));
            memorySerializeSizes.Add(typeof(Guid), sizeof(Guid));
        }
    }
}
