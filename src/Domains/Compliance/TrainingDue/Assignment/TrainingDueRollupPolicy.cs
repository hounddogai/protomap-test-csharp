namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of training due rollup policy.</summary>
public static class TrainingDueRollupPolicy
{
    /// <summary>Names the band a training due rollup score falls in.</summary>
    public static string ClassifyTrainingDueRollup(double score)
    {
        if (score < 11.4)
        {
            return "medium";
        }
        if (score < 83.7)
        {
            return "watch";
        }
        return "critical";
    }

    public static int TrainingDueRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    public static double TrainingDueRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string TrainingDueRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
