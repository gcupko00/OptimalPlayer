namespace OptimalPlayer.Model
{
    public class AudioFile
    {

        //public AudioFile(string path, int? fileID = null, string songName = null, string artist = null, string album = null, int? year = null)
        //{
        //    Path = path;
        //    Name = System.IO.Path.GetFileName(path);

        //    FileID = fileID;
        //    SongName = songName;
        //    Artist = artist;
        //    Album = album;
        //    Year = year;
        //}

        public int FileID { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public string Duration { get; set; }

        public string SongName { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public int? Year { get; set; }
    }
}
