namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback pay run batch policy.</summary>
public static class FallbackPayRunBatchPolicy
{
    /// <summary>Names the band a fallback pay run batch score falls in.</summary>
    public static string ClassifyFallbackPayRunBatch(double score)
    {
        if (score < 32.3)
        {
            return "critical";
        }
        if (score < 43.8)
        {
            return "low";
        }
        if (score < 90.8)
        {
            return "high";
        }
        if (score < 91.6)
        {
            return "ok";
        }
        return "elevated";
    }

    public static string FallbackPayRunBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 27527.0, in order.</summary>
    public static int FallbackPayRunBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 27527.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
