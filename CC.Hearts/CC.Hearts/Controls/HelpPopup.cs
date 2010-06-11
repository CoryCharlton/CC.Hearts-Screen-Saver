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
