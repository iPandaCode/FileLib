using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace FileLib
{
    public partial class FileHelper
    {
        #region 属性
        /// <summary>
        /// 任务线程句柄
        /// </summary>
        public Thread TaskThread { get; set; } = null;
        /// <summary>
        /// 任务信息句柄
        /// </summary>
        internal FileInfoEntity _Info { get; set; } = null;
        #endregion

        #region 构造函数
        public FileHelper(FileInfoEntity info)
        {
            _Info = info;
        }
        #endregion

        #region 任务初始化
        public static List<FileInfoEntity> TaskInit(FileInfoEntity info)
        {
            if (info.Method == Method.Rename)
                return RenameTaskInit(info);
            if (info.Method == Method.Delete)
                return DeleteTaskInit(info);
            if (info.Method == Method.Make)
                return MakeTaskInit(info);
            if (info.Method == Method.Move)
                return MoveTaskInit(info);
            if (info.Method == Method.Copy)
                return CopyTaskInit(info);
            if (info.Method == Method.Open)
                return OpenTaskInit(info);
            else
                return null;
        }
        #endregion

        #region 任务操作

        #region 任务执行
        /// <summary>
        /// 任务执行
        /// </summary>
        public void Run()
        {
            TaskThread = new Thread(() =>
            {
                _Info.ResultCode = ResultCode.UnFinished;
                if (_Info.Method == Method.Rename)
                    Rename(_Info);
                if (_Info.Method == Method.Delete)
                    Delete(_Info);
                if (_Info.Method == Method.Make)
                    Make(_Info);
                if (_Info.Method == Method.Move)
                    Move(_Info);
                if (_Info.Method == Method.Copy)
                    Copy(_Info);
                if (_Info.Method == Method.Open)
                    Open(_Info);
                else
                    return;
            });
            TaskThread.IsBackground = true;
            TaskThread.Start();
        }
        #endregion

        #endregion

        #region 其他函数

        #region 字符串转MD5哈希值字符串
        /// <summary>
        /// 字符串转MD5哈希值字符串
        /// </summary>
        internal static string GetMD5ByString(string str)
        {
            System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
            //将输入字符串转换为字节数组并计算哈希数据  
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
            //创建一个 Stringbuilder 来收集字节并创建字符串  
            StringBuilder sb = new StringBuilder();
            // 循环遍历哈希数据的每一个字节并格式化为十六进制字符串  
            for (int i = 0; i < data.Length; i++)
            {
                //加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
                sb.Append(data[i].ToString("x2"));
            }
            // 返回十六进制字符串  
            return sb.ToString();
        }
        #endregion

        #region 获取指定文件的大小（字节）
        /// <summary>
        /// 获取指定文件的大小（字节）
        /// </summary>
        internal static long GetFileLength(string path)
        {
            if (!File.Exists(path))
            {
                return -1;
            }
            return (new FileInfo(path)).Length;
        }
        #endregion

        #region 获取指定目录的大小（字节，包括子目录包含的文件）
        /// <summary>
        /// 获取指定目录的大小（字节，包括子目录包含的文件）
        /// </summary>
        internal static long GetDirectoryLength(string path)
        {
            if (!Directory.Exists(path))
                return -1;

            long len = 0;

            foreach (FileInfo item in (new DirectoryInfo(path)).GetFiles())
            {
                len += item.Length;
            }
            foreach (string dir in Directory.GetDirectories(path))
            {
                long temp = GetDirectoryLength(dir);
                len += temp == -1 ? 0 : temp;
            }
            return len;
        }
        #endregion

        #region 获取指定目录包含的文件个数（包括子目录包含的文件）
        /// <summary>
        /// 获取指定目录包含的文件个数（包括子目录包含的文件）
        /// </summary>
        internal static long GetFileCount(string path)
        {
            if (!Directory.Exists(path))
                return -1;

            long len = 0;

            foreach (FileInfo item in (new DirectoryInfo(path)).GetFiles())
            {
                len++;
            }
            foreach (string dir in Directory.GetDirectories(path))
            {
                long temp = GetFileCount(dir);
                len += temp == -1 ? 0 : temp;
            }
            return len;
        }
        #endregion

        #region 获取指定目录包含的子目录完整的路径列表（包括子目录包含的目录）
        /// <summary>
        /// 获取指定目录包含的子目录完整的路径列表（包括子目录包含的目录）
        /// 
        internal static List<string> GetDirectories(string path)
        {
            List<string> ls = new List<string>() { };

            if (!Directory.Exists(path))
                return ls;

            foreach (DirectoryInfo item in (new DirectoryInfo(path)).GetDirectories())
            {
                ls.Add(item.FullName.Replace("\\", "/"));
            }
            foreach (string item in Directory.GetDirectories(path))
            {
                ls.AddRange(GetDirectories(item));
            }
            return ls;
        }
        #endregion

        #region 获取指定目录包含的文件完整的路径列表（包括子目录包含的文件）
        /// <summary>
        /// 获取指定目录包含的文件完整的路径列表（包括子目录包含的文件）
        /// </summary>
        internal static List<string> GetFiles(string path)
        {
            List<string> ls = new List<string>();

            if (!Directory.Exists(path))
                return ls;

            foreach (FileInfo item in (new DirectoryInfo(path)).GetFiles())
            {
                ls.Add(item.FullName.Replace("\\", "/"));
            }
            foreach (string item in Directory.GetDirectories(path))
            {
                ls.AddRange(GetFiles(item));
            }
            return ls;
        }
        #endregion

        #region 移除指定文件
        /// <summary>
        /// 移除指定文件
        /// </summary>
        internal static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 移除指定目录
        /// <summary>
        /// 移除指定目录
        /// </summary>
        internal static void DeleteDirectory(string path)
        {
            try
            {
                Directory.Delete(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion   

        #region 重命名指定的文件
        /// <summary>
        /// 重命名指定的文件
        /// </summary>
        internal static void RenameFile(string srcPath, string tarPath)
        {
            MoveFile(srcPath, tarPath);
        }
        #endregion

        #region 重命名指定的目录
        /// <summary>
        /// 重命名指定的目录
        /// </summary>
        internal static void RenameDirectory(string srcPath, string tarPath)
        {
            try
            {
                if (Directory.Exists(tarPath))
                {
                    throw new Exception("目标目录已存在同名文件夹，不允许覆盖");
                }
                Directory.Move(srcPath, tarPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 移动指定的文件
        /// <summary>
        /// 移动指定的文件
        /// </summary>
        internal static void MoveFile(string srcPath, string tarPath)
        {
            try
            {
                if (File.Exists(tarPath))
                {
                    throw new Exception("目标目录已存在同名文件，不允许覆盖");
                }
                File.Move(srcPath, tarPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 新建空文件
        /// <summary>
        /// 新建空文件
        /// </summary>
        internal static void MakeFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    throw new Exception("目标目录已存在同名文件，不允许覆盖");
                }
                File.Create(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 新建空目录
        /// <summary>
        /// 新建空目录
        /// </summary>
        internal static void MakeDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    throw new Exception("目标目录已存在同名目录，不允许覆盖");
                }
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 拷贝文件
        /// <summary>
        /// 拷贝文件
        /// </summary>
        internal static void CopyFile(string srcPath, string tarPath)
        {
            try
            {
                File.Copy(srcPath, tarPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 序列化
        /// <summary>
        /// 序列化
        /// </summary>
        internal static void Serialize<T>(string path, T obj)
        {
            FileStream fs = null;
            try
            {
                //打开文件流
                fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                //以二进制格式序列化
                (new BinaryFormatter()).Serialize(fs, obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                fs.Dispose();
            }
        }
        #endregion

        #region 反序列化
        /// <summary>
        /// 反序列化
        /// </summary>
        internal static T Deserialize<T>(string path)
        {
            FileStream fs = null;
            try
            {
                //打开文件流
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                //反序列化
                return (T)(new BinaryFormatter()).Deserialize(fs);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                fs.Dispose();
            }
        }
        #endregion

        #endregion

        #region 事件
        //完成
        public event EventHandler<FileEventArgs> Done;
        //失败
        public event EventHandler<FileEventArgs> Failed;
        //进度更新
        public event EventHandler<FileEventArgs> ProgressUpdated;
        #endregion
    }
}
