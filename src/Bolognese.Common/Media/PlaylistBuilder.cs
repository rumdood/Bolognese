using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolognese.Common.Media
{
    public class PlaylistBuilder : IPlaylistBuilder
    {
        Playlist IPlaylistBuilder.GeneratePlaylist(IEnumerable<Song> songs, 
                                                   int maxTime, 
                                                   int fudgeFactor)
        {
            double lowerBound = maxTime - fudgeFactor;
            double upperBound = maxTime + fudgeFactor;
            double maxRemainder = upperBound;

            Playlist output = new Playlist();
            double playlistLength = 0;

            foreach (var song in songs)
            {
                if (song.Duration.TotalMinutes >= maxRemainder)
                {
                    continue;
                }

                output.Songs.Add(song);
                playlistLength += song.Duration.TotalMinutes;
                maxRemainder = upperBound - playlistLength;

                if (maxRemainder <= (upperBound - lowerBound))
                {
                    break;
                }
            }

            return output;
        }
    }
}
