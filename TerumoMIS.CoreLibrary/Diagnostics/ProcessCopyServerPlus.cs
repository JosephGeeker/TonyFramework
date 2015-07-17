//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ProcessCopyServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Diagnostics
//	File Name:  ProcessCopyServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:09:45
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Code.Csharper;

namespace TerumoMIS.CoreLibrary.Diagnostics
{
    /// <summary>
    /// 进程复制重启服务
    /// </summary>
    [TcpServerPlus(Service = "ProcessCopy",IsIdentityCommand = true,VerifyMethodType = typeof(Net.Tcp.VerifyMethod))]
    public partial class ProcessCopyServerPlus
    {
        /// <summary>
        /// 文件复制器
        /// </summary>
        [fastCSharp.emit.dataSerialize(IsMemberMap = false)]
        public sealed class copyer : IDisposable
        {
            /// <summary>
            /// 进程标识
            /// </summary>
            public int ProcessId;
            /// <summary>
            /// 进程名称
            /// </summary>
            public string ProcessName;
            /// <summary>
            /// 目标路径
            /// </summary>
            public string Path;
            /// <summary>
            /// 复制文件源路径
            /// </summary>
            public string CopyPath;
            /// <summary>
            /// 进程文件名
            /// </summary>
            public string Process;
            /// <summary>
            /// 进程启动参数
            /// </summary>
            public string Arguments;
            /// <summary>
            /// 超时时间
            /// </summary>
            [fastCSharp.code.ignore]
            private DateTime timeout;
            /// <summary>
            /// 进程信息
            /// </summary>
            [fastCSharp.code.ignore]
            private Process process;
            /// <summary>
            /// 进程复制重启服务
            /// </summary>
            [fastCSharp.code.ignore]
            private processCopyServer server;
            /// <summary>
            /// 
            /// </summary>
            public void Dispose()
            {
                if (server != null)
                {
                    server.removeNoCheck(this);
                    server = null;
                }
            }
            /// <summary>
            /// 删除进程退出事件
            /// </summary>
            internal void RemoveGuard()
            {
                if (process != null)
                {
                    try
                    {
                        process.EnableRaisingEvents = false;
                        process.Exited -= guard;
                    }
                    catch { }
                    finally { pub.Dispose(ref process); }
                }
            }
            /// <summary>
            /// 开始复制文件
            /// </summary>
            internal void Copy()
            {
                bool isLog = true;
                timeout = date.NowSecond.AddMinutes(fastCSharp.config.processCopy.Default.CopyTimeoutMinutes);
                for (int milliseconds = 1 << 4; milliseconds <= 1 << 11; milliseconds <<= 1)
                {
                    Thread.Sleep(milliseconds);
                    if (copy(isLog))
                    {
                        copyStart();
                        return;
                    }
                    isLog = false;
                }
                long tiks = new TimeSpan(0, 0, 4).Ticks;
                timerTask.Default.Add(copy, 4, date.NowSecond.AddSeconds(4), null);
            }
            /// <summary>
            /// 开始复制文件
            /// </summary>
            /// <param name="seconds">休眠秒数</param>
            private void copy(int seconds)
            {
                if (date.NowSecond >= timeout)
                {
                    log.Error.Add("文件复制超时 " + Process, false, false);
                    return;
                }
                if (copy(true))
                {
                    copyStart();
                    return;
                }
                if (seconds != fastCSharp.config.processCopy.Default.CheckTimeoutSeconds)
                {
                    seconds <<= 1;
                    if (seconds <= 0 || seconds > fastCSharp.config.processCopy.Default.CheckTimeoutSeconds) seconds = fastCSharp.config.processCopy.Default.CheckTimeoutSeconds;
                }
                timerTask.Default.Add(copy, seconds, date.NowSecond.AddSeconds(seconds), null);
            }
            /// <summary>
            /// 复制文件
            /// </summary>
            /// <param name="isLog">是否输出错误日志</param>
            /// <returns>是否成功</returns>
            private bool copy(bool isLog)
            {
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(Path);
                    if (!directory.Exists) directory.Create();
                    DirectoryInfo copyDirectory = new DirectoryInfo(CopyPath);
                    Path = directory.fullName();
                    if (copyDirectory.Exists)
                    {
                        foreach (FileInfo file in copyDirectory.GetFiles()) file.CopyTo(Path + file.Name, true);
                        return true;
                    }
                    if (File.Exists(Process)) return true;
                }
                catch (Exception error)
                {
                    if (isLog) log.Error.Add(error, null, false);
                }
                return false;
            }
            /// <summary>
            /// 启动进程
            /// </summary>
            private void copyStart()
            {
                log.Default.Add("文件复制成功 " + Process, false, false);
                start();
            }
            /// <summary>
            /// 启动进程
            /// </summary>
            private void start()
            {
                ProcessStartInfo info = new ProcessStartInfo(Process, Arguments);
                info.UseShellExecute = true;
                info.WorkingDirectory = Path;
                System.Diagnostics.Process.Start(info);
                log.Default.Add("进程启动成功 " + Process, false, false);
            }
            /// <summary>
            /// 默认文件复制器
            /// </summary>
            private static copyer watcher;
            /// <summary>
            /// 默认文件复制器
            /// </summary>
            internal static copyer Watcher
            {
                get
                {
                    if (watcher == null)
                    {
                        string command = Environment.CommandLine;
                        int index = command.IndexOf(' ') + 1;
                        using (Process process = System.Diagnostics.Process.GetCurrentProcess())
                        {
                            FileInfo file = new FileInfo(process.MainModule.FileName);
                            watcher = new diagnostics.processCopyServer.copyer
                            {
                                ProcessId = process.Id,
                                ProcessName = process.ProcessName,
                                Process = file.FullName,
                                Path = file.Directory.FullName,
                                CopyPath = fastCSharp.config.processCopy.Default.WatcherPath,
                                Arguments = index == 0 || index == command.Length ? null : command.Substring(index)
                            };
                        }
                    }
                    return watcher;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            internal bool CheckName()
            {
                using (Process process = System.Diagnostics.Process.GetProcessById(ProcessId))
                {
                    return process != null && process.ProcessName == ProcessName;
                    //if (process != null)
                    //{
                    //    try
                    //    {
                    //        FileInfo file = new FileInfo(process.MainModule.FileName);
                    //        return Process == file.FullName;
                    //    }
                    //    catch(Exception error)
                    //    {
                    //        log.Default.Add(error, null, false);
                    //        return true;
                    //    }
                    //}
                }
                //return false;
            }
            /// <summary>
            /// 进程退出事件
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void guard(object sender, EventArgs e)
            {
                try
                {
                    Dispose();
                    if (File.Exists(Process)) start();
                    else log.Error.Add("没有找到文件 " + Process, false, false);
                }
                catch (Exception error)
                {
                    log.Error.Add(error, "进程启动失败 " + Process, false);
                }
            }
            /// <summary>
            /// 守护进程退出事件
            /// </summary>
            /// <param name="server"></param>
            /// <returns></returns>
            internal bool Guard(processCopyServer server)
            {
                this.server = server;
                try
                {
                    if ((process = System.Diagnostics.Process.GetProcessById(ProcessId)) != null)
                    {
                        process.EnableRaisingEvents = true;
                        process.Exited += guard;
                        log.Default.Add("添加守护进程 " + Process, false, false);
                        return true;
                    }
                }
                catch (Exception error)
                {
                    log.Default.Add(error, null, false);
                }
                return false;
            }
        }
        /// <summary>
        /// 进程复制重启服务端注册验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024)]
        protected virtual bool verify(string value)
        {
            if (fastCSharp.config.processCopy.Default.Verify == null && !fastCSharp.config.pub.Default.IsDebug)
            {
                log.Error.Add("进程复制重启服务验证数据不能为空", false, true);
                return false;
            }
            return fastCSharp.config.processCopy.Default.Verify == value;
        }
        /// <summary>
        /// 复制重启进程
        /// </summary>
        /// <param name="copyer">文件复制器</param>
        [fastCSharp.code.cSharp.tcpServer]
        private void copyStart(copyer copyer)
        {
            if (copyer.CheckName())
            {
                bool isGuard;
                Monitor.Enter(guardLock);
                try
                {
                    isGuard = guards.Remove(copyer.ProcessId);
                }
                finally { Monitor.Exit(guardLock); }
                if (isGuard) saveCache();
                log.Default.Add("启动文件复制 " + copyer.Process, false, false);
                copyer.Copy();
            }
        }
        /// <summary>
        /// 守护进程
        /// </summary>
        /// <param name="copyer">文件信息</param>
        [fastCSharp.code.cSharp.tcpServer]
        private void guard(copyer copyer)
        {
            if (copyer.CheckName())
            {
                copyer cache;
                Monitor.Enter(guardLock);
                try
                {
                    if (guards.TryGetValue(copyer.ProcessId, out cache)) guards[copyer.ProcessId] = copyer;
                    else guards.Add(copyer.ProcessId, copyer);
                }
                finally { Monitor.Exit(guardLock); }
                if (cache != null) pub.Dispose(ref cache);
                if (!copyer.Guard(this))
                {
                    Monitor.Enter(guardLock);
                    try
                    {
                        if (guards.TryGetValue(copyer.ProcessId, out cache) && cache == copyer) guards.Remove(copyer.ProcessId);
                    }
                    finally { Monitor.Exit(guardLock); }
                }
                saveCache();
            }
        }
        /// <summary>
        /// 删除守护进程
        /// </summary>
        /// <param name="copyer">文件信息</param>
        [fastCSharp.code.cSharp.tcpServer]
        private void remove(copyer copyer)
        {
            if (copyer.CheckName()) removeNoCheck(copyer);
        }
        /// <summary>
        /// 删除守护进程
        /// </summary>
        /// <param name="copyer">文件信息</param>
        private void removeNoCheck(copyer copyer)
        {
            copyer cache;
            Monitor.Enter(guardLock);
            try
            {
                if (guards.TryGetValue(copyer.ProcessId, out cache)) guards.Remove(copyer.ProcessId);
            }
            finally { Monitor.Exit(guardLock); }
            if (cache != null)
            {
                cache.RemoveGuard();
                saveCache();
            }
        }
        /// <summary>
        /// 守护进程集合
        /// </summary>
        private static Dictionary<int, copyer> guards;
        /// <summary>
        /// 守护进程集合访问锁
        /// </summary>
        private static readonly object guardLock = new object();
        /// <summary>
        /// 守护进程客户端调用
        /// </summary>
        public static void Guard()
        {
            try
            {
                using (fastCSharp.tcpClient.processCopy processCopy = new fastCSharp.tcpClient.processCopy())
                {
                    if (processCopy.guard(copyer.Watcher).IsReturn) return;
                }
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            log.Error.Add("守护进程客户端调用失败", false, false);
            if (_guardHandle == null) _guardHandle = guard;
            timerTask.Default.Add(_guardHandle, date.NowSecond.AddMinutes(1), null);
        }
        /// <summary>
        /// 守护进程删除客户端调用
        /// </summary>
        public static void Remove()
        {
            try
            {
                using (fastCSharp.tcpClient.processCopy processCopy = new fastCSharp.tcpClient.processCopy()) processCopy.remove(copyer.Watcher);
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
        }
        /// <summary>
        /// 守护进程客户端调用
        /// </summary>
        private static Action _guardHandle;
        /// <summary>
        /// 守护进程客户端调用
        /// </summary>
        private static void guard()
        {
            try
            {
                using (fastCSharp.tcpClient.processCopy processCopy = new fastCSharp.tcpClient.processCopy())
                {
                    if (processCopy.guard(copyer.Watcher).IsReturn)
                    {
                        log.Default.Add("守护进程客户端调用成功", false, false);
                        return;
                    }
                }
            }
            catch { }
            timerTask.Default.Add(_guardHandle, date.NowSecond.AddMinutes(1), null);
        }
        /// <summary>
        /// 默认文件监视器过滤
        /// </summary>
        public static readonly Func<FileSystemEventArgs, bool> DefaultFileWatcherFilter = fileWatcherFilter;
        /// <summary>
        /// 文件监视器过滤
        /// </summary>
        /// <param name="e"></param>
        /// <returns>是否继续检测</returns>
        private static unsafe bool fileWatcherFilter(FileSystemEventArgs e)
        {
            string name = e.FullPath;
            if (name.Length > 4)
            {
                fixed (char* nameFixed = name)
                {
                    char* end = nameFixed + name.Length;
                    int code = *(end - 4) | (*(end - 3) << 8) | (*(end - 2) << 16) | (*(end - 1) << 24) | 0x202020;
                    if (code == ('.' | ('d' << 8) | ('l' << 16) | ('l' << 24))
                        || code == ('.' | ('p' << 8) | ('d' << 16) | ('b' << 24))
                        || code == ('.' | ('e' << 8) | ('x' << 16) | ('e' << 24)))
                    {
                        return true;
                    }
                    if ((code | 0x20000000) == ('n' | ('f' << 8) | ('i' << 16) | ('g' << 24)) && name.Length > 7)
                    {
                        end -= 7;
                        if ((*end | (*(end + 1) << 8) | (*(end + 2) << 16) | 0x2020) == ('.' | ('c' << 8) | ('o' << 16))) return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 进程复制重启客户端调用
        /// </summary>
        /// <param name="processCopyer">文件复制器</param>
        /// <returns>是否成功</returns>
        public static bool CopyStart()
        {
            try
            {
                using (fastCSharp.tcpClient.processCopy processCopy = new fastCSharp.tcpClient.processCopy())
                {
                    processCopy.copyStart(copyer.Watcher);
                }
                return true;
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            return false;
        }
        /// <summary>
        /// 设置TCP服务端
        /// </summary>
        /// <param name="tcpServer">TCP服务端</param>
        public void SetTcpServer(fastCSharp.net.tcp.server tcpServer)
        {
            bool isSave = false;
            Monitor.Enter(guardLock);
            if (guards == null)
            {
                try
                {
                    guards = dictionary.CreateInt<copyer>();
                    if (File.Exists(cacheFile))
                    {
                        foreach (copyer copyer in fastCSharp.emit.dataDeSerializer.DeSerialize<copyer[]>(File.ReadAllBytes(cacheFile)))
                        {
                            guards.Add(copyer.ProcessId, copyer);
                            if (!copyer.Guard(this))
                            {
                                guards.Remove(copyer.ProcessId);
                                isSave = true;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                    try
                    {
                        File.Delete(cacheFile);
                    }
                    catch (Exception error1)
                    {
                        log.Default.Add(error1, null, false);
                    }
                }
            }
            Monitor.Exit(guardLock);
            if (isSave) saveCache();
        }
        /// <summary>
        /// 进程守护缓存文件名
        /// </summary>
        private const string cacheFile = "processCopyServer_Guard.cache";
        /// <summary>
        /// 保存进程守护信息集合到缓存文件
        /// </summary>
        private static void saveCache()
        {
            copyer[] cache;
            Monitor.Enter(guardLock);
            try
            {
                cache = guards.Values.getArray();
            }
            finally { Monitor.Exit(guardLock); }
            try
            {
                if (cache.Length == 0) File.Delete(cacheFile);
                else File.WriteAllBytes(cacheFile, fastCSharp.emit.dataSerializer.Serialize(cache));
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
        }
    }
}
