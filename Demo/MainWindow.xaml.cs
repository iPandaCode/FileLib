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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using FileLib;
using FtpUserControlLib;

namespace Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static FtpUploadListForm uploadForm;
        public static FtpDownloadListForm downloadForm;
        public MainWindow()
        {
            InitializeComponent();          
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FtpHelper.InitFtp("192.168.1.100:8003","admin","873709");
        }

        private void btnUploadDirectory_Click(object sender, RoutedEventArgs e)
        {
            PreUploadDirectoryWindow pre = new PreUploadDirectoryWindow();
            pre.Show();
        }
        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            PreUploadFileWindow pre = new PreUploadFileWindow();
            pre.Show();
        }


        private void btnDownloadDirectory_Click(object sender, RoutedEventArgs e)
        {
            PreDownloadDirectoryWindow pre = new PreDownloadDirectoryWindow();
            pre.Show();
        }

        private void btnDownloadFile_Click(object sender, RoutedEventArgs e)
        {
            PreDownloadFileWindow pre = new PreDownloadFileWindow();
            pre.Show();
        }
    }
}
