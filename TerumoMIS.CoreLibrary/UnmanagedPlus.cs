//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: UnmanagedPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  UnmanagedPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 8:49:18
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 非托管内存
    /// </summary>
    public unsafe static class UnmanagedPlus
    {
        /// <summary>
        /// 指针
        /// </summary>
        private static byte* _data;
        /// <summary>
        /// 指针
        /// </summary>
        public static byte* Data
        {
            get { return _data; }
        }
        /// <summary>
        /// 字节长度
        /// </summary>
        private static int _size;

        /// <summary>
        /// 释放内存
        /// </summary>
        public static void Free()
        {
            Free(_data);
            _data = null;
            _size = 0;
        }
        /// <summary>
        /// 未释放非托管内存句柄数量
        /// </summary>
        private static int _usedCount;
        /// <summary>
        /// 未释放非托管内存句柄数量
        /// </summary>
        public static int UsedCount
        {
            get { return _usedCount; }
        }
        /// <summary>
        /// 申请非托管内存
        /// </summary>
        /// <param name="size">内存字节数</param>
        /// <param name="isClear">是否需要清除</param>
        /// <returns>非托管内存起始指针</returns>
        public static PointerStruct Get(int size, bool isClear = true)
        {
            if (size < 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            if (size != 0)
            {
                byte* data = (byte*)Marshal.AllocHGlobal(size);
                Interlocked.Increment(ref _usedCount);
                if (isClear) fastCSharp.unsafer.memory.Fill(data, (byte)0, size);
                return new PointerStruct { Data = data };
            }
            return default(PointerStruct);
        }
        /// <summary>
        /// 批量申请非托管内存
        /// </summary>
        /// <param name="isClear">是否需要清除</param>
        /// <param name="sizes">内存字节数集合</param>
        /// <returns>非托管内存起始指针</returns>
        public static PointerStruct[] Get(bool isClear, params int[] sizes)
        {
            if (sizes.Length != 0)
            {
                var sum = 0;
                foreach (var size in sizes)
                {
                    if (size < 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
                    checked { sum += size; }
                }
                var pointer = Get(sum, isClear);
                var data = pointer.Byte;
                if (data != null)
                {
                    int index = 0;
                    PointerStruct[] datas = new PointerStruct[sizes.Length];
                    foreach (int size in sizes)
                    {
                        datas[index++] = new PointerStruct { Data = data };
                        data += size;
                    }
                    return datas;
                }
            }
            return null;
        }
        /// <summary>
        /// 释放内存
        /// </summary>
        /// <param name="data">非托管内存起始指针</param>
        public static void Free(ref PointerStruct data)
        {
            if (data.Data != null)
            {
                Interlocked.Decrement(ref _usedCount);
                Marshal.FreeHGlobal((IntPtr)data.Data);
                data.Data = null;
            }
        }
        /// <summary>
        /// 释放内存
        /// </summary>
        /// <param name="data">非托管内存起始指针</param>
        public static void Free(void* data)
        {
            if (data != null)
            {
                Interlocked.Decrement(ref _usedCount);
                Marshal.FreeHGlobal((IntPtr)data);
            }
        }
    }
}
