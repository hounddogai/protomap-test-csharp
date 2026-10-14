namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Functions of fallback attestation agreement from notice.</summary>
public static class FallbackAttestationAgreementFromNotice
{
    public static Dictionary<string, double> SummarizeManualTrainingDueNoticesForAgreement(List<ManualTrainingDueNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<FallbackAttestationAgreement> DeriveFallbackAttestationAgreementsFromNotice(List<ManualTrainingDueNotice> items)
    {
        var outValue = new List<FallbackAttestationAgreement>();
        foreach (var item in items)
        {
            var derived = FallbackAttestationAgreement.NewFallbackAttestationAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeFallbackAttestationAgreement(FallbackAttestationAgreement item)
    {
        return AdaptiveTrainingDueClaimPolicy.ClassifyAdaptiveTrainingDueClaim(item.ProjectedVolume());
    }
}
