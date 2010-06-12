using System;
using System.ComponentModel;
using System.Windows;
using CC.Utilities;
using Microsoft.Win32;

namespace CC.Hearts
{
    public sealed class Settings : DependencyObject, INotifyPropertyChanged
    {
        #region Constructor
        static Settings()
        {
            Instance = new Settings();
        }

        /// <summary>
        /// Creates a new <see cref="Settings"/>. Consider using <see cref="Settings.Instance"/> instead.
        /// </summary>
        public Settings()
        {
            Reset();
            Load();
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty AnimationSpeedProperty = DependencyProperty.Register("AnimationSpeed", typeof(HeartAnimationSpeed), typeof(Settings), new PropertyMetadata(DefaultAnimationSpeed));
        public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register("FramesPerSecond", typeof(int), typeof(Settings), new PropertyMetadata(DefaultFramesPerSecond, OnFramesPerSecondChanged));
        public static readonly DependencyProperty HeartCountProperty = DependencyProperty.Register("HeartCount", typeof(int), typeof(Settings), new PropertyMetadata(0));
        public static readonly DependencyProperty MaximumHeartsProperty = DependencyProperty.Register("MaximumHearts", typeof(int), typeof(Settings), new PropertyMetadata(DefaultMaximumHearts, OnMaximumHeartsChanged));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(int), typeof(Settings), new PropertyMetadata(DefaultScale, OnScaleChanged));
        public static readonly DependencyProperty ShowHelpProperty = DependencyProperty.Register("ShowHelp", typeof(bool), typeof(Settings), new PropertyMetadata(DefaultShowHelp));
        public static readonly DependencyProperty ShowStatusProperty = DependencyProperty.Register("ShowStatus", typeof(bool), typeof(Settings), new PropertyMetadata(DefaultShowStatus));
        #endregion

        #region Private Constants
        // ReSharper disable InconsistentNaming
        #region Registry Keys
        private const string ANIMATION_SPEED = "AnimationSpeed";
        private const string FRAMES_PER_SECOND = "FramesPerSecond";
        private const string MAXIMUM_HEARTS = "MaximumHearts";
        private const string SCALE = "Scale";
        private const string SHOW_HELP = "ShowHelp";
        private const string SHOW_STATUS = "ShowStatus";

        private const string REGISTRY_KEY = @"Software\CC.Hearts Screen Saver";
        #endregion
        #endregion

        #region Public Constants
        #region Default Values
        public const HeartAnimationSpeed DefaultAnimationSpeed = HeartAnimationSpeed.Normal;
        public const int DefaultFramesPerSecond = 30;
        public const int DefaultMaximumHearts = 30;
        public const int DefaultScale = 7;
        public const bool DefaultShowHelp = true;
        public const bool DefaultShowStatus = false;
        #endregion

        #region Maximum Values
        public const int MaximumFramesPerSecond = 60;
        public const int MaximumMaximumHearts = 50;
        public const int MaximumScale = 9;
        #endregion

        #region Minimum Values
        public const int MinimumFramesPerSecond = 30;
        public const int MinimumMaximumHearts = 10;
        public const int MinimumScale = 5;
        #endregion
        // ReSharper restore InconsistentNaming
        #endregion

        #region Public Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Fields
        private bool _IsLoaded;
        #endregion

        #region Public Fields
        public static Settings Instance;
        #endregion

        #region Public Properties
        public HeartAnimationSpeed AnimationSpeed
        {
            get { return (HeartAnimationSpeed)GetValue(AnimationSpeedProperty); }
            set { SetValue(AnimationSpeedProperty, value); }
        }

        public int FramesPerSecond
        {
            get { return (int)GetValue(FramesPerSecondProperty); }
            set { SetValue(FramesPerSecondProperty, value); }
        }

        public int HeartCount
        {
            get { return (int)GetValue(HeartCountProperty); }
            set { SetValue(HeartCountProperty, value); }
        }

        public static bool IsDebug { get; set; }

        public static bool IsPreview
        {
            get { return PreviewHandle != IntPtr.Zero; }
        }

        public int MaximumHearts
        {
            get { return (int) GetValue(MaximumHeartsProperty); }
            set { SetValue(MaximumHeartsProperty, value); }
        }

        public static IntPtr PreviewHandle { get; set; }

        public int Scale
        {
            get { return (int)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public bool ShowHelp
        {
            get { return (bool)GetValue(ShowHelpProperty); }
            set { SetValue(ShowHelpProperty, value); }
        }

        public bool ShowStatus
        {
            get { return (bool) GetValue(ShowStatusProperty); }
            set { SetValue(ShowStatusProperty, value); }
        }

        public static int Tier { get; set; }
        #endregion

        #region Private Methods
        private static RegistryKey OpenRegistryKey()
        {
            return (Registry.LocalMachine.CreateSubKey(REGISTRY_KEY));
        }

        private static void OnFramesPerSecondChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int value = (int)e.NewValue;
            if (value > MaximumFramesPerSecond)
            {
                d.SetValue(e.Property, MaximumFramesPerSecond);
            }
            else if (value < MinimumFramesPerSecond)
            {
                d.SetValue(e.Property, MinimumFramesPerSecond);
            }
        }

        private static void OnMaximumHeartsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int value = (int)e.NewValue;
            if (value > MaximumMaximumHearts)
            {
                d.SetValue(e.Property, MaximumMaximumHearts);
            }
            else if (value < MinimumMaximumHearts)
            {
                d.SetValue(e.Property, MinimumMaximumHearts);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler propertyChangedEventHandler = PropertyChanged;
            if (propertyChangedEventHandler != null)
            {
                propertyChangedEventHandler(this, new PropertyChangedEventArgs(e.Property.Name));
            }

            base.OnPropertyChanged(e);
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int value = (int)e.NewValue;

            if (value > MaximumScale)
            {
                d.SetValue(e.Property, MaximumScale);
            }
            else if (value < MinimumScale)
            {
                d.SetValue(e.Property, MinimumScale);
            }
        }

        #endregion

        #region Public Methods
        public void DecreaseHeartCount()
        {
            HeartCount--;
            if (HeartCount < 0)
            {
                HeartCount = 0;
            }
        }

        public void IncreaseHeartCount()
        {
            HeartCount++;
        }

        public bool Load()
        {
            _IsLoaded = false;

            try
            {
                using (RegistryKey registryKey = OpenRegistryKey())
                {
                    AnimationSpeed = (HeartAnimationSpeed)registryKey.GetValue(ANIMATION_SPEED, DefaultAnimationSpeed);
                    FramesPerSecond = (int)registryKey.GetValue(FRAMES_PER_SECOND, DefaultFramesPerSecond);
                    MaximumHearts = (int)registryKey.GetValue(MAXIMUM_HEARTS, DefaultMaximumHearts);
                    Scale = (int)registryKey.GetValue(SCALE, DefaultScale);
                    ShowHelp = bool.Parse(registryKey.GetValue(SHOW_HELP, DefaultShowHelp).ToString());
                    ShowStatus = bool.Parse(registryKey.GetValue(SHOW_STATUS, DefaultShowStatus).ToString());
                }

                _IsLoaded = true;
            }
            catch (Exception e)
            {
                Logging.LogException(e);
                _IsLoaded = false;
            }

            return _IsLoaded;
        }

        public void Reset()
        {
            AnimationSpeed = DefaultAnimationSpeed;
            FramesPerSecond = DefaultFramesPerSecond;
            MaximumHearts = DefaultMaximumHearts;
            Scale = DefaultScale;
            ShowHelp = DefaultShowHelp;
            ShowStatus = DefaultShowStatus;
        }

        public bool Save()
        {
            bool returnValue;

            try
            {
                using (RegistryKey registryKey = OpenRegistryKey())
                {
                    registryKey.SetValue(ANIMATION_SPEED, (int)AnimationSpeed, RegistryValueKind.DWord);
                    registryKey.SetValue(FRAMES_PER_SECOND, FramesPerSecond, RegistryValueKind.DWord);
                    registryKey.SetValue(MAXIMUM_HEARTS, MaximumHearts, RegistryValueKind.DWord);
                    registryKey.SetValue(SCALE, Scale, RegistryValueKind.DWord);
                    registryKey.SetValue(SHOW_HELP, ShowHelp.ToString(), RegistryValueKind.String);
                    registryKey.SetValue(SHOW_STATUS, ShowStatus.ToString(), RegistryValueKind.String);

                    registryKey.Close();
                }

                returnValue = true;
            }
            catch (Exception e)
            {
                Logging.LogException(e);
                returnValue = false;
            }

            return returnValue;
        }
        #endregion
    }
}
