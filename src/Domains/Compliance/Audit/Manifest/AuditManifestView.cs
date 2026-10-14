namespace Acme.Workforce.Compliance.Audit.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit manifest.</summary>
public sealed record AuditManifestView(string Code, string TitleText, string Phone, string ReviewerText, string SalaryBand, string Department, string QuantityText, string StatusLabel)
{
    public static AuditManifestView ToAuditManifestView(AuditManifest item)
    {
        return new AuditManifestView(
            item.Code,
            item.Title,
            Text.Truncate(item.Phone, 120),
            item.Reviewer,
            item.SalaryBand,
            item.Department,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditManifestView> ToAuditManifestViews(List<AuditManifest> items)
    {
        var views = new List<AuditManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAuditManifestView(item));
        }
        return views;
    }

    public static double SumAuditManifestView(AuditManifestView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
