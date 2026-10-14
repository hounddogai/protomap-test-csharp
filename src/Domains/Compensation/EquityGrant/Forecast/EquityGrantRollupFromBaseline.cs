namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Functions of equity grant rollup from baseline.</summary>
public static class EquityGrantRollupFromBaseline
{
    public static Dictionary<string, double> SummarizePayBandBaselinesForRollup(List<PayBandBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<EquityGrantRollup> DeriveEquityGrantRollupsFromBaseline(List<PayBandBaseline> items)
    {
        var outValue = new List<EquityGrantRollup>();
        foreach (var item in items)
        {
            var derived = EquityGrantRollup.NewEquityGrantRollup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateEquityGrantRollup(EquityGrantRollup item)
    {
        return PayBandScorePolicy.PayBandScoreWeight(5, true);
    }
}
