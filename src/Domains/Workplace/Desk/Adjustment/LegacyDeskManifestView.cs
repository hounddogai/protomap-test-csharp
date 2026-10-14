namespace Acme.Workforce.Workplace.Desk.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy desk manifest.</summary>
public sealed record LegacyDeskManifestView(string CodeText, string Label, string PinnedText, string SegmentText, string Country, string ExpiresAt, string Ratio, string Hours, string WindowDaysText, string Grade, string StatusLabel)
{
    public static LegacyDeskManifestView ToLegacyDeskManifestView(LegacyDeskManifest item)
    {
        return new LegacyDeskManifestView(
            item.Code,
            Text.Truncate(item.Label, 64),
            $"{item.Pinned}",
            item.Segment,
            item.Country,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyDeskManifestView> ToLegacyDeskManifestViews(List<LegacyDeskManifest> items)
    {
        var views = new List<LegacyDeskManifestView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyDeskManifestView(item));
        }
        return views;
    }

    public static double SumLegacyDeskManifestView(LegacyDeskManifestView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedRatio + parsedHours;
    }
}
