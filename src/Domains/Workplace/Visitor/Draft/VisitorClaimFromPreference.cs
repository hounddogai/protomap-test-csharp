namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Workplace.Occupancy.Mapping;

/// <summary>Functions of visitor claim from preference.</summary>
public static class VisitorClaimFromPreference
{
    public static Dictionary<string, double> SummarizeFallbackOccupancyPreferencesForClaim(List<FallbackOccupancyPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<VisitorClaim> DeriveVisitorClaimsFromPreference(List<FallbackOccupancyPreference> items)
    {
        var outValue = new List<VisitorClaim>();
        foreach (var item in items)
        {
            var derived = VisitorClaim.NewVisitorClaim(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }
}
