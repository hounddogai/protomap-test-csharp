namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Functions of retro pay plan from bundle.</summary>
public static class RetroPayPlanFromBundle
{
    public static Dictionary<string, double> SummarizeGarnishmentBundlesForPlan(List<GarnishmentBundle> items)
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

    public static List<RetroPayPlan> DeriveRetroPayPlansFromBundle(List<GarnishmentBundle> items)
    {
        var outValue = new List<RetroPayPlan>();
        foreach (var item in items)
        {
            var derived = RetroPayPlan.NewRetroPayPlan(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceRetroPayPlan(RetroPayPlan item)
    {
        return GarnishmentBundlePolicy.GarnishmentBundleBucket(item.NetExposure());
    }
}
