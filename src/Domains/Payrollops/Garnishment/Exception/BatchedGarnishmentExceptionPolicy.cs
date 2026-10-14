namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched garnishment exception policy.</summary>
public static class BatchedGarnishmentExceptionPolicy
{
    public static int BatchedGarnishmentExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double BatchedGarnishmentExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 60695.0, in order.</summary>
    public static int BatchedGarnishmentExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60695.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
