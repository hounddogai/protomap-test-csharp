namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Compliance.Certification.Notice;

/// <summary>Functions of pending training due ledger from batch.</summary>
public static class PendingTrainingDueLedgerFromBatch
{
    public static Dictionary<string, double> SummarizeWeightedCertificationBatchesForLedger(List<WeightedCertificationBatch> items)
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

    public static List<PendingTrainingDueLedger> DerivePendingTrainingDueLedgersFromBatch(List<WeightedCertificationBatch> items)
    {
        var outValue = new List<PendingTrainingDueLedger>();
        foreach (var item in items)
        {
            var derived = PendingTrainingDueLedger.NewPendingTrainingDueLedger(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPendingTrainingDueLedger(PendingTrainingDueLedger item)
    {
        return WeightedCertificationBatchPolicy.NormalizeWeightedCertificationBatchCode(item.Code);
    }
}
