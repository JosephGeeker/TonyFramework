//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: AjaxPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  AjaxPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:37:46
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
    /// Ajax调用配置
    /// </summary>
    public sealed partial class AjaxPlus:WebPagePlus
    {
        /// <summary>
        /// 公用错误处理函数名称
        /// </summary>
        public const string PubErrorCallName = "pub.Error";
        /// <summary>
        /// AJAX调用
        /// </summary>
        /// <typeparam name="ajaxType">AJAX调用类型</typeparam>
        public abstract class call<ajaxType> : webView.view<ajaxType>, webView.IWebView where ajaxType : call<ajaxType>
        {
            /// <summary>
            /// WEB视图加载
            /// </summary>
            /// <returns>是否成功</returns>
            protected override bool loadView()
            {
                return false;
            }
        }
        /// <summary>
        /// AJAX调用信息
        /// </summary>
        public sealed class call
        {
            /// <summary>
            /// AJAX调用
            /// </summary>
            public Action<loader> Call;
            /// <summary>
            /// 最大接收数据字节数
            /// </summary>
            public int MaxPostDataSize;
            /// <summary>
            /// 内存流最大字节数
            /// </summary>
            public int MaxMemoryStreamSize;
            /// <summary>
            /// 是否只接受POST请求
            /// </summary>
            public bool IsPost;
            /// <summary>
            /// AJAX调用信息
            /// </summary>
            /// <param name="call">AJAX调用</param>
            /// <param name="maxPostDataSize">最大接收数据字节数</param>
            /// <param name="maxMemoryStreamSize">内存流最大字节数</param>
            /// <param name="isPost">是否只接受POST请求</param>
            public call(Action<loader> call, int maxPostDataSize, int maxMemoryStreamSize, bool isPost = true)
            {
                Call = call;
                MaxPostDataSize = maxPostDataSize;
                MaxMemoryStreamSize = maxMemoryStreamSize;
                IsPost = isPost;
            }
        }
        /// <summary>
        /// 表单加载
        /// </summary>
        public class loader : requestForm.ILoadForm
        {
            /// <summary>
            /// HTTP套接字接口
            /// </summary>
            private socketBase socket;
            /// <summary>
            /// 域名服务
            /// </summary>
            private domainServer domainServer;
            /// <summary>
            /// HTTP请求头
            /// </summary>
            private requestHeader request;
            /// <summary>
            /// 套接字请求编号
            /// </summary>
            public long SocketIdentity { get; private set; }
            /// <summary>
            /// AJAX调用信息
            /// </summary>
            private call call;
            /// <summary>
            /// 表单加载
            /// </summary>
            private loader() { }
            /// <summary>
            /// HTTP请求表单
            /// </summary>
            private requestForm form;
            /// <summary>
            /// 表单加载回收
            /// </summary>
            private void push()
            {
                socket = null;
                domainServer = null;
                request = null;
                form = null;
                call = null;
                typePool<loader>.Push(this);
            }
            /// <summary>
            /// 表单回调处理
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            public void OnGetForm(requestForm form)
            {
                if (form == null) push();
                else
                {
                    SocketIdentity = form.Identity;
                    Load(form);
                }
            }
            /// <summary>
            /// WEB视图表单加载
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            internal void Load(requestForm form)
            {
                long identity = SocketIdentity;
                socketBase socket = this.socket;
                try
                {
                    this.form = form;
                    call.Call(this);
                    return;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                finally { push(); }
                socket.ResponseError(identity, response.state.ServerError500);
            }
            /// <summary>
            /// 根据HTTP请求表单值获取内存流最大字节数
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>内存流最大字节数</returns>
            public int MaxMemoryStreamSize(fastCSharp.net.tcp.http.requestForm.value value)
            {
                return call.MaxMemoryStreamSize;
            }
            /// <summary>
            /// 根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            public string GetSaveFileName(fastCSharp.net.tcp.http.requestForm.value value)
            {
                return null;
            }
            /// <summary>
            /// AJAX数据加载
            /// </summary>
            /// <typeparam name="ajaxType">AJAX调用类型</typeparam>
            /// <param name="ajax">AJAX调用</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>HTTP响应,失败返回null</returns>
            public response Load<ajaxType>(ajaxType ajax, bool isPool)
                where ajaxType : webView.view, webView.IWebView
            {
                socketBase socket = this.socket;
                ajax.Socket = socket;
                ajax.DomainServer = domainServer;
                if (ajax.LoadHeader(SocketIdentity, request, isPool))
                {
                    ajax.form = form;
                    return ajax.Response = response.Get(true);
                }
                return null;
            }
            /// <summary>
            /// AJAX数据加载
            /// </summary>
            /// <typeparam name="ajaxType">AJAX调用类型</typeparam>
            /// <param name="ajax">AJAX调用</param>
            /// <param name="parameter">参数值</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>HTTP响应,失败返回null</returns>
            public response Load<ajaxType, valueType>(ajaxType ajax, ref valueType parameter, bool isPool)
                where ajaxType : webView.view, webView.IWebView
            {
                socketBase socket = this.socket;
                try
                {
                    if (form != null && form.Json != null)
                    {
                        if (form.Json.Length != 0 && fastCSharp.emit.jsonParser.Parse(form.Json, ref parameter))
                        {
                            ajax.Socket = socket;
                            ajax.DomainServer = domainServer;
                            if (ajax.LoadHeader(SocketIdentity, request, isPool))
                            {
                                ajax.form = form;
                                return ajax.Response = response.Get(true);
                            }
                            isPool = false;
                        }
                    }
                    else
                    {
                        if (request.ParseQuery(ref parameter))
                        {
                            subString queryJson = request.QueryJson;
                            if (queryJson.Length == 0 || fastCSharp.emit.jsonParser.Parse(queryJson, ref parameter))
                            {
                                ajax.Socket = socket;
                                ajax.DomainServer = domainServer;
                                if (ajax.LoadHeader(SocketIdentity, request, isPool))
                                {
                                    ajax.form = form;
                                    return ajax.Response = response.Get(true);
                                }
                                isPool = false;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
                if (socket.ResponseError(SocketIdentity, response.state.ServerError500) && isPool) typePool<ajaxType>.Push(ajax);
                return null;
            }
            /// <summary>
            /// 加载WEB视图
            /// </summary>
            /// <typeparam name="valueType">WEB视图类型</typeparam>
            /// <param name="view">WEB视图</param>
            /// <param name="isPool">是否使用WEB视图池</param>
            public void LoadView<valueType>(valueType view, bool isPool)
                where valueType : webView.view, webView.IWebView
            {
                view.Socket = socket;
                view.DomainServer = domainServer;
                if (view.LoadHeader(SocketIdentity, request, isPool)) view.Load(form, true);
                else socket.ResponseError(SocketIdentity, response.state.ServerError500);
            }
            /// <summary>
            /// 表单加载
            /// </summary>
            /// <param name="socket">HTTP套接字接口</param>
            /// <param name="domainServer">域名服务</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头</param>
            /// <param name="call">AJAX调用信息</param>
            /// <returns>表单加载</returns>
            internal static loader Get(socketBase socket, domainServer domainServer, long socketIdentity, requestHeader request, call call)
            {
                loader loader = typePool<loader>.Pop() ?? new loader();
                loader.socket = socket;
                loader.domainServer = domainServer;
                loader.SocketIdentity = socketIdentity;
                loader.request = request;
                loader.call = call;
                return loader;
            }
        }
        /// <summary>
        /// 公用AJAX调用
        /// </summary>
        private sealed class pub : call<pub>
        {
            /// <summary>
            /// 公用错误处理参数
            /// </summary>
            public struct errorParameter
            {
                /// <summary>
                /// 错误信息
                /// </summary>
#pragma warning disable
                public string error;
#pragma warning restore
            }
            /// <summary>
            /// 错误信息队列
            /// </summary>
            private static readonly fifoPriorityQueue<hashString, string> errorQueue = new fifoPriorityQueue<hashString, string>();
            /// <summary>
            /// 错误信息队列访问锁
            /// </summary>
            private static int errorQueueLock;
            /// <summary>
            /// 公用错误处理函数
            /// </summary>
            /// <param name="error">错误信息</param>
            public void Error(string error)
            {
                if (error.length() != 0)
                {
                    bool isLog = false;
                    if (error.Length <= config.web.Default.PubErrorMaxSize)
                    {
                        hashString errorHash = error;
                        interlocked.NoCheckCompareSetSleep0(ref errorQueueLock);
                        try
                        {
                            if (errorQueue.Set(errorHash, error) == null)
                            {
                                isLog = true;
                                if (errorQueue.Count > config.web.Default.PubErrorMaxCacheCount) errorQueue.Pop();
                            }
                        }
                        finally { errorQueueLock = 0; }
                    }
                    else isLog = true;
                    if (isLog) fastCSharp.log.Default.Add(error, false, false);
                }
            }
            static pub()
            {
                if (fastCSharp.config.appSetting.IsCheckMemory) checkMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            }
        }
        /// <summary>
        /// AJAX调用加载
        /// </summary>
        /// <typeparam name="loaderType">AJAX调用加载类型</typeparam>
        public abstract class loader<loaderType> : webCall.call<loaderType> where loaderType : loader<loaderType>
        {
            /// <summary>
            /// AJAX调用
            /// </summary>
            /// <param name="methods">AJAX函数调用集合</param>
            protected void load(fastCSharp.stateSearcher.ascii<call> methods)
            {
                try
                {
                    long identity = SocketIdentity;
                    if (!fastCSharp.config.web.Default.IsAjaxReferer || fastCSharp.config.pub.Default.IsDebug || requestHeader.IsReferer)
                    {
                        call call = methods.Get(DomainServer.WebConfig.IgnoreCase ? requestHeader.LowerAjaxCallName : requestHeader.AjaxCallName);
                        if (call == null)
                        {
                            byte[] path = DomainServer.GetViewRewrite(DomainServer.WebConfig.IgnoreCase ? requestHeader.LowerAjaxCallName : requestHeader.AjaxCallName);
                            if (path != null) call = methods.Get(path);
                        }
                        if (call != null && (requestHeader.Method == web.http.methodType.POST || !call.IsPost || requestHeader.IsWebSocket))
                        {
                            if (requestHeader.ContentLength <= call.MaxPostDataSize)
                            {
                                loader loadForm = loader.Get(Socket, DomainServer, identity, requestHeader, call);
                                if (requestHeader.Method == web.http.methodType.POST) Socket.GetForm(identity, loadForm);
                                else loadForm.Load((requestForm)null);
                            }
                            else Socket.ResponseError(identity, net.tcp.http.response.state.ServerError500);
                            return;
                        }
                    }
                    Socket.ResponseError(identity, net.tcp.http.response.state.NotFound404);
                }
                finally { PushPool(); }
            }
            /// <summary>
            /// 公用错误处理函数
            /// </summary>
            /// <param name="loader">表单加载</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            protected static void pubError(fastCSharp.code.cSharp.ajax.loader loader)
            {
                pub view = fastCSharp.typePool<pub>.Pop() ?? new pub();
                pub.errorParameter parameter = new pub.errorParameter();
                response response = loader.Load(view, ref parameter, true);
                if (response != null)
                {
                    try
                    {
                        view.Error(parameter.error);
                    }
                    finally { view.AjaxResponse(ref response); }
                }
            }
        }
        /// <summary>
        /// AJAX异步回调
        /// </summary>
        /// <typeparam name="callbackType">异步回调类型</typeparam>
        /// <typeparam name="ajaxType">AJAX类型</typeparam>
        public abstract class callback<callbackType, ajaxType>
            where callbackType : callback<callbackType, ajaxType>
            where ajaxType : call<ajaxType>
        {
            /// <summary>
            /// 当前AJAX异步回调
            /// </summary>
            private callbackType thisCallback;
            /// <summary>
            /// AJAX回调对象
            /// </summary>
            private ajaxType ajax;
            /// <summary>
            /// HTTP响应
            /// </summary>
            private response response;
            /// <summary>
            /// AJAX回调处理
            /// </summary>
            private Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue> onReturnHandle;
            /// <summary>
            /// AJAX异步回调
            /// </summary>
            protected callback()
            {
                thisCallback = (callbackType)this;
                onReturnHandle = onReturn;
            }
            /// <summary>
            /// AJAX回调处理
            /// </summary>
            /// <param name="value">回调值</param>
            private void onReturn(fastCSharp.code.cSharp.asynchronousMethod.returnValue value)
            {
                try
                {
                    if (value.IsReturn)
                    {
                        ajax.cancelAsynchronous();
                        ajax.AjaxResponse(ref response);
                    }
                    else ajax.serverError500();
                }
                finally
                {
                    ajax = null;
                    response = null;
                    typePool<callbackType>.Push(thisCallback);
                }
            }
            /// <summary>
            /// 获取AJAX回调处理
            /// </summary>
            /// <param name="ajax">AJAX回调对象</param>
            /// <param name="response">HTTP响应</param>
            /// <returns>AJAX回调处理</returns>
            public Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue> Get(ajaxType ajax, response response)
            {
                this.ajax = ajax;
                this.response = response;
                ajax.setAsynchronous();
                return onReturnHandle;
            }
        }
        /// <summary>
        /// AJAX异步回调
        /// </summary>
        /// <typeparam name="callbackType">异步回调类型</typeparam>
        /// <typeparam name="ajaxType">AJAX类型</typeparam>
        /// <typeparam name="returnType">返回值类型</typeparam>
        public abstract class callback<callbackType, ajaxType, returnType>
            where callbackType : callback<callbackType, ajaxType, returnType>
            where ajaxType : webView.view
        {
            /// <summary>
            /// 当前AJAX异步回调
            /// </summary>
            private callbackType thisCallback;
            /// <summary>
            /// AJAX回调对象
            /// </summary>
            protected ajaxType ajax;
            /// <summary>
            /// HTTP响应
            /// </summary>
            protected response response;
            /// <summary>
            /// AJAX回调处理
            /// </summary>
            private Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>> onReturnHandle;
            /// <summary>
            /// AJAX异步回调
            /// </summary>
            protected callback()
            {
                thisCallback = (callbackType)this;
                onReturnHandle = onReturn;
            }
            /// <summary>
            /// AJAX回调处理
            /// </summary>
            /// <param name="value">回调值</param>
            private void onReturn(fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType> value)
            {
                try
                {
                    if (value.IsReturn)
                    {
                        ajax.cancelAsynchronous();
                        onReturnValue(value.Value);
                    }
                    else ajax.serverError500();
                }
                finally
                {
                    ajax = null;
                    response = null;
                    typePool<callbackType>.Push(thisCallback);
                }
            }
            /// <summary>
            /// AJAX回调处理
            /// </summary>
            /// <param name="value">回调值</param>
            protected abstract void onReturnValue(returnType value);
            /// <summary>
            /// 获取AJAX回调处理
            /// </summary>
            /// <param name="ajax">AJAX回调对象</param>
            /// <param name="response">HTTP响应</param>
            /// <returns>AJAX回调处理</returns>
            public Action<fastCSharp.code.cSharp.asynchronousMethod.returnValue<returnType>> Get(ajaxType ajax, response response)
            {
                this.ajax = ajax;
                this.response = response;
                ajax.setAsynchronous();
                return onReturnHandle;
            }
        }
        /// <summary>
        /// 成员是否匹配自定义属性类型
        /// </summary>
        public bool IsAttribute;
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
        /// 是否仅支持POST请求
        /// </summary>
        public bool IsOnlyPost = true;
    }
}
