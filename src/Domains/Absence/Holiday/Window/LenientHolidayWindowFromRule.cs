namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Functions of lenient holiday window from rule.</summary>
public static class LenientHolidayWindowFromRule
{
    public static Dictionary<string, double> SummarizeTieredPolicyRulesForWindow(List<TieredPolicyRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LenientHolidayWindow> DeriveLenientHolidayWindowsFromRule(List<TieredPolicyRule> items)
    {
        var outValue = new List<LenientHolidayWindow>();
        foreach (var item in items)
        {
            var derived = LenientHolidayWindow.NewLenientHolidayWindow(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectLenientHolidayWindow(LenientHolidayWindow item)
    {
        return DefaultPolicyItemPolicy.DefaultPolicyItemWeight(9, true);
    }
}
