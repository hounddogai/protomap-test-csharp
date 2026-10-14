namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of license seat override policy.</summary>
public static class LicenseSeatOverridePolicy
{
    /// <summary>Puts a license seat override code in its canonical LSO- form.</summary>
    public static string NormalizeLicenseSeatOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a license seat override score falls in.</summary>
    public static string ClassifyLicenseSeatOverride(double score)
    {
        if (score < 16.4)
        {
            return "elevated";
        }
        if (score < 22.9)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatOverride()
    {
        var policy = Retry.DefaultPolicy("license-seat-override");
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
}
