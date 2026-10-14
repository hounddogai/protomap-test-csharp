namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Functions of skill profile template from assignment.</summary>
public static class SkillProfileTemplateFromAssignment
{
    public static Dictionary<string, double> SummarizeFallbackPoolAssignmentsForTemplate(List<FallbackPoolAssignment> items)
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

    public static List<SkillProfileTemplate> DeriveSkillProfileTemplatesFromAssignment(List<FallbackPoolAssignment> items)
    {
        var outValue = new List<SkillProfileTemplate>();
        foreach (var item in items)
        {
            var derived = SkillProfileTemplate.NewSkillProfileTemplate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSkillProfileTemplate(SkillProfileTemplate item)
    {
        return LegacyPoolSegmentPolicy.NormalizeLegacyPoolSegmentCode(item.Code);
    }
}
