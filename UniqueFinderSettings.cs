using System.Diagnostics.CodeAnalysis;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace UniqueFinder;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class UniqueFinderSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new(false);

    [Menu("Common settings")]
    public Common Common { get; set; } = new();

    public LargeMap LargeMap { get; set; } = new();
    public Panel Panel { get; set; } = new();
    public Label Label { get; set; } = new();

    public bool Initialized = false;

    public List<string> UniqueNames = [];
}

[Submenu(CollapsedByDefault = true)]
public class Common
{
    public RangeNode<int> UpdateTime { get; set; } = new(250, 10, 1000);
    public RangeNode<int> BlinkTime { get; set; } = new(250, 10, 1000);
    public ToggleNode HideIdentified { get; set; } = new(true);
}

[Submenu(CollapsedByDefault = true)]
public class LargeMap
{
    public ToggleNode Trace { get; set; } = new(true);
    public ToggleNode Blink { get; set; } = new(true);
    public ColorNode Color { get; set; } = new(new Color(214, 0, 255, 255));
    public RangeNode<float> Thickness { get; set; } = new(3f, 1f, 10f);
}

[Submenu(CollapsedByDefault = true)]
public class Panel
{
    public ToggleNode Enabled { get; set; } = new(true);
    public ToggleNode Blink { get; set; } = new(true);
    public RangeNode<int> TextSize { get; set; } = new(2, 1, 20);
    public RangeNode<int> Margin { get; set; } = new(20, 0, 500);
    public ListNode PanelAlign { get; set; } = new() { Values = Enum.GetNames(typeof(EPanelAlign)).ToList(), Value = EPanelAlign.Right.ToString() };
}

[Submenu(CollapsedByDefault = true)]
public class Label
{
    public ToggleNode Outline { get; set; } = new(true);
    public ToggleNode Blink { get; set; } = new(true);
    public ColorNode FrameColor { get; set; } = new(Color.Wheat);
}