namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Functions of batched audit summary from proposal.</summary>
public static class BatchedAuditSummaryFromProposal
{
    public static Dictionary<string, double> SummarizeTrainingDueProposalsForSummary(List<TrainingDueProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<BatchedAuditSummary> DeriveBatchedAuditSummariesFromProposal(List<TrainingDueProposal> items)
    {
        var outValue = new List<BatchedAuditSummary>();
        foreach (var item in items)
        {
            var derived = BatchedAuditSummary.NewBatchedAuditSummary(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitBatchedAuditSummary(BatchedAuditSummary item)
    {
        return AdaptiveTrainingDueExceptionPolicy.ClassifyAdaptiveTrainingDueException(item.WeightedTotal());
    }
}
