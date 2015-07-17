﻿//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TemplatePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  TemplatePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:30:03
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
    /// 树节点模板
    /// </summary>
    public abstract class TemplatePlus
    {
        /// <summary>
        /// 成员信息缓存集合
        /// </summary>
        private static Dictionary<Type, Dictionary<hashString, memberIndex>> memberCache = dictionary.CreateOnly<Type, Dictionary<hashString, memberIndex>>();
        ///// <summary>
        ///// 获取成员信息集合
        ///// </summary>
        ///// <param name="type">类型</param>
        ///// <returns>成员信息集合</returns>
        //protected static Dictionary<hashString, memberIndex> getMemberCache(Type type)
        //{
        //    Dictionary<hashString, memberIndex> values;
        //    if (!memberCache.TryGetValue(type, out values))
        //    {
        //        try
        //        {
        //            memberCache[type] = values = memberIndexGroup.Get((Type)type).Find(memberFilters.Instance)
        //                .getDictionary(value => (hashString)value.Member.Name);
        //        }
        //        catch (Exception error)
        //        {
        //            string output = string.Join(",", memberIndexGroup.Get((Type)type).Find(memberFilters.Instance)
        //                .groupCount(value => value.Member.Name)
        //                .getFind(value => value.Value != 1)
        //                .GetArray(value => value.Key));
        //            log.Error.ThrowReal(error, ((Type)type).FullName + " : " + output, true);
        //        }
        //    }
        //    return values;
        //}
        /// <summary>
        /// 成员树节点
        /// </summary>
        public class memberNode
        {
            /// <summary>
            /// 树节点模板
            /// </summary>
            private template template;
            /// <summary>
            /// 成员类型
            /// </summary>
            public memberType Type { get; private set; }
            /// <summary>
            /// 当前节点成员名称
            /// </summary>
            private string name;
            /// <summary>
            /// 成员名称+成员信息集合
            /// </summary>
            internal Dictionary<hashString, memberIndex> Members
            {
                get
                {
                    Dictionary<hashString, memberIndex> values;
                    Type type = Type.Type;
                    if (!memberCache.TryGetValue(type, out values))
                    {
                        try
                        {
                            memberCache[type] = values = memberIndexGroup.Get(Type).Find(memberFilters.Instance)
                                .getDictionary(value => (hashString)value.Member.Name);
                        }
                        catch (Exception error)
                        {
                            string output = string.Join(",", memberIndexGroup.Get(Type).Find(memberFilters.Instance)
                                .groupCount(value => value.Member.Name)
                                .getFind(value => value.Value != 1)
                                .GetArray(value => value.Key));
                            log.Error.ThrowReal(error, Type.FullName + " : " + output, true);
                        }
                    }
                    return values;
                }
            }
            /// <summary>
            /// 父节点成员
            /// </summary>
            internal memberNode Parent;
            /// <summary>
            /// 节点路径
            /// </summary>
            public string Path { get; internal set; }
            ///// <summary>
            ///// 是否延时加载属性
            ///// </summary>
            //public bool IsLadyProperty;
            /// <summary>
            /// 节点路径全称
            /// </summary>
            internal string FullPath
            {
                get
                {
                    if (Parent != null)
                    {
                        collection<string> path = new collection<string>();
                        for (memberNode member = this; member.Parent != null; member = member.Parent) path.AddExpand("." + member.name);
                        return string.Concat(path.ToArray()).Substring(1);
                    }
                    return null;
                }
            }
            /// <summary>
            /// 根据成员名称获取子节点成员
            /// </summary>
            /// <param name="name">成员名称</param>
            /// <returns>子节点成员</returns>
            public memberNode Get(string name)
            {
                return Get(ref name, false);
            }
            /// <summary>
            /// 根据成员名称获取子节点成员
            /// </summary>
            /// <param name="name">成员名称</param>
            /// <param name="isLast">是否最后层级</param>
            /// <returns>子节点成员</returns>
            internal memberNode Get(ref string name, bool isLast)
            {
                Dictionary<hashString, memberNode> paths;
                if (!template.memberPaths.TryGetValue(this, out paths))
                {
                    template.memberPaths[this] = paths = dictionary.CreateHashString<memberNode>();
                }
                memberNode value;
                if (isLast && template.isCollectionLength && name == "length")
                {
                    if (Type.Type.IsArray) name = "Length";
                    else if (typeof(ICollection).IsAssignableFrom(Type.Type)) name = "Count";
                }
                if (paths.TryGetValue(name, out value)) return value;
                bool isPath = true;
                if (name.Length != 0)
                {
                    memberIndex member;
                    if (Members.TryGetValue(name, out member))
                    {
                        //if (member.Member.customAttribute<fastCSharp.code.ignore>(true) != null) isPath = false;
                        if (member.IsIgnore) isPath = false;
                        fastCSharp.code.cSharp.webView.outputAjax outputAjax = member.GetAttribute<fastCSharp.code.cSharp.webView.outputAjax>(true, true);
                        if (outputAjax != null)
                        {
                            if (outputAjax.BindingName != null)
                            {
                                string outputName = outputAjax.BindingName;
                                value = Get(ref outputName, false);
                            }
                            if (!outputAjax.IsSetup) isPath = false;
                        }
                        value = new memberNode(template, member.Type, name, null);
                        //keyValue<memberIndex, string> propertyIndex;
                        //if (Template.currentMembers.Unsafer.Array[0] == this
                        //    && !Template.propertyNames.TryGetValue(name, out propertyIndex))
                        //{
                        //    Template.propertyNames.Add(name, new keyValue<memberIndex, string>(member, "_p" + Template.propertyNames.Count.toString()));
                        //}
                        //else propertyIndex.Value = name;
                        //, IsLadyProperty = !member.IsField && member.Member.customAttribute<ladyProperty>(false, false) != null
                    }
                }
                else value = new memberNode(template, Type.EnumerableArgumentType, null, null);
                if (value != null)
                {
                    value.Parent = this;
                    //value.template = template;
                    if (isPath) paths[name] = value;
                }
                return value;
            }
            /// <summary>
            /// 节点路径上是否有下级路径
            /// </summary>
            public bool IsNextPath
            {
                get
                {
                    Dictionary<hashString, memberNode> paths;
                    return template.memberPaths.TryGetValue(this, out paths) && paths.Count != 0;
                }
            }
            /// <summary>
            /// 成员树节点
            /// </summary>
            /// <param name="template">树节点模板</param>
            /// <param name="type">成员类型</param>
            /// <param name="name">当前节点成员名称</param>
            /// <param name="path">当前节点成员名称</param>
            internal memberNode(template template, memberType type, string name, string path)
            {
                this.template = template;
                this.Type = type;
                this.name = name;
                Path = path;
                foreach (memberIndex member in Members.Values)
                {
                    //if (member.Member.customAttribute<fastCSharp.code.ignore>(true) == null)
                    if (!member.IsIgnore)
                    {
                        fastCSharp.code.cSharp.webView.outputAjax outputAjax = member.GetAttribute<fastCSharp.code.cSharp.webView.outputAjax>(true, true);
                        if (outputAjax != null && outputAjax.IsSetup)
                        {
                            name = member.Member.Name;
                            Get(ref name, false);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 模板数据视图类型
        /// </summary>
        protected Type viewType;
        /// <summary>
        /// 当前代码字符串
        /// </summary>
        protected stringBuilder code = new stringBuilder();
        /// <summary>
        /// 错误处理委托
        /// </summary>
        protected Action<string> onError;
        /// <summary>
        /// 信息处理委托
        /// </summary>
        protected Action<string> onMessage;
        /// <summary>
        /// 当前代码字符串
        /// </summary>
        public string Code
        {
            get
            {
                pushCode(null);
                return code.ToString();
            }
        }
        /// <summary>
        /// 当前代码字符串常量
        /// </summary>
        protected stringBuilder pushCodes = new stringBuilder();
        /// <summary>
        /// 子段程序代码集合
        /// </summary>
        protected Dictionary<hashString, string> partCodes = dictionary.CreateHashString<string>();
        /// <summary>
        /// 当前成员节点集合
        /// </summary>
        protected list<memberNode> currentMembers = new list<memberNode>();
        /// <summary>
        /// 成员树
        /// </summary>
        protected Dictionary<memberNode, Dictionary<hashString, memberNode>> memberPaths = dictionary.CreateOnly<memberNode, Dictionary<hashString, memberNode>>();
        ///// <summary>
        ///// 属性成员映射集合
        ///// </summary>
        //protected Dictionary<string, keyValue<memberIndex, string>> propertyNames = dictionary.CreateOnly<string, keyValue<memberIndex, string>>();
        /// <summary>
        /// 临时逻辑变量名称
        /// </summary>
        protected string ifName = "_if_";
        /// <summary>
        /// 集合是否支持length属性
        /// </summary>
        protected virtual bool isCollectionLength { get { return false; } }
        /// <summary>
        /// 是否记录循环集合
        /// </summary>
        protected virtual bool isLoopValue
        {
            get { return false; }
        }
        /// <summary>
        /// 获取临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>变量名称</returns>
        protected string path(int index)
        {
            return "_value" + (index == 0 ? (currentMembers.Count - 1) : index).ToString() + "_";
        }
        /// <summary>
        /// 获取循环索引临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>循环索引变量名称</returns>
        protected string loopIndex(int index)
        {
            return "_loopIndex" + (index == 0 ? (currentMembers.Count - 1) : index).ToString() + "_";
        }
        /// <summary>
        /// 获取循环数量临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>循环数量变量名称</returns>
        protected string loopCount(int index)
        {
            return "_loopCount" + (index == 0 ? (currentMembers.Count - 1) : index).ToString() + "_";
        }
        /// <summary>
        /// 获取循环集合临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>循环集合变量名称</returns>
        protected string loopValues(int index)
        {
            return "_loopValues" + (index == 0 ? (currentMembers.Count - 1) : index).ToString() + "_";
        }
        /// <summary>
        /// 获取循环内临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>循环内变量名称</returns>
        protected string loopValue(int index)
        {
            return "_loopValue" + (index == 0 ? (currentMembers.Count - 1) : index).ToString() + "_";
        }
        /// <summary>
        /// 根据成员名称获取成员树节点
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <param name="isDepth">是否深度搜索,false表示当前节点子节点</param>
        /// <returns>成员树节点</returns>
        protected memberNode getMember(string memberName, out bool isDepth)
        {
            int memberIndex = 0;
            while (memberIndex != memberName.Length && memberName[memberIndex] == '.') ++memberIndex;
            memberName = memberName.Substring(memberIndex);
            memberIndex = currentMembers.Count - memberIndex - 1;
            if (memberIndex < 0) memberIndex = 0;
            memberNode value = currentMembers[memberIndex];
            isDepth = false;
            if (memberName.Length != 0)
            {
                string[] names = memberName.Split('.');
                for (int lastIndex = names.Length - 1; memberIndex >= 0; --memberIndex)
                {
                    if ((value = currentMembers[memberIndex].Get(ref names[0], lastIndex == 0)) != null)
                    {
                        if (memberIndex == 0)
                        {
                            //keyValue<memberIndex, string> propertyIndex;
                            //if (!propertyNames.TryGetValue(names[0], out propertyIndex)) propertyIndex.Value = names[0];
                            //value.Path = propertyIndex.Value;
                            value.Path = names[0];
                        }
                        else value.Path = path(memberIndex) + "." + names[0];
                        if (names.Length != 1) isDepth = true;
                        for (int nameIndex = 1; nameIndex != names.Length; ++nameIndex)
                        {
                            if ((value = value.Get(ref names[nameIndex], nameIndex == lastIndex)) == null) break;
                            value.Path = value.Parent.Path + "." + names[nameIndex];
                        }
                        if (value == null) break;
                        else return value;
                    }
                }
                string message = viewType.fullName() + " 未找到属性 " + currentMembers.lastOrDefault().FullPath + " . " + memberName;
                if (checkErrorMemberName(memberName))
                {
                    onMessage(message);
                    return null;
                }
                onError(message);
            }
            return value;
        }
        /// <summary>
        /// 检测错误成员名称
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <returns>是否忽略错误</returns>
        protected virtual bool checkErrorMemberName(string memberName)
        {
            return false;
        }
        /// <summary>
        /// 添加代码
        /// </summary>
        /// <param name="code">代码,null表示截断字符串</param>
        protected virtual void pushCode(string code)
        {
            if (code != null) pushCodes.Add(code);
            else
            {
                code = pushCodes.ToString();
                if (code.Length != 0)
                {
                    this.code.Append(@"
            _code_.Add(@""", code.Replace(@"""", @""""""), @""");");
                }
                pushCodes.Empty();
            }
        }
        /// <summary>
        /// 添加当前成员节点
        /// </summary>
        /// <param name="member">成员节点</param>
        protected void pushMember(memberNode member)
        {
            currentMembers.Add(member);
        }
        /// <summary>
        /// if开始代码段
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <param name="isSkip">是否跳跃层次</param>
        protected void ifStart(string memberName, bool isSkip)
        {
            bool isDepth;
            memberNode member = getMember(memberName, out isDepth);
            pushMember(member);
            if (isSkip) pushMember(member);
            string name = path(0);
            code.Append(@"
                {
                    ", member.Type.FullName, " ", name, " = ", member.Path, ";");
            ifStart(member.Type, name, null);
        }
        /// <summary>
        /// if开始代码段
        /// </summary>
        /// <param name="type">成员类型</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="ifName">if临时变量名称</param>
        protected void ifStart(memberType type, string name, string ifName)
        {
            if (type.IsStruct || type.Type.IsEnum)
            {
                if (type.IsBool)
                {
                    code.Append(@"
                    if (", name, ")");
                }
                else if (type.IsAjaxToString)
                {
                    code.Append(@"
                    if (", name, " != 0)");
                }
            }
            else
            {
                code.Append(@"
                    if (", name, " != null)");
            }
            code.Append(@"
                    {");
            if (ifName != null)
            {
                code.Append(@"
                        ", ifName, " = true;");
            }
        }
        /// <summary>
        /// if结束代码段
        /// </summary>
        /// <param name="isMember">是否删除成员节点</param>
        protected void ifEnd(bool isMember)
        {
            if (isMember) currentMembers.Pop();
            code.Append(@"
                    }
                }");
        }
        /// <summary>
        /// if代码段处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="memberName">成员名称</param>
        /// <param name="isDepth">是否深度搜索</param>
        /// <param name="doMember">成员处理函数</param>
        protected void ifThen(memberNode member, string memberName, bool isDepth, Action<memberNode> doMember)
        {
            if (isDepth)
            {
                pushCode(null);
                subString[] names = splitMemberName(memberName);
                for (int index = 0; index != names.Length - 1; ++index) ifStart(names[index], false);
                doMember(getMember(names[names.Length - 1], out isDepth));
                pushCode(null);
                for (int index = 0; index != names.Length - 1; ++index) ifEnd(true);
            }
            else doMember(member);
        }
        /// <summary>
        /// 输出绑定的数据
        /// </summary>
        /// <param name="member">成员节点</param>
        protected void at(memberNode member)
        {
            pushCode(null);
            if (member.Type.IsString)
            {
                code.Append(@"
            _code_.Add(", member.Path, ");");
            }
            else if (member.Type.IsBool && member.Type.IsStruct)
            {
                code.Append(@"
            _code_.Add(", member.Path, @" ? ""true"" : ""false"");");
            }
            else
            {
                code.Append(@"
            _code_.Add(", member.Path, ".ToString());");
            }
        }
        /// <summary>
        /// 分解成员名称
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <returns>成员名称集合</returns>
        protected static subString[] splitMemberName(string memberName)
        {
            int memberIndex = 0;
            while (memberIndex != memberName.Length && memberName[memberIndex] == '.') ++memberIndex;
            string value = memberName.Substring(0, memberIndex);
            subString[] names = subString.Unsafe(memberName, memberIndex).Split('.').ToArray();
            names[0] = value + names[0];
            return names;
        }
    }
    /// <summary>
    /// 树节点模板
    /// </summary>
    /// <typeparam name="nodeType">树节点类型</typeparam>
    public abstract class template<nodeType> : template where nodeType : template<nodeType>.INode
    {
        /// <summary>
        /// 模板代码节点接口
        /// </summary>
        public interface INode
        {
            /// <summary>
            /// 模板命令
            /// </summary>
            string TemplateCommand { get; }
            /// <summary>
            /// 模板成员名称
            /// </summary>
            string TemplateMemberName { get; }
            /// <summary>
            /// 模板文本代码
            /// </summary>
            string TemplateCode { get; }
            /// <summary>
            /// 子节点数量
            /// </summary>
            int ChildCount { get; }
            /// <summary>
            /// 子节点集合
            /// </summary>
            IEnumerable<nodeType> Childs { get; }
        }
        /// <summary>
        /// 模板command+解析器
        /// </summary>
        protected Dictionary<hashString, Action<nodeType>> creators = dictionary.CreateHashString<Action<nodeType>>();
        /// <summary>
        /// 引用代码树节点
        /// </summary>
        protected Dictionary<hashString, nodeType> nameNodes = dictionary.CreateHashString<nodeType>();
        /// <summary>
        /// 树节点模板
        /// </summary>
        /// <param name="type">模板数据视图</param>
        /// <param name="onError">错误处理委托</param>
        /// <param name="onMessage">消息处理委托</param>
        protected template(Type type, Action<string> onError, Action<string> onMessage)
        {
            this.onError = onError;
            this.onMessage = onMessage;
            currentMembers.Add(new memberNode(this, viewType = type ?? GetType(), null, "this"));
        }
        /// <summary>
        /// 检测成员名称
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        protected virtual string checkMemberName(string memberName)
        {
            return memberName;
        }
        /// <summary>
        /// 添加代码树节点
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void skin(nodeType node)
        {
            Action<nodeType> creator;
            foreach (nodeType son in node.Childs)
            {
                string command = son.TemplateCommand;
                if (command == null) pushCode(son.TemplateCode);
                else if (creators.TryGetValue(command, out creator)) creator(son);
                else onError(viewType.fullName() + " 未找到命名处理函数 " + command + " : " + son.TemplateMemberName);
            }
        }
        /// <summary>
        /// 输出绑定的数据
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected virtual void at(nodeType node)
        {
            bool isDepth;
            string memberName = node.TemplateMemberName;
            memberNode member = getMember(memberName, out isDepth);
            if (member != null) ifThen(member, memberName, isDepth, value => at(value));
        }
        /// <summary>
        /// 注释处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void note(nodeType node)
        {
        }
        /// <summary>
        /// 循环处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="node">代码树节点</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void loop(memberNode member, nodeType node, string name, int popCount)
        {
            code.Append(@"
                    ", name, " = ", member.Path, ";");
            if (popCount != 0) currentMembers.Pop();
            while (popCount != 0)
            {
                ifEnd(true);
                --popCount;
            }
            if (member.Type.EnumerableType == null)
            {
                onError(viewType.fullName() + " 属性不可枚举 " + currentMembers.lastOrDefault().FullPath);
                return;
            }
            pushMember(member);
            string valueName = path(currentMembers.Count);
            if (!member.Type.Type.IsValueType)
            {
                code.Append(@"
                    if (", name, @" != null)");
            }
            code.Append(@"
                    {
                        int ", loopIndex(0), @" = _loopIndex_, ", loopCount(0), @" = _loopCount_;");
            if (isLoopValue)
            {
                code.Append(@"
                        var ", loopValues(0), @" = _loopValues_, ", loopValue(0), @" = _loopValue_;
                        _loopValues_ = ", name, ";");
            }
            code.Append(@"
                        _loopIndex_ = 0;
                        _loopCount_ = ", name, member.Type.Type.IsArray ? ".Length" : ".count()", @";
                        foreach (", member.Type.EnumerableArgumentType.FullName, " " + valueName + " in ", name, @")
                        {");
            if (isLoopValue)
            {
                code.Append(@"
                            _loopValue_ = ", valueName, ";");
            }
            memberNode loopMember = member.Get(string.Empty);
            loopMember.Path = valueName;
            pushMember(loopMember);
            skin(node);
            currentMembers.Pop();
            pushCode(null);
            code.Append(@"
                            ++_loopIndex_;
                        }
                        _loopIndex_ = ", loopIndex(0), @";
                        _loopCount_ = ", loopCount(0), @";");
            if (isLoopValue)
            {
                code.Append(@"
                        _loopValue_ = ", loopValue(0), @";
                        _loopValues_ = ", loopValues(0), ";");
            }
            code.Append(@"
                    }");
            currentMembers.Pop();
        }
        /// <summary>
        /// 循环处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void loop(nodeType node)
        {
            bool isDepth;
            string memberName = checkMemberName(node.TemplateMemberName);
            memberNode member = getMember(memberName, out isDepth);
            if (member == null) return;
            pushCode(null);
            string name = path(currentMembers.Count);
            if (isDepth)
            {
                code.Append(@"
                {
                    ", member.Type.FullName, " ", name, " = default(", member.Type.FullName, ");");
                subString[] names = splitMemberName(memberName);
                ifStart(names[0], true);
                for (int index = 1; index != names.Length - 1; ++index) ifStart(names[index], false);
                loop(getMember(names[names.Length - 1], out isDepth), node, name, names.Length - 1);
            }
            else
            {
                code.Append(@"
                {
                    ", member.Type.FullName, " ", name, ";");
                loop(member, node, name, 0);
            }
            code.Append(@"
                }");
        }
        /// <summary>
        /// 子代码段处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="node">代码树节点</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void push(memberNode member, nodeType node, string name, int popCount)
        {
            code.Append(@"
                    ", name, " = ", member.Path, ";");
            if (popCount != 0) currentMembers.Pop();
            while (popCount != 0)
            {
                ifEnd(true);
                --popCount;
            }
            pushMember(member);
            code.Append(@"
            ", ifName, " = false;");
            ifThen(node, member.Type, name, ifName, false, 0);
            currentMembers.Pop();
        }
        /// <summary>
        /// 子代码段处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void push(nodeType node)
        {
            bool isDepth;
            string memberName = checkMemberName(node.TemplateMemberName);
            memberNode member = getMember(memberName, out isDepth);
            if (member != null && node.ChildCount != 0)
            {
                pushCode(null);
                string name = path(currentMembers.Count);
                code.Append(@"
                {
                    ", member.Type.FullName, " ", name, " = default(", member.Type.FullName, ");");
                if (isDepth)
                {
                    subString[] names = splitMemberName(memberName);
                    ifStart(names[0], true);
                    for (int index = 1; index != names.Length - 1; ++index) ifStart(names[index], false);
                    push(getMember(names[names.Length - 1], out isDepth), node, name, names.Length - 1);
                }
                else push(member, node, name, 0);
                code.Append(@"
                }");
            }
        }
        /// <summary>
        /// if代码段处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        /// <param name="type">成员类型</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="ifName">逻辑变量名称</param>
        /// <param name="isMember">是否删除当前成员节点</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void ifThen(nodeType node, memberType type, string name, string ifName, bool isMember, int popCount)
        {
            ifStart(type, name, ifName);
            while (popCount != 0)
            {
                ifEnd(true);
                --popCount;
            }
            if (isMember) currentMembers.Pop();
            code.Append(@"
                }
            if (", ifName, @")
            {");
            skin(node);
            pushCode(null);
            code.Append(@"
            }");
        }
        /// <summary>
        /// if代码段处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        /// <param name="type">成员类型</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="value">匹配值</param>
        /// <param name="ifName">逻辑变量名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void ifThen(nodeType node, memberType type, string name, string value, string ifName, int popCount)
        {
            if (type.IsStruct || type.Type.IsEnum)
            {
                code.Append(@"
                if (", name, @".ToString() == @""", value.Replace(@"""", @""""""), @""")");
            }
            else
            {
                code.Append(@"
                if (", name, @" != null && ", name, @".ToString() == @""", value.Replace(@"""", @""""""), @""")");
            }
            code.Append(@"
                {
                    ", ifName, @" = true;
                }");
            while (popCount != 0)
            {
                ifEnd(true);
                --popCount;
            }
            code.Append(@"
            if (", ifName, @")
            {");
            skin(node);
            pushCode(null);
            code.Append(@"
            }");
        }
        /// <summary>
        /// if代码段处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="node">代码树节点</param>
        /// <param name="value">匹配值</param>
        /// <param name="ifName">逻辑变量名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void ifThen(memberNode member, nodeType node, string value, string ifName, int popCount)
        {
            if (value == null) ifThen(node, member.Type, member.Path, ifName, false, popCount);
            else ifThen(node, member.Type, member.Path, value, ifName, popCount);
        }
        /// <summary>
        /// 绑定的数据为true非0非null时输出代码
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void ifThen(nodeType node)
        {
            string value = null, memberName = node.TemplateMemberName;
            int valueIndex = memberName.IndexOf('=');
            if (valueIndex != -1)
            {
                value = memberName.Substring(valueIndex + 1);
                memberName = memberName.Substring(0, valueIndex);
            }
            bool isDepth;
            memberNode member = getMember(memberName = checkMemberName(memberName), out isDepth);
            if (member == null) return;
            pushCode(null);
            code.Append(@"
            ", ifName, " = false;");
            if (isDepth)
            {
                subString[] names = splitMemberName(memberName);
                for (int index = 0; index != names.Length - 1; ++index) ifStart(names[index], false);
                ifThen(getMember(names[names.Length - 1], out isDepth), node, value, ifName, names.Length - 1);
            }
            else ifThen(member, node, value, ifName, 0);
        }
        /// <summary>
        /// not代码段处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="node">代码树节点</param>
        /// <param name="value">匹配值</param>
        /// <param name="ifName">逻辑变量名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void not(memberNode member, nodeType node, string value, string ifName, int popCount)
        {
            if (member.Type.IsStruct || member.Type.Type.IsEnum)
            {
                if (value != null)
                {
                    code.Append(@"
                if (", member.Path, @".ToString() != @""", value.Replace(@"""", @""""""), @""")");
                }
                else if (member.Type.IsBool)
                {
                    code.Append(@"
                if (!(bool)", member.Path, ")");
                }
                else if (member.Type.IsAjaxToString)
                {
                    code.Append(@"
                if (", member.Path, " == 0)");
                }
            }
            else if (value != null)
            {
                code.Append(@"
                if (", member.Path, @" == null || ", member.Path, @".ToString() != @""", value.Replace(@"""", @""""""), @""")");
            }
            else
            {
                code.Append(@"
                if (", member.Path, " == null)");
            }
            code.Append(@"
                {
                    ", ifName, @" = true;
                }");
            while (popCount != 0)
            {
                ifEnd(true);
                --popCount;
            }
            code.Append(@"
            if (", ifName, @")
            {");
            skin(node);
            pushCode(null);
            code.Append(@"
            }");
        }
        /// <summary>
        /// 绑定的数据为false或者0或者null时输出代码
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void not(nodeType node)
        {
            string value = null, memberName = node.TemplateMemberName;
            int valueIndex = memberName.IndexOf('=');
            if (valueIndex != -1)
            {
                value = memberName.Substring(valueIndex + 1);
                memberName = memberName.Substring(0, valueIndex);
            }
            bool isDepth;
            memberNode member = getMember(memberName = checkMemberName(memberName), out isDepth);
            if (member == null) return;
            pushCode(null);
            code.Append(@"
            ", ifName, " = false;");
            if (isDepth)
            {
                subString[] names = splitMemberName(memberName);
                for (int index = 0; index != names.Length - 1; ++index) ifStart(names[index], false);
                not(getMember(names[names.Length - 1], out isDepth), node, value, ifName, names.Length - 1);
            }
            else not(member, node, value, ifName, 0);
        }
        /// <summary>
        /// 根据类型名称获取子段模板
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <param name="name">子段模板名称</param>
        /// <returns>子段模板</returns>
        protected virtual nodeType fromNameNode(string typeName, string name)
        {
            return default(nodeType);
        }
#if MONO
#else
        /// <summary>
        /// 子段模板处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void name(nodeType node)
        {
            hashString nameKey = node.TemplateMemberName;
            if (nameNodes.ContainsKey(nameKey)) onError(viewType.fullName() + " NAME " + nameKey.ToString() + " 重复定义");
            nameNodes[nameKey] = node;
            if (node.ChildCount != 0) skin(node);
        }
        /// <summary>
        /// 引用子段模板处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void fromName(nodeType node)
        {
            string memberName = node.TemplateMemberName;
            int typeIndex = memberName.IndexOf('.');
            if (typeIndex == -1)
            {
                if (!nameNodes.TryGetValue(memberName, out node)) onError(viewType.fullName() + " NAME " + memberName + " 未定义");
            }
            else
            {
                node = fromNameNode(memberName.Substring(0, typeIndex), memberName.Substring(++typeIndex));
            }
            if (node != null && node.ChildCount != 0) skin(node);
        }
#endif
        /// <summary>
        /// 子段程序代码处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void part(nodeType node)
        {
            string memberName = node.TemplateMemberName;
            pushCode(null);
            code.Add(@"
            stringBuilder _PART_" + memberName + @"_ = _code_;
            _code_ = new stringBuilder();");
            stringBuilder historyCode = code;
            code = new stringBuilder();
            skin(node);
            pushCode(null);
            string partCode = code.ToString();
            partCodes[memberName] = partCode;
            code = historyCode;
            code.Add(partCode);
            code.Add(@"
            _partCodes_[""" + memberName + @"""] = _code_.ToString();
            _code_ = _PART_" + memberName + @"_;
            _code_.Add(_partCodes_[""" + memberName + @"""]);");
        }
    }
}
