namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Engagement.TownHall.Score;

/// <summary>Functions of regional recognition snapshot from group.</summary>
public static class RegionalRecognitionSnapshotFromGroup
{
    public static Dictionary<string, double> SummarizeRegionalTownHallGroupsForSnapshot(List<RegionalTownHallGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RegionalRecognitionSnapshot> DeriveRegionalRecognitionSnapshotsFromGroup(List<RegionalTownHallGroup> items)
    {
        var outValue = new List<RegionalRecognitionSnapshot>();
        foreach (var item in items)
        {
            var derived = RegionalRecognitionSnapshot.NewRegionalRecognitionSnapshot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRegionalRecognitionSnapshot(RegionalRecognitionSnapshot item)
    {
        return AdaptiveTownHallScorePolicy.ClassifyAdaptiveTownHallScore(item.NetTotal());
    }
}
