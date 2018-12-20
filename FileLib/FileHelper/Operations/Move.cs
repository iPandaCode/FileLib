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
        #region 初始化移动任务
        /// <summary>
        /// 初始化移动任务
        /// </summary>
        private static List<FileInfoEntity> MoveTaskInit(FileInfoEntity info)
        {
            if (info != null)
            {
                //统一文件路径分隔符
                info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
                info.NewFilePath = Regex.Replace(info.NewFilePath.Replace("\\", "/").Trim(), @"[/]$", "");
                info.ModifyDateTime = DateTime.Now;
            }
            return null;
        }
        #endregion

        #region 移动
        /// <summary>
        /// 移动
        /// </summary>
        private void Move(FileInfoEntity info)
        {
            try
            {
                if (info.FileType == FileType.File)
                {
                    FileHelper.MoveFile(info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName, info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName);
                }
                else
                {
                    //获取源目录所有子文件的完整路径列表
                    List<string> filePathList = FileHelper.GetFiles((info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                    //获取源目录所有子目录的完整路径列表
                    List<string> dirPathList = FileHelper.GetDirectories((info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                    //创建目标目录
                    FileHelper.MakeDirectory(info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName);
                    //创建所有目标子目录
                    for (int i = 0; i < dirPathList.Count(); i++)
                    {
                        FileHelper.MakeDirectory(Regex.Replace(dirPathList[i].Replace("\\", "/"), @"^" + (info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName)));
                    }
                    //移动所有子文件
                    for (int i = 0; i < filePathList.Count(); i++)
                    {
                        FileHelper.MoveFile(filePathList[i], Regex.Replace(filePathList[i].Replace("\\", "/"), @"^" + (info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName)));
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
                    //删除所有源的子目录
                    //移动所有子文件
                    for (int i = 0; i < dirPathList.Count(); i++)
                    {
                        FileHelper.DeleteDirectory(dirPathList[i]);
                    }
                    //删除源目录
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
            return;
        }
        #endregion
    }
}
