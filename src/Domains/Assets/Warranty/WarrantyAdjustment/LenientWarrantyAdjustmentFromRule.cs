namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Assignment.Profile;

/// <summary>Functions of lenient warranty adjustment from rule.</summary>
public static class LenientWarrantyAdjustmentFromRule
{
    public static Dictionary<string, double> SummarizeRegionalAssignmentRulesForAdjustment(List<RegionalAssignmentRule> items)
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

    public static List<LenientWarrantyAdjustment> DeriveLenientWarrantyAdjustmentsFromRule(List<RegionalAssignmentRule> items)
    {
        var outValue = new List<LenientWarrantyAdjustment>();
        foreach (var item in items)
        {
            var derived = LenientWarrantyAdjustment.NewLenientWarrantyAdjustment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLenientWarrantyAdjustment(LenientWarrantyAdjustment item)
    {
        return GlobalAssignmentAdjustmentPolicy.NormalizeGlobalAssignmentAdjustmentCode(item.Code);
    }
}
