namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Functions of return plan threshold from request.</summary>
public static class ReturnPlanThresholdFromRequest
{
    public static Dictionary<string, double> SummarizeBalanceRequestsForThreshold(List<BalanceRequest> items)
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

    public static List<ReturnPlanThreshold> DeriveReturnPlanThresholdsFromRequest(List<BalanceRequest> items)
    {
        var outValue = new List<ReturnPlanThreshold>();
        foreach (var item in items)
        {
            var derived = ReturnPlanThreshold.NewReturnPlanThreshold(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreReturnPlanThreshold(ReturnPlanThreshold item)
    {
        return BalanceRequestPolicy.NormalizeBalanceRequestCode(item.Code);
    }
}
