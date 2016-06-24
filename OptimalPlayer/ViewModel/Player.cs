using NAudio.Wave;
using OptimalPlayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OptimalPlayer.ViewModel
{
    public static class Player
    {
        #region Fields
        public static IWavePlayer playbackDevice;
        private static AudioFileReader inputStream;
        private static List<AudioFile> audioFiles;
        #endregion

        #region Properties
        public static AudioFile FilePlaying { get; private set; }

        public static PlaybackState PlaybackState
        {
            get
            {
                if (playbackDevice != null)
                {
                    return playbackDevice.PlaybackState;
                }
                else
                {
                    return PlaybackState.Stopped;
                }
            }
        }

        public static Equalizer Equalizer { get; set; }

        public static EqualizerBand[] EqualizerBands { get; set; }

        public static TimeSpan Duration
        {
            get
            {
                if (playbackDevice != null)
                {
                    return inputStream.TotalTime;
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public static long Length
        {
            get
            {
                if (inputStream != null)
                {
                    return (long)inputStream.TotalTime.TotalSeconds;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static long Position
        {
            get
            {
                if (inputStream != null)
                {
                    return (long)inputStream.CurrentTime.TotalSeconds;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (inputStream != null)
                {
                    inputStream.CurrentTime = TimeSpan.FromSeconds(value);
                }
            }
        }

        public static string PositionTime
        {
            get
            {
                if (inputStream != null)
                {
                    return inputStream.CurrentTime.ToString(@"mm\:ss");
                }
                else
                {
                    return TimeSpan.Zero.ToString();
                }
            }
        }

        public static bool RepeatCurrentTrack { get; set; }

        public static bool RepeatAll { get; set; }

        public static bool Shuffled { get; private set; }

        private static float volume = 1;
        public static float Volume
        {
            get
            {
                if (inputStream != null)
                {
                    return inputStream.Volume;
                }
                else
                {
                    return volume;
                }
            }
            set
            {
                if (inputStream != null)
                {
                    inputStream.Volume = value;
                }

                volume = value;
            }
        }
        #endregion

        #region Methods
        public static void Init(List<AudioFile> inputAudioFiles)
        {
            audioFiles = inputAudioFiles;

            CreateDevice();
        }
        
        public static void StartPlaying(AudioFile fileToPlay)
        {
            try
            {
                Stop();

                inputStream = new AudioFileReader(fileToPlay.Path);

                Equalizer = new Equalizer(inputStream, EqualizerBands);

                playbackDevice.Init(Equalizer);

                playbackDevice.PlaybackStopped += PlaybackDevice_PlaybackStopped;

                FilePlaying = fileToPlay;

                Play();

                filePlaybackStartedEvent();
            }
            catch
            {
                Stop();
            }
        }

        public static void Play()
        {
            if (playbackDevice != null && playbackDevice.PlaybackState != PlaybackState.Playing)
            {
                inputStream.Volume = volume;
                playbackDevice.Play();
            }
        }

        public static void Pause()
        {
            if (playbackDevice != null)
            {
                playbackDevice.Pause();
            }
        }

        public static void Stop()
        {
            if (playbackDevice != null)
            {
                playbackDevice.PlaybackStopped -= PlaybackDevice_PlaybackStopped;

                playbackDevice.Stop();

                playbackDevice.Dispose();
            }

            if (inputStream != null)
            {
                inputStream.Dispose();

                inputStream = null;
            }
        }

        public static void PlayNext()
        {
            if (audioFiles != null && audioFiles.Count > 0)
            {
                if (FilePlaying != audioFiles.Last())
                {
                    StartPlaying(audioFiles[audioFiles.IndexOf(FilePlaying) + 1]);
                }
                else if (RepeatAll)
                {
                    StartPlaying(audioFiles[0]);
                }
            }
        }

        public static void PlayPrevious()
        {
            if (audioFiles != null && audioFiles.Count > 0 && FilePlaying != null && FilePlaying != audioFiles.First())
            {
                StartPlaying(audioFiles[audioFiles.IndexOf(FilePlaying) - 1]);
            }
        }

        public static void Shuffle()
        {
            if(audioFiles.Count > 1)
            {
                int n = audioFiles.Count;
                int randMin = 0;
                Random random = new Random();

                // If some file is being played, make it the first file in playlist and don't shuffle it
                if (FilePlaying != null)
                {
                    audioFiles[audioFiles.IndexOf(FilePlaying)] = audioFiles[0];
                    audioFiles[0] = FilePlaying;
                    randMin = 1;
                }

                while (n > 1)
                {
                    int k = random.Next(randMin, n) % n;
                    n--;

                    AudioFile temp = audioFiles[k];
                    audioFiles[k] = audioFiles[n];
                    audioFiles[n] = temp;
                }
            }

            Shuffled = true;
        }

        public static void Unshuffle()
        {
            audioFiles = audioFiles.OrderBy(audioFile => audioFile.FileID).ToList();

            Shuffled = false;
        }

        private static void CreateDevice()
        {
            playbackDevice = new WaveOutEvent { DesiredLatency = 200 };
        }
        #endregion

        #region Events and event handlers
        private static void PlaybackDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (RepeatCurrentTrack)
            {
                StartPlaying(FilePlaying);
            }
            else if (RepeatAll && FilePlaying == audioFiles.Last())
            {
                StartPlaying(audioFiles.First());
            }
            else if (FilePlaying == audioFiles.Last())
            {
                playbackFinishedEvent();
            }
            else
            {
                PlayNext();
            }
        }

        /// <summary>
        /// Raises when last file in playlist has stopped playling
        /// </summary>
        public static event PlaybackFinishedEvent playbackFinishedEvent;

        public delegate void PlaybackFinishedEvent();
        
        /// <summary>
        /// Raises when playback is started
        /// </summary>
        public static event FilePlaybackStartedEvent filePlaybackStartedEvent;

        public delegate void FilePlaybackStartedEvent();
        #endregion
    }
}
