namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Talent.Nomination.NominationReservation;

/// <summary>Functions of primary mobility proposal from reservation.</summary>
public static class PrimaryMobilityProposalFromReservation
{
    public static Dictionary<string, double> SummarizeBatchedNominationReservationsForProposal(List<BatchedNominationReservation> items)
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

    public static List<PrimaryMobilityProposal> DerivePrimaryMobilityProposalsFromReservation(List<BatchedNominationReservation> items)
    {
        var outValue = new List<PrimaryMobilityProposal>();
        foreach (var item in items)
        {
            var derived = PrimaryMobilityProposal.NewPrimaryMobilityProposal(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPrimaryMobilityProposal(PrimaryMobilityProposal item)
    {
        return GlobalNominationBundlePolicy.GlobalNominationBundleBucket(item.EffectiveValue());
    }
}
