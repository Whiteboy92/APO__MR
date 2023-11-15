using System.Windows;
using System.Windows.Controls;
using APO_Copy_MR.Shared;
using Emgu.CV;
using Emgu.CV.Structure;
namespace APO_Copy_MR;

public partial class PrewittDirectionsWindow
{
    public PrewittDirectionsWindow()
    {
        InitializeComponent();
    }
    
    private void BtnDoPrewittEdgeDetection_Click(object sender, RoutedEventArgs e)
    {
        bool anyRadioButtonSelected = false;
        for (int i = 1; i <= 8; i++)
        {
            RadioButton? radioButton = FindName($"RadioButton{i}") as RadioButton;
            if (radioButton?.IsChecked != true) continue;
            anyRadioButtonSelected = true;
            break;
        }

        if (!anyRadioButtonSelected)
        {
            MessageBox.Show("Please select a matrix.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string direction = GetSelectedDirection();
        Image<Gray, byte> imageInput = ImageWindow.ImageInput?.Convert<Gray, byte>().Clone() ?? throw new InvalidOperationException();
        {
            Image<Gray, byte> image = ImageProcessing.PerformPrewittEdgeDetection(imageInput, direction);

            ImageWindow newImageWindow = new ImageWindow
            {
                DisplayImage = { Source = image.ToBitmapSource(), },
            };
            
            newImageWindow.Show();
            newImageWindow.DisplayImage = new Image();
        }
    }

    private string GetSelectedDirection()
    {
        if (RadioButton1.IsChecked == true)
        {
            return "Top-Left";
        }
        if (RadioButton2.IsChecked == true)
        {
            return "Top";
        }
        if (RadioButton3.IsChecked == true)
        {
            return "Top-Right";
        }
        if (RadioButton4.IsChecked == true)
        {
            return "Left";
        }
        if (RadioButton5.IsChecked == true)
        {
            return "Right";
        }
        if (RadioButton6.IsChecked == true)
        {
            return "Bottom-Left";
        }
        if (RadioButton7.IsChecked == true)
        {
            return "Bottom";
        }
        return RadioButton8.IsChecked == true ? "Bottom-Right" : string.Empty;
    }
}