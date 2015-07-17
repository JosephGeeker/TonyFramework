//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HttpPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Web
//	File Name:  HttpPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:18:02
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

namespace TerumoMIS.CoreLibrary.Web
{
    /// <summary>
    /// Http参数及相关操作
    /// </summary>
    public static class HttpPlus
    {
        /// <summary>
        /// 查询模式类别
        /// </summary>
        public enum methodType : byte
        {
            None = 0,
            /// <summary>
            /// 请求获取Request-URI所标识的资源
            /// </summary>
            GET,
            /// <summary>
            /// 在Request-URI所标识的资源后附加新的数据
            /// </summary>
            POST,
            /// <summary>
            /// 请求获取由Request-URI所标识的资源的响应消息报头
            /// </summary>
            HEAD,
            /// <summary>
            /// 请求服务器存储一个资源，并用Request-URI作为其标识
            /// </summary>
            PUT,
            /// <summary>
            /// 请求服务器删除Request-URI所标识的资源
            /// </summary>
            DELETE,
            /// <summary>
            /// 请求服务器回送收到的请求信息，主要用于测试或诊断
            /// </summary>
            TRACE,
            /// <summary>
            /// 保留将来使用
            /// </summary>
            CONNECT,
            /// <summary>
            /// 请求查询服务器的性能，或者查询与资源相关的选项和需求
            /// </summary>
            OPTIONS
        }
        /// <summary>
        /// 查询模式类型集合
        /// </summary>
        private static methodType[] uniqueTypes;
        /// <summary>
        /// 查询模式字节转枚举
        /// </summary>
        /// <param name="method">查询模式</param>
        /// <returns>查询模式枚举</returns>
        internal static unsafe methodType GetMethod(byte* method)
        {
            uint code = *(uint*)method;
            return uniqueTypes[((code >> 12) ^ code) & ((1U << 4) - 1)];
        }

        unsafe static http()
        {
            uniqueTypes = new methodType[1 << 4];
            uint code;
            byte* methodBufferFixed = (byte*)&code;
            foreach (methodType method in System.Enum.GetValues(typeof(fastCSharp.web.http.methodType)))
            {
                if (method != methodType.None)
                {
                    string methodString = method.ToString();
                    fixed (char* methodFixed = methodString)
                    {
                        byte* write = methodBufferFixed, end = methodBufferFixed;
                        if (methodString.Length >= sizeof(int)) end += sizeof(int);
                        else
                        {
                            code = 0x20202020U;
                            end += methodString.Length;
                        }
                        for (char* read = methodFixed; write != end; *write++ = (byte)*read++) ;
                        uniqueTypes[((code >> 12) ^ code) & ((1U << 4) - 1)] = method;
                    }
                }
            }
        }
    }
}
