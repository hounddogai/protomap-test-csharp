namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Functions of payslip rollup from variance.</summary>
public static class PayslipRollupFromVariance
{
    public static Dictionary<string, double> SummarizeBatchedPayRunVariancesForRollup(List<BatchedPayRunVariance> items)
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

    public static List<PayslipRollup> DerivePayslipRollupsFromVariance(List<BatchedPayRunVariance> items)
    {
        var outValue = new List<PayslipRollup>();
        foreach (var item in items)
        {
            var derived = PayslipRollup.NewPayslipRollup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPayslipRollup(PayslipRollup item)
    {
        return TieredPayRunMilestonePolicy.ClassifyTieredPayRunMilestone(item.ProjectedVolume());
    }
}
