//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: GuidPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  GuidPlus
//	User name:  C1400008
//	Location Time: 2015/7/14 16:53:51
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Runtime.InteropServices;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// Guid联合体
    /// </summary>
    [StructLayout(LayoutKind.Explicit,Pack = 1)]
    public struct GuidStruct
    {
        [FieldOffset(0)]
        public Guid Value;
        [FieldOffset(0)]
        internal byte Byte0;
        [FieldOffset(1)]
        internal byte Byte1;
        [FieldOffset(2)]
        internal byte Byte2;
        [FieldOffset(3)]
        internal byte Byte3;
        [FieldOffset(4)]
        internal byte Byte4;
        [FieldOffset(5)]
        internal byte Byte5;
        [FieldOffset(4)]
        internal ushort Byte45;
        [FieldOffset(6)]
        internal byte Byte6;
        [FieldOffset(7)]
        internal byte Byte7;
        [FieldOffset(6)]
        internal ushort Byte67;
        [FieldOffset(8)]
        internal byte Byte8;
        [FieldOffset(9)]
        internal byte Byte9;
        [FieldOffset(10)]
        internal byte Byte10;
        [FieldOffset(11)]
        internal byte Byte11;
        [FieldOffset(12)]
        internal byte Byte12;
        [FieldOffset(13)]
        internal byte Byte13;
        [FieldOffset(14)]
        internal byte Byte14;
        [FieldOffset(15)]
        internal byte Byte15;
        /// <summary>
        /// 获取字节数组
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns>字节数组</returns>
        public unsafe static byte[] ToByteArray(Guid guid)
        {
            var newGuid = new GuidStruct { Value = guid };
            var data = new byte[16];
            MemoryUnsafe.Copy(&newGuid, data, 16);
            return data;
        }
    }
}
