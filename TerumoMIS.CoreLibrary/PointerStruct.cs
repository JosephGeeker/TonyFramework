//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PointerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  PointerPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 8:54:10
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 指针（因为指针无法静态初始化）
    /// </summary>
    public unsafe struct PointerStruct:IEquatable<PointerStruct>
    {
        /// <summary>
        /// 指针
        /// </summary>
        public void* Data;
        /// <summary>
        /// 字节指针
        /// </summary>
        public byte* Byte
        {
            get { return (byte*)Data; }
        }
        /// <summary>
        /// 字节指针
        /// </summary>
        public sbyte* SByte
        {
            get { return (sbyte*)Data; }
        }
        /// <summary>
        /// 字符指针
        /// </summary>
        public char* Char
        {
            get { return (char*)Data; }
        }
        /// <summary>
        /// 整形指针
        /// </summary>
        public short* Short
        {
            get { return (short*)Data; }
        }
        /// <summary>
        /// 整形指针
        /// </summary>
        public ushort* UShort
        {
            get { return (ushort*)Data; }
        }
        /// <summary>
        /// 整形指针
        /// </summary>
        public int* Int
        {
            get { return (int*)Data; }
        }
        /// <summary>
        /// 整形指针
        /// </summary>
        public uint* UInt
        {
            get { return (uint*)Data; }
        }
        /// <summary>
        /// 整形指针
        /// </summary>
        public long* Long
        {
            get { return (long*)Data; }
        }
        /// <summary>
        /// 整形指针
        /// </summary>
        public ulong* ULong
        {
            get { return (ulong*)Data; }
        }
        /// <summary>
        /// 浮点指针
        /// </summary>
        public float* Float
        {
            get { return (float*)Data; }
        }
        /// <summary>
        /// 双精度浮点指针
        /// </summary>
        public double* Double
        {
            get { return (double*)Data; }
        }
        /// <summary>
        /// 日期指针
        /// </summary>
        public DateTime* DateTime
        {
            get { return (DateTime*)Data; }
        }
        /// <summary>
        /// HASH值
        /// </summary>
        /// <returns>HASH值</returns>
        public override int GetHashCode()
        {
            if (PubPlus.MemoryBits == 64)
                    return (int) ((long) Data >> 3) ^ (int) ((long) Data >> 35);
            return (int)Data >> 2;
        }
        /// <summary>
        /// 指针比较
        /// </summary>
        /// <param name="obj">待比较指针</param>
        /// <returns>指针是否相等</returns>
        public override bool Equals(object obj)
        {
            return Equals((PointerStruct)obj);
        }
        /// <summary>
        /// 指针比较
        /// </summary>
        /// <param name="other">待比较指针</param>
        /// <returns>指针是否相等</returns>
        public bool Equals(PointerStruct other)
        {
            return Data == other.Data;
        }
    }
}
