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

namespace TerumoMIS.CoreLibrary.Web
{
    /// <summary>
    ///     Http参数及相关操作
    /// </summary>
    public static class HttpPlus
    {
        /// <summary>
        ///     查询模式类别
        /// </summary>
        public enum MethodTypeEnum : byte
        {
            None = 0,

            /// <summary>
            ///     请求获取Request-URI所标识的资源
            /// </summary>
            Get,

            /// <summary>
            ///     在Request-URI所标识的资源后附加新的数据
            /// </summary>
            Post,

            /// <summary>
            ///     请求获取由Request-URI所标识的资源的响应消息报头
            /// </summary>
            Head,

            /// <summary>
            ///     请求服务器存储一个资源，并用Request-URI作为其标识
            /// </summary>
            Put,

            /// <summary>
            ///     请求服务器删除Request-URI所标识的资源
            /// </summary>
            Delete,

            /// <summary>
            ///     请求服务器回送收到的请求信息，主要用于测试或诊断
            /// </summary>
            Trace,

            /// <summary>
            ///     保留将来使用
            /// </summary>
            Connect,

            /// <summary>
            ///     请求查询服务器的性能，或者查询与资源相关的选项和需求
            /// </summary>
            Options
        }

        /// <summary>
        ///     查询模式类型集合
        /// </summary>
        private static readonly MethodTypeEnum[] UniqueTypes;

        static unsafe HttpPlus()
        {
            UniqueTypes = new MethodTypeEnum[1 << 4];
            uint code;
            var methodBufferFixed = (byte*) &code;
            foreach (MethodTypeEnum method in Enum.GetValues(typeof (MethodTypeEnum)))
            {
                if (method != MethodTypeEnum.None)
                {
                    var methodString = method.ToString();
                    fixed (char* methodFixed = methodString)
                    {
                        byte* write = methodBufferFixed, end = methodBufferFixed;
                        if (methodString.Length >= sizeof (int)) end += sizeof (int);
                        else
                        {
                            code = 0x20202020U;
                            end += methodString.Length;
                        }
                        for (var read = methodFixed; write != end; *write++ = (byte) *read++)
                        {
                        }
                        UniqueTypes[((code >> 12) ^ code) & ((1U << 4) - 1)] = method;
                    }
                }
            }
        }

        /// <summary>
        ///     查询模式字节转枚举
        /// </summary>
        /// <param name="method">查询模式</param>
        /// <returns>查询模式枚举</returns>
        internal static unsafe MethodTypeEnum GetMethod(byte* method)
        {
            var code = *(uint*) method;
            return UniqueTypes[((code >> 12) ^ code) & ((1U << 4) - 1)];
        }
    }
}