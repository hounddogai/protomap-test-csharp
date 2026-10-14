namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Functions of strict accrual template from adjustment.</summary>
public static class StrictAccrualTemplateFromAdjustment
{
    public static Dictionary<string, double> SummarizeRollingLeaveRequestAdjustmentsForTemplate(List<RollingLeaveRequestAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<StrictAccrualTemplate> DeriveStrictAccrualTemplatesFromAdjustment(List<RollingLeaveRequestAdjustment> items)
    {
        var outValue = new List<StrictAccrualTemplate>();
        foreach (var item in items)
        {
            var derived = StrictAccrualTemplate.NewStrictAccrualTemplate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitStrictAccrualTemplate(StrictAccrualTemplate item)
    {
        return RollingLeaveRequestAdjustmentPolicy.NormalizeRollingLeaveRequestAdjustmentCode(item.Code);
    }
}
