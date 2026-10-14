namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Functions of return plan slot from variance.</summary>
public static class ReturnPlanSlotFromVariance
{
    public static Dictionary<string, double> SummarizeHolidayVariancesForSlot(List<HolidayVariance> items)
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

    public static List<ReturnPlanSlot> DeriveReturnPlanSlotsFromVariance(List<HolidayVariance> items)
    {
        var outValue = new List<ReturnPlanSlot>();
        foreach (var item in items)
        {
            var derived = ReturnPlanSlot.NewReturnPlanSlot(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreReturnPlanSlot(ReturnPlanSlot item)
    {
        return HolidayRulePolicy.HolidayRuleWeight(5, true);
    }
}
