using System.Windows;
using System.Windows.Controls;
using APO_Copy_MR.Shared;
using Emgu.CV;
using Emgu.CV.Structure;
namespace APO_Copy_MR
{
    public partial class HistogramStretchValuesWindow
    {
        public HistogramStretchValuesWindow()
        {
            InitializeComponent();
        }

        private void BtnStretch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(MinValue.Text, out int minVal) && int.TryParse(MaxValue.Text, out int maxVal))
                {
                    if (minVal >= 0 && maxVal <= 255 && minVal < maxVal)
                    {
                        Image<Bgr, byte>? stretchedImage = ImageProcessing.HistogramStretchWithRange(ImageWindow.ImageInput, minVal, maxVal);

                        var hsvImageWindow = new ImageWindow
                        {
                            DisplayImage = 
                            {
                                Source = stretchedImage.ToBitmapSource(),
                            },
                            Title = $"Stretched Histogram {minVal} - {maxVal}" + System.IO.Path.GetFileName(Title)
                        };
                        
                        ImageWindow.ImageInput?.Dispose();
                        ImageWindow.ImageInput = stretchedImage;
                        
                        hsvImageWindow.Show();
                        hsvImageWindow.DisplayImage = new Image();
                        
                        ImageProcessing.Histogram(ImageWindow.ImageInput);
                    }
                    else
                    {
                        MessageBox.Show("Invalid input values. Please enter valid numbers for min and max values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid input format. Please enter valid numbers for min and max values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
            Close();
        }
    }
}
