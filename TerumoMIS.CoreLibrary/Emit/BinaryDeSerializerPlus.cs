//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: BinaryDeSerializerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  BinaryDeSerializerPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 10:00:57
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Runtime.CompilerServices;
using TerumoMIS.CoreLibrary.Code;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    ///     二进制数据序列化
    /// </summary>
    public abstract unsafe class BinaryDeSerializerPlus
    {
        /// <summary>
        ///     反序列化状态
        /// </summary>
        public enum DeSerializeStateEnum : byte
        {
            /// <summary>
            ///     成功
            /// </summary>
            Success,

            /// <summary>
            ///     数据不可识别
            /// </summary>
            UnknownData,

            /// <summary>
            ///     成员位图检测失败
            /// </summary>
            MemberMapPlus,

            /// <summary>
            ///     成员位图类型错误
            /// </summary>
            MemberMapPlusType,

            /// <summary>
            ///     成员位图数量验证失败
            /// </summary>
            MemberMapPlusVerify,

            /// <summary>
            ///     头部数据不匹配
            /// </summary>
            HeaderError,

            /// <summary>
            ///     结束验证错误
            /// </summary>
            EndVerify,

            /// <summary>
            ///     数据完整检测失败
            /// </summary>
            FullDataError,

            /// <summary>
            ///     没有命中历史对象
            /// </summary>
            NoPoint,

            /// <summary>
            ///     数据长度不足
            /// </summary>
            IndexOutOfRange,

            /// <summary>
            ///     不支持对象null解析检测失败
            /// </summary>
            NotNull,

            /// <summary>
            ///     成员索引检测失败
            /// </summary>
            MemberIndex,

            /// <summary>
            ///     JSON反序列化失败
            /// </summary>
            JsonError
        }

        /// <summary>
        ///     公共默认配置参数
        /// </summary>
        protected static readonly ConfigPlus DefaultConfig = new ConfigPlus {IsDisposeMemberMapPlus = true};

        /// <summary>
        ///     反序列化配置参数
        /// </summary>
        protected ConfigPlus DeSerializeConfig;

        /// <summary>
        ///     序列化数据结束位置
        /// </summary>
        protected byte* End;

        /// <summary>
        ///     是否序列化成员位图
        /// </summary>
        protected bool IsMemberMapPlus;

        /// <summary>
        ///     成员位图
        /// </summary>
        protected MemberMapPlus MemberMapPlus;

        /// <summary>
        ///     当前读取数据位置
        /// </summary>
        public byte* Read;

        /// <summary>
        ///     序列化数据起始位置
        /// </summary>
        protected byte* Start;

        /// <summary>
        ///     反序列化状态
        /// </summary>
        protected DeSerializeStateEnum State;

        /// <summary>
        ///     数据字节数组
        /// </summary>
        public byte[] Buffer { get; protected set; }

        /// <summary>
        ///     检测反序列化状态
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckState()
        {
            if (State == DeSerializeStateEnum.Success)
            {
                if (DeSerializeConfig.IsFullData)
                {
                    if (Read != End) Error(DeSerializeStateEnum.FullDataError);
                }
                else if (Read <= End)
                {
                    var length = *(int*) Read;
                    if (length == Read - Start) DeSerializeConfig.DataLength = length + sizeof (int);
                    Error(DeSerializeStateEnum.EndVerify);
                }
                else Error(DeSerializeStateEnum.EndVerify);
            }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Free()
        {
            if (DeSerializeConfig.IsDisposeMemberMapPlus)
            {
                if (MemberMapPlus != null)
                {
                    MemberMapPlus.Dispose();
                    MemberMapPlus = null;
                }
            }
            else MemberMapPlus = null;
        }

        /// <summary>
        ///     设置错误状态
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(DeSerializeStateEnum state)
        {
            State = state;
            if (DeSerializeConfig.IsLogError) LogPlus.Error.Add(state.ToString(), true);
            if (DeSerializeConfig.IsThrowError) throw new Exception(state.ToString());
        }

        /// <summary>
        ///     自定义序列化重置当前读取数据位置
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool VerifyRead(int size)
        {
            if ((Read += size) <= End) return true;
            Error(DeSerializeStateEnum.IndexOutOfRange);
            return false;
        }

        /// <summary>
        ///     JSON反序列化
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ParseJson<TValueType>(ref TValueType value)
        {
            var size = *(int*) Read;
            if (size == 0)
            {
                Read += sizeof (int);
                return;
            }
            if (size > 0 && (size & 1) == 0)
            {
                var start = Read;
                if ((Read += (size + (2 + sizeof (int))) & (int.MaxValue - 3)) <= End)
                {
                    if (!JsonParserPlus.Parse((char*) start, size >> 1, ref value))
                        Error(DeSerializeStateEnum.JsonError);
                    return;
                }
            }
            Error(DeSerializeStateEnum.IndexOutOfRange);
        }

        /// <summary>
        ///     不支持对象null解析检测
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckNullBase()
        {
            if (*(int*) Read == BinarySerializerPlus.NullValue) Read += sizeof (int);
            else Error(DeSerializeStateEnum.NotNull);
        }

        /// <summary>
        ///     对象null值检测
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int CheckNull()
        {
            if (*(int*) Read == BinarySerializerPlus.NullValue)
            {
                Read += sizeof (int);
                return 0;
            }
            return 1;
        }

        /// <summary>
        ///     检测成员数量
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckMemberCount(int count)
        {
            if (*(int*) Read == count)
            {
                Read += sizeof (int);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     成员位图反序列化
        /// </summary>
        /// <param name="fieldCount"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong DeSerializeMemberMapPlus(int fieldCount)
        {
            if (*(int*) Read == fieldCount)
            {
                var value = *(ulong*) (Read + sizeof (int));
                Read += sizeof (int) + sizeof (ulong);
                return value;
            }
            Error(DeSerializeStateEnum.MemberMapPlusVerify);
            return 0;
        }

        /// <summary>
        ///     成员位图反序列化
        /// </summary>
        /// <param name="map"></param>
        /// <param name="fieldCount"></param>
        /// <param name="size"></param>
        internal void DeSerializeMemberMapPlus(byte* map, int fieldCount, int size)
        {
            if (*(int*) Read == fieldCount)
            {
                if (size <= (int) (End - (Read += sizeof (int))))
                {
                    for (var mapEnd = map + (size & (int.MaxValue - sizeof (ulong) + 1));
                        map != mapEnd;
                        map += sizeof (ulong), Read += sizeof (ulong)) *(ulong*) map = *(ulong*) Read;
                    if ((size & sizeof (int)) != 0)
                    {
                        *(uint*) map = *(uint*) Read;
                        Read += sizeof (uint);
                    }
                }
                else Error(DeSerializeStateEnum.IndexOutOfRange);
            }
            else Error(DeSerializeStateEnum.MemberMapPlusVerify);
        }

        /// <summary>
        ///     检测成员位图
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <returns></returns>
        public MemberMapPlus CheckMemberMapPlus<TValueType>()
        {
            if ((*(uint*) Read & 0xc0000000U) == 0)
            {
                if (MemberMapPlus == null)
                {
                    MemberMapPlus = MemberMapPlus<TValueType>.New();
                    if (*Read == 0)
                    {
                        Read += sizeof (int);
                        return MemberMapPlus;
                    }
                }
                else
                {
                    if (MemberMapPlus.Type != MemberMapPlus<TValueType>.Type)
                    {
                        Error(DeSerializeStateEnum.MemberMapPlusType);
                        return null;
                    }
                    if (*Read == 0)
                    {
                        MemberMapPlus.Clear();
                        Read += sizeof (int);
                        return MemberMapPlus;
                    }
                }
                MemberMapPlus.FieldDeSerialize(this);
                return State == DeSerializeStateEnum.Success ? MemberMapPlus : null;
            }
            Error(DeSerializeStateEnum.MemberMapPlus);
            return null;
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref bool value)
        {
            value = *(bool*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize()
        {
            var value = false;
            MemberDeSerialize(ref value);
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref bool value)
        {
            value = *(bool*) Read++;
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, bool[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            for (var index = 0; index != value.Length; ++index) value[index] = arrayMap.Next() != 0;
            return arrayMap.Read;
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref bool? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = *(bool*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     逻辑值反序列化
        /// </summary>
        /// <param name="value">逻辑值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref bool? value)
        {
            if (*Read == 0) value = null;
            else value = *Read == 2;
            ++Read;
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        internal static byte* DeSerialize(byte* data, bool?[] value)
        {
            var arrayMap = new ArrayMapStruct(data, 2);
            for (var index = 0; index != value.Length; ++index) value[index] = arrayMap.NextBool();
            return arrayMap.Read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref byte value)
        {
            value = *Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref byte value)
        {
            value = *Read++;
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, byte[] value)
        {
            MemoryUnsafe.Copy(data, value, value.Length);
            return data + ((value.Length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerializeBase(ref SubArrayStruct<byte> value)
        {
            var length = *(int*) Read;
            if (length == 0)
            {
                value.UnsafeSetLength(0);
                Read += sizeof (int);
            }
            else
            {
                if (((length + (3 + sizeof (int))) & (int.MaxValue - 3)) <= (int) (End - Read))
                {
                    var array = new byte[length];
                    Read = DeSerialize(Read + sizeof (int), array);
                    value.UnsafeSet(array, 0, length);
                }
                else Error(DeSerializeStateEnum.IndexOutOfRange);
            }
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void DeSerialize(ref SubArrayStruct<byte> value)
        {
            var read = DeSerialize(Read, End, Buffer, ref value);
            if (read == null) Error(DeSerializeStateEnum.IndexOutOfRange);
            else Read = read;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="end">结束标记</param>
        /// <param name="buffer">缓冲</param>
        /// <param name="value">值</param>
        /// <param name="read">读取标记</param>
        internal static byte* DeSerialize(byte* read, byte* end, byte[] buffer, ref SubArrayStruct<byte> value)
        {
            var length = *(int*) read;
            if (length > 0)
            {
                var start = read;
                if ((read += (length + (3 + sizeof (int))) & (int.MaxValue - 3)) <= end)
                {
                    fixed (byte* bufferFixed = buffer)
                    {
                        value.UnsafeSet(buffer, (int) (start - bufferFixed) + sizeof (int), length);
                        return read;
                    }
                }
            }
            else if (length == 0)
            {
                value.UnsafeSet(NullValuePlus<byte>.Array, 0, 0);
                return read + sizeof (int);
            }
            else if (length == BinarySerializerPlus.NullValue)
            {
                value.Null();
                return read + sizeof (int);
            }
            return null;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref byte? value)
        {
            value = *Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref byte? value)
        {
            if (*(Read + sizeof (byte)) == 0) value = *Read;
            else value = null;
            Read += sizeof (ushort);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, byte?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var start = (data += ((value.Length + 31) >> 5) << 2);
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *data++;
            }
            return data + ((int) (start - data) & 3);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref sbyte value)
        {
            value = (sbyte) *(int*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref sbyte value)
        {
            value = *(sbyte*) Read++;
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, sbyte[] value)
        {
            fixed (sbyte* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, value.Length);
            return data + ((value.Length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref sbyte? value)
        {
            value = (sbyte) *(int*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref sbyte? value)
        {
            if (*(Read + sizeof (byte)) == 0) value = *(sbyte*) Read;
            else value = null;
            Read += sizeof (ushort);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(sbyte* data, sbyte?[] value)
        {
            var arrayMap = new ArrayMapStruct((byte*) data);
            var start = (data += ((value.Length + 31) >> 5) << 2);
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *data++;
            }
            return (byte*) (data + ((int) (start - data) & 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref short value)
        {
            value = (short) *(int*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref short value)
        {
            value = *(short*) Read;
            Read += sizeof (short);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, short[] value)
        {
            var length = value.Length*sizeof (short);
            fixed (short* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + ((length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref short? value)
        {
            value = (short) *(int*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref short? value)
        {
            if (*(ushort*) (Read + sizeof (ushort)) == 0) value = *(short*) Read;
            else value = null;
            Read += sizeof (int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, short?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            short* read = (short*) (data + (((value.Length + 31) >> 5) << 2)), start = read;
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return ((int) ((byte*) read - (byte*) start) & 2) == 0 ? (byte*) read : (byte*) (read + 1);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref ushort value)
        {
            value = *(ushort*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref ushort value)
        {
            value = *(ushort*) Read;
            Read += sizeof (ushort);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, ushort[] value)
        {
            var length = value.Length*sizeof (ushort);
            fixed (ushort* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + ((length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref ushort? value)
        {
            value = *(ushort*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref ushort? value)
        {
            if (*(ushort*) (Read + sizeof (ushort)) == 0) value = *(ushort*) Read;
            else value = null;
            Read += sizeof (int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, ushort?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            ushort* read = (ushort*) (data + (((value.Length + 31) >> 5) << 2)), start = read;
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return ((int) ((byte*) read - (byte*) start) & 2) == 0 ? (byte*) read : (byte*) (read + 1);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref int value)
        {
            value = *(int*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, int[] value)
        {
            var length = value.Length*sizeof (int);
            fixed (int* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref int? value)
        {
            value = *(int*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref int? value)
        {
            if (*(int*) Read == 0)
            {
                value = *(int*) (Read += sizeof (int));
                Read += sizeof (int);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, int?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (int*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref uint value)
        {
            value = *(uint*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, uint[] value)
        {
            var length = value.Length*sizeof (uint);
            fixed (uint* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref uint? value)
        {
            value = *(uint*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref uint? value)
        {
            if (*(int*) Read == 0)
            {
                value = *(uint*) (Read += sizeof (int));
                Read += sizeof (uint);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, uint?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (uint*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref long value)
        {
            value = *(long*) Read;
            Read += sizeof (long);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, long[] value)
        {
            var length = value.Length*sizeof (long);
            fixed (long* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref long? value)
        {
            value = *(long*) Read;
            Read += sizeof (long);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref long? value)
        {
            if (*(int*) Read == 0)
            {
                value = *(long*) (Read += sizeof (int));
                Read += sizeof (long);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, long?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (long*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref ulong value)
        {
            value = *(ulong*) Read;
            Read += sizeof (ulong);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, ulong[] value)
        {
            var length = value.Length*sizeof (ulong);
            fixed (ulong* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref ulong? value)
        {
            value = *(ulong*) Read;
            Read += sizeof (ulong);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref ulong? value)
        {
            if (*(int*) Read == 0)
            {
                value = *(ulong*) (Read += sizeof (int));
                Read += sizeof (ulong);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, ulong?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (ulong*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref float value)
        {
            value = *(float*) Read;
            Read += sizeof (float);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, float[] value)
        {
            var length = value.Length*sizeof (float);
            fixed (float* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref float? value)
        {
            value = *(float*) Read;
            Read += sizeof (float);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref float? value)
        {
            if (*(int*) Read == 0)
            {
                value = *(float*) (Read += sizeof (int));
                Read += sizeof (float);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, float?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (float*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref double value)
        {
            value = *(double*) Read;
            Read += sizeof (double);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, double[] value)
        {
            var length = value.Length*sizeof (double);
            fixed (double* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref double? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = *(double*) Read;
            Read += sizeof (double);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref double? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (*(int*) Read == 0)
            {
                value = *(double*) (Read += sizeof (int));
                Read += sizeof (double);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, double?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (double*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref decimal value)
        {
            value = *(decimal*) Read;
            Read += sizeof (decimal);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, decimal[] value)
        {
            var length = value.Length*sizeof (decimal);
            fixed (decimal* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref decimal? value)
        {
            value = *(decimal*) Read;
            Read += sizeof (decimal);
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref decimal? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (*(int*) Read == 0)
            {
                value = *(decimal*) (Read += sizeof (int));
                Read += sizeof (decimal);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, decimal?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (decimal*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref char value)
        {
            value = *(char*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref char value)
        {
            value = *(char*) Read;
            Read += sizeof (char);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, char[] value)
        {
            var length = value.Length*sizeof (char);
            fixed (char* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + ((length + 3) & (int.MaxValue - 3));
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref char? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = *(char*) Read;
            Read += sizeof (int);
        }

        /// <summary>
        ///     字符反序列化
        /// </summary>
        /// <param name="value">字符</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref char? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (*(ushort*) (Read + sizeof (char)) == 0) value = *(char*) Read;
            else value = null;
            Read += sizeof (int);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, char?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            char* read = (char*) (data + (((value.Length + 31) >> 5) << 2)), start = read;
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return ((int) ((byte*) read - (byte*) start) & 2) == 0 ? (byte*) read : (byte*) (read + 1);
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref DateTime value)
        {
            value = *(DateTime*) Read;
            Read += sizeof (DateTime);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, DateTime[] value)
        {
            var length = value.Length*sizeof (DateTime);
            fixed (DateTime* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref DateTime? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = *(DateTime*) Read;
            Read += sizeof (DateTime);
        }

        /// <summary>
        ///     时间反序列化
        /// </summary>
        /// <param name="value">时间</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref DateTime? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (*(int*) Read == 0)
            {
                value = *(DateTime*) (Read += sizeof (int));
                Read += sizeof (DateTime);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, DateTime?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (DateTime*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        /// <summary>
        ///     数值反序列化
        /// </summary>
        /// <param name="value">数值</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [IndexDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once RedundantAssignment
        protected void DeSerialize(ref Guid value)
        {
            value = *(Guid*) Read;
            Read += sizeof (Guid);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, Guid[] value)
        {
            var length = value.Length*sizeof (Guid);
            fixed (Guid* valueFixed = value) MemoryUnsafe.Copy(data, valueFixed, length);
            return data + length;
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref Guid? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = *(Guid*) Read;
            Read += sizeof (Guid);
        }

        /// <summary>
        ///     Guid反序列化
        /// </summary>
        /// <param name="value">Guid</param>
        [IndexDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MemberDeSerialize(ref Guid? value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (*(int*) Read == 0)
            {
                value = *(Guid*) (Read += sizeof (int));
                Read += sizeof (Guid);
            }
            else
            {
                Read += sizeof (int);
                value = null;
            }
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte* DeSerialize(byte* data, Guid?[] value)
        {
            var arrayMap = new ArrayMapStruct(data);
            var read = (Guid*) (data + (((value.Length + 31) >> 5) << 2));
            for (var index = 0; index != value.Length; ++index)
            {
                if (arrayMap.Next() == 0) value[index] = null;
                else value[index] = *read++;
            }
            return (byte*) read;
        }

        [IndexDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.DeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberDeSerializeMethodPlus]
        [DataDeSerializerPlus.MemberMapDeSerializeMethodPlus]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DeSerialize(ref SubStringStruct value)
        {
            string stringValue = null;
            if ((Read = DeSerialize(Read, End, ref stringValue)) == null) Error(DeSerializeStateEnum.IndexOutOfRange);
            else value.UnsafeSet(stringValue, 0, stringValue.Length);
        }

        /// <summary>
        ///     反序列化数据
        /// </summary>
        /// <param name="data">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <param name="value">目标数据</param>
        /// <returns>结束位置,失败返回null</returns>
        internal static byte* DeSerialize(byte* data, byte* end, ref string value)
        {
            var length = *(int*) data;
            if ((length & 1) == 0)
            {
                if (length != 0)
                {
                    var dataLength = (length + (3 + sizeof (int))) & (int.MaxValue - 3);
                    if (dataLength <= end - data)
                    {
                        value = new string((char*) (data + sizeof (int)), 0, length >> 1);
                        return data + dataLength;
                    }
                }
                else
                {
                    value = string.Empty;
                    return data + sizeof (int);
                }
            }
            else
            {
                var dataLength = ((length >>= 1) + (3 + sizeof (int))) & (int.MaxValue - 3);
                if (dataLength <= end - data)
                {
                    fixed (char* valueFixed = (value = StringPlus.FastAllocateString(length)))
                    {
                        var start = data + sizeof (int);
                        var write = valueFixed;
                        end = start + length;
                        do
                        {
                            *write++ = (char) *start++;
                        } while (start != end);
                    }
                    return data + dataLength;
                }
            }
            return null;
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumByte<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, byte>.FromInt(*Read);
            Read += sizeof (int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumByteMember<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, byte>.FromInt(*Read++);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumSByte<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, sbyte>.FromInt((sbyte) *(int*) Read);
            Read += sizeof (int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumSByteMember<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, sbyte>.FromInt(*(sbyte*) Read++);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumShort<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, short>.FromInt((short) *(int*) Read);
            Read += sizeof (int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumShortMember<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, short>.FromInt(*(short*) Read);
            Read += sizeof (short);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumUShort<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, ushort>.FromInt(*(ushort*) Read);
            Read += sizeof (int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumUShortMember<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, ushort>.FromInt(*(ushort*) Read);
            Read += sizeof (ushort);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumInt<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, int>.FromInt(*(int*) Read);
            Read += sizeof (int);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumUInt<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, uint>.FromInt(*(uint*) Read);
            Read += sizeof (uint);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumLong<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, long>.FromInt(*(long*) Read);
            Read += sizeof (long);
        }

        /// <summary>
        ///     枚举值序列化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnumULong<TValueType>(ref TValueType value)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = PubPlus.EnumCastEnum<TValueType, ulong>.FromInt(*(ulong*) Read);
            Read += sizeof (ulong);
        }

        /// <summary>
        ///     数组位图
        /// </summary>
        internal struct ArrayMapStruct
        {
            /// <summary>
            ///     当前位
            /// </summary>
            public uint Bit;

            /// <summary>
            ///     当前位图
            /// </summary>
            public uint Map;

            /// <summary>
            ///     当前读取位置
            /// </summary>
            public byte* Read;

            /// <summary>
            ///     数组位图
            /// </summary>
            /// <param name="read">当前读取位置</param>
            public ArrayMapStruct(byte* read)
            {
                Read = read;
                Bit = 1;
                Map = 0;
            }

            /// <summary>
            ///     数组位图
            /// </summary>
            /// <param name="read">当前读取位置</param>
            /// <param name="bit">当前位</param>
            public ArrayMapStruct(byte* read, uint bit)
            {
                Read = read;
                Bit = bit;
                Map = 0;
            }

            /// <summary>
            ///     获取位图数据
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint Next()
            {
                if (Bit == 1)
                {
                    Map = *(uint*) Read;
                    Bit = 1U << 31;
                    Read += sizeof (uint);
                }
                else Bit >>= 1;
                return Map & Bit;
            }

            /// <summary>
            ///     获取位图数据
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool? NextBool()
            {
                if (Bit == 2)
                {
                    Map = *(uint*) Read;
                    Bit = 1U << 31;
                    Read += sizeof (uint);
                }
                else Bit >>= 2;
                if ((Map & Bit) == 0) return null;
                return (Map & (Bit >> 1)) != 0;
            }
        }

        /// <summary>
        ///     配置参数
        /// </summary>
        public sealed class ConfigPlus
        {
            /// <summary>
            ///     是否自动释放成员位图
            /// </summary>
            internal bool IsDisposeMemberMapPlus;

            /// <summary>
            ///     数据是否完整
            /// </summary>
            public bool IsFullData = true;

            /// <summary>
            ///     是否输出错误日志
            /// </summary>
            public bool IsLogError = true;

            /// <summary>
            ///     是否抛出错误异常
            /// </summary>
            public bool IsThrowError;

            /// <summary>
            ///     成员位图
            /// </summary>
            public MemberMapPlus MemberMapPlus;

            /// <summary>
            ///     反序列化状态
            /// </summary>
            public DeSerializeStateEnum State;

            /// <summary>
            ///     数据长度
            /// </summary>
            public int DataLength { get; internal set; }
        }
    }
}