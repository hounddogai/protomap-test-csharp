namespace Acme.Workforce.Compensation.BonusPool.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool threshold.</summary>
public sealed record BonusPoolThresholdView(string Code, string Label, string Description, string RatioText, string Discount, string ContactEmail, string StatusLabel)
{
    public static BonusPoolThresholdView ToBonusPoolThresholdView(BonusPoolThreshold item)
    {
        return new BonusPoolThresholdView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Description,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolThresholdView> ToBonusPoolThresholdViews(List<BonusPoolThreshold> items)
    {
        var views = new List<BonusPoolThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolThresholdView(item));
        }
        return views;
    }

    public static double SumBonusPoolThresholdView(BonusPoolThresholdView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedRatio + parsedDiscount;
    }
}
