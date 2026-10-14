namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg manifest.</summary>
public sealed record ReorgManifestView(string CodeText, string Title, string TenureMonths, string Timezone, string JobCode, string StatusLabel)
{
    public static ReorgManifestView ToReorgManifestView(ReorgManifest item)
    {
        return new ReorgManifestView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.JobCode, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgManifestView> ToReorgManifestViews(List<ReorgManifest> items)
    {
        var views = new List<ReorgManifestView>();
        foreach (var item in items)
        {
            views.Add(ToReorgManifestView(item));
        }
        return views;
    }

    public static double SumReorgManifestView(ReorgManifestView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
