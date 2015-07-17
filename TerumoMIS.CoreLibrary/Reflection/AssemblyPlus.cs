//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: AssemblyPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Reflection
//	File Name:  AssemblyPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:39:52
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Reflection
{
    /// <summary>
    /// 程序集扩展操作
    /// </summary>
    public static class AssemblyPlus
    {
        /// <summary>
        /// 根据程序集名称获取程序集
        /// </summary>
        /// <param name="fullName">程序集名称</param>
        /// <returns>程序集,失败返回null</returns>
        public static Assembly Get(string fullName)
        {
            if (fullName != null)
            {
                Assembly value;
                if (nameCache.TryGetValue(fullName, out value)) return value;
            }
            return null;
        }
        /// <summary>
        /// 获取类型信息
        /// </summary>
        /// <param name="assembly">程序集信息</param>
        /// <param name="fullName">类型全名</param>
        /// <returns>类型信息</returns>
        public static Type getType(this Assembly assembly, string fullName)
        {
            return assembly != null ? assembly.GetType(fullName) : null;
        }
        /// <summary>
        /// 程序集名称缓存
        /// </summary>
        private static interlocked.dictionary<hashString, Assembly> nameCache;
        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="assembly">程序集</param>
        private static void loadAssembly(Assembly assembly)
        {
            nameCache.Set(assembly.FullName, assembly);
        }
        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void loadAssembly(object sender, AssemblyLoadEventArgs args)
        {
            loadAssembly(args.LoadedAssembly);
        }
        static assembly()
        {
            nameCache = new interlocked.dictionary<hashString,Assembly>(dictionary.CreateHashString<Assembly>());
            AppDomain.CurrentDomain.AssemblyLoad += loadAssembly;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) nameCache.Set(assembly.FullName, assembly);
        }
    }
}
