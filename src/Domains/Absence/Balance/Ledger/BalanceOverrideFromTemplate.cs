namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Functions of balance override from template.</summary>
public static class BalanceOverrideFromTemplate
{
    public static Dictionary<string, double> SummarizeReturnPlanTemplatesForOverride(List<ReturnPlanTemplate> items)
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

    public static List<BalanceOverride> DeriveBalanceOverridesFromTemplate(List<ReturnPlanTemplate> items)
    {
        var outValue = new List<BalanceOverride>();
        foreach (var item in items)
        {
            var derived = BalanceOverride.NewBalanceOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyBalanceOverride(BalanceOverride item)
    {
        return StrictReturnPlanSlotPolicy.StrictReturnPlanSlotWeight(6, true);
    }
}
