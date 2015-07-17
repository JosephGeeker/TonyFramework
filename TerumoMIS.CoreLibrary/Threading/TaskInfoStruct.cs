//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TaskInfoStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Threading
//	File Name:  TaskInfoStruct
//	User name:  C1400008
//	Location Time: 2015/7/16 13:33:19
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary.Threading
{
    /// <summary>
    ///     任务信息
    /// </summary>
    internal struct TaskInfoStruct
    {
        /// <summary>
        ///     调用委托
        /// </summary>
        public Action Call;

        /// <summary>
        ///     任务执行出错委托,停止任务参数null
        /// </summary>
        public Action<Exception> OnError;

        /// <summary>
        ///     执行任务
        /// </summary>
        public void Run()
        {
            try
            {
                Call();
            }
            catch (Exception error)
            {
                if (OnError == null) LogPlus.Error.Add(error, null, false);
                else
                {
                    try
                    {
                        OnError(error);
                    }
                    catch (Exception exception)
                    {
                        LogPlus.Error.Add(exception, null, false);
                    }
                }
            }
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        public void RunClear()
        {
            Run();
            Call = null;
            OnError = null;
        }
    }
}