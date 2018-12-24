using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileLib;

namespace FtpUserControlLib
{
    public partial class FtpDownloadListForm : Form
    {
        #region 属性
        /// <summary>
        /// 初始化标记
        /// </summary>
        private bool _IsInit = false;
        #endregion

        #region 构造函数
        /// <summary>
        ///  构造函数
        /// </summary>
        public FtpDownloadListForm()
        {
            InitializeComponent();
            _IsInit = false;
            ftpListUserControl1.listForm = this;
        }
        #endregion

        #region 窗体关闭时
        private void FtpUploadListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FtpDownUploadFileUserControl ctr =
                 (
                      from x in ftpListUserControl1.lboxTask.Items.OfType<FtpDownUploadFileUserControl>()
                      where x.IsRunning == true || x.IsWaitting == true
                      select x
                 ).FirstOrDefault();
            if (ctr != null)
            {
                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("有文件正在下载，确定退出?", "系统提示", messButton);
                //退出
                if (dr == DialogResult.OK)
                {
                    e.Cancel = false;
                }
                //取消退出
                else
                {
                    e.Cancel = true;
                }
            }
        }   
        #endregion       

        #region 对外函数

        #region 初始化
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            ftpListUserControl1.Cancelled += Cancelled;
            ftpListUserControl1.Failed += Failed;
            ftpListUserControl1.Done += Done;
            for (int i = 0; i < FtpHelper.FtpDownloadFileInfoList.Count(); i++)
            {
                ftpListUserControl1.Add(FtpHelper.FtpDownloadFileInfoList[i]);
            }
            _IsInit = true;
        }
        #endregion

        #region 添加任务
        /// <summary>
        /// 添加任务
        /// </summary>
        public void Add(FileInfoEntity info)
        {
            if (!_IsInit)
            {
                MessageBox.Show("请先初始化FtpListForm再添加任务");
                return;
            }
            ftpListUserControl1.Add(info);
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
