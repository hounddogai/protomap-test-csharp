namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Functions of statement of work cycle from request.</summary>
public static class StatementOfWorkCycleFromRequest
{
    public static Dictionary<string, double> SummarizeWeightedRateRequestsForCycle(List<WeightedRateRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<StatementOfWorkCycle> DeriveStatementOfWorkCyclesFromRequest(List<WeightedRateRequest> items)
    {
        var outValue = new List<StatementOfWorkCycle>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkCycle.NewStatementOfWorkCycle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyStatementOfWorkCycle(StatementOfWorkCycle item)
    {
        return WeightedRateRequestPolicy.ClassifyWeightedRateRequest(item.NetExposure());
    }
}
