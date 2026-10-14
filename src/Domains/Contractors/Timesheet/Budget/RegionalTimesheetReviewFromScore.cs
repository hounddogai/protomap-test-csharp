namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Score;

/// <summary>Functions of regional timesheet review from score.</summary>
public static class RegionalTimesheetReviewFromScore
{
    public static Dictionary<string, double> SummarizeScheduledTimesheetScoresForReview(List<ScheduledTimesheetScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<RegionalTimesheetReview> DeriveRegionalTimesheetReviewsFromScore(List<ScheduledTimesheetScore> items)
    {
        var outValue = new List<RegionalTimesheetReview>();
        foreach (var item in items)
        {
            var derived = RegionalTimesheetReview.NewRegionalTimesheetReview(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeRegionalTimesheetReview(RegionalTimesheetReview item)
    {
        return ScheduledTimesheetScorePolicy.ClassifyScheduledTimesheetScore(item.MeasuredValue());
    }
}
