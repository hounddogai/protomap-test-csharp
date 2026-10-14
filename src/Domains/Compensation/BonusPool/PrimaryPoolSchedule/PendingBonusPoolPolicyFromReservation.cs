namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Functions of pending bonus pool policy from reservation.</summary>
public static class PendingBonusPoolPolicyFromReservation
{
    public static Dictionary<string, double> SummarizeAdaptiveMeritCycleReservationsForPolicy(List<AdaptiveMeritCycleReservation> items)
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

    public static List<PendingBonusPoolPolicy> DerivePendingBonusPoolPoliciesFromReservation(List<AdaptiveMeritCycleReservation> items)
    {
        var outValue = new List<PendingBonusPoolPolicy>();
        foreach (var item in items)
        {
            var derived = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimatePendingBonusPoolPolicy(PendingBonusPoolPolicy item)
    {
        return MeritCycleScorePolicy.MeritCycleScoreBucket(item.MeasuredValue());
    }
}
