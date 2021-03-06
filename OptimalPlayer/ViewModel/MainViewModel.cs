using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NAudio.Wave;
using OptimalPlayer.Model;
using OptimalPlayer.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OptimalPlayer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["PlaylistDatabaseConnectionString"].ConnectionString;

        private readonly UserControl playlistControl = new PlaylistsControl();
        private readonly UserControl equalizerControl = new EqualizerControl();

        // When CreatePlaylist command is called, input mode is set to Add
        // and when RenamePlaylist is called, it is set to Rename
        private enum InputMode { Add, Rename, Save };
        private InputMode playlistInputMode;

        // Stores old playlist name when that playlist is being renamed
        private string oldPlaylistName;

        private readonly Dictionary<string, Uri> iconUris = new Dictionary<string, Uri>()
        {
            { "Pause", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/pause_icon.png", UriKind.Absolute) },
            { "Play", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/play_icon.png", UriKind.Absolute) },
            { "Stop", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/stop_icon.png", UriKind.Absolute) },
            { "Previous", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/prev_icon.png", UriKind.Absolute) },
            { "Next", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/next_icon.png", UriKind.Absolute) },
            { "RepeatAll", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/repeat_all_icon.png", UriKind.Absolute) },
            { "NoRepeat", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/no_repeat_icon.png", UriKind.Absolute) },
            { "RepeatCurrent", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/repeat_one_icon.png", UriKind.Absolute) },
            { "NoShuffle", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/no_shuffle_icon.png", UriKind.Absolute) },
            { "Shuffle", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/shuffle_icon.png", UriKind.Absolute) },
            { "Equalizer", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/equalizer_icon.png", UriKind.Absolute) },
            { "Playlist", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/playlist_icon.png", UriKind.Absolute) },
            { "AddPlaylist", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/add_playlist_icon.png", UriKind.Absolute) },
            { "AddFile", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/add_file_icon.png", UriKind.Absolute) },
            { "Delete", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/delete_icon.png", UriKind.Absolute) },
            { "Rename", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/rename_icon.png", UriKind.Absolute) },
            { "LoadPlaylist", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/load_playlist_icon.png", UriKind.Absolute) },
            { "SavePlaylist", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/save_playlist_icon.png", UriKind.Absolute) },
            { "SaveToNewPlaylist", new Uri("pack://application:,,,/OptimalPlayer;component/Resources/save_to_base_icon.png", UriKind.Absolute) }
        };
        #endregion

        #region Constructor
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
                    Player.Stop();
                    selectedPlaylist = value;
                    UpdatePlaylistFromDB();
                    RaisePropertyChanged("SelectedPlaylist");
                    RaisePropertyChanged("WindowTitle");
                    RaisePropertyChanged("PlaybackNextStateIcon");
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
                RaisePropertyChanged("WindowTitle");
            }
        }

        public AudioFile FilePlaying
        {
            get
            {
                return Player.FilePlaying;
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
                PlaylistInputTextboxBackground = "White";
                RaisePropertyChanged("NewPlaylistName");
            }
        }

        private string playlistInputTextboxBackground = "White";
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

        private string windowTitle = "Optimal Player";
        public string WindowTitle
        {
            get
            {
                if (SelectedPlaylist != null && SelectedFile != null)
                {
                    return windowTitle + " - " + SelectedPlaylist + " - " + SelectedFile.Artist + " - " + SelectedFile.SongName;
                }
                else if (SelectedPlaylist == null && SelectedFile != null)
                {
                    return windowTitle + " - " + SelectedFile.Artist + " - " + SelectedFile.SongName;
                }
                else
                {
                    return windowTitle;
                }
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
                    return new BitmapImage(iconUris["Pause"]);
                }
                else
                {
                    return new BitmapImage(iconUris["Play"]);
                }
            }
        }

        public ImageSource StopPlaybackIcon { get { return new BitmapImage(iconUris["Stop"]); } }

        public ImageSource PreviousFileIcon { get { return new BitmapImage(iconUris["Previous"]); } }

        public ImageSource NextFileIcon { get { return new BitmapImage(iconUris["Next"]); } }

        public ImageSource RepeatIcon
        {
            get
            {
                if (Player.RepeatCurrentTrack)
                {
                    return new BitmapImage(iconUris["RepeatAll"]);
                }
                else if (Player.RepeatAll)
                {
                    return new BitmapImage(iconUris["NoRepeat"]);
                }
                else
                {
                    return new BitmapImage(iconUris["RepeatCurrent"]);
                }
            }
        }

        public ImageSource ShuffleIcon
        {
            get
            {
                if (Player.Shuffled)
                {
                    return new BitmapImage(iconUris["NoShuffle"]);
                }
                else
                {
                    return new BitmapImage(iconUris["Shuffle"]);
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
                    return new BitmapImage(iconUris["Equalizer"]);
                }
                else
                {
                    return new BitmapImage(iconUris["Playlist"]);
                }
            }
        }

        public ImageSource NewPlaylistIcon { get { return new BitmapImage(iconUris["AddPlaylist"]); } }

        public ImageSource AddFileIcon { get { return new BitmapImage(iconUris["AddFile"]); } }

        public ImageSource DeleteIcon { get { return new BitmapImage(iconUris["Delete"]); } }

        public ImageSource RenameIcon { get { return new BitmapImage(iconUris["Rename"]); } }

        public ImageSource LoadIcon { get { return new BitmapImage(iconUris["LoadPlaylist"]); } }

        public ImageSource SaveIcon { get { return new BitmapImage(iconUris["SavePlaylist"]); } }

        public ImageSource SaveToNewPlaylistIcon { get { return new BitmapImage(iconUris["SaveToNewPlaylist"]); } }
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
            bool dbSuccess = false;

            try
            {
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Tick += Timer_Tick;
                timer.Start();
                Player.playbackFinishedEvent += Player_playbackFinishedEventHandler;
                Player.filePlaybackStartedEvent += Player_filePlaybackStartedEventHandler;

                dbSuccess = DatabaseInterface.SetupConnection(connectionString);
                Playlists = DatabaseInterface.GetPlaylists();
                SelectedPlaylist = Playlists[0];
            }
            catch
            {
                if (dbSuccess == false)
                {
                    MessageBox.Show("Error trying to set up database connection! You may not be able to save playlists to database.", "Database access error");
                }
            }
        }

        /// <summary>
        /// Initializes commands for view buttons
        /// </summary>
        private void InitCommands()
        {
            CreatePlaylist = new RelayCommand(() => CreatePlaylistExecute());
            CreatePlaylistAndAddFiles = new RelayCommand(() => CreatePlaylistAndAddFilesExecute());
            SavePlaylist = new RelayCommand(() => SavePlaylistExecute());
            DeletePlaylist = new RelayCommand<object>((item) => DeletePlaylistExecute(item));
            RenamePlaylist = new RelayCommand<object>((item) => RenamePlaylistExecute(item));
            OpenPlaylistFile = new RelayCommand(() => OpenPlaylistFileExecute());
            SavePlaylistToFile = new RelayCommand(() => SavePlaylistToFileExecute());
            AddFileToPlaylist = new RelayCommand(() => AddFileToPlaylistExecute());
            DeleteFileFromPlaylist = new RelayCommand<object>((item) => DeleteFileFromPlaylistExecute(item as AudioFile));
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
        private void UpdatePlaylistFromDB()
        {
            if (SelectedPlaylist != null)
            {
                Files = DatabaseInterface.GetPlaylistFiles(SelectedPlaylist);
            }
            if (Files.Count > 0)
            {
                SelectedFile = Files[0];
            }
        }

        /// <summary>
        /// Adds audio files obtained from playlist file to Files list and initializes playback
        /// </summary>
        private void UpdatePlaylistFromFile(List<AudioFile> filesList)
        {
            Player.Stop();

            if (filesList != null)
            {
                Files = new ObservableCollection<AudioFile>(filesList);
            }

            SelectedPlaylist = null;

            if (Files != null && Files.Count > 0)
            {
                SelectedFile = Files[0];
            }
        }

        /// <summary>
        /// Stops playback and deletes all AudioFiles from Files list
        /// </summary>
        private void StopAndClearPlaylist()
        {
            Player.Stop();
            if (Files != null)
            {
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

        private void Player_filePlaybackStartedEventHandler()
        {
            RaisePropertyChanged("FilePlaying");
        }
        #endregion
    }
}