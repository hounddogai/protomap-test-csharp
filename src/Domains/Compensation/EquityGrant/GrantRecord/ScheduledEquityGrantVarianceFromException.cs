namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Functions of scheduled equity grant variance from exception.</summary>
public static class ScheduledEquityGrantVarianceFromException
{
    public static Dictionary<string, double> SummarizeBonusPoolExceptionsForVariance(List<BonusPoolException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<ScheduledEquityGrantVariance> DeriveScheduledEquityGrantVariancesFromException(List<BonusPoolException> items)
    {
        var outValue = new List<ScheduledEquityGrantVariance>();
        foreach (var item in items)
        {
            var derived = ScheduledEquityGrantVariance.NewScheduledEquityGrantVariance(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankScheduledEquityGrantVariance(ScheduledEquityGrantVariance item)
    {
        return FallbackBonusPoolProfilePolicy.FallbackBonusPoolProfileWeight(3, true);
    }
}
