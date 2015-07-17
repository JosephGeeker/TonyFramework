//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: AsynchronousMethodPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  AsynchronousMethodPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:38:51
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

namespace TerumoMIS.CoreLibrary.Code.Csharper
{
    /// <summary>
    /// 方法信息
    /// </summary>
    public abstract class AsynchronousMethodPlus
    {
        /// <summary>
        /// 返回值参数名称
        /// </summary>
        public const string ReturnParameterName = "Return";
        /// <summary>
        /// 异步返回值
        /// </summary>
        public struct returnValue
        {
            /// <summary>
            /// 是否调用成功
            /// </summary>
            public bool IsReturn;
        }
        /// <summary>
        /// 异步返回值
        /// </summary>
        /// <typeparam name="valueType">返回值类型</typeparam>
        public struct returnValue<valueType>
        {
            /// <summary>
            /// 异步调用失败
            /// </summary>
            private static readonly Exception returnException = new Exception("异步调用失败");
            /// <summary>
            /// 返回值
            /// </summary>
            public valueType Value;
            /// <summary>
            /// 是否调用成功
            /// </summary>
            public bool IsReturn;
            /// <summary>
            /// 清空数据
            /// </summary>
            public void Null()
            {
                IsReturn = false;
                Value = default(valueType);
            }
            /// <summary>
            /// 获取返回值
            /// </summary>
            /// <param name="value">异步返回值</param>
            /// <returns>返回值</returns>
            public static implicit operator returnValue<valueType>(valueType value)
            {
                return new returnValue<valueType> { IsReturn = true, Value = value };
            }
            /// <summary>
            /// 获取返回值
            /// </summary>
            /// <param name="value">返回值</param>
            /// <returns>异步返回值</returns>
            public static implicit operator valueType(returnValue<valueType> value)
            {
                if (value.IsReturn) return value.Value;
                throw returnException;
            }
        }
        /// <summary>
        /// 返回参数
        /// </summary>
        /// <typeparam name="valueType">返回参数类型</typeparam>
        public interface IReturnParameter<valueType>
        {
            /// <summary>
            /// 返回值
            /// </summary>
            valueType Return { get; set; }
        }
        /// <summary>
        /// 返回参数
        /// </summary>
        /// <typeparam name="valueType">返回参数类型</typeparam>
        public class returnParameter<valueType> : IReturnParameter<valueType>
        {
            [fastCSharp.emit.jsonSerialize.member(IsIgnoreCurrent = true)]
            [fastCSharp.emit.jsonParse.member(IsIgnoreCurrent = true)]
            internal valueType Ret;
            /// <summary>
            /// 返回值
            /// </summary>
            public valueType Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }
        /// <summary>
        /// 异步回调
        /// </summary>
        public sealed class callReturn
        {
            /// <summary>
            /// 回调委托
            /// </summary>
            private Action<returnValue> callback;
            /// <summary>
            /// 异步回调返回值
            /// </summary>
            /// <param name="isReturn">调用使用成功</param>
            private void onReturn(bool isReturn)
            {
                try
                {
                    callback(new returnValue { IsReturn = isReturn });
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, false);
                }
            }
            /// <summary>
            /// 获取异步回调
            /// </summary>
            /// <param name="callback">回调委托</param>
            /// <returns>异步回调</returns>
            public static Action<bool> Get(Action<returnValue> callback)
            {
                if (callback == null) return null;
                try
                {
                    return new callReturn { callback = callback }.onReturn;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                callback(new returnValue { IsReturn = false });
                return null;
            }
        }
        /// <summary>
        /// 异步回调
        /// </summary>
        /// <typeparam name="outputParameterType">输出参数类型</typeparam>
        public sealed class callReturn<outputParameterType>
        {
            /// <summary>
            /// 回调委托
            /// </summary>
            private Action<returnValue> callback;
            /// <summary>
            /// 异步回调返回值
            /// </summary>
            /// <param name="outputParameter">输出参数</param>
            private void onReturn(returnValue<outputParameterType> outputParameter)
            {
                try
                {
                    callback(new returnValue { IsReturn = outputParameter.IsReturn });
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, false);
                }
            }
            /// <summary>
            /// 获取异步回调
            /// </summary>
            /// <param name="callback">回调委托</param>
            /// <returns>异步回调</returns>
            public static Action<returnValue<outputParameterType>> Get(Action<returnValue> callback)
            {
                if (callback == null) return null;
                try
                {
                    return new callReturn<outputParameterType> { callback = callback }.onReturn;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                callback(new returnValue { IsReturn = false });
                return null;
            }
        }
        /// <summary>
        /// 异步回调
        /// </summary>
        /// <typeparam name="returnType">返回值类型</typeparam>
        /// <typeparam name="outputParameterType">输出参数类型</typeparam>
        public sealed class callReturn<returnType, outputParameterType>
            where outputParameterType : IReturnParameter<returnType>
        {
            /// <summary>
            /// 回调委托
            /// </summary>
            private Action<returnValue<returnType>> callback;
            /// <summary>
            /// 异步回调返回值
            /// </summary>
            /// <param name="outputParameter">输出参数</param>
            private void onReturn(returnValue<outputParameterType> outputParameter)
            {
                try
                {
                    callback(outputParameter.IsReturn ? new returnValue<returnType> { IsReturn = true, Value = outputParameter.Value.Return } : new returnValue<returnType> { IsReturn = false });
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, false);
                }
            }
            /// <summary>
            /// 获取异步回调
            /// </summary>
            /// <param name="callback">回调委托</param>
            /// <returns>异步回调</returns>
            public static Action<returnValue<outputParameterType>> Get(Action<returnValue<returnType>> callback)
            {
                if (callback == null) return null;
                try
                {
                    return new callReturn<returnType, outputParameterType> { callback = callback }.onReturn;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                callback(new returnValue<returnType> { IsReturn = false });
                return null;
            }
        }
        /// <summary>
        /// 异步回调泛型返回值
        /// </summary>
        /// <typeparam name="returnType">返回值类型</typeparam>
        /// <typeparam name="outputParameterType">输出参数类型</typeparam>
        public sealed class callReturnGeneric<returnType, outputParameterType>
            where outputParameterType : IReturnParameter<object>
        {
            /// <summary>
            /// 回调委托
            /// </summary>
            private Action<returnValue<returnType>> callback;
            /// <summary>
            /// 异步回调返回值
            /// </summary>
            /// <param name="outputParameter">输出参数</param>
            private void onReturn(returnValue<outputParameterType> outputParameter)
            {
                try
                {
                    callback(outputParameter.IsReturn ? new returnValue<returnType> { IsReturn = true, Value = (returnType)outputParameter.Value.Return } : new returnValue<returnType> { IsReturn = false });
                }
                catch (Exception error)
                {
                    fastCSharp.log.Error.Add(error, null, false);
                }
            }
            /// <summary>
            /// 异步回调泛型返回值
            /// </summary>
            /// <param name="callback">异步回调返回值</param>
            /// <returns>异步回调返回值</returns>
            public static Action<returnValue<outputParameterType>> Get(Action<returnValue<returnType>> callback)
            {
                if (callback == null) return null;
                try
                {
                    return new callReturnGeneric<returnType, outputParameterType> { callback = callback }.onReturn;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                callback(new returnValue<returnType> { IsReturn = false });
                return null;
            }
        }
        /// <summary>
        /// 同步等待调用
        /// </summary>
        public sealed class waitCall
        {
            /// <summary>
            /// 同步等待
            /// </summary>
            private autoWaitHandle waitHandle;
            /// <summary>
            /// 输出参数
            /// </summary>
            private bool outputParameter;
            /// <summary>
            /// 回调处理
            /// </summary>
            public Action<bool> OnReturn;
            /// <summary>
            /// 调用返回值（警告：每次调用只能使用一次）
            /// </summary>
            public returnValue Value
            {
                get
                {
                    waitHandle.Wait();
                    bool outputParameter = this.outputParameter;
                    this.outputParameter = false;
                    typePool<waitCall>.Push(this);
                    return new returnValue { IsReturn = outputParameter };
                }
            }
            /// <summary>
            /// 同步等待调用
            /// </summary>
            private waitCall()
            {
                waitHandle = new autoWaitHandle(false);
                OnReturn = onReturn;
            }
            /// <summary>
            /// 回调处理
            /// </summary>
            /// <param name="outputParameter">是否调用成功</param>
            private void onReturn(bool outputParameter)
            {
                this.outputParameter = outputParameter;
                //if (!outputParameter) log.Default.Add("异步调用失败(bool)", true, false);
                waitHandle.Set();
            }
            /// <summary>
            /// 获取同步等待调用
            /// </summary>
            /// <returns>同步等待调用</returns>
            public static waitCall Get()
            {
                waitCall value = typePool<waitCall>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = new waitCall();
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                        return null;
                    }
                }
                return value;
            }
        }
        /// <summary>
        /// 同步等待调用
        /// </summary>
        /// <typeparam name="outputParameterType">输出参数类型</typeparam>
        public sealed class waitCall<outputParameterType>
        {
            /// <summary>
            /// 同步等待
            /// </summary>
            private autoWaitHandle waitHandle;
            /// <summary>
            /// 回调处理
            /// </summary>
            public Action<returnValue<outputParameterType>> OnReturn;
            /// <summary>
            /// 输出参数
            /// </summary>
            private returnValue<outputParameterType> outputParameter;
            /// <summary>
            /// 调用返回值（警告：每次调用只能使用一次）
            /// </summary>
            public returnValue<outputParameterType> Value
            {
                get
                {
                    waitHandle.Wait();
                    returnValue<outputParameterType> outputParameter = this.outputParameter;
                    this.outputParameter.Null();
                    typePool<waitCall<outputParameterType>>.Push(this);
                    return outputParameter;
                }
            }
            /// <summary>
            /// 同步等待调用
            /// </summary>
            private waitCall()
            {
                waitHandle = new autoWaitHandle(false);
                OnReturn = onReturn;
            }
            /// <summary>
            /// 回调处理
            /// </summary>
            /// <param name="outputParameter">输出参数</param>
            private void onReturn(returnValue<outputParameterType> outputParameter)
            {
                this.outputParameter = outputParameter;
                //if (!outputParameter.IsReturn) log.Default.Add("异步调用失败()", true, false);
                waitHandle.Set();
            }
            /// <summary>
            /// 获取同步等待调用
            /// </summary>
            /// <returns>同步等待调用</returns>
            public static waitCall<outputParameterType> Get()
            {
                waitCall<outputParameterType> value = typePool<waitCall<outputParameterType>>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = new waitCall<outputParameterType>();
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                        return null;
                    }
                }
                return value;
            }
        }
    }
}
