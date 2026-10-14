namespace Acme.Workforce.Compliance.Exception.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception estimate policy.</summary>
public static class ExceptionEstimatePolicy
{
    /// <summary>Names the band a exception estimate score falls in.</summary>
    public static string ClassifyExceptionEstimate(double score)
    {
        if (score < 22.5)
        {
            return "ok";
        }
        if (score < 41.0)
        {
            return "watch";
        }
        if (score < 86.8)
        {
            return "medium";
        }
        if (score < 95.8)
        {
            return "elevated";
        }
        return "critical";
    }

    public static string ExceptionEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int ExceptionEstimateSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 55787.0, in order.</summary>
    public static int ExceptionEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55787.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
