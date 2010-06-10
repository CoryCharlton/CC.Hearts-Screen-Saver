﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CC.Hearts
{
    public partial class OptionsWindow
    {
        #region Constructor
        public OptionsWindow()
        {
            InitializeComponent();
            _GridMain.CacheMode = new BitmapCache(2);

            SettingsToWindow();
            SetToolTips();
            UpdateApplyButton();
        }
        #endregion

        #region Private Constants
        private const string DefaultsToolTip = "Reset values to their defaults";
        private const string FramesPerSecondToolTip = "The targetted number of frames per second to animate.\r\n\r\n * Decrease to improve performance; Increase to improve quality.";
        private const string MaximumHeartsToolTip = "The maximum number of hearts allowed on the screen.\r\n\r\n * Decrease to improve performance.";
        private const string ScaleToolTip = "The minimum size of the hearts based on a percentage of the screen size.";
        #endregion

        #region Private Event Handlers
        private void _Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void _ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void _ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void _ButtonDefaults_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Are you sure you want to reset all settings to their default values?", System.Windows.Forms.Application.ProductName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No))
            {
                Settings.Instance.Reset();
                SettingsToWindow();
                Save();
            }
        }

        private void _ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Save();
            Close();
        }

        private void _CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateApplyButton();
        }

        private void _SliderFramesPerSecond_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_TextBlockFramesPerSecondValue != null)
            {
                _TextBlockFramesPerSecondValue.Text = ((int)_SliderFramesPerSecond.Value).ToString();
            }

            UpdateApplyButton();
        }

        private void _SliderMaximumHearts_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_TextBlockMaximumHeartsValue != null)
            {
                _TextBlockMaximumHeartsValue.Text = ((int)_SliderMaximumHearts.Value).ToString();
            }

            UpdateApplyButton();
        }

        private void _SliderScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_TextBlockScaleValue != null)
            {
                _TextBlockScaleValue.Text = (int)_SliderScale.Value + "%";
            }

            UpdateApplyButton();
        }
        #endregion

        #region Private Methods
        private bool IsDirty()
        {
            bool returnValue = false;

            if (_CheckBoxShowHelp != null && _CheckBoxShowHelp.IsChecked.GetValueOrDefault() != Settings.Instance.ShowHelp)
            {
                returnValue = true;
            }

            if (_CheckBoxShowStatus != null && _CheckBoxShowStatus.IsChecked.GetValueOrDefault() != Settings.Instance.ShowStatus)
            {
                returnValue = true;
            }

            if (_SliderFramesPerSecond != null && (int)_SliderFramesPerSecond.Value != Settings.Instance.FramesPerSecond)
            {
                returnValue = true;
            }

            if (_SliderMaximumHearts != null && (int)_SliderMaximumHearts.Value != Settings.Instance.MaximumHearts)
            {
                returnValue = true;
            }

            if (_SliderScale != null && (int)_SliderScale.Value != Settings.Instance.Scale)
            {
                returnValue = true;
            }

            return returnValue;
        }

        private void Save()
        {
            WindowToSettings();
            Settings.Instance.Save();
            UpdateApplyButton();
        }

        private void SettingsToWindow()
        {
            _CheckBoxShowHelp.IsChecked = Settings.Instance.ShowHelp;
            _CheckBoxShowStatus.IsChecked = Settings.Instance.ShowStatus;
            _SliderFramesPerSecond.Value = Settings.Instance.FramesPerSecond;
            _SliderMaximumHearts.Value = Settings.Instance.MaximumHearts;
            _SliderScale.Value = Settings.Instance.Scale;
        }

        private void SetToolTips()
        {
            _ButtonDefaults.ToolTip = DefaultsToolTip;

            _SliderFramesPerSecond.ToolTip = FramesPerSecondToolTip;
            _TextBlockFramesPerSecondLabel.ToolTip = FramesPerSecondToolTip;
            _TextBlockFramesPerSecondValue.ToolTip = FramesPerSecondToolTip;

            _SliderMaximumHearts.ToolTip = MaximumHeartsToolTip;
            _TextBlockMaximumHeartsLabel.ToolTip = MaximumHeartsToolTip;
            _TextBlockMaximumHeartsValue.ToolTip = MaximumHeartsToolTip;

            _SliderScale.ToolTip = ScaleToolTip;
            _TextBlockScaleLabel.ToolTip = ScaleToolTip;
            _TextBlockScaleValue.ToolTip = ScaleToolTip;
        }

        private void UpdateApplyButton()
        {
            if (_ButtonApply != null)
            {
                _ButtonApply.IsEnabled = IsDirty();
            }
        }

        private void WindowToSettings()
        {
            Settings.Instance.FramesPerSecond = (int)_SliderFramesPerSecond.Value;
            Settings.Instance.MaximumHearts = (int)_SliderMaximumHearts.Value;
            Settings.Instance.Scale = (int)_SliderScale.Value;
            Settings.Instance.ShowHelp = _CheckBoxShowStatus.IsChecked.GetValueOrDefault();
            Settings.Instance.ShowStatus = _CheckBoxShowStatus.IsChecked.GetValueOrDefault();
        }
        #endregion
    }
}
