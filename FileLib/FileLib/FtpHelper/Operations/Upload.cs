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
        #region 初始化上传任务
        /// <summary>
        /// 初始化上传任务
        /// </summary>
        private static List<FileInfoEntity> UploadInit(FileInfoEntity info)
        {
            if (info == null)
            {
                return null;
            }
            //统一文件路径分隔符
            info.FilePath = Regex.Replace(info.FilePath.Replace("\\", "/").Trim(), @"[/]$", "");
            info.NewFilePath = Regex.Replace(info.NewFilePath.Replace("\\", "/").Trim(), @"[/]$", "");
            //设置操作源信息
            info.OperationNo = Guid.NewGuid().ToString("N"); //任务流水号
            info.ResultCode = ResultCode.New; //新建任务
            info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString(); //操作文本信息
            info.ModifyDateTime = DateTime.Now; //时间
            info.OperationProgress = 0; //进度
            info.FileNo = info.OperationNo; //文件流水号
            info.Length = info.FileType == FileType.File ? FileHelper.GetFileLength((info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName) : FileHelper.GetFileCount((info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName); //操作源大小

            //子文件操作信息（包含子目录）
            List<FileInfoEntity> subFileInfoList = new List<FileInfoEntity>();
            //操作源为文件，返回空的子文件信息列表
            if (info.FileType == FileType.File)
            {
                FileHelper.Serialize<FileInfoEntity>("./Tasks/UploadFile/" + info.FileNo + ".dat", info);
                FtpHelper.FtpUploadFileInfoList.Add(info);
                return null;
            }
            else
            {
                FtpHelper.TryMakeEmptyRemoteDirectory(_Url + (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName));
                FileHelper.Serialize<FileInfoEntity>("./Tasks/UploadDirectory/" + info.FileNo + ".dat", info);
                FtpHelper.FtpUploadDirectoryInfoList.Add(info);
                //获取源的所有子目录完整路径
                List<string> dirPathList = FileHelper.GetDirectories((info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName);
                //获取源的所有子文件完整路径
                List<string> filePathList = FileHelper.GetFiles((info.FilePath == "" ? "" : info.FilePath + "/") + info.FileName);
                //获取源的所有子文件的文件名
                List<string> fileNameList = new List<string>();
                for (int i = 0; i < filePathList.Count(); i++)
                {
                    fileNameList.Add(filePathList[i].Substring(filePathList[i].LastIndexOf("/") + 1, filePathList[i].Length - filePathList[i].LastIndexOf("/") - 1));
                }
                //设置目标的所有子目录的路径（未包含_Url:ftp://192.168.1.100:80**/）
                List<string> newDirPathList = new List<string>();
                foreach (string item in dirPathList)
                {
                    string temp = Regex.Replace(item, "^" + (info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName));           
                    newDirPathList.Add(temp);
                }
                //设置目标的所有子文件的相对路径（过滤_Url）
                List<string> newFileDirPathList = new List<string>();
                foreach (string item in filePathList)
                {
                    string temp = Regex.Replace(item, "^" + (info.FilePath != "" ? info.FilePath + "/" + info.FileName : info.FileName), (info.NewFilePath != "" ? info.NewFilePath + "/" + info.NewFileName : info.NewFileName));
                    try
                    {
                        newFileDirPathList.Add(temp.Substring(0, temp.LastIndexOf("/")));
                    }
                    catch
                    {
                        newFileDirPathList.Add(temp);
                    }
                }
                //设置目标的所有文件的文件名
                List<string> newFileNameList = fileNameList;
                //设置所有子目录信息
                for (int i = 0; i < dirPathList.Count(); i++)
                {
                    FileInfoEntity temp = new FileInfoEntity();
                    temp.OperationNo = info.OperationNo;
                    temp.Method = Method.Upload;
                    temp.FileType = FileType.Directory;
                    temp.ResultCode = ResultCode.New;
                    temp.OperationResultMessage = temp.Method.ToString() + " " + temp.FileType.ToString() + " " + temp.ResultCode.ToString();
                    temp.ModifyDateTime = info.ModifyDateTime;
                    temp.OperationProgress = 0;
                    temp.FileNo = "";
                    temp.Length = 0;
                    temp.FilePath = dirPathList[i].Substring(0, dirPathList[i].LastIndexOf("/"));
                    temp.FileName = dirPathList[i].Substring(dirPathList[i].LastIndexOf("/") + 1, dirPathList[i].Length - dirPathList[i].LastIndexOf("/") - 1);
                    temp.NewFilePath = newDirPathList[i].Substring(0, newDirPathList[i].LastIndexOf("/"));
                    temp.NewFileName = newDirPathList[i].Substring(newDirPathList[i].LastIndexOf("/") + 1, newDirPathList[i].Length - newDirPathList[i].LastIndexOf("/") - 1);
                    subFileInfoList.Add(temp);
                }
                //设置所有子文件信息
                for (int i = 0; i < filePathList.Count(); i++)
                {
                    FileInfoEntity temp = new FileInfoEntity();
                    temp.OperationNo = info.OperationNo;
                    temp.Method = Method.Upload;
                    temp.FileType = FileType.File;
                    temp.ResultCode = ResultCode.New;
                    temp.OperationResultMessage = temp.Method.ToString() + " " + temp.FileType.ToString() + " " + temp.ResultCode.ToString();
                    temp.ModifyDateTime = info.ModifyDateTime;
                    temp.OperationProgress = 0;
                    temp.FileNo = info.OperationNo + "_" + i;
                    temp.Length = FileHelper.GetFileLength(filePathList[i]);
                    temp.FilePath = filePathList[i].Substring(0, filePathList[i].LastIndexOf("/"));
                    temp.FileName = fileNameList[i];
                    temp.NewFilePath = newFileDirPathList[i];
                    temp.NewFileName = fileNameList[i];
                    subFileInfoList.Add(temp);
                    FileHelper.Serialize<FileInfoEntity>("./Tasks/UploadFile/" + temp.FileNo + ".dat", temp);
                    FtpHelper.FtpUploadFileInfoList.Add(temp);
                }
                //创建目标的所有目录
                foreach (string item in newDirPathList)
                {
                    FtpHelper.TryMakeEmptyRemoteDirectory(_Url + item);
                }
                return subFileInfoList;
            }
        }
        #endregion

        #region 上传
        /// <summary>
        /// 上传
        /// </summary>
        private void Upload(FileInfoEntity info)
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
                if (!File.Exists(info.FilePath + "/" + info.FileName) || info.Length != FileHelper.GetFileLength(info.FilePath + "/" + info.FileName))
                {
                    info.ResultCode = ResultCode.SourceError;
                    throw new Exception();
                }
                //判断目标目录是否被移除
                if (!CheckExistOfRemoteDirectory(_Url + info.NewFilePath))
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
                InnerUploadFile(info);
                //再次确认远程文件是否有重名旧文件，有就删除旧文件
                if (CheckExistOfRemoteFile(_Url + (info.NewFilePath == "" ? "" : info.NewFilePath + "/") + info.NewFileName))
                {
                    //移除
                    DeleteRemoteFile(_Url + (info.NewFilePath == "" ? "" : info.NewFilePath + "/") + info.NewFileName);
                }
                //将临时文件名改名
                RenameRemoteFile(_Url + (info.NewFilePath == "" ? "" : info.NewFilePath + "/"), info.NewFileName + ".tmp", info.NewFileName);
                //完成
                info.ResultCode = ResultCode.Done;
                info.OperationResultMessage = info.Method.ToString() + " " + info.FileType.ToString() + " " + info.ResultCode.ToString();
                FileHelper.DeleteFile("./Tasks/UploadFile/" + info.FileNo + ".dat");
                FtpUploadFileInfoList.Remove(info);
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
                    Task.Factory.StartNew(() =>
                    {
                        FtpHelper.DeleteRemoteFile(_Url + (info.NewFilePath == "" ? "" : info.NewFilePath + "/") + info.NewFileName + ".tmp");
                    });                   
                    FileHelper.DeleteFile("./Tasks/UploadFile/" + info.FileNo + ".dat");
                    FtpUploadFileInfoList.Remove(info);
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

        #region 上传实施函数
        /// <summary>
        /// 上传实施函数
        /// </summary>
        private bool InnerUploadFile(FileInfoEntity info)
        {           
            bool done = false;
            FtpWebRequest reqFtp = null;
            FtpWebResponse responFtp = null;
            Stream responStream = null;
            FileStream fstream = null;
            try
            {
                string url = _Url + (info.NewFilePath == "" ? "" : info.NewFilePath + "/") + info.NewFileName + ".tmp";
                //重新获取偏移量
                long offset = GetRemoteFileLength(url);
                offset = offset == -1 ? 0 : offset;
                FtpConnectConfig(url, out reqFtp, WebRequestMethods.Ftp.UploadFile);
                //指定上传的偏移点
                reqFtp.ContentOffset = offset;
                //指定上传的文件大小
                reqFtp.ContentLength = info.Length;
                responStream = reqFtp.GetRequestStream();

                //读写数据流缓冲区
                //2K
                int bufferSize = 2048;
                byte[] buff = new byte[bufferSize];

                //数据流读操作
                fstream = new FileStream(info.FilePath + "/" + info.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                //重置文件流偏移点
                fstream.Seek(offset, 0);
                //从偏移量处开始上传
                int len = 0;
                while ((len = fstream.Read(buff, 0, bufferSize)) > 0)
                {
                    responStream.Write(buff, 0, len);
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
