namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Absence.Policy.Estimate;

/// <summary>Functions of rolling balance proposal from forecast.</summary>
public static class RollingBalanceProposalFromForecast
{
    public static Dictionary<string, double> SummarizePolicyForecastsForProposal(List<PolicyForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<RollingBalanceProposal> DeriveRollingBalanceProposalsFromForecast(List<PolicyForecast> items)
    {
        var outValue = new List<RollingBalanceProposal>();
        foreach (var item in items)
        {
            var derived = RollingBalanceProposal.NewRollingBalanceProposal(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchRollingBalanceProposal(RollingBalanceProposal item)
    {
        return PolicyEstimatePolicy.PolicyEstimateBucket(item.WeightedVolume());
    }
}
