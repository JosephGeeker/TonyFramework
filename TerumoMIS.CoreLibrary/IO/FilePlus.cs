//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FilePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.IO
//	File Name:  FilePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:00:19
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerumoMIS.CoreLibrary.IO
{
    /// <summary>
    /// 文件操作
    /// </summary>
    public static class FilePlus
    {
        /// <summary>
        /// 文件编码BOM唯一哈希
        /// </summary>
        private struct encodingBom : IEquatable<encodingBom>
        {
            /// <summary>
            /// 文件编码
            /// </summary>
            public Encoding Encoding;
            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="encoding">文件编码</param>
            /// <returns>文件编码BOM唯一哈希</returns>
            public static implicit operator encodingBom(Encoding encoding) { return new encodingBom { Encoding = encoding }; }
            /// <summary>
            /// 获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public unsafe override int GetHashCode()
            {
                int codePage = Encoding.CodePage;
                return ((codePage >> 3) | codePage) & 3;
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public unsafe bool Equals(encodingBom other)
            {
                return Encoding == other.Encoding;
            }
            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((encodingBom)obj);
            }
        }
        /// <summary>
        /// 文件编码BOM
        /// </summary>
        public struct bom
        {
            /// <summary>
            /// BOM
            /// </summary>
            public uint Bom;
            /// <summary>
            /// BOM长度
            /// </summary>
            public int Length;
        }
        /// <summary>
        /// 文件编码BOM集合
        /// </summary>
        private static readonly uniqueDictionary<encodingBom, bom> boms;
        /// <summary>
        /// 完全限定文件名必须少于 260 个字符
        /// </summary>
        public const int MaxFullNameLength = 260;
        /// <summary>
        /// 默认簇字节大小
        /// </summary>
        public static readonly uint DefaultBytesPerCluster;
        /// <summary>
        /// 根据磁盘根目录获取簇字节大小
        /// </summary>
        /// <param name="bootPath">磁盘根目录，如@"C:\"</param>
        /// <returns>簇字节大小</returns>
        public static uint BytesPerCluster(string bootPath)
        {
#if MONO
#else
            if (bootPath.Length >= 3 && bootPath[1] == ':')
            {
                uint value = bytesPerCluster(bootPath[bootPath.Length - 1] == '\\' ? bootPath : bootPath.Substring(0, 3));
                if (value != 0) return value;
            }
#endif
            return DefaultBytesPerCluster;
        }
        /// <summary>
        /// 根据磁盘根目录获取簇字节大小
        /// </summary>
        /// <param name="bootPath">磁盘根目录，如@"C:\"</param>
        /// <returns>簇字节大小</returns>
        private static uint bytesPerCluster(string bootPath)
        {
#if MONO
#else
            uint sectorsPerCluster, bytesPerSector, numberOfFreeClusters, totalNumberOfClusters;
            if (kernel32.GetDiskFreeSpace(bootPath, out sectorsPerCluster, out bytesPerSector, out numberOfFreeClusters, out totalNumberOfClusters))
            {
                return sectorsPerCluster * bytesPerSector;
            }
#endif
            return 0;
        }
        /// <summary>
        /// 修改文件名成为默认备份文件 %XXX_fileName
        /// </summary>
        /// <param name="fileName">源文件名</param>
        /// <returns>备份文件名称,失败返回null</returns>
        public static string MoveBak(string fileName)
        {
            if (File.Exists(fileName))
            {
                string newFileName = MoveBakFileName(fileName);
                File.Move(fileName, newFileName);
                return newFileName;
            }
            return null;
        }
        /// <summary>
        /// 获取备份文件名称 %XXX_fileName
        /// </summary>
        /// <param name="fileName">源文件名</param>
        /// <returns>备份文件名称</returns>
        internal static string MoveBakFileName(string fileName)
        {
            int timeIndex = 0;
            string newFileName = null;
            int index = fileName.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            do
            {
                string bakName = "%" + date.NowSecond.ToString("yyyyMMdd-HHmmss") + (timeIndex == 0 ? null : ("_" + timeIndex.toString())) + "_";
                newFileName = index != 0 ? fileName.Insert(index, bakName) : (bakName + fileName);
                ++timeIndex;
            }
            while (File.Exists(newFileName));
            return newFileName;
        }
        /// <summary>
        /// 根据文件编码获取BOM
        /// </summary>
        /// <param name="encoding">文件编码</param>
        /// <returns>文件编码BOM</returns>
        public static bom GetBom(Encoding encoding)
        {
            return boms.Get(encoding, default(bom));
        }
        static file()
        {
            keyValue<encodingBom, bom>[] bomList = new keyValue<encodingBom, bom>[4];
            int count = 0;
            bomList[count++] = new keyValue<encodingBom, bom>(Encoding.Unicode, new bom { Bom = 0xfeffU, Length = 2 });
            bomList[count++] = new keyValue<encodingBom,bom>(Encoding.BigEndianUnicode, new bom { Bom = 0xfffeU, Length = 2 });
            bomList[count++] = new keyValue<encodingBom,bom>(Encoding.UTF8, new bom { Bom = 0xbfbbefU, Length = 3 });
            bomList[count++] = new keyValue<encodingBom,bom>(Encoding.UTF32, new bom { Bom = 0xfeffU, Length = 4 });
            boms = new uniqueDictionary<encodingBom, bom>(bomList, 4);

            DefaultBytesPerCluster = bytesPerCluster(AppDomain.CurrentDomain.BaseDirectory);
            if (DefaultBytesPerCluster == 0) DefaultBytesPerCluster = 1 << 12;
        }
    }
}
