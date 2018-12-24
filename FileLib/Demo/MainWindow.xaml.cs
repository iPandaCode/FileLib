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
using System.Web;
using FileLib;

namespace Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public int TaskSum = 0;
        public int TaskDoneCount = 0;
        public int DownloadTaskSum = 0;
        public int DownloadTaskDoneCount = 0;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //初始化FTP
            FtpHelper.InitFtp("192.168.1.109:8003", "admin", "873709");
           
            foreach (FileInfoEntity item in FtpHelper.FtpUploadFileInfoList)
            {
                FtpTaskUserControl taskControl = new FtpTaskUserControl(item);
                taskControl.mainWindow = this;
                lv.Items.Add(taskControl);
                TaskSum++;
            }
            foreach (FileInfoEntity item in FtpHelper.FtpDownloadFileInfoList)
            {
                FtpTaskUserControl taskControl = new FtpTaskUserControl(item);
                taskControl.mainWindow = this;
                lvDownload.Items.Add(taskControl);
                DownloadTaskSum++;
            }
            ReflashUI();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 1000;
            timer.Enabled = true;
            timer.Elapsed += (s, ex) =>
            {
                this.Dispatcher.Invoke(delegate
                {
                    if (lv.Items.OfType<FtpTaskUserControl>().Where(x => x.btnRun.Content.ToString() == "Pause" && x.btnRun.Visibility == Visibility.Visible).Select(x => { return x; }).FirstOrDefault() == null)
                    {
                        FtpTaskUserControl taskControl = lv.Items.OfType<FtpTaskUserControl>().Where(x => x.btnRun.Content.ToString() == "Wait").Select(x => { return x; }).FirstOrDefault();
                        if (taskControl != null)
                        {
                            taskControl.btnRun.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                    }
                    if (lvDownload.Items.OfType<FtpTaskUserControl>().Where(x => x.btnRun.Content.ToString() == "Pause" && x.btnRun.Visibility == Visibility.Visible).Select(x => { return x; }).FirstOrDefault() == null)
                    {
                        FtpTaskUserControl taskControl = lvDownload.Items.OfType<FtpTaskUserControl>().Where(x => x.btnRun.Content.ToString() == "Wait").Select(x => { return x; }).FirstOrDefault();
                        if (taskControl != null)
                        {
                            taskControl.btnRun.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                    }
                });
            };
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Application.Current.Windows.Count; i++)
            {
                if (Application.Current.Windows[i] != Application.Current.MainWindow)
                {
                    Application.Current.Windows[i].Close();
                }
            }
        }
        private void btnUploadDirectory_Click(object sender, RoutedEventArgs e)
        {
            PreUploadDirectoryWindow pre = new PreUploadDirectoryWindow();
            pre.mainWindow = this;
            pre.Show();
        }
        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            PreUploadFileWindow pre = new PreUploadFileWindow();
            pre.mainWindow = this;
            pre.Show();
        }
       

        private void btnDownloadDirectory_Click(object sender, RoutedEventArgs e)
        {
            PreDownloadDirectoryWindow pre = new PreDownloadDirectoryWindow();
            pre.mainWindow = this;
            pre.Show();
        }

        private void btnDownloadFile_Click(object sender, RoutedEventArgs e)
        {
            PreDownloadFileWindow pre = new PreDownloadFileWindow();
            pre.mainWindow = this;
            pre.Show();
        }

        private void btnReflash_Click(object sender, RoutedEventArgs e)
        {
            var ctrs = (from x in lv.Items.OfType<FtpTaskUserControl>()
                        where x.btnRun.Visibility != Visibility.Visible
                        select x).ToArray();
            for (int i = 0; i < ctrs.Count(); i++)
            {
                lv.Items.Remove(ctrs[i]);
                TaskSum--;
            }
            tblockTaskSum.Text = "上传任务总数：" + TaskSum;
            tblockDoneCount.Text = "已完成：" + TaskDoneCount;

            ctrs = (from x in lvDownload.Items.OfType<FtpTaskUserControl>()
                    where x.btnRun.Visibility != Visibility.Visible
                    select x).ToArray();
            for (int i = 0; i < ctrs.Count(); i++)
            {
                lvDownload.Items.Remove(ctrs[i]);
                DownloadTaskSum--;
            }
            tblockDownloadTaskSum.Text = "下载任务总数：" + DownloadTaskSum;
            tblockDownloadDoneCount.Text = "已完成：" + DownloadTaskDoneCount;
        }

        public void ReflashUI()
        {
            this.Dispatcher.Invoke(delegate
            {
                if (FtpHelper.FtpUploadFileInfoList.Count == 0 && lv.Items.Count == 0)
                {
                    TaskSum = 0;
                    TaskDoneCount = 0;
                }
                if (FtpHelper.FtpDownloadFileInfoList.Count == 0 && lvDownload.Items.Count == 0)
                {
                    DownloadTaskSum = 0;
                    DownloadTaskDoneCount = 0;
                }
                tblockTaskSum.Text = "上传任务总数：" + TaskSum;
                tblockDoneCount.Text = "已完成：" + TaskDoneCount;
                tblockDownloadTaskSum.Text = "下载任务总数：" + DownloadTaskSum;
                tblockDownloadDoneCount.Text = "已完成：" + DownloadTaskDoneCount;
            });
        }
    }
}
