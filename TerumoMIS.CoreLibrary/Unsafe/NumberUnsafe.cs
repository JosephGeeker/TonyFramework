//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: NumberUnsafe
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Unsafe
//	File Name:  NumberUnsafe
//	User name:  C1400008
//	Location Time: 2015/7/16 13:24:10
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

namespace TerumoMIS.CoreLibrary.Unsafe
{
    /// <summary>
    ///     整数相关操作（非安全，请自行确保数据可靠性）
    /// </summary>
    public static class NumberUnsafe
    {
        /// <summary>
        ///     ascii字符串转数字
        /// </summary>
        /// <param name="numbers">ascii字符串,不能为空</param>
        /// <returns>数字</returns>
        public static unsafe int Parse(byte* numbers)
        {
            int value = *numbers;
            uint xor = 0;
            if (value == '-')
            {
                ++numbers;
                xor = uint.MaxValue;
                value = *numbers;
            }
            if ((uint) (value -= '0') < 10)
            {
                for (int number = *++numbers; (uint) (number -= '0') < 10; number = *numbers)
                {
                    value *= 10;
                    ++numbers;
                    value += number;
                }
                return (int) (((uint) value ^ xor) + (xor & 1));
            }
            return 0;
        }
    }
}