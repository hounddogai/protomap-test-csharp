namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Functions of primary pay band checkpoint from digest.</summary>
public static class PrimaryPayBandCheckpointFromDigest
{
    public static Dictionary<string, double> SummarizePendingPayBandDigestsForCheckpoint(List<PendingPayBandDigest> items)
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

    public static List<PrimaryPayBandCheckpoint> DerivePrimaryPayBandCheckpointsFromDigest(List<PendingPayBandDigest> items)
    {
        var outValue = new List<PrimaryPayBandCheckpoint>();
        foreach (var item in items)
        {
            var derived = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPrimaryPayBandCheckpoint(PrimaryPayBandCheckpoint item)
    {
        return BatchedPayBandTemplatePolicy.BatchedPayBandTemplateBucket(item.MeasuredValue());
    }
}
