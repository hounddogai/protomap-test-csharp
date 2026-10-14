namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback bonus pool milestone.</summary>
public sealed record FallbackBonusPoolMilestoneView(string Code, string Name, string Archived, string Discount, string Capacity, string ExpiresAt, string ManagerId, string ExternalRef, string TimezoneText, string Skill, string StatusLabel)
{
    public static FallbackBonusPoolMilestoneView ToFallbackBonusPoolMilestoneView(FallbackBonusPoolMilestone item)
    {
        return new FallbackBonusPoolMilestoneView(
            item.Code,
            Text.Truncate(item.Name, 40),
            $"{item.Archived}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.ExternalRef, 80),
            item.Timezone,
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackBonusPoolMilestoneView> ToFallbackBonusPoolMilestoneViews(List<FallbackBonusPoolMilestone> items)
    {
        var views = new List<FallbackBonusPoolMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackBonusPoolMilestoneView(item));
        }
        return views;
    }

    public static double SumFallbackBonusPoolMilestoneView(FallbackBonusPoolMilestoneView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedCapacity + (double)parsedExpiresAt;
    }
}
