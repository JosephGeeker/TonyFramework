//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PhysicalOldPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.MemoryDataBase
//	File Name:  PhysicalOldPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:08:14
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

namespace TerumoMIS.CoreLibrary.MemoryDataBase
{
    /// <summary>
    /// 内存数据库物理层
    /// </summary>
    internal sealed class PhysicalOldPlus : IDisposable
    {
        /// <summary>
        /// 内存数据库缓冲区集合
        /// </summary>
        internal static readonly memoryPool Buffers =
            memoryPool.GetPool(fastCSharp.config.memoryDatabase.Default.BufferSize);

        /// <summary>
        /// 操作阶段
        /// </summary>
        internal enum step : byte
        {
            /// <summary>
            /// 等待创建文件
            /// </summary>
            Create,

            /// <summary>
            /// 文件创建中
            /// </summary>
            Creatting,

            /// <summary>
            /// 文件已打开
            /// </summary>
            Opened,

            /// <summary>
            /// 正在加载数据
            /// </summary>
            Loadding,

            /// <summary>
            /// 等待操作中
            /// </summary>
            Waitting,

            /// <summary>
            /// 关闭中
            /// </summary>
            Closing,

            /// <summary>
            /// 已正常关闭
            /// </summary>
            Closed
        }

        /// <summary>
        /// 文件串行读取器
        /// </summary>
        internal sealed class fileReader : IDisposable
        {
            /// <summary>
            /// 文件读取流
            /// </summary>
            private FileStream fileStream;

            /// <summary>
            /// 未读取文件长度
            /// </summary>
            private int currentLength;

            /// <summary>
            /// 文件读取缓冲
            /// </summary>
            private byte[] buffer;

            /// <summary>
            /// 文件读取缓冲
            /// </summary>
            private byte[] currentBuffer;

            /// <summary>
            /// 读取数据结果
            /// </summary>
            private subArray<byte> data;

            /// <summary>
            /// 是否释放资源
            /// </summary>
            private int isDisposed;

            /// <summary>
            /// 文件读取等待事件
            /// </summary>
            private readonly EventWaitHandle fileWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);

            /// <summary>
            /// 读取等待事件
            /// </summary>
            private readonly EventWaitHandle readWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            /// <summary>
            /// 文件读取器
            /// </summary>
            /// <param name="fileName">文件名称</param>
            public fileReader(string fileName)
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                    fastCSharp.config.appSetting.StreamBufferSize, FileOptions.SequentialScan);
                currentLength = (int) fileStream.Length;
                threadPool.TinyPool.FastStart(read, null, null);
            }

            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                isDisposed = 1;
                readWaitHandle.Set();
                fileWaitHandle.Set();
                readWaitHandle.Close();
                fileWaitHandle.Close();
            }

            /// <summary>
            /// 读取数据
            /// </summary>
            /// <param name="data">读取的数据</param>
            public void Read(ref subArray<byte> data)
            {
                if (isDisposed == 0)
                {
                    readWaitHandle.WaitOne();
                    data = this.data;
                    fileWaitHandle.Set();
                }
                else data.Null();
            }

            /// <summary>
            /// 读取数据
            /// </summary>
            private void read()
            {
                byte[] defaultBuffer = currentBuffer = fastCSharp.memoryPool.StreamBuffers.Get();
                int bufferLength = currentBuffer.Length;
                byte isError = 0;
                try
                {
                    while (currentLength >= bufferLength)
                    {
                        if (currentBuffer == null) currentBuffer = fastCSharp.memoryPool.StreamBuffers.Get();
                        fileWaitHandle.WaitOne();
                        if ((isDisposed | (fileStream.Read(currentBuffer, 0, bufferLength) ^ bufferLength)) == 0)
                        {
                            byte[] data = currentBuffer;
                            currentBuffer = buffer;
                            this.data.UnsafeSet(buffer = data, 0, bufferLength);
                            currentLength -= bufferLength;
                            readWaitHandle.Set();
                        }
                        else
                        {
                            isError = 1;
                            break;
                        }
                    }
                    if (isError == 0 && currentLength != 0)
                    {
                        if (currentBuffer == null) currentBuffer = new byte[currentLength];
                        fileWaitHandle.WaitOne();
                        if ((isDisposed | (fileStream.Read(currentBuffer, 0, currentLength) ^ currentLength)) == 0)
                        {
                            byte[] data = currentBuffer;
                            currentBuffer = buffer;
                            this.data.UnsafeSet(buffer = data, 0, currentLength);
                            readWaitHandle.Set();
                        }
                        else isError = 1;
                    }
                }
                catch (Exception error)
                {
                    isError = 1;
                    log.Error.Add(error, null, false);
                }
                finally
                {
                    if (isError == 0)
                    {
                        fileWaitHandle.WaitOne();
                        data.UnsafeSet(defaultBuffer, 0, 0);
                    }
                    else data.Null();
                    readWaitHandle.Set();
                    fileStream.Dispose();
                }
            }
        }

        /// <summary>
        /// 数据文件刷新
        /// </summary>
        internal sealed class dataFileRefresh : IDisposable
        {
            /// <summary>
            /// 数据库文件刷新超时周期
            /// </summary>
            private static readonly long timeOutTicks = fastCSharp.config.memoryDatabase.Default.RefreshTimeOutTicks;

            /// <summary>
            /// 超时异常
            /// </summary>
            private static readonly TimeoutException timeoutException = new TimeoutException();

            /// <summary>
            /// 内存数据库物理层
            /// </summary>
            private readonly physicalOld physical;

            /// <summary>
            /// 日志文件头长度
            /// </summary>
            public int LogHeaderDataLength { get; private set; }

            /// <summary>
            /// 最后一次操作时间
            /// </summary>
            private DateTime lastTime;

            ///// <summary>
            ///// 日志缓冲区
            ///// </summary>
            //private byte[] logBuffer;
            ///// <summary>
            ///// 日志缓冲区位置
            ///// </summary>
            //private int logBufferIndex;
            /// <summary>
            /// 备份文件名称
            /// </summary>
            public string BakFileName { get; private set; }

            /// <summary>
            /// 备份日志文件名称
            /// </summary>
            internal string BakLogFileName;

            /// <summary>
            /// 文件流写入器
            /// </summary>
            private readonly fileStreamWriter fileWriter;

            /// <summary>
            /// 等待文件写入结束
            /// </summary>
            private readonly object waitLock = new object();

            /// <summary>
            /// 是否已经释放资源
            /// </summary>
            private bool isDisposed;

            /// <summary>
            /// 是否检测超时
            /// </summary>
            private int isCheckTimeout;

            /// <summary>
            /// 最后一次异常错误
            /// </summary>
            public Exception LastException { get; private set; }

            /// <summary>
            /// 数据文件刷新
            /// </summary>
            /// <param name="physical">内存数据库物理层</param>
            /// <param name="logHeaderDataLength">日志文件头长度</param>
            public dataFileRefresh(physicalOld physical, int logHeaderDataLength)
            {
                this.physical = physical;
                LogHeaderDataLength = logHeaderDataLength;
                string fileName = physical.path + physical.fileName;
                BakFileName = fastCSharp.io.file.MoveBak(fileName);
                fileWriter = new fileStreamWriter(fileName, FileMode.CreateNew, FileShare.Read, FileOptions.None, null);
                lastTime = date.NowSecond;
            }

            /// <summary>
            /// 写入文件数据
            /// </summary>
            /// <param name="data">文件数据</param>
            /// <returns>是否成功</returns>
            public unsafe bool Write(subArray<byte> data)
            {
                try
                {
                    if (fileWriter.UnsafeWriteCopy(data) >= 0) return true;
                    dispose(fileWriter.LastException);
                }
                catch (Exception error)
                {
                    dispose(error);
                }
                return false;
            }

            /// <summary>
            /// 写入文件数据
            /// </summary>
            /// <param name="stream">文件数据流</param>
            /// <returns>是否成功</returns>
            public unsafe bool Write(unmanagedStream stream)
            {
                try
                {
                    if (fileWriter.UnsafeWrite(stream.Data, stream.Length) >= 0) return true;
                    dispose(fileWriter.LastException);
                }
                catch (Exception error)
                {
                    dispose(error);
                }
                return false;
            }

            ///// <summary>
            ///// 写入文件数据
            ///// </summary>
            ///// <param name="data">文件数据</param>
            ///// <param name="length">数据长度</param>
            ///// <returns>是否成功</returns>
            //private unsafe bool write(byte* data, int length)
            //{
            //    lastTime = date.NowSecond;
            //    try
            //    {
            //        if (logBufferIndex == 0) return writeNext(data, length);
            //        int bufferLength = logBuffer.Length - logBufferIndex;
            //        fixed (byte* bufferFixed = logBuffer)
            //        {
            //            if ((length -= bufferLength) >= 0)
            //            {
            //                unsafer.memory.Copy(data, bufferFixed + logBufferIndex, bufferLength);
            //                if (fileWriter.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(logBuffer, 0, Buffers.Size), PushPool = Buffers.PushHandle }) >= 0)
            //                {
            //                    if (length == 0)
            //                    {
            //                        logBuffer = null;
            //                        logBufferIndex = 0;
            //                        return true;
            //                    }
            //                    return writeNext(data + bufferLength, length);
            //                }
            //                dispose(fileWriter.LastException);
            //            }
            //            else
            //            {
            //                unsafer.memory.Copy(data, bufferFixed + logBufferIndex, length += bufferLength);
            //                logBufferIndex += length;
            //                return true;
            //            }
            //        }
            //    }
            //    catch (Exception error)
            //    {
            //        dispose(error);
            //    }
            //    return false;
            //}
            ///// <summary>
            ///// 写入文件数据
            ///// </summary>
            ///// <param name="data">文件数据</param>
            ///// <param name="length">数据长度</param>
            ///// <returns>是否成功</returns>
            //private unsafe bool writeNext(byte* data, int length)
            //{
            //    while ((length -= Buffers.Size) >= 0)
            //    {
            //        byte[] buffer = Buffers.Get();
            //        unsafer.memory.Copy(data, buffer, Buffers.Size);
            //        if (fileWriter.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(logBuffer, 0, Buffers.Size), PushPool = Buffers.PushHandle }) < 0)
            //        {
            //            dispose(fileWriter.LastException);
            //            return false;
            //        }
            //        data += Buffers.Size;
            //    }
            //    logBufferIndex = length + Buffers.Size;
            //    if (logBufferIndex == 0) logBuffer = null;
            //    else unsafer.memory.Copy(data, logBuffer = Buffers.Get(), logBufferIndex);
            //    return true;
            //}
            /// <summary>
            /// 取消数据文件刷新
            /// </summary>
            public void CancelTimeOut()
            {
                if (!isDisposed)
                {
                    DateTime lastTime = this.lastTime, outTime = lastTime.AddTicks(timeOutTicks);
                    if (outTime < date.NowSecond) dispose(timeoutException);
                    else
                    {
                        if (Interlocked.Increment(ref isCheckTimeout) == 1)
                        {
                            timerTask.Default.Add(cancelTimeOut, lastTime, outTime, null);
                        }
                        Monitor.Enter(waitLock);
                        try
                        {
                            if (!isDisposed) Monitor.Wait(waitLock);
                        }
                        finally
                        {
                            Monitor.Exit(waitLock);
                        }
                    }
                }
            }

            /// <summary>
            /// 取消数据文件刷新
            /// </summary>
            /// <param name="lastTime">最后一次操作时间</param>
            private void cancelTimeOut(DateTime lastTime)
            {
                if (lastTime == this.lastTime) dispose(timeoutException);
                else if (!isDisposed)
                {
                    DateTime outTime = (lastTime = this.lastTime).AddTicks(timeOutTicks);
                    if (outTime < date.NowSecond) dispose(timeoutException);
                    else timerTask.Default.Add(cancelTimeOut, lastTime, outTime, null);
                }
            }

            /// <summary>
            /// 释放资源
            /// </summary>
            /// <param name="exception">错误异常</param>
            private void dispose(Exception exception)
            {
                LastException = exception;
                Dispose();
            }

            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                if (!isDisposed)
                {
                    Monitor.Enter(waitLock);
                    try
                    {
                        if (!isDisposed)
                        {
                            isDisposed = true;
                            //if (LastException != null && logBufferIndex != 0)
                            //{
                            //    try
                            //    {
                            //        if (fileWriter.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(logBuffer, 0, logBufferIndex), PushPool = Buffers.PushHandle }) < 0) LastException = fileWriter.LastException;
                            //    }
                            //    catch (Exception error)
                            //    {
                            //        LastException = error;
                            //    }
                            //    logBuffer = null;
                            //    logBufferIndex = 0;
                            //}
                            fileWriter.Dispose();
                            physical.RefreshEnd(this);
                            Monitor.PulseAll(waitLock);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(waitLock);
                    }
                }
            }
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        private readonly string path;

        /// <summary>
        /// 文件名
        /// </summary>
        private readonly string fileName;

        /// <summary>
        /// 日志文件名
        /// </summary>
        private string logFileName
        {
            get { return fileName + ".log"; }
        }

        /// <summary>
        /// 数据文件字节数
        /// </summary>
        public long FileSize
        {
            get
            {
                FileInfo file = new FileInfo(path + this.fileName);
                return file.Exists ? file.Length : -1;
            }
        }

        /// <summary>
        /// 日志文件流写入器
        /// </summary>
        private fileStreamWriter logFileWriter;

        /// <summary>
        /// 日志文件访问锁
        /// </summary>
        private int logFileLock;

        /// <summary>
        /// 日志文件字节数
        /// </summary>
        public long LogFileSize
        {
            get { return logFileWriter.FileSize; }
        }

        ///// <summary>
        ///// 日志缓冲区
        ///// </summary>
        //private byte[] logBuffer;
        ///// <summary>
        ///// 日志缓冲区位置
        ///// </summary>
        //private int logBufferIndex;
        /// <summary>
        /// 当前数据文件刷新
        /// </summary>
        private dataFileRefresh dataRefresh;

        /// <summary>
        /// 数据文件刷新访问锁
        /// </summary>
        private readonly object refreshLock = new object();

        /// <summary>
        /// 当前操作阶段
        /// </summary>
        private int currentStep;

        /// <summary>
        /// 当前操作阶段
        /// </summary>
        public physicalOld.step CurrentStep
        {
            get { return (physicalOld.step) currentStep; }
            private set { currentStep = (int) value; }
        }

        /// <summary>
        /// 最后产生的异常错误
        /// </summary>
        private Exception lastException;

        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        public bool IsDisposed
        {
            get { return isDisposed != 0; }
        }

        /// <summary>
        /// 数据文件刷新是否失败等待日志写入
        /// </summary>
        private bool isRefreshErrorLog;

        /// <summary>
        /// 内存数据库物理层
        /// </summary>
        /// <param name="fileName">数据文件名</param>
        public physicalOld(string fileName)
        {
            try
            {
                FileInfo file = new FileInfo(fileName + ".fmd");
                path = file.Directory.fullName();
                this.fileName = file.Name;
                bool isOpen = file.Exists;
                logFileWriter = new fileStreamWriter(path + logFileName, isOpen ? FileMode.Open : FileMode.CreateNew,
                    FileShare.Read, FileOptions.None, null);
                CurrentStep = isOpen ? physicalOld.step.Opened : physicalOld.step.Waitting;
            }
            catch (Exception error)
            {
                lastException = error;
                Dispose();
            }
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="header">文件头数据</param>
        /// <returns>是否成功</returns>
        public bool Create(subArray<byte> header)
        {
            if (currentStep == (int) physicalOld.step.Waitting)
            {
                string fileName = path + this.fileName;
                try
                {
                    if (!File.Exists(fileName))
                    {
                        using (
                            FileStream file = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write,
                                FileShare.None, 1, FileOptions.WriteThrough))
                        {
                            file.Write(header.Array, header.StartIndex, header.Count);
                        }
                        return true;
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                Dispose();
            }
            return false;
        }

        /// <summary>
        /// 获取数据文件加载器
        /// </summary>
        /// <returns>数据文件加载器</returns>
        internal fileReader GetLoader()
        {
            string fileName = null;
            if (
                Interlocked.CompareExchange(ref currentStep, (int) physicalOld.step.Loadding,
                    (int) physicalOld.step.Opened) == (int) physicalOld.step.Opened)
            {
                fileName = this.fileName;
            }
            else if (currentStep == (int) physicalOld.step.Loadding) fileName = logFileName;
            if (fileName != null)
            {
                try
                {
                    return new fileReader(path + fileName);
                }
                catch (Exception error)
                {
                    lastException = error;
                    Dispose();
                }
            }
            return null;
        }

        /// <summary>
        /// 数据加载错误
        /// </summary>
        private static readonly Exception loadException = new Exception("数据加载错误");

        /// <summary>
        /// 数据加载完成
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Loaded()
        {
            if (
                Interlocked.CompareExchange(ref currentStep, (int) physicalOld.step.Waitting,
                    (int) physicalOld.step.Loadding) == (int) physicalOld.step.Loadding) return true;
            lastException = loadException;
            Dispose();
            return false;
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="data">日志数据</param>
        /// <returns>是否成功写入缓冲区</returns>
        public unsafe bool AppendLog(memoryPool.pushSubArray data)
        {
            if (currentStep == (int) physicalOld.step.Waitting)
            {
                byte isCopy = 0, isLog = 0;
                if (Interlocked.CompareExchange(ref logFileLock, 1, 0) != 0)
                {
                    do
                    {
                        Thread.Sleep(0);
                        if (Interlocked.CompareExchange(ref logFileLock, 1, 0) == 0) break;
                        if (isRefreshErrorLog) Thread.Sleep(1);
                        else Thread.Sleep(0);
                    } while (Interlocked.CompareExchange(ref logFileLock, 1, 0) != 0);
                }
                try
                {
                    if (logFileWriter.UnsafeWrite(data) >= 0) return true;
                    lastException = logFileWriter.LastException;

                    //subArray<byte> dataArray = data.Value;
                    //int length = logBufferIndex + dataArray.Length;
                    //if (length <= Buffers.Size)
                    //{
                    //    if (logBuffer == null) logBuffer = Buffers.Get();
                    //    fixed (byte* dataFixed = dataArray.Array, bufferFixed = logBuffer)
                    //    {
                    //        unsafer.memory.Copy(dataFixed + dataArray.StartIndex, bufferFixed + logBufferIndex, dataArray.Length);
                    //    }
                    //    logBufferIndex = length;
                    //    isCopy = isLog = 1;
                    //}
                    //else if (logBufferIndex == 0)
                    //{
                    //    if (logFileWriter.UnsafeWrite(data) >= 0) isLog = 1;
                    //    else lastException = logFileWriter.LastException;
                    //}
                    //else
                    //{
                    //    fixed (byte* dataFixed = dataArray.Array, bufferFixed = logBuffer)
                    //    {
                    //        unsafer.memory.Copy(dataFixed + dataArray.StartIndex, bufferFixed + logBufferIndex, length = Buffers.Size - logBufferIndex);
                    //        dataArray.UnsafeSet(dataArray.StartIndex + length, dataArray.Length - length);
                    //        if (logFileWriter.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(logBuffer, 0, Buffers.Size), PushPool = Buffers.PushHandle }) >= 0)
                    //        {
                    //            if (dataArray.Length <= Buffers.Size)
                    //            {
                    //                fixed (byte* newBufferFixed = logBuffer = Buffers.Get())
                    //                {
                    //                    unsafer.memory.Copy(dataFixed + dataArray.StartIndex, newBufferFixed, logBufferIndex = dataArray.Length);
                    //                }
                    //                isCopy = isLog = 1;
                    //            }
                    //            else if (logFileWriter.UnsafeWrite(data) >= 0)
                    //            {
                    //                logBufferIndex = 0;
                    //                logBuffer = null;
                    //                isLog = 1;
                    //            }
                    //            else lastException = logFileWriter.LastException;
                    //        }
                    //        else
                    //        {
                    //            isCopy = 1;
                    //            lastException = logFileWriter.LastException;
                    //        }
                    //    }
                    //}
                }
                catch (Exception error)
                {
                    lastException = error;
                }
                finally
                {
                    logFileLock = 0;
                }
                if (isCopy != 0) data.Push();
                if (isLog != 0) return true;
                Dispose();
            }
            return false;
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="data">日志数据</param>
        /// <returns>是否成功写入缓冲区</returns>
        public unsafe bool AppendLogBuffer(subArray<byte> data)
        {
            if (currentStep == (int) physicalOld.step.Waitting)
            {
                fixed (byte* dataFixed = data.Array)
                {
                    if (Interlocked.CompareExchange(ref logFileLock, 1, 0) != 0)
                    {
                        do
                        {
                            Thread.Sleep(0);
                            if (Interlocked.CompareExchange(ref logFileLock, 1, 0) == 0) break;
                            if (isRefreshErrorLog) Thread.Sleep(1);
                            else Thread.Sleep(0);
                        } while (Interlocked.CompareExchange(ref logFileLock, 1, 0) != 0);
                    }
                    try
                    {
                        if (logFileWriter.UnsafeWriteCopy(data) >= 0) return true;
                        lastException = logFileWriter.LastException;

                        //int length = logBufferIndex + data.Length;
                        //if (length <= Buffers.Size)
                        //{
                        //    if (logBuffer == null) logBuffer = Buffers.Get();
                        //    fixed (byte* bufferFixed = logBuffer)
                        //    {
                        //        unsafer.memory.Copy(dataFixed + data.StartIndex, bufferFixed + logBufferIndex, data.Length);
                        //    }
                        //    logBufferIndex = length;
                        //    return true;
                        //}
                        //else
                        //{
                        //    long fileIndex = 0;
                        //    int startIndex = data.StartIndex, dataLenth = data.Length;
                        //    if (logBufferIndex != 0)
                        //    {
                        //        fixed (byte* bufferFixed = logBuffer)
                        //        {
                        //            unsafer.memory.Copy(dataFixed + startIndex, bufferFixed + logBufferIndex, length = Buffers.Size - logBufferIndex);
                        //            startIndex += length;
                        //            dataLenth -= length;
                        //            fileIndex = logFileWriter.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(logBuffer, 0, Buffers.Size), PushPool = Buffers.PushHandle });
                        //        }
                        //    }
                        //    if (fileIndex >= 0)
                        //    {
                        //        for (logBuffer = Buffers.TryGet(); logBuffer != null; logBuffer = Buffers.TryGet())
                        //        {
                        //            if (dataLenth <= Buffers.Size)
                        //            {
                        //                unsafer.memory.Copy(dataFixed + startIndex, logBuffer, logBufferIndex = dataLenth);
                        //                return true;
                        //            }
                        //            unsafer.memory.Copy(dataFixed + startIndex, logBuffer, Buffers.Size);
                        //            startIndex += Buffers.Size;
                        //            dataLenth -= Buffers.Size;
                        //            fileIndex = logFileWriter.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(logBuffer, 0, Buffers.Size), PushPool = Buffers.PushHandle });
                        //            if (fileIndex < 0) break;
                        //        }
                        //        if (logBuffer == null)
                        //        {
                        //            if (dataLenth <= Buffers.Size)
                        //            {
                        //                unsafer.memory.Copy(dataFixed + startIndex, logBuffer = Buffers.Get(), logBufferIndex = dataLenth);
                        //                return true;
                        //            }
                        //            byte[] newData = new byte[dataLenth];
                        //            unsafer.memory.Copy(dataFixed + startIndex, newData, dataLenth);
                        //            if (logFileWriter.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(newData, 0, dataLenth) }) >= 0)
                        //            {
                        //                logBufferIndex = 0;
                        //                return true;
                        //            }
                        //        }
                        //    }
                        //    lastException = logFileWriter.LastException;
                        //}
                    }
                    catch (Exception error)
                    {
                        lastException = error;
                    }
                    finally
                    {
                        logFileLock = 0;
                    }
                }
                Dispose();
            }
            return false;
        }

        ///// <summary>
        ///// 写入缓存
        ///// </summary>
        //private void flushLog()
        //{
        //    if (logBufferIndex != 0)
        //    {
        //        try
        //        {
        //            if (logFileWriter.UnsafeWrite(new memoryPool.pushSubArray { Value = subArray<byte>.Unsafe(logBuffer, 0, logBufferIndex), PushPool = Buffers.PushHandle }) >= 0)
        //            {
        //                logBuffer = null;
        //                logBufferIndex = 0;
        //                return;
        //            }
        //            lastException = logFileWriter.LastException;
        //        }
        //        catch (Exception error)
        //        {
        //            lastException = error;
        //        }
        //    }
        //}
        /// <summary>
        /// 获取数据文件刷新
        /// </summary>
        /// <param name="logHeaderData">日志文件标识</param>
        /// <returns>数据文件刷新,失败返回null</returns>
        public dataFileRefresh CreateRefresh(byte[] logHeaderData)
        {
            if (currentStep == (int) physicalOld.step.Waitting && this.dataRefresh == null)
            {
                string logFileName = path + this.logFileName;
                dataFileRefresh dataRefresh = new dataFileRefresh(this, logHeaderData.Length);
                Monitor.Enter(refreshLock);
                try
                {
                    if (this.dataRefresh == null)
                    {
                        interlocked.CompareSetSleep0(ref logFileLock);
                        try
                        {
                            //flushLog();
                            logFileWriter.Flush(true);
                            logFileWriter.Dispose();
                            logFileWriter = null;
                            dataRefresh.BakLogFileName = fastCSharp.io.file.MoveBak(logFileName);
                            logFileWriter = new fileStreamWriter(logFileName, FileMode.CreateNew, FileShare.Read,
                                FileOptions.None, null);
                            logFileWriter.UnsafeWrite(logHeaderData, 0, logHeaderData.Length);
                        }
                        finally
                        {
                            logFileLock = 0;
                        }
                        this.dataRefresh = dataRefresh;
                    }
                }
                catch (Exception error)
                {
                    lastException = error;
                    Dispose();
                }
                finally
                {
                    Monitor.Exit(refreshLock);
                }
                if (dataRefresh.Equals(this.dataRefresh)) return dataRefresh;
                dataRefresh.Dispose();
            }
            return null;
        }

        /// <summary>
        /// 数据文件刷新结束
        /// </summary>
        /// <param name="dataRefresh">数据文件刷新结束</param>
        public void RefreshEnd(dataFileRefresh dataRefresh)
        {
            if (dataRefresh.Equals(this.dataRefresh))
            {
                bool isDeleteBakFile = false;
                Monitor.Enter(refreshLock);
                try
                {
                    if (dataRefresh.Equals(this.dataRefresh))
                    {
                        this.dataRefresh = null;
                        if (dataRefresh.LastException == null) isDeleteBakFile = true;
                        else
                        {
                            lastException = dataRefresh.LastException;
                            if (File.Exists(dataRefresh.BakFileName) && File.Exists(dataRefresh.BakLogFileName))
                            {
                                try
                                {
                                    string fileName = path + this.fileName, errorFileName = fileName + ".error";
                                    if (File.Exists(fileName))
                                    {
                                        file.MoveBak(errorFileName);
                                        File.Move(fileName, errorFileName);
                                    }
                                    File.Move(dataRefresh.BakFileName, fileName);
                                    isRefreshErrorLog = true;
                                    interlocked.CompareSetSleep0(ref logFileLock);
                                    try
                                    {
                                        //flushLog();
                                        logFileWriter.Flush(true);
                                        logFileWriter.Dispose();
                                        if (logFileWriter.LastException == null)
                                        {
                                            string logFileName = path + this.logFileName,
                                                bakLogFileName = file.MoveBak(logFileName);
                                            File.Move(dataRefresh.BakLogFileName, logFileName);
                                            logFileWriter = new fileStreamWriter(logFileName, FileMode.Open,
                                                FileShare.Read, FileOptions.None, null);
                                            logFileWriter.WriteFile(bakLogFileName, dataRefresh.LogHeaderDataLength);
                                        }
                                        if (logFileWriter.LastException == null)
                                        {
                                            log.Default.Add(lastException, "数据文件已恢复 " + fileName, false);
                                            lastException = null;
                                        }
                                        else log.Error.Add(logFileWriter.LastException, null, false);
                                    }
                                    finally
                                    {
                                        logFileLock = 0;
                                        isRefreshErrorLog = false;
                                    }
                                }
                                catch (Exception error)
                                {
                                    log.Error.Add(error, null, false);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(refreshLock);
                    if (lastException != null) Dispose();
                    if (isDeleteBakFile)
                    {
                        try
                        {
                            if (File.Exists(dataRefresh.BakFileName)) File.Delete(dataRefresh.BakFileName);
                            if (File.Exists(dataRefresh.BakLogFileName)) File.Delete(dataRefresh.BakLogFileName);
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 写入缓存
        /// </summary>
#if MONO
    /// <param name="isDiskFile">是否写入到磁盘文件,MONO无效</param>
#else
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
#endif
        /// <returns>是否成功</returns>
        public bool Flush(bool isDiskFile)
        {
            if (currentStep == (int) physicalOld.step.Waitting)
            {
                if (isRefreshErrorLog) interlocked.CompareSetSleep1(ref logFileLock);
                else interlocked.CompareSetSleep0(ref logFileLock);
                try
                {
                    //flushLog();
                    if (lastException == null)
                    {
                        Exception error = logFileWriter.Flush(isDiskFile);
                        if (error == null) return true;
                        lastException = error;
                    }
                }
                catch (Exception error)
                {
                    lastException = error;
                }
                finally
                {
                    logFileLock = 0;
                }
                Dispose();
            }
            return false;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                Interlocked.Exchange(ref currentStep, (int) physicalOld.step.Closing);
                if (isRefreshErrorLog) interlocked.CompareSetSleep1(ref logFileLock);
                else interlocked.CompareSetSleep0(ref logFileLock);
                try
                {
                    //if (lastException == null) flushLog();
                    if (dataRefresh != null)
                    {
                        isRefreshErrorLog = true;
                        dataRefresh.CancelTimeOut();
                        dataRefresh = null;
                        isRefreshErrorLog = false;
                    }
                    try
                    {
                        logFileWriter.Flush(true);
                        logFileWriter.Dispose();
                    }
                    catch (Exception error)
                    {
                        lastException = error;
                    }
                    //logBufferIndex = 0;
                    //Buffers.Push(ref logBuffer);
                }
                finally
                {
                    logFileLock = 0;
                }
                Interlocked.Exchange(ref currentStep, (int) physicalOld.step.Closed);
                if (lastException != null) log.Error.Add(lastException, fileName, false);
            }
        }
    }
}

