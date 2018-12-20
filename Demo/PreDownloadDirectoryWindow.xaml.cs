using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.IO;
using FileLib;

namespace Demo
{
    /// <summary>
    /// PreDownloadDirectoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreDownloadDirectoryWindow : Window
    {
        public MainWindow mainWindow { get; set; }
        public PreDownloadDirectoryWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //获取Ftp文件夹列表
            List<string> ls = FtpHelper.GetRemoteDirectories("ftp://" + FtpHelper.IP + "/");
            foreach (string item in ls)
            {
                comBox.Items.Add(Regex.Replace(item, @"^ftp://" + FtpHelper.IP + "/", ""));
            }
        }
        private void comBox_Selected(object sender, RoutedEventArgs e)
        {
            string sourcePath = comBox.SelectedValue as string;
            tbDirName.Text = sourcePath.Substring(sourcePath.LastIndexOf("/") + 1, sourcePath.Length - sourcePath.LastIndexOf("/") - 1);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog savePath = new System.Windows.Forms.FolderBrowserDialog();
            if (savePath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbLocalPath.Text = savePath.SelectedPath;
            }
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            string path = comBox.SelectedValue.ToString();
            FileInfoEntity info = new FileInfoEntity();
            info.Method = Method.Download;
            info.FileType = FileType.Directory;
            info.FilePath = path.Contains("/") ? path.Substring(0, path.LastIndexOf("/")) : "";
            info.FileName = path.Contains("/") ? path.Substring(path.LastIndexOf("/") + 1, path.Length - path.LastIndexOf("/") - 1) : path;
            info.NewFilePath = tbLocalPath.Text;
            info.NewFileName = tbDirName.Text;
            if (Directory.Exists(info.NewFilePath + "\\" + info.NewFileName))
            {

                System.Windows.MessageBox.Show("目标文件夹已存在");
            }
            else
            {
                this.Close();
                List<FileInfoEntity> ls = FtpHelper.TaskInit(info).Where(x => x.FileType == FileType.File).Select(x => { return x; }).ToList();
                foreach (FileInfoEntity item in ls)
                {
                    FtpTaskUserControl taskControl = new FtpTaskUserControl(item);
                    taskControl.btnRun.Content = item.ResultCode == ResultCode.New ? "Wait" : "Run";
                    taskControl.mainWindow = mainWindow;
                    mainWindow.DownloadTaskSum++;
                    mainWindow.lvDownload.Items.Add(taskControl);
                }
                mainWindow.ReflashUI();
            }
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

