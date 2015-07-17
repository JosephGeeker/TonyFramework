//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: RemoteTypeStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  RemoteTypeStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 11:27:59
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
using TerumoMIS.CoreLibrary.Emit;

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 远程类型
    /// </summary>
    [DataSerializePlus(IsMemberMap = false)]
    public struct RemoteTypeStruct
    {
        /// <summary>
        /// 程序集名称
        /// </summary>
        private string _assemblyName;
        /// <summary>
        /// 类型名称
        /// </summary>
        private string _name;
        /// <summary>
        /// 是否空类型
        /// </summary>
        public bool IsNull
        {
            get { return _assemblyName == null || _name == null; }
        }
        /// <summary>
        /// 类型
        /// </summary>
        public Type Type
        {
            get
            {
                Type type;
                if (TryGet(out type)) return type;
                LogPlus.Default.Throw(null, "未能加载类型 : " + _name + " in " + _assemblyName, true);
                return null;
            }
        }
        /// <summary>
        /// 远程类型
        /// </summary>
        /// <param name="type">类型</param>
        public RemoteTypeStruct(Type type)
        {
            _name = type.FullName;
            _assemblyName = type.Assembly.FullName;
        }
        /// <summary>
        /// 类型隐式转换
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>远程类型</returns>
        public static implicit operator RemoteTypeStruct(Type type) { return new RemoteTypeStruct(type); }
        /// <summary>
        /// 尝试获取类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否成功</returns>
        public bool TryGet(out Type type)
        {
            Assembly assembly = reflection.assembly.Get(_assemblyName);
            if (assembly != null)
            {
                if ((type = assembly.GetType(_name)) != null) return true;
            }
            else type = null;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _assemblyName + " + " + _name;
        }
    }
}
