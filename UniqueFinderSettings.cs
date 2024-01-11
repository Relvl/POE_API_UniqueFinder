using System.Diagnostics.CodeAnalysis;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace UniqueFinder;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class UniqueFinderSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new(false);

    public RangeNode<int> UpdateTimer { get; set; } = new(250, 0, 5000);

    public ToggleNode EnablePanelDrawing { get; set; } = new(true);
    public ToggleNode BlinkPanelDrawing { get; set; } = new(false);
    public RangeNode<float> TextSize { get; set; } = new(2f, 1f, 20f);

    public ToggleNode EnableMapDrawing { get; set; } = new(true);
    public ToggleNode BlinkMapDrawing { get; set; } = new(false);
    public ToggleNode EnableItemOutline { get; set; } = new(true);
    public ColorNode MapLineColor { get; set; } = new(new Color(214, 0, 255, 255));

    public RangeNode<float> MapLineThickness { get; set; } = new(3f, 1f, 10f);
    
    public RangeNode<int> BlinkFrequency { get; set; } = new(250, 50, 2000);


    public List<string> UniqueNames = [];
    public bool Initialized = false;
}