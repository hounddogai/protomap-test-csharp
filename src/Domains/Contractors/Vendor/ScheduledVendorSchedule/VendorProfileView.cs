namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using System.Globalization;

/// <summary>How screens and exports show a vendor profile.</summary>
public sealed record VendorProfileView(string Code, string LabelText, string Name, string Region, string MaximumText, string Phone, string StatusLabel)
{
    public static VendorProfileView ToVendorProfileView(VendorProfile item)
    {
        return new VendorProfileView(
            item.Code,
            item.Label,
            item.Name,
            item.Region,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorProfileView> ToVendorProfileViews(List<VendorProfile> items)
    {
        var views = new List<VendorProfileView>();
        foreach (var item in items)
        {
            views.Add(ToVendorProfileView(item));
        }
        return views;
    }

    public static double SumVendorProfileView(VendorProfileView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
