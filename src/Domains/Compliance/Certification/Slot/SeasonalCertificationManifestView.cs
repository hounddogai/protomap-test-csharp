namespace Acme.Workforce.Compliance.Certification.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal certification manifest.</summary>
public sealed record SeasonalCertificationManifestView(string Code, string Label, string Threshold, string TeamText, string WindowDays, string GraceDays, string StatusLabel)
{
    public static SeasonalCertificationManifestView ToSeasonalCertificationManifestView(SeasonalCertificationManifest item)
    {
        return new SeasonalCertificationManifestView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 40),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalCertificationManifestView> ToSeasonalCertificationManifestViews(List<SeasonalCertificationManifest> items)
    {
        var views = new List<SeasonalCertificationManifestView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalCertificationManifestView(item));
        }
        return views;
    }

    public static double SumSeasonalCertificationManifestView(SeasonalCertificationManifestView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedWindowDays + (double)parsedGraceDays;
    }
}
