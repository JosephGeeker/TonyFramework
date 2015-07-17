//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FileBlockPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  FileBlockPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:00:44
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

namespace TerumoMIS.CoreLibrary.Config
{
    /// <summary>
    ///     文件分块相关参数
    /// </summary>
    public sealed class FileBlockPlus
    {
        /// <summary>
        ///     默认文件分块相关参数
        /// </summary>
        public static readonly FileBlockPlus Default = new FileBlockPlus();

        /// <summary>
        ///     文件分块服务验证
        /// </summary>
        public string Verify;

        /// <summary>
        ///     文件分块相关参数
        /// </summary>
        private FileBlockPlus()
        {
            PubPlus.LoadConfig(this);
        }
    }
}