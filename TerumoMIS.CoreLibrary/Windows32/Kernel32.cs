//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: Kernel32
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Windows32
//	File Name:  Kernel32
//	User name:  C1400008
//	Location Time: 2015/7/14 14:52:32
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Windows32
{
    /// <summary>
    /// Kernel32.dll API
    /// </summary>
    internal static class Kernel32
    {
        /// <summary>
        /// 内存复制
        /// </summary>
        /// <param name="dest">目标位置</param>
        /// <param name="src">源位置</param>
        /// <param name="length">字节长度</param>
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static unsafe extern void RtlMoveMemory(void* dest, void* src, int length);
        /// <summary>
        /// 获取指定磁盘的信息，包括磁盘的可用空间。
        /// </summary>
        /// <param name="bootPath">磁盘根目录。如：@"C:\"</param>
        /// <param name="sectorsPerCluster">每个簇所包含的扇区个数</param>
        /// <param name="bytesPerSector">每个扇区所占的字节数</param>
        /// <param name="numberOfFreeClusters">空闲的簇的个数</param>
        /// <param name="totalNumberOfClusters">簇的总个数</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetDiskFreeSpace(string bootPath, out uint sectorsPerCluster, out uint bytesPerSector, out uint numberOfFreeClusters, out uint totalNumberOfClusters);
    }
}
