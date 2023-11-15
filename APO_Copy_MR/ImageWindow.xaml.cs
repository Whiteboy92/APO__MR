using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using APO_Copy_MR.Shared;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Win32;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;


namespace APO_Copy_MR
{
    public partial class ImageWindow
    {
        [GeneratedRegex("(\\.[^.]+)$")]
        internal static partial Regex MyRegex();
        internal static HistogramWindow? ActiveHistogramWindow { get; set; }
        internal short Id { get; init; }
        public static Image<Bgr, byte>? ImageInput { get; set; }

        public ImageWindow()
        {
            ActiveHistogramWindow = null;
            InitializeComponent();
    
            var comboBoxes = new[] { ComboBox1, ComboBox2, ComboBox3, ComboBox4, ComboBox5 };
            foreach (var comboBox in comboBoxes)
            {
                comboBox.SelectionChanged += ComboBox_SelectionChanged;
            }
        }

        /// <summary>
        /// Buttons
        /// </summary>
        
        private void BtnDuplicateImage_Click(object sender, RoutedEventArgs e)
        {
            var duplicatedWindow = AppUtility.DuplicateImage(this);
            duplicatedWindow?.Show();
        }

        private void BtnSaveSelectedImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    FileName = Title, // Default file name
                    DefaultExt = ".bmp", // Default file extension
                    Filter = "BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpeg;*.jpg)|*.jpeg;*.jpg|PNG Files (*.png)|*.png|GIF Files (*.gif)|*.gif" // Filter files by extension
                };
                
                var result = dlg.ShowDialog();
                if (result != true) return;
                
                using (var fileStream = new FileStream(dlg.FileName, FileMode.Create))
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((DisplayImage.Source as BitmapSource)!));
                    encoder.Save(fileStream);
                }
                MessageBox.Show("Image saved successfully!", "Save Image", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving image: " + ex.Message, "Save Image", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnConvert2Grey_Click(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }
            ImageInput = ImageProcessing.ConvertToGray(ImageInput);

            DisplayImage.Source = ImageInput.ToBitmapSource();
        }
        
        /// <summary>
        /// AppUtility
        /// </summary>

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppUtility.ResetComboBoxSelections(this);
        }

        /// <summary>
        /// ComboBoxes Selections
        /// </summary>

                private void HistogramStretch_Selected(object sender, RoutedEventArgs e)
        {
            var histogramStretchValuesWindow = new HistogramStretchValuesWindow();
            histogramStretchValuesWindow.Show();
        }

        private void ConvertToLab_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }

            var labChannels = ImageProcessing.ConvertToLab(ImageInput);

            if (labChannels is not { Count: >= 3 }) { return; }

            var imageWindows = new List<ImageWindow>();
            if (imageWindows == null) throw new ArgumentNullException(nameof(imageWindows));

            for (var i = 0; i < labChannels.Count; i++)
            {
                var channelWindow = new ImageWindow
                {
                    Title = $"Channel {i + 1} - {Path.GetFileName(Title)}",
                    DisplayImage = { Source = labChannels[i].ToBitmapSource() },
                    ImageCanvas = { Width = ImageInput.Width, Height = ImageInput.Height }
                };

                imageWindows.Add(channelWindow);
                channelWindow.Show();
            }
        }
        
        private void ConvertToHsv_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }

            var mats = ImageProcessing.SplitChannels(ImageInput);

            if (mats == null || mats.Count < 3) { return; }

            var imageWindows = new List<ImageWindow>();
            if (imageWindows == null) throw new ArgumentNullException(nameof(imageWindows));

            for (var i = 0; i < 3; i++)
            {
                var channelWindow = new ImageWindow
                {
                    Title = $"Channel {i + 1} - {Path.GetFileName(Title)}",
                    DisplayImage = { Source = mats[i].ToBitmapSource() },
                    ImageCanvas = { Width = ImageInput.Width, Height = ImageInput.Height }
                };

                imageWindows.Add(channelWindow);
                channelWindow.Show();
            }
        }
        
        private void DetectEdgesPrewitt_Selected(object sender, RoutedEventArgs e)
        {
            var prewittDirectionsWindow = new PrewittDirectionsWindow();
            prewittDirectionsWindow.Show();
        }
        
        private void Histogram_Selected(object? sender, RoutedEventArgs? routedEventArgs)
        {
            ImageProcessing.Histogram(ImageInput);
        }
        
        private void MedianFiltration_Selected(object sender, RoutedEventArgs e)
        {
            var medianFiltrationScaleWindow = new MedianFiltrationScaleWindow();
            medianFiltrationScaleWindow.Show();
        }

        private void Add_Selected(object sender, RoutedEventArgs e)
        {
            var dualArgumentOperationsWindow = new DualArgumentOperationsWindow(DualArgumentOperationEnum.Add);
            dualArgumentOperationsWindow.Show();
        }
        
        private void Subtract_Selected(object sender, RoutedEventArgs e)
        {
            var dualArgumentOperationsWindow = new DualArgumentOperationsWindow(DualArgumentOperationEnum.Subtract);
            dualArgumentOperationsWindow.Show();
        }
        
        private void Blend_Selected(object sender, RoutedEventArgs e)
        {
            var dualArgumentOperationsWindow = new DualArgumentOperationsWindow(DualArgumentOperationEnum.Blend);
            dualArgumentOperationsWindow.Show();
        }
        
        private void AND_Selected(object sender, RoutedEventArgs e)
        {
            var dualArgumentOperationsWindow = new DualArgumentOperationsWindow(DualArgumentOperationEnum.And);
            dualArgumentOperationsWindow.Show();
        }
        
        private void OR_Selected(object sender, RoutedEventArgs e)
        {
            var dualArgumentOperationsWindow = new DualArgumentOperationsWindow(DualArgumentOperationEnum.Or);
            dualArgumentOperationsWindow.Show();
        }
        
        private void XOR_Selected(object sender, RoutedEventArgs e)
        {
            var dualArgumentOperationsWindow = new DualArgumentOperationsWindow(DualArgumentOperationEnum.Xor);
            dualArgumentOperationsWindow.Show();
        }
        
        private void Negate_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }

            ImageInput = ImageProcessing.Negate(ImageInput);

            DisplayImage.Source = ImageInput.ToBitmapSource();
            ImageProcessing.Histogram(ImageInput);
        }
        
        private void Posterize_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }

            ImageInput = ImageProcessing.Posterize(ImageInput, 8); // Adjust the color levels here

            if (ImageInput == null) { return; }

            DisplayImage.Source = ImageInput.ToBitmapSource();
            ImageProcessing.Histogram(ImageInput);
        }

        private void Erode_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }

            ImageInput = ImageProcessing.Erode(ImageInput, 1); // Adjust the number of iterations here

            DisplayImage.Source = ImageInput.ToBitmapSource();
            ImageProcessing.Histogram(ImageInput);
        }

        private void ConvertToBlackAndWhite_Selected(object sender, RoutedEventArgs e)
        {
            var bwImage = ImageInput?.Convert<Gray, byte>().ThresholdBinary(new Gray(127), new Gray(255));
            
            DisplayImage.Source = bwImage.ToBitmapSource();
            ImageInput = bwImage?.Convert<Bgr, byte>();
            ImageProcessing.Histogram(ImageInput);
        }
        
        private void Dilate_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }
            ImageInput = ImageProcessing.Dilate(ImageInput, 1); // Adjust the number of iterations here

            DisplayImage.Source = ImageInput.ToBitmapSource();
            ImageProcessing.Histogram(ImageInput);
        }

        private void Open_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }

            ImageInput = ImageProcessing.MorphologyEx(ImageInput, MorphOp.Open, new Size(3, 3), new Point(1, 1), 1); // Adjust the parameters here

            if (ImageInput == null) { return; }

            DisplayImage.Source = ImageInput.ToBitmapSource();
            ImageProcessing.Histogram(ImageInput);
        }

        private void Close_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) { return; }

            ImageInput = ImageProcessing.MorphologyEx(ImageInput, MorphOp.Close, new Size(3, 3), new Point(1, 1), 1); // Adjust the parameters here

            if (ImageInput == null) { return; }

            DisplayImage.Source = ImageInput.ToBitmapSource();
            ImageProcessing.Histogram(ImageInput);
        }
        
        private void Skeletonize_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageInput == null) return;

            var imageToSkeletonize = ImageInput.Convert<Gray, byte>().Clone();
            var skeletonizedImage = ImageProcessing.Skeletonize(imageToSkeletonize);

            var imageWindow = new ImageWindow
            {
                Title = "Skeletonized Image",
                DisplayImage =
                {
                    Source = skeletonizedImage.ToBitmapSource(),
                },
                ImageCanvas =
                {
                    Width = ImageInput.Width,
                    Height = ImageInput.Height
                },
            };

            imageWindow.Show();
            imageWindow.DisplayImage = new Image();
            ImageProcessing.Histogram(ImageInput);
        }

        private void LinearSharpening_Selected(object sender, RoutedEventArgs e)
        {
            var proximityMatricesWindow = new ProximityMatricesWindow();
            proximityMatricesWindow.Show();
        }

        private void LinearSmoothing_Selected(object sender, RoutedEventArgs e)
        {
            var imageToBlur = ImageInput?.Clone() ?? throw new InvalidOperationException();
            var linearSmoothing = ImageProcessing.LinearSmoothing(imageToBlur);
            ImageInput = imageToBlur;

            var newImageWindow = new ImageWindow
            {
                Title = "Linear Smoothing",
                DisplayImage =
                {
                    Source = linearSmoothing.ToBitmapSource(),
                },
                ImageCanvas =
                {
                    Width = ImageInput.Width,
                    Height = ImageInput.Height
                },
            };

            newImageWindow.Show();
            newImageWindow.DisplayImage = new Image();
            ImageProcessing.Histogram(ImageInput);
        }

        private void DetectEdgesSobel_Selected(object sender, RoutedEventArgs e)
        {
            var imageToDetect = ImageInput?.Clone() ?? throw new InvalidOperationException();
            
            var detectEdgesSobel = ImageProcessing.DetectEdgesSobel(imageToDetect);
            
            var newImageWindow = new ImageWindow
            {
                Title = "Sobel Edges",
                DisplayImage =
                {
                    Source = detectEdgesSobel.ToBitmapSource(),
                },
                ImageCanvas =
                {
                    Width = ImageInput.Width,
                    Height = ImageInput.Height
                },
            };
            
            ImageInput = detectEdgesSobel.Convert<Bgr, byte>();
            newImageWindow.Show();
            newImageWindow.DisplayImage = new Image();
            ImageProcessing.Histogram(ImageInput);
        }
        
        private void DetectEdgesLaplacian_Selected(object sender, RoutedEventArgs e)
        {
            var imageToDetect = ImageInput?.Clone() ?? throw new InvalidOperationException();
            
            var detectEdgesLaplacian = ImageProcessing.DetectEdgesLaplacian(imageToDetect);

            var newImageWindow = new ImageWindow
            {
                Title = "Laplacian Edges",
                DisplayImage =
                {
                    Source = detectEdgesLaplacian.ToBitmapSource(),
                },
                ImageCanvas =
                {
                    Width = ImageInput.Width,
                    Height = ImageInput.Height
                },
            };

            ImageInput = detectEdgesLaplacian.Convert<Bgr, byte>();
            newImageWindow.Show();
            newImageWindow.DisplayImage = new Image();
            ImageProcessing.Histogram(ImageInput);
        }
        
        private void DetectEdgesCanny_Selected(object sender, RoutedEventArgs e)
        {
            var imageToDetect = ImageInput?.Clone() ?? throw new InvalidOperationException();
            
            var detectEdgesCanny = ImageProcessing.DetectEdgesCanny(imageToDetect);

            var newImageWindow = new ImageWindow
            {
                Title = "Canny Edges",
                DisplayImage =
                {
                    Source = detectEdgesCanny.ToBitmapSource(),
                },
                ImageCanvas =
                {
                    Width = ImageInput.Width,
                    Height = ImageInput.Height
                },
            };

            ImageInput = detectEdgesCanny.Convert<Bgr, byte>();
            newImageWindow.Show();
            newImageWindow.DisplayImage = new Image();
            ImageProcessing.Histogram(ImageInput);
        }
    }
}