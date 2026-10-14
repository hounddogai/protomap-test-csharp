namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Absence.Policy.Rollup;

/// <summary>Functions of accrual forecast from rollup.</summary>
public static class AccrualForecastFromRollup
{
    public static Dictionary<string, double> SummarizeLenientPolicyRollupsForForecast(List<LenientPolicyRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<AccrualForecast> DeriveAccrualForecastsFromRollup(List<LenientPolicyRollup> items)
    {
        var outValue = new List<AccrualForecast>();
        foreach (var item in items)
        {
            var derived = AccrualForecast.NewAccrualForecast(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileAccrualForecast(AccrualForecast item)
    {
        return LenientPolicyRollupPolicy.NormalizeLenientPolicyRollupCode(item.Code);
    }
}
