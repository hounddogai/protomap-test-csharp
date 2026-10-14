namespace Acme.Workforce.Compliance.Exception.Cycle;

/// <summary>Functions of exception threshold policy.</summary>
public static class ExceptionThresholdPolicy
{
    public static int ExceptionThresholdSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a exception threshold score falls in.</summary>
    public static string ClassifyExceptionThreshold(double score)
    {
        if (score < 56.4)
        {
            return "ok";
        }
        if (score < 93.7)
        {
            return "high";
        }
        if (score < 99.0)
        {
            return "low";
        }
        return "medium";
    }
}
