//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CallBackActionPoolPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  CallBackActionPoolPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:29:09
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    /// 回调池
    /// </summary>
    /// <typeparam name="TCallbackType">回调对象类型</typeparam>
    /// <typeparam name="TValueType">回调值类型</typeparam>
    public abstract class CallBackActionPoolPlus<TCallbackType,TValueType>
        where TCallbackType:class
    {
        /// <summary>
        /// 回调委托
        /// </summary>
        public Action<TValueType> Callback;
        /// <summary>
        /// 添加回调对象
        /// </summary>
        /// <param name="poolCallback">回调对象</param>
        /// <param name="value">回调值</param>
        protected void Push(TCallbackType poolCallback, TValueType value)
        {
            var callback = Callback;
            Callback = null;
            try
            {
                typePool<TCallbackType>.Push(poolCallback);
            }
            finally
            {
                if (callback != null)
                {
                    try
                    {
                        callback(value);
                    }
                    catch (Exception error)
                    {
                        LogPlus.Error.Add(error, null, false);
                    }
                }
            }
        }
        /// <summary>
        /// 回调处理
        /// </summary>
        /// <param name="value">回调值</param>
        protected void OnlyCallback(TValueType value)
        {
            var callback = Callback;
            if (callback == null) return;
            try
            {
                callback(value);
            }
            catch (Exception error)
            {
                LogPlus.Error.Add(error, null, false);
            }
        }
    }
}
