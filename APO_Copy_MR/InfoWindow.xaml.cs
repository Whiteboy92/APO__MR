using System.Windows;
namespace APO_Copy_MR;

public partial class InfoWindow
{
    public InfoWindow()
    {
        InitializeComponent();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    public InfoWindow(Window owner)
    {
        InitializeComponent();
        
        Owner = owner;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}