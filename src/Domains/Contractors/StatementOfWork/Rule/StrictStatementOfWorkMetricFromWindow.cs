namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Functions of strict statement of work metric from window.</summary>
public static class StrictStatementOfWorkMetricFromWindow
{
    public static Dictionary<string, double> SummarizePrimaryStatementOfWorkWindowsForMetric(List<PrimaryStatementOfWorkWindow> items)
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

    public static List<StrictStatementOfWorkMetric> DeriveStrictStatementOfWorkMetricsFromWindow(List<PrimaryStatementOfWorkWindow> items)
    {
        var outValue = new List<StrictStatementOfWorkMetric>();
        foreach (var item in items)
        {
            var derived = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitStrictStatementOfWorkMetric(StrictStatementOfWorkMetric item)
    {
        return LegacyStatementOfWorkRecordPolicy.ClassifyLegacyStatementOfWorkRecord(item.EffectiveLoad());
    }
}
