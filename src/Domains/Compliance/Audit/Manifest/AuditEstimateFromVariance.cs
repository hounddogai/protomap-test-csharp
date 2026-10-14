namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Functions of audit estimate from variance.</summary>
public static class AuditEstimateFromVariance
{
    public static Dictionary<string, double> SummarizeTrainingDueVariancesForEstimate(List<TrainingDueVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<AuditEstimate> DeriveAuditEstimatesFromVariance(List<TrainingDueVariance> items)
    {
        var outValue = new List<AuditEstimate>();
        foreach (var item in items)
        {
            var derived = AuditEstimate.NewAuditEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAuditEstimate(AuditEstimate item)
    {
        return TrainingDueGroupPolicy.ClassifyTrainingDueGroup(item.NetVolume());
    }
}
