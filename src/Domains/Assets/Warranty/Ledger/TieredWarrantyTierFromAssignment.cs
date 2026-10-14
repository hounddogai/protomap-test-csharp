namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Functions of tiered warranty tier from assignment.</summary>
public static class TieredWarrantyTierFromAssignment
{
    public static Dictionary<string, double> SummarizePendingWarrantyAssignmentsForTier(List<PendingWarrantyAssignment> items)
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

    public static List<TieredWarrantyTier> DeriveTieredWarrantyTiersFromAssignment(List<PendingWarrantyAssignment> items)
    {
        var outValue = new List<TieredWarrantyTier>();
        foreach (var item in items)
        {
            var derived = TieredWarrantyTier.NewTieredWarrantyTier(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankTieredWarrantyTier(TieredWarrantyTier item)
    {
        return PendingWarrantyAssignmentPolicy.NormalizePendingWarrantyAssignmentCode(item.Code);
    }
}
