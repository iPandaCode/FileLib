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
using System.Windows.Forms;
using FileLib;

namespace Demo
{
    /// <summary>
    /// PreUploadDirectoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreUploadDirectoryWindow : Window
    {
        public MainWindow mainWindow { get; set; }
        public PreUploadDirectoryWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog savePath = new System.Windows.Forms.FolderBrowserDialog();
            if (savePath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbLocalPath.Text = savePath.SelectedPath;
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
            info.FileType = FileType.Directory;
            if (FtpHelper.CheckExistOfRemoteDirectory("ftp://" + FtpHelper.IP + "/" + (string.IsNullOrEmpty(info.NewFilePath) ? "" : info.NewFilePath + "/") + info.NewFileName))
            {
                System.Windows.MessageBox.Show("目标文件夹已存在");
            }
            else
            {
                this.Close();
                List<FileInfoEntity> subDirList = new List<FileInfoEntity>();
                List<FileInfoEntity> ls = FtpHelper.TaskInit(info).Where(x => x.FileType == FileType.File).Select(x => { return x; }).ToList();

                foreach (FileInfoEntity item in ls)
                {
                    FtpTaskUserControl taskControl = new FtpTaskUserControl(item);
                    taskControl.btnRun.Content = item.ResultCode == ResultCode.New ? "Wait" : "Run";
                    taskControl.mainWindow = mainWindow;
                    mainWindow.TaskSum++;
                    mainWindow.lv.Items.Add(taskControl);
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

