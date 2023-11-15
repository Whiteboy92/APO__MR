using System.ComponentModel;
using System.IO;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using Window = System.Windows.Window;

namespace APO_Copy_MR
{
    public partial class MainWindow
    {
        private static short _duplicationCounter;
        private static int Id { get; set; }
        internal static Image<Bgr, byte>? ImageInput { get; set; }

        internal static readonly Dictionary<int, short> DuplicationDictionary = new();

        public MainWindow()
        {
            _duplicationCounter = 0;
            InitializeComponent();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            var infoWindow = new InfoWindow(this)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            infoWindow.ShowDialog();
        }
        
        private void BtnOpenNewFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png"
            };

            if (openFileDialog.ShowDialog() != true) return;
            ImageInput = new Image<Bgr, byte>(openFileDialog.FileName);

            int maxIdValue = DuplicationDictionary.Keys.Any() ? DuplicationDictionary.Keys.Max() : 0;
            Id = (short)(maxIdValue + 1);

            DuplicationDictionary[Id] = _duplicationCounter;
            _duplicationCounter += 1;
            
            string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            string extension = Path.GetExtension(openFileDialog.FileName);

            var imageWindow = new ImageWindow
            {
                Title = $"{fileName} ({DuplicationDictionary[Id]}){extension}",
                DisplayImage =
                {
                    Source = ImageInput.ToBitmapSource(),
                    Width = ImageInput.Width,
                    Height = ImageInput.Height
                },
                ImageCanvas =
                {
                    Width = ImageInput.Width,
                    Height = ImageInput.Height
                }
            };

            if (ImageInput.Height > 1000 || ImageInput.Width > 1000)
            {
                imageWindow.Height = 1000;
                imageWindow.Width = 1000;
            }
            else
            {
                imageWindow.Height = ImageInput.Height + 200;
                imageWindow.Width = ImageInput.Width + 50;
            }

            ImageWindow.ImageInput = ImageInput;
            imageWindow.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (var window in Application.Current.Windows.OfType<Window>().Where(window => window != this))
            {
                window.Close();
            }

            base.OnClosing(e);
        }
    }
}
