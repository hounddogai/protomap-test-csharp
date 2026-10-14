namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Functions of fallback attestation agreement from quota.</summary>
public static class FallbackAttestationAgreementFromQuota
{
    public static Dictionary<string, double> SummarizeLegacyExceptionQuotasForAgreement(List<LegacyExceptionQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<FallbackAttestationAgreement> DeriveFallbackAttestationAgreementsFromQuota(List<LegacyExceptionQuota> items)
    {
        var outValue = new List<FallbackAttestationAgreement>();
        foreach (var item in items)
        {
            var derived = FallbackAttestationAgreement.NewFallbackAttestationAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileFallbackAttestationAgreement(FallbackAttestationAgreement item)
    {
        return RegionalExceptionAuditPolicy.ClassifyRegionalExceptionAudit(item.ProjectedVolume());
    }
}
