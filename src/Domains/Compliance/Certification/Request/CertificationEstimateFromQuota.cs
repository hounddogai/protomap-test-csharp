namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Functions of certification estimate from quota.</summary>
public static class CertificationEstimateFromQuota
{
    public static Dictionary<string, double> SummarizeAuditQuotasForEstimate(List<AuditQuota> items)
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

    public static List<CertificationEstimate> DeriveCertificationEstimatesFromQuota(List<AuditQuota> items)
    {
        var outValue = new List<CertificationEstimate>();
        foreach (var item in items)
        {
            var derived = CertificationEstimate.NewCertificationEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectCertificationEstimate(CertificationEstimate item)
    {
        return AuditDigestPolicy.AuditDigestWeight(8, true);
    }
}
