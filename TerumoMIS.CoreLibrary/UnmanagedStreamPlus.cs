//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: UnmanagedStreamPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  UnmanagedStreamPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 9:06:27
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     非托管内存数据流
    /// </summary>
    public abstract unsafe class UnmanagedStreamBasePlus : IDisposable
    {
        /// <summary>
        ///     默认容器初始尺寸
        /// </summary>
        public const int DefaultLength = 256;

        /// <summary>
        ///     当前数据长度
        /// </summary>
        protected int LengthBase;

        /// <summary>
        ///     非托管内存数据流
        /// </summary>
        /// <param name="length">容器初始尺寸</param>
        protected UnmanagedStreamBasePlus(int length)
        {
            Data = UnmanagedPlus.Get(DataLength = length > 0 ? length : DefaultLength, false).Byte;
            IsUnmanaged = true;
        }

        /// <summary>
        ///     非托管内存数据流
        /// </summary>
        /// <param name="data">无需释放的数据</param>
        /// <param name="dataLength">容器初始尺寸</param>
        protected UnmanagedStreamBasePlus(byte* data, int dataLength)
        {
            if (data == null || dataLength <= 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            Data = data;
            DataLength = dataLength;
        }

        /// <summary>
        ///     内存数据流转换
        /// </summary>
        /// <param name="stream">内存数据流</param>
        protected internal UnmanagedStreamBasePlus(UnmanagedStreamBasePlus stream)
        {
            Data = stream.Data;
            DataLength = stream.DataLength;
            LengthBase = stream.LengthBase;
            IsUnmanaged = stream.IsUnmanaged;
            stream.IsUnmanaged = false;
        }

        /// <summary>
        ///     数据
        /// </summary>
        public byte* Data { get; private set; }

        /// <summary>
        ///     当前写入位置
        /// </summary>
        public byte* CurrentData
        {
            get { return Data + LengthBase; }
        }

        /// <summary>
        ///     数据长度
        /// </summary>
        internal int DataLength { get; private set; }

        /// <summary>
        ///     是否非托管内存数据
        /// </summary>
        internal bool IsUnmanaged { get; private set; }

        /// <summary>
        ///     释放数据容器
        /// </summary>
        public virtual void Dispose()
        {
            Close();
        }

        /// <summary>
        ///     释放数据容器
        /// </summary>
        public virtual void Close()
        {
            if (IsUnmanaged)
            {
                UnmanagedPlus.Free(Data);
                IsUnmanaged = false;
            }
            DataLength = LengthBase = 0;
            Data = null;
        }

        /// <summary>
        ///     清空数据
        /// </summary>
        public virtual void Clear()
        {
            LengthBase = 0;
        }

        /// <summary>
        ///     设置容器尺寸
        /// </summary>
        /// <param name="length">容器尺寸</param>
        protected void SetStreamLength(int length)
        {
            if (length < DefaultLength) length = DefaultLength;
            var newData = UnmanagedPlus.Get(length, false).Byte;
            MemoryUnsafe.Copy(Data, newData, LengthBase);
            if (IsUnmanaged) UnmanagedPlus.Free(Data);
            Data = newData;
            DataLength = length;
            IsUnmanaged = true;
        }

        /// <summary>
        ///     设置容器尺寸
        /// </summary>
        /// <param name="length">容器尺寸</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void TrySetStreamLength(int length)
        {
            if ((length += LengthBase) > DataLength) SetStreamLength(length);
        }

        /// <summary>
        ///     预增数据流长度
        /// </summary>
        /// <param name="length">增加长度</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void PrepLengthBase(int length)
        {
            var newLength = length + LengthBase;
            if (newLength > DataLength) SetStreamLength(Math.Max(newLength, DataLength << 1));
        }

        /// <summary>
        ///     重置当前数据长度
        /// </summary>
        /// <param name="length">当前数据长度</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetLengthBase(int length)
        {
            if (length > 0)
            {
                if (length > DataLength) SetStreamLength(length);
                LengthBase = length;
            }
            else if (length == 0) LengthBase = 0;
            else LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBase(char value)
        {
            PrepLengthBase(sizeof (char));
            *(char*) (Data + LengthBase) = value;
            LengthBase += sizeof (char);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="stream">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteBase(UnmanagedStreamBasePlus stream)
        {
            if (stream != null)
            {
                PrepLengthBase(stream.LengthBase);
                MemoryUnsafe.Copy(stream.Data, Data + LengthBase, stream.LengthBase);
                LengthBase += stream.LengthBase;
            }
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBase(string value)
        {
            if (value != null)
            {
                var length = value.Length << 1;
                PrepLengthBase(length);
                StringUnsafe.Copy(value, Data + LengthBase);
                LengthBase += length;
            }
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBase(SubStringStruct value)
        {
            if (value.Length != 0)
            {
                var length = value.Length << 1;
                PrepLengthBase(length);
                fixed (char* valueFixed = value.Value)
                    MemoryUnsafe.Copy(valueFixed + value.StartIndex, Data + LengthBase, length);
                LengthBase += length;
            }
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="start">字符串起始位置</param>
        /// <param name="count">写入字符数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBase(char* start, int count)
        {
            if (start != null)
            {
                var length = count << 1;
                PrepLengthBase(length);
                MemoryPlus.Copy(start, Data + LengthBase, length);
                LengthBase += length;
            }
        }

        /// <summary>
        ///     写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">写入字符数</param>
        public void WriteBase(string value, int index, int count)
        {
            var range = new ArrayPlus.RangeStruct(value.Length, index, count);
            if (range.GetCount == count)
            {
                PrepLengthBase(count <<= 1);
                fixed (char* valueFixed = value)
                {
                    MemoryUnsafe.Copy(valueFixed + index, Data + LengthBase, count);
                }
                LengthBase += count;
            }
            else if (count != 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
        }

        /// <summary>
        ///     写字符串集合
        /// </summary>
        /// <param name="values">字符串集合</param>
        public void WriteBase(params string[] values)
        {
            if (values != null)
            {
                var length = values.Where(value => value != null).Sum(value => value.Length);
                PrepLengthBase(length <<= 1);
                var write = Data + LengthBase;
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        StringUnsafe.Copy(value, write);
                        write += value.Length << 1;
                    }
                }
                LengthBase += length;
            }
        }

        /// <summary>
        ///     转换成字符串
        /// </summary>
        /// <returns>字符串</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return new string((char*) Data, 0, LengthBase >> 1);
        }

        /// <summary>
        ///     重置数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="length">数据字节长度</param>
        /// <returns>原数据</returns>
        internal virtual byte* GetResetBase(byte* data, int length)
        {
            var value = Data;
            DataLength = length;
            LengthBase = 0;
            Data = data;
            return value;
        }

        /// <summary>
        ///     重置数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="length">数据字节长度</param>
        internal virtual void ResetBase(byte* data, int length)
        {
            if (IsUnmanaged)
            {
                UnmanagedPlus.Free(Data);
                IsUnmanaged = false;
            }
            Data = data;
            DataLength = length;
            LengthBase = 0;
        }

        /// <summary>
        ///     内存数据流转换
        /// </summary>
        /// <param name="stream">内存数据流</param>
        internal virtual void From(UnmanagedStreamBasePlus stream)
        {
            IsUnmanaged = stream.IsUnmanaged;
            Data = stream.Data;
            DataLength = stream.DataLength;
            LengthBase = stream.LengthBase;
            stream.IsUnmanaged = false;
        }

        /// <summary>
        ///     转换成字符流
        /// </summary>
        /// <returns>内存字符流</returns>
        internal CharStreamPlus ToCharStream()
        {
            if ((LengthBase & 1) != 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            DataLength &= (int.MaxValue - 1);
            return new CharStreamPlus(this);
        }
    }

    /// <summary>
    ///     非托管内存数据流
    /// </summary>
    public unsafe class UnmanagedStreamPlus : UnmanagedStreamBasePlus
    {
        /// <summary>
        ///     原始偏移位置
        /// </summary>
        protected int Offset;

        /// <summary>
        ///     非托管内存数据流
        /// </summary>
        /// <param name="length">容器初始尺寸</param>
        public UnmanagedStreamPlus(int length = DefaultLength) : base(length)
        {
        }

        /// <summary>
        ///     非托管内存数据流
        /// </summary>
        /// <param name="data">无需释放的数据</param>
        /// <param name="dataLength">容器初始尺寸</param>
        public UnmanagedStreamPlus(byte* data, int dataLength) : base(data, dataLength)
        {
        }

        /// <summary>
        ///     内存数据流转换
        /// </summary>
        /// <param name="stream">内存数据流</param>
        internal UnmanagedStreamPlus(UnmanagedStreamBasePlus stream) : base(stream)
        {
        }

        /// <summary>
        ///     非安全访问内存数据流
        /// </summary>
        /// <returns>非安全访问内存数据流</returns>
        public UnSaferStruct Unsafer
        {
            get { return new UnSaferStruct {Stream = this}; }
        }

        /// <summary>
        ///     相对于原始偏移位置的数据长度
        /// </summary>
        public int OffsetLength
        {
            get { return Offset + Length; }
        }

        /// <summary>
        ///     当前数据长度
        /// </summary>
        public int Length
        {
            get { return LengthBase; }
        }

        /// <summary>
        ///     预增数据流长度
        /// </summary>
        /// <param name="length">增加长度</param>
        public virtual void PrepLength(int length)
        {
            PrepLengthBase(length);
        }

        /// <summary>
        ///     预增数据流结束
        /// </summary>
        public virtual void PrepLength()
        {
        }

        /// <summary>
        ///     重置当前数据长度
        /// </summary>
        /// <param name="length">当前数据长度</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            SetLengthBase(length);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            if (Length == DataLength) SetStreamLength(Length << 1);
            Data[LengthBase++] = (byte) (value ? 1 : 0);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            if (Length == DataLength) SetStreamLength(Length << 1);
            Data[LengthBase++] = value;
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
        {
            if (LengthBase == DataLength) SetStreamLength(LengthBase << 1);
            Data[LengthBase++] = (byte) value;
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            PrepLength(sizeof (short));
            *(short*) (Data + LengthBase) = value;
            LengthBase += sizeof (short);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value)
        {
            PrepLength(sizeof (ushort));
            *(ushort*) (Data + LengthBase) = value;
            LengthBase += sizeof (ushort);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            PrepLength(sizeof (int));
            *(int*) (Data + LengthBase) = value;
            LengthBase += sizeof (int);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnsafeWrite(int value)
        {
            *(int*) (Data + LengthBase) = value;
            LengthBase += sizeof (int);
            //if (length > DataLength) log.Error.ThrowReal(length.toString() + " > " + DataLength.toString(), true, false);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value)
        {
            PrepLength(sizeof (uint));
            *(uint*) (Data + LengthBase) = value;
            LengthBase += sizeof (uint);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            PrepLength(sizeof (long));
            *(long*) (Data + LengthBase) = value;
            LengthBase += sizeof (long);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value)
        {
            PrepLength(sizeof (ulong));
            *(ulong*) (Data + LengthBase) = value;
            LengthBase += sizeof (ulong);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(DateTime value)
        {
            PrepLength(sizeof (long));
            *(long*) (Data + LengthBase) = value.Ticks;
            LengthBase += sizeof (long);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
        {
            PrepLength(sizeof (float));
            *(float*) (Data + LengthBase) = value;
            LengthBase += sizeof (float);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value)
        {
            PrepLength(sizeof (double));
            *(double*) (Data + LengthBase) = value;
            LengthBase += sizeof (double);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(decimal value)
        {
            PrepLength(sizeof (decimal));
            *(decimal*) (Data + LengthBase) = value;
            LengthBase += sizeof (decimal);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Guid value)
        {
            PrepLength(sizeof (Guid));
            *(Guid*) (Data + LengthBase) = value;
            LengthBase += sizeof (Guid);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="data">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] data)
        {
            if (data != null)
            {
                PrepLength(data.Length);
                MemoryUnsafe.Copy(data, Data + LengthBase, data.Length);
                LengthBase += data.Length;
            }
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="stream">数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(UnmanagedStreamPlus stream)
        {
            WriteBase(stream);
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">写入字节数</param>
        public void Write(byte[] data, int index, int count)
        {
            var range = new ArrayPlus.RangeStruct(data.Length, index, count);
            if (range.GetCount == count)
            {
                PrepLength(count);
                fixed (byte* dataFixed = data)
                {
                    MemoryUnsafe.Copy(dataFixed + range.SkipCount, Data + Length, count);
                }
                LengthBase += count;
            }
            else if (count != 0) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
        }

        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="data">数据</param>
        public void Write(SubArrayStruct<byte> data)
        {
            var count = data.Count;
            if (count != 0)
            {
                PrepLength(count);
                fixed (byte* dataFixed = data.Array)
                {
                    MemoryUnsafe.Copy(dataFixed + data.StartIndex, Data + Length, count);
                }
                LengthBase += count;
            }
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="value">数据</param>
        /// <param name="length">数据长度</param>
        public void Write(byte* value, int length)
        {
            if (value != null)
            {
                PrepLength(length);
                MemoryUnsafe.Copy(value, Data + LengthBase, length);
                LengthBase += length;
            }
        }

        /// <summary>
        ///     转换成字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        public byte[] GetArray()
        {
            if (LengthBase == 0) return NullValuePlus<byte>.Array;
            var data = new byte[LengthBase];
            MemoryUnsafe.Copy(Data, data, LengthBase);
            return data;
        }

        /// <summary>
        ///     转换成字节数组
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">字节数</param>
        /// <returns>字节数组</returns>
        public byte[] GetArray(int index, int count)
        {
            var range = new ArrayPlus.RangeStruct(Length, index, count);
            if (count == range.GetCount)
            {
                var data = new byte[count];
                MemoryUnsafe.Copy(Data + index, data, count);
                return data;
            }
            if (count == 0) return null;
            LogPlus.Default.Throw(LogPlus.ExceptionTypeEnum.IndexOutOfRange);
            return null;
        }

        /// <summary>
        ///     转换成字节数组
        /// </summary>
        /// <param name="copyIndex">复制起始位置</param>
        /// <returns>字节数组</returns>
        internal byte[] GetArray(int copyIndex)
        {
            var data = new byte[LengthBase];
            fixed (byte* dataFixed = data)
                MemoryUnsafe.Copy(Data + copyIndex, dataFixed + copyIndex, LengthBase - copyIndex);
            return data;
        }

        /// <summary>
        ///     转换成字节数组
        /// </summary>
        /// <param name="minSize">复制起始位置</param>
        /// <returns>字节数组</returns>
        public byte[] GetSizeArray(int minSize)
        {
            var data = new byte[LengthBase < minSize ? minSize : LengthBase];
            MemoryUnsafe.Copy(Data, data, LengthBase);
            return data;
        }

        /// <summary>
        ///     转换成字节数组
        /// </summary>
        /// <param name="copyIndex">复制起始位置</param>
        /// <param name="minSize">复制起始位置</param>
        /// <returns>字节数组</returns>
        internal byte[] GetSizeArray(int copyIndex, int minSize)
        {
            var data = new byte[LengthBase < minSize ? minSize : LengthBase];
            fixed (byte* dataFixed = data)
                MemoryUnsafe.Copy(Data + copyIndex, dataFixed + copyIndex, LengthBase - copyIndex);
            return data;
        }

        /// <summary>
        ///     重置数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="length">数据字节长度</param>
        /// <returns>原数据</returns>
        internal override byte* GetResetBase(byte* data, int length)
        {
            data = base.GetResetBase(data, length);
            Offset = 0;
            return data;
        }

        /// <summary>
        ///     重置数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="length">数据字节长度</param>
        internal override void ResetBase(byte* data, int length)
        {
            base.ResetBase(data, length);
            Offset = 0;
        }

        /// <summary>
        ///     内存数据流转换
        /// </summary>
        /// <param name="stream">内存数据流</param>
        internal override void From(UnmanagedStreamBasePlus stream)
        {
            base.From(stream);
            Offset = 0;
        }

        /// <summary>
        ///     内存数据流(请自行确保数据可靠性)
        /// </summary>
        public struct UnSaferStruct
        {
            /// <summary>
            ///     内存数据流
            /// </summary>
            public UnmanagedStreamPlus Stream;

            /// <summary>
            ///     增加数据流长度
            /// </summary>
            /// <param name="length">增加长度</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddSerializeLength(int length)
            {
                Stream.LengthBase += length + (-length & 3);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     增加数据流长度
            /// </summary>
            /// <param name="length">增加长度</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddLength(int length)
            {
                Stream.LengthBase += length;
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     设置数据流长度
            /// </summary>
            /// <param name="length">数据流长度</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetLength(int length)
            {
                Stream.LengthBase = length;
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(bool value)
            {
                Stream.Data[Stream.LengthBase++] = (byte) (value ? 1 : 0);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(byte value)
            {
                Stream.Data[Stream.LengthBase++] = value;
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(sbyte value)
            {
                Stream.Data[Stream.LengthBase++] = (byte) value;
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(short value)
            {
                *(short*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (short);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(ushort value)
            {
                *(ushort*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (ushort);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
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
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(int value)
            {
                Stream.UnsafeWrite(value);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(uint value)
            {
                *(uint*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (uint);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(long value)
            {
                *(long*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (long);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(ulong value)
            {
                *(ulong*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (ulong);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(DateTime value)
            {
                *(long*) Stream.CurrentData = value.Ticks;
                Stream.LengthBase += sizeof (long);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(float value)
            {
                *(float*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (float);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(double value)
            {
                *(double*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (double);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(decimal value)
            {
                *(decimal*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (decimal);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="value">数据</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(Guid value)
            {
                *(Guid*) Stream.CurrentData = value;
                Stream.LengthBase += sizeof (Guid);
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="data">数据,不能为null</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(byte[] data)
            {
                MemoryUnsafe.Copy(data, Stream.CurrentData, data.Length);
                Stream.LengthBase += data.Length;
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }

            /// <summary>
            ///     写数据
            /// </summary>
            /// <param name="stream">数据,不能为null</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(UnmanagedStreamPlus stream)
            {
                MemoryUnsafe.Copy(stream.Data, Stream.CurrentData, stream.Length);
                Stream.LengthBase += stream.Length;
                //if (Stream.length > Stream.DataLength) log.Error.ThrowReal(Stream.length.toString() + " > " + Stream.DataLength.toString(), true, false);
            }
        }
    }
}