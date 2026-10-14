namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Functions of garnishment snapshot from request.</summary>
public static class GarnishmentSnapshotFromRequest
{
    public static Dictionary<string, double> SummarizePayslipRequestsForSnapshot(List<PayslipRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<GarnishmentSnapshot> DeriveGarnishmentSnapshotsFromRequest(List<PayslipRequest> items)
    {
        var outValue = new List<GarnishmentSnapshot>();
        foreach (var item in items)
        {
            var derived = GarnishmentSnapshot.NewGarnishmentSnapshot(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanGarnishmentSnapshot(GarnishmentSnapshot item)
    {
        return PayslipRequestPolicy.ClassifyPayslipRequest(item.EffectiveLoad());
    }
}
