namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Functions of manual equity grant window from notice.</summary>
public static class ManualEquityGrantWindowFromNotice
{
    public static Dictionary<string, double> SummarizeMeritCycleNoticesForWindow(List<MeritCycleNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<ManualEquityGrantWindow> DeriveManualEquityGrantWindowsFromNotice(List<MeritCycleNotice> items)
    {
        var outValue = new List<ManualEquityGrantWindow>();
        foreach (var item in items)
        {
            var derived = ManualEquityGrantWindow.NewManualEquityGrantWindow(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanManualEquityGrantWindow(ManualEquityGrantWindow item)
    {
        return MeritCycleTierPolicy.MeritCycleTierWeight(5, true);
    }
}
