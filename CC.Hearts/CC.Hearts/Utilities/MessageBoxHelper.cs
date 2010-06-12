using System;
using System.Collections.Generic;
using System.Windows;

namespace CC.Hearts.Utilities
{
    public static class MessageBoxHelper
    {
        #region Private Constants
        private const MessageBoxButton DefaultButton = MessageBoxButton.OK;
        private const string DefaultCaption = "";
        private const MessageBoxImage DefaultImage = MessageBoxImage.None;
        #endregion

        #region Internal Methods
        internal static void InternalShow(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBox.Show(messageBoxText, System.Windows.Forms.Application.ProductName + caption, button, icon);
        }
        #endregion

        #region Public Methods
        public static void Show(string messageBoxText)
        {
            InternalShow(messageBoxText, DefaultCaption, DefaultButton, DefaultImage);
        }

        public static void Show(string messageBoxText, MessageBoxImage icon)
        {
            InternalShow(messageBoxText, DefaultCaption, DefaultButton, icon);
        }
        #endregion
    }
}
