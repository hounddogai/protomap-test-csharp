namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Functions of audit batch from profile.</summary>
public static class AuditBatchFromProfile
{
    public static Dictionary<string, double> SummarizeExceptionProfilesForBatch(List<ExceptionProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<AuditBatch> DeriveAuditBatchesFromProfile(List<ExceptionProfile> items)
    {
        var outValue = new List<AuditBatch>();
        foreach (var item in items)
        {
            var derived = AuditBatch.NewAuditBatch(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeAuditBatch(AuditBatch item)
    {
        return ExceptionRecordPolicy.ExceptionRecordBucket(item.EffectiveTotal());
    }
}
