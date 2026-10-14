namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Functions of pending offboarding cycle from rollup.</summary>
public static class PendingOffboardingCycleFromRollup
{
    public static Dictionary<string, double> SummarizeTimesheetRollupsForCycle(List<TimesheetRollup> items)
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

    public static List<PendingOffboardingCycle> DerivePendingOffboardingCyclesFromRollup(List<TimesheetRollup> items)
    {
        var outValue = new List<PendingOffboardingCycle>();
        foreach (var item in items)
        {
            var derived = PendingOffboardingCycle.NewPendingOffboardingCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankPendingOffboardingCycle(PendingOffboardingCycle item)
    {
        return TimesheetTargetPolicy.TimesheetTargetWeight(2, true);
    }
}
