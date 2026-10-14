namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Functions of balance schedule from cycle.</summary>
public static class BalanceScheduleFromCycle
{
    public static Dictionary<string, double> SummarizeAccrualCyclesForSchedule(List<AccrualCycle> items)
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

    public static List<BalanceSchedule> DeriveBalanceSchedulesFromCycle(List<AccrualCycle> items)
    {
        var outValue = new List<BalanceSchedule>();
        foreach (var item in items)
        {
            var derived = BalanceSchedule.NewBalanceSchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignBalanceSchedule(BalanceSchedule item)
    {
        return AccrualScorePolicy.AccrualScoreWeight(8, true);
    }
}
