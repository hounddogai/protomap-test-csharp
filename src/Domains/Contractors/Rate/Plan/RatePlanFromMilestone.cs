namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>Functions of rate plan from milestone.</summary>
public static class RatePlanFromMilestone
{
    public static Dictionary<string, double> SummarizeRateMilestonesForPlan(List<RateMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<RatePlan> DeriveRatePlansFromMilestone(List<RateMilestone> items)
    {
        var outValue = new List<RatePlan>();
        foreach (var item in items)
        {
            var derived = RatePlan.NewRatePlan(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveRatePlan(RatePlan item)
    {
        return RateForecastPolicy.ClassifyRateForecast(item.MeasuredValue());
    }
}
