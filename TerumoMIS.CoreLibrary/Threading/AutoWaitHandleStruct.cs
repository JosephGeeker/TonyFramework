//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: AutoWaitHandleStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  AutoWaitHandleStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 13:27:23
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System.Threading;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    ///     一次性等待锁
    /// </summary>
    internal struct AutoWaitHandleStruct
    {
        /// <summary>
        ///     同步等待
        /// </summary>
        private readonly EventWaitHandle _waitHandle;

        /// <summary>
        ///     同步等待
        /// </summary>
        private int _isWait;

        /// <summary>
        ///     一次性等待锁
        /// </summary>
        /// <param name="isSet">是否默认结束等待</param>
        public AutoWaitHandleStruct(bool isSet)
        {
            _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _isWait = isSet ? 1 : 0;
        }

        /// <summary>
        ///     等待结束
        /// </summary>
        public void Wait()
        {
            //Thread.Sleep(0);
            if (Interlocked.CompareExchange(ref _isWait, 1, 0) == 0) _waitHandle.WaitOne();
            _isWait = 0;
        }

        /// <summary>
        ///     结束等待
        /// </summary>
        public void Set()
        {
            if (Interlocked.CompareExchange(ref _isWait, 1, 0) != 0) _waitHandle.Set();
        }
    }
}