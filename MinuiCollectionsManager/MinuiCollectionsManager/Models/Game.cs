namespace MinuiCollectionsManager.Models;

public class Game
{
    private string _realName;
    public string RealName
    {
        get => _realName;
        set
        {
            _realName = value;
            DisplayName = System.Text.RegularExpressions.Regex.Match(_realName, @"(.+?)(?:\s?\([^\)]+\))*\.\w+$").Groups[1].Value;
        }
    }
    
    public string DisplayName { get; private set; }
    public string DisplayNameWithParent => $"{ParentFolder}/{DisplayName}";
    
    public bool IsInError { get; set; } = false;
    
    public Game(string realName, Folder? parentFolder = null)
    {
        RealName = realName;
        if (parentFolder != null)
        {
            ParentFolder = parentFolder;
        }
    }
    
    #region Navigation
    public Folder ParentFolder { get; set; }
    #endregion

    public string GetPath()
    {
        return $"{ParentFolder.GetPath()}/{RealName}";
    }

    public override string ToString()
    {
        return DisplayName;
    }
}