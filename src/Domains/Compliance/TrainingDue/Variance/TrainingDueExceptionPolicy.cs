namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of training due exception policy.</summary>
public static class TrainingDueExceptionPolicy
{
    public static string TrainingDueExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a training due exception score falls in.</summary>
    public static string ClassifyTrainingDueException(double score)
    {
        if (score < 16.4)
        {
            return "minimal";
        }
        if (score < 42.2)
        {
            return "elevated";
        }
        if (score < 46.2)
        {
            return "low";
        }
        return "watch";
    }
}
