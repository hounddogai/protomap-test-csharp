namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>Functions of retro pay schedule from cycle.</summary>
public static class RetroPayScheduleFromCycle
{
    public static Dictionary<string, double> SummarizeTaxFormCyclesForSchedule(List<TaxFormCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<RetroPaySchedule> DeriveRetroPaySchedulesFromCycle(List<TaxFormCycle> items)
    {
        var outValue = new List<RetroPaySchedule>();
        foreach (var item in items)
        {
            var derived = RetroPaySchedule.NewRetroPaySchedule(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleRetroPaySchedule(RetroPaySchedule item)
    {
        return TaxFormSlotPolicy.TaxFormSlotWeight(6, true);
    }
}
