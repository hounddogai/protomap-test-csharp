namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of town hall profile policy.</summary>
public static class TownHallProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTownHallProfile()
    {
        var policy = Retry.DefaultPolicy("town-hall-profile");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a town hall profile code in its canonical THP- form.</summary>
    public static string NormalizeTownHallProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("THP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"THP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a town hall profile score falls in.</summary>
    public static string ClassifyTownHallProfile(double score)
    {
        if (score < 4.4)
        {
            return "watch";
        }
        if (score < 22.0)
        {
            return "elevated";
        }
        if (score < 82.6)
        {
            return "medium";
        }
        if (score < 88.4)
        {
            return "low";
        }
        return "critical";
    }

    public static double TownHallProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
