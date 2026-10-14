namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.Laptop.LaptopManifest;

/// <summary>Functions of pending license seat slot from manifest.</summary>
public static class PendingLicenseSeatSlotFromManifest
{
    public static Dictionary<string, double> SummarizeLenientLaptopManifestsForSlot(List<LenientLaptopManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PendingLicenseSeatSlot> DerivePendingLicenseSeatSlotsFromManifest(List<LenientLaptopManifest> items)
    {
        var outValue = new List<PendingLicenseSeatSlot>();
        foreach (var item in items)
        {
            var derived = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimatePendingLicenseSeatSlot(PendingLicenseSeatSlot item)
    {
        return PendingLaptopMilestonePolicy.NormalizePendingLaptopMilestoneCode(item.Code);
    }
}
