namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Functions of strict skill profile entry from claim.</summary>
public static class StrictSkillProfileEntryFromClaim
{
    public static Dictionary<string, double> SummarizeStrictSuccessionPlanClaimsForEntry(List<StrictSuccessionPlanClaim> items)
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

    public static List<StrictSkillProfileEntry> DeriveStrictSkillProfileEntriesFromClaim(List<StrictSuccessionPlanClaim> items)
    {
        var outValue = new List<StrictSkillProfileEntry>();
        foreach (var item in items)
        {
            var derived = StrictSkillProfileEntry.NewStrictSkillProfileEntry(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileStrictSkillProfileEntry(StrictSkillProfileEntry item)
    {
        return RegionalSuccessionPlanAgreementPolicy.RegionalSuccessionPlanAgreementWeight(8, true);
    }
}
