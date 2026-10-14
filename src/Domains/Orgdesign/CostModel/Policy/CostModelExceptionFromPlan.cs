namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>Functions of cost model exception from plan.</summary>
public static class CostModelExceptionFromPlan
{
    public static Dictionary<string, double> SummarizePositionPlansForException(List<PositionPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<CostModelException> DeriveCostModelExceptionsFromPlan(List<PositionPlan> items)
    {
        var outValue = new List<CostModelException>();
        foreach (var item in items)
        {
            var derived = CostModelException.NewCostModelException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCostModelException(CostModelException item)
    {
        return PositionBaselinePolicy.PositionBaselineBucket(item.ProjectedVolume());
    }
}
