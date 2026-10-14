namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Functions of audit segment from draft.</summary>
public static class AuditSegmentFromDraft
{
    public static Dictionary<string, double> SummarizeCertificationDraftsForSegment(List<CertificationDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<AuditSegment> DeriveAuditSegmentsFromDraft(List<CertificationDraft> items)
    {
        var outValue = new List<AuditSegment>();
        foreach (var item in items)
        {
            var derived = AuditSegment.NewAuditSegment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeAuditSegment(AuditSegment item)
    {
        return CertificationScorePolicy.NormalizeCertificationScoreCode(item.Code);
    }
}
