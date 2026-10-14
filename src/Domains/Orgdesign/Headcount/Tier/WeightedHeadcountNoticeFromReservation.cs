namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Functions of weighted headcount notice from reservation.</summary>
public static class WeightedHeadcountNoticeFromReservation
{
    public static Dictionary<string, double> SummarizeStrictHeadcountReservationsForNotice(List<StrictHeadcountReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<WeightedHeadcountNotice> DeriveWeightedHeadcountNoticesFromReservation(List<StrictHeadcountReservation> items)
    {
        var outValue = new List<WeightedHeadcountNotice>();
        foreach (var item in items)
        {
            var derived = WeightedHeadcountNotice.NewWeightedHeadcountNotice(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceWeightedHeadcountNotice(WeightedHeadcountNotice item)
    {
        return HeadcountClaimPolicy.ClassifyHeadcountClaim(item.MeasuredValue());
    }
}
