namespace Acme.Workforce.Contractors.Vendor.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict vendor request.</summary>
public sealed record StrictVendorRequestView(string Code, string Title, string Attempts, string Margin, string JobCode, string ExternalRef, string CreatedAtText, string Ratio, string Threshold, string StatusLabel)
{
    public static StrictVendorRequestView ToStrictVendorRequestView(StrictVendorRequest item)
    {
        return new StrictVendorRequestView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 120),
            item.ExternalRef,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictVendorRequestView> ToStrictVendorRequestViews(List<StrictVendorRequest> items)
    {
        var views = new List<StrictVendorRequestView>();
        foreach (var item in items)
        {
            views.Add(ToStrictVendorRequestView(item));
        }
        return views;
    }

    public static double SumStrictVendorRequestView(StrictVendorRequestView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedMargin + (double)parsedCreatedAt;
    }
}
