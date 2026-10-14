namespace Acme.Workforce.Assets.Warranty.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a adaptive warranty audit.</summary>
public sealed record AdaptiveWarrantyAuditView(string Code, string Label, string LocaleText, string JobCode, string Badge, string Approved, string Amount, string StatusLabel)
{
    public static AdaptiveWarrantyAuditView ToAdaptiveWarrantyAuditView(AdaptiveWarrantyAudit item)
    {
        return new AdaptiveWarrantyAuditView(
            item.Code,
            item.Label,
            item.Locale,
            item.JobCode,
            item.Badge,
            $"{item.Approved}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveWarrantyAuditView> ToAdaptiveWarrantyAuditViews(List<AdaptiveWarrantyAudit> items)
    {
        var views = new List<AdaptiveWarrantyAuditView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveWarrantyAuditView(item));
        }
        return views;
    }

    public static double SumAdaptiveWarrantyAuditView(AdaptiveWarrantyAuditView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
