//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MethodImplOptionsEnum
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.DotNet45
//	File Name:  MethodImplOptionsEnum
//	User name:  C1400008
//	Location Time: 2015/7/16 12:14:58
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#if NET4
namespace TerumoMIS.CoreLibrary.DotNet45
{
    /// <summary>
    /// 定义如何实现某方法的详细信息
    /// </summary>
    [Serializable]
    [Flags]
    [ComVisible(true)]
    enum MethodImplOptionsEnum
    {
        /// <summary>
        /// 指定此方法是以非托管代码实现的。
        /// </summary>
        Unmanaged = 4,
        /// <summary>
        /// 指定此方法无法被内联。
        /// </summary>
        NoInlining = 8,
        /// <summary>
        /// 指定声明该方法，但其实现在其他地方提供。
        /// </summary>
        ForwardRef = 16,
        /// <summary>
        /// 指定同时只能由一个线程执行该方法。静态方法锁定类型，而实例方法锁定实例。在任何实例函数中只能执行一个线程，并且在类的任何静态函数中只能执行一个线程。
        /// </summary>
        Synchronized = 32,
        /// <summary>
        /// 指定在调试可能的代码生成问题时，该方法不是由实时 (JIT) 编译器或本机代码生成优化的（请参见 Ngen.exe）。
        /// </summary>
        NoOptimization = 64,
        /// <summary>
        /// 指定此方法签名完全按声明的样子导出。
        /// </summary>
        PreserveSig = 128,
        /// <summary>
        /// 指定一个内部调用。内部调用是对在公共语言运行时本身内部实现的方法的调用。
        /// </summary>
        InternalCall = 4096,
        /// <summary>
        /// 如果可能方法应内联。
        /// </summary>
        AggressiveInlining = 0,
    }
}
#endif