namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of license seat group policy.</summary>
public static class LicenseSeatGroupPolicy
{
    /// <summary>Puts a license seat group code in its canonical LSG- form.</summary>
    public static string NormalizeLicenseSeatGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a license seat group score falls in.</summary>
    public static string ClassifyLicenseSeatGroup(double score)
    {
        if (score < 3.4)
        {
            return "watch";
        }
        if (score < 62.3)
        {
            return "low";
        }
        if (score < 89.8)
        {
            return "medium";
        }
        if (score < 96.3)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatGroup()
    {
        var policy = Retry.DefaultPolicy("license-seat-group");
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
}
