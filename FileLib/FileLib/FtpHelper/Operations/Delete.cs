using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileLib
{
    public partial class FtpHelper
    {
        #region 初始化删除任务
        /// <summary>
        /// 初始化删除任务
        /// </summary>
        private static List<FileInfoEntity> DeleteInit(FileInfoEntity info)
        {
            if (info == null)
            {
                return null;
            }
            //统一文件路径分隔符
            info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
            //设置操作源信息
            info.OperationNo = Guid.NewGuid().ToString("N"); //任务流水号
            info.ResultCode = ResultCode.New; //新建任务
            info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString(); //操作文本信息
            info.ModifyDateTime = DateTime.Now; //时间
            info.OperationProgress = 0; //进度
            info.FileNo = info.OperationNo; //文件流水号
            return GetRemoteSubFileInfoEntities(info);
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        private void Delete(FileInfoEntity info)
        {
            if (info == null)
            {
                return;
            }
            try
            {
                if (info.FileType == FileType.File)
                {
                    FtpHelper.DeleteRemoteFile(_Url + (info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                }
                else
                {
                    //获取目标目录所有子文件的完整路径列表
                    List<string> filePathList = FtpHelper.GetRemoteFiles(_Url + (info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                    //获取目标目录所有子目录的完整路径列表
                    List<string> dirPathList = FtpHelper.GetRemoteDirectories(_Url + (info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                    for (int i = 0; i < filePathList.Count(); i++)
                    {
                        FtpHelper.DeleteRemoteFile(filePathList[i]);
                        info.OperationProgress = (int)((i / 1.00 / filePathList.Count()) * 100);
                        //更新进度事件
                        Task.Factory.StartNew(() =>
                        {
                            if (ProgressUpdated != null)
                            {
                                ProgressUpdated(this, new FileEventArgs(info, null));
                            }
                        });
                    }
                    dirPathList.Reverse();
                    for (int i = 0; i < dirPathList.Count(); i++)
                    {
                        FtpHelper.DeleteRemoteEmptyDirectory(dirPathList[i]);                     
                    }
                    FtpHelper.DeleteRemoteEmptyDirectory(_Url + (info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                }
                info.ResultCode = ResultCode.Done;
                info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString();
                if (Done != null)
                {
                    Done(this, new FileEventArgs(info, null));
                }
            }
            catch 
            {
                info.ResultCode = ResultCode.Failed;
                info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString();
                if (Failed != null)
                {
                    Failed(this, new FileEventArgs(info, new Exception(info.OperationResultMessage) { }));
                }
            }
        }
        #endregion
    }
}
