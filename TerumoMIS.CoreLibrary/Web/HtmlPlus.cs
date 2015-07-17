//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: HtmlPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Web
//	File Name:  HtmlPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:17:09
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using TerumoMIS.CoreLibrary.Unsafe;

namespace TerumoMIS.CoreLibrary.Web
{
    /// <summary>
    ///     Html代码参数及相关操作
    /// </summary>
    public static class HtmlPlus
    {
        /// <summary>
        ///     字符集类型
        /// </summary>
        public enum CharsetTypeEnum
        {
            /// <summary>
            ///     UTF-8
            /// </summary>
            [CharsetInfoPlus(CharsetString = "UTF-8")] Utf8,

            /// <summary>
            ///     GB2312
            /// </summary>
            [CharsetInfoPlus(CharsetString = "GB2312")] Gb2312
        }

        /// <summary>
        ///     标准引用类型
        /// </summary>
        public enum DocTypeEnum
        {
            /// <summary>
            ///     过渡(HTML4.01)
            /// </summary>
            [DocInfoPlus(
                Html =
                    @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">"
                )] Transitional = 0,

            /// <summary>
            ///     严格(不能使用任何表现层的标识和属性，例如<br />)
            /// </summary>
            [DocInfoPlus(
                Html =
                    @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">"
                )] Strict,

            /// <summary>
            ///     框架(专门针对框架页面设计使用的DTD，如果你的页面中包含有框架，需要采用这种DTD)
            /// </summary>
            [DocInfoPlus(
                Html =
                    @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Frameset//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd"">"
                )] Frameset,

            /// <summary>
            /// </summary>
            Xhtml11,

            /// <summary>
            ///     HTML5
            /// </summary>
            [DocInfoPlus(Html = @"<!DOCTYPE html>")] Html5
        }

        /// <summary>
        ///     注释开始
        /// </summary>
        public const string NoteStart = @"
<![CDATA[
";

        /// <summary>
        ///     注释结束
        /// </summary>
        public const string NoteEnd = @"
]]>
";

        /// <summary>
        ///     javscript开始
        /// </summary>
        public const string JsStart = @"
<script language=""javascript"" type=""text/javascript"">
//<![CDATA[
";

        /// <summary>
        ///     javscript结束
        /// </summary>
        public const string JsEnd = @"
//]]>
</script>
";

        /// <summary>
        ///     style开始
        /// </summary>
        public const string StyleStart = @"
<style type=""text/css"">
<![CDATA[
";

        /// <summary>
        ///     style结束
        /// </summary>
        public const string StyletEnd = @"
]]>
</style>
";

        /// <summary>
        ///     标准文档集合
        /// </summary>
        private static readonly DocInfoPlus[] DocTypes = EnumPlus.GetAttributes<DocTypeEnum, DocInfoPlus>();

        /// <summary>
        ///     字符集类型名称集合
        /// </summary>
        private static readonly CharsetInfoPlus[] CharsetTypes =
            EnumPlus.GetAttributes<CharsetTypeEnum, CharsetInfoPlus>();

        /// <summary>
        ///     允许不回合的标签名称集合
        /// </summary>
        public static readonly UniqueHashSetPlus<CanNonRoundTagNameStruct> CanNonRoundTagNames =
            new UniqueHashSetPlus<CanNonRoundTagNameStruct>(
                new CanNonRoundTagNameStruct[]
                {"area", "areatext", "basefont", "br", "col", "colgroup", "hr", "img", "input", "li", "p", "spacer"}, 27);

        /// <summary>
        ///     必须回合的标签名称集合
        /// </summary>
        public static readonly UniqueHashSetPlus<MustRoundTagNameStruct> MustRoundTagNames =
            new UniqueHashSetPlus<MustRoundTagNameStruct>(
                new MustRoundTagNameStruct[]
                {
                    "a", "b", "bgsound", "big", "body", "button", "caption", "center", "div", "em", "embed", "font", "form",
                    "h1", "h2", "h3", "h4", "h5", "h6", "hn", "html", "i", "iframe", "map", "marquee", "multicol",
                    "nobr", "ol", "option", "pre", "s", "select", "small", "span", "strike", "strong", "sub", "sup",
                    "table", "tbody", "td", "textarea", "tfoot", "th", "thead", "tr", "u", "ul"
                }, 239);

        /// <summary>
        ///     脚本安全属性名称集合
        /// </summary>
        public static readonly UniqueHashSetPlus<SafeAttributeStruct> SafeAttributes =
            new UniqueHashSetPlus<SafeAttributeStruct>(
                new SafeAttributeStruct[]
                {
                    "align", "allowtransparency", "alt", "behavior", "bgcolor", "border", "bordercolor", "bordercolordark",
                    "bordercolorlight", "cellpadding", "cellspacing", "checked", "class", "clear", "color", "cols",
                    "colspan", "controls", "coords", "direction", "face", "frame", "frameborder", "gutter", "height",
                    "hspace", "loop", "loopdelay", "marginheight", "marginwidth", "maxlength", "method", "multiple",
                    "rows", "rowspan", "rules", "scrollamount", "scrolldelay", "scrolling", "selected", "shape", "size",
                    "span", "start", "target", "title", "type", "unselectable", "usemap", "valign", "value", "vspace",
                    "width", "wrap"
                }, 253);

        /// <summary>
        ///     URI属性名称集合
        /// </summary>
        public static readonly UniqueHashSetPlus<UriAttributeStruct> UriAttributes =
            new UniqueHashSetPlus<UriAttributeStruct>(new UriAttributeStruct[] {"background", "dynsrc", "href", "src"},
                5);

        /// <summary>
        ///     安全样式名称集合
        /// </summary>
        public static readonly UniqueHashSetPlus<SafeStyleAttributeStruct> SafeStyleAttributes =
            new UniqueHashSetPlus<SafeStyleAttributeStruct>(
                new SafeStyleAttributeStruct[]
                {"font", "font-family", "font-size", "font-weight", "color", "text-decoration"}, 8);

        /// <summary>
        ///     非解析标签名称集合
        /// </summary>
        public static readonly UniqueHashSetPlus<NonanalyticTagNameStruct> NonanalyticTagNames =
            new UniqueHashSetPlus<NonanalyticTagNameStruct>(
                new NonanalyticTagNameStruct[] {"script", "style", "!", "/"}, 6);

        /// <summary>
        ///     非文本标签名称集合
        /// </summary>
        public static readonly UniqueHashSetPlus<NoTextTagNameStruct> NoTextTagNames =
            new UniqueHashSetPlus<NoTextTagNameStruct>(
                new NoTextTagNameStruct[]
                {"script", "style", "pre", "areatext", "!", "/", "input", "iframe", "img", "link", "head"}, 15);

        /// <summary>
        ///     默认HTML编码器
        /// </summary>
        internal static readonly EncoderStruct HtmlEncoder = new EncoderStruct(@"& <>""'");

        /// <summary>
        ///     TextArea编码器
        /// </summary>
        internal static readonly EncoderStruct TextAreaEncoder = new EncoderStruct(@"&<>");

        /// <summary>
        ///     默认HTML编码器
        /// </summary>
        public static IEncoder HtmlIEncoder
        {
            get { return HtmlEncoder; }
        }

        /// <summary>
        ///     TextArea编码器
        /// </summary>
        public static IEncoder TextAreaIEncoder
        {
            get { return TextAreaEncoder; }
        }

        /// <summary>
        ///     获取标准引用代码
        /// </summary>
        /// <returns>文档类型</returns>
        public static string GetHtml(this DocTypeEnum type)
        {
            var typeIndex = (int) type;
            if (typeIndex < 0 || typeIndex >= DocTypes.Length) typeIndex = 0;
            return DocTypes[typeIndex].Html + @"
<html xmlns=""http://www.w3.org/1999/xhtml"">
";
        }

        /// <summary>
        ///     获取字符集代码
        /// </summary>
        /// <returns>字符集代码</returns>
        public static string GetHtml(this CharsetTypeEnum type)
        {
            var typeIndex = (int) type;
            if (typeIndex >= CharsetTypes.Length) typeIndex = -1;
            var html = string.Empty;
            if (typeIndex >= 0)
                html = @"<meta http-equiv=""content-type"" content=""text/html; charset=" +
                       CharsetTypes[typeIndex].CharsetString + @""">
";
            return html;
        }

        /// <summary>
        ///     加载js文件
        /// </summary>
        /// <param name="fileName">被加载的js文件地址</param>
        /// <returns>加载js文件的HTML代码</returns>
        public static string JsFile(string fileName)
        {
            return @"<script language=""javascript"" type=""text/javascript"" src=""" + fileName + @"""></script>";
        }

        /// <summary>
        ///     加载css文件
        /// </summary>
        /// <param name="fileName">被加载的css文件地址</param>
        /// <returns>加载css文件的HTML代码</returns>
        public static string CssFile(string fileName)
        {
            return @"<style type=""text/css"" link=""" + fileName + @"""></style>";
        }

        /// <summary>
        ///     文档类型属性
        /// </summary>
        public sealed class DocInfoPlus : Attribute
        {
            /// <summary>
            ///     标准文档类型头部
            /// </summary>
            public string Html;
        }

        /// <summary>
        ///     字符集类型属性
        /// </summary>
        public sealed class CharsetInfoPlus : Attribute
        {
            /// <summary>
            ///     字符串表示
            /// </summary>
            public string CharsetString;
        }

        /// <summary>
        ///     允许不回合的标签名称唯一哈希
        /// </summary>
        public struct CanNonRoundTagNameStruct : IEquatable<CanNonRoundTagNameStruct>
        {
            /// <summary>
            ///     允许不回合的标签名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(CanNonRoundTagNameStruct other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">允许不回合的标签名称</param>
            /// <returns>允许不回合的标签名称唯一哈希</returns>
            public static implicit operator CanNonRoundTagNameStruct(string name)
            {
                return new CanNonRoundTagNameStruct {Name = name};
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length == 0) return 1;
                var code = (Name[Name.Length - 1] << 7) + Name[0];
                return ((code >> 5) ^ code) & ((1 << 5) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((CanNonRoundTagNameStruct) obj);
            }
        }

        /// <summary>
        ///     必须回合的标签名称唯一哈希
        /// </summary>
        public struct MustRoundTagNameStruct : IEquatable<MustRoundTagNameStruct>
        {
            /// <summary>
            ///     必须回合的标签名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(MustRoundTagNameStruct other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">必须回合的标签名称</param>
            /// <returns>必须回合的标签名称唯一哈希</returns>
            public static implicit operator MustRoundTagNameStruct(string name)
            {
                return new MustRoundTagNameStruct {Name = name};
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length == 0) return 0;
                var code = (Name[Name.Length >> 1] << 14) + (Name[0] << 7) + Name[Name.Length - 1];
                return ((code >> 15) ^ (code >> 13) ^ (code >> 1)) & ((1 << 8) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((MustRoundTagNameStruct) obj);
            }
        }

        /// <summary>
        ///     脚本安全属性名称
        /// </summary>
        public struct SafeAttributeStruct : IEquatable<SafeAttributeStruct>
        {
            /// <summary>
            ///     脚本安全属性名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(SafeAttributeStruct other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">脚本安全属性名称</param>
            /// <returns>脚本安全属性名称唯一哈希</returns>
            public static implicit operator SafeAttributeStruct(string name)
            {
                return new SafeAttributeStruct {Name = name};
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length < 3) return 0;
                var code = (Name[Name.Length - 2] << 14) + (Name[Name.Length >> 1] << 7) + Name[Name.Length >> 3];
                return ((code >> 8) ^ (code >> 3) ^ (code >> 1)) & ((1 << 8) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((SafeAttributeStruct) obj);
            }
        }

        /// <summary>
        ///     URI属性名称唯一哈希
        /// </summary>
        public struct UriAttributeStruct : IEquatable<UriAttributeStruct>
        {
            /// <summary>
            ///     URI属性名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(UriAttributeStruct other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">URI属性名称</param>
            /// <returns>URI属性名称唯一哈希</returns>
            public static implicit operator UriAttributeStruct(string name)
            {
                return new UriAttributeStruct {Name = name};
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return Name[0] & 7;
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((UriAttributeStruct) obj);
            }
        }

        /// <summary>
        ///     安全样式名称唯一哈希
        /// </summary>
        public struct SafeStyleAttributeStruct : IEquatable<SafeStyleAttributeStruct>
        {
            /// <summary>
            ///     安全样式名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(SafeStyleAttributeStruct other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">安全样式名称</param>
            /// <returns>安全样式名称唯一哈希</returns>
            public static implicit operator SafeStyleAttributeStruct(string name)
            {
                return new SafeStyleAttributeStruct {Name = name};
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return Name.Length < 4 ? 0 : Name[Name.Length - 4] & 7;
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((SafeStyleAttributeStruct) obj);
            }
        }

        /// <summary>
        ///     非解析标签名称唯一哈希
        /// </summary>
        public struct NonanalyticTagNameStruct : IEquatable<NonanalyticTagNameStruct>
        {
            /// <summary>
            ///     非解析标签名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(NonanalyticTagNameStruct other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">非解析标签名称</param>
            /// <returns>非解析标签名称唯一哈希</returns>
            public static implicit operator NonanalyticTagNameStruct(string name)
            {
                return new NonanalyticTagNameStruct {Name = name};
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length == 0) return 2;
                return (Name[Name.Length - 1] >> 2) & ((1 << 3) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((NonanalyticTagNameStruct) obj);
            }
        }

        /// <summary>
        ///     非文本标签名称唯一哈希
        /// </summary>
        public struct NoTextTagNameStruct : IEquatable<NoTextTagNameStruct>
        {
            /// <summary>
            ///     非文本标签名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(NoTextTagNameStruct other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">非文本标签名称</param>
            /// <returns>非文本标签名称唯一哈希</returns>
            public static implicit operator NoTextTagNameStruct(string name)
            {
                return new NoTextTagNameStruct {Name = name};
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length == 0) return 5;
                var code = (Name[0] << 7) + Name[Name.Length >> 2];
                return ((code >> 7) ^ (code >> 2)) & ((1 << 4) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((NoTextTagNameStruct) obj);
            }
        }

        /// <summary>
        ///     HTML编码器
        /// </summary>
        public interface IEncoder
        {
            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <returns>HTML编码</returns>
            string ToHtml(string value);

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <returns>HTML编码</returns>
            SubStringStruct ToHtml(SubStringStruct value);

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <param name="stream">HTML编码流</param>
            void ToHtml(SubStringStruct value, UnmanagedStreamPlus stream);
        }

        /// <summary>
        ///     HTML编码器
        /// </summary>
        internal unsafe struct EncoderStruct : IEncoder
        {
            /// <summary>
            ///     HTML转义字符集合
            /// </summary>
            private readonly uint* _htmls;

            /// <summary>
            ///     最大值
            /// </summary>
            private readonly int _size;

            /// <summary>
            ///     HTML编码器
            /// </summary>
            /// <param name="htmls">HTML转义字符集合</param>
            public EncoderStruct(string htmls)
            {
                _size = 0;
                foreach (var htmlChar in htmls)
                {
                    if (htmlChar > _size) _size = htmlChar;
                }
                _htmls = UnmanagedPlus.Get(++_size*sizeof (uint)).UInt;
                foreach (var value in htmls)
                {
                    var div = (value*(int) NumberPlus.Div1016Mul) >> NumberPlus.Div1016Shift;
                    _htmls[value] = (uint) (((value - div*10) << 16) | div | 0x300030);
                }
            }

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <returns>HTML编码</returns>
            public string ToHtml(string value)
            {
                if (value != null)
                {
                    var length = value.Length;
                    fixed (char* valueFixed = value)
                    {
                        var end = valueFixed + length;
                        var count = EncodeCount(valueFixed, end);
                        if (count != 0)
                        {
                            value = StringPlus.FastAllocateString(length + (count << 2));
                            fixed (char* data = value) ToHtml(valueFixed, end, data);
                        }
                    }
                }
                return value;
            }

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <returns>HTML编码</returns>
            public SubStringStruct ToHtml(SubStringStruct value)
            {
                if (value.Length != 0)
                {
                    var length = value.Length;
                    fixed (char* valueFixed = value.Value)
                    {
                        char* start = valueFixed + value.StartIndex, end = start + length;
                        var count = EncodeCount(start, end);
                        if (count != 0)
                        {
                            var newValue = StringPlus.FastAllocateString(length + (count << 2));
                            fixed (char* data = newValue) ToHtml(start, end, data);
                            return SubStringStruct.Unsafe(newValue, 0, newValue.Length);
                        }
                    }
                }
                return value;
            }

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <param name="stream">HTML编码流</param>
            public void ToHtml(SubStringStruct value, UnmanagedStreamPlus stream)
            {
                if (value.Length != 0)
                {
                    if (stream == null) LogPlus.Error.Throw(LogPlus.ExceptionTypeEnum.Null);
                    var length = value.Length;
                    fixed (char* valueFixed = value.Value)
                    {
                        char* start = valueFixed + value.StartIndex, end = start + length;
                        var count = EncodeCount(start, end);
                        if (count == 0)
                        {
                            if (stream != null)
                            {
                                stream.PrepLength(length <<= 1);
                                MemoryUnsafe.Copy(start, stream.CurrentData, length);
                            }
                        }
                        else
                        {
                            length += count << 2;
                            if (stream != null)
                            {
                                stream.PrepLength(length <<= 1);
                                ToHtml(start, end, (char*) stream.CurrentData);
                            }
                        }
                        if (stream != null) stream.Unsafer.AddLength(length);
                    }
                }
            }

            /// <summary>
            ///     HTML转义
            /// </summary>
            /// <param name="start">起始位置</param>
            /// <param name="end">结束位置</param>
            /// <param name="write">写入位置</param>
            private void ToHtml(char* start, char* end, char* write)
            {
                while (start != end)
                {
                    var code = *start++;
                    if (code < _size)
                    {
                        var html = _htmls[code];
                        if (html == 0) *write++ = code;
                        else
                        {
                            *(int*) write = '&' + ('#' << 16);
                            write += 2;
                            *(uint*) write = html;
                            write += 2;
                            *write++ = ';';
                        }
                    }
                    else *write++ = code;
                }
            }

            /// <summary>
            ///     计算编码字符数量
            /// </summary>
            /// <param name="start">起始位置</param>
            /// <param name="end">结束位置</param>
            /// <returns>编码字符数量</returns>
            private int EncodeCount(char* start, char* end)
            {
                var count = 0;
                while (start != end)
                {
                    if (*start < _size && _htmls[*start] != 0) ++count;
                    ++start;
                }
                return count;
            }
        }
    }
}