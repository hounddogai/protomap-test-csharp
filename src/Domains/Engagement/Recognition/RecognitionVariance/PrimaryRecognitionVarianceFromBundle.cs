namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Functions of primary recognition variance from bundle.</summary>
public static class PrimaryRecognitionVarianceFromBundle
{
    public static Dictionary<string, double> SummarizeTownHallBundlesForVariance(List<TownHallBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<PrimaryRecognitionVariance> DerivePrimaryRecognitionVariancesFromBundle(List<TownHallBundle> items)
    {
        var outValue = new List<PrimaryRecognitionVariance>();
        foreach (var item in items)
        {
            var derived = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPrimaryRecognitionVariance(PrimaryRecognitionVariance item)
    {
        return FallbackTownHallDraftPolicy.ClassifyFallbackTownHallDraft(item.WeightedLoad());
    }
}
