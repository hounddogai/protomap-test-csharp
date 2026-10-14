namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Functions of fallback pool draft from reservation.</summary>
public static class FallbackPoolDraftFromReservation
{
    public static Dictionary<string, double> SummarizePoolReservationsForDraft(List<PoolReservation> items)
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

    public static List<FallbackPoolDraft> DeriveFallbackPoolDraftsFromReservation(List<PoolReservation> items)
    {
        var outValue = new List<FallbackPoolDraft>();
        foreach (var item in items)
        {
            var derived = FallbackPoolDraft.NewFallbackPoolDraft(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeFallbackPoolDraft(FallbackPoolDraft item)
    {
        return DefaultPoolExceptionPolicy.DefaultPoolExceptionWeight(9, true);
    }
}
