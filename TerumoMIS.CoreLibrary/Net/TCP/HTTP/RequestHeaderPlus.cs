//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: RequestHeaderPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  RequestHeaderPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:37:08
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
    /// Http请求头部
    /// </summary>
    public sealed class RequestHeaderPlus
    {
        /// <summary>
        /// fastCSharp爬虫标识
        /// </summary>
        public const string fastCSharpSpiderUserAgent = "fastCSharp spider";
        /// <summary>
        /// 最大数据分隔符长度
        /// </summary>
        private const int maxBoundaryLength = 128;
        /// <summary>
        /// 提交数据类型
        /// </summary>
        public enum postType : byte
        {
            /// <summary>
            /// 
            /// </summary>
            None,
            /// <summary>
            /// JSON数据
            /// </summary>
            Json,
            /// <summary>
            /// 表单
            /// </summary>
            Form,
            /// <summary>
            /// 表单数据
            /// </summary>
            FormData,
        }
        /// <summary>
        /// HTTP头名称唯一哈希
        /// </summary>
        private struct headerName : IEquatable<headerName>
        {
            //string[] keys = new string[] { "host", "content-length", "accept-encoding", "connection", "content-type", "cookie", "referer", "range", "user-agent", "if-modified-since", "x-prowarded-for", "if-none-match", "expect", "upgrade", "origin", "sec-webSocket-key", "sec-webSocket-origin" };
            /// <summary>
            /// HTTP头名称
            /// </summary>
            public subArray<byte> Name;
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="name">HTTP头名称</param>
            /// <returns>HTTP头名称唯一哈希</returns>
            public static implicit operator headerName(byte[] name) { return new headerName { Name = subArray<byte>.Unsafe(name, 0, name.Length) }; }
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="name">HTTP头名称</param>
            /// <returns>HTTP头名称唯一哈希</returns>
            public static implicit operator headerName(subArray<byte> name) { return new headerName { Name = name }; }
            /// <summary>
            /// 获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public unsafe override int GetHashCode()
            {
                fixed (byte* nameFixed = Name.Array)
                {
                    byte* start = nameFixed + Name.StartIndex;
                    return (((*(start + (Name.Count >> 1)) | 0x20) >> 2) ^ (*(start + Name.Count - 3) << 1)) & ((1 << 5) - 1);
                }
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public unsafe bool Equals(headerName other)
            {
                if (Name.Count == other.Name.Count)
                {
                    fixed (byte* nameFixed = Name.Array, otherNameFixed = other.Name.Array)
                    {
                        return fastCSharp.unsafer.memory.EqualCase(nameFixed + Name.StartIndex, otherNameFixed + other.Name.StartIndex, Name.Count);
                    }
                }
                return false;
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((headerName)obj);
            }
        }
        /// <summary>
        /// 搜索引擎标识trie图
        /// </summary>
        private sealed class trieGraph
        {
            /// <summary>
            /// trie图节点
            /// </summary>
            private sealed class node
            {
                /// <summary>
                /// 失败节点
                /// </summary>
                public node Link;
                /// <summary>
                /// 关键字集合
                /// </summary>
                public subArray<byte> Keys;
                /// <summary>
                /// 节点集合
                /// </summary>
                public node[] Nodes;
                /// <summary>
                /// 获取节点
                /// </summary>
                /// <param name="key">关键字</param>
                /// <returns>节点</returns>
                public node this[byte key]
                {
                    get { return Nodes[KeyToIndex(key)]; }
                }
                /// <summary>
                /// 节点值
                /// </summary>
                public byte[] Value;
                /// <summary>
                /// 创建节点集合
                /// </summary>
                public void Create()
                {
                    Keys = new subArray<byte>(27);
                    Nodes = new node[27];
                }
                /// <summary>
                /// 创建子节点
                /// </summary>
                /// <param name="letter">当前字符</param>
                /// <returns>子节点</returns>
                public node Create(byte letter)
                {
                    if (Nodes == null)
                    {
                        Create();
                        node nextNode = new node();
                        Nodes[KeyToIndex(letter)] = nextNode;
                        Keys.UnsafeAdd(letter);
                        return nextNode;
                    }
                    else
                    {
                        int index = KeyToIndex(letter);
                        node nextNode = Nodes[index];
                        if (nextNode == null)
                        {
                            Nodes[index] = nextNode = new node();
                            Keys.UnsafeAdd(letter);
                        }
                        return nextNode;
                    }
                }
                /// <summary>
                /// 关键字转节点索引
                /// </summary>
                /// <param name="key">关键字</param>
                /// <returns>节点索引</returns>
                public static int KeyToIndex(byte key)
                {
                    key |= 0x20;
                    key -= (byte)'a';
                    return key < 26 ? key : 26;
                }
            }
            /// <summary>
            /// 树创建器
            /// </summary>
            private unsafe struct treeBuilder
            {
                /// <summary>
                /// 当前节点
                /// </summary>
                public node Node;
                /// <summary>
                /// 结束字符
                /// </summary>
                private byte* end;
                /// <summary>
                /// 创建树
                /// </summary>
                /// <param name="keys">字符数组</param>
                public void Build(byte[] keys)
                {
                    fixed (byte* start = keys)
                    {
                        end = start + keys.Length;
                        build(start);
                    }
                    Node.Value = keys;
                }
                /// <summary>
                /// 创建树
                /// </summary>
                /// <param name="start">当前字符位置</param>
                private void build(byte* start)
                {
                    Node = Node.Create(*start);
                    if (++start != end) build(start);
                }
            }
            /// <summary>
            /// 图创建器
            /// </summary>
            private struct graphBuilder
            {
                /// <summary>
                /// 根节点
                /// </summary>
                public node Boot;
                /// <summary>
                /// 当前处理结果节点集合
                /// </summary>
                public list<node> Writer;
                /// <summary>
                /// 设置根节点
                /// </summary>
                /// <param name="node">根节点</param>
                public void Set(node node)
                {
                    Boot = node;
                    Writer = new list<node>();
                }
                /// <summary>
                /// 建图
                /// </summary>
                /// <param name="reader">处理节点集合</param>
                /// <param name="count">处理节点数量</param>
                public unsafe void Build(node[] reader, int count)
                {
                    Writer.Empty();
                    foreach (node father in reader)
                    {
                        node[] nodes = father.Nodes;
                        fixed (byte* keyFixed = father.Keys.array)
                        {
                            byte* keyEnd = keyFixed + father.Keys.Count;
                            if (father.Link == null)
                            {
                                for (byte* keyStart = keyFixed; keyStart != keyEnd; ++keyStart)
                                {
                                    int index = trieGraph.node.KeyToIndex(*keyStart);
                                    node node = nodes[index];
                                    node.Link = Boot.Nodes[index];
                                    if (node.Nodes != null) Writer.Add(node);
                                }
                            }
                            else
                            {
                                for (byte* keyStart = keyFixed; keyStart != keyEnd; ++keyStart)
                                {
                                    int index = trieGraph.node.KeyToIndex(*keyStart);
                                    node node = nodes[index], link = father.Link;
                                    while ((link.Nodes == null || (node.Link = link.Nodes[index]) == null) && (link = link.Link) != null) ;
                                    if (node.Link == null) node.Link = Boot.Nodes[index];
                                    if (node.Nodes != null) Writer.Add(node);
                                }
                            }
                        }
                        if (--count == 0) break;
                    }
                }
            }
            /// <summary>
            /// 根节点
            /// </summary>
            private node boot;
            /// <summary>
            /// 字节数组trie图
            /// </summary>
            public trieGraph()
            {
                (boot = new node()).Create();
            }
            /// <summary>
            /// 创建trie树
            /// </summary>
            /// <param name="keys">关键字集合</param>
            public void BuildTree(byte[][] keys)
            {
                treeBuilder treeBuilder = new treeBuilder();
                foreach (byte[] key in keys)
                {
                    treeBuilder.Node = boot;
                    treeBuilder.Build(key);
                }
            }
            /// <summary>
            /// 建图
            /// </summary>
            public void BuildGraph()
            {
                graphBuilder builder = new graphBuilder();
                builder.Set(boot);
                list<node> reader = boot.Nodes.getFindList(node => node != null && node.Nodes != null);
                if (reader != null)
                {
                    while (reader.Count != 0)
                    {
                        builder.Build(reader.array, reader.Count);
                        list<node> values = reader;
                        reader = builder.Writer;
                        builder.Writer = values;
                    }
                }
            }
            /// <summary>
            /// 是否存在最小匹配
            /// </summary>
            /// <param name="data">匹配数据</param>
            /// <param name="startIndex">匹配起始位置</param>
            /// <param name="length">匹配数据长度</param>
            /// <returns>是否存在最小匹配</returns>
            public unsafe bool IsMatchLess(byte[] data, int startIndex, int length)
            {
                fixed (byte* valueFixed = data)
                {
                    byte* start = valueFixed + startIndex;
                    return isMatchLess(start, start + length);
                }
            }
            /// <summary>
            /// 是否存在最小匹配
            /// </summary>
            /// <param name="start">匹配起始位置</param>
            /// <param name="end">匹配结束位置</param>
            /// <returns>是否存在最小匹配</returns>
            private unsafe bool isMatchLess(byte* start, byte* end)
            {
                for (node node = boot, nextNode = null; start != end; ++start)
                {
                    byte letter = *start;
                    if (node.Nodes == null || (nextNode = node[letter]) == null)
                    {
                        do
                        {
                            if ((node = node.Link) == null) break;
                            if (node.Value != null) return true;
                        }
                        while (node.Nodes == null || (nextNode = node[letter]) == null);
                        if (node == null && (nextNode = boot[letter]) == null) nextNode = boot;
                    }
                    if (nextNode.Value != null) return true;
                    node = nextNode;
                }
                return false;
            }
        }
        /// <summary>
        /// 查询解析器
        /// </summary>
        private unsafe sealed class queryParser
        {
            /// <summary>
            /// 解析状态
            /// </summary>
            public enum parseState : byte
            {
                /// <summary>
                /// 成功
                /// </summary>
                Success,
                /// <summary>
                /// 逻辑值解析错误
                /// </summary>
                NotBool,
                /// <summary>
                /// 非数字解析错误
                /// </summary>
                NotNumber,
                /// <summary>
                /// 16进制数字解析错误
                /// </summary>
                NotHex,
                /// <summary>
                /// 时间解析错误
                /// </summary>
                NotDateTime,
                /// <summary>
                /// Guid解析错误
                /// </summary>
                NotGuid,
                /// <summary>
                /// 未知类型解析错误
                /// </summary>
                Unknown,
            }
            /// <summary>
            /// 解析类型
            /// </summary>
            private sealed class parseType : Attribute { }
            /// <summary>
            /// 名称状态查找器
            /// </summary>
            internal struct stateSearcher
            {
                /// <summary>
                /// 查询解析器
                /// </summary>
                private queryParser parser;
                /// <summary>
                /// 状态集合
                /// </summary>
                private byte* state;
                /// <summary>
                /// ASCII字符查找表
                /// </summary>
                private byte* charsAscii;
                /// <summary>
                /// 特殊字符串查找表
                /// </summary>
                private byte* charStart;
                /// <summary>
                /// 特殊字符串查找表结束位置
                /// </summary>
                private byte* charEnd;
                /// <summary>
                /// 当前状态
                /// </summary>
                private byte* currentState;
                /// <summary>
                /// 特殊字符起始值
                /// </summary>
                private int charIndex;
                /// <summary>
                /// 查询矩阵单位尺寸类型
                /// </summary>
                private byte tableType;
                /// <summary>
                /// 名称查找器
                /// </summary>
                /// <param name="parser">查询解析器</param>
                /// <param name="data">数据起始位置</param>
                internal stateSearcher(queryParser parser, pointer data)
                {
                    this.parser = parser;
                    if (data.Data == null)
                    {
                        state = charsAscii = charStart = charEnd = currentState = null;
                        charIndex = 0;
                        tableType = 0;
                    }
                    else
                    {
                        int stateCount = *data.Int;
                        currentState = state = data.Byte + sizeof(int);
                        charsAscii = state + stateCount * 3 * sizeof(int);
                        charStart = charsAscii + 128 * sizeof(ushort);
                        charIndex = *(ushort*)charStart;
                        charStart += sizeof(ushort) * 2;
                        charEnd = charStart + *(ushort*)(charStart - sizeof(ushort)) * sizeof(ushort);
                        if (stateCount < 256) tableType = 0;
                        else if (stateCount < 65536) tableType = 1;
                        else tableType = 2;
                    }
                }
                /// <summary>
                /// 获取名称索引
                /// </summary>
                /// <returns>名称索引,失败返回-1</returns>
                internal int SearchName()
                {
                    if (state == null) return -1;
                    byte value = parser.getName();
                    if (value == 0) return *(int*)(currentState + sizeof(int) * 2);
                    currentState = state;
                    do
                    {
                        char* prefix = (char*)(currentState + *(int*)currentState);
                        if (*prefix != 0)
                        {
                            if (value != *prefix) return -1;
                            while (*++prefix != 0)
                            {
                                if (parser.getName() != *prefix) return -1;
                            }
                            value = parser.getName();
                        }
                        if (value == 0) return *(int*)(currentState + sizeof(int) * 2);
                        if (*(int*)(currentState + sizeof(int)) == 0 || value >= 128) return -1;
                        int index = (int)*(ushort*)(charsAscii + (value << 1));
                        byte* table = currentState + *(int*)(currentState + sizeof(int));
                        if (tableType == 0)
                        {
                            if ((index = *(table + index)) == 0) return -1;
                            currentState = state + index * 3 * sizeof(int);
                        }
                        else if (tableType == 1)
                        {
                            if ((index = (int)*(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                            currentState = state + index * 3 * sizeof(int);
                        }
                        else
                        {
                            if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                            currentState = state + index;
                        }
                        value = parser.getName();
                    }
                    while (true);
                }
            }
            /// <summary>
            /// 类型解析器
            /// </summary>
            /// <typeparam name="valueType">目标类型</typeparam>
            internal static class parser<valueType>
            {
                /// <summary>
                /// 解析委托
                /// </summary>
                /// <param name="parser">查询解析器</param>
                /// <param name="value">目标数据</param>
                internal delegate void tryParse(queryParser parser, ref valueType value);
                /// <summary>
                /// 成员解析器集合
                /// </summary>
                private static readonly tryParse[] memberParsers;
                /// <summary>
                /// 成员名称查找数据
                /// </summary>
                private static readonly pointer memberSearcher;
                /// <summary>
                /// 对象解析
                /// </summary>
                /// <param name="parser">查询解析器</param>
                /// <param name="value">目标数据</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static void Parse(queryParser parser, ref valueType value)
                {
                    stateSearcher searcher = new stateSearcher(parser, memberSearcher);
                    while (parser.isQuery())
                    {
                        int index = searcher.SearchName();
                        if (index != -1) memberParsers[index](parser, ref value);
                    }
                }
                static parser()
                {
                    Type type = typeof(valueType);
                    fastCSharp.emit.jsonParse attribute = fastCSharp.code.typeAttribute.GetAttribute<fastCSharp.emit.jsonParse>(type, true, true) ?? fastCSharp.emit.jsonParse.AllMember;
                    fastCSharp.code.fieldIndex defaultMember = null;
                    subArray<fastCSharp.code.fieldIndex> fields = fastCSharp.emit.jsonParser.typeParser.GetFields(fastCSharp.code.memberIndexGroup<valueType>.GetFields(attribute.MemberFilter), attribute, ref defaultMember);
                    subArray<keyValue<fastCSharp.code.propertyIndex, MethodInfo>> properties = fastCSharp.emit.jsonParser.typeParser.GetProperties(fastCSharp.code.memberIndexGroup<valueType>.GetProperties(attribute.MemberFilter), attribute);
                    memberParsers = new tryParse[fields.Count + properties.Count + (defaultMember == null ? 0 : 1)];
                    string[] names = new string[memberParsers.Length];
                    int index = 0;
                    foreach (fastCSharp.code.fieldIndex member in fields)
                    {
                        ILGenerator generator;
                        DynamicMethod dynamicMethod = createDynamicMethod(type, member.Member.Name, member.Member.FieldType, out generator);
                        generator.Emit(OpCodes.Stfld, member.Member);
                        generator.Emit(OpCodes.Ret);
                        tryParse tryParse = (tryParse)dynamicMethod.CreateDelegate(typeof(tryParse));
                        memberParsers[index] = tryParse;
                        names[index++] = member.Member.Name;
                        if (member == defaultMember)
                        {
                            memberParsers[index] = tryParse;
                            names[index++] = string.Empty;
                        }
                    }
                    foreach (keyValue<fastCSharp.code.propertyIndex, MethodInfo> member in properties)
                    {
                        ILGenerator generator;
                        DynamicMethod dynamicMethod = createDynamicMethod(type, member.Key.Member.Name, member.Key.Member.PropertyType, out generator);
                        generator.Emit(member.Value.IsFinal || !member.Value.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, member.Value);
                        generator.Emit(OpCodes.Ret);
                        memberParsers[index] = (tryParse)dynamicMethod.CreateDelegate(typeof(tryParse));
                        names[index++] = member.Key.Member.Name;
                    }
                    if (type.IsGenericType) memberSearcher = fastCSharp.emit.jsonParser.stateSearcher.GetGenericDefinitionMember(type, names);
                    else memberSearcher = fastCSharp.stateSearcher.chars.Create(names);
                }
            }
            /// <summary>
            /// 创建解析委托函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="name">成员名称</param>
            /// <param name="memberType">成员类型</param>
            /// <param name="generator"></param>
            /// <returns>解析委托函数</returns>
            private static DynamicMethod createDynamicMethod(Type type, string name, Type memberType, out ILGenerator generator)
            {
                DynamicMethod dynamicMethod = new DynamicMethod("queryParser" + name, null, new Type[] { typeof(queryParser), type.MakeByRefType() }, type, true);
                generator = dynamicMethod.GetILGenerator();
                LocalBuilder loadMember = generator.DeclareLocal(memberType);
                generator.DeclareLocal(memberType);
                MethodInfo methodInfo = queryParser.getParseMethod(memberType);
                if (methodInfo == null)
                {
                    if (memberType.IsEnum) methodInfo = queryParser.parseEnumMethod.MakeGenericMethod(memberType);
                    else methodInfo = queryParser.unknownMethod.MakeGenericMethod(memberType);
                }
                if (!memberType.IsValueType)
                {
                    generator.Emit(OpCodes.Ldloca_S, loadMember);
                    generator.Emit(OpCodes.Initobj, memberType);
                }
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldloca_S, loadMember);
                generator.Emit(methodInfo.IsFinal || !methodInfo.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, methodInfo);

                generator.Emit(OpCodes.Ldarg_1);
                if (!type.IsValueType) generator.Emit(OpCodes.Ldind_Ref);
                generator.Emit(OpCodes.Ldloc_0);
                return dynamicMethod;
            }
            /// <summary>
            /// 解析状态
            /// </summary>
            private parseState state;
            /// <summary>
            /// HTTP请求头部
            /// </summary>
            private requestHeader requestHeader;
            /// <summary>
            /// 缓冲区起始位置
            /// </summary>
            private byte* bufferFixed;
            /// <summary>
            /// 当前解析位置
            /// </summary>
            private byte* current;
            /// <summary>
            /// 解析结束位置
            /// </summary>
            private byte* end;
            /// <summary>
            /// 当前处理位置
            /// </summary>
            private int queryIndex;
            /// <summary>
            /// 查询解析器
            /// </summary>
            private queryParser() { }
            /// <summary>
            /// 查询解析
            /// </summary>
            /// <typeparam name="valueType">目标类型</typeparam>
            /// <param name="requestHeader">HTTP请求头部</param>
            /// <param name="value">目标数据</param>
            /// <returns>解析状态</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private parseState parse<valueType>(requestHeader requestHeader, ref valueType value)
            {
                this.requestHeader = requestHeader;
                state = parseState.Success;
                fixed (byte* bufferFixed = requestHeader.Buffer)
                {
                    this.bufferFixed = bufferFixed;
                    queryIndex = -1;
                    parser<valueType>.Parse(this, ref value);
                }
                return state;
            }
            /// <summary>
            /// 释放查询解析器
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void free()
            {
                requestHeader = null;
                typePool<queryParser>.Push(this);
            }
            /// <summary>
            /// 解析10进制数字
            /// </summary>
            /// <param name="value">第一位数字</param>
            /// <returns>数字</returns>
            private uint parseUInt32(uint value)
            {
                uint number;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9) return value;
                    value *= 10;
                    value += number;
                    if (++current == end) return value;
                }
                while (true);
            }
            /// <summary>
            /// 解析16进制数字
            /// </summary>
            /// <param name="value">数值</param>
            private void parseHex32(ref uint value)
            {
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                    {
                        state = parseState.NotHex;
                        return;
                    }
                    number += 10;
                }
                value = number;
                if (++current == end) return;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return;
                        number += 10;
                    }
                    value <<= 4;
                    value += number;
                }
                while (++current != end);
            }
            /// <summary>
            /// 逻辑值解析
            /// </summary>
            /// <param name="value">数据</param>
            /// <returns>解析状态</returns>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref bool value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 5)
                {
                    current = bufferFixed + indexs.StartIndex;
                    if ((*current | 0x20) == 'f' && *(int*)(current + 1) == ('a' + ('l' << 8) + ('s' << 16) + ('e' << 24))) value = false;
                    else state = parseState.NotBool;
                }
                else if (indexs.Length == 4)
                {
                    current = bufferFixed + indexs.StartIndex;
                    if (*(int*)current == ('t' + ('r' << 8) + ('u' << 16) + ('e' << 24))) value = true;
                    else state = parseState.NotBool;
                }
                else if (value = indexs.Length == 0) value = false;
                else
                {
                    byte byteValue = (byte)(*(bufferFixed + indexs.StartIndex) - '0');
                    if (byteValue < 10) value = byteValue != 0;
                    else state = parseState.NotBool;
                }
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref byte value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = parseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = (byte)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = (byte)number;
                    return;
                }
                value = (byte)parseUInt32(number);
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref sbyte value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                int sign = 0;
                if (*current == '-')
                {
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = parseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                    return;
                }
                value = sign == 0 ? (sbyte)(byte)parseUInt32(number) : (sbyte)-(int)parseUInt32(number);
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref ushort value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = parseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = (ushort)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = (ushort)number;
                    return;
                }
                value = (ushort)parseUInt32(number);
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref short value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                int sign = 0;
                if (*current == '-')
                {
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = parseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                    return;
                }
                value = sign == 0 ? (short)(ushort)parseUInt32(number) : (short)-(int)parseUInt32(number);
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref uint value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = parseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = number;
                    return;
                }
                value = parseUInt32(number);
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref int value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                int sign = 0;
                if (*current == '-')
                {
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = parseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = sign == 0 ? (int)number : -(int)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    parseHex32(ref number);
                    value = sign == 0 ? (int)number : -(int)number;
                    return;
                }
                value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
            }
            /// <summary>
            /// 解析10进制数字
            /// </summary>
            /// <param name="value">第一位数字</param>
            /// <returns>数字</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ulong parseUInt64(uint value)
            {
                byte* end32 = current + 8;
                if (end32 > end) end32 = end;
                uint number;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9) return value;
                    value *= 10;
                    value += number;
                }
                while (++current != end32);
                if (current == end) return value;
                ulong value64 = value;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9) return value64;
                    value64 *= 10;
                    value64 += number;
                    if (++current == end) return value64;
                }
                while (true);
            }
            /// <summary>
            /// 解析16进制数字
            /// </summary>
            /// <returns>数字</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ulong parseHex64()
            {
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                    {
                        state = parseState.NotHex;
                        return 0;
                    }
                    number += 10;
                }
                if (++current == end) return number;
                uint high = number;
                byte* end32 = current + 7;
                if (end32 > end) end32 = end;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return high;
                        number += 10;
                    }
                    high <<= 4;
                    high += number;
                }
                while (++current != end32);
                if (current == end) return high;
                byte* start = current;
                ulong low = number;
                do
                {
                    if ((number = (uint)(*current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                        {
                            return low | (ulong)high << ((int)((byte*)current - (byte*)start) << 1);
                        }
                        number += 10;
                    }
                    low <<= 4;
                    low += number;
                }
                while (++current != end);
                return low | (ulong)high << ((int)((byte*)current - (byte*)start) << 1);
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref ulong value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = parseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    value = parseHex64();
                    return;
                }
                value = parseUInt64(number);
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref long value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0)
                {
                    value = 0;
                    return;
                }
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                int sign = 0;
                if (*current == '-')
                {
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                uint number = (uint)(*current - '0');
                if (number > 9)
                {
                    state = parseState.NotNumber;
                    return;
                }
                if (++current == end)
                {
                    value = sign == 0 ? (long)(int)number : -(long)(int)number;
                    return;
                }
                if (number == 0)
                {
                    if (*current != 'x')
                    {
                        value = 0;
                        return;
                    }
                    if (++current == end)
                    {
                        state = parseState.NotNumber;
                        return;
                    }
                    value = (long)parseHex64();
                    if (sign != 0) value = -value;
                    return;
                }
                value = (long)parseUInt64(number);
                if (sign != 0) value = -value;
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref float value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = 0;
                else
                {
                    string number = formQuery.JavascriptUnescape(subArray<byte>.Unsafe(requestHeader.Buffer, indexs.StartIndex, indexs.Length));
                    if (!float.TryParse(number, out value)) state = parseState.NotNumber;
                }
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref double value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = 0;
                else
                {
                    string number = formQuery.JavascriptUnescape(subArray<byte>.Unsafe(requestHeader.Buffer, indexs.StartIndex, indexs.Length));
                    if (!double.TryParse(number, out value)) state = parseState.NotNumber;
                }
            }
            /// <summary>
            /// 数字解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref decimal value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = 0;
                else
                {
                    string number = formQuery.JavascriptUnescape(subArray<byte>.Unsafe(requestHeader.Buffer, indexs.StartIndex, indexs.Length));
                    if (!decimal.TryParse(number, out value)) state = parseState.NotNumber;
                }
            }
            /// <summary>
            /// 时间解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref DateTime value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = DateTime.MinValue;
                else
                {
                    string dateTime = formQuery.JavascriptUnescape(subArray<byte>.Unsafe(requestHeader.Buffer, indexs.StartIndex, indexs.Length));
                    if (!DateTime.TryParse(dateTime, out value)) state = parseState.NotDateTime;
                }
            }
            /// <summary>
            /// 解析16进制字符
            /// </summary>
            /// <returns>字符</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint parseHex4()
            {
                uint code = (uint)(*++current - '0'), number = (uint)(*++current - '0');
                if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                if (number > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
                code <<= 12;
                code += (number << 8);
                if ((number = (uint)(*++current - '0')) > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
                code += (number << 4);
                number = (uint)(*++current - '0');
                return code + (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number);
            }
            /// <summary>
            /// 解析16进制字符
            /// </summary>
            /// <returns>字符</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint parseHex2()
            {
                uint code = (uint)(*++current - '0'), number = (uint)(*++current - '0');
                if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                return (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number) + (code << 4);
            }
            /// <summary>
            /// Guid解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref Guid value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = new Guid();
                else if (end - current != 36) state = parseState.NotGuid;
                else
                {
                    current = bufferFixed + indexs.StartIndex;
                    end = current + indexs.Length;
                    guid guid = new guid();
                    guid.Byte3 = (byte)parseHex2();
                    guid.Byte2 = (byte)parseHex2();
                    guid.Byte1 = (byte)parseHex2();
                    guid.Byte0 = (byte)parseHex2();
                    if (*++current != '-')
                    {
                        state = parseState.NotGuid;
                        return;
                    }
                    guid.Byte45 = (ushort)parseHex4();
                    if (*++current != '-')
                    {
                        state = parseState.NotGuid;
                        return;
                    }
                    guid.Byte67 = (ushort)parseHex4();
                    if (*++current != '-')
                    {
                        state = parseState.NotGuid;
                        return;
                    }
                    guid.Byte8 = (byte)parseHex2();
                    guid.Byte9 = (byte)parseHex2();
                    if (*++current != '-')
                    {
                        state = parseState.NotGuid;
                        return;
                    }
                    guid.Byte10 = (byte)parseHex2();
                    guid.Byte11 = (byte)parseHex2();
                    guid.Byte12 = (byte)parseHex2();
                    guid.Byte13 = (byte)parseHex2();
                    guid.Byte14 = (byte)parseHex2();
                    guid.Byte15 = (byte)parseHex2();
                    value = guid.Value;
                }
            }
            /// <summary>
            /// 字符串解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref string value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = string.Empty;
                else value = formQuery.JavascriptUnescape(subArray<byte>.Unsafe(requestHeader.Buffer, indexs.StartIndex, indexs.Length));
            }
            /// <summary>
            /// 字符串解析
            /// </summary>
            /// <param name="value">数据</param>
            [parseType]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Parse(ref subString value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value.UnsafeSet(string.Empty, 0, 0);
                else value = formQuery.JavascriptUnescape(subArray<byte>.Unsafe(requestHeader.Buffer, indexs.StartIndex, indexs.Length));
            }
            /// <summary>
            /// 未知类型解析
            /// </summary>
            /// <param name="value">目标数据</param>
            private unsafe void parseEnum<valueType>(ref valueType value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = default(valueType);
                else
                {
                    subString json = formQuery.JavascriptUnescape(subArray<byte>.Unsafe(requestHeader.Buffer, indexs.StartIndex - 1, indexs.Length + 2));
                    fixed (char* jsonFixed = json.value) *jsonFixed = *(jsonFixed + json.Length - 1) = '"';
                    if (!fastCSharp.emit.jsonParser.Parse(json, ref value)) state = parseState.Unknown;
                }
            }
            /// <summary>
            /// 未知类型解析函数信息
            /// </summary>
            private static readonly MethodInfo parseEnumMethod = typeof(queryParser).GetMethod("parseEnum", BindingFlags.Instance | BindingFlags.NonPublic);
            /// <summary>
            /// 未知类型解析
            /// </summary>
            /// <param name="value">目标数据</param>
            private void unknown<valueType>(ref valueType value)
            {
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Value;
                if (indexs.Length == 0) value = default(valueType);
                else if (!fastCSharp.emit.jsonParser.Parse(formQuery.JavascriptUnescape(subArray<byte>.Unsafe(requestHeader.Buffer, indexs.StartIndex, indexs.Length)), ref value))
                {
                    state = parseState.Unknown;
                }
            }
            /// <summary>
            /// 未知类型解析函数信息
            /// </summary>
            private static readonly MethodInfo unknownMethod = typeof(queryParser).GetMethod("unknown", BindingFlags.Instance | BindingFlags.NonPublic);
            /// <summary>
            /// 是否存在未结束的查询
            /// </summary>
            /// <returns>是否存在未结束的查询</returns>
            private bool isQuery()
            {
                if (++queryIndex == requestHeader.queryIndexs.Count) return false;
                bufferIndex indexs = requestHeader.queryIndexs.array[queryIndex].Key;
                current = bufferFixed + indexs.StartIndex;
                end = current + indexs.Length;
                return true;
            }
            /// <summary>
            /// 获取当前名称字符
            /// </summary>
            /// <returns>当前名称字符,结束返回0</returns>
            private byte getName()
            {
                return current == end ? (byte)0 : *current++;
            }
            /// <summary>
            /// 查询解析
            /// </summary>
            /// <typeparam name="valueType">目标类型</typeparam>
            /// <param name="requestHeader">HTTP请求头部</param>
            /// <param name="value">目标数据</param>
            /// <returns>是否解析成功</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool Parse<valueType>(requestHeader requestHeader, ref valueType value)
            {
                if (requestHeader.queryIndexs.Count != 0)
                {
                    queryParser parser = typePool<queryParser>.Pop() ?? new queryParser();
                    try
                    {
                        return parser.parse<valueType>(requestHeader, ref value) == parseState.Success;
                    }
                    finally { parser.free(); }
                }
                return true;
            }
            /// <summary>
            /// 基本类型解析函数
            /// </summary>
            private static readonly Dictionary<Type, MethodInfo> parseMethods;
            /// <summary>
            /// 获取基本类型解析函数
            /// </summary>
            /// <param name="type">基本类型</param>
            /// <returns>解析函数</returns>
            private static MethodInfo getParseMethod(Type type)
            {
                MethodInfo method;
                return parseMethods.TryGetValue(type, out method) ? method : null;
            }
            static queryParser()
            {
                parseMethods = dictionary.CreateOnly<Type, MethodInfo>();
                foreach (MethodInfo method in typeof(queryParser).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (method.customAttribute<parseType>() != null)
                    {
                        parseMethods.Add(method.GetParameters()[0].ParameterType.GetElementType(), method);
                    }
                }
            }
        }
        /// <summary>
        /// Google请求#!查询名称
        /// </summary>
        private static readonly byte[] googleFragmentName = ("escaped_fragment_=").getBytes();
        /// <summary>
        /// HTTP头名称解析委托
        /// </summary>
        private static readonly uniqueDictionary<headerName, Action<requestHeader, bufferIndex>> parses;
        /// <summary>
        /// 搜索引擎标识trie图
        /// </summary>
        private static readonly trieGraph searchEngines;
        /// <summary>
        /// HTTP请求头部缓冲区
        /// </summary>
        internal byte[] Buffer;
        /// <summary>
        /// 结束位置
        /// </summary>
        internal int EndIndex;
        /// <summary>
        /// 请求URI
        /// </summary>
        private bufferIndex uri;
        /// <summary>
        /// 请求URI
        /// </summary>
        public subArray<byte> Uri
        {
            get { return subArray<byte>.Unsafe(Buffer, uri.StartIndex, uri.Length); }
        }
        /// <summary>
        /// 请求路径
        /// </summary>
        private bufferIndex path;
        /// <summary>
        /// 请求路径
        /// </summary>
        public subArray<byte> Path
        {
            get { return subArray<byte>.Unsafe(Buffer, path.StartIndex, path.Length); }
        }
        /// <summary>
        /// 请求路径是否需要做web视图路径转换
        /// </summary>
        public unsafe bool IsViewPath
        {
            get
            {
                fixed (byte* bufferFixed = Buffer)
                {
                    if (path.Length > 1)
                    {
                        byte* start = bufferFixed + path.StartIndex;
                        if (*start == '/')
                        {
                            start += path.Length;
                            do
                            {
                                if (*--start == '/') return true;
                                if (*start == '.') return false;
                            }
                            while (true);
                        }
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 是否小写路径
        /// </summary>
        private bool isLowerPath;
        /// <summary>
        /// 小写路径
        /// </summary>
        internal unsafe subArray<byte> LowerPath
        {
            get
            {
                if (!isLowerPath)
                {
                    fixed (byte* bufferFixed = Buffer)
                    {
                        byte* start = bufferFixed + path.StartIndex;
                        fastCSharp.unsafer.memory.ToLower(start, start + path.Length);
                    }
                    isLowerPath = true;
                }
                return subArray<byte>.Unsafe(Buffer, path.StartIndex, path.Length);
            }
        }
        /// <summary>
        /// 查询参数索引集合
        /// </summary>
        private list<keyValue<bufferIndex, bufferIndex>> queryIndexs = new list<keyValue<bufferIndex, bufferIndex>>(sizeof(int));
        /// <summary>
        /// 请求域名
        /// </summary>
        private bufferIndex host;
        /// <summary>
        /// 判断来源页是否合法
        /// </summary>
        private bool? isReferer;
        /// <summary>
        /// 判断来源页是否合法
        /// </summary>
        public bool IsReferer
        {
            get
            {
                if (isReferer == null)
                {
                    if (host.Length != 0)
                    {
                        subArray<byte> domain = default(subArray<byte>);
                        if (referer.Length != 0)
                        {
                            domain = fastCSharp.web.domain.GetMainDomainByUrl(Referer);
                        }
                        else if (origin.Length != 0)
                        {
                            domain = fastCSharp.web.domain.GetMainDomainByUrl(subArray<byte>.Unsafe(Buffer, origin.StartIndex, origin.Length));
                        }
                        if (domain.Array != null && domain.equal(fastCSharp.web.domain.GetMainDomain(Host))) isReferer = true;
                    }
                    if (isReferer == null) isReferer = false;
                }
                return (bool)isReferer;
            }
        }
        /// <summary>
        /// 请求域名
        /// </summary>
        public subArray<byte> Host
        {
            get { return subArray<byte>.Unsafe(Buffer, host.StartIndex, host.Length); }
        }
        /// <summary>
        /// 提交数据分隔符
        /// </summary>
        private bufferIndex boundary;
        /// <summary>
        /// 提交数据分隔符
        /// </summary>
        public subArray<byte> Boundary
        {
            get { return subArray<byte>.Unsafe(Buffer, boundary.StartIndex, boundary.Length); }
        }
        /// <summary>
        /// HTTP请求内容类型
        /// </summary>
        private bufferIndex contentType;
        /// <summary>
        /// Cookie
        /// </summary>
        private bufferIndex cookie;
        /// <summary>
        /// 访问来源
        /// </summary>
        private bufferIndex referer;
        /// <summary>
        /// 访问来源
        /// </summary>
        public subArray<byte> Referer
        {
            get { return subArray<byte>.Unsafe(Buffer, referer.StartIndex, referer.Length); }
        }
        /// <summary>
        /// 访问来源
        /// </summary>
        private bufferIndex origin;
        /// <summary>
        /// 请求范围起始位置
        /// </summary>
        internal long RangeStart { get; private set; }
        /// <summary>
        /// 请求范围结束位置
        /// </summary>
        internal long RangeEnd { get; private set; }
        /// <summary>
        /// 请求范围长度
        /// </summary>
        internal long RangeLength
        {
            get { return RangeEnd - RangeStart + 1; }
        }
        /// <summary>
        /// 格式化请求范围
        /// </summary>
        /// <param name="contentLength">内容字节长度</param>
        /// <returns>范围是否有效</returns>
        public bool FormatRange(long contentLength)
        {
            IsFormatRange = true;
            if (RangeStart == 0)
            {
                if (RangeEnd >= contentLength - 1 || RangeEnd < 0) RangeStart = RangeEnd = long.MinValue;
            }
            else if (RangeStart > 0)
            {
                if (RangeStart >= contentLength || (ulong)RangeEnd < (ulong)RangeStart) return false;
                if (RangeEnd >= contentLength || RangeEnd < 0) RangeEnd = contentLength - 1;
            }
            else if (RangeEnd >= 0)
            {
                if (RangeEnd < contentLength) RangeStart = 0;
                else RangeEnd = long.MinValue;
            }
            return true;
        }
        /// <summary>
        /// 是否已经格式化请求范围
        /// </summary>
        internal bool IsFormatRange;
        /// <summary>
        /// 是否存在请求范围
        /// </summary>
        public bool IsRange
        {
            get { return RangeStart >= 0 || RangeEnd >= 0; }
        }
        /// <summary>
        /// 请求范围是否错误
        /// </summary>
        internal bool IsRangeError
        {
            get { return RangeStart > RangeEnd && RangeEnd != long.MinValue; }
        }
        /// <summary>
        /// 浏览器参数
        /// </summary>
        private bufferIndex userAgent;
        /// <summary>
        /// 浏览器参数
        /// </summary>
        public subArray<byte> UserAgent
        {
            get { return subArray<byte>.Unsafe(Buffer, userAgent.StartIndex, userAgent.Length); }
        }
        /// <summary>
        /// 客户端文档时间标识
        /// </summary>
        private bufferIndex ifModifiedSince;
        /// <summary>
        /// 客户端文档时间标识
        /// </summary>
        public subArray<byte> IfModifiedSince
        {
            get { return subArray<byte>.Unsafe(Buffer, ifModifiedSince.StartIndex, ifModifiedSince.Length); }
        }
        /// <summary>
        /// 客户端缓存有效标识
        /// </summary>
        private bufferIndex ifNoneMatch;
        /// <summary>
        /// 客户端缓存有效标识
        /// </summary>
        public subArray<byte> IfNoneMatch
        {
            get { return subArray<byte>.Unsafe(Buffer, ifNoneMatch.StartIndex, ifNoneMatch.Length); }
        }
        /// <summary>
        /// 转发信息
        /// </summary>
        private bufferIndex xProwardedFor;
        /// <summary>
        /// AJAX调用函数名称
        /// </summary>
        private bufferIndex ajaxCallName;
        /// <summary>
        /// AJAX调用函数名称
        /// </summary>
        internal subArray<byte> AjaxCallName
        {
            get { return subArray<byte>.Unsafe(Buffer, ajaxCallName.StartIndex, ajaxCallName.Length); }
        }
        /// <summary>
        /// AJAX调用函数名称是否小写
        /// </summary>
        private bool isLowerAjaxCallName;
        /// <summary>
        /// AJAX调用函数名称
        /// </summary>
        internal unsafe subArray<byte> LowerAjaxCallName
        {
            get
            {
                if (!isLowerAjaxCallName)
                {
                    fixed (byte* bufferFixed = Buffer)
                    {
                        byte* start = bufferFixed + ajaxCallName.StartIndex;
                        fastCSharp.unsafer.memory.ToLower(start, start + ajaxCallName.Length);
                    }
                    isLowerAjaxCallName = true;
                }
                return subArray<byte>.Unsafe(Buffer, ajaxCallName.StartIndex, ajaxCallName.Length);
            }
        }
        /// <summary>
        /// AJAX回调函数名称
        /// </summary>
        private bufferIndex ajaxCallBackName;
        /// <summary>
        /// AJAX回调函数名称
        /// </summary>
        internal subArray<byte> AjaxCallBackName
        {
            get { return subArray<byte>.Unsafe(Buffer, ajaxCallBackName.StartIndex, ajaxCallBackName.Length); }
        }
        /// <summary>
        /// Json字符串
        /// </summary>
        private bufferIndex queryJson;
        /// <summary>
        /// Json字符串
        /// </summary>
        internal subString QueryJson
        {
            get
            {
                if (queryJson.Length != 0)
                {
                    return fastCSharp.web.formQuery.JavascriptUnescape(subArray<byte>.Unsafe(Buffer, queryJson.StartIndex, queryJson.Length));
                }
                return default(subString);
            }
        }
        /// <summary>
        /// 是否重新加载视图
        /// </summary>
        public bool IsReView { get; private set; }
        /// <summary>
        /// HTTP头部名称数据
        /// </summary>
        internal int HeaderCount;
        /// <summary>
        /// 请求内存字节长度,int.MinValue表示未知,-1表示错误
        /// </summary>
        public int ContentLength { get; private set; }
        /// <summary>
        /// JSON编码
        /// </summary>
        internal Encoding JsonEncoding { get; private set; }
        /// <summary>
        /// 查询模式类型
        /// </summary>
        public fastCSharp.web.http.methodType Method { get; internal set; }
        /// <summary>
        /// 提交数据类型
        /// </summary>
        internal postType PostType { get; private set; }
        /// <summary>
        /// 是否需要保持连接
        /// </summary>
        internal bool IsKeepAlive;
        /// <summary>
        /// 是否100 Continue确认
        /// </summary>
        internal bool Is100Continue;
        /// <summary>
        /// 连接是否升级协议
        /// </summary>
        private byte isConnectionUpgrade;
        /// <summary>
        /// 升级协议是否支持WebSocket
        /// </summary>
        private byte isUpgradeWebSocket;
        /// <summary>
        /// 是否WebSocket连接
        /// </summary>
        internal bool IsWebSocket
        {
            get { return (isConnectionUpgrade & isUpgradeWebSocket) != 0 && secWebSocketKey.Length != 0; }
        }
        /// <summary>
        /// WebSocket确认连接值
        /// </summary>
        private bufferIndex secWebSocketKey;
        /// <summary>
        /// WebSocket确认连接值
        /// </summary>
        public subArray<byte> SecWebSocketKey
        {
            get { return subArray<byte>.Unsafe(Buffer, secWebSocketKey.StartIndex, secWebSocketKey.Length); }
        }
        /// <summary>
        /// WebSocket数据
        /// </summary>
        internal subString WebSocketData;
        /// <summary>
        /// 客户端是否支持GZip压缩
        /// </summary>
        internal bool IsGZip { get; set; }
        /// <summary>
        /// HTTP头部是否存在解析错误
        /// </summary>
        internal bool IsHeaderError { get; private set; }
        ///// <summary>
        ///// 是否google搜索引擎
        ///// </summary>
        //public bool IsGoogleQuery
        //{
        //    get { return GoogleQuery.Count != 0; }
        //}
        /// <summary>
        /// URL中是否包含#
        /// </summary>
        private bool? isHash;
        /// <summary>
        /// URL中是否包含#
        /// </summary>
        public bool IsHash
        {
            get
            {
                if (isHash == null) isHash = false;
                return isHash.Value;
            }
        }
        /// <summary>
        /// 是否搜索引擎
        /// </summary>
        private bool? isSearchEngine;
        /// <summary>
        /// 是否搜索引擎
        /// </summary>
        public bool IsSearchEngine
        {
            get
            {
                if (isSearchEngine == null)
                {
                    isSearchEngine = userAgent.Length != 0 && searchEngines.IsMatchLess(Buffer, userAgent.StartIndex, userAgent.Length);
                }
                return isSearchEngine.Value;
            }
        }
        /// <summary>
        /// HTTP请求头
        /// </summary>
        public unsafe requestHeader()
        {
            Buffer = new byte[fastCSharp.config.http.Default.HeaderBufferLength + sizeof(int) + fastCSharp.config.http.Default.MaxHeaderCount * sizeof(bufferIndex) * 2];
        }
        /// <summary>
        /// HTTP头部解析
        /// </summary>
        /// <param name="headerEndIndex">HTTP头部数据结束位置</param>
        /// <param name="receiveEndIndex">HTTP缓冲区接收数据结束位置</param>
        /// <returns>是否成功</returns>
        internal unsafe bool Parse(int headerEndIndex, int receiveEndIndex)
        {
            host.Null();
            IsGZip = false;
            try
            {
                EndIndex = headerEndIndex;
                fixed (byte* bufferFixed = Buffer)
                {
                    if ((Method = fastCSharp.web.http.GetMethod(bufferFixed)) == fastCSharp.web.http.methodType.None) return false;
                    byte* current = bufferFixed, end = bufferFixed + headerEndIndex;
                    for (*end = 32; *current != 32; ++current) ;
                    *end = 13;
                    if (current == end) return false;
                    while (*++current == 32) ;
                    if (current == end) return false;
                    byte* start = current;
                    while (*current != 32 && *current != 13) ++current;
                    uri.Set(start - bufferFixed, current - start);
                    if (uri.Length == 0) return false;
                    while (*current != 13) ++current;

                    byte* headerIndex = bufferFixed + fastCSharp.config.http.Default.HeaderBufferLength + sizeof(int);
                    HeaderCount = ContentLength = 0;
                    cookie.Null();
                    boundary.Null();
                    contentType.Null();
                    referer.Null();
                    userAgent.Null();
                    ifModifiedSince.Null();
                    ifNoneMatch.Null();
                    xProwardedFor.Null();
                    secWebSocketKey.Null();
                    WebSocketData.Null();
                    JsonEncoding = null;
                    isReferer = null;
                    PostType = postType.None;
                    RangeStart = RangeEnd = long.MinValue;
                    IsFormatRange = false;
                    Is100Continue = IsHeaderError = false;
                    isUpgradeWebSocket = isConnectionUpgrade = 0;
                    while (current != end)
                    {
                        if ((current += 2) >= end) return false;
                        for (start = current, *end = (byte)':'; *current != (byte)':'; ++current) ;
                        subArray<byte> name = subArray<byte>.Unsafe(Buffer, (int)(start - bufferFixed), (int)(current - start));
                        *end = 13;
                        if (current == end || *++current != ' ') return false;
                        for (start = ++current; *current != 13; ++current) ;
                        Action<requestHeader, bufferIndex> parseHeaderName = parses.Get(name, null);
                        if (parseHeaderName != null) parseHeaderName(this, new bufferIndex { StartIndex = (short)(start - bufferFixed), Length = (short)(current - start) });
                        else if (HeaderCount == fastCSharp.config.http.Default.MaxHeaderCount)
                        {
                            IsHeaderError = true;
                            break;
                        }
                        else
                        {
                            (*(bufferIndex*)headerIndex).Set(name.StartIndex, name.Count);
                            (*(bufferIndex*)(headerIndex + sizeof(bufferIndex))).Set(start - bufferFixed, current - start);
                            ++HeaderCount;
                            headerIndex += sizeof(bufferIndex) * 2;
                        }
                    }
                    if (host.Length == 0 || ContentLength < 0 || (IsWebSocket && (IsGZip || Method != web.http.methodType.GET || ifModifiedSince.Length != 0))) return false;

                    if (contentType.Length != 0)
                    {
                        start = bufferFixed + contentType.StartIndex;
                        end = start + contentType.Length;
                        current = fastCSharp.unsafer.memory.Find(start, end, (byte)';');
                        int length = current == null ? contentType.Length : (int)(current - start);
                        if (length == 33)
                        {//application/x-www-form-urlencoded
                            if ((((*(int*)start | 0x20202020) ^ ('a' | ('p' << 8) | ('p' << 16) | ('l' << 24)))
                                | ((*(int*)(start + sizeof(int)) | 0x20202020) ^ ('i' | ('c' << 8) | ('a' << 16) | ('t' << 24)))
                                | ((*(int*)(start + sizeof(int) * 2) | 0x00202020) ^ ('i' | ('o' << 8) | ('n' << 16) | ('/' << 24)))
                                | ((*(int*)(start + sizeof(int) * 3) | 0x20200020) ^ ('x' | ('-' << 8) | ('w' << 16) | ('w' << 24)))
                                | ((*(int*)(start + sizeof(int) * 4) | 0x20200020) ^ ('w' | ('-' << 8) | ('f' << 16) | ('o' << 24)))
                                | ((*(int*)(start + sizeof(int) * 5) | 0x20002020) ^ ('r' | ('m' << 8) | ('-' << 16) | ('u' << 24)))
                                | ((*(int*)(start + sizeof(int) * 6) | 0x20202020) ^ ('r' | ('l' << 8) | ('e' << 16) | ('n' << 24)))
                                | ((*(int*)(start + sizeof(int) * 7) | 0x20202020) ^ ('c' | ('o' << 8) | ('d' << 16) | ('e' << 24)))
                                | ((*(start + sizeof(int) * 8) | 0x20) ^ 'd')) == 0)
                            {
                                PostType = postType.Form;
                            }
                        }
                        else if (length == 16)
                        {//application/json; charset=utf-8
                            if ((((*(int*)start | 0x20202020) ^ ('a' | ('p' << 8) | ('p' << 16) | ('l' << 24)))
                                | ((*(int*)(start + sizeof(int)) | 0x20202020) ^ ('i' | ('c' << 8) | ('a' << 16) | ('t' << 24)))
                                | ((*(int*)(start + sizeof(int) * 2) | 0x00202020) ^ ('i' | ('o' << 8) | ('n' << 16) | ('/' << 24)))
                                | ((*(int*)(start + sizeof(int) * 3) | 0x20202020) ^ ('j' | ('s' << 8) | ('o' << 16) | ('n' << 24)))) == 0)
                            {
                                *end = (byte)'c';
                                if (*(start += 16) == ';')
                                {
                                    while (*++start != 'c') ;
                                    if (start != end && (((*(int*)start | 0x20202020) ^ ('c' | ('h' << 8) | ('a' << 16) | ('r' << 24)))
                                        | ((*(int*)(start + sizeof(int)) | 0x00202020) ^ ('s' | ('e' << 8) | ('t' << 16) | ('=' << 24)))) == 0)
                                    {
                                        switch ((byte)(*(start + 10) | 0x20))
                                        {
                                            case (byte)'2'://gb2312
                                                if (*(int*)(start + 10) == ('2' | ('3' << 8) | ('1' << 16) | ('2' << 24))) JsonEncoding = pub.Gb2312;
                                                break;
                                            case (byte)'f'://utf-8
                                                if ((*(int*)(start + 9) | 0x2020) == ('t' | ('f' << 8) | ('-' << 16) | ('8' << 24))) JsonEncoding = Encoding.UTF8;
                                                break;
                                            case (byte)'k'://gbk
                                                if ((*(int*)(start + 7) | 0x20202000) == ('=' | ('g' << 8) | ('b' << 16) | ('k' << 24))) JsonEncoding = pub.Gbk;
                                                break;
                                            case (byte)'g'://big5
                                                if ((*(int*)(start + 8) | 0x00202020) == ('b' | ('i' << 8) | ('g' << 16) | ('5' << 24))) JsonEncoding = pub.Big5;
                                                break;
                                            case (byte)'1'://gb18030
                                                if (*(int*)(start + 11) == ('8' | ('0' << 8) | ('3' << 16) | ('0' << 24))) JsonEncoding = pub.Gb18030;
                                                break;
                                            case (byte)'i'://unicode
                                                if ((*(int*)(start + 11) | 0x20202020) == ('c' | ('o' << 8) | ('d' << 16) | ('e' << 24))) JsonEncoding = Encoding.Unicode;
                                                break;
                                        }
                                    }
                                }
                                *end = 13;
                                if (JsonEncoding == null) JsonEncoding = Encoding.UTF8;
                                PostType = postType.Json;
                            }
                        }
                        else if (length == 19 && contentType.Length > 30)
                        {//multipart/form-data; boundary=---------------------------7dc2e63860144
                            if ((((*(int*)start | 0x20202020) ^ ('m' | ('u' << 8) | ('l' << 16) | ('t' << 24)))
                                | ((*(int*)(start + sizeof(int)) | 0x20202020) ^ ('i' | ('p' << 8) | ('a' << 16) | ('r' << 24)))
                                | ((*(int*)(start + sizeof(int) * 2) | 0x20200020) ^ ('t' | ('/' << 8) | ('f' << 16) | ('o' << 24)))
                                | ((*(int*)(start + sizeof(int) * 3) | 0x20002020) ^ ('r' | ('m' << 8) | ('-' << 16) | ('d' << 24)))
                                | ((*(int*)(start + sizeof(int) * 4) | 0x00202020) ^ ('a' | ('t' << 8) | ('a' << 16) | (';' << 24)))
                                | ((*(int*)(start + sizeof(int) * 5) | 0x20202000) ^ (' ' | ('b' << 8) | ('o' << 16) | ('u' << 24)))
                                | ((*(int*)(start + sizeof(int) * 6) | 0x20202020) ^ ('n' | ('d' << 8) | ('a' << 16) | ('r' << 24)))
                                | ((*(short*)(start += sizeof(int) * 7) | 0x20) ^ ('y' | ('=' << 8)))) == 0)
                            {
                                boundary.Set(contentType.StartIndex + sizeof(int) * 7 + 2, contentType.Length - (sizeof(int) * 7 + 2));
                                if (boundary.Length > maxBoundaryLength) IsHeaderError = true;
                                PostType = postType.FormData;
                            }
                        }
                    }

                    if (Method == fastCSharp.web.http.methodType.POST)
                    {
                        if (PostType == postType.None || ContentLength < ((int)(receiveEndIndex - headerEndIndex) - sizeof(int))) return false;
                    }
                    else
                    {
                        if (PostType != postType.None || (!IsKeepAlive && receiveEndIndex != headerEndIndex + sizeof(int)) || ((uint)ContentLength | (uint)(int)boundary.Length) != 0) return false;
                    }
                    if (!IsHeaderError && !IsRangeError)
                    {
                        queryIndexs.Empty();
                        ajaxCallName.Null();
                        ajaxCallBackName.Null();
                        queryJson.Null();
                        IsReView = isLowerAjaxCallName = false;
                        if (IsWebSocket)
                        {
                            isSearchEngine = isHash = false;
                        }
                        else
                        {
                            isSearchEngine = isHash = null;
                            start = bufferFixed + uri.StartIndex;
                            end = fastCSharp.unsafer.memory.Find(start, start + uri.Length, (byte)'?');
                            if (end == null)
                            {
                                end = fastCSharp.unsafer.memory.Find(start, start + uri.Length, (byte)'#');
                                if (end != null) isSearchEngine = isHash = true;
                            }
                            else if (*(end + 1) == '_')
                            {
                                fixed (byte* googleFixed = googleFragmentName)
                                {
                                    if (unsafer.memory.Equal(googleFixed, end + 2, googleFragmentName.Length))
                                    {
                                        isSearchEngine = isHash = true;
                                        byte* write = end + 1, urlEnd = start + uri.Length;
                                        current = write + googleFragmentName.Length + 1;
                                        byte endValue = *urlEnd;
                                        *urlEnd = (byte)'%';
                                        do
                                        {
                                            while (*current != '%') *write++ = *current++;
                                            if (current == urlEnd) break;
                                            uint code = (uint)(*++current - '0'), number = (uint)(*++current - '0');
                                            if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                                            code = (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number) + (code << 4);
                                            *write++ = code == 0 ? (byte)' ' : (byte)code;
                                        }
                                        while (++current < urlEnd);
                                        *urlEnd = endValue;
                                        uri.Length = (short)((int)(write - bufferFixed) - uri.StartIndex);
                                    }
                                }
                            }
                            isLowerPath = false;
                            if (end == null) path = uri;
                            else
                            {
                                path.Set(uri.StartIndex, (short)(end - start));
                                bufferIndex nameIndex, valueIndex = new bufferIndex();
                                current = end;
                                byte endValue = *(end = start + uri.Length);
                                *end = (byte)'&';
                                if (isHash != null)
                                {
                                    if (*current == '!') ++current;
                                    else if (*current == '%' && *(short*)(current + 1) == '2' + ('1' << 8)) current += 3;
                                }
                                do
                                {
                                    nameIndex.StartIndex = (short)(++current - bufferFixed);
                                    while (*current != '&' && *current != '=') ++current;
                                    nameIndex.Length = (short)((int)(current - bufferFixed) - nameIndex.StartIndex);
                                    if (*current == '=')
                                    {
                                        valueIndex.StartIndex = (short)(++current - bufferFixed);
                                        while (*current != '&') ++current;
                                        valueIndex.Length = (short)((int)(current - bufferFixed) - valueIndex.StartIndex);
                                    }
                                    else valueIndex.Null();
                                    if (nameIndex.Length == 1)
                                    {
                                        char name = (char)*(bufferFixed + nameIndex.StartIndex);
                                        if (name == fastCSharp.config.web.Default.AjaxCallName) ajaxCallName = valueIndex;
                                        else if (name == fastCSharp.config.web.Default.AjaxCallBackName) ajaxCallBackName = valueIndex;
                                        else if (name == fastCSharp.config.web.Default.ReViewName) IsReView = true;
                                        else if (name == fastCSharp.config.web.Default.QueryJsonName) queryJson = valueIndex;
                                        else queryIndexs.Add(new keyValue<bufferIndex, bufferIndex>(nameIndex, valueIndex));
                                    }
                                    else queryIndexs.Add(new keyValue<bufferIndex, bufferIndex>(nameIndex, valueIndex));
                                }
                                while (current != end);
                                *end = endValue;
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception error)
            {
                log.Default.Add(error, null, false);
            }
            return false;
        }
        /// <summary>
        /// WebSocket重置URL
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>是否成功</returns>
        internal unsafe bool SetWebSocketUrl(byte* start, byte* end)
        {
            int length = (int)(end - start);
            if (EndIndex + length <= fastCSharp.config.http.Default.HeaderBufferLength)
            {
                fixed (byte* bufferFixed = Buffer)
                {
                    unsafer.memory.Copy(start, bufferFixed + EndIndex, length);
                    start = bufferFixed + EndIndex;
                    uri.Set(EndIndex, length);
                    byte* current = start;
                    for (*(end = start + length) = (byte)'?'; *current != '?'; ++current) ;
                    queryIndexs.Empty();
                    if (current == end) path = uri;
                    else
                    {
                        path.Set(EndIndex, (short)(current - start));
                        bufferIndex nameIndex, valueIndex;
                        *end = (byte)'&';
                        do
                        {
                            nameIndex.StartIndex = (short)(++current - bufferFixed);
                            while (*current != '&' && *current != '=') ++current;
                            nameIndex.Length = (short)((int)(current - bufferFixed) - nameIndex.StartIndex);
                            if (*current == '=')
                            {
                                valueIndex.StartIndex = (short)(++current - bufferFixed);
                                while (*current != '&') ++current;
                                valueIndex.Length = (short)((int)(current - bufferFixed) - valueIndex.StartIndex);
                                queryIndexs.Add(new keyValue<bufferIndex, bufferIndex>(nameIndex, valueIndex));
                            }
                            else if (nameIndex.Length != 0) queryIndexs.Add(new keyValue<bufferIndex, bufferIndex>(nameIndex, new bufferIndex()));
                        }
                        while (current != end);
                    }
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 判断是否存在Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>是否存在Cookie值</returns>
        internal unsafe bool IsCookie(byte[] name)
        {
            if (cookie.Length > name.Length)
            {
                fixed (byte* nameFixed = name)
                {
                    return getCookie(nameFixed, name.Length).StartIndex == 0;
                }
            }
            return false;
        }
        /// <summary>
        /// 获取Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        internal unsafe string GetCookieString(byte[] name)
        {
            if (cookie.Length > name.Length)
            {
                fixed (byte* nameFixed = name)
                {
                    bufferIndex index = getCookie(nameFixed, name.Length);
                    if (index.StartIndex != 0)
                    {
                        if (index.Length == 0) return string.Empty;
                        fixed (byte* bufferFixed = Buffer) return String.DeSerialize(bufferFixed + index.StartIndex, -index.Length);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 获取Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        internal unsafe subArray<byte> GetCookie(byte[] name)
        {
            if (cookie.Length > name.Length)
            {
                fixed (byte* nameFixed = name)
                {
                    bufferIndex index = getCookie(nameFixed, name.Length);
                    if (index.StartIndex != 0) return subArray<byte>.Unsafe(Buffer, index.StartIndex, index.Length);
                }
            }
            return default(subArray<byte>);
        }
        /// <summary>
        /// 获取Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        public unsafe string GetCookie(string name)
        {
            if (cookie.Length > name.Length && name.Length <= unmanagedStreamBase.DefaultLength)
            {
                bufferIndex index;
                fixed (char* nameFixed = name)
                {
                    pointer cookieNameBuffer = unmanagedPool.TinyBuffers.Get();
                    unsafer.String.WriteBytes(nameFixed, name.Length, cookieNameBuffer.Byte);
                    index = getCookie(cookieNameBuffer.Byte, name.Length);
                    unmanagedPool.TinyBuffers.Push(ref cookieNameBuffer);
                }
                if (index.StartIndex != 0)
                {
                    if (index.Length == 0) return string.Empty;
                    fixed (byte* bufferFixed = Buffer) return String.DeSerialize(bufferFixed + index.StartIndex, -index.Length);
                }
            }
            return null;
        }
        /// <summary>
        /// 获取Cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="nameLength">名称长度</param>
        /// <returns>值</returns>
        private unsafe bufferIndex getCookie(byte* name, int nameLength)
        {
            fixed (byte* bufferFixed = Buffer)
            {
                byte* start = bufferFixed + cookie.StartIndex, end = start + cookie.Length, searchEnd = end - nameLength;
                *end = (byte)';';
                do
                {
                    while (*start == ' ') ++start;
                    if (start >= searchEnd) break;
                    if (*(start + nameLength) == '=')
                    {
                        if (unsafer.memory.Equal(name, start, nameLength))
                        {
                            for (start += nameLength + 1; *start == ' '; ++start) ;
                            int startIndex = (int)(start - bufferFixed);
                            while (*start != ';') ++start;
                            return new bufferIndex { StartIndex = (short)startIndex, Length = (short)((int)(start - bufferFixed) - startIndex) };
                        }
                        start += nameLength + 1;
                    }
                    while (*start != ';') ++start;
                }
                while (++start < searchEnd);
            }
            return default(bufferIndex);
        }
        /// <summary>
        /// 获取查询值
        /// </summary>
        /// <param name="name">查询名称</param>
        /// <returns>查询值</returns>
        private unsafe subArray<byte> getQuery(byte[] name)
        {
            int count = queryIndexs.Count;
            if (count != 0)
            {
                fixed (byte* bufferFixed = Buffer)
                {
                    foreach (keyValue<bufferIndex, bufferIndex> index in queryIndexs.array)
                    {
                        if (index.Key.Length == name.Length
                            && unsafer.memory.Equal(name, bufferFixed + index.Key.StartIndex, name.Length))
                        {
                            return subArray<byte>.Unsafe(Buffer, index.Value.StartIndex, index.Value.Length);
                        }
                        if (--count == 0) break;
                    }
                }
            }
            return default(subArray<byte>);
        }
        /// <summary>
        /// 获取查询整数值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nullValue"></param>
        /// <returns></returns>
        public unsafe int GetQueryInt(byte[] name, int nullValue)
        {
            subArray<byte> value = getQuery(name);
            if (value.Count != 0)
            {
                int intValue = 0;
                fixed (byte* bufferFixed = Buffer)
                {
                    byte* start = bufferFixed + value.StartIndex, end = start + value.Count;
                    for (intValue = *(start) - '0'; ++start != end; intValue += *(start) - '0') intValue *= 10;
                }
                return intValue;
            }
            return nullValue;
        }
        /// <summary>
        /// 查询解析
        /// </summary>
        /// <typeparam name="valueType">目标类型</typeparam>
        /// <param name="value">目标数据</param>
        /// <returns>是否解析成功</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ParseQuery<valueType>(ref valueType value)
        {
            return queryParser.Parse(this, ref value);
        }
        /// <summary>
        /// 请求域名解析
        /// </summary>
        /// <param name="socket">HTTP头部接收器</param>
        /// <param name="value">请求域名索引位置</param>
        private static void parseHost(requestHeader header, bufferIndex value)
        {
            header.host = value;
        }
        /// <summary>
        /// 提交内容数据长度解析
        /// </summary>
        /// <param name="socket">HTTP头部接收器</param>
        /// <param name="value">提交内容数据长度索引位置</param>
        private unsafe static void parseContentLength(requestHeader header, bufferIndex value)
        {
            fixed (byte* dataFixed = header.Buffer)
            {
                for (byte* start = dataFixed + value.StartIndex, end = start + value.Length; start != end; ++start)
                {
                    header.ContentLength *= 10;
                    header.ContentLength += *start - '0';
                }
            }
        }
        /// <summary>
        /// 内容数据编码方式解析
        /// </summary>
        /// <param name="socket">HTTP头部接收器</param>
        /// <param name="value">内容数据编码方式索引位置</param>
        private unsafe static void parseAcceptEncoding(requestHeader header, bufferIndex value)
        {
            if (value.Length >= 4)
            {
                fixed (byte* dataFixed = header.Buffer)
                {
                    byte* start = dataFixed + value.StartIndex, end = start + value.Length;
                    byte endValue = *end;
                    *end = (byte)'g';
                    while (true)
                    {
                        while (*start != 'g') ++start;
                        if (start != end)
                        {
                            if ((*(int*)start | 0x20202020) == ('g' | ('z' << 8) | ('i' << 16) | ('p' << 24)))
                            {
                                header.IsGZip = true;
                                break;
                            }
                            else ++start;
                        }
                        else break;
                    }
                    *end = endValue;
                }
            }
        }
        /// <summary>
        /// 保持连接解析
        /// </summary>
        /// <param name="socket">HTTP头部接收器</param>
        /// <param name="value">保持连接索引位置</param>
        private unsafe static void parseConnection(requestHeader header, bufferIndex value)
        {
            if (value.Length == 10)
            {
                fixed (byte* dataFixed = header.Buffer)
                {
                    byte* start = dataFixed + value.StartIndex;
                    if ((((*(int*)start | 0x20202020) ^ ('k' | ('e' << 8) | ('e' << 16) | ('p' << 24)))
                        | ((*(int*)(start + sizeof(int)) | 0x20202000) ^ ('-' | ('a' << 8) | ('l' << 16) | ('i' << 24)))
                        | ((*(short*)(start + sizeof(int) * 2) | 0x2020) ^ ('v' | ('e' << 8)))) == 0)
                    {
                        header.IsKeepAlive = true;
                    }
                }
            }
            else if (value.Length == 5)
            {
                fixed (byte* dataFixed = header.Buffer)
                {
                    byte* start = dataFixed + value.StartIndex;
                    if ((((*(int*)start | 0x20202020) ^ ('c' | ('l' << 8) | ('o' << 16) | ('s' << 24)))
                        | ((*(start + sizeof(int)) | 0x20) ^ 'e')) == 0)
                    {
                        header.IsKeepAlive = false;
                    }
                }
            }
            else if (value.Length == 7)
            {
                fixed (byte* dataFixed = header.Buffer)
                {
                    byte* start = dataFixed + value.StartIndex;
                    if ((((*(int*)start | 0x20202020) ^ ('u' | ('p' << 8) | ('g' << 16) | ('r' << 24)))
                        | (((*(int*)(start + sizeof(int)) | 0x202020) & 0xffffff) ^ ('a' | ('d' << 8) | ('e' << 16)))) == 0)
                    {
                        header.isConnectionUpgrade = 1;
                    }
                }
            }
        }
        /// <summary>
        /// HTTP请求内容类型解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">HTTP请求内容类型索引位置</param>
        private static void parseContentType(requestHeader header, bufferIndex value)
        {
            header.contentType = value;
        }
        /// <summary>
        /// 访问来源解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">访问来源索引位置</param>
        private static void parseReferer(requestHeader header, bufferIndex value)
        {
            header.referer = value;
        }
        /// <summary>
        /// 访问来源解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">访问来源索引位置</param>
        private static void parseOrigin(requestHeader header, bufferIndex value)
        {
            header.origin = value;
        }
        /// <summary>
        /// 请求字节范围解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">请求字节范围索引位置</param>
        private unsafe static void parseRange(requestHeader header, bufferIndex value)
        {
            if (value.Length > 6)
            {
                fixed (byte* dataFixed = header.Buffer)
                {
                    byte* start = dataFixed + value.StartIndex;
                    if (((*(int*)start ^ ('b' + ('y' << 8) + ('t' << 16) + ('e' << 24))) | (*(short*)(start + 4) ^ ('s' + ('=' << 8)))) == 0)
                    {
                        byte* end = start + value.Length;
                        if (*(start += 6) == '-')
                        {
                            long rangeEnd = 0;
                            while (++start != end)
                            {
                                rangeEnd *= 10;
                                rangeEnd += *start - '0';
                            }
                            header.RangeEnd = rangeEnd;
                            return;
                        }
                        else
                        {
                            long rangeStart = 0;
                            do
                            {
                                int number = *start - '0';
                                if ((uint)number > 9) break;
                                rangeStart *= 10;
                                ++start;
                                rangeStart += number;
                            }
                            while (true);
                            if (rangeStart >= 0 && *start == '-')
                            {
                                if (++start == end)
                                {
                                    header.RangeStart = rangeStart;
                                    return;
                                }
                                else
                                {
                                    long rangeEnd = *start - '0';
                                    while (++start != end)
                                    {
                                        rangeEnd *= 10;
                                        rangeEnd += *start - '0';
                                    }
                                    if (rangeEnd >= rangeStart)
                                    {
                                        header.RangeStart = rangeStart;
                                        header.RangeEnd = rangeEnd;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Cookie解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">Cookie索引位置</param>
        private static void parseCookie(requestHeader header, bufferIndex value)
        {
            header.cookie = value;
        }
        /// <summary>
        /// 浏览器参数解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">浏览器参数索引位置</param>
        private static void parseUserAgent(requestHeader header, bufferIndex value)
        {
            header.userAgent = value;
        }
        /// <summary>
        /// 客户端文档时间标识解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">客户端文档时间标识索引位置</param>
        private static void parseIfModifiedSince(requestHeader header, bufferIndex value)
        {
            header.ifModifiedSince = value;
        }
        /// <summary>
        /// 客户端缓存有效标识解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">客户端缓存有效标识索引位置</param>
        private unsafe static void parseIfNoneMatch(requestHeader header, bufferIndex value)
        {
            if (value.Length >= 2)
            {
                fixed (byte* dataFixed = header.Buffer)
                {
                    byte* start = dataFixed + value.StartIndex;
                    if (*(start + value.Length - 1) == '"')
                    {
                        if (*start == '"') header.ifNoneMatch.Set(value.StartIndex + 1, value.Length - 2);
                        else if ((*(int*)start & 0xffffff) == ('W' + ('/' << 8) + ('"' << 16)) && value.Length >= 4)
                        {
                            header.ifNoneMatch.Set(value.StartIndex + 3, value.Length - 4);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 转发信息解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">转发信息索引位置</param>
        private static void parseXProwardedFor(requestHeader header, bufferIndex value)
        {
            header.xProwardedFor = value;
        }
        /// <summary>
        /// 100 Continue确认解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">100 Continue确认索引位置</param>
        private static unsafe void parseExpect(requestHeader header, bufferIndex value)
        {
            if (value.Length == 12)
            {
                fixed (byte* dataFixed = header.Buffer)
                {
                    byte* start = dataFixed + value.StartIndex;
                    if (((*(int*)start ^ ('1' | ('0' << 8) | ('0' << 16) | ('-' << 24)))
                        | ((*(int*)(start + sizeof(int)) | 0x20202020) ^ ('c' | ('o' << 8) | ('n' << 16) | ('t' << 24)))
                        | ((*(int*)(start + sizeof(int) * 2) | 0x20202020) ^ ('i' | ('n' << 8) | ('u' << 16) | ('e' << 24)))) == 0)
                    {
                        header.Is100Continue = true;
                    }
                }
            }
        }
        /// <summary>
        /// 协议升级支持解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">协议升级支持索引位置</param>
        private static unsafe void parseUpgrade(requestHeader header, bufferIndex value)
        {
            if (value.Length == 9)
            {
                fixed (byte* dataFixed = header.Buffer)
                {
                    byte* start = dataFixed + value.StartIndex;
                    if ((((*(int*)start | 0x20202020) ^ ('w' | ('e' << 8) | ('b' << 16) | ('s' << 24)))
                        | ((*(int*)(start + sizeof(int)) | 0x20202020) ^ ('o' | ('c' << 8) | ('k' << 16) | ('e' << 24)))
                        | ((*(start + sizeof(int) * 2) | 0x20) ^ 't')) == 0)
                    {
                        header.isUpgradeWebSocket = 1;
                    }
                }
            }
        }
        /// <summary>
        /// WebSocket确认连接值解析
        /// </summary>
        /// <param name="header">HTTP头部接收器</param>
        /// <param name="value">WebSocket确认连接值索引位置</param>
        private static void parseSecWebSocketKey(requestHeader header, bufferIndex value)
        {
            if (value.Length <= 32) header.secWebSocketKey = value;
        }
        static RequestHeaderPlus()
        {
            list<keyValue<headerName, Action<requestHeader, bufferIndex>>> parseList = new list<keyValue<headerName, Action<requestHeader, bufferIndex>>>();
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.HostBytes, parseHost));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.ContentLengthBytes, parseContentLength));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.AcceptEncodingBytes, parseAcceptEncoding));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.ConnectionBytes, parseConnection));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.ContentTypeBytes, parseContentType));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.CookieBytes, parseCookie));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.RefererBytes, parseReferer));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.RangeBytes, parseRange));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.UserAgentBytes, parseUserAgent));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.IfModifiedSinceBytes, parseIfModifiedSince));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.IfNoneMatchBytes, parseIfNoneMatch));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.XProwardedForBytes, parseXProwardedFor));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.ExpectBytes, parseExpect));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.UpgradeBytes, parseUpgrade));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.SecWebSocketKeyBytes, parseSecWebSocketKey));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.SecWebSocketOriginBytes, parseOrigin));
            parseList.Add(new keyValue<headerName, Action<requestHeader, bufferIndex>>(header.OriginBytes, parseOrigin));
            parses = new uniqueDictionary<headerName, Action<requestHeader, bufferIndex>>(parseList, 32);

            searchEngines = new trieGraph();
            searchEngines.BuildTree(new byte[][]
            {
                ("Googlebot").getBytes(),
                ("spider").getBytes(),
                //fastCSharpSpiderUserAgent.getBytes(),
                //("iaskspider").getBytes(),
                //("Sogou web spider").getBytes(),
                //("Sogou push spider").getBytes(),
                //("Baiduspider").getBytes(),
                //("Sosospider").getBytes(),
                //("yisouspider").getBytes(),
                ("msnbot").getBytes(),
                ("YandexBot").getBytes(),
                ("Mediapartners-Google").getBytes(),
                ("YoudaoBot").getBytes(),
                ("Yandex").getBytes(),
                ("MJ12bot").getBytes(),
                ("bingbot").getBytes(),
                ("Yahoo! Slurp").getBytes(),
                ("ia_archiver").getBytes(),
                ("GeoHasher").getBytes(),
                ("R6_CommentReader").getBytes(),
                ("SiteBot").getBytes(),
                ("DotBot").getBytes(),
                ("Twiceler").getBytes(),
                ("renren share slurp").getBytes()
            });
            searchEngines.BuildGraph();
        }
    }
}
