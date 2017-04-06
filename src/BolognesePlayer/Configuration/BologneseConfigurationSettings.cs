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

        private const int DefaultShortDuration = 5;
        private const int DefaultLongDuration = 30;
        private const int DefaultPomodorosBeforeLongBreak = 4;

        [ConfigurationProperty("AudioFilePath", IsRequired = true)]
        public string AudioFilePath
        {
            get { return this["AudioFilePath"].ToString(); }
            set { this["AudioFilePath"] = value; }
        }

        [ConfigurationProperty("Shuffle", IsRequired = true)]
        public bool Shuffle
        {
            get { return (bool)this["Shuffle"]; }
            set { this["Shuffle"] = value; }
        }

        /// <summary>
        /// The number of cycles before a longer break is taken between songs
        /// </summary>
        [ConfigurationProperty("LongBreakCount", IsRequired = true)]
        public int LongBreakCount
        {
            get
            {
                int longBreak = (int)this["LongBreakCount"];
                if (longBreak == 0)
                {
                    longBreak = DefaultPomodorosBeforeLongBreak;
                }

                return longBreak;
            }
            set { this["LongBreakCount"] = value; }
        }

        /// <summary>
        /// The number of minutes to wait between songs for short breaks
        /// </summary>
        [ConfigurationProperty("ShortBreakDuration", IsRequired = true)]
        public int ShortBreakDuration
        {
            get
            {
                int shortDuration = (int)this["ShortBreakDuration"];
                if (shortDuration == 0)
                {
                    shortDuration = DefaultShortDuration;
                }

                return shortDuration;
            }
            set { this["ShortBreakDuration"] = value; }
        }

        /// <summary>
        /// The number of minutes to wait between songs for long breaks
        /// </summary>
        [ConfigurationProperty("LongBreakDuration", IsRequired = true)]
        public int LongBreakDuration
        {
            get
            {
                int longDuration = (int)this["LongBreakDuration"];
                if (longDuration == 0)
                {
                    longDuration = DefaultLongDuration;
                }

                return longDuration;
            }
            set { this["LongBreakDuration"] = value; }
        }

        public void Save()
        {
            CurrentConfiguration.Save(ConfigurationSaveMode.Full, true);
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
