using System;
using System.Drawing;
using System.Windows.Forms;

namespace ymzxhelper
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            // 设置 About 窗体基本属性
            this.Text = "关于作者";
            this.ClientSize = new Size(400, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // 创建用于显示说明文字的 Label
            Label lblText = new Label
            {
                Text = "此元梦之星农场助手开源免费，请勿以各种形式商业交易。\r\n作者 星宝@Sakura\r\n如果你觉得好用不妨赞赏下",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Font = new Font("微软雅黑", 10),
                Padding = new Padding(10)
            };

            // 创建用于显示图片的 PictureBox
            PictureBox pbImage = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            // 尝试加载 zs.jpg 图片（确保 zs.jpg 存在于输出目录）
            try
            {
                pbImage.Image = Image.FromFile("zs.jpg");
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载图片失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 使用 TableLayoutPanel 将 Label 和 PictureBox 垂直排列
            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            // 第一行高度固定为 120 像素，第二行占满剩余空间
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            tlp.Controls.Add(lblText, 0, 0);
            tlp.Controls.Add(pbImage, 0, 1);

            this.Controls.Add(tlp);
        }
    }
}
