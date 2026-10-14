namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Functions of pay band schedule from estimate.</summary>
public static class PayBandScheduleFromEstimate
{
    public static Dictionary<string, double> SummarizeDefaultPayBandEstimatesForSchedule(List<DefaultPayBandEstimate> items)
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

    public static List<PayBandSchedule> DerivePayBandSchedulesFromEstimate(List<DefaultPayBandEstimate> items)
    {
        var outValue = new List<PayBandSchedule>();
        foreach (var item in items)
        {
            var derived = PayBandSchedule.NewPayBandSchedule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputePayBandSchedule(PayBandSchedule item)
    {
        return SeasonalPayBandTemplatePolicy.SeasonalPayBandTemplateWeight(2, true);
    }
}
