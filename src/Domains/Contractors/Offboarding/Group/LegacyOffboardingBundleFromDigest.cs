namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Functions of legacy offboarding bundle from digest.</summary>
public static class LegacyOffboardingBundleFromDigest
{
    public static Dictionary<string, double> SummarizeStatementOfWorkDigestsForBundle(List<StatementOfWorkDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<LegacyOffboardingBundle> DeriveLegacyOffboardingBundlesFromDigest(List<StatementOfWorkDigest> items)
    {
        var outValue = new List<LegacyOffboardingBundle>();
        foreach (var item in items)
        {
            var derived = LegacyOffboardingBundle.NewLegacyOffboardingBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceLegacyOffboardingBundle(LegacyOffboardingBundle item)
    {
        return StatementOfWorkDigestPolicy.StatementOfWorkDigestBucket(item.NetVolume());
    }
}
