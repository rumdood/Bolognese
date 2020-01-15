using System;
using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Bolognese.Common.Media;
using System.Diagnostics;
using System.Windows.Threading;

namespace Bolognese.Desktop.Media
{
    public class PomodoroTrackManager : IMediaManager, IHandleWithTask<BuildPlaylistFromFolderRequested>, IHandle<MediaRequest>
    {
        readonly IEventAggregator _events;
        readonly ISongFactory _songFactory;
        Queue<Song> _pomodoroQueue;

        string IMediaManager.CurrentSongTitle => throw new NotImplementedException();
        Queue<Song> IMediaManager.CurrentSongQueue => throw new NotImplementedException();

        Task IMediaManager.BuildPlaylist(string audioFilePath)
        {
            throw new NotImplementedException();
        }

        void IMediaManager.OpenNextSong()
        {
            throw new NotImplementedException();
        }

        void IMediaManager.OpenPlaylist(Playlist playlist)
        {
            throw new NotImplementedException();
        }

        void IMediaManager.Pause()
        {
            throw new NotImplementedException();
        }

        void IMediaManager.PlayOpenSong()
        {
            throw new NotImplementedException();
        }

        void IMediaManager.PlayPlaylist(Playlist playlist)
        {
            _pomodoroQueue = new Queue<Song>(playlist.Songs);
        }

        void IMediaManager.Stop()
        {
            throw new NotImplementedException();
        }

        Task IHandleWithTask<BuildPlaylistFromFolderRequested>.Handle(BuildPlaylistFromFolderRequested message)
        {
            throw new NotImplementedException();
        }

        void IHandle<MediaRequest>.Handle(MediaRequest message)
        {
            throw new NotImplementedException();
        }
    }
}
