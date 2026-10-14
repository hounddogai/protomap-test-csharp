namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>Functions of assignment schedule from target.</summary>
public static class AssignmentScheduleFromTarget
{
    public static Dictionary<string, double> SummarizeSeasonalWarrantyTargetsForSchedule(List<SeasonalWarrantyTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<AssignmentSchedule> DeriveAssignmentSchedulesFromTarget(List<SeasonalWarrantyTarget> items)
    {
        var outValue = new List<AssignmentSchedule>();
        foreach (var item in items)
        {
            var derived = AssignmentSchedule.NewAssignmentSchedule(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAssignmentSchedule(AssignmentSchedule item)
    {
        return SeasonalWarrantyTargetPolicy.ClassifySeasonalWarrantyTarget(item.MeasuredValue());
    }
}
