namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling position assignment policy.</summary>
public static class RollingPositionAssignmentPolicy
{
    /// <summary>Names the band a rolling position assignment score falls in.</summary>
    public static string ClassifyRollingPositionAssignment(double score)
    {
        if (score < 44.8)
        {
            return "elevated";
        }
        if (score < 65.6)
        {
            return "high";
        }
        return "medium";
    }

    public static string RollingPositionAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double RollingPositionAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
