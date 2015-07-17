//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: ContentTypePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Web
//	File Name:  ContentTypePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:14:03
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;

namespace TerumoMIS.CoreLibrary.Web
{
    /// <summary>
    ///     下载文件类型属性
    /// </summary>
    public sealed class ContentTypeInfoPlus : Attribute
    {
        /// <summary>
        ///     扩展名关联下载文件类型
        /// </summary>
        private static readonly StateSearcherPlus<byte[]> contentTypes;

        /// <summary>
        ///     未知扩展名关联下载文件类型
        /// </summary>
        private static readonly byte[] unknownContentType;

        /// <summary>
        ///     文件扩展名
        /// </summary>
        public string ExtensionName;

        /// <summary>
        ///     下载文件类型名称
        /// </summary>
        public string Name;

        static ContentTypeInfoPlus()
        {
            ContentTypeInfoPlus[] types = Enum.GetValues(typeof (ContentTypeEnum))
                .ToArray<ContentTypeEnum>().GetArray(value => EnumPlus<contentType, ContentTypeInfoPlus>.Array(value));
            contentTypes = new StateSearcherPlus.ascii<byte[]>(types.getArray(value => value.ExtensionName),
                types.getArray(value => value.Name.getBytes()));
            unknownContentType = contentTypes.Get("*");
        }

        /// <summary>
        ///     获取扩展名关联下载文件类型
        /// </summary>
        /// <param name="extensionName">扩展名</param>
        /// <returns>扩展名关联下载文件类型</returns>
        public static byte[] GetContentType(string extensionName)
        {
            return contentTypes.Get(extensionName, unknownContentType);
        }
    }

    /// <summary>
    ///     下载文件类型
    /// </summary>
    public enum ContentTypeEnum
    {
        [ContentTypeInfoPlus(ExtensionName = "*", Name = "application/octet-stream")] _,
        [ContentTypeInfoPlus(ExtensionName = "323", Name = "text/h323")] _323,
        [ContentTypeInfoPlus(ExtensionName = "907", Name = "drawing/907")] _907,
        [ContentTypeInfoPlus(ExtensionName = "acp", Name = "audio/x-mei-aac")] acp,
        [ContentTypeInfoPlus(ExtensionName = "ai", Name = "application/postscript")] ai,
        [ContentTypeInfoPlus(ExtensionName = "aif", Name = "audio/aiff")] aif,
        [ContentTypeInfoPlus(ExtensionName = "aifc", Name = "audio/aiff")] aifc,
        [ContentTypeInfoPlus(ExtensionName = "aiff", Name = "audio/aiff")] aiff,
        [ContentTypeInfoPlus(ExtensionName = "asa", Name = "text/asa")] asa,
        [ContentTypeInfoPlus(ExtensionName = "asf", Name = "video/x-ms-asf")] asf,
        [ContentTypeInfoPlus(ExtensionName = "asp", Name = "text/asp")] asp,
        [ContentTypeInfoPlus(ExtensionName = "asx", Name = "video/x-ms-asf")] asx,
        [ContentTypeInfoPlus(ExtensionName = "au", Name = "audio/basic")] au,
        [ContentTypeInfoPlus(ExtensionName = "avi", Name = "video/avi")] avi,
        [ContentTypeInfoPlus(ExtensionName = "awf", Name = "application/vnd.adobe.workflow")] awf,
        [ContentTypeInfoPlus(ExtensionName = "biz", Name = "text/xml")] biz,
        [ContentTypeInfoPlus(ExtensionName = "bmp", Name = "image/msbitmap")] bmp,
        [ContentTypeInfoPlus(ExtensionName = "cat", Name = "application/vnd.ms-pki.seccat")] cat,
        [ContentTypeInfoPlus(ExtensionName = "cdf", Name = "application/x-netcdf")] Cdf,
        [ContentTypeInfoPlus(ExtensionName = "cer", Name = "application/x-x509-ca-cert")] cer,
        [ContentTypeInfoPlus(ExtensionName = "class", Name = "java/*")] _class,
        [ContentTypeInfoPlus(ExtensionName = "cml", Name = "text/xml")] cml,
        [ContentTypeInfoPlus(ExtensionName = "crl", Name = "application/pkix-crl")] crl,
        [ContentTypeInfoPlus(ExtensionName = "crt", Name = "application/x-x509-ca-cert")] crt,
        [ContentTypeInfoPlus(ExtensionName = "css", Name = "text/css")] css,
        [ContentTypeInfoPlus(ExtensionName = "cur", Name = "image/x-icon")] cur,
        [ContentTypeInfoPlus(ExtensionName = "dcd", Name = "text/xml")] dcd,
        [ContentTypeInfoPlus(ExtensionName = "der", Name = "application/x-x509-ca-cert")] der,
        [ContentTypeInfoPlus(ExtensionName = "dll", Name = "application/x-msdownload")] dll,
        [ContentTypeInfoPlus(ExtensionName = "doc", Name = "application/msword")] doc,
        [ContentTypeInfoPlus(ExtensionName = "dot", Name = "application/msword")] dot,
        [ContentTypeInfoPlus(ExtensionName = "dtd", Name = "text/xml")] dtd,
        [ContentTypeInfoPlus(ExtensionName = "edn", Name = "application/vnd.adobe.edn")] edn,
        [ContentTypeInfoPlus(ExtensionName = "eml", Name = "message/rfc822")] eml,
        [ContentTypeInfoPlus(ExtensionName = "ent", Name = "text/xml")] ent,
        [ContentTypeInfoPlus(ExtensionName = "eps", Name = "application/postscript")] eps,
        [ContentTypeInfoPlus(ExtensionName = "exe", Name = "application/x-msdownload")] exe,
        [ContentTypeInfoPlus(ExtensionName = "fax", Name = "image/fax")] fax,
        [ContentTypeInfoPlus(ExtensionName = "fdf", Name = "application/vnd.fdf")] fdf,
        [ContentTypeInfoPlus(ExtensionName = "fif", Name = "application/fractals")] fif,
        [ContentTypeInfoPlus(ExtensionName = "fo", Name = "text/xml")] fo,
        [ContentTypeInfoPlus(ExtensionName = "gif", Name = "image/gif")] gif,
        [ContentTypeInfoPlus(ExtensionName = "hpg", Name = "application/x-hpgl")] hpg,
        [ContentTypeInfoPlus(ExtensionName = "hqx", Name = "application/mac-binhex40")] hqx,
        [ContentTypeInfoPlus(ExtensionName = "hta", Name = "application/hta")] hta,
        [ContentTypeInfoPlus(ExtensionName = "htc", Name = "text/x-component")] htc,
        [ContentTypeInfoPlus(ExtensionName = "htm", Name = "text/html")] htm,
        [ContentTypeInfoPlus(ExtensionName = "html", Name = "text/html")] html,
        [ContentTypeInfoPlus(ExtensionName = "htt", Name = "text/webviewhtml")] htt,
        [ContentTypeInfoPlus(ExtensionName = "htx", Name = "text/html")] htx,
        [ContentTypeInfoPlus(ExtensionName = "ico", Name = "image/x-icon")] ico,
        [ContentTypeInfoPlus(ExtensionName = "iii", Name = "application/x-iphone")] iii,
        [ContentTypeInfoPlus(ExtensionName = "img", Name = "application/x-img")] img,
        [ContentTypeInfoPlus(ExtensionName = "ins", Name = "application/x-internet-signup")] ins,
        [ContentTypeInfoPlus(ExtensionName = "isp", Name = "application/x-internet-signup")] isp,
        [ContentTypeInfoPlus(ExtensionName = "java", Name = "java/*")] java,
        [ContentTypeInfoPlus(ExtensionName = "jfif", Name = "image/jpeg")] jfif,
        [ContentTypeInfoPlus(ExtensionName = "jpe", Name = "image/jpeg")] jpe,
        [ContentTypeInfoPlus(ExtensionName = "jpeg", Name = "image/jpeg")] jpeg,
        [ContentTypeInfoPlus(ExtensionName = "jpg", Name = "image/jpeg")] jpg,
        [ContentTypeInfoPlus(ExtensionName = "js", Name = "application/x-javascript")] js,
        [ContentTypeInfoPlus(ExtensionName = "jsp", Name = "text/html")] jsp,
        [ContentTypeInfoPlus(ExtensionName = "la1", Name = "audio/x-liquid-file")] la1,
        [ContentTypeInfoPlus(ExtensionName = "lar", Name = "application/x-laplayer-reg")] lar,
        [ContentTypeInfoPlus(ExtensionName = "latex", Name = "application/x-latex")] latex,
        [ContentTypeInfoPlus(ExtensionName = "lavs", Name = "audio/x-liquid-secure")] lavs,
        [ContentTypeInfoPlus(ExtensionName = "lmsff", Name = "audio/x-la-lms")] lmsff,
        [ContentTypeInfoPlus(ExtensionName = "ls", Name = "application/x-javascript")] ls,
        [ContentTypeInfoPlus(ExtensionName = "m1v", Name = "video/x-mpeg")] m1v,
        [ContentTypeInfoPlus(ExtensionName = "m2v", Name = "video/x-mpeg")] m2v,
        [ContentTypeInfoPlus(ExtensionName = "m3u", Name = "audio/mpegurl")] m3u,
        [ContentTypeInfoPlus(ExtensionName = "m4e", Name = "video/mpeg4")] m4e,
        [ContentTypeInfoPlus(ExtensionName = "man", Name = "application/x-troff-man")] man,
        [ContentTypeInfoPlus(ExtensionName = "math", Name = "text/xml")] math,
        [ContentTypeInfoPlus(ExtensionName = "mdb", Name = "application/msaccess")] mdb,
        [ContentTypeInfoPlus(ExtensionName = "mfp", Name = "application/x-shockwave-flash")] mfp,
        [ContentTypeInfoPlus(ExtensionName = "mht", Name = "message/rfc822")] mht,
        [ContentTypeInfoPlus(ExtensionName = "mhtml", Name = "message/rfc822")] mhtml,
        [ContentTypeInfoPlus(ExtensionName = "mid", Name = "audio/mid")] mid,
        [ContentTypeInfoPlus(ExtensionName = "midi", Name = "audio/mid")] midi,
        [ContentTypeInfoPlus(ExtensionName = "mml", Name = "text/xml")] mml,
        [ContentTypeInfoPlus(ExtensionName = "mnd", Name = "audio/x-musicnet-download")] mnd,
        [ContentTypeInfoPlus(ExtensionName = "mns", Name = "audio/x-musicnet-stream")] mns,
        [ContentTypeInfoPlus(ExtensionName = "mocha", Name = "application/x-javascript")] mocha,
        [ContentTypeInfoPlus(ExtensionName = "movie", Name = "video/x-sgi-movie")] movie,
        [ContentTypeInfoPlus(ExtensionName = "mp1", Name = "audio/mp1")] mp1,
        [ContentTypeInfoPlus(ExtensionName = "mp2", Name = "audio/mp2")] mp2,
        [ContentTypeInfoPlus(ExtensionName = "mp2v", Name = "video/mpeg")] mp2v,
        [ContentTypeInfoPlus(ExtensionName = "mp3", Name = "audio/mp3")] mp3,
        [ContentTypeInfoPlus(ExtensionName = "mp4", Name = "video/mpeg4")] mp4,
        [ContentTypeInfoPlus(ExtensionName = "mpa", Name = "video/x-mpg")] mpa,
        [ContentTypeInfoPlus(ExtensionName = "mpd", Name = "application/vnd.ms-project")] mpd,
        [ContentTypeInfoPlus(ExtensionName = "mpe", Name = "video/x-mpeg")] mpe,
        [ContentTypeInfoPlus(ExtensionName = "mpeg", Name = "video/mpg")] mpeg,
        [ContentTypeInfoPlus(ExtensionName = "mpg", Name = "video/mpg")] mpg,
        [ContentTypeInfoPlus(ExtensionName = "mpga", Name = "audio/rn-mpeg")] mpga,
        [ContentTypeInfoPlus(ExtensionName = "mpp", Name = "application/vnd.ms-project")] mpp,
        [ContentTypeInfoPlus(ExtensionName = "mps", Name = "video/x-mpeg")] mps,
        [ContentTypeInfoPlus(ExtensionName = "mpt", Name = "application/vnd.ms-project")] mpt,
        [ContentTypeInfoPlus(ExtensionName = "mpv", Name = "video/mpg")] mpv,
        [ContentTypeInfoPlus(ExtensionName = "mpv2", Name = "video/mpeg")] mpv2,
        [ContentTypeInfoPlus(ExtensionName = "mpw", Name = "application/vnd.ms-project")] mpw,
        [ContentTypeInfoPlus(ExtensionName = "mpx", Name = "application/vnd.ms-project")] mpx,
        [ContentTypeInfoPlus(ExtensionName = "mtx", Name = "text/xml")] mtx,
        [ContentTypeInfoPlus(ExtensionName = "mxp", Name = "application/x-mmxp")] mxp,
        [ContentTypeInfoPlus(ExtensionName = "net", Name = "image/pnetvue")] net,
        [ContentTypeInfoPlus(ExtensionName = "nws", Name = "message/rfc822")] nws,
        [ContentTypeInfoPlus(ExtensionName = "odc", Name = "text/x-ms-odc")] odc,
        [ContentTypeInfoPlus(ExtensionName = "p10", Name = "application/pkcs10")] p10,
        [ContentTypeInfoPlus(ExtensionName = "p12", Name = "application/x-pkcs12")] p12,
        [ContentTypeInfoPlus(ExtensionName = "p7b", Name = "application/x-pkcs7-certificates")] p7b,
        [ContentTypeInfoPlus(ExtensionName = "p7c", Name = "application/pkcs7-mime")] p7c,
        [ContentTypeInfoPlus(ExtensionName = "p7m", Name = "application/pkcs7-mime")] p7m,
        [ContentTypeInfoPlus(ExtensionName = "p7r", Name = "application/x-pkcs7-certreqresp")] p7r,
        [ContentTypeInfoPlus(ExtensionName = "p7s", Name = "application/pkcs7-signature")] p7s,
        [ContentTypeInfoPlus(ExtensionName = "pcx", Name = "image/x-pcx")] pcx,
        [ContentTypeInfoPlus(ExtensionName = "pdf", Name = "application/pdf")] pdf,
        [ContentTypeInfoPlus(ExtensionName = "pdx", Name = "application/vnd.adobe.pdx")] pdx,
        [ContentTypeInfoPlus(ExtensionName = "pfx", Name = "application/x-pkcs12")] pfx,
        [ContentTypeInfoPlus(ExtensionName = "pic", Name = "application/x-pic")] pic,
        [ContentTypeInfoPlus(ExtensionName = "pko", Name = "application/vnd.ms-pki.pko")] pko,
        [ContentTypeInfoPlus(ExtensionName = "pl", Name = "application/x-perl")] pl,
        [ContentTypeInfoPlus(ExtensionName = "plg", Name = "text/html")] plg,
        [ContentTypeInfoPlus(ExtensionName = "pls", Name = "audio/scpls")] pls,
        [ContentTypeInfoPlus(ExtensionName = "png", Name = "image/png")] png,
        [ContentTypeInfoPlus(ExtensionName = "pot", Name = "application/vnd.ms-powerpoint")] pot,
        [ContentTypeInfoPlus(ExtensionName = "ppa", Name = "application/vnd.ms-powerpoint")] ppa,
        [ContentTypeInfoPlus(ExtensionName = "pps", Name = "application/vnd.ms-powerpoint")] pps,
        [ContentTypeInfoPlus(ExtensionName = "ppt", Name = "application/vnd.ms-powerpoint")] ppt,
        [ContentTypeInfoPlus(ExtensionName = "prf", Name = "application/pics-rules")] prf,
        [ContentTypeInfoPlus(ExtensionName = "ps", Name = "application/postscript")] ps,
        [ContentTypeInfoPlus(ExtensionName = "pwz", Name = "application/vnd.ms-powerpoint")] pwz,
        [ContentTypeInfoPlus(ExtensionName = "r3t", Name = "text/vnd.rn-realtext3d")] r3t,
        [ContentTypeInfoPlus(ExtensionName = "ra", Name = "audio/vnd.rn-realaudio")] ra,
        [ContentTypeInfoPlus(ExtensionName = "ram", Name = "audio/x-pn-realaudio")] ram,
        [ContentTypeInfoPlus(ExtensionName = "rat", Name = "application/rat-file")] rat,
        [ContentTypeInfoPlus(ExtensionName = "rdf", Name = "text/xml")] rdf,
        [ContentTypeInfoPlus(ExtensionName = "rec", Name = "application/vnd.rn-recording")] rec,
        [ContentTypeInfoPlus(ExtensionName = "rjs", Name = "application/vnd.rn-realsystem-rjs")] rjs,
        [ContentTypeInfoPlus(ExtensionName = "rjt", Name = "application/vnd.rn-realsystem-rjt")] rjt,
        [ContentTypeInfoPlus(ExtensionName = "rm", Name = "application/vnd.rn-realmedia")] rm,
        [ContentTypeInfoPlus(ExtensionName = "rmf", Name = "application/vnd.adobe.rmf")] rmf,
        [ContentTypeInfoPlus(ExtensionName = "rmi", Name = "audio/mid")] rmi,
        [ContentTypeInfoPlus(ExtensionName = "rmj", Name = "application/vnd.rn-realsystem-rmj")] rmj,
        [ContentTypeInfoPlus(ExtensionName = "rmm", Name = "audio/x-pn-realaudio")] rmm,
        [ContentTypeInfoPlus(ExtensionName = "rmp", Name = "application/vnd.rn-rn_music_package")] rmp,
        [ContentTypeInfoPlus(ExtensionName = "rms", Name = "application/vnd.rn-realmedia-secure")] rms,
        [ContentTypeInfoPlus(ExtensionName = "rmvb", Name = "application/vnd.rn-realmedia-vbr")] rmvb,
        [ContentTypeInfoPlus(ExtensionName = "rmx", Name = "application/vnd.rn-realsystem-rmx")] rmx,
        [ContentTypeInfoPlus(ExtensionName = "rnx", Name = "application/vnd.rn-realplayer")] rnx,
        [ContentTypeInfoPlus(ExtensionName = "rp", Name = "image/vnd.rn-realpix")] rp,
        [ContentTypeInfoPlus(ExtensionName = "rpm", Name = "audio/x-pn-realaudio-plugin")] rpm,
        [ContentTypeInfoPlus(ExtensionName = "rsml", Name = "application/vnd.rn-rsml")] rsml,
        [ContentTypeInfoPlus(ExtensionName = "rt", Name = "text/vnd.rn-realtext")] rt,
        [ContentTypeInfoPlus(ExtensionName = "rtf", Name = "application/msword")] rtf,
        [ContentTypeInfoPlus(ExtensionName = "rv", Name = "video/vnd.rn-realvideo")] rv,
        [ContentTypeInfoPlus(ExtensionName = "sit", Name = "application/x-stuffit")] sit,
        [ContentTypeInfoPlus(ExtensionName = "smi", Name = "application/smil")] smi,
        [ContentTypeInfoPlus(ExtensionName = "smil", Name = "application/smil")] smil,
        [ContentTypeInfoPlus(ExtensionName = "snd", Name = "audio/basic")] snd,
        [ContentTypeInfoPlus(ExtensionName = "sol", Name = "text/plain")] sol,
        [ContentTypeInfoPlus(ExtensionName = "sor", Name = "text/plain")] sor,
        [ContentTypeInfoPlus(ExtensionName = "spc", Name = "application/x-pkcs7-certificates")] spc,
        [ContentTypeInfoPlus(ExtensionName = "spl", Name = "application/futuresplash")] spl,
        [ContentTypeInfoPlus(ExtensionName = "spp", Name = "text/xml")] spp,
        [ContentTypeInfoPlus(ExtensionName = "ssm", Name = "application/streamingmedia")] ssm,
        [ContentTypeInfoPlus(ExtensionName = "sst", Name = "application/vnd.ms-pki.certstore")] sst,
        [ContentTypeInfoPlus(ExtensionName = "stl", Name = "application/vnd.ms-pki.stl")] stl,
        [ContentTypeInfoPlus(ExtensionName = "stm", Name = "text/html")] stm,
        [ContentTypeInfoPlus(ExtensionName = "svg", Name = "text/xml")] svg,
        [ContentTypeInfoPlus(ExtensionName = "swf", Name = "application/x-shockwave-flash")] swf,
        [ContentTypeInfoPlus(ExtensionName = "tif", Name = "image/tiff")] tif,
        [ContentTypeInfoPlus(ExtensionName = "tiff", Name = "image/tiff")] tiff,
        [ContentTypeInfoPlus(ExtensionName = "tld", Name = "text/xml")] tld,
        [ContentTypeInfoPlus(ExtensionName = "torrent", Name = "application/x-bittorrent")] torrent,
        [ContentTypeInfoPlus(ExtensionName = "tsd", Name = "text/xml")] tsd,
        [ContentTypeInfoPlus(ExtensionName = "txt", Name = "text/plain")] txt,
        [ContentTypeInfoPlus(ExtensionName = "uin", Name = "application/x-icq")] uin,
        [ContentTypeInfoPlus(ExtensionName = "uls", Name = "text/iuls")] uls,
        [ContentTypeInfoPlus(ExtensionName = "vcf", Name = "text/x-vcard")] vcf,
        [ContentTypeInfoPlus(ExtensionName = "vdx", Name = "application/vnd.visio")] vdx,
        [ContentTypeInfoPlus(ExtensionName = "vml", Name = "text/xml")] vml,
        [ContentTypeInfoPlus(ExtensionName = "vpg", Name = "application/x-vpeg005")] vpg,
        [ContentTypeInfoPlus(ExtensionName = "vsd", Name = "application/vnd.visio")] vsd,
        [ContentTypeInfoPlus(ExtensionName = "vss", Name = "application/vnd.visio")] vss,
        [ContentTypeInfoPlus(ExtensionName = "vst", Name = "application/vnd.visio")] vst,
        [ContentTypeInfoPlus(ExtensionName = "vsw", Name = "application/vnd.visio")] vsw,
        [ContentTypeInfoPlus(ExtensionName = "vsx", Name = "application/vnd.visio")] vsx,
        [ContentTypeInfoPlus(ExtensionName = "vtx", Name = "application/vnd.visio")] vtx,
        [ContentTypeInfoPlus(ExtensionName = "vxml", Name = "text/xml")] vxml,
        [ContentTypeInfoPlus(ExtensionName = "wav", Name = "audio/wav")] wav,
        [ContentTypeInfoPlus(ExtensionName = "wax", Name = "audio/x-ms-wax")] wax,
        [ContentTypeInfoPlus(ExtensionName = "wbmp", Name = "image/vnd.wap.wbmp")] wbmp,
        [ContentTypeInfoPlus(ExtensionName = "wiz", Name = "application/msword")] wiz,
        [ContentTypeInfoPlus(ExtensionName = "wm", Name = "video/x-ms-wm")] wm,
        [ContentTypeInfoPlus(ExtensionName = "wma", Name = "audio/x-ms-wma")] wma,
        [ContentTypeInfoPlus(ExtensionName = "wmd", Name = "application/x-ms-wmd")] wmd,
        [ContentTypeInfoPlus(ExtensionName = "wml", Name = "text/vnd.wap.wml")] wml,
        [ContentTypeInfoPlus(ExtensionName = "wmv", Name = "video/x-ms-wmv")] wmv,
        [ContentTypeInfoPlus(ExtensionName = "wmx", Name = "video/x-ms-wmx")] wmx,
        [ContentTypeInfoPlus(ExtensionName = "wmz", Name = "application/x-ms-wmz")] wmz,
        [ContentTypeInfoPlus(ExtensionName = "wpl", Name = "application/vnd.ms-wpl")] wpl,
        [ContentTypeInfoPlus(ExtensionName = "wsc", Name = "text/scriptlet")] wsc,
        [ContentTypeInfoPlus(ExtensionName = "wsdl", Name = "text/xml")] wsdl,
        [ContentTypeInfoPlus(ExtensionName = "wvx", Name = "video/x-ms-wvx")] wvx,
        [ContentTypeInfoPlus(ExtensionName = "xdp", Name = "application/vnd.adobe.xdp")] xdp,
        [ContentTypeInfoPlus(ExtensionName = "xdr", Name = "text/xml")] xdr,
        [ContentTypeInfoPlus(ExtensionName = "xfd", Name = "application/vnd.adobe.xfd")] xfd,
        [ContentTypeInfoPlus(ExtensionName = "xfdf", Name = "application/vnd.adobe.xfdf")] xfdf,
        [ContentTypeInfoPlus(ExtensionName = "xhtml", Name = "text/html")] xhtml,
        [ContentTypeInfoPlus(ExtensionName = "xls", Name = "application/vnd.ms-excel")] xls,
        [ContentTypeInfoPlus(ExtensionName = "xml", Name = "text/xml")] xml,
        [ContentTypeInfoPlus(ExtensionName = "xpl", Name = "audio/scpls")] xpl,
        [ContentTypeInfoPlus(ExtensionName = "xq", Name = "text/xml")] xq,
        [ContentTypeInfoPlus(ExtensionName = "xql", Name = "text/xml")] xql,
        [ContentTypeInfoPlus(ExtensionName = "xquery", Name = "text/xml")] xquery,
        [ContentTypeInfoPlus(ExtensionName = "xsd", Name = "text/xml")] xsd,
        [ContentTypeInfoPlus(ExtensionName = "xsl", Name = "text/xml")] xsl,
        [ContentTypeInfoPlus(ExtensionName = "xslt", Name = "text/xml")] xslt
    }
}