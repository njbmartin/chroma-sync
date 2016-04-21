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
            for (int i = 0; i < spectrumPoints.Length; i++)
            {
                SpectrumPointData p = spectrumPoints[i];
                int barIndex = p.SpectrumPointIndex;
                //double xCoord = (_barWidth * barIndex) + (BarSpacing * barIndex) + 1 + _barWidth / 2;
                average += p.Value;

            }
            average = (average / spectrumPoints.Length);
            var percentage = (average / height) * 240;
            int r, g, b = 0;
            int r2, g2, b2 = 0;
            HSLColor.HsvToRgb(percentage, 1, (average / height), out r, out g, out b);
            HSLColor.HsvToRgb(percentage, 1, (average / height) / 2, out r2, out g2, out b2);
            Corale.Colore.Core.Color c2 = new Corale.Colore.Core.Color(((double)r / 255), ((double)g / 255), ((double)b / 255));
            Corale.Colore.Core.Chroma.Instance.SetAll(c2);

            foreach (var k in spectrumPoints)
            {
                if (k.SpectrumPointIndex > 22) continue;

                percentage = (average / height) * 240;
                int ra, ga, ba = 0;
                double c = (k.Value / height * 6);
                HSLColor.HsvToRgb((k.Value / height) * 360, 1, (k.Value / height), out ra, out ga, out ba);
                try
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (Main._isRunning)
                        {
                            Corale.Colore.Core.Color c3 = new Corale.Colore.Core.Color(((double)ra / 255), ((double)ga / 255), ((double)ba / 255));
                            Corale.Colore.Core.Keyboard.Instance[5 - i, k.SpectrumPointIndex] = c >= i ? c3 : new Corale.Colore.Core.Color(5,5,5);
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
