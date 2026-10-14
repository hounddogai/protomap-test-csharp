namespace Acme.Workforce.Assets.Warranty.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty bundle.</summary>
public sealed record WarrantyBundleView(string Code, string Title, string Unit, string ExpiresAt, string BadgeText, string EndsAtText, string UpdatedAtText, string Description, string LeaveDaysText, string BatchSize, string PhoneText, string StatusLabel)
{
    public static WarrantyBundleView ToWarrantyBundleView(WarrantyBundle item)
    {
        return new WarrantyBundleView(
            item.Code,
            item.Title,
            item.Unit,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyBundleView> ToWarrantyBundleViews(List<WarrantyBundle> items)
    {
        var views = new List<WarrantyBundleView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyBundleView(item));
        }
        return views;
    }

    public static double SumWarrantyBundleView(WarrantyBundleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedEndsAt + (double)parsedUpdatedAt;
    }
}
