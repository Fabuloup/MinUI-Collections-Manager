namespace MinuiCollectionsManager.Models;

public class Collection
{
    public string Name { get; set; }
    
    public Collection(string name)
    {
        Name = name;
    }
    
    #region Navigation
    
    public List<Game> Games { get; set; } = new();
    
    #endregion
}