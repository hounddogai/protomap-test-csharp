namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Functions of batched training due tier from forecast.</summary>
public static class BatchedTrainingDueTierFromForecast
{
    public static Dictionary<string, double> SummarizeManualCertificationForecastsForTier(List<ManualCertificationForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<BatchedTrainingDueTier> DeriveBatchedTrainingDueTiersFromForecast(List<ManualCertificationForecast> items)
    {
        var outValue = new List<BatchedTrainingDueTier>();
        foreach (var item in items)
        {
            var derived = BatchedTrainingDueTier.NewBatchedTrainingDueTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchBatchedTrainingDueTier(BatchedTrainingDueTier item)
    {
        return CappedCertificationTemplatePolicy.CappedCertificationTemplateBucket(item.EffectiveVolume());
    }
}
