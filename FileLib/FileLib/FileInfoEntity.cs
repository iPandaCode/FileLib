using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileLib
{
    /// <summary>
    /// 文件操作方式
    /// </summary>
    [Serializable]
    public enum Method
    {
        /// <summary>
        /// 上传
        /// </summary>
        Upload,
        /// <summary>
        /// 下载
        /// </summary>
        Download,
        /// <summary>
        /// 删除
        /// </summary>
        Delete,
        /// <summary>
        /// 重命名
        /// </summary>
        Rename,
        /// <summary>
        /// 移动
        /// </summary>
        Move,
        /// <summary>
        /// 创建空目录或文件
        /// </summary>
        Make,
        /// <summary>
        /// 拷贝
        /// </summary>
        Copy,
        /// <summary>
        /// 打开文件
        /// </summary>
        Open
    }
    /// <summary>
    /// 文件类型
    /// </summary>
    [Serializable]
    public enum FileType
    {
        /// <summary>
        /// 目录类型
        /// </summary>
        Directory,
        /// <summary>
        /// 文件类型
        /// </summary>
        File
    }
    /// <summary>
    /// 文件操作状态
    /// </summary>
    [Serializable]
    public enum ResultCode
    {
        /// <summary>
        /// 新增
        /// </summary>
        New = 000,
        /// <summary>
        /// 未完成
        /// </summary>
        UnFinished = 100,
        /// <summary>
        /// 完成
        /// </summary>
        Done = 200,
        /// <summary>
        /// 暂停
        /// </summary>
        Paused = 300,
        /// <summary>
        /// 失败
        /// </summary>
        Failed = 404,
        /// <summary>
        /// 取消
        /// </summary>
        Cancelled = 505,
        /// <summary>
        /// 源错误
        /// </summary>
        SourceError = 606,
        /// <summary>
        /// 目标错误
        /// </summary>
        TargetError = 707,
        /// <summary>
        /// 网络错误
        /// </summary>
        NetworkError = 808
    }
    [Serializable]
    public class FileInfoEntity
    {
        /// <summary>
        /// 操作流水号
        /// </summary>
        public string OperationNo { get; set; }
        /// <summary>
        /// 操作方式
        /// </summary>
        public Method Method { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public FileType FileType { get; set; }
        /// <summary>
        /// 操作状态
        /// </summary>
        public ResultCode ResultCode { get; set; }
        /// <summary>
        /// 操作文本信息
        /// </summary>
        public string OperationResultMessage { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime ModifyDateTime { get; set; }
        /// <summary>
        /// 进度
        /// </summary>
        public int OperationProgress { get; set; }
        /// <summary>
        /// 文件流水号
        /// </summary>
        public string FileNo { get; set; }
        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// 源文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 源文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 目标文件路径
        /// </summary>
        public string NewFilePath { get; set; }
        /// <summary>
        /// 目标文件名
        /// </summary>
        public string NewFileName { get; set; }
    }
}
