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

namespace FtpUserControlLib
{
    /// <summary>
    /// FtpListUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class FtpListUserControl : UserControl
    {
        #region 属性
        /// <summary>
        /// 窗体句柄
        /// </summary>
        public System.Windows.Forms.Form listForm = null;
        /// <summary>
        /// 任务总数
        /// </summary>
        private long _Sum = 0;
        /// <summary>
        /// 完成数
        /// </summary>
        private long _Done = 0;
        #endregion

        #region 构造函数
        /// <summary>
        ///  构造函数
        /// </summary>
        public FtpListUserControl()
        {
            InitializeComponent();
            _Sum = 0;
            _Done = 0;
        }
        #endregion

        #region 控件加载处理
        /// <summary>
        /// 控件加载处理
        /// </summary>
        private void FtpListUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Elapsed += (s, ex) =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    FtpDownUploadFileUserControl ctr =
                        (
                            from x in lboxTask.Items.OfType<FtpDownUploadFileUserControl>()
                            where x.IsRunning == true
                            select x
                        ).FirstOrDefault();
                    if (ctr == null)
                    {
                        ctr =
                       (
                           from x in lboxTask.Items.OfType<FtpDownUploadFileUserControl>()
                           where x.IsRunning == false && x.IsWaitting == true && x.IsCanclled == false && x.IsFailed == false
                           select x
                       ).FirstOrDefault();
                        if (ctr != null)
                        {
                            ctr.Run();
                        }
                    }
                }));
            };
        }
        #endregion

        #region 按钮操作
        //全部取消
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < lboxTask.Items.Count; i++)
            {
                FtpDownUploadFileUserControl ctr = lboxTask.Items[i] as FtpDownUploadFileUserControl;
                if (!ctr.IsCanclled && !ctr.IsFailed)
                {
                    ctr.btnCancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }
        //刷新列表
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            List<FtpDownUploadFileUserControl> ls =
                (
                    from x in lboxTask.Items.OfType<FtpDownUploadFileUserControl>()
                    where x.IsFailed == true || x.IsCanclled == true
                    select x
                ).ToList();
            for (int i = 0; i < ls.Count; i++)
            {
                lboxTask.Items.Remove(ls[i]);
            }
            if (lboxTask.Items.Count == 0)
            {
                _Sum = 0;
                _Done = 0;
                RefreshUI();
            }
        }
        //全部暂停
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < lboxTask.Items.Count; i++)
            {
                FtpDownUploadFileUserControl ctr = lboxTask.Items[i] as FtpDownUploadFileUserControl;
                if (ctr.IsRunning || ctr.IsWaitting)
                {
                    ctr.btnRun.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }
        //全部启动
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < lboxTask.Items.Count; i++)
            {
                FtpDownUploadFileUserControl ctr = lboxTask.Items[i] as FtpDownUploadFileUserControl;
                if (!ctr.IsRunning && !ctr.IsWaitting && !ctr.IsCanclled && !ctr.IsFailed)
                {
                    ctr.btnRun.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }
        #endregion

        #region UI刷新
        private void RefreshUI()
        {
            this.Dispatcher.Invoke(() =>
            {
                tbkSum.Text = "总数：" + _Sum;
                tbkDone.Text = "已完成：" + _Done;
            });
        }
        #endregion

        #region 对外函数

        #region 添加任务
        /// <summary>
        /// 添加任务
        /// </summary>
        public void Add(FileInfoEntity info)
        {
            FtpDownUploadFileUserControl ctr = new FtpDownUploadFileUserControl(info);
            ctr.Cancelled += (s, ex) =>
            {
                _Sum--;
                RefreshUI();
            };
            ctr.Cancelled += Cancelled;
            ctr.Failed += Failed;
            ctr.Done += (s, ex) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    lboxTask.Items.Remove(ctr);
                    if (lboxTask.Items.Count == 0)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            System.Threading.Thread.Sleep(2000);
                            this.Dispatcher.Invoke(() =>
                            {
                                listForm.Close();
                            });
                        });
                    }
                });
                _Done++;
                RefreshUI();
            };
            ctr.Done += Done;
            ctr.RegisterEvent();
            lboxTask.Items.Add(ctr);
            _Sum++;
            RefreshUI();
        }
        #endregion

        #endregion

        #region 事件
        //取消
        public event EventHandler<FileEventArgs> Cancelled;
        //完成
        public event EventHandler<FileEventArgs> Done;
        //失败
        public event EventHandler<FileEventArgs> Failed;
        #endregion
    }
}
