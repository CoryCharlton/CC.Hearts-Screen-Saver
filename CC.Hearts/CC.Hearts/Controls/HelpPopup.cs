using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace CC.Hearts.Controls
{
    public class HelpPopup : Popup
    {
        #region Constructor
        public HelpPopup()
        {
            AllowsTransparency = true;
            Child = _HelpCanvas;
            Placement = PlacementMode.Center;
            PopupAnimation = PopupAnimation.Fade;
            StaysOpen = true;
        }
        #endregion

        #region Private Fields
        private readonly HelpCanvas _HelpCanvas = new HelpCanvas();
        #endregion
    }
}
