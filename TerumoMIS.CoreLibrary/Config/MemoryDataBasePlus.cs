//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MemoryDataBasePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  MemoryDataBasePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:02:31
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary.Config
{
    /// <summary>
    ///     内存数据库相关参数
    /// </summary>
    public sealed class MemoryDataBasePlus : DataBasePlus
    {
        /// <summary>
        ///     数据库日志文件最小刷新尺寸(单位:KB)
        /// </summary>
        internal const int DefaultMinRefreshSize = 1024;

        /// <summary>
        ///     物理层最小数据缓冲区字节数
        /// </summary>
        internal const int MinPhysicalBufferSize = 1 << 12;

        /// <summary>
        ///     默认内存数据库相关参数
        /// </summary>
        public static readonly MemoryDataBasePlus Default = new MemoryDataBasePlus();

        /// <summary>
        ///     客户端缓存字节数
        /// </summary>
        private readonly int _bufferSize = 1 << 10;

        /// <summary>
        ///     缓存默认容器尺寸(单位:2^n)
        /// </summary>
        private readonly byte _cacheCapacity = 16;

        /// <summary>
        ///     数据库日志文件最大刷新比例(:KB)
        /// </summary>
        private readonly int _maxRefreshPerKb = 512;

        /// <summary>
        ///     数据库日志文件最小刷新尺寸(单位:KB)
        /// </summary>
        private readonly int _minRefreshSize = DefaultMinRefreshSize;

        /// <summary>
        ///     物理层默认数据缓冲区字节数(单位:2^n)
        /// </summary>
        private readonly byte _physicalBufferSize = 16;

        /// <summary>
        ///     数据库文件刷新超时秒数
        /// </summary>
        private readonly int _refreshTimeOutSeconds = 30;

        /// <summary>
        ///     物理层服务验证
        /// </summary>
        public string PhysicalVerify;

        /// <summary>
        ///     内存数据库相关参数
        /// </summary>
        private MemoryDataBasePlus()
        {
            PubPlus.LoadConfig(this);
        }

        /// <summary>
        ///     数据库文件刷新超时周期
        /// </summary>
        public long RefreshTimeOutTicks
        {
            get { return new TimeSpan(0, 0, 0, _refreshTimeOutSeconds <= 0 ? 30 : _refreshTimeOutSeconds).Ticks; }
        }

        /// <summary>
        ///     数据库日志文件最小刷新字节数
        /// </summary>
        public int MinRefreshSize
        {
            get { return _minRefreshSize <= DefaultMinRefreshSize ? DefaultMinRefreshSize : _minRefreshSize; }
        }

        /// <summary>
        ///     数据库日志文件最大刷新比例(:KB)
        /// </summary>
        public int MaxRefreshPerKB
        {
            get { return _maxRefreshPerKb > 0 ? _maxRefreshPerKb : 512; }
        }

        /// <summary>
        ///     缓存默认容器尺寸
        /// </summary>
        public int CacheCapacity
        {
            get { return _cacheCapacity >= 8 && _cacheCapacity <= 30 ? 1 << _cacheCapacity : (1 << 16); }
        }

        /// <summary>
        ///     客户端缓存字节数
        /// </summary>
        public int BufferSize
        {
            get { return _bufferSize <= 0 ? 1 << 10 : _bufferSize; }
        }

        /// <summary>
        ///     物理层默认数据缓冲区字节数
        /// </summary>
        public int PhysicalBufferSize
        {
            get
            {
                return _physicalBufferSize >= 12 && _physicalBufferSize <= 30 ? 1 << _physicalBufferSize : (1 << 16);
            }
        }
    }
}