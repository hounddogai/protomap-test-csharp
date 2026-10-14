namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>Functions of batched training due notice from preference.</summary>
public static class BatchedTrainingDueNoticeFromPreference
{
    public static Dictionary<string, double> SummarizeRegionalAttestationPreferencesForNotice(List<RegionalAttestationPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<BatchedTrainingDueNotice> DeriveBatchedTrainingDueNoticesFromPreference(List<RegionalAttestationPreference> items)
    {
        var outValue = new List<BatchedTrainingDueNotice>();
        foreach (var item in items)
        {
            var derived = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleBatchedTrainingDueNotice(BatchedTrainingDueNotice item)
    {
        return RegionalAttestationPreferencePolicy.ClassifyRegionalAttestationPreference(item.MeasuredValue());
    }
}
