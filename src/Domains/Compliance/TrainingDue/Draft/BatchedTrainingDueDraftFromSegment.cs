namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>Functions of batched training due draft from segment.</summary>
public static class BatchedTrainingDueDraftFromSegment
{
    public static Dictionary<string, double> SummarizeAttestationSegmentsForDraft(List<AttestationSegment> items)
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

    public static List<BatchedTrainingDueDraft> DeriveBatchedTrainingDueDraftsFromSegment(List<AttestationSegment> items)
    {
        var outValue = new List<BatchedTrainingDueDraft>();
        foreach (var item in items)
        {
            var derived = BatchedTrainingDueDraft.NewBatchedTrainingDueDraft(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignBatchedTrainingDueDraft(BatchedTrainingDueDraft item)
    {
        return AttestationRecordPolicy.AttestationRecordWeight(5, true);
    }
}
