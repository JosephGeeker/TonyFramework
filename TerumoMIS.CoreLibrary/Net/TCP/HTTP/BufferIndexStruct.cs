//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: BufferIndexStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  BufferIndexStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 15:32:20
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

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    /// <summary>
    /// 索引位置
    /// </summary>
    public struct BufferIndexStruct
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        public short StartIndex;
        /// <summary>
        /// 长度
        /// </summary>
        public short Length;
        /// <summary>
        /// 索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(long startIndex, long length)
        {
            StartIndex = (short)startIndex;
            Length = (short)length;
        }
        /// <summary>
        /// 索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(int startIndex, int length)
        {
            StartIndex = (short)startIndex;
            Length = (short)length;
        }
        /// <summary>
        /// 索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(short startIndex, short length)
        {
            StartIndex = startIndex;
            Length = length;
        }
        /// <summary>
        /// 索引位置置空
        /// </summary>
        public void Null()
        {
            StartIndex = Length = 0;
        }
    }
}
