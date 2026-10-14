namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Functions of statement of work variance from allocation.</summary>
public static class StatementOfWorkVarianceFromAllocation
{
    public static Dictionary<string, double> SummarizeFallbackRateAllocationsForVariance(List<FallbackRateAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<StatementOfWorkVariance> DeriveStatementOfWorkVariancesFromAllocation(List<FallbackRateAllocation> items)
    {
        var outValue = new List<StatementOfWorkVariance>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkVariance.NewStatementOfWorkVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeStatementOfWorkVariance(StatementOfWorkVariance item)
    {
        return RateVariancePolicy.RateVarianceWeight(6, true);
    }
}
