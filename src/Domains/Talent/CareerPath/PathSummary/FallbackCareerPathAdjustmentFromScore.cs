namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Functions of fallback career path adjustment from score.</summary>
public static class FallbackCareerPathAdjustmentFromScore
{
    public static Dictionary<string, double> SummarizeLenientSuccessionPlanScoresForAdjustment(List<LenientSuccessionPlanScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<FallbackCareerPathAdjustment> DeriveFallbackCareerPathAdjustmentsFromScore(List<LenientSuccessionPlanScore> items)
    {
        var outValue = new List<FallbackCareerPathAdjustment>();
        foreach (var item in items)
        {
            var derived = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectFallbackCareerPathAdjustment(FallbackCareerPathAdjustment item)
    {
        return LenientSuccessionPlanScorePolicy.LenientSuccessionPlanScoreWeight(9, true);
    }
}
