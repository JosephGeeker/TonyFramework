//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: PubPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  PubPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:04:11
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerumoMIS.CoreLibrary.Code;
using TerumoMIS.CoreLibrary.Emit;

namespace TerumoMIS.CoreLibrary.Config
{
    /// <summary>
    /// 基本配置
    /// </summary>
    public sealed class PubPlus
    {
        /// <summary>
        /// 程序工作主目录
        /// </summary>
        public string WorkPath { get; private set; }
        /// <summary>
        /// 缓存文件主目录
        /// </summary>
        public string CachePath { get; private set; }
        /// <summary>
        /// 是否调试模式
        /// </summary>
        public bool IsDebug { get; private set; }
        /// <summary>
        /// 是否window服务模式
        /// </summary>
        public bool IsService { get; private set; }

        /// <summary>
        /// 默认分页大小
        /// </summary>
        private int _pageSize = 10;
        /// <summary>
        /// 默认分页大小
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
        }
        /// <summary>
        /// 默认分页大小
        /// </summary>
        private int _maxEnumArraySize = 1024;
        /// <summary>
        /// 最大枚举数组数量
        /// </summary>
        public int MaxEnumArraySize
        {
            get { return _maxEnumArraySize; }
        }
        /// <summary>
        /// 默认任务线程数
        /// </summary>
        private int _taskThreadCount = 128;
        /// <summary>
        /// 默认任务线程数
        /// </summary>
        public int TaskThreadCount
        {
            get
            {
                if (_taskThreadCount > _taskMaxThreadCount)
                {
                    LogPlus.Error.Add("默认任务线程数[" + _taskThreadCount + "] 超出 任务最大线程数[" + _taskMaxThreadCount + "]");
                    return _taskMaxThreadCount;
                }
                return _taskThreadCount;
            }
        }
        /// <summary>
        /// 任务最大线程数
        /// </summary>
        private int _taskMaxThreadCount = 65536;
        /// <summary>
        /// 任务最大线程数
        /// </summary>
        public int TaskMaxThreadCount
        {
            get { return _taskMaxThreadCount; }
        }
        /// <summary>
        /// 死锁检测分钟数,0表示不检测
        /// </summary>
        private int _lockCheckMinutes = 0;
        /// <summary>
        /// 死锁检测分钟数,0表示不检测
        /// </summary>
        public int LockCheckMinutes
        {
            get { return _lockCheckMinutes; }
        }
        /// <summary>
        /// 微型线程任务线程数
        /// </summary>
        private int _tinyThreadCount = 65536;
        /// <summary>
        /// 微型线程任务线程数
        /// </summary>
        public int TinyThreadCount
        {
            get
            {
                if (_tinyThreadCount > _taskMaxThreadCount)
                {
                    LogPlus.Error.Add("微型线程任务线程数[" + _tinyThreadCount + "] 超出 任务最大线程数[" + _taskMaxThreadCount + "]");
                    return _taskMaxThreadCount;
                }
                return _tinyThreadCount;
            }
        }
        /// <summary>
        /// 原始套接字监听缓冲区尺寸(单位:KB)
        /// </summary>
        private int _rawSocketBufferSize = 1024;
        /// <summary>
        /// 原始套接字监听缓冲区尺寸(单位:B)
        /// </summary>
        public int RawSocketBufferSize
        {
            get { return Math.Max(1024 << 10, _rawSocketBufferSize << 10); }
        }
        /// <summary>
        /// 服务器端套接字单次最大发送数据量(单位:KB)
        /// </summary>
        private int _maxServerSocketSendSize = 8;
        /// <summary>
        /// 服务器端套接字单次最大发送数据量(单位:B)
        /// </summary>
        public int MaxServerSocketSendSize
        {
            get { return Math.Max(4 << 10, _maxServerSocketSendSize << 10); }
        }
        /// <summary>
        /// 成员位图内存池大小(单位:KB)
        /// </summary>
        private int _memberMapPoolSize = 8;
        /// <summary>
        /// 成员位图内存池大小(单位:B)
        /// </summary>
        public int MemberMapPoolSize
        {
            get { return Math.Max(4 << 10, _memberMapPoolSize << 10); }
        }
        /// <summary>
        /// 成员位图内存池支持最大成员数量
        /// </summary>
        private int _maxMemberMapCount = 1024;
        /// <summary>
        /// 成员位图内存池支持最大成员数量
        /// </summary>
        public int MaxMemberMapCount
        {
            get { return (Math.Max(1024, _maxMemberMapCount) + 63) & 0x7fffffc0; }
        }
        /// <summary>
        /// 基本配置
        /// </summary>
        private PubPlus()
        {
            LoadConfig(this);
            if (WorkPath == null) WorkPath = CoreLibrary.PubPlus.ApplicationPath;
            else WorkPath = WorkPath.PathSuffix().ToLower();
            if (CachePath == null || !DirectoyPlus.Create(CachePath = CachePath.PathSuffix().ToLower())) CachePath = CoreLibrary.PubPlus.ApplicationPath;
        }
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="type">配置类型</param>
        /// <returns>配置</returns>
        private json.node GetConfig(Type type)
        {
            if (Configs.Type != json.node.nodeType.Null)
            {
                string name = type.FullName, fastCSharpName = CoreLibrary.PubPlus.TmphCoreLib + ".";
                if (name.StartsWith(fastCSharpName, StringComparison.Ordinal))
                {
                    string fastCSharpConfig = fastCSharpName + "config.";
                    name = name.Substring(name.StartsWith(fastCSharpConfig, StringComparison.Ordinal) ? fastCSharpConfig.Length : fastCSharpName.Length);
                }
                json.node config = Configs;
                foreach (string tagName in name.Split('.'))
                {
                    if (config.Type != json.node.nodeType.Dictionary || (config = config[tagName]).Type == json.node.nodeType.Null)
                    {
                        return default(json.node);
                    }
                }
                return config;
            }
            return default(json.node);
        }
        /// <summary>
        /// 配置加载
        /// </summary>
        /// <param name="value">配置对象</param>
        /// <param name="name">配置名称,null表示只匹配类型</param>
        public TValueType LoadConfig<TValueType>(string name = null) where TValueType : struct
        {
            var value = default(TValueType);
            return LoadConfigBase(ref value, name);
        }
        /// <summary>
        /// fastCSharp命名空间
        /// </summary>
        private const string TmphMisName = CoreLibrary.PubPlus.TmphCoreLib + ".";
        /// <summary>
        /// fastCSharp配置命名空间
        /// </summary>
        private const string TmphMisConfigName = TmphMisName + "config.";
        /// <summary>
        /// 配置JSON解析参数
        /// </summary>
        private static readonly JsonParserPlus.ConfigPlus JsonConfig = new JsonParserPlus.ConfigPlus { MemberFilter = MemberFiltersEnum.Instance };
        /// <summary>
        /// 配置加载
        /// </summary>
        /// <param name="value">配置对象</param>
        /// <param name="name">配置名称,null表示只匹配类型</param>
        public static TValueType LoadConfig<TValueType>(TValueType value, string name = null) where TValueType : class
        {
            if (value == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            LoadConfigBase(ref value, name);
            return value;
        }
        /// <summary>
        /// 配置加载
        /// </summary>
        /// <param name="value">配置对象</param>
        /// <param name="name">配置名称,null表示只匹配类型</param>
        public static void LoadConfig<TValueType>(ref TValueType value, string name = null) where TValueType : struct
        {
            LoadConfigBase(ref value, name);
        }
        /// <summary>
        /// 配置加载
        /// </summary>
        /// <param name="value">配置对象</param>
        /// <param name="name">配置名称,null表示只匹配类型</param>
        private static void LoadConfigBase<TValueType>(ref TValueType value, string name)
        {
            if (name == null) name = value.GetType().FullName.ReplaceNotNull('+', '.');
            else name = value.GetType().FullName.ReplaceNotNull('+', '.') + "." + name;
            if (name.StartsWith(TmphMisName, StringComparison.Ordinal))
            {
                name = name.Substring(name.StartsWith(TmphMisConfigName, StringComparison.Ordinal) ? TmphMisConfigName.Length : TmphMisName.Length);
            }
            SubStringStruct json;
            if (Configs.TryGetValue(name, out json)) JsonParserPlus.Parse(json, ref value, JsonConfig);
        }
        /// <summary>
        /// 判断配置名称是否存在
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>配置是否加载存在</returns>
        public static bool IsConfigName(string name)
        {
            return Configs.ContainsKey(name);
        }
        /// <summary>
        /// 配置文件加载
        /// </summary>
        private struct LoaderStruct
        {
            /// <summary>
            /// 历史配置文件
            /// </summary>
            private list<string> _files;
            /// <summary>
            /// 错误信息集合
            /// </summary>
            private list<string> _errors;
            /// <summary>
            /// 加载配置文件
            /// </summary>
            /// <param name="configFile">配置文件名称</param>
            public void Load(string configFile)
            {
                if (_files == null)
                {
                    _files = new list<string>();
                    _errors = new list<string>();
                }
                try
                {
                    Load(new FileInfo(configFile));
                }
                catch (Exception error)
                {
                    LogPlus.Error.Real(error, "配置文件加载失败 : " + configFile, false);
                }
            }
            /// <summary>
            /// 加载配置文件
            /// </summary>
            /// <param name="file">配置文件</param>
            private unsafe void Load(FileInfo file)
            {
                if (file.Exists)
                {
                    var fileName = file.FullName.ToLower();
                    int count = _files.Count;
                    if (count != 0)
                    {
                        foreach (string name in _files.array)
                        {
                            if (_errors.Count == 0)
                            {
                                if (name == fileName)
                                {
                                    _errors.Add("配置文件循环嵌套");
                                    _errors.Add(name);
                                }
                            }
                            else _errors.Add(name);
                            if (--count == 0) break;
                        }
                        if (_errors.Count != 0)
                        {
                            LogPlus.Error.Real(_errors.joinString(@"
"), false);
                            _errors.Empty();
                        }
                    }
                    var config = File.ReadAllText(fileName, AppSettingPlus.Encoding);
                    fixed (char* configFixed = config)
                    {
                        for (char* current = configFixed, end = configFixed + config.Length; current != end; )
                        {
                            var start = current;
                            while (*current != '=' && ++current != end)
                            {
                            }
                            if (current == end) break;
                            var name = SubStringStruct.Unsafe(config, (int)(start - configFixed), (int)(current - start));
                            if (name.Equals(AppSettingPlus.ConfigIncludeName))
                            {
                                for (start = ++current; current != end && *current != '\n'; ++current)
                                {
                                }
                                if (file.DirectoryName != null)
                                    Load(Path.Combine(file.DirectoryName, config.Substring((int)(start - configFixed), (int)(current - start)).Trim()));
                                if (current == end) break;
                                ++current;
                            }
                            else
                            {
                                for (start = ++current; current != end; ++current)
                                {
                                    if (*current == '\n')
                                    {
                                        while (++current != end && *current == '\n')
                                        {
                                        }
                                        if (current == end) break;
                                        if (*current != '\t' && *current != ' ') break;
                                    }
                                }
                                HashStringStruct nameKey = name;
                                if (Configs.ContainsKey(nameKey))
                                {
                                    LogPlus.Error.Real("重复的配置名称 : " + name);
                                }
                                else Configs.Add(nameKey, SubStringStruct.Unsafe(config, (int)(start - configFixed), (int)(current - start)));
                            }
                        }
                    }
                }
                else LogPlus.Default.Real("找不到配置文件 : " + file.FullName);
            }
        }
        /// <summary>
        /// 配置集合
        /// </summary>
        private static readonly Dictionary<HashStringStruct, SubStringStruct> Configs;
        /// <summary>
        /// 默认基本配置
        /// </summary>
        public static readonly PubPlus Default;
        static PubPlus()
        {
            Configs = DictionaryPlus.CreateHashString<SubStringStruct>();
            new LoaderStruct().Load(AppSettingPlus.ConfigFile);
            Default = new PubPlus();
        }
    }
}
