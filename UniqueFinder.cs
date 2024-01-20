using System.Collections;
using System.Diagnostics;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ImGuiNET;

namespace UniqueFinder;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class UniqueFinder : BaseSettingsPlugin<UniqueFinderSettings>
{
    private readonly Stopwatch _blinkTimer = Stopwatch.StartNew();
    private HashSet<GroundItemInstance> _filteredLabelsOnGround = [];
    private bool _blinkTrigger;
    private static readonly WaitTime Wait1Sec = new(1000);
    private readonly PanelRenderer _panelRenderer;

    public UniqueFinder()
    {
        _panelRenderer = new PanelRenderer(this);
    }

    private Element? LargeMap => GameController?.IngameState?.IngameUi?.Map?.LargeMap;
    private IngameUIElements? InGameUi => GameController?.Game?.IngameState?.IngameUi;
    private List<LabelOnGround> LabelsOnGround => GameController?.Game?.IngameState?.IngameUi?.ItemsOnGroundLabels?.ToList() ?? [];


    public override bool Initialise()
    {
        GameController.UnderPanel.WantUse(() => Settings.Enable);
        UniqueArtMapping.Mapping(this);
        if (Settings.UniqueNames.Count == 0 && !Settings.Initialized)
        {
            Settings.UniqueNames.Add("Mageblood");
            Settings.UniqueNames.Add("Headhunter");
        }

        Core.ParallelRunner.Run(ParseThread(), this, $"Coroutine-{nameof(UniqueFinder)}");

        return true;
    }

    private IEnumerator ParseThread()
    {
        while (true)
        {
            if (LabelsOnGround.Count == 0) yield return Wait1Sec;
            if (GameController?.Files is null) yield return Wait1Sec;

            var newFilteredLabelsOnGround = new HashSet<GroundItemInstance>();
            foreach (var labelOnGround in LabelsOnGround)
            {
                labelOnGround.ItemOnGround.TryGetComponent<WorldItem>(out var worldItem);
                if (worldItem is null) continue;
                worldItem.ItemEntity.TryGetComponent<Mods>(out var itemMods);
                if (itemMods is null) continue;
                if (itemMods.ItemRarity != ItemRarity.Unique) continue;
                if (Settings.Common.HideIdentified && itemMods.Identified) continue;
                worldItem.ItemEntity.TryGetComponent<RenderItem>(out var renderItem);
                if (renderItem is null) continue;
                var itemName = UniqueArtMapping.Mapping(this).GetValueOrDefault(renderItem.ResourcePath)?.FirstOrDefault();
                if (itemName is null) continue;
                var namesCopy = new List<string>(Settings.UniqueNames.Where(n => n.Trim().Length > 0));
                if (!namesCopy.Any(n => itemName.Contains(n, StringComparison.OrdinalIgnoreCase))) continue;
                newFilteredLabelsOnGround.Add(new GroundItemInstance(labelOnGround, worldItem, itemMods, renderItem, itemName, GameController!));
            }

            _filteredLabelsOnGround = newFilteredLabelsOnGround;

            yield return Wait1Sec;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public override void Render()
    {
        if (InGameUi is null) return;
        if (InGameUi.FullscreenPanels.Any(p => p.IsVisible)) return;
        if (GameController.Player?.GridPosNum is null) return;

        if (_blinkTimer.ElapsedMilliseconds >= Settings.Common.BlinkTime)
        {
            _blinkTimer.Restart();
            _blinkTrigger = !_blinkTrigger;
        }

        var summary = _filteredLabelsOnGround.OrderBy(i => i.Distance).ToList();
        if (summary.Count <= 0) return;

        if (!Settings.Panel.Blink || _blinkTrigger)
            _panelRenderer.Render(summary);

        foreach (var item in summary)
        {
            if (Settings.LargeMap.Trace && (!Settings.LargeMap.Blink || _blinkTrigger) && LargeMap is not null && LargeMap.IsVisible)
            {
                var itemLocation = GameController.IngameState.Data.GetGridMapScreenPosition(item.Location);
                var playerLocation = GameController.IngameState.Data.GetGridMapScreenPosition(GameController.Player.GridPosNum);
                Graphics.DrawLine(itemLocation, playerLocation, Settings.LargeMap.Thickness, Settings.LargeMap.Color);
            }

            if (item.Label.IsVisibleLocal && Settings.Label.Outline && (!Settings.Label.Blink || _blinkTrigger) && InGameUi?.OpenRightPanel.IsVisible != true)
            {
                const int thickness = 2;
                var rect = item.Label.GetClientRect();
                rect.Inflate(thickness / 2f, thickness / 2f);
                Graphics.DrawFrame(rect, Settings.Label.FrameColor, thickness);
            }
        }

        base.Render();
    }

    public override void DrawSettings()
    {
        ImGui.Text($"Unique names ({UniqueArtMapping.Mapping(this).Count} loaded):");
        if (ImGui.IsItemHovered())
            ImGui.SetItemTooltip("If there is no any unique art loaded - something went wrong, and plugin won't work!");

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