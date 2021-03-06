﻿namespace OptimalPlayer.Model
{
    /// <summary>
    /// This class contains properties which describe audio file characteristics
    /// </summary>
    public class AudioFile
    {
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
