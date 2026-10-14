namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Rate.Snapshot;

/// <summary>Functions of offboarding notice from preference.</summary>
public static class OffboardingNoticeFromPreference
{
    public static Dictionary<string, double> SummarizeRatePreferencesForNotice(List<RatePreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<OffboardingNotice> DeriveOffboardingNoticesFromPreference(List<RatePreference> items)
    {
        var outValue = new List<OffboardingNotice>();
        foreach (var item in items)
        {
            var derived = OffboardingNotice.NewOffboardingNotice(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectOffboardingNotice(OffboardingNotice item)
    {
        return RatePreferencePolicy.RatePreferenceBucket(item.ProjectedExposure());
    }
}
