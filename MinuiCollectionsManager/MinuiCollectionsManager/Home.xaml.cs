using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;

namespace MinuiCollectionsManager;

public partial class Home : Page
{
    public Home()
    {
        InitializeComponent();
    }
    
    private void LoadFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string folderPath = dialog.SelectedPath;
            
            NavigationService.Navigate(new CollectionsManager(folderPath));
        }
    }
}