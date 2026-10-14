namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Functions of audit agreement from snapshot.</summary>
public static class AuditAgreementFromSnapshot
{
    public static Dictionary<string, double> SummarizeCertificationSnapshotsForAgreement(List<CertificationSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<AuditAgreement> DeriveAuditAgreementsFromSnapshot(List<CertificationSnapshot> items)
    {
        var outValue = new List<AuditAgreement>();
        foreach (var item in items)
        {
            var derived = AuditAgreement.NewAuditAgreement(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeAuditAgreement(AuditAgreement item)
    {
        return CertificationAdjustmentPolicy.CertificationAdjustmentBucket(item.NetVolume());
    }
}
