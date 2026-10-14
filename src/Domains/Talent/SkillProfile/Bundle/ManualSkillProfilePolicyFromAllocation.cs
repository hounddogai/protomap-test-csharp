namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Functions of manual skill profile policy from allocation.</summary>
public static class ManualSkillProfilePolicyFromAllocation
{
    public static Dictionary<string, double> SummarizeMobilityAllocationsForPolicy(List<MobilityAllocation> items)
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

    public static List<ManualSkillProfilePolicy> DeriveManualSkillProfilePoliciesFromAllocation(List<MobilityAllocation> items)
    {
        var outValue = new List<ManualSkillProfilePolicy>();
        foreach (var item in items)
        {
            var derived = ManualSkillProfilePolicy.NewManualSkillProfilePolicy(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankManualSkillProfilePolicy(ManualSkillProfilePolicy item)
    {
        return ScheduledMobilityEntryPolicy.NormalizeScheduledMobilityEntryCode(item.Code);
    }
}
