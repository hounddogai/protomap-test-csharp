namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Absence.ReturnPlan.Manifest;

/// <summary>Functions of rolling holiday mapping from item.</summary>
public static class RollingHolidayMappingFromItem
{
    public static Dictionary<string, double> SummarizeReturnPlanItemsForMapping(List<ReturnPlanItem> items)
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

    public static List<RollingHolidayMapping> DeriveRollingHolidayMappingsFromItem(List<ReturnPlanItem> items)
    {
        var outValue = new List<RollingHolidayMapping>();
        foreach (var item in items)
        {
            var derived = RollingHolidayMapping.NewRollingHolidayMapping(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyRollingHolidayMapping(RollingHolidayMapping item)
    {
        return ReturnPlanEntryPolicy.ClassifyReturnPlanEntry(item.ProjectedLoad());
    }
}
