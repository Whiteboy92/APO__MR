using System.Windows;
using APO_Copy_MR.Shared;
using Emgu.CV;
using Emgu.CV.Structure;
namespace APO_Copy_MR;

public sealed partial class DualArgumentOperationsWindow
{
    private readonly DualArgumentOperationEnum operationEnum;
    
    public DualArgumentOperationsWindow(DualArgumentOperationEnum operationEnum)
    {
        this.operationEnum = operationEnum;
        InitializeComponent();
        AddOpenedImageWindowsToComboBoxes();
        SetWindowTitle();
    }
    
    private void AddOpenedImageWindowsToComboBoxes()
    {
        // Clear the ComboBox items
        ComboBoxTop.Items.Clear();
        ComboBoxBottom.Items.Clear();

        // Get all open windows of type ImageWindow
        var imageWindows = Application.Current.Windows.OfType<ImageWindow>();

        // Add the window titles to the ComboBox items
        foreach (var window in imageWindows)
        {
            ComboBoxTop.Items.Add(window.Title);
            ComboBoxBottom.Items.Add(window.Title);
        }
    }

    private void BtnApplyOperation_Click(object sender, RoutedEventArgs e)
    {
        PerformDualArgumentOperation(operationEnum);
    }
    
    private void PerformDualArgumentOperation(DualArgumentOperationEnum dualArgumentOperationEnum)
    {
        if (ComboBoxTop.SelectedItem == null || ComboBoxBottom.SelectedItem == null) return;
    
        string? selectedTopWindow = ComboBoxTop.SelectedItem.ToString();
        string? selectedBottomWindow = ComboBoxBottom.SelectedItem.ToString();

        // Find the ImageWindow by title
        ImageWindow? topImageWindow = Application.Current.Windows.OfType<ImageWindow>().FirstOrDefault(window => window.Title == selectedTopWindow);
        ImageWindow? bottomImageWindow = Application.Current.Windows.OfType<ImageWindow>().FirstOrDefault(window => window.Title == selectedBottomWindow);

        // Check if the ImageWindow is found and get the ImageInput
        if (topImageWindow == null || bottomImageWindow == null) return;
    
        Image<Bgr, byte>? topImage = ImageWindow.ImageInput;
        Image<Bgr, byte>? bottomImage = ImageWindow.ImageInput;


        // Perform the selected operation based on the enum value
        switch (dualArgumentOperationEnum)
        {
            case DualArgumentOperationEnum.Add:
                if (topImage != null)
                {
                    Image<Bgr, byte> resultAdd = topImage.Add(bottomImage);
                    DisplayImageResult(resultAdd);
                }
                break;
            
            case DualArgumentOperationEnum.Subtract:
                if (topImage != null)
                {
                    Image<Bgr, byte> resultSubtract = new Image<Bgr, byte>(topImage.Size);
                    CvInvoke.Subtract(topImage, bottomImage, resultSubtract);
                    DisplayImageResult(resultSubtract);
                }
                break;
            
            case DualArgumentOperationEnum.Blend:
                double alpha = 0.5; // Adjust the blending factor as needed
                if (topImage != null)
                {
                    Image<Bgr, byte> resultBlend = topImage.AddWeighted(bottomImage, alpha, 1 - alpha, 0);
                    DisplayImageResult(resultBlend);
                }
                break;
            
            case DualArgumentOperationEnum.And:
                if (topImage != null)
                {
                    Image<Bgr, byte> resultAnd = topImage.And(bottomImage);
                    DisplayImageResult(resultAnd);
                }
                break;
            
            case DualArgumentOperationEnum.Or:
                if (topImage != null)
                {
                    Image<Bgr, byte> resultOr = topImage.Or(bottomImage);
                    DisplayImageResult(resultOr);
                }
                break;
            
            case DualArgumentOperationEnum.Xor:
                if (topImage != null)
                {
                    Image<Bgr, byte> resultXor = topImage.Xor(bottomImage);
                    DisplayImageResult(resultXor);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dualArgumentOperationEnum), dualArgumentOperationEnum, null);
        }
    }

    private void DisplayImageResult(Image<Bgr, byte> resultImage)
    {
        double width = resultImage.Width;
        double height = resultImage.Height;
        
        var imageWindow = new ImageWindow
        {
            DisplayImage =
            {
                Width = width,
                Height = height,
                Source = resultImage.ToBitmapSource(),
            },
            ImageCanvas =
            {
                // Set the Canvas and Image sizes to match the pixel width and height of the uploaded image
                Width = resultImage.Width,
                Height = resultImage.Height
            },
        };
        imageWindow.Show();
    }
    
    private void SetWindowTitle()
    {
        Title = operationEnum switch
        {
            DualArgumentOperationEnum.Add => "Add Selected",
            DualArgumentOperationEnum.Subtract => "Subtract Selected",
            DualArgumentOperationEnum.Blend => "Blend Selected",
            DualArgumentOperationEnum.And => "AND Selected",
            DualArgumentOperationEnum.Or => "OR Selected",
            DualArgumentOperationEnum.Xor => "XOR Selected",
            _ => "Dual Argument Operation"
        };
    }

}