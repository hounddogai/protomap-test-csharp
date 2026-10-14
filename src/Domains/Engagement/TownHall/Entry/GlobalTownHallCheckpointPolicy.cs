namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global town hall checkpoint policy.</summary>
public static class GlobalTownHallCheckpointPolicy
{
    public static double GlobalTownHallCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string GlobalTownHallCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
