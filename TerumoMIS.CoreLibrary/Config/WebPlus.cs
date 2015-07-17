//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: WebPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  WebPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:08:07
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

namespace TerumoMIS.CoreLibrary.Config
{
    /// <summary>
    /// 网站模块相关参数 
    /// </summary>
    public sealed class WebPlus
    {
        /// <summary>
        /// web视图重新加载禁用输出成员名称
        /// </summary>
        public const string ViewOnlyName = "ViewOnly";
        /// <summary>
        /// fastCSharp js文件路径
        /// </summary>
        private string fastCSharpJsPath;
        /// <summary>
        /// fastCSharp js文件路径
        /// </summary>
        public string FastCSharpJsPath
        {
            get
            {
                if (fastCSharpJsPath != null && !Directory.Exists(fastCSharpJsPath)) fastCSharpJsPath = null;
                if (fastCSharpJsPath == null)
                {
                    try
                    {
                        string jsPath = new DirectoryInfo(CoreLibrary.PubPlus.ApplicationPath).Parent.Parent.fullName() + @"js\";
                        if (Directory.Exists(jsPath)) fastCSharpJsPath = new DirectoryInfo(jsPath).fullName().toLower();
                    }
                    catch (Exception error)
                    {
                        log.Default.Add(error, "没有找到js文件路径", false);
                    }
                }
                return fastCSharpJsPath;
            }
        }
        /// <summary>
        /// AJAX调用名称
        /// </summary>
        private string ajaxWebCallName = "/ajax";
        /// <summary>
        /// AJAX调用名称
        /// </summary>
        public string AjaxWebCallName
        {
            get { return ajaxWebCallName; }
        }
        /// <summary>
        /// AJAX调用函数名称
        /// </summary>
        private char ajaxCallName = 'n';
        /// <summary>
        ///AJAX调用函数名称
        /// </summary>
        public char AjaxCallName
        {
            get { return ajaxCallName; }
        }
        /// <summary>
        /// AJAX回调函数名称
        /// </summary>
        private char ajaxCallBackName = 'c';
        /// <summary>
        /// AJAX回调函数名称
        /// </summary>
        public char AjaxCallBackName
        {
            get { return ajaxCallBackName; }
        }
        /// <summary>
        /// AJAX是否判定Referer来源
        /// </summary>
        private bool isAjaxReferer = true;
        /// <summary>
        /// AJAX是否判定Referer来源
        /// </summary>
        public bool IsAjaxReferer
        {
            get { return isAjaxReferer; }
        }
        /// <summary>
        /// json查询对象名称
        /// </summary>
        private char queryJsonName = 'j';
        /// <summary>
        /// json查询对象名称
        /// </summary>
        public char QueryJsonName
        {
            get { return queryJsonName; }
        }
        /// <summary>
        /// 重新加载视图查询名称
        /// </summary>
        private char reViewName = 'v';
        /// <summary>
        /// 重新加载视图查询名称
        /// </summary>
        public char ReViewName
        {
            get { return reViewName; }
        }
        /// <summary>
        /// web视图默认查询参数名称
        /// </summary>
        private string viewQueryName = "query";
        /// <summary>
        /// web视图默认查询参数名称
        /// </summary>
        public string ViewQueryName
        {
            get { return viewQueryName; }
        }
        ///// <summary>
        ///// Json转换时间差
        ///// </summary>
        //private DateTime parseJavascriptMinTime = new DateTime(1970, 1, 1, 0, 0, 0);
        ///// <summary>
        ///// Json转换时间差
        ///// </summary>
        //public DateTime ParseJavascriptMinTime
        //{
        //    get { return parseJavascriptMinTime; }
        //}
        /// <summary>
        /// 客户端缓存时间(单位:秒)
        /// </summary>
        private int clientCacheSeconds = 0;
        /// <summary>
        /// 客户端缓存时间(单位:秒)
        /// </summary>
        public int ClientCacheSeconds
        {
            get { return clientCacheSeconds < 0 ? 0 : clientCacheSeconds; }
        }
        /// <summary>
        /// 最大文件缓存字节数(单位KB)
        /// </summary>
        private int maxCacheFileSize = 1 << 9;
        /// <summary>
        /// 最大文件缓存字节数(单位KB)
        /// </summary>
        public int MaxCacheFileSize
        {
            get { return maxCacheFileSize < 0 ? 0 : maxCacheFileSize; }
        }
        /// <summary>
        /// 最大缓存字节数(单位MB)
        /// </summary>
        private int maxCacheSize = 1 << 10;
        /// <summary>
        /// 最大缓存字节数(单位MB)
        /// </summary>
        public int MaxCacheSize
        {
            get { return maxCacheSize < 0 ? 0 : maxCacheSize; }
        }
        /// <summary>
        /// HTTP服务路径
        /// </summary>
        private string httpServerPath;
        /// <summary>
        /// HTTP服务路径
        /// </summary>
        public string HttpServerPath
        {
            get
            {
                if (httpServerPath == null) httpServerPath = pub.Default.WorkPath;
                return httpServerPath;
            }
        }
        /// <summary>
        /// 公用错误缓存最大字节数
        /// </summary>
        private int pubErrorMaxSize = 1 << 10;
        /// <summary>
        /// 公用错误缓存最大字节数
        /// </summary>
        public int PubErrorMaxSize
        {
            get { return pubErrorMaxSize; }
        }
        /// <summary>
        /// 公用错误处理最大缓存数量
        /// </summary>
        private int pubErrorMaxCacheCount = 1 << 10;
        /// <summary>
        /// 公用错误处理最大缓存数量
        /// </summary>
        public int PubErrorMaxCacheCount
        {
            get { return pubErrorMaxCacheCount; }
        }
        /// <summary>
        /// 网站模块相关参数
        /// </summary>
        private WebPlus()
        {
            pub.LoadConfig(this);
        }
        /// <summary>
        /// 默认网站模块相关参数
        /// </summary>
        public static readonly web Default = new web();
    }
}
