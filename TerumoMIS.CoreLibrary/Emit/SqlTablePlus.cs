//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: SqlTablePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  SqlTablePlus
//	User name:  C1400008
//	Location Time: 2015/7/13 15:12:53
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
    /// 数据库表格配置
    /// </summary>
    public class SqlTablePlus:Attribute
    {
        /// <summary>
        /// SQL表格操作工具 字段名称
        /// </summary>
        public const string SqlTableName = "SqlTable";
        /// <summary>
        /// 链接类型
        /// </summary>
        public string ConnectionName;
        /// <summary>
        /// 链接类型
        /// </summary>
        public virtual string ConnectionType
        {
            get { return ConnectionName; }
        }
        /// <summary>
        /// 表格名称
        /// </summary>
        public string TableName;
        /// <summary>
        /// 写操作是否加锁
        /// </summary>
        public bool IsLockWrite = true;
        /// <summary>
        /// 是否自动获取自增标识
        /// </summary>
        public bool IsLoadIdentity = true;
        /// <summary>
        /// 获取表格名称
        /// </summary>
        /// <param name="type">表格绑定类型</param>
        /// <returns>表格名称</returns>
        internal unsafe string GetTableName(Type type)
        {
            if (TableName != null) return TableName;
            string name = null;
            if (fastCSharp.config.sql.Default.TableNamePrefixs.Length != 0)
            {
                name = type.fullName();
                foreach (string perfix in fastCSharp.config.sql.Default.TableNamePrefixs)
                {
                    if (name.Length > perfix.Length && name.StartsWith(perfix, StringComparison.Ordinal) && name[perfix.Length] == '.')
                    {
                        return name.Substring(perfix.Length + 1);
                    }
                }
            }
            int depth = fastCSharp.config.sql.Default.TableNameDepth;
            if (depth <= 0) return type.Name;
            if (name == null) name = type.fullName();
            fixed (char* nameFixed = name)
            {
                char* start = nameFixed, end = nameFixed + name.Length;
                do
                {
                    while (start != end && *start != '.') ++start;
                    if (start == end) return type.Name;
                    ++start;
                }
                while (--depth != 0);
                int index = (int)(start - nameFixed);
                while (start != end)
                {
                    if (*start == '.') *start = '_';
                    ++start;
                }
                return name.Substring(index);
            }
        }
        /// <summary>
        /// 取消确认
        /// </summary>
        public sealed class cancel
        {
            /// <summary>
            /// 是否取消
            /// </summary>
            private bool isCancel;
            /// <summary>
            /// 是否取消
            /// </summary>
            public bool IsCancel
            {
                get { return isCancel; }
                set { if (value) isCancel = true; }
            }
        }
        /// <summary>
        /// 更新数据表达式
        /// </summary>
        /// <typeparam name="modelType"></typeparam>
        public struct updateExpression
        {
            /// <summary>
            /// SQL表达式集合
            /// </summary>
            private subArray<keyValue<string, string>> values;
            /// <summary>
            /// SQL表达式数量
            /// </summary>
            internal int Count
            {
                get { return values.Count; }
            }
            /// <summary>
            /// 添加SQL表达式
            /// </summary>
            /// <param name="value"></param>
            internal void Add(keyValue<string, string> value)
            {
                values.Add(value);
            }
            /// <summary>
            /// 更新数据成员位图
            /// </summary>
            /// <typeparam name="modelType"></typeparam>
            /// <returns></returns>
            internal fastCSharp.code.memberMap CreateMemberMap<modelType>()
            {
                fastCSharp.code.memberMap memberMap = fastCSharp.code.memberMap<modelType>.New();
                foreach (keyValue<string, string> value in values.array)
                {
                    if (value.Key == null) break;
                    if (!memberMap.SetMember(value.Key))
                    {
                        memberMap.Dispose();
                        log.Error.Throw(typeof(modelType).fullName() + " 找不到SQL字段 " + value.Key, false, true);
                    }
                }
                return memberMap;
            }
            /// <summary>
            /// 数据更新SQL流
            /// </summary>
            /// <param name="sqlStream"></param>
            internal void Update(charStream sqlStream)
            {
                int isNext = 0;
                foreach (keyValue<string, string> value in values.array)
                {
                    if (value.Key == null) break;
                    if (isNext == 0) isNext = 1;
                    else sqlStream.Write(',');
                    sqlStream.WriteNotNull(value.Key);
                    sqlStream.Write('=');
                    sqlStream.WriteNotNull(value.Value);
                }
            }
        }
        /// <summary>
        /// 数据库表格操作工具
        /// </summary>
        public abstract class sqlTool : IDisposable
        {
            /// <summary>
            /// 自增ID生成器
            /// </summary>
            protected long identity64;
            /// <summary>
            /// 当前自增ID
            /// </summary>
            public long Identity64
            {
                get { return identity64; }
                set
                {
                    if (!sqlTable.IsLoadIdentity) identity64 = value;
                }
            }
            /// <summary>
            /// 自增ID
            /// </summary>
            internal long NextIdentity
            {
                get { return Interlocked.Increment(ref identity64); }
            }
            /// <summary>
            /// 数据库表格配置
            /// </summary>
            private sqlTable sqlTable;
            /// <summary>
            /// SQL操作客户端
            /// </summary>
            internal client Client { get; private set; }
            /// <summary>
            /// 表格名称
            /// </summary>
            internal string TableName { get; private set; }
            /// <summary>
            /// SQL访问锁
            /// </summary>
            public readonly object Lock = new object();
            /// <summary>
            /// 待创建一级索引的成员名称集合
            /// </summary>
            protected fastCSharp.stateSearcher.ascii<string> noIndexMemberNames;
            /// <summary>
            /// 缓存加载等待
            /// </summary>
            public readonly EventWaitHandle LoadWait;
            /// <summary>
            /// 数据库表格是否加载成功
            /// </summary>
            protected bool isTable;
            /// <summary>
            /// 数据库表格是否加载成功
            /// </summary>
            public bool IsTable
            {
                get { return isTable; }
            }
            /// <summary>
            /// 是否锁定更新操作
            /// </summary>
            internal bool IsLockWrite
            {
                get { return sqlTable.IsLockWrite; }
            }
            /// <summary>
            /// 成员名称是否忽略大小写
            /// </summary>
            protected bool ignoreCase;
            /// <summary>
            /// 数据库表格操作工具
            /// </summary>
            /// <param name="sqlTable">数据库表格配置</param>
            /// <param name="client">SQL操作客户端</param>
            /// <param name="tableName">表格名称</param>
            protected sqlTool(sqlTable sqlTable, client client, string tableName)
            {
                this.sqlTable = sqlTable;
                this.Client = client;
                TableName = tableName;
                ignoreCase = Enum<fastCSharp.sql.type, fastCSharp.sql.typeInfo>.Array(client.Connection.Type).IgnoreCase;
                LoadWait = new EventWaitHandle(false, EventResetMode.ManualReset);
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public virtual unsafe void Dispose()
            {
                CoreLibrary.PubPlus.Dispose(ref noIndexMemberNames);
            }
            /// <summary>
            /// 创建索引
            /// </summary>
            /// <param name="name">列名称</param>
            internal void CreateIndex(string name)
            {
                createIndex(name, false);
            }
            /// <summary>
            /// 创建索引
            /// </summary>
            /// <param name="name">列名称</param>
            /// <param name="isUnique">是否唯一值</param>
            protected void createIndex(string name, bool isUnique)
            {
                if (ignoreCase) name = name.ToLower();
                if (noIndexMemberNames.ContainsKey(name))
                {
                    bool isIndex = false;
                    Exception exception = null;
                    Monitor.Enter(Lock);
                    try
                    {
                        if (noIndexMemberNames.Remove(name))
                        {
                            isIndex = true;
                            if (Client.CreateIndex(TableName, new columnCollection
                            {
                                Columns = new column[] { new column { Name = name } },
                                Type = isUnique ? columnCollection.type.UniqueIndex : columnCollection.type.Index
                            }, null))
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception error)
                    {
                        exception = error;
                    }
                    finally { Monitor.Exit(Lock); }
                    if (isIndex) log.Error.Add(exception, "索引 " + TableName + "." + name + " 创建失败", false);
                }
            }
            /// <summary>
            /// 字符串验证
            /// </summary>
            /// <param name="memberName">成员名称</param>
            /// <param name="value">成员值</param>
            /// <param name="length">最大长度</param>
            /// <param name="isAscii">是否ASCII</param>
            /// <param name="isNull">是否可以为null</param>
            /// <returns>字符串是否通过默认验证</returns>
            internal unsafe bool StringVerify(string memberName, string value, int length, bool isAscii, bool isNull)
            {
                if (!isNull && value == null)
                {
                    NullVerify(memberName);
                    return false;
                }
                if (length != 0)
                {
                    if (isAscii)
                    {
                        int nextLength = length - value.Length;
                        if (nextLength >= 0 && value.length() > (length >> 1))
                        {
                            fixed (char* valueFixed = value)
                            {
                                for (char* start = valueFixed, end = valueFixed + value.Length; start != end; ++start)
                                {
                                    if ((*start & 0xff00) != 0 && --nextLength < 0) break;
                                }
                            }
                        }
                        if (nextLength < 0)
                        {
                            log.Error.Add(TableName + "." + memberName + " 超长 " + length.toString(), true, true);
                            return false;
                        }
                    }
                    else
                    {
                        if (value.length() > length)
                        {
                            log.Error.Add(TableName + "." + memberName + " 超长 " + value.Length.toString() + " > " + length.toString(), true, false);
                            return false;
                        }
                    }
                }
                return true;
            }
            /// <summary>
            /// 数据库字符串验证函数
            /// </summary>
            internal static readonly MethodInfo StringVerifyMethod = typeof(fastCSharp.emit.sqlTable.sqlTool).GetMethod("StringVerify", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string), typeof(string), typeof(int), typeof(bool), typeof(bool) }, null);
            /// <summary>
            /// 成员值不能为null
            /// </summary>
            /// <param name="memberName">成员名称</param>
            internal void NullVerify(string memberName)
            {
                log.Error.Add(TableName + "." + memberName + " 不能为null", true, true);
            }
            /// <summary>
            /// 数据库字段空值验证
            /// </summary>
            internal static readonly MethodInfo NullVerifyMethod = typeof(fastCSharp.emit.sqlTable.sqlTool).GetMethod("NullVerify", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
        }
        /// <summary>
        /// 数据库表格操作工具
        /// </summary>
        /// <typeparam name="modelType">模型类型</typeparam>
        public abstract class sqlTool<modelType> : sqlTool
            where modelType : class
        {
            /// <summary>
            /// 更新查询SQL数据成员
            /// </summary>
            protected fastCSharp.code.memberMap selectMemberMap = fastCSharp.code.memberMap<modelType>.New();
            /// <summary>
            /// 更新查询SQL数据成员
            /// </summary>
            internal fastCSharp.code.memberMap SelectMemberMap { get { return selectMemberMap; } }
            /// <summary>
            /// 自增字段名称
            /// </summary>
            public string IdentityName
            {
                get
                {
                    fastCSharp.code.cSharp.sqlModel.fieldInfo identity = sqlModel<modelType>.Identity;
                    return identity != null ? identity.Field.Name : null;
                }
            }
            /// <summary>
            /// 数据库表格操作工具
            /// </summary>
            /// <param name="sqlTable">数据库表格配置</param>
            /// <param name="client">SQL操作客户端</param>
            /// <param name="tableName">表格名称</param>
            protected sqlTool(sqlTable sqlTable, client client, string tableName)
                : base(sqlTable, client, tableName)
            {
            }
            /// <summary>
            /// 获取成员位图
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            /// <returns>成员位图</returns>
            internal fastCSharp.code.memberMap GetMemberMapClearIdentity(fastCSharp.code.memberMap memberMap)
            {
                fastCSharp.code.memberMap value = fastCSharp.emit.sqlModel<modelType>.CopyMemberMap;
                if (memberMap != null && !memberMap.IsDefault) value.And(memberMap);
                if (sqlModel<modelType>.Identity != null) value.ClearMember(sqlModel<modelType>.Identity.MemberMapIndex);
                return value;
            }
            /// <summary>
            /// 获取更新查询SQL数据成员
            /// </summary>
            /// <param name="memberMap">查询SQL数据成员</param>
            /// <returns>更新查询SQL数据成员</returns>
            internal fastCSharp.code.memberMap GetSelectMemberMap(fastCSharp.code.memberMap memberMap)
            {
                fastCSharp.code.memberMap value = selectMemberMap.Copy();
                value.Or(memberMap);
                return value;
            }
            /// <summary>
            /// 设置更新查询SQL数据成员
            /// </summary>
            /// <param name="memberIndex">数据成员索引</param>
            public void SetSelectMember<returnType>(Expression<Func<modelType, returnType>> member)
            {
                selectMemberMap.SetMember(member);
            }
            /// <summary>
            /// 数据更新SQL表达式
            /// </summary>
            /// <param name="returnType">表达式类型</param>
            /// <param name="expression">成员表达式</param>
            /// <returns>数据更新SQL表达式</returns>
            public updateExpression UpdateExpression<returnType>(Expression<Func<modelType, returnType>> expression)
            {
                updateExpression value = new updateExpression();
                AddUpdateExpression(ref value, expression);
                return value;
            }
            /// <summary>
            /// 数据更新SQL表达式
            /// </summary>
            /// <param name="returnType">表达式类型</param>
            /// <param name="expression">成员表达式</param>
            /// <returns>数据更新SQL表达式</returns>
            public updateExpression UpdateExpression<returnType>(Expression<Func<modelType, returnType>> expression, returnType updateValue)
            {
                updateExpression value = new updateExpression();
                AddUpdateExpression(ref value, expression, updateValue);
                return value;
            }
            /// <summary>
            /// 数据更新SQL表达式
            /// </summary>
            /// <typeparam name="returnType"></typeparam>
            /// <param name="value"></param>
            /// <param name="expression"></param>
            public void AddUpdateExpression<returnType>(ref updateExpression value, Expression<Func<modelType, returnType>> expression)
            {
                if (expression != null)
                {
                    keyValue<string, string> sql = Client.GetSql(expression);
                    if (sql.Key == null) log.Error.Throw(log.exceptionType.Null);
                    value.Add(sql);
                }
            }
            /// <summary>
            /// 数据更新SQL表达式
            /// </summary>
            /// <typeparam name="returnType"></typeparam>
            /// <param name="value"></param>
            /// <param name="expression"></param>
            public void AddUpdateExpression<returnType>(ref updateExpression value, Expression<Func<modelType, returnType>> expression, returnType updateValue)
            {
                if (expression != null)
                {
                    keyValue<string, string> sql = Client.GetSql(expression);
                    if (sql.Key == null) log.Error.Throw(log.exceptionType.Null);
                    value.Add(new keyValue<string, string>(sql.Key, Client.ToString(updateValue)));
                }
            }
            /// <summary>
            /// 数据更新SQL表达式
            /// </summary>
            /// <typeparam name="returnType"></typeparam>
            /// <param name="value"></param>
            /// <param name="expression"></param>
            public void AddUpdateExpression(ref updateExpression value, Expression<Action<modelType>> expression)
            {
                if (expression != null)
                {
                    keyValue<string, string> sql = Client.GetSql(expression);
                    if (sql.Key == null) log.Error.Throw(log.exceptionType.Null);
                    value.Add(sql);
                }
            }
            /// <summary>
            /// 数据更新SQL表达式
            /// </summary>
            /// <param name="returnType">表达式类型</param>
            /// <param name="expressions">成员表达式集合</param>
            /// <returns>数据更新SQL表达式</returns>
            public updateExpression UpdateExpression<returnType>(params Expression<Func<modelType, returnType>>[] expressions)
            {
                updateExpression value = new updateExpression();
                AddUpdateExpression(ref value, expressions);
                return value;
            }
            /// <summary>
            /// 数据更新SQL表达式
            /// </summary>
            /// <typeparam name="returnType"></typeparam>
            /// <param name="value"></param>
            /// <param name="expressions"></param>
            public void AddUpdateExpression<returnType>(ref updateExpression value, params Expression<Func<modelType, returnType>>[] expressions)
            {
                foreach (Expression<Func<modelType, returnType>> expression in expressions) AddUpdateExpression(ref value, expression);
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public override unsafe void Dispose()
            {
                base.Dispose();
                CoreLibrary.PubPlus.Dispose(ref selectMemberMap);
            }
        }
        /// <summary>
        /// 数据库表格操作工具
        /// </summary>
        /// <typeparam name="valueType">表格类型</typeparam>
        /// <typeparam name="modelType">模型类型</typeparam>
        public abstract class sqlTool<valueType, modelType> : sqlTool<modelType>
            where valueType : class, modelType
            where modelType : class
        {
            /// <summary>
            /// 数据库表格操作工具
            /// </summary>
            /// <param name="sqlTable">数据库表格配置</param>
            /// <param name="client">SQL操作客户端</param>
            /// <param name="tableName">表格名称</param>
            protected sqlTool(sqlTable sqlTable, client client, string tableName)
                : base(sqlTable, client, tableName)
            {
                fastCSharp.sql.connection.WaitCheckConnection(typeof(valueType));
                try
                {
                    table table = client.GetTable(tableName, null);
                    if (table == null)
                    {
                        Type sqlModelType = fastCSharp.code.cSharp.dataModel.GetModelType<fastCSharp.code.cSharp.sqlModel>(typeof(valueType)) ?? typeof(valueType);
                        table memberTable = sqlModel<modelType>.GetTable(typeof(valueType), sqlTable);
                        client.ToSqlColumn(memberTable);
                        if (client.CreateTable(memberTable, null)) table = memberTable;
                    }
                    string[] names = ignoreCase ? table.Columns.Columns.getArray(value => value.Name.ToLower()) : table.Columns.Columns.getArray(value => value.Name);
                    noIndexMemberNames = new stateSearcher.ascii<string>(names, names);
                    if (table.Indexs != null)
                    {
                        foreach (columnCollection column in table.Indexs) noIndexMemberNames.Remove(ignoreCase ? column.Columns[0].Name.ToLower() : column.Columns[0].Name);
                    }
                    if (table.PrimaryKey != null) noIndexMemberNames.Remove(ignoreCase ? table.PrimaryKey.Columns[0].Name.ToLower() : table.PrimaryKey.Columns[0].Name);
                    isTable = true;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, tableName, false);
                }
                if (IsTable)
                {
                    if (sqlModel<modelType>.Identity != null)
                    {
                        string identityName = sqlModel<modelType>.Identity.Field.Name;
                        if (client.IsIndex) createIndex(identityName, true);
                        selectMemberMap.SetMember(identityName);
                        if (sqlTable.IsLoadIdentity)
                        {
                            IConvertible identityConvertible = client.getValue<valueType, modelType, IConvertible>(this, new selectQuery<modelType> { StringOrders = new keyValue<string, bool>[] { new keyValue<string, bool>(identityName, true) } }, identityName, null);
                            identity64 = identityConvertible == null ? 0 : identityConvertible.ToInt64(null);
                        }
                    }
                    foreach (fastCSharp.code.cSharp.sqlModel.fieldInfo field in sqlModel<modelType>.PrimaryKeys)
                    {
                        selectMemberMap.SetMember(field.MemberMapIndex);
                    }
                }
            }
            /// <summary>
            /// 添加数据之前的验证事件
            /// </summary>
            public event Action<valueType, cancel> OnInsert;
            /// <summary>
            /// 添加数据之前的验证事件
            /// </summary>
            /// <param name="value">待插入数据</param>
            /// <returns>是否可插入数据库</returns>
            internal bool CallOnInsert(valueType value)
            {
                if (OnInsert != null)
                {
                    cancel cancel = new cancel();
                    OnInsert(value, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }
            /// <summary>
            /// 添加数据之前的验证事件
            /// </summary>
            /// <param name="values">待插入数据集合</param>
            /// <returns>是否可插入数据库</returns>
            internal bool CallOnInsert(valueType[] values)
            {
                if (OnInsert != null)
                {
                    cancel cancel = new cancel();
                    foreach (valueType value in values)
                    {
                        OnInsert(value, cancel);
                        if (cancel.IsCancel) return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 添加数据之前的验证事件
            /// </summary>
            public event Action<valueType, cancel> OnInsertLock;
            /// <summary>
            /// 添加数据之前的验证事件
            /// </summary>
            /// <param name="value">待插入数据</param>
            /// <returns>是否可插入数据库</returns>
            internal bool CallOnInsertLock(valueType value)
            {
                if (OnInsertLock != null)
                {
                    cancel cancel = new cancel();
                    OnInsertLock(value, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }
            /// <summary>
            /// 添加数据之前的验证事件
            /// </summary>
            /// <param name="values">待插入数据集合</param>
            /// <returns>是否可插入数据库</returns>
            internal bool CallOnInsertLock(valueType[] values)
            {
                if (OnInsertLock != null)
                {
                    cancel cancel = new cancel();
                    foreach (valueType value in values)
                    {
                        OnInsertLock(value, cancel);
                        if (cancel.IsCancel) return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// 添加数据之后的处理事件
            /// </summary>
            public event Action<valueType> OnInsertedLock;
            /// <summary>
            /// 添加数据之后的处理事件
            /// </summary>
            /// <param name="value">被插入的数据</param>
            internal void CallOnInsertedLock(valueType value)
            {
                if (OnInsertedLock != null) OnInsertedLock(value);
            }
            /// <summary>
            /// 添加数据之后的处理事件
            /// </summary>
            public event Action<valueType> OnInserted;
            /// <summary>
            /// 添加数据之后的处理事件
            /// </summary>
            /// <param name="value">被插入的数据</param>
            internal void CallOnInserted(valueType value)
            {
                if (OnInserted != null) OnInserted(value);
            }
            /// <summary>
            /// 更新数据之前的验证事件
            /// </summary>
            public event Action<valueType, fastCSharp.code.memberMap, cancel> OnUpdate;
            /// <summary>
            /// 更新数据之前的验证事件
            /// </summary>
            /// <param name="value">待更新数据</param>
            /// <param name="memberMap">更新成员位图</param>
            /// <returns>是否可更新数据库</returns>
            internal bool CallOnUpdate(valueType value, fastCSharp.code.memberMap memberMap)
            {
                if (OnUpdate != null)
                {
                    cancel cancel = new cancel();
                    OnUpdate(value, memberMap, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }
            /// <summary>
            /// 更新数据之前的验证事件
            /// </summary>
            public event Action<valueType, fastCSharp.code.memberMap, cancel> OnUpdateLock;
            /// <summary>
            /// 更新数据之前的验证事件
            /// </summary>
            /// <param name="value">待更新数据</param>
            /// <param name="memberMap">更新成员位图</param>
            /// <returns>是否可更新数据库</returns>
            internal bool CallOnUpdateLock(valueType value, fastCSharp.code.memberMap memberMap)
            {
                if (OnUpdateLock != null)
                {
                    cancel cancel = new cancel();
                    OnUpdateLock(value, memberMap, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }
            /// <summary>
            /// 更新数据之后的处理事件
            /// </summary>
            public event Action<valueType, valueType, fastCSharp.code.memberMap> OnUpdatedLock;
            /// <summary>
            /// 更新数据之后的处理事件
            /// </summary>
            /// <param name="value">更新后的数据</param>
            /// <param name="oldValue">更新前的数据</param>
            /// <param name="memberMap">更新成员位图</param>
            internal void CallOnUpdatedLock(valueType value, valueType oldValue, fastCSharp.code.memberMap memberMap)
            {
                if (OnUpdatedLock != null) OnUpdatedLock(value, oldValue, memberMap);
            }
            /// <summary>
            /// 更新数据之后的处理事件
            /// </summary>
            public event Action<valueType, valueType, fastCSharp.code.memberMap> OnUpdated;
            /// <summary>
            /// 更新数据之后的处理事件
            /// </summary>
            /// <param name="value">更新后的数据</param>
            /// <param name="oldValue">更新前的数据</param>
            /// <param name="memberMap">更新成员位图</param>
            internal void CallOnUpdated(valueType value, valueType oldValue, fastCSharp.code.memberMap memberMap)
            {
                if (OnUpdated != null) OnUpdated(value, oldValue, memberMap);
            }
            /// <summary>
            /// 删除数据之前的验证事件
            /// </summary>
            public event Action<valueType, cancel> OnDelete;
            /// <summary>
            /// 删除数据之前的验证事件
            /// </summary>
            /// <param name="value">待删除数据</param>
            /// <returns>是否可删除数据</returns>
            internal bool CallOnDelete(valueType value)
            {
                if (OnDelete != null)
                {
                    cancel cancel = new cancel();
                    OnDelete(value, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }
            /// <summary>
            /// 删除数据之前的验证事件
            /// </summary>
            public event Action<valueType, cancel> OnDeleteLock;
            /// <summary>
            /// 删除数据之前的验证事件
            /// </summary>
            /// <param name="value">待删除数据</param>
            /// <returns>是否可删除数据</returns>
            internal bool CallOnDeleteLock(valueType value)
            {
                if (OnDeleteLock != null)
                {
                    cancel cancel = new cancel();
                    OnDeleteLock(value, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }
            /// <summary>
            /// 删除数据之后的处理事件
            /// </summary>
            public event Action<valueType> OnDeletedLock;
            /// <summary>
            /// 删除数据之后的处理事件
            /// </summary>
            /// <param name="value">被删除的数据</param>
            internal void CallOnDeletedLock(valueType value)
            {
                if (OnDeletedLock != null) OnDeletedLock(value);
            }
            /// <summary>
            /// 删除数据之后的处理事件
            /// </summary>
            public event Action<valueType> OnDeleted;
            /// <summary>
            /// 删除数据之后的处理事件
            /// </summary>
            /// <param name="value">被删除的数据</param>
            internal void CallOnDeleted(valueType value)
            {
                if (OnDeleted != null) OnDeleted(value);
            }
            /// <summary>
            /// 添加数据是否启用应用程序事务
            /// </summary>
            internal bool IsInsertTransaction
            {
                get
                {
                    return OnInsertedLock != null || OnInserted != null;
                }
            }
            /// <summary>
            /// 添加数据是否启用应用程序事务
            /// </summary>
            internal bool IsUpdateTransaction
            {
                get
                {
                    return OnUpdatedLock != null || OnUpdated != null;
                }
            }
            /// <summary>
            /// 删除数据是否启用应用程序事务
            /// </summary>
            internal bool IsDeleteTransaction
            {
                get
                {
                    return OnDeletedLock != null || OnDeleted != null;
                }
            }
            /// <summary>
            /// 数据集合转DataTable
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="values">数据集合</param>
            /// <returns>数据集合</returns>
            internal DataTable GetDataTable(valueType[] values)
            {
                DataTable dataTable = new DataTable(TableName);
                foreach (keyValue<string, Type> column in sqlModel<modelType>.toArray.DataColumns) dataTable.Columns.Add(new DataColumn(column.Key, column.Value));
                foreach (valueType value in values)
                {
                    object[] memberValues = new object[dataTable.Columns.Count];
                    int index = 0;
                    sqlModel<modelType>.toArray.ToArray(value, memberValues, ref index);
                    dataTable.Rows.Add(memberValues);
                }
                return dataTable;
            }
            /// <summary>
            /// 查询数据集合
            /// </summary>
            /// <param name="expression">查询条件表达式</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据集合</returns>
            public IEnumerable<valueType> Where(Expression<Func<modelType, bool>> expression = null, fastCSharp.code.memberMap memberMap = null)
            {
                return Client.Select(this, (selectQuery<modelType>)expression, memberMap);
            }
            /// <summary>
            /// 查询数据集合
            /// </summary>
            /// <param name="query">查询信息</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据集合</returns>
            public IEnumerable<valueType> Where(selectQuery<modelType> query, fastCSharp.code.memberMap memberMap = null)
            {
                return Client.Select(this, query, memberMap);
            }
            /// <summary>
            /// 查询数据集合
            /// </summary>
            /// <param name="expression">查询条件表达式</param>
            /// <returns>数据集合,失败返回-1</returns>
            public int Count(Expression<Func<modelType, bool>> expression = null)
            {
                return Client.Count(this, expression);
            }
            /// <summary>
            /// 添加到数据库
            /// </summary>
            /// <param name="value">待添加数据</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>添加是否成功</returns>
            public bool Insert(valueType value, bool isIgnoreTransaction = false, fastCSharp.code.memberMap memberMap = null)
            {
                return Client.Insert(this, value, memberMap, isIgnoreTransaction);
            }
            /// <summary>
            /// 添加到数据库
            /// </summary>
            /// <param name="values">数据集合</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>添加是否成功</returns>
            public bool Insert(valueType[] values, bool isIgnoreTransaction = false)
            {
                return values.length() != 0 && Client.Insert(this, values, isIgnoreTransaction) != null;
            }
            /// <summary>
            /// 根据自增id获取数据对象
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="value">关键字数据对象</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据对象</returns>
            public valueType GetByIdentity(valueType value, fastCSharp.code.memberMap memberMap = null)
            {
                if (sqlModel<modelType>.Identity == null) log.Error.Throw(log.exceptionType.ErrorOperation);
                return Client.GetByIdentity(this, value, memberMap);
            }
            /// <summary>
            /// 获取数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据对象</returns>
            public valueType GetByIdentity(long identity, fastCSharp.code.memberMap memberMap = null)
            {
                valueType value = fastCSharp.emit.constructor<valueType>.New();
                sqlModel<modelType>.SetIdentity(value, identity);
                return GetByIdentity(value, memberMap);
            }
            /// <summary>
            /// 获取数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据对象</returns>
            public valueType GetByIdentity(int identity, fastCSharp.code.memberMap memberMap = null)
            {
                return GetByIdentity((long)identity, memberMap);
            }
            /// <summary>
            /// 根据关键字获取数据对象
            /// </summary>
            /// <typeparam name="valueType">数据类型</typeparam>
            /// <param name="value">关键字数据对象</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据对象</returns>
            public valueType GetByPrimaryKey(valueType value, fastCSharp.code.memberMap memberMap = null)
            {
                if (sqlModel<modelType>.PrimaryKeys.Length == 0) log.Error.Throw(log.exceptionType.ErrorOperation);
                return Client.GetByPrimaryKey(this, value, memberMap);
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="value">待修改数据</param>
            /// <param name="memberMap">成员位图</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否修改成功</returns>
            public bool UpdateByIdentity(valueType value, fastCSharp.code.memberMap memberMap = null, bool isIgnoreTransaction = false)
            {
                if (sqlModel<modelType>.Identity == null) log.Error.Throw(log.exceptionType.ErrorOperation);
                return Client.UpdateByIdentity(this, value, memberMap, isIgnoreTransaction);
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="value">待修改数据</param>
            /// <param name="memberMap">成员位图</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否修改成功</returns>
            public bool UpdateByPrimaryKey(valueType value, fastCSharp.code.memberMap memberMap = null, bool isIgnoreTransaction = false)
            {
                if (sqlModel<modelType>.PrimaryKeys.Length == 0) log.Error.Throw(log.exceptionType.ErrorOperation);
                return Client.Update(this, value, memberMap, isIgnoreTransaction);
            }
            /// <summary>
            /// 删除数据库记录
            /// </summary>
            /// <param name="value">待删除数据</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否成功</returns>
            public bool DeleteByIdentity(valueType value, bool isIgnoreTransaction = false)
            {
                if (sqlModel<modelType>.Identity == null) log.Error.Throw(log.exceptionType.ErrorOperation);
                return Client.DeleteByIdentity(this, value, isIgnoreTransaction);
            }
            /// <summary>
            /// 删除数据库记录
            /// </summary>
            /// <param name="identity"></param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否成功</returns>
            public bool DeleteByIdentity(long identity, bool isIgnoreTransaction = false)
            {
                if (sqlModel<modelType>.Identity == null) log.Error.Throw(log.exceptionType.ErrorOperation);
                valueType value = fastCSharp.emit.constructor<valueType>.New();
                sqlModel<modelType>.SetIdentity(value, identity);
                return Client.DeleteByIdentity(this, value, isIgnoreTransaction);
            }
            /// <summary>
            /// 删除数据库记录
            /// </summary>
            /// <param name="identity"></param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否成功</returns>
            public bool DeleteByIdentity(int identity, bool isIgnoreTransaction = false)
            {
                return DeleteByIdentity((long)identity, isIgnoreTransaction);
            }
            /// <summary>
            /// 删除数据库记录
            /// </summary>
            /// <param name="value">待删除数据</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否成功</returns>
            public bool DeleteByPrimaryKey(valueType value, bool isIgnoreTransaction = false)
            {
                if (sqlModel<modelType>.PrimaryKeys.Length == 0) log.Error.Throw(log.exceptionType.ErrorOperation);
                return Client.Delete(this, value, isIgnoreTransaction);
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="sqlExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByIdentity(long identity, updateExpression updateExpression, bool isIgnoreTransaction = false)
            {
                if (updateExpression.Count != 0)
                {
                    valueType value = fastCSharp.emit.constructor<valueType>.New();
                    sqlModel<modelType>.SetIdentity(value, identity);
                    if (Client.UpdateByIdentity(this, value, updateExpression, isIgnoreTransaction)) return value;
                }
                return null;
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="sqlExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByIdentity(int identity, updateExpression updateExpression, bool isIgnoreTransaction = false)
            {
                return UpdateByIdentity((long)identity, updateExpression, isIgnoreTransaction);
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="expression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByIdentity<returnType>(long identity, Expression<Func<modelType, returnType>> expression, bool isIgnoreTransaction = false)
            {
                return expression != null ? UpdateByIdentity(identity, UpdateExpression(expression), isIgnoreTransaction) : null;
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="expression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByIdentity<returnType>(long identity, Expression<Func<modelType, returnType>> expression, returnType returnValue, bool isIgnoreTransaction = false)
            {
                return expression != null ? UpdateByIdentity(identity, UpdateExpression(expression, returnValue), isIgnoreTransaction) : null;
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="expression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByIdentity<returnType>(int identity, Expression<Func<modelType, returnType>> expression, bool isIgnoreTransaction = false)
            {
                return UpdateByIdentity((long)identity, expression, isIgnoreTransaction);
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="expression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByIdentity<returnType>(int identity, Expression<Func<modelType, returnType>> expression, returnType returnValue, bool isIgnoreTransaction = false)
            {
                return UpdateByIdentity((long)identity, expression, returnValue, isIgnoreTransaction);
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <param name="expressions">SQL表达式</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByIdentity<returnType>(long identity, bool isIgnoreTransaction, params Expression<Func<modelType, returnType>>[] expressions)
            {
                return expressions.Length != 0 ? UpdateByIdentity(identity, UpdateExpression(expressions), isIgnoreTransaction) : null;
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <param name="expressions">SQL表达式</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByIdentity<returnType>(int identity, bool isIgnoreTransaction, params Expression<Func<modelType, returnType>>[] expressions)
            {
                return UpdateByIdentity((long)identity, isIgnoreTransaction, expressions);
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="primaryKey">SQL关键字</param>
            /// <param name="updateExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByPrimaryKey(valueType value, updateExpression updateExpression, bool isIgnoreTransaction = false)
            {
                if (updateExpression.Count != 0)
                {
                    if (Client.Update(this, value, updateExpression, isIgnoreTransaction)) return value;
                }
                return null;
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="primaryKey">SQL关键字</param>
            /// <param name="updateExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByPrimaryKey<returnType>(valueType value, Expression<Func<modelType, returnType>> expression, bool isIgnoreTransaction = false)
            {
                return expression != null ? UpdateByPrimaryKey(value, UpdateExpression(expression), isIgnoreTransaction) : null;
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="primaryKey">SQL关键字</param>
            /// <param name="updateExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByPrimaryKey<returnType>(valueType value, Expression<Func<modelType, returnType>> expression, returnType returnValue, bool isIgnoreTransaction = false)
            {
                return expression != null ? UpdateByPrimaryKey(value, UpdateExpression(expression, returnValue), isIgnoreTransaction) : null;
            }
            /// <summary>
            /// 修改数据库记录
            /// </summary>
            /// <param name="primaryKey">SQL关键字</param>
            /// <param name="updateExpression">SQL表达式</param>
            /// <param name="expressions">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public valueType UpdateByPrimaryKey<returnType>(valueType value, bool isIgnoreTransaction, params Expression<Func<modelType, returnType>>[] expressions)
            {
                return expressions.Length != 0 ? UpdateByPrimaryKey(value, UpdateExpression(expressions), isIgnoreTransaction) : null;
            }
        }
        /// <summary>
        /// JSON操作客户端
        /// </summary>
        public sealed class jsonTool : IDisposable
        {
            /// <summary>
            /// JSON解析配置参数
            /// </summary>
            private static readonly jsonParser.config jsonConfig = new jsonParser.config { IsGetJson = false, IsTempString = true };
            /// <summary>
            /// JSON解析配置参数访问锁
            /// </summary>
            private static readonly object jsonConfigLock = new object();
            /// <summary>
            /// 数据库表格操作工具
            /// </summary>
            private interface ISqlTable
            {
                /// <summary>
                /// 字段名称集合
                /// </summary>
                string[] FieldNames { get; }
                /// <summary>
                /// 自增字段名称
                /// </summary>
                string IdentityName { get; }
                /// <summary>
                /// 关键字名称集合
                /// </summary>
                string[] PrimaryKeyNames { get; }
                /// <summary>
                /// 根据JSON查询数据
                /// </summary>
                /// <param name="json"></param>
                /// <returns>查询数据的JSON字符串</returns>
                keyValue<string, string>[] Query(string json);
                /// <summary>
                /// 根据JSON更新数据
                /// </summary>
                /// <param name="json"></param>
                /// <param name="values"></param>
                /// <returns>更新是否成功</returns>
                bool Update(string json, keyValue<string, string>[] values);
                /// <summary>
                /// 根据JSON更新数据
                /// </summary>
                /// <param name="json"></param>
                /// <returns>更新是否成功</returns>
                bool Delete(string json);
            }
            /// <summary>
            /// 数据库表格操作工具
            /// </summary>
            /// <typeparam name="valueType"></typeparam>
            /// <typeparam name="modelType"></typeparam>
            private sealed class sqlTable<valueType, modelType> : ISqlTable
                where valueType : class, modelType
                where modelType : class
            {
                /// <summary>
                /// JSON解析成员位图
                /// </summary>
                private static fastCSharp.code.memberMap jsonMemberMap = fastCSharp.code.memberMap<modelType>.New();
                /// <summary>
                /// JSON解析成员位图参数访问锁
                /// </summary>
                private static readonly object jsonMemberMapLock = new object();
                /// <summary>
                /// 成员名称索引查找数据
                /// </summary>
                private static readonly pointer searcher = fastCSharp.stateSearcher.chars.Create(sqlModel<modelType>.Fields.getArray(value => value.Field.Name));
                /// <summary>
                /// 数据库表格操作工具
                /// </summary>
                private sqlTool<valueType, modelType> sqlTool;
                /// <summary>
                /// 字段名称集合
                /// </summary>
                public string[] FieldNames
                {
                    get { return sqlModel<modelType>.Fields.getArray(value => value.Field.Name); }
                }
                /// <summary>
                /// 自增字段名称
                /// </summary>
                public string IdentityName
                {
                    get { return sqlModel<modelType>.Identity == null ? null : sqlModel<modelType>.Identity.Field.Name; }
                }
                /// <summary>
                /// 关键字名称集合
                /// </summary>
                public string[] PrimaryKeyNames
                {
                    get { return sqlModel<modelType>.PrimaryKeys.getArray(value => value.Field.Name); }
                }
                /// <summary>
                /// 数据库表格操作工具
                /// </summary>
                /// <param name="sqlTool">数据库表格操作工具</param>
                private sqlTable(sqlTool<valueType, modelType> sqlTool)
                {
                    this.sqlTool = sqlTool;
                }
                /// <summary>
                /// JSON解析
                /// </summary>
                /// <param name="value"></param>
                /// <param name="json"></param>
                /// <returns></returns>
                private bool parseJson(modelType value, string json)
                {
                    Monitor.Enter(jsonConfig);
                    try
                    {
                        jsonConfig.MemberMap = jsonMemberMap;
                        return jsonParser.Parse<modelType>(json, ref value, jsonConfig);
                    }
                    finally { Monitor.Exit(jsonConfig); }
                }
                /// <summary>
                /// 根据JSON查询数据
                /// </summary>
                /// <param name="json"></param>
                /// <returns>查询数据的JSON字符串</returns>
                public keyValue<string, string>[] Query(string json)
                {
                    valueType value = fastCSharp.emit.constructor<valueType>.New();
                    Monitor.Enter(jsonMemberMapLock);
                    try
                    {
                        if (parseJson(value, json))
                        {
                            if (sqlModel<modelType>.Identity != null && jsonMemberMap.IsMember(sqlModel<modelType>.Identity.MemberMapIndex))
                            {
                                value = sqlTool.GetByIdentity(value);
                            }
                            else if (sqlModel<modelType>.PrimaryKeys.Length != 0) value = sqlTool.GetByPrimaryKey(value);
                            else value = null;
                        }
                        else value = null;
                    }
                    finally
                    {
                        jsonMemberMap.Clear();
                        Monitor.Exit(jsonMemberMapLock);
                    }
                    if (value != null)
                    {
                        return sqlModel<modelType>.Fields.getArray(field => new keyValue<string, string>(field.Field.Name, jsonSerializer.ObjectToJson(field.Field.GetValue(value))));
                    }
                    return null;
                }
                /// <summary>
                /// 根据JSON更新数据
                /// </summary>
                /// <param name="json"></param>
                /// <param name="values"></param>
                /// <returns>更新是否成功</returns>
                public bool Update(string json, keyValue<string, string>[] values)
                {
                    valueType value = fastCSharp.emit.constructor<valueType>.New();
                    int isUpdate = 0;
                    Monitor.Enter(jsonMemberMapLock);
                    try
                    {
                        if (parseJson(value, json))
                        {
                            if (sqlModel<modelType>.Identity != null && jsonMemberMap.IsMember(sqlModel<modelType>.Identity.MemberMapIndex)) isUpdate = 1;
                            else if (sqlModel<modelType>.PrimaryKeys.Length != 0) isUpdate = 2;
                            if (isUpdate != 0)
                            {
                                fastCSharp.stateSearcher.chars nameSearcher = new fastCSharp.stateSearcher.chars(searcher);
                                foreach (keyValue<string, string> nameValue in values)
                                {
                                    int index = nameSearcher.Search(nameValue.Key);
                                    if (index != -1)
                                    {
                                        fastCSharp.code.cSharp.sqlModel.fieldInfo field = sqlModel<modelType>.Fields[index];
                                        field.Field.SetValue(value, jsonParser.ParseType(field.Field.FieldType, nameValue.Value));
                                        jsonMemberMap.SetMember(field.MemberMapIndex);
                                    }
                                }
                                return isUpdate == 1 ? sqlTool.UpdateByIdentity(value, jsonMemberMap) : sqlTool.UpdateByPrimaryKey(value, jsonMemberMap);
                            }
                        }
                    }
                    finally
                    {
                        jsonMemberMap.Clear();
                        Monitor.Exit(jsonMemberMapLock);
                    }
                    return false;
                }
                /// <summary>
                /// 根据JSON更新数据
                /// </summary>
                /// <param name="json"></param>
                /// <returns>更新是否成功</returns>
                public bool Delete(string json)
                {
                    valueType value = fastCSharp.emit.constructor<valueType>.New();
                    bool isIdentity = false, isPrimaryKey = false;
                    Monitor.Enter(jsonMemberMapLock);
                    try
                    {
                        if (parseJson(value, json))
                        {
                            if (sqlModel<modelType>.Identity != null && jsonMemberMap.IsMember(sqlModel<modelType>.Identity.MemberMapIndex))
                            {
                                isIdentity = true;
                            }
                            else if (sqlModel<modelType>.PrimaryKeys.Length != 0) isPrimaryKey = true;
                        }
                    }
                    finally
                    {
                        jsonMemberMap.Clear();
                        Monitor.Exit(jsonMemberMapLock);
                    }
                    if (isIdentity) return sqlTool.DeleteByIdentity(value);
                    if (isPrimaryKey) return sqlTool.DeleteByPrimaryKey(value);
                    return false;
                }
                /// <summary>
                /// 获取数据库表格操作工具
                /// </summary>
                /// <returns>数据库表格操作工具</returns>
                private static ISqlTable get()
                {
                    if (sqlModel<modelType>.Identity != null || sqlModel<modelType>.PrimaryKeys.Length != 0)
                    {
                        FieldInfo field = typeof(valueType).GetField(fastCSharp.emit.sqlTable.SqlTableName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        if (field != null)
                        {
                            sqlTool<valueType, modelType> sqlTable = field.GetValue(null) as sqlTool<valueType, modelType>;
                            if (sqlTable != null) return new sqlTable<valueType, modelType>(sqlTable);
                        }
                    }
                    return null;
                }
            }
            /// <summary>
            /// 类型名称索引查找数据
            /// </summary>
            private pointer searcher;
            /// <summary>
            /// 数据库表格操作工具集合
            /// </summary>
            private ISqlTable[] sqlTables;
            /// <summary>
            /// 类型名称集合
            /// </summary>
            public string[] Names { get; private set; }
            /// <summary>
            /// JSON操作客户端
            /// </summary>
            /// <param name="assembly">数据库表格相关程序集</param>
            public jsonTool(Assembly assembly)
            {
                if (assembly == null) log.Error.Throw(log.exceptionType.Null);
                subArray<keyValue<string, ISqlTable>> sqlTables = default(subArray<keyValue<string, ISqlTable>>);
                foreach (Type type in assembly.GetTypes())
                {
                    sqlTable attribute = fastCSharp.code.typeAttribute.GetAttribute<sqlTable>(type, false, true);
                    if (attribute != null && Array.IndexOf(fastCSharp.config.sql.Default.CheckConnection, attribute.ConnectionType) != -1)
                    {
                        object sqlTable = typeof(sqlTable<,>).MakeGenericType(type, fastCSharp.code.cSharp.dataModel.GetModelType<fastCSharp.code.cSharp.sqlModel>(type) ?? type)
                            .GetMethod("get", BindingFlags.Static | BindingFlags.NonPublic)
                            .Invoke(null, null);
                        if (sqlTable != null) sqlTables.Add(new keyValue<string, ISqlTable>(type.fullName(), (ISqlTable)sqlTable));
                    }
                }
                if (sqlTables.Count != 0)
                {
                    sqlTables.Sort((left, right) => left.Key.CompareTo(right.Key));
                    this.sqlTables = sqlTables.GetArray(value => value.Value);
                    searcher = fastCSharp.stateSearcher.chars.Create(Names = sqlTables.GetArray(value => value.Key));
                }
            }
            /// <summary>
            /// 获取数据库表格操作工具
            /// </summary>
            /// <param name="type"></param>
            /// <returns>数据库表格操作工具</returns>
            private ISqlTable get(string type)
            {
                int index = new fastCSharp.stateSearcher.chars(searcher).Search(type);
                return index >= 0 ? sqlTables[index] : null;
            }
            /// <summary>
            /// 获取字段名称集合
            /// </summary>
            /// <param name="type"></param>
            /// <param name="identityName">自增字段名称</param>
            /// <param name="primaryKeyNames">关键字名称集合</param>
            /// <returns>字段名称集合</returns>
            public string[] GetFields(string type, ref string identityName, ref string[] primaryKeyNames)
            {
                ISqlTable sqlTable = get(type);
                if (sqlTable != null)
                {
                    identityName = sqlTable.IdentityName;
                    primaryKeyNames = sqlTable.PrimaryKeyNames;
                    return sqlTable.FieldNames;
                }
                return null;
            }
            /// <summary>
            /// 根据JSON查询数据
            /// </summary>
            /// <param name="type"></param>
            /// <param name="json"></param>
            /// <returns>查询数据的JSON字符串</returns>
            public keyValue<string, string>[] Query(string type, string json)
            {
                ISqlTable sqlTable = get(type);
                return sqlTable != null ? sqlTable.Query(json) : null;
            }
            /// <summary>
            /// 根据JSON更新数据
            /// </summary>
            /// <param name="type"></param>
            /// <param name="json"></param>
            /// <param name="values"></param>
            /// <returns>更新是否成功</returns>
            public bool Update(string type, string json, keyValue<string, string>[] values)
            {
                ISqlTable sqlTable = get(type);
                return sqlTable != null && sqlTable.Update(json, values);
            }
            /// <summary>
            /// 根据JSON更新数据
            /// </summary>
            /// <param name="type"></param>
            /// <param name="json"></param>
            /// <returns>更新是否成功</returns>
            public bool Delete(string type, string json)
            {
                ISqlTable sqlTable = get(type);
                return sqlTable != null && sqlTable.Delete(json);
            }
            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                unmanaged.Free(ref searcher);
            }
        }
    }
    /// <summary>
    /// 数据库表格操作工具
    /// </summary>
    /// <typeparam name="valueType">表格类型</typeparam>
    /// <typeparam name="modelType">模型类型</typeparam>
    public class sqlTable<valueType, modelType> : sqlTable.sqlTool<valueType, modelType>
        where valueType : class, modelType
        where modelType : class
    {
        /// <summary>
        /// 数据库表格操作工具
        /// </summary>
        /// <param name="sqlTable">数据库表格配置</param>
        /// <param name="client">SQL操作客户端</param>
        /// <param name="tableName">表格名称</param>
        protected sqlTable(sqlTable sqlTable, client client, string tableName)
            : base(sqlTable, client, tableName)
        {
        }
        /// <summary>
        /// 获取数据库表格操作工具
        /// </summary>
        /// <returns>数据库表格操作工具</returns>
        public static sqlTable<valueType, modelType> Get()
        {
            Type type = typeof(valueType);
            sqlTable sqlTable = fastCSharp.code.typeAttribute.GetAttribute<sqlTable>(type, false, true);
            if (sqlTable != null && Array.IndexOf(fastCSharp.config.sql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
            {
                return new sqlTable<valueType, modelType>(sqlTable, connection.GetConnection(sqlTable.ConnectionType).Client, sqlTable.GetTableName(type));
            }
            return null;
        }
    }

    /// <summary>
    /// 数据库表格操作工具
    /// </summary>
    /// <typeparam name="modelType">模型类型</typeparam>
    public sealed class sqlModelTable<modelType> : sqlTable<modelType, modelType>
        where modelType : class
    {
        /// <summary>
        /// 数据库表格操作工具
        /// </summary>
        /// <param name="sqlTable">数据库表格配置</param>
        /// <param name="client">SQL操作客户端</param>
        /// <param name="tableName">表格名称</param>
        private sqlModelTable(sqlTable sqlTable, client client, string tableName)
            : base(sqlTable, client, tableName)
        {
        }
        /// <summary>
        /// 获取数据库表格操作工具
        /// </summary>
        /// <returns>数据库表格操作工具</returns>
        public new static sqlModelTable<modelType> Get()
        {
            Type type = typeof(modelType);
            sqlTable sqlTable = fastCSharp.code.typeAttribute.GetAttribute<sqlTable>(type, false, true);
            if (sqlTable != null && Array.IndexOf(fastCSharp.config.sql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
            {
                return new sqlModelTable<modelType>(sqlTable, connection.GetConnection(sqlTable.ConnectionType).Client, sqlTable.GetTableName(type));
            }
            return null;
        }
    }
    /// <summary>
    /// 数据库表格操作工具
    /// </summary>
    /// <typeparam name="valueType">表格类型</typeparam>
    /// <typeparam name="modelType">模型类型</typeparam>
    /// <typeparam name="keyType">关键字类型</typeparam>
    public class sqlTable<valueType, modelType, keyType> : sqlTable.sqlTool<valueType, modelType>
        where valueType : class, modelType
        where modelType : class
        where keyType : IEquatable<keyType>
    {
        /// <summary>
        /// 设置关键字
        /// </summary>
        private Action<modelType, keyType> setPrimaryKey;
        /// <summary>
        /// 获取关键字
        /// </summary>
        public Func<modelType, keyType> GetPrimaryKey { get; private set; }
        /// <summary>
        /// 数据库表格操作工具
        /// </summary>
        /// <param name="sqlTable">数据库表格配置</param>
        /// <param name="client">SQL操作客户端</param>
        /// <param name="tableName">表格名称</param>
        protected sqlTable(sqlTable sqlTable, client client, string tableName)
            : base(sqlTable, client, tableName)
        {
            FieldInfo[] primaryKeys = sqlModel<modelType>.PrimaryKeys.getArray(value => value.Field);
            GetPrimaryKey = databaseModel<modelType>.GetPrimaryKeyGetter<keyType>("GetSqlPrimaryKey", primaryKeys);
            setPrimaryKey = databaseModel<modelType>.GetPrimaryKeySetter<keyType>("SetSqlPrimaryKey", primaryKeys);
        }
        /// <summary>
        /// 获取数据库表格操作工具
        /// </summary>
        /// <returns>数据库表格操作工具</returns>
        public static sqlTable<valueType, modelType, keyType> Get()
        {
            Type type = typeof(valueType);
            sqlTable sqlTable = fastCSharp.code.typeAttribute.GetAttribute<sqlTable>(type, false, true);
            if (sqlTable != null && Array.IndexOf(fastCSharp.config.sql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
            {
                return new sqlTable<valueType, modelType, keyType>(sqlTable, connection.GetConnection(sqlTable.ConnectionType).Client, sqlTable.GetTableName(type));
            }
            return null;
        }
        /// <summary>
        /// 根据关键字获取数据对象
        /// </summary>
        /// <typeparam name="valueType">数据类型</typeparam>
        /// <param name="key">关键字</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>数据对象</returns>
        public valueType GetByPrimaryKey(keyType key, fastCSharp.code.memberMap memberMap = null)
        {
            valueType value = fastCSharp.emit.constructor<valueType>.New();
            setPrimaryKey(value, key);
            return GetByPrimaryKey(value, memberMap);
        }
        /// <summary>
        /// 修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="updateExpression">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public valueType Update(keyType key, sqlTable.updateExpression updateExpression, bool isIgnoreTransaction = false)
        {
            if (updateExpression.Count != 0)
            {
                valueType value = fastCSharp.emit.constructor<valueType>.New();
                setPrimaryKey(value, key);
                return UpdateByPrimaryKey(value, updateExpression, isIgnoreTransaction);
            }
            return null;
        }
        /// <summary>
        /// 修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="expression">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public valueType Update<returnType>(keyType key, Expression<Func<modelType, returnType>> expression, bool isIgnoreTransaction = false)
        {
            return expression != null ? Update(key, UpdateExpression(expression), isIgnoreTransaction) : null;
        }
        /// <summary>
        /// 修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="expression">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public valueType Update<returnType>(keyType key, Expression<Func<modelType, returnType>> expression, returnType returnValue, bool isIgnoreTransaction = false)
        {
            return expression != null ? Update(key, UpdateExpression(expression, returnValue), isIgnoreTransaction) : null;
        }
        /// <summary>
        /// 修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="expressions">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public valueType Update<returnType>(keyType key, bool isIgnoreTransaction = false, params Expression<Func<modelType, returnType>>[] expressions)
        {
            return expressions.Length != 0 ? Update(key, UpdateExpression(expressions), isIgnoreTransaction) : null;
        }
        /// <summary>
        /// 修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="updateExpression">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public bool Delete(keyType key, bool isIgnoreTransaction = false)
        {
            valueType value = fastCSharp.emit.constructor<valueType>.New();
            setPrimaryKey(value, key);
            return DeleteByPrimaryKey(value, isIgnoreTransaction);
        }
    }
    /// <summary>
    /// 数据库表格操作工具
    /// </summary>
    /// <typeparam name="modelType">模型类型</typeparam>
    /// <typeparam name="keyType">关键字类型</typeparam>
    public sealed class sqlModelTable<modelType, keyType> : sqlTable<modelType, modelType, keyType>
        where modelType : class
        where keyType : IEquatable<keyType>
    {
        /// <summary>
        /// 数据库表格操作工具
        /// </summary>
        /// <param name="sqlTable">数据库表格配置</param>
        /// <param name="client">SQL操作客户端</param>
        /// <param name="tableName">表格名称</param>
        private sqlModelTable(sqlTable sqlTable, client client, string tableName)
            : base(sqlTable, client, tableName)
        {
        }
        /// <summary>
        /// 获取数据库表格操作工具
        /// </summary>
        /// <returns>数据库表格操作工具</returns>
        public new static sqlModelTable<modelType, keyType> Get()
        {
            Type type = typeof(modelType);
            sqlTable sqlTable = fastCSharp.code.typeAttribute.GetAttribute<sqlTable>(type, false, true);
            if (sqlTable != null && Array.IndexOf(fastCSharp.config.sql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
            {
                return new sqlModelTable<modelType, keyType>(sqlTable, connection.GetConnection(sqlTable.ConnectionType).Client, sqlTable.GetTableName(type));
            }
            return null;
        }
    }
}
