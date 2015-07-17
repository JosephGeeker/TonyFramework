//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: StreamUnsafe
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Unsafe
//	File Name:  StreamUnsafe
//	User name:  C1400008
//	Location Time: 2015/7/16 13:25:16
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.IO;

namespace TerumoMIS.CoreLibrary.Unsafe
{
    /// <summary>
    ///     流扩展操作（非安全，请自行确保数据可靠性）
    /// </summary>
    public static class StreamUnsafe
    {
        /// <summary>
        ///     32位整数0的字节数组
        /// </summary>
        private static readonly byte[] IntZore = BitConverter.GetBytes(0);

        /// <summary>
        ///     数据流写入32位整数0
        /// </summary>
        /// <param name="stream">数据流</param>
        public static void WirteIntZore(Stream stream)
        {
            stream.Write(IntZore, 0, sizeof (int));
        }
    }
}