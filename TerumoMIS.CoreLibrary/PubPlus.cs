using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    ///     常用公共定义
    /// </summary>
    public static class PubPlus
    {
        /// <summary>
        ///     项目常量，不可修改
        /// </summary>
        public const string TmphCoreLib = "TmphCoreLib";

        /// <summary>
        ///     基数排序数据量
        /// </summary>
        public const int RadixSortSize = 1 << 9;

        /// <summary>
        ///     64位基数排序数据量
        /// </summary>
        public const int RadixSortSize64 = 4 << 9;

        /// <summary>
        ///     默认空字符
        /// </summary>
        public const char NullChar = (char) 0;

        /// <summary>
        ///     程序执行主目录(小写字母)
        /// </summary>
        public static readonly string ApplicationPath =
            new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName.ToLower();

        /// <summary>
        ///     CPU核心数量
        /// </summary>
        public static readonly int CpuCount = Environment.ProcessorCount;

        /// <summary>
        ///     程序启用时间
        /// </summary>
        public static readonly DateTime StartTime = DateTime.Now;

        /// <summary>
        ///     最小时间值
        /// </summary>
        public static readonly DateTime MinTime = new DateTime(1900, 1, 1);

        /// <summary>
        ///     GB2312编码
        /// </summary>
        public static readonly Encoding Gb2312 = Encoding.GetEncoding("gb2312");

        /// <summary>
        ///     gb18030编码
        /// </summary>
        public static readonly Encoding Gb18030 = Encoding.GetEncoding("gb18030");

        /// <summary>
        ///     gbk编码
        /// </summary>
        public static readonly Encoding Gbk = Encoding.GetEncoding("gbk");

        /// <summary>
        ///     big5编码
        /// </summary>
        public static readonly Encoding Big5 = Encoding.GetEncoding("big5");

        /// <summary>
        ///     默认自增标识
        /// </summary>
        private static int _identity32;

        /// <summary>
        ///     默认自增标识
        /// </summary>
        private static long _identity;

        /// <summary>
        ///     SHA1哈希加密
        /// </summary>
        private static readonly SHA1CryptoServiceProvider Sha1CryptoServiceProvider = new SHA1CryptoServiceProvider();

        /// <summary>
        ///     SHA1哈希加密访问锁
        /// </summary>
        private static readonly object Sha1Lock = new object();

        /// <summary>
        ///     内存位长
        /// </summary>
        public static readonly int MemoryBits;

        /// <summary>
        ///     内存字节长度
        /// </summary>
        public static readonly int MemoryBytes;

        static unsafe PubPlus()
        {
            byte* bytes = stackalloc byte[4];
            if (((long) bytes >> 32) == 0)
            {
                MemoryBits = ((long) (bytes + 0x100000000L) >> 32) == 0 ? 32 : 64;
            }
            else MemoryBits = 64;
            MemoryBytes = MemoryBits >> 3;
        }

        /// <summary>
        ///     默认自增标识
        /// </summary>
        internal static int Identity32
        {
            get { return Interlocked.Increment(ref _identity32); }
        }

        /// <summary>
        ///     默认自增标识
        /// </summary>
        public static long Identity
        {
            get { return Interlocked.Increment(ref _identity); }
        }

        /// <summary>
        ///     SHA1哈希加密
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">数据长度</param>
        /// <returns>SHA1哈希</returns>
        public static byte[] Sha1(byte[] buffer, int startIndex, int length)
        {
            Monitor.Enter(Sha1Lock);
            try
            {
                buffer = Sha1CryptoServiceProvider.ComputeHash(buffer, startIndex, length);
            }
            finally
            {
                Monitor.Exit(Sha1Lock);
            }
            return buffer;
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <typeparam name="TValueType">资源类型</typeparam>
        /// <param name="resource">资源引用</param>
        public static void Dispose<TValueType>(TValueType resource)
            where TValueType : class, IDisposable
        {
            if (resource != null)
            {
                try
                {
                    resource.Dispose();
                }
                catch (Exception error)
                {
                    LogPlus.Default.Add(error, null, false);
                }
            }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <typeparam name="TValueType">资源类型</typeparam>
        /// <param name="resource">资源引用</param>
        public static void Dispose<TValueType>(ref TValueType resource)
            where TValueType : class, IDisposable
        {
            Exception exception = null;
            Dispose(ref resource, ref exception);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <typeparam name="TValueType">资源类型</typeparam>
        /// <param name="resource">资源引用</param>
        public static void Dispose<TValueType>(ref TValueType[] resource) where TValueType : class, IDisposable
        {
            var values = Interlocked.Exchange(ref resource, null);
            if (values != null)
            {
                foreach (var value in values)
                {
                    try
                    {
                        value.Dispose();
                    }
                    catch (Exception error)
                    {
                        LogPlus.Default.Add(error, null, false);
                    }
                }
            }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <typeparam name="TValueType">资源类型</typeparam>
        /// <param name="resource">资源引用</param>
        /// <param name="exception">错误异常引用</param>
        public static void Dispose<TValueType>(ref TValueType resource, ref Exception exception)
            where TValueType : class, IDisposable
        {
            var value = Interlocked.Exchange(ref resource, null);
            if (value != null)
            {
                try
                {
                    value.Dispose();
                }
                catch (Exception error)
                {
                    exception = error;
                }
            }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <typeparam name="TValueType">资源类型</typeparam>
        /// <param name="accessLock">资源访问锁</param>
        /// <param name="resource">资源引用</param>
        public static void Dispose<TValueType>(object accessLock, ref TValueType resource)
            where TValueType : class, IDisposable
        {
            Exception exception = null;
            Dispose(accessLock, ref resource, ref exception);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <typeparam name="TValueType">资源类型</typeparam>
        /// <param name="accessLock">资源访问锁</param>
        /// <param name="resource">资源引用</param>
        /// <param name="exception">错误异常引用</param>
        public static void Dispose<TValueType>(object accessLock, ref TValueType resource, ref Exception exception)
            where TValueType : class, IDisposable
        {
            var value = resource;
            if (value != null)
            {
                Monitor.Enter(accessLock);
                try
                {
                    if (resource != null)
                    {
                        value.Dispose();
                        resource = null;
                    }
                }
                catch (Exception error)
                {
                    exception = error;
                }
                finally
                {
                    Monitor.Exit(accessLock);
                }
            }
        }

        /// <summary>
        ///     函数调用,用于链式编程
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">数据</param>
        /// <param name="method">函数调用</param>
        /// <returns>数据</returns>
        public static TValueType Action<TValueType>(this TValueType value, Action<TValueType> method)
            where TValueType : class
        {
            if (method == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
            if (value != null) if (method != null) method(value);
            return value;
        }
    }
}