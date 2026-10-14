namespace Acme.Workforce.Contractors.Vendor.Review;

using System.Globalization;

/// <summary>How screens and exports show a vendor review.</summary>
public sealed record VendorReviewView(string Code, string Title, string Rating, string Currency, string CreatedAtText, string Capacity, string Badge, string StatusLabel)
{
    public static VendorReviewView ToVendorReviewView(VendorReview item)
    {
        return new VendorReviewView(
            item.Code,
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorReviewView> ToVendorReviewViews(List<VendorReview> items)
    {
        var views = new List<VendorReviewView>();
        foreach (var item in items)
        {
            views.Add(ToVendorReviewView(item));
        }
        return views;
    }

    public static double SumVendorReviewView(VendorReviewView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedCreatedAt + (double)parsedCapacity;
    }
}
