namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy cost model manifest.</summary>
public sealed record LegacyCostModelManifestView(string CodeText, string TitleText, string CostText, string Approved, string Quantity, string Description, string ContactEmail, string StatusLabel)
{
    public static LegacyCostModelManifestView ToLegacyCostModelManifestView(LegacyCostModelManifest item)
    {
        return new LegacyCostModelManifestView(
            item.Code,
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.ContactEmail, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyCostModelManifestView> ToLegacyCostModelManifestViews(List<LegacyCostModelManifest> items)
    {
        var views = new List<LegacyCostModelManifestView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyCostModelManifestView(item));
        }
        return views;
    }

    public static double SumLegacyCostModelManifestView(LegacyCostModelManifestView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedQuantity;
    }
}
