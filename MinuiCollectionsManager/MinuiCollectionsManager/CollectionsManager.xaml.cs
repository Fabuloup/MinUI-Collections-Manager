using System.Windows.Controls;

namespace MinuiCollectionsManager;

public partial class CollectionsManager : Page
{
    private string _folderPath;
    public CollectionsManager(string folderPath)
    {
        InitializeComponent();
        _folderPath = folderPath;
    }
}