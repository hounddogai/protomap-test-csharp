namespace Acme.Workforce.Talent.SkillProfile.Agreement;

using Acme.Workforce.Talent.Mobility.MobilityReservation;

/// <summary>Functions of adaptive skill profile threshold from reservation.</summary>
public static class AdaptiveSkillProfileThresholdFromReservation
{
    public static Dictionary<string, double> SummarizePendingMobilityReservationsForThreshold(List<PendingMobilityReservation> items)
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

    public static List<AdaptiveSkillProfileThreshold> DeriveAdaptiveSkillProfileThresholdsFromReservation(List<PendingMobilityReservation> items)
    {
        var outValue = new List<AdaptiveSkillProfileThreshold>();
        foreach (var item in items)
        {
            var derived = AdaptiveSkillProfileThreshold.NewAdaptiveSkillProfileThreshold(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeAdaptiveSkillProfileThreshold(AdaptiveSkillProfileThreshold item)
    {
        return MobilityPolicyPolicy.ClassifyMobilityPolicy(item.ProjectedLoad());
    }
}
