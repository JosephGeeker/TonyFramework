//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FixedMapStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  FixedMapStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 16:20:10
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Runtime.CompilerServices;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     指针位图
    /// </summary>
    public unsafe struct FixedMapStruct
    {
        /// <summary>
        ///     位图指针
        /// </summary>
        private readonly byte* _map;

        /// <summary>
        ///     指针位图
        /// </summary>
        /// <param name="map">位图指针,不能为null</param>
        public FixedMapStruct(void* map)
        {
            _map = (byte*) map;
        }

        /// <summary>
        ///     指针位图
        /// </summary>
        /// <param name="map">位图指针,不能为null</param>
        public FixedMapStruct(PointerStruct map)
        {
            _map = map.Byte;
        }

        /// <summary>
        ///     指针位图
        /// </summary>
        /// <param name="map">位图指针,不能为null</param>
        /// <param name="length">字节数,大于等于0</param>
        /// <param name="value">初始值</param>
        public FixedMapStruct(void* map, int length, byte value = 0)
        {
            _map = (byte*) map;
            MemoryPlus.Fill(map, value, length);
        }

        /// <summary>
        ///     非安全访问指针位图
        /// </summary>
        public UnSaferStruct Unsafer
        {
            get { return new UnSaferStruct(_map); }
        }

        /// <summary>
        ///     位图指针
        /// </summary>
        public byte* Map
        {
            get { return _map; }
        }

        /// <summary>
        ///     设置占位
        /// </summary>
        /// <param name="bit">位值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int bit)
        {
            _map[bit >> 3] |= (byte) (1 << (bit & 7));
        }

        /// <summary>
        ///     获取占位状态
        /// </summary>
        /// <param name="bit">位值</param>
        /// <returns>是否已占位</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int bit)
        {
            return (_map[bit >> 3] & (1 << (bit & 7))) != 0;
        }

        /// <summary>
        ///     获取占位状态
        /// </summary>
        /// <param name="bit">位值</param>
        /// <returns>是否已占位</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(char bit)
        {
            return (_map[bit >> 3] & (1 << (bit & 7))) != 0;
        }

        /// <summary>
        ///     设置占位段
        /// </summary>
        /// <param name="start">位值</param>
        /// <param name="count">段长</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int start, int count)
        {
            if (start < 0)
            {
                count += start;
                start = 0;
            }
            if (count > 0) MemoryUnsafe.FillBits(_map, start, count);
        }

        /// <summary>
        ///     清除占位段
        /// </summary>
        /// <param name="start">位值</param>
        /// <param name="count">段长</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(int start, int count)
        {
            if (start < 0)
            {
                count += start;
                start = 0;
            }
            if (count > 0) MemoryUnsafe.ClearBits(_map, start, count);
        }

        /// <summary>
        ///     非安全访问指针位图(请自行确保数据可靠性)
        /// </summary>
        public struct UnSaferStruct
        {
            /// <summary>
            ///     位图指针
            /// </summary>
            private readonly byte* _map;

            /// <summary>
            ///     非安全访问指针位图
            /// </summary>
            /// <param name="map">位图指针</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnSaferStruct(byte* map)
            {
                _map = map;
            }

            /// <summary>
            ///     设置占位
            /// </summary>
            /// <param name="bit">位值</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set(int bit)
            {
                _map[bit >> 3] |= (byte) (1 << (bit & 7));
            }

            /// <summary>
            ///     获取占位状态
            /// </summary>
            /// <param name="bit">位值</param>
            /// <returns>是否已占位</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Get(int bit)
            {
                return (_map[bit >> 3] & (byte) (1 << (bit & 7))) != 0;
            }

            /// <summary>
            ///     设置占位段
            /// </summary>
            /// <param name="start">位值,大于等于0</param>
            /// <param name="count">段长,大于等于0</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set(int start, int count)
            {
                MemoryUnsafe.FillBits(_map, start, count);
            }

            /// <summary>
            ///     清除占位段
            /// </summary>
            /// <param name="start">位值,大于等于0</param>
            /// <param name="count">段长,大于等于0</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear(int start, int count)
            {
                MemoryUnsafe.ClearBits(_map, start, count);
            }
        }
    }

    /// <summary>
    ///     指针位图
    /// </summary>
    /// <typeparam name="TEnumType">枚举类型</typeparam>
    public unsafe struct FixedMapStruct<TEnumType> where TEnumType : IConvertible
    {
        /// <summary>
        ///     位图指针
        /// </summary>
        private readonly byte* _map;

        /// <summary>
        ///     最大值
        /// </summary>
        private readonly uint _size;

        ///// <summary>
        ///// 指针位图(非托管内存，必须自行释放)
        ///// </summary>
        //public fixedMap()
        //{
        //    size = (uint)fastCSharp.Enum.GetMaxValue<enumType>(-1) + 1;
        //    map = size != 0 ? unmanaged.Get(((int)size + 7) >> 3, true) : null;
        //}
        /// <summary>
        ///     指针位图
        /// </summary>
        /// <param name="map">枚举位图</param>
        /// <param name="size">最大值</param>
        public FixedMapStruct(byte* map, uint size)
        {
            if (map == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            _map = map;
            _size = size;
        }

        /// <summary>
        ///     非安全访问指针位图
        /// </summary>
        public FixedMapStruct.UnSaferStruct Unsafer
        {
            get { return new FixedMapStruct.UnSaferStruct(_map); }
        }

        /// <summary>
        ///     位图指针
        /// </summary>
        public byte* Map
        {
            get { return _map; }
        }

        /// <summary>
        ///     设置占位
        /// </summary>
        /// <param name="bit">位值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int bit)
        {
            if ((uint) bit < _size) _map[bit >> 3] |= (byte) (1 << (bit & 7));
        }

        /// <summary>
        ///     设置占位
        /// </summary>
        /// <param name="value">位值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TEnumType value)
        {
            Set(value.ToInt32(null));
        }

        /// <summary>
        ///     清除占位
        /// </summary>
        /// <param name="bit">位值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(int bit)
        {
            if ((uint) bit < _size) _map[bit >> 3] &= (byte) (0xff - (1 << (bit & 7)));
        }

        /// <summary>
        ///     设置占位
        /// </summary>
        /// <param name="value">位值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(TEnumType value)
        {
            Clear(value.ToInt32(null));
        }

        /// <summary>
        ///     获取占位状态
        /// </summary>
        /// <param name="bit">位值</param>
        /// <returns>是否已占位</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int bit)
        {
            return (uint) bit < _size && (_map[bit >> 3] & (byte) (1 << (bit & 7))) != 0;
        }

        /// <summary>
        ///     获取占位状态
        /// </summary>
        /// <returns>是否已占位</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(TEnumType value)
        {
            return Get(value.ToInt32(null));
        }
    }
}