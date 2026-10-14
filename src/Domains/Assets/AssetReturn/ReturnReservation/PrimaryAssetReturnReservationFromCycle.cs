namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Functions of primary asset return reservation from cycle.</summary>
public static class PrimaryAssetReturnReservationFromCycle
{
    public static Dictionary<string, double> SummarizeDefaultWarrantyCyclesForReservation(List<DefaultWarrantyCycle> items)
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

    public static List<PrimaryAssetReturnReservation> DerivePrimaryAssetReturnReservationsFromCycle(List<DefaultWarrantyCycle> items)
    {
        var outValue = new List<PrimaryAssetReturnReservation>();
        foreach (var item in items)
        {
            var derived = PrimaryAssetReturnReservation.NewPrimaryAssetReturnReservation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocatePrimaryAssetReturnReservation(PrimaryAssetReturnReservation item)
    {
        return WarrantyCyclePolicy.WarrantyCycleWeight(3, true);
    }
}
