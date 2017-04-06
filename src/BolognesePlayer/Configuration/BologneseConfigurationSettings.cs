using System;
using System.Configuration;

namespace Bolognese.Desktop
{
    public class BologneseConfigurationSettings : ConfigurationSection, IConfigurationSettings
    {
        private static BologneseConfigurationSettings _instance = null;

        public static string SectionName
        {
            get { return "UserSettings"; }
        }

        string _audioFilePath = string.Empty;
        bool _shuffle = false;
        int _shortBreakDuration = 10;
        int _longBreakDuration = 30;
        int _longBreakCount = 3;

        [ConfigurationProperty("AudioFilePath", IsRequired = true)]
        public string AudioFilePath
        {
            get { return _audioFilePath; }
            set { _audioFilePath = value; }
        }

        [ConfigurationProperty("Shuffle", IsRequired = true)]
        public bool Shuffle
        {
            get { return _shuffle; }
            set { _shuffle = value; }
        }

        /// <summary>
        /// The number of cycles before a longer break is taken between songs
        /// </summary>
        [ConfigurationProperty("LongBreakCount", IsRequired = true)]
        public int LongBreakCount
        {
            get { return _longBreakCount; }
            set
            {
                _longBreakCount = value;
            }
        }

        /// <summary>
        /// The number of minutes to wait between songs for short breaks
        /// </summary>
        [ConfigurationProperty("ShortBreakDuration", IsRequired = true)]
        public int ShortBreakDuration
        {
            get { return _shortBreakDuration; }
            set { _shortBreakDuration = value; }
        }

        /// <summary>
        /// The number of minutes to wait between songs for long breaks
        /// </summary>
        [ConfigurationProperty("LongBreakDuration", IsRequired = true)]
        public int LongBreakDuration
        {
            get { return _longBreakDuration; }
            set { _longBreakDuration = value; }
        }

        public void Save()
        {
            CurrentConfiguration.Save(ConfigurationSaveMode.Modified, true);
        }

        private static void InitializeConfigurationSettings()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            BologneseConfigurationSettings userSettings = config.GetSection(SectionName) as BologneseConfigurationSettings;

            if (userSettings == null)
            {
                userSettings = new BologneseConfigurationSettings();
                userSettings.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
                config.Sections.Add("UserSettings", userSettings);
                config.Save(ConfigurationSaveMode.Full, true);
            }

            _instance = userSettings;
        }

        public static BologneseConfigurationSettings GetConfigurationSettings()
        {
            if (_instance == null)
            {
                InitializeConfigurationSettings();
            }

            return _instance;
        }
    }
}
