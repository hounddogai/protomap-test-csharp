namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Functions of capped rate policy from draft.</summary>
public static class CappedRatePolicyFromDraft
{
    public static Dictionary<string, double> SummarizeDefaultRateDraftsForPolicy(List<DefaultRateDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<CappedRatePolicy> DeriveCappedRatePoliciesFromDraft(List<DefaultRateDraft> items)
    {
        var outValue = new List<CappedRatePolicy>();
        foreach (var item in items)
        {
            var derived = CappedRatePolicy.NewCappedRatePolicy(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeCappedRatePolicy(CappedRatePolicy item)
    {
        return FallbackRateCyclePolicy.NormalizeFallbackRateCycleCode(item.Code);
    }
}
