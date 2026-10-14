namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Functions of lenient timesheet score from manifest.</summary>
public static class LenientTimesheetScoreFromManifest
{
    public static Dictionary<string, double> SummarizeEngagementManifestsForScore(List<EngagementManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LenientTimesheetScore> DeriveLenientTimesheetScoresFromManifest(List<EngagementManifest> items)
    {
        var outValue = new List<LenientTimesheetScore>();
        foreach (var item in items)
        {
            var derived = LenientTimesheetScore.NewLenientTimesheetScore(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLenientTimesheetScore(LenientTimesheetScore item)
    {
        return EngagementProfilePolicy.ClassifyEngagementProfile(item.ProjectedLoad());
    }
}
