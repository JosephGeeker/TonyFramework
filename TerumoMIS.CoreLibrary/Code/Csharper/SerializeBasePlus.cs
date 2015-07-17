//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SerializeBasePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  SerializeBasePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:43:55
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
    public abstract class SerializeBasePlus:MemberFilterPlus.PublicInstanceFieldPlus
    {
        /// <summary>
        /// 序列化成员配置
        /// </summary>
        public sealed class member : ignoreOld
        {
        }
        /// <summary>
        /// 默认选择类型
        /// </summary>
        protected const code.memberFilters defaultMemberFilter = code.memberFilters.InstanceField;
        /// <summary>
        /// 空对象长度
        /// </summary>
        internal static readonly byte[] NullValueData = BitConverter.GetBytes(fastCSharp.emit.binarySerializer.NullValue);
        /// <summary>
        /// 未知类型数据
        /// </summary>
        internal static class unknownValue
        {
            /// <summary>
            /// 泛型类型缓存
            /// </summary>
            private static readonly Dictionary<Type, keyValue<Type, Type>> genericTypes = dictionary.CreateOnly<Type, keyValue<Type, Type>>();
            /// <summary>
            /// 泛型类型缓存访问锁
            /// </summary>
            private static int genericTypeLock;
            /// <summary>
            /// 泛型类型缓存版本
            /// </summary>
            private static int genericTypeVersion;
            /// <summary>
            /// 获取 泛型类型
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>泛型类型</returns>
            public static Type GetGenericType(Type type)
            {
                keyValue<Type, Type> genericType;
                int version = genericTypeVersion;
                if (genericTypes.TryGetValue(type, out genericType) && genericType.Key == type) return genericType.Value;
                interlocked.CompareSetSleep(ref genericTypeLock);
                try
                {
                    if (version == genericTypeVersion || !genericTypes.TryGetValue(type, out genericType))
                    {
                        genericTypes.Add(type, genericType = new keyValue<Type, Type>(type, typeof(unknownValue<>).MakeGenericType(type)));
                        ++genericTypeVersion;
                    }
                }
                finally { genericTypeLock = 0; }
                return genericType.Value;
            }
            /// <summary>
            /// 未知类型数据转换
            /// </summary>
            /// <param name="value">未知类型数据</param>
            /// <returns>未知类型数据</returns>
            private static object converter<valueType>(object value)
            {
                return new unknownValue<valueType> { Value = (valueType)value };
            }
            /// <summary>
            /// 未知类型数据转换 函数信息
            /// </summary>
            private static readonly MethodInfo converterMethod = typeof(unknownValue).GetMethod("converter", BindingFlags.Static | BindingFlags.NonPublic);
            /// <summary>
            /// 未知类型数据转换
            /// </summary>
            /// <param name="value">待转换数据</param>
            /// <param name="convertType">目标类型</param>
            /// <returns>转换后的数据</returns>
            public static object Converter(object value, Type convertType)
            {
                return ((Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), converterMethod.MakeGenericMethod(convertType)))(value);
            }
            /// <summary>
            /// 未知类型数据转换
            /// </summary>
            /// <param name="value">未知类型数据</param>
            /// <returns>未知类型数据</returns>
            private static object getValue<valueType>(object value)
            {
                return ((unknownValue<valueType>)value).Value;
            }
            /// <summary>
            /// 未知类型数据转换 函数信息
            /// </summary>
            private static readonly MethodInfo getValueMethod = typeof(unknownValue).GetMethod("getValue", BindingFlags.Static | BindingFlags.NonPublic);
            /// <summary>
            /// 未知类型数据转换
            /// </summary>
            /// <param name="type">原类型</param>
            /// <param name="convertType">目标类型</param>
            /// <param name="value">待转换数据</param>
            /// <returns>转换后的数据</returns>
            public static object GetValue(Type type, Type convertType, object value)
            {
                return ((Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), getValueMethod.MakeGenericMethod(convertType)))(value);
            }
        }
        /// <summary>
        /// 未知类型数据
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        protected struct unknownValue<valueType>
        {
            /// <summary>
            /// 数据
            /// </summary>
            public valueType Value;
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        public abstract unsafe class serializer
        {
            /// <summary>
            /// 是否检测相同的引用成员
            /// </summary>
            protected bool isReferenceMember;
            /// <summary>
            /// 历史对象指针位置
            /// </summary>
            protected internal Dictionary<objectReference, int> points;
            /// </summary>
            /// 数据流起始位置
            /// </summary>
            protected int streamStartIndex;
            /// <summary>
            /// 成员选择(反射模式)
            /// </summary>
            protected internal code.memberFilters memberFilter;
            /// <summary>
            /// 当前写入位置
            /// </summary>
            protected internal byte* write;
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            protected serializer(bool isReferenceMember)
            {
                this.isReferenceMember = isReferenceMember;
                if (isReferenceMember && points == null) points = dictionary<objectReference>.Create<int>();
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="parentSerializer">序列化</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            protected serializer(serializer parentSerializer, bool isReferenceMember)
            {
                points = parentSerializer.points;
                this.isReferenceMember = isReferenceMember;
                if (isReferenceMember && points == null) points = dictionary<objectReference>.Create<int>();
            }
            /// <summary>
            /// 设置历史对象指针位置
            /// </summary>
            /// <param name="serializer">成员序列化器</param>
            public void SetPoint(serializer serializer)
            {
                if (points == null) points = serializer.points;
            }
        }
        /// <summary>
        /// 对象序列化
        /// </summary>
        public abstract unsafe class unmanagedStreamSerializer : serializer
        {
            /// <summary>
            /// 数据流
            /// </summary>
            public unmanagedStream dataStream { get; protected set; }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="stream">序列化流</param>
            /// <param name="memberCount">长远数量</param>
            protected unmanagedStreamSerializer(bool isReferenceMember, unmanagedStream stream, int memberCount)
                : base(isReferenceMember)
            {
                dataStream = stream;
                streamStartIndex = stream.OffsetLength;
                stream.Write((int)fastCSharp.code.cSharp.serializeVersion.serialize);
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="stream">序列化流</param>
            /// <param name="memberFilter">成员选择</param>
            protected unmanagedStreamSerializer(bool isReferenceMember, unmanagedStream stream, code.memberFilters memberFilter)
                : base(isReferenceMember)
            {
                dataStream = stream;
                streamStartIndex = stream.OffsetLength;
                this.memberFilter = memberFilter;
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="parentSerializer">序列化</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            protected unmanagedStreamSerializer(unmanagedStreamSerializer parentSerializer, bool isReferenceMember)
                : base(parentSerializer, isReferenceMember)
            {
                dataStream = parentSerializer.dataStream;
                streamStartIndex = parentSerializer.streamStartIndex;
                this.memberFilter = defaultMemberFilter;
            }
            /// <summary>
            /// 历史记录检测
            /// </summary>
            /// <param name="value">检测对象</param>
            /// <returns>是否新的对象(非历史对象)</returns>
            protected internal bool checkPoint(object value)
            {
                return value != null && checkPointNotNull(value);
            }
            /// <summary>
            /// 历史记录检测
            /// </summary>
            /// <param name="value">检测对象</param>
            /// <returns>是否新的对象(非历史对象)</returns>
            protected internal bool checkPointNotNull(object value)
            {
                int length;
                if (points.TryGetValue(new objectReference { Value = value }, out length))
                {
                    dataStream.Write(-length);
                    return false;
                }
                points[new objectReference { Value = value }] = dataStream.OffsetLength - streamStartIndex;
                return true;
            }
            /// <summary>
            /// 历史记录检测
            /// </summary>
            /// <param name="value">检测对象</param>
            /// <returns>是否新的对象(非历史对象)</returns>
            protected internal bool checkPointReferenceMember(object value)
            {
                return !isReferenceMember || checkPoint(value);
            }
            /// <summary>
            /// 字符串序列化
            /// </summary>
            /// <param name="value">字符串</param>
            protected internal void serializeString(string value)
            {
                if (value != null)
                {
                    if (value.Length == 0) dataStream.Write(0);
                    else if (!isReferenceMember || checkPointNotNull(value)) fastCSharp.emit.dataSerializer.Serialize(dataStream, value);
                }
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(byte?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(sbyte?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(short?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(ushort?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(int?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(uint?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(long?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(ulong?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(char?[] array)
            {
                fastCSharp.emit.dataSerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(DateTime?[] array)
            {
                fastCSharp.emit.dataSerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(float?[] array)
            {
                fastCSharp.emit.binarySerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(double?[] array)
            {
                fastCSharp.emit.dataSerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(decimal?[] array)
            {
                fastCSharp.emit.dataSerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 序列化可空数组
            /// </summary>
            /// <param name="array">数组数据</param>
            protected void nullArrayNoPoint(Guid?[] array)
            {
                fastCSharp.emit.dataSerializer.Serialize(dataStream, array);
            }
            /// <summary>
            /// 未知类型数组序列化
            /// </summary>
            /// <typeparam name="valueType">对象类型</typeparam>
            /// <param name="array">数组数据</param>
            protected static void arrayMap<valueType>(valueType[] array, unmanagedStream dataStream)
            {
                int length = sizeof(int) + (((array.Length + 31) >> 5) << 2);
                dataStream.PrepLength(length);
                unmanagedStream.unsafer unsafeStream = dataStream.Unsafer;
                byte* write = dataStream.CurrentData;
                *(int*)write = array.Length;
                fixedMap nullMap = new fixedMap(write += sizeof(int));
                unsafer.memory.Fill(write, 0U, (length >> 2) - 1);
                unsafeStream.AddLength(length);
                length = 0;
                foreach (valueType nextValue in array)
                {
                    if (nextValue == null) nullMap.Set(length);
                    ++length;
                }
                dataStream.PrepLength();
            }
            /// <summary>
            /// 序列化字符串数组
            /// </summary>
            /// <param name="array">字符串数组</param>
            protected void stringArrayNoPoint(string[] array)
            {
                arrayMap(array, dataStream);
                if (isReferenceMember)
                {
                    foreach (string nextValue in array)
                    {
                        if (nextValue != null)
                        {
                            if (nextValue.Length == 0) dataStream.Write(0);
                            else if (checkPointNotNull(nextValue)) fastCSharp.emit.dataSerializer.Serialize(dataStream, nextValue);
                        }
                    }
                }
                else
                {
                    foreach (string nextValue in array)
                    {
                        if (nextValue != null)
                        {
                            if (nextValue.Length == 0) dataStream.Write(0);
                            else fastCSharp.emit.dataSerializer.Serialize(dataStream, nextValue);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 对象反序列化
        /// </summary>
        public unsafe abstract class deSerializer
        {
            /// <summary>
            /// 历史对象指针位置
            /// </summary>
            protected internal Dictionary<int, object> points;
            /// <summary>
            /// 当前位置
            /// </summary>
            public byte* Read { get; protected internal set; }
            /// <summary>
            /// 数据起始位置
            /// </summary>
            protected internal byte* dataStart;
            /// <summary>
            /// 数据结束位置
            /// </summary>
            public byte* dataEnd { get; protected set; }
            /// <summary>
            /// 是否检测相同的引用成员
            /// </summary>
            protected internal bool isReferenceMember;
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            protected deSerializer(bool isReferenceMember)
            {
                this.isReferenceMember = isReferenceMember;
                if (isReferenceMember) points = dictionary.CreateInt<object>();
            }
            /// <summary>
            /// 对象集合反序列化
            /// </summary>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            /// <param name="dataStart">序列化数据起始位置</param>
            /// <param name="dataEnd">序列化数据结束位置</param>
            protected deSerializer(bool isReferenceMember, byte* dataStart, byte* dataEnd)
                : this(isReferenceMember)
            {
                this.dataStart = Read = dataStart;
                this.dataEnd = dataEnd - sizeof(int);
            }
            /// <summary>
            /// 对象反序列化
            /// </summary>
            /// <param name="data">序列化数据</param>
            /// <param name="isReferenceMember">是否检测相同的引用成员</param>
            protected deSerializer(deSerializer parentDeSerializer, bool isReferenceMember)
            {
                dataEnd = parentDeSerializer.dataEnd;
                dataStart = parentDeSerializer.dataStart;
                Read = parentDeSerializer.Read;
                points = parentDeSerializer.points;
                this.isReferenceMember = isReferenceMember;
                if (isReferenceMember && points == null) points = dictionary.CreateInt<object>();
            }
            /// <summary>
            /// 设置当前位置
            /// </summary>
            /// <param name="deSerializer">对象反序列化</param>
            public void SetReadPoint(deSerializer deSerializer)
            {
                Read = deSerializer.Read;
                if (points == null) points = deSerializer.points;
            }
            /// <summary>
            /// 检测数据是否结束
            /// </summary>
            /// <param name="length">字节数</param>
            private void checkEnd(long length)
            {
                if ((dataEnd - Read) < length) fastCSharp.log.Default.Throw("数据不足 " + (dataEnd - Read).toString() + " < " + length.toString(), true, false);
            }
            /// <summary>
            /// 历史记录检测
            /// </summary>
            /// <returns>历史记录对象,失败返回null</returns>
            protected object getPoint()
            {
                object value;
                int point = *(int*)Read;
                if (points.TryGetValue(point, out value))
                {
                    Read += sizeof(int);
                    return value;
                }
                return null;
            }
            /// <summary>
            /// 反序列化字符串
            /// </summary>
            /// <returns>字符串</returns>
            protected string getStringNoPoint()
            {
                int length = *(int*)Read, point = (int)(Read - dataStart);
                Read += sizeof(int);
                if ((length & 1) == 0)
                {
                    if (length != 0)
                    {
                        checkEnd(length);
                        string value = new string((char*)Read, 0, length >> 1);
                        Read += length + (length & 2);
                        if (isReferenceMember) points.Add(-point, value);
                        return value;
                    }
                }
                else if (length > 1)
                {
                    checkEnd(length >>= 1);
                    string value = fastCSharp.String.FastAllocateString(length);
                    fixed (char* valueFixed = value)
                    {
                        char* start = valueFixed;
                        for (byte* end = Read + length; Read != end; *start++ = (char)*Read++) ;
                    }
                    if ((length & 3) != 0) Read += -length & 3;
                    if (isReferenceMember) points.Add(-point, value);
                    return value;
                }
                return string.Empty;
            }
            /// <summary>
            /// 反序列化字符串
            /// </summary>
            /// <returns>字符串</returns>
            protected internal string getString()
            {
                if (isReferenceMember)
                {
                    object reference = getPoint();
                    if (reference != null) return (string)reference;
                }
                return getStringNoPoint();
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void deSerializeNoPoint(bool[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 创建数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <returns>数组数据</returns>
            protected valueType[] createArray<valueType>()
            {
                int length = *(int*)Read;
                checkEnd((length + 7) >> 3);
                valueType[] array = length != 0 ? new valueType[length] : nullValue<valueType>.Array;
                if (isReferenceMember) points.Add(-(int)(Read - dataStart), array);
                Read += sizeof(int);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected bool?[] boolNullArrayNoPoint()
            {
                bool?[] data = createArray<bool?>();
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, data);
                return data;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(byte?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected byte?[] byteNullArrayNoPoint()
            {
                byte?[] array = createArray<byte?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(sbyte?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize((sbyte*)Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected sbyte?[] sByteNullArrayNoPoint()
            {
                sbyte?[] array = createArray<sbyte?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(short?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected short?[] shortNullArrayNoPoint()
            {
                short?[] array = createArray<short?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(ushort?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected ushort?[] uShortNullArrayNoPoint()
            {
                ushort?[] array = createArray<ushort?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(int?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected int?[] intNullArrayNoPoint()
            {
                int?[] array = createArray<int?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(uint?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected uint?[] uIntNullArrayNoPoint()
            {
                uint?[] array = createArray<uint?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(long?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected long?[] longNullArrayNoPoint()
            {
                long?[] array = createArray<long?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(ulong?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected ulong?[] uLongNullArrayNoPoint()
            {
                ulong?[] array = createArray<ulong?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(char?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected char?[] charNullArrayNoPoint()
            {
                char?[] array = createArray<char?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(DateTime?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected DateTime?[] dateTimeNullArrayNoPoint()
            {
                DateTime?[] array = createArray<DateTime?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(float?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected float?[] floatNullArrayNoPoint()
            {
                float?[] array = createArray<float?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(double?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected double?[] doubleNullArrayNoPoint()
            {
                double?[] array = createArray<double?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(decimal?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected decimal?[] decimalNullArrayNoPoint()
            {
                decimal?[] array = createArray<decimal?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void nullArrayNoPoint(Guid?[] array)
            {
                Read = fastCSharp.emit.dataDeSerializer.DeSerialize(Read, array);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected Guid?[] guidNullArrayNoPoint()
            {
                Guid?[] array = createArray<Guid?>();
                nullArrayNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            /// <param name="size">结构体字节数</param>
            protected void deSerializeNoPoint(void* array, int size)
            {
                unsafer.memory.Copy(Read, array, size);
                Read += (size + 3) & (int.MaxValue - 3);
            }
            /// <summary>
            /// 创建数组
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="size">单个数据字节大小</param>
            /// <returns>数组数据</returns>
            protected valueType[] createArray<valueType>(int size)
            {
                int length = *(int*)Read;
                checkEnd(length * size);
                valueType[] array = length != 0 ? new valueType[length] : nullValue<valueType>.Array;
                if (isReferenceMember && length != 0) points.Add(-(int)(Read - dataStart), array);
                Read += sizeof(int);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void deSerializeNoPoint(DateTime[] array)
            {
                int length = array.Length * sizeof(DateTime);
                fixed (DateTime* dataFixed = array) fastCSharp.unsafer.memory.Copy(Read, dataFixed, length);
                Read += length;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected DateTime[] dateTimeArrayNoPoint()
            {
                DateTime[] array = createArray<DateTime>(sizeof(DateTime));
                deSerializeNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void deSerializeNoPoint(decimal[] array)
            {
                int length = array.Length * sizeof(decimal);
                fixed (decimal* dataFixed = array) fastCSharp.unsafer.memory.Copy(Read, dataFixed, length);
                Read += (length + 3) & (int.MaxValue - 3);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected decimal[] decimalArrayNoPoint()
            {
                decimal[] array = createArray<decimal>(sizeof(decimal));
                deSerializeNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <param name="array">结构体数组</param>
            protected void deSerializeNoPoint(Guid[] array)
            {
                int length = array.Length * sizeof(Guid);
                fixed (Guid* dataFixed = array) fastCSharp.unsafer.memory.Copy(Read, dataFixed, length);
                Read += (length + 3) & (int.MaxValue - 3);
            }
            /// <summary>
            /// 反序列化结构体数组
            /// </summary>
            /// <returns>结构体数组</returns>
            protected Guid[] guidArrayNoPoint()
            {
                Guid[] array = createArray<Guid>(sizeof(Guid));
                deSerializeNoPoint(array);
                return array;
            }
            /// <summary>
            /// 反序列化字符串数组
            /// </summary>
            /// <param name="array">字符串数组</param>
            protected void stringArrayNoPoint(string[] array)
            {
                fixedMap nullMap = new fixedMap(Read);
                Read += ((array.Length + 31) >> 5) << 2;
                for (int index = 0; index != array.Length; ++index)
                {
                    if (!nullMap.Get(index)) array[index] = getString();
                }
            }
        }
        /// <summary>
        /// 对象序列化(反射模式)
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        internal abstract class serializer<valueType>
        {
            /// <summary>
            /// 二次类型转换器
            /// </summary>
            protected sealed class converter2
            {
                /// <summary>
                /// 第一次类型转换器
                /// </summary>
                public Func<object, object> Converter1;
                /// <summary>
                /// 第一次类型转换器
                /// </summary>
                public Func<object, object> Converter2;
                /// <summary>
                /// 类型转换
                /// </summary>
                /// <param name="value">原始数据</param>
                /// <returns>目标数据</returns>
                public object Convert(object value)
                {
                    return Converter2(Converter1(value));
                }
            }
            /// <summary>
            /// 是否值类型
            /// </summary>
            protected static readonly bool isStruct;
            /// <summary>
            /// 是否未知类型泛型包装
            /// </summary>
            protected static readonly bool isUnknownValue;
            /// <summary>
            /// 可空类型的泛型参数类型
            /// </summary>
            protected static readonly Type nullType;
            /// <summary>
            /// 默认序列化代码生成自定义属性集合
            /// </summary>
            protected static subArray<keyValue<Type, object>> serializeAttributes;
            /// <summary>
            /// 默认序列化代码生成自定义属性集合访问锁
            /// </summary>
            private static int serializeAttributeLock;
            /// <summary>
            /// 获取默认序列化代码生成自定义属性
            /// </summary>
            /// <typeparam name="serializeType">序列化代码生成自定义属性类型</typeparam>
            /// <returns>默认序列化代码生成自定义属性</returns>
            protected static serializeType getSerializeAttribute<serializeType>()
                where serializeType : serializeBase
            {
                int count = serializeAttributes.Count;
                if (count != 0)
                {
                    foreach (keyValue<Type, object> value in serializeAttributes.array)
                    {
                        if (value.Key == typeof(serializeType)) return (serializeType)value.Value;
                        if (--count == 0) break;
                    }
                }
                serializeType serializeAttribute = (serializeType)typeof(serializeType).GetField("SerializeAttribute", BindingFlags.Static | BindingFlags.Public).GetValue(null);
                interlocked.CompareSetSleep0(ref serializeAttributeLock);
                try
                {
                    if ((count = serializeAttributes.Count) != 0)
                    {
                        foreach (keyValue<Type, object> value in serializeAttributes.array)
                        {
                            if (value.Key == typeof(serializeType)) return (serializeType)value.Value;
                            if (--count == 0) break;
                        }
                    }
                    serializeAttributes.Add(new keyValue<Type, object>(typeof(serializeType), serializeAttribute));
                }
                finally { serializeAttributeLock = 0; }
                return serializeAttribute;
            }
            static serializer()
            {
                Type type = typeof(valueType);
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(unknownValue<>))
                {
                    isUnknownValue = isStruct = true;
                }
                else
                {
                    memberType memberType = type;
                    if (type.isStruct()) isStruct = true;
                    nullType = type.nullableType();
                }
            }
        }
        /// <summary>
        /// 对象序列化(反射模式)
        /// </summary>
        /// <typeparam name="valueType">对象类型</typeparam>
        /// <typeparam name="serializeType">序列化类型</typeparam>
        internal abstract class serializer<valueType, serializeType> : serializer<valueType> where serializeType : serializeBase
        {
            /// <summary>
            /// 是否序列化接口类型
            /// </summary>
            protected static readonly bool isISerialize;
            /// <summary>
            /// 序列化代码生成自定义属性
            /// </summary>
            protected internal readonly static serializeType serializeAttribute;
            /// <summary>
            /// 动态成员分组
            /// </summary>
            protected readonly static memberGroup<valueType> memberGroup;
            /// <summary>
            /// 成员排序
            /// </summary>
            protected readonly static code.memberInfo[] sortMembers;
            /// <summary>
            /// 成员排序
            /// </summary>
            protected readonly static pointer memberSort;
            /// <summary>
            /// 是否允许空值位图
            /// </summary>
            protected readonly static fixedMap isNullMap;
            /// <summary>
            /// 是否值类型序列化位图
            /// </summary>
            protected readonly static fixedMap isMemberSerializeMap;
            /// <summary>
            /// 公共动态字段成员位图
            /// </summary>
            private readonly static memberMap<valueType> publicInstanceFieldMemberMap;
            /// <summary>
            /// 非公共动态字段成员位图
            /// </summary>
            private readonly static memberMap<valueType> nonPublicInstanceFieldMemberMap;
            /// <summary>
            /// 公共动态属性成员位图
            /// </summary>
            private readonly static memberMap<valueType> publicInstancePropertyMemberMap;
            /// <summary>
            /// 非公共动态属性成员位图
            /// </summary>
            private readonly static memberMap<valueType> nonPublicInstancePropertyMemberMap;
            /// <summary>
            /// 所有成员数量
            /// </summary>
            protected readonly static int memberCount;
            /// <summary>
            /// 成员位图字节长度
            /// </summary>
            protected readonly static int memberMapSize;
            /// <summary>
            /// 序列化基本字节数
            /// </summary>
            protected readonly internal static int serializeSize;
            /// <summary>
            /// 未知类型字段
            /// </summary>
            protected readonly static fieldInfo<valueType> unknownField;
            /// <summary>
            /// 未知类型是否值类型
            /// </summary>
            protected readonly static bool isUnknownMemberSerialize;
            /// <summary>
            /// 未知类型是否允许空值
            /// </summary>
            protected readonly static bool isUnknownNull;
            /// <summary>
            /// 获取序列化成员位图
            /// </summary>
            /// <param name="filter">成员选择</param>
            /// <param name="map">成员位图</param>
            /// <returns>序列化成员位图</returns>
            protected static memberMap<valueType> getSerializeMemberMap(code.memberFilters filter, IMemberMap map)
            {
                memberMap<valueType> memberMap = map == null ? default(memberMap<valueType>) : ((memberMap<valueType>)map).Copy();
                if ((filter & code.memberFilters.PublicInstanceField) != 0)
                {
                    if (!publicInstanceFieldMemberMap.IsDefault)
                    {
                        if (memberMap.IsDefault) memberMap = publicInstanceFieldMemberMap.Copy();
                        else memberMap.And(publicInstanceFieldMemberMap);
                    }
                }
                if ((filter & code.memberFilters.NonPublicInstanceField) != 0)
                {
                    if (!nonPublicInstanceFieldMemberMap.IsDefault)
                    {
                        if (memberMap.IsDefault) memberMap = nonPublicInstanceFieldMemberMap.Copy();
                        else memberMap.And(nonPublicInstanceFieldMemberMap);
                    }
                }
                if ((filter & code.memberFilters.PublicInstanceProperty) != 0)
                {
                    if (!publicInstancePropertyMemberMap.IsDefault)
                    {
                        if (memberMap.IsDefault) memberMap = publicInstancePropertyMemberMap.Copy();
                        else memberMap.And(publicInstancePropertyMemberMap);
                    }
                }
                if ((filter & code.memberFilters.NonPublicInstanceProperty) != 0)
                {
                    if (!nonPublicInstancePropertyMemberMap.IsDefault)
                    {
                        if (memberMap.IsDefault) memberMap = nonPublicInstancePropertyMemberMap.Copy();
                        else memberMap.And(nonPublicInstancePropertyMemberMap);
                    }
                }
                return memberMap;
            }
            static unsafe serializer()
            {
                serializeAttribute = typeof(valueType).customAttribute<serializeType>() ?? getSerializeAttribute<serializeType>();
                if (isUnknownValue)
                {
                    if (typeof(serializeType) == typeof(serialize))
                    {
                        unknownField = (fieldInfo<valueType>)memberGroup<valueType>.GetAllMembers()[0];
                        serializeSize = (unknownField.MemberType.SerializeSize + 3) & (int.MaxValue - 3);
                        if (unknownField.MemberType.SerializeType.IsNull) isUnknownNull = true;
                        if (unknownField.MemberType.IsMemberSerialize) isUnknownMemberSerialize = true;
                    }
                }
                else
                {
                    Type type = typeof(valueType);
                    memberType memberType = type;
                    bool isSerialize = false;
                    if (typeof(serializeType) == typeof(serialize))
                    {
                        if (memberType.IsISerialize) isISerialize = true;
                    }
                    if (!isSerialize)
                    {
                        bool isSetMemberGroup = false;
                        if (isStruct)
                        {
                            if (nullType == null && !memberType.IsMemberSerialize) isSetMemberGroup = true;
                        }
                        else if (!type.IsArray && type != typeof(string)) isSetMemberGroup = true;
                        if (isSetMemberGroup)
                        {
                            memberGroup = memberGroup<valueType>.Create<serializeBase.member>(serializeAttribute.IsAttribute, serializeAttribute.IsBaseTypeAttribute, serializeAttribute.IsInheritAttribute
                                , value => value.CanGet & value.CanSet && (serializeAttribute.IsObject || value.MemberType.SerializeType.Type != typeof(object))
                                    && (serializeAttribute.IsInterface || !value.MemberType.SerializeType.Type.IsInterface), fastCSharp.code.memberFilters.Instance);
                            memberCount = memberIndexGroup<valueType>.MemberCount;
                            sortMembers = memberGroup.Members.sort((left, right) => right.MemberType.SerializeSize - left.MemberType.SerializeSize);
                            memberMapSize = ((memberCount + 31) >> 5) << 2;
                            pointer[] pointers = unmanaged.Get(true, sortMembers.Length * sizeof(int), memberMapSize, memberMapSize);
                            memberSort = pointers[0];
                            isMemberSerializeMap = new fixedMap(pointers[1].Byte);
                            isNullMap = new fixedMap(pointers[2].Byte);
                            int* sortMemberIndex = memberSort.Int;
                            int size = 0;
                            foreach (code.memberInfo member in sortMembers)
                            {
                                int memberIndex = member.MemberIndex;
                                size += member.MemberType.SerializeSize;
                                *sortMemberIndex++ = memberIndex;
                                if (member.MemberType.SerializeType.IsNull) isNullMap.Set(memberIndex);
                                if (member.MemberType.IsMemberSerialize) isMemberSerializeMap.Set(memberIndex);
                                if (member.Filter == code.memberFilters.PublicInstanceField) publicInstanceFieldMemberMap.SetMember(memberIndex);
                                else if (member.Filter == code.memberFilters.NonPublicInstanceField) nonPublicInstanceFieldMemberMap.SetMember(memberIndex);
                                else if (member.Filter == code.memberFilters.PublicInstanceProperty) publicInstancePropertyMemberMap.SetMember(memberIndex);
                                else if (member.Filter == code.memberFilters.NonPublicInstanceProperty) nonPublicInstancePropertyMemberMap.SetMember(memberIndex);
                            }
                            serializeSize = (size + 3) & (int.MaxValue - 3);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 序列化隐式转换类型
        /// </summary>
        public Type SerializeType;
        /// <summary>
        /// 是否检测相同的引用成员
        /// </summary>
        public bool IsReferenceMember = true;
        /// <summary>
        /// 是否支持object序列化
        /// </summary>
        public bool IsObject;
        /// <summary>
        /// 是否支持接口序列化
        /// </summary>
        public bool IsInterface;
        /// <summary>
        /// 是否反射模式
        /// </summary>
        public bool IsReflection;
    }
}
