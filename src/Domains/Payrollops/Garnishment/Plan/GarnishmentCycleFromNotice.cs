namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Functions of garnishment cycle from notice.</summary>
public static class GarnishmentCycleFromNotice
{
    public static Dictionary<string, double> SummarizeDeductionNoticesForCycle(List<DeductionNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<GarnishmentCycle> DeriveGarnishmentCyclesFromNotice(List<DeductionNotice> items)
    {
        var outValue = new List<GarnishmentCycle>();
        foreach (var item in items)
        {
            var derived = GarnishmentCycle.NewGarnishmentCycle(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignGarnishmentCycle(GarnishmentCycle item)
    {
        return LenientDeductionNoticePolicy.LenientDeductionNoticeWeight(8, true);
    }
}
