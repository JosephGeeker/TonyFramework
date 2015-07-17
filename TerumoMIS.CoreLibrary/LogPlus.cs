//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: LogPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  LogPlus
//	User name:  C1400008
//	Location Time: 2015/7/8 16:58:08
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 日志处理
    /// </summary>
    public sealed class LogPlus:IDisposable
    {
        /// <summary>
        /// 异常类型
        /// </summary>
        public enum ExceptionTypeEnum
        {
            None,
            /// <summary>
            /// 关键值为空
            /// </summary>
            Null,
            /// <summary>
            /// 索引超出范围
            /// </summary>
            IndexOutOfRange,
            /// <summary>
            /// 操作不可用
            /// </summary>
            ErrorOperation,
        }
        /// <summary>
        /// 缓存类型
        /// </summary>
        public enum CacheTypeEnum
        {
            /// <summary>
            /// 不缓存
            /// </summary>
            None,
            /// <summary>
            /// 先进先出
            /// </summary>
            Queue,
            /// <summary>
            /// 最后一次
            /// </summary>
            Last,
        }
        /// <summary>
        /// 日志信息
        /// </summary>
        private sealed class DebugPlus
        {
            /// <summary>
            /// 调用堆栈
            /// </summary>
            public StackTrace StackTrace;
            /// <summary>
            /// 调用堆栈帧
            /// </summary>
            public StackFrame StackFrame;
            /// <summary>
            /// 提示信息
            /// </summary>
            public string Message;
            /// <summary>
            /// 错误异常
            /// </summary>
            public Exception Exception;
            /// <summary>
            /// 异常类型
            /// </summary>
            public ExceptionTypeEnum Type;
            /// <summary>
            /// 字符串
            /// </summary>
            public string ParseString;
            /// <summary>
            /// 字符串
            /// </summary>
            /// <returns>字符串</returns>
            public override string ToString()
            {
                if (ParseString == null)
                {
                    string stackFrameMethodTypeName = null, stackFrameMethodString = null, stackFrameFile = null, stackFrameLine = null, stackFrameColumn = null;
                    if (StackFrame != null)
                    {
                        var stackFrameMethod = StackFrame.GetMethod();
                        if (stackFrameMethod.ReflectedType != null)
                            stackFrameMethodTypeName = stackFrameMethod.ReflectedType.FullName;
                        stackFrameMethodString = stackFrameMethod.ToString();
                        stackFrameFile = StackFrame.GetFileName();
                        if (stackFrameFile != null)
                        {
                            stackFrameLine = StackFrame.GetFileLineNumber().ToString();
                            stackFrameColumn = StackFrame.GetFileColumnNumber().ToString();
                        }
                    }
                    var stackTrace = StackTrace == null ? null : StackTrace.ToString();
                    var exception = Exception == null ? null : Exception.ToString();
                    interlocked.NoCheckCompareSetSleep0(ref _toStringStreamLock);
                    try
                    {
                        if (Message != null)
                        {
                            ToStringStream.WriteNotNull(@"附加信息 : ");
                            ToStringStream.WriteNotNull(Message);
                        }
                        if (StackFrame != null)
                        {
                            ToStringStream.Write(@"堆栈帧信息 : ");
                            ToStringStream.WriteNotNull(stackFrameMethodTypeName);
                            ToStringStream.WriteNotNull(" + ");
                            ToStringStream.WriteNotNull(stackFrameMethodString);
                            if (stackFrameFile != null)
                            {
                                ToStringStream.WriteNotNull(" in ");
                                ToStringStream.WriteNotNull(stackFrameFile);
                                ToStringStream.WriteNotNull(" line ");
                                ToStringStream.WriteNotNull(stackFrameLine);
                                ToStringStream.WriteNotNull(" col ");
                                ToStringStream.Write(stackFrameColumn);
                            }
                        }
                        if (stackTrace != null)
                        {
                            ToStringStream.WriteNotNull(@"堆栈信息 : ");
                            ToStringStream.WriteNotNull(stackTrace);
                        }
                        if (exception != null)
                        {
                            ToStringStream.WriteNotNull(@"异常信息 : ");
                            ToStringStream.WriteNotNull(exception);
                        }
                        if (Type != ExceptionTypeEnum.None)
                        {
                            ToStringStream.WriteNotNull("异常类型 : ");
                            ToStringStream.WriteNotNull(Type.ToString());
                        }
                        ParseString = ToStringStream.ToString();
                    }
                    finally
                    {
                        ToStringStream.Clear();
                        _toStringStreamLock = 0;
                    }
                }
                return ParseString;
            }
        }
        /// <summary>
        /// 异常错误信息前缀
        /// </summary>
        public const string ExceptionPrefix = pub.fastCSharp + " Exception : ";
        /// <summary>
        /// 日志文件前缀
        /// </summary>
        public const string DefaultFilePrefix = "log_";
        /// <summary>
        /// 字符串转换流
        /// </summary>
        private static readonly charStream ToStringStream = new charStream();
        /// <summary>
        /// 字符串转换流访问锁
        /// </summary>
        private static int _toStringStreamLock;

        /// <summary>
        /// 日志文件流
        /// </summary>
        private fileStreamWriter _fileStream;
        /// <summary>
        /// 日志文件流访问锁
        /// </summary>
        private int _fileLock;
        /// <summary>
        /// 日志文件名
        /// </summary>
        private string _fileName;
        /// <summary>
        /// 日志文件名
        /// </summary>
        public string FileName
        {
            get { return _fileStream != null ? _fileStream.FileName : null; }
        }
        /// <summary>
        /// 日志缓存队列
        /// </summary>
        private readonly fifoPriorityQueue<hashString, bool> _cache = new fifoPriorityQueue<hashString, bool>();
        /// <summary>
        /// 最后一次输出缓存
        /// </summary>
        private hashString _lastCache;
        /// <summary>
        /// 最大缓存数量
        /// </summary>
        private readonly int _maxCacheCount;
        /// <summary>
        /// 日志缓存访问锁
        /// </summary>
        private int _cacheLock;
        /// <summary>
        /// 是否已经释放资源
        /// </summary>
        private int _isDisposed;
        /// <summary>
        /// 最大字节长度(小于等于0表示不限)
        /// </summary>
        public int MaxSize = fastCSharp.config.appSetting.MaxLogSize;
        /// <summary>
        /// 日志处理
        /// </summary>
        /// <param name="fileName">日志文件</param>
        /// <param name="maxCacheCount">最大缓存数量</param>
        public LogPlus(string fileName, int maxCacheCount)
        {
            if ((_fileName = fileName) != null)
            {
                _maxCacheCount = maxCacheCount <= 0 ? 1 : maxCacheCount;
                Open();
                if (_fileStream != null) fastCSharp.domainUnload.AddLast(Dispose);
            }
        }
        /// <summary>
        /// 打开日志文件
        /// </summary>
        private void Open()
        {
            try
            {
                _fileStream = new fileStreamWriter(_fileName, FileMode.OpenOrCreate, FileShare.Read, FileOptions.None, false, fastCSharp.config.appSetting.Encoding);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
            }
            if (_fileStream == null)
            {
                try
                {
                    if (File.Exists(_fileName))
                    {
                        _fileStream = new fileStreamWriter(io.file.MoveBakFileName(_fileName), FileMode.OpenOrCreate, FileShare.Read, FileOptions.None, false, fastCSharp.config.appSetting.Encoding);
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.ToString());
                }
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref _isDisposed) == 1)
            {
                fastCSharp.domainUnload.RemoveLast(Dispose, false);
                if (_fileStream != null)
                {
                    try
                    {
                        string fileName = _fileStream.FileName;
                        pub.Dispose(ref _fileStream);
                        io.file.MoveBak(fileName);
                    }
                    catch
                    {
                        pub.Dispose(ref _fileStream);
                    }
                }
            }
        }
        /// <summary>
        /// 日志信息写文件
        /// </summary>
        /// <param name="value">日志信息</param>
        private void Output(DebugPlus value)
        {
            if (_isDisposed == 0)
            {
                if (_fileStream == null)
                {
                    Console.WriteLine(@" " + date.NowSecond.toString() + " : " + value.ToString());
                }
                else
                {
                    memoryPool.pushSubArray data = _fileStream.GetBytes(@" " + date.NowSecond.toString() + " : " + value.ToString() + @" ");
                    if (Interlocked.CompareExchange(ref _fileLock, 1, 0) != 0)
                    {
                        Thread.Sleep(0);
                        while (Interlocked.CompareExchange(ref _fileLock, 1, 0) != 0) Thread.Sleep(1);
                    }
                    try
                    {
                        if (_fileStream.UnsafeWrite(data) >= MaxSize && MaxSize > 0) MoveBakBase();
                        else _fileStream.WaitWriteBuffer();
                    }
                    finally { _fileLock = 0; }
                }
            }
        }
        /// <summary>
        /// 日志信息写文件
        /// </summary>
        /// <param name="value">日志信息</param>
        private void RealOutput(DebugPlus value)
        {
            if (_isDisposed == 0)
            {
                if (_fileStream == null)
                {
                    Console.WriteLine(@" " + date.NowSecond.toString() + " : " + value.ToString());
                }
                else
                {
                    memoryPool.pushSubArray data = _fileStream.GetBytes(@" " + date.NowSecond.toString() + " : " + value.ToString() + @" ");
                    if (Interlocked.CompareExchange(ref _fileLock, 1, 0) != 0)
                    {
                        Thread.Sleep(0);
                        while (Interlocked.CompareExchange(ref _fileLock, 1, 0) != 0) Thread.Sleep(1);
                    }
                    try
                    {
                        if (_fileStream.UnsafeWrite(data) >= MaxSize && MaxSize > 0) MoveBakBase();
                        else _fileStream.Flush(true);
                    }
                    finally { _fileLock = 0; }
                }
            }
        }
        /// <summary>
        /// 检测缓存是否存在
        /// </summary>
        /// <param name="value">日志信息</param>
        /// <param name="isQueue">是否缓存队列</param>
        /// <returns>是否继续输出日志</returns>
        private bool CheckCache(DebugPlus value, bool isQueue)
        {
            hashString key = value.ToString();
            if (isQueue)
            {
                interlocked.NoCheckCompareSetSleep0(ref _cacheLock);
                try
                {
                    if (_cache.Get(key, false)) return false;
                    _cache.Set(key, true);
                    if (_cache.Count > _maxCacheCount) _cache.Pop();
                }
                finally { _cacheLock = 0; }
                return true;
            }
            if (key.Equals(_lastCache)) return false;
            _lastCache = key;
            return true;
        }
        /// <summary>
        /// 移动日志文件
        /// </summary>
        /// <returns>新的日志文件名称</returns>
        public string MoveBak()
        {
            if (_fileStream != null)
            {
                if (Interlocked.CompareExchange(ref _fileLock, 1, 0) != 0)
                {
                    Thread.Sleep(0);
                    while (Interlocked.CompareExchange(ref _fileLock, 1, 0) != 0) Thread.Sleep(1);
                }
                try
                {
                    return MoveBakBase();
                }
                finally { _fileLock = 0; }
            }
            return null;
        }
        /// <summary>
        /// 移动日志文件
        /// </summary>
        /// <returns>新的日志文件名称</returns>
        private string MoveBakBase()
        {
            string fileName = _fileStream.FileName, bakFileName = null;
            pub.Dispose(ref _fileStream);
            bakFileName = io.file.MoveBak(fileName);
            Open();
            return bakFileName;
        }
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="message">提示信息</param>
        /// <param name="isCache">是否缓存</param>
        public void Add(Exception error, string message = null, bool isCache = true)
        {
            Add(error, message, isCache ? CacheTypeEnum.Queue : CacheTypeEnum.None);
        }
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="message">提示信息</param>
        /// <param name="cacheTypeEnum">缓存类型</param>
        public void Add(Exception error, string message, CacheTypeEnum cacheTypeEnum)
        {
            if (error != null && error.Message.StartsWith(ExceptionPrefix, StringComparison.Ordinal)) error = null;
            if (error == null)
            {
                if (message != null) Add(message, true, cacheTypeEnum);
            }
            else
            {
                var value = new DebugPlus
                {
                    Exception = error,
                    Message = message
                };
                if (cacheTypeEnum == CacheTypeEnum.None || CheckCache(value, cacheTypeEnum == CacheTypeEnum.Queue)) Output(value);
            }
        }
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="isStackTrace">是否包含调用堆栈</param>
        /// <param name="isCache">是否缓存</param>
        public void Add(string message, bool isStackTrace = false, bool isCache = false)
        {
            Add(message, isStackTrace, isCache ? CacheTypeEnum.Queue : CacheTypeEnum.None);
        }
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="isStackTrace">是否包含调用堆栈</param>
        /// <param name="cacheTypeEnum">缓存类型</param>
        public void Add(string message, bool isStackTrace, CacheTypeEnum cacheTypeEnum)
        {
            DebugPlus value = new DebugPlus
            {
                StackTrace = isStackTrace ? new StackTrace() : null,
                StackFrame = isStackTrace ? null : new StackFrame(1),
                Message = message
            };
            if (cacheTypeEnum == CacheTypeEnum.None || CheckCache(value, cacheTypeEnum == CacheTypeEnum.Queue)) Output(value);
        }
        /// <summary>
        /// 实时添加日志
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="message">提示信息</param>
        /// <param name="isCache">是否缓存</param>
        public void Real(Exception error, string message = null, bool isCache = true)
        {
            Real(error, message, isCache ? CacheTypeEnum.Queue : CacheTypeEnum.None);
        }
        /// <summary>
        /// 实时添加日志
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="message">提示信息</param>
        /// <param name="cacheTypeEnum">缓存类型</param>
        public void Real(Exception error, string message, CacheTypeEnum cacheTypeEnum)
        {
            if (error != null && error.Message.StartsWith(ExceptionPrefix, StringComparison.Ordinal)) error = null;
            if (error == null)
            {
                if (message != null) Real(message, true, cacheTypeEnum);
            }
            else
            {
                var value = new DebugPlus
                {
                    Exception = error,
                    Message = message
                };
                if (cacheTypeEnum == CacheTypeEnum.None || CheckCache(value, cacheTypeEnum == CacheTypeEnum.Queue)) RealOutput(value);
            }
        }
        /// <summary>
        /// 实时添加日志
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="isStackTrace">是否包含调用堆栈</param>
        /// <param name="isCache">是否缓存</param>
        public void Real(string message, bool isStackTrace = false, bool isCache = false)
        {
            Real(message, isStackTrace, isCache ? CacheTypeEnum.Queue : CacheTypeEnum.None);
        }
        /// <summary>
        /// 实时添加日志
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="isStackTrace">是否包含调用堆栈</param>
        /// <param name="cacheTypeEnum">是否缓存</param>
        public void Real(string message, bool isStackTrace, CacheTypeEnum cacheTypeEnum)
        {
            var value = new DebugPlus
            {
                StackTrace = isStackTrace ? new StackTrace() : null,
                StackFrame = isStackTrace ? null : new StackFrame(1),
                Message = message
            };
            if (_cache == CacheTypeEnum.None || CheckCache(value, _cache == CacheTypeEnum.Queue)) RealOutput(value);
        }
        /// <summary>
        /// 添加日志并抛出异常
        /// </summary>
        /// <param name="error">异常类型</param>
        public void Throw(ExceptionTypeEnum error)
        {
            var value = new DebugPlus
            {
                StackTrace = new StackTrace(),
                Type = error
            };
            if (CheckCache(value, true)) Output(value);
            throw new Exception(ExceptionPrefix + value.ToString());
        }
        /// <summary>
        /// 添加日志并抛出异常
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="message">提示信息</param>
        /// <param name="isCache">是否缓存</param>
        public void Throw(Exception error, string message = null, bool isCache = true)
        {
            Throw(error, message, isCache ? CacheTypeEnum.Queue : CacheTypeEnum.None);
        }
        /// <summary>
        /// 添加日志并抛出异常
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="message">提示信息</param>
        /// <param name="cacheTypeEnum">是否缓存</param>
        public void Throw(Exception error, string message, CacheTypeEnum cacheTypeEnum)
        {
            if (error != null && error.Message.StartsWith(ExceptionPrefix, StringComparison.Ordinal)) error = null;
            if (error == null)
            {
                if (message != null) Throw(message, true, cacheTypeEnum);
            }
            else
            {
                var value = new DebugPlus
                {
                    Exception = error,
                    Message = message
                };
                if (cacheTypeEnum == CacheTypeEnum.None || CheckCache(value, cacheTypeEnum == CacheTypeEnum.Queue)) Output(value);
                throw error != null ? new Exception(ExceptionPrefix + message, error) : new Exception(ExceptionPrefix + message);
            }
        }
        /// <summary>
        /// 添加日志并抛出异常
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="isStackTrace">是否包含调用堆栈</param>
        /// <param name="isCache">是否缓存</param>
        public void Throw(string message, bool isStackTrace = false, bool isCache = false)
        {
            Throw(message, isStackTrace, isCache ? CacheTypeEnum.Queue : CacheTypeEnum.None);
        }
        /// <summary>
        /// 添加日志并抛出异常
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="isStackTrace">是否包含调用堆栈</param>
        /// <param name="cacheTypeEnum">是否缓存</param>
        public void Throw(string message, bool isStackTrace, CacheTypeEnum cacheTypeEnum)
        {
            var value = new DebugPlus
            {
                StackTrace = isStackTrace ? new StackTrace() : null,
                StackFrame = isStackTrace ? null : new StackFrame(1),
                Message = message
            };
            if (cacheTypeEnum == CacheTypeEnum.None || CheckCache(value, cacheTypeEnum == CacheTypeEnum.Queue)) Output(value);
            throw new Exception(ExceptionPrefix + message);
        }
        /// <summary>
        /// 实时添加日志并抛出异常
        /// </summary>
        /// <param name="error">异常类型</param>
        public void ThrowReal(ExceptionTypeEnum error)
        {
            var value = new DebugPlus
            {
                StackTrace = new StackTrace(),
                Type = error
            };
            if (CheckCache(value, true)) RealOutput(value);
            throw new Exception(ExceptionPrefix + value.ToString());
        }
        /// <summary>
        /// 实时添加日志并抛出异常
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="message">提示信息</param>
        /// <param name="isCache">是否缓存</param>
        public void ThrowReal(Exception error, string message = null, bool isCache = true)
        {
            ThrowReal(error, message, isCache ? CacheTypeEnum.Queue : CacheTypeEnum.None);
        }
        /// <summary>
        /// 实时添加日志并抛出异常
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="message">提示信息</param>
        /// <param name="cacheTypeEnum">是否缓存</param>
        public void ThrowReal(Exception error, string message, CacheTypeEnum cacheTypeEnum)
        {
            if (error != null && error.Message.StartsWith(ExceptionPrefix, StringComparison.Ordinal)) error = null;
            if (error == null)
            {
                if (message != null) ThrowReal(message, true, cacheTypeEnum);
            }
            else
            {
                var value = new DebugPlus
                {
                    Exception = error,
                    Message = message
                };
                if (cacheTypeEnum == CacheTypeEnum.None || CheckCache(value, cacheTypeEnum == CacheTypeEnum.Queue)) RealOutput(value);
                throw error != null ? new Exception(ExceptionPrefix + message, error) : new Exception(ExceptionPrefix + message);
            }
        }
        /// <summary>
        /// 实时添加日志并抛出异常
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="isStackTrace">是否包含调用堆栈</param>
        /// <param name="isCache">是否缓存</param>
        public void ThrowReal(string message, bool isStackTrace = false, bool isCache = false)
        {
            ThrowReal(message, isStackTrace, isCache ? CacheTypeEnum.Queue : CacheTypeEnum.None);
        }
        /// <summary>
        /// 实时添加日志并抛出异常
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="isStackTrace">是否包含调用堆栈</param>
        /// <param name="cacheTypeEnum">是否缓存</param>
        public void ThrowReal(string message, bool isStackTrace, CacheTypeEnum cacheTypeEnum)
        {
            var value = new DebugPlus
            {
                StackTrace = isStackTrace ? new StackTrace() : null,
                StackFrame = isStackTrace ? null : new StackFrame(1),
                Message = message
            };
            if (cacheTypeEnum == CacheTypeEnum.None || CheckCache(value, cacheTypeEnum == CacheTypeEnum.Queue)) RealOutput(value);
            throw new Exception(ExceptionPrefix + message);
        }
        /// <summary>
        /// 信息日志，一般用于辅助定位BUG
        /// </summary>
        public static readonly LogPlus Default;
        /// <summary>
        /// 重要错误日志，说明很可能存在BUG
        /// </summary>
        public static readonly LogPlus Error;
        static LogPlus()
        {
            Default = new LogPlus(config.appSetting.IsLogConsole ? null : config.appSetting.LogPath + DefaultFilePrefix + "default.txt", config.appSetting.MaxLogCacheCount);
            Error = config.appSetting.IsErrorLog ? new LogPlus(config.appSetting.IsLogConsole ? null : config.appSetting.LogPath + DefaultFilePrefix + "error.txt", config.appSetting.MaxLogCacheCount) : Default;
            if (config.appSetting.IsPoolDebug) Default.Add("对象池采用纠错模式", false, CacheTypeEnum.None);
            if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
