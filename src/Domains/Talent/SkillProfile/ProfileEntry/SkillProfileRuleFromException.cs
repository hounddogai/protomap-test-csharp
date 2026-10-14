namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Functions of skill profile rule from exception.</summary>
public static class SkillProfileRuleFromException
{
    public static Dictionary<string, double> SummarizeManualSkillProfileExceptionsForRule(List<ManualSkillProfileException> items)
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

    public static List<SkillProfileRule> DeriveSkillProfileRulesFromException(List<ManualSkillProfileException> items)
    {
        var outValue = new List<SkillProfileRule>();
        foreach (var item in items)
        {
            var derived = SkillProfileRule.NewSkillProfileRule(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSkillProfileRule(SkillProfileRule item)
    {
        return DefaultSkillProfileDraftPolicy.DefaultSkillProfileDraftBucket(item.EffectiveExposure());
    }
}
