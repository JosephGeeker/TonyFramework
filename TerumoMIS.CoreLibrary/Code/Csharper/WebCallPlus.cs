//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: WebCallPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  WebCallPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:49:25
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
    /// Web调用配置
    /// </summary>
    public sealed partial class WebCallPlus:WebPagePlus
    {
        /// <summary>
        /// 成员是否匹配自定义属性类型
        /// </summary>
        public bool IsAttribute = true;
        /// <summary>
        /// 是否搜索父类自定义属性
        /// </summary>
        public bool IsBaseTypeAttribute;
        /// <summary>
        /// 成员匹配自定义属性是否可继承
        /// </summary>
        public bool IsInheritAttribute;
        /// <summary>
        /// 调用全名
        /// </summary>
        public string FullName;
        /// <summary>
        /// 是否仅支持POST请求(类型有效)
        /// </summary>
        public bool IsOnlyPost;
        /// <summary>
        /// web调用接口
        /// </summary>
        public interface IWebCall : webPage.IWebPage
        {
            /// <summary>
            /// HTTP请求表单设置
            /// </summary>
            fastCSharp.net.tcp.http.requestForm RequestForm { set; }
            /// <summary>
            /// 解析web调用参数
            /// </summary>
            /// <typeparam name="parameterType">web调用参数类型</typeparam>
            /// <param name="parameter">web调用参数</param>
            /// <returns>是否成功</returns>
            bool ParseParameter<parameterType>(ref parameterType parameter)
                where parameterType : struct;
        }
        /// <summary>
        /// web调用池
        /// </summary>
        public abstract class callPool
        {
            /// <summary>
            /// web调用
            /// </summary>
            public abstract bool Call();
        }
        /// <summary>
        /// web调用池
        /// </summary>
        /// <typeparam name="callType">web调用类型</typeparam>
        /// <typeparam name="webType">web调用实例类型</typeparam>
        public abstract class callPool<callType, webType> : callPool
            where callType : callPool<callType, webType>
            where webType : IWebCall
        {
            /// <summary>
            /// web调用
            /// </summary>
            public webType WebCall;
        }
        /// <summary>
        /// web调用池
        /// </summary>
        /// <typeparam name="callType">web调用类型</typeparam>
        /// <typeparam name="webType">web调用实例类型</typeparam>
        /// <typeparam name="parameterType">web调用参数类型</typeparam>
        public abstract class callPool<callType, webType, parameterType> : callPool<callType, webType>
            where callType : callPool<callType, webType, parameterType>
            where webType : IWebCall
            where parameterType : struct
        {
            /// <summary>
            /// web调用参数
            /// </summary>
            public parameterType Parameter;
            /// <summary>
            /// web调用池
            /// </summary>
            protected callPool() : base() { }
        }
        /// <summary>
        /// web调用
        /// </summary>
        public abstract class call : webPage.page
        {
            /// <summary>
            /// HTTP请求表单设置
            /// </summary>
            public fastCSharp.net.tcp.http.requestForm RequestForm
            {
                set
                {
                    if (form == null) form = value;
                    else log.Error.Throw(log.exceptionType.ErrorOperation);
                }
            }
            /// <summary>
            /// 解析web调用参数
            /// </summary>
            /// <typeparam name="parameterType">web调用参数类型</typeparam>
            /// <param name="parameter">web调用参数</param>
            /// <returns>是否成功</returns>
            public bool ParseParameter<parameterType>(ref parameterType parameter)
                 where parameterType : struct
            {
                if (form != null && form.Json != null)
                {
                    if (form.Json.Length != 0
                        && !fastCSharp.emit.jsonParser.Parse(form.Json, ref parameter))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!requestHeader.ParseQuery(ref parameter)) return false;
                    subString queryJson = requestHeader.QueryJson;
                    if (queryJson.Length != 0
                         && !fastCSharp.emit.jsonParser.Parse(queryJson, ref parameter))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            public virtual string GetSaveFileName(fastCSharp.net.tcp.http.requestForm.value value)
            {
                return null;
            }
        }
        /// <summary>
        /// web调用
        /// </summary>
        /// <typeparam name="callType">web调用类型</typeparam>
        public abstract class call<callType> : call, IWebCall where callType : call<callType>
        {
            ///// <summary>
            ///// 是否已经加载HTTP请求头部
            ///// </summary>
            //private int isLoadHeader;
            /// <summary>
            /// 当前web调用
            /// </summary>
            private callType thisCall;
            /// <summary>
            /// 是否使用对象池
            /// </summary>
            private bool isPool;
            /// <summary>
            /// WEB页面回收
            /// </summary>
            internal override void PushPool()
            {
                if (isPool)
                {
                    isPool = false;
                    clear();
                    if (thisCall == null) thisCall = (callType)this;
                    typePool<callType>.Push(thisCall);
                }
            }
            ///// <summary>
            ///// WEB页面回收
            ///// </summary>
            //protected static void pushPool(callType call)
            //{
            //    if (call.isLoadHeader != 0)
            //    {
            //        call.clear();
            //        if (Interlocked.CompareExchange(ref call.isLoadHeader, 0, 2) == 2) typePool<callType>.Push(call);
            //        return;
            //    }
            //    log.Default.Add("WEB页面回收", true, true);
            //}
            /// <summary>
            /// HTTP请求头部处理
            /// </summary>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头部</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>是否成功</returns>
            internal override bool LoadHeader(long socketIdentity, fastCSharp.net.tcp.http.requestHeader request, bool isPool)
            {
                //if (Interlocked.CompareExchange(ref isLoadHeader, isPool ? 2 : 1, 0) == 0)
                //{
                SocketIdentity = socketIdentity;
                requestHeader = request;
                responseEncoding = request.IsWebSocket ? Encoding.UTF8 : DomainServer.ResponseEncoding;
                this.isPool = isPool;
                return true;
                //}
                //log.Default.Add(typeof(callType).fullName() + " 页面回收错误[" + isLoadHeader.toString() + "]", false, true);
                //return false;
            }
        }
    }
}
