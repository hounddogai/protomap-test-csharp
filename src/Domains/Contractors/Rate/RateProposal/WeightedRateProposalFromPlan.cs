namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Functions of weighted rate proposal from plan.</summary>
public static class WeightedRateProposalFromPlan
{
    public static Dictionary<string, double> SummarizeOffboardingPlansForProposal(List<OffboardingPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<WeightedRateProposal> DeriveWeightedRateProposalsFromPlan(List<OffboardingPlan> items)
    {
        var outValue = new List<WeightedRateProposal>();
        foreach (var item in items)
        {
            var derived = WeightedRateProposal.NewWeightedRateProposal(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleWeightedRateProposal(WeightedRateProposal item)
    {
        return OffboardingPlanPolicy.NormalizeOffboardingPlanCode(item.Code);
    }
}
