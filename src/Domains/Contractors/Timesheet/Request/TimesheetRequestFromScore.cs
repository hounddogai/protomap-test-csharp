namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Contractors.Vendor.Score;

/// <summary>Functions of timesheet request from score.</summary>
public static class TimesheetRequestFromScore
{
    public static Dictionary<string, double> SummarizeVendorScoresForRequest(List<VendorScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<TimesheetRequest> DeriveTimesheetRequestsFromScore(List<VendorScore> items)
    {
        var outValue = new List<TimesheetRequest>();
        foreach (var item in items)
        {
            var derived = TimesheetRequest.NewTimesheetRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanTimesheetRequest(TimesheetRequest item)
    {
        return AdaptiveVendorAssignmentPolicy.AdaptiveVendorAssignmentWeight(4, true);
    }
}
