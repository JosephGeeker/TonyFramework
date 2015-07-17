//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FileStreamWriterPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.IO
//	File Name:  FileStreamWriterPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:04:49
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.IO
{
    /// <summary>
    /// 文件流写入器
    /// </summary>
    public class FileStreamWriterPlus:IDisposable
    {
        /// <summary>
        /// 最大文件缓存集合字节数
        /// </summary>
        private const int maxBufferSize = 1 << 20;
        /// <summary>
        /// 文件共享方式
        /// </summary>
        private FileShare fileShare;
        /// <summary>
        /// 附加选项
        /// </summary>
        private FileOptions fileOption;
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// 文件流
        /// </summary>
        private FileStream fileStream;
        /// <summary>
        /// 刷新检测周期
        /// </summary>
        private long checkFlushTicks = date.SecondTicks << 1;
        /// <summary>
        /// 缓存刷新检测秒数
        /// </summary>
        public uint CheckFlushSecond
        {
            set { checkFlushTicks = (long)value * date.SecondTicks; }
        }
        /// <summary>
        /// 缓存刷新检测毫秒数
        /// </summary>
        public uint CheckFlushMillisecond
        {
            set { checkFlushTicks = (long)value * date.MillisecondTicks; }
        }
        /// <summary>
        /// 文件流长度
        /// </summary>
        public long FileSize
        {
            get
            {
                FileStream fileStream = this.fileStream;
                return fileStream != null ? fileStream.Length : -1;
            }
        }
        /// <summary>
        /// 文件有效长度(已经写入)
        /// </summary>
        protected long fileLength;
        /// <summary>
        /// 当前写入缓存后的文件长度
        /// </summary>
        protected long fileBufferLength;
        /// <summary>
        /// 未写入文件缓存集合字节数
        /// </summary>
        private long bufferSize;
        /// <summary>
        /// 待写入文件缓存集合位置索引
        /// </summary>
        protected long bufferIndex;
        /// <summary>
        /// 是否需要等待写入缓存
        /// </summary>
        public bool IsWaitBuffer
        {
            get { return bufferSize > maxBufferSize; }
        }
        /// <summary>
        /// 文件编码
        /// </summary>
        private Encoding encoding;
        /// <summary>
        /// 文件写入缓冲区
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// 文件写入缓冲字节长度
        /// </summary>
        protected int bufferLength;
        /// <summary>
        /// 文件写入缓冲起始位置
        /// </summary>
        private int startIndex;
        /// <summary>
        /// 当前写入位置
        /// </summary>
        private int currentIndex;
        /// <summary>
        /// 待写入文件缓存集合
        /// </summary>
        protected list<memoryPool.pushSubArray> buffers = new list<memoryPool.pushSubArray>(sizeof(int));
        /// <summary>
        /// 正在写入文件缓存集合
        /// </summary>
        private list<memoryPool.pushSubArray> currentBuffers = new list<memoryPool.pushSubArray>(sizeof(int));
        /// <summary>
        /// 缓存刷新等待事件
        /// </summary>
        private EventWaitHandle flushWait;
        /// <summary>
        /// 缓存刷新等待数量
        /// </summary>
        protected int flushCount;
        /// <summary>
        /// 缓存操作锁
        /// </summary>
        protected int bufferLock;
        /// <summary>
        /// 最后一次异常错误
        /// </summary>
        public Exception LastException { get; private set; }
        /// <summary>
        /// 写入文件
        /// </summary>
        private Action writeFileHandle;
        /// <summary>
        /// 刷新检测
        /// </summary>
        private Action checkFlushHandle;
        /// <summary>
        /// 刷新检测时间
        /// </summary>
        private DateTime checkFlushTime;
        /// <summary>
        /// 是否正在检测刷新
        /// </summary>
        private int isCheckFlush;
        /// <summary>
        /// 是否写日志
        /// </summary>
        private bool isLog;
        /// <summary>
        /// 是否正在刷新
        /// </summary>
        private byte isFlush;
        /// <summary>
        /// 是否正在写文件
        /// </summary>
        private byte isWritting;
        /// <summary>
        /// 最后一个数据是否数据复制缓冲区
        /// </summary>
        private byte isCopyBuffer;
        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        protected byte isDisposed;
        /// <summary>
        /// 内存池
        /// </summary>
        private memoryPool memoryPool;
        /// <summary>
        /// 文件流写入器
        /// </summary>
        /// <param name="fileName">文件全名</param>
        /// <param name="mode">打开方式</param>
        /// <param name="fileShare">共享访问方式</param>
        /// <param name="fileOption">附加选项</param>
        /// <param name="encoding">文件编码</param>
        public fileStreamWriter(string fileName, FileMode mode = FileMode.CreateNew, FileShare fileShare = FileShare.None, FileOptions fileOption = FileOptions.None, bool isLog = true, Encoding encoding = null)
        {
            if (fileName.length() == 0) log.Error.Throw(log.exceptionType.Null);
            FileName = fileName;
            this.isLog = isLog;
            this.fileShare = fileShare;
            this.fileOption = fileOption;
            this.encoding = encoding;
            memoryPool = memoryPool.GetPool(bufferLength = (int)file.BytesPerCluster(fileName));
            buffer = memoryPool.Get();
            writeFileHandle = writeFile;
            open(mode);
            flushWait = new EventWaitHandle(true, EventResetMode.ManualReset);
        }
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="mode">打开方式</param>
        private unsafe void open(FileMode mode)
        {
            startIndex = currentIndex = 0;
            fileStream = new FileStream(FileName, mode, FileAccess.Write, fileShare, bufferLength, fileOption);
            fileLength = fileBufferLength = bufferIndex = fileStream.Length;
            if (fileLength != 0)
            {
                fileStream.Seek(0, SeekOrigin.End);
                startIndex = currentIndex = (int)(fileLength % bufferLength);
            }
            else if (encoding != null)
            {
                file.bom bom = file.GetBom(encoding);
                if ((currentIndex = bom.Length) != 0)
                {
                    bufferIndex = (fileBufferLength += currentIndex);
                    fixed (byte* bufferFixed = buffer) *(uint*)bufferFixed = bom.Bom;
                }
            }
        }
        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="value">数据</param>
        ///// <returns>写入位置,失败返回-1</returns>
        //public long Write(string value)
        //{
        //    return value.length() != 0 ? UnsafeWrite(value) : 0;
        //}
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="count">写入字节数</param>
        /// <returns>写入位置,失败返回-1</returns>
        public long Write(byte[] data, int count)
        {
            if (count > data.length() || count < 0) log.Error.Throw(log.exceptionType.IndexOutOfRange);
            return count != 0 ? UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(data, 0, count) }) : 0;
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">写入字节数</param>
        /// <returns>写入位置,失败返回-1</returns>
        public long Write(byte[] data, int index, int count)
        {
            if (index + count > data.length() || index < 0 || count < 0) log.Error.Throw(log.exceptionType.IndexOutOfRange);
            return count != 0 ? UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(data, index, count) }) : 0;
        }
        /// <summary>
        /// 字符串转换成字节数组
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>字节数组+缓冲区入池调用</returns>
        internal unsafe memoryPool.pushSubArray GetBytes(string value)
        {
            Encoding encoding = this.encoding ?? fastCSharp.config.appSetting.Encoding;
            int length = encoding.CodePage == Encoding.Unicode.CodePage ? value.Length << 1 : encoding.GetByteCount(value);
            memoryPool pool = memoryPool.GetDefaultPool(length);
            byte[] data = pool.Get(length);
            if (encoding.CodePage == Encoding.Unicode.CodePage)
            {
                fixed (byte* dataFixed = data) unsafer.String.Copy(value, dataFixed);
            }
            else encoding.GetBytes(value, 0, value.Length, data, 0);
            return new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(data, 0, length), PushPool = pool.PushHandle };
        }
        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="value">数据</param>
        ///// <returns>写入位置,失败返回-1</returns>
        //internal unsafe long UnsafeWrite(string value)
        //{
        //    return UnsafeWrite(GetBytes(value));
        //}
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>写入位置,失败返回-1</returns>
        internal long UnsafeWrite(byte[] data)
        {
            return UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(data, 0, data.Length) });
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">写入字节数</param>
        /// <returns>写入位置,失败返回-1</returns>
        internal long UnsafeWrite(byte[] data, int index, int count)
        {
            return UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(data, index, count) });
        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>写入位置,失败返回-1</returns>
        internal long UnsafeWrite(memoryPool.pushSubArray data)
        {
            flushWait.Reset();
            subArray<byte> dataArray = data.Value;
            interlocked.NoCheckCompareSetSleep0(ref bufferLock);
            if (isDisposed == 0)
            {
                long fileBufferLength = this.fileBufferLength;
                this.fileBufferLength += dataArray.Count;
                if (isWritting == 0)
                {
                    int length = currentIndex + dataArray.Count;
                    if (length < bufferLength && flushCount == 0)
                    {
                        Buffer.BlockCopy(dataArray.array, dataArray.StartIndex, buffer, currentIndex, dataArray.Count);
                        checkFlushTime = date.NowSecond.AddTicks(checkFlushTicks);
                        currentIndex = length;
                        bufferIndex = this.fileBufferLength;
                        bufferLock = 0;
                        data.Push();
                        setCheckFlush();
                    }
                    else
                    {
                        buffers.array[0] = data;
                        buffers.Unsafer.AddLength(1);
                        bufferSize += dataArray.Count;
                        isFlush = 0;
                        isWritting = 1;
                        isCopyBuffer = 0;
                        bufferLock = 0;
                        threadPool.TinyPool.FastStart(writeFileHandle, null, null);
                    }
                }
                else
                {
                    try
                    {
                        buffers.Add(data);
                        bufferSize += dataArray.Count;
                        isCopyBuffer = 0;
                    }
                    finally { bufferLock = 0; }
                }
                return fileBufferLength;
            }
            bufferLock = 0;
            data.Push();
            return -1;
        }
        ///// <summary>
        ///// 复制数据并写入
        ///// </summary>
        ///// <param name="data">数据</param>
        ///// <returns>写入位置,失败返回-1</returns>
        //internal long UnsafeWriteCopy(subArray<byte> data)
        //{
        //    byte isWriteBuffer = 0, isWrite = 0;
        //    interlocked.NoCheckCompareSetSleep0(ref bufferLock);
        //    long fileBufferLength = this.fileBufferLength;
        //    byte isDisposed = this.isDisposed;
        //    if (isDisposed == 0)
        //    {
        //        this.fileBufferLength += data.Count;
        //        if (isWritting == 0)
        //        {
        //            int length = currentIndex + data.Count;
        //            if (length < bufferLength)
        //            {
        //                Buffer.BlockCopy(data.Array, data.StartIndex, buffer, currentIndex, data.Count);
        //                currentIndex = length;
        //                bufferLock = 0;
        //                isWriteBuffer = 1;
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    copy(data.array, data.StartIndex, data.Count);
        //                    bufferSize += data.Count;
        //                    isWritting = 1;
        //                }
        //                finally { bufferLock = 0; }
        //                isWrite = 1;
        //            }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                copy(data.array, data.StartIndex, data.Count);
        //                bufferSize += data.Count;
        //            }
        //            finally { bufferLock = 0; }
        //        }
        //    }
        //    else bufferLock = 0;
        //    if (isDisposed == 0)
        //    {
        //        if (isWrite == 0)
        //        {
        //            if (isWriteBuffer != 0) setCheckFlush();
        //        }
        //        else threadPool.TinyPool.FastStart(writeFileHandle, null, null);
        //        return fileBufferLength;
        //    }
        //    return -1;
        //}
        ///// <summary>
        ///// 复制数据到缓冲区
        ///// </summary>
        ///// <param name="data">数据</param>
        ///// <param name="startIndex">数据其实位置</param>
        ///// <param name="length">数据长度</param>
        //private void copy(byte[] data, int startIndex, int length)
        //{
        //    if (isCopyBuffer != 0)
        //    {
        //        memoryPool.pushSubArray[] bufferArray = buffers.array;
        //        int bufferIndex = buffers.Count - 1;
        //        subArray<byte> copyBuffer = bufferArray[bufferIndex].Value;
        //        int freeLength = copyBuffer.FreeLength;
        //        if (length <= freeLength)
        //        {
        //            Buffer.BlockCopy(data, startIndex, copyBuffer.array, copyBuffer.EndIndex, length);
        //            bufferArray[bufferIndex].Value.UnsafeSetLength(copyBuffer.Count + length);
        //            if (length == freeLength) isCopyBuffer = 0;
        //            return;
        //        }
        //        Buffer.BlockCopy(data, startIndex, copyBuffer.array, copyBuffer.EndIndex, freeLength);
        //        bufferArray[bufferIndex].Value.UnsafeSetLength(copyBuffer.array.Length);
        //        startIndex += freeLength;
        //        length -= freeLength;
        //    }
        //    do
        //    {
        //        byte[] buffer = memoryPool.TryGet();
        //        if (buffer == null)
        //        {
        //            if (length <= memoryPool.Size)
        //            {
        //                Buffer.BlockCopy(data, startIndex, buffer = memoryPool.Get(), 0, length);
        //                isCopyBuffer = length == buffer.Length ? (byte)0 : (byte)1;
        //                buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(buffer, 0, length), PushPool = memoryPool.PushHandle });
        //                return;
        //            }
        //            Buffer.BlockCopy(data, startIndex, buffer = new byte[length], 0, length);
        //            buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(buffer, 0, length) });
        //            isCopyBuffer = 0;
        //            return;
        //        }
        //        if (length <= buffer.Length)
        //        {
        //            Buffer.BlockCopy(data, startIndex, buffer, 0, length);
        //            isCopyBuffer = length == buffer.Length ? (byte)0 : (byte)1;
        //            buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(buffer, 0, length), PushPool = memoryPool.PushHandle });
        //            return;
        //        }
        //        Buffer.BlockCopy(data, startIndex, buffer, 0, buffer.Length);
        //        startIndex += buffer.Length;
        //        length -= buffer.Length;
        //        buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(buffer, 0, buffer.Length), PushPool = memoryPool.PushHandle });
        //    }
        //    while (true);
        //}
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="dataLength">数据长度</param>
        /// <returns>写入位置,失败返回-1</returns>
        internal unsafe long UnsafeWrite(byte* data, int dataLength)
        {
            flushWait.Reset();
            Exception exception = null;
            fixed (byte* bufferFixed = buffer)
            {
                interlocked.NoCheckCompareSetSleep0(ref bufferLock);
                if (isDisposed == 0)
                {
                    long fileBufferLength = this.fileBufferLength;
                    this.fileBufferLength += dataLength;
                    if (isWritting == 0)
                    {
                        int length = currentIndex + dataLength;
                        if (length < bufferLength && flushCount == 0)
                        {
                            unsafer.memory.Copy(data, bufferFixed + currentIndex, dataLength);
                            checkFlushTime = date.NowSecond.AddTicks(checkFlushTicks);
                            currentIndex = length;
                            bufferIndex = this.fileBufferLength;
                            bufferLock = 0;
                            setCheckFlush();
                        }
                        else
                        {
                            try
                            {
                                copy(data, dataLength);
                                bufferSize += dataLength;
                                isFlush = 0;
                                isWritting = 1;
                            }
                            catch (Exception error) { exception = error; }
                            finally { bufferLock = 0; }
                            if (exception == null) threadPool.TinyPool.FastStart(writeFileHandle, null, null);
                            else
                            {
                                this.error(exception);
                                return -1;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            copy(data, dataLength);
                            bufferSize += dataLength;
                        }
                        catch (Exception error) { exception = error; }
                        finally { bufferLock = 0; }
                        if (exception != null)
                        {
                            this.error(exception);
                            return -1;
                        }
                    }
                    return fileBufferLength;
                }
                bufferLock = 0;
            }
            return -1;
        }
        /// <summary>
        /// 复制数据到缓冲区
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="length">数据长度</param>
        private unsafe void copy(byte* data, int length)
        {
            if (isCopyBuffer != 0)
            {
                memoryPool.pushSubArray[] bufferArray = buffers.array;
                int bufferIndex = buffers.Count - 1;
                subArray<byte> copyBuffer = bufferArray[bufferIndex].Value;
                int freeLength = copyBuffer.FreeLength;
                if (length <= freeLength)
                {
                    fixed (byte* bufferFixed = copyBuffer.array) unsafer.memory.Copy(data, bufferFixed + copyBuffer.EndIndex, length);
                    bufferArray[bufferIndex].Value.UnsafeSetLength(copyBuffer.Count + length);
                    if (length == freeLength) isCopyBuffer = 0;
                    return;
                }
                fixed (byte* bufferFixed = copyBuffer.array) unsafer.memory.Copy(data, bufferFixed + copyBuffer.EndIndex, freeLength);
                bufferArray[bufferIndex].Value.UnsafeSetLength(copyBuffer.array.Length);
                data += freeLength;
                length -= freeLength;
            }
            do
            {
                byte[] buffer = memoryPool.TryGet();
                if (buffer == null)
                {
                    if (length <= memoryPool.Size)
                    {
                        unsafer.memory.Copy(data, buffer = memoryPool.Get(), length);
                        isCopyBuffer = length == buffer.Length ? (byte)0 : (byte)1;
                        buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(buffer, 0, length), PushPool = memoryPool.PushHandle });
                        return;
                    }
                    unsafer.memory.Copy(data, buffer = new byte[length], length);
                    buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(buffer, 0, length) });
                    isCopyBuffer = 0;
                    return;
                }
                if (length <= buffer.Length)
                {
                    unsafer.memory.Copy(data, buffer, length);
                    isCopyBuffer = length == buffer.Length ? (byte)0 : (byte)1;
                    buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(buffer, 0, length), PushPool = memoryPool.PushHandle });
                    return;
                }
                unsafer.memory.Copy(data, buffer, buffer.Length);
                data += buffer.Length;
                length -= buffer.Length;
                buffers.Add(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(buffer, 0, buffer.Length), PushPool = memoryPool.PushHandle });
            }
            while (true);
        }
        /// <summary>
        /// 写入文件数据
        /// </summary>
        private void writeFile()
        {
            try
            {
                do
                {
                    interlocked.NoCheckCompareSetSleep0(ref bufferLock);
                    int bufferCount = buffers.Count;
                    if (bufferCount == 0)
                    {
                        if ((flushCount | isFlush) == 0)
                        {
                            checkFlushTime = date.NowSecond.AddTicks(checkFlushTicks);
                            isWritting = 0;
                            if (currentIndex == startIndex) bufferLock = 0;
                            else
                            {
                                bufferLock = 0;
                                setCheckFlush();
                            }
                            break;
                        }
                        isFlush = 0;
                        int writeSize = currentIndex - startIndex;
                        if (writeSize == 0)
                        {
                            bufferLock = 0;
                            if (buffers.Count == 0)
                            {
                                fileStream.Flush();
                                if (buffers.Count == 0) flushWait.Set();
                            }
                            continue;
                        }
                        bufferLock = 0;
                        fileStream.Write(buffer, startIndex, writeSize);
                        interlocked.NoCheckCompareSetSleep0(ref bufferLock);
                        fileLength += writeSize;
                        bufferLock = 0;
                        startIndex = currentIndex;
                        if (buffers.Count == 0)
                        {
                            fileStream.Flush();
                            if (buffers.Count == 0) flushWait.Set();
                        }
                        continue;
                    }
                    list<memoryPool.pushSubArray> datas = buffers;
                    isCopyBuffer = 0;
                    buffers = currentBuffers;
                    bufferIndex = fileBufferLength;
                    currentBuffers = datas;
                    bufferLock = 0;
                    foreach (memoryPool.pushSubArray data in datas.array)
                    {
                        int dataSize = data.Value.Count, writeSize = writeFile(data.Value);
                        interlocked.NoCheckCompareSetSleep0(ref bufferLock);
                        fileLength += writeSize;
                        bufferSize -= dataSize;
                        bufferLock = 0;
                        data.Push();
                        if (--bufferCount == 0) break;
                    }
                    Array.Clear(datas.array, 0, datas.Count);
                    datas.Empty();
                    if (isCopyBuffer != 0 && buffers.Count == 0) Thread.Sleep(0);
                }
                while (true);
            }
            catch (Exception error)
            {
                this.error(error);
            }
        }
        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>写入文件字节数</returns>
        private int writeFile(subArray<byte> data)
        {
            int count = data.Count, length = currentIndex + count;
            if (length < bufferLength)
            {
                Buffer.BlockCopy(data.Array, data.StartIndex, buffer, currentIndex, count);
                currentIndex = length;
                return 0;
            }
            byte[] dataArray = data.Array;
            int index = data.StartIndex;
            length = bufferLength - currentIndex;
            if (currentIndex == startIndex)
            {
                fileStream.Write(dataArray, index, length += ((count - length) / bufferLength) * bufferLength);
                index += length;
                count -= length;
            }
            else
            {
                Buffer.BlockCopy(dataArray, index, buffer, currentIndex, length);
                index += length;
                count -= length;
                fileStream.Write(buffer, startIndex, length = bufferLength - startIndex);
                int size = count / bufferLength;
                if (size != 0)
                {
                    fileStream.Write(dataArray, index, size *= bufferLength);
                    index += size;
                    count -= size;
                    length += size;
                }
            }
            Buffer.BlockCopy(dataArray, index, buffer, startIndex = 0, currentIndex = count);
            return length;
        }
        ///// <summary>
        ///// 同步写入文件
        ///// </summary>
        ///// <param name="fileName">文件名</param>
        ///// <param name="startIndex">读取文件起始位置</param>
        //internal void WriteFile(string fileName, int startIndex)
        //{
        //    if (File.Exists(fileName))
        //    {
        //        try
        //        {
        //            using (FileStream readFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLength, FileOptions.SequentialScan))
        //            {
        //                readFileStream.Seek(startIndex, SeekOrigin.Begin);
        //                int length = readFileStream.Read(buffer, currentIndex, bufferLength - currentIndex);
        //                fileBufferLength += length;
        //                if ((currentIndex += length) == bufferLength)
        //                {
        //                    fileStream.Write(buffer, this.startIndex, length = bufferLength - this.startIndex);
        //                    this.startIndex = 0;
        //                    fileLength += length;
        //                    do
        //                    {
        //                        currentIndex = readFileStream.Read(buffer, 0, bufferLength);
        //                        fileBufferLength += currentIndex;
        //                        if (currentIndex == bufferLength)
        //                        {
        //                            fileStream.Write(buffer, 0, bufferLength);
        //                            fileLength += currentIndex;
        //                        }
        //                        else break;
        //                    }
        //                    while (true);
        //                }
        //            }
        //        }
        //        catch (Exception error)
        //        {
        //            this.error(error);
        //        }
        //    }
        //}
        /// <summary>
        /// 等待缓存写入
        /// </summary>
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
        /// <returns>最后一次异常错误</returns>
        public Exception Flush(bool isDiskFile)
        {
            flush(true);
            if (isDiskFile)
            {
                FileStream fileStream = this.fileStream;
                if (fileStream != null) fileStream.Flush(true);
            }
            return LastException;
        }
        /// <summary>
        /// 设置刷新检测
        /// </summary>
        private void setCheckFlush()
        {
            if (Interlocked.CompareExchange(ref isCheckFlush, 1, 0) == 0)
            {
                if (checkFlushHandle == null) checkFlushHandle = checkFlush;
                timerTask.Default.Add(checkFlushHandle, checkFlushTime, null);
            }
        }
        /// <summary>
        /// 刷新检测
        /// </summary>
        private void checkFlush()
        {
            if (isWritting == 0)
            {
                if (checkFlushTime <= date.NowSecond)
                {
                    try
                    {
                        flush(false);
                    }
                    finally { isCheckFlush = 0; }
                }
                else timerTask.Default.Add(checkFlushHandle, checkFlushTime, null);
            }
            else isCheckFlush = 0;
        }
        /// <summary>
        /// 等待缓存写入
        /// </summary>
        /// <param name="isWait"></param>
        protected void flush(bool isWait)
        {
            if (LastException == null)
            {
                interlocked.NoCheckCompareSetSleep0(ref bufferLock);
                if (isWritting == 0 && fileBufferLength != fileLength)
                {
                    isWritting = isFlush = 1;
                    bufferLock = 0;
                    writeFile();
                    if (isWait)
                    {
                        Interlocked.Increment(ref flushCount);
                        flushWait.WaitOne();
                        Interlocked.Decrement(ref flushCount);
                    }
                }
                else bufferLock = 0;
            }
        }
        /// <summary>
        /// 等待缓存写入
        /// </summary>
        public void WaitWriteBuffer()
        {
            if (bufferSize > maxBufferSize)
            {
                Thread.Sleep(0);
                while (bufferSize > maxBufferSize) Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 写文件错误
        /// </summary>
        /// <param name="error">错误异常</param>
        private void error(Exception error)
        {
            if (isLog) log.Default.Add(error, null, false);
            LastException = error;
            interlocked.NoCheckCompareSetSleep0(ref bufferLock);
            byte isDisposed = this.isDisposed;
            currentIndex = startIndex;
            isWritting = 0;
            fileBufferLength = fileLength = 0;
            this.isDisposed = 1;
            bufferLock = 0;
            dispose();
            pub.Dispose(ref fileStream);
            list<memoryPool.pushSubArray> buffers = null;
            Interlocked.Exchange(ref buffers, this.buffers);
            try
            {
                if (buffers != null && buffers.Count != 0)
                {
                    memoryPool.pushSubArray[] dataArray = buffers.array;
                    for (int index = buffers.Count; index != 0; dataArray[--index].Push()) ;
                }
            }
            finally
            {
                currentBuffers = null;
                memoryPool.Push(ref buffer);
                if (isDisposed == 0)
                {
                    flushWait.Set();
                    flushWait.Close();
                }
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void dispose() { }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            interlocked.NoCheckCompareSetSleep0(ref bufferLock);
            if (isDisposed == 0)
            {
                isDisposed = 1;
                bufferLock = 0;
                while(LastException == null)
                {
                    Interlocked.Increment(ref flushCount);
                    flushWait.WaitOne();
                    Interlocked.Decrement(ref flushCount);
                    if (LastException == null)
                    {
                        interlocked.NoCheckCompareSetSleep0(ref bufferLock);
                        if (fileBufferLength == fileLength)
                        {
                            bufferLock = 0;
                            break;
                        }
                        bufferLock = 0;
                    }
                }
                dispose();
                pub.Dispose(ref fileStream);
                interlocked.NoCheckCompareSetSleep0(ref bufferLock);
                buffers.Null();
                currentBuffers.Null();
                bufferLock = 0;
                memoryPool.Push(ref buffer);
                flushWait.Set();
                flushWait.Close();
            }
            else bufferLock = 0;
        }
    }
}
