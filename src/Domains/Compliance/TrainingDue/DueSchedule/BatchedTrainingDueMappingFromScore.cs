namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.Audit.Slot;

/// <summary>Functions of batched training due mapping from score.</summary>
public static class BatchedTrainingDueMappingFromScore
{
    public static Dictionary<string, double> SummarizeAuditScoresForMapping(List<AuditScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<BatchedTrainingDueMapping> DeriveBatchedTrainingDueMappingsFromScore(List<AuditScore> items)
    {
        var outValue = new List<BatchedTrainingDueMapping>();
        foreach (var item in items)
        {
            var derived = BatchedTrainingDueMapping.NewBatchedTrainingDueMapping(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchBatchedTrainingDueMapping(BatchedTrainingDueMapping item)
    {
        return AuditScorePolicy.NormalizeAuditScoreCode(item.Code);
    }
}
