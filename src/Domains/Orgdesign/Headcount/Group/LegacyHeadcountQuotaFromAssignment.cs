namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Functions of legacy headcount quota from assignment.</summary>
public static class LegacyHeadcountQuotaFromAssignment
{
    public static Dictionary<string, double> SummarizeLegacyHeadcountAssignmentsForQuota(List<LegacyHeadcountAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<LegacyHeadcountQuota> DeriveLegacyHeadcountQuotasFromAssignment(List<LegacyHeadcountAssignment> items)
    {
        var outValue = new List<LegacyHeadcountQuota>();
        foreach (var item in items)
        {
            var derived = LegacyHeadcountQuota.NewLegacyHeadcountQuota(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLegacyHeadcountQuota(LegacyHeadcountQuota item)
    {
        return HeadcountMetricPolicy.HeadcountMetricBucket(item.MeasuredValue());
    }
}
