using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BongoBeat
{
    public class BongoBeatForm : Form
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP   = 0x0002;
        private const int  WM_HOTKEY         = 0x0312;
        private const int  HOTKEY_ID         = 1;
        private const int  HOTKEY_ESC        = 2;

        private TextBox speedInput;
        private TextBox keyInput;
        private Label   statusLabel;
        private Point   dragStartPoint;
        private bool    isDragging = false;

        private bool   isClicking     = false;
        private int    clicksPerSecond = 10;
        private byte   activeVk       = 0xA4; // VK_LMENU (Left Alt)
        private Thread clickThread;

        public BongoBeatForm()
        {
            InitializeUI();
            RegisterHotkeys();
        }

        private void InitializeUI()
        {
            this.Text            = "BongoBeat";
            this.Size            = new Size(140, 95);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost         = true;
            this.StartPosition   = FormStartPosition.CenterScreen;

            speedInput = new TextBox
            {
                Location = new Point(10, 15),
                Size     = new Size(50, 20),
                Text     = "10",
                Font     = new Font("Arial", 10)
            };
            speedInput.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };

            statusLabel = new Label
            {
                Location  = new Point(70, 12),
                Size      = new Size(20, 20),
                Text      = "■",
                Font      = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label helpLabel = new Label
            {
                Location  = new Point(95, 15),
                Size      = new Size(35, 20),
                Text      = "F8",
                Font      = new Font("Arial", 8),
                ForeColor = Color.Gray
            };

            keyInput = new TextBox
            {
                Location = new Point(10, 42),
                Size     = new Size(115, 20),
                Text     = "Alt",
                Font     = new Font("Arial", 9)
            };

            this.Controls.Add(speedInput);
            this.Controls.Add(statusLabel);
            this.Controls.Add(helpLabel);
            this.Controls.Add(keyInput);

            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp   += Form_MouseUp;
        }

        private void RegisterHotkeys()
        {
            this.Load += (s, e) =>
            {
                RegisterHotKey(this.Handle, HOTKEY_ID,  0, (uint)Keys.F8);
                RegisterHotKey(this.Handle, HOTKEY_ESC, 0, (uint)Keys.Escape);
            };
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                if (m.WParam.ToInt32() == HOTKEY_ID)  ToggleClicking();
                if (m.WParam.ToInt32() == HOTKEY_ESC) Application.Exit();
            }
            base.WndProc(ref m);
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDragging = true; dragStartPoint = e.Location; }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = this.Location;
                p.X += e.X - dragStartPoint.X;
                p.Y += e.Y - dragStartPoint.Y;
                this.Location = p;
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e) { isDragging = false; }

        private void ToggleClicking()
        {
            if (isClicking) StopClicking();
            else            StartClicking();
        }

        private void StartClicking()
        {
            if (!int.TryParse(speedInput.Text, out clicksPerSecond)) return;
            if (clicksPerSecond < 1 || clicksPerSecond > 500)        return;

            byte vk = ParseKey(keyInput.Text.Trim());
            activeVk = vk != 0 ? vk : (byte)0xA4;

            isClicking            = true;
            statusLabel.ForeColor = Color.Lime;
            speedInput.Enabled    = false;
            keyInput.Enabled      = false;

            clickThread = new Thread(ClickLoop) { IsBackground = true };
            clickThread.Start();
        }

        private void StopClicking()
        {
            isClicking            = false;
            statusLabel.ForeColor = Color.Red;
            speedInput.Enabled    = true;
            keyInput.Enabled      = true;
        }

        private void ClickLoop()
        {
            int interval = 1000 / clicksPerSecond;
            while (isClicking)
            {
                try { PerformPress(); Thread.Sleep(interval); }
                catch { break; }
            }
        }

        private byte ParseKey(string name)
        {
            if (name.Equals("Alt",   StringComparison.OrdinalIgnoreCase)) return 0xA4;
            if (name.Equals("Ctrl",  StringComparison.OrdinalIgnoreCase)) return 0xA2;
            if (name.Equals("Shift", StringComparison.OrdinalIgnoreCase)) return 0xA0;
            Keys k;
            if (Enum.TryParse(name, true, out k) && (int)k <= 0xFF) return (byte)k;
            return 0;
        }

        private void PerformPress()
        {
            keybd_event(activeVk, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(20);
            keybd_event(activeVk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isClicking = false;
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            UnregisterHotKey(this.Handle, HOTKEY_ESC);
            base.OnFormClosing(e);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BongoBeatForm());
        }
    }
}
