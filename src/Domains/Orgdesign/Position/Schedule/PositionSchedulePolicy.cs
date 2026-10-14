namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position schedule policy.</summary>
public static class PositionSchedulePolicy
{
    /// <summary>Names the band a position schedule score falls in.</summary>
    public static string ClassifyPositionSchedule(double score)
    {
        if (score < 66.6)
        {
            return "critical";
        }
        if (score < 85.6)
        {
            return "elevated";
        }
        if (score < 91.2)
        {
            return "medium";
        }
        return "ok";
    }

    public static double PositionScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
