namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending tax form threshold policy.</summary>
public static class PendingTaxFormThresholdPolicy
{
    /// <summary>Names the band a pending tax form threshold score falls in.</summary>
    public static string ClassifyPendingTaxFormThreshold(double score)
    {
        if (score < 21.3)
        {
            return "minimal";
        }
        if (score < 26.3)
        {
            return "ok";
        }
        if (score < 44.3)
        {
            return "critical";
        }
        return "elevated";
    }

    public static string PendingTaxFormThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 93225.0, in order.</summary>
    public static int PendingTaxFormThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93225.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
