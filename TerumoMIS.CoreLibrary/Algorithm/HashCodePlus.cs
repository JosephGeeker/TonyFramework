//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HashCodePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Algorithm
//	File Name:  HashCodePlus
//	User name:  C1400008
//	Location Time: 2015/7/8 16:03:35
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

namespace TerumoMIS.CoreLibrary.Algorithm
{
    /// <summary>
    /// 计算Hash值
    /// </summary>
    public static class HashCodePlus
    {
        /// <summary>
        /// 计算32位Hash值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>32位Hash值</returns>
        public unsafe static int GetHashCode(byte[] data)
        {
            if (data != null && data.Length != 0)
            {
                fixed (byte* fixedData = data) return GetHashCode(fixedData, data.Length);
            }
            return 0;
        }
        /// <summary>
        /// 计算32位Hash值
        /// </summary>
        /// <param name="data">数据起始位置</param>
        /// <param name="length">数据长度</param>
        /// <returns>32位HASH值</returns>
        internal unsafe static int GetHashCode(void* data, int length)
        {
            if (PubPlus.MemoryBits == 64)
            {
                var value = GetHashCode64((byte*)data, length);
                return (int)(value ^ (value >> 32));
            }
            //  一般编码都以字节为基本单位,也就是说基本单位长度为8bit;
            //  常用编码可能比较集中,造成编码中出现伪固定位(大多时候某些固定位都是同一值)
            //  采用移位的方式:当移位量为1或7时,一般只能覆盖掉1个固定位;当移位量为3或5时,一般能覆盖掉3个固定位;所以本程序使用的移位量为8x+5/3
            //  由于64=5+59=13+3*17=3*7+43=29+5*7=37+3*3*3=5*9+19=53+11=61+3,其中(5+59),(53+11),(61+3)为素数对成为最佳移位量,本程序选择中性素数对53+11
            //  由于32=5+3*3*3=13+19=3*7+11=29+3,其中(13+19),(29+3)为素数对成为最佳移位量,本程序选择中性素数对13+19
            if (length >= sizeof(uint))
            {
                var value = *(uint*)data;
                var start = (byte*)data;
                for (var end = start + (length & (int.MaxValue - sizeof(uint) + 1)); (start += sizeof(uint)) != end; value ^= *(uint*)start)
                {
                    value = (value << 13) | (value >> 19);
                }
                if ((length & (sizeof(uint) - 1)) != 0)
                {
                    value = (value << 13) | (value >> 19);
                    value ^= *(uint*)start << ((sizeof(uint) - (length & (sizeof(uint) - 1))) << 3);
                }
                return (int)value ^ length;
            }
            return (int)(*(uint*)data << ((sizeof(uint) - length) << 3)) ^ length;
        }
        /// <summary>
        /// 计算64位Hash值
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>64位Hash值</returns>
        public unsafe static long GetHashCode64(byte[] data)
        {
            if (data != null && data.Length != 0)
            {
                fixed (byte* fixedData = data) return GetHashCode64(fixedData, data.Length);
            }
            return 0;
        }
        /// <summary>
        /// 计算64位Hash值
        /// </summary>
        /// <param name="start">数据起始位置</param>
        /// <param name="length">数据长度</param>
        /// <returns>64位Hash值</returns>
        private unsafe static long GetHashCode64(byte* start, int length)
        {
            if (length >= sizeof(ulong))
            {
                var value = *(ulong*)start;
                for (var end = start + (length & (int.MaxValue - sizeof(ulong) + 1)); (start += sizeof(ulong)) != end; value ^= *(ulong*)start)
                {
                    value = (value << 53) | (value >> 11);
                }
                if ((length & (sizeof(ulong) - 1)) != 0)
                {
                    value = (value << 53) | (value >> 11);
                    value ^= *(ulong*)start << ((sizeof(ulong) - (length & (sizeof(ulong) - 1))) << 3);
                }
                return (long)value ^ ((long)length << 19);
            }
            return (long)(*(ulong*)start << ((sizeof(ulong) - length) << 3)) ^ ((long)length << 19);
        }
    }
}
