namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>Functions of skill profile preference from policy.</summary>
public static class SkillProfilePreferenceFromPolicy
{
    public static Dictionary<string, double> SummarizeSuccessionPlanPoliciesForPreference(List<SuccessionPlanPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<SkillProfilePreference> DeriveSkillProfilePreferencesFromPolicy(List<SuccessionPlanPolicy> items)
    {
        var outValue = new List<SkillProfilePreference>();
        foreach (var item in items)
        {
            var derived = SkillProfilePreference.NewSkillProfilePreference(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleSkillProfilePreference(SkillProfilePreference item)
    {
        return SuccessionPlanPolicyPolicy.SuccessionPlanPolicyBucket(item.AdjustedVolume());
    }
}
