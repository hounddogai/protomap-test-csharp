namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Contractors.Timesheet.Exception;

/// <summary>Functions of scheduled engagement review from manifest.</summary>
public static class ScheduledEngagementReviewFromManifest
{
    public static Dictionary<string, double> SummarizeAdaptiveTimesheetManifestsForReview(List<AdaptiveTimesheetManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<ScheduledEngagementReview> DeriveScheduledEngagementReviewsFromManifest(List<AdaptiveTimesheetManifest> items)
    {
        var outValue = new List<ScheduledEngagementReview>();
        foreach (var item in items)
        {
            var derived = ScheduledEngagementReview.NewScheduledEngagementReview(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveScheduledEngagementReview(ScheduledEngagementReview item)
    {
        return AdaptiveTimesheetManifestPolicy.NormalizeAdaptiveTimesheetManifestCode(item.Code);
    }
}
