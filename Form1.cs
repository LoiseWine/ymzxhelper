using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace ymzxhelper
{
    public partial class Form1 : Form
    {
        // 声明控件
        private WebView2 webView;

        // 挂机相关字段
        private bool isAutomationActive = false;
        private CancellationTokenSource? automationCts;
        private Task? automationTask;

        // 主布局容器
        private TableLayoutPanel mainLayout;

        // 右侧控制区容器，用于实现垂直居中
        private TableLayoutPanel controlLayout;
        // 内部流式布局面板，用于排列文本框和按钮
        private FlowLayoutPanel innerPanel;

        // 控制区内的具体控件
        private TextBox textBox;
        private Button btnRefresh;
        private Button btnToggleHang;
        private Button btnAbout;



        public Form1()
        {
            InitializeComponent();

            // 设置窗体属性
            this.Text = "元梦之星农场助手";
            this.ClientSize = new Size(1500, 720);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            try
            {
                this.Icon = new Icon("app.ico");
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载图标失败：" + ex.Message);
            }

            // ------------------------------
            // 创建主布局 TableLayoutPanel：2列1行
            // ------------------------------
            mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 1;
            mainLayout.ColumnCount = 2;
            // 设置第一列宽度为绝对 1280，第二列宽度为绝对 220
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1280));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.Controls.Add(mainLayout);

           

            // ------------------------------
            // 左侧区域：添加 WebView2 控件（填充整个区域）
            // ------------------------------
            webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            mainLayout.Controls.Add(webView, 0, 0);

            // ------------------------------
            // 右侧区域：创建控制区容器
            // ------------------------------
            // 使用 TableLayoutPanel 分 3 行，利用上下两行的百分比行实现中间内容垂直居中
            controlLayout = new TableLayoutPanel();
            controlLayout.Dock = DockStyle.Fill;
            controlLayout.RowCount = 3;
            controlLayout.ColumnCount = 1;
            controlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // 上方填充
            controlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // 中间内容区域
            controlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // 下方填充
            controlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainLayout.Controls.Add(controlLayout, 1, 0);

            

            // 在控制区中间行加入一个 FlowLayoutPanel，用于垂直排列控件
            innerPanel = new FlowLayoutPanel();
            innerPanel.FlowDirection = FlowDirection.TopDown;
            innerPanel.AutoSize = true;
            innerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // 设置 Anchor 为 None，可在所在单元格中自动居中
            innerPanel.Anchor = AnchorStyles.None;
            controlLayout.Controls.Add(innerPanel, 0, 1);

            // ------------------------------
            // 在 innerPanel 内添加控件：文本框 + 3 个按钮
            // ------------------------------
            // 文本框：尺寸 180x30
            //textBox = new TextBox();
            //textBox.Text = "此助手开源免费";
            //textBox.Size = new Size(180, 180);
            // 设置上下间距
            //textBox.Margin = new Padding(0, 10, 0, 10);
            //innerPanel.Controls.Add(textBox);
            textBox = new TextBox
            {
                Text = "此软件开源免费，禁止以任何商业形式售卖.\r\n 默认脚每隔130秒循环一次。\r\n原理很简单：用webview2轻量化加载腾讯先锋网页云游戏，左侧网页区，右侧控制区。\r\n 软件更新地址 https://wwtu.lanzoue.com/b0sxhgj9i 密码:6666 ",    // 请根据需要修改预设显示内容
                ReadOnly = true,           // 用户不可编辑
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("微软雅黑", 10),
                Width = 180,
                Height = 400,               // 调整高度以适应多行文本
                Margin = new Padding(0, 10, 0, 10),
                Multiline = true,          // 允许多行文本
                WordWrap = true            // 启用自动换行
            };
            // 设置 Anchor 为 None，使其在单元格内居中
            textBox.Anchor = AnchorStyles.None;
            controlLayout.Controls.Add(textBox, 0, 0);

            // 按钮 "刷新"：尺寸 180x40
            btnRefresh = new Button();
            btnRefresh.Text = "刷新";
            btnRefresh.Size = new Size(180, 40);
            btnRefresh.Margin = new Padding(0, 10, 0, 10);
            innerPanel.Controls.Add(btnRefresh);
            btnRefresh.Click += BtnRefresh_Click;


            // 按钮 "一键挂机/停止挂机"：尺寸 180x40
            btnToggleHang = new Button();
            btnToggleHang.Text = "一键挂机/停止挂机";
            btnToggleHang.Size = new Size(180, 40);
            btnToggleHang.Margin = new Padding(0, 10, 0, 10);
            innerPanel.Controls.Add(btnToggleHang);
            // 设置“一键挂机/停止挂机”按钮点击事件
            btnToggleHang.Click += BtnToggleHang_Click;


            // 按钮 "关于作者"：尺寸 180x40
            btnAbout = new Button();
            btnAbout.Text = "关于作者";
            btnAbout.Size = new Size(180, 40);
            btnAbout.Margin = new Padding(0, 10, 0, 10);
            innerPanel.Controls.Add(btnAbout);

            // 为 "关于作者" 按钮添加点击事件处理程序
            btnAbout.Click += BtnAbout_Click;

            // 添加图标面板到控制区第三行（水平排列 3 个图标）
            FlowLayoutPanel iconPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None,
                Margin = new Padding(0, 10, 0, 10)
            };
            controlLayout.Controls.Add(iconPanel, 0, 2);

            // 创建 GitHub 图标
            PictureBox pbGithub = new PictureBox
            {
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                Margin = new Padding(5)
            };
            try
            {
                pbGithub.Image = Image.FromFile("github.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载 github.png 失败：" + ex.Message);
            }
            pbGithub.Click += (s, e) => OpenUrl("https://github.com/LoiseWine/ymzxhelper");
            iconPanel.Controls.Add(pbGithub);

            // 创建 YouTube 图标
            PictureBox pbYoutube = new PictureBox
            {
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                Margin = new Padding(5)
            };
            try
            {
                pbYoutube.Image = Image.FromFile("youtube.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载 youtube.png 失败：" + ex.Message);
            }
            pbYoutube.Click += (s, e) => OpenUrl("https://www.youtube.com/@LoiseWine");
            iconPanel.Controls.Add(pbYoutube);

            // 创建 bilibili 图标
            PictureBox pbBilibili = new PictureBox
            {
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                Margin = new Padding(5)
            };
            try
            {
                pbBilibili.Image = Image.FromFile("bilibili.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载 bilibili.png 失败：" + ex.Message);
            }
            pbBilibili.Click += (s, e) => OpenUrl("https://space.bilibili.com/3493086715972236");
            iconPanel.Controls.Add(pbBilibili);

            // 订阅窗体加载事件，初始化 WebView2
            this.Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // 异步初始化 WebView2 环境
            await webView.EnsureCoreWebView2Async(null);

           

            // 导航到目标网页（可根据实际需求修改网址）
            webView.CoreWebView2.Navigate("https://gamer.qq.com/v2/game/96897?ichannel=pcgames0Fpcgames1");
        }
        // "关于作者" 按钮的点击事件处理程序
        private void BtnAbout_Click(object sender, EventArgs e)
        {
            // 弹出 AboutForm 对话框
            using (AboutForm about = new AboutForm())
            {
                about.ShowDialog(this);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            // 检查 WebView2 是否已经初始化完毕
            if (webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Reload();
            }
            else
            {
                MessageBox.Show("网页尚未加载完成，请稍后重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 打开指定网址，使用默认浏览器
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法打开链接: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // “一键挂机/停止挂机”按钮的点击事件处理程序
        // “一键挂机/停止挂机”按钮点击事件
        private async void BtnToggleHang_Click(object? sender, EventArgs e)
        {
            if (!isAutomationActive)
            {
                isAutomationActive = true;
                btnToggleHang.Text = "停止挂机";
                automationCts = new CancellationTokenSource();
                automationTask = RunAutomationLoop(automationCts.Token);
            }
            else
            {
                automationCts?.Cancel();
                isAutomationActive = false;
                btnToggleHang.Text = "一键挂机/停止挂机";
            }
        }
        /// <summary>
        /// 后台挂机流程：
        /// 每个循环依次：
        ///   1. 点击 WebView2 正中央以激活窗口；
        ///   2. 模拟按下并释放 R 键；
        ///   3. 模拟按下 A 键保持 1.2 秒，再释放；
        ///   4. 模拟按下并释放 Q 键；
        ///   5. 计算 WebView2 右下角向左上各偏移 10 像素的点击位置，
        ///      每隔 5 秒点击一次，共点击 26 次；
        /// 循环无限执行，直到用户停止挂机。
        /// </summary>
        private async Task RunAutomationLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 1. 点击 WebView2 正中央
                Point center = GetControlCenter(webView);
                SetCursorPos(center.X, center.Y);
                MouseClick();

                // 2. 模拟按下并释放 R 键 (虚拟键码 0x52)
                SendKey(0x52, true);
                SendKey(0x52, false);

                // 3. 模拟按下 A 键 (0x41)，保持 1.2 秒后释放
                SendKey(0x41, true);
                await Task.Delay(1200, token);
                SendKey(0x41, false);

                // 4. 模拟按下并释放 Q 键 (0x51)
                SendKey(0x51, true);
                SendKey(0x51, false);

                // 5. 计算目标点击位置：WebView2 右下角向左偏移 20 像素、向上偏移 20 像素
                Point bottomRight = GetControlBottomRight(webView);
                Point clickPoint = new Point(bottomRight.X - 20, bottomRight.Y - 20);

                // 每隔 2 秒点击一次，总共点击 65 次
                for (int i = 0; i < 65; i++)
                {
                    if (token.IsCancellationRequested)
                        break;
                    SetCursorPos(clickPoint.X, clickPoint.Y);
                    MouseClick();
                    await Task.Delay(2000, token);
                }
            }
        }
        // 辅助方法：获取控件正中央的屏幕坐标
        private Point GetControlCenter(Control ctrl)
        {
            if (ctrl.InvokeRequired)
            {
                return (Point)ctrl.Invoke(new Func<Point>(() =>
                    ctrl.PointToScreen(new Point(ctrl.ClientSize.Width / 2, ctrl.ClientSize.Height / 2))
                ));
            }
            else
            {
                return ctrl.PointToScreen(new Point(ctrl.ClientSize.Width / 2, ctrl.ClientSize.Height / 2));
            }
        }
        // 辅助方法：获取控件右下角的屏幕坐标
        private Point GetControlBottomRight(Control ctrl)
        {
            if (ctrl.InvokeRequired)
            {
                return (Point)ctrl.Invoke(new Func<Point>(() =>
                    ctrl.PointToScreen(new Point(ctrl.ClientSize.Width, ctrl.ClientSize.Height))
                ));
            }
            else
            {
                return ctrl.PointToScreen(new Point(ctrl.ClientSize.Width, ctrl.ClientSize.Height));
            }
        }
        /// <summary>
        /// 使用 Windows API 模拟键盘输入
        /// </summary>
        /// <param name="key">虚拟键码</param>
        /// <param name="keyDown">true 表示按下，false 表示释放</param>
        private void SendKey(ushort key, bool keyDown)
        {
            INPUT input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = key,
                        wScan = 0,
                        dwFlags = keyDown ? 0 : KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            INPUT[] inputs = new INPUT[] { input };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
        /// <summary>
        /// 使用 Windows API 模拟鼠标左键点击
        /// </summary>
        private void MouseClick()
        {
            INPUT[] inputs = new INPUT[2];

            // 鼠标左键按下
            inputs[0].type = INPUT_MOUSE;
            inputs[0].U.mi.dx = 0;
            inputs[0].U.mi.dy = 0;
            inputs[0].U.mi.mouseData = 0;
            inputs[0].U.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            inputs[0].U.mi.time = 0;
            inputs[0].U.mi.dwExtraInfo = IntPtr.Zero;

            // 鼠标左键释放
            inputs[1].type = INPUT_MOUSE;
            inputs[1].U.mi.dx = 0;
            inputs[1].U.mi.dy = 0;
            inputs[1].U.mi.mouseData = 0;
            inputs[1].U.mi.dwFlags = MOUSEEVENTF_LEFTUP;
            inputs[1].U.mi.time = 0;
            inputs[1].U.mi.dwExtraInfo = IntPtr.Zero;

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
        #region Windows API 与结构体定义

        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion



    }
}
