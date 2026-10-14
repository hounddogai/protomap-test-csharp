namespace Acme.Workforce.Contractors.Vendor.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor record.</summary>
public sealed record VendorRecordView(string CodeText, string Name, string Country, string DurationMinutes, string UpdatedAt, string CurrencyText, string StatusLabel)
{
    public static VendorRecordView ToVendorRecordView(VendorRecord item)
    {
        return new VendorRecordView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Country,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorRecordView> ToVendorRecordViews(List<VendorRecord> items)
    {
        var views = new List<VendorRecordView>();
        foreach (var item in items)
        {
            views.Add(ToVendorRecordView(item));
        }
        return views;
    }

    public static double SumVendorRecordView(VendorRecordView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedUpdatedAt;
    }
}
