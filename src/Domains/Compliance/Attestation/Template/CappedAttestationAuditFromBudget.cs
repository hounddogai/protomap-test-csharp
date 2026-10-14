namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Functions of capped attestation audit from budget.</summary>
public static class CappedAttestationAuditFromBudget
{
    public static Dictionary<string, double> SummarizeCertificationBudgetsForAudit(List<CertificationBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<CappedAttestationAudit> DeriveCappedAttestationAuditsFromBudget(List<CertificationBudget> items)
    {
        var outValue = new List<CappedAttestationAudit>();
        foreach (var item in items)
        {
            var derived = CappedAttestationAudit.NewCappedAttestationAudit(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankCappedAttestationAudit(CappedAttestationAudit item)
    {
        return TieredCertificationCheckpointPolicy.TieredCertificationCheckpointBucket(item.WeightedValue());
    }
}
