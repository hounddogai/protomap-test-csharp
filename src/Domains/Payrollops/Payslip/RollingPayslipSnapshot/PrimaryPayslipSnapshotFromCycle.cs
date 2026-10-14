namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Functions of primary payslip snapshot from cycle.</summary>
public static class PrimaryPayslipSnapshotFromCycle
{
    public static Dictionary<string, double> SummarizeRetroPayCyclesForSnapshot(List<RetroPayCycle> items)
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

    public static List<PrimaryPayslipSnapshot> DerivePrimaryPayslipSnapshotsFromCycle(List<RetroPayCycle> items)
    {
        var outValue = new List<PrimaryPayslipSnapshot>();
        foreach (var item in items)
        {
            var derived = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePrimaryPayslipSnapshot(PrimaryPayslipSnapshot item)
    {
        return SeasonalRetroPayClaimPolicy.SeasonalRetroPayClaimBucket(item.NetTotal());
    }
}
