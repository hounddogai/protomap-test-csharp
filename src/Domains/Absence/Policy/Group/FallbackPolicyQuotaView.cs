namespace Acme.Workforce.Absence.Policy.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback policy quota.</summary>
public sealed record FallbackPolicyQuotaView(string CodeText, string NameText, string DiscountText, string EndsAt, string Cost, string DurationMinutes, string JobCode, string StatusLabel)
{
    public static FallbackPolicyQuotaView ToFallbackPolicyQuotaView(FallbackPolicyQuota item)
    {
        return new FallbackPolicyQuotaView(
            item.Code,
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPolicyQuotaView> ToFallbackPolicyQuotaViews(List<FallbackPolicyQuota> items)
    {
        var views = new List<FallbackPolicyQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPolicyQuotaView(item));
        }
        return views;
    }

    public static double SumFallbackPolicyQuotaView(FallbackPolicyQuotaView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedEndsAt + parsedCost;
    }
}
