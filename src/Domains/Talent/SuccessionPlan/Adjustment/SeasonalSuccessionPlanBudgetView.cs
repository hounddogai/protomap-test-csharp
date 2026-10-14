namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal succession plan budget.</summary>
public sealed record SeasonalSuccessionPlanBudgetView(string CodeText, string Label, string RatingText, string Cost, string UpdatedAt, string Ratio, string StartsAtText, string Source, string CurrencyText, string StatusLabel)
{
    public static SeasonalSuccessionPlanBudgetView ToSeasonalSuccessionPlanBudgetView(SeasonalSuccessionPlanBudget item)
    {
        return new SeasonalSuccessionPlanBudgetView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Source,
            Text.Truncate(item.Currency, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalSuccessionPlanBudgetView> ToSeasonalSuccessionPlanBudgetViews(List<SeasonalSuccessionPlanBudget> items)
    {
        var views = new List<SeasonalSuccessionPlanBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalSuccessionPlanBudgetView(item));
        }
        return views;
    }

    public static double SumSeasonalSuccessionPlanBudgetView(SeasonalSuccessionPlanBudgetView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedRating + parsedCost + (double)parsedUpdatedAt;
    }
}
