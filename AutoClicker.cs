using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AutoClicker
{
    public class AutoClickerForm : Form
    {
        // Windows API
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        // UI Components
        private TextBox speedInput;
        private Label statusLabel;
        private Point dragStartPoint;
        private bool isDragging = false;

        // Clicker State
        private bool isClicking = false;
        private int clicksPerSecond = 10;
        private Thread clickThread;

        public AutoClickerForm()
        {
            InitializeUI();
            RegisterHotkeys();
        }

        private void InitializeUI()
        {
            // Window settings
            this.Text = "自动点击";
            this.Size = new Size(140, 70);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Speed input
            speedInput = new TextBox
            {
                Location = new Point(10, 15),
                Size = new Size(50, 20),
                Text = "10",
                Font = new Font("Arial", 10)
            };
            speedInput.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };

            // Status indicator
            statusLabel = new Label
            {
                Location = new Point(70, 12),
                Size = new Size(20, 20),
                Text = "■",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Help label
            Label helpLabel = new Label
            {
                Location = new Point(95, 15),
                Size = new Size(35, 20),
                Text = "F8",
                Font = new Font("Arial", 8),
                ForeColor = Color.Gray
            };

            this.Controls.Add(speedInput);
            this.Controls.Add(statusLabel);
            this.Controls.Add(helpLabel);

            // Drag support
            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;
        }

        private void RegisterHotkeys()
        {
            // Global hotkey handler
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F8)
                {
                    ToggleClicking();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    Application.Exit();
                }
            };

            // Global hook for F8 (even when not focused)
            Application.AddMessageFilter(new HotkeyMessageFilter(this));
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = this.Location;
                newLocation.X += e.X - dragStartPoint.X;
                newLocation.Y += e.Y - dragStartPoint.Y;
                this.Location = newLocation;
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void ToggleClicking()
        {
            if (isClicking)
            {
                StopClicking();
            }
            else
            {
                StartClicking();
            }
        }

        private void StartClicking()
        {
            if (!int.TryParse(speedInput.Text, out clicksPerSecond))
                return;

            if (clicksPerSecond < 1 || clicksPerSecond > 500)
                return;

            isClicking = true;
            statusLabel.ForeColor = Color.Lime;
            speedInput.Enabled = false;

            clickThread = new Thread(ClickLoop)
            {
                IsBackground = true
            };
            clickThread.Start();
        }

        private void StopClicking()
        {
            isClicking = false;
            statusLabel.ForeColor = Color.Red;
            speedInput.Enabled = true;
        }

        private void ClickLoop()
        {
            int interval = 1000 / clicksPerSecond;

            while (isClicking)
            {
                try
                {
                    PerformClick();
                    Thread.Sleep(interval);
                }
                catch
                {
                    break;
                }
            }
        }

        private void PerformClick()
        {
            POINT point;
            GetCursorPos(out point);

            mouse_event(MOUSEEVENTF_LEFTDOWN, point.X, point.Y, 0, UIntPtr.Zero);
            Thread.Sleep(20);
            mouse_event(MOUSEEVENTF_LEFTUP, point.X, point.Y, 0, UIntPtr.Zero);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isClicking = false;
            base.OnFormClosing(e);
        }

        // Global hotkey message filter
        private class HotkeyMessageFilter : IMessageFilter
        {
            private const int WM_KEYDOWN = 0x0100;
            private AutoClickerForm form;

            public HotkeyMessageFilter(AutoClickerForm form)
            {
                this.form = form;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_KEYDOWN)
                {
                    Keys key = (Keys)(int)m.WParam;
                    if (key == Keys.F8)
                    {
                        form.ToggleClicking();
                        return true;
                    }
                }
                return false;
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AutoClickerForm());
        }
    }
}
