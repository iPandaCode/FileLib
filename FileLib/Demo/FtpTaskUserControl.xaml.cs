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
using FileLib;

namespace Demo
{
    /// <summary>
    /// FtpTaskUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class FtpTaskUserControl : UserControl
    {
        public MainWindow mainWindow { get; set; } = null;
        private FtpHelper _FtpHelper { get; set; } = null;

        public FtpTaskUserControl(FileInfoEntity info)
        {
            InitializeComponent();
            this.Dispatcher.Invoke(delegate
            {
                pBar.Value = (int)info.OperationProgress;
            });
            _FtpHelper = new FtpHelper(info);
            tblockFileName.Text = info.FileName;
            _FtpHelper.Started += _FtpHelper_Started;
            _FtpHelper.Paused += _FtpHelper_Paused;
            _FtpHelper.Resumed += _FtpHelper_Resumed;
            _FtpHelper.Cancelled += _FtpHelper_Cancelled;
            _FtpHelper.Failed += _FtpHelper_Failed;
            _FtpHelper.ProgressUpdated += _FtpHelper_ProgressUpdated;
            _FtpHelper.Done += _FtpHelper_Done;
            //用户控件销毁前处理
            Dispatcher.ShutdownStarted += (object sender, EventArgs e) =>
            {
                if (_FtpHelper.TaskThread != null)
                {
                    _FtpHelper.TaskThread.Abort();
                    _FtpHelper.TaskThread = null;
                }
            };

        }
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (btnRun.Content.ToString() == "Run" || btnRun.Content.ToString() == "Wait")
            {
                _FtpHelper.Run();
                btnRun.Content = "Pause";
            }
            else
            {
                _FtpHelper.Pause();
                btnRun.Content = "Run";
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _FtpHelper.Cancel();
            btnRun.Visibility = Visibility.Hidden;
            btnCancel.Visibility = Visibility.Hidden;
        }
        private void _FtpHelper_ProgressUpdated(object sender, FileEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(delegate
                {
                    pBar.Value = (int)e.FileInfoEntity.OperationProgress;
                });

            }
            catch { }


        }
        private void _FtpHelper_Started(object sender, FileEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                tblockTip.Text = e.FileInfoEntity.ResultCode.ToString();
            });
        }
        private void _FtpHelper_Paused(object sender, FileEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                btnRun.Content = "Run";
                tblockTip.Text = e.FileInfoEntity.ResultCode.ToString();
            });
        }
        private void _FtpHelper_Resumed(object sender, FileEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                tblockTip.Text = e.FileInfoEntity.ResultCode.ToString();
            });
        }
        private void _FtpHelper_Failed(object sender, FileEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                tblockTip.Text = e.FileInfoEntity.ResultCode.ToString();
            });
        }
        private void _FtpHelper_Cancelled(object sender, FileEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                tblockTip.Text = e.FileInfoEntity.ResultCode.ToString();
            });
        }
        private void _FtpHelper_Done(object sender, FileEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                if (e.FileInfoEntity.Method == Method.Upload)
                {
                    mainWindow.TaskDoneCount++;
                    mainWindow.lv.Items.Remove(this);
                }
                else if (e.FileInfoEntity.Method == Method.Download)
                {
                    mainWindow.DownloadTaskDoneCount++;
                    mainWindow.lvDownload.Items.Remove(this);
                }
                tblockTip.Text = e.FileInfoEntity.ResultCode.ToString();
                mainWindow.ReflashUI();
            });
        }
    }
}

