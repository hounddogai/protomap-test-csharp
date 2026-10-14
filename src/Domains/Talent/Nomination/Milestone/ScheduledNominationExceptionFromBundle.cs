namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.NominationReservation;

/// <summary>Functions of scheduled nomination exception from bundle.</summary>
public static class ScheduledNominationExceptionFromBundle
{
    public static Dictionary<string, double> SummarizeGlobalNominationBundlesForException(List<GlobalNominationBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<ScheduledNominationException> DeriveScheduledNominationExceptionsFromBundle(List<GlobalNominationBundle> items)
    {
        var outValue = new List<ScheduledNominationException>();
        foreach (var item in items)
        {
            var derived = ScheduledNominationException.NewScheduledNominationException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeScheduledNominationException(ScheduledNominationException item)
    {
        return BatchedNominationReservationPolicy.BatchedNominationReservationBucket(item.MeasuredValue());
    }
}
