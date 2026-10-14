namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Functions of mobility window from item.</summary>
public static class MobilityWindowFromItem
{
    public static Dictionary<string, double> SummarizeSuccessionPlanItemsForWindow(List<SuccessionPlanItem> items)
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

    public static List<MobilityWindow> DeriveMobilityWindowsFromItem(List<SuccessionPlanItem> items)
    {
        var outValue = new List<MobilityWindow>();
        foreach (var item in items)
        {
            var derived = MobilityWindow.NewMobilityWindow(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileMobilityWindow(MobilityWindow item)
    {
        return SuccessionPlanItemPolicy.SuccessionPlanItemWeight(6, true);
    }
}
