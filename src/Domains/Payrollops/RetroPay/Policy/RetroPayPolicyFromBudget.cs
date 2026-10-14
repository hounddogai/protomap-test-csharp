namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Functions of retro pay policy from budget.</summary>
public static class RetroPayPolicyFromBudget
{
    public static Dictionary<string, double> SummarizeGarnishmentBudgetsForPolicy(List<GarnishmentBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RetroPayPolicy> DeriveRetroPayPoliciesFromBudget(List<GarnishmentBudget> items)
    {
        var outValue = new List<RetroPayPolicy>();
        foreach (var item in items)
        {
            var derived = RetroPayPolicy.NewRetroPayPolicy(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyRetroPayPolicy(RetroPayPolicy item)
    {
        return GarnishmentBudgetPolicy.ClassifyGarnishmentBudget(item.NetVolume());
    }
}
