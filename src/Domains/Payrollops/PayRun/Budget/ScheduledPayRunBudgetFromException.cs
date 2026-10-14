namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Functions of scheduled pay run budget from exception.</summary>
public static class ScheduledPayRunBudgetFromException
{
    public static Dictionary<string, double> SummarizeFallbackGarnishmentExceptionsForBudget(List<FallbackGarnishmentException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<ScheduledPayRunBudget> DeriveScheduledPayRunBudgetsFromException(List<FallbackGarnishmentException> items)
    {
        var outValue = new List<ScheduledPayRunBudget>();
        foreach (var item in items)
        {
            var derived = ScheduledPayRunBudget.NewScheduledPayRunBudget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveScheduledPayRunBudget(ScheduledPayRunBudget item)
    {
        return SeasonalGarnishmentNoticePolicy.SeasonalGarnishmentNoticeBucket(item.NetTotal());
    }
}
