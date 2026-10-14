namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Functions of bonus pool forecast from exception.</summary>
public static class BonusPoolForecastFromException
{
    public static Dictionary<string, double> SummarizeEquityGrantExceptionsForForecast(List<EquityGrantException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<BonusPoolForecast> DeriveBonusPoolForecastsFromException(List<EquityGrantException> items)
    {
        var outValue = new List<BonusPoolForecast>();
        foreach (var item in items)
        {
            var derived = BonusPoolForecast.NewBonusPoolForecast(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeBonusPoolForecast(BonusPoolForecast item)
    {
        return LenientEquityGrantSummaryPolicy.LenientEquityGrantSummaryBucket(item.AdjustedLoad());
    }
}
