namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>Functions of rolling bonus pool proposal from profile.</summary>
public static class RollingBonusPoolProposalFromProfile
{
    public static Dictionary<string, double> SummarizeRollingBonusPoolProfilesForProposal(List<RollingBonusPoolProfile> items)
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

    public static List<RollingBonusPoolProposal> DeriveRollingBonusPoolProposalsFromProfile(List<RollingBonusPoolProfile> items)
    {
        var outValue = new List<RollingBonusPoolProposal>();
        foreach (var item in items)
        {
            var derived = RollingBonusPoolProposal.NewRollingBonusPoolProposal(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateRollingBonusPoolProposal(RollingBonusPoolProposal item)
    {
        return DefaultBonusPoolReservationPolicy.DefaultBonusPoolReservationWeight(3, true);
    }
}
