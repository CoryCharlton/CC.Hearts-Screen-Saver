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
        public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register("FramesPerSecond", typeof(int), typeof(Settings), new PropertyMetadata(DEFAULT_FRAMES_PER_SECOND, OnFramesPerSecondChanged));
        public static readonly DependencyProperty HeartCountProperty = DependencyProperty.Register("HeartCount", typeof(int), typeof(Settings), new PropertyMetadata(0));
        public static readonly DependencyProperty MaximumHeartsProperty = DependencyProperty.Register("MaximumHearts", typeof(int), typeof(Settings), new PropertyMetadata(DEFAULT_MAXIMUM_HEARTS, OnMaximumHeartsChanged));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(int), typeof(Settings), new PropertyMetadata(DEFAULT_SCALE, OnScaleChanged));
        public static readonly DependencyProperty ShowStatusProperty = DependencyProperty.Register("ShowStatus", typeof(bool), typeof(Settings), new PropertyMetadata(DEFAULT_SHOW_STATUS));
        #endregion

        #region Private Constants
        // ReSharper disable InconsistentNaming
        #region Default Values
        private const int DEFAULT_FRAMES_PER_SECOND = 45;
        private const int DEFAULT_MAXIMUM_HEARTS = 30;
        private const int DEFAULT_SCALE = 10;
        private const bool DEFAULT_SHOW_STATUS = false;
        #endregion

        #region Registry Keys
        private const string FRAMES_PER_SECOND = "FramesPerSecond";
        private const string MAXIMUM_HEARTS = "MaximumHearts";
        private const string SCALE = "Scale";
        private const string SHOW_STATUS = "ShowStatus";

        private const string REGISTRY_KEY = @"Software\CC.Hearts Screensaver";
        #endregion
        #endregion

        #region Public Constants
        #region Maximum Values
        public const int MAXIMUM_FRAMES_PER_SECOND = 60;
        public const int MAXIMUM_MAXIMUM_HEARTS = 50;
        public const int MAXIMUM_SCALE = 15;
        #endregion

        #region Minimum Values
        public const int MINIMUM_FRAMES_PER_SECOND = 30;
        public const int MINIMUM_MAXIMUM_HEARTS = 10;
        public const int MINIMUM_SCALE = 5;
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
            if (value > MAXIMUM_FRAMES_PER_SECOND)
            {
                d.SetValue(e.Property, MAXIMUM_FRAMES_PER_SECOND);
            }
            else if (value < MINIMUM_FRAMES_PER_SECOND)
            {
                d.SetValue(e.Property, MINIMUM_FRAMES_PER_SECOND);
            }
        }

        private static void OnMaximumHeartsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int value = (int)e.NewValue;
            if (value > MAXIMUM_MAXIMUM_HEARTS)
            {
                d.SetValue(e.Property, MAXIMUM_MAXIMUM_HEARTS);
            }
            else if (value < MINIMUM_MAXIMUM_HEARTS)
            {
                d.SetValue(e.Property, MINIMUM_MAXIMUM_HEARTS);
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

            if (value > MAXIMUM_SCALE)
            {
                d.SetValue(e.Property, MAXIMUM_SCALE);
            }
            else if (value < MINIMUM_SCALE)
            {
                d.SetValue(e.Property, MINIMUM_SCALE);
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
                    FramesPerSecond = (int)registryKey.GetValue(FRAMES_PER_SECOND, DEFAULT_FRAMES_PER_SECOND);
                    MaximumHearts = (int)registryKey.GetValue(MAXIMUM_HEARTS, DEFAULT_MAXIMUM_HEARTS);
                    Scale = (int)registryKey.GetValue(SCALE, DEFAULT_SCALE);
                    ShowStatus = bool.Parse(registryKey.GetValue(SHOW_STATUS, DEFAULT_SHOW_STATUS).ToString());
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
            FramesPerSecond = DEFAULT_FRAMES_PER_SECOND;
            MaximumHearts = DEFAULT_MAXIMUM_HEARTS;
            Scale = DEFAULT_SCALE;
            ShowStatus = DEFAULT_SHOW_STATUS;
        }

        public bool Save()
        {
            bool returnValue;

            try
            {
                using (RegistryKey registryKey = OpenRegistryKey())
                {
                    registryKey.SetValue(FRAMES_PER_SECOND, FramesPerSecond, RegistryValueKind.DWord);
                    registryKey.SetValue(MAXIMUM_HEARTS, MaximumHearts, RegistryValueKind.DWord);
                    registryKey.SetValue(SCALE, Scale, RegistryValueKind.DWord);
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
