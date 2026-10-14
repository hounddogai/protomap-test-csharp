namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Functions of seasonal exception group from group.</summary>
public static class SeasonalExceptionGroupFromGroup
{
    public static Dictionary<string, double> SummarizeLegacyPolicyAckGroupsForGroup(List<LegacyPolicyAckGroup> items)
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

    public static List<SeasonalExceptionGroup> DeriveSeasonalExceptionGroupsFromGroup(List<LegacyPolicyAckGroup> items)
    {
        var outValue = new List<SeasonalExceptionGroup>();
        foreach (var item in items)
        {
            var derived = SeasonalExceptionGroup.NewSeasonalExceptionGroup(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplySeasonalExceptionGroup(SeasonalExceptionGroup item)
    {
        return LegacyPolicyAckGroupPolicy.LegacyPolicyAckGroupBucket(item.ProjectedLoad());
    }
}
