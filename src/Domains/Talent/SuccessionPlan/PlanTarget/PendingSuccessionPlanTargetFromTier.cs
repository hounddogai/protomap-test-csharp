namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Functions of pending succession plan target from tier.</summary>
public static class PendingSuccessionPlanTargetFromTier
{
    public static Dictionary<string, double> SummarizeFallbackSkillProfileTiersForTarget(List<FallbackSkillProfileTier> items)
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

    public static List<PendingSuccessionPlanTarget> DerivePendingSuccessionPlanTargetsFromTier(List<FallbackSkillProfileTier> items)
    {
        var outValue = new List<PendingSuccessionPlanTarget>();
        foreach (var item in items)
        {
            var derived = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePendingSuccessionPlanTarget(PendingSuccessionPlanTarget item)
    {
        return SkillProfileCheckpointPolicy.NormalizeSkillProfileCheckpointCode(item.Code);
    }
}
