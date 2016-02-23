using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.GPU;
using Emgu.CV.Structure;
using Emgu.CV.VideoStab;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;


namespace TestProj
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Capture _capture;
        private Timer _timer;
        private string _videoPath = "test.mp4";
        private bool _started = false;
        private Image<Gray, byte> _prevFrame;
        private Image<Gray, byte> _prevBuffer;
        private Matrix _rotation;
        private OnePassStabilizer _stabilizer;
        private FrameSource _frameSource;
        private PointF[][] _prevFeatures;
        private PointF[] _currFeatures;

        private int _videoRate = 20;

        public MainWindow()
        {
            InitializeComponent();

            // _capture = new Capture(_videoPath);
            _capture = new Capture();
            _frameSource = new CaptureFrameSource(_capture);
            _stabilizer = new OnePassStabilizer(_frameSource);        

            _timer = new Timer(1000 / _videoRate);
            _timer.Elapsed += OnTimerTick;

            _prevBuffer = new Image<Gray, byte>(640, 480);

            _rotation = DenseMatrix.OfArray(new double[,]{{0}, {0}, {0}});

        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public void OnTimerTick(object sender, EventArgs args)
        {

            using (var currentFrame = _capture.RetrieveGrayFrame())
            {
                //var tmp = _stabilizer.NextFrame();
                if (_prevFrame != null)
                {
                    Image<Gray, float> flowX = new Image<Gray, float>(640, 480);
                    Image<Gray, float> flowY = new Image<Gray, float>(640, 480);

                    //OpticalFlow.Farneback(_prevFrame, currentFrame, flowX, flowY, 0.1, 2, 4, 1, 2, 1.2, OPTICALFLOW_FARNEBACK_FLAG.FARNEBACK_GAUSSIAN);

                    //OpticalFlow.HS(_prevFrame, currentFrame, true, flowX, flowY, 1, new MCvTermCriteria(20));
                    //cv2.calcOpticalFlowFarneback(prvs,next, None, 0.5, 3, 15, 3, 5, 1.2, 0)

                    //OpticalFlow.LK(_prevFrame, currentFrame, new System.Drawing.Size(5, 5), flowX, flowY);


                    //var currentBuffer = new Image<Gray, byte>(640, 480);
                    //var currentFeatures = new PointF[0];
                    //var currentStatus = new byte[0];
                    //var trackError = new float[0];
                    //OpticalFlow.PyrLK(_prevFrame, currentFrame, _prevBuffer, currentBuffer, _prevFeatures, new System.Drawing.Size(5, 5), 4, new MCvTermCriteria(5), LKFLOW_TYPE.DEFAULT, out currentFeatures, out currentStatus, out trackError);
                    //_prevFeatures = currentFeatures;

                    _prevFeatures = _prevFrame.GoodFeaturesToTrack(5, 1, 10, 10);
                    var criteria = new MCvTermCriteria();
                    byte[] status;
                    float[] error;

                    OpticalFlow.PyrLK(_prevFrame, currentFrame, _prevFeatures[0], new System.Drawing.Size(10,10), 1, criteria, out _currFeatures, out status, out error);

                    CalculateAngles(flowX, flowY, 10);

                    var flowMap = currentFrame.Clone().Convert<Gray, float>();

                    //DrawFlowVectors(flowMap, flowX, flowY, 5);
                    DrawFlowVectorsForPyrLk(flowMap, _prevFeatures[0], _currFeatures);

                    ShowImages(currentFrame, flowMap);
                    ShowAngles();

                }

                _prevFrame = currentFrame.Clone();
            }
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
                        _capture.Dispose();
                        _capture = new Capture();
                        _capture.Grab();
                        break;
                    }
                case "Video":
                    {
                        _capture.Dispose();
                        _capture = new Capture(_videoPath);
                        _capture.Grab();
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

        private void ShowImages(Image<Gray, byte> left, Image<Gray, float> right)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new
                Action(() =>
                {
                    if (left != null)
                    {
                        SourceImage.Source = BitmapToImageSource(left.ToBitmap());
                    }
                }));

            Dispatcher.Invoke(DispatcherPriority.Background, new
                Action(() =>
                {
                    if (right != null)
                    {
                        DestinationImage.Source = BitmapToImageSource(right.ToBitmap());
                    }
                }));
        }

        private void ShowAngles()
        {
            Dispatcher.Invoke(() =>
            {
                RxLabel.Content = string.Format("Rx: {0};", Math.Round(_rotation[0, 0]*180/Math.PI, 5));
                RyLabel.Content = string.Format("Ry: {0};", Math.Round(_rotation[1, 0]*180/Math.PI, 5));
                RzLabel.Content = string.Format("Rz: {0};", Math.Round(_rotation[2, 0] * 180 / Math.PI, 5));
            });
        }

        private void DrawFlowVectors(Image<Gray, float> image, Image<Gray, float> flowX, Image<Gray, float> flowY,
            int step)
        {
            for (int i = 0; i < image.Height; i+=step)
            {
                for (int j = 0; j < image.Width; j+=step)
                {
                    var from = new PointF(j, i);
                    var to = new PointF(j + flowY.Data[i, j, 0], i + flowX.Data[i,j,0]);

                    image.Draw(new LineSegment2DF(from, to), new Gray(100), 1);
                }
            }
        }

        private void DrawFlowVectorsForPyrLk(Image<Gray, float> image, PointF[] prev, PointF[] cur)
        {
            for (int i = 0; i < prev.Length; i++)
            {
                image.Draw(new LineSegment2DF(prev[i], cur[i]), new Gray(100), 1);
            }
        }

        private void DrawFlowVectorsInFitures(Image<Gray, float> image, Image<Gray, float> flowX, Image<Gray, float> flowY,
            PointF[]features)
        {
            foreach (var feature in features)
            {
                var from = new PointF(feature.X, feature.Y);
                var to = new PointF(feature.Y + flowY.Data[(int)feature.X, (int)feature.Y, 0] * 10, (int)feature.X + flowX.Data[(int)feature.X, (int)feature.Y, 0] * 10);
                image.Draw(new LineSegment2DF(from, to), new Gray(100), 1);
            }
        }

        private void CalculateAngles(Image<Gray, float> flowX, Image<Gray, float> flowY,
            int step)
        {
            double a = 0;
            double b = 0;
            double c = 0;
            double d = 0;
            double e = 0;
            double f = 0;
            double k = 0;
            double l = 0;
            double m = 0;

            for (int y = 0; y < flowX.Height; y += step)
            {
                for (int x = 0; x < flowX.Width; x += step)
                {
                    var u = flowX.Data[y, x, 0] * 3;
                    var v = flowY.Data[y, x, 0] * 3;

                    a += x * x * y * y + (y * y + 1);
                    b += (x*x + 1) + x*x*y*y;
                    c += x*x + y*y;
                    d += x*y*(x*x + y*y + 2);
                    e += y;
                    f += x;
                    k += u*x*y + v*(y*y + 1);
                    l += u*(x*x + 1) + v*x*y;
                    m += u*y - v*x;
                }
            }

            Matrix matrix33 = DenseMatrix.OfArray(new double[,]{{a,d,f}, {d,b,e}, {f, e, c}});
            Matrix matrix31 = DenseMatrix.OfArray(new double[,]{{k}, {l}, {m}});

            _rotation += matrix33.Inverse() * matrix31;
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            _rotation = DenseMatrix.OfArray(new double[,] { { 0 }, { 0 }, { 0 } });

        }
    }
}