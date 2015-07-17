//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SocketAsyncEventArgsPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net
//	File Name:  SocketAsyncEventArgsPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 14:59:41
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Net
{
    /// <summary>
    /// 异步回调参数
    /// </summary>
    public static class SocketAsyncEventArgsPlus
    {
        /// <summary>
        /// 获取一个异步回调参数
        /// </summary>
        /// <returns>异步回调参数</returns>
        internal static SocketAsyncEventArgs Get()
        {
            SocketAsyncEventArgs value = typePool<SocketAsyncEventArgs>.Pop();
            if (value == null)
            {
                value = new SocketAsyncEventArgs();
                value.SocketFlags = System.Net.Sockets.SocketFlags.None;
                value.DisconnectReuseSocket = false;
            }
            return value;
        }
        /// <summary>
        /// 添加异步回调参数
        /// </summary>
        /// <param name="value">异步回调参数</param>
        internal static void Push(ref SocketAsyncEventArgs value)
        {
            value.SetBuffer(null, 0, 0);
            value.UserToken = null;
            value.SocketError = SocketError.Success;
            typePool<SocketAsyncEventArgs>.Push(value);
        }
    }
}
