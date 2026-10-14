namespace Acme.Workforce.Compliance.Attestation.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global attestation baseline.</summary>
public sealed record GlobalAttestationBaselineView(string CodeText, string Label, string Hours, string QuantityText, string Version, string ManagerId, string ExpiresAt, string StatusLabel)
{
    public static GlobalAttestationBaselineView ToGlobalAttestationBaselineView(GlobalAttestationBaseline item)
    {
        return new GlobalAttestationBaselineView(
            item.Code,
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAttestationBaselineView> ToGlobalAttestationBaselineViews(List<GlobalAttestationBaseline> items)
    {
        var views = new List<GlobalAttestationBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAttestationBaselineView(item));
        }
        return views;
    }

    public static double SumGlobalAttestationBaselineView(GlobalAttestationBaselineView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedQuantity + (double)parsedVersion;
    }
}
