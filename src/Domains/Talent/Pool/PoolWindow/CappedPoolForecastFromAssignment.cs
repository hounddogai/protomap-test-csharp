namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>Functions of capped pool forecast from assignment.</summary>
public static class CappedPoolForecastFromAssignment
{
    public static Dictionary<string, double> SummarizeDefaultNominationAssignmentsForForecast(List<DefaultNominationAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<CappedPoolForecast> DeriveCappedPoolForecastsFromAssignment(List<DefaultNominationAssignment> items)
    {
        var outValue = new List<CappedPoolForecast>();
        foreach (var item in items)
        {
            var derived = CappedPoolForecast.NewCappedPoolForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateCappedPoolForecast(CappedPoolForecast item)
    {
        return DefaultNominationAssignmentPolicy.NormalizeDefaultNominationAssignmentCode(item.Code);
    }
}
