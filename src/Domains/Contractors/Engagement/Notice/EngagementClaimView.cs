namespace Acme.Workforce.Contractors.Engagement.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement claim.</summary>
public sealed record EngagementClaimView(string CodeText, string Label, string ExternalRefText, string Discount, string Rating, string ManagerId, string Cost, string StatusLabel)
{
    public static EngagementClaimView ToEngagementClaimView(EngagementClaim item)
    {
        return new EngagementClaimView(
            Text.Truncate(item.Code, 64),
            item.Label,
            Text.Truncate(item.ExternalRef, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementClaimView> ToEngagementClaimViews(List<EngagementClaim> items)
    {
        var views = new List<EngagementClaimView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementClaimView(item));
        }
        return views;
    }

    public static double SumEngagementClaimView(EngagementClaimView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedRating + parsedCost;
    }
}
