namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Functions of weighted warranty draft from slot.</summary>
public static class WeightedWarrantyDraftFromSlot
{
    public static Dictionary<string, double> SummarizeScheduledAssignmentSlotsForDraft(List<ScheduledAssignmentSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<WeightedWarrantyDraft> DeriveWeightedWarrantyDraftsFromSlot(List<ScheduledAssignmentSlot> items)
    {
        var outValue = new List<WeightedWarrantyDraft>();
        foreach (var item in items)
        {
            var derived = WeightedWarrantyDraft.NewWeightedWarrantyDraft(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeWeightedWarrantyDraft(WeightedWarrantyDraft item)
    {
        return RegionalAssignmentAgreementPolicy.ClassifyRegionalAssignmentAgreement(item.EffectiveVolume());
    }
}
