namespace Acme.Workforce.Contractors.Vendor.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global vendor review.</summary>
public sealed record GlobalVendorReviewView(string Code, string Name, string BatchSize, string ChannelText, string Country, string Region, string TenureMonths, string Priority, string ExternalRefText, string OwnerIdText, string Ceiling, string StatusLabel)
{
    public static GlobalVendorReviewView ToGlobalVendorReviewView(GlobalVendorReview item)
    {
        return new GlobalVendorReviewView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Country,
            item.Region,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 120),
            item.OwnerId,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalVendorReviewView> ToGlobalVendorReviewViews(List<GlobalVendorReview> items)
    {
        var views = new List<GlobalVendorReviewView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalVendorReviewView(item));
        }
        return views;
    }

    public static double SumGlobalVendorReviewView(GlobalVendorReviewView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedTenureMonths + (double)parsedPriority;
    }
}
