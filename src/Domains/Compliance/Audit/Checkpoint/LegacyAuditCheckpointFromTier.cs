namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Functions of legacy audit checkpoint from tier.</summary>
public static class LegacyAuditCheckpointFromTier
{
    public static Dictionary<string, double> SummarizeTrainingDueTiersForCheckpoint(List<TrainingDueTier> items)
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

    public static List<LegacyAuditCheckpoint> DeriveLegacyAuditCheckpointsFromTier(List<TrainingDueTier> items)
    {
        var outValue = new List<LegacyAuditCheckpoint>();
        foreach (var item in items)
        {
            var derived = LegacyAuditCheckpoint.NewLegacyAuditCheckpoint(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyLegacyAuditCheckpointExtra(LegacyAuditCheckpoint item)
    {
        return TrainingDueDigestPolicy.TrainingDueDigestWeight(6, true);
    }
}
