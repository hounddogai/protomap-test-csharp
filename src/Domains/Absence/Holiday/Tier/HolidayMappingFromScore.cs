namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Functions of holiday mapping from score.</summary>
public static class HolidayMappingFromScore
{
    public static Dictionary<string, double> SummarizeLeaveRequestScoresForMapping(List<LeaveRequestScore> items)
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

    public static List<HolidayMapping> DeriveHolidayMappingsFromScore(List<LeaveRequestScore> items)
    {
        var outValue = new List<HolidayMapping>();
        foreach (var item in items)
        {
            var derived = HolidayMapping.NewHolidayMapping(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectHolidayMapping(HolidayMapping item)
    {
        return LeaveRequestScorePolicy.ClassifyLeaveRequestScore(item.AdjustedVolume());
    }
}
