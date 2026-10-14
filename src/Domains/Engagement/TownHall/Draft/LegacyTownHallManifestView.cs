namespace Acme.Workforce.Engagement.TownHall.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy town hall manifest.</summary>
public sealed record LegacyTownHallManifestView(string Code, string Name, string CreatedAtText, string TenureMonths, string CategoryText, string JobCodeText, string StatusLabel)
{
    public static LegacyTownHallManifestView ToLegacyTownHallManifestView(LegacyTownHallManifest item)
    {
        return new LegacyTownHallManifestView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.JobCode, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTownHallManifestView> ToLegacyTownHallManifestViews(List<LegacyTownHallManifest> items)
    {
        var views = new List<LegacyTownHallManifestView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTownHallManifestView(item));
        }
        return views;
    }

    public static double SumLegacyTownHallManifestView(LegacyTownHallManifestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedTenureMonths;
    }
}
