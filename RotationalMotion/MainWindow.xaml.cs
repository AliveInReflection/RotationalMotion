using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
		private Timer _timer;
		private bool _started = false;

		private ImageProcessor _processor;
		private IOpticalFlowAlgorithm _algorithm;

		private int _videoRate = 30;

		public MainWindow()
		{
			InitializeComponent();

			_timer = new Timer(1000 / _videoRate);
			_timer.Elapsed += OnTimerTick;

			_processor = new ImageProcessor();
			_processor.FileEndRiched += ProcessorOnFileEndRiched;


			_algorithm = _algorithm = new PyrLkOpticalFlowAlgorithm();

		}

		public void OnTimerTick(object sender, EventArgs args)
		{
			var result = _processor.NextFrame(_algorithm);
			Show(result);
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
						_processor.ChangeCapture();
						break;
					}
				case "Video":
					{
						_processor.ChangeCapture("roll.mp4");
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

		private void Show(ProcessingResult result)
		{

			Dispatcher.Invoke(DispatcherPriority.Background, new
				Action(() =>
				{
					DestinationImage.Source = result.Frame.ToImageSource();
					result.Frame.Dispose();
					if (result.AngularPosition != null)
					{
						RollLabel.Content = string.Format("Roll: {0};", result.AngularPosition.Roll.ToDegrees());
						PitchLabel.Content = string.Format("Pitch: {0};", result.AngularPosition.Pitch.ToDegrees());
						YawingLabel.Content = string.Format("Yawing: {0};", result.AngularPosition.Yawing.ToDegrees());
					}
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

	}
}