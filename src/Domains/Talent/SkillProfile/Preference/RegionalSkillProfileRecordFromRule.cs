namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Functions of regional skill profile record from rule.</summary>
public static class RegionalSkillProfileRecordFromRule
{
    public static Dictionary<string, double> SummarizeCareerPathRulesForRecord(List<CareerPathRule> items)
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

    public static List<RegionalSkillProfileRecord> DeriveRegionalSkillProfileRecordsFromRule(List<CareerPathRule> items)
    {
        var outValue = new List<RegionalSkillProfileRecord>();
        foreach (var item in items)
        {
            var derived = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRegionalSkillProfileRecord(RegionalSkillProfileRecord item)
    {
        return CareerPathRulePolicy.NormalizeCareerPathRuleCode(item.Code);
    }
}
