namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Functions of occupancy profile from adjustment.</summary>
public static class OccupancyProfileFromAdjustment
{
    public static Dictionary<string, double> SummarizeGlobalVisitorAdjustmentsForProfile(List<GlobalVisitorAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<OccupancyProfile> DeriveOccupancyProfilesFromAdjustment(List<GlobalVisitorAdjustment> items)
    {
        var outValue = new List<OccupancyProfile>();
        foreach (var item in items)
        {
            var derived = OccupancyProfile.NewOccupancyProfile(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectOccupancyProfile(OccupancyProfile item)
    {
        return CappedVisitorAssignmentPolicy.ClassifyCappedVisitorAssignment(item.ProjectedValue());
    }
}
