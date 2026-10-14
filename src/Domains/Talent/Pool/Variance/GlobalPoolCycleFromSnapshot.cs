namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>Functions of global pool cycle from snapshot.</summary>
public static class GlobalPoolCycleFromSnapshot
{
    public static Dictionary<string, double> SummarizeFallbackSkillProfileSnapshotsForCycle(List<FallbackSkillProfileSnapshot> items)
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

    public static List<GlobalPoolCycle> DeriveGlobalPoolCyclesFromSnapshot(List<FallbackSkillProfileSnapshot> items)
    {
        var outValue = new List<GlobalPoolCycle>();
        foreach (var item in items)
        {
            var derived = GlobalPoolCycle.NewGlobalPoolCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeGlobalPoolCycle(GlobalPoolCycle item)
    {
        return StrictSkillProfileEntryPolicy.NormalizeStrictSkillProfileEntryCode(item.Code);
    }
}
