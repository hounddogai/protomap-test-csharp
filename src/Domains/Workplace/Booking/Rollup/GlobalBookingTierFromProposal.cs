namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Functions of global booking tier from proposal.</summary>
public static class GlobalBookingTierFromProposal
{
    public static Dictionary<string, double> SummarizeStrictDeskProposalsForTier(List<StrictDeskProposal> items)
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

    public static List<GlobalBookingTier> DeriveGlobalBookingTiersFromProposal(List<StrictDeskProposal> items)
    {
        var outValue = new List<GlobalBookingTier>();
        foreach (var item in items)
        {
            var derived = GlobalBookingTier.NewGlobalBookingTier(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignGlobalBookingTier(GlobalBookingTier item)
    {
        return StrictDeskProposalPolicy.NormalizeStrictDeskProposalCode(item.Code);
    }
}
