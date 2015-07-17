//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: CsharpPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  CsharpPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 16:42:20
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// C#代码生成扩展
    /// </summary>
    public static class CsharpPlus
    {
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        public unsafe static string joinString<valueType>(this valueType[] array, char join, bool isNull = false)
        {
            return fastCSharp.emit.numberToCharStream<valueType>.JoinString(array, join, isNull);
        }
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        public unsafe static string joinString<valueType>(this subArray<valueType> array, char join, bool isNull = false)
        {
            return fastCSharp.emit.numberToCharStream<valueType>.JoinString(array, join, isNull);
        }
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="stream">字符流</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        public unsafe static void joinString(this int[] array, charStream stream, char join, bool isNull = false)
        {
            if (array.length() == 0)
            {
                if (isNull) fastCSharp.web.ajax.WriteNull(stream);
                return;
            }
            fastCSharp.emit.numberToCharStream<int>.NumberJoinChar(stream, array, 0, array.Length, join, isNull);
        }
        /// <summary>
        /// 连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="stream">字符流</param>
        /// <param name="join">字符连接</param>
        /// <returns>连接后的字符串</returns>
        public unsafe static void joinString(this subArray<int> array, charStream stream, char join, bool isNull = false)
        {
            if (array.Count == 0)
            {
                if (isNull) fastCSharp.web.ajax.WriteNull(stream);
                return;
            }
            fastCSharp.emit.numberToCharStream<int>.NumberJoinChar(stream, array.array, array.StartIndex, array.Count, join, isNull);
        }

        /// <summary>
        /// 创建成员位图
        /// </summary>
        /// <typeparam name="valueType">表格模型类型</typeparam>
        /// <param name="sqlTable"></param>
        /// <returns></returns>
        public static fastCSharp.code.memberMap<valueType>.builder CreateMemberMap<valueType>(this fastCSharp.emit.sqlTable.sqlTool<valueType> sqlTable)
            where valueType : class
        {
            return new fastCSharp.code.memberMap<valueType>.builder(sqlTable != null);
        }
        /// <summary>
        /// 判断成员索引是否有效
        /// </summary>
        /// <typeparam name="returnType"></typeparam>
        /// <param name="memberMap"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public static fastCSharp.code.memberMap.memberIndex CreateMemberIndex<valueType, returnType>(this fastCSharp.emit.sqlTable.sqlTool<valueType> sqlTable, Expression<Func<valueType, returnType>> member)
            where valueType : class
        {
            return fastCSharp.code.memberMap<valueType>.CreateMemberIndex(member);
        }
        /// <summary>
        /// 创建成员位图
        /// </summary>
        /// <typeparam name="valueType">表格模型类型</typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static fastCSharp.code.memberMap<valueType>.builder CreateMemberMap<valueType>(this fastCSharp.emit.memoryDatabaseTable.table<valueType> table)
            where valueType : class
        {
            return new fastCSharp.code.memberMap<valueType>.builder(table != null);
        }

        /// <summary>
        /// 对象成员复制
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <typeparam name="baseType"></typeparam>
        /// <param name="value"></param>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        public static valueType Copy<valueType, baseType>(this baseType value, fastCSharp.code.memberMap memberMap = null)
            where valueType : class, baseType
        {
            if (value == null) return null;
            valueType newValue = fastCSharp.emit.constructor<valueType>.New();
            fastCSharp.emit.memberCopyer<baseType>.Copy(newValue, value, memberMap);
            return newValue;
        }
        /// <summary>
        /// 对象成员复制
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        public static valueType Copy<valueType>(this valueType value, fastCSharp.code.memberMap memberMap = null)
            where valueType : class
        {
            if (value == null) return null;
            valueType newValue = fastCSharp.emit.constructor<valueType>.New();
            fastCSharp.emit.memberCopyer<valueType>.Copy(newValue, value, memberMap);
            return newValue;
        }

        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="jsonStream">Json输出缓冲区</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToJson<valueType>(this valueType value, charStream jsonStream, fastCSharp.emit.jsonSerializer.config config = null)
        {
            fastCSharp.emit.jsonSerializer.ToJson(value, jsonStream, config);
        }
        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>Json字符串</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToJson<valueType>(this valueType value, fastCSharp.emit.jsonSerializer.config config = null)
        {
            return fastCSharp.emit.jsonSerializer.ToJson(value, config);
        }
        /// <summary>
        /// Json解析
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">目标数据</param>
        /// <param name="json">Json字符串</param>
        /// <param name="config">配置参数</param>
        /// <returns>是否解析成功</returns>
        public static bool FromJson<valueType>(this valueType value, subString json, fastCSharp.emit.jsonParser.config config = null)
        {
            return fastCSharp.emit.jsonParser.Parse(json, ref value, config);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="stream">序列化输出缓冲区</param>
        /// <param name="config">配置参数</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<valueType>(this valueType value, unmanagedStream stream, fastCSharp.emit.dataSerializer.config config = null) where valueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            fastCSharp.emit.dataSerializer.CodeSerialize(value, stream, config);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="valueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="config">配置参数</param>
        /// <returns>序列化数据</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Serialize<valueType>(this valueType value, fastCSharp.emit.dataSerializer.config config = null) where valueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            return fastCSharp.emit.dataSerializer.CodeSerialize(value, config);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="data"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        public static bool DeSerialize<valueType>(this valueType value, byte[] data, fastCSharp.emit.dataDeSerializer.config config = null) where valueType : fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            return fastCSharp.emit.dataDeSerializer.CodeDeSerialize(data, ref value, config);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="data"></param>
        /// <param name="config"></param>
        /// <returns>是否成功</returns>
        public static bool DeSerialize<valueType>(this valueType value, subArray<byte> data, fastCSharp.emit.dataDeSerializer.config config = null) where valueType : class, fastCSharp.code.cSharp.dataSerialize.ISerialize
        {
            return fastCSharp.emit.dataDeSerializer.CodeDeSerialize(data, ref value, config);
        }
    }
}
