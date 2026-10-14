namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception rule policy.</summary>
public static class ExceptionRulePolicy
{
    public static double ExceptionRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a exception rule score falls in.</summary>
    public static string ClassifyExceptionRule(double score)
    {
        if (score < 51.7)
        {
            return "high";
        }
        if (score < 61.6)
        {
            return "elevated";
        }
        if (score < 86.7)
        {
            return "minimal";
        }
        if (score < 87.6)
        {
            return "critical";
        }
        return "medium";
    }
}
