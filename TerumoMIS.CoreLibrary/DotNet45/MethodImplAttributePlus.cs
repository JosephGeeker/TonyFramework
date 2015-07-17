//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: MethodImplAttributePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.DotNet45
//	File Name:  MethodImplAttributePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:12:21
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#if NET4
namespace TerumoMIS.CoreLibrary.DotNet45
{
    /// <summary>
    /// 指定如何实现某方法的详细信息，无法继承此类
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    [AttributeUsage(AttributeTargets.Constructor|AttributeTargets.Method,Inherited = false)]
    sealed class MethodImplAttributePlus:Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        internal MethodImplOptions _val;
        /// <summary>
        /// 一个 System.Runtime.CompilerServices.MethodCodeType 值，指示为此方法提供了哪种类型的实现。
        /// </summary>
        public System.Runtime.CompilerServices.MethodCodeType MethodCodeType = System.Runtime.CompilerServices.MethodCodeType.IL;

        /// <summary>
        /// 初始化 MethodImplAttribute 类的新实例。
        /// </summary>
        public MethodImplAttribute() { }
        /// <summary>
        /// 使用指定的 System.Runtime.CompilerServices.MethodImplOptions 值初始化 MethodImplAttribute类的新实例。
        /// </summary>
        /// <param name="methodImplOptions">一个 System.Runtime.CompilerServices.MethodImplOptions 值，该值指定属性化方法的属性。</param>
        public MethodImplAttribute(MethodImplOptions methodImplOptions)
        {
            MethodImplOptions options = MethodImplOptions.InternalCall | MethodImplOptions.PreserveSig | MethodImplOptions.NoOptimization | MethodImplOptions.Synchronized | MethodImplOptions.ForwardRef | MethodImplOptions.NoInlining | MethodImplOptions.Unmanaged | MethodImplOptions.AggressiveInlining;
            this._val = ((MethodImplOptions)methodImplOptions) & options;
        }
        /// <summary>
        /// 使用指定的 System.Runtime.CompilerServices.MethodImplOptions 值初始化 MethodImplAttribute类的新实例。
        /// </summary>
        /// <param name="value">一个位屏蔽，表示所需的 System.Runtime.CompilerServices.MethodImplOptions 值，该值指定属性化方法的属性。</param>
        public MethodImplAttribute(short value)
        {
            this._val = (MethodImplOptions)value;
        }
        /// <summary>
        /// 获取描述属性化方法的 System.Runtime.CompilerServices.MethodImplOptions 值。
        /// </summary>
        public MethodImplOptions Value
        {
            get { return this._val; }
        }
    }
}
#else
#endif
