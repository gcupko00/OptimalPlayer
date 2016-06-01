using OptimalPlayer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Windows;
using System.Xml;

namespace OptimalPlayer.ViewModel
{
    /// <summary>
    /// This class contains methods which are used to read various formats of playlists
    /// </summary>
    static class PlaylistReader
    {
        private static readonly string xspfXmlns = "http://xspf.org/ns/0/";

        /// <summary>
        /// Acording to file extension, calls methods which parse that file or throws an exception
        /// </summary>
        /// <param name="playlistPath">File name of targeted playlist</param>
        /// <returns>List of audio files obtained from playlist</returns>
        public static List<AudioFile> GetFilesFromPlaylist(string playlistPath)
        {
            try
            {
                string fileExtension = Path.GetExtension(playlistPath);

                switch (fileExtension)
                {
                    case ".xspf":
                        return ParseXSPF(playlistPath);
                    case ".wpl":
                        return ParseWPL(playlistPath);
                    case ".m3u":
                        return ParseM3U(playlistPath);
                    default:
                        throw new IOException("File extension is not supported");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Used to obtain file names from XSPF playlist name and create audio files from them
        /// </summary>
        /// <param name="playlistPath">File name of targeted playlist</param>
        /// <returns>List of audio files obtained from playlist</returns>
        private static List<AudioFile> ParseXSPF(string playlistPath)
        {
            XmlDocument document = new XmlDocument();

            try
            {
                List<AudioFile> audioFiles = new List<AudioFile>();
                
                document.Load(playlistPath);

                if (document.XSPFValid())
                {
                    XmlNodeList locations = document.GetElementsByTagName("location");

                    for (int i = 0; i < locations.Count; i++)
                    {
                        string filePath = HttpUtility.UrlDecode(locations[i].InnerText).Substring(8);

                        TagLib.File file = TagLib.File.Create(filePath);

                        audioFiles.Add(AudioFileFromPath(filePath, i));
                    }
                }
                else
                {
                    throw new FileFormatException("Playlist file is not valid");
                }

                return audioFiles;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Used to obtain file names from WPL playlist name and create audio files from them
        /// </summary>
        /// <param name="playlistPath">File name of targeted playlist</param>
        /// <returns>List of audio files obtained from playlist</returns>
        private static List<AudioFile> ParseWPL(string playlistPath)
        {
            XmlDocument document = new XmlDocument();

            try
            {
                List<AudioFile> audioFiles = new List<AudioFile>();

                document.Load(playlistPath);

                if (document.WPLValid())
                {
                    XmlNodeList medias = document.GetElementsByTagName("media");

                    for (int i = 0; i < medias.Count; i++)
                    {
                        string filePath = HttpUtility.UrlDecode(medias[i].Attributes["src"].InnerText);

                        TagLib.File file = TagLib.File.Create(filePath);

                        audioFiles.Add(AudioFileFromPath(filePath, i));
                    }
                }
                else
                {
                    throw new FileFormatException("Playlist file is not valid");
                }

                return audioFiles;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Used to obtain file names from M3U playlist name and create audio files from them
        /// </summary>
        /// <param name="playlistPath">File name of targeted playlist</param>
        /// <returns>List of audio files obtained from playlist</returns>
        private static List<AudioFile> ParseM3U(string playlistPath)
        {
            StreamReader document = new StreamReader(playlistPath);
            string line;

            try
            {
                List<AudioFile> audioFiles = new List<AudioFile>();

                if (document.M3UValid())
                {
                    int i = 0;
                    while ((line = document.ReadLine()) != null)
                    {
                        if (line.Substring(0, 8) != "#EXTINF:")
                        {
                            // There can be some html encoded symbols, such as &amp;
                            // so we must deal with it
                            string filePath = HttpUtility.HtmlDecode(line);

                            TagLib.File file = TagLib.File.Create(filePath);

                            audioFiles.Add(AudioFileFromPath(filePath, i++));
                        }
                    }
                }
                else
                {
                    throw new FileFormatException("Playlist file is not valid");
                }

                return audioFiles;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private static bool XSPFValid(this XmlDocument document)
        {

            XmlNodeList playlistNodes = document.GetElementsByTagName("playlist");

            if (playlistNodes.Count == 1 && playlistNodes[0].Attributes["xmlns"].InnerText == xspfXmlns)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool WPLValid(this XmlDocument document)
        {
            if (document.FirstChild.Name == "wpl")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool M3UValid(this StreamReader document)
        {
            if (document.ReadLine() == "#EXTM3U")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates new AudioFile object from file path
        /// </summary>
        /// <param name="filePath">Full path to file</param>
        /// <param name="fileID">Identification number whic will be assigned to AudioFile</param>
        /// <returns></returns>
        private static AudioFile AudioFileFromPath(string filePath, int fileID)
        {
            TagLib.File file = TagLib.File.Create(filePath);

            return new AudioFile
            {
                FileID = fileID,
                Path = filePath,
                Name = Path.GetFileNameWithoutExtension(filePath),
                Duration = (new NAudio.Wave.AudioFileReader(filePath)).TotalTime.ToString(@"mm\:ss"),
                SongName = file.Tag.Title,
                Artist = file.Tag.JoinedPerformers,
                Album = file.Tag.Album,
                Year = (int?)file.Tag.Year
            };
        }
    }
}
