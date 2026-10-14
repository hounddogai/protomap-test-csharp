namespace Acme.Workforce.Engagement.TownHall.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy town hall checkpoint.</summary>
public sealed record LegacyTownHallCheckpointView(string CodeText, string LabelText, string Fte, string Archived, string Region, string Country, string NameText, string ExternalRef, string Threshold, string Hours, string StatusLabel)
{
    public static LegacyTownHallCheckpointView ToLegacyTownHallCheckpointView(LegacyTownHallCheckpoint item)
    {
        return new LegacyTownHallCheckpointView(
            item.Code,
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Region,
            Text.Truncate(item.Country, 40),
            item.Name,
            Text.Truncate(item.ExternalRef, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTownHallCheckpointView> ToLegacyTownHallCheckpointViews(List<LegacyTownHallCheckpoint> items)
    {
        var views = new List<LegacyTownHallCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTownHallCheckpointView(item));
        }
        return views;
    }

    public static double SumLegacyTownHallCheckpointView(LegacyTownHallCheckpointView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedFte + parsedThreshold + parsedHours;
    }
}
