namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Functions of legacy action plan metric from manifest.</summary>
public static class LegacyActionPlanMetricFromManifest
{
    public static Dictionary<string, double> SummarizeRecognitionManifestsForMetric(List<RecognitionManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<LegacyActionPlanMetric> DeriveLegacyActionPlanMetricsFromManifest(List<RecognitionManifest> items)
    {
        var outValue = new List<LegacyActionPlanMetric>();
        foreach (var item in items)
        {
            var derived = LegacyActionPlanMetric.NewLegacyActionPlanMetric(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleLegacyActionPlanMetric(LegacyActionPlanMetric item)
    {
        return RecognitionManifestPolicy.NormalizeRecognitionManifestCode(item.Code);
    }
}
