//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ObjectReferenceStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  ObjectReferenceStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 16:58:37
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
    /// 对象引用
    /// </summary>
    public struct ObjectReferenceStruct:IEquatable<ObjectReferenceStruct>
    {
        /// <summary>
        /// 对象
        /// </summary>
        public object Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(objectReference other)
        {
            Type type = Value.GetType();
            if (Value.GetType() == other.Value.GetType())
            {
                return type == typeof(string) ? (string)Value == (string)other.Value : object.ReferenceEquals(Value, other.Value);
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            try
            {
                return Value.GetHashCode();
            }
            catch
            {
                return Value.GetType().GetHashCode();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals((objectReference)obj);
        }
    }
}
