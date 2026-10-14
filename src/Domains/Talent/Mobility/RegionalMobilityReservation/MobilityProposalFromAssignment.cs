namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>Functions of mobility proposal from assignment.</summary>
public static class MobilityProposalFromAssignment
{
    public static Dictionary<string, double> SummarizeDefaultNominationAssignmentsForProposal(List<DefaultNominationAssignment> items)
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

    public static List<MobilityProposal> DeriveMobilityProposalsFromAssignment(List<DefaultNominationAssignment> items)
    {
        var outValue = new List<MobilityProposal>();
        foreach (var item in items)
        {
            var derived = MobilityProposal.NewMobilityProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveMobilityProposal(MobilityProposal item)
    {
        return LenientNominationRequestPolicy.NormalizeLenientNominationRequestCode(item.Code);
    }
}
