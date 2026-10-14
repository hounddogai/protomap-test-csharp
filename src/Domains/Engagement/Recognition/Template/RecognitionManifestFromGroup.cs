namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Functions of recognition manifest from group.</summary>
public static class RecognitionManifestFromGroup
{
    public static Dictionary<string, double> SummarizeTownHallGroupsForManifest(List<TownHallGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<RecognitionManifest> DeriveRecognitionManifestsFromGroup(List<TownHallGroup> items)
    {
        var outValue = new List<RecognitionManifest>();
        foreach (var item in items)
        {
            var derived = RecognitionManifest.NewRecognitionManifest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitRecognitionManifest(RecognitionManifest item)
    {
        return PrimaryTownHallRulePolicy.PrimaryTownHallRuleWeight(1, true);
    }
}
