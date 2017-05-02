using Bolognese.Common.Pomodoro;
using Caliburn.Micro;
using System.Media;

namespace Bolognese.Desktop
{
    public class Alerter : IHandle<SegmentStatusChanged>
    {
        readonly IEventAggregator _events;
        readonly SoundPlayer _player;

        public Alerter(IEventAggregator events)
        {
            _events = events;
            _events.Subscribe(this);
            _player = new SoundPlayer(Properties.Resources.SegmentCompleteAlert);
        }

        void PlayAlert()
        {
            _player.Play();
        }

        void IHandle<SegmentStatusChanged>.Handle(SegmentStatusChanged message)
        {
            if (message.Status == SegmentStatus.Complete)
            {
                PlayAlert();
            }
        }
    }
}
