namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>Functions of adaptive garnishment plan from slot.</summary>
public static class AdaptiveGarnishmentPlanFromSlot
{
    public static Dictionary<string, double> SummarizeTaxFormSlotsForPlan(List<TaxFormSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AdaptiveGarnishmentPlan> DeriveAdaptiveGarnishmentPlansFromSlot(List<TaxFormSlot> items)
    {
        var outValue = new List<AdaptiveGarnishmentPlan>();
        foreach (var item in items)
        {
            var derived = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleAdaptiveGarnishmentPlan(AdaptiveGarnishmentPlan item)
    {
        return TaxFormSlotPolicy.TaxFormSlotWeight(6, true);
    }
}
