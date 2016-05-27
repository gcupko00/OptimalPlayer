using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using NAudio.Wave;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using OptimalPlayer.View;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OptimalPlayer.Model;
using System.Configuration;

namespace OptimalPlayer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        // Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\PlaylistsDatabase.mdf;Integrated Security=True
        private string connectionString = ConfigurationManager.ConnectionStrings["PlaylistDatabaseConnectionString"].ConnectionString;

        private UserControl playlistControl = new PlaylistsControl();
        private UserControl equalizerControl = new EqualizerControl();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            InitCommands();
            InitUI();
            InitData();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Playlists loaded from database and displayed in side control
        /// </summary>
        public ObservableCollection<string> Playlists { get; set; }

        public ObservableCollection<AudioFile> files;
        /// <summary>
        /// Files loaded from database depending on selected playlist and displayed in main listview
        /// </summary>
        public ObservableCollection<AudioFile> Files
        {
            get
            {
                return files;
            }
            set
            {
                files = value;
                if (files != null)
                {
                    Player.Init(files.ToList());
                }
                // Must raise because ObservableCollection doesn't notify when its value is changed
                // but only when items are added or removed or when the whole list is refreshed
                RaisePropertyChanged("Files");
            }
        }

        private string selectedPlaylist;
        /// <summary>
        /// Currently selected playlist from which files are displayed in main listview
        /// </summary>
        public string SelectedPlaylist
        {
            get
            {
                return selectedPlaylist;
            }
            set
            {
                if (selectedPlaylist != value)
                {
                    selectedPlaylist = value;
                    StopExecute();
                    UpdatePlaylist();
                    RaisePropertyChanged("SelectedPlaylist");
                }
            }
        }

        private AudioFile selectedFile;
        /// <summary>
        /// Currently selected audio file which is loaded to audio playback
        /// </summary>
        public AudioFile SelectedFile
        {
            get
            {
                return selectedFile;
            }
            set
            {
                selectedFile = value;
                RaisePropertyChanged("SelectedFile");
            }
        }
        
        /// <summary>
        /// Gets or sets playback volume in Player
        /// </summary>
        public float Volume
        {
            get
            {
                return Player.Volume;
            }
            set
            {
                Player.Volume = value;
                RaisePropertyChanged("Volume");
            }
        }

        /// <summary>
        /// Gets length of currently playing file in seconds
        /// </summary>
        public long PlaylingFileLength
        {
            get
            {
                return Player.Length;
            }
        }

        /// <summary>
        /// Gets or sets current position in file being played (in seconds)
        /// </summary>
        public long CurrentPosition
        {
            get
            {
                return Player.Position;
            }
            set
            {
                Player.Position = value;
                RaisePropertyChanged("Position");
            }
        }

        /// <summary>
        /// Gets current position of file being played as formatted string
        /// </summary>
        public string CurrentPositionTime
        {
            get
            {
                return Player.PositionTime;
            }
        }

        private UserControl sideControl;
        /// <summary>
        /// Currently selected side control
        /// </summary>
        public UserControl SideControl
        {
            get
            {
                return sideControl;
            }
            set
            {
                sideControl = value;
                RaisePropertyChanged("SideControl");
            }
        }

        private UserControl playlistInputControl = null;
        /// <summary>
        /// Gets or sets control used to add new playlist
        /// </summary>
        public UserControl PlaylistInputControl
        {
            get
            {
                return playlistInputControl;
            }
            set
            {
                playlistInputControl = value;
                RaisePropertyChanged("PlaylistInputControl");
            }
        }

        private string newPlaylistName = "New Playlist";
        /// <summary>
        /// Gets or sets name of playlist being added
        /// </summary>
        public string NewPlaylistName
        {
            get
            {
                return newPlaylistName;
            }
            set
            {
                newPlaylistName = value;
                PlaylistInputTextboxBackground = "Transparent";
                RaisePropertyChanged("NewPlaylistName");
            }
        }

        private string playlistInputTextboxBackground = "Transparent";
        /// <summary>
        /// Gets or sets background color for new playlist name textbox
        /// </summary>
        public string PlaylistInputTextboxBackground
        {
            get
            {
                return playlistInputTextboxBackground;
            }
            set
            {
                playlistInputTextboxBackground = value;
                RaisePropertyChanged("PlaylistInputTextboxBackground");
            }
        }

        #region Button images
        /// <summary>
        /// Icon to be displayed on play button according to current state of audio playback
        /// </summary>
        public ImageSource PlaybackNextStateIcon
        {
            get
            {
                if (Player.PlaybackState == PlaybackState.Playing)
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\pause_icon.png"));
                }
                else
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\play_icon.png"));
                }
            }
        }

        public ImageSource StopPlaybackIcon
        {
            get
            {
                return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\stop_icon.png"));
            }
        }

        public ImageSource PreviousFileIcon
        {
            get
            {
                return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\prev_icon.png"));
            }
        }

        public ImageSource NextFileIcon
        {
            get
            {
                return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\next_icon.png"));
            }
        }

        public ImageSource RepeatIcon
        {
            // to be updated to return icon according to playback repeat state
            get
            {
                if (Player.RepeatCurrentTrack)
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\repeat_all_icon.png"));
                }
                else if (Player.RepeatAll)
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\no_repeat_icon.png"));
                }
                else
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\repeat_one_icon.png"));
                }
            }
        }

        public ImageSource ShuffleIcon
        {
            get
            {
                if (Player.Shuffled)
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\no_shuffle_icon.png"));
                }
                else
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\shuffle_icon.png"));
                }
            }
        }

        /// <summary>
        /// Icon to be displayed on button which is used to change side control
        /// </summary>
        public ImageSource SideControlIcon
        {
            get
            {
                if (SideControl is PlaylistsControl)
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\equalizer_icon.png"));
                }
                else
                {
                    return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\playlist_icon.png"));
                }
            }
        }

        public ImageSource NewPlaylistIcon
        {
            get
            {
                return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\add_playlist_icon.png"));
            }
        }

        public ImageSource AddFileIcon
        {
            get
            {
                return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\add_file_icon.png"));
            }
        }

        public ImageSource DeleteIcon
        {
            get
            {
                return new BitmapImage(new Uri(@"C:\Users\Gligorije\Documents\Visual Studio 2015\Projects\OptimalPlayer\OptimalPlayer\Resources\delete_icon.png"));
            }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Initializes user interface
        /// </summary>
        private void InitUI()
        {
            SideControl = playlistControl;
        }

        /// <summary>
        /// Initializes necessary data
        /// </summary>
        private void InitData()
        {
            try
            {
                DatabaseInterface.SetupConnection(connectionString);
                Playlists = DatabaseInterface.GetPlaylists();
                SelectedPlaylist = Playlists[0];
                UpdatePlaylist();
                Player.playbackFinishedEventHandler += Player_playbackFinishedEventHandler;
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            catch
            {
                MessageBox.Show("Unable to load files!", "Error");
            }
        }
        
        /// <summary>
        /// Initializes commands for view buttons
        /// </summary>
        private void InitCommands()
        {
            CreatePlaylist = new RelayCommand(() => CreatePlaylistExecute());
            SavePlaylist = new RelayCommand(() => SavePlaylistExecute());
            DeletePlaylist = new RelayCommand<object>((item) => DeletePlaylistExecute(item));
            AddFileToPlaylist = new RelayCommand(() => AddFileToPlaylistExecute());
            DeleteFileFromPlaylist = new RelayCommand(() => DeleteFileFromPlaylistExecute());
            PlayPause = new RelayCommand(() => PlayPauseExecute());
            PlayNext = new RelayCommand(() => PlayNextExecute());
            PlayPrevious = new RelayCommand(() => PlayPreviousExecute());
            PlayClicked = new RelayCommand(() => PlayClickedExecute());
            Stop = new RelayCommand(() => StopExecute());
            RepeatCommand = new RelayCommand(() => RepeatCommandExecute());
            ShuffleUnshuffleCommand = new RelayCommand(() => ShuffleUnshuffleCommandExecute());
            ChangeSideControl = new RelayCommand(() => ChangeSideControlExecute());
            SelectDoubleClickedPlaylist = new RelayCommand<object>((item) => SelectDoubleClickedPlaylistExecute(item));
        }

        /// <summary>
        /// Updates list of files according to selected playlist
        /// </summary>
        private void UpdatePlaylist()
        {
            if (SelectedPlaylist != null)
            {
                Files = DatabaseInterface.GetPlaylistFiles(SelectedPlaylist);
                if (Files != null && Files.Count > 1)
                {
                    SelectedFile = Files[0];
                    Player.Init(Files.ToList());
                }
            }
            else
            {
                Player.Stop();
                Files.Clear();
            }
        }

        /// <summary>
        /// Updates list of playlists in playlist control
        /// </summary>
        private void RefreshPlaylistsList()
        {
            Playlists = DatabaseInterface.GetPlaylists();
            RaisePropertyChanged("Playlists");
        }
        #endregion

        #region Commands
        public ICommand CreatePlaylist { get; set; }

        private void CreatePlaylistExecute()
        {
            if (PlaylistInputControl == null)
            {
                PlaylistInputControl = new PlaylistInputControl();
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
                DatabaseInterface.AddPlaylist(NewPlaylistName);
                RefreshPlaylistsList();
                PlaylistInputControl = null;
            }
            else
            {
                PlaylistInputTextboxBackground = "LightCoral";
            }

            NewPlaylistName = "New Playlist";
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
        #endregion

        #region Event handlers
        private void Timer_Tick(object sender, EventArgs e)
        {
            RaisePropertyChanged("CurrentPosition");
            RaisePropertyChanged("PlaylingFileLength");
            RaisePropertyChanged("CurrentPositionTime");
        }

        private void Player_playbackFinishedEventHandler()
        {
            RaisePropertyChanged("PlaybackNextStateIcon");
        }
        #endregion
    }
}