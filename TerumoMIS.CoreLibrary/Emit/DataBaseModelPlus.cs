//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DataBaseModelPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Emit
//	File Name:  DataBaseModelPlus
//	User name:  C1400008
//	Location Time: 2015/7/13 11:51:24
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.Emit
{
    /// <summary>
    /// 数据库表格类型
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public abstract class DataBaseModelPlus<TValueType>
    {
        /// <summary>
        /// 获取自增字段获取器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        protected static Func<TValueType, int> GetIdentityGetter32(string name, FieldInfo field)
        {
            var dynamicMethod = new DynamicMethod(name, typeof(int), new[] { typeof(TValueType) }, typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, field);
            if (field.FieldType != typeof(int) && field.FieldType != typeof(uint)) generator.Emit(OpCodes.Conv_I4);
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType, int>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, int>));
        }
        /// <summary>
        /// 获取自增字段设置器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        protected static Action<TValueType, int> GetIdentitySetter32(string name, FieldInfo field)
        {
            var dynamicMethod = new DynamicMethod(name, null, new[] { typeof(TValueType), typeof(int) }, typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            if (field.FieldType == typeof(long) || field.FieldType == typeof(ulong)) generator.Emit(OpCodes.Conv_I8);
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);
            return (Action<TValueType, int>)dynamicMethod.CreateDelegate(typeof(Action<TValueType, int>));
        }
        /// <summary>
        /// 获取关键字获取器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        internal static Func<TValueType, TKeyType> GetPrimaryKeyGetter<TKeyType>(string name, FieldInfo[] primaryKeys)
        {
            if (primaryKeys.Length == 0) return null;
            var dynamicMethod = new DynamicMethod(name, typeof(TKeyType), new[] { typeof(TValueType) }, typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            if (primaryKeys.Length == 1)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, primaryKeys[0]);
            }
            else
            {
                var key = generator.DeclareLocal(typeof(TKeyType));
                generator.Emit(OpCodes.Ldloca_S, key);
                generator.Emit(OpCodes.Initobj, typeof(TKeyType));
                foreach (FieldInfo primaryKey in primaryKeys)
                {
                    generator.Emit(OpCodes.Ldloca_S, key);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, primaryKey);
                    generator.Emit(OpCodes.Stfld, typeof(TKeyType).GetField(primaryKey.Name, BindingFlags.Instance | BindingFlags.Public));
                }
                generator.Emit(OpCodes.Ldloc_0);
            }
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType, TKeyType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, TKeyType>));
        }
        /// <summary>
        /// 获取关键字设置器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        internal static Action<TValueType, TKeyType> GetPrimaryKeySetter<TKeyType>(string name, FieldInfo[] primaryKeys)
        {
            if (primaryKeys.Length == 0) return null;
            var dynamicMethod = new DynamicMethod(name, null, new[] { typeof(TValueType), typeof(TKeyType) }, typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            if (primaryKeys.Length == 1)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, primaryKeys[0]);
            }
            else
            {
                foreach (FieldInfo primaryKey in primaryKeys)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarga_S, 1);
                    generator.Emit(OpCodes.Ldfld, typeof(TKeyType).GetField(primaryKey.Name, BindingFlags.Instance | BindingFlags.Public));
                    generator.Emit(OpCodes.Stfld, primaryKey);
                }
            }
            generator.Emit(OpCodes.Ret);
            return (Action<TValueType, TKeyType>)dynamicMethod.CreateDelegate(typeof(Action<TValueType, TKeyType>));
        }
    }
}
