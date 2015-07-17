//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ProcessCopyPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  ProcessCopyPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:03:25
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.IO;

namespace TerumoMIS.CoreLibrary.Config
{
    /// <summary>
    ///     进程复制重启服务配置
    /// </summary>
    public sealed class ProcessCopyPlus
    {
        /// <summary>
        ///     默认进程复制重启服务配置
        /// </summary>
        public static readonly ProcessCopyPlus Default = new ProcessCopyPlus();

        /// <summary>
        ///     文件更新重启检测时间(单位:秒)
        /// </summary>
        private readonly int _checkTimeoutSeconds = 5;

        /// <summary>
        ///     文件更新重启复制超时时间(单位:分)
        /// </summary>
        private readonly int _copyTimeoutMinutes = 10;

        /// <summary>
        ///     进程复制重启失败最大休眠秒数
        /// </summary>
        private readonly int _maxThreadSeconds = 10;

        /// <summary>
        ///     进程复制重启服务验证
        /// </summary>
        public string Verify;

        /// <summary>
        ///     文件监视路径
        /// </summary>
        public string WatcherPath;

        /// <summary>
        ///     进程复制重启服务配置
        /// </summary>
        private ProcessCopyPlus()
        {
            PubPlus.LoadConfig(this);
            if (WatcherPath == null) return;
            try
            {
                var fileWatcherDirectory = new DirectoryInfo(WatcherPath);
                if (fileWatcherDirectory.Exists) WatcherPath = fileWatcherDirectory.FullName.ToLower();
                else
                {
                    WatcherPath = null;
                    LogPlus.Error.Add("没有找到文件监视路径 " + WatcherPath, false, false);
                }
            }
            catch (Exception error)
            {
                LogPlus.Error.Add(error, WatcherPath, false);
                WatcherPath = null;
            }
        }

        /// <summary>
        ///     文件更新重启检测时间(单位:秒)
        /// </summary>
        public int CheckTimeoutSeconds
        {
            get { return Math.Max(_checkTimeoutSeconds, 2); }
        }

        /// <summary>
        ///     文件更新重启复制超时时间(单位:分)
        /// </summary>
        public int CopyTimeoutMinutes
        {
            get { return Math.Max(_copyTimeoutMinutes, 1); }
        }

        /// <summary>
        ///     进程复制重启失败最大休眠秒数
        /// </summary>
        public int MaxThreadSeconds
        {
            get { return Math.Max(_maxThreadSeconds, 2); }
        }
    }
}