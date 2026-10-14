namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Compliance.Audit.Review;

/// <summary>Functions of exception baseline from checkpoint.</summary>
public static class ExceptionBaselineFromCheckpoint
{
    public static Dictionary<string, double> SummarizeDefaultAuditCheckpointsForBaseline(List<DefaultAuditCheckpoint> items)
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

    public static List<ExceptionBaseline> DeriveExceptionBaselinesFromCheckpoint(List<DefaultAuditCheckpoint> items)
    {
        var outValue = new List<ExceptionBaseline>();
        foreach (var item in items)
        {
            var derived = ExceptionBaseline.NewExceptionBaseline(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchExceptionBaseline(ExceptionBaseline item)
    {
        return DefaultAuditCheckpointPolicy.ClassifyDefaultAuditCheckpoint(item.AdjustedExposure());
    }
}
