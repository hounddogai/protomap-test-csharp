namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Functions of skill profile metric from mapping.</summary>
public static class SkillProfileMetricFromMapping
{
    public static Dictionary<string, double> SummarizeCappedSkillProfileMappingsForMetric(List<CappedSkillProfileMapping> items)
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

    public static List<SkillProfileMetric> DeriveSkillProfileMetricsFromMapping(List<CappedSkillProfileMapping> items)
    {
        var outValue = new List<SkillProfileMetric>();
        foreach (var item in items)
        {
            var derived = SkillProfileMetric.NewSkillProfileMetric(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeSkillProfileMetric(SkillProfileMetric item)
    {
        return CappedSkillProfileMappingPolicy.NormalizeCappedSkillProfileMappingCode(item.Code);
    }
}
