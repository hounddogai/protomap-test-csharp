namespace Acme.Workforce.Contractors.Vendor.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor segment.</summary>
public sealed record VendorSegmentView(string Code, string Title, string OwnerIdText, string CurrencyText, string GraceDaysText, string LeaveDaysText, string CeilingText, string StatusLabel)
{
    public static VendorSegmentView ToVendorSegmentView(VendorSegment item)
    {
        return new VendorSegmentView(
            Text.Truncate(item.Code, 64),
            item.Title,
            Text.Truncate(item.OwnerId, 120),
            Text.Truncate(item.Currency, 40),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorSegmentView> ToVendorSegmentViews(List<VendorSegment> items)
    {
        var views = new List<VendorSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToVendorSegmentView(item));
        }
        return views;
    }

    public static double SumVendorSegmentView(VendorSegmentView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedLeaveDays + parsedCeiling;
    }
}
