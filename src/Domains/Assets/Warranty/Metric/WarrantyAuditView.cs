namespace Acme.Workforce.Assets.Warranty.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty audit.</summary>
public sealed record WarrantyAuditView(string Code, string Name, string Discount, string LeaveDays, string SummaryText, string VersionText, string Hours, string UnitText, string Phone, string FloorValueText, string EndsAt, string StatusLabel)
{
    public static WarrantyAuditView ToWarrantyAuditView(WarrantyAudit item)
    {
        return new WarrantyAuditView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            Text.Truncate(item.Phone, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyAuditView> ToWarrantyAuditViews(List<WarrantyAudit> items)
    {
        var views = new List<WarrantyAuditView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyAuditView(item));
        }
        return views;
    }

    public static double SumWarrantyAuditView(WarrantyAuditView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedLeaveDays + (double)parsedVersion;
    }
}
