using System;
using System.Collections.Generic;
using System.Text;

namespace Bolognese.Common.Pomodoro
{
    public interface IPomodoroManager
    {
        void Initialize();
        void StartNextSegment();
        void StopSegment();
        void ResumeSegment();
        void RestartSegment();
    }
}
