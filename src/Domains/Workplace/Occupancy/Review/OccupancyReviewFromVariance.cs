namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Functions of occupancy review from variance.</summary>
public static class OccupancyReviewFromVariance
{
    public static Dictionary<string, double> SummarizeDeskVariancesForReview(List<DeskVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<OccupancyReview> DeriveOccupancyReviewsFromVariance(List<DeskVariance> items)
    {
        var outValue = new List<OccupancyReview>();
        foreach (var item in items)
        {
            var derived = OccupancyReview.NewOccupancyReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceOccupancyReview(OccupancyReview item)
    {
        return StrictDeskBudgetPolicy.NormalizeStrictDeskBudgetCode(item.Code);
    }
}
