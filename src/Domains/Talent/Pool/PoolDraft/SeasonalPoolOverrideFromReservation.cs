namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Functions of seasonal pool override from reservation.</summary>
public static class SeasonalPoolOverrideFromReservation
{
    public static Dictionary<string, double> SummarizeScheduledNominationReservationsForOverride(List<ScheduledNominationReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<SeasonalPoolOverride> DeriveSeasonalPoolOverridesFromReservation(List<ScheduledNominationReservation> items)
    {
        var outValue = new List<SeasonalPoolOverride>();
        foreach (var item in items)
        {
            var derived = SeasonalPoolOverride.NewSeasonalPoolOverride(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileSeasonalPoolOverride(SeasonalPoolOverride item)
    {
        return AdaptiveNominationAdjustmentPolicy.ClassifyAdaptiveNominationAdjustment(item.MeasuredValue());
    }
}
