namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Functions of rolling pay run exception from rollup.</summary>
public static class RollingPayRunExceptionFromRollup
{
    public static Dictionary<string, double> SummarizeTaxFormRollupsForException(List<TaxFormRollup> items)
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

    public static List<RollingPayRunException> DeriveRollingPayRunExceptionsFromRollup(List<TaxFormRollup> items)
    {
        var outValue = new List<RollingPayRunException>();
        foreach (var item in items)
        {
            var derived = RollingPayRunException.NewRollingPayRunException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRollingPayRunException(RollingPayRunException item)
    {
        return TaxFormVariancePolicy.TaxFormVarianceBucket(item.MeasuredValue());
    }
}
