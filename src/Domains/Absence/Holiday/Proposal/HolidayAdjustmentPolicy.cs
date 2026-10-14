namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of holiday adjustment policy.</summary>
public static class HolidayAdjustmentPolicy
{
    public static double HolidayAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a holiday adjustment code in its canonical HA- form.</summary>
    public static string NormalizeHolidayAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a holiday adjustment score falls in.</summary>
    public static string ClassifyHolidayAdjustment(double score)
    {
        if (score < 10.1)
        {
            return "watch";
        }
        if (score < 46.7)
        {
            return "high";
        }
        if (score < 74.8)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHolidayAdjustment()
    {
        var policy = Retry.DefaultPolicy("holiday-adjustment");
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
