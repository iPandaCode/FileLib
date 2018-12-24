using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileLib
{
    public partial class FileHelper
    {
        #region 初始化删除任务
        /// <summary>
        /// 初始化删除任务
        /// </summary>
        private static List<FileInfoEntity> DeleteTaskInit(FileInfoEntity info)
        {
            if (info == null)
            {
                return null;
            }
            //统一文件路径分隔符
            info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
            info.ModifyDateTime = DateTime.Now;
            return null;
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
                    FileHelper.DeleteFile((info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                }
                else
                {
                    //获取目标目录所有子文件的完整路径列表
                    List<string> filePathList = FileHelper.GetFiles((info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                    //获取目标目录所有子目录的完整路径列表
                    List<string> dirPathList = FileHelper.GetDirectories((info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                    for (int i = 0; i < filePathList.Count(); i++)
                    {
                        FileHelper.DeleteFile(filePathList[i]);
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
                        FileHelper.DeleteDirectory(dirPathList[i]);                     
                    }
                    FileHelper.DeleteDirectory((info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
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
                    Failed(this, new FileEventArgs(info, new Exception(ex.Message) { }));
                }
            }
        }
        #endregion
    }
}
