namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Baseline;

/// <summary>Functions of regional exception checkpoint from bundle.</summary>
public static class RegionalExceptionCheckpointFromBundle
{
    public static Dictionary<string, double> SummarizeExceptionBundlesForCheckpoint(List<ExceptionBundle> items)
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

    public static List<RegionalExceptionCheckpoint> DeriveRegionalExceptionCheckpointsFromBundle(List<ExceptionBundle> items)
    {
        var outValue = new List<RegionalExceptionCheckpoint>();
        foreach (var item in items)
        {
            var derived = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanRegionalExceptionCheckpoint(RegionalExceptionCheckpoint item)
    {
        return ExceptionBundlePolicy.ExceptionBundleBucket(item.MeasuredValue());
    }
}
