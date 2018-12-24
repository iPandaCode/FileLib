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
        #region 初始化新建空文件任务
        /// <summary>
        /// 初始化新建空文件任务
        /// </summary>
        private static List<FileInfoEntity> MakeTaskInit(FileInfoEntity info)
        {
            if (info == null)
            {
                return null;
            }
            //统一文件路径分隔符
            info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
            return null;
        }
        #endregion

        #region 新建空文件
        /// <summary>
        /// 新建空文件
        /// </summary>
        private void Make(FileInfoEntity info)
        {
            if (info == null)
            {
                return;
            }
            try
            {
                if (info.FileType == FileType.Directory)
                {
                    FileHelper.MakeDirectory((info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
                }
                else
                {
                    FileHelper.MakeFile((info.FilePath != "" ? info.FilePath + "/" : "") + info.FileName);
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
