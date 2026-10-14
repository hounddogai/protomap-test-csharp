namespace Acme.Workforce.Orgdesign.Position.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered position template.</summary>
public sealed record TieredPositionTemplateView(string Code, string Label, string Channel, string WindowDays, string Checksum, string ArchivedText, string UnitText, string LeaveDays, string StatusLabel)
{
    public static TieredPositionTemplateView ToTieredPositionTemplateView(TieredPositionTemplate item)
    {
        return new TieredPositionTemplateView(
            item.Code,
            item.Label,
            Text.Truncate(item.Channel, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            $"{item.Archived}",
            Text.Truncate(item.Unit, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPositionTemplateView> ToTieredPositionTemplateViews(List<TieredPositionTemplate> items)
    {
        var views = new List<TieredPositionTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPositionTemplateView(item));
        }
        return views;
    }

    public static double SumTieredPositionTemplateView(TieredPositionTemplateView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedLeaveDays;
    }
}
