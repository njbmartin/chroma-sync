using CSCore.DSP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultrabox.ChromaSync.Plugin.AudioVisualiser
{
    public class LineSpectrum : SpectrumBase
    {
        private int _barCount;
        private double _barSpacing;
        private int _range = 720;

        public LineSpectrum(FftSize fftSize)
        {
            FftSize = fftSize;
        }


        public double BarSpacing
        {
            get { return _barSpacing; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");
                _barSpacing = value;
                UpdateFrequencyMapping();

                RaisePropertyChanged("BarSpacing");
                RaisePropertyChanged("BarWidth");
            }
        }

        public int BarCount
        {
            get { return _barCount; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");
                _barCount = value;
                SpectrumResolution = value;
                UpdateFrequencyMapping();

                RaisePropertyChanged("BarCount");
                RaisePropertyChanged("BarWidth");
            }
        }

        public void CreateSpectrumLine(int height)
        {


            var fftBuffer = new float[(int)FftSize];

            if (SpectrumProvider.GetFftData(fftBuffer, this))
            {


                CreateSpectrumLineInternal(fftBuffer, height);

            }
            return;
        }

        private void CreateSpectrumLineInternal(float[] fftBuffer, int height)
        {
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);
            double average = 0;
            double peak = 0;
            for (int i = 0; i < spectrumPoints.Length; i++)
            {
                SpectrumPointData p = spectrumPoints[i];
                int barIndex = p.SpectrumPointIndex;
                //double xCoord = (_barWidth * barIndex) + (BarSpacing * barIndex) + 1 + _barWidth / 2;
                average += p.Value;
                peak = p.Value > peak ? p.Value : peak;
            }
            average = (average / spectrumPoints.Length);
            var percentage = (average / peak) * _range;
            int r, g, b = 0;
            int r2, g2, b2 = 0;
            HSLColor.HsvToRgb(percentage, 1, (average / height), out r, out g, out b);
            HSLColor.HsvToRgb(percentage, 1, (average / height) / 2, out r2, out g2, out b2);
            Corale.Colore.Core.Color c2 = new Corale.Colore.Core.Color(((double)r / 255), ((double)g / 255), ((double)b / 255));
            Corale.Colore.Core.Mouse.Instance.SetAll(c2);
            Corale.Colore.Core.Mousepad.Instance.SetAll(c2);
            Corale.Colore.Core.Mouse.Instance.SetAll(c2);
            foreach (var k in spectrumPoints)
            {
                if (k.SpectrumPointIndex > 22) continue;

                percentage = (average / height) * _range;
                int ra, ga, ba = 0;
                double c = (k.Value / (peak)) * (height-2);
                
                try
                {
                    for (int i = 1; i <= height; i++)
                    {
                        if (Main._isRunning)
                        {
                            HSLColor.HsvToRgb((k.Value / peak) * (_range / i), 1, (k.Value / average), out ra, out ga, out ba);
                            Corale.Colore.Core.Color c3 = new Corale.Colore.Core.Color(((double)ra / 255), ((double)ga / 255), ((double)ba / 255));
                            Corale.Colore.Core.Keyboard.Instance[height - i, k.SpectrumPointIndex] = c >= i ? c3 : new Corale.Colore.Core.Color(10, 10, 10);
                        }
                        else
                        {
                            return;
                        }

                    }
                    for (int i = 1; i <= 4; i++)
                    {
                        if (Main._isRunning)
                        {
                            HSLColor.HsvToRgb((k.Value / peak) * (_range / i), 1, (k.Value / peak), out ra, out ga, out ba);
                            Corale.Colore.Core.Color c3 = new Corale.Colore.Core.Color(((double)ra / 255), ((double)ga / 255), ((double)ba / 255));
                            if (k.SpectrumPointIndex < 5)
                                Corale.Colore.Core.Keypad.Instance[4 - i, k.SpectrumPointIndex] = c >= i ? c3 : new Corale.Colore.Core.Color(1, 1, 1);
                        }
                        else
                        {
                            return;
                        }

                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            //Corale.Colore.Core.Headset.Instance.Clear();
            //Ultrabox.ChromaSync.Plugin.Arduino.Serial.Send(r2 + "," + g2 + "," + b2);

        }


    }
}
