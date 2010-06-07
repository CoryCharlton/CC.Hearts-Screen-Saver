using System;
using System.Threading;
using CC.Utilities;
using Microsoft.Win32;

namespace CC.Hearts
{
    public static class Settings
    {
        #region Constructor
        static Settings()
        {
            Reset();
            Load();
        }
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

        #region Private Fields
        private static int _FramesPerSecond;
        private static int _HeartCount;
        private static bool _IsLoaded;
        private static int _MaximumHearts;
        private static int _Scale;
        private static bool _ShowStatus;
        #endregion

        #region Public Events
        public static EventHandler<SettingChangedEventArgs> SettingChanged;
        #endregion

        #region Public Properties
        public static int FramesPerSecond
        {
            get { return _FramesPerSecond; }
            set
            {
                int targetValue = value;

                if (value > MAXIMUM_FRAMES_PER_SECOND)
                {
                    targetValue = MAXIMUM_FRAMES_PER_SECOND;
                }
                else if (value < MINIMUM_FRAMES_PER_SECOND)
                {
                    targetValue = MINIMUM_FRAMES_PER_SECOND;
                }

                if (targetValue != _FramesPerSecond)
                {
                    _FramesPerSecond = targetValue;

                    OnSettingChanged(CreateSettingChangedEventArgs(Setting.FramesPerSecond));
                }
            }
        }

        public static int HeartCount
        {
            get { return _HeartCount; }
            set
            {
                Interlocked.Exchange(ref _HeartCount, value);
            }
        }

        public static bool IsDebug { get; set; }

        public static bool IsPreview
        {
            get { return PreviewHandle != IntPtr.Zero; }
        }

        public static int MaximumHearts
        {
            get { return _MaximumHearts; }
            set
            {
                int targetValue = value;

                if (value > MAXIMUM_MAXIMUM_HEARTS)
                {
                    targetValue = MAXIMUM_MAXIMUM_HEARTS;
                }
                else if (value < MINIMUM_MAXIMUM_HEARTS)
                {
                    targetValue = MINIMUM_MAXIMUM_HEARTS;
                }

                if (targetValue != _MaximumHearts)
                {
                    _MaximumHearts = targetValue;

                    OnSettingChanged(CreateSettingChangedEventArgs(Setting.MaximumHearts));
                }
            }
        }

        public static IntPtr PreviewHandle { get; set; }

        public static int Scale
        {
            get { return _Scale; }
            set
            {
                int targetValue = value;

                if (value > MAXIMUM_SCALE)
                {
                    targetValue = MAXIMUM_SCALE;
                }
                else if (value < MINIMUM_SCALE)
                {
                    targetValue = MINIMUM_SCALE;
                }

                if (targetValue != _Scale)
                {
                    _Scale = targetValue;

                    OnSettingChanged(CreateSettingChangedEventArgs(Setting.Scale));
                }
            }
        }

        public static bool ShowStatus
        {
            get { return _ShowStatus; }
            set
            {
                if (_ShowStatus != value)
                {
                    _ShowStatus = value;

                    OnSettingChanged(CreateSettingChangedEventArgs(Setting.ShowStatus));
                }
            }
        }

        public static int Tier { get; set; }
        #endregion

        #region Private Methods
        private static SettingChangedEventArgs CreateSettingChangedEventArgs(Setting setting)
        {
            return new SettingChangedEventArgs(setting, !_IsLoaded);            
        }

        private static RegistryKey OpenRegistryKey()
        {
            return (Registry.LocalMachine.CreateSubKey(REGISTRY_KEY));
        }

        private static void OnSettingChanged(SettingChangedEventArgs eventArgs)
        {
            if (SettingChanged != null)
            {
                SettingChanged(null, eventArgs);
            }
        }
        #endregion

        #region Public Methods
        public static void DecreaseHeartCount()
        {
            int currentCount = Interlocked.Decrement(ref _HeartCount);
            if (currentCount < 0)
            {
                Interlocked.Exchange(ref _HeartCount, 0);
            }
        }

        public static void IncreaseHeartCount()
        {
            Interlocked.Increment(ref _HeartCount);
        }

        public static bool Load()
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

        public static void Reset()
        {
            FramesPerSecond = DEFAULT_FRAMES_PER_SECOND;
            MaximumHearts = DEFAULT_MAXIMUM_HEARTS;
            Scale = DEFAULT_SCALE;
            ShowStatus = DEFAULT_SHOW_STATUS;
        }

        public static bool Save()
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
