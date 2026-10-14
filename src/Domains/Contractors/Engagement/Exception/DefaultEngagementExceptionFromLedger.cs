namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Functions of default engagement exception from ledger.</summary>
public static class DefaultEngagementExceptionFromLedger
{
    public static Dictionary<string, double> SummarizeAdaptiveOffboardingLedgersForException(List<AdaptiveOffboardingLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<DefaultEngagementException> DeriveDefaultEngagementExceptionsFromLedger(List<AdaptiveOffboardingLedger> items)
    {
        var outValue = new List<DefaultEngagementException>();
        foreach (var item in items)
        {
            var derived = DefaultEngagementException.NewDefaultEngagementException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceDefaultEngagementException(DefaultEngagementException item)
    {
        return AdaptiveOffboardingSegmentPolicy.NormalizeAdaptiveOffboardingSegmentCode(item.Code);
    }
}
