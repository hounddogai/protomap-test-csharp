namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Functions of offboarding rollup from proposal.</summary>
public static class OffboardingRollupFromProposal
{
    public static Dictionary<string, double> SummarizeRateProposalsForRollup(List<RateProposal> items)
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

    public static List<OffboardingRollup> DeriveOffboardingRollupsFromProposal(List<RateProposal> items)
    {
        var outValue = new List<OffboardingRollup>();
        foreach (var item in items)
        {
            var derived = OffboardingRollup.NewOffboardingRollup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyOffboardingRollup(OffboardingRollup item)
    {
        return RateProposalPolicy.RateProposalWeight(3, true);
    }
}
