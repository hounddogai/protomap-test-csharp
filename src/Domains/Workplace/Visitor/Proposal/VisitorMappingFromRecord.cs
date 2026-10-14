namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Functions of visitor mapping from record.</summary>
public static class VisitorMappingFromRecord
{
    public static Dictionary<string, double> SummarizeSeasonalFloorPlanRecordsForMapping(List<SeasonalFloorPlanRecord> items)
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

    public static List<VisitorMapping> DeriveVisitorMappingsFromRecord(List<SeasonalFloorPlanRecord> items)
    {
        var outValue = new List<VisitorMapping>();
        foreach (var item in items)
        {
            var derived = VisitorMapping.NewVisitorMapping(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignVisitorMapping(VisitorMapping item)
    {
        return FloorPlanAssignmentPolicy.ClassifyFloorPlanAssignment(item.ProjectedValue());
    }
}
