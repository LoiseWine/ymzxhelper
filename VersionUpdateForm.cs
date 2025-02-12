using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ymzxhelper
{
    public class VersionUpdateForm : Form
    {
        public VersionUpdateForm()
        {
            // 设置窗体基本属性
            this.Text = "版本更新";
            this.ClientSize = new Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 使用 TableLayoutPanel 组织内容
            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // 标题区域
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // 文本区域
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // 按钮区域
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            this.Controls.Add(tlp);

            // 标题 Label
            Label lblTitle = new Label
            {
                Text = "版本更新内容",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微软雅黑", 14, FontStyle.Bold)
            };
            tlp.Controls.Add(lblTitle, 0, 0);

            // 使用 RichTextBox 显示更新内容（允许复制，并自动检测超链接）
            RichTextBox richText = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = SystemColors.Window,
                Font = new Font("微软雅黑", 10),
                DetectUrls = true,
                Text = "1.1.3版本更新内容：\r\n" +
                       "- 修复了无法后台挂机问题\r\n" +
                       "- 修复了窗口显示异常问题\r\n" +
                       "- 修复了无法多开的问题\r\n" +
                       "- 软件更新地址：https://wwtu.lanzoue.com/b0sxhgj9i 密码:e5fg\r\n" +
                       "- 点击链接打开网页下载更新。"
            };
            // RichTextBox 自动检测 URL，如果用户点击超链接，默认行为是打开系统浏览器
            // 如果需要自定义链接点击行为，可以处理 LinkClicked 事件
            richText.LinkClicked += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("无法打开链接: " + ex.Message);
                }
            };
            tlp.Controls.Add(richText, 0, 1);

            // 按钮区域：添加一个“确定”按钮
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };
            Button btnOK = new Button
            {
                Text = "确定",
                Size = new Size(80, 30)
            };
            btnOK.Click += (s, e) => this.Close();
            pnlButtons.Controls.Add(btnOK);
            tlp.Controls.Add(pnlButtons, 0, 2);
        }
    }
}
