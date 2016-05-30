using Microsoft.Win32;
using NAudio.Wave;
using OptimalPlayer.View;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace OptimalPlayer.ViewModel
{
    public partial class MainViewModel
    {
        public ICommand CreatePlaylist { get; set; }

        private void CreatePlaylistExecute()
        {
            if (PlaylistInputControl == null)
            {
                PlaylistInputControl = new PlaylistInputControl();
                playlistInputMode = InputMode.Add;
            }
            else
            {
                PlaylistInputControl = null;
            }
        }

        public ICommand SavePlaylist { get; set; }

        private void SavePlaylistExecute()
        {
            if (!Playlists.Contains(NewPlaylistName) && !String.IsNullOrEmpty(NewPlaylistName))
            {
                if (playlistInputMode == InputMode.Add)
                {
                    DatabaseInterface.AddPlaylist(NewPlaylistName);
                }
                else if (playlistInputMode == InputMode.Rename)
                {
                    DatabaseInterface.RenamePlaylist(oldPlaylistName, NewPlaylistName);
                }

                RefreshPlaylistsList();
                PlaylistInputControl = null;

                NewPlaylistName = "New Playlist";
            }
            else
            {
                PlaylistInputTextboxBackground = "LightCoral";
            }
        }

        public ICommand DeletePlaylist { get; set; }

        private void DeletePlaylistExecute(object playlist)
        {
            if (playlist.ToString() == SelectedPlaylist)
            {
                StopExecute();
                if (SelectedPlaylist != Playlists.First())
                {
                    SelectedPlaylist = Playlists[Playlists.IndexOf(SelectedPlaylist) - 1];
                }
                else if (SelectedPlaylist != Playlists.Last())
                {
                    SelectedPlaylist = Playlists[Playlists.IndexOf(SelectedPlaylist) + 1];
                }
            }

            DatabaseInterface.DeletePlaylist(playlist.ToString());
            RefreshPlaylistsList();
            UpdatePlaylist();
            RaisePropertyChanged("PlaybackNextStateIcon");
        }

        public ICommand RenamePlaylist { get; set; }

        private void RenamePlaylistExecute(object playlist)
        {
            if (PlaylistInputControl == null)
            {
                PlaylistInputControl = new PlaylistInputControl();
                oldPlaylistName = playlist.ToString();
                playlistInputMode = InputMode.Rename;
            }
            else
            {
                PlaylistInputControl = null;
            }
        }

        public ICommand OpenPlaylistFile { get; set; }

        private void OpenPlaylistFileExecute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Supported Files (*.xspf;*.wpl;*.m3u)|*.xspf;*.wpl;*.m3u|All Files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                Files = PlaylistReader.GetFilesFromPlaylist(openFileDialog.FileName);
            }

            SelectedPlaylist = null;
            UpdatePlaylist();
        }

        public ICommand AddFileToPlaylist { get; set; }

        private void AddFileToPlaylistExecute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Supported Files (*.wav;*.mp3)|*.wav;*.mp3|All Files (*.*)|*.*";
            openFileDialog.Multiselect = true;
            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                if (Playlists.Count == 0)
                {
                    SavePlaylistExecute();
                    RefreshPlaylistsList();
                    SelectedPlaylist = Playlists[0];
                }
                else if (SelectedPlaylist == null)
                {
                    MessageBox.Show("Please, select the playlist!", "No playlist selected");
                }

                foreach (string fileName in openFileDialog.FileNames)
                {
                    DatabaseInterface.AddFileToPlaylist(SelectedPlaylist, fileName);
                }
            }

            UpdatePlaylist();
        }

        public ICommand DeleteFileFromPlaylist { get; set; }

        private void DeleteFileFromPlaylistExecute()
        {
            try
            {
                if (SelectedFile == Player.FilePlaying)
                {
                    PlayNextExecute();
                }

                if (SelectedFile == Files.Last())
                {
                    StopExecute();
                }

                DatabaseInterface.RemoveFileFromPlaylist(SelectedPlaylist, SelectedFile.Path);
                Files.Remove(SelectedFile);

                RaisePropertyChanged("PlaybackNextStateIcon");
            }
            catch
            {
                return;
            }
        }

        public ICommand PlayPause { get; set; }

        private void PlayPauseExecute()
        {
            if (Player.PlaybackState == PlaybackState.Playing)
            {
                Player.Pause();
            }
            else if (Player.PlaybackState == PlaybackState.Paused)
            {
                Player.Play();
            }
            else
            {
                Player.StartPlaying(SelectedFile);
            }

            RaisePropertyChanged("PlaybackNextStateIcon");
        }

        public ICommand Stop { get; set; }

        private void StopExecute()
        {
            Player.Stop();

            RaisePropertyChanged("PlaybackNextStateIcon");
        }

        public ICommand PlayNext { get; set; }

        private void PlayNextExecute()
        {
            Player.PlayNext();

            RaisePropertyChanged("PlaybackNextStateIcon");
        }

        public ICommand PlayPrevious { get; set; }

        private void PlayPreviousExecute()
        {
            Player.PlayPrevious();

            RaisePropertyChanged("PlaybackNextStateIcon");
        }

        public ICommand PlayClicked { get; set; }

        private void PlayClickedExecute()
        {
            Player.StartPlaying(SelectedFile);

            RaisePropertyChanged("PlaybackNextStateIcon");
        }

        public ICommand RepeatCommand { get; set; }

        private void RepeatCommandExecute()
        {
            if (!Player.RepeatCurrentTrack && !Player.RepeatAll)
            {
                Player.RepeatCurrentTrack = true;
                Player.RepeatAll = false;
            }
            else if (Player.RepeatCurrentTrack && !Player.RepeatAll)
            {
                Player.RepeatCurrentTrack = false;
                Player.RepeatAll = true;
            }
            else
            {
                Player.RepeatCurrentTrack = false;
                Player.RepeatAll = false;
            }

            RaisePropertyChanged("RepeatIcon");
        }

        public ICommand ShuffleUnshuffleCommand { get; set; }

        private void ShuffleUnshuffleCommandExecute()
        {
            if (!Player.Shuffled)
            {
                Player.Shuffle();
            }
            else
            {
                Player.Unshuffle();
            }

            RaisePropertyChanged("ShuffleIcon");
        }

        public ICommand ChangeSideControl { get; set; }

        private void ChangeSideControlExecute()
        {
            if (SideControl == playlistControl)
            {
                SideControl = equalizerControl;
            }
            else
            {
                SideControl = playlistControl;
            }

            RaisePropertyChanged("SideControlIcon");
        }

        public ICommand SelectDoubleClickedPlaylist { get; set; }

        private void SelectDoubleClickedPlaylistExecute(object playlist)
        {
            SelectedPlaylist = playlist.ToString();
        }
    }
}
