namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual career path profile policy.</summary>
public static class ManualCareerPathProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualCareerPathProfile()
    {
        var policy = Retry.DefaultPolicy("manual-career-path-profile");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double ManualCareerPathProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a manual career path profile score falls in.</summary>
    public static string ClassifyManualCareerPathProfile(double score)
    {
        if (score < 16.0)
        {
            return "critical";
        }
        if (score < 62.1)
        {
            return "medium";
        }
        return "watch";
    }

    /// <summary>Puts a manual career path profile code in its canonical MCPP- form.</summary>
    public static string NormalizeManualCareerPathProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
