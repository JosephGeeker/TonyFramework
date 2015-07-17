//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: JsonNodeStruct
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  JsonNodeStruct
//	User name:  C1400008
//	Location Time: 2015/7/13 14:40:28
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

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// Json节点
    /// </summary>
    public struct JsonNodeStruct
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public enum TypeEnum : byte
        {
            /// <summary>
            /// 空值
            /// </summary>
            Null,
            /// <summary>
            /// 字符串
            /// </summary>
            String,
            /// <summary>
            /// 未解析字符串
            /// </summary>
            QuoteString,
            /// <summary>
            /// 数字字符串
            /// </summary>
            NumberString,
            /// <summary>
            /// 非数值
            /// </summary>
            NaN,
            /// <summary>
            /// 时间周期值
            /// </summary>
            DateTimeTick,
            /// <summary>
            /// 逻辑值
            /// </summary>
            Bool,
            /// <summary>
            /// 列表
            /// </summary>
            List,
            /// <summary>
            /// 字典
            /// </summary>
            Dictionary,
        }
        /// <summary>
        /// 64位整数值
        /// </summary>
        internal long Int64;
        /// <summary>
        /// 字典
        /// </summary>
        private KeyValueStruct<JsonNodeStruct, JsonNodeStruct>[] _dictionary;
        /// <summary>
        /// 字典
        /// </summary>
        internal SubArrayStruct<KeyValueStruct<JsonNodeStruct, JsonNodeStruct>> Dictionary
        {
            get
            {
                return SubArrayStruct<KeyValueStruct<JsonNodeStruct, JsonNodeStruct>>.Unsafe(_dictionary, 0, (int)Int64);
            }
        }
        /// <summary>
        /// 根据名称获取JSON节点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonNodeStruct this[string name]
        {
            get
            {
                for (int index = 0, count = (int)Int64; index != count; ++index)
                {
                    if (_dictionary[index].Key.KeyName.Equals(name)) return _dictionary[index].Value;
                }
                return default(JsonNodeStruct);
            }
        }
        /// <summary>
        /// 字典名称
        /// </summary>
        private SubStringStruct KeyName
        {
            get
            {
                if (Type == TypeEnum.QuoteString)
                {
                    String = jsonParser.ParseQuoteString(String, (int)(Int64 >> 32), (char)Int64, (int)Int64 >> 16);
                    Type = TypeEnum.String;
                }
                return String;
            }
        }
        /// <summary>
        /// 列表
        /// </summary>
        private JsonNodeStruct[] _list;
        /// <summary>
        /// 列表
        /// </summary>
        public SubArrayStruct<JsonNodeStruct> List
        {
            get
            {
                return Type == TypeEnum.List ? SubArrayStruct<JsonNodeStruct>.Unsafe(_list, 0, (int)Int64) : default(SubArrayStruct<JsonNodeStruct>);
            }
        }
        /// <summary>
        /// 字典或列表数据量
        /// </summary>
        public int Count
        {
            get
            {
                return Type == TypeEnum.Dictionary || Type == TypeEnum.List ? (int)Int64 : 0;
            }
        }
        /// <summary>
        /// 字符串
        /// </summary>
        internal SubStringStruct String;
        /// <summary>
        /// 类型
        /// </summary>
        public TypeEnum Type { get; internal set; }
        /// <summary>
        /// 设置列表
        /// </summary>
        /// <param name="list">列表</param>
        internal void SetList(SubArrayStruct<JsonNodeStruct> list)
        {
            this._list = list.array;
            Int64 = list.Count;
            Type = TypeEnum.List;
        }
        /// <summary>
        /// 设置字典
        /// </summary>
        /// <param name="dictionary">字典</param>
        internal void SetDictionary(SubArrayStruct<KeyValueStruct<JsonNodeStruct, JsonNodeStruct>> dictionary)
        {
            this._dictionary = dictionary.array;
            Int64 = dictionary.Count;
            Type = TypeEnum.Dictionary;
        }

        /// <summary>
        /// 未解析字符串
        /// </summary>
        /// <param name="escapeIndex">未解析字符串起始位置</param>
        /// <param name="quote">字符串引号</param>
        /// <param name="isTempString"></param>
        internal void SetQuoteString(int escapeIndex, char quote, bool isTempString)
        {
            Type = TypeEnum.QuoteString;
            Int64 = ((long)escapeIndex << 32) + quote;
            if (isTempString) Int64 += 0x10000;
        }
        /// <summary>
        /// 设置数字字符串
        /// </summary>
        /// <param name="quote"></param>
        internal void SetNumberString(char quote)
        {
            Int64 = quote;
            Type = TypeEnum.NumberString;
        }
        /// <summary>
        /// 创建字典节点
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static JsonNodeStruct CreateDictionary(SubArrayStruct<KeyValueStruct<SubStringStruct, JsonNodeStruct>> dictionary)
        {
            var node = new JsonNodeStruct { Type = TypeEnum.Dictionary };
            node.SetDictionary(dictionary.GetArray(value => new KeyValueStruct<JsonNodeStruct, JsonNodeStruct>(new JsonNodeStruct { Type = TypeEnum.String, String = value.Key }, value.Value)).ToSubArray());
            return node;
        }
    }
}
