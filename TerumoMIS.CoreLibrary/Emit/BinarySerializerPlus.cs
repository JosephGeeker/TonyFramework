//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: BinarySerializerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  BinarySerializerPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 11:11:41
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using TerumoMIS.CoreLibrary.Code;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 二进制数据序列化
    /// </summary>
    public unsafe abstract class BinarySerializerPlus:IDisposable
    {
        /// <summary>
        /// 空对象
        /// </summary>
        public const int NullValue = int.MinValue;
        /// <summary>
        /// 数组位图
        /// </summary>
        internal struct ArrayMapStruct
        {
            /// <summary>
            /// 当前位
            /// </summary>
            public uint Bit;
            /// <summary>
            /// 当前位图
            /// </summary>
            public uint Map;
            /// <summary>
            /// 当前写入位置
            /// </summary>
            public byte* Write;
            /// <summary>
            /// 数组位图
            /// </summary>
            /// <param name="stream">序列化数据流</param>
            /// <param name="arrayLength">数组长度</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ArrayMapStruct(UnmanagedStreamPlus stream, int arrayLength)
            {
                int length = ((arrayLength + (31 + 32)) >> 5) << 2;
                Bit = 1U << 31;
                stream.PrepLength(length);
                Write = stream.CurrentData;
                Map = 0;
                *(int*)Write = arrayLength;
                stream.Unsafer.AddLength(length);
            }
            /// <summary>
            /// 数组位图
            /// </summary>
            /// <param name="stream">序列化数据流</param>
            /// <param name="arrayLength">数组长度</param>
            /// <param name="prepLength">附加长度</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ArrayMapStruct(UnmanagedStreamPlus stream, int arrayLength, int prepLength)
            {
                int length = ((arrayLength + (31 + 32)) >> 5) << 2;
                Bit = 1U << 31;
                stream.PrepLength(length + prepLength);
                Write = stream.CurrentData;
                Map = 0;
                *(int*)Write = arrayLength;
                stream.Unsafer.AddLength(length);
            }
            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="value">是否写位图</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Next(bool value)
            {
                if (value) Map |= Bit;
                if (Bit == 1)
                {
                    *(uint*)(Write += sizeof(int)) = Map;
                    Bit = 1U << 31;
                    Map = 0;
                }
                else Bit >>= 1;
            }
            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="value">是否写位图</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void NextNot(bool value)
            {
                if (!value) Map |= Bit;
                if (Bit == 1)
                {
                    *(uint*)(Write += sizeof(int)) = Map;
                    Bit = 1U << 31;
                    Map = 0;
                }
                else Bit >>= 1;
            }
            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="value">是否写位图</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Next(bool? value)
            {
                if (value.HasValue)
                {
                    Map |= Bit;
                    if ((bool)value) Map |= (Bit >> 1);
                }
                if (Bit == 2)
                {
                    *(uint*)(Write += sizeof(int)) = Map;
                    Bit = 1U << 31;
                    Map = 0;
                }
                else Bit >>= 2;
            }
            /// <summary>
            /// 位图写入结束
            /// </summary>
            /// <param name="stream">序列化数据流</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void End(UnmanagedStreamPlus stream)
            {
                if (Bit != 1U << 31) *(uint*)(Write + sizeof(int)) = Map;
                stream.PrepLength();
            }
        }
        /// <summary>
        /// 配置参数
        /// </summary>
        public class ConfigPlus
        {
            /// <summary>
            /// 序列化头部数据
            /// </summary>
            internal const uint HeaderMapValue = 0x51031000U;
            /// <summary>
            /// 序列化头部数据
            /// </summary>
            internal const uint HeaderMapAndValue = 0xffffff00U;
            /// <summary>
            /// 是否序列化成员位图
            /// </summary>
            internal const int MemberMapValue = 1;
            /// <summary>
            /// 成员位图
            /// </summary>
            public MemberMapPlus MemberMap;
            /// <summary>
            /// 成员位图类型不匹配是否输出错误信息
            /// </summary>
            public bool IsMemberMapErrorLog = true;
            /// <summary>
            /// 序列化头部数据
            /// </summary>
            internal virtual int HeaderValue
            {
                get
                {
                    var value = (int)HeaderMapValue;
                    if (MemberMap != null) value += MemberMapValue;
                    return value;
                }
            }
        }
        /// <summary>
        /// 字段信息
        /// </summary>
        internal class FieldInfoPlus
        {
            /// <summary>
            /// 字段信息
            /// </summary>
            public FieldInfo Field;
            /// <summary>
            /// 成员索引
            /// </summary>
            public int MemberIndex;
            /// <summary>
            /// 固定分组排序字节数
            /// </summary>
            internal byte FixedSize;
            /// <summary>
            /// 字段信息
            /// </summary>
            /// <param name="field"></param>
            internal FieldInfoPlus(fieldIndex field)
            {
                Field = field.Member;
                MemberIndex = field.MemberIndex;
                if (Field.FieldType.IsEnum) FixedSizes.TryGetValue(Field.FieldType.GetEnumUnderlyingType(), out FixedSize);
                else FixedSizes.TryGetValue(Field.FieldType, out FixedSize);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <returns></returns>
            internal static int FixedSizeSort(FieldInfoPlus left, FieldInfoPlus right)
            {
                return (int)(right.FixedSize & (0U - right.FixedSize)) - (int)(left.FixedSize & (0U - left.FixedSize));
            }
            /// <summary>
            /// 固定类型字节数
            /// </summary>
            private static readonly Dictionary<Type, byte> FixedSizes;
            static FieldInfoPlus()
            {
                FixedSizes = DictionaryPlus.CreateOnly<Type, byte>();
                FixedSizes.Add(typeof(bool), sizeof(bool));
                FixedSizes.Add(typeof(byte), sizeof(byte));
                FixedSizes.Add(typeof(sbyte), sizeof(sbyte));
                FixedSizes.Add(typeof(short), sizeof(short));
                FixedSizes.Add(typeof(ushort), sizeof(ushort));
                FixedSizes.Add(typeof(int), sizeof(int));
                FixedSizes.Add(typeof(uint), sizeof(uint));
                FixedSizes.Add(typeof(long), sizeof(long));
                FixedSizes.Add(typeof(ulong), sizeof(ulong));
                FixedSizes.Add(typeof(char), sizeof(char));
                FixedSizes.Add(typeof(DateTime), sizeof(long));
                FixedSizes.Add(typeof(float), sizeof(float));
                FixedSizes.Add(typeof(double), sizeof(double));
                FixedSizes.Add(typeof(decimal), sizeof(decimal));
                FixedSizes.Add(typeof(Guid), (byte)sizeof(Guid));
                FixedSizes.Add(typeof(bool?), sizeof(byte));
                FixedSizes.Add(typeof(byte?), sizeof(ushort));
                FixedSizes.Add(typeof(sbyte?), sizeof(ushort));
                FixedSizes.Add(typeof(short?), sizeof(uint));
                FixedSizes.Add(typeof(ushort?), sizeof(uint));
                FixedSizes.Add(typeof(int?), sizeof(int) + sizeof(int));
                FixedSizes.Add(typeof(uint?), sizeof(uint) + sizeof(int));
                FixedSizes.Add(typeof(long?), sizeof(long) + sizeof(int));
                FixedSizes.Add(typeof(ulong?), sizeof(ulong) + sizeof(int));
                FixedSizes.Add(typeof(char?), sizeof(uint));
                FixedSizes.Add(typeof(DateTime?), sizeof(long) + sizeof(int));
                FixedSizes.Add(typeof(float?), sizeof(float) + sizeof(int));
                FixedSizes.Add(typeof(double?), sizeof(double) + sizeof(int));
                FixedSizes.Add(typeof(decimal?), sizeof(decimal) + sizeof(int));
                FixedSizes.Add(typeof(Guid?), (byte)(sizeof(Guid) + sizeof(int)));
            }
        }
        /// <summary>
        /// 字段集合信息
        /// </summary>
        /// <typeparam name="TFieldType"></typeparam>
        internal struct FieldsStruct<TFieldType> where TFieldType : FieldInfoPlus
        {
            /// <summary>
            /// 固定序列化字段
            /// </summary>
            public SubArrayStruct<TFieldType> FixedFields;
            /// <summary>
            /// 非固定序列化字段
            /// </summary>
            public SubArrayStruct<TFieldType> Fields;
            /// <summary>
            /// JSON混合序列化字段
            /// </summary>
            public SubArrayStruct<fieldIndex> JsonFields;
            /// <summary>
            /// 固定序列化字段字节数
            /// </summary>
            public int FixedSize;
        }
        /// <summary>
        /// 序列化输出缓冲区
        /// </summary>
        public readonly UnmanagedStreamPlus Stream = new UnmanagedStreamPlus((byte*)PubPlus.PuzzleValue, 1);
        /// <summary>
        /// JSON序列化输出缓冲区
        /// </summary>
        private charStream _jsonStream;
        /// <summary>
        /// JSON序列化配置参数
        /// </summary>
        private JsonSerializerPlus.config _jsonConfig;
        /// <summary>
        /// JSON序列化成员位图
        /// </summary>
        private MemberMapPlus _jsonMemberMap;
        /// <summary>
        /// 序列化输出缓冲区字段信息
        /// </summary>
        internal static readonly FieldInfo StreamField = typeof(BinarySerializerPlus).GetField("Stream", BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// 成员位图
        /// </summary>
        protected MemberMapPlus MemberMap;
        /// <summary>
        /// 成员位图
        /// </summary>
        protected MemberMapPlus CurrentMemberMap;
        /// </summary>
        /// 数据流起始位置
        /// </summary>
        protected int StreamStartIndex;
        /// <summary>
        /// 序列化配置参数
        /// </summary>
        protected ConfigPlus BinarySerializerConfig;
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            CoreLibrary.PubPlus.Dispose(ref _jsonMemberMap);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Free()
        {
            MemberMap = CurrentMemberMap = null;
        }
        /// <summary>
        /// 获取JSON序列化输出缓冲区
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal charStream ResetJsonStream(void* data, int size)
        {
            if (_jsonStream == null) return _jsonStream = new charStream((char*)data, size >> 1);
            _jsonStream.Reset((byte*)data, size);
            return _jsonStream;
        }
        /// <summary>
        /// 获取JSON序列化配置参数
        /// </summary>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        protected JsonSerializerPlus.config GetJsonConfig(MemberMapPlus memberMap)
        {
            if (_jsonConfig == null) _jsonConfig = new JsonSerializerPlus.config { CheckLoopDepth = fastCSharp.config.appSetting.JsonDepth };
            _jsonConfig.MemberMap = memberMap;
            return _jsonConfig;
        }
        /// <summary>
        /// 获取JSON成员位图
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="memberMap"></param>
        /// <param name="memberIndexs"></param>
        /// <returns></returns>
        protected MemberMapPlus GetJsonMemberMap<TValueType>(MemberMapPlus memberMap, int[] memberIndexs)
        {
            var count = 0;
            foreach (var memberIndex in memberIndexs)
            {
                if (memberMap.IsMember(memberIndex))
                {
                    if (count == 0 && _jsonMemberMap == null) _jsonMemberMap = memberMap<TValueType>.Empty();
                    _jsonMemberMap.SetMember(memberIndex);
                    ++count;
                }
            }
            return count == 0 ? null : _jsonMemberMap;
        }
        /// <summary>
        /// 序列化成员位图
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemberMapPlus SerializeMemberMap<TValueType>()
        {
            if (MemberMap != null)
            {
                CurrentMemberMap = MemberMap;
                MemberMap = null;
                if (CurrentMemberMap.Type == MemberMapPlus<TValueType>.Type)
                {
                    CurrentMemberMap.FieldSerialize(Stream);
                    return CurrentMemberMap;
                }
                if (BinarySerializerConfig.IsMemberMapErrorLog) LogPlus.Error.Add("二进制序列化成员位图类型匹配失败", true, true);
            }
            return null;
        }
        /// <summary>
        /// 逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(bool value)
        {
            Stream.Write(value ? 1 : 0);
        }
        /// <summary>
        /// 逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(bool value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数据,不能为null</param>
        public static void Serialize(UnmanagedStreamPlus stream, bool[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length);
            foreach (var value in array) arrayMap.Next(value);
            arrayMap.End(stream);
        }
        /// <summary>
        /// 逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(bool? value)
        {
            Stream.Write(value != null && (bool)value ? 1 : 0);
        }
        /// <summary>
        /// 逻辑值序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(bool? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((bool)value ? (byte)2 : (byte)1);
            else Stream.Unsafer.Write((byte)0);
        }

        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数据,不能为null</param>
        internal static void Serialize(UnmanagedStreamPlus stream, bool?[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length << 1);
            foreach (var value in array) arrayMap.Next(value);
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(byte value)
        {
            Stream.Write((uint)value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(byte value)
        {
            Stream.Unsafer.Write(value);
        }

        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">数据类型</param>
        /// <param name="data">数据,不能为null</param>
        /// <param name="arrayLength">数据数量</param>
        /// <param name="size">单个数据字节数</param>
        public static void Serialize(UnmanagedStreamPlus stream, void* data, int arrayLength, int size)
        {
            int dataSize = arrayLength * size, length = (dataSize + (3 + sizeof(int))) & (int.MaxValue - 3);
            stream.PrepLength(length);
            var write = stream.CurrentData;
            *(int*)write = arrayLength;
            MemoryUnsafe.Copy(data, write + sizeof(int), dataSize);
            stream.Unsafer.AddLength(length);
            stream.PrepLength();
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, byte[] data)
        {
            fixed (byte* dataFixed = data) Serialize(stream, dataFixed, data.Length, 1);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(SubArrayStruct<byte> value)
        {
            if (value.Count == 0) Stream.Write(0);
            else Serialize(Stream, value);
        }
        /// <summary>
        /// 预增数据流长度并序列化数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        internal static void Serialize(UnmanagedStreamPlus stream, SubArrayStruct<byte> data)
        {
            var length = sizeof(int) + ((data.Count + 3) & (int.MaxValue - 3));
            stream.PrepLength(length);
            var write = stream.CurrentData;
            *(int*)write = data.Count;
            fixed (byte* dataFixed = data.Array) MemoryUnsafe.Copy(dataFixed + data.StartIndex, write + sizeof(int), data.Count);
            stream.Unsafer.AddLength(length);
            stream.PrepLength();
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(byte? value)
        {
            if (value != null) Stream.Write((uint)(byte)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(byte? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((ushort)(byte)value);
            else Stream.Unsafer.Write(short.MinValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, byte?[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length, array.Length);
            var write = stream.CurrentData;
            foreach (byte? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (byte)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)(write - stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(sbyte value)
        {
            Stream.Write((int)value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(sbyte value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, sbyte[] data)
        {
            fixed (sbyte* dataFixed = data) Serialize(stream, dataFixed, data.Length, 1);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(sbyte? value)
        {
            if (value != null) Stream.Write((int)(sbyte)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(sbyte? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((ushort)(byte)(sbyte)value);
            else Stream.Unsafer.Write(short.MinValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, sbyte?[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length, array.Length);
            var write = (sbyte*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (sbyte)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)(write - (sbyte*)stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(short value)
        {
            Stream.Write((int)value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(short value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, short[] data)
        {
            fixed (short* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(short));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(short? value)
        {
            if (value != null) Stream.Write((int)(short)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(short? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(ushort)(short)value);
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, short?[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(short));
            var write = (short*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (short)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)((byte*)write - stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(ushort value)
        {
            Stream.Write((uint)value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(ushort value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, ushort[] data)
        {
            fixed (ushort* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(ushort));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(ushort? value)
        {
            if (value != null) Stream.Write((uint)(ushort)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(ushort? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(ushort)value);
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, ushort?[] array)
        {
            ArrayMapStruct arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(ushort));
            ushort* write = (ushort*)stream.CurrentData;
            foreach (ushort? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (ushort)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)((byte*)write - stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(int value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(int value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, int[] data)
        {
            fixed (int* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(int));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(int? value)
        {
            if (value != null) Stream.Write((int)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(int? value)
        {
            if (value.HasValue)
            {
                byte* data = Stream.CurrentData;
                *(int*)data = 0;
                *(int*)(data + sizeof(int)) = (int)value;
                Stream.Unsafer.AddLength(sizeof(int) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, int?[] array)
        {
            ArrayMapStruct arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(int));
            int* write = (int*)stream.CurrentData;
            foreach (int? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (int)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(uint value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(uint value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, uint[] data)
        {
            fixed (uint* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(uint));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(uint? value)
        {
            if (value != null) Stream.Write((uint)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(uint? value)
        {
            if (value.HasValue)
            {
                byte* data = Stream.CurrentData;
                *(int*)data = 0;
                *(uint*)(data + sizeof(int)) = (uint)value;
                Stream.Unsafer.AddLength(sizeof(uint) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, uint?[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(uint));
            var write = (uint*)stream.CurrentData;
            foreach (uint? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (uint)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(long value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(long value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, long[] data)
        {
            fixed (long* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(long));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(long? value)
        {
            if (value != null) Stream.Write((long)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(long? value)
        {
            if (value.HasValue)
            {
                byte* data = Stream.CurrentData;
                *(int*)data = 0;
                *(long*)(data + sizeof(int)) = (long)value;
                Stream.Unsafer.AddLength(sizeof(long) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, long?[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(long));
            var write = (long*)stream.CurrentData;
            foreach (long? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (long)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(ulong value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(ulong value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, ulong[] data)
        {
            fixed (ulong* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(ulong));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(ulong? value)
        {
            if (value != null) Stream.Write((ulong)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(ulong? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(ulong*)(data + sizeof(int)) = (ulong)value;
                Stream.Unsafer.AddLength(sizeof(ulong) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, ulong?[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(ulong));
            var write = (ulong*)stream.CurrentData;
            foreach (var value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (ulong)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(float value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(float value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, float[] data)
        {
            fixed (float* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(float));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(float? value)
        {
            if (value != null) Stream.Write((float)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(float? value)
        {
            if (value.HasValue)
            {
                var data = Stream.CurrentData;
                *(int*)data = 0;
                *(float*)(data + sizeof(int)) = (float)value;
                Stream.Unsafer.AddLength(sizeof(float) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, float?[] array)
        {
            var arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(float));
            var write = (float*)stream.CurrentData;
            foreach (float? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (float)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(double value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(double value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, double[] data)
        {
            fixed (double* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(double));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(double? value)
        {
            if (value != null) Stream.Write((double)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(double? value)
        {
            if (value.HasValue)
            {
                byte* data = Stream.CurrentData;
                *(int*)data = 0;
                *(double*)(data + sizeof(int)) = (double)value;
                Stream.Unsafer.AddLength(sizeof(double) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, double?[] array)
        {
            ArrayMapStruct arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(double));
            double* write = (double*)stream.CurrentData;
            foreach (double? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (double)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(decimal value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(decimal value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(UnmanagedStreamPlus stream, decimal[] data)
        {
            fixed (decimal* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(decimal));
        }
        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(decimal? value)
        {
            if (value != null) Stream.Write((decimal)value);
        }

        /// <summary>
        /// 数值序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(decimal? value)
        {
            if (value.HasValue)
            {
                byte* data = Stream.CurrentData;
                *(int*)data = 0;
                *(decimal*)(data + sizeof(int)) = (decimal)value;
                Stream.Unsafer.AddLength(sizeof(decimal) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, decimal?[] array)
        {
            ArrayMapStruct arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(decimal));
            decimal* write = (decimal*)stream.CurrentData;
            foreach (decimal? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (decimal)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(char value)
        {
            Stream.Write((uint)value);
        }
        /// <summary>
        /// 字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(char value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, char[] data)
        {
            fixed (char* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(char));
        }
        /// <summary>
        /// 字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(char? value)
        {
            if (value != null) Stream.Write((uint)(char)value);
        }

        /// <summary>
        /// 字符序列化
        /// </summary>
        /// <param name="value">字符</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(char? value)
        {
            if (value.HasValue) Stream.Unsafer.Write((uint)(char)value);
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, char?[] array)
        {
            ArrayMapStruct arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(char));
            char* write = (char*)stream.CurrentData;
            foreach (char? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (char)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength(((int)((byte*)write - stream.CurrentData) + 3) & (int.MaxValue - 3));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(DateTime value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// 时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(DateTime value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(UnmanagedStreamPlus stream, DateTime[] data)
        {
            fixed (DateTime* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(DateTime));
        }
        /// <summary>
        /// 时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(DateTime? value)
        {
            if (value != null) Stream.Write((DateTime)value);
        }

        /// <summary>
        /// 时间序列化
        /// </summary>
        /// <param name="value">时间</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(DateTime? value)
        {
            if (value.HasValue)
            {
                byte* data = Stream.CurrentData;
                *(int*)data = 0;
                *(DateTime*)(data + sizeof(int)) = (DateTime)value;
                Stream.Unsafer.AddLength(sizeof(DateTime) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, DateTime?[] array)
        {
            ArrayMapStruct arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(DateTime));
            DateTime* write = (DateTime*)stream.CurrentData;
            foreach (DateTime? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (DateTime)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(Guid value)
        {
            Stream.Write(value);
        }
        /// <summary>
        /// Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [IndexSerializerPlus.MemberMapSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(Guid value)
        {
            Stream.Unsafer.Write(value);
        }
        /// <summary>
        /// 预增数据流长度并写入长度与数据(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="data">数据,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(UnmanagedStreamPlus stream, Guid[] data)
        {
            fixed (Guid* dataFixed = data) Serialize(stream, dataFixed, data.Length, sizeof(Guid));
        }
        /// <summary>
        /// Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(Guid? value)
        {
            if (value != null) Stream.Write((Guid)value);
        }

        /// <summary>
        /// Guid序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [IndexSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberSerialize(Guid? value)
        {
            if (value.HasValue)
            {
                byte* data = Stream.CurrentData;
                *(int*)data = 0;
                *(Guid*)(data + sizeof(int)) = (Guid)value;
                Stream.Unsafer.AddLength(sizeof(Guid) + sizeof(int));
            }
            else Stream.Unsafer.Write(NullValue);
        }
        /// <summary>
        /// 序列化可空数组
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="array">数组数据</param>
        internal static void Serialize(UnmanagedStreamPlus stream, Guid?[] array)
        {
            ArrayMapStruct arrayMap = new ArrayMapStruct(stream, array.Length, array.Length * sizeof(Guid));
            Guid* write = (Guid*)stream.CurrentData;
            foreach (Guid? value in array)
            {
                if (value.HasValue)
                {
                    arrayMap.Next(true);
                    *write++ = (Guid)value;
                }
                else arrayMap.Next(false);
            }
            stream.Unsafer.AddLength((int)((byte*)write - stream.CurrentData));
            arrayMap.End(stream);
        }
        /// <summary>
        /// 字符串序列化
        /// </summary>
        /// <param name="valueFixed"></param>
        /// <param name="stream"></param>
        /// <param name="stringLength"></param>
        private static void Serialize(char* valueFixed, UnmanagedStreamPlus stream, int stringLength)
        {
            char* start = valueFixed, end = valueFixed + stringLength;
            do
            {
                if ((*start & 0xff00) != 0)
                {
                    var length = ((stringLength <<= 1) + (3 + sizeof(int))) & (int.MaxValue - 3);
                    stream.PrepLength(length);
                    start = (char*)stream.CurrentData;
                    MemoryUnsafe.Copy(valueFixed, (byte*)start + sizeof(int), *(int*)start = stringLength);
                    stream.Unsafer.AddLength(length);
                    stream.PrepLength();
                    return;
                }
            }
            while (++start != end);
            {
                int length = (stringLength + (3 + sizeof(int))) & (int.MaxValue - 3);
                stream.PrepLength(length);
                byte* write = stream.CurrentData;
                *(int*)write = (stringLength << 1) + 1;
                write += sizeof(int);
                do
                {
                    *write++ = (byte)*valueFixed++;
                }
                while (valueFixed != end);
                stream.Unsafer.AddLength(length);
                stream.PrepLength();
            }
        }
        /// <summary>
        /// 预增数据流长度并序列化字符串(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="value">字符串,不能为null,长度不能为0</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, string value)
        {
            //if (value.Length != 0)
            //{
            fixed (char* valueFixed = value) Serialize(valueFixed, stream, value.Length);
            //}
            //else stream.Write(0);
        }
        /// <summary>
        /// 字符串序列化
        /// </summary>
        /// <param name="value">字符串</param>
        [IndexSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.SerializeMethodPlus]
        [DataSerializerPlus.MemberSerializeMethodPlus]
        [DataSerializerPlus.MemberMapSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(SubStringStruct value)
        {
            Serialize(Stream, value);
        }
        /// <summary>
        /// 预增数据流长度并序列化字符串(4字节对齐)
        /// </summary>
        /// <param name="stream">序列化数据流</param>
        /// <param name="value">字符串,不能为null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Serialize(UnmanagedStreamPlus stream, SubStringStruct value)
        {
            if (value.Length == 0) stream.Write(0);
            else
            {
                fixed (char* valueFixed = value.Value) Serialize(valueFixed + value.StartIndex, stream, value.Length);
            }
        }

        /// <summary>
        /// 枚举值序列化
        /// </summary>
        /// <param name="value">值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumByteMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(PubPlus.EnumCastEnum<TValueType, byte>.ToInt(value));
        }

        /// <summary>
        /// 枚举值序列化
        /// </summary>
        /// <param name="value">值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumSByteMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(PubPlus.EnumCastEnum<TValueType, sbyte>.ToInt(value));
        }
        /// <summary>
        /// 枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumShortMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(PubPlus.EnumCastEnum<TValueType, short>.ToInt(value));
        }

        /// <summary>
        /// 枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumUShortMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(PubPlus.EnumCastEnum<TValueType, ushort>.ToInt(value));
        }

        /// <summary>
        /// 枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumIntMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(PubPlus.EnumCastEnum<TValueType, int>.ToInt(value));
        }

        /// <summary>
        /// 枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumUIntMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(PubPlus.EnumCastEnum<TValueType, uint>.ToInt(value));
        }

        /// <summary>
        /// 枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumLongMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(PubPlus.EnumCastEnum<TValueType, long>.ToInt(value));
        }

        /// <summary>
        /// 枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumULongMember<TValueType>(TValueType value)
        {
            Stream.Unsafer.Write(PubPlus.EnumCastEnum<TValueType, ulong>.ToInt(value));
        }
    }
}
