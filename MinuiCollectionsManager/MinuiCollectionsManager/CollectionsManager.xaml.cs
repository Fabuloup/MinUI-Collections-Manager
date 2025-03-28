using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MinuiCollectionsManager.Models;
using Button = System.Windows.Controls.Button;
using DataObject = System.Windows.Forms.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace MinuiCollectionsManager;

public partial class CollectionsManager : Page
{
    private string _folderPath;
    private List<Game> _games = new();
    private List<Collection> _collections = new();
    private Point _dragStartPoint;
    public CollectionsManager(string folderPath)
    {
        _folderPath = folderPath;
        InitializeComponent();
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        _games.Clear();
        _collections.Clear();
        var romsFolderPath = Path.Combine(_folderPath, "Roms");
        if (!Directory.Exists(romsFolderPath))
        {
            MessageBox.Show("The folder 'Roms' does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        await LoadGamesRecursively(romsFolderPath, new Folder("Roms"));
        GamesList.ItemsSource = null;
        GamesList.ItemsSource = _games;
        _ = Task.Run(LoadCollections) ;
    }

    private async Task LoadGamesRecursively(string parentFolderPath, Folder parentFolder)
    {
        List<Task> tasks = new();
        var directories = Directory.GetDirectories(parentFolderPath);
        foreach (var directory in directories)
        {
            var folderName = Path.GetFileName(directory);
            var childFolder = new Folder(folderName, parentFolder);
            tasks.Add(LoadGamesRecursively(directory, childFolder));
            
        }
        
        var files = Directory.GetFiles(parentFolderPath)
            .Where(file => !System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(file), @"^((\..+)|([^\.]+)|(.+\.txt))$"));
        foreach (var file in files)
        {
            var game = new Game(Path.GetFileName(file), parentFolder);
            _games.Add(game);
        }
        
        await Task.WhenAll(tasks);
    }

    private void LoadCollections()
    {
        if (!Directory.Exists(Path.Combine(_folderPath, "Collections")))
        {
            Directory.CreateDirectory(Path.Combine(_folderPath, "Collections"));
        }
        var txtFiles = Directory.GetFiles(Path.Combine(_folderPath, "Collections"), "*.txt");
        foreach (var txtFile in txtFiles)
        {
            var collectionName = Path.GetFileNameWithoutExtension(txtFile);
            var collection = new Collection(collectionName);
            
            var lines = File.ReadAllLines(txtFile);
            foreach (var line in lines)
            {
                var fileName = Path.GetFileName(line);
                var existingGame = _games.FirstOrDefault(game => game.RealName == fileName);
                if (existingGame != null)
                {
                    collection.Games.Add(existingGame);
                }
                else
                {
                    var tmpParentFolder = new Folder(Path.GetFileName(Path.GetDirectoryName(line)) ?? string.Empty);
                    var parentFolder = _games.Select(g => g.ParentFolder).FirstOrDefault(f => f.DisplayName == tmpParentFolder.DisplayName);

                    if (parentFolder != null)
                    {
                        collection.Games.Add(new Game(fileName, parentFolder)
                        {
                            IsInError = true
                        });
                    }
                }
            }
            _collections.Add(collection);
        }
        CollectionsComboBox.Dispatcher.Invoke(() => CollectionsComboBox.ItemsSource = null);
        CollectionsComboBox.Dispatcher.Invoke(() => CollectionsComboBox.ItemsSource = _collections);
        CollectionsComboBox.Dispatcher.Invoke(() => CollectionsComboBox.SelectedIndex = 0);
    }
    
    #region Actions
    private void LoadFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _folderPath = dialog.SelectedPath;
            _ = InitializeAsync();
        }
    }
    
    private void RefreshCollections_Click(object sender, RoutedEventArgs e)
    {
        _ = InitializeAsync();
    }
    
    private void SaveCollections_Click(object sender, RoutedEventArgs e)
    {
        string collectionsFolderPath = Path.Combine(_folderPath, "Collections");
        if (!Directory.Exists(collectionsFolderPath))
        {
            Directory.CreateDirectory(collectionsFolderPath);
        }

        foreach (var collection in _collections)
        {
            string collectionFilePath = Path.Combine(collectionsFolderPath, $"{collection.Name}.txt");
            using (StreamWriter writer = new StreamWriter(collectionFilePath))
            {
                collection.Games = collection.Games.Where(g => !g.IsInError).ToList();
                foreach (var game in collection.Games)
                {
                    writer.WriteLine($"/{game.GetPath()}");
                }
            }
        }
        
        var existingCollectionFiles = Directory.GetFiles(collectionsFolderPath, "*.txt");
        foreach (var existingFile in existingCollectionFiles)
        {
            var existingCollectionName = Path.GetFileNameWithoutExtension(existingFile);
            if (!_collections.Any(c => c.Name == existingCollectionName))
            {
                File.Delete(existingFile);
            }
        }

        _ = InitializeAsync();
        MessageBox.Show("Collections saved successfully.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private void CollectionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedCollection = CollectionsComboBox.SelectedItem as Collection;
        if (selectedCollection != null)
        {
            CollectionGamesList.ItemsSource = null;
            CollectionGamesList.ItemsSource = selectedCollection.Games;
        }
        else
        {
            CollectionGamesList.ItemsSource = null;
        }
    }
    
    private void AddCollection_Click(object sender, RoutedEventArgs e)
    {
        var inputDialog = new InputDialog("Enter collection name:");
        if (inputDialog.ShowDialog() == true)
        {
            var collectionName = inputDialog.ResponseText;
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                var newCollection = new Collection(collectionName);
                _collections.Add(newCollection);
                CollectionsComboBox.ItemsSource = null;
                CollectionsComboBox.ItemsSource = _collections;
                CollectionsComboBox.SelectedItem = newCollection;
            }
        }
    }

    private void DeleteCollection_Click(object sender, RoutedEventArgs e)
    {
        var selectedCollection = CollectionsComboBox.SelectedItem as Collection;
        if (selectedCollection != null)
        {
            _collections.Remove(selectedCollection);
            CollectionsComboBox.ItemsSource = null;
            CollectionsComboBox.ItemsSource = _collections;
            CollectionsComboBox.SelectedIndex = 0;
        }
    }
    
    private void GamesList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void GamesList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        Point mousePos = e.GetPosition(null);
        Vector diff = _dragStartPoint - mousePos;

        if (e.LeftButton == MouseButtonState.Pressed &&
            (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
             Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
            ListBox listBox = sender as ListBox;
            ListBoxItem listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

            if (listBoxItem != null)
            {
                Game game = (Game)listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);
                DataObject dragData = new DataObject("GameName", game.RealName);
                DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
            }
        }
    }
    
    private void CollectionGamesList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void CollectionGamesList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        Point mousePos = e.GetPosition(null);
        Vector diff = _dragStartPoint - mousePos;

        if (e.LeftButton == MouseButtonState.Pressed &&
            (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
             Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
            ListBox listBox = sender as ListBox;
            ListBoxItem listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

            if (listBoxItem != null)
            {
                Game game = (Game)listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);
                DataObject dragData = new DataObject("GameName", game.RealName);
                DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
            }
        }
    }

    private void CollectionGamesList_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("GameName"))
        {
            e.Effects = DragDropEffects.None;
        }
        else
        {
            e.Effects = DragDropEffects.Move;
        }
        e.Handled = true;
    }

    private void CollectionGamesList_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("GameName"))
        {
            string gameName = e.Data.GetData("GameName") as string;
            var selectedCollection = CollectionsComboBox.SelectedItem as Collection;
            if (gameName != null && selectedCollection != null)
            {
                Game game = _games.FirstOrDefault(g => g.RealName == gameName);
                if (game != null)
                {
                    
                    Point dropPosition = e.GetPosition(CollectionGamesList);
                    int dropIndex = GetCurrentIndex(dropPosition);

                    if (selectedCollection.Games.Contains(game))
                    {
                        selectedCollection.Games.Remove(game);
                    }
                    
                    if (dropIndex >= 0 && dropIndex <= selectedCollection.Games.Count)
                    {
                        selectedCollection.Games.Insert(dropIndex, game);
                    }
                    else
                    {
                        selectedCollection.Games.Add(game);
                    }
                    CollectionGamesList.ItemsSource = null;
                    CollectionGamesList.ItemsSource = selectedCollection.Games;
                }
            }
        }
        e.Handled = true;
    }

    private void DeleteGame_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var game = button?.Tag as Game;
        var selectedCollection = CollectionsComboBox.SelectedItem as Collection;
    
        if (game != null && selectedCollection != null)
        {
            selectedCollection.Games.Remove(game);
            CollectionGamesList.ItemsSource = null;
            CollectionGamesList.ItemsSource = selectedCollection.Games;
        }
    }
    #endregion
    
    private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null && !(current is T))
        {
            current = VisualTreeHelper.GetParent(current);
        }
        return current as T;
    }
    
    private int GetCurrentIndex(Point dropPosition)
    {
        int index = -1;
        for (int i = 0; i < CollectionGamesList.Items.Count; i++)
        {
            ListBoxItem item = (ListBoxItem)CollectionGamesList.ItemContainerGenerator.ContainerFromIndex(i);
            if (item != null)
            {
                Point itemPosition = item.TransformToAncestor(CollectionGamesList).Transform(new Point(0, 0));
                if (dropPosition.Y < itemPosition.Y + item.ActualHeight)
                {
                    index = i;
                    break;
                }
            }
        }
        return index;
    }
}