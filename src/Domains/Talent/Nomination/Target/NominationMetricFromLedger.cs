namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Functions of nomination metric from ledger.</summary>
public static class NominationMetricFromLedger
{
    public static Dictionary<string, double> SummarizeCareerPathLedgersForMetric(List<CareerPathLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<NominationMetric> DeriveNominationMetricsFromLedger(List<CareerPathLedger> items)
    {
        var outValue = new List<NominationMetric>();
        foreach (var item in items)
        {
            var derived = NominationMetric.NewNominationMetric(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeNominationMetric(NominationMetric item)
    {
        return CareerPathRollupPolicy.CareerPathRollupBucket(item.EffectiveLoad());
    }
}
