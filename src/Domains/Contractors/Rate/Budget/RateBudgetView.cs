namespace Acme.Workforce.Contractors.Rate.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate budget.</summary>
public sealed record RateBudgetView(string Code, string Name, string ThresholdText, string Minimum, string Active, string Cost, string VersionText, string OwnerId, string StartsAt, string StatusLabel)
{
    public static RateBudgetView ToRateBudgetView(RateBudget item)
    {
        return new RateBudgetView(
            item.Code,
            item.Name,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateBudgetView> ToRateBudgetViews(List<RateBudget> items)
    {
        var views = new List<RateBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToRateBudgetView(item));
        }
        return views;
    }

    public static double SumRateBudgetView(RateBudgetView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedMinimum + parsedCost;
    }
}
