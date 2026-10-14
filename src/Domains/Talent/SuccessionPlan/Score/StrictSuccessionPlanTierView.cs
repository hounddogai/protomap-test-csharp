namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using System.Globalization;

/// <summary>How screens and exports show a strict succession plan tier.</summary>
public sealed record StrictSuccessionPlanTierView(string CodeText, string NameText, string FteText, string Capacity, string BudgetText, string Ceiling, string CreatedAt, string Cost, string MinimumText, string StatusLabel)
{
    public static StrictSuccessionPlanTierView ToStrictSuccessionPlanTierView(StrictSuccessionPlanTier item)
    {
        return new StrictSuccessionPlanTierView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictSuccessionPlanTierView> ToStrictSuccessionPlanTierViews(List<StrictSuccessionPlanTier> items)
    {
        var views = new List<StrictSuccessionPlanTierView>();
        foreach (var item in items)
        {
            views.Add(ToStrictSuccessionPlanTierView(item));
        }
        return views;
    }

    public static double SumStrictSuccessionPlanTierView(StrictSuccessionPlanTierView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedCapacity + parsedBudget;
    }
}
