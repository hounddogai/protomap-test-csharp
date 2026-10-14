namespace Acme.Workforce.Contractors.Vendor.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor batch.</summary>
public sealed record VendorBatchView(string Code, string LabelText, string PhoneText, string Summary, string Version, string ActiveText, string StatusLabel)
{
    public static VendorBatchView ToVendorBatchView(VendorBatch item)
    {
        return new VendorBatchView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Phone,
            item.Summary,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorBatchView> ToVendorBatchViews(List<VendorBatch> items)
    {
        var views = new List<VendorBatchView>();
        foreach (var item in items)
        {
            views.Add(ToVendorBatchView(item));
        }
        return views;
    }

    public static double SumVendorBatchView(VendorBatchView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
