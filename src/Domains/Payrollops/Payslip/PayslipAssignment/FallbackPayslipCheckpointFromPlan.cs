namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Functions of fallback payslip checkpoint from plan.</summary>
public static class FallbackPayslipCheckpointFromPlan
{
    public static Dictionary<string, double> SummarizeFallbackGarnishmentPlansForCheckpoint(List<FallbackGarnishmentPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<FallbackPayslipCheckpoint> DeriveFallbackPayslipCheckpointsFromPlan(List<FallbackGarnishmentPlan> items)
    {
        var outValue = new List<FallbackPayslipCheckpoint>();
        foreach (var item in items)
        {
            var derived = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitFallbackPayslipCheckpoint(FallbackPayslipCheckpoint item)
    {
        return FallbackGarnishmentPlanPolicy.NormalizeFallbackGarnishmentPlanCode(item.Code);
    }
}
