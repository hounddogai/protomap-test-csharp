namespace Acme.Workforce.Contractors.Vendor.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor exception.</summary>
public sealed record VendorExceptionView(string CodeText, string LabelText, string Department, string StartsAtText, string Retries, string Threshold, string StatusLabel)
{
    public static VendorExceptionView ToVendorExceptionView(VendorException item)
    {
        return new VendorExceptionView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Department, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorExceptionView> ToVendorExceptionViews(List<VendorException> items)
    {
        var views = new List<VendorExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToVendorExceptionView(item));
        }
        return views;
    }

    public static double SumVendorExceptionView(VendorExceptionView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedRetries + parsedThreshold;
    }
}
