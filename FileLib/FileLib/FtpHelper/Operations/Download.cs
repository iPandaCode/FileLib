using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace FileLib
{
    public partial class FtpHelper
    {
        #region 初始化下载任务
        /// <summary>
        /// 初始化下载任务
        /// </summary>
        private static List<FileInfoEntity> DownloadInit(FileInfoEntity info)
        {
            if (info == null)
            {
                return null;
            }
            //统一文件路径分隔符
            info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
            info.NewFilePath = Regex.Replace(info.NewFilePath.Replace("/", "\\").Trim(), @"[\\]$", "");
            //设置操作源信息
            info.OperationNo = Guid.NewGuid().ToString("N"); //任务流水号
            info.ResultCode = ResultCode.New; //新建任务
            info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString(); //操作文本信息
            info.ModifyDateTime = DateTime.Now; //时间
            info.OperationProgress = 0; //进度
            info.FileNo = info.OperationNo; //文件流水号
            info.Length = info.FileType == FileType.File ? GetRemoteFileLength(_Url + (info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName) : GetRemoteFileCount(_Url + (info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName); //操作源大小
            //操作源为文件，返回空的子文件信息列表
            if (info.FileType == FileType.File)
            {
                FileHelper.Serialize<FileInfoEntity>("./Tasks/DownloadFile/" + info.FileNo + ".dat", info);
                FtpHelper.FtpDownloadFileInfoList.Add(info);        
                return null;
            }
            else
            {
                //子文件操作信息（包含子目录）
                List<FileInfoEntity> subFileInfoList = new List<FileInfoEntity>();
                if (!Directory.Exists(info.NewFilePath != "" ? info.NewFilePath + "\\" + info.NewFileName : info.NewFileName))
                {
                    Directory.CreateDirectory(info.NewFilePath != "" ? info.NewFilePath + "\\" + info.NewFileName : info.NewFileName);
                }
                FileHelper.Serialize<FileInfoEntity>("./Tasks/DownloadDirectory/" + info.FileNo + ".dat", info);
                FtpHelper.FtpDownloadDirectoryInfoList.Add(info);
                //获取源的所有子目录完整路径
                List<string> dirPathList = GetRemoteDirectories(_Url + (info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName);
                //获取源的所有子文件完整路径(包含_Url:ftp://192.168.1.100:80**/)
                List<string> filePathList = GetRemoteFiles(_Url + (info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName);
                //获取源的所有子文件的文件名
                List<string> fileNameList = new List<string>();
                for (int i = 0; i < filePathList.Count(); i++)
                {
                     fileNameList.Add(filePathList[i].Substring(filePathList[i].LastIndexOf("/") + 1, filePathList[i].Length - filePathList[i].LastIndexOf("/") - 1));
                }            
                //获取源的所有子文件的相对路径（过滤_Url）
                List<string> fileDirPathList = new List<string>();
                for (int i = 0; i < filePathList.Count(); i++)
                {
                    string temp = Regex.Replace(filePathList[i], "^" + _Url, "");
                    try
                    {
                        fileDirPathList.Add(temp.Substring(0, temp.LastIndexOf("/")));
                    }
                    catch
                    {
                        fileDirPathList.Add(temp);
                    }
                }
                //设置目标的所有子目录的完整路径
                List<string> newDirPathList = new List<string>();
                foreach (string item in dirPathList)
                {
                    string temp = Regex.Replace(item, "^" + _Url + (info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.NewFilePath != "" ?  info.NewFilePath + "/" + info.NewFileName: info.NewFileName));
                    newDirPathList.Add(temp);
                }
                //设置目标的所有子文件的目录路径
                List<string> newFileDirPathList = new List<string>();
                foreach (string item in fileDirPathList)
                {
                    string temp = Regex.Replace(item, "^" + (info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName));
                    newFileDirPathList.Add(temp);
                }
                //设置目标的所有子文件的文件名
                List<string> newFileNameList = fileNameList;
                //设置所有子目录信息
                foreach (string item in dirPathList)
                {
                    FileInfoEntity temp = new FileInfoEntity();
                    temp.OperationNo = info.OperationNo;
                    temp.Method = Method.Download;
                    temp.FileType = FileType.Directory;
                    temp.ResultCode = ResultCode.New; 
                    temp.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString(); 
                    temp.ModifyDateTime = info.ModifyDateTime; 
                    temp.OperationProgress = 0; 
                    temp.FileNo = ""; 
                    temp.Length = 0;
                    string path = item.Replace(_Url, "");
                    temp.FilePath = path.Contains("/") ? path.Substring(0, path.LastIndexOf("/")) : "";
                    temp.FileName = path.Contains("/") ? path.Substring(path.LastIndexOf("/") + 1, path.Length - path.LastIndexOf("/") - 1) : path;
                    path = item.Replace(_Url + (info.FilePath == "" ? info.FileName : info.FilePath + "/" + info.FileName), info.NewFilePath + "\\" + info.NewFileName).Replace("/", "\\");
                    temp.NewFilePath = path.Contains("\\") ? path.Substring(0, path.LastIndexOf("\\")) : "";
                    temp.NewFileName = temp.FileName;
                    subFileInfoList.Add(temp);
                }
                //设置所有子文件信息
                for(int i = 0; i < fileDirPathList.Count(); i++)
                {
                    FileInfoEntity temp = new FileInfoEntity();
                    temp.OperationNo = info.OperationNo;
                    temp.Method = Method.Download;
                    temp.FileType = FileType.File;
                    temp.ResultCode = ResultCode.New;
                    temp.OperationResultMessage = temp.Method.ToString() + " " + temp.FileType.ToString() + " " + temp.ResultCode.ToString();
                    temp.ModifyDateTime = info.ModifyDateTime;
                    temp.OperationProgress = 0;
                    temp.FileNo = info.OperationNo + "_" + i;
                    temp.Length = GetRemoteFileLength(filePathList[i]);
                    temp.FilePath = fileDirPathList[i];
                    temp.FileName = fileNameList[i];
                    temp.NewFilePath = newFileDirPathList[i];
                    temp.NewFileName = newFileNameList[i];
                    subFileInfoList.Add(temp);
                    FileHelper.Serialize<FileInfoEntity>("./Tasks/DownloadFile/" + temp.FileNo + ".dat", temp);
                    FtpHelper.FtpDownloadFileInfoList.Add(temp);
                }
                //创建目标的所有目录
                foreach (string item in newDirPathList)
                {
                    if (!Directory.Exists(item))
                    {
                        Directory.CreateDirectory(item);
                    }
                }
                return subFileInfoList;
            }
        }
        #endregion

        #region 下载
        /// <summary>
        /// 下载
        /// </summary>
        private void Download(FileInfoEntity info)
        {
            if (info == null)
            {
                return;
            }
            try
            {
                 //判断网络是否异常
                if (!CheckLinkState(_Url))
                {
                    info.ResultCode = ResultCode.NetworkError;
                    throw new Exception();
                }
                //判断源文件是否更新或者移除
                if (!CheckExistOfRemoteFile(_Url + (info.FilePath.Trim() == "" ? "" : info.FilePath + "/") + info.FileName) || info.Length != GetRemoteFileLength(_Url + (info.FilePath.Trim() == "" ? "" : info.FilePath + "/") + info.FileName))
                {
                    info.ResultCode = ResultCode.SourceError;
                    throw new Exception();
                }
                //判断目标目录是否被移除
                if (!Directory.Exists(info.NewFilePath))
                {
                    info.ResultCode = ResultCode.TargetError;
                    throw new Exception();
                }
                //启动上传通知事件
                if (Started != null)
                {
                    Started(this, new FileEventArgs(info, null));
                }
                //执行任务
                InnerDownloadFile(info);
                //将临时文件名改名
                if (File.Exists(info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName))
                {
                    info.ResultCode = ResultCode.Failed;
                    throw new Exception("目标目录已存在同名文件，不允许覆盖");
                }
                FileHelper.RenameFile((info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName) + ".tmp", (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName));
                //完成
                info.ResultCode = ResultCode.Done;
                info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString();
                FileHelper.DeleteFile("./Tasks/DownloadFile/" + info.FileNo + ".dat");
                FtpDownloadFileInfoList.Remove(info);
                if (Done != null)
                {
                    Done(this, new FileEventArgs(info, null));
                }
            }
            catch
            {
                //任务失败
                if (info.ResultCode == ResultCode.Cancelled || info.ResultCode == ResultCode.Failed || info.ResultCode == ResultCode.SourceError || info.ResultCode == ResultCode.TargetError)
                {
                    info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString();
                    FileHelper.DeleteFile((_Info.NewFilePath == "" ? "" : _Info.NewFilePath + "/") + _Info.NewFileName + ".tmp");
                    FileHelper.DeleteFile("./Tasks/DownloadFile/" + info.FileNo + ".dat");
                    FtpHelper.FtpDownloadFileInfoList.Remove(_Info);
                    if (info.ResultCode == ResultCode.Cancelled)
                    {
                        if (Cancelled != null)
                        {
                            Cancelled(this, new FileEventArgs(info, new Exception(info.OperationResultMessage) { }));
                        }
                    }
                    else
                    {
                        if (Failed != null)
                        {
                            Failed(this, new FileEventArgs(info, new Exception(info.OperationResultMessage) { }));
                        }
                    }
                }
                //任务暂停
                else
                {
                    info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString();
                    if (Paused != null)
                    {
                        Paused(this, new FileEventArgs(info, new Exception(info.OperationResultMessage) { }));
                    }
                }
            }
            finally
            {
                //nothing
            }
        }
        #endregion

        #region 下载实施函数
        /// <summary>
        /// 下载实施函数
        /// </summary>
        private bool InnerDownloadFile(FileInfoEntity info)
        {
            bool done = false;
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;
            FileStream fstream = null;
            try
            {
                string url = _Url + (info.FilePath.Trim() == "" ? "" : info.FilePath + "/") + info.FileName;
                string targetPath = (info.NewFilePath.Trim() == "" ? "" : info.NewFilePath + "\\").Replace("\\\\", "\\") + info.NewFileName + ".tmp";
                //重新获取偏移量
                long offset = FileHelper.GetFileLength(targetPath);
                offset = offset == -1 ? 0 : offset;
                FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.DownloadFile);
                //指定下载的偏移点
                reqFtp.ContentOffset = offset;
                //指定下载的文件大小
                reqFtp.ContentLength = info.Length;
                responFtp = (FtpWebResponse)reqFtp.GetResponse();
                responStream = responFtp.GetResponseStream();

                //读写数据流缓冲区
                //2K
                int bufferSize = 2048;
                byte[] buff = new byte[bufferSize];

                //数据流读操作
                fstream = new FileStream(targetPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                //重置文件流偏移点
                fstream.Seek(offset, 0);
                //从偏移量处开始下载
                int len = 0;
                while ((len = responStream.Read(buff, 0, bufferSize)) > 0)
                {
                    fstream.Write(buff, 0, len);
                    offset += len;
                    info.OperationProgress = (int)((offset / 1.00 / info.Length) * 100);
                    //更新进度事件
                    Task.Factory.StartNew(() =>
                    {
                        if (ProgressUpdated != null)
                        {
                            ProgressUpdated(this, new FileEventArgs(info, null));
                        }
                    });
                }
                done = true;
            }
            catch (Exception ex)
            {
                info.ResultCode = info.ResultCode == ResultCode.UnFinished ? ResultCode.NetworkError : info.ResultCode;
                throw ex;
            }
            finally
            {
                Dispose(new dynamic[] { responFtp, responStream, fstream });
            }
            return done;
        }
        #endregion
    }
}
