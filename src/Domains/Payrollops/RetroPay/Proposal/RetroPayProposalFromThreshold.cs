namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Payrollops.Garnishment.RecordItem;

/// <summary>Functions of retro pay proposal from threshold.</summary>
public static class RetroPayProposalFromThreshold
{
    public static Dictionary<string, double> SummarizeGarnishmentThresholdsForProposal(List<GarnishmentThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RetroPayProposal> DeriveRetroPayProposalsFromThreshold(List<GarnishmentThreshold> items)
    {
        var outValue = new List<RetroPayProposal>();
        foreach (var item in items)
        {
            var derived = RetroPayProposal.NewRetroPayProposal(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitRetroPayProposal(RetroPayProposal item)
    {
        return GarnishmentThresholdPolicy.GarnishmentThresholdWeight(1, true);
    }
}
