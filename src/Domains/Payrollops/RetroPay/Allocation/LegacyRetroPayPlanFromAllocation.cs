namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Functions of legacy retro pay plan from allocation.</summary>
public static class LegacyRetroPayPlanFromAllocation
{
    public static Dictionary<string, double> SummarizeRetroPayAllocationsForPlan(List<RetroPayAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<LegacyRetroPayPlan> DeriveLegacyRetroPayPlansFromAllocation(List<RetroPayAllocation> items)
    {
        var outValue = new List<LegacyRetroPayPlan>();
        foreach (var item in items)
        {
            var derived = LegacyRetroPayPlan.NewLegacyRetroPayPlan(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeLegacyRetroPayPlan(LegacyRetroPayPlan item)
    {
        return RetroPayReservationPolicy.ClassifyRetroPayReservation(item.WeightedLoad());
    }
}
