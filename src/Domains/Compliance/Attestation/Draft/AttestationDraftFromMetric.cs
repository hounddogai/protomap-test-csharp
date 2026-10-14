namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Functions of attestation draft from metric.</summary>
public static class AttestationDraftFromMetric
{
    public static Dictionary<string, double> SummarizeTrainingDueMetricsForDraft(List<TrainingDueMetric> items)
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

    public static List<AttestationDraft> DeriveAttestationDraftsFromMetric(List<TrainingDueMetric> items)
    {
        var outValue = new List<AttestationDraft>();
        foreach (var item in items)
        {
            var derived = AttestationDraft.NewAttestationDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankAttestationDraft(AttestationDraft item)
    {
        return TrainingDueBundlePolicy.TrainingDueBundleBucket(item.WeightedTotal());
    }
}
