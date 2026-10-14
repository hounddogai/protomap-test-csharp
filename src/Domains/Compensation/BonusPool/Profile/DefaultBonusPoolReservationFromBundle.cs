namespace Acme.Workforce.Compensation.BonusPool.Profile;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Functions of default bonus pool reservation from bundle.</summary>
public static class DefaultBonusPoolReservationFromBundle
{
    public static Dictionary<string, double> SummarizeMeritCycleBundlesForReservation(List<MeritCycleBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<DefaultBonusPoolReservation> DeriveDefaultBonusPoolReservationsFromBundle(List<MeritCycleBundle> items)
    {
        var outValue = new List<DefaultBonusPoolReservation>();
        foreach (var item in items)
        {
            var derived = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanDefaultBonusPoolReservation(DefaultBonusPoolReservation item)
    {
        return MeritCycleBundlePolicy.MeritCycleBundleBucket(item.ProjectedLoad());
    }
}
