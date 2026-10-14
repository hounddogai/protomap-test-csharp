namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Functions of pay band quota from schedule.</summary>
public static class PayBandQuotaFromSchedule
{
    public static Dictionary<string, double> SummarizeTieredPayBandSchedulesForQuota(List<TieredPayBandSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PayBandQuota> DerivePayBandQuotasFromSchedule(List<TieredPayBandSchedule> items)
    {
        var outValue = new List<PayBandQuota>();
        foreach (var item in items)
        {
            var derived = PayBandQuota.NewPayBandQuota(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitPayBandQuota(PayBandQuota item)
    {
        return PayBandSchedulePolicy.PayBandScheduleWeight(2, true);
    }
}
