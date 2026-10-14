namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Functions of lenient statement of work bundle from override.</summary>
public static class LenientStatementOfWorkBundleFromOverride
{
    public static Dictionary<string, double> SummarizeRateOverridesForBundle(List<RateOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<LenientStatementOfWorkBundle> DeriveLenientStatementOfWorkBundlesFromOverride(List<RateOverride> items)
    {
        var outValue = new List<LenientStatementOfWorkBundle>();
        foreach (var item in items)
        {
            var derived = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLenientStatementOfWorkBundle(LenientStatementOfWorkBundle item)
    {
        return RollingRateVariancePolicy.ClassifyRollingRateVariance(item.MeasuredValue());
    }
}
