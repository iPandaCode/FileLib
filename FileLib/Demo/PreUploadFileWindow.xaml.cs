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
using FileLib;

namespace Demo
{
    /// <summary>
    /// PreUploadFileWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreUploadFileWindow : Window
    {
        public MainWindow mainWindow { get; set; }
        public PreUploadFileWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog savePath = new System.Windows.Forms.OpenFileDialog();
            if (savePath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbLocalPath.Text = savePath.FileName;
                string path = tbLocalPath.Text;
                string filePath = path.Substring(0, path.LastIndexOf("\\"));
                string fileName = path.Substring(filePath.Length + 1, path.Length - filePath.Length - 1);
                tbRomteFile.Text = fileName;
            }
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            FileInfoEntity info = new FileInfoEntity();
            string path = tbLocalPath.Text;
            info.FilePath = path.Substring(0, path.LastIndexOf("\\"));
            info.FileName = path.Substring(info.FilePath.Length + 1, path.Length - info.FilePath.Length - 1);
            info.NewFilePath = "";
            info.NewFileName = tbRomteFile.Text;
            info.FileType = FileType.File;
            if (FtpHelper.CheckExistOfRemoteFile("ftp://" + FtpHelper.IP + "/" + (string.IsNullOrEmpty(info.NewFilePath) ? "" : info.NewFilePath + "/") + info.NewFileName))
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
                mainWindow.lv.Items.Add(taskControl);
                mainWindow.TaskSum++;
                mainWindow.ReflashUI();
            }
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

