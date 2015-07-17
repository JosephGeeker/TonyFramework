//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CheckMemoryPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  CheckMemoryPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 16:41:27
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

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 内存检测
    /// </summary>
    public static class CheckMemoryPlus
    {
        /// <summary>
        /// 检测类型集合
        /// </summary>
        private static arrayPool<Type> pool = arrayPool<Type>.Create();
        /// <summary>
        /// 添加类型
        /// </summary>
        /// <param name="type"></param>
        public static void Add(Type type)
        {
            pool.Push(type);
        }
        /// <summary>
        /// 获取检测类型集合
        /// </summary>
        /// <returns></returns>
        public static subArray<Type> GetTypes()
        {
            int count = pool.Count;
            return subArray<Type>.Unsafe(pool.Array, 0, count);
        }
    }
}
