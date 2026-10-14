namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Functions of default holiday assignment from checkpoint.</summary>
public static class DefaultHolidayAssignmentFromCheckpoint
{
    public static Dictionary<string, double> SummarizeAccrualCheckpointsForAssignment(List<AccrualCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<DefaultHolidayAssignment> DeriveDefaultHolidayAssignmentsFromCheckpoint(List<AccrualCheckpoint> items)
    {
        var outValue = new List<DefaultHolidayAssignment>();
        foreach (var item in items)
        {
            var derived = DefaultHolidayAssignment.NewDefaultHolidayAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateDefaultHolidayAssignment(DefaultHolidayAssignment item)
    {
        return AccrualCheckpointPolicy.NormalizeAccrualCheckpointCode(item.Code);
    }
}
