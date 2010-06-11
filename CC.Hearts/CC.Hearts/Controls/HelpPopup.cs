using System;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace CC.Hearts.Controls
{
    public class HelpPopup : Popup
    {
        #region Constructor
        public HelpPopup()
        {
            _Timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher);
            _Timer.Tick += _Timer_Tick;

            AllowsTransparency = true;
            Child = _HelpCanvas;
            Placement = PlacementMode.Center;
            PopupAnimation = PopupAnimation.Fade;
            StaysOpen = true;
        }
        #endregion

        #region Private Fields
        private readonly HelpCanvas _HelpCanvas = new HelpCanvas();
        private readonly DispatcherTimer _Timer;
        #endregion

        #region Private Event Handlers
        private void _Timer_Tick(object sender, EventArgs e)
        {
            _Timer.IsEnabled = false;

            IsOpen = false;
        }
        #endregion

        #region Public Methods
        public void Hide()
        {
            _Timer.IsEnabled = false;

            IsOpen = false;
        }
        public void Show()
        {
            Show(0);    
        }

        public void Show(int closeSeconds)
        {
            _Timer.IsEnabled = false;

            IsOpen = true;

            if (closeSeconds > 0)
            {
                _Timer.Interval = TimeSpan.FromSeconds(closeSeconds);
                _Timer.IsEnabled = true;
            }
        }
        #endregion
    }
}
