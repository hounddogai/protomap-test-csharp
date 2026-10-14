namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Functions of primary offboarding slot from review.</summary>
public static class PrimaryOffboardingSlotFromReview
{
    public static Dictionary<string, double> SummarizeRegionalOffboardingReviewsForSlot(List<RegionalOffboardingReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<PrimaryOffboardingSlot> DerivePrimaryOffboardingSlotsFromReview(List<RegionalOffboardingReview> items)
    {
        var outValue = new List<PrimaryOffboardingSlot>();
        foreach (var item in items)
        {
            var derived = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputePrimaryOffboardingSlot(PrimaryOffboardingSlot item)
    {
        return RegionalOffboardingReviewPolicy.ClassifyRegionalOffboardingReview(item.MeasuredValue());
    }
}
