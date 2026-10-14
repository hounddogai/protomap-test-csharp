namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Functions of manual headcount tier from cycle.</summary>
public static class ManualHeadcountTierFromCycle
{
    public static Dictionary<string, double> SummarizeHeadcountCyclesForTier(List<HeadcountCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<ManualHeadcountTier> DeriveManualHeadcountTiersFromCycle(List<HeadcountCycle> items)
    {
        var outValue = new List<ManualHeadcountTier>();
        foreach (var item in items)
        {
            var derived = ManualHeadcountTier.NewManualHeadcountTier(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyManualHeadcountTier(ManualHeadcountTier item)
    {
        return WeightedHeadcountNoticePolicy.WeightedHeadcountNoticeBucket(item.EffectiveExposure());
    }
}
