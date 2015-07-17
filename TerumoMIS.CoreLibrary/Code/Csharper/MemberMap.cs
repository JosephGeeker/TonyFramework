//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberMap
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  MemberMap
//	User name:  C1400008
//	Location Time: 2015/7/16 11:41:17
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
    /// 成员位图接口
    /// </summary>
    public unsafe interface IMemberMap
    {
        /// <summary>
        /// 是否默认全部成员有效
        /// </summary>
        bool IsDefault { get; }
        /// <summary>
        /// 序列化字节长度
        /// </summary>
        int SerializeSize { get; }
        /// <summary>
        /// 设置成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        void SetMember(int memberIndex);
        /// <summary>
        /// 清除成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        void ClearMember(int memberIndex);
        /// <summary>
        /// 判断成员索引是否有效
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        /// <returns>成员索引是否有效</returns>
        bool IsMember(int memberIndex);
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="stream">数据流</param>
        void Serialize(unmanagedStream stream);
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="stream">数据流</param>
        void Serialize(Stream stream);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>结束位置</returns>
        byte* DeSerialize(byte* data);
        /// <summary>
        /// 获取成员名称
        /// </summary>
        /// <param name="index">成员索引</param>
        /// <returns>成员名称</returns>
        string GetName(int index);
        /// <summary>
        /// 获取成员数量
        /// </summary>
        /// <returns>成员数量</returns>
        int GetMemberCount();
        /// <summary>
        /// 获取成员类型
        /// </summary>
        /// <param name="index">成员索引</param>
        /// <returns>成员类型</returns>
        Type GetType(int index);
        /// <summary>
        /// 位图入内存池
        /// </summary>
        void PushPool();
    }
    /// <summary>
    /// 成员位图接口
    /// </summary>
    /// <typeparam name="memberType">成员位图类型</typeparam>
    public interface IMemberMap<memberType> : IMemberMap
    {
        /// <summary>
        /// 成员位图
        /// </summary>
        /// <returns>成员位图</returns>
        memberType Copy();
        /// <summary>
        /// 成员交集运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        void And(memberType memberMap);
        /// <summary>
        /// 成员异或运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        void Xor(memberType memberMap);
        /// <summary>
        /// 成员并集运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        void Or(memberType memberMap);
    }
    /// <summary>
    /// 成员位图代码生成自定义属性
    /// </summary>
    public sealed partial class memberMap : memberFilter.instance
    {
        /// <summary>
        /// 空成员位图
        /// </summary>
        public unsafe struct NullMemberMap : IMemberMap
        {
            /// <summary>
            /// 序列化字节长度
            /// </summary>
            private int serializeSize;
            /// <summary>
            /// 空成员位图
            /// </summary>
            /// <param name="memberCount">成员数量</param>
            public NullMemberMap(int memberCount)
            {
                serializeSize = ((memberCount + 31) >> 5) << 2;
            }
            /// <summary>
            /// 是否默认全部成员有效
            /// </summary>
            public bool IsDefault { get { return true; } }
            /// <summary>
            /// 序列化字节长度
            /// </summary>
            public int SerializeSize { get { return serializeSize; } }
            /// <summary>
            /// 设置成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            public void SetMember(int memberIndex) { }
            /// <summary>
            /// 清除成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            public void ClearMember(int memberIndex) { }
            /// <summary>
            /// 判断成员索引是否有效
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            /// <returns>成员索引是否有效</returns>
            public bool IsMember(int memberIndex) { return true; }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            public void Serialize(unmanagedStream stream) { }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            public void Serialize(Stream stream) { }
            /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="data">数据</param>
            /// <returns>结束位置</returns>
            public byte* DeSerialize(byte* data) { return data; }
            /// <summary>
            /// 获取成员名称
            /// </summary>
            /// <param name="index">成员索引</param>
            /// <returns>成员名称</returns>
            public string GetName(int index) { return null; }
            /// <summary>
            /// 获取成员数量
            /// </summary>
            /// <returns>成员数量</returns>
            public int GetMemberCount() { return 0; }
            /// <summary>
            /// 获取成员类型
            /// </summary>
            /// <param name="index">成员索引</param>
            /// <returns>成员类型</returns>
            public Type GetType(int index) { return null; }
            /// <summary>
            /// 位图入内存池
            /// </summary>
            public void PushPool() { }
        }
        /// <summary>
        /// 成员设置获取器
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        internal struct memberGetter<valueType>
        {
            /// <summary>
            /// 数据对象
            /// </summary>
            private valueType value;
            /// <summary>
            /// 获取对象成员的委托集合
            /// </summary>
            private Func<valueType, object>[] getValues;
            /// <summary>
            /// 成员设置获取器
            /// </summary>
            /// <param name="value">数据对象</param>
            /// <param name="getValues">获取对象成员的委托集合</param>
            public memberGetter(valueType value, Func<valueType, object>[] getValues)
            {
                this.value = value;
                this.getValues = getValues;
            }
            /// <summary>
            /// 获取成员值
            /// </summary>
            /// <param name="index">成员索引</param>
            /// <returns>成员值</returns>
            public object this[int index]
            {
                get { return getValues[index](value); }
            }
        }
        /// <summary>
        /// 序列化缓冲区
        /// </summary>
        internal static readonly byte[] SerializeBuffer = new byte[sizeof(ulong)];
        /// <summary>
        /// 序列化缓冲区访问锁
        /// </summary>
        internal static int SerializeBufferLock;
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="map">成员位图</param>
        /// <param name="serializeSize">序列化字节长度</param>
        /// <param name="stream">数据流</param>
        public static unsafe void Serialize(byte[] map, int serializeSize, unmanagedStream stream)
        {
            if (map == null) stream.Write(-serializeSize);
            else
            {
                stream.Write(serializeSize);
                stream.Write(map, 0, serializeSize);
            }
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="map">成员位图</param>
        /// <param name="serializeSize">序列化字节长度</param>
        /// <param name="stream">数据流</param>
        public unsafe static void Serialize(byte[] map, int serializeSize, Stream stream)
        {
            fixed (byte* bufferFixed = SerializeBuffer)
            {
                if (map == null)
                {
                    interlocked.CompareSetSleep0(ref SerializeBufferLock);
                    try
                    {
                        *(int*)bufferFixed = -serializeSize;
                        stream.Write(SerializeBuffer, 0, sizeof(int));
                    }
                    finally { SerializeBufferLock = 0; }
                }
                else
                {
                    interlocked.CompareSetSleep0(ref SerializeBufferLock);
                    try
                    {
                        *(int*)bufferFixed = serializeSize;
                        stream.Write(SerializeBuffer, 0, sizeof(int));
                    }
                    finally { SerializeBufferLock = 0; }
                    stream.Write(map, 0, serializeSize);
                }
            }
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="data">数据流</param>
        /// <param name="memoryPool">内存池</param>
        /// <param name="map">成员位图</param>
        /// <returns>数据流结束位置,失败返回null</returns>
        public static unsafe byte* DeSerialize(byte* data, memoryPool memoryPool, ref byte[] map)
        {
            int serializeSize = memoryPool.Size;
            if (*(int*)data == -serializeSize)
            {
                map = null;
                return data + sizeof(int);
            }
            if (*(int*)data == serializeSize)
            {
                if (map == null) map = memoryPool.Get();
                fixed (byte* mapFixed = map) fastCSharp.unsafer.memory.Copy(data += sizeof(int), mapFixed, serializeSize);
                return data + serializeSize;
            }
            log.Error.Add("成员位图序列化字节长度错误 " + (*(int*)data).toString() + "<>" + serializeSize.toString(), true, false);
            return null;
        }
        /// <summary>
        /// 成员交集运算,忽略默认成员
        /// </summary>
        /// <param name="map">成员位图</param>
        /// <param name="andMap">成员位图</param>
        /// <returns>成员位图</returns>
        public static unsafe byte[] And(byte[] map, byte[] andMap)
        {
            if (map != null && andMap != null)
            {
                fixed (byte* mapFixed = map, andMapFixed = andMap)
                {
                    for (byte* write = mapFixed, end = mapFixed + map.Length, read = andMapFixed; write != end; write += sizeof(uint), read += sizeof(uint))
                    {
                        *(uint*)write &= *(uint*)read;
                    }
                }
                return map;
            }
            return null;
        }
        /// <summary>
        /// 成员异或运算,忽略默认成员
        /// </summary>
        /// <param name="map">成员位图</param>
        /// <param name="xorMap">成员位图</param>
        /// <returns>成员位图</returns>
        public static unsafe byte[] Xor(byte[] map, byte[] xorMap)
        {
            if (map != null && xorMap != null)
            {
                fixed (byte* mapFixed = map, xorMapFixed = xorMap)
                {
                    for (byte* write = mapFixed, end = mapFixed + map.Length, read = xorMapFixed; write != end; write += sizeof(uint), read += sizeof(uint))
                    {
                        *(uint*)write ^= *(uint*)read;
                    }
                }
                return map;
            }
            return null;
        }
        /// <summary>
        /// 成员并集运算,忽略默认成员
        /// </summary>
        /// <param name="map">成员位图</param>
        /// <param name="orMap">成员位图</param>
        /// <returns>成员位图</returns>
        public static unsafe byte[] Or(byte[] map, byte[] orMap)
        {
            if (map != null && orMap != null)
            {
                fixed (byte* mapFixed = map, orMapFixed = orMap)
                {
                    for (byte* write = mapFixed, end = mapFixed + map.Length, read = orMapFixed; write != end; write += sizeof(uint), read += sizeof(uint))
                    {
                        *(uint*)write |= *(uint*)read;
                    }
                }
                return map;
            }
            return null;
        }
        /// <summary>
        /// 获取内存池
        /// </summary>
        /// <param name="serializeSize">序列化字节长度</param>
        /// <returns>内存池</returns>
        public static memoryPool GetMemoryPool(int serializeSize)
        {
            return memoryPool.GetPool(serializeSize);
        }
        /// <summary>
        /// 获取位图并初始化
        /// </summary>
        /// <param name="memoryPool">内存池</param>
        /// <returns>位图</returns>
        public static unsafe byte[] GetMap(memoryPool memoryPool)
        {
            bool isNew;
            byte[] map = memoryPool.Get(out isNew);
            if (!isNew) Array.Clear(map, 0, map.Length);
            return map;
        }
        /// <summary>
        /// 复制位图并初始化
        /// </summary>
        /// <param name="map">位图</param>
        /// <param name="memoryPool">内存池</param>
        /// <returns>位图</returns>
        public static unsafe byte[] CopyMap(byte[] map, memoryPool memoryPool)
        {
            if (map == null) return null;
            if (map.Length == memoryPool.Size)
            {
                byte[] newMap = memoryPool.Get();
                Buffer.BlockCopy(map, 0, newMap, 0, map.Length);
                return newMap;
            }
            log.Error.Throw(log.exceptionType.IndexOutOfRange);
            return null;
        }
    }
    /// <summary>
    /// 成员位图(反射模式)
    /// </summary>
    /// <typeparam name="valueType">对象类型</typeparam>
    public struct memberMap<valueType> : IMemberMap<memberMap<valueType>>
    {
        /// <summary>
        /// 内存池
        /// </summary>
        private static readonly fastCSharp.memoryPool memoryPool;
        /// <summary>
        /// 成员信息集合
        /// </summary>
        private readonly static memberIndex[] memberIndexs;
        /// <summary>
        /// 成员名称索引查找表
        /// </summary>
        internal readonly static Dictionary<subString, int> NameIndexs;
        /// <summary>
        /// 序列化字节长度
        /// </summary>
        private readonly static int serializeSize;
        /// <summary>
        /// 所有成员数量
        /// </summary>
        public static readonly int MemberCount;
        /// <summary>
        /// 复制缓冲流
        /// </summary>
        private static readonly unmanagedStream copyStream;
        /// <summary>
        /// 复制缓冲流访问锁
        /// </summary>
        private static int copyStreamLock;
        /// <summary>
        /// 成员位图
        /// </summary>
        private ulong map64;
        /// <summary>
        /// 成员位图
        /// </summary>
        private byte[] map;
        /// <summary>
        /// 成员位图
        /// </summary>
        /// <param name="members">成员名称集合</param>
        public memberMap(params string[] members)
        {
            map64 = 0;
            map = null;
            if (members.Length != 0)
            {
                if (serializeSize <= 8)
                {
                    foreach (string member in members) map64 |= 1UL << getIndex(member);
                }
                else
                {
                    map = memberMap.GetMap(memoryPool);
                    foreach (string member in members)
                    {
                        int index = getIndex(member);
                        map[index >> 3] |= (byte)(1 << (index & 7));
                    }
                }
            }
        }
        /// <summary>
        /// 获取成员索引
        /// </summary>
        /// <param name="member">成员名称</param>
        /// <returns>成员索引</returns>
        private int getIndex(subString member)
        {
            int index;
            if (NameIndexs.TryGetValue(member, out index)) return index;
            log.Error.Throw(typeof(valueType).fullName() + " 未找到成员 " + member, true, true);
            return -1;
        }
        /// <summary>
        /// 成员位图
        /// </summary>
        /// <returns>成员位图</returns>
        public unsafe memberMap<valueType> Copy()
        {
            if (serializeSize <= 8) return new memberMap<valueType> { map64 = map64 };
            return new memberMap<valueType> { map = memberMap.CopyMap(map, memoryPool) };
        }
        /// <summary>
        /// 是否默认全部成员有效
        /// </summary>
        public bool IsDefault
        {
            get
            {
                return serializeSize <= 8 ? map64 == 0 : map == null;
            }
        }
        /// <summary>
        /// 序列化字节长度
        /// </summary>
        public int SerializeSize
        {
            get
            {
                return serializeSize;
            }
        }
        /// <summary>
        /// 设置成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        public void SetMember(int memberIndex)
        {
            if (serializeSize <= 8) map64 |= 1UL << memberIndex;
            else
            {
                if (map == null) map = memberMap.GetMap(memoryPool);
                map[memberIndex >> 3] |= (byte)(1 << (memberIndex & 7));
            }
        }
        /// <summary>
        /// 设置成员,忽略默认成员
        /// </summary>
        /// <param name="member">成员名称</param>
        public void SetMember(string member)
        {
            SetMember(getIndex(member));
        }
        /// <summary>
        /// 清除成员,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员名称</param>
        public void ClearMember(string member)
        {
            ClearMember(getIndex(member));
        }
        /// <summary>
        /// 清除成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        public void ClearMember(int memberIndex)
        {
            if (serializeSize <= 8) map64 &= (1UL << memberIndex) ^ ulong.MaxValue;
            else if (map != null) map[memberIndex >> 3] &= (byte)(byte.MaxValue ^ (1 << (memberIndex & 7)));
        }
        /// <summary>
        /// 成员交集运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public unsafe void And(memberMap<valueType> memberMap)
        {
            if (serializeSize <= 8)
            {
                if (map64 != 0)
                {
                    if (memberMap.map64 == 0) map64 = 0;
                    else map64 &= memberMap.map64;
                }
            }
            else map = cSharp.memberMap.And(map, memberMap.map);
        }
        /// <summary>
        /// 成员异或运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public unsafe void Xor(memberMap<valueType> memberMap)
        {
            if (serializeSize <= 8)
            {
                if (map64 != 0)
                {
                    if (memberMap.map64 == 0) map64 = 0;
                    else map64 ^= memberMap.map64;
                }
            }
            else map = cSharp.memberMap.Xor(map, memberMap.map);
        }
        /// <summary>
        /// 成员并集运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public unsafe void Or(memberMap<valueType> memberMap)
        {
            if (serializeSize <= 8)
            {
                if (map64 != 0)
                {
                    if (memberMap.map64 == 0) map64 = 0;
                    else map64 |= memberMap.map64;
                }
            }
            else map = cSharp.memberMap.Or(map, memberMap.map);
        }
        /// <summary>
        /// 判断成员索引是否有效
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        /// <returns>成员索引是否有效</returns>
        public bool IsMember(int memberIndex)
        {
            if (serializeSize <= 8) return map64 == 0 || (map64 & (1UL << memberIndex)) != 0;
            return map == null || (map[memberIndex >> 3] & (1 << (memberIndex & 7))) != 0;
        }
        /// <summary>
        /// 判断成员是否有效
        /// </summary>
        /// <param name="member">成员名称</param>
        /// <returns>成员是否有效</returns>
        public bool IsMember(string member)
        {
            return IsMember(getIndex(member));
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="stream">数据流</param>
        public unsafe void Serialize(unmanagedStream stream)
        {
            if (serializeSize == 4) stream.Write((uint)map64);
            else if (serializeSize == 8) stream.Write(map64);
            else fastCSharp.code.cSharp.memberMap.Serialize(map, SerializeSize, stream);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="stream">数据流</param>
        public unsafe void Serialize(System.IO.Stream stream)
        {
            if (serializeSize <= 8)
            {
                fixed (byte* bufferFixed = memberMap.SerializeBuffer)
                {
                    interlocked.CompareSetSleep0(ref memberMap.SerializeBufferLock);
                    try
                    {
                        *(ulong*)bufferFixed = map64;
                        stream.Write(memberMap.SerializeBuffer, 0, serializeSize);
                    }
                    finally { memberMap.SerializeBufferLock = 0; }
                }
            }
            else fastCSharp.code.cSharp.memberMap.Serialize(map, SerializeSize, stream);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>结束位置</returns>
        public unsafe byte* DeSerialize(byte* data)
        {
            if (serializeSize == 4)
            {
                map64 = *(uint*)data;
                return data + 4;
            }
            if (serializeSize == 8)
            {
                map64 = *(ulong*)data;
                return data + 8;
            }
            return fastCSharp.code.cSharp.memberMap.DeSerialize(data, memoryPool, ref map);
        }
        /// <summary>
        /// 复制成员位图
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public unsafe void CopyFrom(IMemberMap memberMap)
        {
            if (memberMap.IsDefault || memberMap.SerializeSize != serializeSize)
            {
                this.map = null;
                map64 = 0;
            }
            else
            {
                if (serializeSize <= 8)
                {
                    ulong map = 0;
                    interlocked.CompareSetSleep0(ref copyStreamLock);
                    try
                    {
                        copyStream.Reset((byte*)&map, 8);
                        memberMap.Serialize(copyStream);
                    }
                    finally { copyStreamLock = 0; }
                    map64 = map;
                }
                else
                {
                    if (this.map == null) this.map = fastCSharp.code.cSharp.memberMap.GetMap(memoryPool);
                    fixed (byte* mapFxied = this.map)
                    {
                        interlocked.CompareSetSleep0(ref copyStreamLock);
                        try
                        {
                            copyStream.Clear();
                            memberMap.Serialize(copyStream);
                            unsafer.memory.Copy(copyStream.Data + sizeof(int), mapFxied, serializeSize);
                        }
                        finally { copyStreamLock = 0; }
                    }
                }
            }
        }
        /// <summary>
        /// 获取成员名称
        /// </summary>
        /// <param name="index">成员索引</param>
        /// <returns>成员名称</returns>
        public string GetName(int index)
        {
            return memberIndexs[index].Member.Name;
        }
        /// <summary>
        /// 获取成员数量
        /// </summary>
        /// <returns>成员数量</returns>
        public int GetMemberCount()
        {
            return MemberCount;
        }
        /// <summary>
        /// 获取成员类型
        /// </summary>
        /// <param name="index">成员索引</param>
        /// <returns>成员类型</returns>
        public Type GetType(int index)
        {
            return memberIndexs[index].Type;
        }
        /// <summary>
        /// 位图入内存池
        /// </summary>
        public void PushPool()
        {
            if (memoryPool != null) memoryPool.Push(ref map);
        }
        /// <summary>
        /// 获取成员名称
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        /// <returns>成员名称</returns>
        public static string GetMemberName(int memberIndex)
        {
            return memberIndexs[memberIndex].Member.Name;
        }
        /// <summary>
        /// 获取成员类型
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        /// <returns>成员类型</returns>
        public static Type GetMemberType(int memberIndex)
        {
            return memberIndexs[memberIndex].Type;
        }
        unsafe static memberMap()
        {
            memberIndexs = memberIndexGroup<valueType>.GetAllMembers();
            NameIndexs = dictionary.CreateSubString<int>();
            foreach (memberIndex memberIndex in memberIndexs) NameIndexs.Add(memberIndex.Member.Name, memberIndex.MemberIndex);
            MemberCount = memberIndexs.Length;
            serializeSize = (((memberIndexs.Length - 1) >> 5) + 1) << 2;
            if (serializeSize <= 8)
            {
                byte buffer;
                copyStream = new unmanagedStream(&buffer, 1);
            }
            else
            {
                copyStream = new unmanagedStream(serializeSize + sizeof(int));
                memoryPool = fastCSharp.code.cSharp.memberMap.GetMemoryPool(serializeSize);
            }
        }
    }
}
