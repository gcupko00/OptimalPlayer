using NAudio.Dsp;
using NAudio.Wave;
using OptimalPlayer.Model;

namespace OptimalPlayer.ViewModel
{
    public class Equalizer : ISampleProvider
    {
        #region Fields
        private readonly ISampleProvider sourceProvider;
        private readonly EqualizerBand[] bands;
        private readonly BiQuadFilter[,] filters;
        private readonly int channels;
        private readonly int bandCount;
        private bool updated;
        #endregion

        #region Constructors
        public Equalizer(ISampleProvider sourceProvider, EqualizerBand[] bands)
        {
            this.sourceProvider = sourceProvider;
            this.bands = bands;
            channels = sourceProvider.WaveFormat.Channels;
            bandCount = bands.Length;
            filters = new BiQuadFilter[channels, bands.Length];
            CreateFilters();
        }
        #endregion

        #region Properties
        public WaveFormat WaveFormat
        {
            get
            {
                return sourceProvider.WaveFormat;
            }
        }
        #endregion

        #region Methods
        private void CreateFilters()
        {
            for (int bandIndex = 0; bandIndex < bandCount; bandIndex++)
            {
                EqualizerBand band = bands[bandIndex];

                for (int n = 0; n < channels; n++)
                {
                    if (filters[n, bandIndex] == null)
                    {
                        filters[n, bandIndex] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, band.Frequency, band.Bandwidth, band.Gain);
                    }
                    else
                    {
                        filters[n, bandIndex].SetPeakingEq(sourceProvider.WaveFormat.SampleRate, band.Frequency, band.Bandwidth, band.Gain);
                    }
                }
            }
        }

        public void Update()
        {
            CreateFilters();
            updated = true;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = sourceProvider.Read(buffer, offset, count);

            if (updated)
            {
                CreateFilters();
                updated = false;
            }

            for (int n = 0; n < samplesRead; n++)
            {
                int ch = n % channels;

                for (int bandIndex = 0; bandIndex < bandCount; bandIndex++)
                {
                    buffer[offset + n] = filters[ch, bandIndex].Transform(buffer[offset + n]);
                }
            }

            return samplesRead;
        }
        #endregion
    }
}
