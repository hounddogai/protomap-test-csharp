namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Orgdesign.CostModel.Policy;

/// <summary>Functions of regional position metric from exception.</summary>
public static class RegionalPositionMetricFromException
{
    public static Dictionary<string, double> SummarizeCostModelExceptionsForMetric(List<CostModelException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RegionalPositionMetric> DeriveRegionalPositionMetricsFromException(List<CostModelException> items)
    {
        var outValue = new List<RegionalPositionMetric>();
        foreach (var item in items)
        {
            var derived = RegionalPositionMetric.NewRegionalPositionMetric(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRegionalPositionMetric(RegionalPositionMetric item)
    {
        return CostModelPolicyPolicy.NormalizeCostModelPolicyCode(item.Code);
    }
}
