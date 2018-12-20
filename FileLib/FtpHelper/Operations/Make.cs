﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileLib
{
    public partial class FtpHelper
    {
        #region 初始化新建空文件任务
        /// <summary>
        /// 初始化新建空文件任务
        /// </summary>
        private static List<FileInfoEntity> MakeInit(FileInfoEntity info)
        {
            //统一文件路径分隔符
            info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
            //设置操作源信息
            info.OperationNo = Guid.NewGuid().ToString("N"); //任务流水号
            info.ResultCode = ResultCode.New; //新建任务
            info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString(); //操作文本信息
            info.ModifyDateTime = DateTime.Now; //时间
            info.OperationProgress = 0; //进度
            info.FileNo = info.OperationNo; //文件流水号
            return null;
        }
        #endregion

        #region 新建空文件
        /// <summary>
        /// 新建空文件
        /// </summary>
        private void Make(FileInfoEntity info)
        {
            try
            {
                if (info.FileType == FileType.Directory)
                {
                    FtpHelper.MakeEmptyRemoteDirectory(_Url + (info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                }
                else
                {
                    FtpHelper.MakeEmptyRemoteFile(_Url + (info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                }
                info.ResultCode = ResultCode.Done;
                info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString();
                if (Done != null)
                {
                    Done(this, new FileEventArgs(info, null));
                }
            }
            catch (Exception ex)
            {
                info.ResultCode = ResultCode.Failed;
                info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString();
                if (Failed != null)
                {
                    Failed(this, new FileEventArgs(info, new Exception(info.OperationResultMessage) { }));
                }
            }
            return;
        }
        #endregion
    }
}
