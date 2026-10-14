namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

/// <summary>Functions of lenient recognition batch from preference.</summary>
public static class LenientRecognitionBatchFromPreference
{
    public static Dictionary<string, double> SummarizeManualActionPlanPreferencesForBatch(List<ManualActionPlanPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<LenientRecognitionBatch> DeriveLenientRecognitionBatchesFromPreference(List<ManualActionPlanPreference> items)
    {
        var outValue = new List<LenientRecognitionBatch>();
        foreach (var item in items)
        {
            var derived = LenientRecognitionBatch.NewLenientRecognitionBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLenientRecognitionBatch(LenientRecognitionBatch item)
    {
        return ManualActionPlanPreferencePolicy.NormalizeManualActionPlanPreferenceCode(item.Code);
    }
}
