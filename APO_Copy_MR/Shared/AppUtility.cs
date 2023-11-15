using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using static APO_Copy_MR.MainWindow;
namespace APO_Copy_MR.Shared;

public static class AppUtility
{
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null)
            yield break;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T comboBoxItem)
                yield return comboBoxItem;

            foreach (var grandchild in FindVisualChildren<T>(child))
                yield return grandchild;
        }
    }

    public static void ResetComboBoxSelections(DependencyObject parent)
    {
        foreach (var control in FindVisualChildren<ComboBox>(parent))
        {
            control.SelectedIndex = 0;
        }
    }
    
    public static ImageWindow? DuplicateImage(ImageWindow sourceWindow)
    {
        if (ImageWindow.ImageInput == null) { return null; }

        ImageInput = ImageWindow.ImageInput;
        short duplicationCounter = (short)(DuplicationDictionary.TryGetValue(sourceWindow.Id, out var value) ? value + 1 : 1);

        var maxIdValue = DuplicationDictionary.Keys.Any() ? DuplicationDictionary.Keys.Max() : 0;
        short newId = (short)(maxIdValue + 1);
        string newTitle = GetDuplicatedImageTitle(sourceWindow.Title, duplicationCounter);
        var newImageInput = ImageInput.Clone();
        var newDisplayImage = new Image<Bgr, byte>(newImageInput.Width, newImageInput.Height);
        newImageInput.CopyTo(newDisplayImage);

        var imageWindow = new ImageWindow
        {
            Width = sourceWindow.Width,
            Height = sourceWindow.Height,
            Title = newTitle,
            Id = newId,
            DisplayImage =
            {
                Source = newDisplayImage.ToBitmapSource(),
                Width = newImageInput.Width,
                Height = newImageInput.Height
            },
            ImageCanvas =
            {
                Width = newImageInput.Width,
                Height = newImageInput.Height
            },
        };

        DuplicationDictionary[imageWindow.Id] = duplicationCounter;
        return imageWindow;
    }


    private static string GetDuplicatedImageTitle(string originalTitle, int duplicationCounter)
    {
        var regex = ImageWindow.MyRegex();
        var extensionMatch = regex.Match(originalTitle);
        var extension = extensionMatch.Success ? extensionMatch.Groups[1].Value : "";

        var regex2 = new Regex($@"\((\d+)\){extension}$");
        var match = regex2.Match(originalTitle);

        string duplicatedTitle;
        if (match.Success)
        {
                duplicationCounter = int.Parse(match.Groups[1].Value) + 1;
                duplicatedTitle = regex2.Replace(originalTitle, $"({duplicationCounter}){extension}");
        }
        else
        {
            duplicatedTitle = regex.Replace(originalTitle, $"({duplicationCounter}){extension}");
        }

        return duplicatedTitle;
    }     
}
