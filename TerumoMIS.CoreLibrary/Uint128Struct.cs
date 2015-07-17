//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: Uint128Struct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  Uint128Struct
//	User name:  C1400008
//	Location Time: 2015/7/13 9:17:57
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Runtime.InteropServices;
using TerumoMIS.CoreLibrary.Code.Csharper;
using TerumoMIS.CoreLibrary.Emit;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 128位整数
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Uint128Struct:IEquatable<Uint128Struct>
    {
        /// <summary>
        /// 低32位
        /// </summary>
        [FieldOffset(0)]
        private uint bit0;
        /// <summary>
        /// 32-64位
        /// </summary>
        [FieldOffset(sizeof(uint))]
        private uint bit32;
        /// <summary>
        /// 64-96位
        /// </summary>
        [FieldOffset(sizeof(ulong))]
        private uint bit64;
        /// <summary>
        /// 高32位
        /// </summary>
        [FieldOffset(sizeof(ulong) + sizeof(uint))]
        private uint bit96;
        /// <summary>
        /// 低64位
        /// </summary>
        [FieldOffset(0)]
        public ulong Low;
        /// <summary>
        /// 高64位
        /// </summary>
        [FieldOffset(sizeof(ulong))]
        public ulong High;
        /// <summary>
        /// 分段求和
        /// </summary>
        /// <param name="value"></param>
        public void AppendSum(ulong value)
        {
            Low += value & 0xffffffffUL;
            High += value >> 32;
        }
        /// <summary>
        /// 结束分段求和
        /// </summary>
        public void SumEnd()
        {
            High += bit32;
            bit32 = bit64;
            bit64 = bit96;
            bit96 = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Uint128Struct other)
        {
            return ((Low ^ other.Low) | (High ^ other.High)) == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(bit0 ^ bit32 ^ bit64 ^ bit96);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals((Uint128Struct)obj);
        }
        /// <summary>
        /// 16进制字符串解析
        /// </summary>
        /// <param name="data"></param>
        internal unsafe void ParseHex(byte* data)
        {
            var next = data + 8;
            bit96 = NumberPlus.ParseHex(data, next);
            bit64 = NumberPlus.ParseHex(next, data += 16);
            bit32 = NumberPlus.ParseHex(data, next += 16);
            bit0 = NumberPlus.ParseHex(next, data + 16);
        }
        /// <summary>
        /// 转换成16进制字符串
        /// </summary>
        /// <returns></returns>
        internal unsafe byte[] ToHex()
        {
            var data = new byte[32];
            fixed (byte* dataFixed = data)
            {
                NumberPlus.ToHex(bit96, dataFixed);
                NumberPlus.ToHex(bit64, dataFixed + 8);
                NumberPlus.ToHex(bit32, dataFixed + 16);
                NumberPlus.ToHex(bit0, dataFixed + 24);
            }
            return data;
        }

        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="serializer">对象序列化器</param>
        /// <param name="value">128位整数</param>
        [DataSerializerPlus.custom]
        private unsafe static void Serialize(DataSerializerPlus serializer, Uint128Struct value)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            var stream = serializer.Stream;
            stream.PrepLength(sizeof(ulong) + sizeof(ulong));
            var write = stream.CurrentData;
            *(ulong*)write = value.Low;
            *(ulong*)(write + sizeof(ulong)) = value.High;
            stream.Unsafer.AddLength(sizeof(ulong) + sizeof(ulong));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="deSerializer">对象反序列化器</param>
        /// <param name="value">128位整数</param>
        [DataSerializerPlus.custom]
        private unsafe static void DeSerialize(DataSerializerPlus deSerializer, ref Uint128Struct value)
        {
            if (deSerializer.VerifyRead(sizeof(ulong) + sizeof(ulong)))
            {
                byte* dataStart = deSerializer.Read;
                value.Low = *(ulong*)(dataStart - sizeof(ulong) * 2);
                value.High = *(ulong*)(dataStart - sizeof(ulong));
            }
        }
    }
}
