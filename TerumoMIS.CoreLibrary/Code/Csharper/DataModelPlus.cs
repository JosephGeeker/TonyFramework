//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DataModelPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  DataModelPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 15:15:13
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Code.Csharper
{
    /// <summary>
    /// 数据库表格模型配置
    /// </summary>
    public abstract class DataModelPlus:MemberFilterPlus.PublicInstanceFieldPlus
    {
        /// <summary>
        /// 是否有序比较
        /// </summary>
        public bool IsComparable;
        ///// <summary>
        ///// 是否检查添加数据的自增值
        ///// </summary>
        //public bool IsCheckAppendIdentity = true;
        /// <summary>
        /// 获取数据库表格模型类型
        /// </summary>
        /// <param name="type">数据库表格绑定类型</param>
        /// <returns>数据库表格模型类型,失败返回null</returns>
        internal static Type GetModelType<modelType>(Type type) where modelType : dataModel
        {
            do
            {
                modelType sqlModel = fastCSharp.code.typeAttribute.GetAttribute<modelType>(type, false, true);
                if (sqlModel != null) return type;
                if ((type = type.BaseType) == null) return null;
            }
            while (true);
        }
        /// <summary>
        /// 获取字段成员集合
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeAttribute">类型配置</param>
        /// <returns>字段成员集合</returns>
        public static subArray<memberInfo> GetPrimaryKeys<modeType>(Type type, modeType model) where modeType : dataModel
        {
            fieldIndex[] fields = (fieldIndex[])typeof(fastCSharp.code.memberIndexGroup<>).MakeGenericType(type).GetMethod("GetFields", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { model.MemberFilter });
            subArray<memberInfo> values = new subArray<memberInfo>();
            foreach (fieldIndex field in fields)
            {
                type = field.Member.FieldType;
                if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    dataMember attribute = field.GetSetupAttribute<dataMember>(true, true);
                    if (attribute != null && attribute.PrimaryKeyIndex != 0) values.Add(new memberInfo(type, field.Member.Name, attribute.PrimaryKeyIndex));
                }
            }
            return values.Sort((left, right) =>
            {
                int value = left.MemberIndex - right.MemberIndex;
                return value == 0 ? left.MemberName.CompareTo(right.MemberName) : value;
            });
        }
    }
}
