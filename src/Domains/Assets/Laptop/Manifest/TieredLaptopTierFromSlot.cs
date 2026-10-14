namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Functions of tiered laptop tier from slot.</summary>
public static class TieredLaptopTierFromSlot
{
    public static Dictionary<string, double> SummarizeDefaultRefreshSlotsForTier(List<DefaultRefreshSlot> items)
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

    public static List<TieredLaptopTier> DeriveTieredLaptopTiersFromSlot(List<DefaultRefreshSlot> items)
    {
        var outValue = new List<TieredLaptopTier>();
        foreach (var item in items)
        {
            var derived = TieredLaptopTier.NewTieredLaptopTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanTieredLaptopTier(TieredLaptopTier item)
    {
        return DefaultRefreshSlotPolicy.DefaultRefreshSlotWeight(2, true);
    }
}
