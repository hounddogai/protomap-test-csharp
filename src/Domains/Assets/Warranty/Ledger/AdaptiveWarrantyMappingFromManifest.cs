namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Functions of adaptive warranty mapping from manifest.</summary>
public static class AdaptiveWarrantyMappingFromManifest
{
    public static Dictionary<string, double> SummarizeAssignmentManifestsForMapping(List<AssignmentManifest> items)
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

    public static List<AdaptiveWarrantyMapping> DeriveAdaptiveWarrantyMappingsFromManifest(List<AssignmentManifest> items)
    {
        var outValue = new List<AdaptiveWarrantyMapping>();
        foreach (var item in items)
        {
            var derived = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAdaptiveWarrantyMapping(AdaptiveWarrantyMapping item)
    {
        return ScheduledAssignmentEstimatePolicy.NormalizeScheduledAssignmentEstimateCode(item.Code);
    }
}
