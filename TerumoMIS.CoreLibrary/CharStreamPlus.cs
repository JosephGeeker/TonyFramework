//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CharStreamPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  CharStreamPlus
//	User name:  C1400008
//	Location Time: 2015/7/14 14:55:10
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System.Runtime.CompilerServices;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     内存字符流
    /// </summary>
    public sealed unsafe class CharStreamPlus : UnmanagedStreamBasePlus
    {
        /// <summary>
        ///     内存数据流
        /// </summary>
        /// <param name="length">容器初始尺寸</param>
        public CharStreamPlus(int length = DefaultLength) : base(length << 1)
        {
        }

        /// <summary>
        ///     非托管内存数据流
        /// </summary>
        /// <param name="data">无需释放的数据</param>
        /// <param name="length">容器初始尺寸</param>
        public CharStreamPlus(char* data, int length) : base((byte*) data, length << 1)
        {
        }

        /// <summary>
        ///     内存数据流转换
        /// </summary>
        /// <param name="stream">内存数据流</param>
        internal CharStreamPlus(UnmanagedStreamBasePlus stream) : base(stream)
        {
        }

        /// <summary>
        ///     非安全访问内存字符流
        /// </summary>
        /// <returns>非安全访问内存字符流</returns>
        public UnSaferStruct Unsafer
        {
            get { return new UnSaferStruct {Stream = this}; }
        }

        /// <summary>
        ///     数据
        /// </summary>
        public char* Char
        {
            get { return (char*) Data; }
        }

        /// <summary>
        ///     当前写入位置
        /// </summary>
        public char* CurrentChar
        {
            get { return (char*) CurrentData; }
        }

        /// <summary>
        ///     当前数据长度
        /// </summary>
        public int Length
        {
            get { return LengthBase >> 1; }
        }

        /// <summary>
        ///     预增数据流长度
        /// </summary>
        /// <param name="length">增加长度</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepLength(int length)
        {
            PrepLengthBase(length << 1);
        }

        /// <summary>
        ///     重置当前数据长度
        /// </summary>
        /// <param name="length">当前数据长度</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            SetLengthBase(length << 1);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="stream">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(CharStreamPlus stream)
        {
            WriteBase(stream);
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteNotNull(string value)
        {
            var length = value.Length << 1;
            PrepLength(length);
            StringUnsafe.Copy(value, Data + LengthBase);
            LengthBase += length;
        }

        /// <summary>
        ///     内存字符流(请自行确保数据可靠性)
        /// </summary>
        public struct UnSaferStruct
        {
            /// <summary>
            ///     内存字符流
            /// </summary>
            public CharStreamPlus Stream;

            /// <summary>
            ///     增加数据流长度
            /// </summary>
            /// <param name="length">增加长度</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddLength(int length)
            {
                Stream.LengthBase += length << 1;
            }

            /// <summary>
            ///     设置数据流长度
            /// </summary>
            /// <param name="length">数据流长度</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetLength(int length)
            {
                Stream.LengthBase = length << 1;
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(char value)
            {
                *(char*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (char);
            }

            /// <summary>
            ///     写字符串
            /// </summary>
            /// <param name="start">字符串起始位置,不能为null</param>
            /// <param name="count">写入字符数，必须>=0</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(char* start, int count)
            {
                MemoryPlus.Copy(start, Stream.CurrentData, count <<= 1);
                Stream.LengthBase += count;
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="stream">数据,不能为null</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(CharStreamPlus stream)
            {
                MemoryUnsafe.Copy(stream.CurrentData, Stream.CurrentData, stream.Length);
                Stream.LengthBase += stream.Length;
            }
        }
    }
}