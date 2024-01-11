using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Helpers;
using SharpDX;
using Point = System.Drawing.Point;
using Vector2 = System.Numerics.Vector2;

namespace UniqueFinder;

public class GroundItemInstance(LabelOnGround labelOnGround, WorldItem worldItem, Mods mods, RenderItem renderItem, string itemName, GameController gc)
{
    public Entity WorldEntity => labelOnGround.ItemOnGround;
    public Vector2 Location => WorldEntity.GridPosNum;
    public float Distance => gc?.Player?.GridPosNum.Distance(Location) ?? float.MaxValue;

    public ColorBGRA TextColor => labelOnGround.Label.TextColor;
    public ColorBGRA BorderColor => labelOnGround.Label.BordColor;
    public ColorBGRA BackgroundColor => labelOnGround.Label.BgColor;

    public string ItemName => itemName;

    public Element Label => labelOnGround.Label;
}