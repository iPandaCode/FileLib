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
using System.Timers;
using System.Text.RegularExpressions;
using System.Drawing;
using FileLib;


namespace FtpUserControlLib
{
    /// <summary>
    /// FtpDownUploadFileUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class FtpDownUploadFileUserControl : UserControl
    {
        #region 属性
        /// <summary>
        /// 任务处理状态标记
        /// </summary>
        public bool IsRunning { get; private set; } = false;
        /// <summary>
        /// 任务等待状态标记
        /// </summary>
        public bool IsWaitting { get; private set; } = false;
        /// <summary>
        /// 任务失败标记
        /// </summary>
        public bool IsFailed { get; private set; } = false;
        /// <summary>
        /// 任务取消标记
        /// </summary>
        public bool IsCanclled { get; private set; } = false;
        /// <summary>
        /// FtpHelper任务执行对象句柄
        /// </summary>
        private FtpHelper _FtpHelper = null;
        /// <summary>
        /// 任务信息句柄
        /// </summary>
        private FileInfoEntity _Info = null;
        /// <summary>
        /// 文件大小
        /// </summary>
        private long _Length = 0;
        /// <summary>
        /// 进度
        /// </summary>
        private int _Progress = 0;
        /// <summary>
        /// 网速
        /// </summary>
        private long _Speed = 0;
        /// <summary>
        /// 定时器句柄
        /// </summary>
        private Timer _Timer = null;
        #endregion

        #region 构造函数
        /// <summary>
        /// 弃用
        /// </summary>
        public FtpDownUploadFileUserControl()
        {
            InitializeComponent();
        }
        public FtpDownUploadFileUserControl(FileInfoEntity info)
        {
            InitializeComponent();
            _Info = info;
            _FtpHelper = new FtpHelper(info);
            Init();
        }
        #endregion

        #region 按钮操作
        //运行
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            btnRun.IsEnabled = false;
            wpUnnormalTip.Visibility = Visibility.Collapsed;
            wpNormalTip.Visibility = Visibility.Visible;
            if (IsRunning)
            {
                IsRunning = false;
                IsWaitting = false;
                btnRun.Content = "\ue63d";
                tbkNormalTip.Text = "正在暂停中";
                CloseTimer();
                _FtpHelper.Pause();
            }
            else
            {
                if (IsWaitting)
                {
                    IsRunning = false;
                    IsWaitting = false;
                    btnRun.Content = "\ue63d";
                    tbkNormalTip.Text = "进度" + _Info.OperationProgress + "%  已暂停";
                }
                else
                {
                    IsRunning = false;
                    IsWaitting = true;
                    btnRun.Content = "\ue69d";
                    tbkNormalTip.Text = "排队等待中";
                }
            }
        }
        //取消
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsRunning = false;
            IsWaitting = false;
            btnRun.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
            tbkNormalTip.Text = "任务正在取消...";
            CloseTimer();
            _FtpHelper.Cancel();
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            IsRunning = false;
            IsWaitting = _Info.ResultCode == ResultCode.New ? true : false;
            IsFailed = false;
            IsCanclled = false;
            _Length = _Info.Length;
            _Speed = 0;
            _Progress = 0;
            InitEvent();
            InitUI();
        }
        #endregion

        #region UI初始化
        /// <summary>
        /// UI初始化
        /// </summary>
        private void InitUI()
        {
            switch (_Info.Method)
            {
                case Method.Download: tbkMethod.Text = "\ue659"; break;
                case Method.Upload: tbkMethod.Text = "\ue658"; break;
                default: tbkMethod.Text = "未知"; break;
            }
            //word
            if (Regex.IsMatch(_Info.FileName, @"(.doc[ ]*)$|(.docx[ ]*)$", RegexOptions.IgnoreCase))
            {
                tbkExtension.Text = "\ue67d";
                tbkExtension.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x43, 0x7b, 0xf1));
            }
            //excel
            else if (Regex.IsMatch(_Info.FileName, @"(.xls[ ]*)$|(.xlsx[ ]*)$", RegexOptions.IgnoreCase))
            {
                tbkExtension.Text = "\ue68a";
                tbkExtension.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x1e, 0xcd, 0x95));
            }
            //ppt
            else if (Regex.IsMatch(_Info.FileName, @"(.ppt[ ]*)$|(.pptx[ ]*)$|(.pps[ ]*)$|(.pot[ ]*)$", RegexOptions.IgnoreCase))
            {
                tbkExtension.Text = "\ue6a7";
                tbkExtension.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0xf0, 0x62, 0x1f));
            }
            //压缩文件
            else if (Regex.IsMatch(_Info.FileName, @"(.zip[ ]*)$|(.rar[ ]*)$|(.arj[ ]*)$|(.iso[ ]*)$|(.z[ ]*)$|(.cab[ ]*)$|(.tar[ ]*)$|(.gz[ ]*)$|(.bz2[ ]*)$|(.uue[ ]*)$", RegexOptions.IgnoreCase))
            {
                tbkExtension.Text = "\ue620";
                tbkExtension.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0xb4, 0x6a, 0x11));
            }
            //图片
            else if (Regex.IsMatch(_Info.FileName, @"(.bmp[ ]*)$|(.jpg[ ]*)$|(.jpeg[ ]*)$|(.png[ ]*)$|(.gif[ ]*)$|(.tga[ ]*)$|(.tif[ ]*)$", RegexOptions.IgnoreCase))
            {
                tbkExtension.Text = "\ue60b";
                tbkExtension.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x37, 0xc9, 0x00));
            }
            //视频
            else if (Regex.IsMatch(_Info.FileName, @"(.mp4[ ]*)$|(.avi[ ]*)$|(.wmv[ ]*)$|(.asf[ ]*)$|(.rm[ ]*)$|(.rmvb[ ]*)$|(.asx[ ]*)$|(.mpg[ ]*)$|(.mpeg[ ]*)$|(.mpe[ ]*)$|(.3gp[ ]*)$|(.mov[ ]*)$|(.m4v[ ]*)$|(.mkv[ ]*)$", RegexOptions.IgnoreCase))
            {
                tbkExtension.Text = "\ue600";
                tbkExtension.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0xda, 0xda, 0x08));
            }
            //音乐
            else if (Regex.IsMatch(_Info.FileName, @"(.mp1[ ]*)$|(.mp2[ ]*)$|(.mp3[ ]*)$|(.wav[ ]*)$|(.aif[ ]*)$|(.aiff[ ]*)$|(.au[ ]*)$|(.ra[ ]*)$|(.rm[ ]*)$|(.rma[ ]*)$|(.mid[ ]*)$|(.rmi[ ]*)$", RegexOptions.IgnoreCase))
            {
                tbkExtension.Text = "\ue69c";
                tbkExtension.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x00, 0xea, 0xda));
            }
            //其他
            else
            {
                tbkExtension.Text = "\ue6d5";
                tbkExtension.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0xb9, 0xc3, 0xd3));
            }
            tbkFileName.Text = _Info.NewFileName;
            tbkFileSize.Text = GetFileSizeString(_Info.Length);
            tbkNormalTip.Text = _Info.ResultCode == ResultCode.New ? "排队等待中" : ("进度" + _Info.OperationProgress + "%  已暂停");
            btnRun.Content = _Info.ResultCode == ResultCode.New ? "\ue69d" : "\ue63d";
            wpUnnormalTip.Visibility = Visibility.Collapsed;
            wpNormalTip.Visibility = Visibility.Visible;
        }
        #endregion

        #region 事件初始化
        /// <summary>
        /// 事件初始化
        /// </summary>
        private void InitEvent()
        {          
            _FtpHelper.Started += _FtpHelper_Started; //启动
            _FtpHelper.ProgressUpdated += _FtpHelper_ProgressUpdated; //进度更新
            _FtpHelper.Paused += _FtpHelper_Paused; //暂停
            _FtpHelper.Cancelled += _FtpHelper_Cancelled; //取消
            _FtpHelper.Failed += _FtpHelper_Failed; //失败
            _FtpHelper.Done += _FtpHelper_Done; //完成
            this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted; //进程退出时
        }
        private void _FtpHelper_Started(object sender, FileEventArgs e)
        {
            try
            {
                IsRunning = true;
                IsWaitting = false;
                this.Dispatcher.Invoke(delegate
                {
                    btnRun.IsEnabled = true;
                });
                StartTimer();
            }
            catch
            {
                //nothing
            };
        }        
        private void _FtpHelper_ProgressUpdated(object sender, FileEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(delegate
                {
                    _Progress = e.FileInfoEntity.OperationProgress;
                    _Speed += 2048;
                    pBar.Value = IsRunning == false ? 0 : _Progress;
                });
            }
            catch
            {
                //nothing
            };
        }
        private void _FtpHelper_Paused(object sender, FileEventArgs e)
        {
            try
            {
                IsRunning = false;
                IsWaitting = false;
                CloseTimer();
                this.Dispatcher.Invoke(delegate
                {
                    btnRun.IsEnabled = true;
                    tbkNormalTip.Text = "进度" + e.FileInfoEntity.OperationProgress + "%  已暂停";
                    pBar.Value = 0;
                    btnRun.Content = "\ue63d";
                    if (e.FileInfoEntity.ResultCode == ResultCode.NetworkError)
                    {
                        tbkUnnormalTip.Text = "网络异常：已暂停";
                        wpUnnormalTip.Visibility = Visibility.Visible;
                        wpNormalTip.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch
            {
                //nothing
            };
        }
        private void _FtpHelper_Cancelled(object sender, FileEventArgs e)
        {
            try
            {
                IsRunning = false;
                IsWaitting = false;
                IsCanclled = true;
                CloseTimer();
                this.Dispatcher.Invoke(delegate
                {
                    pBar.Value = 0;
                    tbkUnnormalTip.Text = "任务已删除";
                    wpUnnormalTip.Visibility = Visibility.Visible;
                    wpNormalTip.Visibility = Visibility.Collapsed;
                    btnRun.Visibility = Visibility.Collapsed;
                    btnCancel.Visibility = Visibility.Collapsed;
                });
            }
            catch
            {
                //nothing
            };
        }
        private void _FtpHelper_Failed(object sender, FileEventArgs e)
        {
            try
            {
                IsRunning = false;
                IsWaitting = false;
                IsFailed = true;
                CloseTimer();
                this.Dispatcher.Invoke(delegate
                {
                    pBar.Value = 0;
                    if (e.FileInfoEntity.ResultCode == ResultCode.SourceError)
                    {
                        tbkUnnormalTip.Text = "任务失败：源文件已被修改或移除";
                    }
                    else if (e.FileInfoEntity.ResultCode == ResultCode.TargetError)
                    {
                        tbkUnnormalTip.Text = "任务失败：目标目录不存在";
                    }
                    else if (e.FileInfoEntity.ResultCode == ResultCode.Failed)
                    {
                        tbkUnnormalTip.Text = "任务失败：目标目录已存在同名文件，不允许覆盖";
                    }
                    wpUnnormalTip.Visibility = Visibility.Visible;
                    wpNormalTip.Visibility = Visibility.Collapsed;
                    btnRun.Visibility = Visibility.Collapsed;
                    btnCancel.Visibility = Visibility.Collapsed;
                });
            }
            catch
            {
                //nothing
            };
        }
        private void _FtpHelper_Done(object sender, FileEventArgs e)
        {
            try
            {
                IsRunning = false;
                IsWaitting = false;
                CloseTimer();
                this.Dispatcher.Invoke(delegate
                {
                    pBar.Value = 0;
                    tbkNormalTip.Text = e.FileInfoEntity.ModifyDateTime.ToString("yyyy-MM-dd") + "  已完成";
                    btnRun.Visibility = Visibility.Collapsed;
                    btnCancel.Visibility = Visibility.Collapsed;
                });
            }
            catch
            {
                //nothing
            };
        }
        /// <summary>
        /// 进程退出时
        /// </summary>
        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            if (_Info != null && !IsCanclled && !IsFailed)
            {
                _Info.ResultCode = ResultCode.UnFinished;
                FileHelper.Serialize<FileInfoEntity>("./Tasks/" + _Info.Method + _Info.FileType + "/" + _Info.FileNo + ".dat", _Info);
            }
        }
        #endregion

        #region 启动定时器
        /// <summary>
        /// 启动定时器
        /// </summary>
        private void StartTimer()
        {
            _Timer = new Timer();
            _Timer.Enabled = true;
            _Timer.AutoReset = true;
            _Timer.Interval = 1000;
            _Timer.Elapsed += (s, ex) =>
            {
                if (IsRunning)
                {
                    string strSpeed = GetNetworkSpeedString(_Speed);
                    string strRtime = GetRemainingTimeString(_Speed, _Length, _Progress);
                    this.Dispatcher.Invoke(delegate
                    {
                        tbkNormalTip.Text = strRtime + "  " + strSpeed;
                    });
                    _Speed = 0;
                }
            };
            _Timer.Start();
        }
        #endregion

        #region 关闭定时器
        /// <summary>
        /// 关闭定时器
        /// </summary>
        private void CloseTimer()
        {
            if (_Timer != null)
            {
                _Timer.Dispose();
                _Timer = null;
            }
        }
        #endregion

        #region 获取文件大小的字符串
        /// <summary>
        /// 获取文件大小的字符串
        /// </summary>
        private string GetFileSizeString(long len)
        {
            if (len < 0)
                return "未知大小";

            string strLen = string.Empty;
            double temp;
            if ((temp = len / 1.00 / 1024 / 1024 / 1024) > 0.01)
                strLen = string.Format("{0:F}", temp) + "GB";
            else if ((temp = len / 1.00 / 1024 / 1024) > 0.01)
                strLen = string.Format("{0:F}", temp) + "MB";
            else if ((temp = len / 1.00 / 1024) > 0.01)
                strLen = string.Format("{0:F}", temp) + "KB";
            else
                strLen = string.Format("{0:F}", len) + "B";
            return strLen;
        }
        #endregion

        #region 获取网速字符串
        /// <summary>
        /// 获取网速字符串
        /// </summary>
        private string GetNetworkSpeedString(long speed)
        {
            if (speed <= 0)
                return "0/bs";

            string strLen = string.Empty;
            double temp;
            if ((temp = speed / 1.00 / 1024 / 1024 / 1024) > 0.1)
                strLen = string.Format("{0:F}", temp) + "/Gbs";
            else if ((temp = speed / 1.00 / 1024 / 1024) > 0.01)
                strLen = string.Format("{0:F}", temp) + "/Mbs";
            else if ((temp = speed / 1.00 / 1024) > 0.01)
                strLen = string.Format("{0:F}", temp) + "/Kbs";
            else
                strLen = string.Format("{0:F}", speed) + "/bs";
            return strLen;
        }
        #endregion

        #region 获取剩余任务时间字符串
        /// <summary>
        /// 获取剩余任务时间字符串
        /// </summary>
        private string GetRemainingTimeString(long speed, long len, int progress)
        {
            if (speed <= 0 || len <= 0)
                return "";

            string strLen = string.Empty;
            int h, m, s;
            long rtime = ((long)(len * (100 - progress) / 100.00)) / speed;
            if ((h = (int)(rtime / 60 / 60)) >= 1)
            {
                strLen = string.Format("剩余时间：大于1小时");
            }
            else
            {
                m = (int)(rtime / 60);
                s = (int)(rtime - m * 60);
                strLen = string.Format("剩余时间：00 : {0} : {1}", (m < 10 ? "0" + m : m.ToString()), (s < 10 ? "0" + s : s.ToString()));
            }
            return strLen;
        }
        #endregion

        #region 对外函数

        #region 注册事件
        /// <summary>
        /// 注册事件
        /// </summary>
        public void RegisterEvent()
        {
            _FtpHelper.Done += Done;
            _FtpHelper.Cancelled += Cancelled;
            _FtpHelper.Failed += Failed;
        }
        #endregion

        #region 启动任务
        /// <summary>
        /// 启动任务
        /// </summary>
        public void Run()
        {
            IsRunning = true;
            IsWaitting = false;
            _FtpHelper.Run();
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
