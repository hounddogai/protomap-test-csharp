namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Functions of position variance from metric.</summary>
public static class PositionVarianceFromMetric
{
    public static Dictionary<string, double> SummarizePositionMetricsForVariance(List<PositionMetric> items)
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

    public static List<PositionVariance> DerivePositionVariancesFromMetric(List<PositionMetric> items)
    {
        var outValue = new List<PositionVariance>();
        foreach (var item in items)
        {
            var derived = PositionVariance.NewPositionVariance(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPositionVariance(PositionVariance item)
    {
        return PositionAssignmentPolicy.NormalizePositionAssignmentCode(item.Code);
    }
}
