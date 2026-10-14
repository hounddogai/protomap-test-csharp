namespace Acme.Workforce.Talent.Mobility.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility manifest.</summary>
public sealed record MobilityManifestView(string Code, string Name, string DurationMinutes, string ManagerIdText, string Badge, string StatusLabel)
{
    public static MobilityManifestView ToMobilityManifestView(MobilityManifest item)
    {
        return new MobilityManifestView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.Badge, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityManifestView> ToMobilityManifestViews(List<MobilityManifest> items)
    {
        var views = new List<MobilityManifestView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityManifestView(item));
        }
        return views;
    }

    public static double SumMobilityManifestView(MobilityManifestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
