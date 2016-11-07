using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.DSP;
using System;
using System.Timers;
using System.Diagnostics;
using System.Linq;

namespace Ultrabox.ChromaSync.Plugin.AudioVisualiser
{
    public enum ScalingStrategy
    {
        Decibel,
        Linear,
        Sqrt
    }

    public class Main
    {
        private static WasapiOut _wasapiOut;
        private static IWaveSource _source;
        private static LineSpectrum _lineSpectrum;
        private static Timer _spotifyTimer;
        private static Timer _timer1;
        private static WasapiLoopbackCapture _wasapiCapture;
        public static bool _isRunning = false;
        private static int _milliseconds = 50;


        public static void RequestStart()
        {
            Debug.WriteLine("Starting Visualiser");
            _wasapiCapture = new WasapiLoopbackCapture();

            _timer1 = new Timer(_milliseconds);
            _timer1.Elapsed += Main_Elapsed;
            _spotifyTimer = new Timer(_milliseconds);
            _spotifyTimer.Elapsed += _spotifyTimer_Elapsed;
            try
            {

                _wasapiCapture.Initialize();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            var wasapiCaptureSource = new SoundInSource(_wasapiCapture);

            wasapiCaptureSource.FillWithZeros = true;
            var sampleSource = wasapiCaptureSource.ToSampleSource();
            var peakMeter = new PeakMeter(sampleSource);
            peakMeter.Interval = _milliseconds;
            var waveSource = peakMeter.ToWaveSource();
            const FftSize fftSize = FftSize.Fft4096;
            IWaveSource source = waveSource;
            var spectrumProvider = new BasicSpectrumProvider(source.WaveFormat.Channels,
                source.WaveFormat.SampleRate, fftSize);

            using (var device = _wasapiCapture.Device)
            using (var endpointVolume = AudioEndpointVolume.FromDevice(device))
            {
                var volume = endpointVolume.GetMasterVolumeLevelScalar();
                Debug.WriteLine("Volume: {0}", endpointVolume.MasterVolumeLevel);
                Debug.WriteLine("Volume: {0}", endpointVolume.MasterVolumeLevelScalar);
                Debug.WriteLine("State: {0}", device);
                // endpointVolume.SetMasterVolumeLevelScalar(0.13f, Guid.Empty);
            }

            _lineSpectrum = new LineSpectrum(fftSize)
            {
                SpectrumProvider = spectrumProvider,
                UseAverage = true,
                BarCount = 22,
                MaximumFrequency = 500,
                IsXLogScale = true,
                ScalingStrategy = ScalingStrategy.Sqrt
            };

            var notificationSource = new SingleBlockNotificationStream(source.ToSampleSource());
            notificationSource.SingleBlockRead += (s, a) => spectrumProvider.Add(a.Left, a.Right);
            _source = notificationSource.ToWaveSource(16);

            _wasapiOut = new WasapiOut(true, AudioClientShareMode.Shared, 10);
            _wasapiOut.Initialize(_source.ToMono());
            _wasapiOut.Volume = 0;
            _spotifyTimer.Start();


        }

        private static void _spotifyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isSpotifyPlaying())
            {
                if (!_isRunning) startPlaying();
            }
            else
            {
                if (_isRunning) stopPlaying();
            }

        }

        public static void startPlaying()
        {
            Debug.WriteLine("Started to play");
            _wasapiCapture.Start();
            _wasapiOut.Play();
            _isRunning = true;
            _timer1.Start();
        }

        public static void stopPlaying()
        {
            _isRunning = false;
            if (_timer1 != null)
                _timer1.Stop();
            if (_wasapiOut != null)
                _wasapiOut.Stop();
            if (_wasapiCapture != null)
                _wasapiCapture.Stop();
        }

        private static void Main_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Corale.Colore.Core.Chroma.Instance.Initialized)
                GenerateLineSpectrum();
        }


        private static void GenerateLineSpectrum()
        {
            _lineSpectrum.CreateSpectrumLine(6);
        }

        public static void RequestStop()
        {
            Stop();
        }

        public static void Stop()
        {
            stopPlaying();
            if (_spotifyTimer != null)
                _spotifyTimer.Stop();
            if (_timer1 != null)
                _timer1.Stop();

            if (_source != null)
            {

                _source.Dispose();
                _source = null;
            }

            if (_wasapiOut != null)
            {
                _wasapiOut.Stop();
                _wasapiOut.Dispose();
                _wasapiOut = null;
            }

            if (_wasapiCapture != null)
            {
                _wasapiCapture.Stop();
                _wasapiCapture.Dispose();
                _wasapiCapture = null;
            }


        }


        private static bool isSpotifyPlaying()
        {
            var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

            if (proc == null)
            {

                return false;
            }
            if (string.Equals(proc.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))
            {

                return false;
            }

            return true;
        }


    }



}
