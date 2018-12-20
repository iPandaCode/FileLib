using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileLib
{
    public class FileEventArgs
    {
        /// <summary>
        /// 异常句柄
        /// </summary>
        public Exception Exception { get; set; } = null;
        /// <summary>
        /// 文件任务信息句柄
        /// </summary>
        public FileInfoEntity FileInfoEntity { get; set; } = null;
        public FileEventArgs(FileInfoEntity info, Exception ex)
        {
            FileInfoEntity = info;
            Exception = ex;
        }
    }
}
