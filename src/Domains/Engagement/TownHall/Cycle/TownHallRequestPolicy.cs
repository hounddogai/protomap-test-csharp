namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of town hall request policy.</summary>
public static class TownHallRequestPolicy
{
    public static double TownHallRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.23;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTownHallRequest()
    {
        var policy = Retry.DefaultPolicy("town-hall-request");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a town hall request score falls in.</summary>
    public static string ClassifyTownHallRequest(double score)
    {
        if (score < 62.1)
        {
            return "ok";
        }
        if (score < 69.0)
        {
            return "critical";
        }
        if (score < 77.4)
        {
            return "high";
        }
        return "low";
    }
}
