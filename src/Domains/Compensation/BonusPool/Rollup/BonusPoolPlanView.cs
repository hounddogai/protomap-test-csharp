namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool plan.</summary>
public sealed record BonusPoolPlanView(string Code, string Name, string Active, string RatingText, string Segment, string RatioText, string Discount, string DurationMinutes, string StatusLabel)
{
    public static BonusPoolPlanView ToBonusPoolPlanView(BonusPoolPlan item)
    {
        return new BonusPoolPlanView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Active}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolPlanView> ToBonusPoolPlanViews(List<BonusPoolPlan> items)
    {
        var views = new List<BonusPoolPlanView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolPlanView(item));
        }
        return views;
    }

    public static double SumBonusPoolPlanView(BonusPoolPlanView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedRating + parsedRatio + parsedDiscount;
    }
}
