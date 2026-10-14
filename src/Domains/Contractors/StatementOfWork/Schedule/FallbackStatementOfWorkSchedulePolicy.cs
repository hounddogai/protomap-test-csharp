namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback statement of work schedule policy.</summary>
public static class FallbackStatementOfWorkSchedulePolicy
{
    public static string FallbackStatementOfWorkScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double FallbackStatementOfWorkScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a fallback statement of work schedule score falls in.</summary>
    public static string ClassifyFallbackStatementOfWorkSchedule(double score)
    {
        if (score < 23.7)
        {
            return "low";
        }
        if (score < 66.4)
        {
            return "elevated";
        }
        if (score < 89.0)
        {
            return "ok";
        }
        return "watch";
    }
}
