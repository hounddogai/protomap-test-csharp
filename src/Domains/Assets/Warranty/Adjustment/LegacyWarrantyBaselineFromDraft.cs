namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

/// <summary>Functions of legacy warranty baseline from draft.</summary>
public static class LegacyWarrantyBaselineFromDraft
{
    public static Dictionary<string, double> SummarizeManualAssignmentDraftsForBaseline(List<ManualAssignmentDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<LegacyWarrantyBaseline> DeriveLegacyWarrantyBaselinesFromDraft(List<ManualAssignmentDraft> items)
    {
        var outValue = new List<LegacyWarrantyBaseline>();
        foreach (var item in items)
        {
            var derived = LegacyWarrantyBaseline.NewLegacyWarrantyBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceLegacyWarrantyBaseline(LegacyWarrantyBaseline item)
    {
        return AssignmentBatchPolicy.NormalizeAssignmentBatchCode(item.Code);
    }
}
