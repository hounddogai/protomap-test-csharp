namespace Acme.Workforce.Talent.CareerPath.Forecast;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Functions of career path tier from exception.</summary>
public static class CareerPathTierFromException
{
    public static Dictionary<string, double> SummarizePoolExceptionsForTier(List<PoolException> items)
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

    public static List<CareerPathTier> DeriveCareerPathTiersFromException(List<PoolException> items)
    {
        var outValue = new List<CareerPathTier>();
        foreach (var item in items)
        {
            var derived = CareerPathTier.NewCareerPathTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanCareerPathTier(CareerPathTier item)
    {
        return TieredPoolEntryPolicy.TieredPoolEntryBucket(item.AdjustedLoad());
    }
}
