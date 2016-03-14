﻿using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using MathNet.Numerics.LinearAlgebra.Double;
using RotationalMotion.Abstract;
using RotationalMotion.Concrete;
using RotationalMotion.Infrastructure;
using RotationalMotion.Models;
using RotationalMotion.Utils;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;


namespace RotationalMotion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _timer;
        private string _videoPath = "test.mp4";
        private bool _started = false;

        private ImageProcessor _processor;
        private IOpticalFlowAlgorithm _algorithm;

        private int _videoRate = 15;

        public MainWindow()
        {
            InitializeComponent();

            _timer = new Timer(1000 / _videoRate);
            _timer.Elapsed += OnTimerTick;

            _processor = new ImageProcessor();
            //_algorithm = new PyrLkOpticalFlowAlgorithm();
            _algorithm = new FarnebackOpticalFlowAlgorithm(1280, 720, 20);
            //_algorithm = new DualTVLOpticalFlowAlgorithm(640, 480, 20);
            //_algorithm = new LKOpticalFlowAlgorithm(640, 480, 20);
        }

        

        public void OnTimerTick(object sender, EventArgs args)
        {
            var result = _processor.NextFrame(_algorithm);
            Show(result);
        }

        private void OnCaptureTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = (ComboBox)sender;
            _timer.Stop();
            _started = false;

            var selectedValue = combobox.SelectedValue.ToString().Split().Last();

            switch (selectedValue)
            {
                case "Camera":
                    {
                        
                        break;
                    }
                case "Video":
                    {

                        break;
                    }
                default:
                    {
                        throw new Exception("Wrong capture source selected");
                    }
            }
        }

        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_started)
            {
                _started = true;
                _timer.Start();
            }
        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            if (_started)
            {
                _started = false;
                _timer.Stop();
            }
        }

        private void Show(ProcessingResult result)
        {

            Dispatcher.Invoke(DispatcherPriority.Background, new
                Action(() =>
                {
                    //SourceImage.Source = result.Previous.ToImageSource();
                    DestinationImage.Source = result.Frame.ToImageSource();
                    result.Frame.Dispose();
                    RollLabel.Content = string.Format("Roll: {0};", _processor.Roll.ToDegrees());
                    PitchLabel.Content = string.Format("Pitch: {0};", _processor.Pitch.ToDegrees());
                    YawingLabel.Content = string.Format("Yawing: {0};", _processor.Yawing.ToDegrees());
                }));
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            _processor.Reset();
        }       
    }
}