namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Functions of batched exception record from exception.</summary>
public static class BatchedExceptionRecordFromException
{
    public static Dictionary<string, double> SummarizeAttestationExceptionsForRecord(List<AttestationException> items)
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

    public static List<BatchedExceptionRecord> DeriveBatchedExceptionRecordsFromException(List<AttestationException> items)
    {
        var outValue = new List<BatchedExceptionRecord>();
        foreach (var item in items)
        {
            var derived = BatchedExceptionRecord.NewBatchedExceptionRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectBatchedExceptionRecord(BatchedExceptionRecord item)
    {
        return PendingAttestationSegmentPolicy.ClassifyPendingAttestationSegment(item.ProjectedLoad());
    }
}
