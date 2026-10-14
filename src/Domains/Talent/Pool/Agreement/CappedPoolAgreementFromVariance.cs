namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Functions of capped pool agreement from variance.</summary>
public static class CappedPoolAgreementFromVariance
{
    public static Dictionary<string, double> SummarizeLegacyPoolVariancesForAgreement(List<LegacyPoolVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<CappedPoolAgreement> DeriveCappedPoolAgreementsFromVariance(List<LegacyPoolVariance> items)
    {
        var outValue = new List<CappedPoolAgreement>();
        foreach (var item in items)
        {
            var derived = CappedPoolAgreement.NewCappedPoolAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankCappedPoolAgreement(CappedPoolAgreement item)
    {
        return GlobalPoolCyclePolicy.GlobalPoolCycleBucket(item.WeightedValue());
    }
}
