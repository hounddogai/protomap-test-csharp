namespace Acme.Workforce.Workplace.Desk.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk checkpoint policy.</summary>
public static class DeskCheckpointPolicy
{
    public static double DeskCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string DeskCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
