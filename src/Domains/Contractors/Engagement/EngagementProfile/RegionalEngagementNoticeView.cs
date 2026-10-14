namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using System.Globalization;

/// <summary>How screens and exports show a regional engagement notice.</summary>
public sealed record RegionalEngagementNoticeView(string Code, string Title, string QuantityText, string Discount, string ExpiresAt, string StatusLabel)
{
    public static RegionalEngagementNoticeView ToRegionalEngagementNoticeView(RegionalEngagementNotice item)
    {
        return new RegionalEngagementNoticeView(
            item.Code,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalEngagementNoticeView> ToRegionalEngagementNoticeViews(List<RegionalEngagementNotice> items)
    {
        var views = new List<RegionalEngagementNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalEngagementNoticeView(item));
        }
        return views;
    }

    public static double SumRegionalEngagementNoticeView(RegionalEngagementNoticeView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedDiscount + (double)parsedExpiresAt;
    }
}
