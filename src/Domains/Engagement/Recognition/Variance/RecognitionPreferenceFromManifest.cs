namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Functions of recognition preference from manifest.</summary>
public static class RecognitionPreferenceFromManifest
{
    public static Dictionary<string, double> SummarizeTownHallManifestsForPreference(List<TownHallManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<RecognitionPreference> DeriveRecognitionPreferencesFromManifest(List<TownHallManifest> items)
    {
        var outValue = new List<RecognitionPreference>();
        foreach (var item in items)
        {
            var derived = RecognitionPreference.NewRecognitionPreference(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRecognitionPreference(RecognitionPreference item)
    {
        return TownHallMetricPolicy.ClassifyTownHallMetric(item.NetExposure());
    }
}
