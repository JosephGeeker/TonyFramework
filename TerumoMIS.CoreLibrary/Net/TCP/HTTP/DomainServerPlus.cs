//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DomainServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  DomainServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:34:40
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

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    /// <summary>
    /// 域名服务
    /// </summary>
    public abstract class DomainServerPlus:IDisposable
    {
        /// <summary>
        /// 网站生成配置
        /// </summary>
        private sealed class nullWebConfig : fastCSharp.code.webConfig
        {
            /// <summary>
            /// 默认Cookie域名
            /// </summary>
            public override string CookieDomain
            {
                get { return null; }
            }
            /// <summary>
            /// 视图加载失败重定向
            /// </summary>
            public override string NoViewLocation
            {
                get { return null; }
            }
            /// <summary>
            /// 网站生成配置
            /// </summary>
            public static readonly nullWebConfig Default = new nullWebConfig();
        }
        /// <summary>
        /// 文件缓存
        /// </summary>
        protected sealed class fileCache : IDisposable
        {
            /// <summary>
            /// HTTP头部预留字节数
            /// </summary>
            public const int HttpHeaderSize = 256 + 64;
            /// <summary>
            /// 文件数据
            /// </summary>
            private subArray<byte> data;
            /// <summary>
            /// 文件数据
            /// </summary>
            public subArray<byte> Data
            {
                get
                {
                    if (IsData == 0)
                    {
                        Thread.Sleep(0);
                        while (IsData == 0) Thread.Sleep(1);
                    }
                    return data;
                }
            }
            /// <summary>
            /// 文件压缩数据
            /// </summary>
            private subArray<byte> gZipData;
            /// <summary>
            /// 文件数据
            /// </summary>
            public subArray<byte> GZipData
            {
                get
                {
                    if (IsData == 0)
                    {
                        Thread.Sleep(0);
                        while (IsData == 0) Thread.Sleep(1);
                    }
                    return gZipData;
                }
            }
            /// <summary>
            /// 最后修改时间
            /// </summary>
            internal byte[] lastModified;
            /// <summary>
            /// 最后修改时间
            /// </summary>
            public byte[] LastModified { get { return lastModified; } }
            /// <summary>
            /// HTTP响应输出内容类型
            /// </summary>
            public byte[] ContentType { get; internal set; }
            /// <summary>
            /// 是否已经获取数据
            /// </summary>
            internal int IsData;
            /// <summary>
            /// 是否HTML
            /// </summary>
            internal bool IsHtml;
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                date.ByteBuffers.Push(ref lastModified);
            }
            /// <summary>
            /// 文件缓存
            /// </summary>
            /// <param name="data">文件数据</param>
            /// <param name="contentType">HTTP响应输出内容类型</param>
            /// <param name="isGZip">是否压缩</param>
            /// <param name="lastModified">最后修改时间</param>
            internal void Set(subArray<byte> data, byte[] contentType, bool isGZip, byte[] lastModified, bool isHtml)
            {
                lastModified = date.CopyBytes(lastModified);
                Set(data, contentType, isGZip, isHtml);
            }
            /// <summary>
            /// 文件缓存
            /// </summary>
            /// <param name="data">文件数据</param>
            /// <param name="contentType">HTTP响应输出内容类型</param>
            /// <param name="isGZip">是否压缩</param>
            internal void Set(subArray<byte> data, byte[] contentType, bool isGZip, bool isHtml)
            {
                ContentType = contentType;
                IsHtml = isHtml;
                try
                {
                    this.data = data;
                    if (isGZip) gZipData = response.GetCompress(data, null, data.StartIndex);
                    if (gZipData.Count == 0) gZipData = data;
                }
                finally { IsData = 1; }
            }
            /// <summary>
            /// 文件数据字节数
            /// </summary>
            public int Size
            {
                get
                {
                    int size = data.Count;
                    if (data.Array != gZipData.Array) size += gZipData.Count;
                    return size;
                }
            }
        }
        /// <summary>
        /// 默认扩展名唯一哈希
        /// </summary>
        private struct defaultExtensionName : IEquatable<defaultExtensionName>
        {
            /// <summary>
            /// 扩展名
            /// </summary>
            public subString ExtensionName;
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="name">扩展名</param>
            /// <returns>默认扩展名唯一哈希</returns>
            public static implicit operator defaultExtensionName(string name)
            {
                return new defaultExtensionName { ExtensionName = subString.Unsafe(name, 0) };
            }
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="name">扩展名</param>
            /// <returns>默认扩展名唯一哈希</returns>
            public static implicit operator defaultExtensionName(subString name) { return new defaultExtensionName { ExtensionName = name }; }
            /// <summary>
            /// 获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public unsafe override int GetHashCode()
            {
                fixed (char* nameFixed = ExtensionName.value)
                {
                    char* start = nameFixed + ExtensionName.StartIndex;
                    uint code = (uint)(start[ExtensionName.Length - 2] << 16) + (uint)(start[ExtensionName.Length >> 2] << 8) + (uint)start[ExtensionName.Length - 1];
                    return (int)(((code >> 11) ^ (code >> 6) ^ code) & ((1U << 7) - 1));
                }
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public unsafe bool Equals(defaultExtensionName other)
            {
                return ExtensionName.Equals(other.ExtensionName);
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((defaultExtensionName)obj);
            }
        }
        /// <summary>
        /// 默认扩展名集合
        /// </summary>
        private static readonly uniqueHashSet<defaultExtensionName> defaultExtensionNames = new uniqueHashSet<defaultExtensionName>(new defaultExtensionName[] { "avi", "bmp", "css", "cur", "doc", "docx", "gif", "htm", "html", "ico", "jpg", "jpeg", "js", "mp3", "mp4", "mpg", "pdf", "png", "rar", "rm", "rmvb", "svg", "swf", "txt", "wav", "xml", "xls", "xlsx", "zip", "z7" }, 107);
        /// <summary>
        /// 默认非压缩扩展名唯一哈希
        /// </summary>
        private struct defaultCompressExtensionName : IEquatable<defaultCompressExtensionName>
        {
            /// <summary>
            /// 非压缩扩展名
            /// </summary>
            public string ExtensionName;
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="name">非压缩扩展名</param>
            /// <returns>默认扩展名唯一哈希</returns>
            public static implicit operator defaultCompressExtensionName(string name) { return new defaultCompressExtensionName { ExtensionName = name }; }
            /// <summary>
            /// 获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public unsafe override int GetHashCode()
            {
                fixed (char* nameFixed = ExtensionName)
                {
                    int code = (nameFixed[ExtensionName.Length >> 2] << 8) + nameFixed[1];
                    return (nameFixed[ExtensionName.Length - 1] ^ (code >> 3) ^ (code >> 5)) & ((1 << 5) - 1);
                }
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public unsafe bool Equals(defaultCompressExtensionName other)
            {
                return ExtensionName == other.ExtensionName;
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((defaultCompressExtensionName)obj);
            }
        }
        /// <summary>
        /// 默认非压缩扩展名集合
        /// </summary>
        private static readonly uniqueHashSet<defaultCompressExtensionName> defaultCompressExtensionNames = new uniqueHashSet<defaultCompressExtensionName>(new defaultCompressExtensionName[] { "avi", "cur", "gif", "ico", "jpg", "jpeg", "mp3", "mp4", "mpg", "png", "rar", "rm", "rmvb", "wav", "zip", "z7" }, 27);
        /// <summary>
        /// 内容类型
        /// </summary>
        protected static byte[] ContentTypeBytes { get { return header.ContentTypeBytes; } }
        /// <summary>
        /// 文件缓存名称缓冲区
        /// </summary>
        private static readonly memoryPool cacheNameBuffer = memoryPool.GetPool(fastCSharp.io.file.MaxFullNameLength << 1);
        /// <summary>
        /// 加载检测路径
        /// </summary>
        internal string LoadCheckPath;
        /// <summary>
        /// 文件路径
        /// </summary>
        protected virtual string path { get { return null; } }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string WorkPath { get; private set; }
        /// <summary>
        /// 最大缓存字节数(单位MB)
        /// </summary>
        protected virtual int maxCacheSize { get { return config.web.Default.MaxCacheSize; } }
        /// <summary>
        /// 最大缓存字节数
        /// </summary>
        private int cacheSize;
        /// <summary>
        /// 最大文件缓存字节数(单位KB)
        /// </summary>
        protected virtual int maxCacheFileSize { get { return config.web.Default.MaxCacheFileSize; } }
        /// <summary>
        /// 网站生成配置
        /// </summary>
        protected internal readonly fastCSharp.code.webConfig WebConfig;
        /// <summary>
        /// 输出编码
        /// </summary>
        internal readonly Encoding ResponseEncoding;
        /// <summary>
        /// 默认内容类型头部
        /// </summary>
        internal readonly byte[] HtmlContentType;
        /// <summary>
        /// 默认内容类型头部
        /// </summary>
        internal readonly byte[] JsContentType;
        /// <summary>
        /// 获取Session
        /// </summary>
        public ISession Session { get; protected set; }
        /// <summary>
        /// 最大文件缓存字节数
        /// </summary>
        private int fileSize;
        /// <summary>
        /// 客户端缓存时间(单位:秒)
        /// </summary>
        protected virtual int clientCacheSeconds { get { return config.web.Default.ClientCacheSeconds; } }
        /// <summary>
        /// 缓存控制参数
        /// </summary>
        protected byte[] cacheControl;
        /// <summary>
        /// 当前缓存字节数
        /// </summary>
        private int currentCacheSize;
        /// <summary>
        /// 文件缓存是否预留HTTP头部
        /// </summary>
        protected virtual bool isCacheHttpHeader { get { return false; } }
        /// <summary>
        /// 文件缓存是否预留HTTP头部
        /// </summary>
        private bool isCacheHeader;
        /// <summary>
        /// HTML文件缓存是否预留HTTP头部
        /// </summary>
        protected virtual bool isCacheHtmlHttpHeader { get { return false; } }
        /// <summary>
        /// HTML文件缓存是否预留HTTP头部
        /// </summary>
        private bool isCacheHtmlHeader;
        /// <summary>
        /// 域名信息集合
        /// </summary>
        private domain[] domains;
        /// <summary>
        /// 停止服务处理
        /// </summary>
        private Action onStop;
        /// <summary>
        /// 文件缓存
        /// </summary>
        private fifoPriorityQueue<hashBytes, fileCache> cache;
        /// <summary>
        /// 文件缓存访问锁
        /// </summary>
        private int cacheLock;
        ///// <summary>
        ///// 是否支持请求范围
        ///// </summary>
        //protected internal virtual bool isRequestRange { get { return false; } }
        /// <summary>
        /// 是否启动服务
        /// </summary>
        private int isStart;
        /// <summary>
        /// 是否停止服务
        /// </summary>
        private int isDisposed;
        /// <summary>
        ///错误输出数据
        /// </summary>
        protected keyValue<response, response>[] errorResponse;
        /// <summary>
        /// 域名服务
        /// </summary>
        protected domainServer()
        {
            WebConfig = getWebConfig() ?? nullWebConfig.Default;
            if (WebConfig != null) ResponseEncoding = WebConfig.Encoding;
            if (ResponseEncoding == null) ResponseEncoding = fastCSharp.config.appSetting.Encoding;
            if (ResponseEncoding.CodePage == fastCSharp.config.appSetting.Encoding.CodePage)
            {
                HtmlContentType = response.HtmlContentType;
                JsContentType = response.JsContentType;
            }
            else
            {
                HtmlContentType = ("text/html; charset=" + ResponseEncoding.WebName).getBytes();
                JsContentType = ("application/x-javascript; charset=" + ResponseEncoding.WebName).getBytes();
            }
        }
        /// <summary>
        /// 网站生成配置
        /// </summary>
        /// <returns>网站生成配置</returns>
        protected virtual fastCSharp.code.webConfig getWebConfig() { return null; }
        /// <summary>
        /// 启动HTTP服务
        /// </summary>
        /// <param name="domains">域名信息集合</param>
        /// <param name="onStop">停止服务处理</param>
        /// <returns>是否启动成功</returns>
        public abstract bool Start(domain[] domains, Action onStop);
        /// <summary>
        /// 创建错误输出数据
        /// </summary>
        protected unsafe virtual void createErrorResponse()
        {
            keyValue<response, response>[] errorResponse = new keyValue<response, response>[Enum.GetMaxValue<response.state>(-1) + 1];
            int isResponse = 0;
            try
            {
                byte[] path = new byte[9];
                fixed (byte* pathFixed = path)
                {
                    *pathFixed = (byte)'/';
                    *(int*)(pathFixed + sizeof(int)) = '.' + ('h' << 8) + ('t' << 16) + ('m' << 24);
                    *(pathFixed + sizeof(int) * 2) = (byte)'l';
                    foreach (response.state type in System.Enum.GetValues(typeof(response.state)))
                    {
                        response.stateInfo state = Enum<response.state, response.stateInfo>.Array((int)type);
                        if (state != null && state.IsError)
                        {
                            int stateValue = state.Number, value = stateValue / 100;
                            *(pathFixed + 1) = (byte)(value + '0');
                            stateValue -= value * 100;
                            *(pathFixed + 2) = (byte)((value = stateValue / 10) + '0');
                            *(pathFixed + 3) = (byte)((stateValue - value * 10) + '0');
                            keyValue<response, fileCache> cache = file(subArray<byte>.Unsafe(path, 0, path.Length), default(subArray<byte>));
                            fileCache fileCache = cache.Value;
                            if (fileCache == null)
                            {
                                if (cache.Key != null)
                                {
                                    errorResponse[(int)type].Set(cache.Key, cache.Key);
                                    isResponse = 1;
                                }
                            }
                            else
                            {
                                response response = response.Get(), gzipResponse = response.Get();
                                response.State = gzipResponse.State = type;
                                response.SetBody(fileCache.Data, true, fileCache.Data.StartIndex == fileCache.HttpHeaderSize);
                                gzipResponse.SetBody(fileCache.GZipData, true, fileCache.GZipData.StartIndex == fileCache.HttpHeaderSize);
                                gzipResponse.ContentEncoding = response.GZipEncoding;
                                errorResponse[(int)type].Set(response, gzipResponse);
                                isResponse = 1;
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            if (isResponse != 0) this.errorResponse = errorResponse;
        }
        /// <summary>
        /// HTTP请求处理
        /// </summary>
        /// <param name="socket">HTTP套接字</param>
        /// <param name="socketIdentity">套接字操作编号</param>
        public abstract void Request(socketBase socket, long socketIdentity);
        /// <summary>
        /// 获取WEB视图URL重写路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual byte[] GetViewRewrite(subArray<byte> path)
        {
            return null;
        }
        /// <summary>
        /// 获取错误数据
        /// </summary>
        /// <param name="state">错误状态</param>
        /// <param name="isGzip">是否支持GZip压缩</param>
        /// <returns>错误数据</returns>
        internal response GetErrorResponseData(response.state state, bool isGzip)
        {
            if (errorResponse != null)
            {
                return isGzip ? errorResponse[(int)state].Value : errorResponse[(int)state].Key;
            }
            return null;
        }
        /// <summary>
        /// 设置文件缓存
        /// </summary>
        protected void setCache()
        {
            cache = new fifoPriorityQueue<hashBytes, fileCache>();
            cacheSize = maxCacheSize << 20;
            if (cacheSize < 0) log.Default.Add("最大缓存字节数(单位MB) " + maxCacheSize.toString() + " << 20 = " + cacheSize.toString(), false, false);
            fileSize = maxCacheFileSize << 10;
            if (fileSize < 0) log.Default.Add("最大文件缓存字节数(单位MB) " + maxCacheSize.toString() + " << 10 = " + fileSize.toString(), false, false);
            if (fileSize > cacheSize) fileSize = cacheSize;
            cacheControl = ("public, max-age=" + clientCacheSeconds.toString()).getBytes();
            isCacheHeader = isCacheHttpHeader;
            isCacheHtmlHeader = isCacheHtmlHttpHeader;
        }
        /// <summary>
        /// HTTP文件请求处理
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <param name="ifModifiedSince">文件修改时间</param>
        /// <returns>文件缓存+HTTP响应输出</returns>
        protected unsafe keyValue<response, fileCache> file(subArray<byte> path, subArray<byte> ifModifiedSince)
        {
            string cacheFileName = null;
            try
            {
                if (path.Count != 0 && WorkPath.Length + path.Count <= fastCSharp.io.file.MaxFullNameLength)
                {
                    byte[] buffer = cacheNameBuffer.Get();
                    try
                    {
                        fixed (byte* bufferFixed = buffer, pathFixed = path.Array)
                        {
                            byte* pathStart = pathFixed + path.StartIndex, lowerPath = bufferFixed, pathEnd = pathStart + path.Count;
                            if (*pathStart == '/') ++pathStart;
                            *pathEnd = (byte)':';
                            byte directorySeparatorChar = (byte)Path.DirectorySeparatorChar;
                            while (*pathStart != ':')
                            {
                                if ((uint)(*pathStart - 'A') < 26) *lowerPath++ = (byte)(*pathStart++ | 0x20);
                                else
                                {
                                    *lowerPath++ = *pathStart == '/' ? directorySeparatorChar : *pathStart;
                                    ++pathStart;
                                }
                            }
                            if (pathStart != pathEnd) return new keyValue<response, fileCache>(response.Blank, null);
                            hashBytes cacheKey = subArray<byte>.Unsafe(buffer, 0, (int)(lowerPath - bufferFixed));
                            interlocked.NoCheckCompareSetSleep0(ref cacheLock);
                            fileCache fileCache = cache.Get(cacheKey, null);
                            cacheLock = 0;
                            if (fileCache == null)
                            {
                                cacheFileName = fastCSharp.String.FastAllocateString(WorkPath.Length + cacheKey.Length);
                                fixed (char* nameFixed = cacheFileName)
                                {
                                    unsafer.String.Copy(WorkPath, nameFixed);
                                    unsafer.memory.ToLower(bufferFixed, bufferFixed + cacheKey.Length, nameFixed + WorkPath.Length);
                                }
                                FileInfo file = new FileInfo(cacheFileName);
                                if (file.Exists)
                                {
                                    string fileName = file.FullName;
                                    if (fileName.Length <= fastCSharp.io.file.MaxFullNameLength && fileName.toLower().StartsWith(WorkPath, StringComparison.Ordinal))
                                    {
                                        subString extensionName = subString.Unsafe(fileName, 0, 0);
                                        int extensionIndex = fileName.LastIndexOf('.');
                                        if (++extensionIndex != 0)
                                        {
                                            int pathIndex = fileName.LastIndexOf(Path.DirectorySeparatorChar);
                                            if (pathIndex < extensionIndex) extensionName.UnsafeSet(extensionIndex, fileName.Length - extensionIndex);
                                        }
                                        if (isFile(extensionName))
                                        {
                                            bool isNewFile = false;
                                            cacheKey = cacheKey.Copy();
                                            interlocked.NoCheckCompareSetSleep0(ref cacheLock);
                                            try
                                            {
                                                if ((fileCache = cache.Get(cacheKey, null)) == null)
                                                {
                                                    cache.Set(cacheKey, fileCache = new fileCache());
                                                    isNewFile = true;
                                                }
                                            }
                                            finally { cacheLock = 0; }
                                            if (isNewFile)
                                            {
                                                try
                                                {
                                                    fileCache.lastModified = file.LastWriteTimeUtc.toBytes();
                                                    if (ifModifiedSince.Count == fileCache.lastModified.Length && fastCSharp.unsafer.memory.Equal(fileCache.lastModified, pathFixed + ifModifiedSince.StartIndex, ifModifiedSince.Count)) return new keyValue<response, fileCache>(response.NotChanged304, null);
                                                    if (file.Length <= fileSize)
                                                    {
                                                        bool isHtml = extensionName == "html" || extensionName == "htm";
                                                        subArray<byte> fileData = readCacheFile(extensionName);
                                                        fileCache.Set(fileData, isHtml ? HtmlContentType : fastCSharp.web.contentTypeInfo.GetContentType(extensionName), this.isCompress(extensionName), isHtml);
                                                        int cacheSize = fileCache.Size, minSize = this.cacheSize <= cacheSize ? cacheSize : this.cacheSize;
                                                        interlocked.NoCheckCompareSetSleep0(ref cacheLock);
                                                        try
                                                        {
                                                            cache.Set(cacheKey, fileCache);
                                                            for (currentCacheSize += cacheSize; currentCacheSize > minSize; currentCacheSize -= cache.Pop().Value.Size) ;
                                                        }
                                                        finally { cacheLock = 0; }
                                                    }
                                                    else
                                                    {
                                                        response fileResponse = response.Get(true);
                                                        fileResponse.State = http.response.state.Ok200;
                                                        fileResponse.BodyFile = fileName;
                                                        fileResponse.CacheControl = cacheControl;
                                                        fileResponse.ContentType = extensionName == "html" || extensionName == "htm" ? HtmlContentType : fastCSharp.web.contentTypeInfo.GetContentType(extensionName);
                                                        fileResponse.LastModified = fileCache.lastModified;
                                                        return new keyValue<response, fileCache>(fileResponse, null);
                                                    }
                                                }
                                                finally
                                                {
                                                    if (fileCache.IsData == 0)
                                                    {
                                                        fileCache.IsData = 1;
                                                        interlocked.NoCheckCompareSetSleep0(ref cacheLock);
                                                        try
                                                        {
                                                            if (cache.Remove(cacheKey, out fileCache)) fileCache.Dispose();
                                                        }
                                                        finally
                                                        {
                                                            cacheLock = 0;
                                                            fileCache = null;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return new keyValue<response, fileCache>(null, fileCache);
                        }
                    }
                    finally { cacheNameBuffer.Push(ref buffer); }
                }
            }
            catch (Exception error)
            {
                log.Error.Add(error, cacheFileName, false);
            }
            return default(keyValue<response, fileCache>);
        }
        /// <summary>
        /// 读取缓存文件内容
        /// </summary>
        /// <param name="extensionName">文件扩展名</param>
        /// <returns>文件内容</returns>
        protected virtual subArray<byte> readCacheFile(subString extensionName)
        {
            return ReadCacheFile(extensionName.value, WebConfig.IsFileCacheHeader);
        }
        /// <summary>
        /// 读取缓存文件内容
        /// </summary>
        /// <param name="extensionName"></param>
        /// <param name="isFileCacheHeader"></param>
        /// <returns></returns>
        public static subArray<byte> ReadCacheFile(string extensionName, bool isFileCacheHeader)
        {
            if (isFileCacheHeader)
            {
                using (FileStream fileStream = new FileStream(extensionName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int length = (int)fileStream.Length;
                    byte[] data = new byte[fileCache.HttpHeaderSize + length];
                    fileStream.Read(data, fileCache.HttpHeaderSize, length);
                    return subArray<byte>.Unsafe(data, fileCache.HttpHeaderSize, length);
                }
            }
            return new subArray<byte>(File.ReadAllBytes(extensionName));
        }
        /// <summary>
        /// HTTP文件请求处理
        /// </summary>
        /// <param name="request">请求头部信息</param>
        /// <returns>HTTP响应</returns>
        protected response file(requestHeader request)
        {
            try
            {
                return file(request, file(request.Path, request.IfModifiedSince));
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            return null;
        }
        /// <summary>
        /// HTTP文件请求处理
        /// </summary>
        /// <param name="request">请求头部信息</param>
        /// <param name="path">重定向URL</param>
        /// <returns>HTTP响应</returns>
        protected response file(requestHeader request, byte[] path)
        {
            try
            {
                return file(request, file(subArray<byte>.Unsafe(path, 0, path.Length), request.IfModifiedSince));
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            return null;
        }
        /// <summary>
        /// HTTP文件请求处理
        /// </summary>
        /// <param name="request">请求头部信息</param>
        /// <param name="cache">文件输出信息</param>
        /// <returns>HTTP响应</returns>
        private unsafe response file(requestHeader request, keyValue<response, fileCache> cache)
        {
            fileCache fileCache = cache.Value;
            if (fileCache == null)
            {
                response fileResponse = cache.Key;
                if (fileResponse != null && fileResponse.BodyFile != null
                    && request.IsRange && !request.FormatRange(fileResponse.BodySize))
                {
                    return fastCSharp.net.tcp.http.response.RangeNotSatisfiable416;
                }
                return fileResponse;
            }
            if (request.IsRange && !request.FormatRange(fileCache.Data.Count))
            {
                return fastCSharp.net.tcp.http.response.RangeNotSatisfiable416;
            }
            subArray<byte> body = request.IsGZip && !request.IsRange ? fileCache.GZipData : fileCache.Data;
            response response = response.Get(true);
            response.State = http.response.state.Ok200;
            response.SetBody(body, true, body.StartIndex == fileCache.HttpHeaderSize && (fileCache.IsHtml ? isCacheHtmlHeader : isCacheHeader));
            response.CacheControl = cacheControl;
            response.ContentType = fileCache.ContentType;
            if (body.Array != fileCache.Data.Array) response.ContentEncoding = response.GZipEncoding;
            response.LastModified = fileCache.lastModified;
            return response;
        }
        /// <summary>
        /// 设置文件缓存
        /// </summary>
        /// <param name="request">请求头部信息</param>
        /// <param name="response">HTTP响应信息</param>
        /// <param name="contentType">HTTP响应输出类型</param>
        protected unsafe void setCache(requestHeader request, response response, byte[] contentType)
        {
            subArray<byte> path = request.Path;
            if (path.Count != 0 && path.Count <= fastCSharp.io.file.MaxFullNameLength)
            {
                try
                {
                    fixed (byte* pathFixed = path.Array)
                    {
                        byte[] buffer;
                        byte* pathStart = pathFixed + path.StartIndex;
                        if (*pathStart == '/')
                        {
                            ++pathStart;
                            buffer = new byte[path.Count - 1];
                        }
                        else buffer = new byte[path.Count];
                        fixed (byte* bufferFixed = buffer) unsafer.memory.ToLower(pathStart, pathStart + buffer.Length, bufferFixed);
                        hashBytes cacheKey = subArray<byte>.Unsafe(buffer, 0, buffer.Length);
                        fileCache fileCache = new fileCache();
                        fileCache.Set(response.Body, contentType, false, response.LastModified, false);
                        int cacheSize = fileCache.Size, minSize = this.cacheSize <= cacheSize ? cacheSize : this.cacheSize;
                        interlocked.NoCheckCompareSetSleep0(ref cacheLock);
                        try
                        {
                            fileCache oldFileCache = cache.Set(cacheKey, fileCache);
                            currentCacheSize += cacheSize;
                            if (oldFileCache == null)
                            {
                                while (currentCacheSize > minSize)
                                {
                                    fileCache removeFileCache = cache.Pop().Value;
                                    currentCacheSize -= removeFileCache.Size;
                                    removeFileCache.Dispose();
                                }
                            }
                            else
                            {
                                currentCacheSize -= oldFileCache.Size;
                                oldFileCache.Dispose();
                            }
                        }
                        finally { cacheLock = 0; }
                    }
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
            }
        }
        /// <summary>
        /// 是否允许文件扩展名
        /// </summary>
        /// <param name="extensionName">文件扩展名</param>
        /// <returns>是否允许文件扩展名</returns>
        protected virtual bool isFile(subString extensionName)
        {
            return extensionName.Length != 0 && defaultExtensionNames.Contains(extensionName);
        }
        /// <summary>
        /// 是否允许压缩文件扩展名
        /// </summary>
        /// <param name="extensionName">文件扩展名</param>
        /// <returns>是否允许压缩文件扩展名</returns>
        protected virtual bool isCompress(string extensionName)
        {
            return !defaultCompressExtensionNames.Contains(extensionName);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual bool dispose()
        {
            isStart = 1;
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                if (onStop != null) onStop();
                if (cache != null)
                {
                    while (cache.Count != 0) cache.Pop().Value.Dispose();
                    cache = null;
                }
                return true;
            }
            isDisposed = 1;
            return false;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            dispose();
        }
        /// <summary>
        /// 重定向服务
        /// </summary>
        public abstract class locationServer : domainServer
        {
            /// <summary>
            /// 重定向域名
            /// </summary>
            private byte[] locationDomain;
            /// <summary>
            /// 客户端缓存时间(单位:秒)
            /// </summary>
            protected override int clientCacheSeconds
            {
                get { return 0; }
            }
            /// <summary>
            /// 最大文件缓存字节数(单位KB)
            /// </summary>
            protected override int maxCacheFileSize
            {
                get { return 0; }
            }
            /// <summary>
            /// 文件路径
            /// </summary>
            protected override int maxCacheSize
            {
                get { return 0; }
            }
            /// <summary>
            /// 启动HTTP服务
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="onStop">停止服务处理</param>
            /// <returns>是否启动成功</returns>
            public override bool Start(domain[] domains, Action onStop)
            {
                if (isStart == 0)
                {
                    string domain = getLocationDomain();
                    if (domain.length() != 0)
                    {
                        if (domain[domain.Length - 1] != '/') domain += "/";
                        byte[] domainData = domain.getBytes();
                        if (Interlocked.CompareExchange(ref isStart, 1, 0) == 0)
                        {
                            locationDomain = domainData;
                            this.domains = domains;
                            this.onStop = onStop;
                            return true;
                        }
                    }
                }
                return false;
            }
            /// <summary>
            /// 获取包含协议的重定向域名,比如 http://www.ligudan.com
            /// </summary>
            /// <returns>获取包含协议的重定向域名</returns>
            protected abstract string getLocationDomain();
            /// <summary>
            /// HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            public override unsafe void Request(socketBase socket, long socketIdentity)
            {
                requestHeader request = socket.RequestHeader;
                if ((request.ContentLength | request.Boundary.Count) == 0)
                {
                    response response = response.Get(true);
                    response.State = http.response.state.MovedPermanently301;
                    subArray<byte> uri = request.Uri;
                    if (uri.Count != 0 && locationDomain.Length + uri.Count <= cacheNameBuffer.Size)
                    {
                        fixed (byte* uriFixed = uri.Array)
                        {
                            byte* uriStart = uriFixed + uri.StartIndex;
                            int length = uri.Count;
                            if (*uriStart == '/')
                            {
                                --length;
                                ++uriStart;
                            }
                            if (length != 0)
                            {
                                byte[] buffer = cacheNameBuffer.Get(0);
                                try
                                {
                                    response.Location.UnsafeSet(buffer, 0, locationDomain.Length + length);
                                    fixed (byte* locationFixed = buffer)
                                    {
                                        unsafer.memory.Copy(locationDomain, locationFixed, locationDomain.Length);
                                        unsafer.memory.Copy(uriStart, locationFixed + locationDomain.Length, length);
                                    }
                                    socket.Response(socketIdentity, ref response);
                                    return;
                                }
                                finally { cacheNameBuffer.Push(ref buffer); }
                            }
                        }
                    }
                    response.Location.UnsafeSet(locationDomain, 0, locationDomain.Length);
                    interlocked.NoCheckCompareSetSleep0(ref cacheLock);
                    try
                    {
                        socket.Response(socketIdentity, ref response);
                    }
                    finally { cacheLock = 0; }
                }
                else socket.ResponseError(socketIdentity, http.response.state.BadRequest400);
            }
        }
        /// <summary>
        /// 文件服务
        /// </summary>
        public abstract class fileServer : domainServer
        {
            /// <summary>
            /// 文件监视器
            /// </summary>
            private FileSystemWatcher fileWatcher;
            /// <summary>
            /// 启动HTTP服务
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="onStop">停止服务处理</param>
            /// <returns>是否启动成功</returns>
            public override bool Start(domain[] domains, Action onStop)
            {
                string path = (this.path.toLower() ?? LoadCheckPath).pathSuffix().ToLower();
                if (Directory.Exists(path) && Interlocked.CompareExchange(ref isStart, 1, 0) == 0)
                {
                    WorkPath = path;
                    this.domains = domains;
                    this.onStop = onStop;
                    setCache();
                    fileWatcher = new FileSystemWatcher(path);
                    fileWatcher.IncludeSubdirectories = true;
                    fileWatcher.EnableRaisingEvents = true;
                    fileWatcher.Changed += fileChanged;
                    fileWatcher.Deleted += fileChanged;
                    createErrorResponse();
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 文件更新事件
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private unsafe void fileChanged(object sender, FileSystemEventArgs e)
            {
                string fullPath = e.FullPath;
                byte[] buffer = cacheNameBuffer.Get();
                try
                {
                    fixed (byte* bufferFixed = buffer)
                    fixed (char* pathFixed = fullPath)
                    {
                        byte* write = bufferFixed;
                        char* start = pathFixed + WorkPath.Length, end = start + fullPath.Length;
                        while (start != end)
                        {
                            char value = *start++;
                            if ((uint)(value - 'A') < 26) *write++ = (byte)(value | 0x20);
                            else *write++ = (byte)value;
                        }
                        hashBytes cacheKey = subArray<byte>.Unsafe(buffer, 0, (int)(write - bufferFixed));
                        fileCache cacheData;
                        interlocked.NoCheckCompareSetSleep0(ref cacheLock);
                        try
                        {
                            if (cache.Remove(cacheKey, out cacheData))
                            {
                                currentCacheSize -= cacheData.Size;
                                cacheData.Dispose();
                            }
                        }
                        finally { cacheLock = 0; }
                    }
                }
                finally { cacheNameBuffer.Push(ref buffer); }
            }
            /// <summary>
            /// HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            public override void Request(socketBase socket, long socketIdentity)
            {
                response response = file(socket.RequestHeader);
                if (response != null) socket.Response(socketIdentity, ref response);
                else socket.ResponseError(socketIdentity, http.response.state.NotFound404);
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            protected override bool dispose()
            {
                if (base.dispose())
                {
                    if (fileWatcher != null)
                    {
                        fileWatcher.EnableRaisingEvents = false;
                        fileWatcher.Changed -= fileChanged;
                        fileWatcher.Deleted -= fileChanged;
                        fileWatcher.Dispose();
                    }
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 静态文件服务
        /// </summary>
        public abstract class staticFileServer : fileServer
        {
            /// <summary>
            /// 客户端缓存时间(单位:秒)
            /// </summary>
            protected override int clientCacheSeconds
            {
                get { return 10 * 365 * 24 * 60 * 60; }
            }
            /// <summary>
            /// 文件缓存是否预留HTTP头部
            /// </summary>
            protected override bool isCacheHttpHeader { get { return true; } }
            /// <summary>
            /// HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            public override void Request(socketBase socket, long socketIdentity)
            {
                requestHeader request = socket.RequestHeader;
                if (request.IfModifiedSince.Count == 0) this.request(socket, socketIdentity, request);
                else socket.Response(socketIdentity, response.NotChanged304);
            }
            /// <summary>
            /// HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">请求头部信息</param>
            protected virtual void request(socketBase socket, long socketIdentity, requestHeader request)
            {
                response response = file(request);
                if (response != null) socket.Response(socketIdentity, ref response);
                else socket.ResponseError(socketIdentity, http.response.state.NotFound404);
            }
        }
        /// <summary>
        /// WEB视图服务
        /// </summary>
        public abstract class viewServer : fileServer
        {
            /// <summary>
            /// 表单加载
            /// </summary>
            private class loadForm<callType, webType> : requestForm.ILoadForm
                where callType : webCall.callPool<callType, webType>
                where webType : webPage.page, webCall.IWebCall
            {
                /// <summary>
                /// HTTP套接字接口
                /// </summary>
                private socketBase socket;
                /// <summary>
                /// HTTP请求头
                /// </summary>
                private requestHeader request;
                /// <summary>
                /// WEB调用
                /// </summary>
                private callType webCall;
                /// <summary>
                /// 内存流最大字节数
                /// </summary>
                private int maxMemoryStreamSize;
                /// <summary>
                /// 表单加载
                /// </summary>
                private loadForm() { }
                /// <summary>
                /// 表单回调处理
                /// </summary>
                /// <param name="form">HTTP请求表单</param>
                public void OnGetForm(requestForm form)
                {
                    long identity;
                    if (form == null)
                    {
                        identity = webCall.WebCall.SocketIdentity;
                        webCall.WebCall.PushPool();
                        webCall.WebCall = null;
                        typePool<callType>.Push(webCall);
                    }
                    else
                    {
                        identity = form.Identity;
                        response response = null;
                        try
                        {
                            webType call = webCall.WebCall;
                            call.Response = response.Get(true);
                            call.SocketIdentity = identity;
                            call.RequestForm = form;
                            if (webCall.Call()) return;
                        }
                        catch (Exception error)
                        {
                            log.Error.Add(error, null, false);
                        }
                        finally { response.Push(ref response); }
                    }
                    socket.ResponseError(identity, fastCSharp.net.tcp.http.response.state.ServerError500);
                    socket = null;
                    request = null;
                    webCall = null;
                    typePool<loadForm<callType, webType>>.Push(this);
                }
                /// <summary>
                /// 根据HTTP请求表单值获取内存流最大字节数
                /// </summary>
                /// <param name="value">HTTP请求表单值</param>
                /// <returns>内存流最大字节数</returns>
                public int MaxMemoryStreamSize(requestForm.value value)
                {
                    return maxMemoryStreamSize > 0 ? maxMemoryStreamSize : fastCSharp.config.appSetting.StreamBufferSize;
                }
                /// <summary>
                /// 根据HTTP请求表单值获取保存文件全称
                /// </summary>
                /// <param name="value">HTTP请求表单值</param>
                /// <returns>文件全称</returns>
                public string GetSaveFileName(requestForm.value value)
                {
                    return webCall.WebCall.GetSaveFileName(value);
                }
                /// <summary>
                /// 获取表单加载
                /// </summary>
                /// <param name="socket">HTTP套接字接口</param>
                /// <param name="request">HTTP请求头</param>
                /// <param name="webCall">WEB调用接口</param>
                /// <param name="maxMemoryStreamSize">内存流最大字节数</param>
                /// <returns>表单加载</returns>
                public static loadForm<callType, webType> Get(socketBase socket, requestHeader request
                    , callType webCall, int maxMemoryStreamSize)
                {
                    loadForm<callType, webType> loadForm = typePool<loadForm<callType, webType>>.Pop() ?? new loadForm<callType, webType>();
                    loadForm.socket = socket;
                    loadForm.request = request;
                    loadForm.webCall = webCall;
                    loadForm.maxMemoryStreamSize = maxMemoryStreamSize;
                    return loadForm;
                }
            }
            /// <summary>
            /// WEB视图处理集合
            /// </summary>
            protected virtual keyValue<string[], Action<socketBase, long, requestHeader>[]> views
            {
                get { return new keyValue<string[], Action<socketBase, long, requestHeader>[]>(nullValue<string>.Array, nullValue<Action<socketBase, long, requestHeader>>.Array); }
            }
            /// <summary>
            /// WEB视图处理委托集合
            /// </summary>
            private stateSearcher.ascii<Action<socketBase, long, requestHeader>> viewMethods;
            /// <summary>
            /// WEB视图URL重写路径集合
            /// </summary>
            protected virtual keyValue<string[], string[]> rewrites
            {
                get { return new keyValue<string[], string[]>(nullValue<string>.Array, nullValue<string>.Array); }
            }
            /// <summary>
            /// WEB视图URL重写路径集合
            /// </summary>
            private stateSearcher.ascii<byte[]> viewRewrites;
            /// <summary>
            /// WEB调用处理集合
            /// </summary>
            protected virtual keyValue<string[], Action<socketBase, long, requestHeader>[]> calls
            {
                get { return new keyValue<string[], Action<socketBase, long, requestHeader>[]>(nullValue<string>.Array, nullValue<Action<socketBase, long, requestHeader>>.Array); }
            }
            /// <summary>
            /// WEB调用处理委托集合
            /// </summary>
            private stateSearcher.ascii<Action<socketBase, long, requestHeader>> callMethods;
            /// <summary>
            /// HTML文件缓存是否预留HTTP头部
            /// </summary>
            protected override bool isCacheHtmlHttpHeader { get { return true; } }
            /// <summary>
            /// 释放资源
            /// </summary>
            /// <returns></returns>
            protected override bool dispose()
            {
                if (base.dispose())
                {
                    pub.Dispose(ref viewMethods);
                    pub.Dispose(ref viewRewrites);
                    pub.Dispose(ref callMethods);
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 启动HTTP服务
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="onStop">停止服务处理</param>
            /// <returns>是否启动成功</returns>
            public override bool Start(domain[] domains, Action onStop)
            {
                keyValue<string[], Action<socketBase, long, requestHeader>[]> views = this.views;
                viewMethods = new stateSearcher.ascii<Action<socketBase, long, requestHeader>>(views.Key, views.Value);
                keyValue<string[], Action<socketBase, long, requestHeader>[]> calls = this.calls;
                callMethods = new stateSearcher.ascii<Action<socketBase, long, requestHeader>>(calls.Key, calls.Value);
                keyValue<string[], string[]> rewrites = this.rewrites;
                viewRewrites = new stateSearcher.ascii<byte[]>(rewrites.Key, rewrites.Value.getArray(value => value.getBytes()));
                return base.Start(domains, onStop);
            }
            /// <summary>
            /// HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            public override void Request(socketBase socket, long socketIdentity)
            {
                requestHeader request = socket.RequestHeader;
                Action<socketBase, long, requestHeader> view = null;
                if (request.IsSearchEngine)
                {
                    if (request.IsViewPath)
                    {
                        byte[] path = viewRewrites.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path);
                        if (path != null) view = viewMethods.Get(path);
                    }
                    if (view == null) view = viewMethods.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path) ?? callMethods.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path);
                }
                else view = callMethods.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path);
                if (view == null)
                {
                    if (!request.IsSearchEngine && request.IsViewPath)
                    {
                        byte[] path = viewRewrites.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path);
                        if (path != null)
                        {
                            response response = file(socket.RequestHeader, path);
                            if (response != null)
                            {
                                socket.Response(socketIdentity, ref response);
                                return;
                            }
                        }
                        socket.ResponseError(socketIdentity, http.response.state.NotFound404);
                    }
                    else base.Request(socket, socketIdentity);
                }
                else
                {
                    try
                    {
                        view(socket, socketIdentity, request);
                        return;
                    }
                    catch (Exception error)
                    {
                        socket.ResponseError(socketIdentity, response.state.ServerError500);
                        log.Error.Add(error, null, false);
                    }
                }
            }
            /// <summary>
            /// 获取WEB视图URL重写路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public override byte[] GetViewRewrite(subArray<byte> path)
            {
                return viewRewrites.Get(path);
            }
            /// <summary>
            /// 加载页面视图
            /// </summary>
            /// <param name="socket">HTTP套接字接口</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头</param>
            /// <param name="view">WEB视图接口</param>
            /// <param name="isPool">是否使用WEB视图池</param>
            protected void load<viewType>(socketBase socket, long socketIdentity, requestHeader request, viewType view, bool isPool)
                where viewType : webView.view
            {
                if ((request.ContentLength | request.Boundary.Count) == 0 && request.Method == web.http.methodType.GET)
                {
                    view.Socket = socket;
                    view.DomainServer = this;
                    if (view.LoadHeader(socketIdentity, request, isPool))
                    {
                        view.Load(null, false);
                        return;
                    }
                }
                else if (isPool) typePool<viewType>.Push(view);
                socket.ResponseError(socketIdentity, response.state.ServerError500);
            }
            /// <summary>
            /// 加载web调用
            /// </summary>
            /// <param name="socket">HTTP套接字接口</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头</param>
            /// <param name="call">web调用</param>
            /// <param name="maxPostDataSize"></param>
            /// <param name="maxMemoryStreamSize"></param>
            /// <param name="isOnlyPost"></param>
            /// <param name="isPool"></param>
            protected void load<callType, webType>(socketBase socket, long socketIdentity, requestHeader request, callType call
                , int maxPostDataSize, int maxMemoryStreamSize, bool isOnlyPost, bool isPool)
                where callType : webCall.callPool<callType, webType>
                where webType : webPage.page, webCall.IWebCall
            {
                if (request.ContentLength <= maxPostDataSize && (request.Method == web.http.methodType.POST || !isOnlyPost))
                {
                    webType webCall = call.WebCall;
                    webCall.Socket = socket;
                    webCall.DomainServer = this;
                    webCall.LoadHeader(socketIdentity, request, isPool);
                    if (request.Method == web.http.methodType.POST)
                    {
                        socket.GetForm(socketIdentity, loadForm<callType, webType>.Get(socket, request, call, maxMemoryStreamSize));
                        return;
                    }
                    webCall.RequestForm = null;
                    response response = webCall.Response = response.Get(true);
                    try
                    {
                        if (call.Call()) return;
                    }
                    finally { response.Push(ref response); }
                }
                else
                {
                    if (isPool) typePool<webType>.Push(ref call.WebCall);
                    typePool<callType>.Push(call);
                }
                socket.ResponseError(socketIdentity, http.response.state.ServerError500);
            }
            /// <summary>
            /// 加载web调用
            /// </summary>
            /// <param name="socket">HTTP套接字接口</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头</param>
            /// <param name="call">web调用</param>
            protected void loadAjax<callType, webType>(socketBase socket, long socketIdentity, requestHeader request, callType call)
                where callType : webCall.callPool<callType, webType>
                where webType : webCall.call, webCall.IWebCall
            {
                webType webCall = call.WebCall;
                webCall.Socket = socket;
                webCall.DomainServer = this;
                if (webCall.LoadHeader(socketIdentity, request, true) && call.Call()) return;
                socket.ResponseError(socketIdentity, response.state.ServerError500);
            }
        }
        /// <summary>
        /// WEB视图服务
        /// </summary>
        public abstract class viewServer<sessionType> : viewServer
        {
            /// <summary>
            /// Session
            /// </summary>
            private ISession<sessionType> session;
            /// <summary>
            /// 启动HTTP服务
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="onStop">停止服务处理</param>
            /// <returns>是否启动成功</returns>
            public override bool Start(domain[] domains, Action onStop)
            {
                Session = session = getSession();
                return base.Start(domains, onStop);
            }
            /// <summary>
            /// 获取Session
            /// </summary>
            /// <returns>Session</returns>
            protected virtual ISession<sessionType> getSession()
            {
                return new session<sessionType>();
            }
        }
    }
}
