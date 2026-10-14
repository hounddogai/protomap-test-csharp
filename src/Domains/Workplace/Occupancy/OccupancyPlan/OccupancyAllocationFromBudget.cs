namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Visitor.Exception;

/// <summary>Functions of occupancy allocation from budget.</summary>
public static class OccupancyAllocationFromBudget
{
    public static Dictionary<string, double> SummarizeVisitorBudgetsForAllocation(List<VisitorBudget> items)
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

    public static List<OccupancyAllocation> DeriveOccupancyAllocationsFromBudget(List<VisitorBudget> items)
    {
        var outValue = new List<OccupancyAllocation>();
        foreach (var item in items)
        {
            var derived = OccupancyAllocation.NewOccupancyAllocation(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankOccupancyAllocation(OccupancyAllocation item)
    {
        return LenientVisitorExceptionPolicy.NormalizeLenientVisitorExceptionCode(item.Code);
    }
}
