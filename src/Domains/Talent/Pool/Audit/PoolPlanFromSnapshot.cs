namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Functions of pool plan from snapshot.</summary>
public static class PoolPlanFromSnapshot
{
    public static Dictionary<string, double> SummarizeSkillProfileSnapshotsForPlan(List<SkillProfileSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<PoolPlan> DerivePoolPlansFromSnapshot(List<SkillProfileSnapshot> items)
    {
        var outValue = new List<PoolPlan>();
        foreach (var item in items)
        {
            var derived = PoolPlan.NewPoolPlan(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizePoolPlan(PoolPlan item)
    {
        return SkillProfileSnapshotPolicy.SkillProfileSnapshotWeight(5, true);
    }
}
