using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using OxyPlot;
using RotationalMotion.Abstract;
using RotationalMotion.Concrete;
using RotationalMotion.Infrastructure;
using RotationalMotion.Models;
using RotationalMotion.Utils;


namespace RotationalMotion
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string _filePath = "kiev.mp4";
		private Timer _timer;
		private bool _started = false;

		private ImageProcessor _processor;
		private IOpticalFlowAlgorithm _algorithm;

		private List<DataPoint> lPitch; 
		private List<DataPoint> lRoll; 
		private List<DataPoint> lYawing; 

		private int _videoRate = 20;

		public MainWindow()
		{
			InitializeComponent();

			_timer = new Timer(1000 / _videoRate);
			_timer.Elapsed += OnTimerTick;

			_processor = new ImageProcessor();
			_processor.FileEndRiched += ProcessorOnFileEndRiched;

			pPitch.ItemsSource = lPitch = new List<DataPoint>();
			pRoll.ItemsSource= lRoll = new List<DataPoint>();
			pYawing.ItemsSource = lYawing = new List<DataPoint>();


			_algorithm = _algorithm = new PyrLkOpticalFlowAlgorithm();

		}

		public void OnTimerTick(object sender, EventArgs args)
		{
			_processor.NextFrame(_algorithm);
			Show();
		}

		private void OnCaptureTypeChanged(object sender, SelectionChangedEventArgs e)
		{
			var combobox = (ComboBox)sender;

			var wasRan = _started;

			if (_started)
			{
				_timer.Stop();
				_started = false;
			}

			var selectedValue = combobox.SelectedItem.ToString().Split().Last();

			switch (selectedValue)
			{
				case "Camera":
					{
						ChooseFileButton.Visibility = Visibility.Hidden;
						_processor.ChangeCapture();
						break;
					}
				case "Video":
					{
						ChooseFileButton.Visibility = Visibility.Visible;
						_processor.ChangeCapture(_filePath);
						break;
					}
				default:
					{
						throw new Exception("Wrong capture source selected");
					}
			}

			if (wasRan)
			{
				_timer.Start();
			}
		}

		private void OnStartButtonClick(object sender, RoutedEventArgs e)
		{
			var button = (Button) sender;
			if (!_started)
			{
				_started = true;
				_timer.Start();
				button.Content = "Stop";
			}
			else
			{
				_started = false;
				_timer.Stop();
				button.Content = "Resume";
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

		private void Show()
		{

			Dispatcher.Invoke(DispatcherPriority.Background, new
				Action(() =>
				{
					DestinationImage.Source = _processor.Image?.ToImageSource();

					RollLabel.Content = string.Format("Roll: {0};", _processor.Roll.ToDegrees());
					PitchLabel.Content = string.Format("Pitch: {0};", _processor.Pitch.ToDegrees());
					YawingLabel.Content = string.Format("Yawing: {0};", _processor.Yawing.ToDegrees());

					lPitch.Add(new DataPoint(DateTime.Now.Ticks, _processor.Pitch.ToDegrees()));
					lRoll.Add(new DataPoint(DateTime.Now.Ticks, _processor.Roll.ToDegrees()));
					lYawing.Add(new DataPoint(DateTime.Now.Ticks, _processor.Yawing.ToDegrees()));

					if (lPitch.Count > 200)
					{
						lPitch.Remove(lPitch.First());
						lRoll.Remove(lRoll.First());
						lYawing.Remove(lYawing.First());
					}

					AnglesPlot.InvalidatePlot();
				}));
		}

		private void OnClearButtonClick(object sender, RoutedEventArgs e)
		{
			_processor.Reset();
		}


		private void OnAlgorithmChanged(object sender, SelectionChangedEventArgs e)
		{
			var combobox = (ComboBox)sender;

			var wasRan = _started;

			if (_started)
			{
				_timer.Stop();
			}

			var selectedValue = combobox.SelectedItem.ToString().Split().Last();

			switch (selectedValue)
			{
				case "PyrLK":
					{
						_algorithm = _algorithm = new PyrLkOpticalFlowAlgorithm();
						break;
					}
				case "Farneback":
					{
						_algorithm = new FarnebackOpticalFlowAlgorithm(_processor.Width, _processor.Height, 20);
						break;
					}
				default:
					{
						throw new Exception("Wrong capture source selected");
					}

			}

			if (wasRan)
			{
				_timer.Start();
			}
		}

		private void ProcessorOnFileEndRiched(object sender, EventArgs eventArgs)
		{
			_timer.Stop();
			_started = false;
		}

		private void OnChooseButtonClick(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			openFileDialog.Filter = "Videos Files |*.mp4;";
			openFileDialog.RestoreDirectory = true;

			if (openFileDialog.ShowDialog().Value)
			{
				_filePath = openFileDialog.FileName;
				_processor.ChangeCapture(_filePath);
			}
		}
	}
}