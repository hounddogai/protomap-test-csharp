namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>Functions of attestation segment from draft.</summary>
public static class AttestationSegmentFromDraft
{
    public static Dictionary<string, double> SummarizeExceptionDraftsForSegment(List<ExceptionDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<AttestationSegment> DeriveAttestationSegmentsFromDraft(List<ExceptionDraft> items)
    {
        var outValue = new List<AttestationSegment>();
        foreach (var item in items)
        {
            var derived = AttestationSegment.NewAttestationSegment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAttestationSegment(AttestationSegment item)
    {
        return ExceptionPolicyPolicy.ClassifyExceptionPolicy(item.NetValue());
    }
}
