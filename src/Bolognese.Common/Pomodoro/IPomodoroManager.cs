using System;
using System.Collections.Generic;
using System.Text;

namespace Bolognese.Common.Pomodoro
{
    public interface IPomodoroManager
    {
        void Initialize();
        void GetNextSegement();
        void StartSegment();
        void StopSegment();
        void ResumeSegment();
        void RestartSegment();
    }
}
