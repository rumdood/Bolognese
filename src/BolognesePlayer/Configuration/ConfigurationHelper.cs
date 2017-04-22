using System.Configuration;

namespace Bolognese.Desktop.Configuration
{
    public static class ConfigurationHelper
    {
        public static BologneseConfigurationSettings GetConfiguration()
        {
            System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            var bologneseConfiguration = configuration.GetSection(BologneseConfigurationSettings.SectionName) as BologneseConfigurationSettings;

            if (bologneseConfiguration == null)
            {
                bologneseConfiguration = new BologneseConfigurationSettings();
                bologneseConfiguration.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
                configuration.Sections.Add(BologneseConfigurationSettings.SectionName, bologneseConfiguration);
                configuration.Save(ConfigurationSaveMode.Full, true);
            }

            return bologneseConfiguration;
        }
    }
}
