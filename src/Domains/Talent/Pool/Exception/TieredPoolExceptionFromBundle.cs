namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Functions of tiered pool exception from bundle.</summary>
public static class TieredPoolExceptionFromBundle
{
    public static Dictionary<string, double> SummarizePoolBundlesForException(List<PoolBundle> items)
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

    public static List<TieredPoolException> DeriveTieredPoolExceptionsFromBundle(List<PoolBundle> items)
    {
        var outValue = new List<TieredPoolException>();
        foreach (var item in items)
        {
            var derived = TieredPoolException.NewTieredPoolException(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveTieredPoolException(TieredPoolException item)
    {
        return StrictPoolRulePolicy.StrictPoolRuleWeight(1, true);
    }
}
