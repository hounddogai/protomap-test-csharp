namespace Acme.Workforce.Orgdesign.Position.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Functions of position plan from claim.</summary>
public static class PositionPlanFromClaim
{
    public static Dictionary<string, double> SummarizeHeadcountClaimsForPlan(List<HeadcountClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PositionPlan> DerivePositionPlansFromClaim(List<HeadcountClaim> items)
    {
        var outValue = new List<PositionPlan>();
        foreach (var item in items)
        {
            var derived = PositionPlan.NewPositionPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizePositionPlan(PositionPlan item)
    {
        return HeadcountClaimPolicy.HeadcountClaimWeight(7, true);
    }
}
