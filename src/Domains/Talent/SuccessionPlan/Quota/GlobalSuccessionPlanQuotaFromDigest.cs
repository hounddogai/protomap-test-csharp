namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Functions of global succession plan quota from digest.</summary>
public static class GlobalSuccessionPlanQuotaFromDigest
{
    public static Dictionary<string, double> SummarizeTieredSkillProfileDigestsForQuota(List<TieredSkillProfileDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<GlobalSuccessionPlanQuota> DeriveGlobalSuccessionPlanQuotasFromDigest(List<TieredSkillProfileDigest> items)
    {
        var outValue = new List<GlobalSuccessionPlanQuota>();
        foreach (var item in items)
        {
            var derived = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanGlobalSuccessionPlanQuota(GlobalSuccessionPlanQuota item)
    {
        return SkillProfileReviewPolicy.SkillProfileReviewWeight(8, true);
    }
}
