//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FileBlockStreamPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.IO
//	File Name:  FileBlockStreamPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:03:49
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.IO
{
    /// <summary>
    /// 文件分块写入流
    /// </summary>
    public sealed class FileBlockStreamPlus:FileStreamWriterPlus
    {
        /// <summary>
        /// 文件索引
        /// </summary>
        [fastCSharp.emit.sqlColumn]
        public struct index
        {
            /// <summary>
            /// 位置索引
            /// </summary>
            public long Index;
            /// <summary>
            /// 数据大小
            /// </summary>
            public int Size;
            /// <summary>
            /// 文件分块结束位置
            /// </summary>
            public long EndIndex
            {
                get { return Index + (Size + sizeof(int)); }
            }
            /// <summary>
            /// 清空数据
            /// </summary>
            public void Null()
            {
                Index = 0;
                Size = 0;
            }
            /// <summary>
            /// 重置文件索引
            /// </summary>
            /// <param name="index"></param>
            /// <param name="size"></param>
            /// <returns></returns>
            internal int ReSet(long index, int size)
            {
                if (Index == index)
                {
                    if (Size == size) return 1;
                }
                else Index = index;
                Size = size;
                return 0;
            }
        }
        /// <summary>
        /// 文件读取
        /// </summary>
        private sealed class reader
        {
            /// <summary>
            /// 读取文件位置
            /// </summary>
            private index index;
            /// <summary>
            /// 文件分块写入流
            /// </summary>
            public fileBlockStream FileStream;
            /// <summary>
            /// 读取文件回调函数
            /// </summary>
            private Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>, bool> onReaded;
            /// <summary>
            /// 下一个文件读取
            /// </summary>
            public reader Next;
            /// <summary>
            /// 内存池
            /// </summary>
            private memoryPool memoryPool;
            /// <summary>
            /// 文件数据缓冲区
            /// </summary>
            private byte[] buffer;
            /// <summary>
            /// 开始读取文件
            /// </summary>
            public Action ReadHandle { get; private set; }
            /// <summary>
            /// 文件分块结束位置
            /// </summary>
            public long EndIndex
            {
                get { return index.EndIndex; }
            }
            /// <summary>
            /// 文件读取
            /// </summary>
            private reader()
            {
                ReadHandle = Read;
            }
            /// <summary>
            /// 开始读取文件
            /// </summary>
            public unsafe void Read()
            {
                do
                {
                    int readSize = index.Size + sizeof(int);
                    try
                    {
                        if (FileStream.isDisposed == 0)
                        {
                            buffer = (memoryPool = memoryPool.GetPool(readSize)).Get();
                            FileStream fileReader = FileStream.fileReader;
                            long offset = fileReader.Position - index.Index;
                            if (offset >= 0 || -offset < index.Index) fileReader.Seek(offset, SeekOrigin.Current);
                            else fileReader.Seek(index.Index, SeekOrigin.Begin);
                            if (fileReader.Read(buffer, 0, readSize) == readSize)
                            {
                                fixed (byte* bufferFixed = buffer)
                                {
                                    if (*(int*)bufferFixed == index.Size) readSize = index.Size;
                                    else log.Default.Add(FileStream.FileName + " index[" + index.Index.toString() + "] size[" + (*(int*)bufferFixed).toString() + "]<>" + index.Size.toString(), false, false);
                                }
                            }
                            else readSize = 0;
                        }
                    }
                    catch (Exception error)
                    {
                        log.Default.Add(error, null, false);
                    }
                    Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>, bool> onReaded = this.onReaded;
                    if (readSize == index.Size)
                    {
                        if (onReaded(new fastCSharp.code.cSharp.tcpBase.subByteArrayEvent { Buffer = subArray<byte>.Unsafe(buffer, sizeof(int), index.Size), Event = memoryPool.PushSubArray })) buffer = null;
                        else memoryPool.Push(ref buffer);
                    }
                    else
                    {
                        onReaded(default(fastCSharp.code.cSharp.tcpBase.subByteArrayEvent));
                        if (memoryPool != null) memoryPool.Push(ref buffer);
                    }
                    reader next = FileStream.next(this);
                    if (next == null)
                    {
                        FileStream = null;
                        onReaded = null;
                        memoryPool = null;
                        typePool<reader>.Push(this);
                        return;
                    }
                    onReaded = next.onReaded;
                    index = next.index;
                    next.onReaded = null;
                    typePool<reader>.Push(next);
                }
                while (true);
            }
            /// <summary>
            /// 取消文件读取
            /// </summary>
            public void Cancel()
            {
                Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>, bool> onReaded = this.onReaded;
                this.onReaded = null;
                typePool<reader>.Push(this);
                onReaded(default(fastCSharp.code.cSharp.tcpBase.subByteArrayEvent));
            }
            /// <summary>
            /// 文件读取
            /// </summary>
            /// <param name="index">读取文件位置</param>
            /// <param name="onReaded">读取文件回调函数</param>
            /// <returns></returns>
            public static reader Get(index index, Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>, bool> onReaded)
            {
                reader reader = typePool<reader>.Pop();
                if (reader == null)
                {
                    try
                    {
                        reader = new reader();
                    }
                    catch { return null; }
                }
                reader.onReaded = onReaded;
                reader.index = index;
                return reader;
            }
        }
        /// <summary>
        /// 文件读取流
        /// </summary>
        private FileStream fileReader;
        /// <summary>
        /// 文件读取
        /// </summary>
        private reader currentReader;
        /// <summary>
        /// 等待缓存写入
        /// </summary>
        private Action<reader> waitHandle;
        /// <summary>
        /// 文件读取访问锁
        /// </summary>
        private int readerLock;
        /// <summary>
        /// 文件分块写入流
        /// </summary>
        /// <param name="fileName">文件全名</param>
        /// <param name="fileOption">附加选项</param>
        public fileBlockStream(string fileName, FileOptions fileOption = FileOptions.None)
            : base(fileName, File.Exists(fileName) ? FileMode.Open : FileMode.CreateNew, FileShare.Read, fileOption)
        {
            fileReader = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLength, fileOption);
            waitHandle = wait;
        }
        /// <summary>
        /// 设置文件读取
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private bool set(reader reader)
        {
            interlocked.NoCheckCompareSetSleep0(ref readerLock);
            if (currentReader == null)
            {
                currentReader = reader;
                readerLock = 0;
                reader.FileStream = this;
                return true;
            }
            currentReader.Next = reader;
            readerLock = 0;
            return false;
        }
        /// <summary>
        /// 读取下一个文件数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private reader next(reader reader)
        {
            interlocked.NoCheckCompareSetSleep0(ref readerLock);
            reader nextReader = reader.Next;
            if (nextReader == null) currentReader = null;
            else if ((reader.Next = nextReader.Next) == null) currentReader = reader;
            readerLock = 0;
            return nextReader;
        }
        /// <summary>
        /// 读取文件分块数据//showjim+cache
        /// </summary>
        /// <param name="index">文件分块数据位置</param>
        /// <param name="size">文件分块字节大小</param>
        /// <param name="onReaded"></param>
        internal unsafe void Read(index index, Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>, bool> onReaded)
        {
            if (onReaded == null) log.Error.Throw(log.exceptionType.Null);
            long endIndex = index.EndIndex;
            if (index.Size > 0 && ((int)index.Index & 3) == 0 && endIndex <= fileBufferLength)
            {
                if (endIndex <= fileLength)
                {
                    reader reader = reader.Get(index, onReaded);
                    if (reader != null)
                    {
                        if (set(reader)) fastCSharp.threading.threadPool.TinyPool.FastStart(reader.ReadHandle, null, null);
                        return;
                    }
                }
                else
                {
                    memoryPool memoryPool = null;
                    byte[] buffer = null;
                    int copyedSize = int.MinValue;
                    interlocked.NoCheckCompareSetSleep0(ref bufferLock);
                    if (isDisposed == 0)
                    {
                        if (index.Index >= bufferIndex)
                        {
                            index.Index -= bufferIndex;
                            try
                            {
                                buffer = (memoryPool = memoryPool.GetPool(index.Size)).Get(index.Size);
                                foreach (memoryPool.pushSubArray nextData in buffers.array)
                                {
                                    subArray<byte> data = nextData.SubArray;
                                    if (index.Index != 0)
                                    {
                                        if (index.Index >= data.Count)
                                        {
                                            index.Index -= data.Count;
                                            continue;
                                        }
                                        data.UnsafeSet(data.StartIndex + (int)index.Index, data.Count - (int)index.Index);
                                        index.Index = 0;
                                    }
                                    if (copyedSize < 0)
                                    {
                                        fixed (byte* dataFixed = data.array)
                                        {
                                            if (*(int*)(dataFixed + data.StartIndex) != index.Size) break;
                                        }
                                        if ((copyedSize = data.Count - sizeof(int)) == 0) continue;
                                        data.UnsafeSet(data.StartIndex + sizeof(int), copyedSize);
                                        copyedSize = 0;
                                    }
                                    int copySize = index.Size - copyedSize;
                                    if (data.Count >= copySize)
                                    {
                                        Buffer.BlockCopy(data.array, data.StartIndex, buffer, copyedSize, copySize);
                                        copyedSize = index.Size;
                                        break;
                                    }
                                    Buffer.BlockCopy(data.array, data.StartIndex, buffer, copyedSize, copySize);
                                    copyedSize += copySize;
                                }
                            }
                            catch (Exception error)
                            {
                                log.Default.Add(error, null, false);
                            }
                            finally { bufferLock = 0; }
                            if (copyedSize == index.Size)
                            {
                                onReaded(new fastCSharp.code.cSharp.tcpBase.subByteArrayEvent { Buffer = subArray<byte>.Unsafe(buffer, 0, index.Size), Event = memoryPool.PushSubArray });
                                return;
                            }
                        }
                        else
                        {
                            bufferLock = 0;
                            reader reader = reader.Get(index, onReaded);
                            if (reader != null)
                            {
                                fastCSharp.threading.threadPool.TinyPool.FastStart(waitHandle, reader, null, null);
                                return;
                            }
                        }
                    }
                    else bufferLock = 0;
                }
            }
            onReaded(default(fastCSharp.code.cSharp.tcpBase.subByteArrayEvent));
        }
        /// <summary>
        /// 等待缓存写入
        /// </summary>
        /// <param name="reader"></param>
        private void wait(reader reader)
        {
            long endIndex = reader.EndIndex;
            if (endIndex <= fileLength)
            {
                if (set(reader)) reader.Read();
                return;
            }
            flush(true);
            if (isDisposed == 0)
            {
                if (set(reader)) reader.Read();
            }
            else reader.Cancel();
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void dispose()
        {
            pub.Dispose(ref fileReader);
        }
    }
}
