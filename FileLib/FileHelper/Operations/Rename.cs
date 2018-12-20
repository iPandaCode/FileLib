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
        #region 初始化重命名任务
        /// <summary>
        /// 初始化重命名任务
        /// </summary>
        private static List<FileInfoEntity> RenameTaskInit(FileInfoEntity info)
        {
            if (info != null)
            {
                //统一文件路径分隔符
                info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
                info.ModifyDateTime = DateTime.Now;
            }
            return null;
        }
        #endregion

        #region 重命名
        /// <summary>
        /// 重命名
        /// </summary>
        private void Rename(FileInfoEntity info)
        {
            try
            {
                if (info.FileType == FileType.Directory)
                {
                    FileHelper.RenameDirectory((info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.FilePath != "" ? info.FilePath + "/" + info.NewFileName : info.NewFileName));
                }
                else
                {
                    FileHelper.RenameFile((info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.FilePath != "" ? info.FilePath + "/" + info.NewFileName : info.NewFileName));
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
