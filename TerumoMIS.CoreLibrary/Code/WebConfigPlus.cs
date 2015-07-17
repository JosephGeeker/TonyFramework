//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: WebConfigPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  WebConfigPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:35:53
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

namespace TerumoMIS.CoreLibrary.Code
{
    /// <summary>
    /// 网站生成配置
    /// </summary>
    public abstract class WebConfigPlus
    {
        /// <summary>
        /// Session类型
        /// </summary>
        public virtual Type SessionType
        {
            get { return typeof(int); }
        }
        /// <summary>
        /// 文件编码
        /// </summary>
        public virtual Encoding Encoding
        {
            get { return fastCSharp.config.appSetting.Encoding; }
        }
        /// <summary>
        /// 默认Cookie域名
        /// </summary>
        public abstract string CookieDomain { get; }
        /// <summary>
        /// 静态文件网站域名
        /// </summary>
        public virtual string StaticFileDomain { get { return CookieDomain; } }
        /// <summary>
        /// 图片文件域名
        /// </summary>
        public virtual string ImageDomain { get { return CookieDomain; } }
        /// <summary>
        /// 轮询网站域名
        /// </summary>
        public virtual string PollDomain { get { return CookieDomain; } }
        /// <summary>
        /// 视图加载失败重定向
        /// </summary>
        public virtual string NoViewLocation { get { return null; } }
        /// <summary>
        /// WEB视图扩展默认文件名称
        /// </summary>
        public virtual string ViewJsFileName
        {
            get { return "webView"; }
        }
        /// <summary>
        /// 客户端缓存标识版本
        /// </summary>
        public virtual int ETagVersion
        {
            get { return 0; }
        }
        /// <summary>
        /// 是否忽略大小写
        /// </summary>
        public virtual bool IgnoreCase
        {
            get { return true; }
        }
        /// <summary>
        /// 文件缓存是否预留HTTP头部空间
        /// </summary>
        public virtual bool IsFileCacheHeader
        {
            get { return true; }
        }
    }
}
