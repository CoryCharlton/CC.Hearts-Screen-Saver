using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CC.Hearts.Utilities
{
    public class FramesRateMonitor : DependencyObject
    {
        #region Constructor
        public FramesRateMonitor() : this(15)
        {
            // Empty constructor    
        }

        public FramesRateMonitor(int sampleSize)
        {
            _Samples = new List<double>(sampleSize);

            SampleSize = sampleSize;
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register("FramesPerSecond", typeof(double), typeof(FramesRateMonitor), new PropertyMetadata(0.00));
        #endregion

        #region Private Fields
        private int _FrameCount;
        private readonly List<double> _Samples;
        private bool _IsEnabled;
        private readonly Stopwatch _Stopwatch = new Stopwatch();
        #endregion

        #region Public Properties
        public double FramesPerSecond
        {
            get { return (double)GetValue(FramesPerSecondProperty); }
            set { SetValue(FramesPerSecondProperty, value); }
        }

        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        public int SampleSize { get; set; }
        #endregion

        #region Private Methods
        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            if (_IsEnabled)
            {
                double totalSeconds = _Stopwatch.Elapsed.TotalSeconds;

                if (!_Stopwatch.IsRunning)
                {
                    _Stopwatch.Start();
                }
            
                _FrameCount++;

                if (totalSeconds > 1)
                {
                    double framesPerSecond = (_FrameCount/totalSeconds);
                    _Samples.Add(framesPerSecond);

                    while (_Samples.Count > SampleSize)
                    {
                        _Samples.RemoveAt(0);
                    }

                    // ReSharper disable RedundantAssignment
                    FramesPerSecond = _Samples.Aggregate((totalValue, nextValue) => totalValue += nextValue)/_Samples.Count;
                    // ReSharper restore RedundantAssignment

                    _FrameCount = 0;
                    _Stopwatch.Reset();
                }
            }
        }
        #endregion

        #region Public Methods
        public void Reset()
        {
            FramesPerSecond = 0;

            _FrameCount = 0;
            _Samples.Clear();
            _Stopwatch.Reset();
        }

        public void Start()
        {
            if (!_IsEnabled)
            {
                _IsEnabled = true;

                Reset();
                CompositionTarget.Rendering += CompositionTargetRendering;

                _Stopwatch.Start();
            }
        }

        public void Stop()
        {
            _IsEnabled = false;

            CompositionTarget.Rendering -= CompositionTargetRendering;            

            _Stopwatch.Stop();
        }
        #endregion
    }
}
