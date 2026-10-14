namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Functions of adjustment bundle from window.</summary>
public static class AdjustmentBundleFromWindow
{
    public static Dictionary<string, double> SummarizePayBandWindowsForBundle(List<PayBandWindow> items)
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

    public static List<AdjustmentBundle> DeriveAdjustmentBundlesFromWindow(List<PayBandWindow> items)
    {
        var outValue = new List<AdjustmentBundle>();
        foreach (var item in items)
        {
            var derived = AdjustmentBundle.NewAdjustmentBundle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanAdjustmentBundle(AdjustmentBundle item)
    {
        return DefaultPayBandAllocationPolicy.DefaultPayBandAllocationWeight(1, true);
    }
}
