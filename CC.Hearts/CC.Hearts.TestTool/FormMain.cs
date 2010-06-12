using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CC.Utilities;
using Timer=System.Timers.Timer;

namespace CC.Hearts.TestTool
{
    public partial class FormMain : Form
    {
        #region Constructor
        public FormMain()
        {
            InitializeComponent();

            _Timer = new Timer(250);
            _Timer.Elapsed += _Timer_Elapsed;
        }
        #endregion

        #region Private Fields
        private bool _Closing;
        private Process _ScreenSaverProcess;
        private readonly Timer _Timer;
        #endregion

        #region Private Event Handlers
        // ReSharper disable InconsistentNaming
        private void _ButtonKillProcess_Click(object sender, EventArgs e)
        {
            KillProcess();
            StartMiniPreview();
        }

        private void _ButtonPreview_Click(object sender, EventArgs e)
        {
            StartProcess(true);
        }

        private void _ButtonSettings_Click(object sender, EventArgs e)
        {
            StartProcess("/c:blahblah", true); //To test the /c:###### parameter I'm seeing on Windows Vista (maybe others). Not sure what the number is
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _Closing = true;

            KillProcess();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Show();
            StartMiniPreview();
        }

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SetProcessInfo();
        }
        // ReSharper restore InconsistentNaming
        #endregion

        #region Private Methods
        private void ClearImage()
        {
            if (_PanelMiniPreview.BackgroundImage != null)
            {
                _PanelMiniPreview.BackgroundImage.Dispose();
                _PanelMiniPreview.BackgroundImage = null;
            }
        }

        private void KillProcess()
        {
            if (_ScreenSaverProcess != null)
            {
                if (!_ScreenSaverProcess.HasExited)
                {
                    _ScreenSaverProcess.Kill();
                }

                _ScreenSaverProcess.Dispose();
                _ScreenSaverProcess = null;
            }

            SetImage();
            SetProcessInfo();

            _Timer.Stop();
        }

        private void SetImage()
        {
            ClearImage();

            _PanelMiniPreview.BackgroundImage = Utilities.Utilities.CaptureScreen();
        }

        private void SetProcessInfo()
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new MethodInvoker(SetProcessInfo));
                }
                catch (ObjectDisposedException exception)
                {
                    Logging.LogException(exception);
                }
            }
            else
            {
                if (_ScreenSaverProcess != null)
                {
                    _ScreenSaverProcess.Refresh();
                    _ButtonKillProcess.Text = _ScreenSaverProcess.Id + " (" + _ScreenSaverProcess.HandleCount + "/" + _ScreenSaverProcess.Threads.Count + ")";
                }
                else
                {
                    _ButtonKillProcess.Text = string.Empty;
                    _Timer.Enabled = false;
                }
            }
        }

        private void StartMiniPreview()
        {
            StartProcess("/p " + _PanelMiniPreview.Handle, false);
            ClearImage();
        }

        private void StartProcess(bool waitForExit)
        {
            StartProcess(null, waitForExit);
        }

        private void StartProcess(string arguments, bool waitForExit)
        {
            KillProcess();

            if (_Closing)
            {
                return;
            }

            _ScreenSaverProcess = new Process();
            var startInfo = new ProcessStartInfo(Path.Combine(Application.StartupPath, @"..\..\..\CC.Hearts\bin\Debug\CCHearts.exe"))
                                {
                                    WorkingDirectory = Path.Combine(Application.StartupPath, @"..\..\..\CC.Hearts\bin\Debug\")
                                };

            if (_CheckBoxDebug.Checked)
            {
                startInfo.Arguments = "/d true" + (!string.IsNullOrEmpty(arguments) ? " " + arguments : string.Empty);
            }
            else if (!string.IsNullOrEmpty(arguments))
            {
                startInfo.Arguments = arguments;
            }

            _ScreenSaverProcess.StartInfo = startInfo;
            _ScreenSaverProcess.Start();
            _Timer.Start();

            if (waitForExit)
            {
                UseWaitCursor = true;
                
                _ButtonPreview.Enabled = false;
                _ButtonSettings.Enabled = false;
                _CheckBoxDebug.Enabled = false;

                while (!_Closing && !_ScreenSaverProcess.WaitForExit(250))
                {
                    Application.DoEvents();
                }

                if (!_Closing)
                {
                    StartMiniPreview();
                }
            }

            UseWaitCursor = false;

            _ButtonPreview.Enabled = true;
            _ButtonSettings.Enabled = true;
            _CheckBoxDebug.Enabled = true;
        }
        #endregion
    }
}
