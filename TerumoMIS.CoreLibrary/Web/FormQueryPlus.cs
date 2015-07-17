//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FormQueryPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Web
//	File Name:  FormQueryPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:15:24
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

namespace TerumoMIS.CoreLibrary.Web
{
    /// <summary>
    ///     Get或Post查询字符串相关操作
    /// </summary>
    public static class FormQueryPlus
    {
        /// <summary>
        ///     模拟javascript编码函数escape
        /// </summary>
        /// <param name="value">原字符串</param>
        /// <returns>escape编码后的字符串</returns>
        public static unsafe string JavascriptEscape(string value)
        {
            if (value.Length != 0)
            {
                fixed (char* valueFixed = value)
                {
                    var end = valueFixed + value.Length;
                    var length = 0;
                    for (var start = valueFixed; start != end; ++start)
                    {
                        if ((*start & 0xff00) == 0)
                        {
                            if ((uint) (*start - '0') >= 10 && (uint) ((*start | 0x20) - 'a') >= 26) length += 2;
                        }
                        else length += 5;
                    }
                    if (length != 0)
                    {
                        var newValue = StringPlus.FastAllocateString(length + value.Length);
                        fixed (char* newValueFixed = newValue)
                        {
                            var write = (byte*) newValueFixed;
                            for (var start = valueFixed; start != end; ++start)
                            {
                                uint charValue = *start;
                                if ((charValue & 0xff00) == 0)
                                {
                                    if (charValue - '0' < 10 || (charValue | 0x20) - 'a' < 26)
                                    {
                                        *(char*) write = (char) charValue;
                                        write += sizeof (char);
                                    }
                                    else
                                    {
                                        var code = charValue >> 4;
                                        *(char*) write = '%';
                                        code += code < 10 ? '0' : (uint) ('0' + 'A' - '9' - 1);
                                        write += sizeof (char);
                                        code += (charValue << 16) & 0xf0000;
                                        *(uint*) write = code +
                                                         (code < 0xa0000
                                                             ? (uint) '0' << 16
                                                             : ((uint) ('0' + 'A' - '9' - 1) << 16));
                                        write += sizeof (uint);
                                    }
                                }
                                else
                                {
                                    var code = charValue >> 12;
                                    *(int*) write = '%' + ('u' << 16);
                                    code += code < 10 ? '0' : (uint) ('0' + 'A' - '9' - 1);
                                    write += sizeof (int);
                                    code += (charValue & 0xf00) << 8;
                                    *(uint*) write = code +
                                                     (code < 0xa0000
                                                         ? (uint) '0' << 16
                                                         : ((uint) ('0' + 'A' - '9' - 1) << 16));
                                    code = (charValue >> 4) & 0xf;
                                    write += sizeof (uint);
                                    code += code < 10 ? '0' : (uint) ('0' + 'A' - '9' - 1);
                                    code += (charValue << 16) & 0xf0000;
                                    *(uint*) write = code +
                                                     (code < 0xa0000
                                                         ? (uint) '0' << 16
                                                         : ((uint) ('0' + 'A' - '9' - 1) << 16));
                                    write += sizeof (uint);
                                }
                            }
                        }
                        return newValue;
                    }
                }
            }
            return value;
        }

        ///// <summary>
        ///// 模拟javascript解码函数unescape
        ///// </summary>
        ///// <param name="value">原字符串</param>
        ///// <returns>unescape解码后的字符串</returns>
        //public static unsafe string JavascriptUnescape(string value)
        //{
        //    if (value != null)
        //    {
        //        fixed (char* valueFixed = value)
        //        {
        //            char* start = valueFixed, end = valueFixed + value.Length;
        //            while (start != end && *start != '%')
        //            {
        //                if (*start == 0) *start = ' ';
        //                ++start;
        //            }
        //            if (start != end)
        //            {
        //                char* write = start;
        //                do
        //                {
        //                    if (*++start == 'u')
        //                    {
        //                        uint code = (uint)(*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
        //                        code <<= 4;
        //                        code += (uint)(*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
        //                        code <<= 4;
        //                        code += (uint)(*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
        //                        code <<= 4;
        //                        code += (uint)(*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
        //                        *write++ = code != 0 ? (char)code : ' ';
        //                    }
        //                    else
        //                    {
        //                        uint code = (uint)(*start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
        //                        code <<= 4;
        //                        code += (uint)(*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
        //                        *write++ = code != 0 ? (char)code : ' ';
        //                    }
        //                    while (++start < end && *start != '%') *write++ = *start != 0 ? *start : ' ';
        //                }
        //                while (start < end);
        //                return new string(valueFixed, 0, (int)(write - valueFixed));
        //            }
        //        }
        //    }
        //    return value;
        //}
        /// <summary>
        ///     模拟javascript解码函数unescape
        /// </summary>
        /// <param name="value">原字符串</param>
        /// <returns>unescape解码后的字符串</returns>
        internal static unsafe SubStringStruct JavascriptUnescape(SubStringStruct value)
        {
            if (value.Length != 0)
            {
                fixed (char* valueFixed = value.Value)
                {
                    char* start = valueFixed + value.StartIndex, end = start + value.Length;
                    while (start != end && *start != '%')
                    {
                        if (*start == 0) *start = ' ';
                        ++start;
                    }
                    if (start != end)
                    {
                        var write = start;
                        do
                        {
                            if (*++start == 'u')
                            {
                                var code =
                                    (uint)
                                        (*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
                                code <<= 4;
                                code +=
                                    (uint)
                                        (*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
                                code <<= 4;
                                code +=
                                    (uint)
                                        (*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
                                code <<= 4;
                                code +=
                                    (uint)
                                        (*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
                                *write++ = code != 0 ? (char) code : ' ';
                            }
                            else
                            {
                                var code =
                                    (uint)
                                        (*start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
                                code <<= 4;
                                code +=
                                    (uint)
                                        (*++start - '0' < 10 ? *start - '0' : ((*start & 0xdf) - ('0' + 'A' - '9' - 1)));
                                *write++ = code != 0 ? (char) code : ' ';
                            }
                            while (++start < end && *start != '%') *write++ = *start != 0 ? *start : ' ';
                        } while (start < end);
                        return SubStringStruct.Unsafe(value.Value, value.StartIndex,
                            (int) (write - valueFixed) - value.StartIndex);
                    }
                }
            }
            return value;
        }

        /// <summary>
        ///     模拟javascript解码函数unescape
        /// </summary>
        /// <param name="value">原字符串</param>
        /// <returns>unescape解码后的字符串</returns>
        internal static unsafe SubStringStruct JavascriptUnescape(SubArrayStruct<byte> value)
        {
            if (value.Count != 0)
            {
                var newValue = StringPlus.FastAllocateString(value.Count);
                fixed (char* newValueFixed = newValue)
                fixed (byte* valueFixed = value.Array)
                {
                    byte* start = valueFixed + value.StartIndex, end = start + value.Count;
                    var write = newValueFixed;
                    while (start != end && *start != '%')
                    {
                        *write++ = *start == 0 ? ' ' : (char) *start;
                        ++start;
                    }
                    if (start != end)
                    {
                        do
                        {
                            if (*++start == 'u')
                            {
                                uint code = (uint) (*++start - '0'), number = (uint) (*++start - '0');
                                if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                                if (number > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
                                code <<= 12;
                                code += (number << 8);
                                if ((number = (uint) (*++start - '0')) > 9)
                                    number = ((number - ('A' - '0')) & 0xffdfU) + 10;
                                code += (number << 4);
                                number = (uint) (*++start - '0');
                                code += (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number);
                                *write++ = code == 0 ? ' ' : (char) code;
                            }
                            else
                            {
                                uint code = (uint) (*start - '0'), number = (uint) (*++start - '0');
                                if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                                code = (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number) + (code << 4);
                                *write++ = code == 0 ? ' ' : (char) code;
                            }
                            while (++start < end && *start != '%') *write++ = *start == 0 ? ' ' : (char) *start;
                        } while (start < end);
                        return SubStringStruct.Unsafe(newValue, 0, (int) (write - newValueFixed));
                    }
                    return SubStringStruct.Unsafe(newValue, 0, value.Count);
                }
            }
            return default(SubStringStruct);
        }
    }
}