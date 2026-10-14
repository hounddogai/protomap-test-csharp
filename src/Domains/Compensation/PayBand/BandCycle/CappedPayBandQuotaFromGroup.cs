namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Functions of capped pay band quota from group.</summary>
public static class CappedPayBandQuotaFromGroup
{
    public static Dictionary<string, double> SummarizeDefaultPayBandGroupsForQuota(List<DefaultPayBandGroup> items)
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

    public static List<CappedPayBandQuota> DeriveCappedPayBandQuotasFromGroup(List<DefaultPayBandGroup> items)
    {
        var outValue = new List<CappedPayBandQuota>();
        foreach (var item in items)
        {
            var derived = CappedPayBandQuota.NewCappedPayBandQuota(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignCappedPayBandQuota(CappedPayBandQuota item)
    {
        return DefaultPayBandGroupPolicy.DefaultPayBandGroupBucket(item.MeasuredValue());
    }
}
