using System;

namespace CC.Hearts
{
    public class SettingChangedEventArgs : EventArgs
    {
        public SettingChangedEventArgs(Setting setting, bool isLoadEvent)
        {
            IsLoadEvent = isLoadEvent;
            Setting = setting;
        }

        public bool IsLoadEvent { get; private set; }

        public Setting Setting { get; private set; }
    }
}
