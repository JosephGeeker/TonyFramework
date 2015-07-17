//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CreateFileTimeoutWatcherPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.IO
//	File Name:  CreateFileTimeoutWatcherPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:57:51
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
    /// 新建文件监视
    /// </summary>
    public sealed class CreateFileTimeoutWatcherPlus:IDisposable
    {
        /// <summary>
        /// 监视计数
        /// </summary>
        private struct counter
        {
            /// <summary>
            /// 文件监视器
            /// </summary>
            public FileSystemWatcher Watcher;
            /// <summary>
            /// 监视计数
            /// </summary>
            public int Count;
            /// <summary>
            /// 文件监视器初始化
            /// </summary>
            /// <param name="path">监视路径</param>
            /// <param name="onCreated">新建文件处理</param>
            public void Create(string path, FileSystemEventHandler onCreated)
            {
                Watcher = new FileSystemWatcher(path);
                Watcher.IncludeSubdirectories = false;
                Watcher.EnableRaisingEvents = true;
                Watcher.Created += onCreated;
                Count = 1;
            }
        }
        /// <summary>
        /// 超时检测时钟周期
        /// </summary>
        private long timeoutTicks;
        /// <summary>
        /// 超时处理
        /// </summary>
        private Action onTimeout;
        /// <summary>
        /// 新建文件前置过滤
        /// </summary>
        private Func<FileSystemEventArgs, bool> filter;
        /// <summary>
        /// 新建文件处理
        /// </summary>
        private FileSystemEventHandler onCreatedHandle;
        /// <summary>
        /// 超时检测处理
        /// </summary>
        private Action checkTimeoutHandle;
        /// <summary>
        /// 文件监视器集合
        /// </summary>
        private Dictionary<hashString, counter> watchers;
        /// <summary>
        /// 超时检测文件集合
        /// </summary>
        private subArray<keyValue<FileInfo, DateTime>> files;
        /// <summary>
        /// 文件监视器集合访问锁
        /// </summary>
        private int watcherLock;
        /// <summary>
        /// 超时检测文件集合访问锁
        /// </summary>
        private readonly object fileLock = new object();
        /// <summary>
        /// 超时检测访问锁
        /// </summary>
        private int checkLock;
        /// <summary>
        /// 是否释放资源
        /// </summary>
        private int isDisposed;
        /// <summary>
        /// 新建文件监视
        /// </summary>
        /// <param name="seconds">超时检测秒数</param>
        /// <param name="onTimeout">超时处理</param>
        /// <param name="filter">新建文件前置过滤</param>
        public createFlieTimeoutWatcher(int seconds, Action onTimeout, Func<FileSystemEventArgs, bool> filter = null)
        {
            if (onTimeout == null) log.Error.Throw(log.exceptionType.Null);
            timeoutTicks = new TimeSpan(0, 0, seconds > 0 ? (seconds + 1) : 2).Ticks;
            this.onTimeout = onTimeout;
            this.filter = filter;
            onCreatedHandle = filter == null ? (FileSystemEventHandler)onCreated : onCreatedFilter;
            checkTimeoutHandle = checkTimeout;
            watchers = dictionary.CreateHashString<counter>();
        }
        /// <summary>
        /// 添加监视路径
        /// </summary>
        /// <param name="path">监视路径</param>
        public void Add(string path)
        {
            if (isDisposed == 0)
            {
                path = path.ToLower();
                counter counter;
                hashString pathKey = path;
                interlocked.NoCheckCompareSetSleep0(ref watcherLock);
                try
                {
                    if (watchers.TryGetValue(pathKey, out counter))
                    {
                        ++counter.Count;
                        watchers[pathKey] = counter;
                    }
                    else
                    {
                        counter.Create(path, onCreatedHandle);
                        watchers.Add(pathKey, counter);
                    }
                }
                finally { watcherLock = 0; }
            }
        }
        /// <summary>
        /// 删除监视路径
        /// </summary>
        /// <param name="path">监视路径</param>
        public void Remove(string path)
        {
            path = path.ToLower();
            counter counter;
            hashString pathKey = path;
            interlocked.NoCheckCompareSetSleep0(ref watcherLock);
            try
            {
                if (watchers.TryGetValue(pathKey, out counter))
                {
                    if (--counter.Count == 0) watchers.Remove(pathKey);
                    else watchers[pathKey] = counter;
                }
            }
            finally { watcherLock = 0; }
            if (counter.Count == 0 && counter.Watcher != null) dispose(counter.Watcher);
        }
        /// <summary>
        /// 新建文件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCreated(object sender, FileSystemEventArgs e)
        {
            onCreated(e);
        }
        /// <summary>
        /// 新建文件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCreatedFilter(object sender, FileSystemEventArgs e)
        {
            if (filter(e)) onCreated(e);
        }
        /// <summary>
        /// 新建文件处理
        /// </summary>
        /// <param name="e"></param>
        private void onCreated(FileSystemEventArgs e)
        {
            FileInfo file = new FileInfo(e.FullPath);
            if (file.Exists)
            {
                DateTime timeout = date.NowSecond.AddTicks(timeoutTicks);
                Monitor.Enter(fileLock);
                try
                {
                    files.Add(new keyValue<FileInfo, DateTime>(file, timeout));
                }
                finally { Monitor.Exit(fileLock); }
                if (Interlocked.CompareExchange(ref checkLock, 1, 0) == 0) timerTask.Default.Add(checkTimeoutHandle, date.NowSecond.AddTicks(timeoutTicks), null);
            }
        }
        /// <summary>
        /// 超时检测处理
        /// </summary>
        private void checkTimeout()
        {
            DateTime now = date.NowSecond;
            int index = 0;
            Monitor.Enter(fileLock);
            int count = files.Count;
            keyValue<FileInfo, DateTime>[] fileArray = files.array;
            try
            {
                while (index != count)
                {
                    keyValue<FileInfo, DateTime> fileTime = fileArray[index];
                    if (fileTime.Value <= now)
                    {
                        FileInfo file = fileTime.Key;
                        long length = file.Length;
                        file.Refresh();
                        if (file.Exists)
                        {
                            if (length == file.Length)
                            {
                                try
                                {
                                    using (FileStream fileStream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                    {
                                        fileArray[index--] = fileArray[--count];
                                    }
                                }
                                catch { }
                            }
                        }
                        else fileArray[index--] = fileArray[--count];
                    }
                    ++index;
                }
                files.UnsafeSetLength(count);
            }
            finally
            {
                Monitor.Exit(fileLock);
                if (count == 0)
                {
                    try
                    {
                        onTimeout();
                    }
                    finally { checkLock = 0; }
                }
                else timerTask.Default.Add(checkTimeoutHandle, date.NowSecond.AddTicks(timeoutTicks), null);
            }
        }
        /// <summary>
        /// 关闭文件监视器
        /// </summary>
        /// <param name="watcher">文件监视器</param>
        private void dispose(FileSystemWatcher watcher)
        {
            using (watcher)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Created -= onCreatedHandle;
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            isDisposed = 1;
            counter[] counters = nullValue<counter>.Array;
            interlocked.NoCheckCompareSetSleep0(ref watcherLock);
            try
            {
                if (watchers.Count != 0)
                {
                    counters = watchers.Values.getArray();
                    watchers.Clear();
                }
            }
            finally { watcherLock = 0; }
            foreach (counter counter in counters) dispose(counter.Watcher);
        }
    }
}
