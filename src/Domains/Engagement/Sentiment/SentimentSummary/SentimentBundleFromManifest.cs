namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

using Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>Functions of sentiment bundle from manifest.</summary>
public static class SentimentBundleFromManifest
{
    public static Dictionary<string, double> SummarizeActionPlanManifestsForBundle(List<ActionPlanManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<SentimentBundle> DeriveSentimentBundlesFromManifest(List<ActionPlanManifest> items)
    {
        var outValue = new List<SentimentBundle>();
        foreach (var item in items)
        {
            var derived = SentimentBundle.NewSentimentBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeSentimentBundle(SentimentBundle item)
    {
        return ActionPlanManifestPolicy.NormalizeActionPlanManifestCode(item.Code);
    }
}
