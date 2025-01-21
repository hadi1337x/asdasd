using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Item", menuName = "Objects/Item")]
public class Item : ScriptableObject
{
    public int id;
    public new string name;
    public string description;
    public TileBase tile;

    public bool isForeground;
    public bool isBackground;
    public bool canWalkThrough;
    public bool isPlatform;

    public bool CanStandOn()
    {
        return isPlatform;
    }
}
