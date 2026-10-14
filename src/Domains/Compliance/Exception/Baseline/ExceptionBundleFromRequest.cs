namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Functions of exception bundle from request.</summary>
public static class ExceptionBundleFromRequest
{
    public static Dictionary<string, double> SummarizeAttestationRequestsForBundle(List<AttestationRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<ExceptionBundle> DeriveExceptionBundlesFromRequest(List<AttestationRequest> items)
    {
        var outValue = new List<ExceptionBundle>();
        foreach (var item in items)
        {
            var derived = ExceptionBundle.NewExceptionBundle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateExceptionBundle(ExceptionBundle item)
    {
        return AttestationProposalPolicy.NormalizeAttestationProposalCode(item.Code);
    }
}
