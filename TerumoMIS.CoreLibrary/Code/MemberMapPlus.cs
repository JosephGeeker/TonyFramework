//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemberMapPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  MemberMapPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 15:37:01
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Emit;
using TerumoMIS.CoreLibrary.Threading;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 成员位图
    /// </summary>
    public unsafe abstract class MemberMapPlus:IEquatable<MemberMapPlus>,IDisposable
    {
        /// <summary>
        /// 成员位图类型信息
        /// </summary>
        internal sealed class TypePlus
        {
            /// <summary>
            /// 成员位图内存池
            /// </summary>
            internal PointPlus.PoolPlus Pool;
            /// <summary>
            /// 名称索引查找数据
            /// </summary>
            public PointerStruct NameIndexSearcher { get; private set; }
            /// <summary>
            /// 成员数量
            /// </summary>
            public int MemberCount { get; private set; }
            /// <summary>
            /// 字段成员数量
            /// </summary>
            public int FieldCount { get; private set; }
            /// <summary>
            /// 成员位图字节数量
            /// </summary>
            public int MemberMapSize { get; private set; }
            /// <summary>
            /// 字段成员位图序列化字节数量
            /// </summary>
            public int FieldSerializeSize { get; private set; }
            /// <summary>
            /// 成员位图类型信息
            /// </summary>
            /// <param name="members">成员索引集合</param>
            /// <param name="fieldCount">字段成员数量</param>
            public TypePlus(fastCSharp.code.memberIndex[] members, int fieldCount)
            {
                FieldCount = fieldCount;
                if ((MemberCount = members.Length) < 64) MemberMapSize = MemberCount < 32 ? 4 : 8;
                else MemberMapSize = ((MemberCount + 63) >> 6) << 3;
                NameIndexSearcher = fastCSharp.stateSearcher.chars.Create(members.getArray(value => value.Member.Name));
                if (MemberCount >= 64)
                {
                    Pool = PointPlus.PoolPlus.GetPool(MemberMapSize);
                    FieldSerializeSize = ((fieldCount + 31) >> 5) << 2;
                }
            }
            /// <summary>
            /// 获取成员索引
            /// </summary>
            /// <param name="member">成员名称</param>
            /// <returns>成员索引,失败返回-1</returns>
            public int GetMemberIndex(string name)
            {
                return name != null ? new JsonParserPlus.stateSearche.chars(NameIndexSearcher).Search(name) : -1;
            }
        }
        /// <summary>
        /// 成员位图
        /// </summary>
        internal sealed class ValuePlus : MemberMapPlus
        {
            /// <summary>
            /// 成员位图
            /// </summary>
            private ulong _map;
            /// <summary>
            /// 是否默认全部成员有效
            /// </summary>
            public override bool IsDefault
            {
                get
                {
                    return _map == 0;
                }
            }
            /// <summary>
            /// 成员位图
            /// </summary>
            /// <param name="type">成员位图类型信息</param>
            public ValuePlus(TypePlus type) : base(type) { }
            /// <summary>
            /// 比较是否相等
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public override bool Equals(MemberMapPlus other)
            {
                var value = other as ValuePlus;
                return value != null && _map == value._map;
            }
            /// <summary>
            /// 添加所有成员
            /// </summary>
            internal override void Full()
            {
                _map = ulong.MaxValue;
            }
            /// <summary>
            /// 清空所有成员
            /// </summary>
            internal override void Empty()
            {
                _map = 0x8000000000000000UL;
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="stream"></param>
            internal override void FieldSerialize(UnmanagedStreamPlus stream)
            {
                if (_map == 0) stream.Write(0);
                else
                {
                    stream.PrepLength(sizeof(int) + sizeof(ulong));
                    byte* data = stream.CurrentData;
                    *(int*)data = Type.FieldCount;
                    *(ulong*)(data + sizeof(int)) = _map;
                    stream.Unsafer.AddLength(sizeof(int) + sizeof(ulong));
                    stream.PrepLength();
                }
            }
            /// <summary>
            /// 字段成员反序列化
            /// </summary>
            /// <param name="deSerializer"></param>
            internal override void FieldDeSerialize(BinaryDeSerializerPlus deSerializer)
            {
                _map = deSerializer.DeSerializeMemberMap(Type.FieldCount);
            }
            /// <summary>
            /// 设置成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            internal override void SetMember(int memberIndex)
            {
                _map |= (1UL << memberIndex) | 0x8000000000000000UL;
            }
            /// <summary>
            /// 清除所有成员
            /// </summary>
            internal override void Clear()
            {
                _map = 0;
            }
            /// <summary>
            /// 清除成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            internal override void ClearMember(int memberIndex)
            {
                _map &= (1UL << memberIndex) ^ ulong.MaxValue;
            }
            /// <summary>
            /// 判断成员索引是否有效
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            /// <returns>成员索引是否有效</returns>
            internal override bool IsMember(int memberIndex)
            {
                return _map == 0 || (_map & (1UL << memberIndex)) != 0;
            }
            /// <summary>
            /// 成员位图
            /// </summary>
            /// <returns>成员位图</returns>
            public override MemberMapPlus Copy()
            {
                var value = new ValuePlus(Type);
                value._map = _map;
                return value;
            }
            /// <summary>
            /// 成员交集运算
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void And(MemberMapPlus memberMap)
            {
                if (memberMap == null || memberMap.IsDefault) return;
                if (Type != memberMap.Type) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
                if (_map == 0) _map = ((ValuePlus)memberMap)._map;
                else _map &= ((ValuePlus)memberMap)._map;
            }
            /// <summary>
            /// 成员异或运算,忽略默认成员
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void Xor(MemberMapPlus memberMap)
            {
                if (memberMap == null || memberMap.IsDefault || _map == 0) return;
                if (Type != memberMap.Type) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
                _map ^= ((ValuePlus)memberMap)._map;
                _map |= 0x8000000000000000UL;
            }
            /// <summary>
            /// 成员并集运算,忽略默认成员
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void Or(MemberMapPlus memberMap)
            {
                if (this._map != 0)
                {
                    var map = ((ValuePlus)memberMap)._map;
                    if (map == 0) this._map = 0;
                    else this._map |= map;
                }
            }
        }
        /// <summary>
        /// 指针成员位图
        /// </summary>
        internal sealed class PointPlus : MemberMapPlus
        {
            /// <summary>
            /// 成员位图内存池
            /// </summary>
            internal sealed class PoolPlus
            {
                /// <summary>
                /// 空闲内存地址
                /// </summary>
                private byte* _free;
                /// <summary>
                /// 成员位图字节数量
                /// </summary>
                private int _size;
                /// <summary>
                /// 空闲内存地址访问锁
                /// </summary>
                private int _freeLock;
                /// <summary>
                /// 成员位图内存池
                /// </summary>
                /// <param name="size">成员位图字节数量</param>
                private PoolPlus(int size)
                {
                    _size = size;
                }
                /// <summary>
                /// 获取成员位图
                /// </summary>
                /// <returns>成员位图</returns>
                public byte* Get()
                {
                    byte* value;
                    InterlockedPlus.NoCheckCompareSetSleep0(ref _freeLock);
                    if (_free != null)
                    {
                        value = _free;
                        //free = (byte*)*(ulong*)free;
                        _free = *(byte**)_free;
                        _freeLock = 0;
                        return value;
                    }
                    _freeLock = 0;
                    InterlockedPlus.NoCheckCompareSetSleep0(ref _memoryLock);
                    var size = (int)(_memoryEnd - _memoryStart);
                    if (size >= _size)
                    {
                        value = _memoryStart;
                        _memoryStart += _size;
                        _memoryLock = 0;
                        return value;
                    }
                    _memoryLock = 0;
                    Monitor.Enter(CreateLock);
                    InterlockedPlus.NoCheckCompareSetSleep0(ref _memoryLock);
                    if (((int)(_memoryEnd - _memoryStart)) >= _size)
                    {
                        value = _memoryStart;
                        _memoryStart += _size;
                        _memoryLock = 0;
                        Monitor.Exit(CreateLock);
                        return value;
                    }
                    _memoryLock = 0;
                    try
                    {
                        byte* start = UnmanagedPlus.Get(MemorySize, false).Byte;
                        InterlockedPlus.NoCheckCompareSetSleep0(ref _memoryLock);
                        value = _memoryStart = start;
                        _memoryEnd = start + MemorySize;
                        _memoryStart += _size;
                        _memoryLock = 0;
                    }
                    finally { Monitor.Exit(CreateLock); }
                    Interlocked.Increment(ref _memoryCount);
                    return value;
                }
                /// <summary>
                /// 获取成员位图
                /// </summary>
                /// <returns>成员位图</returns>
                public byte* GetClear()
                {
                    byte* value = Get(), write = value + _size;
                    do
                    {
                        *(ulong*)(write -= sizeof(ulong)) = 0;
                    }
                    while (write != value);
                    return value;
                }
                /// <summary>
                /// 成员位图入池
                /// </summary>
                /// <param name="map">成员位图</param>
                public void Push(byte* map)
                {
                    //*(ulong*)map = (ulong)free;
                    *(byte**)map = _free;
                    _free = map;
                }
                /// <summary>
                /// 获取成员位图内存池
                /// </summary>
                /// <param name="size">成员位图字节数量</param>
                /// <returns></returns>
                public static PoolPlus GetPool(int size)
                {
                    var index = size >> 3;
                    if (index < Pools.Length)
                    {
                        PoolPlus pool = Pools[index];
                        if (pool != null) return pool;
                        InterlockedPlus.NoCheckCompareSetSleep0(ref _poolLock);
                        if ((pool = Pools[index]) == null)
                        {
                            try
                            {
                                Pools[index] = pool = new PoolPlus(size);
                            }
                            finally { _poolLock = 0; }
                            return pool;
                        }
                        _poolLock = 0;
                        return pool;
                    }
                    return null;
                }
                /// <summary>
                /// 成员位图内存池集合
                /// </summary>
                private static readonly PoolPlus[] Pools;
                /// <summary>
                /// 成员位图内存池集合访问锁
                /// </summary>
                private static int _poolLock;
                /// <summary>
                /// 内存申请数量
                /// </summary>
                private static int _memoryCount;
                /// <summary>
                /// 成员位图内存池字节大小
                /// </summary>
                private static readonly int MemorySize = fastCSharp.config.pub.Default.MemberMapPoolSize;
                /// <summary>
                /// 成员位图内存池起始位置
                /// </summary>
                private static byte* _memoryStart;
                /// <summary>
                /// 成员位图内存池结束位置
                /// </summary>
                private static byte* _memoryEnd;
                /// <summary>
                /// 成员位图内存池访问锁
                /// </summary>
                private static int _memoryLock;
                /// <summary>
                /// 成员位图内存池访问锁
                /// </summary>
                private static readonly object CreateLock = new object();
                static PoolPlus()
                {
                    int count = fastCSharp.config.pub.Default.MaxMemberMapCount;
                    if ((count >> 3) >= fastCSharp.config.pub.Default.MemberMapPoolSize) LogPlus.Error.Add("成员位图支持数量过大 " + count);
                    Pools = new PoolPlus[count >> 6];
                }
            }
            /// <summary>
            /// 成员位图
            /// </summary>
            private byte* _map;
            /// <summary>
            /// 是否默认全部成员有效
            /// </summary>
            public override bool IsDefault { get { return _map == null; } }
            /// <summary>
            /// 成员位图
            /// </summary>
            /// <param name="type">成员位图类型信息</param>
            public PointPlus(TypePlus type) : base(type) { }

            /// <summary>
            /// 比较是否相等
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public override bool Equals(MemberMapPlus other)
            {
                var value = other as PointPlus;
                if (value != null)
                {
                    if (_map == null) return value._map == null;
                    if (value._map != null)
                    {
                        byte* write = _map, end = _map + Type.MemberMapSize, read = value._map;
                        var bits = *(ulong*)write ^ *(ulong*)read;
                        while ((write += sizeof(ulong)) != end) bits |= *(ulong*)write ^ *(ulong*)(read += sizeof(ulong));
                        return bits == 0;
                    }
                }
                return false;
            }
            /// <summary>
            /// 添加所有成员
            /// </summary>
            internal override void Full()
            {
                if (_map == null) _map = Type.Pool.Get();
                byte* write = _map, end = _map + Type.MemberMapSize;
                do
                {
                    *(ulong*)write = ulong.MaxValue;
                }
                while ((write += sizeof(ulong)) != end);
            }
            /// <summary>
            /// 清空所有成员
            /// </summary>
            internal override void Empty()
            {
                if (_map == null) _map = Type.Pool.GetClear();
                else
                {
                    byte* write = _map, end = _map + Type.MemberMapSize;
                    do
                    {
                        *(ulong*)write = 0;
                    }
                    while ((write += sizeof(ulong)) != end);
                }
            }
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="stream"></param>
            internal override void FieldSerialize(UnmanagedStreamPlus stream)
            {
                if (_map == null) stream.Write(0);
                else
                {
                    stream.PrepLength(Type.FieldSerializeSize + sizeof(int));
                    byte* data = stream.CurrentData, read = _map;
                    *(int*)data = Type.FieldCount;
                    data += sizeof(int);
                    for (byte* end = _map + (Type.FieldSerializeSize & (int.MaxValue - sizeof(ulong) + 1)); read != end; read += sizeof(ulong), data += sizeof(ulong)) *(ulong*)data = *(ulong*)read;
                    if ((Type.FieldSerializeSize & sizeof(int)) != 0) *(uint*)data = *(uint*)read;
                    stream.Unsafer.AddLength(Type.FieldSerializeSize + sizeof(int));
                    stream.PrepLength();
                }
            }
            /// <summary>
            /// 字段成员反序列化
            /// </summary>
            /// <param name="deSerializer"></param>
            internal override void FieldDeSerialize(BinaryDeSerializerPlus deSerializer)
            {
                if (_map == null) _map = Type.Pool.Get();
                deSerializer.DeSerializeMemberMap(_map, Type.FieldCount, Type.FieldSerializeSize);
            }
            /// <summary>
            /// 设置成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            internal override void SetMember(int memberIndex)
            {
                if (_map == null) _map = Type.Pool.GetClear();
                _map[memberIndex >> 3] |= (byte)(1 << (memberIndex & 7));
            }
            /// <summary>
            /// 清除所有成员
            /// </summary>
            internal override void Clear()
            {
                Dispose();
            }
            /// <summary>
            /// 清除成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            internal override void ClearMember(int memberIndex)
            {
                if (_map != null) _map[memberIndex >> 3] &= (byte)(byte.MaxValue ^ (1 << (memberIndex & 7)));
            }
            /// <summary>
            /// 判断成员索引是否有效
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            /// <returns>成员索引是否有效</returns>
            internal override bool IsMember(int memberIndex)
            {
                return _map == null || (_map[memberIndex >> 3] & (1 << (memberIndex & 7))) != 0;
            }
            /// <summary>
            /// 成员位图
            /// </summary>
            /// <returns>成员位图</returns>
            public override MemberMapPlus Copy()
            {
                var value = new PointPlus(Type);
                if (_map != null) MemoryUnsafe.Copy(_map, value._map = Type.Pool.Get(), Type.MemberMapSize);
                return value;
            }
            /// <summary>
            /// 成员交集运算
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void And(MemberMapPlus memberMap)
            {
                if (memberMap == null || memberMap.IsDefault) return;
                if (Type != memberMap.Type) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
                if (_map == null) MemoryUnsafe.Copy(((PointPlus)memberMap)._map, _map = Type.Pool.Get(), Type.MemberMapSize);
                else
                {
                    byte* write = _map, end = _map + Type.MemberMapSize, read = ((PointPlus)memberMap)._map;
                    *(ulong*)write &= *(ulong*)read;
                    while ((write += sizeof(ulong)) != end) *(ulong*)write &= *(ulong*)(read += sizeof(ulong));
                }
            }
            /// <summary>
            /// 成员异或运算,忽略默认成员
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void Xor(MemberMapPlus memberMap)
            {
                if (memberMap == null || memberMap.IsDefault || _map == null) return;
                if (Type != memberMap.Type) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
                byte* write = _map, end = _map + Type.MemberMapSize, read = ((PointPlus)memberMap)._map;
                *(ulong*)write ^= *(ulong*)read;
                while ((write += sizeof(ulong)) != end) *(ulong*)write ^= *(ulong*)(read += sizeof(ulong));
            }
            /// <summary>
            /// 成员并集运算,忽略默认成员
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void Or(MemberMapPlus memberMap)
            {
                if (_map == null) return;
                if (memberMap == null || memberMap.IsDefault)
                {
                    Type.Pool.Push(_map);
                    _map = null;
                    return;
                }
                if (Type != memberMap.Type) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.ErrorOperation);
                byte* write = _map, end = _map + Type.MemberMapSize, read = ((PointPlus)memberMap)._map;
                *(ulong*)write |= *(ulong*)read;
                while ((write += sizeof(ulong)) != end) *(ulong*)write |= *(ulong*)(read += sizeof(ulong));
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public override void Dispose()
            {
                if (_map != null)
                {
                    Type.Pool.Push(_map);
                    _map = null;
                }
            }
        }
        /// <summary>
        /// 成员索引
        /// </summary>
        public sealed class MemberIndexPlus
        {
            /// <summary>
            /// 成员索引
            /// </summary>
            private int _index;
            /// <summary>
            /// 成员索引
            /// </summary>
            /// <param name="index">成员索引</param>
            internal MemberIndexPlus(int index)
            {
                _index = index;
            }
            /// <summary>
            /// 判断成员索引是否有效
            /// </summary>
            /// <param name="memberMap"></param>
            /// <returns>成员索引是否有效</returns>
            public bool IsMember(MemberMapPlus memberMap)
            {
                return memberMap != null && memberMap.IsMember(_index);
            }
            /// <summary>
            /// 清除成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberMap"></param>
            public void ClearMember(MemberMapPlus memberMap)
            {
                if (memberMap != null) memberMap.ClearMember(_index);
            }
            /// <summary>
            /// 设置成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberMap"></param>
            internal void SetMember(MemberMapPlus memberMap)
            {
                if (memberMap != null) memberMap.SetMember(_index);
            }
        }
        /// <summary>
        /// 成员位图类型信息
        /// </summary>
        internal TypePlus Type { get; private set; }
        /// <summary>
        /// 是否默认全部成员有效
        /// </summary>
        public abstract bool IsDefault { get; }
        /// <summary>
        /// 成员位图
        /// </summary>
        /// <param name="type">成员位图类型信息</param>
        internal MemberMapPlus(TypePlus type)
        {
            Type = type;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose() { }
        /// <summary>
        /// 比较是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool Equals(MemberMapPlus other);
        /// <summary>
        /// 添加所有成员
        /// </summary>
        internal abstract void Full();
        /// <summary>
        /// 清空所有成员
        /// </summary>
        internal abstract void Empty();
        /// <summary>
        /// 字段成员序列化
        /// </summary>
        /// <param name="stream"></param>
        internal abstract void FieldSerialize(UnmanagedStreamPlus stream);
        /// <summary>
        /// 字段成员反序列化
        /// </summary>
        /// <param name="deSerializer"></param>
        internal abstract void FieldDeSerialize(BinaryDeSerializerPlus deSerializer);
        /// <summary>
        /// 设置成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        internal abstract void SetMember(int memberIndex);
        /// <summary>
        /// 设置成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <returns>是否成功</returns>
        internal bool SetMember(string memberName)
        {
            int index = Type.GetMemberIndex(memberName);
            if (index >= 0)
            {
                SetMember(index);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置成员索引,忽略默认成员
        /// </summary>
        /// <param name="member">成员</param>
        /// <returns>是否成功</returns>
        internal bool SetMember(LambdaExpression member)
        {
            return SetMember(GetFieldName(member));
        }
        /// <summary>
        /// 清除所有成员
        /// </summary>
        internal abstract void Clear();
        /// <summary>
        /// 清除成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        internal abstract void ClearMember(int memberIndex);
        ///// <summary>
        ///// 清除成员索引,忽略默认成员
        ///// </summary>
        ///// <param name="memberName">成员名称</param>
        //public void ClearMember(string memberName)
        //{
        //    int index = Type.GetMemberIndex(memberName);
        //    if (index >= 0) ClearMember(index);
        //}
        /// <summary>
        /// 判断成员索引是否有效
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        /// <returns>成员索引是否有效</returns>
        internal abstract bool IsMember(int memberIndex);
        /// <summary>
        /// 判断成员位图是否匹配成员索引
        /// </summary>
        internal static readonly MethodInfo IsMemberMethod = typeof(MemberMapPlus).GetMethod("IsMember", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
        ///// <summary>
        ///// 判断成员索引是否有效
        ///// </summary>
        ///// <param name="memberName">成员名称</param>
        ///// <returns>成员索引是否有效</returns>
        //public bool IsMember(string memberName)
        //{
        //    int index = Type.GetMemberIndex(memberName);
        //    return index >= 0 && IsMember(index);
        //}
        /// <summary>
        /// 成员位图
        /// </summary>
        /// <returns>成员位图</returns>
        public abstract MemberMapPlus Copy();
        /// <summary>
        /// 成员交集运算
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public abstract void And(MemberMapPlus memberMap);
        /// <summary>
        /// 成员异或运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public abstract void Xor(MemberMapPlus memberMap);
        /// <summary>
        /// 成员并集运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public abstract void Or(MemberMapPlus memberMap);
        /// <summary>
        /// 获取成员名称
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static string GetFieldName(LambdaExpression member)
        {
            Expression expression = member.Body;
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                FieldInfo field = ((MemberExpression)expression).Member as FieldInfo;
                if (field != null) return field.Name;
            }
            return null;
        }
    }
    /// <summary>
    /// 成员位图
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public static class MemberMapPlus<TValueType>
    {
        /// <summary>
        /// 创建成员位图
        /// </summary>
        public struct BuilderStruct
        {
            /// <summary>
            /// 成员位图
            /// </summary>
            private MemberMapPlus _memberMap;
            /// <summary>
            /// 成员位图
            /// </summary>
            /// <param name="value">创建成员位图</param>
            /// <returns>成员位图</returns>
            public static implicit operator MemberMapPlus(BuilderStruct value)
            {
                return value._memberMap;
            }
            /// <summary>
            /// 创建成员位图
            /// </summary>
            /// <param name="isNew">是否创建成员</param>
            internal BuilderStruct(bool isNew)
            {
                _memberMap = isNew ? New() : null;
            }
            /// <summary>
            /// 添加成员
            /// </summary>
            /// <typeparam name="TReturnType"></typeparam>
            /// <param name="member"></param>
            /// <returns></returns>
            public BuilderStruct Append<TReturnType>(Expression<Func<TValueType, TReturnType>> member)
            {
                if (_memberMap != null) _memberMap.SetMember(member);
                return this;
            }
        }
        /// <summary>
        /// 成员位图类型信息
        /// </summary>
        internal static readonly MemberMapPlus.TypePlus Type = new MemberMapPlus.TypePlus(MemberIndexGroupPlus<TValueType>.GetAllMembers(), MemberIndexGroupPlus<TValueType>.FieldCount);
        /// <summary>
        /// 默认成员位图
        /// </summary>
        internal static readonly MemberMapPlus Default = New();
        /// <summary>
        /// 默认成员位图
        /// </summary>
        /// <returns></returns>
        public static MemberMapPlus New()
        {
            return Type.MemberCount < 64 ? (MemberMapPlus)new MemberMapPlus.ValuePlus(Type) : new MemberMapPlus.PointPlus(Type);
        }
        /// <summary>
        /// 所有成员位图
        /// </summary>
        /// <returns></returns>
        public static MemberMapPlus Full()
        {
            var value = Type.MemberCount < 64 ? (MemberMapPlus)new MemberMapPlus.ValuePlus(Type) : new MemberMapPlus.PointPlus(Type);
            value.Full();
            return value;
        }
        /// <summary>
        /// 空成员位图
        /// </summary>
        /// <returns></returns>
        public static MemberMapPlus Empty()
        {
            var value = Type.MemberCount < 64 ? (MemberMapPlus)new MemberMapPlus.ValuePlus(Type) : new MemberMapPlus.PointPlus(Type);
            value.Empty();
            return value;
        }
        /// <summary>
        /// 创建成员索引
        /// </summary>
        /// <typeparam name="TReturnType"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static MemberMapPlus.MemberIndexPlus CreateMemberIndex<TReturnType>(Expression<Func<TValueType, TReturnType>> member)
        {
            var index = Type.GetMemberIndex(MemberMapPlus.GetFieldName(member));
            return index >= 0 ? new MemberMapPlus.MemberIndexPlus(index) : null;
        }

        /// <summary>
        /// 判断成员位图类型是否匹配
        /// </summary>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        public static bool IsType(MemberMapPlus memberMap)
        {
            return memberMap.Type == Type;
        }
        /// <summary>
        /// 成员位图
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        internal static MemberMapPlus New(params string[] names)
        {
            if (Type.MemberCount < 64)
            {
                var value = new MemberMapPlus.ValuePlus(Type);
                foreach (var name in names) value.SetMember(name);
                return value;
            }
            var point = new MemberMapPlus.PointPlus(Type);
            foreach (var name in names) point.SetMember(name);
            return point;
        }
        /// <summary>
        /// 成员位图
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public static MemberMapPlus New(params LambdaExpression[] members)
        {
            if (Type.MemberCount < 64)
            {
                var value = new MemberMapPlus.ValuePlus(Type);
                foreach (var member in members) value.SetMember(member);
                return value;
            }
            var point = new MemberMapPlus.PointPlus(Type);
            foreach (var member in members) point.SetMember(member);
            return point;
        }
    }
}
