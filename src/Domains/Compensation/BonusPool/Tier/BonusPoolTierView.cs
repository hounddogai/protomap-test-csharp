namespace Acme.Workforce.Compensation.BonusPool.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool tier.</summary>
public sealed record BonusPoolTierView(string CodeText, string Label, string HoursText, string ExpiresAtText, string UpdatedAt, string Location, string RetriesText, string CostText, string Ceiling, string StatusLabel)
{
    public static BonusPoolTierView ToBonusPoolTierView(BonusPoolTier item)
    {
        return new BonusPoolTierView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolTierView> ToBonusPoolTierViews(List<BonusPoolTier> items)
    {
        var views = new List<BonusPoolTierView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolTierView(item));
        }
        return views;
    }

    public static double SumBonusPoolTierView(BonusPoolTierView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedExpiresAt + (double)parsedUpdatedAt;
    }
}
