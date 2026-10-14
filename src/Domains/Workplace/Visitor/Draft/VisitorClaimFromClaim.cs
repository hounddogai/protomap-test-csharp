namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Functions of visitor claim from claim.</summary>
public static class VisitorClaimFromClaim
{
    public static Dictionary<string, double> SummarizeAccessBadgeClaimsForClaim(List<AccessBadgeClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<VisitorClaim> DeriveVisitorClaimsFromClaim(List<AccessBadgeClaim> items)
    {
        var outValue = new List<VisitorClaim>();
        foreach (var item in items)
        {
            var derived = VisitorClaim.NewVisitorClaim(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileVisitorClaim(VisitorClaim item)
    {
        return GlobalAccessBadgeCyclePolicy.GlobalAccessBadgeCycleWeight(7, true);
    }
}
