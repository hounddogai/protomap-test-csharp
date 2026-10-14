namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Functions of payslip group from snapshot.</summary>
public static class PayslipGroupFromSnapshot
{
    public static Dictionary<string, double> SummarizeGarnishmentSnapshotsForGroup(List<GarnishmentSnapshot> items)
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

    public static List<PayslipGroup> DerivePayslipGroupsFromSnapshot(List<GarnishmentSnapshot> items)
    {
        var outValue = new List<PayslipGroup>();
        foreach (var item in items)
        {
            var derived = PayslipGroup.NewPayslipGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPayslipGroup(PayslipGroup item)
    {
        return GarnishmentSnapshotPolicy.NormalizeGarnishmentSnapshotCode(item.Code);
    }
}
