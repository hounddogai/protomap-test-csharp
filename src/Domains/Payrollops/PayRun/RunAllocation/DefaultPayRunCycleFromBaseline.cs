namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Functions of default pay run cycle from baseline.</summary>
public static class DefaultPayRunCycleFromBaseline
{
    public static Dictionary<string, double> SummarizeSeasonalPayslipBaselinesForCycle(List<SeasonalPayslipBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<DefaultPayRunCycle> DeriveDefaultPayRunCyclesFromBaseline(List<SeasonalPayslipBaseline> items)
    {
        var outValue = new List<DefaultPayRunCycle>();
        foreach (var item in items)
        {
            var derived = DefaultPayRunCycle.NewDefaultPayRunCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeDefaultPayRunCycle(DefaultPayRunCycle item)
    {
        return PayslipRollupPolicy.ClassifyPayslipRollup(item.MeasuredValue());
    }
}
