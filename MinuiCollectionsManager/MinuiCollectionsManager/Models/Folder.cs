namespace MinuiCollectionsManager.Models;

public class Folder
{
    public int? Order { get; private set; }
    
    private string _realName;
    public string RealName
    {
        get => _realName;
        set
        {
            _realName = value;
            var match = System.Text.RegularExpressions.Regex.Match(_realName,
                @"^(?:(\d+)\)\s)?(.+?)(\.disabled)?$");
            Order = int.TryParse(match.Groups[1].Value, out var order) ? order : (int?)null;
            DisplayName = match.Groups[2].Value;
            IsHidden = !string.IsNullOrEmpty(match.Groups[3].Value);
        }
    }

    public bool IsHidden { get; private set; } = false;
    
    public string DisplayName { get; private set; }
    
    public Folder(string realName, Folder? parentFolder = null)
    {
        RealName = realName;
        if (parentFolder != null)
        {
            ParentFolder = parentFolder;
        }
    }
    
    #region Navigation
    public Folder? ParentFolder { get; set; }
    #endregion

    public string GetPath()
    {
        return $"{(ParentFolder == null ? "" : $"{ParentFolder.GetPath()}/")}{RealName}";
    }

    public override string ToString()
    {
        return DisplayName;
    }
}