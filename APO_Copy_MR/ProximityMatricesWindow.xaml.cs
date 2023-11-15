using System.Windows;
using System.Windows.Controls;
using APO_Copy_MR.Shared;
using Emgu.CV;
using Emgu.CV.Structure;
namespace APO_Copy_MR
{
    public partial class ProximityMatricesWindow
    {
        public ProximityMatricesWindow()
        {
            InitializeComponent();
            AddRadioButtonEventHandlers();
        }

        private void AddRadioButtonEventHandlers()
        {
            RadioButton1.Checked += RadioButton_Checked;
            RadioButton2.Checked += RadioButton_Checked;
            RadioButton3.Checked += RadioButton_Checked;
            RadioButton4.Checked += RadioButton_Checked;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton clickedRadioButton = (RadioButton)sender;
            
            foreach (StackPanel stackPanel in MainGrid.Children.OfType<StackPanel>())
            {
                foreach (RadioButton radioButton in stackPanel.Children.OfType<RadioButton>().Where(r => r != clickedRadioButton))
                {
                    radioButton.IsChecked = false;
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length <= 3) return;
            textBox.Text = textBox.Text.Substring(0, 3);
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void BtnDoLinearSharpening_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedMatrixNumber() == 0)
            {
                MessageBox.Show("Please select a matrix.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int[] matrix = GetTextBoxValues();

                if (ImageWindow.ImageInput != null)
                {
                    Image<Bgr, byte> image = ImageProcessing.LinearSharpening(ImageWindow.ImageInput, matrix);

                    ImageWindow newImageWindow = new ImageWindow
                    {
                        DisplayImage =
                        {
                            Source = image.ToBitmapSource(),
                        },
                    };

                    ImageWindow.ImageInput = image;
                    newImageWindow.Show();
                    newImageWindow.DisplayImage = new Image();
                }
                else
                {
                    throw new InvalidOperationException("Invalid input format. Please enter valid numbers for min and max values.");
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid input format. Please enter valid numbers for min and max values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Close();
        }
        
        private TextBox FindTextBox(int index)
        {
            int matrixNumber = GetSelectedMatrixNumber();
            string textBoxName = $"Tb{index + 1}Matrix{matrixNumber}";
            TextBox textBox = ((TextBox)FindName(textBoxName)!);

            if (textBox == null) { throw new ArgumentException($"TextBox {textBoxName} not found."); }

            return textBox;
        }

        private int[] GetTextBoxValues()
        {
            int[] values = new int[9];

            try
            {
                for (int index = 0; index < 9; index++)
                {
                    TextBox textBox = FindTextBox(index);
                    if (int.TryParse(textBox.Text, out int value))
                    {
                        values[index] = value;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return values;
        }
        
        private int GetSelectedMatrixNumber()
        {
            if (RadioButton1.IsChecked == true) { return 1; }
            if (RadioButton2.IsChecked == true) { return 2; }
            if (RadioButton3.IsChecked == true) { return 3; }
            return RadioButton4.IsChecked == true ? 4 : 0;
        }
    }
}