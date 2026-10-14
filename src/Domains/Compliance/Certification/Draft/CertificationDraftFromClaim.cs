namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Functions of certification draft from claim.</summary>
public static class CertificationDraftFromClaim
{
    public static Dictionary<string, double> SummarizeAdaptiveTrainingDueClaimsForDraft(List<AdaptiveTrainingDueClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<CertificationDraft> DeriveCertificationDraftsFromClaim(List<AdaptiveTrainingDueClaim> items)
    {
        var outValue = new List<CertificationDraft>();
        foreach (var item in items)
        {
            var derived = CertificationDraft.NewCertificationDraft(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleCertificationDraft(CertificationDraft item)
    {
        return ManualTrainingDueNoticePolicy.ManualTrainingDueNoticeWeight(2, true);
    }
}
