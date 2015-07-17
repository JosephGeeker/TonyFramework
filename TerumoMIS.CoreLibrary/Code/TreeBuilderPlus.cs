//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TreeBuilderPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code
//	File Name:  TreeBuilderPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:31:45
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
    /// 建树器
    /// </summary>
    /// <typeparam name="TNodeType">树节点类型</typeparam>
    /// <typeparam name="TTagType">树节点标识类型</typeparam>
    public sealed class TreeBuilderPlus<TNodeType,TTagType>
        where TNodeType:TreeBuilderPlus<TNodeType,TTagType>.INode
        where TTagType:IEquatable<TTagType>
    {
        /// <summary>
        /// 节点接口
        /// </summary>
        public interface INode
        {
            /// <summary>
            /// 树节点标识
            /// </summary>
            TTagType Tag { get; }
            /// <summary>
            /// 设置子节点集合
            /// </summary>
            /// <param name="childs">子节点集合</param>
            void SetChilds(TNodeType[] childs);
        }
        /// <summary>
        /// 当前节点集合
        /// </summary>
        private list<keyValue<TNodeType, bool>> nodes = new list<keyValue<TNodeType, bool>>();
        ///// <summary>
        ///// 树节点回合检测器,必须回合返回true
        ///// </summary>
        //private func<nodeType, bool> checkRound;
        /// <summary>
        /// 建树器
        /// </summary>
        public treeBuilder() { }
        ///// <summary>
        ///// 建树器
        ///// </summary>
        ///// <param name="checkRound">树节点回合检测器,必须回合返回true</param>
        //public treeBuilder(func<nodeType, bool> checkRound)
        //{
        //    this.checkRound = checkRound;
        //}
        /// <summary>
        /// 清除节点
        /// </summary>
        public void Empty()
        {
            nodes.Empty();
        }
        /// <summary>
        /// 追加新节点
        /// </summary>
        /// <param name="node">新节点</param>
        public void Append(TNodeType node)
        {
            nodes.Add(new keyValue<TNodeType, bool>(node, true));
        }
        /// <summary>
        /// 追加新节点
        /// </summary>
        /// <param name="node">新节点</param>
        /// <param name="isRound">是否需要判断回合</param>
        public void Append(TNodeType node, bool isRound)
        {
            nodes.Add(new keyValue<TNodeType, bool>(node, isRound));
        }
        /// <summary>
        /// 检测节点回合状态
        /// </summary>
        private enum checkType
        {
            /// <summary>
            /// 节点回合成功
            /// </summary>
            Ok,
            /// <summary>
            /// 缺少回合节点
            /// </summary>
            LessRound,
            /// <summary>
            /// 未知的回合节点
            /// </summary>
            UnknownRound,
        }
        /// <summary>
        /// 节点回合
        /// </summary>
        /// <param name="tagName">树节点标识</param>
        /// <returns>节点回合状态</returns>
        private checkType round(TTagType tagName, bool isAny)
        {
            keyValue<TNodeType, bool>[] array = nodes.array;
            for (int index = nodes.Count; index != 0; )
            {
                if (array[--index].Value)
                {
                    TNodeType node = array[index].Key;
                    if (tagName.Equals(node.Tag))
                    {
                        array[index].Set(node, false);
                        node.SetChilds(array.sub(++index, nodes.Count - index).GetArray(value => value.Key));
                        nodes.Unsafer.AddLength(index - nodes.Count);
                        return checkType.Ok;
                    }
                    else if (!isAny) return checkType.LessRound;
                    //else if (checkRound != null && checkRound(node)) return checkType.LessRound;
                }
            }
            return checkType.UnknownRound;
        }
        /// <summary>
        /// 节点回合
        /// </summary>
        /// <param name="tagName">树节点标识</param>
        public void Round(TTagType tagName)
        {
            checkType check = round(tagName, false);
            switch (check)
            {
                case checkType.LessRound:
                    log.Error.Throw("缺少回合节点 : " + tagName.ToString() + @"
" + nodes.JoinString(@"
", value => value.Key.Tag.ToString()), false, false);
                    break;
                case checkType.UnknownRound:
                    log.Error.Throw("未知的回合节点 : " + tagName.ToString() + @"
" + nodes.JoinString(@"
", value => value.Key.Tag.ToString()), false, false);
                    break;
            }
        }
        /// <summary>
        /// 节点回合
        /// </summary>
        /// <param name="tagName">树节点标识</param>
        /// <param name="isAny">是否匹配任意索引位置,否则只能匹配最后一个索引位置</param>
        /// <returns>节点回合是否成功</returns>
        public bool IsRound(TTagType tagName, bool isAny)
        {
            return round(tagName, isAny) == checkType.Ok;
        }
        /// <summary>
        /// 建树结束
        /// </summary>
        /// <returns>根节点集合</returns>
        public TNodeType[] End()
        {
            //if (checkRound != null)
            //{
                keyValue<TNodeType, bool>[] array = nodes.array;
                for (int index = nodes.Count; index != 0; )
                {
                    //if (array[--index].Value && checkRound(array[index].Key))
                    if (array[--index].Value)
                    {
                        log.Error.Throw("缺少回合节点 : " + nodes.JoinString(@" \ ", value => value.Key.Tag.ToString()), false, false);
                    }
                }
            //}
            return nodes.GetArray(value => value.Key);
        }
    }
}
