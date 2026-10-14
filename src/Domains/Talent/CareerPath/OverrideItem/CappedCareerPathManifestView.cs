namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped career path manifest.</summary>
public sealed record CappedCareerPathManifestView(string Code, string NameText, string Amount, string Hours, string RegionText, string Grade, string Version, string RetriesText, string ArchivedText, string ExternalRefText, string StatusLabel)
{
    public static CappedCareerPathManifestView ToCappedCareerPathManifestView(CappedCareerPathManifest item)
    {
        return new CappedCareerPathManifestView(
            item.Code,
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            Text.Truncate(item.Grade, 80),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedCareerPathManifestView> ToCappedCareerPathManifestViews(List<CappedCareerPathManifest> items)
    {
        var views = new List<CappedCareerPathManifestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedCareerPathManifestView(item));
        }
        return views;
    }

    public static double SumCappedCareerPathManifestView(CappedCareerPathManifestView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedAmount + parsedHours + (double)parsedVersion;
    }
}
