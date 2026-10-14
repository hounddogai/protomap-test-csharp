namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Engagement.PulseSurvey.Batch;

/// <summary>Functions of pulse survey tier from batch.</summary>
public static class PulseSurveyTierFromBatch
{
    public static Dictionary<string, double> SummarizePulseSurveyBatchesForTier(List<PulseSurveyBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<PulseSurveyTier> DerivePulseSurveyTiersFromBatch(List<PulseSurveyBatch> items)
    {
        var outValue = new List<PulseSurveyTier>();
        foreach (var item in items)
        {
            var derived = PulseSurveyTier.NewPulseSurveyTier(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DerivePulseSurveyTier(PulseSurveyTier item)
    {
        return PulseSurveyBatchPolicy.PulseSurveyBatchWeight(7, true);
    }
}
