using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 空值相关参数
    /// </summary>
    /// <typeparam name="TValueType">值类型</typeparam>
    public static class NullValuePlus<TValueType>
    {
        /// <summary>
        /// 默认空值
        /// </summary>
        public static readonly TValueType Value = default(TValueType);
        /// <summary>
        /// 0元素数组
        /// </summary>
        public static readonly TValueType[] Array = new TValueType[0];
    }
}
