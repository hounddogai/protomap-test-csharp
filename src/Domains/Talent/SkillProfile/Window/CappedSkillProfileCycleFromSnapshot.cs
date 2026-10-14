namespace Acme.Workforce.Talent.SkillProfile.Window;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Functions of capped skill profile cycle from snapshot.</summary>
public static class CappedSkillProfileCycleFromSnapshot
{
    public static Dictionary<string, double> SummarizePoolSnapshotsForCycle(List<PoolSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<CappedSkillProfileCycle> DeriveCappedSkillProfileCyclesFromSnapshot(List<PoolSnapshot> items)
    {
        var outValue = new List<CappedSkillProfileCycle>();
        foreach (var item in items)
        {
            var derived = CappedSkillProfileCycle.NewCappedSkillProfileCycle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreCappedSkillProfileCycle(CappedSkillProfileCycle item)
    {
        return GlobalPoolForecastPolicy.ClassifyGlobalPoolForecast(item.MeasuredValue());
    }
}
