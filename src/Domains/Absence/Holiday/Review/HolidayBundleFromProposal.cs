namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Functions of holiday bundle from proposal.</summary>
public static class HolidayBundleFromProposal
{
    public static Dictionary<string, double> SummarizePolicyProposalsForBundle(List<PolicyProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<HolidayBundle> DeriveHolidayBundlesFromProposal(List<PolicyProposal> items)
    {
        var outValue = new List<HolidayBundle>();
        foreach (var item in items)
        {
            var derived = HolidayBundle.NewHolidayBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceHolidayBundle(HolidayBundle item)
    {
        return PolicyQuotaPolicy.PolicyQuotaBucket(item.MeasuredValue());
    }
}
