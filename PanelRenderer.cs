using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;
using static ExileCore.Shared.Enums.FontAlign;
using Vector2 = System.Numerics.Vector2;

namespace UniqueFinder;

public class PanelRenderer(UniqueFinder plugin)
{
    private const int ItemMargin = 4;
    private const int TextPaddingX = 5;
    private const int TextPaddingY = 2;
    private const int BorderWidth = 1;

    private EPanelAlign _align = EPanelAlign.Left;
    private float _margin;
    private Vector2 _pos = Vector2.Zero;
    private FontAlign _fontAlign = Right;

    public void Render(List<GroundItemInstance> summary)
    {
        if (plugin.GameController?.IngameState?.IngameUi is null) return;
        if (!plugin.Settings.Panel.Enabled) return;
        if (plugin.GameController.IngameState.IngameUi.OpenRightPanel?.IsVisible == true) return;

        Reset();
        foreach (var instance in summary) RenderNextElement(instance);
    }


    private void Reset()
    {
        _align = Enum.Parse<EPanelAlign>(plugin.Settings.Panel.PanelAlign.Value);
        _margin = _align == EPanelAlign.Bottom ? -plugin.Settings.Panel.Margin : plugin.Settings.Panel.Margin;
        var rect = plugin.GameController.IngameState.IngameUi.Map.GetClientRect();
        switch (_align)
        {
            case EPanelAlign.Left:
                _pos = new Vector2(_margin, 200 /* todo! settings? */);
                _fontAlign = Left;
                break;
            case EPanelAlign.Top:
                _pos = new Vector2(rect.Width / 2, _margin);
                _fontAlign = Center;
                break;
            case EPanelAlign.Right:
                _pos = plugin.GameController.UnderPanel.StartDrawPoint.ToVector2Num();
                _pos.X -= _margin;
                _fontAlign = Right;
                // In case of missing UI offsets...
                if (_pos.X <= 0) _pos = new Vector2(rect.Width - _margin, 500 /* todo settings? default? */);
                break;
            case EPanelAlign.Bottom:
                _pos = new Vector2(rect.Width / 2, rect.Height + _margin);
                _fontAlign = Center;
                break;
        }
    }

    private void RenderNextElement(GroundItemInstance item)
    {
        var textSize = plugin.Graphics.MeasureText(item.ItemName) * plugin.Settings.Panel.TextSize;
        var fullWidth = (textSize.X + TextPaddingX * 2 + BorderWidth * 2) * plugin.Settings.Panel.TextSize;
        var textHeight = textSize.Y + TextPaddingY * 2 + BorderWidth * 2;
        var stepShift = _align == EPanelAlign.Bottom ? -(textHeight + ItemMargin) : textHeight + ItemMargin;

        RectangleF boxRect;
        Vector2 textPos;
        switch (_align)
        {
            default:
            case EPanelAlign.Left:
                boxRect = new RectangleF(_pos.X, _pos.Y, fullWidth, textHeight);
                textPos = new Vector2(_pos.X + TextPaddingX, _pos.Y + BorderWidth * 2);
                break;
            case EPanelAlign.Top:
                boxRect = new RectangleF(_pos.X - fullWidth / 2, _pos.Y, fullWidth, textHeight);
                textPos = new Vector2(_pos.X + BorderWidth, _pos.Y + BorderWidth * 2);
                break;
            case EPanelAlign.Right:
                boxRect = new RectangleF(_pos.X - fullWidth, _pos.Y, fullWidth, textHeight);
                textPos = new Vector2(_pos.X - TextPaddingX, _pos.Y + BorderWidth * 2);
                break;
            case EPanelAlign.Bottom:
                boxRect = new RectangleF(_pos.X - fullWidth / 2, _pos.Y + stepShift + ItemMargin, fullWidth, textHeight);
                textPos = new Vector2(_pos.X + BorderWidth, _pos.Y + stepShift + ItemMargin + BorderWidth * 2);
                break;
        }

        plugin.Graphics.DrawBox(boxRect, item.BackgroundColor);
        
        plugin.Graphics.DrawFrame(boxRect, item.BorderColor, BorderWidth);

        using (plugin.Graphics.SetTextScale(plugin.Settings.Panel.TextSize))
        {
            plugin.Graphics.DrawText(item.ItemName, textPos, item.TextColor, _fontAlign);
        }

        _pos.Y += stepShift;
    }
}