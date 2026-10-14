namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of engagement target policy.</summary>
public static class EngagementTargetPolicy
{
    /// <summary>Names the band a engagement target score falls in.</summary>
    public static string ClassifyEngagementTarget(double score)
    {
        if (score < 26.6)
        {
            return "elevated";
        }
        if (score < 28.5)
        {
            return "minimal";
        }
        return "high";
    }

    public static double EngagementTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEngagementTarget()
    {
        var policy = Retry.DefaultPolicy("engagement-target");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a engagement target code in its canonical ET- form.</summary>
    public static string NormalizeEngagementTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ET-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ET-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
