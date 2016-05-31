using Microsoft.Win32;
using OptimalPlayer.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace OptimalPlayer.ViewModel
{
    /// <summary>
    /// This class contains methods which are used to save list of AudioFiles as playlist file
    /// </summary>
    static class PlaylistExporter
    {
        /// <summary>
        /// Opens new SaveFileDialog and creates file that is used to store data
        /// </summary>
        /// <param name="audioFilesList">List of audio files to be exported to playlist file</param>
        public static void SavePlaylist(List<AudioFile> audioFilesList)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "My Playlist";
            saveFileDialog.Filter = "M3U Playlist|*.m3u";
            saveFileDialog.AddExtension = true;
            bool? result = saveFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                saveFileDialog.FileName.CreateM3UFile(audioFilesList);
            }
        }

        /// <summary>
        /// Used to write AudioFiles paths and info to m3u playlist file
        /// </summary>
        /// <param name="fileName">M3U playlist file's full name</param>
        /// <param name="audioFilesList">List of audio files to be exported to mm3u file</param>
        private static void CreateM3UFile(this string fileName, List<AudioFile> audioFilesList)
        {
            if (Path.GetExtension(fileName) != ".m3u")
            {
                return;
            }

            using (StreamWriter m3uFile = new StreamWriter(fileName))
            {
                m3uFile.WriteLine("#EXTM3U");

                foreach (AudioFile audioFile in audioFilesList)
                {
                    string trackInfo = "#EXTINF:" + (int)(TimeSpan.Parse(audioFile.Duration).TotalSeconds / 60) + "," + audioFile.Artist + " - " + audioFile.SongName;
                    m3uFile.WriteLine(trackInfo);

                    m3uFile.WriteLine(audioFile.Path);
                }
            }
        }
    }
}
