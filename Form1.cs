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
        //private TableLayoutPanel mainLayout;


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
            this.Text = "元梦之星农场助手 1.1.3";
            this.ClientSize = new Size(1500, 720);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.None;


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
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2
            };
            // 左侧区域固定宽度 1280
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1280));
            // 右侧区域固定宽度 220
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            // 行设置为 100% 高度
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            // 将主布局添加到窗体
            this.Controls.Add(mainLayout);



            // ------------------------------
            // 左侧区域：添加 WebView2 控件（填充整个区域）
            // ------------------------------
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            mainLayout.Controls.Add(webView, 0, 0);

            // ------------------------------
            // 右侧区域：创建控制区容器
            // ------------------------------
            // 使用 TableLayoutPanel 分 3 行，利用上下两行的百分比行实现中间内容垂直居中
            controlLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            controlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // 第一行：只读文本框区域（AutoSize）
            controlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // 第二行：按钮区域（AutoSize）
            controlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // 第三行：图标区域（AutoSize）
            controlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.Controls.Add(controlLayout, 1, 0);


            // ------------------------------
            // 添加只读文本框到控制区第一行
            // ------------------------------
            textBox = new TextBox
            {
                Text = "此软件开源免费，禁止以任何商业形式售卖.\r\n\r\n预设循环的挂机程序：R键复位 - A键走向无人机 - Q键执行无人机 - 鼠标点击右下角消除各种弹窗\r\n\r\n请勿最小化，保持窗口置于前台。建议等进农场之后再点击开始挂机\r\n\r\n更新地址 https://wwtu.lanzoue.com/b0sxhgj9i 密码:e5fg",
                ReadOnly = true,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("微软雅黑", 10),
                Width = 180,
                Height = 300,
                Margin = new Padding(0, 10, 0, 10),
                Multiline = true,
                WordWrap = true,
                Anchor = AnchorStyles.None
            };
            controlLayout.Controls.Add(textBox, 0, 0);

            // ------------------------------
            // 添加按钮面板到控制区第二行（用于垂直排列按钮）
            // ------------------------------
            innerPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None
            };
            controlLayout.Controls.Add(innerPanel, 0, 1);

            // 按钮 "刷新"：尺寸 180x40
            btnRefresh = new Button
            {
                Text = "刷新",
                Size = new Size(180, 40),
                Margin = new Padding(0, 10, 0, 10)
            };
            innerPanel.Controls.Add(btnRefresh);
            btnRefresh.Click += BtnRefresh_Click;


            // 按钮 "一键挂机/停止挂机"：尺寸 180x40
            btnToggleHang = new Button
            {
                Text = "一键挂机/停止挂机",
                Size = new Size(180, 40),
                Margin = new Padding(0, 10, 0, 10)
            };
            innerPanel.Controls.Add(btnToggleHang);
            btnToggleHang.Click += BtnToggleHang_Click;


            // 按钮 "关于作者"：尺寸 180x40
            btnAbout = new Button
            {
                Text = "关于作者",
                Size = new Size(180, 40),
                Margin = new Padding(0, 10, 0, 10)
            };
            innerPanel.Controls.Add(btnAbout);
            btnAbout.Click += BtnAbout_Click;

            // 新增按钮“版本更新”，放在“关于作者”下方，样式与其他按钮相同
            Button btnVersionUpdate = new Button
            {
                Text = "版本更新",
                Size = new Size(180, 40),
                Margin = new Padding(0, 10, 0, 10)
            };
            innerPanel.Controls.Add(btnVersionUpdate);
            btnVersionUpdate.Click += BtnVersionUpdate_Click;




            // ------------------------------
            // 添加图标面板到控制区第三行（水平排列 3 个图标）
            // ------------------------------
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
            // 构造一个唯一的用户数据文件夹路径，比如放在本地应用数据目录下 1.1.3版新增解决多开问题
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string uniqueFolder = System.IO.Path.Combine(baseFolder, "ymzxhelper", Guid.NewGuid().ToString());


            // 创建 WebView2 独立环境，并指定独立的用户数据文件夹
            var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, uniqueFolder);
            await webView.EnsureCoreWebView2Async(env);

            

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
        //“版本更新”按钮的点击事件处理程序
        private void BtnVersionUpdate_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "1.1.3版本更新内容：\r\n" +
                "修复了无法后台挂机问题，但未完全解决\r\n" +
                "修复了窗口显示异常问题\r\n" +
                "修复了无法多开的问题\r\n" +
                "修复了抢占鼠标的问题，用更好的方式执行脚本\r\n" +
                "预设循环的挂机程序：R键复位 - A键走向无人机 - Q键执行无人机 - 鼠标点击右下角消除各种弹窗\r\n" +
                "软件更新地址 https://wwtu.lanzoue.com/b0sxhgj9i 密码:e5fg",
                "版本更新",  // 添加一个标题字符串
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
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
        //在需要自动化操作的地方，使用 WebView2 的 ExecuteScriptAsync 注入 JavaScript
        private async Task RunAutomationLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 1. 点击页面正中央（激活窗口）
                await webView.CoreWebView2.ExecuteScriptAsync(@"
            (function(){
                var x = window.innerWidth / 2;
                var y = window.innerHeight / 2;
                var evt = new MouseEvent('click', {
                    bubbles: true,
                    cancelable: true,
                    view: window,
                    clientX: x,
                    clientY: y
                });
                document.elementFromPoint(x, y).dispatchEvent(evt);
            })();
        ");

                // 2. 模拟按下并释放 R 键
                await webView.CoreWebView2.ExecuteScriptAsync(@"
            (function(){
                var evtDown = new KeyboardEvent('keydown', {key: 'R', keyCode: 82, bubbles: true});
                var evtUp   = new KeyboardEvent('keyup', {key: 'R', keyCode: 82, bubbles: true});
                document.dispatchEvent(evtDown);
                document.dispatchEvent(evtUp);
            })();
        ");

                // 3. 模拟按下 A 键，保持1.2秒后释放
                await webView.CoreWebView2.ExecuteScriptAsync(@"
            (function(){
                var evtDown = new KeyboardEvent('keydown', {key: 'A', keyCode: 65, bubbles: true});
                document.dispatchEvent(evtDown);
            })();
        ");
                await Task.Delay(1200, token);
                await webView.CoreWebView2.ExecuteScriptAsync(@"
            (function(){
                var evtUp = new KeyboardEvent('keyup', {key: 'A', keyCode: 65, bubbles: true});
                document.dispatchEvent(evtUp);
            })();
        ");

                // 4. 模拟按下并释放 Q 键
                await webView.CoreWebView2.ExecuteScriptAsync(@"
            (function(){
                var evtDown = new KeyboardEvent('keydown', {key: 'Q', keyCode: 81, bubbles: true});
                var evtUp   = new KeyboardEvent('keyup', {key: 'Q', keyCode: 81, bubbles: true});
                document.dispatchEvent(evtDown);
                document.dispatchEvent(evtUp);
            })();
        ");

                // 5. 模拟点击右下角的位置：计算 x = window.innerWidth - 10, y = window.innerHeight - 10
                for (int i = 0; i < 26; i++)
                {
                    if (token.IsCancellationRequested)
                        break;

                    await webView.CoreWebView2.ExecuteScriptAsync(@"
                (function(){
                    var x = window.innerWidth - 10;
                    var y = window.innerHeight - 10;
                    var evt = new MouseEvent('click', {
                        bubbles: true,
                        cancelable: true,
                        view: window,
                        clientX: x,
                        clientY: y
                    });
                    document.elementFromPoint(x, y).dispatchEvent(evt);
                })();
            ");
                    await Task.Delay(5000, token);
                }
            }
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
