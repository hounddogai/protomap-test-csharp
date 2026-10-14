namespace Acme.Workforce.Workplace.Desk.Allocation;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Functions of desk allocation from forecast.</summary>
public static class DeskAllocationFromForecast
{
    public static Dictionary<string, double> SummarizeCappedDeskForecastsForAllocation(List<CappedDeskForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<DeskAllocation> DeriveDeskAllocationsFromForecast(List<CappedDeskForecast> items)
    {
        var outValue = new List<DeskAllocation>();
        foreach (var item in items)
        {
            var derived = DeskAllocation.NewDeskAllocation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreDeskAllocation(DeskAllocation item)
    {
        return PendingDeskBaselinePolicy.PendingDeskBaselineWeight(2, true);
    }
}
