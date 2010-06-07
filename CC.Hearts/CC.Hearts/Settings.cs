using System;
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
        private const int DEFAULT_MAXIMUM_HEARTS = 55;
        private const int DEFAULT_SCALE = 10;
        #endregion

        #region Registry Keys
        private const string FRAMES_PER_SECOND = "FramesPerSecond";
        private const string MAXIMUM_HEARTS = "MaximumHearts";
        private const string SCALE = "Scale";

        private const string REGISTRY_KEY = @"Software\CC.Hearts Screensaver";
        #endregion
        #endregion

        #region Public Constants
        #region Maximum Values
        public const int MAXIMUM_FRAMES_PER_SECOND = 60;
        public const int MAXIMUM_MAXIMUM_HEARTS = 100;
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
        private static int _MaximumHearts;
        private static int _Scale;
        #endregion

        #region Public Properties
        public static int FramesPerSecond
        {
            get { return _FramesPerSecond; }
            set
            {
                if (value > MAXIMUM_FRAMES_PER_SECOND)
                {
                    _FramesPerSecond = MAXIMUM_FRAMES_PER_SECOND;
                }
                else if (value < MINIMUM_FRAMES_PER_SECOND)
                {
                    _FramesPerSecond = MINIMUM_FRAMES_PER_SECOND;
                }
                else
                {
                    _FramesPerSecond = value;
                }
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
                if (value > MAXIMUM_MAXIMUM_HEARTS)
                {
                    _MaximumHearts = MAXIMUM_MAXIMUM_HEARTS;
                }
                else if (value < MINIMUM_MAXIMUM_HEARTS)
                {
                    _MaximumHearts = MINIMUM_MAXIMUM_HEARTS;
                }
                else
                {
                    _MaximumHearts = value;
                }
            }
        }

        public static IntPtr PreviewHandle { get; set; }

        public static int Scale
        {
            get { return _Scale; }
            set
            {
                if (value > MAXIMUM_SCALE)
                {
                    _Scale = MAXIMUM_SCALE;
                }
                else if (value < MINIMUM_SCALE)
                {
                    _Scale = MINIMUM_SCALE;
                }
                else
                {
                    _Scale = value;
                }
            }
        }
        #endregion

        #region Private Methods
        private static RegistryKey OpenRegistryKey()
        {
            return (Registry.LocalMachine.CreateSubKey(REGISTRY_KEY));
        }
        #endregion

        #region Public Methods
        public static bool Load()
        {
            bool returnValue;

            try
            {
                using (RegistryKey registryKey = OpenRegistryKey())
                {
                    FramesPerSecond = (int)registryKey.GetValue(FRAMES_PER_SECOND, DEFAULT_FRAMES_PER_SECOND);
                    MaximumHearts = (int)registryKey.GetValue(MAXIMUM_HEARTS, DEFAULT_MAXIMUM_HEARTS);
                    Scale = (int)registryKey.GetValue(SCALE, DEFAULT_SCALE);
                    
                    //RandomVerse = bool.Parse(registryKey.GetValue(RANDOM_VERSE, _DefaultRandomVerse).ToString());
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

        public static void Reset()
        {
            FramesPerSecond = DEFAULT_FRAMES_PER_SECOND;
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

                    //registryKey.SetValue(RANDOM_VERSE, RandomVerse.ToString(), RegistryValueKind.String);
                    
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
