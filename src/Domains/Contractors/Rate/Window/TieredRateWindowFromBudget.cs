namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Functions of tiered rate window from budget.</summary>
public static class TieredRateWindowFromBudget
{
    public static Dictionary<string, double> SummarizeLegacyEngagementBudgetsForWindow(List<LegacyEngagementBudget> items)
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

    public static List<TieredRateWindow> DeriveTieredRateWindowsFromBudget(List<LegacyEngagementBudget> items)
    {
        var outValue = new List<TieredRateWindow>();
        foreach (var item in items)
        {
            var derived = TieredRateWindow.NewTieredRateWindow(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTieredRateWindow(TieredRateWindow item)
    {
        return LegacyEngagementBudgetPolicy.NormalizeLegacyEngagementBudgetCode(item.Code);
    }
}
