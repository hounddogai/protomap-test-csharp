namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Functions of batched attestation window from claim.</summary>
public static class BatchedAttestationWindowFromClaim
{
    public static Dictionary<string, double> SummarizeExceptionClaimsForWindow(List<ExceptionClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<BatchedAttestationWindow> DeriveBatchedAttestationWindowsFromClaim(List<ExceptionClaim> items)
    {
        var outValue = new List<BatchedAttestationWindow>();
        foreach (var item in items)
        {
            var derived = BatchedAttestationWindow.NewBatchedAttestationWindow(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeBatchedAttestationWindow(BatchedAttestationWindow item)
    {
        return SeasonalExceptionGroupPolicy.SeasonalExceptionGroupWeight(3, true);
    }
}
