namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default cost model manifest.</summary>
public sealed record DefaultCostModelManifestView(string Code, string Title, string ExternalRef, string CostCenter, string Archived, string ManagerId, string Retries, string CountryText, string Location, string StatusLabel)
{
    public static DefaultCostModelManifestView ToDefaultCostModelManifestView(DefaultCostModelManifest item)
    {
        return new DefaultCostModelManifestView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.ExternalRef,
            Text.Truncate(item.CostCenter, 40),
            $"{item.Archived}",
            item.ManagerId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 40),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultCostModelManifestView> ToDefaultCostModelManifestViews(List<DefaultCostModelManifest> items)
    {
        var views = new List<DefaultCostModelManifestView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultCostModelManifestView(item));
        }
        return views;
    }

    public static double SumDefaultCostModelManifestView(DefaultCostModelManifestView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
