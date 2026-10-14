namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Assets.Refresh.Mapping;

/// <summary>Functions of warranty profile from mapping.</summary>
public static class WarrantyProfileFromMapping
{
    public static Dictionary<string, double> SummarizeScheduledRefreshMappingsForProfile(List<ScheduledRefreshMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<WarrantyProfile> DeriveWarrantyProfilesFromMapping(List<ScheduledRefreshMapping> items)
    {
        var outValue = new List<WarrantyProfile>();
        foreach (var item in items)
        {
            var derived = WarrantyProfile.NewWarrantyProfile(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateWarrantyProfile(WarrantyProfile item)
    {
        return ScheduledRefreshMappingPolicy.ClassifyScheduledRefreshMapping(item.EffectiveVolume());
    }
}
