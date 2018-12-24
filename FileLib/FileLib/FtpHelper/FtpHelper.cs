using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FileLib
{
    public partial class FtpHelper
    {
        #region 属性
        /// <summary>
        /// 全局文件夹目录上传任务列表
        /// </summary>
        public static List<FileInfoEntity> FtpUploadDirectoryInfoList { get; set; } = new List<FileInfoEntity>();
        /// <summary>
        /// 全局文件任务上传列表
        /// </summary>
        public static List<FileInfoEntity> FtpUploadFileInfoList { get; set; } = new List<FileInfoEntity>();
        /// <summary>
        /// 全局文件夹目录下载任务列表
        /// </summary>
        public static List<FileInfoEntity> FtpDownloadDirectoryInfoList { get; set; } = new List<FileInfoEntity>();
        /// <summary>
        /// 全局文件任务下载列表
        /// </summary>
        public static List<FileInfoEntity> FtpDownloadFileInfoList { get; set; } = new List<FileInfoEntity>();
        /// <summary>
        /// 全局Ftp服务器IP地址
        /// </summary>
        public static string IP { get; set; } = string.Empty;
        /// <summary>
        /// 全局Ftp用户 
        /// </summary>
        public static string User { get; set; } = string.Empty;
        /// <summary>
        /// 全局Ftp密码
        /// </summary>
        public static string Pwd { get; set; } = string.Empty;
        /// <summary>
        /// 全局Ftp远程根目录路径
        /// </summary>
        internal static string _Url { get; set; } = string.Empty;
        /// <summary>
        /// 任务线程句柄
        /// </summary>
        public Thread TaskThread { get; set; } = null;
        /// <summary>
        /// 任务信息句柄
        /// </summary>
        internal FileInfoEntity _Info { get; set; } = null;
        /// <summary>
        /// Ftp初始化标记
        /// </summary>
        private static bool _IsFtpInit = false;

        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public FtpHelper(FileInfoEntity info)
        {
            _Info = info;
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化
        /// </summary>
        public static void InitFtp(string ip, string user, string pwd)
        {
            if (!_IsFtpInit)
            {
                try
                {
                    _Url = "ftp://" + ip + "/";
                    IP = ip;
                    User = user;
                    Pwd = pwd;
                    if (!Directory.Exists("./Tasks"))
                    {
                        Directory.CreateDirectory("Tasks");
                    }
                    if (!Directory.Exists("./Tasks/UploadDirectory"))
                    {
                        Directory.CreateDirectory("./Tasks/UploadDirectory");
                    }
                    if (!Directory.Exists("./Tasks/UploadFile"))
                    {
                        Directory.CreateDirectory("./Tasks/UploadFile");
                    }
                    if (!Directory.Exists("./Tasks/DownloadDirectory"))
                    {
                        Directory.CreateDirectory("./Tasks/DownloadDirectory");
                    }
                    if (!Directory.Exists("./Tasks/DownloadFile"))
                    {
                        Directory.CreateDirectory("./Tasks/DownloadFile");
                    }
                    //读取未完成的上传、下载任务列表
                    List<string> paths = FileHelper.GetFiles("./Tasks/UploadDirectory");
                    if (paths != null)
                    {
                        foreach (string item in paths)
                        {
                            FtpUploadDirectoryInfoList.Add(FileHelper.Deserialize<FileInfoEntity>(item));
                        }
                    }
                    paths = FileHelper.GetFiles("./Tasks/UploadFile");
                    if (paths != null)
                    {
                        foreach (string item in paths)
                        {
                            FtpUploadFileInfoList.Add(FileHelper.Deserialize<FileInfoEntity>(item));
                        }
                    }
                    paths = FileHelper.GetFiles("./Tasks/DownloadDirectory");
                    if (paths != null)
                    {
                        foreach (string item in paths)
                        {
                            FtpDownloadDirectoryInfoList.Add(FileHelper.Deserialize<FileInfoEntity>(item));
                        }
                    }
                    paths = FileHelper.GetFiles("./Tasks/DownloadFile");
                    if (paths != null)
                    {
                        foreach (string item in paths)
                        {
                            FtpDownloadFileInfoList.Add(FileHelper.Deserialize<FileInfoEntity>(item));
                        }
                    }
                    //删除过期任务
                    for (int i = 0; i < FtpUploadFileInfoList.Count(); i++)
                    {
                        if (FtpUploadFileInfoList[i].ResultCode == ResultCode.Failed || FtpUploadFileInfoList[i].ResultCode == ResultCode.Cancelled)
                        {
                            FileHelper.DeleteFile("./Tasks/UploadFile/" + FtpUploadFileInfoList[i].FileNo + ".dat");
                            FtpUploadFileInfoList.Remove(FtpUploadFileInfoList[i]);
                        }
                    }
                    for (int i = 0; i < FtpDownloadFileInfoList.Count(); i++)
                    {
                        if (FtpDownloadFileInfoList[i].ResultCode == ResultCode.Failed || FtpDownloadFileInfoList[i].ResultCode == ResultCode.Cancelled)
                        {
                            FileHelper.DeleteFile("./Tasks/DownloadFile/" + FtpDownloadFileInfoList[i].FileNo + ".dat");
                            FtpDownloadFileInfoList.Remove(FtpDownloadFileInfoList[i]);
                        }
                    }
                    for (int i = 0; i < FtpUploadDirectoryInfoList.Count(); i++)
                    {
                        var query = (from x in FtpUploadFileInfoList
                                     where x.OperationNo == FtpUploadDirectoryInfoList[i].OperationNo
                                     select x).FirstOrDefault();
                        if (query == null)
                        {
                            FileHelper.DeleteFile("./Tasks/UploadDirectory/" + FtpUploadDirectoryInfoList[i].FileNo + ".dat");
                            FtpUploadDirectoryInfoList.Remove(FtpUploadDirectoryInfoList[i]);
                        }
                    }
                    for (int i = 0; i < FtpDownloadDirectoryInfoList.Count(); i++)
                    {
                        var query = (from x in FtpDownloadFileInfoList
                                     where x.OperationNo == FtpDownloadDirectoryInfoList[i].OperationNo
                                     select x).FirstOrDefault();
                        if (query == null)
                        {
                            FileHelper.DeleteFile("./Tasks/DownloadDirectory/" + FtpDownloadDirectoryInfoList[i].FileNo + ".dat");
                            FtpDownloadDirectoryInfoList.Remove(FtpDownloadDirectoryInfoList[i]);
                        }
                    }
                }
                catch
                {
                    //nothing
                }
                finally
                {
                    _IsFtpInit = true;  
                }
            }
        }
        #endregion

        #region 任务初始化
        /// <summary>
        /// 任务初始化
        /// </summary>
        public static List<FileInfoEntity> TaskInit(FileInfoEntity info)
        {
            switch (info.Method)
            {
                case Method.Delete:
                    return FtpHelper.DeleteInit(info);
                case Method.Download:
                    return FtpHelper.DownloadInit(info);
                case Method.Make:
                    return FtpHelper.MakeInit(info);
                case Method.Move:
                    return FtpHelper.MoveInit(info);
                case Method.Rename:
                    return FtpHelper.RenameInit(info);
                case Method.Upload:
                    return FtpHelper.UploadInit(info);
                default:
                    return null;
            }     
        }
        #endregion

        #region 任务操作

        #region 任务执行
        /// <summary>
        /// 任务执行
        /// </summary>
        public void Run()
        {
            try
            {
                TaskThread = new Thread(() =>
                {
                    _Info.ResultCode = ResultCode.UnFinished;

                    switch (_Info.Method)
                    {
                        case Method.Delete:
                            this.Delete(_Info);
                            break;
                        case Method.Download:
                            this.Download(_Info);
                            break;
                        case Method.Make:
                            this.Make(_Info);
                            break;
                        case Method.Move:
                            this.Move(_Info);
                            break;
                        case Method.Rename:
                            this.Rename(_Info);
                            break;
                        case Method.Upload:
                            this.Upload(_Info);
                            break;
                        default:
                            break;
                    }
                    TaskThread = null;
                });
                TaskThread.IsBackground = true;
                TaskThread.Start();
            }
            catch { };
        }
        #endregion

        #region 任务暂停
        /// <summary>
        /// 任务暂停
        /// </summary>
        public void Pause()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    //此功能仅对上传下载有效
                    if (_Info.Method != Method.Upload && _Info.Method != Method.Download)
                        return;

                    _Info.ResultCode = ResultCode.Paused;

                    if (TaskThread != null)
                    {
                        TaskThread.Abort();
                        TaskThread = null;
                    }
                    else
                    {
                        if (Paused != null)
                        {
                            Paused(this, new FileEventArgs(_Info, null));
                        }
                    }
                });
            }
            catch { };
        }
        #endregion

        #region 任务取消
        /// <summary>
        /// 任务取消
        /// </summary>
        public void Cancel()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    //此功能仅对上传下载有效
                    if (_Info.Method != Method.Upload && _Info.Method != Method.Download)
                        return;

                    _Info.ResultCode = ResultCode.Cancelled;
                    if (TaskThread != null)
                    {
                        TaskThread.Abort();
                        TaskThread = null;
                    }
                    else
                    {
                        if (_Info.Method == Method.Upload)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                FtpHelper.DeleteRemoteFile(_Url + (_Info.NewFilePath == "" ? "" : _Info.NewFilePath + "/") + _Info.NewFileName + ".tmp");
                            });                    
                            FtpHelper.FtpUploadFileInfoList.Remove(_Info);
                        }
                        else
                        {
                            FileHelper.DeleteFile((_Info.NewFilePath == "" ? "" : _Info.NewFilePath + "/") + _Info.NewFileName + ".tmp");
                            FtpHelper.FtpDownloadFileInfoList.Remove(_Info);
                        }
                        FileHelper.DeleteFile("./Tasks/" + _Info.Method.ToString() + "File/" + _Info.FileNo + ".dat");
                        if (Cancelled != null)
                        {
                            Cancelled(this, new FileEventArgs(_Info, null));
                        }
                    }
                });
            }
            catch { };
        }
        #endregion

        #endregion

        #region 其他函数

        #region 释放非托管资源
        /// <summary>
        /// 释放非托管资源
        /// </summary>
        internal static void Dispose(dynamic[] res)
        {
            for (int i = 0; i < res.Length; i++)
            {
                if (res[i] != null)
                {
                    try
                    {
                        res[i].Dispose();
                    }
                    catch
                    {
                        //nothing
                    }
                    finally
                    {
                        res[i] = null;
                    }
                }
            }
        }
        #endregion

        #region Ftp连接配置
        /// <summary>
        /// Ftp连接配置
        /// </summary>
        internal static void FtpConnectConfig(string url, out FtpWebRequest reqFtp, string method)
        {
            //特殊字符#、%等转化
            reqFtp = (FtpWebRequest)WebRequest.Create(Regex.Replace(url, @"[#]", System.Web.HttpUtility.UrlEncode("#")));
            reqFtp.Credentials = new NetworkCredential(FtpHelper.User, FtpHelper.Pwd);
            reqFtp.Method = method;
            reqFtp.UseBinary = true;
            reqFtp.KeepAlive = false;
        }
        #endregion

        #region 检测Ftp网络是否异常
        /// <summary>
        /// 检测Ftp网络是否异常
        /// </summary>
        public static bool CheckLinkState(string url)
        {
            bool canPing = false;

            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.ListDirectoryDetails);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
                responStream = responFtp.GetResponseStream();
                canPing = true;
            }
            catch
            {
                canPing = false;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp, responStream });
            }
            return canPing;
        }
        #endregion

        #region 检测远程目录是否存在
        /// <summary>
        /// 检测远程目录是否存在
        /// </summary>
        public static bool CheckExistOfRemoteDirectory(string url)
        {
            bool exist = false;
            //尝试创建该目录，如果可以创建，说明该目录不存在
            try
            {
                MakeEmptyRemoteDirectory(url);
                DeleteRemoteEmptyDirectory(url);
            }
            catch
            {
                exist = true;
            }
            return exist;
        }
        #endregion

        #region 检测远程文件是否存在
        /// <summary>
        /// 检测远程文件是否存在
        /// </summary>
        public static bool CheckExistOfRemoteFile(string url)
        {
            bool exist = false;

            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.GetFileSize);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
                responStream = responFtp.GetResponseStream();
                exist = true;
            }
            catch 
            {
                exist = false;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp, responStream });
            }
            return exist;
        }
        #endregion

        #region 获取远程文件大小（字节）
        /// <summary>
        /// 获取远程文件大小（字节）
        /// </summary>
        public static long GetRemoteFileLength(string url)
        {
            long len = 0;
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.GetFileSize);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
                responStream = responFtp.GetResponseStream();
                len = responFtp.ContentLength;
            }
            catch
            {
                len = -1;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp, responStream });
            }
            return len;
        }
        #endregion

        #region 获取远程目录的大小（字节，包括子目录包含的文件）
        /// <summary>
        /// 获取远程目录的大小（字节，包括子目录包含的文件）
        /// </summary>
        public static long GetRemoteDirectoryLength(string url)
        {
            long len = 0;

            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;
            StreamReader reader = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.ListDirectoryDetails);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
                responStream = responFtp.GetResponseStream();
                reader = new StreamReader(responStream);
                string pattern = @"^(\d+-\d+-\d+\s+\d+:\d+(?:AM|PM))\s+(<DIR>|\d+)\s+(.+)$";
                Regex regex = new Regex(pattern);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Groups[2].Value != "<DIR>")
                    {
                        len += long.Parse(match.Groups[2].Value);
                    }
                    else
                    {
                        long temp = GetRemoteDirectoryLength(url + "/" + match.Groups[3].Value);
                        len += temp == -1 ? 0 : temp;
                    }
                }
            }
            catch
            {
                len = -1;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp, responStream, reader });
            }
            return len;
        }
        #endregion

        #region 获取远程目录包含的文件个数（包括子目录包含的文件）
        /// <summary>
        /// 获取远程目录包含的文件个数（包括子目录包含的文件）
        /// </summary>
        public static long GetRemoteFileCount(string url)
        {
            long len = 0;

            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;
            StreamReader reader = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.ListDirectoryDetails);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
                responStream = responFtp.GetResponseStream();
                reader = new StreamReader(responStream);
                string pattern = @"^(\d+-\d+-\d+\s+\d+:\d+(?:AM|PM))\s+(<DIR>|\d+)\s+(.+)$";
                Regex regex = new Regex(pattern);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Groups[2].Value != "<DIR>")
                    {
                        len++;
                    }
                    else
                    {
                        long temp = GetRemoteFileCount(url + "/" + match.Groups[3].Value);
                        len += temp == -1 ? 0 : temp;
                    }
                }
            }
            catch
            {
                len = -1;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp, responStream, reader });
            }
            return len;
        }
        #endregion

        #region 获取远程目录包含的子目录的完整路径列表（包括子目录包含的目录）
        /// <summary>
        /// 获取远程目录包含的子目录的完整路径列表（包括子目录包含的目录）
        /// </summary>
        public static List<string> GetRemoteDirectories(string url)
        {
            List<string> ls = new List<string>();

            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;
            StreamReader reader = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.ListDirectoryDetails);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
                responStream = responFtp.GetResponseStream();
                reader = new StreamReader(responStream);
                string pattern = @"^(\d+-\d+-\d+\s+\d+:\d+(?:AM|PM))\s+(<DIR>|\d+)\s+(.+)$";
                Regex regex = new Regex(pattern);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Groups[2].Value == "<DIR>")
                    {
                        ls.Add(Regex.Replace(url.Trim(), @"[/]$", "") + "/" + match.Groups[3].Value);
                        ls.AddRange(GetRemoteDirectories(Regex.Replace(url.Trim(), @"[/]$", "") + "/" + match.Groups[3].Value));
                    }
                }
            }
            catch
            {
                ls = new List<string>();
            }
            finally
            {
                Dispose(new dynamic[] { responFtp, responStream, reader });
            }
            return ls;
        }
        #endregion

        # region 获取远程目录包含的文件完整的路径列表（包括子目录包含的文件）
        /// <summary>
        /// 获取远程目录包含的文件完整的路径列表（包括子目录包含的文件）
        /// </summary>
        public static List<string> GetRemoteFiles(string url)
        {
            List<string> ls = new List<string>();

            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;
            StreamReader reader = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.ListDirectoryDetails);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
                responStream = responFtp.GetResponseStream();
                reader = new StreamReader(responStream);
                string pattern = @"^(\d+-\d+-\d+\s+\d+:\d+(?:AM|PM))\s+(<DIR>|\d+)\s+(.+)$";
                Regex regex = new Regex(pattern);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Groups[2].Value != "<DIR>")
                    {
                        ls.Add(Regex.Replace(url.Trim(), @"[/]$", "") + "/" + match.Groups[3].Value);
                    }
                    if (match.Groups[2].Value == "<DIR>")
                    {
                        ls.AddRange(GetRemoteFiles(Regex.Replace(url.Trim(), @"[/]$", "") + "/" + match.Groups[3].Value));
                    }
                }
            }
            catch
            {
                ls = new List<string>();
            }
            finally
            {
                Dispose(new dynamic[] { responFtp, responStream, reader });
            }
            return ls;
        }
        #endregion

        # region 获取远程目录所有子目录和子文件的FileInfoEntity信息
        /// <summary>
        /// 获取远程目录所有子目录和子文件的FileInfoEntity信息
        /// </summary>
        internal static List<FileInfoEntity> GetRemoteSubFileInfoEntities(FileInfoEntity info)
        {
            if (info.FileType == FileType.File)
            {
                return null;
            }
            else
            {
                //子文件操作信息（包含子目录）
                List<FileInfoEntity> subFileInfoList = new List<FileInfoEntity>();
                //获取源的所有子目录完整路径
                List<string> dirPathList = GetRemoteDirectories(_Url + (info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName);
                //获取源的所有子目录的名称
                List<string> dirNameList = new List<string>();
                for (int i = 0; i < dirPathList.Count(); i++)
                {
                    try
                    {
                        dirNameList.Add(dirPathList[i].Substring(dirPathList[i].LastIndexOf("/") + 1, dirPathList[i].Length - dirPathList[i].LastIndexOf("/") - 1));
                    }
                    catch
                    {
                        dirNameList.Add(dirPathList[i]);
                    }
                }
                //获取源的所有子目录的相对路径（过滤_Url和文件名）                   
                for (int i = 0; i < dirPathList.Count(); i++)
                {
                    string temp = Regex.Replace(dirPathList[i], "^" + _Url, "");
                    try
                    {
                        temp = temp.Substring(0, temp.LastIndexOf("/"));
                    }
                    catch
                    {
                        //nothing
                    }
                    dirPathList[i] = temp;
                }           
                //添加子目录信息
                for (int i = 0; i < dirPathList.Count(); i++)
                {
                    FileInfoEntity temp = new FileInfoEntity();
                    temp.OperationNo = info.OperationNo;
                    temp.Method = info.Method;
                    temp.FileType = FileType.Directory;
                    temp.ModifyDateTime = info.ModifyDateTime;
                    temp.FileNo = "";
                    temp.FilePath = dirPathList[i];
                    temp.FileName = dirNameList[i];
                    temp.NewFilePath = Regex.Replace(dirPathList[i], "^" + (info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName));
                    temp.NewFileName = temp.FileName;
                    subFileInfoList.Add(temp);
                }
                //获取源的所有子文件完整路径
                List<string> filePathList = GetRemoteFiles(_Url + (info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName);
                //获取源的所有子文件的名称
                List<string> fileNameList = new List<string>();
                for (int i = 0; i < filePathList.Count(); i++)
                {
                    try
                    {
                        fileNameList.Add(filePathList[i].Substring(filePathList[i].LastIndexOf("/") + 1, filePathList[i].Length - filePathList[i].LastIndexOf("/") - 1));
                    }
                    catch
                    {
                        fileNameList.Add(filePathList[i]);
                    }
                }
                //获取源的所有子文件的相对路径（过滤_Url和文件名）                   
                for (int i = 0; i < filePathList.Count(); i++)
                {
                    string temp = Regex.Replace(filePathList[i], "^" + _Url, "");
                    try
                    {
                        temp = temp.Substring(0, temp.LastIndexOf("/"));
                    }
                    catch
                    {
                        //nothing
                    }
                    filePathList[i] = temp;
                }
                //添加子目录信息
                for (int i = 0; i < filePathList.Count(); i++)
                {
                    FileInfoEntity temp = new FileInfoEntity();
                    temp.OperationNo = info.OperationNo;
                    temp.Method = info.Method;
                    temp.FileType = FileType.File;
                    temp.ModifyDateTime = info.ModifyDateTime;
                    temp.FileNo = info.OperationNo + "_" + i;
                    temp.FilePath = filePathList[i];
                    temp.FileName = fileNameList[i];
                    temp.NewFilePath = Regex.Replace(filePathList[i], "^" + (info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName));
                    temp.NewFileName = temp.FileName;
                    subFileInfoList.Add(temp);
                }
                return subFileInfoList;
            }
        }
        #endregion

        #region 创建空的远程文件
        /// <summary>
        /// 创建空的远程文件
        /// </summary>
        internal static void MakeEmptyRemoteFile(string url)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.UploadFile);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp });
            }
        }
        #endregion
        #region 尝试创建空的远程文件
        /// <summary>
        /// 尝试创建空的远程文件
        /// </summary>
        internal static bool TryMakeEmptyRemoteFile(string url)
        {
            bool done = false;
            try
            {
                MakeEmptyRemoteFile(url);
                done = true;

            }
            catch (Exception ex)
            {
                //nothing
            }
            return done;
        }
        #endregion

        #region 创建空的远程目录
        /// <summary>
        /// 创建空的远程目录
        /// </summary>
        internal static void MakeEmptyRemoteDirectory(string url)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.MakeDirectory);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp });
            }
        }
        #endregion
        #region 尝试创建空的远程目录
        /// <summary>
        /// 尝试创建空的远程目录
        /// </summary>
        internal static bool TryMakeEmptyRemoteDirectory(string url)
        {
            bool done = false;
            try
            {
                MakeEmptyRemoteDirectory(url);
                done = true;

            }
            catch (Exception ex)
            {
                //nothing
            }
            return done;
        }
        #endregion

        #region 移除远程文件
        /// <summary>
        /// 移除远程文件
        /// </summary>
        internal static void DeleteRemoteFile(string url)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.DeleteFile);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp });
            }
        }
        #endregion
        # region 尝试移除远程文件
        /// <summary>
        /// 尝试移除远程文件
        /// </summary>
        internal static bool TryDeleteRemoteFile(string url)
        {
            bool done = false;
            try
            {
                DeleteRemoteFile(url);
                done = true;

            }
            catch (Exception ex)
            {
                //nothing
            }
            return done;
        }
        #endregion

        #region 移除远程空目录
        /// <summary>
        /// 移除远程空目录
        /// </summary>
        internal static void DeleteRemoteEmptyDirectory(string url)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;

            FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.RemoveDirectory);
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp });
            }
        }
        #endregion
        # region 尝试移除远程空目录
        /// <summary>
        /// 尝试移除远程空目录
        /// </summary>
        internal static bool TryDeleteRemoteEmptyDirectory(string url)
        {
            bool done = false;
            try
            {
                DeleteRemoteEmptyDirectory(url);
                done = true;

            }
            catch (Exception ex)
            {
                //nothing
            }
            return done;
        }
        #endregion

        #region 重命名远程文件
        /// <summary>
        /// 重命名远程文件
        /// </summary>
        internal static void RenameRemoteFile(string dirUrl, string name, string newName)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;

            FtpConnectConfig(dirUrl + name, out reqFtp, WebRequestMethods.Ftp.Rename);
            reqFtp.RenameTo = newName;
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp });
            }
        }
        #endregion
        # region 尝试重命名远程文件
        /// <summary>
        /// 尝试重命名远程文件
        /// </summary>
        internal static bool TryRenameRemoteFile(string dirUrl, string name, string newName)
        {
            bool done = false;
            try
            {
                RenameRemoteFile(dirUrl, name, newName);
                done = true;

            }
            catch (Exception ex)
            {
                //nothing
            }
            return done;
        }
        #endregion

        #region 重命名远程目录
        /// <summary>
        /// 重命名远程文件
        /// </summary>
        internal static void RenameRemoteDirectory(string dirUrl, string name, string newName)
        {
            FtpHelper.RenameRemoteFile(dirUrl, name, newName);
        }
        #endregion
        # region 尝试重命名远程文件
        /// <summary>
        /// 尝试重命名远程文件
        /// </summary>
        internal static bool TryRenameRemoteDirectory(string url)
        {
            bool done = false;
            try
            {
                DeleteRemoteFile(url);
                done = true;

            }
            catch (Exception ex)
            {
                //nothing
            }
            return done;
        }
        #endregion

        #region 移动远程目录或文件
        /// <summary>
        /// 移动远程目录或文件
        /// </summary>
        internal static void MoveRemoteDirectoryFile(string oldUrl, string newUrl)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;

            FtpConnectConfig(oldUrl, out reqFtp, WebRequestMethods.Ftp.Rename);
            reqFtp.RenameTo = newUrl;
            try
            {
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp });
            }
        }
        #endregion
        # region 尝试移动远程目录或文件
        /// <summary>
        /// 尝试移动远程目录或文件
        /// </summary>
        internal static bool TryMoveRemoteDirectoryFile(string oldUrl, string newUrl)
        {
            bool done = false;
            try
            {
                MoveRemoteDirectoryFile(oldUrl, newUrl);
                done = true;
            }
            catch (Exception ex)
            {
                //nothing
            }
            return done;
        }
        #endregion

        #endregion

        #region 事件
        //启动
        public event EventHandler<FileEventArgs> Started;
        //暂停
        public event EventHandler<FileEventArgs> Paused;
        //取消
        public event EventHandler<FileEventArgs> Cancelled;
        //完成
        public event EventHandler<FileEventArgs> Done;
        //失败
        public event EventHandler<FileEventArgs> Failed;
        //进度更新
        public event EventHandler<FileEventArgs> ProgressUpdated;
        #endregion
    }
}
