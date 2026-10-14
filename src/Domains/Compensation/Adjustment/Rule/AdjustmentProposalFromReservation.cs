namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Compensation.MeritCycle.Reservation;

/// <summary>Functions of adjustment proposal from reservation.</summary>
public static class AdjustmentProposalFromReservation
{
    public static Dictionary<string, double> SummarizeMeritCycleReservationsForProposal(List<MeritCycleReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<AdjustmentProposal> DeriveAdjustmentProposalsFromReservation(List<MeritCycleReservation> items)
    {
        var outValue = new List<AdjustmentProposal>();
        foreach (var item in items)
        {
            var derived = AdjustmentProposal.NewAdjustmentProposal(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAdjustmentProposal(AdjustmentProposal item)
    {
        return MeritCycleForecastPolicy.MeritCycleForecastBucket(item.AdjustedExposure());
    }
}
