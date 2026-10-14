namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Absence.Balance.Proposal;

/// <summary>Functions of global accrual estimate from proposal.</summary>
public static class GlobalAccrualEstimateFromProposal
{
    public static Dictionary<string, double> SummarizeRollingBalanceProposalsForEstimate(List<RollingBalanceProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<GlobalAccrualEstimate> DeriveGlobalAccrualEstimatesFromProposal(List<RollingBalanceProposal> items)
    {
        var outValue = new List<GlobalAccrualEstimate>();
        foreach (var item in items)
        {
            var derived = GlobalAccrualEstimate.NewGlobalAccrualEstimate(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignGlobalAccrualEstimate(GlobalAccrualEstimate item)
    {
        return TieredBalanceTargetPolicy.NormalizeTieredBalanceTargetCode(item.Code);
    }
}
