//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DirectoyPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  DirectoyPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 8:52:06
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.IO;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 目录相关操作
    /// </summary>
    public static class DirectoyPlus
    {
        /// <summary>
        /// 目录分隔符
        /// </summary>
        public static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();
        /// <summary>
        /// 取以\结尾的路径全名
        /// </summary>
        /// <param name="path">目录</param>
        /// <returns>\结尾的路径全名</returns>
        public static string FullName(this DirectoryInfo path)
        {
            return path != null ? path.FullName.PathSuffix() : null;
        }
        /// <summary>
        /// 路径补全结尾的\
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>路径</returns>
        public static string PathSuffix(this string path)
        {
            if (path.Length != 0)
            {
                return StringUnsafe.Last(path) == Path.DirectorySeparatorChar ? path : (path + DirectorySeparator);
            }
            return DirectorySeparator;
        }
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path">目录</param>
        /// <returns>是否创建成功</returns>
        public static bool Create(string path)
        {
            if (path != null)
            {
                if (Directory.Exists(path)) return true;
                try
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                catch (Exception error)
                {
                    LogPlus.Error.Add(error, "目录创建失败 : " + path, false);
                }
            }
            return false;
        }
    }
}
