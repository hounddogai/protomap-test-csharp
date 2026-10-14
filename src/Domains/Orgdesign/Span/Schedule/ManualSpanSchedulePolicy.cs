namespace Acme.Workforce.Orgdesign.Span.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual span schedule policy.</summary>
public static class ManualSpanSchedulePolicy
{
    /// <summary>Names the band a manual span schedule score falls in.</summary>
    public static string ClassifyManualSpanSchedule(double score)
    {
        if (score < 58.9)
        {
            return "critical";
        }
        if (score < 75.0)
        {
            return "watch";
        }
        if (score < 99.0)
        {
            return "ok";
        }
        return "elevated";
    }

    public static double ManualSpanScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
