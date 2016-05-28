using OptimalPlayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;

namespace OptimalPlayer.ViewModel
{
    public static class DatabaseInterface
    {
        private static SqlConnection connection;

        /// <summary>
        /// Creates connection to SQL database
        /// </summary>
        /// <param name="connectionString">The connection string that includes parameters needed to establish the initial connection</param>
        /// <returns>True if connection is established successfully, otherwise false.</returns>
        public static bool SetupConnection(string connectionString)
        {
            try
            {
                connection = new SqlConnection(connectionString);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // NOTE
        // also should probably return playlist id's 
        // ENDNOTE
        /// <summary>
        /// This method is used to load playlists from database
        /// </summary>
        /// <returns>List of playlist names</returns>
        public static ObservableCollection<string> GetPlaylists()
        {
            using (SqlCommand command = new SqlCommand("SELECT playlist_name FROM Playlists", connection))
            {
                DataTable table = new DataTable();

                try
                {
                    connection.Open();
                    table.Load(command.ExecuteReader());
                    connection.Close();

                    List<DataRow> rows = table.AsEnumerable().ToList();

                    // NOTE
                    // Should try to load from data table to list of playlist objects
                    // http://stackoverflow.com/questions/208532/how-do-you-convert-a-datatable-into-a-generic-list
                    // ENDNOTE
                    return new ObservableCollection<string>((from DataRow row in table.Rows
                                                             select row["playlist_name"].ToString()).ToList());
                }
                catch (Exception e)
                {
                    connection.Close();
                    MessageBox.Show(e.Message, "Error trying to fetch data");
                    return null;
                }
            }
        }

        /// <summary>
        /// Loads file paths from database for specified playlist
        /// </summary>
        /// <param name="playlistName">Currently selected playlist which is bound to files being loaded</param>
        /// <returns>List of audio files loaded from database</returns>
        public static ObservableCollection<AudioFile> GetPlaylistFiles(string playlistName)
        {
            int playlistID = GetPlaylistID(playlistName);
            string commandString = "SELECT * FROM Files WHERE playlist_id=@playlistid;";

            using (SqlCommand command = new SqlCommand(commandString, connection))
            {
                DataTable table = new DataTable();

                try
                {
                    command.Parameters.Add("@playlistid", SqlDbType.Int).Value = playlistID;

                    connection.Open();
                    table.Load(command.ExecuteReader());
                    connection.Close();

                    List<AudioFile> audioFiles = table.AsEnumerable().Select(row => new AudioFile
                    {
                        FileID = row.Field<int>(0),
                        Path = String.IsNullOrEmpty(row.Field<string>(1)) ? "" : row.Field<string>(1),
                        Name = String.IsNullOrEmpty(row.Field<string>(2)) ? "" : row.Field<string>(2),
                        Duration = TimeSpan.FromTicks(row.Field<long?>(3).GetValueOrDefault()).ToString(@"mm\:ss"),
                        SongName = String.IsNullOrEmpty(row.Field<string>(4)) ? "" : row.Field<string>(4),
                        Artist = String.IsNullOrEmpty(row.Field<string>(5)) ? "" : row.Field<string>(5),
                        Album = String.IsNullOrEmpty(row.Field<string>(6)) ? "" : row.Field<string>(6),
                        Year = row.Field<int?>(7).GetValueOrDefault() < 1900 ? (int?)null : row.Field<int?>(7).GetValueOrDefault()
                    }).ToList();

                    return new ObservableCollection<AudioFile>(audioFiles);
                }
                catch (Exception e)
                {
                    connection.Close();
                    MessageBox.Show(e.Message, "Error trying to load data");
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds new playlist to database
        /// </summary>
        /// <param name="newPlaylistName">Name of the playlist being added</param>
        public static void AddPlaylist(string newPlaylistName)
        {
            string commandString = "INSERT INTO Playlists (playlist_name) VALUES (@playlistname)";

            using (SqlCommand command = new SqlCommand(commandString, connection))
            {
                try
                {
                    command.Parameters.Add("@playlistname", SqlDbType.VarChar).Value = newPlaylistName;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception e)
                {
                    connection.Close();
                    MessageBox.Show(e.Message, "Error trying to add playlist");
                }
            }
        }

        public static void DeletePlaylist(string playlistName)
        {
            int playlistID = GetPlaylistID(playlistName);
            string commandString = "DELETE FROM Playlists WHERE playlist_id=@playlistid";

            using (SqlCommand command = new SqlCommand(commandString, connection))
            {
                try
                {
                    RemoveFilesByPlaylistID(playlistID);

                    command.Parameters.Add("@playlistid", SqlDbType.Int).Value = playlistID;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception e)
                {
                    connection.Close();
                    MessageBox.Show(e.Message, "Error trying to delete playlist");
                }
            }
        }

        /// <summary>
        /// Adds file path to database
        /// </summary>
        /// <param name="playlistName">Playlist to which file will be bound</param>
        /// <param name="filePath">File path to be stored</param>
        public static void AddFileToPlaylist(string playlistName, string filePath)
        {
            int playlistID = GetPlaylistID(playlistName);
            TagLib.File file = TagLib.File.Create(filePath);
            NAudio.Wave.AudioFileReader audioFile = new NAudio.Wave.AudioFileReader(filePath);

            string commandString = "INSERT INTO Files (file_path, file_name, duration, song_name, artist, album, year, playlist_id)"
                + " VALUES (@filepath, @filename, @duration, @songname, @artist, @album, @year, @playlistid);";
            
            using (SqlCommand command = new SqlCommand(commandString, connection))
            {
                try
                {
                    command.Parameters.Add("@filepath", SqlDbType.VarChar).Value = filePath;
                    command.Parameters.Add("@filename", SqlDbType.VarChar).Value = Path.GetFileNameWithoutExtension(filePath);
                    command.Parameters.Add("@duration", SqlDbType.BigInt).Value = audioFile.TotalTime.Ticks;
                    command.Parameters.Add("@songname", SqlDbType.VarChar).Value = String.IsNullOrEmpty(file.Tag.Title) ? "" : file.Tag.Title;
                    command.Parameters.Add("@artist", SqlDbType.VarChar).Value = String.IsNullOrEmpty(file.Tag.JoinedPerformers) ? "" : file.Tag.JoinedPerformers;
                    command.Parameters.Add("@album", SqlDbType.VarChar).Value = String.IsNullOrEmpty(file.Tag.Album) ? "" : file.Tag.Album;
                    command.Parameters.Add("@year", SqlDbType.Int).Value = file.Tag.Year;
                    command.Parameters.Add("@playlistid", SqlDbType.Int).Value = playlistID;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception e)
                {
                    connection.Close();
                    MessageBox.Show(e.Message, "Error trying to insert data");
                }
            }
        }

        /// <summary>
        /// Deletes file path from database
        /// </summary>
        /// <param name="playlistName">Playlist to which file is bound</param>
        /// <param name="filePath">File path to be deleted</param>
        public static void RemoveFileFromPlaylist(string playlistName, string filePath)
        {
            int playlistID = GetPlaylistID(playlistName);
            string commandString = "DELETE FROM Files WHERE file_path=@filepath AND playlist_id=@playlistid;";

            using (SqlCommand command = new SqlCommand(commandString, connection))
            {
                try
                {
                    command.Parameters.Add("@filepath", SqlDbType.VarChar).Value = filePath;
                    command.Parameters.Add("@playlistid", SqlDbType.Int).Value = playlistID;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception e)
                {
                    connection.Close();
                    MessageBox.Show(e.Message, "Error trying to remove data");
                }
            }
        }

        private static void RemoveFilesByPlaylistID(int playlistID)
        {
            string commandString = "DELETE FROM Files WHERE playlist_id=@playlistid;";

            using (SqlCommand command = new SqlCommand(commandString, connection))
            {
                try
                {
                    command.Parameters.Add("@playlistid", SqlDbType.Int).Value = playlistID;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception e)
                {
                    connection.Close();
                    MessageBox.Show(e.Message, "Error trying to remove data");
                }
            }
        }

        private static int GetPlaylistID(string playlistName)
        {

            string commandString = "SELECT playlist_id FROM Playlists WHERE playlist_name=@playlistname;";
            
            using (SqlCommand command = new SqlCommand(commandString, connection))
            {
                try
                {
                    command.Parameters.Add("@playlistname", SqlDbType.VarChar).Value = playlistName;

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    int playlistID = (int)reader[0];
                    connection.Close();

                    return playlistID;
                }
                catch (Exception e)
                {
                    connection.Close();
                    MessageBox.Show(e.Message);
                    return -1;
                }
            }
        }
    }
}
