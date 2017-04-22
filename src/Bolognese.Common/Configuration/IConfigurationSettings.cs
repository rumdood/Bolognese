namespace Bolognese.Common.Configuration
{
    public interface IConfigurationSettings
    {
        string AudioFilePath { get; set; }
        bool Shuffle { get; set; }
        int LongBreakCount { get; set; }
        int LongBreakDuration { get; set; }
        int ShortBreakDuration { get; set; }
        int PomodoroDuration { get; set; }
        void Save();
    }
}