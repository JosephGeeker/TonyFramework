//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: RequestFormPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Net.TCP.HTTP
//	File Name:  RequestFormPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 15:36:26
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

namespace TerumoMIS.CoreLibrary.Net.TCP.HTTP
{
    /// <summary>
    /// Http请求表单
    /// </summary>
    public sealed partial class RequestFormPlus
    {
        /// <summary>
        /// HTTP请求表单加载接口
        /// </summary>
        public interface ILoadForm
        {
            /// <summary>
            /// 表单回调处理
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            void OnGetForm(requestForm form);
            /// <summary>
            /// 根据HTTP请求表单值获取内存流最大字节数
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>内存流最大字节数</returns>
            int MaxMemoryStreamSize(fastCSharp.net.tcp.http.requestForm.value value);
            /// <summary>
            /// 根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            string GetSaveFileName(fastCSharp.net.tcp.http.requestForm.value value);
        }
        /// <summary>
        /// HTTP请求表单值
        /// </summary>
        public struct value
        {
            /// <summary>
            /// 名称
            /// </summary>
            public subArray<byte> Name;
            /// <summary>
            /// 表单值
            /// </summary>
            public subArray<byte> Value;
            /// <summary>
            /// 客户端文件名称
            /// </summary>
            public subArray<byte> FileName;
            /// <summary>
            /// 服务器端文件名称
            /// </summary>
            public string SaveFileName;
            /// <summary>
            /// 设置文件表单数据
            /// </summary>
            internal void SetFileValue()
            {
                if (SaveFileName != null)
                {
                    try
                    {
                        if (File.Exists(SaveFileName))
                        {
                            byte[] data = File.ReadAllBytes(SaveFileName);
                            Value.UnsafeSet(data, 0, data.Length);
                            File.Delete(SaveFileName);
                        }
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                }
                SaveFileName = null;
            }
            /// <summary>
            /// 保存到目标文件
            /// </summary>
            /// <param name="fileName">目标文件名称</param>
            public void SaveFile(string fileName)
            {
                if (SaveFileName == null)
                {
                    using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 1, FileOptions.WriteThrough))
                    {
                        fileStream.Write(Value.Array, Value.StartIndex, Value.Count);
                    }
                }
                else
                {
                    File.Move(SaveFileName, fileName);
                    SaveFileName = null;
                }
            }
            /// <summary>
            /// 保存图片
            /// </summary>
            /// <param name="fileName">不包含扩展名的图片文件名称</param>
            /// <param name="imageTypes">默认允许上传的图片类型集合</param>
            /// <returns>包含扩展名的图片文件名称,失败返回null</returns>
            public string SaveImage(string fileName, Dictionary<ImageFormat, string> imageTypes = null)
            {
                try
                {
                    string type = null;
                    if (SaveFileName == null)
                    {
                        if (Value.Count != 0)
                        {
                            using (MemoryStream stream = new MemoryStream(Value.Array, Value.StartIndex, Value.Count))
                            using (Image image = Image.FromStream(stream))
                            {
                                (imageTypes ?? defaultImageTypes).TryGetValue(image.RawFormat, out type);
                            }
                        }
                    }
                    else
                    {
                        using (Image image = Image.FromFile(SaveFileName)) (imageTypes ?? defaultImageTypes).TryGetValue(image.RawFormat, out type);
                    }
                    if (type != null)
                    {
                        fileName += "." + type;
                        if (Value.Count == 0)
                        {
                            File.Move(SaveFileName, fileName);
                            SaveFileName = null;
                        }
                        else
                        {
                            using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                            {
                                fileStream.Write(Value.Array, Value.StartIndex, Value.Count);
                            }
                        }
                        return fileName;
                    }
                }
                catch (Exception error)
                {
                    log.Default.Add(error, null, false);
                }
                return null;
            }
            /// <summary>
            /// 清表单数据
            /// </summary>
            internal void Clear()
            {
                Name.Null();
                Value.Null();
                FileName.Null();
                if (SaveFileName != null)
                {
                    try
                    {
                        if (File.Exists(SaveFileName)) File.Delete(SaveFileName);
                    }
                    catch (Exception error)
                    {
                        log.Error.Add(error, null, false);
                    }
                    SaveFileName = null;
                }
            }
            /// <summary>
            /// 清除数据
            /// </summary>
            internal void Null()
            {
                Name.Null();
                Value.Null();
                FileName.Null();
                SaveFileName = null;
            }

            /// <summary>
            /// 默认允许上传的图片扩展名集合
            /// </summary>
            private static readonly string[] defaultImageExtensions = new string[] { "jpeg", "gif", "bmp", "png" };
            /// <summary>
            /// 默认允许上传的图片扩展名集合
            /// </summary>
            public static readonly fastCSharp.stateSearcher.ascii<string> DefaultImageExtensions = new fastCSharp.stateSearcher.ascii<string>(defaultImageExtensions, defaultImageExtensions);
            /// <summary>
            /// 默认允许上传的图片类型集合
            /// </summary>
            private static readonly Dictionary<ImageFormat, string> defaultImageTypes;
            static value()
            {
                defaultImageTypes = dictionary.CreateOnly<ImageFormat, string>();
                defaultImageTypes.Add(ImageFormat.Jpeg, ImageFormat.Jpeg.ToString().toLower());
                defaultImageTypes.Add(ImageFormat.Gif, ImageFormat.Gif.ToString().toLower());
                defaultImageTypes.Add(ImageFormat.Bmp, ImageFormat.Bmp.ToString().toLower());
                defaultImageTypes.Add(ImageFormat.MemoryBmp, ImageFormat.Bmp.ToString().toLower());
                defaultImageTypes.Add(ImageFormat.Png, ImageFormat.Png.ToString().toLower());
            }
        }
        /// <summary>
        /// HTTP操作标识
        /// </summary>
        internal long Identity;
        /// <summary>
        /// 表单数据缓冲区
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// 表单数据集合
        /// </summary>
        internal list<value> FormValues = new list<value>(sizeof(int));
        /// <summary>
        /// 文件集合
        /// </summary>
        public readonly list<value> Files = new list<value>(sizeof(int));
        /// <summary>
        /// JSON字符串
        /// </summary>
        internal string Json;
        /// <summary>
        /// 清除表单数据
        /// </summary>
        internal void Clear()
        {
            clear(FormValues);
            clear(Files);
            Json = null;
        }
        /// <summary>
        /// 解析表单数据
        /// </summary>
        /// <param name="buffer">表单数据缓冲区</param>
        /// <param name="length">表单数据长度</param>
        /// <returns>是否成功</returns>
        internal unsafe bool Parse(byte[] buffer, int length)
        {
            fixed (byte* bufferFixed = buffer)
            {
                byte* current = bufferFixed - 1, end = bufferFixed + length;
                *end = (byte)'&';
                try
                {
                    do
                    {
                        int nameIndex = (int)(++current - bufferFixed);
                        while (*current != '&' && *current != '=') ++current;
                        int nameLength = (int)(current - bufferFixed) - nameIndex;
                        if (*current == '=')
                        {
                            int valueIndex = (int)(++current - bufferFixed);
                            while (*current != '&') ++current;
                            if (nameLength == 1 && buffer[nameIndex] == fastCSharp.config.web.Default.QueryJsonName)
                            {
                                Parse(buffer, valueIndex, (int)(current - bufferFixed) - valueIndex, Encoding.UTF8);//showjim编码问题
                            }
                            else FormValues.Add(new requestForm.value { Name = subArray<byte>.Unsafe(buffer, nameIndex, nameLength), Value = subArray<byte>.Unsafe(buffer, valueIndex, (int)(current - bufferFixed) - valueIndex) });
                        }
                        else if (nameLength != 0)
                        {
                            FormValues.Add(new requestForm.value { Name = subArray<byte>.Unsafe(buffer, nameIndex, nameLength), Value = subArray<byte>.Unsafe(buffer, 0, 0) });
                        }
                    }
                    while (current != end);
                    this.buffer = buffer;
                    return true;
                }
                catch (Exception error)
                {
                    log.Error.Add(error, null, false);
                }
            }
            return false;
        }
        /// <summary>
        /// JSON数据转换成JSON字符串
        /// </summary>
        /// <param name="buffer">数据缓冲区</param>
        /// <param name="startIndex">数据其实位置</param>
        /// <param name="length">JSON数据长度</param>
        /// <param name="encoding">编码</param>
        /// <returns>是否成功</returns>
        internal unsafe bool Parse(byte[] buffer, int startIndex, int length, Encoding encoding)
        {
            if (length == 0)
            {
                Json = string.Empty;
                return true;
            }
            try
            {
                if (encoding == Encoding.Unicode)
                {
                    Json = fastCSharp.String.FastAllocateString(length >> 1);
                    fixed (char* jsonFixed = Json)
                    fixed (byte* bufferFixed = buffer)
                    {
                        fastCSharp.unsafer.memory.Copy(bufferFixed + startIndex, jsonFixed, length);
                    }
                }
                else if (encoding == Encoding.ASCII)
                {
                    fixed (byte* bufferFixed = buffer) Json = fastCSharp.String.DeSerialize(bufferFixed + startIndex, -length);
                }
                else Json = encoding.GetString(buffer, startIndex, length);
                this.buffer = buffer;
                return true;
            }
            catch (Exception error)
            {
                log.Error.Add(error, null, false);
            }
            return false;
        }

        /// <summary>
        /// 设置文件表单数据
        /// </summary>
        internal unsafe void SetFileValue()
        {
            value[] formArray = FormValues.array;
            for (int index = 0, count = FormValues.Count; index != count; ++index) formArray[index].SetFileValue();
        }
        /// <summary>
        /// 清除表单数据
        /// </summary>
        /// <param name="values">表单数据集合</param>
        private static void clear(list<value> values)
        {
            int count = values.Count;
            if (count != 0)
            {
                value[] formArray = values.array;
                for (int index = 0; index != count; ++index) formArray[index].Clear();
                values.Empty();
            }
        }
    }
}
