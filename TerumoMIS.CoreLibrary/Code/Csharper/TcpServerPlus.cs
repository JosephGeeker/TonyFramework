//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: TcpServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Code.Csharper
//	File Name:  TcpServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 11:48:25
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

namespace TerumoMIS.CoreLibrary.Code.Csharper
{
    /// <summary>
    /// TCP服务调用配置,定义类必须实现ITcpServerPlus接口
    /// </summary>
    public partial class TcpServerPlus:TcpBasePlus
    {
        /// <summary>
        /// TCP服务接口
        /// </summary>
        public interface ITcpServer
        {
            /// <summary>
            /// 设置TCP服务端
            /// </summary>
            /// <param name="tcpServer">TCP服务端</param>
            void SetTcpServer(fastCSharp.net.tcp.server tcpServer);
        }
        /// <summary>
        /// 获取TCP调用泛型函数集合
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns>TCP调用泛型函数集合</returns>
        public static Dictionary<genericMethod, MethodInfo> GetGenericMethods(Type type)
        {
            if (type != null)
            {
                tcpServer tcpServer = fastCSharp.code.typeAttribute.GetAttribute<tcpServer>(type, false, false);//cSharp.Default.IsInheritAttribute
                if (tcpServer != null && tcpServer.IsSetup)
                {
                    Dictionary<genericMethod, MethodInfo> values = dictionary.Create<genericMethod, MethodInfo>();
                    code.methodInfo[] methods = code.methodInfo.GetMethods<tcpServer>(type, tcpServer.MemberFilter, false, tcpServer.IsAttribute, tcpServer.IsBaseTypeAttribute, tcpServer.IsInheritAttribute);
                    if (type.IsGenericType)
                    {
                        code.methodInfo[] definitionMethods = code.methodInfo.GetMethods<tcpServer>(type.GetGenericTypeDefinition(), tcpServer.MemberFilter, false, tcpServer.IsAttribute, tcpServer.IsBaseTypeAttribute, tcpServer.IsInheritAttribute);
                        int index = 0;
                        foreach (code.methodInfo method in methods)
                        {
                            if (method.Method.IsGenericMethod) values.Add(new genericMethod(definitionMethods[index].Method), method.Method);
                            ++index;
                        }
                    }
                    else
                    {
                        foreach (code.methodInfo method in methods)
                        {
                            if (method.Method.IsGenericMethod) values.Add(new genericMethod(method.Method), method.Method);
                        }
                    }
                    return values;
                }
            }
            return null;
        }
        /// <summary>
        /// 泛型方法调用
        /// </summary>
        /// <param name="value">服务器端目标对象</param>
        /// <param name="method">泛型方法信息</param>
        /// <param name="types">泛型参数类型集合</param>
        /// <param name="parameters">调用参数</param>
        /// <returns>返回值</returns>
        public static object InvokeGenericMethod(object value, MethodInfo method, fastCSharp.code.remoteType[] types, params object[] parameters)
        {
            if (method == null) fastCSharp.log.Error.Throw(fastCSharp.log.exceptionType.Null);
            return method.MakeGenericMethod(types.getArray(type => type.Type)).Invoke(value, parameters);
        }
        /// <summary>
        /// 成员选择类型
        /// </summary>
        public code.memberFilters Filter = code.memberFilters.NonPublicInstance;
        /// <summary>
        /// 成员选择类型
        /// </summary>
        public code.memberFilters MemberFilter
        {
            get { return Filter & code.memberFilters.Instance; }
        }
        /// <summary>
        /// 是否服务器端(服务配置)
        /// </summary>
        public bool IsServer;
        /// <summary>
        /// 是否生成客户端调用接口(服务配置)
        /// </summary>
        public bool IsClientInterface;
        /// <summary>
        /// 客户端调用接口名称(服务配置)
        /// </summary>
        public string ClientInterfaceName;
        /// <summary>
        /// 客户端附加接口类型(服务配置)
        /// </summary>
        public Type ClientInterfaceType;
        /// <summary>
        /// 是否生成负载均衡服务(服务配置)
        /// </summary>
        public bool IsLoadBalancing;
        /// <summary>
        /// 自定义负载均衡类型(服务配置)
        /// </summary>
        public Type LoadBalancingType;
        /// <summary>
        /// 负载均衡错误尝试次数
        /// </summary>
        public int LoadBalancingTryCount = 3;
        /// <summary>
        /// 负载均衡服务端检测间隔秒数
        /// </summary>
        public int LoadBalancingCheckSeconds;
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="type">TCP服务器类型</param>
        /// <returns>TCP调用服务器端配置信息</returns>
        public static tcpServer GetConfig(Type type)
        {
            tcpServer attribute = fastCSharp.code.typeAttribute.GetAttribute<tcpServer>(type, false, true);
            return attribute != null ? fastCSharp.config.pub.LoadConfig(attribute, attribute.Service) : null;
        }
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="serviceName">TCP调用服务名称</param>
        /// <param name="type">TCP服务器类型</param>
        /// <returns>TCP调用服务器端配置信息</returns>
        public static tcpServer GetConfig(string serviceName, Type type = null)
        {
            tcpServer attribute = fastCSharp.config.pub.LoadConfig(type != null ? fastCSharp.code.typeAttribute.GetAttribute<tcpServer>(type, false, true) ?? new tcpServer() : new tcpServer(), serviceName);
            if (attribute.Service == null) attribute.Service = serviceName;
            return attribute;
        }
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="serviceName">TCP调用服务名称</param>
        /// <param name="type">TCP服务器类型</param>
        /// <returns>TCP调用服务器端配置信息</returns>
        public static tcpServer GetTcpCallConfig(string serviceName, Type type = null)
        {
            tcpServer attribute = new tcpServer();
            if (type != null)
            {
                tcpCall tcpCall = fastCSharp.code.typeAttribute.GetAttribute<tcpCall>(type, false, true);
                if (tcpCall != null) attribute.CopyFrom(tcpCall);
            }
            attribute = fastCSharp.config.pub.LoadConfig(attribute, serviceName);
            if (attribute.Service == null) attribute.Service = serviceName;
            return attribute;
        }
    }
}
