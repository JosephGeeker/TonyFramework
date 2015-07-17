//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: FileBlockServerPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.IO
//	File Name:  FileBlockServerPlus
//	User name:  C1400008
//	Location Time: 2015/7/16 13:01:14
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
using TerumoMIS.CoreLibrary.Code.Csharper;

namespace TerumoMIS.CoreLibrary.IO
{
    /// <summary>
    /// 文件分块服务
    /// </summary>
    [TcpServerPlus(Service = "FileBlock",IsIdentityCommand = true,IsServerAsynchronousReceive = false,IsClientAsynchronousReceive = false,VerifyMethodType = typeof(VerifyMethodPlus))]
    public partial class FileBlockServerPlus:IDisposable
    {
        /// <summary>
        /// 文件分块写入流
        /// </summary>
        private fileBlockStream fileStream;
        /// <summary>
        /// 文件分块服务
        /// </summary>
        public fileBlockServer()
        {
            log.Error.Throw(log.exceptionType.ErrorOperation);
        }
        /// <summary>
        /// 文件分块服务
        /// </summary>
        /// <param name="fileName">文件全名</param>
        public fileBlockServer(string fileName)
        {
            fileStream = new fileBlockStream(fileName);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            pub.Dispose(ref fileStream);
        }
        /// <summary>
        /// 文件分块服务验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [fastCSharp.code.cSharp.tcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024)]
        private bool verify(string value)
        {
            if (fastCSharp.config.fileBlock.Default.Verify == null && !fastCSharp.config.pub.Default.IsDebug)
            {
                fastCSharp.log.Error.Add("文件分块服务验证数据不能为空", false, true);
                return false;
            }
            return fastCSharp.config.fileBlock.Default.Verify == value;
        }
        /// <summary>
        /// 读取文件分块数据
        /// </summary>
        /// <param name="index">文件分块数据位置</param>
        /// <param name="buffer">数据缓冲区</param>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false, IsServerAsynchronousCallback = true)]
        private void read(fileBlockStream.index index, ref fastCSharp.code.cSharp.tcpBase.subByteArrayEvent buffer, Func<fastCSharp.code.cSharp.asynchronousMethod.returnValue<fastCSharp.code.cSharp.tcpBase.subByteArrayEvent>, bool> onReturn)
        {
            fileBlockStream fileStream = this.fileStream;
            if (fileStream == null) onReturn(default(fastCSharp.code.cSharp.tcpBase.subByteArrayEvent));
            else fileStream.Read(index, onReturn);
        }
        /// <summary>
        /// 写入文件分块数据
        /// </summary>
        /// <param name="dataStream">文件分块数据</param>
        /// <returns>写入文件位置</returns>
        [fastCSharp.code.cSharp.tcpServer(IsServerAsynchronousTask = false, IsClientCallbackTask = false, IsClientAsynchronous = true)]
        private unsafe long write(fastCSharp.code.cSharp.tcpBase.subByteUnmanagedStream dataStream)
        {
            if (fileStream != null)
            {
                subArray<byte> buffer = dataStream.Buffer;
                if (buffer.Count != 0)
                {
                    fixed (byte* bufferFixed = buffer.array)
                    {
                        byte* start = bufferFixed - sizeof(int);
                        *(int*)start = buffer.Count;
                        return fileStream.UnsafeWrite(start, buffer.Count + (-buffer.Count & 3) + sizeof(int));
                    }
                }
            }
            return -1;
        }
        /// <summary>
        /// 等待缓存写入
        /// </summary>
        [fastCSharp.code.cSharp.tcpServer]
        private void waitBuffer()
        {
            if (fileStream != null) fileStream.WaitWriteBuffer();
        }
        /// <summary>
        /// 写入缓存
        /// </summary>
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
        /// <returns>是否成功</returns>
        [fastCSharp.code.cSharp.tcpServer]
        private bool flush(bool isDiskFile)
        {
            return fileStream != null && fileStream.Flush(isDiskFile) != null;
        }
    }
}
