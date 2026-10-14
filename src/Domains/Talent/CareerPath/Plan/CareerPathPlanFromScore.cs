namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Functions of career path plan from score.</summary>
public static class CareerPathPlanFromScore
{
    public static Dictionary<string, double> SummarizeSuccessionPlanScoresForPlan(List<SuccessionPlanScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<CareerPathPlan> DeriveCareerPathPlansFromScore(List<SuccessionPlanScore> items)
    {
        var outValue = new List<CareerPathPlan>();
        foreach (var item in items)
        {
            var derived = CareerPathPlan.NewCareerPathPlan(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignCareerPathPlan(CareerPathPlan item)
    {
        return SuccessionPlanRollupPolicy.SuccessionPlanRollupBucket(item.MeasuredValue());
    }
}
