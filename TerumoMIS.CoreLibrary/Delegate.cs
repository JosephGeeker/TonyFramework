//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: Delegate
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  Delegate
//	User name:  C1400008
//	Location Time: 2015/7/16 16:43:52
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
    /// 字符串转换委托
    /// </summary>
    /// <typeparam name="returnType">目标类型</typeparam>
    /// <param name="stringValue">字符串</param>
    /// <param name="value">目标对象</param>
    /// <returns>是否转换成功</returns>
    public delegate bool tryParse<returnType>(string stringValue, out returnType value);
    /// <summary>
    /// 入池函数调用委托
    /// </summary>
    /// <typeparam name="parameterType">输入参数类型</typeparam>
    /// <param name="parameter">输入参数</param>
    public delegate void pushPool<parameterType>(ref parameterType parameter);
}
