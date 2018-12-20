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
        #region 初始化打开任务
        /// <summary>
        /// 初始化打开任务
        /// </summary>
        private static List<FileInfoEntity> OpenTaskInit(FileInfoEntity info)
        {
            if (info != null)
            {
                //统一文件路径分隔符
                info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
            }
            return null;
        }
        #endregion

        #region 打开（仅对文件有效，对目录无效）
        /// <summary>
        /// 打开（仅对文件有效，对目录无效）
        /// </summary>
        private void Open(FileInfoEntity info)
        {
            try
            {
                if (info.FileType == FileType.Directory)
                {
                    throw new Exception("打开文件的类型必须是文件，不能是目录");
                }
                else
                {
                    System.Diagnostics.Process.Start(info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName);
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
