namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback mobility entry.</summary>
public sealed record FallbackMobilityEntryView(string Code, string Title, string FloorValueText, string Version, string Skill, string Archived, string Reviewer, string StatusLabel)
{
    public static FallbackMobilityEntryView ToFallbackMobilityEntryView(FallbackMobilityEntry item)
    {
        return new FallbackMobilityEntryView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Archived}",
            Text.Truncate(item.Reviewer, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackMobilityEntryView> ToFallbackMobilityEntryViews(List<FallbackMobilityEntry> items)
    {
        var views = new List<FallbackMobilityEntryView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackMobilityEntryView(item));
        }
        return views;
    }

    public static double SumFallbackMobilityEntryView(FallbackMobilityEntryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedVersion;
    }
}
