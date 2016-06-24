using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OptimalPlayer.Model;
using System.Windows.Input;

namespace OptimalPlayer.ViewModel
{
    public class EqualizerViewModel : ViewModelBase
    {
        public EqualizerViewModel()
        {
            ResetToDefault = new RelayCommand(() => ResetToDefaultExecute());

            Player.EqualizerBands = new EqualizerBand[]
            {
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 100, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 200, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 400, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 800, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 1200, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 2400, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 4800, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 9600, Gain = 0},
            };

            PropertyChanged += EqualizerViewModel_PropertyChanged;
        }        

        public float MinimumGain
        {
            get { return -30; }
        }

        public float MaximumGain
        {
            get { return 30; }
        }

        public float Band1
        {
            get { return Player.EqualizerBands[0].Gain; }
            set
            {
                if (Player.EqualizerBands[0].Gain != value)
                {
                    Player.EqualizerBands[0].Gain = value;
                    RaisePropertyChanged("Band1");
                }
            }
        }

        public float Band2
        {
            get { return Player.EqualizerBands[1].Gain; }
            set
            {
                if (Player.EqualizerBands[1].Gain != value)
                {
                    Player.EqualizerBands[1].Gain = value;
                    RaisePropertyChanged("Band2");
                }
            }
        }

        public float Band3
        {
            get { return Player.EqualizerBands[2].Gain; }
            set
            {
                if (Player.EqualizerBands[2].Gain != value)
                {
                    Player.EqualizerBands[2].Gain = value;
                    RaisePropertyChanged("Band3");
                }
            }
        }

        public float Band4
        {
            get { return Player.EqualizerBands[3].Gain; }
            set
            {
                if (Player.EqualizerBands[3].Gain != value)
                {
                    Player.EqualizerBands[3].Gain = value;
                    RaisePropertyChanged("Band4");
                }
            }
        }

        public float Band5
        {
            get { return Player.EqualizerBands[4].Gain; }
            set
            {
                if (Player.EqualizerBands[4].Gain != value)
                {
                    Player.EqualizerBands[4].Gain = value;
                    RaisePropertyChanged("Band5");
                }
            }
        }

        public float Band6
        {
            get { return Player.EqualizerBands[5].Gain; }
            set
            {
                if (Player.EqualizerBands[5].Gain != value)
                {
                    Player.EqualizerBands[5].Gain = value;
                    RaisePropertyChanged("Band6");
                }
            }
        }

        public float Band7
        {
            get { return Player.EqualizerBands[6].Gain; }
            set
            {
                if (Player.EqualizerBands[6].Gain != value)
                {
                    Player.EqualizerBands[6].Gain = value;
                    RaisePropertyChanged("Band7");
                }
            }
        }

        public float Band8
        {
            get { return Player.EqualizerBands[7].Gain; }
            set
            {
                if (Player.EqualizerBands[7].Gain != value)
                {
                    Player.EqualizerBands[7].Gain = value;
                    RaisePropertyChanged("Band8");
                }
            }
        }

        public ICommand ResetToDefault { get; set; }

        private void ResetToDefaultExecute()
        {
            for (int i = 0; i < Player.EqualizerBands.Length; i++)
            {
                Player.EqualizerBands[i].Gain = 0;

                RaisePropertyChanged("Band" + (i+1));
            }
        }

        private void EqualizerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Player.Equalizer != null)
            {
                Player.Equalizer.Update();
            }
        }
    }
}
