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
        #region 初始化重命名任务
        /// <summary>
        /// 初始化重命名任务
        /// </summary>
        private static List<FileInfoEntity> RenameInit(FileInfoEntity info)
        {
            if (info == null)
            {
                return null;
            }
            else
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
                info.NewFilePath = info.FilePath;
                //info.Length = info.FileType == FileType.File ? GetRemoteFileLength(_Url + (info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName) : GetRemoteFileCount(_Url + (info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName); //操作源大小
                return GetRemoteSubFileInfoEntities(info);
            }
        }
        #endregion

        #region 重命名
        /// <summary>
        /// 重命名
        /// </summary>
        private void Rename(FileInfoEntity info)
        {
            if (info == null)
                return;
            try
            {
                if (info.FileType == FileType.Directory)
                {
                    FtpHelper.RenameRemoteDirectory(_Url + (info.FilePath != "" ? info.FilePath + "/" : ""), info.FileName, info.NewFileName);
                }
                else
                {
                    FtpHelper.RenameRemoteFile(_Url + (info.FilePath != "" ? info.FilePath + "/" : ""), info.FileName, info.NewFileName);
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
