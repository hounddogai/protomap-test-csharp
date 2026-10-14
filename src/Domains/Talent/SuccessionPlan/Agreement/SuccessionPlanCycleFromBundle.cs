namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Functions of succession plan cycle from bundle.</summary>
public static class SuccessionPlanCycleFromBundle
{
    public static Dictionary<string, double> SummarizeSkillProfileBundlesForCycle(List<SkillProfileBundle> items)
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

    public static List<SuccessionPlanCycle> DeriveSuccessionPlanCyclesFromBundle(List<SkillProfileBundle> items)
    {
        var outValue = new List<SuccessionPlanCycle>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanCycle.NewSuccessionPlanCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreSuccessionPlanCycle(SuccessionPlanCycle item)
    {
        return SkillProfileBundlePolicy.NormalizeSkillProfileBundleCode(item.Code);
    }
}
