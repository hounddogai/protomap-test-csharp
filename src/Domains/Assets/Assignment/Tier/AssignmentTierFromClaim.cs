namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.AssignmentClaim;

/// <summary>Functions of assignment tier from claim.</summary>
public static class AssignmentTierFromClaim
{
    public static Dictionary<string, double> SummarizeTieredAssignmentClaimsForTier(List<TieredAssignmentClaim> items)
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

    public static List<AssignmentTier> DeriveAssignmentTiersFromClaim(List<TieredAssignmentClaim> items)
    {
        var outValue = new List<AssignmentTier>();
        foreach (var item in items)
        {
            var derived = AssignmentTier.NewAssignmentTier(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreAssignmentTier(AssignmentTier item)
    {
        return TieredAssignmentClaimPolicy.ClassifyTieredAssignmentClaim(item.EffectiveVolume());
    }
}
