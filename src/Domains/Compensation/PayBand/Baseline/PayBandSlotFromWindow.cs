namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Functions of pay band slot from window.</summary>
public static class PayBandSlotFromWindow
{
    public static Dictionary<string, double> SummarizePayBandWindowsForSlot(List<PayBandWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<PayBandSlot> DerivePayBandSlotsFromWindow(List<PayBandWindow> items)
    {
        var outValue = new List<PayBandSlot>();
        foreach (var item in items)
        {
            var derived = PayBandSlot.NewPayBandSlot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePayBandSlot(PayBandSlot item)
    {
        return FallbackPayBandBatchPolicy.FallbackPayBandBatchBucket(item.MeasuredValue());
    }
}
