namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Functions of offboarding profile from batch.</summary>
public static class OffboardingProfileFromBatch
{
    public static Dictionary<string, double> SummarizeOffboardingBatchesForProfile(List<OffboardingBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<OffboardingProfile> DeriveOffboardingProfilesFromBatch(List<OffboardingBatch> items)
    {
        var outValue = new List<OffboardingProfile>();
        foreach (var item in items)
        {
            var derived = OffboardingProfile.NewOffboardingProfile(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeOffboardingProfile(OffboardingProfile item)
    {
        return OffboardingThresholdPolicy.OffboardingThresholdBucket(item.ProjectedVolume());
    }
}
