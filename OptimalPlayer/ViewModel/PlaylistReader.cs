using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimalPlayer.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Windows;
using System.Xml.Linq;
using System.Web;

namespace OptimalPlayer.ViewModel
{
    static class PlaylistReader
    {
        private static readonly string xspfXmlns = "http://xspf.org/ns/0/";

        public static ObservableCollection<AudioFile> GetFilesFromPlaylist(string playlistPath)
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

        private static ObservableCollection<AudioFile> ParseXSPF(string playlistPath)
        {
            XmlDocument document = new XmlDocument();

            try
            {
                ObservableCollection<AudioFile> audioFiles = new ObservableCollection<AudioFile>();
                
                document.Load(playlistPath);

                if (XSPFValid(document))
                {
                    XmlNodeList locations = document.GetElementsByTagName("location");

                    for (int i = 0; i < locations.Count; i++)
                    {
                        string filePath = HttpUtility.UrlDecode(locations[i].InnerText).Substring(8);
                        TagLib.File file = TagLib.File.Create(filePath);

                        audioFiles.Add(new AudioFile
                        {
                            FileID = i,
                            Path = filePath,
                            Name = Path.GetFileNameWithoutExtension(filePath),
                            Duration = (new NAudio.Wave.AudioFileReader(filePath)).TotalTime.ToString(@"mm\:ss"),
                            SongName = file.Tag.Title,
                            Artist = file.Tag.JoinedPerformers,
                            Album = file.Tag.Album,
                            Year = (int?)file.Tag.Year
                        });
                    }
                }
                else
                {
                    throw new FileFormatException("Playlist file format is not valid");
                }

                return audioFiles;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private static ObservableCollection<AudioFile> ParseWPL(string playlistPath)
        {
            throw new NotImplementedException();
        }

        private static ObservableCollection<AudioFile> ParseM3U(string playlistPath)
        {
            throw new NotImplementedException();
        }

        private static bool XSPFValid(XmlDocument document)
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
    }
}
