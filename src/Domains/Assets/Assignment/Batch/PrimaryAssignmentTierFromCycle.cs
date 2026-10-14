namespace Acme.Workforce.Assets.Assignment.Batch;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Functions of primary assignment tier from cycle.</summary>
public static class PrimaryAssignmentTierFromCycle
{
    public static Dictionary<string, double> SummarizeDefaultWarrantyCyclesForTier(List<DefaultWarrantyCycle> items)
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

    public static List<PrimaryAssignmentTier> DerivePrimaryAssignmentTiersFromCycle(List<DefaultWarrantyCycle> items)
    {
        var outValue = new List<PrimaryAssignmentTier>();
        foreach (var item in items)
        {
            var derived = PrimaryAssignmentTier.NewPrimaryAssignmentTier(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyPrimaryAssignmentTier(PrimaryAssignmentTier item)
    {
        return LenientWarrantyAdjustmentPolicy.NormalizeLenientWarrantyAdjustmentCode(item.Code);
    }
}
