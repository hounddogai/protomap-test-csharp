namespace Acme.Workforce.Contractors.Vendor.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor assignment.</summary>
public sealed record VendorAssignmentView(string Code, string Title, string ExpiresAt, string QuantityText, string Capacity, string Ratio, string TenureMonths, string RevisionText, string OwnerId, string StatusLabel)
{
    public static VendorAssignmentView ToVendorAssignmentView(VendorAssignment item)
    {
        return new VendorAssignmentView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorAssignmentView> ToVendorAssignmentViews(List<VendorAssignment> items)
    {
        var views = new List<VendorAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToVendorAssignmentView(item));
        }
        return views;
    }

    public static double SumVendorAssignmentView(VendorAssignmentView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedQuantity + (double)parsedCapacity;
    }
}
