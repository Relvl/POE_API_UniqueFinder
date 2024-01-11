using System.Diagnostics;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace UniqueFinder;

public class UniqueFinder : BaseSettingsPlugin<UniqueFinderSettings>
{
    private const string CustomUniqueArtMappingPath = "uniqueArtMapping.json";

    private Dictionary<string, List<string>> _mapping = new();
    private readonly Stopwatch _timer = Stopwatch.StartNew();
    private readonly Stopwatch _blinkTimer = Stopwatch.StartNew();
    private HashSet<GroundItemInstance> _filteredLabelsOnGround = [];
    private readonly Vector2 _borederOffset = new(-1, 1);
    private bool _blinkTrigger = false;

    private Element? LargeMap => GameController?.IngameState?.IngameUi?.Map?.LargeMap;
    private IngameUIElements? InGameUi => GameController?.Game?.IngameState?.IngameUi;
    private List<LabelOnGround> LabelsOnGround => GameController?.Game?.IngameState?.IngameUi?.ItemsOnGroundLabels?.ToList() ?? [];

    private Dictionary<string, List<string>> Mapping()
    {
        if (_mapping.Count == 0)
        {
            var customFilePath = Path.Join(DirectoryFullName, CustomUniqueArtMappingPath);
            if (File.Exists(customFilePath))
            {
                try
                {
                    _mapping = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(customFilePath)) ??
                               new Dictionary<string, List<string>>();
                }
                catch (Exception ex)
                {
                    LogError($"Unable to load art mapping: {ex}");
                }
            }
        }

        return _mapping;
    }

    public override bool Initialise()
    {
        GameController.UnderPanel.WantUse(() => Settings.Enable);
        Mapping();
        if (Settings.UniqueNames.Count == 0 && !Settings.Initialized)
        {
            Settings.UniqueNames.Add("Mageblood");
            Settings.UniqueNames.Add("Headhunter");
        }

        return true;
    }

    public override Job Tick()
    {
        if (_timer.ElapsedMilliseconds <= Settings.UpdateTimer) return base.Tick();
        if (LabelsOnGround.Count == 0) return base.Tick();
        if (GameController?.Files is null) return base.Tick();

        var newFilteredLabelsOnGround = new HashSet<GroundItemInstance>();
        foreach (var labelOnGround in LabelsOnGround)
        {
            labelOnGround.ItemOnGround.TryGetComponent<WorldItem>(out var worldItem);
            if (worldItem is null) continue;
            worldItem.ItemEntity.TryGetComponent<Mods>(out var itemMods);
            if (itemMods is null) continue;
            if (itemMods.ItemRarity != ItemRarity.Unique) continue;
            worldItem.ItemEntity.TryGetComponent<RenderItem>(out var renderItem);
            if (renderItem is null) continue;
            var itemName = Mapping().GetValueOrDefault(renderItem.ResourcePath)?.Where(i => !i.StartsWith("Replica")).FirstOrDefault();
            if (itemName is null) continue;
            var namesCopy = new List<string>(Settings.UniqueNames.Where(n => n.Trim().Length > 0));
            if (!namesCopy.Any(n => itemName.Contains(n, StringComparison.OrdinalIgnoreCase))) continue;
            newFilteredLabelsOnGround.Add(new GroundItemInstance(labelOnGround, worldItem, itemMods, renderItem, itemName, GameController));
        }

        _filteredLabelsOnGround = newFilteredLabelsOnGround;

        _timer.Restart();
        return base.Tick();
    }

    public override void Render()
    {
        if (InGameUi is null) return;
        if (InGameUi.FullscreenPanels.Any(p => p.IsVisible)) return;
        if (GameController.Player?.GridPosNum is null) return;

        if (_blinkTimer.ElapsedMilliseconds >= Settings.BlinkFrequency)
        {
            _blinkTimer.Restart();
            _blinkTrigger = !_blinkTrigger;
        }

        var summary = _filteredLabelsOnGround.OrderBy(i => i.Distance).ToList();
        if (summary.Count <= 0) return;

        var position = GameController.UnderPanel.StartDrawPoint.ToVector2Num();
        position.X -= 20f; // todo settings? this is shift from the right

        foreach (var item in summary)
        {
            if (Settings.EnablePanelDrawing && (!Settings.BlinkPanelDrawing || _blinkTrigger) && !InGameUi.OpenRightPanel.IsVisible)
            {
                var height = DrawItemIncremented(position, item);
                position.Y += height;
            }

            if (Settings.EnableMapDrawing && (!Settings.BlinkMapDrawing || _blinkTrigger))
            {
                if (LargeMap is not null && LargeMap.IsVisible)
                {
                    var itemLocation = GameController.IngameState.Data.GetGridMapScreenPosition(item.Location);
                    var playerLocation = GameController.IngameState.Data.GetGridMapScreenPosition(GameController.Player.GridPosNum);
                    Graphics.DrawLine(itemLocation, playerLocation, Settings.MapLineThickness, Settings.MapLineColor);
                }
            }

            if (item.Label.IsVisibleLocal && Settings.EnableItemOutline && _blinkTrigger)
            {
                var thickness = 2;
                var rect = item.Label.GetClientRect();
                rect.Inflate(thickness / 2f, thickness / 2f);
                Graphics.DrawFrame(rect, Color.Wheat, thickness);
            }
        }

        base.Render();
    }

    private float DrawItemIncremented(Vector2 position, GroundItemInstance item)
    {
        position += _borederOffset;
        var baseTextSize = Graphics.MeasureText(item.ItemName);
        var textSize = baseTextSize * Settings.TextSize;
        var fullWidth = textSize.X + 10 * Settings.TextSize;
        var textHeightWithPadding = textSize.Y + 4;
        var boxRect = new RectangleF(position.X - fullWidth, position.Y, fullWidth, textHeightWithPadding);
        Graphics.DrawBox(boxRect, item.BackgroundColor);

        // Borders
        var frameRect = boxRect;
        frameRect.Inflate(1, 1);
        Graphics.DrawFrame(frameRect, item.BorderColor, 1);

        using (Graphics.SetTextScale(Settings.TextSize))
        {
            var textPos = position + new Vector2(-5, textHeightWithPadding / 2 - textSize.Y / 2);
            Graphics.DrawText(item.ItemName, textPos, item.TextColor, FontAlign.Right);
        }

        return textHeightWithPadding + 1;
    }

    public override void DrawSettings()
    {
        ImGui.Text("Unique names:");

        ImGui.Indent();
        for (var i = 0; i < Settings.UniqueNames.Count; i++)
        {
            var name = Settings.UniqueNames[i];
            if (ImGui.InputTextWithHint($"##UniqueName-{i}", "Enter Uniquie Item name...", ref name, 128))
            {
                Settings.UniqueNames[i] = name;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(10);
            if (ImGui.Button($"x##remove-{i}"))
            {
                Settings.UniqueNames.RemoveAt(i);
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Remove this line");
            ImGui.PopItemWidth();
        }

        if (ImGui.Button("+##AddNewLine")) Settings.UniqueNames.Add("");
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add new line");

        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();

        base.DrawSettings();
    }
}