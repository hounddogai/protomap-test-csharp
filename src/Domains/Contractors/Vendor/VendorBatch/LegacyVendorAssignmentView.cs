namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy vendor assignment.</summary>
public sealed record LegacyVendorAssignmentView(string Code, string Label, string Channel, string Badge, string Attempts, string UpdatedAtText, string LocationText, string Category, string MinimumText, string Priority, string StatusLabel)
{
    public static LegacyVendorAssignmentView ToLegacyVendorAssignmentView(LegacyVendorAssignment item)
    {
        return new LegacyVendorAssignmentView(
            item.Code,
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Channel, 80),
            Text.Truncate(item.Badge, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Category,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyVendorAssignmentView> ToLegacyVendorAssignmentViews(List<LegacyVendorAssignment> items)
    {
        var views = new List<LegacyVendorAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyVendorAssignmentView(item));
        }
        return views;
    }

    public static double SumLegacyVendorAssignmentView(LegacyVendorAssignmentView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedUpdatedAt + parsedMinimum;
    }
}
