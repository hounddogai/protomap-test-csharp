namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>Functions of seasonal adjustment reservation from claim.</summary>
public static class SeasonalAdjustmentReservationFromClaim
{
    public static Dictionary<string, double> SummarizeFallbackMeritCycleClaimsForReservation(List<FallbackMeritCycleClaim> items)
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

    public static List<SeasonalAdjustmentReservation> DeriveSeasonalAdjustmentReservationsFromClaim(List<FallbackMeritCycleClaim> items)
    {
        var outValue = new List<SeasonalAdjustmentReservation>();
        foreach (var item in items)
        {
            var derived = SeasonalAdjustmentReservation.NewSeasonalAdjustmentReservation(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSeasonalAdjustmentReservation(SeasonalAdjustmentReservation item)
    {
        return GlobalMeritCycleRollupPolicy.NormalizeGlobalMeritCycleRollupCode(item.Code);
    }
}
