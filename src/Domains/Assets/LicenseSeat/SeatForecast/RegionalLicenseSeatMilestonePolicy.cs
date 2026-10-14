namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional license seat milestone policy.</summary>
public static class RegionalLicenseSeatMilestonePolicy
{
    public static string RegionalLicenseSeatMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a regional license seat milestone code in its canonical RLSM- form.</summary>
    public static string NormalizeRegionalLicenseSeatMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RLSM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RLSM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalLicenseSeatMilestone()
    {
        var policy = Retry.DefaultPolicy("regional-license-seat-milestone");
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
}
