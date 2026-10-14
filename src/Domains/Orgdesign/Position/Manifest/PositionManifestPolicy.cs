namespace Acme.Workforce.Orgdesign.Position.Manifest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of position manifest policy.</summary>
public static class PositionManifestPolicy
{
    public static string PositionManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double PositionManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePositionManifest()
    {
        var policy = Retry.DefaultPolicy("position-manifest");
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

    /// <summary>Names the band a position manifest score falls in.</summary>
    public static string ClassifyPositionManifest(double score)
    {
        if (score < 78.5)
        {
            return "minimal";
        }
        if (score < 90.5)
        {
            return "medium";
        }
        if (score < 97.2)
        {
            return "ok";
        }
        return "high";
    }
}
