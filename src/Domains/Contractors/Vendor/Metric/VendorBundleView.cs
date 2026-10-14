namespace Acme.Workforce.Contractors.Vendor.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor bundle.</summary>
public sealed record VendorBundleView(string Code, string TitleText, string Rating, string SkillText, string LocationText, string ExpiresAt, string Pinned, string HoursText, string LeaveDaysText, string StatusLabel)
{
    public static VendorBundleView ToVendorBundleView(VendorBundle item)
    {
        return new VendorBundleView(
            item.Code,
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            item.Location,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorBundleView> ToVendorBundleViews(List<VendorBundle> items)
    {
        var views = new List<VendorBundleView>();
        foreach (var item in items)
        {
            views.Add(ToVendorBundleView(item));
        }
        return views;
    }

    public static double SumVendorBundleView(VendorBundleView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedExpiresAt + parsedHours;
    }
}
