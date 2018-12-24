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
    /// PreDownloadFileWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreDownloadFileWindow : Window
    {
        public MainWindow mainWindow { get; set; }
        public PreDownloadFileWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //获取Ftp文件夹列表
            List<string> ls = FtpHelper.GetRemoteFiles("ftp://" + FtpHelper.IP + "/");
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
            info.FileType = FileType.File;
            info.FilePath = path.Contains("/") ? path.Substring(0, path.LastIndexOf("/")) : "";
            info.FileName = path.Contains("/") ? path.Substring(path.LastIndexOf("/") + 1, path.Length - path.LastIndexOf("/") - 1) : path;
            info.NewFilePath = tbLocalPath.Text;
            info.NewFileName = tbDirName.Text;
            if (File.Exists(info.NewFilePath + "\\" + info.NewFileName))
            {
                System.Windows.MessageBox.Show("目标文件已存在");
            }
            else
            {
                this.Close();
                FtpHelper.TaskInit(info);
                FtpTaskUserControl taskControl = new FtpTaskUserControl(info);
                taskControl.btnRun.Content = info.ResultCode == ResultCode.New ? "Wait" : "Run";
                taskControl.mainWindow = mainWindow;
                mainWindow.DownloadTaskSum++;
                mainWindow.lvDownload.Items.Add(taskControl);
                mainWindow.ReflashUI();
            }
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
