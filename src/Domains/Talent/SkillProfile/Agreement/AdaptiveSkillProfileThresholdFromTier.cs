namespace Acme.Workforce.Talent.SkillProfile.Agreement;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Functions of adaptive skill profile threshold from tier.</summary>
public static class AdaptiveSkillProfileThresholdFromTier
{
    public static Dictionary<string, double> SummarizeStrictNominationTiersForThreshold(List<StrictNominationTier> items)
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

    public static List<AdaptiveSkillProfileThreshold> DeriveAdaptiveSkillProfileThresholdsFromTier(List<StrictNominationTier> items)
    {
        var outValue = new List<AdaptiveSkillProfileThreshold>();
        foreach (var item in items)
        {
            var derived = AdaptiveSkillProfileThreshold.NewAdaptiveSkillProfileThreshold(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignAdaptiveSkillProfileThreshold(AdaptiveSkillProfileThreshold item)
    {
        return NominationExceptionPolicy.NominationExceptionBucket(item.ProjectedLoad());
    }
}
