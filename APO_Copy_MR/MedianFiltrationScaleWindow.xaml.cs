using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Structure;
namespace APO_Copy_MR;

public sealed partial class MedianFiltrationScaleWindow
{
    public MedianFiltrationScaleWindow()
    {
        InitializeComponent();
    }
    
    private void BtnPerformMedianFiltration(object sender, RoutedEventArgs e)
    {
        if (RadioButton3.IsChecked == false &&
            RadioButton5.IsChecked == false &&
            RadioButton7.IsChecked == false)
        {
            MessageBox.Show("Please select a matrix.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string? kernel = GetSelectedRadioButtonContent();
        
        ApplyMedianFiltration(kernel);
    }
    
    private string? GetSelectedRadioButtonContent()
    {
        if (RadioButton3.IsChecked == true)
        {
            return RadioButton3.Content.ToString();
        }
        if (RadioButton5.IsChecked == true)
        {
            return RadioButton5.Content.ToString();
        }
        return RadioButton7.IsChecked == true ? RadioButton7.Content.ToString() : string.Empty; // No radio button selected

    }

    private void ApplyMedianFiltration(string? selectedKernelSize)
    {
        // Determine the kernel size based on the selected radio button content
        int kernelSize = selectedKernelSize switch
        {
            "3x3" => 3,
            "5x5" => 5,
            "7x7" => 7,
            _ => 0
        };

        Image<Gray, byte> inputImage = ImageWindow.ImageInput?.Convert<Gray, byte>().Clone() ?? throw new InvalidOperationException();
        {
            // Apply median filtration using Emgu.CV
            Image<Gray, byte> image = inputImage.SmoothMedian(kernelSize);

            var newImageWindow = new ImageWindow
            {
                DisplayImage =
                {
                    Source = image.ToBitmapSource(),
                },
            };
            
            newImageWindow.Show();
            newImageWindow.DisplayImage = new Image();
        }
    }

}