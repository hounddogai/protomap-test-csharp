namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Functions of rate estimate from summary.</summary>
public static class RateEstimateFromSummary
{
    public static Dictionary<string, double> SummarizePendingTimesheetSummariesForEstimate(List<PendingTimesheetSummary> items)
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

    public static List<RateEstimate> DeriveRateEstimatesFromSummary(List<PendingTimesheetSummary> items)
    {
        var outValue = new List<RateEstimate>();
        foreach (var item in items)
        {
            var derived = RateEstimate.NewRateEstimate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignRateEstimate(RateEstimate item)
    {
        return TimesheetTierPolicy.ClassifyTimesheetTier(item.NetValue());
    }
}
