namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional holiday policy policy.</summary>
public static class RegionalHolidayPolicyPolicy
{
    /// <summary>Puts a regional holiday policy code in its canonical RHP- form.</summary>
    public static string NormalizeRegionalHolidayPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RHP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RHP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a regional holiday policy score falls in.</summary>
    public static string ClassifyRegionalHolidayPolicy(double score)
    {
        if (score < 16.3)
        {
            return "low";
        }
        if (score < 24.4)
        {
            return "elevated";
        }
        if (score < 66.2)
        {
            return "critical";
        }
        if (score < 95.8)
        {
            return "minimal";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalHolidayPolicy()
    {
        var policy = Retry.DefaultPolicy("regional-holiday-policy");
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

    public static double RegionalHolidayPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
