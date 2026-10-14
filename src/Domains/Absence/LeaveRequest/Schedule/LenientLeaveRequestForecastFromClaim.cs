namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Functions of lenient leave request forecast from claim.</summary>
public static class LenientLeaveRequestForecastFromClaim
{
    public static Dictionary<string, double> SummarizeStrictLeaveRequestClaimsForForecast(List<StrictLeaveRequestClaim> items)
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

    public static List<LenientLeaveRequestForecast> DeriveLenientLeaveRequestForecastsFromClaim(List<StrictLeaveRequestClaim> items)
    {
        var outValue = new List<LenientLeaveRequestForecast>();
        foreach (var item in items)
        {
            var derived = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeLenientLeaveRequestForecast(LenientLeaveRequestForecast item)
    {
        return AdaptiveLeaveRequestPolicyPolicy.ClassifyAdaptiveLeaveRequestPolicy(item.WeightedTotal());
    }
}
