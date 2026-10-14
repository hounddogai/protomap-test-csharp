namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Functions of warranty mapping from notice.</summary>
public static class WarrantyMappingFromNotice
{
    public static Dictionary<string, double> SummarizeAssignmentNoticesForMapping(List<AssignmentNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<WarrantyMapping> DeriveWarrantyMappingsFromNotice(List<AssignmentNotice> items)
    {
        var outValue = new List<WarrantyMapping>();
        foreach (var item in items)
        {
            var derived = WarrantyMapping.NewWarrantyMapping(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyWarrantyMapping(WarrantyMapping item)
    {
        return AssignmentCheckpointPolicy.AssignmentCheckpointBucket(item.ProjectedLoad());
    }
}
