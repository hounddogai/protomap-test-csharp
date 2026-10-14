namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Functions of scheduled cost model policy from exception.</summary>
public static class ScheduledCostModelPolicyFromException
{
    public static Dictionary<string, double> SummarizeReorgExceptionsForPolicy(List<ReorgException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<ScheduledCostModelPolicy> DeriveScheduledCostModelPoliciesFromException(List<ReorgException> items)
    {
        var outValue = new List<ScheduledCostModelPolicy>();
        foreach (var item in items)
        {
            var derived = ScheduledCostModelPolicy.NewScheduledCostModelPolicy(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitScheduledCostModelPolicy(ScheduledCostModelPolicy item)
    {
        return ReorgExceptionPolicy.ReorgExceptionWeight(1, true);
    }
}
