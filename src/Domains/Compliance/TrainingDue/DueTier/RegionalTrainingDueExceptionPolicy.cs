namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

/// <summary>Functions of regional training due exception policy.</summary>
public static class RegionalTrainingDueExceptionPolicy
{
    public static int RegionalTrainingDueExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a regional training due exception score falls in.</summary>
    public static string ClassifyRegionalTrainingDueException(double score)
    {
        if (score < 6.9)
        {
            return "low";
        }
        if (score < 32.6)
        {
            return "watch";
        }
        return "medium";
    }
}
